using FamilyTree.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FamilyTree.ViewModels
{
	public class TreeViewModel
	{
		private readonly Tree _tree;

		public TreeViewModel(Tree tree)
		{
			_tree = tree;
		}

		public string Family
		{
			get { return _tree.Family; }
		}

		public PersonViewModel Person
		{
			get { return new PersonViewModel(_tree.Person, 0); }
		}
	}
}