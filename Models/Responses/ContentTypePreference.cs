using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;

namespace FamilyTree.Models.Responses
{
	public class ContentTypePreference : IEnumerable<string>
	{
		private readonly string _acceptHeader;

		public ContentTypePreference(HttpRequestBase request)
		{
			if (!string.IsNullOrEmpty(request.QueryString["format"]))
				_acceptHeader = request.QueryString["format"];
			else
				_acceptHeader = request.Headers["Accept"];
		}

		public bool HasPreference
		{
			get { return !string.IsNullOrEmpty(_acceptHeader); }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<string> GetEnumerator()
		{
			var orderedContentTypes = from contentType in _GetAcceptableContentTypes()
									  orderby contentType.Quality descending
									  select contentType;

			return orderedContentTypes.Select(ct => ct.MediaType).GetEnumerator();
		}

		private IEnumerable<MediaTypeWithQualityHeaderValue> _GetAcceptableContentTypes()
		{
			foreach (var acceptableContentType in _acceptHeader.Split(','))
			{
				var match = Regex.Match(acceptableContentType, @"^(?<mimeType>.+?)(\s*;\s*q\=(?<preference>.*))?$");
			
				if (!match.Success)
					continue;

				var mimeType = match.Groups["mimeType"].Value;
				if (mimeType == "*/*")
					continue;
			
				var preferenceValue = match.Groups["preference"].Value;
				var preference = _GetPreference(preferenceValue);
				yield return new MediaTypeWithQualityHeaderValue(match.Groups["mimeType"].Value, preference);
			}
		}

		private static double _GetPreference(string value)
		{
			if (string.IsNullOrEmpty(value))
				return 1;
			
			if (value == "*")
				return 1;
			
			return double.Parse(value);
		}
	}
}