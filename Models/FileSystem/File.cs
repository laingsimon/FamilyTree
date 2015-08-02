﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace FamilyTree.Models.FileSystem
{
	[DebuggerDisplay("{_directory}/{_name,nq}")]
	public class File : IFile
	{
		private readonly string _name;
		private readonly IDirectory _directory;
		private readonly long _size;
		private readonly DateTime _lastWriteTimeUtc;
		private readonly IFileSystem _fileSystem;

		public File(
			string name,
			IDirectory directory,
			long size,
			DateTime lastWriteTimeUtc,
			IFileSystem fileSystem)
		{
			_name = name;
			_directory = directory;
			_size = size;
			_lastWriteTimeUtc = lastWriteTimeUtc;
			_fileSystem = fileSystem;
		}

		public string Name
		{
			[DebuggerStepThrough]
			get { return _name; }
		}

		public IDirectory Directory
		{
			[DebuggerStepThrough]
			get { return _directory; }
		}

		public long Size
		{
			[DebuggerStepThrough]
			get { return _size; }
		}

		public DateTime LastWriteTimeUtc
		{
			[DebuggerStepThrough]
			get { return _lastWriteTimeUtc; }
		}

		[DebuggerStepThrough]
		public Stream OpenRead()
		{
			return _fileSystem.OpenRead(this);
		}

		[DebuggerStepThrough]
		public Stream OpenWrite()
		{
			return _fileSystem.OpenWrite(this);
		}

		public override int GetHashCode()
		{
			return _name.GetHashCode() ^ _directory.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var file = obj as IFile;
			if (file == null)
				return false;

			return file.Name.Equals(_name, StringComparison.OrdinalIgnoreCase)
				&& file.Directory.Equals(_directory);
		}
	}
}