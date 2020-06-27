using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace FamilyTree.Models.FileSystem.AzureStorage
{
    public class AzureStorageCache
    {
        private readonly Cache _cache = HttpRuntime.Cache;

        public AzureStorageDirectoryReference GetDirectory(string relativePath, Func<AzureStorageDirectoryReference> getReference)
        {
            var key = $"directory:{relativePath.ToLower()}";
            var cachedItem = _cache.Get(key) as AzureStorageDirectoryReference;
            return cachedItem ?? _StoreInCache(key, getReference);
        }

        private T _StoreInCache<T>(string key, Func<T> getItem)
            where T : class
        {
            var item = getItem();
            if (item == null)
            {
                return null;
            }

            _cache.Insert(key, item, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1));
            return item;
        }

        public ICollection<AzureStorageFileReference> GetFiles(string relativePath, Func<ICollection<AzureStorageFileReference>> getFiles)
        {
            var key = $"files:{relativePath.ToLower()}";
            var cachedItem = _cache.Get(key) as ICollection<AzureStorageFileReference>;
            return cachedItem ?? _StoreInCache(key, getFiles);
        }
    }
}