using System.Collections.Generic;

public class ZilliaxCustomizableCardEventHandler : CollectionCardEventHandler
{
	public override void OnCardAdded(CollectionDeckTray collectionDeckTray, CollectionDeck deck, EntityDef cardEntityDef, TAG_PREMIUM premium, Actor animateActor)
	{
		SideboardDeck zilliaxSideboard = deck?.GetSideboard(cardEntityDef.GetCardId());
		if (zilliaxSideboard == null)
		{
			return;
		}
		List<int> sideboardCardIds = new List<int>(zilliaxSideboard.GetCards());
		zilliaxSideboard.RemoveAllCards();
		foreach (int cardId in sideboardCardIds)
		{
			zilliaxSideboard.AddCard(GameUtils.TranslateDbIdToCardId(cardId), premium, false, null);
		}
		if (deck.CreatedFromShareableDeck == null && !deck.IsCreatedWithDeckComplete)
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_BUILD_A_ZILLIAX_TOOLTIP_FTUE, out long hasSeenZilliaxTutorial);
			if (hasSeenZilliaxTutorial <= 0)
			{
				DeckTrayDeckTileVisual tileVisual = collectionDeckTray.SnapToCardInDeckTray(cardEntityDef.GetCardId(), deck);
				zilliaxSideboard.DataModel.HighlightEditButton = true;
				collectionDeckTray.ShowSideboardPopupForTutorial(tileVisual, GameStrings.Get("GLUE_ZILLIAX_FTUE_SIDEBOARD"));
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_BUILD_A_ZILLIAX_TOOLTIP_FTUE, 1L));
			}
		}
	}
}
