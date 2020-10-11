using FamilyTree.Models.FileSystem.AzureStorage;
using System.Web;

namespace FamilyTree.Models.FileSystem
{
    public static class FileSystemFactory
    {
        public static IFileSystem GetFileSystem()
        {
            return new CachingFileSystem(
                AzureStorageFileSystem.CanUseFileSystem()
                    ? new AzureStorageFileSystem()
                    : (IFileSystem)new HttpFileSystem(),
                HttpRuntime.Cache,
                CachingFileSystem.ShouldRefreshCache(HttpContext.Current));
        }
    }
}