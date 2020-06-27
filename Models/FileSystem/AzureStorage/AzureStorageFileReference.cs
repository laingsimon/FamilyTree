using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using System.Web;

namespace FamilyTree.Models.FileSystem.AzureStorage
{
    public class AzureStorageFileReference
    {
        public string FileName { get; }
        public Uri Uri { get; }
        public StorageUri StorageUri { get; }

        public AzureStorageFileReference(Uri uri, StorageUri storageUri)
        {
            FileName = HttpUtility.UrlDecode(Path.GetFileName(uri.AbsoluteUri));
            Uri = uri;
            StorageUri = storageUri;
        }
    }
}