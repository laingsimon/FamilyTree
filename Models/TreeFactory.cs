using System.IO;
using System.Linq;
using System.Xml.Serialization;
using FamilyTree.Models.DTO;
using FamilyTree.Models.FileSystem;
using System;

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
			if (file == null || file == FileSystem.File.Null)
				return null;

			using (var stream = file.OpenRead())
			using (var fileStream = new StreamReader(stream))
			{
				if (stream == Stream.Null)
					throw new InvalidOperationException("Unable to load file " + file.Name);

				var serialiser = new XmlSerializer(typeof(Tree));
				try
				{
					return (Tree)serialiser.Deserialize(fileStream);
				}
				catch (InvalidOperationException exc)
				{
					throw new InvalidOperationException(string.Format("Unable to load file {0} ({1} bytes)", file.Name, file.Size), exc);
				}
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
			return Path.GetInvalidFileNameChars().Any(
				ch => treeName.IndexOf(ch) != -1);
		}
	}
}