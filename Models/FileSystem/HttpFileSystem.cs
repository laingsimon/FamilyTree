using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Configuration;
using Newtonsoft.Json;
using System.Web;

namespace FamilyTree.Models.FileSystem
{
	public class HttpFileSystem : IFileSystem
	{
		private static readonly Uri _configuredBaseUri = _GetConfiguredBaseUri();

		private readonly Uri _baseUri;
		private readonly JsonSerializer _serialiser;
		private readonly WebClient _webClient;

		public HttpFileSystem(Uri baseUri = null, JsonSerializer serialiser = null)
		{
			_baseUri = baseUri ?? _configuredBaseUri;
			_serialiser = serialiser ?? new JsonSerializer();
			_serialiser.Converters.Add(new FileJsonConverter(this));
			_serialiser.Converters.Add(new DirectoryJsonConverter(this));

			_webClient = new RequestMirroringWebClient();
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
			var uri = new Uri(
				_baseUri,
				string.Format(
					"FileSystem/File?path={0}",
					HttpUtility.UrlEncode(path)));
			try
			{
				var jsonData = _webClient.OpenRead(uri);

				_AssertJsonContentType(jsonData, uri, _webClient);

				return _Deserialise<IFile>(jsonData);
			}
			catch (WebException exc)
			{
				_HandleWebException(exc, uri);
				throw;
			}
		}

		public IDirectory GetDirectory(string path)
		{
			var uri = new Uri(
				_baseUri,
				string.Format(
					"FileSystem/Directory?path={0}",
					HttpUtility.UrlEncode(path)));
			try
			{
				var jsonData = _webClient.OpenRead(uri);

				_AssertJsonContentType(jsonData, uri, _webClient);

				return _Deserialise<IDirectory>(jsonData);
			}
			catch (WebException exc)
			{
				_HandleWebException(exc, uri);
				throw;
			}
		}

		public IEnumerable<IFile> GetFiles(IDirectory directory, string searchPattern)
		{
			var uri = new Uri(
				_baseUri,
				string.Format(
					"FileSystem/Files?directoryPath={0}&searchPattern={1}",
					HttpUtility.UrlEncode(_PathToRoot(directory)),
					HttpUtility.UrlEncode(searchPattern)));
			try
			{
				var jsonData = _webClient.OpenRead(uri);

				_AssertJsonContentType(jsonData, uri, _webClient);

				return _Deserialise<IFile[]>(jsonData);
			}
			catch (WebException exc)
			{
				_HandleWebException(exc, uri);
				throw;
			}
		}

		public IEnumerable<IDirectory> GetDirectories(IDirectory directory)
		{
			var uri = new Uri(
				_baseUri,
				string.Format(
					"FileSystem/Directories?directoryPath={0}",
					HttpUtility.UrlEncode(_PathToRoot(directory))));
			try
			{
				var jsonData = _webClient.OpenRead(uri);

				_AssertJsonContentType(jsonData, uri, _webClient);

				return _Deserialise<IDirectory[]>(jsonData);
			}
			catch (WebException exc)
			{
				_HandleWebException(exc, uri);
				throw;
			}
		}

		public Stream OpenRead(IFile file)
		{
			var uri = new Uri(
				_baseUri,
				string.Format(
					"FileSystem/FileContent?path={0}",
					HttpUtility.UrlEncode(_PathToRoot(file))));
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
				var uri = new Uri(
					_baseUri,
					string.Format(
						"FileSystem/FileContent?path={0}",
						HttpUtility.UrlEncode(_PathToRoot(file))));
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

		private string _PathToRoot(IFile file)
		{
			return _PathToRoot(file.Directory) + "\\" + file.Name;
		}

		private string _PathToRoot(IDirectory directory)
		{
			if (directory == null)
				return "";

			var parentDirectory = _PathToRoot(directory.Parent);
			if (string.IsNullOrEmpty(parentDirectory))
				return directory.Name;

			return parentDirectory + "\\" + directory.Name;
		}

		private void _AssertJsonContentType(Stream responseStream, Uri uri, WebClient webClient)
		{
			var contentType = webClient.ResponseHeaders["Content-Type"];
			if (string.IsNullOrEmpty(contentType))
				throw new InvalidOperationException("ContentType header not returned - " + uri.ToString() + "\r\n" + _GetResponseData(responseStream));

			if (!contentType.Contains("application/json"))
				throw new FormatException("Not json content returned - " + uri.ToString() + "\r\n" + _GetResponseData(responseStream));
		}

		private string _GetResponseData(Stream stream)
		{
#if DEBUG
			using (var streamReader = new StreamReader(stream))
			{
				var nonJsonData = streamReader.ReadToEnd();

				return nonJsonData.Substring(0, Math.Min(nonJsonData.Length, 1024));
			}
#else
			return "";
#endif
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
            {
                var filePath = HttpUtility.UrlDecode(uri.Query.Replace("?path=", ""));
                throw new FileNotFoundException("File not found on " + uri.Host + " - '" + filePath + "'");
            }

			if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
				throw new InvalidOperationException("Bad request: " + uri);

			if (httpResponse.StatusCode == HttpStatusCode.LengthRequired)
				throw new InvalidOperationException("No file data sent");

            if (httpResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var message = reader.ReadToEnd();
                    throw new InvalidOperationException("Service unavailable: " + message);
                }
            }

			throw new InvalidOperationException("Unable to process request - " + uri, exception);
		}
	}
}