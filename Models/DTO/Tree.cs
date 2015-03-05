using FamilyTree.ViewModels;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FamilyTree.Models.DTO
{
	public class Tree
	{
		[XmlAttribute]
		public string Family { get; set; }

		public Person Person { get; set; }

		public IReadOnlyCollection<Person> FindChildren(string fromHandle, string toHandle)
		{
			var viewModel = new OtherTreeViewModel(fromHandle, toHandle, "");
			return Person.FindChildren(viewModel);
		}
	}
}