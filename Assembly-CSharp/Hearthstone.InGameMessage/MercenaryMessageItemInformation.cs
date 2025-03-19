namespace Hearthstone.InGameMessage;

public class MercenaryMessageItemInformation
{
	public InGameMessageItemDisplayContent.ItemType ItemType { get; set; }

	public string ItemId { get; set; }

	public TAG_PREMIUM itemPremiumType { get; set; }

	public int mercArtVariationId { get; set; }
}
