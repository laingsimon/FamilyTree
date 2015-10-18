using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
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

		private HttpClient(HttpRequest request)
		{
			_cookies = request.Cookies;
		}

		public HttpResponse Get(Uri uri)
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

				_throttler.WaitAsync();

				Trace.TraceInformation("Request: {0}", request.RequestUri);

				var response = request.GetResponseAsync().Result;

				Trace.TraceInformation("Response to: {0}", request.RequestUri);
				_throttler.Release();
				Trace.TraceInformation("Slot released");

				var cacheEntry = new HttpResponse(request.RequestUri, (HttpWebResponse)response);
				_cache.Insert(request.RequestUri.ToString(), cacheEntry, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1));
				Trace.TraceInformation("Data cached for {0}", request.RequestUri);

				return cacheEntry;
			}
			catch (Exception exc)
			{
				_throttler.Release();

				Trace.TraceWarning("Error received for {0}: {1}", request.RequestUri, _GetMessage(exc));
				_LogWebException(request, exc as WebException);

				return _HandleException(exc, cachedResponse);
			}
		}

		private string _GetMessage(Exception exc)
		{
			var aggregateException = exc as AggregateException;
			if (aggregateException != null)
			{
				if (aggregateException.InnerExceptions.Count != 1)
					return string.Format("{0} errors occurred", aggregateException.InnerExceptions.Count);

				return _GetMessage(aggregateException.InnerExceptions[0]);
			}

			var webException = exc as WebException;
			if (webException != null)
				return string.Format("{0}: {1}", webException.Status, webException.Message);

			return string.Format("{0}: {1}", exc.GetType().Name, exc.Message);
		}

		private HttpResponse _HandleException(AggregateException exc, HttpResponse cachedResponse)
		{
			if (exc.InnerExceptions.Count == 1)
				return _HandleException(exc.InnerExceptions[0], cachedResponse);

			throw exc;
		}

		private static HttpResponse _HandleException(WebException exc, HttpResponse cachedResponse)
		{
			if (cachedResponse == null)
				throw exc;

			if (_IsNotModified(exc))
				return cachedResponse;

			throw exc;
		}

		private HttpResponse _HandleException(Exception exc, HttpResponse cachedResponse)
		{
			var aggregateException = exc as AggregateException;
			if (aggregateException != null)
				return _HandleException(aggregateException, cachedResponse);
			var webException = exc as WebException;
			if (webException != null)
				return _HandleException(webException, cachedResponse);

			throw exc;
		}

		private static bool _IsNotModified(WebException exc)
		{
			var webResponse = exc.Response as HttpWebResponse;
			return webResponse != null && webResponse.StatusCode == HttpStatusCode.NotModified;
		}

		// ReSharper disable UnusedMethodReturnValue.Global
		public HttpResponse Post(Uri uri, Stream data)
		// ReSharper restore UnusedMethodReturnValue.Global
		{
			var request = WebRequest.CreateHttp(uri);
			request.Method = "POST";
			_AddCookiesToRequest(request, _cookies);

			try
			{
				var requestStream = request.GetRequestStream();
				data.CopyTo(requestStream);
				requestStream.Close();

				var response = (HttpWebResponse)request.GetResponseAsync().Result;
				return new HttpResponse(uri, response);
			}
			catch (Exception exc)
			{
				return _HandleException(exc, null);
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

		private static void _LogWebException(WebRequest request, WebException exc)
		{
			if (exc == null)
				return;

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

		private static string _GetResponseBody(Stream stream)
		{
			if (stream == null || !stream.CanRead)
				return null;

			using (var reader = new StreamReader(stream))
				return reader.ReadToEnd();
		}
	}
}