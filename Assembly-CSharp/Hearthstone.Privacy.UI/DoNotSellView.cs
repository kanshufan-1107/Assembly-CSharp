using System;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Privacy.UI;

public sealed class DoNotSellView : MonoBehaviour
{
	private sealed class InvokeLimiter
	{
		private readonly Action m_action;

		private readonly double m_delaySecs;

		private DateTime m_lastInvoke = DateTime.MinValue;

		public InvokeLimiter(Action action, double delaySecs)
		{
			m_action = action;
			m_delaySecs = delaySecs;
		}

		public void Invoke()
		{
			DateTime currentTime = DateTime.Now;
			if (!((currentTime - m_lastInvoke).TotalSeconds < m_delaySecs))
			{
				m_lastInvoke = currentTime;
				m_action?.Invoke();
			}
		}
	}

	public UIBButton m_doNotSellButton;

	public AsyncReference m_moreInfoButton;

	public double InfoDelaySec = 0.25;

	public double DNSDelaySec = 1.0;

	public event Action OnDoNotSellPressed;

	public event Action OnMoreInfoPressed;

	public void Awake()
	{
		InvokeLimiter dnsInvokeLimiter = new InvokeLimiter(SignalDnsPressed, DNSDelaySec);
		InvokeLimiter moreInfoInvokeLimiter = new InvokeLimiter(SignalMoreInfoPressed, InfoDelaySec);
		m_doNotSellButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			dnsInvokeLimiter.Invoke();
		});
		SetupButtonOnReady(m_moreInfoButton, moreInfoInvokeLimiter);
	}

	private static void SetupButtonOnReady(AsyncReference buttonReference, InvokeLimiter limiter)
	{
		buttonReference.RegisterReadyListener(delegate(Widget x)
		{
			x.GetComponentInChildren<UIBButton>().AddEventListener(UIEventType.RELEASE, delegate
			{
				limiter.Invoke();
			});
		});
	}

	private void SignalDnsPressed()
	{
		this.OnDoNotSellPressed?.Invoke();
	}

	private void SignalMoreInfoPressed()
	{
		this.OnMoreInfoPressed?.Invoke();
	}
}
