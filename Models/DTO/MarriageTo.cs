using System.Diagnostics;

namespace FamilyTree.Models.DTO
{
	[DebuggerDisplay("{Person.Name.First,nq} {Person.Name.Last,nq}")]
	// ReSharper disable ClassNeverInstantiated.Global
	public class MarriageTo
	// ReSharper restore ClassNeverInstantiated.Global
	{
		// ReSharper disable UnusedAutoPropertyAccessor.Global
		public Person Person { get; set; }
		// ReSharper restore UnusedAutoPropertyAccessor.Global
	}
}