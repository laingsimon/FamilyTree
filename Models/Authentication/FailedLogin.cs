using System;
using Microsoft.WindowsAzure.Storage.Table;
using FamilyTree.Repositories.Authentication;

namespace FamilyTree.Models.Authentication
{
	public class FailedLogin : TableEntity
	{
		public FailedLogin(string key)
		{
			PartitionKey = FailedLoginRepository.PartitionKey;
			RowKey = key;
		}

		public FailedLogin()
		{ }

		public int Attempts { get; set; }
		public DateTime LastAttempt { get; set; }
		public DateTime NextAllowedLogin { get; set; }
	}
}