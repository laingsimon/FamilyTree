using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Diagnostics;
using System.Web;

namespace FamilyTree
{
    public class FileNotFoundSwallowingXmlResolver : XmlResolver
    {
        private readonly XmlResolver _resolver;
        private readonly IXPathNavigable _fileNotFound;
        private readonly TextWriter _log;

        public FileNotFoundSwallowingXmlResolver(TextWriter log)
	    {
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
            var extension = Path.GetExtension(path);
            var isXsl = extension.Equals(".xsl", StringComparison.OrdinalIgnoreCase);

            if (File.Exists(path) || isXsl)
                return _resolver.GetEntity(absoluteUri, role, ofObjectToReturn);

            _log.WriteLine(string.Format("FileNotFound: {0}", path));
            return _fileNotFound;
        }
    }
}
