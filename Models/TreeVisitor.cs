using System;
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
		private readonly IReadOnlyCollection<TreeVisit> _visits;
		private readonly HashSet<string> _visitedFiles = new HashSet<string>();

		public TreeVisitor(params TreeVisit[] visits)
		{
			_visits = visits;
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
			foreach (var visit in _visits)
			{
				foreach (var node in xDocument.XPathSelectElements(visit.XPath))
				{
					var subTreeName = ((IEnumerable)node.XPathEvaluate(visit.TreeNameSelector)).Cast<XAttribute>().Single().Value;
					if (string.IsNullOrEmpty(subTreeName) || subTreeName.Contains('?'))
						continue;

					try
					{
						var subTreeFile = new FileInfo(Path.Combine(treeFile.DirectoryName, subTreeName + ".xml"));
						Visit(subTreeFile, visitee);
					}
					catch (ArgumentException exc)
					{
						throw new ArgumentException("Error accessing subTree '" + subTreeName + "' from " + treeFile.Name, exc);
					}
				}
			}
		}
	}
}