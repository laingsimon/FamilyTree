using FamilyTree.Models.FileSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FamilyTree.Models
{
	public class TreeVisitor
	{
		private readonly IReadOnlyCollection<TreeVisit> _visits;
		private readonly HashSet<IFile> _visitedFiles = new HashSet<IFile>();

		public TreeVisitor(params TreeVisit[] visits)
		{
			_visits = visits;
		}

		public void Visit(IFile treeFile, ITreeVisitee visitee)
		{
			if (treeFile == null || treeFile == File.Null)
				return;

			if (_visitedFiles.Contains(treeFile))
				return;

			visitee.Visit(treeFile);
			_visitedFiles.Add(treeFile);

			XDocument xDocument;
			try
			{
				using (var stream = treeFile.OpenRead())
				{
					if (stream == System.IO.Stream.Null)
						throw new InvalidOperationException("Could not load file for " + treeFile.Name);
					xDocument = XDocument.Load(stream);
				}
			}
			catch (XmlException exc)
			{
				throw new XmlException("Error loading '" + treeFile.Name + "'", exc);
			}

			foreach (var visit in _visits)
			{
				foreach (var node in xDocument.XPathSelectElements(visit.XPath))
				{
					var subTreeName = ((IEnumerable)node.XPathEvaluate(visit.TreeNameSelector)).Cast<XAttribute>().Single().Value;
					if (string.IsNullOrEmpty(subTreeName) || subTreeName.Contains('?'))
						continue;

					try
					{
						var subTreeFiles = treeFile.Directory.GetFiles(subTreeName + ".xml").ToArray();

						if (subTreeFiles.Length > 1)
							throw new InvalidOperationException("Multiple (" + subTreeFiles.Length + ") files found for name " + subTreeName + ".xml");

						var subTreeFile = subTreeFiles.SingleOrDefault();

						if (subTreeFile != null && subTreeFile != File.Null)
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