using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Services;

namespace Hearthstone.Login;

public class DesktopLoginTokenFetcher : IPlatformLoginTokenFetcher
{
	private string m_lastWebToken;

	private bool m_disallowRepeatTokens;

	protected readonly OfflineAuthTokenCache m_authTokenCache;

	private ILogger Logger { get; }

	private bool HasPreviousToken => !string.IsNullOrEmpty(m_lastWebToken);

	public DesktopLoginTokenFetcher(ILogger logger)
	{
		Logger = logger;
		m_authTokenCache = new OfflineAuthTokenCache(logger);
		Logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "Constructing DesktopLoginTokenFetcher");
	}

	public TokenPromise FetchToken(string challengeUrl)
	{
		Logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "Running FetchToken() from DesktopLoginTokenFetcher");
		string token = GetTokenIfAvailable();
		TokenPromise tokenPromise = new TokenPromise();
		TokenPromise.ResultType result = ((!string.IsNullOrEmpty(token)) ? TokenPromise.ResultType.Success : TokenPromise.ResultType.Failure);
		tokenPromise.SetResult(result, token);
		return tokenPromise;
	}

	private string GetTokenIfAvailable()
	{
		string token = m_authTokenCache.TakeStoredToken();
		if (string.IsNullOrEmpty(token))
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "Trying to use token from token fetcher.");
			token = GetTokenFromTokenFetcher();
		}
		if (string.IsNullOrEmpty(token))
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "Trying to use stored launch option WEB_TOKEN");
			token = GetLaunchOption("WEB_TOKEN", encrypted: true);
		}
		if (m_lastWebToken == null || !m_lastWebToken.Equals(token))
		{
			m_lastWebToken = token;
			m_disallowRepeatTokens = false;
		}
		else if (m_disallowRepeatTokens)
		{
			token = null;
			Logger.Log(Blizzard.T5.Core.LogLevel.Information, "A repeated token was retrieved when disallowed. Setting to null token for failure");
		}
		return token;
	}

	public void ClearCachedAuthentication()
	{
		m_disallowRepeatTokens = true;
		m_authTokenCache.ClearStoredToken();
	}

	private string GetTokenFromTokenFetcher()
	{
		string webToken = null;
		if (IsTokenFetcherAllowed())
		{
			if (HearthstoneApplication.IsInternal() && HasPreviousToken)
			{
				Logger.Log(Blizzard.T5.Core.LogLevel.Information, "Fetching new webToken since last one failed...");
				webToken = ServiceManager.Get<ITokenFetcherService>().GenerateWebAuthToken(forceGenerate: true);
			}
			if (string.IsNullOrEmpty(webToken))
			{
				webToken = ServiceManager.Get<ITokenFetcherService>().GetCurrentAuthToken();
			}
		}
		return webToken;
	}

	public string GetCachedAuthTokenIfAvailable()
	{
		string token = m_authTokenCache.GetStoredToken();
		if (string.IsNullOrEmpty(token))
		{
			token = GetTokenIfAvailable();
		}
		return token;
	}

	protected virtual bool IsTokenFetcherAllowed()
	{
		return false;
	}

	protected virtual string GetLaunchOption(string optionKey, bool encrypted)
	{
		return BattleNet.GetLaunchOption(optionKey, encrypted);
	}
}
