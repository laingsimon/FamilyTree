using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using FamilyTree.Models;

namespace FamilyTree.Controllers
{
	public class TreeController : Controller
	{
		private static readonly string _defaultFamily = ConfigurationManager.AppSettings["DefaultFamily"];
		
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
				Response.AddHeader("FilePath", relativePath);
				return RedirectToAction("List");
			}

			return new XslTransformResult(familyFilePath);
		}

		public ActionResult List()
		{
			var files = Directory.GetFiles(Server.MapPath("~/Data"), "*.xml");
			return View(files.Select(fn => new FileInfo(fn)).ToArray());
		}

		public ActionResult Json(string family)
		{
			var fileName = Server.MapPath(string.Format("~/Data/{0}.xml", family));

			Response.AddHeader("FileName", fileName);
			if (!System.IO.File.Exists(fileName))
				return HttpNotFound();

			try
			{
				using (var fileStream = new StreamReader(fileName))
				{
					var serialiser = new System.Xml.Serialization.XmlSerializer(typeof(Tree));
					var tree = (Tree)serialiser.Deserialize(fileStream);

					return new JsonResult
					{
						JsonRequestBehavior = JsonRequestBehavior.AllowGet,
						Data = tree
					};
				}
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
	}
}
