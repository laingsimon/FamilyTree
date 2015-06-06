using FamilyTree.Models.FileSystem;
using System.IO.Compression;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Responses
{
	public interface IContentResponder
	{
		ActionResult GetResponse(IFile file, HttpContextBase context);
		string GetEtag(IFile file);
		void AddToZip(IFile file, ZipArchive zipFile);
	}
}