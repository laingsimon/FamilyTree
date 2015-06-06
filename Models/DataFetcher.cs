using FamilyTree.Models.FileSystem;
using FamilyTree.Models.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		private readonly IFileSystem _fileSystem;

		public DataFetcher(IFileSystem fileSystem, Func<string, string> mapPath)
		{
			_fileSystem = fileSystem;
			_mapPath = mapPath;
		}

		public System.IO.Stream GetData(IContentResponder responder, params string[] families)
		{
			var zipStream = new System.IO.MemoryStream();
			using (var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create, false))
			{
				foreach (var family in families)
					_AddToZip(responder, family, zipFile);
			}

			var readableStream = new System.IO.MemoryStream(zipStream.ToArray());
			return readableStream;
		}

		private void _AddToZip(IContentResponder responder, string family, ZipArchive zipFile)
		{
			var file = _fileSystem.GetFile(string.Format("~/Data/{0}.xml", family));

			if (file != null)
				responder.AddToZip(file, zipFile);
		}
	}
}