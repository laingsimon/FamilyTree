using System.IO;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Xsl;

namespace FamilyTree.Models.XmlTransformation
{
	public class XslTransformResult : ActionResult
	{
		private const string _xslPath = @"~/Xsl/ft.xsl";

		private readonly string _filePath;
		private readonly StringWriter _log;

		private static readonly XsltSettings _xsltSettings = new XsltSettings
		{
			EnableDocumentFunction = true
		};

		public XslTransformResult(string filePath)
		{
			_filePath = filePath;
			_log = new StringWriter();
		}

		public override void ExecuteResult(ControllerContext context)
		{
			var request = context.HttpContext.Request;
			var response = context.HttpContext.Response;
			var server = context.HttpContext.Server;

			var resolver = new FileNotFoundSwallowingXmlResolver(server.MapPath, _log);
			
			var transform = new XslCompiledTransform(true);
			transform.Load(server.MapPath(_xslPath), _xsltSettings, resolver);

			using (var writer = new StringWriter())
			using (var xmlWriter = XmlWriter.Create(writer, transform.OutputSettings))
			{
				var xslArguments = new XsltArgumentList();
				xslArguments.AddParam("viewContext", "", Path.GetFileNameWithoutExtension(_filePath));

				using (var reader = XmlReader.Create(_filePath))
					transform.Transform(reader, xslArguments, xmlWriter, resolver);

				response.StatusCode = 200;

				if (request.QueryString["debug"] != null)
				{
					response.ContentType = "text/plain";
					response.Write(_log.GetStringBuilder().ToString());
				}
				else
				{
					response.ContentType = "text/html";
					response.Write(writer.GetStringBuilder().ToString());
				}
			}
		}
	}
}