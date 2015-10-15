using System.Web.Mvc;
using System.Web;

namespace FamilyTree.Models.FileSystem
{
	public static class FileSystemFactory
	{
		public static IFileSystem GetFileSystem(Controller controller)
		{
			return new CachingFileSystem(
				new HttpFileSystem(),
				HttpRuntime.Cache);
		}
	}
}