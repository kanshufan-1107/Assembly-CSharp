using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hearthstone;
using UnityEngine;

[CustomEditClass]
public class BaconCollectionPageManager : CollectiblePageManager
{
	public CollectionClassTab m_heroSkinsTab;

	public CollectionClassTab m_guideSkinsTab;

	public CollectionClassTab m_boardSkinsTab;

	public CollectionClassTab m_finishersTab;

	public CollectionClassTab m_emotesTab;

	public BaconClassFilterButton m_heroSkinsButton;

	public BaconClassFilterButton m_guideSkinsButton;

	public BaconClassFilterButton m_boardSkinsButton;

	public BaconClassFilterButton m_finishersButton;

	public BaconClassFilterButton m_emotesButton;

	public BaconClassFilterHeaderButton m_classFilterHeader;

	private static CollectionUtils.ViewMode[] TAG_ORDERING = new CollectionUtils.ViewMode[5]
	{
		CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS,
		CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS,
		CollectionUtils.ViewMode.BATTLEGROUNDS_BOARD_SKINS,
		CollectionUtils.ViewMode.BATTLEGROUNDS_FINISHERS,
		CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES
	};

	private static readonly int NUM_PAGE_FLIPS_BEFORE_SET_FILTER_TUTORIAL = 3;

	private CollectibleCardBaconHeroesFilter m_baconHeroesCollection = new CollectibleCardBaconHeroesFilter();

	private CollectibleCardBaconGuidesFilter m_baconGuidesCollection = new CollectibleCardBaconGuidesFilter();

	private CollectibleBattlegroundsBoardSet m_baconBoardsCollection = new CollectibleBattlegroundsBoardSet();

	private CollectibleBattlegroundsFinisherSet m_baconFinishersCollection = new CollectibleBattlegroundsFinisherSet();

	private CollectibleBattlegroundsEmoteSet m_baconEmotesCollection = new CollectibleBattlegroundsEmoteSet();

	private int m_numPageFlipsThisSession;

	private bool m_allowHoverHighlight = true;

	private List<Vector3> m_listTabPos = new List<Vector3>();

	private List<Vector3> m_listButtonPos = new List<Vector3>();

	private List<BaconClassFilterButton> m_listButton = new List<BaconClassFilterButton>();

	protected override void Start()
	{
		SetUpBookButtons();
		base.Start();
		NetCache.Get().RegisterScreenCollectionManager(OnNetCacheReady);
	}

	protected override void Awake()
	{
		base.Awake();
		m_baconHeroesCollection.Init(CollectiblePageDisplay.GetMaxCardsPerPage(CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS));
		m_baconGuidesCollection.Init(CollectiblePageDisplay.GetMaxCardsPerPage(CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS));
		m_baconBoardsCollection.AddItemsFromDbf();
		m_baconBoardsCollection.ItemsPerPage = CollectiblePageDisplay.GetMaxCardsPerPage(CollectionUtils.ViewMode.BATTLEGROUNDS_BOARD_SKINS);
		m_baconFinishersCollection.AddItemsFromDbf();
		m_baconFinishersCollection.ItemsPerPage = CollectiblePageDisplay.GetMaxCardsPerPage(CollectionUtils.ViewMode.BATTLEGROUNDS_FINISHERS);
		m_baconEmotesCollection.AddItemsFromDbf();
		m_baconEmotesCollection.ItemsPerPage = CollectiblePageDisplay.GetMaxCardsPerPage(CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES);
		UpdateFilteredHeroes();
		UpdateFilteredGuides();
		UpdateFilteredBoards();
		UpdateFilteredFinishers();
		UpdateFilteredEmotes();
		UpdateTabNewItemCounts();
		NetCache.Get().FavoriteBattlegroundsHeroSkinChanged += OnFavoriteBattlegroundsHeroSkinChanged;
		NetCache.Get().FavoriteBattlegroundsGuideSkinChanged += OnFavoriteBattlegroundsGuideSkinChanged;
		NetCache.Get().FavoriteBattlegroundsBoardSkinChanged += OnFavoriteBattlegroundsBoardSkinChanged;
		NetCache.Get().FavoriteBattlegroundsFinisherChanged += OnFavoriteBattlegroundsFinisherChanged;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (NetCache.Get() != null)
		{
			NetCache.Get().FavoriteBattlegroundsHeroSkinChanged -= OnFavoriteBattlegroundsHeroSkinChanged;
			NetCache.Get().FavoriteBattlegroundsGuideSkinChanged -= OnFavoriteBattlegroundsGuideSkinChanged;
			NetCache.Get().FavoriteBattlegroundsBoardSkinChanged -= OnFavoriteBattlegroundsBoardSkinChanged;
			NetCache.Get().FavoriteBattlegroundsFinisherChanged -= OnFavoriteBattlegroundsFinisherChanged;
			NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		}
	}

