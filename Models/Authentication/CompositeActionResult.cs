using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Authentication
{
	public class CompositeActionResult : ActionResult
	{
		private readonly IReadOnlyCollection<ActionResult> _results;

		public CompositeActionResult(params ActionResult[] results)
		{
			_results = results;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			foreach (var result in _results)
				result.ExecuteResult(context);
		}
	}
}