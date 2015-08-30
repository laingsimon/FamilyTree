using FamilyTree.Models.Authentication;

namespace FamilyTree.Repositories.Authentication
{
	public class DebugUserRepository : IUserRepository
	{
		public void DeleteUser(string username)
		{ }

		public User GetUser(string username)
		{
			return new User
			{
				PasswordHash = "hash",
				RowKey = username,
				DisplayName = username,
				Scheme = SecurityScheme.Debug,
				SuperUser = true
			};
		}

		public void InsertOrUpdate(User user)
		{ }
	}
}