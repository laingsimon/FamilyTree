using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FamilyTree.Models.FileSystem
{
	public interface IFile
	{
		Stream OpenRead();
		string Name { get; }
		IDirectory Directory { get; }
		long Size { get; }
		DateTime LastWriteTimeUtc { get; }
	}
}