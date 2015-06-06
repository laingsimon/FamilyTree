using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using FamilyTree.Models.Responses;
using FamilyTree.Models;
using FamilyTree.Models.FileSystem;

namespace FamilyTree.Controllers
{
	public class TreeController : Controller
	{
		private static readonly string _defaultFamily = ConfigurationManager.AppSettings["DefaultFamily"];
		private readonly ContentNegotiation _contentNegotiation;
		private readonly IFileSystem _fileSystem;

		public TreeController()
		{
			_fileSystem = FileSystemFactory.GetFileSystem(this);
			var xslHtmlResponder = new XslContentResponder(_fileSystem);
			var razorHtmlResponder = new RazorContentResponder(_fileSystem);
			_contentNegotiation = new ContentNegotiation(razorHtmlResponder)
			{
				{ "text/html", razorHtmlResponder },
				{ "application/xhtml+xml", razorHtmlResponder },
				{ "text/html+razor", razorHtmlResponder },
				{ "text/html+xsl", xslHtmlResponder },
				{ "text/cache", new CacheDetailResponder(_fileSystem) }
			};
		}

		public ActionResult Index(string family)
		{
			return RedirectToAction("Family", new { family = family ?? _defaultFamily });
		}

		public ActionResult Family(string family)
		{
			var relativePath = string.Format("~/Data/{0}.xml", family);
			var file = _fileSystem.GetFile(relativePath);
			
			if (file == null)
				return RedirectToAction("List");

			try
			{
				var contentTypePreference = new ContentTypePreference(Request);
				var responder = _contentNegotiation.GetMostAppropriateResponder(contentTypePreference);

				var etag = responder.GetEtag(file);
				
				if (!ETagHelper.HasChanged(Request, etag))
					return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotModified);

				ETagHelper.AddEtagHeaderToResponse(Response, etag);
				return responder.GetResponse(file, HttpContext);
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
			var files = _fileSystem.GetDirectory("~/Data").GetFiles("*.xml");
			return View(files.ToArray());
		}
	}
}
