using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Configuration;
using Newtonsoft.Json;
using System.Web;
using System.Linq;

namespace FamilyTree.Models.FileSystem
{
	public class HttpFileSystem : IFileSystem
	{
		private static readonly Uri _configuredBaseUri = _GetConfiguredBaseUri();

		private readonly Uri _baseUri;
		private readonly JsonSerializer _serialiser;
		private readonly HttpClient _webClient;

		public HttpFileSystem(Uri baseUri = null, JsonSerializer serialiser = null)
		{
			_baseUri = baseUri ?? _configuredBaseUri;
			_serialiser = serialiser ?? new JsonSerializer();
			_serialiser.Converters.Add(new FileJsonConverter(this));
			_serialiser.Converters.Add(new DirectoryJsonConverter(this));

			_webClient = new HttpClient();
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
				var jsonData = _webClient.Get(uri);

				_AssertJsonContentType(jsonData, uri);

				return _Deserialise<IFile>(jsonData);
			}
			catch (Exception exc)
			{
				return _HandleException(exc, uri, File.Null);
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
				var jsonData = _webClient.Get(uri);

				_AssertJsonContentType(jsonData, uri);

				return _Deserialise<IDirectory>(jsonData);
			}
			catch (Exception exc)
			{
				return _HandleException(exc, uri, Directory.Null);
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
				var jsonData = _webClient.Get(uri);

				_AssertJsonContentType(jsonData, uri);

				return _Deserialise<IFile[]>(jsonData);
			}
			catch (Exception exc)
			{
				return _HandleException(exc, uri, new IFile[0]);
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
				var jsonData = _webClient.Get(uri);

				_AssertJsonContentType(jsonData, uri);

				return _Deserialise<IDirectory[]>(jsonData);
			}
			catch (Exception exc)
			{
				return _HandleException(exc, uri, new IDirectory[0]);
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
				return _webClient.Get(uri).Body;
			}
			catch (Exception exc)
			{
				return _HandleException(exc, uri, Stream.Null);
			}
		}

		public bool FileExists(string path)
		{
			var file = GetFile(path);
			return file != File.Null;
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
					_webClient.Post(uri, stream);
				}
				catch (Exception exc)
				{
					_HandleException(exc, uri, Stream.Null);
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

		private static string _PathToRoot(IFile file)
		{
			return _PathToRoot(file.Directory) + "/" + file.Name;
		}

		private static string _PathToRoot(IDirectory directory)
		{
			if (directory == null)
				return "~";

			var parentDirectory = _PathToRoot(directory.Parent);
			if (string.IsNullOrEmpty(parentDirectory))
				return "~/" + directory.Name;

			return parentDirectory + "/" + directory.Name;
		}

		private static void _AssertJsonContentType(HttpResponse responseStream, Uri uri)
		{
			var contentType = responseStream.ContentType;
			if (string.IsNullOrEmpty(contentType))
				throw new InvalidOperationException("ContentType header not returned - " + uri + "\r\n" + _GetResponseData(responseStream));

			if (!contentType.Contains("application/json"))
				throw new FormatException("Not json content returned - " + uri + "\r\n" + _GetResponseData(responseStream));
		}

		private static string _GetResponseData(HttpResponse stream)
		{
			return new StreamReader(stream.Body).ReadToEnd();
		}

		private T _Deserialise<T>(HttpResponse jsonData)
		{
			using (var reader = new JsonTextReader(new StreamReader(jsonData.Body)))
				return _serialiser.Deserialize<T>(reader);
		}

		private static T _HandleException<T>(Exception exception, Uri uri, T notFoundResponse)
			where T : class
		{
			var webException = exception as WebException;
			if (webException != null)
				return _HandleWebException(webException, uri, notFoundResponse);

			var aggregateException = exception as AggregateException;
			if (aggregateException != null)
			{
				if (aggregateException.InnerExceptions.Count == 1)
					return _HandleException(aggregateException.InnerExceptions.Single(), uri, notFoundResponse);
			}

			throw exception;
		}

		private static T _HandleWebException<T>(WebException exception, Uri uri, T notFoundResponse)
			where T : class
		{
			var httpResponse = exception.Response as HttpWebResponse;
			if (httpResponse == null)
				throw exception;

			if (httpResponse.StatusCode == HttpStatusCode.NotFound)
				return notFoundResponse;

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