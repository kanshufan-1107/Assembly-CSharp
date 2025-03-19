using System;
using Blizzard.MobileAuth;

namespace Hearthstone.Login;

internal class WebQueueFlowWrapper : IWebQueueFlowCallback
{
	private readonly IMobileAuth m_mobileAuth;

	private Action<string> m_successCallback;

	private Action m_cancelledCallback;

	private Action<BlzMobileAuthError> m_errorCallback;

	public WebQueueFlowWrapper(IMobileAuth mobileAuth)
	{
		m_mobileAuth = mobileAuth;
	}

	public WebQueueFlowWrapper Success(Action<string> successCallback)
	{
		m_successCallback = successCallback;
		return this;
	}

	public WebQueueFlowWrapper Cancelled(Action cancelledCallback)
	{
		m_cancelledCallback = cancelledCallback;
		return this;
	}

	public WebQueueFlowWrapper Error(Action<BlzMobileAuthError> errorCallback)
	{
		m_errorCallback = errorCallback;
		return this;
	}

	public void BeginWebQueueFlow(QueueInfo info)
	{
		m_mobileAuth.StartWebQueueFlow(info, this);
	}

	public void OnWebQueueFlowSuccess(QueueInfo? queueInfo, string queueStamp)
	{
		m_successCallback?.Invoke(queueStamp);
	}

	public void OnWebQueueFlowError(QueueInfo? queueInfo, BlzMobileAuthError error)
	{
		m_errorCallback?.Invoke(error);
	}

	public void OnWebQueueFlowCancel(QueueInfo? queueInfo)
	{
		m_cancelledCallback?.Invoke();
	}
}
