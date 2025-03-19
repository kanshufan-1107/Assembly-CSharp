using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class BaconCollectionDisplay : CollectibleDisplay
{
	[CustomEditField(Sections = "Bones")]
	public Transform m_setFilterTutorialBone;

	[CustomEditField(Sections = "Objects")]
	public BaconCollectionPageManager m_pageManager;

	[CustomEditField(Sections = "Objects")]
	public NestedPrefab m_setFilterTrayContainer;

	[CustomEditField(Sections = "Objects")]
	public BaconCollectionFilterButton m_baconFilterButton;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_boardDetailsDisplayReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_boardDetailsRenderedReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_finisherDetailsDisplayReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_finisherDetailsRenderedReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_emoteDetailsDisplayReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_emoteLayoutDisplayReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_emoteTrayReference;

	[CustomEditField(Sections = "Widgets")]
	public AsyncReference m_emotePreviewButtonReference;

	private Notification m_innkeeperLClickReminder;

	private List<FilterStateListener> m_setFilterListeners = new List<FilterStateListener>();

	private List<FilterStateListener> m_manaFilterListeners = new List<FilterStateListener>();

	private ShareableDeck m_cachedShareableDeck;

	private CollectionUtils.BattlegroundsHeroSkinFilterMode m_heroSkinFilterMode;

	private bool m_searchTriggeredHeroSkinFilter;

	private bool m_boardDetailsDisplayFinishedLoading;

	private BaconBoardCollectionDetails m_boardDetailsDisplay;

	private bool m_boardDetailsRenderedFinishedLoading;

	private BaconCosmeticPreviewTester m_boardDetailsRendered;

	private bool m_finisherDetailsDisplayFinishedLoading;

	private BaconFinisherCollectionDetails m_finisherDetailsDisplay;

	private bool m_finisherDetailsRenderedFinishedLoading;

	private BaconCosmeticPreviewTester m_finisherDetailsRendered;

	private bool m_emoteDetailsDisplayFinishedLoading;

	private BaconEmoteCollectionDetails m_emoteDetailsDisplay;

	private bool m_emoteLayoutDisplayFinishedLoading;

	private BaconEmoteCollectionLayout m_emoteLayoutDisplay;

	private bool m_emoteTrayFinishedLoading;

	private BaconEmoteTray m_emoteTray;

	private UIBButton m_emoteLayoutDisplayButton;

	public override void Start()
	{
		NetCache.Get().RegisterScreenCollectionManager(OnNetCacheReady);
		CollectionManager.Get().RegisterCollectionNetHandlers();
		CollectionManager.Get().RegisterCollectionLoadedListener(base.OnCollectionLoaded);
		CollectionManager.Get().RegisterCollectionChangedListener(OnCollectionChanged);
		CollectionManager.Get().RegisterCardRewardsInsertedListener(OnCardRewardsInserted);
		CollectionManager.Get().RegisterNewCardSeenListener(OnNewCardSeen);
		CardBackManager.Get().SetSearchText(null);
		Navigation.Push(OnBackOutOfCollectionScreen);
		m_boardDetailsDisplayReference.RegisterReadyListener<VisualController>(OnBoardDetailsDisplayReady);
		m_boardDetailsRenderedReference.RegisterReadyListener<BaconCosmeticPreviewTester>(OnBoardDetailsRenderedReady);
		m_finisherDetailsDisplayReference.RegisterReadyListener<VisualController>(OnFinisherDetailsDisplayReady);
		m_finisherDetailsRenderedReference.RegisterReadyListener<BaconCosmeticPreviewTester>(OnFinisherDetailsRenderedReady);
		m_emoteDetailsDisplayReference.RegisterReadyListener<VisualController>(OnEmoteDetailsDisplayReady);
		m_emoteLayoutDisplayReference.RegisterReadyListener<VisualController>(OnEmoteLayoutDisplayReady);
		m_emoteTrayReference.RegisterReadyListener<VisualController>(OnEmoteTrayReady);
		m_emotePreviewButtonReference.RegisterReadyListener<UIBButton>(OnEmotePreviewButtonReady);
		base.Start();
		m_pageManager.ShowHeroSkins();
		DoEnterCollectionManagerEvents();
		MusicManager.Get().StartPlaylist(MusicPlaylistType.CollectionManager_Battlegrounds);
		CollectionManager.Get().RequestDeckContentsForDecksWithoutContentsLoaded();
		StartCoroutine(WaitUntilReady());
	}

	public CollectionUtils.BattlegroundsHeroSkinFilterMode GetHeroSkinFilterMode()
	{
		return m_heroSkinFilterMode;
	}

	public void ToggleHeroSkinFilterMode()
	{
		int filterModeAsInt = (int)m_heroSkinFilterMode;
		filterModeAsInt++;
		if (filterModeAsInt >= 2)
		{
			m_heroSkinFilterMode = CollectionUtils.BattlegroundsHeroSkinFilterMode.DEFAULT;
		}
		else
		{
			m_heroSkinFilterMode = (CollectionUtils.BattlegroundsHeroSkinFilterMode)filterModeAsInt;
		}
		m_pageManager.UpdateHeroSkinsFilterType();
	}

	public bool TryCheckEmoteInLoadout(int emoteId, out bool inLoadout)
	{
		if (m_emoteTray == null || !m_emoteTray.IsLoadoutValid())
		{
			inLoadout = false;
			return false;
		}
		inLoadout = m_emoteTray.IsEmoteInLoadout(emoteId);
		return true;
	}

	protected override void Awake()
	{
		HearthstonePerformance.Get()?.StartPerformanceFlow(new FlowPerformance.SetupConfig
		{
			FlowType = Blizzard.Telemetry.WTCG.Client.FlowPerformance.FlowType.COLLECTION_MANAGER
		});
		base.Awake();
		StartCoroutine(InitCollectionWhenReady());
	}

	private BattlegroundsEmoteLoadoutDataModel GetOrCreateEmoteLoadoutDataModel()
	{
		BattlegroundsEmoteLoadoutDataModel dataModel = m_emoteTray.GetLoadoutDataModel();
		if (dataModel == null)
		{
			dataModel = CollectionManager.Get().CreateEmoteLoadoutDataModel();
		}
		return dataModel;
	}

	public void SetEmoteLoadout(BattlegroundsEmoteLoadoutDataModel dataModel)
	{
		m_emoteTray.SetLoadoutDataModel(dataModel);
		m_emoteTray.UpdateImageWidgetVisibility(dataModel);
	}

	protected override void OnDestroy()
	{
		UserAttentionManager.StopBlocking(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS);
		base.OnDestroy();
	}

	public override CollectiblePageManager GetPageManager()
	{
		return m_pageManager;
	}

	public override void Unload()
	{
		m_unloading = true;
		NotificationManager.Get().DestroyAllPopUps();
		UnloadAllTextures();
		CollectionInputMgr.Get().Unload();
		if (m_boardDetailsDisplay != null)
		{
			m_boardDetailsDisplay.Unload();
		}
		if (m_finisherDetailsDisplay != null)
		{
			m_finisherDetailsDisplay.Unload();
		}
		if (m_emoteDetailsDisplay != null)
		{
			m_emoteDetailsDisplay.Unload();
		}
		if (m_emoteLayoutDisplay != null)
		{
			m_emoteLayoutDisplay.Unload();
		}
		if (m_emoteTray != null)
		{
			m_emoteTray.Unload();
		}
		CollectionManager.Get().RemoveCollectionLoadedListener(base.OnCollectionLoaded);
		CollectionManager.Get().RemoveCollectionChangedListener(OnCollectionChanged);
		CollectionManager.Get().RemoveCardRewardsInsertedListener(OnCardRewardsInserted);
		CollectionManager.Get().RemoveCollectionNetHandlers();
		CollectionManager.Get().RemoveNewCardSeenListener(OnNewCardSeen);
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		m_unloading = false;
	}

	public override void Exit()
	{
		EnableInput(enable: false);
		NotificationManager.Get().DestroyAllPopUps();
		if (m_pageManager != null)
		{
			m_pageManager.Exit();
		}
		SceneMgr.Mode nextMode = SceneMgr.Get().GetPrevMode();
		HearthstonePerformance.Get()?.StopCurrentFlow();
		SceneMgr.Get().SetNextMode(nextMode);
		BaconTelemetry.SendBattlegroundsCollectionResultExitCollection();
	}

	public override void CollectionPageContentsChanged<TCollectible>(ICollection<TCollectible> collectiblesToDisplay, CollectionActorsReadyCallback callback, object callbackData)
	{
		Log.CollectionManager.Print("transitionPageId={0} pagesTurning={1}", m_pageManager.GetTransitionPageId(), m_pageManager.ArePagesTurning());
		bool displayEmptyPage = false;
		if (collectiblesToDisplay == null)
		{
			Log.CollectionManager.Print("artStacksToDisplay is null!");
			displayEmptyPage = true;
		}
		else if (collectiblesToDisplay.Count == 0)
		{
			Log.CollectionManager.Print("artStacksToDisplay has a count of 0!");
			displayEmptyPage = true;
		}
		if (displayEmptyPage)
		{
			callback?.Invoke(new List<CollectionCardActors>(), new List<ICollectible>(), callbackData);
		}
		else
		{
			if (m_unloading)
			{
				return;
			}
			foreach (CollectionCardActors previousCardActor in m_previousCardActors)
			{
				previousCardActor.Destroy();
			}
			m_previousCardActors.Clear();
			m_previousCardActors = m_cardActors;
			m_cardActors = new List<CollectionCardActors>();
			bool playerHasEarlyAccessHeroes = RewardTrackManager.Get().HasBattlegroundsPreviewHeroes();
			List<ICollectible> nonActorCollectibles = new List<ICollectible>();
			foreach (TCollectible item in collectiblesToDisplay)
			{
				ICollectible collectible = item;
				if (!(collectible is CollectibleCard card))
				{
					nonActorCollectibles.Add(collectible);
					continue;
				}
				EntityDef entityDef = DefLoader.Get().GetEntityDef(card.CardId);
				using DefLoader.DisposableCardDef cardDef = DefLoader.Get().GetCardDef(card.CardId, card.PremiumType);
				string actorPath = ((m_currentViewMode == CollectionUtils.ViewMode.BATTLEGROUNDS_GUIDE_SKINS) ? "Card_Guide_Skin.prefab:cf2cadaa8c6f7244fb9500edb2046c8b" : "Card_Bacon_Hero_Skin.prefab:7b4af2ee64cfdf24e8ebc8fc817b9761");
				GameObject newActorObj = AssetLoader.Get().InstantiatePrefab(actorPath, AssetLoadingOptions.IgnorePrefabPosition);
				if (newActorObj == null)
				{
					Debug.LogError("Unable to load card actor.");
					continue;
				}
				Actor newActor = newActorObj.GetComponent<Actor>();
				if (newActor == null)
				{
					Debug.LogError("Actor object does not contain Actor component.");
					continue;
				}
				newActor.SetEntityDef(entityDef);
				newActor.SetCardDef(cardDef);
				newActor.SetPremium(card.PremiumType);
				newActor.CreateBannedRibbon();
				BaconCollectionHeroSkin heroSkin = newActorObj.GetComponent<BaconCollectionHeroSkin>();
				if (heroSkin != null)
				{
					heroSkin.SetCardStateDisplay(card, entityDef, playerHasEarlyAccessHeroes);
				}
				BaconCollectionGuideSkin guideSkin = newActorObj.GetComponent<BaconCollectionGuideSkin>();
				if (guideSkin != null)
				{
					guideSkin.SetCardStateDisplay(card);
				}
				newActor.UpdateAllComponents();
				m_cardActors.Add(new CollectionCardActors(newActor));
			}
			callback?.Invoke(m_cardActors, nonActorCollectibles, callbackData);
		}
	}

	private bool OnBackOutOfCollectionScreen()
	{
		if (this == null || base.gameObject == null)
		{
			return true;
		}
		Exit();
		return true;
	}

	public override void SetViewMode(CollectionUtils.ViewMode mode, bool triggerResponse, CollectionUtils.ViewModeData userdata = null)
	{
		Log.CollectionManager.Print("mode={0}-->{1} triggerResponse={2}", m_currentViewMode, mode, triggerResponse);
		if (m_currentViewMode != mode)
		{
			CollectionUtils.ViewMode prevMode = m_currentViewMode;
			m_currentViewMode = mode;
			OnSwitchViewModeResponse(triggerResponse, prevMode, mode, userdata);
			m_baconFilterButton.SetActive(mode == CollectionUtils.ViewMode.BATTLEGROUNDS_HERO_SKINS);
			if (mode == CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES)
			{
				ShowEmoteTray();
				ToggleLayoutButton(toggle: true);
			}
			else if (prevMode == CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES)
			{
				HideEmoteTray();
				ToggleLayoutButton(toggle: false);
			}
		}
	}

	private void ToggleLayoutButton(bool toggle)
	{
		if (m_emoteLayoutDisplayButton != null)
		{
			m_emoteLayoutDisplayButton.SetEnabled(toggle);
			m_emoteLayoutDisplayButton.Flip(!toggle);
		}
	}

	public override void FilterBySearchText(string newSearchText)
	{
		string oldSearchText = m_search.GetText();
		base.FilterBySearchText(newSearchText);
		OnSearchDeactivated_Internal(oldSearchText, newSearchText);
	}

	public override void HideAllTips()
	{
		if (m_innkeeperLClickReminder != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_innkeeperLClickReminder);
		}
	}

	public override void ShowInnkeeperLClickHelp(EntityDef entityDef)
	{
		bool isHeroSkin = entityDef?.IsHeroSkin() ?? false;
		ShowInnkeeperLClickHelp(isHeroSkin);
	}

	private void ShowInnkeeperLClickHelp(bool isHero)
	{
		if (isHero)
		{
			m_innkeeperLClickReminder = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_CM_LCLICK_HERO"), "", 3f);
		}
		else
		{
			m_innkeeperLClickReminder = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_CM_LCLICK"), "", 3f);
		}
	}

	public override void SetFilterCallback(List<TAG_CARD_SET> cardSets, List<int> specificCards, PegasusShared.FormatType formatType, SetFilterItem item, bool transitionPage)
	{
		ShowSetFilterCards(cardSets, specificCards, transitionPage);
	}

	private void ShowSetFilterCards(List<TAG_CARD_SET> cardSets, List<int> specificCards, bool transitionPage = true)
	{
		if (specificCards != null)
		{
			ShowSpecificCards(specificCards);
		}
		else
		{
			ShowSets(cardSets, transitionPage);
		}
	}

	private void ShowSets(List<TAG_CARD_SET> cardSets, bool transitionPage = true)
	{
		m_pageManager.FilterByCardSets(cardSets, transitionPage);
		NotifyFilterUpdate(m_setFilterListeners, cardSets != null, null);
	}

	protected override void ShowSpecificCards(List<int> specificCards)
	{
		base.ShowSpecificCards(specificCards);
		NotifyFilterUpdate(m_setFilterListeners, specificCards != null, null);
	}

	public override void ResetFilters(bool updateVisuals = true)
	{
		base.ResetFilters(updateVisuals);
		if (m_setFilterTray != null)
		{
			m_setFilterTray.ClearFilter();
		}
	}

	public void ShowBoardDetailsDisplay(BattlegroundsBoardSkinDataModel dataModel, BattlegroundsBoardSkinCollectionPageDataModel pageModel)
	{
		if (!(m_boardDetailsDisplay == null) && m_boardDetailsDisplay.CanShow(dataModel, pageModel))
		{
			m_boardDetailsDisplay.AssignDataModels(dataModel, pageModel);
			m_boardDetailsDisplay.Show();
		}
	}

	public void ShowBoardDetailsRendered(BattlegroundsBoardSkinDataModel dataModel, BattlegroundsBoardSkinCollectionPageDataModel pageModel)
	{
		if (!(m_boardDetailsRendered == null))
		{
			m_boardDetailsRendered.gameObject.SetActive(value: true);
			m_boardDetailsRendered.GetComponent<BaconBoardCollectionDetails>().AssignDataModels(dataModel, pageModel);
			m_boardDetailsRendered.GetComponent<BaconBoardCollectionDetails>().Show();
			m_boardDetailsRendered.GetComponent<Widget>().BindDataModel(dataModel);
			m_boardDetailsRendered.GetComponent<Widget>().TriggerEvent("RunCosmetic");
		}
	}

	public void ShowFinisherDetailsDisplay(BattlegroundsFinisherDataModel dataModel, BattlegroundsFinisherCollectionPageDataModel pageModel)
	{
		if (!(m_finisherDetailsDisplay == null) && m_finisherDetailsDisplay.CanShow(dataModel, pageModel))
		{
			m_finisherDetailsDisplay.AssignDataModels(dataModel, pageModel);
			m_finisherDetailsDisplay.Show();
		}
	}

	public void ShowFinisherDetailsRendered(BattlegroundsFinisherDataModel dataModel, BattlegroundsFinisherCollectionPageDataModel pageModel)
	{
		if (!(m_finisherDetailsRendered == null))
		{
			m_finisherDetailsRendered.gameObject.SetActive(value: true);
			m_finisherDetailsRendered.GetComponent<BaconFinisherCollectionDetails>().AssignDataModels(dataModel, pageModel);
			m_finisherDetailsRendered.GetComponent<BaconFinisherCollectionDetails>().Show();
			m_finisherDetailsRendered.GetComponent<Widget>().BindDataModel(dataModel);
			m_finisherDetailsRendered.GetComponent<Widget>().TriggerEvent("RunCosmetic");
		}
	}

	public void ShowEmoteDetailsDisplay(BattlegroundsEmoteDataModel dataModel, BattlegroundsEmoteCollectionPageDataModel pageModel)
	{
		if (!(m_emoteDetailsDisplay == null) && m_emoteDetailsDisplay.CanShow(dataModel, pageModel))
		{
			m_emoteDetailsDisplay.AssignDataModels(dataModel, pageModel);
			m_emoteDetailsDisplay.Show();
		}
	}

	public bool IsEmoteDetailsShowing()
	{
		return m_emoteDetailsDisplay.isActiveAndEnabled;
	}

	public void ShowEmoteLayoutDisplay()
	{
		if (!(m_emoteLayoutDisplay == null) && m_currentViewMode == CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES)
		{
			BattlegroundsEmoteLoadoutDataModel loadoutDataModel = GetOrCreateEmoteLoadoutDataModel();
			m_emoteLayoutDisplay.Show(loadoutDataModel, m_emoteTray);
		}
	}

	public void ShowEmoteTray()
	{
		if (!(m_emoteTray == null))
		{
			BattlegroundsEmoteLoadoutDataModel loadoutDataModel = GetOrCreateEmoteLoadoutDataModel();
			m_emoteTray.Show(loadoutDataModel);
		}
	}

	public void HideEmoteTray()
	{
		if (!(m_emoteTray == null))
		{
			m_emoteTray.Hide();
		}
	}

	private void OnCardRewardsInserted(List<string> cardID, List<TAG_PREMIUM> premium)
	{
		m_pageManager.RefreshCurrentPageContents();
	}

	private void OnNewCardSeen(string cardID, TAG_PREMIUM premium)
	{
		if (m_pageManager != null)
		{
			m_pageManager.UpdateTabNewItemCounts();
		}
	}

	protected override void OnCollectionChanged()
	{
		if (m_currentViewMode != CollectionUtils.ViewMode.MASS_DISENCHANT)
		{
			m_pageManager.NotifyOfCollectionChanged();
		}
		if (m_pageManager != null)
		{
			m_pageManager.UpdateTabNewItemCounts();
		}
	}

	private IEnumerator WaitUntilReady()
	{
		while (!m_netCacheReady && Network.IsLoggedIn())
		{
			yield return null;
		}
		while (!m_boardDetailsDisplayFinishedLoading)
		{
			yield return null;
		}
		while (!m_boardDetailsRenderedFinishedLoading)
		{
			yield return null;
		}
		while (!m_finisherDetailsDisplayFinishedLoading)
		{
			yield return null;
		}
		while (!m_finisherDetailsRenderedFinishedLoading)
		{
			yield return null;
		}
		while (!m_emoteDetailsDisplayFinishedLoading)
		{
			yield return null;
		}
		while (!m_emoteLayoutDisplayFinishedLoading)
		{
			yield return null;
		}
		while (!m_emoteTrayFinishedLoading)
		{
			yield return null;
		}
		m_isReady = true;
	}

	private IEnumerator InitCollectionWhenReady()
	{
		while (!m_pageManager.IsFullyLoaded())
		{
			yield return null;
		}
		m_pageManager.ShowHeroSkins();
	}

	private void OnNetCacheReady()
	{
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Collection.Manager)
		{
			if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
				Error.AddWarningLoc("GLOBAL_FEATURE_DISABLED_TITLE", "GLOBAL_FEATURE_DISABLED_MESSAGE_COLLECTION");
			}
		}
		else
		{
			m_netCacheReady = true;
		}
	}

	private void OnBoardDetailsDisplayReady(VisualController vc)
	{
		m_boardDetailsDisplay = vc.GetComponentInChildren<BaconBoardCollectionDetails>();
		m_boardDetailsDisplayFinishedLoading = true;
	}

	private void OnBoardDetailsRenderedReady(BaconCosmeticPreviewTester display)
	{
		m_boardDetailsRendered = display;
		m_boardDetailsRenderedFinishedLoading = true;
	}

	private void OnFinisherDetailsDisplayReady(VisualController vc)
	{
		m_finisherDetailsDisplay = vc.GetComponentInChildren<BaconFinisherCollectionDetails>();
		m_finisherDetailsDisplayFinishedLoading = true;
	}

	private void OnFinisherDetailsRenderedReady(BaconCosmeticPreviewTester display)
	{
		m_finisherDetailsRendered = display;
		m_finisherDetailsRenderedFinishedLoading = true;
	}

	private void OnEmoteDetailsDisplayReady(VisualController vc)
	{
		m_emoteDetailsDisplay = vc.GetComponentInChildren<BaconEmoteCollectionDetails>();
		m_emoteDetailsDisplayFinishedLoading = true;
	}

	private void OnEmoteLayoutDisplayReady(VisualController vc)
	{
		m_emoteLayoutDisplay = vc.GetComponentInChildren<BaconEmoteCollectionLayout>();
		m_emoteLayoutDisplayFinishedLoading = true;
	}

	private void OnEmoteTrayReady(VisualController vc)
	{
		m_emoteTray = vc.GetComponentInChildren<BaconEmoteTray>();
		m_emoteTrayFinishedLoading = true;
	}

	private void OnEmotePreviewButtonReady(UIBButton button)
	{
		button.AddEventListener(UIEventType.RELEASE, delegate
		{
			ShowEmoteLayoutDisplay();
		});
		m_emoteLayoutDisplayButton = button;
		ToggleLayoutButton(m_currentViewMode == CollectionUtils.ViewMode.BATTLEGROUNDS_EMOTES);
	}

	protected override void LoadAllTextures()
	{
	}

	protected override void UnloadAllTextures()
	{
	}

	private IEnumerator DoBookOpeningAnimations()
	{
		while (m_isBookCoverLoading)
		{
			yield return null;
		}
		if (m_cover != null)
		{
			m_cover.Open(base.OnCoverOpened);
		}
		else
		{
			OnCoverOpened();
		}
	}

	private IEnumerator SetBookToOpen()
	{
		while (m_isBookCoverLoading)
		{
			yield return null;
		}
		if (m_cover != null)
		{
			m_cover.SetOpenState();
		}
	}

	protected override void OnSearchDeactivated(string oldSearchText, string newSearchText)
	{
		OnSearchDeactivated_Internal(oldSearchText, newSearchText);
	}

	private void OnSearchDeactivated_Internal(string oldSearchText, string newSearchText)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			EnableInput(enable: true);
		}
		if (oldSearchText == newSearchText)
		{
			OnSearchFilterComplete();
			return;
		}
		string[] source = newSearchText.ToLower().Split(CollectibleFilteredSet<ICollectible>.SearchTokenDelimiters, StringSplitOptions.RemoveEmptyEntries);
		string missingString = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MISSING");
		if (source.Contains(missingString))
		{
			m_heroSkinFilterMode = CollectionUtils.BattlegroundsHeroSkinFilterMode.ALL;
			m_pageManager.UpdateHeroSkinsFilterType(transitionPage: false);
			m_baconFilterButton.FilterUpdated();
			m_searchTriggeredHeroSkinFilter = true;
		}
		else
		{
			ResetFilterSettingsFromSearch();
		}
		NotifyFilterUpdate(m_searchFilterListeners, !string.IsNullOrEmpty(newSearchText), newSearchText);
		m_pageManager.ChangeSearchTextFilter(newSearchText, base.OnSearchFilterComplete, null);
	}

	protected override void OnSearchCleared(bool transitionPage)
	{
		ResetFilterSettingsFromSearch();
		NotifyFilterUpdate(m_searchFilterListeners, active: false, "");
		m_pageManager.ChangeSearchTextFilter("", transitionPage);
		base.OnSearchCleared(transitionPage);
	}

	private void ResetFilterSettingsFromSearch()
	{
		if (m_searchTriggeredHeroSkinFilter)
		{
			m_heroSkinFilterMode = CollectionUtils.BattlegroundsHeroSkinFilterMode.DEFAULT;
			m_pageManager.UpdateHeroSkinsFilterType(transitionPage: false);
			m_baconFilterButton.FilterUpdated();
		}
		m_searchTriggeredHeroSkinFilter = false;
	}

	private void DoEnterCollectionManagerEvents()
	{
		if (CollectionManager.Get().HasVisitedCollection())
		{
			EnableInput(enable: true);
			OpenBookImmediately();
		}
		else
		{
			CollectionManager.Get().SetHasVisitedCollection(enable: true);
			EnableInput(enable: false);
			StartCoroutine(OpenBookWhenReady());
		}
	}

	private void OpenBookImmediately()
	{
		SceneMgr.Get().GetMode();
		_ = 18;
		StartCoroutine(SetBookToOpen());
	}

	private IEnumerator OpenBookWhenReady()
	{
		while (CollectionManager.Get().IsWaitingForBoxTransition())
		{
			yield return null;
		}
		SceneMgr.Get().GetMode();
		m_pageManager.OnBookOpening();
		StartCoroutine(DoBookOpeningAnimations());
	}
}
