using System.IO;

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
			if (file == null)
				return null;

			return Path.GetExtension(file.Name);
		}

		public static string GetFileNameWithoutExtension(this IFile file)
		{
			if (file == null)
				return null;

			return Path.GetFileNameWithoutExtension(file.Name);
		}

		public static IFile CloneWithFileSystem(this IFile file, IFileSystem fileSystem)
		{
			if (file == null)
				return null;

			return new File(
				file.Name,
				file.Directory.CloneWithFileSystem(fileSystem),
				file.Size,
				file.LastWriteTimeUtc,
				fileSystem);
		}

		public static IDirectory CloneWithFileSystem(this IDirectory directory, IFileSystem fileSystem)
		{
			if (directory == null)
				return null;

			return new Directory(
				directory.Name,
				directory.Parent.CloneWithFileSystem(fileSystem),
				fileSystem);
		}
	}
}