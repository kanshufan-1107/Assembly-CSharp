using System.Collections.Generic;

namespace Hearthstone.InGameMessage;

public class MercenaryMessageBountyContent : IMessageContent
{
	public string Title { get; set; }

	public string BodyText { get; set; }

	public List<MercenaryMessageBountyItemInformation> Items { get; set; }

	public string Url { get; set; }
}
