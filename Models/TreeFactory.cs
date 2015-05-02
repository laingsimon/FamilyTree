using FamilyTree.Models.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

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

		public Tree LoadFromFileName(string fileName)
		{
			using (var fileStream = new StreamReader(fileName))
			{
				var serialiser = new System.Xml.Serialization.XmlSerializer(typeof(Tree));
				return (Tree)serialiser.Deserialize(fileStream);
			}
		}
	}
}