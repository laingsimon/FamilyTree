using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Responses
{
	public class XmlContentResponder : IContentResponder
	{
		public ActionResult GetResponse(string fileName, HttpContextBase context)
		{
			return new FileContentResult(File.ReadAllBytes(fileName), "text/xml");
		}

		public string GetEtag(string fileName)
		{
			return ETagHelper.GetEtagFromFile(new FileInfo(fileName));
		}
	}
}