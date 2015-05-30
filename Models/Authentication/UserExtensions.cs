﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace FamilyTree.Models.Authentication
{
	public static class UserExtensions
	{
		public static string GetClaim(this IPrincipal user, string type)
		{
			var claimsIdentity = user.Identity as ClaimsIdentity;
			if (claimsIdentity == null || !claimsIdentity.IsAuthenticated)
				return null;

			var claim = claimsIdentity.FindFirst(type);
			if (claim == null)
				return null;

			return claim.Value;
		}

		public static bool IsSuperUser(this IPrincipal user)
		{
			return user.IsInRole(OwinAuthenticationActionResult.SuperUserRole);
		}

		public static bool CanAdminister(this IPrincipal user, string family)
		{
			var value = user.GetClaim(OwinAuthenticationActionResult.AdministerFamilies);
			if (value == null)
				return false;

			var splitFamilies = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			return splitFamilies.Contains(family, StringComparer.OrdinalIgnoreCase);
		}
	}
}