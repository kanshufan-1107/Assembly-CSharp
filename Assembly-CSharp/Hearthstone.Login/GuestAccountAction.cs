using System;
using Blizzard.MobileAuth;

namespace Hearthstone.Login;

internal class GuestAccountAction : IGuestSoftAccountCallback
{
	private readonly Action<IGuestSoftAccountCallback> m_action;

	private Action<Account> m_successCb;

	private Action<BlzMobileAuthError> m_errorCb;

	private Action<QueueInfo> m_queueDeferredCb;

	private GuestAccountAction(Action<IGuestSoftAccountCallback> action)
	{
		m_action = action;
	}

	public static GuestAccountAction GenerateGuestSoftAccount(IMobileAuth mobileAuth, string queueStamp)
	{
		return new GuestAccountAction(delegate(IGuestSoftAccountCallback callback)
		{
			if (string.IsNullOrEmpty(queueStamp))
			{
				mobileAuth.GenerateGuestSoftAccount(callback);
			}
			else
			{
				mobileAuth.GenerateGuestSoftAccount(queueStamp, callback);
			}
		});
	}

	public static GuestAccountAction AuthenticateSoftAccount(IMobileAuth mobileAuth, GuestSoftAccountId id)
	{
		return new GuestAccountAction(delegate(IGuestSoftAccountCallback callback)
		{
			mobileAuth.AuthenticateSoftAccount(id, callback);
		});
	}

	public GuestAccountAction Success(Action<Account> successCb)
	{
		m_successCb = successCb;
		return this;
	}

	public GuestAccountAction Error(Action<BlzMobileAuthError> errorCb)
	{
		m_errorCb = errorCb;
		return this;
	}

	public GuestAccountAction QueueDeferred(Action<QueueInfo> queueDeferredCb)
	{
		m_queueDeferredCb = queueDeferredCb;
		return this;
	}

	public void Execute()
	{
		m_action?.Invoke(this);
	}

	public void OnGuestSoftAccountSuccess(Account account)
	{
		m_successCb?.Invoke(account);
	}

	public void OnGuestSoftAccountError(BlzMobileAuthError error)
	{
		m_errorCb?.Invoke(error);
	}

	public void OnGuestSoftAccountQueueDeferred(QueueInfo queueInfo)
	{
		m_queueDeferredCb?.Invoke(queueInfo);
	}
}
