using Microsoft.WindowsAzure.Storage;
using System;
using System.Net;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

namespace FamilyTree
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode,
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);

			MvcHandler.DisableMvcResponseHeader = true;
		}

		// ReSharper disable UnusedMember.Global
		// ReSharper disable UnusedParameter.Global
		protected void Application_OnError(object sender, EventArgs args)
			// ReSharper restore UnusedParameter.Global
		// ReSharper restore UnusedMember.Global
		{
			var error = Server.GetLastError();
			while (error != null && (error is TargetInvocationException || error.InnerException is TargetInvocationException))
				error = error.InnerException;

			if (error is StorageException)
			{
				Server.ClearError();
				Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
				Response.Write(error.Message);
				Response.End();
			}
		}
	}
}