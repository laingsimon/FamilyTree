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
		// ReSharper disable UnusedAutoPropertyAccessor.Global
		public DateTime LastAttempt { get; set; }
		// ReSharper restore UnusedAutoPropertyAccessor.Global
		public DateTime NextAllowedLogin { get; set; }
	}
}