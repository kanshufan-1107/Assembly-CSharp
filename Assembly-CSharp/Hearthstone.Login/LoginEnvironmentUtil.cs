using Blizzard.T5.Configuration;

namespace Hearthstone.Login;

internal static class LoginEnvironmentUtil
{
	private static string s_environmentTypeOverride = Vars.Key("MASDK.EnvironmentTypeOverride").GetStr(null);

	private static string s_logininUrlOverride = Vars.Key("MASDK.LoginUrlOverride").GetStr(null);

	private static string s_accountCreationUrlOverride = Vars.Key("MASDK.AccountCreationUrlOverride").GetStr(null);

	public static string OverrideEnvironmentIfNeeded(string url)
	{
		if (!IsEnvironmentOverriden())
		{
			return url;
		}
		return GetQAEnvironmentForUrl(url);
	}

	public static string GetQAEnvironmentForUrl(string url)
	{
		return url.Replace("-live-", "-qa-");
	}

	public static bool IsEnvironmentOverriden()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return false;
		}
		return Vars.Key("Aurora.UseQALogin").GetBool(def: false);
	}
}
