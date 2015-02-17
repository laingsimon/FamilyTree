using System;
using System.IO;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FamilyTree.Models.XmlTransformation
{
	public class FileNotFoundSwallowingXmlResolver : XmlResolver
	{
		private readonly Func<string, string> _mapPath;
		private readonly TextWriter _log;
		private readonly XmlUrlResolver _resolver;
		private readonly XPathNavigator _fileNotFound;

		public FileNotFoundSwallowingXmlResolver(Func<string, string> mapPath, TextWriter log)
		{
			_mapPath = mapPath;
			_log = log;
			_resolver = new XmlUrlResolver();

			var fileNotFound = new XDocument();
			_fileNotFound = fileNotFound.CreateNavigator();
		}

		public override Uri ResolveUri(Uri baseUri, string relativeUri)
		{
			_log.WriteLine("Resolving uri: {0}, {1}", baseUri, relativeUri);
			return _resolver.ResolveUri(baseUri, relativeUri);
		}

		public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			_log.WriteLine("Getting entity: {0}, {1}, {2}", absoluteUri, role, ofObjectToReturn);
			var path = HttpUtility.UrlDecode(absoluteUri.AbsolutePath);

			if (path.Contains("~/"))
			{
				var relativePath = path.Substring(path.IndexOf("~/"));
				path = _mapPath(relativePath);
				absoluteUri = new Uri(path);
			}

			var extension = Path.GetExtension(path);
			var isXsl = extension.Equals(".xsl", StringComparison.OrdinalIgnoreCase);

			if (File.Exists(path) || isXsl)
				return _resolver.GetEntity(absoluteUri, role, ofObjectToReturn);

			_log.WriteLine("FileNotFound: {0}", path);
			return _fileNotFound;
		}
	}
}