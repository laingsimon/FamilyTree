using System.Diagnostics;
using System.Xml.Serialization;

namespace FamilyTree.Models.DTO
{
	[DebuggerDisplay("{First,nq} {Last,nq}")]
	public class Name
	{
		[XmlAttribute]
		public string Title { get; set; }

		[XmlAttribute]
		public string First { get; set; }

		[XmlAttribute]
		public string Last { get; set; }

		[XmlAttribute]
		public string Nickname { get; set; }

		[XmlAttribute]
		public string Middle { get; set; }
	}
}