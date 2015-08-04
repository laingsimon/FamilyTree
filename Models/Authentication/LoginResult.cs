using System;
using System.Web;
using System.Web.Mvc;
using FamilyTree.ViewModels;

namespace FamilyTree.Models.Authentication
{
	public abstract class LoginResult
	{
		public static LoginResult Success(User user)
		{
			return new _Success(new OwinAuthenticationActionResult(user));
		}

		public static LoginResult Failed(FailedLogin failedLogin)
		{
			return new _Failed(failedLogin);
		}

		public static void Logout()
		{
			OwinAuthenticationActionResult.Logout();
		}

		public abstract ActionResult Respond(LoginViewModel viewModel);

		private LoginResult()
		{ }

		private class _Failed : LoginResult
		{
			private readonly FailedLogin _failedLogin;

			public _Failed(FailedLogin failedLogin)
			{
				_failedLogin = failedLogin;
			}

			private string _BuildMessage()
			{
				var basicMessage = @"Login attempt failed.
Please check your username and password";

				if (_failedLogin.Attempts <= 1)
					return basicMessage;

				return string.Format(
					@"{0}
Please wait until {1:HH:mm:ss} until attempting again",
					basicMessage,
					_failedLogin.NextAllowedLogin.ToLocalTime());
			}

			public override ActionResult Respond(LoginViewModel viewModel)
			{
				viewModel.Messages = _BuildMessage();
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

			private static Uri _GetAbsoluteUri(string relativeUrl)
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