using System.IO;

namespace FamilyTree.Models
{
	public interface ITreeVisitee
	{
		void Visit(FileInfo treeFile);
	}
}