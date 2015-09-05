using System;
using System.IO;
using System.Net;

namespace FamilyTree.Models
{
	public class HttpResponse
	{
		private readonly byte[] _body;
		private readonly Uri _uri;
		private readonly DateTime _lastModified;
		private readonly WebHeaderCollection _headers;
		private readonly DateTimeOffset _cachedTime;

		public HttpResponse(Uri requestUri, HttpWebResponse response)
		{
			_uri = requestUri;
			_body = _ReadBody(response.GetResponseStream());
			_lastModified = response.LastModified;
			_headers = response.Headers;
			_cachedTime = DateTimeOffset.UtcNow;
		}

		public DateTimeOffset CachedTime
		{
			get { return _cachedTime; }
		}

		public Uri Uri
		{
			get { return _uri; }
		}

		public DateTime LastModified
		{
			get { return _lastModified; }
		}

		public string Etag
		{
			get { return _headers[HttpResponseHeader.ETag]; }
		}

		public WebHeaderCollection Headers
		{
			get { return _headers; }
		}

		public string ContentType
		{
			get { return _headers[HttpResponseHeader.ContentType]; }
		}

		public Stream Body
		{
			get { return new MemoryStream(_body); }
		}

		private static byte[] _ReadBody(Stream stream)
		{
			var memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);

			return memoryStream.ToArray();
		}
	}
}