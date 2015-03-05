using FamilyTree.Models.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Responses
{
	public class RazorContentResponder : IContentResponder
	{
		private readonly Controller _controller;
		private readonly Func<string, string> _mapPath;

		public RazorContentResponder(Func<string, string> mapPath, Controller controller)
		{
			_controller = controller;
			_mapPath = mapPath;
		}

		public ActionResult GetResponse(string fileName, HttpContextBase context)
		{
			using (var fileStream = new StreamReader(fileName))
			{
				var serialiser = new System.Xml.Serialization.XmlSerializer(typeof(Tree));
				var tree = (Tree)serialiser.Deserialize(fileStream);

				var result = new ViewResult
				{
					ViewName = "Family"
				};
				result.ViewData.Model = tree;

				return result;
			}
		}

		public string GetEtag(string fileName)
		{
			var razorFile = new FileInfo(_mapPath("~/Views/Tree/Family.cshtml"));
			var razorFileDateString = razorFile.LastWriteTimeUtc.ToString("yyyy-MM-dd@HH:mm:ss");

			return ETagHelper.GetEtagFromFile(new FileInfo(fileName), customEtagSuffix: razorFileDateString, includeAssemblyDate: true);
		}
	}
}