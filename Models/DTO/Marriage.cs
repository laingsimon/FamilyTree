using System.Diagnostics;
using System.Xml.Serialization;

namespace FamilyTree.Models.DTO
{
	[DebuggerDisplay("To {To.Person.Name.First,nq} {To.Person.Name.Last,nq}")]
	public class Marriage
	{
		[XmlAttribute]
		public string Date { get; set; }

		[XmlAttribute]
		public string Location { get; set; }

		[XmlAttribute]
		public MarriageStatus Status { get; set; }

		[XmlAttribute]
		public MarriageType Type { get; set; }

		public MarriageTo To { get; set; }

		public MarriageChildren Children { get; set; }
	}
}