using System.Web;

namespace FamilyTree.Models.FileSystem
{
	public static class FileSystemFactory
	{
		public static IFileSystem GetFileSystem()
		{
			return new CachingFileSystem(
				new HttpFileSystem(),
				HttpRuntime.Cache,
				CachingFileSystem.ShouldRefreshCache(HttpContext.Current));
		}
	}
}