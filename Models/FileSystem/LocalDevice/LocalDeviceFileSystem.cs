using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FamilyTree.Models.FileSystem.LocalDevice
{
	public class LocalDeviceFileSystem : IFileSystem
	{
		private readonly Func<string, string> _mapPath;

		public LocalDeviceFileSystem(Func<string, string> mapPath)
		{
			_mapPath = mapPath;
		}

		public IFile GetFile(string path)
		{
			var ioFile = new FileInfo(_mapPath(path));
			if (!ioFile.Exists)
				throw new FileNotFoundException("File not found", path);

			return new File(
				ioFile.Name,
				_GetDirectory(ioFile.Directory),
				ioFile.Length,
				ioFile.LastWriteTimeUtc,
				this);
		}

		public IDirectory GetDirectory(string path)
		{
			var ioDirectory = new DirectoryInfo(_mapPath(path));
			if (!ioDirectory.Exists)
				throw new DirectoryNotFoundException("Directory not found - " + ioDirectory.FullName);

			return _GetDirectory(ioDirectory);
		}

		public IEnumerable<IFile> GetFiles(IDirectory directory, string searchPattern)
		{
			var ioDirectory = new DirectoryInfo(_GetFullPath(directory));
			if (!ioDirectory.Exists)
				throw new DirectoryNotFoundException("Directory not found - " + ioDirectory.FullName);

			return from ioSubFile in ioDirectory.EnumerateFiles(searchPattern)
				   select new File(
					   ioSubFile.Name,
					   directory,
					   ioSubFile.Length,
					   ioSubFile.LastWriteTimeUtc,
					   this);
		}

		public IEnumerable<IDirectory> GetDirectories(IDirectory directory)
		{
			var ioDirectory = new DirectoryInfo(_GetFullPath(directory));
			if (!ioDirectory.Exists)
				throw new DirectoryNotFoundException("Directory not found - " + ioDirectory.FullName);

			return from ioSubDir in ioDirectory.EnumerateDirectories()
				   select new Directory(
					   ioSubDir.Name,
					   directory,
					   this);
		}

		public Stream OpenRead(IFile file)
		{
			var ioFile = new FileInfo(_GetFullPath(file));
			if (!ioFile.Exists)
				throw new FileNotFoundException("File not found", ioFile.FullName);

			return ioFile.OpenRead();
		}

		public Stream OpenWrite(IFile file)
		{
			var ioFile = new FileInfo(_GetFullPath(file));
			if (!ioFile.Exists)
				throw new FileNotFoundException("File not found", ioFile.FullName);

			return ioFile.OpenWrite();
		}

		public bool FileExists(string path)
		{
			return System.IO.File.Exists(_mapPath(path));
		}

		private static string _GetFullPath(IFile file)
		{
			return string.Join(
				Path.DirectorySeparatorChar.ToString(),
				_GetFullPath(file.Directory),
				file.Name);
		}

		private static string _GetFullPath(IDirectory directory)
		{
			return string.Join(Path.DirectorySeparatorChar.ToString(), _PathNames(directory).ToArray());
		}

		private static IEnumerable<string> _PathNames(IDirectory directory)
		{
			if (directory.Parent != null)
			{
				foreach (var name in _PathNames(directory.Parent))
					yield return name;
			}

			yield return directory.Name;
		}

		private IDirectory _GetDirectory(DirectoryInfo directory)
		{
			if (directory == null)
				return null;

			return new Directory(
				directory.Name,
				_GetDirectory(directory.Parent),
				this);
		}

		public IFile CreateFile(string path)
		{
			return new File(
				Path.GetFileName(path),
				new Directory(
					Path.GetDirectoryName(path),
					null,
					this),
				0,
				DateTime.MinValue,
				this
				);
		}
	}
}