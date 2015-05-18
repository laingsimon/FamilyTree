using System.Diagnostics;

namespace FamilyTree.Models.DTO
{
	[DebuggerDisplay("{Person.Name.First,nq} {Person.Name.Last,nq}")]
	public class MarriageTo
	{
		public Person Person { get; set; }
	}
}