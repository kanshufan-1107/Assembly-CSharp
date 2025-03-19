using Blizzard.MobileAuth;
using Blizzard.T5.Services;

namespace Hearthstone.Login;

internal class MobileLoginTransition
{
	private enum LoginType
	{
		UNKNOWN,
		LEGACY,
		MASDK
	}

	private IMobileAuth MobileAuth { get; set; }

	private LoginType CurrentLogin { get; set; }

	public MobileLoginTransition(IMobileAuth mobileAuth)
	{
		MobileAuth = mobileAuth;
		CurrentLogin = LoginType.MASDK;
		Log.Login.PrintDebug("Transition created with Current Login {0}", CurrentLogin);
	}

	public void OnTokenFetchStarted()
	{
		LoginType lastLogin = GetLastLoginType();
		Log.Login.PrintDebug("Transition Fetch started. Current: {0} LastLogin: {1}", CurrentLogin, lastLogin);
		if (CurrentLogin == LoginType.MASDK && lastLogin == LoginType.LEGACY)
		{
			TransitionFromLegacyToMASDK();
		}
		SetLastLoginType(CurrentLogin);
	}

	public void OnLoginTokenFetched()
	{
		if (CurrentLogin == LoginType.MASDK)
		{
			SaveMASDKAccountInfo();
		}
	}

	public void OnClearAuthentication()
	{
		ClearSavedAccountInfo();
	}

	private LoginType GetLastLoginType()
	{
		return Options.Get().GetEnum(Option.LAST_LOGIN_TYPE, LoginType.UNKNOWN);
	}

	private void SetLastLoginType(LoginType loginType)
	{
		Options.Get().SetEnum(Option.LAST_LOGIN_TYPE, loginType);
	}

	private void SaveMASDKAccountInfo()
	{
		Account? account = MobileAuth.GetAuthenticatedAccount();
		Log.Login.PrintDebug($"Saving account for transition.\n                Token {!string.IsNullOrEmpty(account?.authenticationToken)} |\n                guestId {!string.IsNullOrEmpty(account?.bnetGuestId)}");
		SaveTransitionAccountInformation(account?.authenticationToken, account?.bnetGuestId);
	}

	private void TransitionFromLegacyToMASDK()
	{
		Log.Login.PrintInfo("Transitioning from Legacy to MASDK");
		ServiceManager.Get<ILoginService>()?.ClearAllSavedAccounts();
	}

	private void SaveTransitionAccountInformation(string authToken, string guestAccountId)
	{
		Options options = Options.Get();
		if (!string.IsNullOrEmpty(authToken))
		{
			options.SetString(Option.TRANSITION_AUTH_TOKEN, authToken);
		}
		if (!string.IsNullOrEmpty(guestAccountId))
		{
			options.SetString(Option.TRANSITION_GUEST_ID, guestAccountId);
		}
	}

	private void ClearSavedAccountInfo()
	{
		Log.Login.PrintInfo("Clearing saved transition info");
		Options options = Options.Get();
		options.DeleteOption(Option.TRANSITION_AUTH_TOKEN);
		options.DeleteOption(Option.TRANSITION_GUEST_ID);
	}
}
