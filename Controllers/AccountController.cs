using FamilyTree.Models.Authentication;
using FamilyTree.Repositories.Authentication;
using FamilyTree.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FamilyTree.Controllers
{
    public class AccountController : Controller
	{
		private readonly UserAuthenticationStrategy _authenticationStrategy;
		private readonly UserRepository _userRepository;

		public AccountController()
			:this(null)
		{ }

		public AccountController(UserAuthenticationStrategy authenticationStrategy = null)
		{
			_authenticationStrategy = authenticationStrategy 
				?? new UserAuthenticationStrategy(
						new UserRepository(),
						new FailedLoginService(
							new FailedLoginRepository()),
						UserAuthenticationStrategy.DefaultSchemes);

			_userRepository = new UserRepository();
		}

		public ActionResult Index()
		{
			if (HttpContext.User.Identity.IsAuthenticated)
				return RedirectToAction("List", "Tree");

			return RedirectToAction("Login");
		}

		[HttpGet]
		public ActionResult Login(string returnUrl = null)
		{
			return View(new LoginViewModel
			{
				ReturnUrl = returnUrl
			});
		}

		[HttpPost]
		public ActionResult Login(LoginViewModel viewModel)
		{
			if (!ModelState.IsValid)
			{
				viewModel.Messages = "Please verify that you have provided all the required information";
				return View(viewModel);
			}

			if (viewModel.ReturnUrl == null || !Url.IsLocalUrl(viewModel.ReturnUrl))
				viewModel.ReturnUrl = Url.Action("List", "Tree");

			var result = _authenticationStrategy.AttemptLogin(viewModel.UserName, viewModel.Password, HttpContext.Request);

			return result.Respond(viewModel);
		}

		public ActionResult Logout()
		{
			_authenticationStrategy.Logout();

			return RedirectToAction("Login", "Account");
		}

		[Authorize(Roles = Roles.SuperUser)]
		public ActionResult Register()
		{
			return View(new RegisterViewModel());
		}

		[Authorize(Roles = Roles.SuperUser)]
		[HttpPost]
		public ActionResult Register(RegisterViewModel viewModel)
		{
			if (!ModelState.IsValid)
			{
				viewModel.Messages = "There are errors with the registration form";
				return View(viewModel);
			}

			var existingUser = _userRepository.GetUser(viewModel.UserName);
			if (existingUser != null)
			{
				viewModel.Messages = "User already exists with this name";
				return View(viewModel);
			}

			var scheme = new HashingAndSaltingSecurityScheme();
			var user = new User(viewModel.UserName)
			{
				DisplayName = viewModel.DisplayName
			};
			scheme.SetData(user, viewModel.Password);

			_userRepository.InsertOrUpdate(user);

			return RedirectToAction("Login", "Account");
		}
    }
}
