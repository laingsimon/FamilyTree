using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Responses
{
	public class CacheDetailResponder : IContentResponder
	{
		private readonly Func<string, string> _mapPath;

		public CacheDetailResponder(Func<string, string> mapPath)
		{
			_mapPath = mapPath;
		}

		public ActionResult GetResponse(string fileName, HttpContextBase context)
		{
			var xslFile = new FileInfo(_mapPath("~/Xsl/ft.xsl"));
			
			var fileDates = (from treeDate in ETagHelper.GetFileWriteTimes(new FileInfo(fileName))
							 orderby treeDate.Value descending
							 select string.Format("{0} = {1:yyyy-MM-dd@HH:mm:ss}", treeDate.Key, treeDate.Value)).ToList();

			fileDates.Insert(0, string.Format("{0} = {1:yyyy-MM-dd@HH:mm:ss}", "Assembly", ETagHelper.GetAssemblyDate()));
			fileDates.Insert(1, string.Format("{0} = {1:yyyy-MM-dd@HH:mm:ss}", xslFile.Name, xslFile.LastWriteTimeUtc));

			return new ContentResult
			{
				Content = string.Join("\r\n", fileDates),
				ContentType = "text/plain"
			};
		}

		public string GetEtag(string fileName)
		{
			return null;
		}
	}
}