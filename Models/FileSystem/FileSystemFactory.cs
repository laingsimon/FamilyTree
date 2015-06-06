using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.FileSystem
{
	public static class FileSystemFactory
	{
		public static IFileSystem GetFileSystem(Controller controller)
		{
			return new LocalFileSystem(s => controller.Server.MapPath(s));
		}
	}
}