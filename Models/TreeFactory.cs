using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using FamilyTree.Models.DTO;

namespace FamilyTree.Models
{
	public class TreeFactory
	{
		private readonly Func<string, string> _mapPath;

		public TreeFactory(Func<string, string> mapPath = null)
		{
			_mapPath = mapPath ?? HttpContext.Current.Server.MapPath;
		}

		public Tree Load(string treeName)
		{
			var fileName = _mapPath(string.Format("~/Data/{0}.xml", treeName));
			return LoadFromFileName(fileName);
		}

		public static Tree LoadFromFileName(string fileName)
		{
			using (var fileStream = new StreamReader(fileName))
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

			var fileName = _mapPath(string.Format("~/Data/{0}.xml", treeName));
			return File.Exists(fileName);
		}

		private static bool _ContainsInvalidCharacters(string treeName)
		{
			return Path.GetInvalidFileNameChars().Any(
				ch => treeName.IndexOf(ch) != -1);
		}
	}
}