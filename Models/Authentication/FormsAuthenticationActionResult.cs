using System.Web.Mvc;
using System.Web.Security;

namespace FamilyTree.Models.Authentication
{
	public class FormsAuthenticationActionResult : ActionResult
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