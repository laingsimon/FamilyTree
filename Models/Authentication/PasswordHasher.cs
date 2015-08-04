using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace FamilyTree.Models.Authentication
{
	/// <summary>
	/// Taken from http://crackstation.net/hashing-security.htm
	/// </summary>
	public class PasswordHasher : IPasswordHasher
	{
		private readonly int _iterations;
		private readonly int _countSaltBytes;
		private readonly int _countHashBytes;

		public PasswordHasher(int iterations = 1000, int countSaltBytes = 24, int countHashBytes = 24)
		{
			_iterations = iterations;
			_countSaltBytes = countSaltBytes;
			_countHashBytes = countHashBytes;
		}

		public SaltedHash CreateHash(string password)
		{
			// Generate a random salt
			var csprng = new RNGCryptoServiceProvider();
			var salt = new byte[_countSaltBytes];
			csprng.GetBytes(salt);

			// Hash the password and encode the parameters
			var hash = PBKDF2(password, salt, _iterations, _countHashBytes);
			return new SaltedHash
			{
				Hash = Convert.ToBase64String(hash),
				Salt = Convert.ToBase64String(salt)
			};
		}

		public bool ValidateHash(string plainTextPassword, string storedHash, string salt)
		{
			var saltAsBytes = Convert.FromBase64String(salt);
			var storedHashAsBytes = Convert.FromBase64String(storedHash);

			var testHash = PBKDF2(plainTextPassword, saltAsBytes, _iterations, storedHashAsBytes.Length);

			return _SlowEquals(storedHashAsBytes, testHash);
		}

		private static bool _SlowEquals(IReadOnlyList<byte> a, IReadOnlyList<byte> b)
		{
			var diff = (uint)a.Count ^ (uint)b.Count;
			for (var i = 0; i < a.Count && i < b.Count; i++)
				diff |= (uint)(a[i] ^ b[i]);
			return diff == 0;
		}

		// ReSharper disable InconsistentNaming
		private static byte[] PBKDF2(string password, byte[] salt, int iterations, int cb)
		// ReSharper restore InconsistentNaming
		{
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt) { IterationCount = iterations })
			{
				return pbkdf2.GetBytes(cb);
			}
		}
	}
}