using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace FamilyTree.Models.DTO
{
	[DebuggerDisplay("{Persons.Count}")]
	public class MarriageChildren
	{
		[XmlAttribute]
		public string EntryPoint { get; set; }

		[XmlAttribute]
		public string SeeOtherTree { get; set; }

		[XmlElement("Person")]
		public List<Person> People { get; set; }
	}
}