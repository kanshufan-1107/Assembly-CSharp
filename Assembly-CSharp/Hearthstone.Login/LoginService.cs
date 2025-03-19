using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AntiCheatSDK;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.MobileAuth;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.Core;
using Hearthstone.Streaming;
using HearthstoneTelemetry;

namespace Hearthstone.Login;

public class LoginService : ILoginService, IService, IHasUpdate
{
	private class AccountRemoveLogger : IOnAccountRemovedCallback
	{
		[CompilerGenerated]
		private string _003CAccountId_003Ek__BackingField;

		[CompilerGenerated]
		private ILogger _003CLogger_003Ek__BackingField;

		private string AccountId
		{
			[CompilerGenerated]
			set
			{
				_003CAccountId_003Ek__BackingField = value;
			}
		}

		private ILogger Logger
		{
			[CompilerGenerated]
			set
			{
				_003CLogger_003Ek__BackingField = value;
			}
		}

		public AccountRemoveLogger(string accountId, ILogger logger)
		{
			AccountId = accountId;
			Logger = logger;
		}
	}

	private bool m_processWebAuth;

	private IPlatformLoginTokenFetcher m_tokenFetcher;

	private MobileLoginTransition m_mobileLoginTransition;

	private int m_loginChallengesProcessed;

	private string m_lastAuthKeyAttempted;

	private bool m_lastAuthKeyRepeated;

	private ILogger Logger { get; }

	private IMobileAuth MobileAuth { get; }

	private MASDKAccountHealup AccountHealup { get; }

	public LoginService(ILogger logger)
	{
		Logger = logger;
		MobileAuth = new MobileAuthAdapter();
		AccountHealup = new MASDKAccountHealup(logger, TelemetryManager.Client());
	}

