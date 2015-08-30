namespace FamilyTree.Models.Authentication
{
	public class DebugSecurityScheme : ISecurityScheme
	{
		public ISecurityScheme UpgradedSecurityScheme
		{
			get { return null; }
		}

		public void SetData(User user, string password)
		{ }

		public bool Validate(User user, string password)
		{
			return true;
		}
	}
}