﻿using System.IO;
using System.IO.Compression;
using FamilyTree.Models.FileSystem;
using FamilyTree.Models.Responses;

namespace FamilyTree.Models
{
	public class DataFetcher
	{
		private readonly IFileSystem _fileSystem;

		public DataFetcher(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
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
			var file = _fileSystem.GetFile(string.Format("~/Data/{0}.xml", family));

			if (file != null)
				responder.AddToZip(file, zipFile);
		}
	}
}