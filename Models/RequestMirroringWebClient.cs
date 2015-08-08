using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace FamilyTree.Models
{
	public class RequestMirroringWebClient : WebClient
	{
		private readonly HttpCookieCollection _cookies;

		public RequestMirroringWebClient()
			:this(HttpContext.Current != null
				? HttpContext.Current.Request
				: null)
		{ }

		public RequestMirroringWebClient(HttpRequest request)
		{
			if (request == null)
				throw new ArgumentNullException("request");

			_cookies = request.Cookies;
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			var httpRequest = (HttpWebRequest)request;
			_AddCookiesToRequest(httpRequest, _cookies);
			return base.GetWebResponse(request);
		}

		private static void _AddCookiesToRequest(HttpWebRequest httpRequest, HttpCookieCollection cookies)
		{
			if (httpRequest.CookieContainer == null)
				httpRequest.CookieContainer = new CookieContainer();

			foreach (HttpCookie cookie in cookies)
			{
				httpRequest.CookieContainer.Add(new Cookie
				{
					Name = cookie.Name,
					Value = cookie.Value,
					HttpOnly = cookie.HttpOnly,
					Expires = cookie.Expires
				});
			}
		}
	}
}