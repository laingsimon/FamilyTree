using FamilyTree.Models;
using FamilyTree.Models.FileSystem;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace FamilyTree.Controllers
{
	public class PhotoController : Controller
	{
		private static readonly string _defaultPhotoPath = ConfigurationManager.AppSettings["DefaultPhotoPath"];

		private readonly IFileSystem _fileSystem;

		public PhotoController()
		{
			_fileSystem = FileSystemFactory.GetFileSystem();
		}

		public ActionResult Index(string family, string firstName, string middleName, string dob, string size)
		{
			var middleNamePart = string.IsNullOrEmpty(middleName) || middleName == "-"
				? ""
				: "-" + middleName;

			var dateOfBirth = string.IsNullOrEmpty(dob) || dob == "-"
				? DateTime.MinValue
				: _TryParseExact(dob, "d-M-yyyy");
			var fileName = string.Format("~/Photos/{0}{1}-{2}_{3:ddMMyyyy}.jpg", firstName, middleNamePart, family, dateOfBirth);
			if (!_fileSystem.FileExists(fileName))
				return RedirectToAction("Unknown", new { size });

			var photoFile = _fileSystem.GetFile(fileName);
			return _ProcessPhoto(size, photoFile);
		}

		public ActionResult Unknown(string size)
		{
			var photoFile = _fileSystem.GetFile(_defaultPhotoPath);
			return _ProcessPhoto(size, photoFile);
		}

		private ActionResult _ProcessPhoto(string size, IFile photoFile)
		{
			if (photoFile == null || photoFile == Models.FileSystem.File.Null)
				return HttpNotFound();

			var currentEtag = ETagHelper.GetEtagFromFile(photoFile, size);
			if (!ETagHelper.HasChanged(Request, currentEtag))
				return new HttpStatusCodeResult(HttpStatusCode.NotModified);

			if (string.IsNullOrEmpty(size))
			{
				ETagHelper.AddEtagHeaderToResponse(Response, currentEtag);
				return new FileStreamResult(photoFile.OpenRead(), "image/jpeg");
			}

			return _ResizePhoto(photoFile, size);
		}

		private static DateTime _TryParseExact(string dateString, string format)
		{
			DateTime date;
			return DateTime.TryParseExact(dateString, format, null, DateTimeStyles.None, out date)
				? date
				: DateTime.MinValue;
		}

		private ActionResult _ResizePhoto(IFile photoFile, string size)
		{
			using (var photoStream = photoFile.OpenRead())
			using (var bitmap = Image.FromStream(photoStream))
			{
				var currentEtag = ETagHelper.GetEtagFromFile(photoFile, size);
				ETagHelper.AddEtagHeaderToResponse(Response, currentEtag);

				var desiredSize = _ParseSize(size, bitmap.Size);
				if (desiredSize.IsEmpty)
					return new HttpStatusCodeResult(204);

				var stream = new System.IO.MemoryStream();

				var resizedImage = new Bitmap(desiredSize.Width, desiredSize.Height, PixelFormat.Format32bppArgb);
				using (var graphics = Graphics.FromImage(resizedImage))
				{
					graphics.InterpolationMode = InterpolationMode.Bicubic;
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.DrawImage(
						bitmap,
						0, 0,
						resizedImage.Width,
						resizedImage.Height);

					resizedImage.Save(stream, ImageFormat.Jpeg);

					return File(stream.ToArray(), "image/jpeg");
				}
			}
		}

		private static Size _ParseSize(string size, SizeF originalImageSize)
		{
			var match = Regex.Match(size, @"(?<width>\d+)x(?<height>\d+)|w(?<width>\d+)|h(?<height>\d+)");
			if (!match.Success)
				return Size.Empty;

			int width;
			int height;

			var hasWidth = int.TryParse(match.Groups["width"].Value, out width);
			var hasHeight = int.TryParse(match.Groups["height"].Value, out height);

			if (hasWidth && hasHeight)
				return new Size(width, height);

			//respect aspect ratio
			if (hasWidth)
				return new Size(width, (int)((width / originalImageSize.Width) * originalImageSize.Height));
			if (hasHeight)
				return new Size((int)((height / originalImageSize.Height) * originalImageSize.Width), height);

			return Size.Empty;
		}
	}
}
