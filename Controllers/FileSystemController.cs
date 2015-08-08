using System.IO;
using System.Net;
using System.Web.Mvc;
using FamilyTree.Models.Authentication;
using FamilyTree.Models.FileSystem;
using FamilyTree.Models.FileSystem.AzureStorage;
using JsonResult = FamilyTree.Models.JsonResult;

namespace FamilyTree.Controllers
{
	[Authorize(Roles = Roles.SuperUser)]
	public class FileSystemController : Controller
	{
		private readonly IFileSystem _fileSystem;

		public FileSystemController()
			: this(new AzureStorageFileSystem())
		{ }

		public FileSystemController(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		[HttpGet]
		public ActionResult File(string path)
		{
			if (string.IsNullOrEmpty(path))
				return HttpNotFound();
			if (!path.StartsWith("~"))
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var file = _fileSystem.GetFile(path);
			if (file == null)
				return HttpNotFound();

			return new JsonResult(file);
		}

		[HttpGet]
		public ActionResult Directory(string path)
		{
			if (string.IsNullOrEmpty(path))
				return HttpNotFound();
			if (!path.StartsWith("~"))
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var directory = _fileSystem.GetDirectory(path);
			if (directory == null)
				return HttpNotFound();

			return new JsonResult(directory);
		}

		[HttpGet]
		public ActionResult Files(string directoryPath, string searchPattern)
		{
			if (string.IsNullOrEmpty(directoryPath))
				return HttpNotFound();
			if (!directoryPath.StartsWith("~"))
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var directory = _fileSystem.GetDirectory(directoryPath);

			if (directory == null)
				return HttpNotFound();

			var files = _fileSystem.GetFiles(directory, searchPattern);
			if (files == null)
				return HttpNotFound();

			return new JsonResult(files);
		}

		[HttpGet]
		public ActionResult Directories(string directoryPath)
		{
			if (string.IsNullOrEmpty(directoryPath))
				return HttpNotFound();
			if (!directoryPath.StartsWith("~"))
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var directory = _fileSystem.GetDirectory(directoryPath);

			if (directory == null)
				return HttpNotFound();

			var directories = _fileSystem.GetDirectories(directory);
			if (directories == null)
				return HttpNotFound();

			return new JsonResult(directories);
		}

		[HttpGet]
		public ActionResult FileContent(string path)
		{
			if (string.IsNullOrEmpty(path))
				return HttpNotFound();
			if (!path.StartsWith("~"))
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var file = _fileSystem.GetFile(path);
			if (file == null)
				return HttpNotFound();

			return new FileStreamResult(file.OpenRead(), "application/octet-stream");
		}

		[HttpPost]
		[Authorize(Users="simon")]
		public ActionResult FileContent(string path, Stream newContent)
		{
			if (string.IsNullOrEmpty(path))
				return HttpNotFound();
			if (!path.StartsWith("~"))
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			if (newContent == null)
				newContent = Request.InputStream;
			if (newContent == null)
				return new HttpStatusCodeResult(HttpStatusCode.LengthRequired);

			var file = _fileSystem.GetFile(path);
			if (file == null)
				return HttpNotFound();

			using (var writeStream = file.OpenWrite())
				newContent.CopyTo(writeStream);

			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}
	}
}