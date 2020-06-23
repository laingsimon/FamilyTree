using System.IO;
using System.Net;
using System.Web.Mvc;
using FamilyTree.Models.Authentication;
using FamilyTree.Models.FileSystem;
using FamilyTree.Models.FileSystem.AzureStorage;
using JsonResult = FamilyTree.Models.JsonResult;
using System.Diagnostics;
// ReSharper disable RedundantUsingDirective
using FamilyTree.Models.FileSystem.LocalDevice;
// ReSharper restore RedundantUsingDirective
using FamilyTree.Models;
using System.Web;
using System;

namespace FamilyTree.Controllers
{
	public class FileSystemController : Controller
	{
		private readonly IFileSystem _fileSystem;

		// ReSharper disable UnusedMember.Global
		public FileSystemController()
		// ReSharper restore UnusedMember.Global
			: this(new CachingFileSystem(
				new AzureStorageFileSystem(),
				HttpRuntime.Cache,
				CachingFileSystem.ShouldRefreshCache(System.Web.HttpContext.Current)))
		{ }

		private FileSystemController(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		[HttpGet]
		public ActionResult File(string path)
		{
			Trace.WriteLine(string.Format("File: {0} ({1})", path, Request.Url), "FileSystemController");

			try
			{
				if (string.IsNullOrEmpty(path))
                    return new StatusContentResult("Path is null or empty", "text/plain", HttpStatusCode.NotFound);

                if (!path.StartsWith("~"))
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

				var file = _fileSystem.GetFile(path);
				if (file == null || file == Models.FileSystem.File.Null)
                    return new StatusContentResult("File not found", "text/plain", HttpStatusCode.NotFound);

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
                return new StatusContentResult("File not found", "text/plain", HttpStatusCode.NotFound);
            }
        }

		[HttpGet]
		public ActionResult Directory(string path)
		{
			Trace.WriteLine(string.Format("Directory: {0}", path), "FileSystemController");

			if (string.IsNullOrEmpty(path))
                return new StatusContentResult("Path is null or empty", "text/plain", HttpStatusCode.NotFound);
            if (!path.StartsWith("~"))
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			try
			{
				var directory = _fileSystem.GetDirectory(path);
				if (directory == null || directory == Models.FileSystem.Directory.Null)
                    return new StatusContentResult("Directory not found", "text/plain", HttpStatusCode.NotFound);

                return new JsonResult(directory);
			}
			catch (DirectoryNotFoundException)
			{
                return new StatusContentResult("Directory not found", "text/plain", HttpStatusCode.NotFound);
            }
        }

		[HttpGet]
		public ActionResult Files(string directoryPath, string searchPattern)
		{
			if (string.IsNullOrEmpty(directoryPath))
                return new StatusContentResult("Path is null or empty", "text/plain", HttpStatusCode.NotFound);
            if (!directoryPath.StartsWith("~"))
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var directory = _fileSystem.GetDirectory(directoryPath);

			if (directory == null || directory == Models.FileSystem.Directory.Null)
                return new StatusContentResult("Directory not found", "text/plain", HttpStatusCode.NotFound);

            var files = _fileSystem.GetFiles(directory, searchPattern);
			if (files == null)
                return new StatusContentResult("No files found", "text/plain", HttpStatusCode.NotFound);

            return new JsonResult(files);
		}

		[HttpGet]
		public ActionResult Directories(string directoryPath)
		{
			if (string.IsNullOrEmpty(directoryPath))
                return new StatusContentResult("Path is null or empty", "text/plain", HttpStatusCode.NotFound);
            if (!directoryPath.StartsWith("~"))
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			var directory = _fileSystem.GetDirectory(directoryPath);

			if (directory == null || directory == Models.FileSystem.Directory.Null)
                return new StatusContentResult("Directory not found", "text/plain", HttpStatusCode.NotFound);

            var directories = _fileSystem.GetDirectories(directory);
			if (directories == null)
                return new StatusContentResult("Directory not found", "text/plain", HttpStatusCode.NotFound);

            return new JsonResult(directories);
		}

		[HttpGet]
		public ActionResult FileContent(string path)
		{
			Trace.WriteLine(string.Format("Content: {0}", path), "FileSystemController");

			try
			{
				if (string.IsNullOrEmpty(path))
                    return new StatusContentResult("Path is null or empty", "text/plain", HttpStatusCode.NotFound);
                if (!path.StartsWith("~"))
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

				var file = _fileSystem.GetFile(path);
				if (file == null || file == Models.FileSystem.File.Null)
                    return new StatusContentResult("File not found", "text/plain", HttpStatusCode.NotFound);

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
                return new StatusContentResult("File not found", "text/plain", HttpStatusCode.NotFound);
			}
            catch (Exception exc)
            {
                Console.WriteLine("Error retrieving file content:\n" + Request.Url + "\n\n" + exc.Message);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
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