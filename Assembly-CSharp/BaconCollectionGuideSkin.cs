public class BaconCollectionGuideSkin : BaconCollectionSkin
{
	protected override string GetFavoritedText()
	{
		return GameStrings.Get("GLUE_BACON_COLLECTION_FAVORITE_GUIDE");
	}

	public void SetCardStateDisplay(CollectibleCard card)
	{
		if (CollectionManager.Get().GetBattlegroundsGuideSkinIdForCardId(card.CardDbId, out var _) && !CollectionManager.Get().OwnsBattlegroundsGuideSkin(card.CardDbId))
		{
			base.gameObject.GetComponent<Actor>().MissingCardEffect();
		}
	}
}
