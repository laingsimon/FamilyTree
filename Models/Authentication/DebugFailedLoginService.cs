using System.Web;

namespace FamilyTree.Models.Authentication
{
	public class DebugFailedLoginService : IFailedLoginService
	{
		public FailedLogin GetFailedLogin(HttpRequestBase request)
		{
			return null;
		}

		public FailedLogin InsertFailedLogin(HttpRequestBase request)
		{
			return new FailedLogin();
		}

		public void RemoveFailedLogin(HttpRequestBase request)
		{  }
	}
}