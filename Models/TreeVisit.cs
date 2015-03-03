using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FamilyTree.Models
{
	public class TreeVisit
	{
		private readonly string _xPath;
		private readonly string _treeNameSelector;

		public TreeVisit(string xPath, string treeNameSelector)
		{
			_xPath = xPath;
			_treeNameSelector = treeNameSelector;
		}

		public string XPath
		{
			get { return _xPath; }
		}

		public string TreeNameSelector
		{
			get { return _treeNameSelector; }
		}
	}
}