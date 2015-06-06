using FamilyTree.Models.FileSystem;

namespace FamilyTree.Models
{
	public interface ITreeVisitee
	{
		void Visit(IFile treeFile);
	}
}