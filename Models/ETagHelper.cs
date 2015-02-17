using System;
using System.IO;

namespace FamilyTree.Models
{
	public class ETagHelper
	{
		public static string GetEtagFromFile(FileInfo fileInfo, string customEtagSuffix = null, DateTime? customDateTimePart = null)
		{
			var lastWriteTime = fileInfo.LastWriteTimeUtc;
			var size = fileInfo.Length;
			var fileName = fileInfo.FullName;
			var customEtagSuffixHashCode = customEtagSuffix == null ? "" : customEtagSuffix.GetHashCode().ToString();
			var customDateTimePartHashCode = customDateTimePart == null ? "" : customDateTimePart.GetHashCode().ToString();

			return lastWriteTime.GetHashCode()
				+ size.GetHashCode()
				+ fileName.GetHashCode()
				+ customEtagSuffixHashCode
				+ customDateTimePartHashCode;
		}
	}
}