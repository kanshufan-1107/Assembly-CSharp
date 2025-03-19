using System.Collections.Generic;

namespace Hearthstone.InGameMessage;

public class MercenaryMessageContent : IMessageContent
{
	public string Title { get; set; }

	public string BodyText { get; set; }

	public string Url { get; set; }

	public List<MercenaryMessageItemInformation> Items { get; set; }
}
