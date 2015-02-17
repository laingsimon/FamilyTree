using System.Diagnostics;

namespace FamilyTree.Models.DTO
{
	[DebuggerDisplay("{Person.First,nq} {Person.Last,nq}")]
	public class MarriageTo
	{
		public Person Person { get; set; }
	}
}