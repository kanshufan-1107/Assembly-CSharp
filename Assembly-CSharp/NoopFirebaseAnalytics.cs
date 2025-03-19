using System.Collections.Generic;
using Blizzard.T5.Core;

public class NoopFirebaseAnalytics : IFirebaseAnalytics
{
	public bool IsEnabled => false;

	public NoopFirebaseAnalytics(ILogger logger, bool optOutOfSharing)
	{
	}

	public void OptOutOfSharing()
	{
	}

	public void SetCollectionEnabled(bool enabled)
	{
	}

	public void SendEvent(string eventName, Dictionary<string, string> eventParams)
	{
	}

	public void SetCustomerUserId(string id)
	{
	}
}
