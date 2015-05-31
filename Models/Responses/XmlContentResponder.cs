using System.IO;
using System.IO.Compression;
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

		public void AddToZip(string fileName, ZipArchive zipFile)
		{
			var file = new FileInfo(fileName);

			var entry = zipFile.CreateEntry(Path.GetFileNameWithoutExtension(fileName) + ".xml");

			using (var stream = entry.Open())
			{
				file.OpenRead().CopyTo(stream);
			}			
		}
	}
}