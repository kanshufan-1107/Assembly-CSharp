public static class ShopPopupSpawner
{
	public static void CreatePurchaseConfirmPopup(string localizedProductName, CurrencyType currencyType, AlertPopup.ResponseCallback callback)
	{
		string headerText;
		string bodyText;
		switch (currencyType)
		{
		case CurrencyType.GOLD:
			headerText = GameStrings.Get("GLUE_SHOP_GOLD_PURCHASE_WARNING_HEADER");
			bodyText = GameStrings.Format("GLUE_SHOP_GOLD_PURCHASE_WARNING", localizedProductName);
			break;
		case CurrencyType.CN_RUNESTONES:
		case CurrencyType.ROW_RUNESTONES:
			headerText = GameStrings.Get("GLUE_SHOP_RUNESTONES_PURCHASE_WARNING_HEADER");
			bodyText = GameStrings.Format("GLUE_SHOP_RUNESTONES_PURCHASE_WARNING", localizedProductName);
			break;
		default:
			Log.Store.PrintError($"Cannot handle currency {currencyType} for confirmation");
			callback?.Invoke(AlertPopup.Response.CANCEL, null);
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_confirmText = GameStrings.Get("GLOBAL_CONFIRM"),
			m_cancelText = GameStrings.Get("GLOBAL_CANCEL"),
			m_alertTextAlignment = UberText.AlignmentOptions.Center,
			m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle,
			m_headerText = headerText,
			m_text = bodyText,
			m_responseCallback = callback
		};
		DialogManager.Get().ShowPopup(info);
	}
}
