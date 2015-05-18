using System.IO;
using System.Web;
using System.Web.Mvc;
using FamilyTree.ViewModels;

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
	}
}