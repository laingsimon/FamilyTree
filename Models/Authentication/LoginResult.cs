using FamilyTree.ViewModels;
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
			return new _Success(new FormsAuthenticationActionResult(user));
		}

		public static LoginResult Failed(DateTime restrictedUntil)
		{
			return new _Failed(restrictedUntil);
		}

		public static void Logout()
		{
			FormsAuthenticationActionResult.Logout();
		}

		public abstract ActionResult Respond(LoginViewModel viewModel);

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
					restrictedUntil.ToLocalTime());
			}

			public override ActionResult Respond(LoginViewModel viewModel)
			{
				viewModel.Messages = _BuildMessage(_restrictedUntil);
				return new ViewResult
				{
					ViewData =
					{
						Model = viewModel
					}
				};
			}
		}

		private class _Success : LoginResult
		{
			private readonly ActionResult _setAuthenticated;

			public _Success(ActionResult setAuthenticated)
			{
				_setAuthenticated = setAuthenticated;
			}

			public override ActionResult Respond(LoginViewModel viewModel)
			{
				var returnUrl = _GetAbsoluteUri(viewModel.ReturnUrl); 
				
				return new CompositeActionResult(
					_setAuthenticated,
					new RedirectResult(returnUrl.ToString()));
			}

			private Uri _GetAbsoluteUri(string relativeUrl)
			{
				var request = HttpContext.Current.Request;

				var baseUri = new Uri(
					string.Format(
						"{0}://{1}",
						request.Url.Scheme,
						request.Url.Host));
				return new Uri(baseUri, relativeUrl);
			}
		}
	}
}