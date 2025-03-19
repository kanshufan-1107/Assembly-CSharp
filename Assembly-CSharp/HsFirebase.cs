using System.Collections.Generic;
using Blizzard.T5.Core;

public class HsFirebase
{
	private IFirebaseAnalytics m_firebaseAnalytics;

	public bool IsEnabled => m_firebaseAnalytics.IsEnabled;

	public HsFirebase(ILogger logger, bool optOutOfSharing)
	{
		m_firebaseAnalytics = new NoopFirebaseAnalytics(logger, optOutOfSharing);
	}

	public void OptOutOfSharing()
	{
		m_firebaseAnalytics.OptOutOfSharing();
	}

	public void SetEnabledCollection(bool enabled)
	{
		m_firebaseAnalytics.SetCollectionEnabled(enabled);
	}

	public void SendEvent(string eventName, Dictionary<string, string> eventParams)
	{
		m_firebaseAnalytics.SendEvent(eventName, eventParams);
	}

	public void SetCustomerUserId(string id)
	{
		m_firebaseAnalytics.SetCustomerUserId(id);
	}
}
