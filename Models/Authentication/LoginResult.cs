using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Models.Authentication
{
	public abstract class LoginResult
	{
		public static LoginResult Success(User user)
		{
			return new _Success(user);
		}

		public static LoginResult Failed(DateTime restrictedUntil)
		{
			return new _Failed(restrictedUntil);
		}

		public abstract ActionResult Respond(HttpResponseBase response, Uri returnUrl);

		private LoginResult()
		{ }

		private class _Failed : LoginResult
		{
			private readonly DateTime _restrictedUntil;

			public _Failed(DateTime restrictedUntil)
			{
				_restrictedUntil = restrictedUntil;
			}

			private static string _BuildMessage(DateTime restrictedUntil)
			{
				return string.Format(
					@"Login attempt failed. Please check your username and password
Please wait until {0:HH:mm:ss} until attempting again",
					restrictedUntil);
			}

			public override ActionResult Respond(HttpResponseBase response, Uri returnUrl)
			{
				return new CompositeActionResult(
					new HttpStatusCodeResult(HttpStatusCode.Unauthorized),
					new ContentResult
					{
						Content = _BuildMessage(_restrictedUntil)
					});
			}
		}

		private class _Success : LoginResult
		{
			private readonly User _user;

			public _Success(User user)
			{
				_user = user;
			}

			public override ActionResult Respond(HttpResponseBase response, Uri returnUrl)
			{
				if (returnUrl == null)
					return new HttpStatusCodeResult(HttpStatusCode.OK);
				else
					return new RedirectResult(returnUrl.ToString());
			}
		}
	}
}