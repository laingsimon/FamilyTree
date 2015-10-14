using System;
using System.Diagnostics;
using System.IO;

namespace FamilyTree.Models.FileSystem
{
	[DebuggerDisplay("{_originalFile.Directory}/{_originalFile.Name}")]
	public class InterceptedFile : IFile
	{
		private readonly IFileSystem _fileSystem;
		private readonly IFile _originalFile;

		public InterceptedFile(IFile originalFile, IFileSystem fileSystem)
		{
			_originalFile = originalFile;
			_fileSystem = fileSystem;
		}

		public IDirectory Directory
		{
			get
			{
				return new InterceptedDirectory(_originalFile.Directory, _fileSystem);
			}
		}

		public DateTime LastWriteTimeUtc
		{
			[DebuggerStepThrough]
			get { return _originalFile.LastWriteTimeUtc; }
		}

		public string Name
		{
			[DebuggerStepThrough]
			get { return _originalFile.Name; }
		}

		public long Size
		{
			[DebuggerStepThrough]
			get { return _originalFile.Size; }
		}

		public Stream OpenRead()
		{
			return _fileSystem.OpenRead(_originalFile);
		}

		public Stream OpenWrite()
		{
			return _fileSystem.OpenWrite(_originalFile);
		}

		public override int GetHashCode()
		{
			return _originalFile.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return _originalFile.Equals(obj);
		}
	}
}