	public override bool JumpToPageWithCard(string cardID, TAG_PREMIUM premium, DelOnPageTransitionComplete callback, object callbackData)
	{
		Debug.LogWarning("Attempted to jump to a page with a card in Battlegrounds, which the collection screen does not allow.");
		return false;
	}

	public override void ChangeSearchTextFilter(string newSearchText, DelOnPageTransitionComplete callback, object callbackData, bool transitionPage = true)
	{
		m_baconHeroesCollection.FilterSearchText(newSearchText);
		m_baconGuidesCollection.FilterSearchText(newSearchText);
		m_baconBoardsCollection.SearchString = newSearchText;
		m_baconFinishersCollection.SearchString = newSearchText;
		m_baconEmotesCollection.SearchString = newSearchText;
		CardBackManager.Get().SetSearchText(newSearchText);
		UpdateFilteredHeroes();
		UpdateFilteredGuides();
		UpdateFilteredBoards();
		UpdateFilteredFinishers();
		UpdateFilteredEmotes();
		UpdateTabNewItemCounts();
		if (transitionPage)
		{
			m_currentPageNum = 1;
			TransitionPageWhenReady(PageTransitionType.MANY_PAGE_LEFT, useCurrentPageNum: false, callback, callbackData);
		}
	}

	public override void RemoveSearchTextFilter(DelOnPageTransitionComplete callback, object callbackData, bool transitionPage = true)
	{
		m_baconHeroesCollection.FilterSearchText(null);
		m_baconGuidesCollection.FilterSearchText(null);
		m_baconBoardsCollection.SearchString = null;
		m_baconFinishersCollection.SearchString = null;
		m_baconEmotesCollection.SearchString = null;
		CardBackManager.Get().SetSearchText(null);
		UpdateFilteredHeroes();
		UpdateFilteredGuides();
		UpdateFilteredBoards();
		UpdateFilteredFinishers();
		UpdateFilteredEmotes();
		UpdateTabNewItemCounts();
		if (transitionPage)
		{
			m_currentPageNum = 1;
		}
		base.RemoveSearchTextFilter(callback, callbackData, transitionPage);
	}

	public void UpdateHeroSkinsFilterType(bool transitionPage = true)
	{
		UpdateFilteredHeroes();
		UpdateTabNewItemCounts();
		if (transitionPage)
		{
			m_currentPageNum = 1;
			TransitionPageWhenReady(PageTransitionType.MANY_PAGE_LEFT, useCurrentPageNum: false, null, null);
		}
	}

	public override void NotifyOfCollectionChanged()
	{
	}

	public void EnableEmoteHoverHighlights(bool enable)
	{
		m_allowHoverHighlight = enable;
		string eventName = (m_allowHoverHighlight ? "ENABLE_HOVER_HIGHLIGHT" : "DISABLE_HOVER_HIGHLIGHT");
		(GetCurrentPage() as BaconCollectionPageDisplay).m_EmotesWidget.TriggerEvent(eventName);
	}

