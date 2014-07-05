using System;
using System.Web;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Text;

namespace FamilyTree
{
	public class TransformHandler : IHttpHandler
	{
        private const string _xslPath = @"~/Render/ft.xsl";
        private static readonly XsltSettings _xsltSettings = new XsltSettings
        {
            EnableDocumentFunction = true
        };

		public void ProcessRequest(HttpContext context)
		{
            var request = context.Request;
            var url = request.Url;
            var withoutQueryString = url.PathAndQuery.Replace("?" + url.Query, "");
            var path = withoutQueryString.Substring(request.ApplicationPath.Length);
            var fileName = context.Server.MapPath("~" + context.Server.UrlDecode(path));

            if (Path.GetExtension(fileName) == "")
                fileName += ".xml";

            var xslPath = context.Server.MapPath(_xslPath);

            if (!File.Exists(fileName))
            {
                context.Response.Headers.Add("FileName", fileName);
                context.Response.StatusCode = 404; 
                return;
            } else if (!Path.GetExtension(fileName).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = _GetContentType(Path.GetExtension(fileName));
                context.Response.WriteFile(fileName);
                return;
            }

            var log = new StringWriter();
            try
            {
                var html = _TransformFile(xslPath, fileName, log);

                if (request.QueryString["log"] == "1")
                {
                    _WriteLog(log.GetStringBuilder(), context.Response);
                    return;
                }

                context.Response.ContentType = "text/html";
                context.Response.Write(html);
            }
            catch (System.Exception exc)
            {
                _WriteLog(log.GetStringBuilder(), context.Response);
                context.Response.StatusCode = 500;
                context.Response.Write(exc.ToString() + "\r\n\r\n");
            }
		}

        private static string _GetContentType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".css": return "text/css";
                case ".js": return "text/javascript";
                case ".jpg": return "image/jpg";
                default:
                    return "application/octet-stream";
            }
        }

        private void _WriteLog(StringBuilder log, HttpResponse response)
        {
            response.ContentType = "text/plain";
            response.Write(log.ToString());
        }
		
		private string _TransformFile(string xsl, string fileName, TextWriter log)
		{
            var transform = new XslCompiledTransform(true);
            var resolver = new FileNotFoundSwallowingXmlResolver(log);
            transform.Load(xsl, _xsltSettings, resolver);

            var writerSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            };
			using (var writer = new StringWriter())
			using (var xmlWriter = XmlWriter.Create(writer, transform.OutputSettings))
			{
                using (var reader = XmlReader.Create(fileName))
				    transform.Transform(reader, new XsltArgumentList(), xmlWriter, resolver);
				
				return writer.GetStringBuilder().ToString();
			}
		}
		
		public bool IsReusable
		{
			get { return true; }
		}
	}
}