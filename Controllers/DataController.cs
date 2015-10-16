using System.Linq;
using System.Web.Mvc;
using FamilyTree.Models;
using FamilyTree.Models.Authentication;
using FamilyTree.Models.FileSystem;
using FamilyTree.Models.Responses;

namespace FamilyTree.Controllers
{
	[Authorize(Roles = Roles.SuperUser)]
	public class DataController : Controller
	{
		private readonly ContentNegotiation _contentNegotiation;
		private readonly DataFetcher _fetcher;
		private readonly IFileSystem _fileSystem;

		public DataController()
		{
			_fileSystem = FileSystemFactory.GetFileSystem();
			var xmlContentResponder = new XmlContentResponder();
			_contentNegotiation = new ContentNegotiation(xmlContentResponder)
			{
				{ "text/xml", xmlContentResponder },
				{ "application/xml", xmlContentResponder },
				{ "application/json", new JsonContentResponder(_fileSystem, JsonContentResponder.Value.Dto) },
				{ "application/json+viewmodel", new JsonContentResponder(_fileSystem, JsonContentResponder.Value.ViewModel) },
			};

			_fetcher = new DataFetcher(_fileSystem);
		}


		public ActionResult Zip(string[] name)
		{
			var preference = new ContentTypePreference(Request);
			var responder = _contentNegotiation.GetMostAppropriateResponder(preference);

			if (name == null || !name.Any())
				name = _fileSystem
						.GetDirectory("~/Data")
						.GetFiles("*.xml")
						.Select(file => file.GetFileNameWithoutExtension())
						.ToArray();

			var stream = _fetcher.GetData(responder, name);

			return new FileStreamResult(stream, "application/zip")
				{
					FileDownloadName = "Data.zip"
				};
		}
	}
}
