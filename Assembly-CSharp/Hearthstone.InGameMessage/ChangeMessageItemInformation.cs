namespace Hearthstone.InGameMessage;

public class ChangeMessageItemInformation
{
	public InGameMessageItemDisplayContent.ItemType ItemType { get; set; }

	public string ItemId { get; set; }

	public TAG_PREMIUM itemPremiumType { get; set; }
}
