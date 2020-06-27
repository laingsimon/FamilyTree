using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FamilyTree.Models.FileSystem.AzureStorage
{
    public class AzureStorageDirectoryReference
    {
        public string RelativePath { get; }

        public AzureStorageDirectoryReference(string relativePath)
        {
            RelativePath = relativePath;
        }

        public ICollection<AzureStorageFileReference> GetFiles(AzureStorageCache cache, CloudBlobContainer container)
        {
            return cache.GetFiles(RelativePath, () =>
            {
                var directory = GetAzureDirectory(container);

                Trace.TraceInformation("directory[{0}].ListBlobs()", RelativePath);
                var blobs = directory.ListBlobs();
                return blobs
                    .Select(blob => new AzureStorageFileReference(blob.Uri, blob.StorageUri))
                    .ToArray();
            });
        }

        private CloudBlobDirectory GetAzureDirectory(CloudBlobContainer container)
        {
            Trace.TraceInformation("_GetAzureDirectory({0})", RelativePath);
            return container.GetDirectoryReference(RelativePath);
        }

        public CloudBlockBlob GetBlockBlobReference(string name, CloudBlobContainer container)
        {
            var directory = GetAzureDirectory(container);
            return directory.GetBlockBlobReference(name);
        }
    }
}