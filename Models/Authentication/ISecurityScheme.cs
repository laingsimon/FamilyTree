namespace FamilyTree.Models.Authentication
{
	public interface ISecurityScheme
	{
		bool Validate(User user, string password);
		void SetData(User user, string password);
	}
}