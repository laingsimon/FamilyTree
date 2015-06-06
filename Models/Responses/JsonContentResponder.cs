using System.Web;
using System.Web.Mvc;
using FamilyTree.ViewModels;
using Newtonsoft.Json;
using System.IO.Compression;
using Newtonsoft.Json.Converters;
using FamilyTree.Models.FileSystem;

namespace FamilyTree.Models.Responses
{
	public class JsonContentResponder : IContentResponder
	{
		private readonly Value _value;
		private readonly TreeViewModelFactory _viewModelFactory;
		private readonly IFileSystem _fileSystem;
		private readonly TreeFactory _treeFactory;

		public JsonContentResponder(
			IFileSystem fileSystem,
			Value value,
			TreeFactory treeFactory = null,
			TreeViewModelFactory viewModelFactory = null)
		{
			_fileSystem = fileSystem;
			_treeFactory = treeFactory ?? new TreeFactory(fileSystem);
			_value = value;
			_viewModelFactory = viewModelFactory ?? new TreeViewModelFactory(_treeFactory, new TreeParser());
		}

		public enum Value
		{
			Dto,
			ViewModel
		}

		public ActionResult GetResponse(IFile file, HttpContextBase context)
		{
			var data = _GetValue(file);
			return new JsonResult(data);
		}

		private object _GetValue(IFile file)
		{
			var tree = _treeFactory.LoadFromFile(file);

			if (_value == Value.Dto)
				return tree;

			return _viewModelFactory.Create(tree);
		}

		public string GetEtag(IFile file)
		{
			return ETagHelper.GetEtagFromFile(file, includeAssemblyDate: true);
		}

		public void AddToZip(IFile file, ZipArchive zipFile)
		{
			var data = _GetValue(file);

			var entry = zipFile.CreateEntry(file.GetFileNameWithoutExtension() + ".json");

			using (var stream = entry.Open())
			using (var writer = new JsonTextWriter(new System.IO.StreamWriter(stream)) { CloseOutput = false })
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