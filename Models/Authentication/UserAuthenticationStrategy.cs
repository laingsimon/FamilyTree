using System.Collections.Generic;
using System.Web;
using FamilyTree.Repositories.Authentication;

namespace FamilyTree.Models.Authentication
{
	public class UserAuthenticationStrategy
	{
		public static readonly IReadOnlyDictionary<SecurityScheme, ISecurityScheme> DefaultSchemes =
			new Dictionary<SecurityScheme, ISecurityScheme>
			{
				{ SecurityScheme.Pbkdf2SaltedHash, new Pbkdf2HashingAndSaltingSecurityScheme() },
				{ SecurityScheme.Debug, new DebugSecurityScheme() }
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
				var failedLogin = _failedLoginService.GetFailedLogin(request);
				if (failedLogin != null)
					return LoginResult.Failed(failedLogin);

				user = _repository.GetUser(username);
				if (user == null)
					return _Failed(request);

				if (!_securitySchemes.ContainsKey(user.Scheme))
					return _Failed(request);

				var scheme = _securitySchemes[user.Scheme];
				var isValid = scheme.Validate(user, password);

				if (isValid)
				{
					_UpgradeUserSecurity(user, scheme, password);

					_failedLoginService.RemoveFailedLogin(request);
					return LoginResult.Success(user);
				}
				else
					return _Failed(request);
			}
			finally
			{
				if (user != null)
					_repository.InsertOrUpdate(user);
			}
		}

		private static void _UpgradeUserSecurity(User user, ISecurityScheme scheme, string password)
		{
			var upgradedSecurityScheme = scheme.UpgradedSecurityScheme;
			if (upgradedSecurityScheme == null)
				return;

			upgradedSecurityScheme.SetData(user, password);
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