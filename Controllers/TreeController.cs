using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using FamilyTree.Models.Responses;
using FamilyTree.Models;
using FamilyTree.Models.DTO;
using FamilyTree.ViewModels;

namespace FamilyTree.Controllers
{
	public class TreeController : Controller
	{
		private static readonly string _defaultFamily = ConfigurationManager.AppSettings["DefaultFamily"];
		private readonly ContentNegotiation _contentNegotiation;

		public TreeController()
		{
			var htmlResponder = new XslContentResponder(s => Server.MapPath(s));
			_contentNegotiation = new ContentNegotiation(htmlResponder)
			{
				{ "text/html", htmlResponder },
				{ "application/xhtml+xml", htmlResponder },
				{ "text/xml", new XmlContentResponder() },
				{ "application/xml", new XmlContentResponder() },
				{ "application/json", new JsonContentResponder() },
				{ "text/cache", new CacheDetailResponder(s => Server.MapPath(s)) },
				{ "text/razor", new RazorContentResponder(s => Server.MapPath(s), this) }
			};
		}

		public ActionResult Index(string family)
		{
			return RedirectToAction("Family", new { family = family ?? _defaultFamily });
		}

		public ActionResult SubTree(string family, string fromHandle, string toHandle)
		{
			var relativePath = string.Format("~/Data/{0}.xml", family);
			var familyFilePath = Server.MapPath(relativePath);

			if (!System.IO.File.Exists(familyFilePath))
				return Content("");

			var responder = new RazorContentResponder(s => Server.MapPath(s), this);
			var result = (ViewResult)responder.GetResponse(familyFilePath, HttpContext);

			var tree = (Tree)result.Model;
			var children = tree.FindChildren(fromHandle, toHandle);

			if (children == null)
				return View("EntryPointNotFound", new OtherTreeViewModel(fromHandle, toHandle, relativePath));

			return View("~/Views/Tree/DisplayTemplates/ShowChildren.cshtml", children);
		}

		public ActionResult Family(string family)
		{
			var relativePath = string.Format("~/Data/{0}.xml", family);
			var familyFilePath = Server.MapPath(relativePath);

			if (!System.IO.File.Exists(familyFilePath))
				return RedirectToAction("List");

			try
			{
				var contentTypePreference = new ContentTypePreference(Request);
				var responder = _contentNegotiation.GetMostAppropriateResponder(contentTypePreference);

				var etag = responder.GetEtag(familyFilePath);
				
				if (!ETagHelper.HasChanged(Request, etag))
					return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotModified);

				ETagHelper.AddEtagHeaderToResponse(Response, etag);
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
