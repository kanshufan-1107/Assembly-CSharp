using System;

public class MercenariesClassFilterHeaderButton : PegUIElement
{
	public SlidingTray m_roleFilterTray;

	public UberText m_headerText;

	public MercenariesClassFilterButtonContainer m_container;

	private TAG_ROLE m_displayedRole;

	protected override void Awake()
	{
		base.Awake();
		AddEventListener(UIEventType.RELEASE, delegate
		{
			HandleRelease();
		});
		LettuceCollectionPageManager pageManager = (CollectionManager.Get()?.GetCollectibleDisplay()?.GetPageManager() as LettuceCollectionPageManager) ?? null;
		if (pageManager != null)
		{
			pageManager.PageTransitioned += OnPageTransitioned;
		}
		RefreshHeaderText(pageManager);
	}

	protected override void OnDestroy()
	{
		LettuceCollectionPageManager pageManager = (CollectionManager.Get()?.GetCollectibleDisplay()?.GetPageManager() as LettuceCollectionPageManager) ?? null;
		if (pageManager != null)
		{
			pageManager.PageTransitioned -= OnPageTransitioned;
		}
		base.OnDestroy();
	}

	public void HandleRelease()
	{
		m_roleFilterTray.ToggleTraySlider(show: true);
		m_container.UpdateRoleButtons();
		NotificationManager.Get().DestroyAllPopUps();
	}

	public void OnPageTransitioned(object sender, EventArgs e)
	{
		RefreshHeaderText(sender as LettuceCollectionPageManager);
	}

	public void RefreshHeaderText(LettuceCollectionPageManager pageManager)
	{
		TAG_ROLE currentRoleContext = pageManager.CurrentRoleContext;
		if (currentRoleContext != m_displayedRole)
		{
			if (pageManager.CurrentRoleContext == TAG_ROLE.FIGHTER)
			{
				m_headerText.Text = GameStrings.Get("GLUE_LETTUCE_MERCENARY_TUTORIAL_INGAME_POPUP_BODY_2");
			}
			else if (pageManager.CurrentRoleContext == TAG_ROLE.CASTER)
			{
				m_headerText.Text = GameStrings.Get("GLUE_LETTUCE_MERCENARY_TUTORIAL_INGAME_POPUP_BODY_3");
			}
			else if (pageManager.CurrentRoleContext == TAG_ROLE.TANK)
			{
				m_headerText.Text = GameStrings.Get("GLUE_LETTUCE_MERCENARY_TUTORIAL_INGAME_POPUP_BODY_1");
			}
			m_displayedRole = currentRoleContext;
		}
	}
}
