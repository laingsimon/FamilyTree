using System.Diagnostics;
using System.Xml.Serialization;

namespace FamilyTree.Models.DTO
{
	[DebuggerDisplay("{Name.First,nq} {Name.Last,nq}")]
	public class Person
	{
		// ReSharper disable UnusedAutoPropertyAccessor.Global
		[XmlAttribute]
		public Gender Gender { get; set; }

		public Name Name { get; set; }

		public Event Birth { get; set; }

		public Event Death { get; set; }

		[XmlElement("Marriage")]
		public Marriage[] Marriages { get; set; }

		public MarriageChildren Children { get; set; }
		// ReSharper restore UnusedAutoPropertyAccessor.Global
	}
}