using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FamilyTree.Models
{
	public class TreeVisitor
	{
		private readonly string _xPath;
		private readonly string _subTreeSelector;
		private readonly HashSet<string> _visitedFiles = new HashSet<string>();

		public TreeVisitor(string xPath, string subTreeSelector)
		{
			_xPath = xPath;
			_subTreeSelector = subTreeSelector;
		}

		public void Visit(FileInfo treeFile, ITreeVisitee visitee)
		{
			if (!treeFile.Exists)
				return;

			if (_visitedFiles.Contains(treeFile.FullName))
				return;

			visitee.Visit(treeFile);
			_visitedFiles.Add(treeFile.FullName);

			var xDocument = XDocument.Load(treeFile.FullName);
			foreach (var node in xDocument.XPathSelectElements(_xPath))
			{
				var subTreeName = ((IEnumerable)node.XPathEvaluate(_subTreeSelector)).Cast<XAttribute>().Single().Value;
				if (string.IsNullOrEmpty(subTreeName))
					continue;

				var subTreeFile = new FileInfo(Path.Combine(treeFile.DirectoryName, subTreeName + ".xml"));
				Visit(subTreeFile, visitee);
			}
		}
	}
}