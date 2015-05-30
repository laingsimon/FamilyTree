using System.Collections.Generic;
using System.Web;
using FamilyTree.Repositories.Authentication;
using System.Web.Security;

namespace FamilyTree.Models.Authentication
{
	public class UserAuthenticationStrategy
	{
		public static readonly IReadOnlyDictionary<SecurityScheme, ISecurityScheme> DefaultSchemes =
			new Dictionary<SecurityScheme, ISecurityScheme>
			{
				{ SecurityScheme.SaltedHash, new HashingAndSaltingSecurityScheme() }
			};

		private readonly UserRepository _repository;
		private readonly FailedLoginService _failedLoginService;
		private readonly IReadOnlyDictionary<SecurityScheme, ISecurityScheme> _securitySchemes;

		public UserAuthenticationStrategy(
			UserRepository repository,
			FailedLoginService failedLoginService,
			IReadOnlyDictionary<SecurityScheme, ISecurityScheme> securitySchemes)
		{
			_repository = repository;
			_failedLoginService = failedLoginService;
			_securitySchemes = securitySchemes;
		}

		public LoginResult AttemptLogin(string username, string password, HttpRequestBase request)
		{
			User user = null;

			try
			{
				var nextAllowedLogin = _failedLoginService.GetNextAllowedLogin(request);
				if (nextAllowedLogin.HasValue)
					return LoginResult.Failed(nextAllowedLogin.Value);

				user = _repository.GetUser(username);
				if (user == null)
					return _Failed(request);

				if (!_securitySchemes.ContainsKey(user.Scheme))
					return _Failed(request);

				var scheme = _securitySchemes[user.Scheme];
				var isValid = scheme.Validate(user, password);

				if (isValid)
					return LoginResult.Success(user);
				else
					return _Failed(request);
			}
			finally
			{
				if (user != null)
					_repository.InsertOrUpdate(user);
			}
		}

		public void Logout()
		{
			LoginResult.Logout();
		}

		private LoginResult _Failed(HttpRequestBase request)
		{
			var restrictedUntil = _failedLoginService.InsertFailedLogin(request);
			return LoginResult.Failed(restrictedUntil);
		}
	}
}