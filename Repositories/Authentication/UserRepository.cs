using FamilyTree.Models.Authentication;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace FamilyTree.Repositories.Authentication
{
	public class UserRepository : IUserRepository
	{
		public const string PartitionKey = "user";
		private const string _tableName = "user";

		private readonly CloudTable _table;

		public UserRepository()
		{
			var storageAccount = CloudStorageAccount.Parse(
				CloudConfigurationManager.GetSetting("StorageConnectionString"));

			var tableClient = storageAccount.CreateCloudTableClient();
			_table = tableClient.GetTableReference(_tableName);
			_table.CreateIfNotExists();
		}

		public User GetUser(string username)
		{
			if (string.IsNullOrEmpty(username))
				throw new ArgumentNullException("username");
			
			var operation = TableOperation.Retrieve<User>(PartitionKey, username);
			var result = _table.Execute(operation);

			return (User)result.Result;
		}

		public void InsertOrUpdate(User user)
		{
			var operation = TableOperation.InsertOrReplace(user);
			_table.Execute(operation);
		}

		public void DeleteUser(string username)
		{
			var user = GetUser(username);
			if (user == null)
				return;

			var operation = TableOperation.Delete(user);
			_table.Execute(operation);
		}
	}
}