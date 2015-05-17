using FamilyTree.Repositories.Authentication;
using Microsoft.WindowsAzure.Storage.Table;

namespace FamilyTree.Models.Authentication
{
	public class User : TableEntity
	{
		public User(string username)
		{
			PartitionKey = UserRepository.PartitionKey;
			RowKey = username;
		}

		public User()
		{ }

		public string PasswordHash { get; set; }
		public string Salt { get; set; }
		public SecurityScheme Scheme { get; set; }

		public string DisplayName { get; set; }
	}
}