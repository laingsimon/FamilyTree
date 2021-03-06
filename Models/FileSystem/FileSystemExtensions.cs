﻿using System.IO;

namespace FamilyTree.Models.FileSystem
{
	public static class FileSystemExtensions
	{
		public static byte[] ReadAllBytes(this IFile file)
		{
			if (file == null || file == File.Null)
				throw new FileNotFoundException();

			var content = new byte[file.Size];
			using (var stream = file.OpenRead())
			{
				int readBytes;
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
			if (file == null || file == File.Null)
				return "";

			return Path.GetExtension(file.Name);
		}

		public static string GetFileNameWithoutExtension(this IFile file)
		{
			if (file == null || file == File.Null)
				return "";

			return Path.GetFileNameWithoutExtension(file.Name);
		}

		public static IFile CloneWithFileSystem(this IFile file, IFileSystem fileSystem)
		{
			if (file == null || file == File.Null)
				return File.Null;

			return new File(
				file.Name,
				file.Directory._CloneWithFileSystem(fileSystem),
				file.Size,
				file.LastWriteTimeUtc,
				fileSystem);
		}

		private static IDirectory _CloneWithFileSystem(this IDirectory directory, IFileSystem fileSystem)
		{
			if (directory == null || directory == Directory.Null)
				return Directory.Null;

			return new Directory(
				directory.Name,
				directory.Parent._CloneWithFileSystem(fileSystem),
				fileSystem);
		}
	}
}