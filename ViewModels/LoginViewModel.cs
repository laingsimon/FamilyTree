using System.ComponentModel.DataAnnotations;

namespace FamilyTree.ViewModels
{
	public class LoginViewModel
	{
		public string ReturnUrl { get; set; }

		[Required]
		public string UserName { get; set; }

		[Required]
		public string Password { get; set; }

		public string Messages { get; set; }
	}
}