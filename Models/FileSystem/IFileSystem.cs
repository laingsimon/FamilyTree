using System.Collections.Generic;
using System.IO;

namespace FamilyTree.Models.FileSystem
{
	public interface IFileSystem
	{
		IFile GetFile(string path);
		IDirectory GetDirectory(string path);
		IEnumerable<IFile> GetFiles(IDirectory directory, string searchPattern);
		IEnumerable<IDirectory> GetDirectories(IDirectory directory);
		Stream OpenRead(IFile file);
		bool FileExists(string path);
		Stream OpenWrite(IFile file);
		IFile CreateFile(string path);
    }
}