using System.Xml.Serialization;

namespace FamilyTree.Models
{
	public class Tree
	{
		[XmlAttribute]
		public string Family { get; set; }

		public Person Person { get; set; }
	}
}