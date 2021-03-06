﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FamilyTree.Models.FileSystem.LocalDevice
{
	public class LocalDeviceFileSystem : IFileSystem
	{
		private readonly Func<string, string> _mapPath;
		private readonly Lazy<DirectoryInfo> _rootPath;

		public LocalDeviceFileSystem(Func<string, string> mapPath)
		{
			_mapPath = mapPath;
			_rootPath = new Lazy<DirectoryInfo>(() => new DirectoryInfo(mapPath("~")));
        }

		public IFile GetFile(string path)
		{
			var ioFile = new FileInfo(_mapPath(path));
			if (!ioFile.Exists)
				return File.Null;

			return new File(
				ioFile.Name,
				_GetDirectory(ioFile.Directory) ?? Directory.Null,
				ioFile.Length,
				ioFile.LastWriteTimeUtc,
				this);
		}

		public IDirectory GetDirectory(string path)
		{
			var ioDirectory = new DirectoryInfo(_mapPath(path));
			if (!ioDirectory.Exists)
				return Directory.Null;

			return _GetDirectory(ioDirectory) ?? Directory.Null;
		}

		public IEnumerable<IFile> GetFiles(IDirectory directory, string searchPattern)
		{
			var ioDirectory = new DirectoryInfo(_GetFullPath(directory));
			if (!ioDirectory.Exists)
				return new IFile[0];

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
				return new IDirectory[0];

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
				return Stream.Null;

			return ioFile.OpenRead();
		}

		public Stream OpenWrite(IFile file)
		{
			var ioFile = new FileInfo(_GetFullPath(file));
			if (!ioFile.Exists)
				return Stream.Null;

			return ioFile.OpenWrite();
		}

		public bool FileExists(string path)
		{
			return System.IO.File.Exists(_mapPath(path));
		}

		private string _GetFullPath(IFile file)
		{
			return string.Join(
				Path.DirectorySeparatorChar.ToString(),
				_GetFullPath(file.Directory),
				file.Name);
		}

		private string _GetFullPath(IDirectory directory)
		{
			return string.Join(Path.DirectorySeparatorChar.ToString(), _PathNames(directory).ToArray());
		}

		private IEnumerable<string> _PathNames(IDirectory directory)
		{
			if (directory.Parent != null)
			{
				foreach (var name in _PathNames(directory.Parent))
				{
					if (name == "~")
						yield return _mapPath("~");
					else
						yield return name;
				}
			}

			yield return directory.Name;
		}

		private IDirectory _GetDirectory(DirectoryInfo directory)
		{
			if (directory == null)
				return null;

			if (directory.FullName.Equals(_rootPath.Value.FullName, StringComparison.OrdinalIgnoreCase))
				return new Directory("~", null, this);

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