using UnityEngine;

namespace Hearthstone.InGameMessage;

public class ShopMessageContent : IMessageContent
{
	public string Title { get; set; }

	public string TextBody { get; set; }

	public long ProductID { get; set; }

	public bool OpenFullShop { get; set; }

	public string ShopDeepLink { get; set; }

	public string TextureAssetUrl { get; internal set; }

	public Texture ImageTexture { get; internal set; }
}
