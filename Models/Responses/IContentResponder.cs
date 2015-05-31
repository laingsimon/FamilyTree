using System.IO.Compression;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Responses
{
	public interface IContentResponder
	{
		ActionResult GetResponse(string fileName, HttpContextBase context);
		string GetEtag(string fileName);
		void AddToZip(string fileName, ZipArchive zipFile);
	}
}