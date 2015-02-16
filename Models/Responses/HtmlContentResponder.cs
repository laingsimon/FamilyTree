using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Responses
{
	public class HtmlContentResponder : IContentResponder
	{
		private readonly Func<string, string> _mapPath;

		public HtmlContentResponder(Func<string, string> mapPath)
		{
			_mapPath = mapPath;
		}

		public ActionResult GetResponse(string fileName, HttpContextBase context)
		{
			return new XslTransformResult(fileName);
		}

		public string GetEtag(DateTime assemblyDate, string fileName)
		{
			var xslFile = new FileInfo(_mapPath("~/Xsl/ft.xsl"));
			var xslFileDateString = xslFile.LastWriteTimeUtc.ToString("yyyy-MM-dd@HH:mm:ss");

			return ETagHelper.GetEtagFromFile(new FileInfo(fileName), customEtagSuffix: xslFileDateString, customDateTimePart: assemblyDate);
		}
	}
}