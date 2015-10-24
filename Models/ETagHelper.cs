using FamilyTree.Models.FileSystem;
using System;
using System.Collections.Generic;
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
			return System.IO.File.GetLastWriteTimeUtc(path);
		}

		public static string GetEtagFromFile(IFile fileInfo, string customEtagSuffix = null, bool includeAssemblyDate = false)
		{
			var lastWriteTime = GetFileWriteTimes(fileInfo).Max(fileDate => fileDate.Value);
			var size = fileInfo.Size;
			var fileName = fileInfo.Name;
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

			return "\"" + etag + "\"" != ifNoneMatch;
		}

		public static void AddEtagHeaderToResponse(HttpResponseBase response, string etag)
		{
			if (string.IsNullOrEmpty(etag))
				return;

			response.AddHeader("ETag", "\"" + etag + "\"");
		}

		public static Dictionary<string, DateTime> GetFileWriteTimes(IFile rootFile)
		{
			if (!rootFile.GetExtension().Equals(".xml", StringComparison.OrdinalIgnoreCase))
			{
				return new Dictionary<string, DateTime>
				{
					{ rootFile.Name, rootFile.LastWriteTimeUtc }
				};
			}

			var visitor = new TreeVisitor(
				new TreeVisit("//Children[@SeeOtherTree]", "@SeeOtherTree"),
				new TreeVisit("//Marriage/To", "Person/Name/@Last")
			);

			var treeDateVisitee = new TreeDateVisitee();
			visitor.Visit(rootFile, treeDateVisitee);

			return treeDateVisitee.FileDates;
		}
	}
}