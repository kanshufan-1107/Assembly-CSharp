using System.Collections.Generic;

public interface IFirebaseAnalytics
{
	bool IsEnabled { get; }

	void OptOutOfSharing();

	void SetCollectionEnabled(bool enabled);

	void SendEvent(string eventName, Dictionary<string, string> eventParams);

	void SetCustomerUserId(string id);
}
