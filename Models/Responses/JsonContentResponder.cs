using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Responses
{
	public class JsonContentResponder : IContentResponder
	{
		public ActionResult GetResponse(string fileName, HttpContextBase context)
		{
			using (var fileStream = new StreamReader(fileName))
			{
				var serialiser = new System.Xml.Serialization.XmlSerializer(typeof(Tree));
				var tree = (Tree)serialiser.Deserialize(fileStream);

				return new JsonResult(tree);
			}
		}

		public string GetEtag(DateTime assemblyDate, string fileName)
		{
			return ETagHelper.GetEtagFromFile(new FileInfo(fileName), customDateTimePart: assemblyDate);
		}
	}
}