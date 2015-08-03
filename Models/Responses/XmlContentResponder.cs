using FamilyTree.Models.FileSystem;
using System.IO.Compression;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Responses
{
	public class XmlContentResponder : IContentResponder
	{
		public ActionResult GetResponse(IFile file, HttpContextBase context)
		{
			return new FileContentResult(file.ReadAllBytes(), "text/xml");
		}

		public string GetEtag(IFile file)
		{
			return ETagHelper.GetEtagFromFile(file);
		}

		public void AddToZip(IFile file, ZipArchive zipFile)
		{
			var entry = zipFile.CreateEntry(file.GetFileNameWithoutExtension() + ".xml");

			using (var stream = entry.Open())
			{
				file.OpenRead().CopyTo(stream);
			}
		}
	}
}