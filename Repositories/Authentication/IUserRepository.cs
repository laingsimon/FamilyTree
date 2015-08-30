using FamilyTree.Models.Authentication;

namespace FamilyTree.Repositories.Authentication
{
	public interface IUserRepository
	{
		void DeleteUser(string username);
		User GetUser(string username);
		void InsertOrUpdate(User user);
	}
}