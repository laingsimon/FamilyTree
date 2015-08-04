namespace FamilyTree.Models.Authentication
{
	public class SaltedHash
	{
		public string Hash { get; set; }
		public string Salt { get; set; }
	}
}