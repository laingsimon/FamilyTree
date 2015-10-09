using System.IO;
using System.Net;
using System.Web.Mvc;
using FamilyTree.Models.Authentication;
using FamilyTree.Models.FileSystem;
using FamilyTree.Models.FileSystem.AzureStorage;
using JsonResult = FamilyTree.Models.JsonResult;
using System.Diagnostics;
using FamilyTree.Models.FileSystem.LocalDevice;
using FamilyTree.Models;

namespace FamilyTree.Controllers
{
	[Authorize(Roles = Roles.SuperUser)]
	public class FileSystemController : Controller
	{
		private readonly IFileSystem _fileSystem;

		//public FileSystemController()
		//	: this(new AzureStorageFileSystem())
		//{ }

		public FileSystemController()
		{
			_fileSystem = new LocalDeviceFileSystem(path => Server.MapPath(path));
		}

		public FileSystemController(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		[HttpGet]
		public ActionResult File(string path)
		{
			Trace.WriteLine(string.Format("File: {0}", path), "FileSystemController");

			try
			{
				if (string.IsNullOrEmpty(path))
					return HttpNotFound();
				if (!path.StartsWith("~"))
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

				var file = _fileSystem.GetFile(path);
				if (file == null || file == Models.FileSystem.File.Null)
					return HttpNotFound();

				var currentEtag = ETagHelper.GetEtagFromFile(file);
				var ifModifiedSince = Request.Headers["If-Modified-Since"];

				if (!ETagHelper.HasChanged(Request, currentEtag) || ifModifiedSince == file.LastWriteTimeUtc.ToString("R"))
					return new HttpStatusCodeResult(HttpStatusCode.NotModified);

				Response.Headers.Add("Last-Modified", file.LastWriteTimeUtc.ToString("R"));
				ETagHelper.AddEtagHeaderToResponse(Response, currentEtag);
				return new JsonResult(file);
			}
			catch (FileNotFoundException)
			{
				return HttpNotFound();
			}
		}

		[HttpGet]
		public ActionResult Directory(string path)
		{
			Trace.WriteLine(string.Format("Directory: {0}", path), "FileSystemController");

			if (string.IsNullOrEmpty(path))
				return HttpNotFound();
			if (!path.StartsWith("~"))
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			try
			{
				var directory = _fileSystem.GetDirectory(path);
				if (directory == null || directory == Models.FileSystem.Directory.Null)
					return HttpNotFound();

				return new JsonResult(directory);
			}
			catch (DirectoryNotFoundException)
			{
				return HttpNotFound();
			}
		}

		[HttpGet]
		public ActionResult Files(string directoryPath, string searchPattern)
		{
			if (string.IsNullOrEmpty(directoryPath))
				return HttpNotFound();
			if (!directoryPath.StartsWith("~"))
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var directory = _fileSystem.GetDirectory(directoryPath);

			if (directory == null || directory == Models.FileSystem.Directory.Null)
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

			if (directory == null || directory == Models.FileSystem.Directory.Null)
				return HttpNotFound();

			var directories = _fileSystem.GetDirectories(directory);
			if (directories == null)
				return HttpNotFound();

			return new JsonResult(directories);
		}

		[HttpGet]
		public ActionResult FileContent(string path)
		{
			Trace.WriteLine(string.Format("Content: {0}", path), "FileSystemController");

			try
			{
				if (string.IsNullOrEmpty(path))
					return HttpNotFound();
				if (!path.StartsWith("~"))
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

				var file = _fileSystem.GetFile(path);
				if (file == null || file == Models.FileSystem.File.Null)
					return HttpNotFound();

				var currentEtag = ETagHelper.GetEtagFromFile(file);
				var ifModifiedSince = Request.Headers["If-Modified-Since"];

				if (!ETagHelper.HasChanged(Request, currentEtag) || ifModifiedSince == file.LastWriteTimeUtc.ToString("R"))
					return new HttpStatusCodeResult(HttpStatusCode.NotModified);

				Response.AddHeader("Last-Modified", file.LastWriteTimeUtc.ToString("R"));
				ETagHelper.AddEtagHeaderToResponse(Response, currentEtag);
				return new FileStreamResult(file.OpenRead(), "application/octet-stream");
			}
			catch (FileNotFoundException)
			{
				return HttpNotFound();
			}
		}

		[HttpPost]
		[Authorize(Users = "simon")]
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
			if (file == null || file == Models.FileSystem.File.Null)
				return HttpNotFound();

			using (var writeStream = file.OpenWrite())
				newContent.CopyTo(writeStream);

			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}
	}
}