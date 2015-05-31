using FamilyTree.Models;
using FamilyTree.Models.Authentication;
using FamilyTree.Models.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Controllers
{
	[Authorize(Roles = Roles.SuperUser)]
    public class DataController : Controller
    {
		private readonly ContentNegotiation _contentNegotiation;
		private readonly DataFetcher _fetcher;

		public DataController()
		{
			var xmlContentResponder = new XmlContentResponder();
			_contentNegotiation = new ContentNegotiation(xmlContentResponder)
			{
				{ "text/xml", xmlContentResponder },
				{ "application/xml", xmlContentResponder },
				{ "application/json", new JsonContentResponder(JsonContentResponder.Value.Dto) },
				{ "application/json+viewmodel", new JsonContentResponder(JsonContentResponder.Value.ViewModel) },
			};

			_fetcher = new DataFetcher(s => Server.MapPath(s));
		}


        public ActionResult Zip(string[] name)
		{
			var preference = new ContentTypePreference(Request);
			var responder = _contentNegotiation.GetMostAppropriateResponder(preference);

			if (name == null || !name.Any())
				name = Directory
					.GetFiles(Server.MapPath("~/Data"), "*.xml")
					.Select(filePath => Path.GetFileNameWithoutExtension(filePath))
					.ToArray();

			var stream = _fetcher.GetData(responder, name);

			return new FileStreamResult(stream, "application/zip")
				{
					FileDownloadName = "Data.zip"
				};
		}
    }
}
