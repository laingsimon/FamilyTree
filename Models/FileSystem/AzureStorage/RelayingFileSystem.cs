using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FamilyTree.Models.FileSystem.AzureStorage
{
	public class RelayingFileSystem : IFileSystem
	{
		private readonly IFileSystem _read;
		private readonly IFileSystem _write;

		public RelayingFileSystem(IFileSystem read, IFileSystem write)
		{
			_read = read;
			_write = write;
		}

		public IFile GetFile(string path)
		{
			var readFile = _read.GetFile(path);
			var writeFile = _write.GetFile(path);

			if (readFile != null)
			{
				if (writeFile == null)
					writeFile = _write.CreateFile(path);

				using (var writeStream = _write.OpenWrite(writeFile))
				using (var readStream = _read.OpenRead(readFile))
				{
					readStream.CopyTo(writeStream);
				}

				return readFile;
			}

			if (writeFile != null)
				return writeFile;

			return null;
		}

		public IDirectory GetDirectory(string path)
		{
			return _read.GetDirectory(path);
		}

		public IEnumerable<IFile> GetFiles(IDirectory directory, string searchPattern)
		{
			return _read.GetFiles(directory, searchPattern);
		}

		public IEnumerable<IDirectory> GetDirectories(IDirectory directory)
		{
			return _read.GetDirectories(directory);
		}

		public Stream OpenRead(IFile file)
		{
			return _read.OpenRead(file); //TODO: Write to the _write filesystem (to update any contents)
		}

		public bool FileExists(string path)
		{
			return _read.FileExists(path);
		}

		public Stream OpenWrite(IFile file)
		{
			return _read.OpenWrite(file); //TODO: Write to the _write filesystem
		}

		public IFile CreateFile(string path)
		{
			return _read.CreateFile(path); //TODO: Write to the _write filesystem
		}
	}
}