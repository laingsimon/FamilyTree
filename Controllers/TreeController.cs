using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using FamilyTree.Models.Responses;

namespace FamilyTree.Controllers
{
	public class TreeController : Controller
	{
		private static readonly string _defaultFamily = ConfigurationManager.AppSettings["DefaultFamily"];
		private readonly ContentNegotiation _contentNegotiation;

		public TreeController()
		{
			var htmlResponder = new HtmlContentResponder();
			_contentNegotiation = new ContentNegotiation(htmlResponder)
			{
				{ "text/html", htmlResponder },
				{ "application/xhtml+xml", htmlResponder },
				{ "text/xml", new XmlContentResponder() },
				{ "application/xml", new XmlContentResponder() },
				{ "application/json", new JsonContentResponder() }
			};
		}

		public ActionResult Index(string family)
		{
			return RedirectToAction("Family", new { family = family ?? _defaultFamily });
		}

		public ActionResult Family(string family)
		{
			var relativePath = string.Format("~/Data/{0}.xml", family);
			var familyFilePath = Server.MapPath(relativePath);
			if (!System.IO.File.Exists(familyFilePath))
			{
				return RedirectToAction("List");
			}

			try
			{
				var contentTypePreference = new ContentTypePreference(Request);
				var responder = _contentNegotiation.GetMostAppropriateResponder(contentTypePreference);

				return responder.GetResponse(familyFilePath, HttpContext);
			}
			catch (Exception exc)
			{
				Response.StatusCode = 500;

				return new ContentResult
				{
					Content = exc.ToString(),
					ContentType = "text/plain"
				};
			}
		}

		public ActionResult List()
		{
			var files = Directory.GetFiles(Server.MapPath("~/Data"), "*.xml");
			return View(files.Select(fn => new FileInfo(fn)).ToArray());
		}
	}
}
