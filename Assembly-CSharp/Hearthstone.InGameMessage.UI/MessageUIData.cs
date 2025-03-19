using System.Collections.Generic;

namespace Hearthstone.InGameMessage.UI;

public class MessageUIData
{
	public string UID { get; set; }

	public string EventId { get; set; }

	public List<string> Region { get; set; }

	public string ViewCountId { get; set; }

	public int Priority { get; set; }

	public string ContentType { get; set; }

	public MessageLayoutType LayoutType { get; set; }

	public UIMessageCallbacks Callbacks { get; } = new UIMessageCallbacks();

	public int MaxViewCount { get; set; }

	public IMessageContent MessageData { get; set; }

	public void CopyValues(MessageUIData newData)
	{
		LayoutType = newData.LayoutType;
		Callbacks.OnViewed = newData.Callbacks.OnViewed;
		Callbacks.OnClosed = newData.Callbacks.OnClosed;
		Callbacks.OnStoreOpened = newData.Callbacks.OnStoreOpened;
		Callbacks.OnDisplayed = newData.Callbacks.OnDisplayed;
		Priority = newData.Priority;
		MaxViewCount = newData.MaxViewCount;
		MessageData = newData.MessageData;
	}
}
