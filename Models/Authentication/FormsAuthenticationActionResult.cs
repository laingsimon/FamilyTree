using System.Web.Mvc;
using System.Web.Security;

namespace FamilyTree.Models.Authentication
{
	// ReSharper disable UnusedMember.Global
	public class FormsAuthenticationActionResult : ActionResult
	// ReSharper restore UnusedMember.Global
	{
		private readonly User _user;

		public FormsAuthenticationActionResult(User user)
		{
			_user = user;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			FormsAuthentication.SetAuthCookie(_user.RowKey, false);
		}

		public static void Logout()
		{
			FormsAuthentication.SignOut();
		}
	}
}