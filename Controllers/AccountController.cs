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
						UserAuthenticationStrategy.DefaultSchemes);;
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

			var result = _authenticationStrategy.AttemptLogin(viewModel.UserName, viewModel.Password, HttpContext.Request);

			var returnUrl = _GetAbsoluteUri(viewModel.ReturnUrl);

			return result.Respond(HttpContext.Response, returnUrl);
		}

		private Uri _GetAbsoluteUri(string relativeUrl)
		{
			if (string.IsNullOrEmpty(relativeUrl) || !Url.IsLocalUrl(relativeUrl))
				relativeUrl = Url.Action("List", "Tree");

			var baseUri = new Uri(
				string.Format(
					"{0}://{1}",
					HttpContext.Request.Url.Scheme,
					HttpContext.Request.Url.Host));
			return new Uri(baseUri, relativeUrl);
		}
    }
}
