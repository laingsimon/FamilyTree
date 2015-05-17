using System;

namespace FamilyTree.Models.Authentication
{
	public class HashingAndSaltingSecurityScheme : ISecurityScheme
	{
		public bool Validate(User user, string password)
		{
			var passwordHash = _GeneratePasswordHash(password, user.Salt);
			return passwordHash == user.PasswordHash;
		}

		public void SetData(User user, string password)
		{
			var salt = _GenerateSalt(user);
			var passwordHash = _GeneratePasswordHash(password, salt);

			user.Salt = salt;
			user.PasswordHash = passwordHash;
			user.Scheme = SecurityScheme.SaltedHash;
		}

		private static string _GenerateSalt(User user)
		{
			return Guid.NewGuid().ToString();
		}

		private static string _GeneratePasswordHash(string password, string salt)
		{
			return _Md5Hash(salt + password);
		}

		private static string _Md5Hash(string data)
		{
			return "";
		}
	}
}