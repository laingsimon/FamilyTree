using System;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using FamilyTree.Models.DTO;
using FamilyTree.Models.FileSystem;

namespace FamilyTree.Models
{
	public class TreeFactory
	{
		private readonly IFileSystem _fileSystem;

		public TreeFactory(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public Tree Load(string treeName)
		{
			var fileName = string.Format("~/Data/{0}.xml", treeName);
			return LoadFromFile(_fileSystem.GetFile(fileName));
		}

		public Tree LoadFromFile(IFile file)
		{
			if (file == null)
				return null;

			using (var stream = file.OpenRead())
			using (var fileStream = new System.IO.StreamReader(stream))
			{
				var serialiser = new XmlSerializer(typeof(Tree));
				return (Tree)serialiser.Deserialize(fileStream);
			}
		}

		public bool OtherTreeExists(string treeName)
		{
			if (string.IsNullOrEmpty(treeName))
				return false;
			if (_ContainsInvalidCharacters(treeName))
				return false;

			var fileName = string.Format("~/Data/{0}.xml", treeName);
			return _fileSystem.FileExists(fileName);
		}

		private static bool _ContainsInvalidCharacters(string treeName)
		{
			return System.IO.Path.GetInvalidFileNameChars().Any(
				ch => treeName.IndexOf(ch) != -1);
		}
	}
}