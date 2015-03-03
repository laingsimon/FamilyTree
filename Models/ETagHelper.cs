using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FamilyTree.Models
{
	public class ETagHelper
	{
		private static readonly Lazy<DateTime> _assemblyDate = new Lazy<DateTime>(GetAssemblyDate);

		public static DateTime GetAssemblyDate()
		{
			var path = typeof(ETagHelper).Assembly.Location;
			return File.GetLastWriteTimeUtc(path);
		}

		public static string GetEtagFromFile(FileInfo fileInfo, string customEtagSuffix = null, bool includeAssemblyDate = false)
		{
			var lastWriteTime = GetFileWriteTimes(fileInfo).Max(fileDate => fileDate.Value);
			var size = fileInfo.Length;
			var fileName = fileInfo.FullName;
			var customEtagSuffixHashCode = customEtagSuffix == null ? "" : customEtagSuffix.GetHashCode().ToString();
			var customDateTimePartHashCode = includeAssemblyDate ? "" : _assemblyDate.GetHashCode().ToString();

			return lastWriteTime.GetHashCode()
				+ size.GetHashCode()
				+ fileName.GetHashCode()
				+ customEtagSuffixHashCode
				+ customDateTimePartHashCode;
		}

		public static bool HasChanged(HttpRequestBase request, string etag)
		{
			var ifNoneMatch = request.Headers["If-None-Match"];
			if (string.IsNullOrEmpty(ifNoneMatch))
				return true; //no etag in request

			return "\"" + etag + "\"" == ifNoneMatch;
		}

		public static void AddEtagHeaderToResponse(HttpResponseBase response, string etag)
		{
			if (string.IsNullOrEmpty(etag))
				return;

			response.AddHeader("ETag", "\"" + etag + "\"");
		}

		public static Dictionary<string, DateTime> GetFileWriteTimes(FileInfo rootFile)
		{
			var visitor = new TreeVisitor("//Children[@SeeOtherTree]", "@SeeOtherTree");

			var treeDateVisitee = new TreeDateVisitee();
			visitor.Visit(rootFile, treeDateVisitee);

			return treeDateVisitee.FileDates;
		}
	}
}