	protected override bool CanUserTurnPages()
	{
		if (CraftingManager.GetIsInCraftingMode())
		{
			return false;
		}
		CardBackInfoManager cardBackInfoManager = CardBackInfoManager.Get();
		if (cardBackInfoManager != null && cardBackInfoManager.IsPreviewing)
		{
			return false;
		}
		BaconHeroSkinInfoManager heroSkinInfoManager = BaconHeroSkinInfoManager.Get();
		if (heroSkinInfoManager != null && heroSkinInfoManager.IsShowingPreview)
		{
			return false;
		}
		return base.CanUserTurnPages();
	}

	private BaconCollectionPageDisplay PageAsCollectionPage(BookPageDisplay page)
	{
		BaconCollectionPageDisplay obj = page as BaconCollectionPageDisplay;
		if (obj == null)
		{
			Log.CollectionManager.PrintError("Page in BaconCollectionPageManager is not a BaconCollectionPageDisplay!  This should not happen!");
		}
		return obj;
	}

	protected override bool ShouldShowTab(BookTab tab)
	{
		if (NetCache.Get() == null)
		{
			return true;
		}
		NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (guardianVars == null)
		{
			Log.Net.PrintError("No NetCacheFeatures info in NetCache.");
			return true;
		}
		if (tab == m_boardSkinsTab)
		{
			return guardianVars.BattlegroundsBoardSkinsEnabled;
		}
		if (tab == m_finishersTab)
		{
			return guardianVars.BattlegroundsFinishersEnabled;
		}
		if (tab == m_emotesTab)
		{
			return guardianVars.BattlegroundsEmotesEnabled;
		}
		return true;
	}

