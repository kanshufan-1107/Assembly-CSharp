public class GalakrondCollectionCardEventHandler : CollectionCardEventHandler
{
	public override void OnCardAdded(CollectionDeckTray collectionDeckTray, CollectionDeck deck, EntityDef _, TAG_PREMIUM __, Actor ____)
	{
		if (deck.CreatedFromShareableDeck != null || deck.IsCreatedWithDeckComplete)
		{
			return;
		}
		string galakrondCardIdForClass = GameUtils.GetGalakrondCardIdByClass(deck.GetClass());
		if (string.IsNullOrEmpty(galakrondCardIdForClass) || deck.GetCardIdCount(galakrondCardIdForClass) != 0)
		{
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_DECK_TRAY_GALAKROND_PROMPT_TITLE");
		info.m_text = GameStrings.Get("GLUE_DECK_TRAY_GALAKROND_PROMPT_DESC");
		info.m_iconSet = AlertPopup.PopupInfo.IconSet.Default;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_confirmText = GameStrings.Get("GLUE_COLLECTION_DECK_COMPLETE_POPUP_CONFIRM");
		info.m_cancelText = GameStrings.Get("GLUE_COLLECTION_DECK_COMPLETE_POPUP_CANCEL");
		info.m_responseCallback = delegate(AlertPopup.Response response, object userData)
		{
			if (response == AlertPopup.Response.CONFIRM)
			{
				TAG_PREMIUM? tAG_PREMIUM = deck.GetPreferredPremiumThatCanBeAdded(galakrondCardIdForClass);
				if (!tAG_PREMIUM.HasValue)
				{
					tAG_PREMIUM = TAG_PREMIUM.NORMAL;
				}
				collectionDeckTray.AddCard(DefLoader.Get().GetEntityDef(galakrondCardIdForClass), tAG_PREMIUM.Value, true, null);
			}
		};
		DialogManager.Get().ShowPopup(info);
	}
}
