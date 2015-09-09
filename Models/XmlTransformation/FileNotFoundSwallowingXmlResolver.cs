using FamilyTree.Models.FileSystem;
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
		private readonly TextWriter _log;
		private readonly XmlUrlResolver _resolver;
		private readonly XPathNavigator _fileNotFound;
		private readonly IFileSystem _fileSystem;
		private readonly string _rootPath;

		public FileNotFoundSwallowingXmlResolver(IFileSystem fileSystem, TextWriter log, string rootPath)
		{
			_fileSystem = fileSystem;
			_log = log;
			_resolver = new XmlUrlResolver();
			_rootPath = rootPath;

			var fileNotFound = new XDocument();
			_fileNotFound = fileNotFound.CreateNavigator();
		}

		public override Uri ResolveUri(Uri baseUri, string relativeUri)
		{
			_log.WriteLine("Resolving uri: {0}, {1}", baseUri, relativeUri);

			if (relativeUri.StartsWith("~/"))
			{
				return new Uri(Path.Combine(_rootPath, relativeUri.Substring(2)));
			}

			return _resolver.ResolveUri(baseUri, relativeUri);
		}

		public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			_log.WriteLine("Getting entity: {0}, {1}, {2}", absoluteUri, role, ofObjectToReturn);
			var path = HttpUtility.UrlDecode(absoluteUri.AbsolutePath).Replace("/", "\\");

			if (!path.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("AbsolutePath '" + path + "' is not a sub path of '" + _rootPath + "'");

			var relativePath = path.Contains("~")
				? path.Substring(path.IndexOf("~"))
				: "~\\" + path.Substring(_rootPath.Length);
			
			var file = Path.GetFileNameWithoutExtension(relativePath) != "?" && _fileSystem.FileExists(relativePath)
				? _fileSystem.GetFile(relativePath)
				: null;

			var extension = file.GetExtension();
			var isXsl = ".xsl".Equals(extension, StringComparison.OrdinalIgnoreCase);

			if (file != null || isXsl)
				return _resolver.GetEntity(absoluteUri, role, ofObjectToReturn);

			_log.WriteLine("FileNotFound: {0}", path);
			return _fileNotFound;
		}
	}
}