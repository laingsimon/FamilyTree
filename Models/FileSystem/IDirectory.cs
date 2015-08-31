using Newtonsoft.Json;
using System.Collections.Generic;

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