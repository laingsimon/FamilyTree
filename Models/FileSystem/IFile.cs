using Newtonsoft.Json;
using System;
using System.IO;

namespace FamilyTree.Models.FileSystem
{
	public interface IFile
	{
		Stream OpenRead();
		Stream OpenWrite();
		string Name { get; }
		[JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
		IDirectory Directory { get; }
		long Size { get; }
		DateTime LastWriteTimeUtc { get; }
	}
}