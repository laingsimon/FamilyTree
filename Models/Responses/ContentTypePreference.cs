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
			_acceptHeader = string.IsNullOrEmpty(request.QueryString["format"])
				? request.Headers["Accept"]
				: request.QueryString["format"];
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
			return from acceptableContentType in _acceptHeader.Split(',')
				   let match = Regex.Match(acceptableContentType, @"^(?<mimeType>.+?)(\s*;\s*q\=(?<preference>.*))?$")
				   let mimeType = match.Groups["mimeType"].Value
				   where match.Success && mimeType != "*/*"
				   let preference = _GetPreference(match.Groups["preference"].Value)
				   select new MediaTypeWithQualityHeaderValue(match.Groups["mimeType"].Value, preference);
		}

		private static double _GetPreference(string value)
		{
			if (string.IsNullOrEmpty(value) || value == "*")
				return 1;
			
			return double.Parse(value);
		}
	}
}