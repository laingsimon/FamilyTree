using System.Diagnostics;

namespace FamilyTree.Models
{
	[DebuggerDisplay("{Person.First,nq} {Person.Last,nq}")]
	public class MarriageTo
	{
		public Person Person { get; set; }
	}
}