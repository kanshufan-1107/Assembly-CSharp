public class ZilliaxPortraitCardVisual : PegUIElement
{
	protected override void Awake()
	{
		base.Awake();
		SetDragTolerance(5f);
	}

	protected override void OnDrag()
	{
		if (CollectionManager.Get().GetEditedDeck()?.GetCurrentSideboardDeck() is ZilliaxSideboardDeck zilliaxSideboardDeck)
		{
			CollectionDeckSlot cosmeticDeckSlot = zilliaxSideboardDeck.GetCosmeticModuleCollectionDeckSlot();
			if (cosmeticDeckSlot != null)
			{
				CollectionDeckTray collectionDeckTray = CollectionDeckTray.Get();
				DeckTrayDeckTileVisual deckTileVisual = collectionDeckTray.GetCurrentCardListContext().CreateCardTileVisual("DraggableZilliaxPortrait", base.transform);
				CollectionDeckSlot fakeDeckSlot = new CollectionDeckSlot();
				fakeDeckSlot.CardID = GameUtils.TranslateDbIdToCardId(cosmeticDeckSlot.GetEntityDef().GetTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_LINKED_FUNCTIONALMOUDLE));
				fakeDeckSlot.SetCount(1, cosmeticDeckSlot.PreferredPremium);
				deckTileVisual.SetSlot(zilliaxSideboardDeck, fakeDeckSlot, useSliderAnimations: false, offsetCardNameForRunes: false);
				collectionDeckTray.RemoveCard(cosmeticDeckSlot.CardID, cosmeticDeckSlot.PreferredPremium, valid: true);
				CollectionInputMgr.Get().GrabCardTile(deckTileVisual, removeCard: false);
			}
		}
	}
}
