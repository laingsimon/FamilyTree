﻿using System.Configuration;
using System.Web.Mvc;
using FamilyTree.Models;

namespace FamilyTree.Controllers
{
	public class TreeController : Controller
	{
		private static readonly string _defaultFamily = ConfigurationManager.AppSettings["DefaultFamily"];
		
		public ActionResult Index(string id)
		{
			return RedirectToAction("Family", new { id = id ?? _defaultFamily });
		}

		public ActionResult Family(string id)
		{
			var relativePath = string.Format("~/Data/{0}.xml", id);
			var familyFilePath = Server.MapPath(relativePath);
			if (!System.IO.File.Exists(familyFilePath))
			{
				Response.AddHeader("FilePath", relativePath);
				return new HttpNotFoundResult();
			}

			return new XslTransformResult(familyFilePath);
		}
	}
}
