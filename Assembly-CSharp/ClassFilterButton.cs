using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class ClassFilterButton : PegUIElement
{
	public GameObject m_newCardCount;

	public UberText m_newCardCountText;

	public GameObject m_disabled;

	public CollectionUtils.ViewMode m_tabViewMode;

	[SerializeField]
	private GameObject m_touristBadge;

	[SerializeField]
	private GameObject m_persistentClassGlow;

	[SerializeField]
	private float m_tooltipScale;

	private CollectionTabInfo m_tabInfo;

	private TooltipZone m_tooltipZone;

	private bool m_shouldSuppressTouristTooltip;

	protected override void Awake()
	{
		m_tooltipZone = GetComponent<TooltipZone>();
		AddEventListener(UIEventType.RELEASE, delegate
		{
			HandleRelease();
		});
		AddEventListener(UIEventType.ROLLOVER, delegate
		{
			HandleRollover();
		});
		AddEventListener(UIEventType.ROLLOUT, delegate
		{
			HandleRollout();
		});
		CollectionManager cm = CollectionManager.Get();
		if (cm != null)
		{
			CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
			if (collectionManagerDisplay != null)
			{
				CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
				if (collectionPageManager != null)
				{
					collectionPageManager.OnSuppressTouristTooltipChanged += OnSuppressTouristTooltipChanged;
					m_shouldSuppressTouristTooltip = collectionPageManager.SuppressTouristTooltip;
				}
			}
		}
		base.Awake();
	}

	protected override void OnDestroy()
	{
		CollectionManager cm = CollectionManager.Get();
		if (cm != null)
		{
			CollectionManagerDisplay collectionManagerDisplay = cm.GetCollectibleDisplay() as CollectionManagerDisplay;
			if (collectionManagerDisplay != null)
			{
				CollectionPageManager collectionPageManager = collectionManagerDisplay.GetPageManager() as CollectionPageManager;
				if (collectionPageManager != null)
				{
					collectionPageManager.OnSuppressTouristTooltipChanged -= OnSuppressTouristTooltipChanged;
				}
			}
		}
		base.OnDestroy();
	}

	public void HandleRelease()
	{
		CollectionManager collectionManager = CollectionManager.Get();
		CollectibleDisplay collectibleDisplay = collectionManager.GetCollectibleDisplay();
		CollectionPageManager pageManager = collectibleDisplay.GetPageManager() as CollectionPageManager;
		switch (m_tabViewMode)
		{
		case CollectionUtils.ViewMode.CARDS:
			if (m_tabInfo.tagClass != 0)
			{
				if (pageManager == null)
				{
					Debug.Log("ClassFilterButton: HandleRelease: pageManager is null");
					return;
				}
				pageManager.JumpToCollectionClassPage(m_tabInfo);
			}
			break;
		case CollectionUtils.ViewMode.HERO_PICKER:
		{
			if (pageManager == null)
			{
				Debug.Log("ClassFilterButton: HandleRelease: pageManager is null");
				return;
			}
			CollectionUtils.ViewMode newViewMode = ((pageManager.IsSearching() || collectionManager.GetEditedDeck() != null) ? CollectionUtils.ViewMode.HERO_SKINS : CollectionUtils.ViewMode.HERO_PICKER);
			collectibleDisplay.SetViewMode(newViewMode);
			break;
		}
		case CollectionUtils.ViewMode.CARD_BACKS:
			collectibleDisplay.SetViewMode(CollectionUtils.ViewMode.CARD_BACKS);
			break;
		case CollectionUtils.ViewMode.COINS:
			collectibleDisplay.SetViewMode(CollectionUtils.ViewMode.COINS);
			break;
		}
		if (m_tooltipZone != null)
		{
			m_tooltipZone.HideTooltip();
		}
		GetComponentInParent<SlidingTray>().HideTray();
	}

	public void HandleRollover()
	{
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		bool isTouristClassButton = false;
		if (deck != null && deck.GetTouristClasses().Contains(m_tabInfo.tagClass))
		{
			isTouristClassButton = true;
		}
		if (m_tooltipZone != null && isTouristClassButton && !m_shouldSuppressTouristTooltip)
		{
			m_tooltipZone.ShowTooltip(GameStrings.Get("GLUE_TOURIST_CLASS_TAB_TOOLTIP_HEADER"), GameStrings.Get("GLUE_TOURIST_CLASS_TAB_TOOLTIP_DESCRIPTION"), m_tooltipScale);
		}
	}

	public void HandleRollout()
	{
		if (m_tooltipZone != null)
		{
			m_tooltipZone.HideTooltip();
		}
	}

	public void SetTabInfo(CollectionTabInfo tabInfo, Material material)
	{
		m_tabInfo = tabInfo;
		Renderer component = GetComponent<Renderer>();
		component.SetMaterial(material);
		bool disabled = m_tabInfo.tagClass == TAG_CLASS.INVALID;
		GetComponent<BoxCollider>().enabled = !disabled;
		component.enabled = !disabled;
		if (m_newCardCount != null)
		{
			m_newCardCount.SetActive(!disabled);
		}
		if (m_disabled != null)
		{
			m_disabled.SetActive(disabled);
		}
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		bool isTouristClassButton = false;
		if (deck != null && deck.GetTouristClasses().Contains(tabInfo.tagClass))
		{
			isTouristClassButton = true;
		}
		if (m_touristBadge != null)
		{
			m_touristBadge.SetActive(isTouristClassButton);
		}
		if (!(m_persistentClassGlow != null))
		{
			return;
		}
		bool shouldShowGlow = false;
		CollectionManager cm = CollectionManager.Get();
		if (cm != null)
		{
			CollectibleDisplay collectibleDisplay = cm.GetCollectibleDisplay();
			if (collectibleDisplay != null)
			{
				CollectionPageManager pageManager = collectibleDisplay.GetPageManager() as CollectionPageManager;
				if (pageManager != null)
				{
					shouldShowGlow = pageManager.ShouldClassTabShowPersistentGlow(tabInfo.tagClass);
				}
			}
		}
		m_persistentClassGlow.SetActive(shouldShowGlow);
	}

	public void SetFixedClassTabEnabled(bool isEnabled)
	{
		base.gameObject.SetActive(isEnabled);
		GetComponent<BoxCollider>().enabled = isEnabled;
		GetComponent<Renderer>().enabled = isEnabled;
		if (m_newCardCount != null)
		{
			m_newCardCount.SetActive(isEnabled);
		}
		if (m_disabled != null)
		{
			m_disabled.SetActive(!isEnabled);
		}
	}

	public void SetNewCardCount(int count)
	{
		if (m_newCardCount != null)
		{
			m_newCardCount.SetActive(count > 0);
		}
		if (count > 0 && m_newCardCountText != null)
		{
			m_newCardCountText.Text = GameStrings.Format("GLUE_COLLECTION_NEW_CARD_CALLOUT", count);
		}
	}

	private void OnSuppressTouristTooltipChanged(bool shouldSuppressTooltips)
	{
		m_shouldSuppressTouristTooltip = shouldSuppressTooltips;
		if (shouldSuppressTooltips && m_tooltipZone != null)
		{
			m_tooltipZone.HideTooltip();
		}
	}

	public TAG_CLASS GetTagClass()
	{
		return m_tabInfo.tagClass;
	}
}
