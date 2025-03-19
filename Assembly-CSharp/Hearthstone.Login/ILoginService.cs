using System;
using Blizzard.T5.Services;

namespace Hearthstone.Login;

public interface ILoginService : IService, IHasUpdate
{
	void StartLogin();

	void ClearAuthentication();

	void HealupCurrentTemporaryAccount(Action<bool> finishedCallback = null);

	void ClearAllSavedAccounts();

	string GetCachedAuthTokenIfAvailable();

	void WipeAllAuthenticationData();

	void MergeCurrentTemporaryAccount(Action<bool> finishedCallback = null);
}
