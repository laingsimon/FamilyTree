namespace FamilyTree.Models.Authentication
{
	public class Pbkdf2HashingAndSaltingSecurityScheme : ISecurityScheme
	{
		private readonly IPasswordHasher _hasher = new PasswordHasher();

		public bool Validate(User user, string password)
		{
			return _hasher.ValidateHash(password, user.PasswordHash, user.Salt);
		}

		public void SetData(User user, string password)
		{
			var saltyHash = _hasher.CreateHash(password);

			user.Salt = saltyHash.Salt;
			user.PasswordHash = saltyHash.Hash;
			user.Scheme = SecurityScheme.Pbkdf2SaltedHash;
		}

		public ISecurityScheme UpgradedSecurityScheme
		{
			get { return null; }
		}
	}
}