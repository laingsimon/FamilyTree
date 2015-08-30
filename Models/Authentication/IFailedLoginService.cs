using System.Web;

namespace FamilyTree.Models.Authentication
{
	public interface IFailedLoginService
	{
		FailedLogin GetFailedLogin(HttpRequestBase request);
		FailedLogin InsertFailedLogin(HttpRequestBase request);
		void RemoveFailedLogin(HttpRequestBase request);
	}
}