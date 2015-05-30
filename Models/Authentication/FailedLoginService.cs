using System;
using System.Web;
using FamilyTree.Repositories.Authentication;

namespace FamilyTree.Models.Authentication
{
	public class FailedLoginService
	{
		private readonly FailedLoginRepository _repository;
		private readonly int _secondsDelaySeed;

		public FailedLoginService(FailedLoginRepository repository, int secondsDelaySeed = 2)
		{
			_repository = repository;
			_secondsDelaySeed = secondsDelaySeed;
		}

		public DateTime InsertFailedLogin(HttpRequestBase request)
		{
			var key = _GetKey(request);
			var failedLogin = _repository.Get(key) ?? new FailedLogin(key)
			{
				Attempts = 0
			};
			failedLogin.Attempts++;

			var secondsDelayRequired = Math.Pow(_secondsDelaySeed, failedLogin.Attempts);
			var now = DateTime.UtcNow;
			var restrictedUntil = now.AddSeconds(secondsDelayRequired);
			failedLogin.LastAttempt = now;
			failedLogin.NextAllowedLogin = restrictedUntil;

			_repository.InsertOrUpdate(failedLogin);

			return failedLogin.NextAllowedLogin;
		}

		public DateTime? GetNextAllowedLogin(HttpRequestBase request)
		{
			var key = _GetKey(request);
			var failedLogin = _repository.Get(key);

			if (failedLogin == null)
				return null;

			if (failedLogin.NextAllowedLogin >= DateTime.UtcNow)
				return failedLogin.NextAllowedLogin;

			return null;
		}

		public void RemoveFailedLogin(HttpRequestBase request)
		{
			var key = _GetKey(request);
			var failedLogin = _repository.Get(key);

			if (failedLogin == null)
				return;

			if (failedLogin.NextAllowedLogin < DateTime.UtcNow)
				_repository.Delete(key);
		}

		private string _GetKey(HttpRequestBase request)
		{
			return request.UserHostAddress;
		}
	}
}