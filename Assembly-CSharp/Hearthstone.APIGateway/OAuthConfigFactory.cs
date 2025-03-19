namespace Hearthstone.APIGateway;

internal static class OAuthConfigFactory
{
	public enum AuthEnvironment
	{
		QA,
		PRODUCTION
	}

	private const string SSO_AUTH_ENDPOINT_QA = "";

	private const string SSO_AUTH_ENDPOINT_QA_CN = "";

	private const string CLIENT_ID_QA = "";

	private const string SSO_AUTH_ENDIPOINT_PROD = "https://oauth.battle.net/sso";

	private const string SSO_AUTH_ENDIPOINT_PROD_CN = "https://oauth.battlenet.com.cn/sso";

	private const string CLIENT_ID_PROD = "f587268ac35a4b8cb946637ba1a7dfab";

	public static OAuthConfiguration CreateConfig(AuthEnvironment environment, bool useChinaEndpoints)
	{
		OAuthConfiguration result = default(OAuthConfiguration);
		result.ClientID = GetClientIDForEnvironment(environment);
		result.AuthEndpointURL = GetSSOAuthEndpoint(environment, useChinaEndpoints);
		return result;
	}

	private static string GetSSOAuthEndpoint(AuthEnvironment environment, bool useChinaEndpoints)
	{
		if (useChinaEndpoints)
		{
			if (environment != 0)
			{
				return "https://oauth.battlenet.com.cn/sso";
			}
			return "";
		}
		if (environment != 0)
		{
			return "https://oauth.battle.net/sso";
		}
		return "";
	}

	private static string GetClientIDForEnvironment(AuthEnvironment environment)
	{
		if (environment != 0)
		{
			return "f587268ac35a4b8cb946637ba1a7dfab";
		}
		return "";
	}
}
