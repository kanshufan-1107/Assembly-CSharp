using System;
using System.Runtime.CompilerServices;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Hearthstone.Core;

namespace Hearthstone.Login;

public class OfflineAuthTokenCache
{
	[CompilerGenerated]
	private bool _003CFailedToGenerateLoginWebToken_003Ek__BackingField;

	protected DateTime? m_lastRequestedLoginTokenTime;

	private readonly ILogger m_logger;

	protected string m_webToken;

	private bool FailedToGenerateLoginWebToken
	{
		[CompilerGenerated]
		set
		{
			_003CFailedToGenerateLoginWebToken_003Ek__BackingField = value;
		}
	}

	protected TimeSpan? LoginTokenAge { get; set; }

	public OfflineAuthTokenCache(ILogger logger)
	{
		m_logger = logger;
		Processor.RegisterUpdateDelegate(RefreshTokenCacheIfNeeded);
	}

	public string GetStoredToken()
	{
		return m_webToken;
	}

	public string TakeStoredToken()
	{
		string storedToken = GetStoredToken();
		ClearStoredToken();
		return storedToken;
	}

	public void ClearStoredToken()
	{
		m_webToken = null;
		m_lastRequestedLoginTokenTime = null;
		LoginTokenAge = null;
	}

	public void RefreshTokenCacheIfNeeded()
	{
		if (Vars.Key("Debug.TokenCacheEnabled").GetBool(def: true))
		{
			if (m_lastRequestedLoginTokenTime.HasValue)
			{
				DateTime now = DateTime.Now;
				DateTime? lastRequestedLoginTokenTime = m_lastRequestedLoginTokenTime;
				LoginTokenAge = now - lastRequestedLoginTokenTime;
			}
			if (Network.IsLoggedIn() && (!m_lastRequestedLoginTokenTime.HasValue || LoginTokenHasExpired()))
			{
				m_logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "LoginManager: Generating new SSO login token.");
				m_lastRequestedLoginTokenTime = DateTime.Now;
				BattleNet.GenerateWtcgWebCredentials(OnGenerateWebCredentialsResponse);
				ScheduleCallback(5f, realTime: true, OnGenerateWebCredentialsTimedOut);
			}
		}
	}

	protected virtual void ScheduleCallback(float secondsToWait, bool realTime, Processor.ScheduledCallback cb, object userData = null)
	{
		Processor.ScheduleCallback(secondsToWait, realTime, cb, userData);
	}

	private void OnGenerateWebCredentialsTimedOut(object userData)
	{
		if (LoginTokenHasExpired())
		{
			FailedToGenerateLoginWebToken = true;
			m_logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "OnGenerateWebCredentialsTimedOut() - Timed out before receiving SSO token.");
		}
	}

	private bool LoginTokenHasExpired()
	{
		if (LoginTokenAge.HasValue)
		{
			return LoginTokenAge > TimeSpan.FromSeconds(12600.0);
		}
		return false;
	}

	private void OnGenerateWebCredentialsResponse(bool hasToken, string token)
	{
		if (!hasToken)
		{
			m_logger?.Log(Blizzard.T5.Core.LogLevel.Error, "Unable to generate login token.");
			FailedToGenerateLoginWebToken = true;
			return;
		}
		TimeSpan responseTime = TimeSpan.MinValue;
		if (m_lastRequestedLoginTokenTime.HasValue)
		{
			responseTime = DateTime.Now - m_lastRequestedLoginTokenTime.Value;
			LoginTokenAge = TimeSpan.Zero;
		}
		m_logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "OnGenerateWebCredentialsResponse() - OldToken={0} | NewToken={1} | ResponseTime={2}", m_webToken, token, responseTime.TotalSeconds);
		m_webToken = token;
		FailedToGenerateLoginWebToken = false;
	}
}
