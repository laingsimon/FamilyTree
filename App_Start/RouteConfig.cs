using System.Web.Mvc;
using System.Web.Routing;

namespace FamilyTree
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Photo",
				url: "{controller}/{action}/{family}/{firstName}/{middleName}/{dob}/{size}",
				defaults: new { controller = "Photo", action = "Index", middleName = UrlParameter.Optional, size = UrlParameter.Optional },
				constraints: new { controller = "Photo" }
			);

			routes.MapRoute(
				name: "Tree",
				url: "{controller}/{action}/{family}",
				defaults: new { controller = "Tree", action = "Index", family = UrlParameter.Optional }
			);
		}
	}
}