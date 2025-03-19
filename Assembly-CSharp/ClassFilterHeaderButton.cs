using System;
using UnityEngine;

public class ClassFilterHeaderButton : PegUIElement
{
	public SlidingTray m_classFilterTray;

	public UberText m_headerText;

	public Transform m_showTwoRowsBone;

	public ClassFilterButtonContainer m_container;

	public GameObject m_sparkleVFX;

	public GameObject m_persistentClassGlow;

	public event Action OnPressed;

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
		NotificationManager.Get().DestroyAllPopUps();
		m_container.UpdateButtons();
		m_classFilterTray.ToggleTraySlider(show: true, m_showTwoRowsBone);
		this.OnPressed?.Invoke();
	}

	public void SetMode(CollectionUtils.ViewMode mode, TAG_CLASS? classTag, string textOverride = "")
	{
		Log.CollectionManager.Print("transitionPageId={0} mode={1} classTag={2}", CollectionManager.Get().GetCollectibleDisplay().GetPageManager()
			.GetTransitionPageId(), mode, classTag);
		if (!string.IsNullOrEmpty(textOverride))
		{
			m_headerText.Text = textOverride;
			return;
		}
		switch (mode)
		{
		case CollectionUtils.ViewMode.CARD_BACKS:
			m_headerText.Text = GameStrings.Get("GLUE_COLLECTION_MANAGER_CARD_BACKS_TITLE");
			return;
		case CollectionUtils.ViewMode.HERO_SKINS:
		case CollectionUtils.ViewMode.HERO_PICKER:
			m_headerText.Text = GameStrings.Get("GLUE_COLLECTION_MANAGER_HERO_SKINS_TITLE");
			return;
		case CollectionUtils.ViewMode.COINS:
			m_headerText.Text = GameStrings.Get("GLUE_COLLECTION_MANAGER_COIN_TITLE");
			return;
		}
		if (classTag.HasValue)
		{
			m_headerText.Text = GameStrings.GetClassName(classTag.Value);
		}
		else
		{
			m_headerText.Text = "";
		}
	}

	public void PlaySparkleVFX()
	{
		if (m_sparkleVFX != null)
		{
			PlayMakerFSM playMakerFSM = m_sparkleVFX.GetComponentInChildren<PlayMakerFSM>();
			if (playMakerFSM != null)
			{
				playMakerFSM.SendEvent("Activate");
			}
		}
	}

	public void SetShouldShowPersistentClassGlow(bool shouldShowGlow)
	{
		if (m_persistentClassGlow != null)
		{
			m_persistentClassGlow.SetActive(shouldShowGlow);
		}
	}
}
