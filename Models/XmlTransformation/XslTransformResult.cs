using FamilyTree.Models.FileSystem;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Xsl;

namespace FamilyTree.Models.XmlTransformation
{
	public class XslTransformResult : ActionResult
	{
		private const string _xslPath = @"~/Xsl/ft.xsl";

		private readonly System.IO.StringWriter _log;
		private readonly IFile _file;
		private readonly IFileSystem _fileSystem;

		private static readonly XsltSettings _xsltSettings = new XsltSettings
		{
			EnableDocumentFunction = true
		};

		public XslTransformResult(IFileSystem fileSystem, IFile file)
		{
			_fileSystem = fileSystem;
			_file = file;
			_log = new System.IO.StringWriter();
		}

		public override void ExecuteResult(ControllerContext context)
		{
			var request = context.HttpContext.Request;
			var response = context.HttpContext.Response;
			var server = context.HttpContext.Server;

			var resolver = new FileNotFoundSwallowingXmlResolver(_fileSystem, _log);

			var transform = new XslCompiledTransform(true);
			transform.Load(server.MapPath(_xslPath), _xsltSettings, resolver);

			using (var writer = new System.IO.StringWriter())
			using (var xmlWriter = XmlWriter.Create(writer, transform.OutputSettings))
			{
				var xslArguments = new XsltArgumentList();
				xslArguments.AddParam("viewContext", "", _file.GetFileNameWithoutExtension());

				using (var stream = _file.OpenRead())
				using (var reader = XmlReader.Create(stream))
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