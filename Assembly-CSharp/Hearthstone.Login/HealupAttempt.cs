using System;
using Blizzard.MobileAuth;
using Blizzard.T5.Core;
using Blizzard.Telemetry.WTCG.Client;
using HearthstoneTelemetry;

namespace Hearthstone.Login;

internal sealed class HealupAttempt : IAuthenticationFlowCallback
{
	private enum ResultType
	{
		Success,
		Cancelled,
		Failure
	}

	private readonly HealupType m_healupType;

	private readonly IMobileAuth m_mobileAuth;

	private readonly ILogger m_logger;

	private readonly ITelemetryClient m_telemetryClient;

	private readonly AdTrackingManager m_adTrackingManager;

	private Action<bool> m_completionCb;

	public HealupAttempt(HealupType healupType, IMobileAuth mobileAuth, ILogger logger, ITelemetryClient telemetryClient, AdTrackingManager adTrackingManager)
	{
		m_healupType = healupType;
		m_mobileAuth = mobileAuth;
		m_logger = logger;
		m_telemetryClient = telemetryClient;
		m_adTrackingManager = adTrackingManager;
	}

	public void Begin(Account accountToHeal, Action<bool> completionCb)
	{
		m_completionCb = completionCb;
		StartFlowForType(accountToHeal);
	}

	private void StartFlowForType(Account accountToHeal)
	{
		switch (m_healupType)
		{
		case HealupType.NewUser:
			m_mobileAuth.StartHealUpFlow(accountToHeal, this);
			break;
		case HealupType.ExistingAccount:
			m_mobileAuth.StartMergeFlow(accountToHeal, this);
			break;
		default:
			throw new ArgumentOutOfRangeException("m_healupType", m_healupType, "Unknown heal-up type");
		}
	}

	public void Authenticated(Account authenticatedAccount, string flowTrackingId)
	{
		m_logger?.Log(LogLevel.Information, $"Healup success {authenticatedAccount.displayName}, type {m_healupType}, flowId {flowTrackingId}");
		m_adTrackingManager?.TrackHeadlessAccountHealedUp(authenticatedAccount.accountId);
		SendResultTelemetry(ResultType.Success);
		m_completionCb?.Invoke(obj: true);
	}

	public void AuthenticationCancelled(string flowTrackingId)
	{
		m_logger?.Log(LogLevel.Information, "Heal-up canceled, flowId " + flowTrackingId);
		SendResultTelemetry(ResultType.Cancelled);
		m_completionCb?.Invoke(obj: false);
	}

	public void AuthenticationError(BlzMobileAuthError error, string flowTrackingId)
	{
		m_logger?.Log(LogLevel.Error, $"Healup error for heal-up type {m_healupType} with flowId {flowTrackingId}: {error.errorCode} {error.errorContext} {error.errorMessage}");
		SendResultTelemetry(ResultType.Failure, error.errorCode);
		m_completionCb?.Invoke(obj: false);
	}

	private void SendResultTelemetry(ResultType resultType, int errorCode = 0)
	{
		IProtoBuf message = GetResultTelemetry(resultType, errorCode);
		m_telemetryClient.EnqueueMessage(message);
	}

	private IProtoBuf GetResultTelemetry(ResultType resultType, int errorCode = 0)
	{
		HealupType healupType = m_healupType;
		switch (resultType)
		{
		case ResultType.Success:
			switch (healupType)
			{
			case HealupType.NewUser:
				return new AccountHealUpResult
				{
					Result = AccountHealUpResult.HealUpResult.SUCCESS
				};
			case HealupType.ExistingAccount:
				return new AccountMergeResult
				{
					Result = AccountMergeResult.MergeResult.SUCCESS
				};
			}
			break;
		case ResultType.Cancelled:
			switch (healupType)
			{
			case HealupType.NewUser:
				return new AccountHealUpResult
				{
					Result = AccountHealUpResult.HealUpResult.CANCELED
				};
			case HealupType.ExistingAccount:
				return new AccountMergeResult
				{
					Result = AccountMergeResult.MergeResult.CANCELED
				};
			}
			break;
		case ResultType.Failure:
			switch (healupType)
			{
			case HealupType.NewUser:
				return new AccountHealUpResult
				{
					Result = AccountHealUpResult.HealUpResult.FAILURE,
					ErrorCode = errorCode
				};
			case HealupType.ExistingAccount:
				return new AccountMergeResult
				{
					Result = AccountMergeResult.MergeResult.FAILURE,
					ErrorCode = errorCode
				};
			}
			break;
		}
		throw new ArgumentOutOfRangeException();
	}

	public void AuthenticationNewSoftAccountRequested(string flowTrackingId)
	{
		throw new InvalidOperationException("Got a new soft account request on healup attempt " + flowTrackingId);
	}
}
