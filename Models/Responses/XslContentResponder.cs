using System.IO.Compression;
using System.Web;
using System.Web.Mvc;
using FamilyTree.Models.FileSystem;
using FamilyTree.Models.XmlTransformation;

namespace FamilyTree.Models.Responses
{
	public class XslContentResponder : IContentResponder
	{
		private readonly IFileSystem _fileSystem;

		public XslContentResponder(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public ActionResult GetResponse(IFile file, HttpContextBase context)
		{
			return new XslTransformResult(_fileSystem, file);
		}

		public string GetEtag(IFile file)
		{
			var xslFile = _fileSystem.GetFile("~/Xsl/ft.xsl");
			var xslFileDateString = xslFile.LastWriteTimeUtc.ToString("yyyy-MM-dd@HH:mm:ss");

			return ETagHelper.GetEtagFromFile(xslFile, customEtagSuffix: xslFileDateString, includeAssemblyDate: true);
		}

		public void AddToZip(IFile file, ZipArchive zipFile)
		{ }
	}
}