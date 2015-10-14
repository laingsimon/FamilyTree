using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace FamilyTree.Models
{
	public class HttpClient
	{
		private static readonly Cache _cache = HttpRuntime.Cache;
		private static readonly SemaphoreSlim _throttler = new SemaphoreSlim(2);

		private readonly HttpCookieCollection _cookies;

		public HttpClient()
			: this(HttpContext.Current != null
				? HttpContext.Current.Request
				: null)
		{ }

		public HttpClient(HttpRequest request)
		{
			_cookies = request.Cookies;
		}

		public async Task<HttpResponse> Get(Uri uri)
		{
			var cachedResponse = (HttpResponse)_cache.Get(uri.ToString());

			var request = WebRequest.CreateHttp(uri);
			_AddCookiesToRequest(request, _cookies);
			request.Method = "GET";
			try
			{
				if (cachedResponse != null)
				{
					request.IfModifiedSince = cachedResponse.LastModified;
					request.Headers["If-None-Match"] = cachedResponse.Etag;
				}

				Trace.TraceInformation("Waiting for slot for {0}", request.RequestUri);

				await _throttler.WaitAsync();

				Trace.TraceInformation("Request: {0}", request.RequestUri);

				var response = await request.GetResponseAsync();

				Trace.TraceInformation("Response to: {0}", request.RequestUri);
				_throttler.Release();
				Trace.TraceInformation("Slot released");

				var cacheEntry = new HttpResponse(request.RequestUri, (HttpWebResponse)response);
				_cache.Insert(request.RequestUri.ToString(), cacheEntry, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1));
				Trace.TraceInformation("Data cached for {0}", request.RequestUri);

				return cacheEntry;
			}
			catch (WebException exc)
			{
				_throttler.Release();

				Trace.TraceWarning("Error received for {0}: {1}", request.RequestUri, exc.Message);

				if (_IsNotModified(exc))
				{
					Trace.TraceWarning("Data not modified for {0}, returning cached copy", request.RequestUri);
					return cachedResponse;
				}

				_HandleWebException(request, exc);
				throw;
			}
		}

		private bool _IsNotModified(WebException exc)
		{
			var webResponse = exc.Response as HttpWebResponse;
			return webResponse != null && webResponse.StatusCode == HttpStatusCode.NotModified;
		}

		public async Task<HttpResponse> Post(Uri uri, Stream data)
		{
			var request = WebRequest.CreateHttp(uri);
			request.Method = "POST";
			_AddCookiesToRequest(request, _cookies);

			try
			{
				var requestStream = request.GetRequestStream();
				data.CopyTo(requestStream);
				requestStream.Close();

				var response = (HttpWebResponse) await request.GetResponseAsync();
				return new HttpResponse(uri, response);
			}
			catch (WebException exc)
			{
				_HandleWebException(request, exc);
				throw;
			}
		}

		private static void _AddCookiesToRequest(HttpWebRequest httpRequest, HttpCookieCollection cookies)
		{
			if (httpRequest.CookieContainer == null)
				httpRequest.CookieContainer = new CookieContainer();

			foreach (string cookieName in cookies)
			{
				var cookie = cookies[cookieName];

				httpRequest.CookieContainer.Add(new Cookie
				{
					Name = cookie.Name,
					Value = cookie.Value,
					HttpOnly = cookie.HttpOnly,
					Expires = cookie.Expires == DateTime.MinValue ? DateTime.Now.AddHours(1) : cookie.Expires,
					Domain = cookie.Domain ?? httpRequest.Host
				});
			}
		}

		private void _HandleWebException(WebRequest request, WebException exc)
		{
			var response = exc.Response as HttpWebResponse;
			if (response == null)
				return;

			var responseBody = response.StatusCode != HttpStatusCode.NotFound
								? "\r\n" + _GetResponseBody(response.GetResponseStream())
								: "";
			Trace.WriteLine(
				string.Format(
					"{0}: {1}{2}",
					request.RequestUri.PathAndQuery,
					response.StatusCode,
					responseBody),
				"HttpFileSystem");
		}

		private string _GetResponseBody(Stream stream)
		{
			if (stream == null || !stream.CanRead)
				return null;

			using (var reader = new StreamReader(stream))
				return reader.ReadToEnd();
		}
	}
}