	public Type[] GetDependencies()
	{
		return new List<Type>
		{
			typeof(Network),
			typeof(GameDownloadManager)
		}.ToArray();
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		if (PlatformSettings.IsMobileRuntimeOS)
		{
			yield return new WaitForGameDownloadManagerAvailable();
		}
		Logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "LoginServise Initialize: Done WaitForGameDownloadManagerAvailable");
		m_tokenFetcher = CreatePlatformTokenFetcher(Logger, serviceLocator);
		if (PlatformSettings.IsMobileRuntimeOS)
		{
			m_mobileLoginTransition = new MobileLoginTransition(MobileAuth);
		}
		if (Vars.Key("Mobile.WipeAuthData").GetBool(def: false))
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "Wipe auth data config flag set. Wiping data and clearing flag");
			WipeAllAuthenticationData();
			Vars.Key("Mobile.WipeAuthData").Set("False", permanent: true);
			Vars.SaveConfig();
		}
		if (!string.IsNullOrEmpty(m_tokenFetcher.GetCachedAuthTokenIfAvailable()))
		{
			Network.SetShouldBeConnectedToAurora(shouldBeConnected: true);
			Logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "LoginServise Initialize: SetShouldBeConnectedToAurora(true)");
		}
	}

	public bool IsLoggedIn()
	{
		return Network.IsLoggedIn();
	}

	public bool IsAttemptingLogin()
	{
		return m_processWebAuth;
	}

	public void Shutdown()
	{
		m_tokenFetcher = null;
	}

	public void ClearAuthentication()
	{
		m_tokenFetcher?.ClearCachedAuthentication();
		m_mobileLoginTransition?.OnClearAuthentication();
		if (ServiceManager.TryGet<AntiCheatManager>(out var manager))
		{
			manager.ClearExtraParams();
		}
	}

	public void ClearAllSavedAccounts()
	{
		if (!HearthstoneApplication.IsInternal() || !PlatformSettings.IsMobileRuntimeOS)
		{
			return;
		}
		List<Account> accounts = Blizzard.MobileAuth.MobileAuth.GetAllAccounts();
		if (accounts == null)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "There were no accounts to remove");
			return;
		}
		foreach (Account account in accounts)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "Attempting remove account {0} {1}", account.accountId, account.battleTag);
			MobileAuth.RemoveAccountById(account.accountId, new AccountRemoveLogger(account.accountId, Logger));
		}
	}

	public void StartLogin()
	{
		if (!IsLoggedIn() && !IsAttemptingLogin())
		{
			m_mobileLoginTransition?.OnTokenFetchStarted();
			Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "Beginning processing login challenges");
			ResetToStartingLoginState();
		}
	}

	private void ResetToStartingLoginState()
	{
		m_processWebAuth = true;
		m_lastAuthKeyAttempted = null;
		m_lastAuthKeyRepeated = false;
		m_loginChallengesProcessed = 0;
		HearthstoneApplication.Get().WillReset += OnClientReset;
	}

	public void HealupCurrentTemporaryAccount(Action<bool> finishedCallback = null)
	{
		AccountHealup.StartHealup(HealupType.NewUser, MobileAuth, finishedCallback);
	}

	public void MergeCurrentTemporaryAccount(Action<bool> finishedCallback = null)
	{
		AccountHealup.StartHealup(HealupType.ExistingAccount, MobileAuth, finishedCallback);
	}

	public void Update()
	{
		if (!m_processWebAuth)
		{
			return;
		}
		if (IsLoggedIn())
		{
			ClearStartLoginStates();
			Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "We are now logged in, stopping processing challenges");
		}
		else
		{
			if (!BattleNet.HasExternalChallenge())
			{
				return;
			}
			if (!BattleNet.CheckWebAuth(out var challengeUrl))
			{
				Logger?.Log(Blizzard.T5.Core.LogLevel.Warning, "Went to process a challenge but there was no url");
				return;
			}
			challengeUrl = LoginEnvironmentUtil.OverrideEnvironmentIfNeeded(challengeUrl);
			if (m_loginChallengesProcessed > 0 && m_lastAuthKeyRepeated)
			{
				Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "Multiple attempts likely failed with same auth token, clearing authentication data");
				m_lastAuthKeyRepeated = false;
				ClearAuthentication();
			}
			Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "Processing challenge, previous login challenge attempts {0}", m_loginChallengesProcessed);
			ProcessChallenge(challengeUrl);
			if (IsLoginChallenge(challengeUrl))
			{
				m_loginChallengesProcessed++;
			}
		}
	}

	public string GetCachedAuthTokenIfAvailable()
	{
		return m_tokenFetcher?.GetCachedAuthTokenIfAvailable();
	}

	public void WipeAllAuthenticationData()
	{
		if (!HearthstoneApplication.IsPublic())
		{
			ClearAuthentication();
			ClearAllSavedAccounts();
			LegacyAuth.DeleteStoredToken();
			TemporaryAccountManager.Get()?.DeleteTemporaryAccountData();
			Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "All authentication data wiped");
		}
	}

	private void ClearStartLoginStates()
	{
		m_processWebAuth = false;
		m_lastAuthKeyAttempted = null;
		m_lastAuthKeyRepeated = false;
		m_loginChallengesProcessed = 0;
		HearthstoneApplication.Get().WillReset -= OnClientReset;
	}

	private static bool IsLoginChallenge(string challengeUrl)
	{
		return challengeUrl.Contains("externalChallenge=login", StringComparison.InvariantCulture);
	}

	private void OnProvideTokenFailure()
	{
		Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "Failed to get token to respond to login challenge");
		TelemetryManager.Client().SendWebLoginError();
		Network.Get().ReportTokenFetchFailure();
	}

	private void OnProvideTokenCancelled()
	{
		Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "User canceled login challenge");
		HearthstoneApplication.Get().Reset();
	}

	private void ProcessChallenge(string challengeUrl)
	{
		Logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "Starting challenge process for url {0}", challengeUrl);
		if (!PlatformSettings.IsMobileRuntimeOS && challengeUrl.Contains("externalChallenge=legal"))
		{
			LoginLegalChallengeFlow.StartLegalChallenge(challengeUrl, delegate
			{
				OnProvideTokenSuccess(m_tokenFetcher.GetCachedAuthTokenIfAvailable());
			}, Logger);
		}
		else if (challengeUrl.Contains("program=wtcg-stea&thirdPartyAccount"))
		{
			HearthstoneApplication.SendStartupTimeTelemetry("LoginService.ProcessChallenge.LinkingThirdPartyAccount");
			ulong externalAccountID = 0uL;
			TelemetryManager.Client().SendExternalAccountLinkingState(ExternalAccountLinkingState.Status.STARTED, externalAccountID);
			ExternalAccountLinkingFlow.StartLinkingAccount(challengeUrl, delegate
			{
				OnProvideTokenSuccess(m_tokenFetcher.GetCachedAuthTokenIfAvailable());
			}, externalAccountID, Logger);
		}
		else
		{
			HearthstoneApplication.SendStartupTimeTelemetry("LoginService.ProcessChallenge.FetchToken");
			Processor.QueueJobIfNotExist("Job_FetchToken", Job_FetchToken(challengeUrl));
		}
	}

	private IEnumerator<IAsyncJobResult> Job_FetchToken(string challengeUrl)
	{
		Logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "LoginService Job_FetchToken");
		FetchLoginToken tokenFetchDepenency = new FetchLoginToken(challengeUrl, m_tokenFetcher);
		yield return tokenFetchDepenency;
		ITelemetryClient telemetryClient = TelemetryManager.Client();
		switch (tokenFetchDepenency.CurrentStatus)
		{
		case FetchLoginToken.Status.Success:
			telemetryClient.SendLoginTokenFetchResult(LoginTokenFetchResult.TokenFetchResult.SUCCESS);
			OnProvideTokenSuccess(tokenFetchDepenency.Token);
			break;
		case FetchLoginToken.Status.Cancelled:
			telemetryClient.SendLoginTokenFetchResult(LoginTokenFetchResult.TokenFetchResult.CANCELED);
			OnProvideTokenCancelled();
			break;
		case FetchLoginToken.Status.Failed:
			telemetryClient.SendLoginTokenFetchResult(LoginTokenFetchResult.TokenFetchResult.FAILURE);
			OnProvideTokenFailure();
			break;
		}
	}

	private void OnProvideTokenSuccess(string token)
	{
		BattleNet.ProvideWebAuthToken(token);
		m_lastAuthKeyRepeated = !string.IsNullOrEmpty(m_lastAuthKeyAttempted) && m_lastAuthKeyAttempted == token;
		m_lastAuthKeyAttempted = token;
		m_mobileLoginTransition?.OnLoginTokenFetched();
	}

	private IPlatformLoginTokenFetcher CreatePlatformTokenFetcher(ILogger logger, ServiceLocator serviceLocator)
	{
		Logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "Using Desktop Token Fetcher");
		return new DesktopLoginTokenFetcher(logger);
	}

	private void OnClientReset()
	{
		Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "Clearing Login Service state for client reset");
		EnsureConnectToAuroraIsCorrect();
		ClearStartLoginStates();
	}

	private void EnsureConnectToAuroraIsCorrect()
	{
		if (PlatformSettings.IsMobileRuntimeOS && !Options.Get().GetBool(Option.CONNECT_TO_AURORA, defaultVal: false) && MobileAuth.IsAuthenticated)
		{
			Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "Restart during login with successfully authentication. Forcing login on next start");
			Options.Get().SetBool(Option.CONNECT_TO_AURORA, val: true);
		}
	}
}
