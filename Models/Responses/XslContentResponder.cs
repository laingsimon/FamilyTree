using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using FamilyTree.Models.XmlTransformation;

namespace FamilyTree.Models.Responses
{
	public class XslContentResponder : IContentResponder
	{
		private readonly Func<string, string> _mapPath;

		public XslContentResponder(Func<string, string> mapPath)
		{
			_mapPath = mapPath;
		}

		public ActionResult GetResponse(string fileName, HttpContextBase context)
		{
			return new XslTransformResult(fileName);
		}

		public string GetEtag(string fileName)
		{
			var xslFile = new FileInfo(_mapPath("~/Xsl/ft.xsl"));
			var xslFileDateString = xslFile.LastWriteTimeUtc.ToString("yyyy-MM-dd@HH:mm:ss");

			return ETagHelper.GetEtagFromFile(new FileInfo(fileName), customEtagSuffix: xslFileDateString, includeAssemblyDate: true);
		}
	}
}