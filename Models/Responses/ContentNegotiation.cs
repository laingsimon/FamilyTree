using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Responses
{
	public class ContentNegotiation : IEnumerable<string>
	{
		private readonly Dictionary<string, IContentResponder> _responders =
			new Dictionary<string, IContentResponder>(StringComparer.OrdinalIgnoreCase);
		private readonly IContentResponder _defaultResponder;

		public ContentNegotiation(IContentResponder defaultResponder)
		{
			_defaultResponder = defaultResponder;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotSupportedException();
		}

		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			throw new NotSupportedException();
		}

		public void Add(string mimeType, IContentResponder responder)
		{
			_responders.Add(mimeType, responder);
		}

		public IContentResponder GetMostAppropriateResponder(ContentTypePreference preference)
		{
			if (!preference.HasPreference && _defaultResponder != null)
				return _defaultResponder;

			foreach (var mimeType in preference)
			{
				if (_responders.ContainsKey(mimeType))
					return _responders[mimeType];
			}

			return new _UnacceptableResponder(_responders.Keys);
		}

		private class _UnacceptableResponder : IContentResponder
		{
			private readonly IEnumerable<string> _acceptableContentTypes;

			public _UnacceptableResponder(IEnumerable<string> acceptableContentTypes)
			{
				_acceptableContentTypes = acceptableContentTypes;
			}

			public ActionResult GetResponse(string fileName, HttpContextBase context)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;

				return new ContentResult
				{
					ContentType = "text/plain",
					Content = string.Format("Acceptable content types: {0}", string.Join(", ", _acceptableContentTypes))
				};
			}

			public string GetEtag(string fileName)
			{
				return null;
			}
		}
	}
}