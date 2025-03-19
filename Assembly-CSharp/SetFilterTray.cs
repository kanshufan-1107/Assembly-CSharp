using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Services;
using PegasusShared;
using UnityEngine;

public class SetFilterTray : MonoBehaviour
{
	public UIBScrollable m_scroller;

	public GameObject m_contents;

	public CollectionSetFilterDropdownToggle m_toggleButton;

	public PegUIElement m_hideArea;

	public GameObject m_trayObject;

	public GameObject m_contentsBone;

	public GameObject m_headerPrefab;

	public GameObject m_itemPrefab;

	public GameObject m_showBone;

	public GameObject m_hideBone;

	public GameObject m_setFilterButtonGlow;

	private bool m_shown;

	private FormatType m_formatType = FormatType.FT_WILD;

	private bool m_editingDeck;

	private bool m_showUnownedSets;

	private bool m_isAnimating;

	private bool m_glowEnabled;

	private List<SetFilterItem> m_items = new List<SetFilterItem>();

	private float m_lastCollectionQueryTime;

	private HashSet<TAG_CARD_SET> m_setsWithOwnedCards = new HashSet<TAG_CARD_SET>();

	private SetFilterItem m_selected;

	private SetFilterItem m_lastSelected;

	private void Awake()
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_toggleButton.AddEventListener(UIEventType.PRESS, delegate
			{
				Show(show: true);
			});
			m_hideArea.AddEventListener(UIEventType.PRESS, delegate
			{
				Show(show: false);
			});
			m_trayObject.SetActive(value: false);
		}
		else
		{
			m_hideArea.gameObject.SetActive(value: false);
		}
		m_toggleButton.gameObject.SetActive(value: false);
		CollectionManager.Get().RegisterEditedDeckChanged(OnEditedDeckChanged);
	}

	public void SetButtonShown(bool isShown)
	{
		m_toggleButton.gameObject.SetActive(isShown);
	}

	public void SetButtonEnabled(bool isEnabled)
	{
		m_toggleButton.SetEnabled(isEnabled);
		m_toggleButton.SetEnabledVisual(isEnabled);
		if (m_setFilterButtonGlow != null)
		{
			m_setFilterButtonGlow.SetActive(m_glowEnabled && isEnabled);
		}
	}

	public void SetFilterButtonGlowActive(bool active)
	{
		m_glowEnabled = active;
		if (m_setFilterButtonGlow != null)
		{
			m_setFilterButtonGlow.SetActive(active);
		}
	}

	public void AddHeader(string headerName, FormatType formatType, bool editModeOnly = false)
	{
		GameObject obj = Object.Instantiate(m_headerPrefab);
		GameUtils.SetParent(obj, m_contents);
		obj.SetActive(value: false);
		SetFilterItem item = obj.GetComponent<SetFilterItem>();
		UIBScrollableItem scrollableItem = obj.GetComponent<UIBScrollableItem>();
		item.IsHeader = true;
		item.Text = headerName;
		item.Height = scrollableItem.m_size.z;
		item.FormatType = formatType;
		item.EditModeOnly = editModeOnly;
		m_items.Add(item);
	}

	public void AddItem(string itemName, string iconTextureAssetRef, UnityEngine.Vector2? iconOffset, SetFilterItem.ItemSelectedCallback callback, List<TAG_CARD_SET> data, FormatType formatType, bool isAllStandard = false, bool editModeOnly = false)
	{
		SetFilterItem item = AddItemUsingTexture(itemName, null, iconOffset, callback, data, null, formatType, isAllStandard, tooltipActive: false, null, null, editModeOnly);
		if (string.IsNullOrEmpty(iconTextureAssetRef))
		{
			return;
		}
		AssetHandleCallback<Texture> loadTextureCb = delegate(AssetReference assetRef, AssetHandle<Texture> texture, object loadTextureCbData)
		{
			SetFilterItem setFilterItem = loadTextureCbData as SetFilterItem;
			if (setFilterItem == null)
			{
				texture?.Dispose();
			}
			else
			{
				ServiceManager.Get<DisposablesCleaner>()?.Attach(setFilterItem, texture);
				setFilterItem.IconTexture = texture;
				setFilterItem.IconOffset = iconOffset;
			}
		};
		AssetLoader.Get().LoadAsset(iconTextureAssetRef, loadTextureCb, item);
	}

	public SetFilterItem AddItemUsingTexture(string itemName, Texture iconTexture, UnityEngine.Vector2? iconOffset, SetFilterItem.ItemSelectedCallback callback, List<TAG_CARD_SET> cardSets, List<int> specificCards, FormatType formatType, bool isAllStandard = false, bool tooltipActive = false, string tooltipHeadline = null, string tooltipDescription = null, bool editModeOnly = false)
	{
		GameObject obj = Object.Instantiate(m_itemPrefab);
		SetFilterItem item = obj.GetComponent<SetFilterItem>();
		GameUtils.SetParent(obj, m_contents);
		obj.SetActive(value: false);
		UIBScrollableItem scrollableItem = obj.GetComponent<UIBScrollableItem>();
		item.IsHeader = false;
		item.Text = itemName;
		item.Height = scrollableItem.m_size.z;
		item.FormatType = formatType;
		item.IsAllStandard = isAllStandard;
		item.CardSets = cardSets;
		item.SpecificCards = specificCards;
		item.Callback = callback;
		item.IconTexture = iconTexture;
		item.IconOffset = iconOffset;
		item.TooltipHeadline = tooltipHeadline;
		item.TooltipDescription = tooltipDescription;
		item.Tooltip.ScreenConstraintLayerOverride = GameLayer.Default;
		item.ShowTooltip = tooltipActive;
		item.EditModeOnly = editModeOnly;
		m_items.Add(item);
		item.AddEventListener(UIEventType.RELEASE, delegate
		{
			Select(item);
		});
		return item;
	}

	public void SelectFirstItem(bool transitionPage = true)
	{
		foreach (SetFilterItem item in m_items)
		{
			if (!item.IsHeader)
			{
				UIBScrollableItem scrollableItem = item.GetComponent<UIBScrollableItem>();
				if (scrollableItem != null && scrollableItem.m_active == UIBScrollableItem.ActiveState.Active)
				{
					Select(item, callCallback: true, transitionPage);
					break;
				}
			}
		}
	}

	public bool SelectFirstItemWithFormat(FormatType formatType, bool transitionPage = true)
	{
		SetFilterItem firstItemWithFormat = m_items.Where(delegate(SetFilterItem item)
		{
			UIBScrollableItem component = item.GetComponent<UIBScrollableItem>();
			return !item.IsHeader && item.FormatType == formatType && component != null && component.m_active == UIBScrollableItem.ActiveState.Active;
		}).FirstOrDefault();
		if (firstItemWithFormat == null)
		{
			return false;
		}
		Select(firstItemWithFormat, transitionPage);
		return true;
	}

	public bool HasActiveFilter()
	{
		foreach (SetFilterItem item in m_items)
		{
			if (!item.IsHeader && item.GetComponent<UIBScrollableItem>().m_active != UIBScrollableItem.ActiveState.Inactive)
			{
				if (item == m_selected)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	public void Select(SetFilterItem item, bool callCallback = true, bool transitionPage = true)
	{
		if (!(item == m_selected))
		{
			if (m_selected != null)
			{
				m_selected.SetSelected(selected: false);
				m_lastSelected = m_selected;
			}
			m_selected = item;
			item.SetSelected(selected: true);
			if (callCallback)
			{
				item.Callback(item.CardSets, item.SpecificCards, item.FormatType, item, transitionPage);
			}
			m_toggleButton.SetToggleIcon(item.IconTexture, item.IconOffset.Value);
		}
	}

	public void SelectPreviouslySelectedItem()
	{
		Select(m_lastSelected, callCallback: false);
	}

	public void UpdateSetFilters(FormatType formatType, bool editingDeck, bool showUnownedSets)
	{
		if (m_formatType != formatType || m_editingDeck != editingDeck || m_showUnownedSets != showUnownedSets)
		{
			m_formatType = formatType;
			m_editingDeck = editingDeck;
			m_showUnownedSets = showUnownedSets;
			Arrange();
		}
	}

	public void ClearFilter(bool transitionPage = true)
	{
		SelectFirstItem(transitionPage);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			SetButtonShown(isShown: false);
		}
	}

	public void Show(bool show)
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			if (m_isAnimating)
			{
				return;
			}
			m_shown = show;
			m_trayObject.SetActive(value: true);
			m_hideArea.gameObject.SetActive(value: true);
			UIBHighlight toggleButtonHighlight = m_toggleButton.GetComponent<UIBHighlight>();
			if (toggleButtonHighlight != null)
			{
				toggleButtonHighlight.AlwaysOver = show;
			}
			m_isAnimating = true;
			if (show)
			{
				Arrange();
				m_trayObject.transform.localPosition = m_hideBone.transform.localPosition;
				Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
				tweenArgs.Add("position", m_showBone.transform.localPosition);
				tweenArgs.Add("time", 0.35f);
				tweenArgs.Add("easetype", iTween.EaseType.easeOutCubic);
				tweenArgs.Add("islocal", true);
				tweenArgs.Add("oncomplete", "FinishFilterShown");
				tweenArgs.Add("oncompletetarget", base.gameObject);
				iTween.MoveTo(m_trayObject, tweenArgs);
				SoundManager.Get().LoadAndPlay("choose_opponent_panel_slide_on.prefab:66491d3d01ed663429ab80daf6a5e880", base.gameObject);
			}
			else
			{
				m_trayObject.transform.localPosition = m_showBone.transform.localPosition;
				Hashtable tweenArgs2 = iTweenManager.Get().GetTweenHashTable();
				tweenArgs2.Add("position", m_hideBone.transform.localPosition);
				tweenArgs2.Add("time", 0.25f);
				tweenArgs2.Add("easetype", iTween.EaseType.easeOutCubic);
				tweenArgs2.Add("islocal", true);
				tweenArgs2.Add("oncomplete", "FinishFilterHidden");
				tweenArgs2.Add("oncompletetarget", base.gameObject);
				iTween.MoveTo(m_trayObject, tweenArgs2);
				SoundManager.Get().LoadAndPlay("choose_opponent_panel_slide_off.prefab:3139d09eb94899d41b9bf612649f47bf", base.gameObject);
			}
			m_hideArea.gameObject.SetActive(m_shown);
		}
		else
		{
			m_shown = show;
			if (show)
			{
				Arrange();
			}
		}
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.HideSetFilterTutorial();
		}
	}

	public bool IsShown()
	{
		return m_shown;
	}

	private void FinishFilterShown()
	{
		m_isAnimating = false;
	}

	private void FinishFilterHidden()
	{
		m_isAnimating = false;
		m_trayObject.SetActive(value: false);
		m_hideArea.gameObject.SetActive(value: false);
	}

	private void Arrange()
	{
		m_scroller.ClearVisibleAffectObjects();
		if (!m_showUnownedSets)
		{
			EvaluateOwnership();
		}
		Vector3 position = m_contentsBone.transform.position;
		bool needToResetFilter = false;
		List<TAG_CARD_SET> tavernBrawlCardSets = new List<TAG_CARD_SET>();
		TavernBrawlDisplay tbDisplay = TavernBrawlDisplay.Get();
		bool isTavernBrawlSetsOnly = false;
		if (tbDisplay != null && tbDisplay.IsInDeckEditMode())
		{
			TavernBrawlManager tbManager = TavernBrawlManager.Get();
			if (tbManager != null && tbManager.CurrentMission().showOnlySetsFromDeckRuleset)
			{
				isTavernBrawlSetsOnly = true;
				tavernBrawlCardSets = GameUtils.GetSetsForDeckRuleset(TavernBrawlManager.Get().GetCurrentDeckRuleset()).ToList();
			}
		}
		foreach (SetFilterItem item in m_items)
		{
			UIBScrollableItem scrollableItem = item.GetComponent<UIBScrollableItem>();
			if (scrollableItem == null)
			{
				Debug.LogWarning("SetFilterItem has no UIBScrollableItem component!");
				continue;
			}
			bool shouldSkipItem = false;
			if (item.FormatType == FormatType.FT_WILD && m_formatType != FormatType.FT_WILD)
			{
				shouldSkipItem = true;
			}
			else if (item.FormatType == FormatType.FT_CLASSIC && m_formatType == FormatType.FT_STANDARD)
			{
				shouldSkipItem = true;
			}
			else if (item.FormatType != FormatType.FT_CLASSIC && m_formatType == FormatType.FT_CLASSIC)
			{
				shouldSkipItem = true;
			}
			else if (item.FormatType == FormatType.FT_TWIST && !RankMgr.IsCurrentTwistSeasonActive())
			{
				shouldSkipItem = true;
			}
			else if (item.FormatType == FormatType.FT_TWIST && m_formatType == FormatType.FT_STANDARD)
			{
				shouldSkipItem = true;
			}
			else if (item.FormatType != FormatType.FT_TWIST && m_formatType == FormatType.FT_TWIST)
			{
				shouldSkipItem = true;
			}
			else if (m_editingDeck && item.IsAllStandard && m_formatType == FormatType.FT_WILD)
			{
				shouldSkipItem = true;
			}
			else if (m_editingDeck && item.FormatType == FormatType.FT_CLASSIC && m_formatType != FormatType.FT_CLASSIC)
			{
				shouldSkipItem = true;
			}
			else if (m_editingDeck && item.FormatType == FormatType.FT_TWIST && m_formatType != FormatType.FT_TWIST)
			{
				shouldSkipItem = true;
			}
			else if (!m_showUnownedSets && !OwnCardInSetsForItem(item))
			{
				shouldSkipItem = true;
			}
			else if (item.EditModeOnly && !m_editingDeck)
			{
				shouldSkipItem = true;
			}
			else if (isTavernBrawlSetsOnly && item.CardSets != null && item.CardSets.Count == 1 && !tavernBrawlCardSets.Contains(item.CardSets[0]))
			{
				shouldSkipItem = true;
			}
			if (shouldSkipItem)
			{
				if (item == m_selected)
				{
					needToResetFilter = true;
				}
				item.gameObject.SetActive(value: false);
				scrollableItem.m_active = UIBScrollableItem.ActiveState.Inactive;
			}
			else
			{
				item.gameObject.SetActive(value: true);
				scrollableItem.m_active = UIBScrollableItem.ActiveState.Active;
				item.gameObject.transform.position = position;
				position.z -= item.Height;
				m_scroller.AddVisibleAffectedObject(item.gameObject, new Vector3(item.Height, item.Height, item.Height), visible: true);
			}
		}
		if (needToResetFilter)
		{
			SelectFirstItem();
		}
		m_scroller.UpdateAndFireVisibleAffectedObjects();
	}

	private void EvaluateOwnership(bool force = false)
	{
		if (m_lastCollectionQueryTime > CollectionManager.Get().CollectionLastModifiedTime() && !force)
		{
			return;
		}
		List<TAG_CLASS> currentEditingClasses = new List<TAG_CLASS>();
		CollectionDeck currentEditingDeck = CollectionManager.Get().GetEditedDeck();
		if (currentEditingDeck != null)
		{
			currentEditingClasses = currentEditingDeck.GetClasses();
		}
		m_setsWithOwnedCards.Clear();
		float profileStart = Time.realtimeSinceStartup;
		List<CollectibleCard> cards = CollectionManager.Get().GetAllCards();
		for (int i = 0; i < cards.Count; i++)
		{
			CollectibleCard card = cards[i];
			if (card.OwnedCount <= 0)
			{
				continue;
			}
			if (currentEditingClasses.Count > 0)
			{
				if (currentEditingClasses.Contains(card.Class) || card.Class == TAG_CLASS.NEUTRAL)
				{
					m_setsWithOwnedCards.Add(card.Set);
				}
			}
			else
			{
				m_setsWithOwnedCards.Add(card.Set);
			}
		}
		Log.Performance.Print("SetFilterTray - Evaluating Ownership took {0} seconds.", Time.realtimeSinceStartup - profileStart);
		m_lastCollectionQueryTime = Time.realtimeSinceStartup;
	}

	private bool OwnCardInSetsForItem(SetFilterItem item)
	{
		if (item.CardSets == null)
		{
			return true;
		}
		for (int i = 0; i < item.CardSets.Count; i++)
		{
			if (m_setsWithOwnedCards.Contains(item.CardSets[i]))
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveAllItems()
	{
		foreach (SetFilterItem item in m_items)
		{
			item.gameObject.SetActive(value: false);
		}
		m_items.Clear();
	}

	private void OnEditedDeckChanged(CollectionDeck newDeck, CollectionDeck oldDeck, object callbackData)
	{
		EvaluateOwnership(force: true);
	}

	private void OnDestroy()
	{
		CollectionManager.Get().RemoveEditedDeckChanged(OnEditedDeckChanged);
	}
}
