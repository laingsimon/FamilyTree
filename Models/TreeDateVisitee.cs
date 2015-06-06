using FamilyTree.Models.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FamilyTree.Models
{
	public class TreeDateVisitee : ITreeVisitee
	{
		private readonly Dictionary<string, DateTime> _fileDates = new Dictionary<string, DateTime>();

		public void Visit(IFile treeFile)
		{
			_fileDates.Add(treeFile.Name, treeFile.LastWriteTimeUtc);
		}

		public Dictionary<string, DateTime> FileDates
		{
			[DebuggerStepThrough]
			get { return _fileDates; }
		}
	}
}