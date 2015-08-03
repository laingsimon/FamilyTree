using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Configuration;
using Newtonsoft.Json;

namespace FamilyTree.Models.FileSystem
{
	public class HttpFileSystem : IFileSystem
	{
		private static readonly Uri _configuredBaseUri = _GetConfiguredBaseUri();

		private readonly Uri _baseUri;
		private readonly JsonSerializer _serialiser;
		private readonly WebClient _webClient;

		public HttpFileSystem(Uri baseUri = null, ICredentials credentials = null, JsonSerializer serialiser = null)
		{
			_baseUri = baseUri ?? _configuredBaseUri;
			_serialiser = serialiser ?? new JsonSerializer();

			_webClient = new WebClient
			{
				Credentials = credentials
			};
		}

		private static Uri _GetConfiguredBaseUri()
		{
			var baseUri = WebConfigurationManager.AppSettings["HttpFileSystem.BaseUri"];
			if (baseUri == null)
				throw new InvalidOperationException("HttpFileSystem.BaseUri is not configured");
			if (!baseUri.EndsWith("/"))
				baseUri += "/";

			return new Uri(baseUri, UriKind.Absolute);
		}

		public IFile GetFile(string path)
		{
			var uri = new Uri(_baseUri, string.Format("FileSystem/File?path={0}", path));
			try
			{
				var jsonData = _webClient.OpenRead(uri);
				return _Deserialise<File>(jsonData);
			}
			catch (WebException exc)
			{
				_HandleWebException(exc, uri);
				throw;
			}
		}

		public IDirectory GetDirectory(string path)
		{
			var uri = new Uri(_baseUri, string.Format("FileSystem/Directory?path={0}", path));
			try
			{
				var jsonData = _webClient.OpenRead(uri);
				return _Deserialise<Directory>(jsonData);
			}
			catch (WebException exc)
			{
				_HandleWebException(exc, uri);
				throw;
			}
		}

		public IEnumerable<IFile> GetFiles(IDirectory directory, string searchPattern)
		{
			var uri = new Uri(_baseUri, string.Format("FileSystem/Files?directoryPath={0}&searchPattern={1}", directory.Name, searchPattern));
			try
			{
				var jsonData = _webClient.OpenRead(uri);
				return _Deserialise<File[]>(jsonData);
			}
			catch (WebException exc)
			{
				_HandleWebException(exc, uri);
				throw;
			}
		}

		public IEnumerable<IDirectory> GetDirectories(IDirectory directory)
		{
			var uri = new Uri(_baseUri, string.Format("FileSystem/Directories?directoryPath={0}", directory.Name));
			try
			{
				var jsonData = _webClient.OpenRead(uri);
				return _Deserialise<Directory[]>(jsonData);
			}
			catch (WebException exc)
			{
				_HandleWebException(exc, uri);
				throw;
			}
		}

		public Stream OpenRead(IFile file)
		{
			var uri = new Uri(_baseUri, string.Format("FileSystem/FileContent?path={0}", file.Name));
			try
			{
				return _webClient.OpenRead(uri);
			}
			catch (WebException exc)
			{
				_HandleWebException(exc, uri);
				throw;
			}
		}

		public bool FileExists(string path)
		{
			try
			{
				GetFile(path);
				return true;
			}
			catch (WebException)
			{
				return false;
			}
		}

		public Stream OpenWrite(IFile file)
		{
			return new DelayedWriteStream(stream =>
			{
				var uri = new Uri(_baseUri, string.Format("FileSystem/FileContent?path={0}", file.Name));
				try
				{
					var writeStream = _webClient.OpenWrite(uri, "POST");
					stream.CopyTo(writeStream);
					writeStream.Close();
				}
				catch (WebException exc)
				{
					_HandleWebException(exc, uri);
					throw;
				}
			});
		}

		public IFile CreateFile(string path)
		{
			return new File(
				Path.GetFileName(path),
				new Directory(
					Path.GetDirectoryName(path),
					null,
					this),
				0,
				DateTime.MinValue,
				this
				);
		}

		private T _Deserialise<T>(Stream jsonData)
		{
			using (var reader = new JsonTextReader(new StreamReader(jsonData)))
				return _serialiser.Deserialize<T>(reader);
		}

		private static void _HandleWebException(WebException exception, Uri uri)
		{
			var httpResponse = exception.Response as HttpWebResponse;
			if (httpResponse == null)
				throw exception;

			if (httpResponse.StatusCode == HttpStatusCode.NotFound)
				throw new FileNotFoundException(uri.ToString());

			if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
				throw new InvalidOperationException();

			if (httpResponse.StatusCode == HttpStatusCode.LengthRequired)
				throw new InvalidOperationException("No file data sent");

			throw new InvalidOperationException("Unable to process request", exception);
		}
	}
}