	protected override void SetUpBookTabs()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return;
		}
		bool isTouch = UniversalInputManager.Get().IsTouchMode();
		CollectionClassTab[] nonClassTabs = new CollectionClassTab[5] { m_heroSkinsTab, m_guideSkinsTab, m_boardSkinsTab, m_finishersTab, m_emotesTab };
		UIEvent.Handler[] nonClassHandlers = new UIEvent.Handler[5] { OnHeroSkinsTabPressed, OnGuideSkinsTabPressed, OnBoardSkinsTabPressed, OnFinishersTabPressed, OnEmotesTabPressed };
		int handlerIdx = 0;
		CollectionClassTab[] array = nonClassTabs;
		foreach (CollectionClassTab tab in array)
		{
			if (tab != null)
			{
				tab.Init(TAG_CLASS.NEUTRAL);
				tab.AddEventListener(UIEventType.RELEASE, nonClassHandlers[handlerIdx]);
				tab.AddEventListener(UIEventType.ROLLOVER, OnTabOver);
				tab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut);
				tab.AddEventListener(UIEventType.ROLLOVER, base.OnTabOver_Touch);
				tab.AddEventListener(UIEventType.ROLLOUT, base.OnTabOut_Touch);
				tab.SetReceiveReleaseWithoutMouseDown(isTouch);
				m_allTabs.Add(tab);
				m_listTabPos.Add(tab.transform.localPosition);
				m_tabVisibility[tab] = true;
			}
			handlerIdx++;
		}
		PositionBookTabs(animate: false);
		m_initializedTabPositions = true;
	}

	protected override void PositionBookTabs(bool animate)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return;
		}
		int tabCount = m_allTabs.Count();
		int iTabPos = 0;
		for (int i = 0; i < tabCount; i++)
		{
			CollectionClassTab tab = (CollectionClassTab)m_allTabs[i];
			bool showTab = ShouldShowTab(tab);
			tab.SetIsVisible(showTab);
			tab.SetTargetVisibility(showTab);
			m_tabVisibility[tab] = showTab;
			tab.gameObject.SetActive(showTab);
			if (showTab)
			{
				tab.transform.localPosition = m_listTabPos[iTabPos++];
			}
		}
	}

	private bool ShouldShowButton(BaconClassFilterButton button)
	{
		if (NetCache.Get() == null)
		{
			return true;
		}
		NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (guardianVars == null)
		{
			Log.Net.PrintError("No NetCacheFeatures info in NetCache.");
			return true;
		}
		if (button == m_boardSkinsButton)
		{
			return guardianVars.BattlegroundsBoardSkinsEnabled;
		}
		if (button == m_finishersButton)
		{
			return guardianVars.BattlegroundsFinishersEnabled;
		}
		if (button == m_emotesButton)
		{
			return guardianVars.BattlegroundsEmotesEnabled;
		}
		return true;
	}

	private void SetUpBookButtons()
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			return;
		}
		BaconClassFilterButton[] array = new BaconClassFilterButton[5] { m_heroSkinsButton, m_guideSkinsButton, m_boardSkinsButton, m_finishersButton, m_emotesButton };
		foreach (BaconClassFilterButton button in array)
		{
			if (button != null)
			{
				m_listButton.Add(button);
				m_listButtonPos.Add(button.transform.localPosition);
			}
		}
		PositionBookButtons();
	}

	private void PositionBookButtons()
	{
		int buttonCount = m_listButton.Count();
		int iButtonPos = 0;
		for (int i = 0; i < buttonCount; i++)
		{
			BaconClassFilterButton button = m_listButton[i];
			bool showButton = ShouldShowButton(button);
			button.gameObject.SetActive(showButton);
			if (showButton)
			{
				button.transform.localPosition = m_listButtonPos[iButtonPos++];
			}
		}
	}

	private void OnNetCacheReady()
	{
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			PositionBookButtons();
		}
		else
		{
			PositionBookTabs(animate: false);
		}
	}

	public void ShowHeroSkins()
	{
		m_currentPageNum = 1;
		UpdateFilteredHeroes();
		UpdateTabNewItemCounts();
		CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS, triggerResponse: false);
		TransitionPageWhenReady(PageTransitionType.NONE, useCurrentPageNum: false, null, null);
	}

	public void ShowGuideSkins()
	{
		m_currentPageNum = 1;
		UpdateFilteredGuides();
		UpdateTabNewItemCounts();
		CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS, triggerResponse: false);
		TransitionPageWhenReady(PageTransitionType.NONE, useCurrentPageNum: false, null, null);
	}

	public void ShowBoardSkins()
	{
		m_currentPageNum = 1;
		UpdateFilteredBoards();
		UpdateTabNewItemCounts();
		CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.BATTLEGROUNDS_BOARD_SKINS, triggerResponse: false);
		TransitionPageWhenReady(PageTransitionType.NONE, useCurrentPageNum: false, null, null);
	}

	public void ShowFinishers()
	{
		m_currentPageNum = 1;
		UpdateFilteredFinishers();
		UpdateTabNewItemCounts();
		CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.BATTLEGROUNDS_FINISHERS, triggerResponse: false);
		TransitionPageWhenReady(PageTransitionType.NONE, useCurrentPageNum: false, null, null);
	}

	public void ShowEmotes()
	{
		m_currentPageNum = 1;
		UpdateFilteredEmotes();
		UpdateTabNewItemCounts();
		CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES, triggerResponse: false);
		TransitionPageWhenReady(PageTransitionType.NONE, useCurrentPageNum: false, null, null);
	}

	protected override void AssembleEmptyPageUI(BookPageDisplay page)
	{
		base.AssembleEmptyPageUI(page);
		AssembleEmptyPageUI(page as CollectiblePageDisplay, displayNoMatchesText: false);
	}

	protected override void AssembleEmptyPageUI(CollectiblePageDisplay page, bool displayNoMatchesText)
	{
		BaconCollectionPageDisplay cpd = PageAsCollectionPage(page);
		if (cpd == null)
		{
			Log.CollectionManager.PrintError("Page in CollectionPageManager is not a BaconCollectionPageDisplay!  This should not happen!");
			return;
		}
		cpd.ShowNoMatchesFound(displayNoMatchesText, m_baconHeroesCollection.FindCardsResult, showHints: false);
		cpd.SetPageCountText(GameStrings.Get("GLUE_COLLECTION_EMPTY_PAGE"));
	}

	protected override bool AssembleCollectiblePage<TCollectible>(TransitionReadyCallbackData transitionReadyCallbackData, ICollection<TCollectible> collectiblesToDisplay, int totalNumPages)
	{
		bool pageIsEmpty = base.AssembleCollectiblePage(transitionReadyCallbackData, collectiblesToDisplay, totalNumPages);
		CollectionUtils.ViewMode viewMode = CollectionManager.Get().GetCollectibleDisplay().GetViewMode();
		BaconCollectionPageDisplay page = PageAsCollectionPage(transitionReadyCallbackData.m_assembledPage);
		switch (viewMode)
		{
		case CollectionUtils.ViewMode.BATTLEGROUNDS_BOARD_SKINS:
			page.SetBoardSkins();
			break;
		case CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS:
			page.SetGuideSkins();
			break;
		case CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS:
			page.SetHeroSkins();
			break;
		case CollectionUtils.ViewMode.BATTLEGROUNDS_FINISHERS:
			page.SetFinishers();
			break;
		case CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES:
			page.SetEmotes();
			break;
		}
		if (pageIsEmpty)
		{
			return true;
		}
		page.SetPageCountText(GameStrings.Format("GLUE_COLLECTION_PAGE_NUM", m_currentPageNum));
		page.ShowNoMatchesFound(show: false);
		SetHasPreviousAndNextPages(m_currentPageNum > 1, m_currentPageNum < totalNumPages);
		CollectionManager.Get().GetCollectibleDisplay().CollectionPageContentsChanged(collectiblesToDisplay, delegate(List<CollectionCardActors> actorList, List<ICollectible> nonActorCollectibleList, object data)
		{
			page.UpdateCollectionItems(actorList, nonActorCollectibleList, viewMode);
			TransitionPageNextFrame(transitionReadyCallbackData);
		}, null);
		return true;
	}

	protected override void AssemblePage(TransitionReadyCallbackData transitionReadyCallbackData, bool useCurrentPageNum)
	{
		switch (CollectionManager.Get().GetCollectibleDisplay().GetViewMode())
		{
		case CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS:
		{
			List<CollectibleCard> heroesToDisplay = m_baconHeroesCollection.GetPageContents(m_currentPageNum);
			AssembleCollectiblePage(transitionReadyCallbackData, heroesToDisplay, m_baconHeroesCollection.GetTotalNumPages());
			break;
		}
		case CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS:
		{
			List<CollectibleCard> guidesToDisplay = m_baconGuidesCollection.GetPageContents(m_currentPageNum);
			AssembleCollectiblePage(transitionReadyCallbackData, guidesToDisplay, m_baconGuidesCollection.GetTotalNumPages());
			break;
		}
		case CollectionUtils.ViewMode.BATTLEGROUNDS_BOARD_SKINS:
		{
			List<CollectibleBattlegroundsBoard> boardsToDisplay = m_baconBoardsCollection.GetPageContents(m_currentPageNum);
			AssembleCollectiblePage(transitionReadyCallbackData, boardsToDisplay, m_baconBoardsCollection.TotalPages);
			break;
		}
		case CollectionUtils.ViewMode.BATTLEGROUNDS_FINISHERS:
		{
			List<CollectibleBattlegroundsFinisher> finishersToDisplay = m_baconFinishersCollection.GetPageContents(m_currentPageNum);
			AssembleCollectiblePage(transitionReadyCallbackData, finishersToDisplay, m_baconFinishersCollection.TotalPages);
			break;
		}
		case CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES:
		{
			List<CollectibleBattlegroundsEmote> emotesToDisplay = m_baconEmotesCollection.GetPageContents(m_currentPageNum);
			AssembleCollectiblePage(transitionReadyCallbackData, emotesToDisplay, m_baconEmotesCollection.TotalPages);
			break;
		}
		}
	}

	private void UpdateFilteredHeroes()
	{
		m_baconHeroesCollection.UpdateResults();
	}

	private void UpdateFilteredGuides()
	{
		m_baconGuidesCollection.UpdateResults();
	}

	private void UpdateFilteredBoards()
	{
		m_baconBoardsCollection.UpdateFilters();
	}

	private void UpdateFilteredFinishers()
	{
		m_baconFinishersCollection.UpdateFilters();
	}

	private void UpdateFilteredEmotes()
	{
		m_baconEmotesCollection.UpdateFilters();
	}

	protected override void UpdateFilteredCards()
	{
		Debug.LogWarning("BaconCollectionPageManager.UpdateFilteredCards should not be used!");
	}

	public void UpdateTabNewItemCounts()
	{
		if (m_heroSkinsTab != null)
		{
			m_heroSkinsTab.UpdateNewItemCount(CollectionManager.Get().CountNewBattlegroundsHeroSkins());
		}
		if (m_guideSkinsTab != null)
		{
			m_guideSkinsTab.UpdateNewItemCount(CollectionManager.Get().CountNewBattlegroundsGuideSkins());
		}
		if (m_boardSkinsTab != null)
		{
			m_boardSkinsTab.UpdateNewItemCount(CollectionManager.Get().CountNewBattlegroundsBoardSkins());
		}
		if (m_finishersTab != null)
		{
			m_finishersTab.UpdateNewItemCount(CollectionManager.Get().CountNewBattlegroundsFinishers());
		}
		if (m_emotesTab != null)
		{
			m_emotesTab.UpdateNewItemCount(CollectionManager.Get().CountNewBattlegroundsEmotes());
		}
		if (m_heroSkinsButton != null)
		{
			m_heroSkinsButton.UpdateNewItemCount(CollectionManager.Get().CountNewBattlegroundsHeroSkins());
		}
		if (m_guideSkinsButton != null)
		{
			m_guideSkinsButton.UpdateNewItemCount(CollectionManager.Get().CountNewBattlegroundsGuideSkins());
		}
		if (m_boardSkinsButton != null)
		{
			m_boardSkinsButton.UpdateNewItemCount(CollectionManager.Get().CountNewBattlegroundsBoardSkins());
		}
		if (m_finishersButton != null)
		{
			m_finishersButton.UpdateNewItemCount(CollectionManager.Get().CountNewBattlegroundsFinishers());
		}
		if (m_emotesButton != null)
		{
			m_emotesButton.UpdateNewItemCount(CollectionManager.Get().CountNewBattlegroundsEmotes());
		}
	}

	protected override void TransitionPage(object callbackData)
	{
		base.TransitionPage(callbackData);
		SetCurrentModeTab();
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES)
		{
			EnableEmoteHoverHighlights(m_allowHoverHighlight);
		}
	}

	private void SetCurrentModeTab()
	{
		BookTab selectNewTab = null;
		CollectionUtils.ViewMode currentMode = CollectionManager.Get().GetCollectibleDisplay().GetViewMode();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_classFilterHeader.SetMode(currentMode);
			return;
		}
		selectNewTab = currentMode switch
		{
			CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS => m_heroSkinsTab, 
			CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS => m_guideSkinsTab, 
			CollectionUtils.ViewMode.BATTLEGROUNDS_BOARD_SKINS => m_boardSkinsTab, 
			CollectionUtils.ViewMode.BATTLEGROUNDS_FINISHERS => m_finishersTab, 
			CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES => m_emotesTab, 
			_ => null, 
		};
		if (!(selectNewTab == m_currentTab))
		{
			DeselectCurrentTab();
			m_currentTab = selectNewTab;
			if (m_currentTab != null)
			{
				StopCoroutine(CollectiblePageManager.SELECT_TAB_COROUTINE_NAME);
				StartCoroutine(CollectiblePageManager.SELECT_TAB_COROUTINE_NAME, m_currentTab);
			}
		}
	}

	protected override void OnPageTransitionRequested()
	{
		m_numPageFlipsThisSession++;
		int @int = Options.Get().GetInt(Option.PAGE_MOUSE_OVERS);
		int updatedPageFlips = @int + 1;
		if (@int < m_numPlageFlipsBeforeStopShowingArrows)
		{
			Options.Get().SetInt(Option.PAGE_MOUSE_OVERS, updatedPageFlips);
		}
		ShowSetFilterTutorialIfNeeded();
	}

	protected override void OnPageTurnComplete(object callbackData, int operationId)
	{
		if (m_numPageFlipsThisSession % CollectiblePageManager.NUM_PAGE_FLIPS_UNTIL_UNLOAD_UNUSED_ASSETS == 0)
		{
			HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
			if (hearthstoneApplication != null)
			{
				hearthstoneApplication.UnloadUnusedAssets();
			}
		}
		base.OnPageTurnComplete(callbackData, operationId);
	}

	private void ShowSetFilterTutorialIfNeeded()
	{
		if (!Options.Get().GetBool(Option.HAS_SEEN_SET_FILTER_TUTORIAL) && !CollectionManager.Get().IsInEditMode() && CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.CARDS && m_cardsCollection.CardSetFilterIsAllStandardSets())
		{
			CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
			if (!(cmd == null) && !cmd.IsShowingSetFilterTray() && CollectionManager.Get().AccountHasWildCards() && RankMgr.Get().WildCardsAllowedInCurrentLeague() && m_numPageFlipsThisSession >= NUM_PAGE_FLIPS_BEFORE_SET_FILTER_TUTORIAL)
			{
				cmd.ShowSetFilterTutorial(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS);
				Options.Get().SetBool(Option.HAS_SEEN_SET_FILTER_TUTORIAL, val: true);
			}
		}
	}

	protected override void OnCollectionManagerViewModeChanged(CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode mode, CollectionUtils.ViewModeData userdata, bool triggerResponse)
	{
		if (!triggerResponse)
		{
			return;
		}
		Log.CollectionManager.Print("transitionPageId={0} pagesTurning={1} mode={2}-->{3} triggerResponse={4}", m_transitionPageId, m_pagesCurrentlyTurning, prevMode, mode, triggerResponse);
		m_currentPageNum = 1;
		int prevId = 0;
		int newId = 0;
		for (int i = 0; i < TAG_ORDERING.Length; i++)
		{
			if (prevMode == TAG_ORDERING[i])
			{
				prevId = i;
			}
			if (mode == TAG_ORDERING[i])
			{
				newId = i;
			}
		}
		PageTransitionType transition = ((newId - prevId >= 0) ? PageTransitionType.SINGLE_PAGE_RIGHT : PageTransitionType.SINGLE_PAGE_LEFT);
		DelOnPageTransitionComplete callback = null;
		object callbackData = null;
		if (userdata != null)
		{
			callback = userdata.m_pageTransitionCompleteCallback;
			callbackData = userdata.m_pageTransitionCompleteData;
		}
		if (m_turnPageCoroutine != null)
		{
			StopCoroutine(m_turnPageCoroutine);
		}
		m_turnPageCoroutine = StartCoroutine(ViewModeChangedWaitToTurnPage(transition, callback, callbackData));
	}

	private IEnumerator ViewModeChangedWaitToTurnPage(PageTransitionType transition, DelOnPageTransitionComplete callback, object callbackData)
	{
		TransitionPageWhenReady(transition, useCurrentPageNum: true, callback, callbackData);
		yield break;
	}

	public void OnFavoriteBattlegroundsHeroSkinChanged(int baseHeroCardId, int battlegroundsHeroSkinId)
	{
		PageAsCollectionPage(GetCurrentPage()).UpdateFavoriteHeroSkins(CollectionManager.Get().GetCollectibleDisplay().GetViewMode());
	}

	public void OnFavoriteBattlegroundsGuideSkinChanged(BattlegroundsGuideSkinId? newFavoriteBattlegroundsGuideSkin)
	{
		PageAsCollectionPage(GetCurrentPage()).UpdateFavoriteGuideSkins(CollectionManager.Get().GetCollectibleDisplay().GetViewMode());
	}

	public void OnFavoriteBattlegroundsBoardSkinChanged(BattlegroundsBoardSkinId? newFavoriteBattlegroundsBoardSkin)
	{
		PageAsCollectionPage(GetCurrentPage()).UpdateFavoriteBoardSkins(CollectionManager.Get().GetCollectibleDisplay().GetViewMode());
		if (PartyManager.Get().IsInBattlegroundsParty())
		{
			PartyManager.Get().UpdateBattlegroundsBoardSkinMemberAttribute();
		}
	}

	public void OnFavoriteBattlegroundsFinisherChanged(BattlegroundsFinisherId? newFavoriteBattlegroundsFinisher)
	{
		PageAsCollectionPage(GetCurrentPage()).UpdateFavoriteFinisherSkins(CollectionManager.Get().GetCollectibleDisplay().GetViewMode());
		if (PartyManager.Get().IsInBattlegroundsParty())
		{
			PartyManager.Get().UpdateBattlegroundsStrikeMemberAttribute();
		}
	}

	public void SetEmoteEquippedState(BattlegroundsEmoteId emoteId, bool isEquipped)
	{
		PageAsCollectionPage(GetCurrentPage()).SetEmoteEquippedState(emoteId, isEquipped);
	}

	private void OnHeroSkinsTabPressed(UIEvent e)
	{
		if (CanUserTurnPages())
		{
			CollectionClassTab classTab = e.GetElement() as CollectionClassTab;
			if (!(classTab == null) && !(classTab == m_currentTab) && ShouldShowTab(m_heroSkinsTab))
			{
				UpdateFilteredHeroes();
				CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS);
				UpdateTabNewItemCounts();
			}
		}
	}

	private void OnGuideSkinsTabPressed(UIEvent e)
	{
		if (CanUserTurnPages())
		{
			CollectionClassTab classTab = e.GetElement() as CollectionClassTab;
			if (!(classTab == null) && !(classTab == m_currentTab) && ShouldShowTab(m_guideSkinsTab))
			{
				UpdateFilteredGuides();
				UpdateTabNewItemCounts();
				CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS);
			}
		}
	}

	private void OnBoardSkinsTabPressed(UIEvent e)
	{
		if (CanUserTurnPages())
		{
			CollectionClassTab classTab = e.GetElement() as CollectionClassTab;
			if (!(classTab == null) && !(classTab == m_currentTab) && ShouldShowTab(m_boardSkinsTab))
			{
				UpdateFilteredBoards();
				UpdateTabNewItemCounts();
				CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.BATTLEGROUNDS_BOARD_SKINS);
			}
		}
	}

	private void OnFinishersTabPressed(UIEvent e)
	{
		if (CanUserTurnPages())
		{
			CollectionClassTab classTab = e.GetElement() as CollectionClassTab;
			if (!(classTab == null) && !(classTab == m_currentTab) && ShouldShowTab(m_finishersTab))
			{
				UpdateFilteredFinishers();
				UpdateTabNewItemCounts();
				CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.BATTLEGROUNDS_FINISHERS);
			}
		}
	}

	private void OnEmotesTabPressed(UIEvent e)
	{
		if (CanUserTurnPages())
		{
			CollectionClassTab classTab = e.GetElement() as CollectionClassTab;
			if (!(classTab == null) && !(classTab == m_currentTab) && ShouldShowTab(m_emotesTab))
			{
				UpdateFilteredEmotes();
				UpdateTabNewItemCounts();
				CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES);
			}
		}
	}

	private HashSet<int> GetCurrentDeckTrayModeCardBackIds()
	{
		return CardBackManager.Get().GetCardBackIds(!CollectionManager.Get().IsInEditMode());
	}
}
