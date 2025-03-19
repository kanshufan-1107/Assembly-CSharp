namespace Hearthstone.InGameMessage;

public class InGameMessageItemDisplayContent
{
	public enum ItemType
	{
		Card,
		Hero,
		Merc,
		Bounty,
		BattlegroundsCard
	}

	public ItemType ChangeType { get; set; }

	public string itemChangeId { get; set; }

	public TAG_PREMIUM itemPremiumType { get; set; }

	public int mercArtVariationId { get; set; }
}
