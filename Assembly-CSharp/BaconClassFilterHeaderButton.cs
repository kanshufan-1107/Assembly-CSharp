using UnityEngine;

public class BaconClassFilterHeaderButton : PegUIElement
{
	public SlidingTray m_classFilterTray;

	public UberText m_headerText;

	public Transform m_showTwoRowsBone;

	private ClassFilterButton[] m_buttons;

	protected override void Awake()
	{
		AddEventListener(UIEventType.RELEASE, delegate
		{
			HandleRelease();
		});
		base.Awake();
	}

	public void HandleRelease()
	{
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.HideDeckHelpPopup();
		}
		CollectionManager.Get().GetEditedDeck();
		if (m_buttons == null)
		{
			m_buttons = m_classFilterTray.GetComponentsInChildren<ClassFilterButton>();
		}
		m_classFilterTray.ToggleTraySlider(show: true, m_showTwoRowsBone);
		NotificationManager.Get().DestroyAllPopUps();
	}

	public void SetMode(CollectionUtils.ViewMode mode)
	{
		Log.CollectionManager.Print("transitionPageId={0} mode={1}", CollectionManager.Get().GetCollectibleDisplay().GetPageManager()
			.GetTransitionPageId(), mode);
		switch (mode)
		{
		case CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS:
			m_headerText.Text = GameStrings.Get("GLUE_COLLECTION_MANAGER_HERO_SKINS_TITLE");
			break;
		case CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS:
			m_headerText.Text = GameStrings.Get("GLUE_BACON_COLLECTION_MANAGER_GUIDE_SKINS_TITLE");
			break;
		case CollectionUtils.ViewMode.BATTLEGROUNDS_BOARD_SKINS:
			m_headerText.Text = GameStrings.Get("GLUE_BACON_COLLECTION_MANAGER_BOARD_SKINS_TITLE");
			break;
		case CollectionUtils.ViewMode.BATTLEGROUNDS_FINISHERS:
			m_headerText.Text = GameStrings.Get("GLUE_BACON_COLLECTION_MANAGER_FINISHERS_TITLE");
			break;
		case CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES:
			m_headerText.Text = GameStrings.Get("GLUE_BACON_COLLECTION_MANAGER_EMOTES_TITLE");
			break;
		default:
			m_headerText.Text = "";
			break;
		}
	}
}
