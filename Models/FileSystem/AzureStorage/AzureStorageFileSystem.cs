using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Diagnostics;

namespace FamilyTree.Models.FileSystem.AzureStorage
{

    public class AzureStorageFileSystem : IFileSystem
    {
        private static AzureStorageCache _cache = new AzureStorageCache();

        private const string _containerName = "filesystem";
        private readonly CloudBlobClient _client;
        private readonly CloudBlobContainer _container;

        public AzureStorageFileSystem()
        {
            var storageAccount = CloudStorageAccount.Parse(
                GetStorageConnectionString());

            _client = storageAccount.CreateCloudBlobClient();
            _container = _client.GetContainerReference(_containerName);
            _container.CreateIfNotExists();
        }

        private static string GetStorageConnectionString()
        {
            var connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            if (string.IsNullOrEmpty(connectionString))
                return GetConnectionStringFromServer();

            return connectionString;
        }

        private static string GetConnectionStringFromServer(bool throwIfNotFound = true)
        {
            const string environmentVariableName = "FamilyTree_StorageConnectionString";
            var connectionString = Environment.GetEnvironmentVariable(environmentVariableName);
            if (string.IsNullOrEmpty(connectionString))
            {
                if (throwIfNotFound)
                    throw new InvalidOperationException("StorageConnectionString is not configured in the web.config or in the " + environmentVariableName + " environment variable");

                return null;
            }

            return connectionString;
        }

        public static bool CanUseFileSystem()
        {
            var connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            return !string.IsNullOrEmpty(connectionString) || !string.IsNullOrEmpty(GetConnectionStringFromServer(false));
        }

        private static IEnumerable<string> _GetPathParts(string path)
        {
            var parts = path.Split('/').Skip(1).ToArray();
            var lastPart = parts.Last();
            var containsFileName = lastPart.Contains(".");
            var partsToTake = containsFileName
                ? parts.Length - 1
                : parts.Length;

            return parts.Take(partsToTake);
        }

        private static IEnumerable<string> _GetFullPath(IDirectory directory)
        {
            if (directory.Parent != null)
            {
                foreach (var parent in _GetFullPath(directory.Parent))
                    yield return parent;
            }

            yield return directory.Name;
        }

        private IDirectory _GetDirectoryFromPath(IReadOnlyCollection<string> parts)
        {
            if (!parts.Any())
                return null;

            var firstPart = parts.First();
            var remainingParts = parts.Skip(1);

            return new Directory(
                firstPart,
                _GetDirectoryFromPath(remainingParts.ToArray()),
                this);
        }

        private IDirectory _GetDirectoryFromPath(string path)
        {
            var parts = _GetPathParts(path);
            return _GetDirectoryFromPath(parts.ToArray());
        }

        private AzureStorageDirectoryReference _GetAzureDirectory(IFile path)
        {
            var relativePath = string.Join("/", _GetFullPath(path.Directory));

            return _GetAzureDirectory(relativePath);
        }

        private AzureStorageDirectoryReference _GetAzureDirectory(string path)
        {
            return _cache.GetDirectory(path, () =>
            {
                var relativePath = string.Join("/", _GetPathParts(path));
                return new AzureStorageDirectoryReference(relativePath);
            });
        }

        private ICloudBlob _GetAzureFile(IFile file)
        {
            var fullPath = "~/" + string.Join("/", _GetFullPath(file.Directory)) + "/" + file.Name;
            return _GetAzureFile(fullPath);
        }

        private ICloudBlob _GetAzureFile(string path)
        {
            var directory = _GetAzureDirectory(path);
            var fileName = Path.GetFileName(path);

            var files = directory.GetFiles(_cache, _container);
            var file = files.SingleOrDefault(f => f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));

            if (file == null)
            {
                Trace.TraceWarning($"No file found with path {path} out of {files.Count} file/s [{string.Join(",", files.Select(f => f.FileName))}]");
                return null;
            }

            try
            {
                Trace.TraceInformation("GetBlobReferenceFromServer({0})", file.StorageUri);
                return _client.GetBlobReferenceFromServer(file.StorageUri);
            }
            finally
            {
                Trace.TraceInformation("Reference fetched from azure");
            }
        }

        public IFile GetFile(string path)
        {
            var blobRef = _GetAzureFile(path);
            if (blobRef == null)
                return File.Null;

            var directory = _GetDirectoryFromPath(path);
            var lastModified = blobRef.Properties.LastModified ?? DateTimeOffset.MinValue;
            var length = blobRef.Properties.Length;

            return new File(
                Path.GetFileName(blobRef.Name),
                directory,
                length,
                lastModified.UtcDateTime,
                this);
        }

        public IDirectory GetDirectory(string path)
        {
            return _GetDirectoryFromPath(path);
        }

        public IEnumerable<IFile> GetFiles(IDirectory directory, string searchPattern)
        {
            var fullPath = string.Join("/", _GetFullPath(directory));

            Trace.TraceInformation("GetFiles({0})", fullPath);
            var storageDirectory = _cache.GetDirectory(fullPath, () =>
            {
                return new AzureStorageDirectoryReference(fullPath);
            });
            var files = storageDirectory.GetFiles(_cache, _container);

            var azureDirectory = _container.GetDirectoryReference(fullPath);

            return from item in files
                    where _MatchesSearchPattern(item, searchPattern)
                    let file = _client.GetBlobReferenceFromServer(item.StorageUri)
                    let lastModified = file.Properties.LastModified ?? DateTimeOffset.MinValue
                    select new File(
                        Path.GetFileName(file.Name),
                        directory,
                        file.Properties.Length,
                        lastModified.UtcDateTime,
                        this);
        }

        private static bool _MatchesSearchPattern(AzureStorageFileReference item, string searchPattern)
        {
            var fileName = item.Uri.Segments.Last();
            var comparer = new FilePatternComparer();
            return comparer.Equals(fileName, searchPattern);
        }

        public IEnumerable<IDirectory> GetDirectories(IDirectory directory)
        {
            return new[] { directory };
        }

        public Stream OpenRead(IFile file)
        {
            var azureFile = _GetAzureFile(file);
            if (azureFile == null)
                return Stream.Null;

            return azureFile.OpenRead();
        }

        public bool FileExists(string path)
        {
            return _GetAzureFile(path) != null;
        }

        public Stream OpenWrite(IFile file)
        {
            var azureFile = _GetAzureFile(file);
            if (azureFile == null)
            {
                var directory = _GetAzureDirectory(file);
                var reference = directory.GetBlockBlobReference(file.Name, _container);

                return new DelayedWriteStream(stream => reference.UploadFromStream(stream));
            }

            return new DelayedWriteStream(stream => azureFile.UploadFromStream(stream));
        }

        public IFile CreateFile(string path)
        {
            return new File(
                Path.GetFileName(path),
                _GetDirectoryFromPath(path),
                0,
                DateTime.MinValue,
                this);
        }
    }
}