using System.Diagnostics;
using System.Xml.Serialization;

namespace FamilyTree.Models
{
	[DebuggerDisplay("To {MarriageTo.Name.First,nq} {MarriageTo.Name.Last,nq}")]
	public class Marriage
	{
		[XmlAttribute]
		public string Date { get; set; }

		[XmlAttribute]
		public string Location { get; set; }

		public string Status { get; set; }

		public MarriageTo To { get; set; }

		public MarriageChildren Children { get; set; }
	}
}