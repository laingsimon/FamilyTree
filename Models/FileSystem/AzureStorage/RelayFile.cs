using System;
using System.IO;

namespace FamilyTree.Models.FileSystem.AzureStorage
{
	public class RelayFile : IFile
	{
		private readonly IFile _readFile;
		private readonly IFile _writeFile;

		public RelayFile(IFile readFile, IFile writeFile)
		{
			_readFile = readFile;
			_writeFile = writeFile;
		}

		internal IFile GetReadFile()
		{
			return _readFile;
		}

		public DateTime LastWriteTimeUtc
		{
			get { return _readFile.LastWriteTimeUtc; }
		}

		public long Size
		{
			get { return _readFile.Size; }
		}

		public IDirectory Directory
		{
			get { return new RelayDirectory(_readFile.Directory, _writeFile.Directory); }
		}

		public string Name
		{
			get { return _readFile.Name; }
		}

		public Stream OpenWrite()
		{
			var remoteWrite = _writeFile.OpenWrite();
			var localWrite = _readFile.OpenWrite();

			return new MulticastStream(localWrite, remoteWrite);
		}

		public Stream OpenRead()
		{
			using (var readStream = _readFile.OpenRead())
			using (var writeStream = _writeFile.OpenWrite())
				readStream.CopyTo(writeStream);

			return _readFile.OpenRead();
		}

		public override int GetHashCode()
		{
			return _readFile.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format(
				"{0} -> {1}",
				_readFile,
				_writeFile);
		}

		public override bool Equals(object obj)
		{
			return _readFile.Equals(obj)
				|| _writeFile.Equals(obj);
		}
	}
}