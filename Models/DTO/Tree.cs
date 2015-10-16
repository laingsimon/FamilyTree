using System.Xml.Serialization;

namespace FamilyTree.Models.DTO
{
	public class Tree
	{
		[XmlAttribute]
		public string Family { get; set; }

		// ReSharper disable UnusedAutoPropertyAccessor.Global
		public Person Person { get; set; }
		// ReSharper restore UnusedAutoPropertyAccessor.Global
	}
}