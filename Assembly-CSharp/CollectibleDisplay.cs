using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Services;
using PegasusShared;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class CollectibleDisplay : AbsSceneDisplay
{
	public delegate void DelTextureLoaded(TAG_CLASS classTag, Texture classTexture, object callbackData);

	public delegate void CollectionActorsReadyCallback(List<CollectionCardActors> actors, List<ICollectible> nonActorCollectibles, object callbackData);

	public delegate void ViewModeChangedListener(CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode mode, CollectionUtils.ViewModeData userdata, bool triggerResponse);

	public delegate void FilterStateListener(bool filterActive, object value);

	protected class TextureRequests
	{
		public class Request
		{
			public DelTextureLoaded m_callback;

			public object m_callbackData;
		}

		public List<Request> m_requests;
	}

	[CustomEditField(Sections = "Prefabs")]
	public CollectionCardVisual m_cardVisualPrefab;

	[CustomEditField(Sections = "Bones")]
	public GameObject m_activeSearchBone;

	[CustomEditField(Sections = "Bones")]
	public GameObject m_activeSearchBone_Win8;

	[CustomEditField(Sections = "Bones")]
	public GameObject m_craftingTrayHiddenBone;

	[CustomEditField(Sections = "Bones")]
	public GameObject m_craftingTrayShownBone;

	[CustomEditField(Sections = "Bones")]
	public GameObject m_root;

	[FormerlySerializedAs("m_coverPrefab")]
	[CustomEditField(Sections = "Objects", T = EditType.GAME_OBJECT)]
	public String_MobileOverride m_bookCoverPrefab;

	[CustomEditField(Sections = "Objects")]
	public CollectionCoverDisplay m_cover;

	[CustomEditField(Sections = "Objects")]
	public CollectionSearch m_search;

	[CustomEditField(Sections = "Objects")]
	public CraftingModeButton m_craftingModeButton;

	[CustomEditField(Sections = "Objects")]
	public ActiveFilterButton m_filterButton;

	[CustomEditField(Sections = "Objects")]
	public GameObject m_filterButtonGlow;

	[CustomEditField(Sections = "Objects")]
	public PegUIElement m_inputBlocker;

	[CustomEditField(Sections = "Controls")]
	public CollectionUtils.CollectionPageLayoutSettings m_pageLayoutSettings = new CollectionUtils.CollectionPageLayoutSettings();

	[CustomEditField(Sections = "Materials")]
	public Material m_goldenCardNotOwnedMeshMaterial;

	[CustomEditField(Sections = "Materials")]
	public Material m_cardNotOwnedMeshMaterial;

	protected bool m_netCacheReady;

	protected bool m_gameSaveDataReady;

	protected bool m_isReady;

	protected bool m_isBookOpened;

	protected bool m_unloading;

	protected List<CollectionCardActors> m_cardActors = new List<CollectionCardActors>();

	protected List<CollectionCardActors> m_previousCardActors = new List<CollectionCardActors>();

	protected bool m_setFilterTrayInitialized;

	protected bool m_isBookCoverLoading;

	protected CraftingTrayBase m_craftingTray;

	protected SetFilterTray m_setFilterTray;

	protected RelatedCardsTray m_relatedCardsTray;

	protected CollectionUtils.ViewMode m_currentViewMode;

	protected CollectionUtils.ViewSubmode m_currentViewSubmode;

	protected List<FilterStateListener> m_searchFilterListeners = new List<FilterStateListener>();

	protected int m_inputBlockers;

	protected bool m_searchTriggeredCrafting;

	protected bool m_searchTriggeredCraftingInBackground;

	protected const float CRAFTING_TRAY_SLIDE_IN_TIME = 0.25f;

	protected PlatformDependentValue<int> m_onscreenDecks = new PlatformDependentValue<int>(PlatformCategory.Screen)
	{
		PC = 8,
		Phone = 4
	};

	protected readonly PlatformDependentValue<bool> ALWAYS_SHOW_PAGING_ARROWS = new PlatformDependentValue<bool>(PlatformCategory.OS)
	{
		iOS = true,
		Android = true,
		PC = false,
		Mac = false
	};

	public event ViewModeChangedListener OnViewModeChanged;

	public override void Start()
	{
		base.Start();
		m_inputBlocker.AddEventListener(UIEventType.RELEASE, OnInputBlockerRelease);
		m_search.RegisterActivatedListener(OnSearchActivated);
		m_search.RegisterDeactivatedListener(OnSearchDeactivated);
		m_search.RegisterClearedListener(OnSearchCleared);
		int numPageFlips = Options.Get().GetInt(Option.PAGE_MOUSE_OVERS);
		CollectiblePageManager pageManager = GetPageManager();
		if (pageManager.m_numPlageFlipsBeforeStopShowingArrows == 0 || numPageFlips < pageManager.m_numPlageFlipsBeforeStopShowingArrows || (bool)ALWAYS_SHOW_PAGING_ARROWS)
		{
			pageManager.LoadPagingArrows();
		}
		m_currentViewMode = GetInitialViewMode();
	}

	protected virtual void Awake()
	{
		if (CollectionManager.Get() != null)
		{
			CollectionManager.Get().SetCollectibleDisplay(this);
		}
		if (ServiceManager.Get<IGraphicsManager>().RenderQualityLevel != 0 && PlatformSettings.Memory == MemoryCategory.High && m_cover == null && !m_isBookCoverLoading)
		{
			m_isBookCoverLoading = true;
			AssetLoader.Get().InstantiatePrefab((string)m_bookCoverPrefab, OnBookCoverLoaded);
		}
		LoadAllTextures();
		EnableInput(enable: true);
	}

	protected virtual void OnDestroy()
	{
		if (CollectionManager.Get() != null)
		{
			CollectionManager.Get().SetCollectibleDisplay(null);
		}
	}

	public Material GetGoldenCardNotOwnedMeshMaterial()
	{
		return m_goldenCardNotOwnedMeshMaterial;
	}

	public Material GetCardNotOwnedMeshMaterial()
	{
		return m_cardNotOwnedMeshMaterial;
	}

	public CollectionCardVisual GetCardVisualPrefab()
	{
		return m_cardVisualPrefab;
	}

	public abstract CollectiblePageManager GetPageManager();

	public bool IsReady()
	{
		return m_isReady;
	}

	public bool IsBookOpened()
	{
		return m_isBookOpened;
	}

	public abstract void Unload();

	public abstract void Exit();

	public abstract void CollectionPageContentsChanged<TCollectible>(ICollection<TCollectible> collectiblesToDisplay, CollectionActorsReadyCallback callback, object callbackData) where TCollectible : ICollectible;

	public abstract void SetViewMode(CollectionUtils.ViewMode mode, bool triggerResponse, CollectionUtils.ViewModeData userdata = null);

	public virtual void SetViewSubmode(CollectionUtils.ViewSubmode mode)
	{
		m_currentViewSubmode = mode;
	}

	public abstract void HideAllTips();

	public abstract void SetFilterCallback(List<TAG_CARD_SET> cardSets, List<int> specificCards, FormatType formatType, SetFilterItem item, bool transitionPage);

	public abstract void ShowInnkeeperLClickHelp(EntityDef entityDef);

	public bool ShouldShowNewCardGlow(string cardID, TAG_PREMIUM premium)
	{
		return CollectionManager.Get().GetCard(cardID, premium)?.IsNewCard ?? false;
	}

	public CollectionUtils.CollectionPageLayoutSettings.Variables GetCurrentPageLayoutSettings()
	{
		return GetPageLayoutSettings(m_currentViewMode);
	}

	public CollectionUtils.CollectionPageLayoutSettings.Variables GetPageLayoutSettings(CollectionUtils.ViewMode viewMode)
	{
		return m_pageLayoutSettings.GetVariables(viewMode);
	}

	public void SetViewMode(CollectionUtils.ViewMode mode, CollectionUtils.ViewModeData userdata = null)
	{
		SetViewMode(mode, triggerResponse: true, userdata);
	}

	public CollectionUtils.ViewMode GetViewMode()
	{
		return m_currentViewMode;
	}

	public CollectionUtils.ViewSubmode GetViewSubmode()
	{
		return m_currentViewSubmode;
	}

	public bool SetFilterTrayInitialized()
	{
		return m_setFilterTrayInitialized;
	}

	public virtual void FilterBySearchText(string newSearchText)
	{
		m_search.SetText(newSearchText);
	}

	protected virtual void ShowSpecificCards(List<int> specificCards)
	{
		GetPageManager().FilterBySpecificCards(specificCards);
	}

	public void GoToPageWithCard(string cardID, TAG_PREMIUM premium)
	{
		if (m_currentViewMode == CollectionUtils.ViewMode.DECK_TEMPLATE)
		{
			SetViewMode(CollectionUtils.ViewMode.CARDS, new CollectionUtils.ViewModeData
			{
				m_setPageByCard = cardID,
				m_setPageByPremium = premium
			});
		}
		else
		{
			GetPageManager().JumpToPageWithCard(cardID, premium);
		}
	}

	public void UpdateCurrentPageCardLocks(bool playSound = false)
	{
		if (GetPageManager() == null)
		{
			Log.CollectionManager.PrintError("CollectibleDisplay.UpdateCurrentPageCardLocks - GetPageManager returned null!");
		}
		else
		{
			GetPageManager().UpdateCurrentPageCardLocks(playSound);
		}
	}

	public bool ViewModeChangedListenerExists(ViewModeChangedListener listener)
	{
		return this.OnViewModeChanged.GetInvocationList().Contains(listener);
	}

	public void RegisterSearchFilterListener(FilterStateListener listener)
	{
		m_searchFilterListeners.Add(listener);
	}

	public void UnregisterSearchFilterListener(FilterStateListener listener)
	{
		m_searchFilterListeners.Remove(listener);
	}

	public virtual void ResetFilters(bool updateVisuals = true)
	{
		m_search.ClearFilter(updateVisuals);
	}

	public void EnableInput(bool enable)
	{
		if (!enable)
		{
			m_inputBlockers++;
		}
		else if (m_inputBlockers > 0)
		{
			m_inputBlockers--;
		}
		bool blockerEnabled = m_inputBlockers > 0;
		if (m_inputBlocker == null)
		{
			Log.CollectionManager.PrintError("CollectibleDisplay.EnableInput - input blocker is null!");
		}
		else
		{
			m_inputBlocker.gameObject.SetActive(blockerEnabled);
		}
	}

	public bool InCraftingMode()
	{
		if (m_craftingTray != null)
		{
			return m_craftingTray.IsShown();
		}
		return false;
	}

	protected override bool ShouldStartShown()
	{
		return true;
	}

	public override bool IsFinishedLoading(out string failureMessage)
	{
		failureMessage = "CollectibleDisplay is never ready.";
		return m_isReady;
	}

	protected void OnCollectionLoaded()
	{
		GetPageManager().OnCollectionLoaded();
	}

	protected virtual void OnCollectionChanged()
	{
		GetPageManager().NotifyOfCollectionChanged();
	}

	protected void NotifyFilterUpdate(List<FilterStateListener> listeners, bool active, object value)
	{
		foreach (FilterStateListener listener in listeners)
		{
			listener(active, value);
		}
	}

	protected virtual CollectionUtils.ViewMode GetInitialViewMode()
	{
		List<CollectibleCard> ownedCards = CollectionManager.Get().GetOwnedCards();
		if (ownedCards.Any((CollectibleCard x) => !x.IsHeroSkin))
		{
			return CollectionUtils.ViewMode.CARDS;
		}
		if (CardBackManager.Get().GetNumCardBacksOwned() > 0)
		{
			return CollectionUtils.ViewMode.CARD_BACKS;
		}
		if (ownedCards.Any((CollectibleCard x) => x.IsHeroSkin))
		{
			return CollectionUtils.ViewMode.HERO_SKINS;
		}
		if (CosmeticCoinManager.Get().GetCoinsOwned().Count > 0)
		{
			return CollectionUtils.ViewMode.COINS;
		}
		Debug.Log("CollectibleDisplay:GetInitialViewMode: Player has no cards, card backs, hero skins or coins. Defaulting to Cards view");
		return CollectionUtils.ViewMode.CARDS;
	}

	protected abstract void LoadAllTextures();

	protected abstract void UnloadAllTextures();

	protected virtual void OnBookCoverLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_isBookCoverLoading = false;
		if (m_root != null)
		{
			go.transform.SetParent(m_root.transform, worldPositionStays: true);
		}
		m_cover = go.GetComponent<CollectionCoverDisplay>();
	}

	protected void OnInputBlockerRelease(UIEvent e)
	{
		m_search.Deactivate();
	}

	protected void OnSearchActivated()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			EnableInput(enable: false);
		}
		GetPageManager().EnablePageTurn(enable: false);
	}

	protected abstract void OnSearchDeactivated(string oldSearchText, string newSearchText);

	protected virtual void OnSearchCleared(bool transitionPage)
	{
		GetPageManager().EnablePageTurn(enable: true);
	}

	protected void OnSearchFilterComplete(object callbackdata = null)
	{
		GetPageManager().EnablePageTurn(enable: true);
	}

	protected void OnCoverOpened()
	{
		m_isBookOpened = true;
		EnableInput(enable: true);
	}

	protected virtual void OnSwitchViewModeResponse(bool triggerResponse, CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode newMode, CollectionUtils.ViewModeData userdata)
	{
		this.OnViewModeChanged?.Invoke(prevMode, newMode, userdata, triggerResponse);
	}

	protected virtual CraftingTrayBase GetCraftingTrayComponent(GameObject go)
	{
		return go.GetComponent<CraftingTray>();
	}

	protected virtual void OnCraftingTrayLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		go.SetActive(value: false);
		m_craftingTray = GetCraftingTrayComponent(go);
		go.transform.parent = m_craftingTrayShownBone.transform.parent;
		go.transform.localPosition = m_craftingTrayHiddenBone.transform.localPosition;
		go.transform.localScale = m_craftingTrayHiddenBone.transform.localScale;
	}

	protected void OnCraftingModeButtonReleased(UIEvent e)
	{
		if (m_craftingTray.IsShown())
		{
			m_craftingTray.Hide();
		}
		else
		{
			ShowCraftingTray(null, null, null, null, null);
		}
	}

	public virtual void ShowCraftingTray(bool? includeUncraftable = null, bool? normalOwned = null, bool? normalMissing = null, bool? premiumOwned = null, bool? premiumMissing = null, bool updatePage = true)
	{
		m_craftingTray.gameObject.SetActive(value: true);
		m_craftingTray.Show(includeUncraftable, normalOwned, normalMissing, premiumOwned, premiumMissing, updatePage);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", m_craftingTrayShownBone.transform.localPosition);
		args.Add("islocal", true);
		args.Add("time", 0.25f);
		args.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.Stop(m_craftingTray.gameObject);
		iTween.MoveTo(m_craftingTray.gameObject, args);
		m_craftingModeButton.ShowActiveGlow(show: true);
	}

	public virtual void HideCraftingTray()
	{
		m_craftingTray.gameObject.SetActive(value: true);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", m_craftingTrayHiddenBone.transform.localPosition);
		args.Add("islocal", true);
		args.Add("time", 0.25f);
		args.Add("easetype", iTween.EaseType.easeOutBounce);
		args.Add("oncomplete", (Action<object>)delegate
		{
			m_craftingTray.gameObject.SetActive(value: false);
		});
		iTween.Stop(m_craftingTray.gameObject);
		iTween.MoveTo(m_craftingTray.gameObject, args);
		m_craftingModeButton.ShowActiveGlow(show: false);
	}
}
