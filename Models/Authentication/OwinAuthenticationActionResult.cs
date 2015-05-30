﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Security.Principal;
using System.Security.Claims;
using System.Web.Mvc;

namespace FamilyTree.Models.Authentication
{
	public class OwinAuthenticationActionResult : ActionResult
	{
		public const string AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie;
		public const string SuperUserRole = "SuperUser";
		public const string AdministerFamilies = "AdministerFamilies";

		private readonly User _user;

		public OwinAuthenticationActionResult(User user)
		{
			_user = user;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			_SignIn(context.HttpContext);
		}

		public static void Logout()
		{
			_SignOut(new HttpContextWrapper(HttpContext.Current));
		}

		private static void _SignOut(HttpContextBase httpContext)
		{
			var authenticationManager = httpContext.GetOwinContext().Authentication;
			authenticationManager.SignOut(AuthenticationType);
		}

		private void _SignIn(HttpContextBase httpContext)
		{
			var authenticationManager = httpContext.GetOwinContext().Authentication;
			var userIdentity = _CreateIdentity(_user, AuthenticationType);
			authenticationManager.SignIn(new AuthenticationProperties() { }, userIdentity);
		}

		private static ClaimsIdentity _CreateIdentity(User user, string authenticationType)
		{
			return new ClaimsIdentity(
				new GenericIdentity(user.RowKey),
				_GetClaims(user),
				AuthenticationType,
				ClaimsIdentity.DefaultNameClaimType,
				ClaimsIdentity.DefaultRoleClaimType);
		}

		private static IEnumerable<Claim> _GetClaims(User user)
		{
			yield return new Claim(ClaimTypes.GivenName, user.DisplayName);

			if (!string.IsNullOrEmpty(user.AdministerFamilies))
				yield return new Claim(AdministerFamilies, user.AdministerFamilies);

			if (user.SuperUser)
				yield return new Claim(ClaimTypes.Role, SuperUserRole);
		}
	}
}