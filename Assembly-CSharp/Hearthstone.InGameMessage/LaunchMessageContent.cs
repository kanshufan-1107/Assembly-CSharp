namespace Hearthstone.InGameMessage;

public class LaunchMessageContent : IMessageContent
{
	public string Title { get; set; }

	public string TextBody { get; set; }

	public string IconType { get; set; }

	public string ImageMaterial { get; set; }

	public string ImageTexture { get; set; }

	public string SubLayout { get; set; }

	public LaunchMessageEffectContent Effect { get; set; }

	public string Url { get; set; }
}
