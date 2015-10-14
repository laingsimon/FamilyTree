using System.Collections.Generic;
using System.Diagnostics;

namespace FamilyTree.Models.FileSystem
{
	[DebuggerDisplay("{_originalDirectory.Name}")]
	public class InterceptedDirectory : IDirectory
	{
		private readonly IFileSystem _fileSystem;
		private readonly IDirectory _originalDirectory;

		public InterceptedDirectory(IDirectory originalDirectory, IFileSystem fileSystem)
		{
			_originalDirectory = originalDirectory;
			_fileSystem = fileSystem;
		}

		public string Name
		{
			[DebuggerStepThrough]
			get { return _originalDirectory.Name; }
		}

		public IDirectory Parent
		{
			get
			{
				return _originalDirectory.Parent != null
					? new InterceptedDirectory(_originalDirectory.Parent, _fileSystem)
					: null;
			}
		}

		public IEnumerable<IDirectory> GetDirectories()
		{
			return _fileSystem.GetDirectories(_originalDirectory);
		}

		public IEnumerable<IFile> GetFiles(string searchPattern)
		{
			return _fileSystem.GetFiles(_originalDirectory, searchPattern);
		}

		public override int GetHashCode()
		{
			return _originalDirectory.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return _originalDirectory.Equals(obj);
		}
	}
}