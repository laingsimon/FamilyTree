using System.ComponentModel.DataAnnotations;

namespace FamilyTree.ViewModels
{
	public class RegisterViewModel
	{
		[Required, MinLength(5)]
		public string UserName { get; set; }

		[Required, MinLength(5)]
		public string Password { get; set; }

		[Required, MinLength(5)]
		public string DisplayName { get; set; }

		public string Messages { get; set; }
	}
}