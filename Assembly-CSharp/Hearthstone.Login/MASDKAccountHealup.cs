using System;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.MobileAuth;
using Blizzard.T5.Core;
using HearthstoneTelemetry;

namespace Hearthstone.Login;

internal sealed class MASDKAccountHealup
{
	private bool m_hasHandledDisconnect;

	private bool m_hasHandledExpectedError;

	private Action<bool> m_finishedCallback;

	private HealupAttempt m_attempt;

	private ILogger Logger { get; }

	private ITelemetryClient TelemetryClient { get; }

	private bool IsHealingUp => m_attempt != null;

	public MASDKAccountHealup(ILogger logger, ITelemetryClient telemetryClient)
	{
		Logger = logger;
		TelemetryClient = telemetryClient;
	}

	public void StartHealup(HealupType healupType, IMobileAuth mobileAuth, Action<bool> finishedCallback)
	{
		if (!IsHealingUp)
		{
			m_finishedCallback = finishedCallback;
			Account? account = mobileAuth.GetAuthenticatedAccount();
			if (!account.HasValue)
			{
				Logger?.Log(Blizzard.T5.Core.LogLevel.Error, "No authenticated account! Cannot perform healup");
			}
			else
			{
				PresentHealupForAccount(healupType, mobileAuth, account.Value);
			}
		}
	}

	private void PresentHealupForAccount(HealupType healupType, IMobileAuth mobileAuth, Account account)
	{
		SetStartingHealupStates();
		Logger?.Log(Blizzard.T5.Core.LogLevel.Information, "Attempting healup of account " + account.displayName + " " + account.regionId);
		m_attempt = new HealupAttempt(healupType, mobileAuth, Logger, TelemetryClient, AdTrackingManager.Get());
		m_attempt.Begin(account, OnAttemptCompleted);
	}

	private void OnAttemptCompleted(bool success)
	{
		ClearHealupStates();
		if (success)
		{
			UpdateOptionsForHealupSuccess();
			DisconnectAndRestartForLogin();
		}
		m_finishedCallback?.Invoke(success);
	}

	private void SetStartingHealupStates()
	{
		BeginHandlingBnetErrors();
		ReconnectMgr.Get().SetSuppressUtilReconnect(value: true);
	}

	private static void UpdateOptionsForHealupSuccess()
	{
		Options.Get().SetBool(Option.CREATED_ACCOUNT, val: true);
		Options.Get().DeleteOption(Option.LAST_HEAL_UP_EVENT_DATE);
	}

	private static void DisconnectAndRestartForLogin()
	{
		BattleNet.RequestCloseAurora();
		HearthstoneApplication.Get().ResetAndForceLogin();
	}

	private void ClearHealupStates()
	{
		FinishHandlingBnetErrors();
		m_finishedCallback = null;
		m_attempt = null;
		ReconnectMgr.Get().SetSuppressUtilReconnect(value: false);
	}

	private void BeginHandlingBnetErrors()
	{
		m_hasHandledDisconnect = false;
		m_hasHandledExpectedError = false;
		Network.Get().AddBnetErrorListener(OnBnetError);
	}

	private void FinishHandlingBnetErrors()
	{
		Network.Get().RemoveBnetErrorListener(OnBnetError);
	}

	private bool OnBnetError(BnetErrorInfo info, object userData)
	{
		if (m_hasHandledDisconnect && m_hasHandledExpectedError)
		{
			return false;
		}
		BattleNetErrors error = info.GetError();
		Logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "OnBnetError: " + error);
		switch (error)
		{
		case BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED:
			Logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "Ignored expected disconnect for a heal up");
			m_hasHandledDisconnect = true;
			return true;
		case BattleNetErrors.ERROR_SESSION_DATA_CHANGED:
		case BattleNetErrors.ERROR_SESSION_ADMIN_KICK:
			Logger?.Log(Blizzard.T5.Core.LogLevel.Debug, "Ignored expected error for a heal up");
			m_hasHandledExpectedError = true;
			return true;
		default:
			return false;
		}
	}
}
