using FamilyTree.Models.Authentication;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace FamilyTree.Repositories.Authentication
{
	public class FailedLoginRepository
	{
		public const string PartitionKey = "failedlogin";
		private const string _tableName = "failedlogin";

		private readonly CloudTable _table;

		public FailedLoginRepository()
		{
			var storageAccount = CloudStorageAccount.Parse(
				CloudConfigurationManager.GetSetting("StorageConnectionString"));

			var tableClient = storageAccount.CreateCloudTableClient();
			_table = tableClient.GetTableReference(_tableName);
			_table.CreateIfNotExists();
		}

		public FailedLogin Get(string key)
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			var operation = TableOperation.Retrieve<FailedLogin>(PartitionKey, key);
			var result = _table.Execute(operation);

			return (FailedLogin)result.Result;
		}

		public void Delete(string key)
		{
			var login = Get(key);
			if (login == null)
				return;

			var operation = TableOperation.Delete(login);
			_table.Execute(operation);
		}

		public void InsertOrUpdate(FailedLogin login)
		{
			var operation = TableOperation.InsertOrReplace(login);
			_table.Execute(operation);
		}
	}
}