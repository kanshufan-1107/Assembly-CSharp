using PegasusShared;
using UnityEngine;

public class TavernBrawlPhoneDeckTray : BasePhoneDeckTray
{
	[CustomEditField(Sections = "Buttons")]
	public StandardPegButtonNew m_RetireButton;

	private static TavernBrawlPhoneDeckTray s_instance;

	protected override void Awake()
	{
		base.Awake();
		m_RetireButton.AddEventListener(UIEventType.RELEASE, OnRetireClicked);
		s_instance = this;
	}

	private void OnDestroy()
	{
		s_instance = null;
		CollectionManager.Get().ClearEditedDeck();
	}

	public static TavernBrawlPhoneDeckTray Get()
	{
		return s_instance;
	}

	public void Initialize()
	{
		CollectionDeck tavernBrawlDeck = TavernBrawlManager.Get().CurrentDeck();
		if (tavernBrawlDeck != null)
		{
			OnTavernBrawlDeckInitialized(tavernBrawlDeck);
		}
	}

	private void OnTavernBrawlDeckInitialized(CollectionDeck tavernBrawlDeck)
	{
		if (tavernBrawlDeck == null)
		{
			Debug.LogError("Tavern Brawl deck is null.");
			return;
		}
		CollectionManager.Get().SetEditedDeck(tavernBrawlDeck);
		OnCardCountUpdated(tavernBrawlDeck.GetTotalCardCount(), tavernBrawlDeck.GetMaxCardCount());
		m_cardsContent.UpdateCardList();
	}

	private void OnRetireClicked(UIEvent e)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_showAlertIcon = false;
		info.m_headerText = GameStrings.Get("GLUE_TAVERN_BRAWL_RETIRE_CONFIRM_HEADER");
		if (TavernBrawlManager.Get().CurrentSeasonBrawlMode == TavernBrawlMode.TB_MODE_HEROIC)
		{
			info.m_text = GameStrings.Get("GLUE_TAVERN_BRAWL_RETIRE_CONFIRM_DESC");
		}
		else
		{
			info.m_text = GameStrings.Get("GLUE_BRAWLISEUM_RETIRE_CONFIRM_DESC");
		}
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_responseCallback = OnRetireButtonConfirmationResponse;
		DialogManager.Get().ShowPopup(info);
	}

	private void OnRetireButtonConfirmationResponse(AlertPopup.Response response, object userData)
	{
		if (response != AlertPopup.Response.CANCEL)
		{
			Network.Get().TavernBrawlRetire();
		}
	}
}
