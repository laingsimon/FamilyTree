using FamilyTree.Models.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models
{
	public class DataFetcher
	{
		private readonly Func<string, string> _mapPath;

		public DataFetcher(Func<string, string> mapPath)
		{
			_mapPath = mapPath;
		}

		public Stream GetData(IContentResponder responder, params string[] families)
		{
			var zipStream = new MemoryStream();
			using (var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create, false))
			{
				foreach (var family in families)
					_AddToZip(responder, family, zipFile);
			}

			var readableStream = new MemoryStream(zipStream.ToArray());
			return readableStream;
		}

		private void _AddToZip(IContentResponder responder, string family, ZipArchive zipFile)
		{
			var fileName = _mapPath(string.Format("~/Data/{0}.xml", family));

			if (File.Exists(fileName))
				responder.AddToZip(fileName, zipFile);
		}
	}
}