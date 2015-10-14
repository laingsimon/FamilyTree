using System.Web.Mvc;
using System;
using System.Web;

namespace FamilyTree.Models.FileSystem
{
	public static class FileSystemFactory
	{
		public static IFileSystem GetFileSystem(Controller controller)
		{
			return new CachingFileSystem(
				new HttpFileSystem(
					new Uri("http://localhost/FamilyTree/", UriKind.Absolute)),
				HttpRuntime.Cache);
		}
	}
}