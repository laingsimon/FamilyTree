using System;
using System.Net;
using System.Web;

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

		public abstract void Respond(HttpResponseBase response, Uri returnUrl);

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

			public override void Respond(HttpResponseBase response, Uri returnUrl)
			{
				response.StatusCode = (int)HttpStatusCode.Unauthorized;
				response.Write(_BuildMessage(_restrictedUntil));
			}
		}

		private class _Success : LoginResult
		{
			private readonly User _user;

			public _Success(User user)
			{
				_user = user;
			}

			public override void Respond(HttpResponseBase response, Uri returnUrl)
			{
				if (returnUrl == null)
				{
					response.StatusCode = (int)HttpStatusCode.OK;
				}
				else
				{
					response.StatusCode = (int)HttpStatusCode.Found;
					response.Headers.Add("Location", returnUrl.ToString());
				}
			}
		}
	}
}