using System.Collections.Generic;

namespace Hearthstone.InGameMessage;

public class TextMessageContent : IMessageContent
{
	public string Title { get; set; }

	public string ImageType { get; set; }

	public string TextBody { get; set; }

	public string ImageMaterial { get; set; }

	public string ImageTexture { get; set; }

	public List<TextMessageItemInformation> DisplayItems { get; set; }

	public string Url { get; set; }
}
