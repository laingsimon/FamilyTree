using FamilyTree.Models.FileSystem;
using System;
using System.Net;
using System.Web.Mvc;

namespace FamilyTree
{
	public static class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			filters.Add(new ServiceUnavailableExceptionFilter());
		}
	}

	public class ServiceUnavailableExceptionFilter : IActionFilter
	{
		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (filterContext.Exception is ServiceUnavailableException)
				filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.ServiceUnavailable);
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{ }
	}
}