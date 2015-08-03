using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FamilyTree.Models.FileSystem.AzureStorage
{
	public class AzureStorageFileSystem : IFileSystem
	{
		private const string _containerName = "filesystem";
		private readonly CloudBlobClient _client;
		private readonly CloudBlobContainer _container;

		public AzureStorageFileSystem()
		{
			var storageAccount = CloudStorageAccount.Parse(
				CloudConfigurationManager.GetSetting("StorageConnectionString"));

			_client = storageAccount.CreateCloudBlobClient();
			_container = _client.GetContainerReference(_containerName);
			_container.CreateIfNotExists();
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

		private CloudBlobDirectory _GetAzureDirectory(IFile path)
		{
			var relativePath = string.Join("/", _GetFullPath(path.Directory));
			return _container.GetDirectoryReference(relativePath);
		}

		private CloudBlobDirectory _GetAzureDirectory(string path)
		{
			var relativePath = string.Join("/", _GetPathParts(path));
			return _container.GetDirectoryReference(relativePath);
		}

		private ICloudBlob _GetAzureFile(IFile file)
		{
			var fullPath = string.Join("/", _GetFullPath(file.Directory)) + "/" + file.Name;
			return _GetAzureFile(fullPath);
		}

		private ICloudBlob _GetAzureFile(string path)
		{
			var directory = _GetAzureDirectory(path);
			var fileName = Path.GetFileName(path);
			var blobItem = (from blob in directory.ListBlobs()
							let blobFileName = Path.GetFileName(blob.Uri.AbsoluteUri)
							where blobFileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)
							select blob).SingleOrDefault();

			return blobItem == null
				? null
				: _client.GetBlobReferenceFromServer(blobItem.StorageUri);
		}

		public IFile GetFile(string path)
		{
			var blobRef = _GetAzureFile(path);
			if (blobRef == null)
				return null;

			var directory = _GetDirectoryFromPath(path);
			var lastModified = blobRef.Properties.LastModified ?? DateTimeOffset.MinValue;
			var length = blobRef.Properties.Length;

			return new File(
				blobRef.Name,
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

			var azureDirectory = _container.GetDirectoryReference(fullPath);
			return from item in azureDirectory.ListBlobs()
				   where _MatchesSearchPattern(item, searchPattern)
				   let file = _client.GetBlobReferenceFromServer(item.StorageUri)
				   let lastModified = file.Properties.LastModified ?? DateTimeOffset.MinValue
				   select new File(
					   file.Name,
					   directory,
					   file.Properties.Length,
					   lastModified.UtcDateTime,
					   this);
		}

		private static bool _MatchesSearchPattern(IListBlobItem item, string searchPattern)
		{
			if (searchPattern.Contains("*"))
				throw new NotImplementedException("Wildcard file matching isn't yet implemented in AzureStorageFileSystem");

			var fileName = item.Uri.Segments.Last();
			return fileName.Equals(searchPattern, StringComparison.OrdinalIgnoreCase);
		}

		public IEnumerable<IDirectory> GetDirectories(IDirectory directory)
		{
			return new[] { directory };
		}

		public Stream OpenRead(IFile file)
		{
			var azureFile = _GetAzureFile(file);
			if (azureFile == null)
				throw new FileNotFoundException();

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
				var reference = directory.GetBlockBlobReference(file.Name);

				return new FileWriteStream(reference);
			}

			return new FileWriteStream(azureFile);
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