using System.IO;
using System.Web;
using System.Web.Mvc;
using FamilyTree.ViewModels;
using Newtonsoft.Json;
using System.IO.Compression;
using Newtonsoft.Json.Converters;

namespace FamilyTree.Models.Responses
{
	public class JsonContentResponder : IContentResponder
	{
		private readonly Value _value;
		private readonly TreeViewModelFactory _viewModelFactory;

		public JsonContentResponder(
			Value value,
			TreeFactory treeFactory = null,
			TreeViewModelFactory viewModelFactory = null)
		{
			treeFactory = treeFactory ?? new TreeFactory();
			_value = value;
			_viewModelFactory = viewModelFactory ?? new TreeViewModelFactory(treeFactory, new TreeParser());
		}

		public enum Value
		{
			Dto,
			ViewModel
		}

		public ActionResult GetResponse(string fileName, HttpContextBase context)
		{
			var data = _GetValue(fileName);
			return new JsonResult(data);
		}

		private object _GetValue(string fileName)
		{
			var tree = TreeFactory.LoadFromFileName(fileName);

			if (_value == Value.Dto)
				return tree;

			return _viewModelFactory.Create(tree);
		}

		public string GetEtag(string fileName)
		{
			return ETagHelper.GetEtagFromFile(new FileInfo(fileName), includeAssemblyDate: true);
		}

		public void AddToZip(string fileName, ZipArchive zipFile)
		{
			var file = new FileInfo(fileName);
			var data = _GetValue(fileName);

			var entry = zipFile.CreateEntry(Path.GetFileNameWithoutExtension(fileName) + ".json");

			using (var stream = entry.Open())
			using (var writer = new JsonTextWriter(new StreamWriter(stream)) { CloseOutput = false })
			{
				var serialiser = new JsonSerializer
				{
					Formatting = Formatting.Indented,
					Converters = 
					{
						new StringEnumConverter()
					},
					NullValueHandling = NullValueHandling.Ignore
				};
				serialiser.Serialize(writer, data);
			}
		}
	}
}