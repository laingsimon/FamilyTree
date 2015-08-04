namespace FamilyTree.Models.Authentication
{
	public interface IPasswordHasher
	{
		SaltedHash CreateHash(string password);
		bool ValidateHash(string plainTextPassword, string storedHash, string salt);
	}
}