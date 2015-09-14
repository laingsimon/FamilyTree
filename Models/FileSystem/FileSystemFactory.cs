using System.Web.Mvc;
using System;

namespace FamilyTree.Models.FileSystem
{
	public static class FileSystemFactory
	{
		public static IFileSystem GetFileSystem(Controller controller)
		{
			return new HttpFileSystem(
				new Uri("http://localhost/FamilyTree/", UriKind.Absolute));
		}
	}
}