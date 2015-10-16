using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FamilyTree.Models.FileSystem.AzureStorage
{
	// ReSharper disable UnusedMember.Global
	public class RelayingFileSystem : IFileSystem
	// ReSharper restore UnusedMember.Global
	{
		private readonly IFileSystem _read;
		private readonly IFileSystem _write;
		private readonly bool _updateAllFilesOnDestination;

		public RelayingFileSystem(IFileSystem read, IFileSystem write, bool updateAllFilesOnDestination = false)
		{
			_read = read;
			_write = write;
			_updateAllFilesOnDestination = updateAllFilesOnDestination;
		}

		public IFile GetFile(string path)
		{
			//prefer the file on the destination - unless force is defined
			var writeFile = _write.GetFile(path);
			var readFile = _read.FileExists(path) ? _read.GetFile(path) : null;

			if (readFile == null)
				return writeFile;

			if (!_updateAllFilesOnDestination && writeFile != null && writeFile.Size > 0)
				return new RelayFile(readFile, writeFile);

			if (writeFile == null)
				writeFile = _write.CreateFile(path);

			using (var writeStream = _write.OpenWrite(writeFile))
			using (var readStream = _read.OpenRead(readFile))
			{
				readStream.CopyTo(writeStream);
			}

			return readFile;
		}

		public IDirectory GetDirectory(string path)
		{
			return new RelayDirectory(
				_read.GetDirectory(path),
				_write.GetDirectory(path));
		}

		public IEnumerable<IFile> GetFiles(IDirectory directory, string searchPattern)
		{
			var relayDirectory = (RelayDirectory)directory;
			var readDirectory = relayDirectory.GetReadDirectory();
			var readFiles = _read.GetFiles(readDirectory, searchPattern);
			var writeDirectory = relayDirectory.GetWriteDirectory();

			foreach (var readFile in readFiles)
			{
				var writeFile = writeDirectory.GetFiles(readFile.Name).SingleOrDefault();
				if (writeFile == null)
				{
					//TODO: create file
					continue; //TODO: re-write writeFile with new file and yield
				}

				yield return new RelayFile(readFile, writeFile);
			}
		}

		public IEnumerable<IDirectory> GetDirectories(IDirectory directory)
		{
			var relayDirectory = (RelayDirectory)directory;
			var readDirectories = _read.GetDirectories(relayDirectory.GetReadDirectory());

			foreach (var readDirectory in readDirectories)
			{
				var writeDirectory = _write.GetDirectory(readDirectory.Name); //TODO: ???
				yield return new RelayDirectory(readDirectory, writeDirectory);
			}
		}

		public Stream OpenRead(IFile file)
		{
			throw new NotSupportedException();
		}

		public bool FileExists(string path)
		{
			return _write.FileExists(path) || _read.FileExists(path);
		}

		public Stream OpenWrite(IFile file)
		{
			throw new NotSupportedException();
		}

		public IFile CreateFile(string path)
		{
			return new RelayFile(
				_read.CreateFile(path),
				_write.CreateFile(path));
		}
	}
}