using System.Collections.Generic;

namespace FamilyTree.Models.FileSystem.AzureStorage
{
	public class RelayDirectory : IDirectory
	{
		private readonly IDirectory _readDirectory;
		private readonly IDirectory _writeDirectory;

		public RelayDirectory(IDirectory readDirectory, IDirectory writeDirectory)
		{
			_readDirectory = readDirectory;
			_writeDirectory = writeDirectory;
		}

		internal IDirectory GetReadDirectory()
		{
			return _readDirectory;
		}

		internal IDirectory GetWriteDirectory()
		{
			return _writeDirectory;
		}

		public IEnumerable<IDirectory> GetDirectories()
		{
			return _readDirectory.GetDirectories();
		}

		public IEnumerable<IFile> GetFiles(string searchPattern)
		{
			return _readDirectory.GetFiles(searchPattern);
		}

		public IDirectory Parent
		{
			get { return _readDirectory.Parent; }
		}

		public string Name
		{
			get { return _readDirectory.Name; }
		}
	}
}