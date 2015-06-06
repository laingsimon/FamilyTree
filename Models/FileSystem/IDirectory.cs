using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FamilyTree.Models.FileSystem
{
	public interface IDirectory
	{
		string Name { get; }
		IDirectory Parent { get; }
		IEnumerable<IFile> GetFiles(string searchPattern);
		IEnumerable<IDirectory> GetDirectories();
	}
}