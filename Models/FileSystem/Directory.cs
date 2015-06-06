using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace FamilyTree.Models.FileSystem
{
	public class Directory : IDirectory
	{
		private readonly string _name;
		private readonly IDirectory _parent;
		private readonly IFileSystem _fileSystem;

		public Directory(
			string name,
			IDirectory parent,
			IFileSystem fileSystem)
		{
			_name = name;
			_parent = parent;
			_fileSystem = fileSystem;
		}

		public string Name
		{
			[DebuggerStepThrough]
			get { return _name; }
		}

		public IDirectory Parent
		{
			[DebuggerStepThrough]
			get { return _parent; }
		}

		public IEnumerable<IFile> GetFiles(string searchPattern)
		{
			return _fileSystem.GetFiles(this, searchPattern);
		}

		public IEnumerable<IDirectory> GetDirectories()
		{
			return _fileSystem.GetDirectories(this);
		}
	}
}