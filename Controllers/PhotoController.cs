using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
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
				: DateTime.ParseExact(dob, "dd-MM-yyyy", null);
			var fileName = string.Format("~/Photos/{0}{1}-{2}_{3:ddMMyyyy}.jpg", firstName, middleNamePart, family, dateOfBirth);
			var photoFile = new FileInfo(Server.MapPath(fileName));
			if (!photoFile.Exists)
				photoFile = new FileInfo(Server.MapPath(_defaultPhotoPath));

			if (string.IsNullOrEmpty(size))
				return File(photoFile.FullName, "image/jpeg");

			return _ResizePhoto(photoFile, size);
		}

		private ActionResult _ResizePhoto(FileInfo photoFile, string size)
		{
			if (!photoFile.Exists)
			{
				Response.AddHeader("FileName", photoFile.FullName);
				return HttpNotFound();
			}

			using (var bitmap = Image.FromFile(photoFile.FullName))
			{
				var desiredSize = _ParseSize(size, bitmap.Size);
				if (desiredSize.IsEmpty)
					return new HttpStatusCodeResult(204);

				var stream = new MemoryStream();

				var resizedImage = new Bitmap(desiredSize.Width, desiredSize.Height, PixelFormat.Format32bppArgb);
				using (var graphics = Graphics.FromImage(resizedImage))
				{
					graphics.InterpolationMode = InterpolationMode.Bicubic;
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
