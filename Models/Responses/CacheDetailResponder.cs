using FamilyTree.Models.FileSystem;
using System;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Responses
{
	public class CacheDetailResponder : IContentResponder
	{
		private readonly IFileSystem _fileSystem;

		public CacheDetailResponder(
			IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public ActionResult GetResponse(IFile file, HttpContextBase context)
		{
			var xslFile = _fileSystem.GetFile("~/Xsl/ft.xsl");
			
			var fileDates = (from treeDate in ETagHelper.GetFileWriteTimes(file)
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

		public string GetEtag(IFile file)
		{
			return null;
		}

		public void AddToZip(IFile file, ZipArchive zipFile)
		{ }
	}
}