using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace FamilyTree.Models.DTO
{
	[DebuggerDisplay("{Name.First,nq} {Name.Last,nq}")]
	public class Person
	{
		[XmlAttribute]
		public Gender Gender { get; set; }

		public Name Name { get; set; }
		
		public Event Birth { get; set; }

		public Event Death { get; set; }

		[XmlElement("Marriage")]
		public Marriage[] Marriages { get; set; }

		public MarriageChildren Children { get; set; }

		public IReadOnlyCollection<Person> FindChildren(ViewModels.OtherTreeViewModel viewModel)
		{
			if (Children == null)
				return null;

			if (Children.EntryPoint == viewModel.EntryPoint
				|| Children.EntryPoint == viewModel.EntryPointReversed)
				return Children.People;

			if (Children.People == null)
				return null;

			foreach (var child in Children.People)
			{
				var foundChildren = child.FindChildren(viewModel);
				if (foundChildren != null)
					return foundChildren;
			}

			return null;
		}
	}
}