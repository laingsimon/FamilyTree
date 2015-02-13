using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Responses
{
	public class HtmlContentResponder : IContentResponder
	{
		public ActionResult GetResponse(string fileName, HttpContextBase context)
		{
			return new XslTransformResult(fileName);
		}
	}
}