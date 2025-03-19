using System.Collections.Generic;

namespace Hearthstone.InGameMessage;

public class ChangeMessageContent : IMessageContent
{
	public string Title { get; set; }

	public string BodyText { get; set; }

	public string Url { get; set; }

	public List<ChangeMessageItemInformation> ChangeItems { get; set; }
}
