using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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

		public override string ToString()
		{
			if (_parent == null)
				return _name;

			return _parent + "/" + _name;
		}

		public override int GetHashCode()
		{
			var parentHashCode = _parent != null
				? _parent.GetHashCode()
				: 0;
			return _name.GetHashCode() ^ parentHashCode;
		}

		public override bool Equals(object obj)
		{
			var directory = obj as IDirectory;
			if (directory == null)
				return false;

			if (!directory.Name.Equals(_name, StringComparison.OrdinalIgnoreCase))
				return false;

			if (_parent == null && directory.Parent != null)
				return false;
			if (_parent == null && directory.Parent == null)
				return true;

			return _parent.Equals(directory.Parent);
		}
	}
}