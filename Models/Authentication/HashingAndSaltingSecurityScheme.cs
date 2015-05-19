using System;
using System.Data.HashFunction;
using System.Text;

namespace FamilyTree.Models.Authentication
{
	public class HashingAndSaltingSecurityScheme : ISecurityScheme
	{
		private readonly HashFunctionAsyncBase _hasher = new ModifiedBernsteinHash();

		public bool Validate(User user, string password)
		{
			var passwordHash = _GeneratePasswordHash(password, user.Salt);
			return passwordHash == user.PasswordHash;
		}

		public void SetData(User user, string password)
		{
			var salt = _GenerateSalt(user);

			user.Salt = salt;
			user.PasswordHash = _GeneratePasswordHash(password, salt);
			user.Scheme = SecurityScheme.SaltedHash;
		}

		private static string _GenerateSalt(User user)
		{
			return Guid.NewGuid().ToString();
		}

		private string _GeneratePasswordHash(string password, string salt)
		{
			var passwordBytes = Encoding.UTF8.GetBytes(salt + password);
			return Encoding.UTF8.GetString(_hasher.ComputeHash(passwordBytes));
		}
	}
}