using System.Web;
using System.Web.Mvc;
using FamilyTree.ViewModels;
using System.IO.Compression;
using FamilyTree.Models.FileSystem;

namespace FamilyTree.Models.Responses
{
	public class RazorContentResponder : IContentResponder
	{
		private readonly TreeViewModelFactory _viewModelFactory;
		private readonly IFileSystem _fileSystem;
		private readonly TreeFactory _treeFactory;
		private readonly IFileSystem _localFileSystem;

		public RazorContentResponder(
			IFileSystem fileSystem,
			IFileSystem localFileSystem,
			TreeFactory treeFactory = null,
			TreeViewModelFactory viewModelFactory = null)
		{
			_fileSystem = fileSystem;
			_localFileSystem = localFileSystem;
			_treeFactory = treeFactory ?? new TreeFactory(fileSystem);
			_viewModelFactory = viewModelFactory ?? new TreeViewModelFactory(_treeFactory, new TreeParser());
		}

		public ActionResult GetResponse(IFile file, HttpContextBase context)
		{
			var tree = _treeFactory.LoadFromFile(file);
			var viewModel = _viewModelFactory.Create(tree);

			return new ViewResult
			{
				ViewData =
				{
					Model = viewModel
				}
			};
		}

		public string GetEtag(IFile file)
		{
			var razorFile = _localFileSystem.GetFile("~/Views/Tree/Family.cshtml");
			var xslFileDateString = razorFile.LastWriteTimeUtc.ToString("yyyy-MM-dd@HH:mm:ss");

			return ETagHelper.GetEtagFromFile(file, customEtagSuffix: xslFileDateString, includeAssemblyDate: true);
		}

		public void AddToZip(IFile file, ZipArchive zipFile)
		{ }
	}
}