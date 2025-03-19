using System;
using Blizzard.MobileAuth;
using Blizzard.T5.Core;

namespace Hearthstone.Login;

internal class GuestCreateAndAuthFlow
{
	private readonly IMobileAuth m_mobileAuth;

	private readonly ILogger m_logger;

	private Action<Account> m_successCallback;

	private Action<BlzMobileAuthError> m_errorCallback;

	private Action m_cancelledCallback;

	public GuestCreateAndAuthFlow(IMobileAuth mobileAuth, ILogger logger)
	{
		m_mobileAuth = mobileAuth;
		m_logger = logger;
	}

	public GuestCreateAndAuthFlow Success(Action<Account> successCallback)
	{
		m_successCallback = successCallback;
		return this;
	}

	public GuestCreateAndAuthFlow Cancelled(Action cancelledCallback)
	{
		m_cancelledCallback = cancelledCallback;
		return this;
	}

	public GuestCreateAndAuthFlow Error(Action<BlzMobileAuthError> errorCallback)
	{
		m_errorCallback = errorCallback;
		return this;
	}

	public void Begin()
	{
		StartCreate();
	}

	private void StartCreate(string queueStamp = null)
	{
		GuestAccountAction.GenerateGuestSoftAccount(m_mobileAuth, queueStamp).Success(OnAccountGenerated).Error(OnAccountGenerationFailed)
			.QueueDeferred(OnCreationQueued)
			.Execute();
	}

	private void OnAccountGenerated(Account account)
	{
		m_successCallback?.Invoke(account);
	}

	private void OnAccountGenerationFailed(BlzMobileAuthError error)
	{
		m_errorCallback?.Invoke(error);
	}

	private void OnCreationQueued(QueueInfo info)
	{
		m_logger?.Log(LogLevel.Information, "Soft account creation queued. Starting web queue.");
		new WebQueueFlowWrapper(m_mobileAuth).Success(StartCreate).Cancelled(delegate
		{
			m_cancelledCallback?.Invoke();
		}).Error(delegate(BlzMobileAuthError error)
		{
			m_errorCallback?.Invoke(error);
		})
			.BeginWebQueueFlow(info);
	}
}
