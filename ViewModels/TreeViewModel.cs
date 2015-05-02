using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FamilyTree.ViewModels
{
	public class TreeViewModel
	{
		public string Name { get; set; }
		public PersonViewModel Root { get; set; }
	}
}