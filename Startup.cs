using FamilyTree;
using FamilyTree.Models.Authentication;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(Startup))]
namespace FamilyTree
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationType = OwinAuthenticationActionResult.AuthenticationType,
				LoginPath = new PathString("/Account/Login")
			});
		}
	}
}