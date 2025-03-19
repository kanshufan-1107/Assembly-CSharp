using Assets;

public class MercenariesCraftingTray : CraftingTrayBase
{
	public UIBButton m_doneButton;

	public UberText m_mercenaryCount;

	public CheckBox m_showOnlyPromotableCheckbox;

	public CheckBox m_showCraftableCheckbox;

	private bool m_shown;

	private static MercenariesCraftingTray s_instance;

	private void Awake()
	{
		s_instance = this;
	}

	private void Start()
	{
		m_doneButton.AddEventListener(UIEventType.RELEASE, OnDoneButtonReleased);
		m_showOnlyPromotableCheckbox.AddEventListener(UIEventType.RELEASE, ToggleShowOnlyPromotable);
		m_showOnlyPromotableCheckbox.SetChecked(isChecked: false);
		m_showCraftableCheckbox.AddEventListener(UIEventType.RELEASE, ToggleShowCraftable);
		m_showCraftableCheckbox.SetChecked(isChecked: false);
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static MercenariesCraftingTray Get()
	{
		return s_instance;
	}

	public override void Show(bool? overrideShowCraftable = null, bool? overrideShowOnlyPromotable = null, bool? unused1 = null, bool? unused2 = null, bool? unused3 = null, bool updatePage = true)
	{
		m_shown = true;
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.CRAFTING);
		if (overrideShowCraftable.HasValue)
		{
			m_showCraftableCheckbox.SetChecked(overrideShowCraftable.Value);
		}
		if (overrideShowOnlyPromotable.HasValue)
		{
			m_showOnlyPromotableCheckbox.SetChecked(overrideShowOnlyPromotable.Value);
		}
		SetMercenaryTotalCount();
		(CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as LettuceCollectionPageManager).ShowCraftingModeMercs(null, null, m_showCraftableCheckbox.IsChecked(), m_showOnlyPromotableCheckbox.IsChecked(), updatePage);
	}

	public override void Hide()
	{
		m_shown = false;
		PresenceMgr.Get().SetPrevStatus();
		CollectibleDisplay cd = CollectionManager.Get().GetCollectibleDisplay();
		if (cd != null)
		{
			cd.HideCraftingTray();
			BookPageManager.PageTransitionType pageTransitionType = BookPageManager.PageTransitionType.NONE;
			cd.GetPageManager().HideCraftingModeCards(pageTransitionType);
		}
	}

	public override bool IsShown()
	{
		return m_shown;
	}

	private void OnDoneButtonReleased(UIEvent e)
	{
		Hide();
	}

	private void SetMercenaryTotalCount()
	{
		int ownedCount = CollectionManager.Get().GetOwnedMercenaryCount();
		int totalCount = CollectionManager.Get().GetTotalMercenaryCount();
		m_mercenaryCount.Text = GameStrings.Format("GLUE_DECK_TRAY_COUNT", ownedCount, totalCount);
	}

	private void ToggleShowOnlyPromotable(UIEvent e)
	{
		bool isChecked = m_showOnlyPromotableCheckbox.IsChecked();
		(CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as LettuceCollectionPageManager).ShowCraftingModeMercs(null, null, m_showCraftableCheckbox.IsChecked(), isChecked, updatePage: true, toggleChanged: true);
		if (isChecked)
		{
			SoundManager.Get().LoadAndPlay("checkbox_toggle_on.prefab:8be4c59e7387600468ac88787943da8b", base.gameObject);
		}
		else
		{
			SoundManager.Get().LoadAndPlay("checkbox_toggle_off.prefab:fa341d119cee1d14c941b63dba112af3", base.gameObject);
		}
	}

	private void ToggleShowCraftable(UIEvent e)
	{
		bool isChecked = m_showCraftableCheckbox.IsChecked();
		(CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as LettuceCollectionPageManager).ShowCraftingModeMercs(null, null, isChecked, m_showOnlyPromotableCheckbox.IsChecked(), updatePage: true, toggleChanged: true);
		if (isChecked)
		{
			SoundManager.Get().LoadAndPlay("checkbox_toggle_on.prefab:8be4c59e7387600468ac88787943da8b", base.gameObject);
		}
		else
		{
			SoundManager.Get().LoadAndPlay("checkbox_toggle_off.prefab:fa341d119cee1d14c941b63dba112af3", base.gameObject);
		}
	}
}
