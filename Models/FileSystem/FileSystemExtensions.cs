using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FamilyTree.Models.FileSystem
{
	public static class FileSystemExtensions
	{
		public static byte[] ReadAllBytes(this IFile file)
		{
			if (file == null)
				throw new FileNotFoundException();

			var content = new byte[file.Size];
			using (var stream = file.OpenRead())
			{
				var readBytes = 0;
				var offset = 0;
				while ((readBytes = stream.Read(content, offset, 4096)) > 0)
				{
					offset += readBytes;
				}
			}

			return content;
		}

		public static string GetExtension(this IFile file)
		{
			if (file == null)
				return null;

			return System.IO.Path.GetExtension(file.Name);
		}

		public static string GetFileNameWithoutExtension(this IFile file)
		{
			if (file == null)
				return null;

			return System.IO.Path.GetFileNameWithoutExtension(file.Name);
		}
	}
}