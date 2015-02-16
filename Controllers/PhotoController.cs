using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace FamilyTree.Controllers
{
	public class PhotoController : Controller
	{
		private static readonly string _defaultPhotoPath = ConfigurationManager.AppSettings["DefaultPhotoPath"];

		public ActionResult Index(string family, string firstName, string middleName, string dob, string size)
		{
			var middleNamePart = string.IsNullOrEmpty(middleName) || middleName == "-"
				? ""
				: "-" + middleName;

			var dateOfBirth = string.IsNullOrEmpty(dob) || dob == "-"
				? DateTime.MinValue
				: _TryParseExact(dob, "d-M-yyyy");
			var fileName = string.Format("~/Photos/{0}{1}-{2}_{3:ddMMyyyy}.jpg", firstName, middleNamePart, family, dateOfBirth);
			var photoFile = new FileInfo(Server.MapPath(fileName));
			if (!photoFile.Exists)
				return RedirectToAction("Unknown", new { size });

			return _ProcessPhoto(size, photoFile);
		}

		public ActionResult Unknown(string size)
		{
			var photoFile = new FileInfo(Server.MapPath(_defaultPhotoPath));
			return _ProcessPhoto(size, photoFile);
		}

		private ActionResult _ProcessPhoto(string size, FileInfo photoFile)
		{
			if (_NotModified(photoFile, Request.Headers["If-None-Match"], size))
				return new HttpStatusCodeResult(HttpStatusCode.NotModified);

			if (string.IsNullOrEmpty(size))
			{
				_AddETagHeader(photoFile, size);
				return File(photoFile.FullName, "image/jpeg");
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

		private ActionResult _ResizePhoto(FileInfo photoFile, string size)
		{
			if (!photoFile.Exists)
				return HttpNotFound();
			
			using (var bitmap = Image.FromFile(photoFile.FullName))
			{
				var desiredSize = _ParseSize(size, bitmap.Size);
				if (desiredSize.IsEmpty)
				{
					_AddETagHeader(photoFile, size);
					return new HttpStatusCodeResult(204);
				}

				var stream = new MemoryStream();

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

					_AddETagHeader(photoFile, size);
					return File(stream.ToArray(), "image/jpeg");
				}
			}
		}

		private static bool _NotModified(FileInfo photoFile, string matchEtag, string customEtagSuffix)
		{
			if (string.IsNullOrEmpty(matchEtag))
				return false;

			var currentEtag = _GenerateEtag(photoFile, customEtagSuffix);
			return currentEtag == matchEtag;
		}

		private void _AddETagHeader(FileInfo photoFile, string customEtagSuffix)
		{
			var etag = _GenerateEtag(photoFile, customEtagSuffix);
			Response.AddHeader("ETag", etag);
		}

		private static string _GenerateEtag(FileInfo photoFile, string customEtagSuffix)
		{
			var lastWriteTime = photoFile.LastWriteTimeUtc;
			var size = photoFile.Length;
			var fileName = photoFile.FullName;
			var customEtagSuffixHashCode = (customEtagSuffix ?? "").GetHashCode();

			var etag = string.Format(
				"\"{0}\"",
				lastWriteTime.GetHashCode() + size.GetHashCode() + fileName.GetHashCode() + customEtagSuffixHashCode);
			return etag;
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
