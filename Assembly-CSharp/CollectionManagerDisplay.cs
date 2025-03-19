using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone;
using Hearthstone.Util;
using PegasusShared;
using UnityEngine;
using UnityEngine.Serialization;

[CustomEditClass]
public class CollectionManagerDisplay : CollectibleDisplay
{
	[CustomEditField(Sections = "Bones")]
	public Transform m_deckTemplateHiddenBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_deckTemplateShownBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_deckTemplateTutorialWelcomeBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_deckTemplateTutorialReminderBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_editDeckTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_convertDeckTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_setFilterTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_cardBackDeckTrayTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_multipleFavoriteCardBackTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_multipleFavoriteHeroTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_touristCollectionTutorialBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_touristClassTabTutorialBone;

	[CustomEditField(Sections = "Objects")]
	public CollectionPageManager m_pageManager;

	[CustomEditField(Sections = "Objects")]
	public ManaFilterTabManager m_manaTabManager;

	[CustomEditField(Sections = "Objects")]
	public Notification m_deckTemplateCardReplacePopup;

	[CustomEditField(Sections = "Objects")]
	public NestedPrefab m_setFilterTrayContainer;

	[CustomEditField(Sections = "Objects")]
	public GameObject m_runeLockedCheckboxContainer;

	[CustomEditField(Sections = "Objects")]
	public CheckBox m_runelockedCheckbox;

	[CustomEditField(Sections = "Objects")]
	public UberText m_runeLockedCheckboxLabel;

	[CustomEditField(Sections = "Controls")]
	public Texture m_allSetsTexture;

	[CustomEditField(Sections = "Controls")]
	public UnityEngine.Vector2 m_allSetsIconOffset;

	[CustomEditField(Sections = "Controls")]
	public Texture m_wildSetsTexture;

	[CustomEditField(Sections = "Controls")]
	public UnityEngine.Vector2 m_wildSetsIconOffset;

	[CustomEditField(Sections = "Controls")]
	public Texture m_featuredCardsTexture;

	[CustomEditField(Sections = "Controls")]
	public UnityEngine.Vector2 m_featuredCardsIconOffset;

	[CustomEditField(Sections = "Controls")]
	public Texture m_classicSetsTexture;

	[CustomEditField(Sections = "Controls")]
	public UnityEngine.Vector2 m_classicSetsIconOffset;

	[CustomEditField(Sections = "Controls")]
	public Texture m_twistSetsTexture;

	[CustomEditField(Sections = "Controls")]
	public UnityEngine.Vector2 m_twistSetsIconOffset;

	[CustomEditField(Sections = "CM Customization Ref")]
	public GameObject m_bookBack;

	[FormerlySerializedAs("m_tavernBrawlBookBackMesh")]
	[CustomEditField(Sections = "CM Customization Ref")]
	public Mesh m_customBookBackMesh;

	[CustomEditField(Sections = "CM Customization Ref")]
	[FormerlySerializedAs("m_tavernBrawlObjectsToSwap")]
	public List<GameObject> m_customObjectsToSwap = new List<GameObject>();

	[CustomEditField(Sections = "Tavern Brawl Changes", T = EditType.TEXTURE)]
	[FormerlySerializedAs("m_corkBackTexture")]
	public string m_tbCorkBackTexture;

	[CustomEditField(Sections = "Tavern Brawl Changes")]
	public Material m_tavernBrawlElements;

	[CustomEditField(Sections = "Duels Changes", T = EditType.TEXTURE)]
	public string m_duelsCorkBackTexture;

	[CustomEditField(Sections = "Duels Changes")]
	public Material m_duelsElements;

	[CustomEditField(Sections = "Prefabs", T = EditType.GAME_OBJECT)]
	public string m_collectionSpinnerPrefab;

	private Map<TAG_CLASS, AssetHandle<Texture>> m_loadedClassTextures = new Map<TAG_CLASS, AssetHandle<Texture>>();

	private AssetHandle<Texture> m_loadedCorkBackTexture;

	private bool m_selectingNewDeckHero;

	private long m_showDeckContentsRequest;

	private bool m_shouldShowMultipleFavoriteCardBackTutorial;

	private bool m_shouldShowMultipleFavoriteHeroTutorial;

	private Notification m_deckHelpPopup;

	private Notification m_innkeeperLClickReminder;

	private List<FilterStateListener> m_setFilterListeners = new List<FilterStateListener>();

	private List<FilterStateListener> m_manaFilterListeners = new List<FilterStateListener>();

	private DeckTemplatePicker m_deckTemplatePickerPhone;

	private HeroPickerDisplay m_heroPickerDisplay;

	private Notification m_createDeckNotification;

	private Notification m_multipleFavoriteCardBacksNotification;

	private Notification m_multipleFavoriteHeroesNotification;

	private Notification m_convertTutorialPopup;

	private IEnumerator m_showConvertTutorialCoroutine;

	private Notification m_setFilterTutorialPopup;

	private IEnumerator m_showSetFilterTutorialCoroutine;

	private bool m_showingDeckTemplateTips;

	private float m_deckTemplateTipWaitTime;

	private bool m_manaFilterIsFromSearchText;

	private ShareableDeck m_cachedShareableDeck;

	private EventTimingType m_currentActiveFeaturedCardsEvent;

	protected bool m_viewModeHidingCraftingTray;

	private TAG_CLASS? m_heroSkinClass;

	private static readonly string PASTE_DECK_POPUP_ID = "PASTE_DECK_POPUP";

	private CollectionManagerSpinnerPopup m_collectionSpinner;

	public PegasusShared.FormatType CurrentSetFilterFormatType { get; set; }

	public bool IsManaFilterEvenValues => m_manaTabManager.IsFilterEvenValues;

	public bool IsManaFilterOddValues => m_manaTabManager.IsFilterOddValues;

	public static event Action<bool> HideLockedRunesCheckboxToggled;

	public override void Start()
	{
		NetCache.Get().RegisterScreenCollectionManager(OnNetCacheReady);
		CollectionManager.Get().RegisterCollectionNetHandlers();
		CollectionManager.Get().RegisterCollectionLoadedListener(base.OnCollectionLoaded);
		CollectionManager.Get().RegisterCollectionChangedListener(OnCollectionChanged);
		CollectionManager.Get().RegisterDeckCreatedListener(OnDeckCreatedByPlayer);
		CollectionManager.Get().RegisterDeckContentsListener(OnDeckContents);
		CollectionManager.Get().RegisterNewCardSeenListener(OnNewCardSeen);
		CollectionManager.Get().RegisterCardRewardsInsertedListener(OnCardRewardsInserted);
		CardBackManager.Get().SetSearchText(null);
		CosmeticCoinManager.Get().SetSearchText(null);
		m_shouldShowMultipleFavoriteCardBackTutorial = Network.IsLoggedIn() && !GameUtils.IsGSDFlagSet(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_HAS_SEEN_MULTIPLE_FAVORITE_CARD_BACKS);
		m_shouldShowMultipleFavoriteHeroTutorial = Network.IsLoggedIn() && !GameUtils.IsGSDFlagSet(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_HAS_SEEN_MULTIPLE_FAVORITE_HEROES);
		base.Start();
		if (m_setFilterTrayContainer != null)
		{
			m_setFilterTray = m_setFilterTrayContainer.PrefabGameObject(instantiateIfNeeded: true).GetComponentInChildren<SetFilterTray>(includeInactive: true);
			m_setFilterTray.m_toggleButton.AddEventListener(UIEventType.PRESS, delegate
			{
				OnSetFilterButtonPressed();
			});
		}
		if (m_filterButton != null)
		{
			if (ShouldSeeFilterButton())
			{
				m_filterButton.m_inactiveFilterButton.AddEventListener(UIEventType.PRESS, delegate
				{
					OnPhoneFilterButtonPressed();
				});
			}
			else
			{
				m_filterButton.gameObject.SetActive(value: false);
			}
		}
		bool showAdvancedCM = Options.Get().GetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, defaultVal: false);
		ShowAdvancedCollectionManager(showAdvancedCM);
		if (!showAdvancedCM)
		{
			Options.Get().RegisterChangedListener(Option.SHOW_ADVANCED_COLLECTIONMANAGER, OnShowAdvancedCMChanged);
		}
		DoEnterCollectionManagerEvents();
		if (!IsSpecialOneDeckMode())
		{
			MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_CollectionManager);
		}
		if (CollectionManager.Get().ShouldShowWildToStandardTutorial())
		{
			UserAttentionManager.StartBlocking(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS);
		}
		SetTavernBrawlTexturesIfNecessary();
		CollectionManager.Get().RequestDeckContentsForDecksWithoutContentsLoaded();
		StartCoroutine(WaitUntilReady());
		CollectionDeckTray.SideboardCardTileRightClicked += OnCardTileRightClicked;
		if (RegionUtils.IsCNLegalRegion)
		{
			GameObject obj = AssetLoader.Get().InstantiatePrefab(m_collectionSpinnerPrefab);
			m_collectionSpinner = ((obj != null) ? obj.GetComponent<CollectionManagerSpinnerPopup>() : null);
			if (m_collectionSpinner == null)
			{
				Log.CollectionManager.Print("Unable to create collection spinner");
				return;
			}
			CollectionManager.Get().RegisterRenameFinishedListener(OnRenameFinished);
			CollectionManager.Get().RegisterRenameValidatedListener(OnRenameValidated);
		}
	}

	private void OnRenameFinished()
	{
		if (!(m_collectionSpinner == null))
		{
			m_collectionSpinner.Show(OnRenameSpinnerOkClicked);
		}
	}

	private void OnRenameValidated(bool successful)
	{
		if (!(m_collectionSpinner == null))
		{
			m_collectionSpinner.UpdateSuccessOrFail(successful);
		}
	}

	private void OnRenameSpinnerOkClicked()
	{
	}

	protected override void Awake()
	{
		HearthstonePerformance.Get()?.StartPerformanceFlow(new FlowPerformance.SetupConfig
		{
			FlowType = Blizzard.Telemetry.WTCG.Client.FlowPerformance.FlowType.COLLECTION_MANAGER
		});
		m_manaTabManager.OnFilterCleared += ManaFilterTab_OnManaFilterCleared;
		m_manaTabManager.OnManaValueActivated += ManaFilterTab_OnManaValueActivated;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_deckTemplatePickerPhone = AssetLoader.Get().InstantiatePrefab("DeckTemplate_phone.prefab:a8a8fbcd170064edfb0eeac3f836a13b").GetComponent<DeckTemplatePicker>();
			SlidingTray component = m_deckTemplatePickerPhone.GetComponent<SlidingTray>();
			component.m_trayHiddenBone = m_deckTemplateHiddenBone.transform;
			component.m_trayShownBone = m_deckTemplateShownBone.transform;
		}
		CollectionManager.Get().HasSeenOvercappedDeckInfoPopup = false;
		CollectionManager.Get().HasSeenExtraRunesDeckInfoPopup = false;
		CurrentSetFilterFormatType = PegasusShared.FormatType.FT_STANDARD;
		base.Awake();
		StartCoroutine(InitCollectionWhenReady());
	}

	private void OnEnable()
	{
		if ((bool)m_runelockedCheckbox)
		{
			m_runelockedCheckbox.AddEventListener(UIEventType.RELEASE, ToggleHideRuneCards);
			m_runelockedCheckbox.SetChecked(isChecked: true);
		}
		if ((bool)m_runeLockedCheckboxLabel)
		{
			m_runeLockedCheckboxLabel.Text = GameStrings.Get("GLOBAL_DEATHKNIGHT_RUNE_LOCKED");
		}
	}

	private void OnDisable()
	{
		if ((bool)m_runelockedCheckbox)
		{
			m_runelockedCheckbox.RemoveEventListener(UIEventType.RELEASE, ToggleHideRuneCards);
		}
	}

	private void ToggleHideRuneCards(UIEvent e)
	{
		bool isChecked = m_runelockedCheckbox.IsChecked();
		CollectionManagerDisplay.HideLockedRunesCheckboxToggled?.Invoke(isChecked);
	}

	protected override void OnDestroy()
	{
		m_manaTabManager.OnFilterCleared -= ManaFilterTab_OnManaFilterCleared;
		m_manaTabManager.OnManaValueActivated -= ManaFilterTab_OnManaValueActivated;
		AssetHandle.SafeDispose(ref m_loadedCorkBackTexture);
		m_loadedClassTextures.DisposeValuesAndClear();
		if (m_deckTemplatePickerPhone != null)
		{
			UnityEngine.Object.Destroy(m_deckTemplatePickerPhone.gameObject);
			m_deckTemplatePickerPhone = null;
		}
		UserAttentionManager.StopBlocking(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS);
		base.OnDestroy();
	}

	private void Update()
	{
		if (HearthstoneApplication.IsInternal() && !UniversalInputManager.Get().IsTextInputActive())
		{
			if (InputCollection.GetKeyDown(KeyCode.Alpha1))
			{
				SetViewMode(CollectionUtils.ViewMode.HERO_SKINS);
			}
			else if (InputCollection.GetKeyDown(KeyCode.Alpha2))
			{
				SetViewMode(CollectionUtils.ViewMode.CARDS);
			}
			else if (InputCollection.GetKeyDown(KeyCode.Alpha3))
			{
				SetViewMode(CollectionUtils.ViewMode.CARD_BACKS);
			}
			else if (InputCollection.GetKeyDown(KeyCode.Alpha4))
			{
				SetViewMode(CollectionUtils.ViewMode.DECK_TEMPLATE);
			}
			else if (InputCollection.GetKeyDown(KeyCode.Alpha4))
			{
				OnCraftingModeButtonReleased(null);
			}
		}
		ShowDeckTemplateTipsIfNeeded();
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			StartCoroutine(OnApplicationFocusCoroutine());
		}
	}

	private IEnumerator OnApplicationFocusCoroutine()
	{
		yield return null;
		CheckClipboardAndPromptPlayerToPaste();
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
		CollectionDeckTray.Get().GetCardsContent().UnregisterCardTileRightClickedListener(OnCardTileRightClicked);
		CollectionDeckTray.Get().Unload();
		CollectionInputMgr.Get().Unload();
		CollectionManager.Get().RemoveCollectionLoadedListener(base.OnCollectionLoaded);
		CollectionManager.Get().RemoveCollectionChangedListener(OnCollectionChanged);
		CollectionManager.Get().RemoveDeckCreatedListener(OnDeckCreatedByPlayer);
		CollectionManager.Get().RemoveDeckContentsListener(OnDeckContents);
		CollectionManager.Get().RemoveNewCardSeenListener(OnNewCardSeen);
		CollectionManager.Get().RemoveCardRewardsInsertedListener(OnCardRewardsInserted);
		CollectionManager.Get().RemoveCollectionNetHandlers();
		CollectionManager.Get().RemoveRenameFinishedListener(OnRenameFinished);
		CollectionManager.Get().RemoveRenameValidatedListener(OnRenameValidated);
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		Options.Get().UnregisterChangedListener(Option.SHOW_ADVANCED_COLLECTIONMANAGER, OnShowAdvancedCMChanged);
		CollectionDeckTray.SideboardCardTileRightClicked -= OnCardTileRightClicked;
		m_unloading = false;
	}

	public override void Exit()
	{
		EnableInput(enable: false);
		DialogManager.Get().DismissAlertOrRemoveFromQueue(PASTE_DECK_POPUP_ID);
		NotificationManager.Get().DestroyAllPopUps();
		CollectionDeckTray.Get().Exit();
		if (m_pageManager != null)
		{
			m_pageManager.Exit();
		}
		CraftingManager.Get().SetCraftingUIActive(active: false);
		SceneMgr.Mode nextMode = SceneMgr.Get().GetPrevMode();
		if (nextMode == SceneMgr.Mode.GAMEPLAY)
		{
			nextMode = SceneMgr.Mode.HUB;
		}
		if (!Network.IsLoggedIn() && nextMode != SceneMgr.Mode.HUB)
		{
			DialogManager.Get().ShowReconnectHelperDialog();
			nextMode = SceneMgr.Mode.HUB;
			Navigation.Clear();
		}
		HearthstonePerformance.Get()?.StopCurrentFlow();
		SceneMgr.TransitionHandlerType transitionHandler = SceneMgr.TransitionHandlerType.SCENEMGR;
		if (SceneMgr.Get().IsInTavernBrawlMode() && nextMode == SceneMgr.Mode.GAME_MODE)
		{
			transitionHandler = SceneMgr.TransitionHandlerType.NEXT_SCENE;
		}
		SceneMgr.Get().SetNextMode(nextMode, transitionHandler);
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
		if (m_unloading)
		{
			return;
		}
		ClearCardActors();
		if (displayEmptyPage)
		{
			callback?.Invoke(new List<CollectionCardActors>(), new List<ICollectible>(), callbackData);
			return;
		}
		NetCache.Get().GetArcaneDustBalance();
		DefLoader.Get();
		List<ICollectible> nonCardCollectibles = new List<ICollectible>();
		foreach (TCollectible item in collectiblesToDisplay)
		{
			ICollectible collectible = item;
			if (!(collectible is CollectibleCard card))
			{
				nonCardCollectibles.Add(collectible);
				continue;
			}
			EntityDef entityDef = card.GetEntityDef();
			TAG_PREMIUM premiumType = card.PremiumType;
			bool isHero = entityDef.IsHeroSkin();
			string actorPath = (isHero ? ActorNames.GetHeroSkinOrHandActor(entityDef, premiumType) : ActorNames.GetHandActor(entityDef, premiumType));
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
			newActor.SetPremium(premiumType);
			newActor.CreateBannedRibbon();
			if ((m_currentViewMode != 0 || m_currentViewSubmode != CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES) && card.OwnedCount == 0)
			{
				CraftingManager craftingManager = CraftingManager.Get();
				bool canUpgrade = craftingManager.CanUpgradeCardToGolden(entityDef.GetCardId(), premiumType) == CraftingManager.CanCraftCardResult.CanUpgrade;
				bool canCraft = craftingManager.CanCraftCardRightNow(entityDef, premiumType) == CraftingManager.CanCraftCardResult.CanCraft;
				if (card.IsCraftable && (canCraft || canUpgrade))
				{
					newActor.GhostCardEffect(GhostCard.Type.MISSING, premiumType, update: false);
				}
				else if (isHero && HeroSkinUtils.CanBuyHeroSkinFromCollectionManager(entityDef.GetCardId()))
				{
					newActor.GhostCardEffect(GhostCard.Type.PURCHASABLE_HERO_SKIN, premiumType, update: false);
				}
				else
				{
					newActor.MissingCardEffect(refreshOnFocus: true, updateComponents: false);
				}
			}
			newActor.UpdateAllComponents(needsGhostUpdate: false);
			m_cardActors.Add(new CollectionCardActors(newActor));
		}
		callback?.Invoke(m_cardActors, nonCardCollectibles, callbackData);
	}

	public void CollectionPageContentsChangedToCardBacks(List<CardBackManager.OwnedCardBack> cardBacksToDisplay, CollectionActorsReadyCallback callback)
	{
		Log.CollectionManager.Print("transitionPageId={0} pagesTurning={1}", m_pageManager.GetTransitionPageId(), m_pageManager.ArePagesTurning());
		List<CollectionCardActors> result = new List<CollectionCardActors>();
		ClearCardActors();
		if (cardBacksToDisplay.Count == 0)
		{
			if (callback != null)
			{
				callback(result, null, null);
			}
			return;
		}
		int numCardBacksToLoad = cardBacksToDisplay.Count;
		Action<int, CardBackManager.OwnedCardBack, Actor> cbLoadedCallback = delegate(int index, CardBackManager.OwnedCardBack cardBack, Actor actor)
		{
			if (actor != null)
			{
				result[index] = new CollectionCardActors(actor);
				actor.SetCardbackUpdateIgnore(ignoreUpdate: true);
				CollectionCardBack component = actor.GetComponent<CollectionCardBack>();
				if (component != null)
				{
					component.SetCardBackId(cardBack.m_cardBackId);
					component.SetCardBackName(CardBackManager.Get().GetCardBackName(cardBack.m_cardBackId));
				}
				else
				{
					Debug.LogError("CollectionCardBack component does not exist on actor!");
				}
				if (!cardBack.m_owned)
				{
					if (cardBack.m_canBuy)
					{
						actor.GhostCardEffect(GhostCard.Type.MISSING);
					}
					else
					{
						actor.MissingCardEffect();
					}
				}
			}
			int num = numCardBacksToLoad - 1;
			numCardBacksToLoad = num;
			if (numCardBacksToLoad == 0 && callback != null)
			{
				callback(result, null, null);
			}
		};
		for (int i = 0; i < cardBacksToDisplay.Count; i++)
		{
			int currIndex = i;
			CardBackManager.OwnedCardBack cardBackLoad = cardBacksToDisplay[i];
			int cardBackId = cardBackLoad.m_cardBackId;
			result.Add(null);
			CardBackManager cbm = CardBackManager.Get();
			if (cbm == null || !cbm.LoadCardBackByIndex(cardBackId, delegate(CardBackManager.LoadCardBackData cardBackData)
			{
				GameObject gameObject = cardBackData.m_GameObject;
				gameObject.transform.parent = base.transform;
				gameObject.name = gameObject.name + "_" + cardBackData.m_CardBackIndex;
				Actor component2 = gameObject.GetComponent<Actor>();
				if (component2 == null)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
				else
				{
					GameObject cardMesh = component2.m_cardMesh;
					component2.SetCardbackUpdateIgnore(ignoreUpdate: true);
					component2.SetUnlit();
					if (cardMesh != null)
					{
						Material material = cardMesh.GetComponent<Renderer>().GetMaterial();
						if (material.HasProperty("_SpecularIntensity"))
						{
							material.SetFloat("_SpecularIntensity", 0f);
						}
					}
					m_cardActors.Add(new CollectionCardActors(component2));
				}
				cbLoadedCallback(currIndex, cardBackLoad, component2);
			}, "Collection_Card_Back.prefab:a208f592a46e4f447b3026e82444177e"))
			{
				cbLoadedCallback(currIndex, cardBackLoad, null);
			}
		}
	}

	public void RequestContentsToShowDeck(long deckID)
	{
		m_showDeckContentsRequest = deckID;
		CollectionManager.Get().RequestDeckContents(m_showDeckContentsRequest);
	}

	public void ShowPhoneDeckTemplateTray()
	{
		m_pageManager.UpdateDeckTemplate(m_deckTemplatePickerPhone);
		SlidingTray component = m_deckTemplatePickerPhone.GetComponent<SlidingTray>();
		component.RegisterTrayToggleListener(m_deckTemplatePickerPhone.OnTrayToggled);
		component.ShowTray();
	}

	public DeckTemplatePicker GetPhoneDeckTemplateTray()
	{
		return m_deckTemplatePickerPhone;
	}

	public override void SetViewMode(CollectionUtils.ViewMode mode, bool triggerResponse, CollectionUtils.ViewModeData userdata = null)
	{
		Log.CollectionManager.Print("mode={0}-->{1} triggerResponse={2} isUpdatingTrayMode={3}", m_currentViewMode, mode, triggerResponse, CollectionDeckTray.Get().IsUpdatingTrayMode());
		if (m_currentViewMode == mode || (CollectionDeckTray.Get().IsUpdatingTrayMode() && (mode == CollectionUtils.ViewMode.HERO_SKINS || mode == CollectionUtils.ViewMode.CARD_BACKS || mode == CollectionUtils.ViewMode.COINS)))
		{
			return;
		}
		if (mode == CollectionUtils.ViewMode.DECK_TEMPLATE)
		{
			if (!CollectionManager.Get().IsInEditMode() || SceneMgr.Get().IsInTavernBrawlMode())
			{
				return;
			}
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				ShowPhoneDeckTemplateTray();
			}
		}
		if (mode != 0)
		{
			SetRuneLockedCheckboxVisible(visible: false);
		}
		CollectionUtils.ViewMode prevMode = m_currentViewMode;
		m_currentViewMode = mode;
		OnSwitchViewModeResponse(triggerResponse, prevMode, mode, userdata);
	}

	public override void SetViewSubmode(CollectionUtils.ViewSubmode mode)
	{
		bool num = m_currentViewSubmode != mode;
		base.SetViewSubmode(mode);
		if (num)
		{
			OnSwitchViewSubmodeResponse(mode);
		}
	}

	public bool ViewModeHasVisibleDeckList()
	{
		if (m_currentViewMode != CollectionUtils.ViewMode.DECK_TEMPLATE)
		{
			return m_currentViewMode != CollectionUtils.ViewMode.MASS_DISENCHANT;
		}
		return false;
	}

	public void OnDoneEditingDeck()
	{
		ShowAppropriateSetFilters();
		if (m_currentViewMode == CollectionUtils.ViewMode.DECK_TEMPLATE)
		{
			SetViewMode(CollectionUtils.ViewMode.CARDS, triggerResponse: false);
		}
		if (!SceneMgr.Get().IsInTavernBrawlMode())
		{
			m_pageManager.SetDeckRuleset(null);
		}
		SetRuneLockedCheckboxVisible(visible: false);
		m_pageManager.OnDoneEditingDeck();
	}

	private void ManaFilterTab_OnManaFilterCleared(bool transitionPage)
	{
		ManaFilterTab_OnManaValueActivated(-1, transitionPage);
		m_manaFilterIsFromSearchText = false;
	}

	public void ManaFilterTab_OnManaValueActivated(int cost, bool transitionPage)
	{
		if (m_manaFilterIsFromSearchText)
		{
			bool updateManaFilterToMatchSearchText = false;
			RemoveManaTokenFromSearchText(updateManaFilterToMatchSearchText);
		}
		bool isActive = m_manaTabManager.IsManaValueActive(cost);
		string costString = ((cost < 7) ? cost.ToString() : (cost + "+"));
		NotifyFilterUpdate(m_manaFilterListeners, isActive, costString);
		m_pageManager.FilterByManaCost(cost, transitionPage);
	}

	public override void FilterBySearchText(string newSearchText)
	{
		string oldSearchText = m_search.GetText();
		base.FilterBySearchText(newSearchText);
		OnSearchDeactivated_Internal(oldSearchText, newSearchText, updateManaFilterToMatchSearchText: true);
	}

	private void RemoveManaTokenFromSearchText(bool updateManaFilterToMatchSearchText)
	{
		string oldSearchText = m_search.GetText();
		if (string.IsNullOrEmpty(oldSearchText))
		{
			return;
		}
		string[] oldSearchTerms = oldSearchText.Split(CollectibleFilteredSet<ICollectible>.SearchTokenDelimiters, StringSplitOptions.RemoveEmptyEntries);
		if (oldSearchTerms.Length == 0)
		{
			return;
		}
		bool hasManaToken = false;
		Func<string, bool> isManaToken = GetIsManaSearchTokenFunc();
		string[] newSearchTerms = oldSearchTerms.Where(delegate(string t)
		{
			if (isManaToken(t))
			{
				hasManaToken = true;
				return false;
			}
			return true;
		}).ToArray();
		if (hasManaToken)
		{
			string newSearchText = string.Join(new string(CollectibleFilteredSet<ICollectible>.SearchTokenDelimiters[0], 1), newSearchTerms);
			m_search.SetText(newSearchText);
			OnSearchDeactivated_Internal(oldSearchText, m_search.GetText(), updateManaFilterToMatchSearchText);
		}
	}

	private void UpdateManaFilterToMatchSearchText(string searchText, bool transitionPage = true)
	{
		if (string.IsNullOrEmpty(searchText) || !m_manaTabManager.Enabled)
		{
			m_manaTabManager.ClearFilter(transitionPage);
			return;
		}
		Func<string, bool> isManaToken = GetIsManaSearchTokenFunc();
		string manaSearchToken = searchText.Split(CollectibleFilteredSet<ICollectible>.SearchTokenDelimiters, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(isManaToken);
		if (manaSearchToken != null)
		{
			if (m_pageManager.IsManaCostFilterActive)
			{
				m_pageManager.FilterByManaCost(-1, transitionPage);
			}
			string val = manaSearchToken.Split(CollectibleFilteredSet<ICollectible>.SearchTagColons, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
			GeneralUtils.ParseNumericRange(val, out var isNumericValue, out var minVal, out var maxVal);
			string searchTerm = null;
			if (isNumericValue)
			{
				m_manaTabManager.SetFilter_Range(minVal, maxVal);
				searchTerm = val;
			}
			else
			{
				string even = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_EVEN_MANA").ToLower();
				string odd = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_ODD_MANA").ToLower();
				string lowerVal = val.ToLower();
				bool isEven = lowerVal == even;
				bool isOdd = !isEven && lowerVal == odd;
				if (isOdd || isEven)
				{
					m_manaTabManager.SetFilter_EvenOdd(isOdd);
					searchTerm = CollectibleCardFilter.CreateSearchTerm_Mana_OddEven(isOdd);
				}
			}
			if (searchTerm != null)
			{
				m_manaFilterIsFromSearchText = true;
				NotifyFilterUpdate(m_manaFilterListeners, active: true, searchTerm);
			}
		}
		else
		{
			m_manaTabManager.ClearFilter(transitionPage);
		}
	}

	private Func<string, bool> GetIsManaSearchTokenFunc()
	{
		string manaToken = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MANA").ToLower();
		string evenTokenValue = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_EVEN_MANA").ToLower();
		string oddTokenValue = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_ODD_MANA").ToLower();
		return delegate(string token)
		{
			string[] array = token.Split(CollectibleFilteredSet<ICollectible>.SearchTagColons, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length >= 2 && array[0].Trim().ToLower() == manaToken)
			{
				string text = array[1].Trim();
				GeneralUtils.ParseNumericRange(text, out var isNumericalValue, out var _, out var _);
				if (isNumericalValue)
				{
					return true;
				}
				string text2 = text.ToLower();
				if (text2 == oddTokenValue || text2 == evenTokenValue)
				{
					return true;
				}
			}
			return false;
		};
	}

	public override void HideAllTips()
	{
		if (m_innkeeperLClickReminder != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_innkeeperLClickReminder);
		}
		HideDeckHelpPopup();
		if (m_convertTutorialPopup != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_convertTutorialPopup);
		}
		if (m_createDeckNotification != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_createDeckNotification);
		}
		if (m_multipleFavoriteCardBacksNotification != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_multipleFavoriteCardBacksNotification);
		}
	}

	public void HideDeckHelpPopup()
	{
		if (m_deckHelpPopup != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_deckHelpPopup);
		}
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		if (deckTray != null)
		{
			deckTray.GetCardsContent()?.HideDeckHelpPopup();
			deckTray.GetCardBackContent()?.HideTutorials();
		}
	}

	public override void ShowInnkeeperLClickHelp(EntityDef entityDef)
	{
		bool isHeroSkin = entityDef?.IsHeroSkin() ?? false;
		ShowInnkeeperLClickHelp(isHeroSkin);
	}

	private void ShowInnkeeperLClickHelp(bool isHero)
	{
		if (!CollectionDeckTray.Get().IsShowingDeckContents())
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
	}

	public void ShowPremiumCardsNotOwned(bool show)
	{
		m_pageManager.ShowCardsNotOwned(show, null);
	}

	private void FeaturedCardsSetFilterCallback(List<TAG_CARD_SET> cardSets, List<int> specificCards, PegasusShared.FormatType formatType, SetFilterItem item, bool transitionPage)
	{
		SetLastSeenFeaturedCardsEvent(m_currentActiveFeaturedCardsEvent, GameSaveKeySubkeyId.COLLECTION_MANAGER_LAST_SEEN_FEATURED_CARDS_EVENT_ITEM);
		item.SetIconFxActive(active: false);
		SetFilterCallback(cardSets, specificCards, formatType, item, transitionPage);
	}

	public override void SetFilterCallback(List<TAG_CARD_SET> cardSets, List<int> specificCards, PegasusShared.FormatType formatType, SetFilterItem item, bool transitionPage)
	{
		bool isWild = formatType == PegasusShared.FormatType.FT_WILD;
		if (isWild && !CollectionManager.Get().AccountHasUnlockedWild() && !SceneMgr.Get().IsInTavernBrawlMode())
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_COLLECTION_SET_FILTER_WILD_SET_HEADER"),
				m_text = GameStrings.Get("GLUE_COLLECTION_SET_FILTER_WILD_SET_BODY"),
				m_cancelText = GameStrings.Get("GLOBAL_CANCEL"),
				m_confirmText = GameStrings.Get("GLOBAL_BUTTON_YES"),
				m_showAlertIcon = true,
				m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
				m_responseCallback = delegate(AlertPopup.Response response, object userData)
				{
					if (response == AlertPopup.Response.CONFIRM)
					{
						ShowSetFilterCards(cardSets, specificCards, transitionPage);
					}
					else
					{
						m_setFilterTray.SelectPreviouslySelectedItem();
					}
				}
			};
			DialogManager.Get().ShowPopup(info);
		}
		else
		{
			m_search.SetWildModeActive(isWild);
			if (formatType == PegasusShared.FormatType.FT_TWIST)
			{
				ShowTwistFilterCards(transitionPage, cardSets);
			}
			else
			{
				ShowSetFilterCards(cardSets, specificCards, transitionPage);
			}
			CurrentSetFilterFormatType = formatType;
		}
	}

	private void ShowTwistFilterCards(bool transitionPage, List<TAG_CARD_SET> cardSets)
	{
		List<HashSet<string>> subsets = null;
		PegasusShared.FormatType formatType = PegasusShared.FormatType.FT_TWIST;
		DeckRuleset ruleset = DeckRuleset.GetRuleset(formatType);
		if (ruleset == null)
		{
			return;
		}
		subsets = ruleset.GetCardIdsFromAnySubsetRules();
		List<TAG_CLASS> validClasses = null;
		if (!CollectionManager.Get().IsInEditMode())
		{
			validClasses = new List<TAG_CLASS>();
			List<TAG_CLASS> excludedClasses = RankMgr.GetExcludedClassesForFormat(formatType);
			foreach (TAG_CLASS classTag in Enum.GetValues(typeof(TAG_CLASS)))
			{
				if (!excludedClasses.Contains(classTag))
				{
					validClasses.Add(classTag);
				}
			}
		}
		m_pageManager.FilterBySubsetsCardsetsAndClasses(subsets, cardSets, validClasses, transitionPage);
		NotifyFilterUpdate(m_setFilterListeners, cardSets != null, null);
	}

	private void ShowSetFilterCards(List<TAG_CARD_SET> cardSets, List<int> specificCards, bool transitionPage = true)
	{
		if (!CollectionManager.Get().IsInEditMode())
		{
			m_pageManager.FilterByClasses(null);
		}
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

	public HeroPickerDisplay GetHeroPickerDisplay()
	{
		return m_heroPickerDisplay;
	}

	public void EnterSelectNewDeckHeroMode()
	{
		if (!m_selectingNewDeckHero)
		{
			EnableInput(enable: false);
			m_selectingNewDeckHero = true;
			m_heroPickerDisplay = AssetLoader.Get().InstantiatePrefab("HeroPicker.prefab:59e2d2f899d09f4488a194df18967915").GetComponent<HeroPickerDisplay>();
			NotificationManager.Get().DestroyAllPopUps();
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
			{
				m_pageManager.HideNonDeckTemplateTabs(hide: true);
			}
			CheckClipboardAndPromptPlayerToPaste();
		}
	}

	public void ExitSelectNewDeckHeroMode()
	{
		m_selectingNewDeckHero = false;
	}

	public void CancelSelectNewDeckHeroMode()
	{
		EnableInput(enable: true);
		m_pageManager.HideNonDeckTemplateTabs(hide: false, updateTabs: true);
		ExitSelectNewDeckHeroMode();
	}

	public bool CanViewHeroSkins()
	{
		CollectionManager cm = CollectionManager.Get();
		CollectionDeck currDeck = CollectionManager.Get().GetEditedDeck();
		if (currDeck == null)
		{
			return true;
		}
		return cm.GetCountOfOwnedHeroesForClass(currDeck.GetClass()) > 1;
	}

	public bool CanViewCardBacks()
	{
		return CardBackManager.Get().GetCardBacksOwned().Count > 1;
	}

	public bool CanViewCoins()
	{
		return CosmeticCoinManager.Get().GetTotalCoinsOwned() > 1;
	}

	public void RegisterManaFilterListener(FilterStateListener listener)
	{
		m_manaFilterListeners.Add(listener);
	}

	public void UnregisterManaFilterListener(FilterStateListener listener)
	{
		m_manaFilterListeners.Remove(listener);
	}

	public void RegisterSetFilterListener(FilterStateListener listener)
	{
		m_setFilterListeners.Add(listener);
	}

	public void UnregisterSetFilterListener(FilterStateListener listener)
	{
		m_setFilterListeners.Remove(listener);
	}

	public override void ResetFilters(bool updateVisuals = true)
	{
		if (m_manaTabManager != null)
		{
			m_manaTabManager.ClearFilter(transitionPage: false);
		}
		if (m_setFilterTray != null)
		{
			m_setFilterTray.ClearFilter(transitionPage: false);
		}
		base.ResetFilters(updateVisuals);
	}

	public void ShowAppropriateSetFilters()
	{
		bool inCraftingMode = InCraftingMode();
		bool editingDeck = CollectionManager.Get().IsInEditMode();
		PegasusShared.FormatType formatType = PegasusShared.FormatType.FT_STANDARD;
		if (editingDeck)
		{
			formatType = CollectionManager.Get().GetEditedDeck().FormatType;
		}
		else if (RankMgr.Get().WildCardsAllowedInCurrentLeague())
		{
			if (CollectionManager.Get().ShouldAccountSeeStandardWild() || inCraftingMode)
			{
				formatType = PegasusShared.FormatType.FT_WILD;
			}
		}
		else if (inCraftingMode)
		{
			formatType = PegasusShared.FormatType.FT_STANDARD;
		}
		else if (CollectionManager.Get().AccountHasUnlockedWild())
		{
			formatType = PegasusShared.FormatType.FT_WILD;
		}
		UpdateSetFilters(formatType, editingDeck, inCraftingMode);
	}

	public void UpdateSetFilters(PegasusShared.FormatType formatType, bool editingDeck, bool showUnownedSets = false)
	{
		m_setFilterTray.UpdateSetFilters(formatType, editingDeck, showUnownedSets);
	}

	public ActiveFilterButton GetFilterButton()
	{
		return m_filterButton;
	}

	public void HideFilterTrayOnStartDragCard()
	{
		if (IsShowingSetFilterTray())
		{
			m_filterButton.m_setFilterTray.ToggleTraySlider(show: false);
		}
	}

	public void UnhideFilterTrayOnStopDragCard()
	{
		if (IsShowingSetFilterTray())
		{
			m_filterButton.m_setFilterTray.ToggleTraySlider(show: true);
		}
	}

	public void WaitThenUnhideFilterTrayOnStopDragCard()
	{
		if (IsShowingSetFilterTray())
		{
			StartCoroutine(WaitThenUnhideFilterTrayOnStopDragCard_Coroutine());
		}
	}

	private IEnumerator WaitThenUnhideFilterTrayOnStopDragCard_Coroutine()
	{
		yield return new WaitForSeconds(0.5f);
		if (CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay != null && IsShowingSetFilterTray() && CollectionInputMgr.Get() != null && !CollectionInputMgr.Get().HasHeldCard())
		{
			m_filterButton.m_setFilterTray.ToggleTraySlider(show: true);
		}
	}

	public bool SetFilterIsDefaultSelection()
	{
		if (m_setFilterTray == null)
		{
			return true;
		}
		return !m_setFilterTray.HasActiveFilter();
	}

	public bool IsShowingSetFilterTray()
	{
		if (m_setFilterTray == null)
		{
			return false;
		}
		return m_setFilterTray.IsShown();
	}

	public bool IsSelectingNewDeckHero()
	{
		return m_selectingNewDeckHero;
	}

	private void OnDeckContents(long deckID)
	{
		if (deckID == m_showDeckContentsRequest)
		{
			m_showDeckContentsRequest = 0L;
			ShowDeck(deckID, isNewDeck: false, showDeckTemplatePage: false, null);
		}
		else
		{
			CollectionDeckTray.Get().GetDecksContent().OnDeckContentsUpdated(deckID);
		}
	}

	private void OnDeckCreatedByPlayer(long deckID, string name)
	{
		bool showDeckTemplatePage = false;
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
		{
			CollectionManager collectionManager = CollectionManager.Get();
			if (collectionManager == null)
			{
				Debug.LogError("CollectionManagerDisplay.OnDeckCreatedByPlayer: CollectionManager.Get() returned null");
				return;
			}
			CollectionDeck curDeck = collectionManager.GetDeck(deckID);
			if (curDeck == null)
			{
				Debug.LogError("CollectionManagerDisplay.OnDeckCreatedByPlayer: Could not get deck " + deckID);
				return;
			}
			if (CollectionManager.Get().GetNonStarterTemplateDecks(curDeck.FormatType, curDeck.GetClass()).Count > 0)
			{
				showDeckTemplatePage = true;
			}
		}
		ShowDeck(deckID, isNewDeck: true, showDeckTemplatePage, null);
	}

	private void OnNewCardSeen(string cardID, TAG_PREMIUM premium)
	{
		m_pageManager.UpdateClassTabNewCardCounts();
	}

	private void OnCardRewardsInserted(List<string> cardID, List<TAG_PREMIUM> premium)
	{
		m_pageManager.RefreshCurrentPageContents();
	}

	protected override void OnCollectionChanged()
	{
		if (m_currentViewMode != CollectionUtils.ViewMode.MASS_DISENCHANT)
		{
			m_pageManager.NotifyOfCollectionChanged();
		}
	}

	private void ClearCardActors()
	{
		foreach (CollectionCardActors previousCardActor in m_previousCardActors)
		{
			previousCardActor.Destroy();
		}
		m_previousCardActors.Clear();
		m_previousCardActors = m_cardActors;
		m_cardActors = new List<CollectionCardActors>();
	}

	private IEnumerator WaitUntilReady()
	{
		while (!m_netCacheReady && Network.IsLoggedIn())
		{
			yield return 0;
		}
		InitDeckTray();
	}

	private void InitDeckTray()
	{
		CollectionDeckTray collectionDeckTray = CollectionDeckTray.Get();
		collectionDeckTray.Initialize();
		collectionDeckTray.RegisterModeSwitchedListener(delegate
		{
			UpdateCurrentPageCardLocks();
		});
		collectionDeckTray.GetCardsContent().RegisterCardTileRightClickedListener(OnCardTileRightClicked);
		m_isReady = true;
	}

	private IEnumerator InitCollectionWhenReady()
	{
		while (!m_pageManager.IsFullyLoaded())
		{
			yield return null;
		}
		m_pageManager.LoadMassDisenchantScreen();
		m_pageManager.OnCollectionLoaded();
	}

	protected override bool ShouldStartShown()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return false;
		}
		return base.ShouldStartShown();
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

	private void OnShowAdvancedCMChanged(Option option, object prevValue, bool existed, object userData)
	{
		bool showAdvancedCM = Options.Get().GetBool(Option.SHOW_ADVANCED_COLLECTIONMANAGER, defaultVal: false);
		if (showAdvancedCM)
		{
			Options.Get().UnregisterChangedListener(Option.SHOW_ADVANCED_COLLECTIONMANAGER, OnShowAdvancedCMChanged);
		}
		ShowAdvancedCollectionManager(showAdvancedCM);
		m_manaTabManager.ActivateTabs(active: true);
	}

	private void OnCardTileRightClicked(DeckTrayDeckTileVisual cardTile)
	{
		if (GetViewMode() != CollectionUtils.ViewMode.DECK_TEMPLATE && (!TavernBrawlDisplay.IsTavernBrawlOpen() || !TavernBrawlDisplay.IsTavernBrawlViewing()) && !GameUtils.IsZilliaxModule(cardTile.GetActor().GetEntityDef()))
		{
			if (!cardTile.GetSlot().Owned)
			{
				CraftingManager.Get().EnterCraftMode(cardTile.GetActor());
			}
			GoToPageWithCard(cardTile.GetCardID(), cardTile.GetPremium());
		}
	}

	protected override void LoadAllTextures()
	{
		foreach (TAG_CLASS classTag in Enum.GetValues(typeof(TAG_CLASS)))
		{
			string textureAssetPath = GetClassTextureAssetPath(classTag);
			if (!string.IsNullOrEmpty(textureAssetPath))
			{
				AssetLoader.Get().LoadAsset<Texture>(textureAssetPath, OnClassTextureLoaded, classTag);
			}
		}
	}

	protected override void UnloadAllTextures()
	{
		m_loadedClassTextures.DisposeValuesAndClear();
	}

	public static string GetClassTextureAssetPath(TAG_CLASS classTag)
	{
		return classTag switch
		{
			TAG_CLASS.DRUID => "Druid.psd:e2417dc1394f54349956b2e24a27f923", 
			TAG_CLASS.HUNTER => "Hunter.psd:16178c8d6ed14814dae893bad9de80d5", 
			TAG_CLASS.MAGE => "Mage.psd:8dcb9bd578b6c01448cf1021c6157dfd", 
			TAG_CLASS.PALADIN => "Paladin.psd:50ba8fc595684d440866ac130c146d57", 
			TAG_CLASS.PRIEST => "Priest.psd:5fa4606c71c0dff4eb0b07b88ba83197", 
			TAG_CLASS.ROGUE => "Rogue.psd:47dc46a5269d7fc4a8a9ebada8f2d890", 
			TAG_CLASS.SHAMAN => "Shaman.psd:2e468e3b0f7a7804a9335333c9e673e2", 
			TAG_CLASS.WARLOCK => "Warlock.psd:d6077adee4894df43a67617620de56a9", 
			TAG_CLASS.WARRIOR => "Warrior.psd:5376d479d4155ca419f8afa5e42ba505", 
			_ => "", 
		};
	}

	private void SetTavernBrawlTexturesIfNecessary()
	{
		if (!SceneMgr.Get().IsInTavernBrawlMode())
		{
			return;
		}
		if (m_bookBack != null && !string.IsNullOrEmpty(m_tbCorkBackTexture) && m_customBookBackMesh != null)
		{
			m_bookBack.GetComponent<MeshFilter>().mesh = m_customBookBackMesh;
			AssetLoader.Get().LoadAsset(ref m_loadedCorkBackTexture, m_tbCorkBackTexture);
			m_bookBack.GetComponent<MeshRenderer>().GetMaterial().SetTexture("_MainTex", m_loadedCorkBackTexture);
			m_setFilterTray.m_toggleButton.SetButtonBackgroundMaterial();
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return;
		}
		foreach (GameObject go in m_customObjectsToSwap)
		{
			Renderer renderer = go.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.SetMaterial(m_tavernBrawlElements);
				continue;
			}
			Debug.LogErrorFormat("Failed to swap material for TavernBrawl object: {0}", go.name);
		}
	}

	private void OnClassTextureLoaded(AssetReference assetRef, AssetHandle<Texture> loadedTexture, object callbackData)
	{
		if (loadedTexture == null)
		{
			Debug.LogWarning($"CollectionManagerDisplay.OnClassTextureLoaded(): asset for {assetRef} is null!");
			return;
		}
		TAG_CLASS classTag = (TAG_CLASS)callbackData;
		m_loadedClassTextures.SetOrReplaceDisposable(classTag, loadedTexture);
	}

	public void ShowCurrentEditedDeck()
	{
		CollectionDeck currDeck = CollectionManager.Get().GetEditedDeck();
		if (currDeck != null)
		{
			List<TAG_CLASS> deckClasses = currDeck.GetClasses();
			ShowDeckHelper(currDeck, deckClasses, isNewDeck: false, showDeckTemplatePage: false, null);
		}
	}

	public void ShowDeck(long deckID, bool isNewDeck, bool showDeckTemplatePage, CollectionUtils.ViewMode? setNewViewMode = null)
	{
		CollectionDeck currDeck = CollectionManager.Get().GetDeck(deckID);
		if (currDeck == null)
		{
			return;
		}
		List<TAG_CLASS> deckClasses = GetDeckHeroClasses(deckID);
		ShowDeckHelper(currDeck, deckClasses, isNewDeck, showDeckTemplatePage, setNewViewMode);
		if (!showDeckTemplatePage)
		{
			bool shouldShowAllClassCards = false;
			if (m_pageManager != null)
			{
				shouldShowAllClassCards = m_pageManager.ShouldShowAllClassCards(currDeck);
			}
			SetRuneLockedCheckboxVisible(GameUtils.HasClassTag(TAG_CLASS.DEATHKNIGHT, deckClasses) && !shouldShowAllClassCards);
		}
	}

	private void ShowDeckHelper(CollectionDeck currDeck, List<TAG_CLASS> deckClasses, bool isNewDeck, bool showDeckTemplatePage, CollectionUtils.ViewMode? setNewViewMode = null)
	{
		if (currDeck.HasUIHeroOverride() && m_currentViewMode == CollectionUtils.ViewMode.HERO_SKINS)
		{
			m_pageManager.JumpToCollectionClassPage(deckClasses.First());
		}
		if (!showDeckTemplatePage)
		{
			m_pageManager.HideNonDeckTemplateTabs(hide: false);
		}
		CollectionManager.Get().StartEditingDeck(currDeck, isNewDeck);
		if (showDeckTemplatePage)
		{
			setNewViewMode = CollectionUtils.ViewMode.DECK_TEMPLATE;
		}
		else if ((m_currentViewMode == CollectionUtils.ViewMode.HERO_SKINS && !CanViewHeroSkins()) || (m_currentViewMode == CollectionUtils.ViewMode.CARD_BACKS && !CanViewCardBacks()) || (m_currentViewMode == CollectionUtils.ViewMode.COINS && !CanViewCoins()))
		{
			setNewViewMode = CollectionUtils.ViewMode.CARDS;
		}
		CollectionDeckTray.Get().ShowDeck(setNewViewMode ?? GetViewMode());
		if (setNewViewMode.HasValue)
		{
			bool newViewModeTriggerResponse = showDeckTemplatePage;
			SetViewMode(setNewViewMode.Value, newViewModeTriggerResponse);
		}
		UpdateSetFilters(currDeck.FormatType, editingDeck: true);
		m_pageManager.UpdateFiltersForDeck(currDeck, deckClasses, isNewDeck);
		m_pageManager.UpdateCraftingModeButtonDustBottleVisibility(CollectionManager.Get().GetCardsToMassDisenchantCount());
		NotificationManager.Get().DestroyNotification(m_createDeckNotification, 0.25f);
	}

	private List<TAG_CLASS> GetDeckHeroClasses(long deckID)
	{
		List<TAG_CLASS> results = new List<TAG_CLASS>();
		CollectionDeck deck = CollectionManager.Get().GetDeck(deckID);
		if (deck == null)
		{
			Log.CollectionManager.Print($"CollectionManagerDisplay no deck with ID {deckID}!");
			results.Add(TAG_CLASS.INVALID);
			return results;
		}
		return deck.GetClasses();
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
		m_manaTabManager.ActivateTabs(active: true);
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
		OnCoverOpened();
		m_manaTabManager.ActivateTabs(active: true);
	}

	private void DoBookClosingAnimations()
	{
		if (m_cover != null)
		{
			m_cover.Close();
		}
		m_manaTabManager.ActivateTabs(active: false);
	}

	private void ShowAdvancedCollectionManager(bool show)
	{
		show |= (bool)UniversalInputManager.UsePhoneUI;
		m_manaTabManager.gameObject.SetActive(show);
		if (m_setFilterTray != null)
		{
			bool showSetFilterButton = show && !UniversalInputManager.UsePhoneUI;
			m_setFilterTray.SetButtonShown(showSetFilterButton);
		}
		if (m_craftingTray == null)
		{
			AssetLoader.Get().InstantiatePrefab(UniversalInputManager.UsePhoneUI ? "CraftingTray_phone.prefab:bd4719b05f6f24870be20fa595b2032a" : "CraftingTray.prefab:dae9f103e23a53f459baeef392daa984", OnCraftingTrayLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
		if (ShouldSeeCraftingButton())
		{
			m_craftingModeButton.gameObject.SetActive(value: true);
			m_craftingModeButton.AddEventListener(UIEventType.RELEASE, base.OnCraftingModeButtonReleased);
		}
		else
		{
			m_craftingModeButton.gameObject.SetActive(value: false);
		}
		if (m_setFilterTray != null && show && !m_setFilterTrayInitialized)
		{
			m_setFilterTray.AddItemUsingTexture(GameStrings.Get("GLUE_COLLECTION_ALL_STANDARD_CARDS"), m_allSetsTexture, m_allSetsIconOffset, SetFilterCallback, new List<TAG_CARD_SET>(GameUtils.GetStandardSets()), null, PegasusShared.FormatType.FT_STANDARD, isAllStandard: true, tooltipActive: true, GameStrings.Get("GLUE_TOOLTIP_HEADER_ALL_STANDARD_CARDS"), GameStrings.Get("GLUE_TOOLTIP_DESCRIPTION_ALL_STANDARD_CARDS"));
			m_setFilterTray.AddItemUsingTexture(GameStrings.Get("GLUE_COLLECTION_WILD_CARDS"), m_wildSetsTexture, m_wildSetsIconOffset, SetFilterCallback, new List<TAG_CARD_SET>(GameUtils.GetAllWildPlayableSets()), null, PegasusShared.FormatType.FT_WILD, isAllStandard: false, tooltipActive: true, GameStrings.Get("GLUE_TOOLTIP_HEADER_WILD_CARDS"), GameStrings.Get("GLUE_TOOLTIP_DESCRIPTION_WILD_CARDS"));
			m_setFilterTray.AddItemUsingTexture(GameStrings.Get("GLUE_COLLECTION_TWIST_CARDS"), m_twistSetsTexture, m_twistSetsIconOffset, SetFilterCallback, new List<TAG_CARD_SET>(GameUtils.GetTwistSets()), null, PegasusShared.FormatType.FT_TWIST, isAllStandard: false, tooltipActive: true, GameStrings.Get("GLUE_TOOLTIP_HEADER_TWIST_CARDS"), GameStrings.Get("GLUE_TOOLTIP_DESCRIPTION_TWIST_CARDS"));
			List<int> featuredCards = CollectionManager.GetFeaturedCards();
			if (featuredCards.Any())
			{
				SetFilterItem updatedCardsItem = m_setFilterTray.AddItemUsingTexture(GameStrings.Get("GLUE_COLLECTION_NEW_CARDS"), m_featuredCardsTexture, m_featuredCardsIconOffset, FeaturedCardsSetFilterCallback, null, featuredCards, PegasusShared.FormatType.FT_STANDARD);
				m_currentActiveFeaturedCardsEvent = GameDbf.Card.GetRecord(featuredCards.First()).FeaturedCardsEvent;
				StartCoroutine(SetIconFxIfFeaturedCardsEventNotSeen(updatedCardsItem, m_currentActiveFeaturedCardsEvent));
				StartCoroutine(SetFeaturedCardsSetFilterGlowIfNotSeen(m_currentActiveFeaturedCardsEvent));
			}
			PopulateSetFilters();
			m_setFilterTrayInitialized = true;
		}
		else if (!show)
		{
			ShowSets(new List<TAG_CARD_SET>(GameUtils.GetStandardSets()));
		}
		ShowAppropriateSetFilters();
		if (show)
		{
			m_manaTabManager.SetUpTabs();
		}
	}

	private void AddTwistSetFilters()
	{
		m_setFilterTray.AddHeader(GameStrings.Get("GLUE_COLLECTION_TWIST_SETS"), PegasusShared.FormatType.FT_TWIST, editModeOnly: true);
		TAG_CARD_SET[] twistSets = GameUtils.GetTwistSets();
		foreach (TAG_CARD_SET cardSetId in twistSets)
		{
			AddSetFilter(cardSetId, PegasusShared.FormatType.FT_TWIST, editModeOnly: true);
		}
	}

	private void AddStandardSetFilters()
	{
		m_setFilterTray.AddHeader(GameStrings.Get("GLUE_COLLECTION_STANDARD_SETS"), PegasusShared.FormatType.FT_STANDARD);
		AddSetFilters(isWild: false);
	}

	private void AddWildSetFilters()
	{
		m_setFilterTray.AddHeader(GameStrings.Get("GLUE_COLLECTION_WILD_SETS"), PegasusShared.FormatType.FT_WILD);
		AddSetFilters(isWild: true);
	}

	private void AddSetFilters(bool isWild)
	{
		foreach (TAG_CARD_SET cardSetId2 in from cardSetId in CollectionManager.Get().GetDisplayableCardSets()
			where GameUtils.IsWildCardSet(cardSetId) == isWild && !GameUtils.IsClassicCardSet(cardSetId) && (!GameUtils.IsLegacySet(cardSetId) || cardSetId == TAG_CARD_SET.LEGACY)
			orderby GameDbf.GetIndex().GetCardSet(cardSetId)?.ReleaseOrder ?? 0 descending
			select cardSetId)
		{
			AddSetFilter(cardSetId2);
		}
	}

	private void AddSetFilter(TAG_CARD_SET cardSet, PegasusShared.FormatType formatType = PegasusShared.FormatType.FT_UNKNOWN, bool editModeOnly = false)
	{
		List<TAG_CARD_SET> sets = new List<TAG_CARD_SET>();
		if (cardSet == TAG_CARD_SET.LEGACY)
		{
			sets.AddRange(GameUtils.GetLegacySets());
		}
		else
		{
			if (GameUtils.IsLegacySet(cardSet))
			{
				return;
			}
			sets.Add(cardSet);
		}
		string iconTextureAssetRef = null;
		UnityEngine.Vector2? iconOffset = null;
		CardSetDbfRecord cardSetRecord = GameDbf.GetIndex().GetCardSet(cardSet);
		if (cardSetRecord != null)
		{
			if (cardSetRecord.IsCoreCardSet)
			{
				iconTextureAssetRef = "Filter_Icons_Core.tif:effec2b862f39224bac756f4a498164a";
				iconOffset = SetRotationIcon.GetYearIconTextureOffset() / 2f;
			}
			else
			{
				iconTextureAssetRef = cardSetRecord.FilterIconTexture;
				iconOffset = new UnityEngine.Vector2((float)cardSetRecord.FilterIconOffsetX, (float)cardSetRecord.FilterIconOffsetY);
			}
		}
		PegasusShared.FormatType format = ((formatType == PegasusShared.FormatType.FT_UNKNOWN) ? GameUtils.GetCardSetFormat(cardSet) : formatType);
		m_setFilterTray.AddItem(GameStrings.GetCardSetNameShortened(cardSet), iconTextureAssetRef, iconOffset, SetFilterCallback, sets, format, isAllStandard: false, editModeOnly);
	}

	public void PopulateSetFilters(bool shouldReset = false)
	{
		if (shouldReset)
		{
			m_setFilterTray.RemoveAllItems();
		}
		AddStandardSetFilters();
		AddWildSetFilters();
		AddTwistSetFilters();
		if (CollectionManager.Get().GetDisplayableCardSets().Contains(TAG_CARD_SET.SLUSH))
		{
			AddSetFilter(TAG_CARD_SET.SLUSH);
		}
		if (Options.GetInRankedPlayMode())
		{
			if (!m_setFilterTray.SelectFirstItemWithFormat(Options.GetFormatType()))
			{
				m_setFilterTray.SelectFirstItem();
			}
		}
		else
		{
			m_setFilterTray.SelectFirstItem();
		}
	}

	private long GetLastSeenFeaturedCardsEvent(GameSaveKeySubkeyId gameSaveSubkeyId)
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.COLLECTION_MANAGER, gameSaveSubkeyId, out List<long> lastSeenFeaturedCardsEventList);
		long lastSeenFeaturedCardsEventId = 0L;
		if (lastSeenFeaturedCardsEventList != null && lastSeenFeaturedCardsEventList.Any())
		{
			lastSeenFeaturedCardsEventId = lastSeenFeaturedCardsEventList.First();
		}
		return lastSeenFeaturedCardsEventId;
	}

	private IEnumerator SetIconFxIfFeaturedCardsEventNotSeen(SetFilterItem setFilterItem, EventTimingType currentActiveFeaturedCardsEvent)
	{
		while (!m_isReady)
		{
			yield return null;
		}
		long lastSeenFeaturedCardsEventId = GetLastSeenFeaturedCardsEvent(GameSaveKeySubkeyId.COLLECTION_MANAGER_LAST_SEEN_FEATURED_CARDS_EVENT_ITEM);
		long currentActiveEventId = EventTimingManager.Get().GetEventIdFromEventName(currentActiveFeaturedCardsEvent);
		if (currentActiveEventId != -1 && currentActiveEventId != lastSeenFeaturedCardsEventId)
		{
			setFilterItem.SetIconFxActive(active: true);
		}
	}

	private IEnumerator SetFeaturedCardsSetFilterGlowIfNotSeen(EventTimingType currentActiveFeaturedCardsEvent)
	{
		while (!m_isReady)
		{
			yield return null;
		}
		long lastSeenFeaturedCardsEventId = GetLastSeenFeaturedCardsEvent(GameSaveKeySubkeyId.COLLECTION_MANAGER_LAST_SEEN_FEATURED_CARDS_EVENT_BUTTON);
		long currentActiveEventId = EventTimingManager.Get().GetEventIdFromEventName(currentActiveFeaturedCardsEvent);
		if (currentActiveEventId != -1 && currentActiveEventId != lastSeenFeaturedCardsEventId)
		{
			m_setFilterTray.SetFilterButtonGlowActive(active: true);
			if (m_filterButtonGlow != null)
			{
				m_filterButtonGlow.SetActive(value: true);
			}
		}
	}

	private void SetLastSeenFeaturedCardsEvent(EventTimingType currentActiveFeaturedCardsEvent, GameSaveKeySubkeyId subkeyId)
	{
		if (currentActiveFeaturedCardsEvent != EventTimingType.UNKNOWN)
		{
			long currentActiveEventId = EventTimingManager.Get().GetEventIdFromEventName(currentActiveFeaturedCardsEvent);
			if (currentActiveEventId != -1 && GetLastSeenFeaturedCardsEvent(subkeyId) != currentActiveEventId)
			{
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.COLLECTION_MANAGER, subkeyId, currentActiveEventId));
			}
		}
	}

	protected override void OnSearchDeactivated(string oldSearchText, string newSearchText)
	{
		OnSearchDeactivated_Internal(oldSearchText, newSearchText, updateManaFilterToMatchSearchText: true);
	}

	private void OnSearchDeactivated_Internal(string oldSearchText, string newSearchText, bool updateManaFilterToMatchSearchText)
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
		if (!m_craftingTray.IsShown() && newSearchText.ToLower() == GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MISSING"))
		{
			if (m_currentViewMode == CollectionUtils.ViewMode.CARDS)
			{
				ShowCraftingTray(null, null, null, null, null, updatePage: false);
			}
			else
			{
				(m_craftingTray as CraftingTray).EnableCraftingInBackground();
				m_searchTriggeredCraftingInBackground = true;
			}
			m_searchTriggeredCrafting = true;
		}
		else if (newSearchText.ToLower() != GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MISSING"))
		{
			ResetFilterSettingsFromSearch();
		}
		NotifyFilterUpdate(m_searchFilterListeners, !string.IsNullOrEmpty(newSearchText), newSearchText);
		if (updateManaFilterToMatchSearchText)
		{
			UpdateManaFilterToMatchSearchText(newSearchText, transitionPage: false);
		}
		m_pageManager.ChangeSearchTextFilter(newSearchText, base.OnSearchFilterComplete, null);
	}

	protected override void OnSearchCleared(bool transitionPage)
	{
		ResetFilterSettingsFromSearch();
		NotifyFilterUpdate(m_searchFilterListeners, active: false, "");
		m_pageManager.ChangeSearchTextFilter("", transitionPage);
		if (m_manaFilterIsFromSearchText)
		{
			m_manaTabManager.ClearFilter();
		}
		base.OnSearchCleared(transitionPage);
	}

	private void ResetFilterSettingsFromSearch()
	{
		if (m_searchTriggeredCrafting)
		{
			m_viewModeHidingCraftingTray = false;
			if (m_craftingTray.IsShown())
			{
				m_craftingTray.Hide();
			}
			else
			{
				(m_craftingTray as CraftingTray).EnableCraftingInBackground(enable: false);
			}
		}
		m_searchTriggeredCrafting = false;
		m_searchTriggeredCraftingInBackground = false;
	}

	public void ShowTavernBrawlDeck(long deckID)
	{
		CollectionDeckTray.Get().GetDecksContent().SetEditingTraySection(0);
		CollectionDeckTray.Get().SetTrayMode(DeckTray.DeckContentTypes.Decks);
		RequestContentsToShowDeck(deckID);
	}

	public void ShowDuelsDeckHeader()
	{
		CollectionDeckTray.Get().GetDecksContent().SetEditingTraySection(0);
		CollectionDeckTray.Get().GetDecksContent().GetEditingTraySection()
			.m_deckBox.HideBanner();
	}

	private void DoEnterCollectionManagerEvents()
	{
		if (CollectionManager.Get().HasVisitedCollection() || IsSpecialOneDeckMode())
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
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.COLLECTION);
		}
		StartCoroutine(SetBookToOpen());
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
		{
			StartCoroutine(ShowCollectionTipsIfNeeded());
		}
	}

	private IEnumerator OpenBookWhenReady()
	{
		while (CollectionManager.Get().IsWaitingForBoxTransition())
		{
			yield return null;
		}
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
		{
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.COLLECTION);
		}
		m_pageManager.OnBookOpening();
		StartCoroutine(DoBookOpeningAnimations());
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
		{
			StartCoroutine(ShowCollectionTipsIfNeeded());
		}
	}

	private void ShowCraftingTipIfNeeded()
	{
		if (!Options.Get().GetBool(Option.TIP_CRAFTING_UNLOCKED, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("CollectionManagerDisplay.ShowCraftingTipIfNeeded"))
		{
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_DISENCHANT_31"), "VO_INNKEEPER_DISENCHANT_31.prefab:4a0246488dc2d8146b1db88de5c603ff");
			Options.Get().SetBool(Option.TIP_CRAFTING_UNLOCKED, val: true);
		}
	}

	private Vector3 GetNewDeckPosition()
	{
		Vector3 offset = (UniversalInputManager.UsePhoneUI ? new Vector3(25.7f, 2.6f, 0f) : new Vector3(17.5f, 0f, 0f));
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		if (deckTray != null)
		{
			return deckTray.GetDecksContent().GetNewDeckButtonPosition() - offset;
		}
		return new Vector3(0f, 0f, 0f);
	}

	private Vector3 GetLastDeckPosition()
	{
		Vector3 offset = (UniversalInputManager.UsePhoneUI ? new Vector3(15.8f, 0f, 6f) : new Vector3(9.6f, 0f, 3f));
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		if (deckTray != null)
		{
			return deckTray.GetDecksContent().GetLastUsedTraySection().transform.position - offset;
		}
		return new Vector3(0f, 0f, 0f);
	}

	private Vector3 GetMiddleDeckPosition()
	{
		int deckOffset = 4;
		Vector3 offset = (UniversalInputManager.UsePhoneUI ? new Vector3(15.8f, 0f, 6f) : new Vector3(9.6f, 0f, 3f));
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		if (deckTray != null)
		{
			return deckTray.GetDecksContent().GetTraySection(deckOffset).transform.position - offset;
		}
		return new Vector3(0f, 0f, 0f);
	}

	private void ShowSetRotationNewDeckIndicator(float f)
	{
		string tipString = "";
		Vector3 newDeckPosition;
		if (CollectionManager.Get().GetNumberOfWildDecks() >= 27)
		{
			tipString = GameStrings.Get("GLUE_COLLECTION_TUTORIAL15");
			newDeckPosition = GetMiddleDeckPosition();
		}
		else
		{
			if (CollectionManager.Get().GetNumberOfWildDecks() <= 0)
			{
				return;
			}
			if (CollectionManager.Get().GetNumberOfStandardDecks() > 0)
			{
				tipString = GameStrings.Get("GLUE_COLLECTION_TUTORIAL14");
				newDeckPosition = GetLastDeckPosition();
			}
			else
			{
				tipString = GameStrings.Get("GLUE_COLLECTION_TUTORIAL10");
				CollectionDeckTray.Get().GetDecksContent().m_newDeckButton.m_highlightState.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
				newDeckPosition = GetNewDeckPosition();
			}
		}
		m_createDeckNotification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS, newDeckPosition, m_editDeckTutorialBone.localScale, tipString);
		if (m_createDeckNotification != null)
		{
			m_createDeckNotification.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
			m_createDeckNotification.PulseReminderEveryXSeconds(3f);
		}
	}

	public IEnumerator ShowCollectionTipsIfNeeded()
	{
		while (CollectionManager.Get().IsWaitingForBoxTransition())
		{
			yield return null;
		}
		int deckCount = CollectionManager.Get().GetDecks(DeckType.NORMAL_DECK).Count;
		if (UserAttentionManager.CanShowAttentionGrabber(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS, "CollectionManagerDisplay.ShowCollectionTipsIfNeeded:ShowSetRotationTutorial") && CollectionManager.Get().ShouldShowWildToStandardTutorial())
		{
			CollectionDeckTray deckTray = CollectionDeckTray.Get();
			while (deckTray.IsUpdatingTrayMode() || !deckTray.GetDecksContent().IsDoneEntering())
			{
				yield return null;
			}
			if (deckCount >= 27)
			{
				NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS, GameStrings.Get("GLUE_COLLECTION_TUTORIAL11"), "VO_INNKEEPER_Male_Dwarf_FULL_DECKS_06.prefab:21adedb0a5456c24da1b2918c3d04e5a");
				ShowSetRotationNewDeckIndicator(0f);
			}
			else if (deckCount > (int)m_onscreenDecks)
			{
				deckTray.m_scrollbar.SetScroll(1f, ShowSetRotationNewDeckIndicator, iTween.EaseType.easeOutBounce, 0.75f, blockInputWhileScrolling: true);
			}
			else
			{
				ShowSetRotationNewDeckIndicator(0f);
			}
			yield break;
		}
		if (GameModeUtils.HasPlayedAPracticeMatch())
		{
			Options.Get().SetBool(Option.HAS_SEEN_COLLECTIONMANAGER_AFTER_PRACTICE, val: true);
		}
		if (!Options.Get().GetBool(Option.HAS_SEEN_COLLECTIONMANAGER, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("UserAttentionManager.CanShowAttentionGrabber:" + Option.HAS_SEEN_COLLECTIONMANAGER))
		{
			NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, GameStrings.Get("VO_INNKEEPER_CM_WELCOME"), "VO_INNKEEPER_Male_Dwarf_CM_WELCOME_23.prefab:c8afdeaaf2189eb42aad9d29f6a97994");
			Options.Get().SetBool(Option.HAS_SEEN_COLLECTIONMANAGER, val: true);
			yield return new WaitForSeconds(3.5f);
		}
		else
		{
			yield return new WaitForSeconds(1f);
		}
		bool num = UserAttentionManager.CanShowAttentionGrabber("CollectionManagerDisplay.ShowCollectionTipsIfNeeded:" + Option.HAS_STARTED_A_DECK);
		bool hasUserStartedAnyDeck = Options.Get().GetBool(Option.HAS_STARTED_A_DECK, defaultVal: false);
		if (num && !hasUserStartedAnyDeck && deckCount > 0)
		{
			m_deckHelpPopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_editDeckTutorialBone.position, m_editDeckTutorialBone.localScale, GameStrings.Get("GLUE_COLLECTION_TUTORIAL07"));
			m_deckHelpPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
			m_deckHelpPopup.PulseReminderEveryXSeconds(3f);
		}
	}

	private void ShowDeckTemplateTipsIfNeeded()
	{
		CollectionDeckTray collectionDeckTray = CollectionDeckTray.Get();
		bool deckHelpPopupShowing = m_deckHelpPopup != null && m_deckHelpPopup.gameObject != null;
		Notification deckTrayHelpPopup = collectionDeckTray.GetCardsContent().GetDeckHelpPopup();
		bool editingTipsShowing = deckTrayHelpPopup != null && deckTrayHelpPopup.gameObject != null;
		bool createDeckNotificationShowing = m_createDeckNotification != null && m_createDeckNotification.gameObject != null;
		bool isSideboardTrayOpen = collectionDeckTray.IsSideboardOpen;
		bool isSideboardPopupShowing = collectionDeckTray.IsShowingSideboardPopup;
		bool shouldHide = (m_craftingTray != null && m_craftingTray.IsShown()) || CraftingManager.GetIsInCraftingMode() || DeckHelper.Get().IsActive() || deckHelpPopupShowing || editingTipsShowing || createDeckNotificationShowing || collectionDeckTray.GetDecksContent().IsShowingDeckOptions || isSideboardTrayOpen || isSideboardPopupShowing;
		CollectionDeckSlot invalidSlot = CollectionDeckTray.Get().GetCardsContent().FindInvalidSlot();
		if (invalidSlot != null && !shouldHide)
		{
			if (m_showingDeckTemplateTips || CollectionDeckTray.Get().GetCurrentContentType() != DeckTray.DeckContentTypes.Cards || !CollectionDeckTray.Get().GetCardsContent().HasFinishedEntering())
			{
				return;
			}
			string tipText;
			if (invalidSlot.Owned)
			{
				if (invalidSlot.Owned && Options.Get().GetBool(Option.HAS_SEEN_INVALID_ROTATED_CARD))
				{
					return;
				}
				EntityDef entityDef = DefLoader.Get().GetEntityDef(invalidSlot.CardID);
				CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
				tipText = ((entityDef == null || deck == null || !GameUtils.IsBanned(deck, entityDef)) ? GameStrings.Get("GLUE_COLLECTION_TUTORIAL_REPLACE_WILD_CARDS_NPR") : GameStrings.Get("GLUE_COLLECTION_DECK_HELPER_REPLACE_BANNED_CARD"));
			}
			else
			{
				if (Options.Get().GetBool(Option.HAS_SEEN_DECK_TEMPLATE_GHOST_CARD) || !UserAttentionManager.CanShowAttentionGrabber("CollectionManagerDisplay.ShowDeckTemplateTipsIfNeeded:" + Option.HAS_SEEN_DECK_TEMPLATE_GHOST_CARD))
				{
					return;
				}
				if (m_currentViewMode == CollectionUtils.ViewMode.DECK_TEMPLATE)
				{
					if ((bool)UniversalInputManager.UsePhoneUI)
					{
						invalidSlot = m_deckTemplatePickerPhone.m_phoneTray.GetCardsContent().FindInvalidSlot();
						if (invalidSlot == null)
						{
							Debug.LogError("Phone Template Tray and CollectionDeckTray mismatch. Missing invalid card on Template.");
							return;
						}
					}
					tipText = GameStrings.Get("GLUE_COLLECTION_TUTORIAL_TEMPLATE_REPLACE_1");
					if (m_deckTemplateTipWaitTime < 0.5f)
					{
						m_deckTemplateTipWaitTime += Time.deltaTime;
						return;
					}
				}
				else
				{
					tipText = GameStrings.Get("GLUE_COLLECTION_TUTORIAL_TEMPLATE_REPLACE_2");
					if (m_deckTemplateTipWaitTime < 1f)
					{
						m_deckTemplateTipWaitTime += Time.deltaTime;
						return;
					}
				}
			}
			DeckTrayDeckTileVisual invalidTile = CollectionDeckTray.Get().GetCardsContent().GetCardTileVisual(invalidSlot.Index);
			if (invalidTile == null)
			{
				return;
			}
			float invalidTipMaxHeight = -60f;
			Vector3 invalidTipPosition = OverlayUI.Get().GetRelativePosition(invalidTile.transform.position, Box.Get().m_Camera.GetComponent<Camera>(), OverlayUI.Get().m_heightScale.m_Center);
			Vector3 invalidTipScale;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				invalidTipPosition.x -= 95.395f;
				invalidTipPosition.z -= 0.25f;
				invalidTipScale = 27.5f * Vector3.one;
			}
			else
			{
				invalidTipPosition.x -= 50.5f;
				invalidTipPosition.z -= 0.25f;
				invalidTipScale = NotificationManager.NOTIFICATITON_WORLD_SCALE;
			}
			if (invalidTipPosition.z < invalidTipMaxHeight)
			{
				invalidTipPosition.z = invalidTipMaxHeight;
			}
			if (m_currentViewMode == CollectionUtils.ViewMode.DECK_TEMPLATE)
			{
				m_deckTemplateCardReplacePopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, invalidTipPosition, invalidTipScale, tipText, convertLegacyPosition: false);
				if (m_deckTemplateCardReplacePopup != null)
				{
					m_deckTemplateCardReplacePopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
					NotificationManager.Get().DestroyNotification(m_deckTemplateCardReplacePopup, 3.5f);
				}
			}
			else
			{
				m_deckTemplateCardReplacePopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS, invalidTipPosition, invalidTipScale, tipText, convertLegacyPosition: false);
				if (m_deckTemplateCardReplacePopup != null)
				{
					m_deckTemplateCardReplacePopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
					m_deckTemplateCardReplacePopup.PulseReminderEveryXSeconds(3f);
				}
			}
			m_deckTemplateTipWaitTime = 0f;
			m_showingDeckTemplateTips = true;
		}
		else
		{
			if (m_showingDeckTemplateTips)
			{
				NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL_TEMPLATE_REPLACE_1"));
				NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL_TEMPLATE_REPLACE_2"));
				NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL_REPLACE_WILD_CARDS"));
				NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_TUTORIAL_REPLACE_WILD_CARDS_NPR"));
				NotificationManager.Get().DestroyNotificationWithText(GameStrings.Get("GLUE_COLLECTION_DECK_HELPER_REPLACE_BANNED_CARD"));
			}
			m_deckTemplateTipWaitTime = 0f;
			m_showingDeckTemplateTips = false;
		}
	}

	public void ShowCardBackTipsIfNeeded()
	{
		if (!m_shouldShowMultipleFavoriteCardBackTutorial || CollectionManager.Get().IsInEditMode() || CardBackManager.Get().GetNumCardBacksOwned() <= 3)
		{
			return;
		}
		if (m_multipleFavoriteCardBackTutorialBone == null)
		{
			Debug.LogWarning("No bone for multiple card back tutorial. Did you forget a connection in CollectionManagerDisplay?");
			return;
		}
		string tipString = GameStrings.Get("GLUE_COLLECTION_TUTORIAL_MULTIPLE_FAVORITE_CARD_BACKS");
		m_multipleFavoriteCardBacksNotification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS, m_multipleFavoriteCardBackTutorialBone, tipString);
		if (m_multipleFavoriteCardBacksNotification != null)
		{
			m_multipleFavoriteCardBacksNotification.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
			m_multipleFavoriteCardBacksNotification.PulseReminderEveryXSeconds(3f);
			m_shouldShowMultipleFavoriteCardBackTutorial = false;
			GameUtils.SetGSDFlag(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_HAS_SEEN_MULTIPLE_FAVORITE_CARD_BACKS, enableFlag: true);
		}
	}

	public void HideCardBackTips()
	{
		if (m_multipleFavoriteCardBacksNotification != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_multipleFavoriteCardBacksNotification);
		}
	}

	public void ShowHeroTipsIfNeeded()
	{
		if (!m_heroSkinClass.HasValue || !m_shouldShowMultipleFavoriteHeroTutorial || CollectionManager.Get().IsInEditMode() || CollectionManager.Get().GetCountOfOwnedHeroesForClass(m_heroSkinClass.Value) < 2)
		{
			return;
		}
		if (m_multipleFavoriteHeroTutorialBone == null)
		{
			Debug.LogWarning("No bone for multiple favorite heroes tutorial. Did you forget a connection in CollectionManagerDisplay?");
			return;
		}
		string tipString = GameStrings.Get("GLUE_COLLECTION_TUTORIAL_MULTIPLE_FAVORITE_HEROES");
		m_multipleFavoriteHeroesNotification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.SET_ROTATION_CM_TUTORIALS, m_multipleFavoriteHeroTutorialBone, tipString);
		if (m_multipleFavoriteHeroesNotification != null)
		{
			m_multipleFavoriteHeroesNotification.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
			m_multipleFavoriteHeroesNotification.PulseReminderEveryXSeconds(3f);
			m_shouldShowMultipleFavoriteHeroTutorial = false;
			GameUtils.SetGSDFlag(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_HAS_SEEN_MULTIPLE_FAVORITE_HEROES, enableFlag: true);
		}
	}

	public void HideHeroTips()
	{
		if (m_multipleFavoriteHeroesNotification != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_multipleFavoriteHeroesNotification);
		}
	}

	public void HideAllCosmeticTips()
	{
		HideCardBackTips();
		HideHeroTips();
	}

	protected override void OnSwitchViewModeResponse(bool triggerResponse, CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode newMode, CollectionUtils.ViewModeData userdata)
	{
		base.OnSwitchViewModeResponse(triggerResponse, prevMode, newMode, userdata);
		EnableSetAndManaFiltersByViewMode(newMode, m_currentViewSubmode);
		EnableCraftingByViewMode(newMode, m_currentViewSubmode);
		EnableTutorialsByViewMode(newMode);
	}

	private void OnSwitchViewSubmodeResponse(CollectionUtils.ViewSubmode newSubmode)
	{
		EnableSetAndManaFiltersByViewMode(m_currentViewMode, newSubmode);
		EnableCraftingByViewMode(m_currentViewMode, newSubmode);
		if (m_filterButton != null)
		{
			bool shouldShowFilterButton = ShouldSeeFilterButton() && newSubmode != CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES;
			m_filterButton.gameObject.SetActive(shouldShowFilterButton);
		}
	}

	private void EnableSetAndManaFiltersByViewMode(CollectionUtils.ViewMode viewMode, CollectionUtils.ViewSubmode viewSubmode)
	{
		bool isZilliaxViewSubmode = viewSubmode == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES;
		bool enabled = viewMode == CollectionUtils.ViewMode.CARDS && !isZilliaxViewSubmode;
		m_manaTabManager.Enabled = enabled;
		if (m_setFilterTray != null)
		{
			m_setFilterTray.SetButtonEnabled(enabled);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_setFilterTray.gameObject.SetActive(enabled);
			}
		}
		m_search.SetEnabled(!isZilliaxViewSubmode);
	}

	private void EnableCraftingByViewMode(CollectionUtils.ViewMode viewMode, CollectionUtils.ViewSubmode viewSubmode)
	{
		bool shouldEnable = (viewMode == CollectionUtils.ViewMode.CARDS || viewMode == CollectionUtils.ViewMode.MASS_DISENCHANT) && viewSubmode != CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES;
		m_craftingModeButton.Enable(shouldEnable);
		bool craftingEnabledButTrayHidden = m_viewModeHidingCraftingTray || m_searchTriggeredCraftingInBackground;
		if (!shouldEnable)
		{
			CraftingTray collectionCraftingTray = m_craftingTray as CraftingTray;
			if (collectionCraftingTray.IsShown())
			{
				collectionCraftingTray.Hide(updatePage: false);
				m_viewModeHidingCraftingTray = true;
			}
		}
		else if (shouldEnable && craftingEnabledButTrayHidden)
		{
			ShowCraftingTray(null, null, null, null, null, updatePage: false);
			m_viewModeHidingCraftingTray = false;
			m_searchTriggeredCraftingInBackground = false;
		}
	}

	public void EnableTutorialsByViewMode(CollectionUtils.ViewMode viewMode)
	{
		HideAllCosmeticTips();
		switch (viewMode)
		{
		case CollectionUtils.ViewMode.CARD_BACKS:
			ShowCardBackTipsIfNeeded();
			break;
		case CollectionUtils.ViewMode.HERO_SKINS:
			ShowHeroTipsIfNeeded();
			break;
		}
	}

	private void OnSetFilterButtonPressed()
	{
		SetLastSeenFeaturedCardsEvent(m_currentActiveFeaturedCardsEvent, GameSaveKeySubkeyId.COLLECTION_MANAGER_LAST_SEEN_FEATURED_CARDS_EVENT_BUTTON);
		m_setFilterTray.SetFilterButtonGlowActive(active: false);
	}

	private void OnPhoneFilterButtonPressed()
	{
		SetLastSeenFeaturedCardsEvent(m_currentActiveFeaturedCardsEvent, GameSaveKeySubkeyId.COLLECTION_MANAGER_LAST_SEEN_FEATURED_CARDS_EVENT_BUTTON);
		m_filterButtonGlow.SetActive(value: false);
	}

	protected override CraftingTrayBase GetCraftingTrayComponent(GameObject go)
	{
		return go.GetComponent<CraftingTray>();
	}

	protected override void OnCraftingTrayLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		base.OnCraftingTrayLoaded(assetRef, go, callbackData);
		m_pageManager.UpdateMassDisenchant();
	}

	public override void ShowCraftingTray(bool? includeUncraftable = null, bool? normalOwned = null, bool? normalMissing = null, bool? premiumOwned = null, bool? premiumMissing = null, bool updatePage = true)
	{
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		if (deckTray != null)
		{
			DeckTrayDeckListContent decksContent = deckTray.GetDecksContent();
			if (decksContent != null)
			{
				decksContent.CancelRenameEditingDeck();
			}
		}
		HideDeckHelpPopup();
		base.ShowCraftingTray(includeUncraftable, normalOwned, normalMissing, premiumOwned, premiumMissing, updatePage);
		ShowAppropriateSetFilters();
	}

	public override void HideCraftingTray()
	{
		base.HideCraftingTray();
		ShowAppropriateSetFilters();
	}

	public void ShowConvertTutorial(UserAttentionBlocker blocker)
	{
		if (UserAttentionManager.CanShowAttentionGrabber(blocker, "CollectionManagerDisplay.ShowConvertTutorial"))
		{
			m_showConvertTutorialCoroutine = ShowConvertTutorialCoroutine(blocker);
			StartCoroutine(m_showConvertTutorialCoroutine);
		}
	}

	private IEnumerator ShowConvertTutorialCoroutine(UserAttentionBlocker blocker)
	{
		if (m_createDeckNotification != null)
		{
			NotificationManager.Get().DestroyNotification(m_createDeckNotification, 0.25f);
		}
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		while (deckTray.IsUpdatingTrayMode() || !deckTray.GetDecksContent().IsDoneEntering())
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		if (ViewModeHasVisibleDeckList())
		{
			m_convertTutorialPopup = NotificationManager.Get().CreatePopupText(blocker, m_convertDeckTutorialBone.position, m_convertDeckTutorialBone.localScale, GameStrings.Get("GLUE_COLLECTION_TUTORIAL12"));
			if (m_convertTutorialPopup != null)
			{
				m_convertTutorialPopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
				m_convertTutorialPopup.PulseReminderEveryXSeconds(3f);
			}
			m_showConvertTutorialCoroutine = null;
		}
	}

	public void HideConvertTutorial()
	{
		if (m_showConvertTutorialCoroutine != null)
		{
			StopCoroutine(m_showConvertTutorialCoroutine);
			m_showConvertTutorialCoroutine = null;
		}
		if (m_convertTutorialPopup != null)
		{
			NotificationManager.Get().DestroyNotification(m_convertTutorialPopup, 0.25f);
		}
	}

	public void ShowSetFilterTutorial(UserAttentionBlocker blocker)
	{
		if (UserAttentionManager.CanShowAttentionGrabber(blocker, "CollectionManagerDisplay.ShowSetFilterTutorial"))
		{
			m_showSetFilterTutorialCoroutine = ShowSetFilterTutorialCoroutine(blocker);
			StartCoroutine(m_showSetFilterTutorialCoroutine);
		}
	}

	private IEnumerator ShowSetFilterTutorialCoroutine(UserAttentionBlocker blocker)
	{
		if (m_setFilterTutorialPopup != null)
		{
			NotificationManager.Get().DestroyNotification(m_setFilterTutorialPopup, 0f);
		}
		m_setFilterTutorialPopup = NotificationManager.Get().CreatePopupText(blocker, m_setFilterTutorialBone.position, m_setFilterTutorialBone.localScale, GameStrings.Get("GLUE_COLLECTION_TUTORIAL17"));
		if (m_setFilterTutorialPopup != null)
		{
			m_setFilterTutorialPopup.ShowPopUpArrow(UniversalInputManager.UsePhoneUI ? Notification.PopUpArrowDirection.Up : Notification.PopUpArrowDirection.LeftDown);
			m_setFilterTutorialPopup.PulseReminderEveryXSeconds(3f);
		}
		yield return new WaitForSeconds(6f);
		HideSetFilterTutorial();
	}

	public void HideSetFilterTutorial()
	{
		if (m_showSetFilterTutorialCoroutine != null)
		{
			StopCoroutine(m_showSetFilterTutorialCoroutine);
			m_showSetFilterTutorialCoroutine = null;
		}
		if (m_setFilterTutorialPopup != null)
		{
			NotificationManager.Get().DestroyNotification(m_setFilterTutorialPopup, 0.25f);
		}
	}

	public void SetRuneLockedCheckboxVisible(bool visible)
	{
		m_runeLockedCheckboxContainer.SetActive(visible);
		m_runelockedCheckbox.SetChecked(CollectionPageManager.IsShowingLockedRuneCards);
	}

	public void ShowStandardInfoTutorial(UserAttentionBlocker blocker)
	{
		NotificationManager.Get().CreateInnkeeperQuote(blocker, GameStrings.Get("GLUE_COLLECTION_TUTORIAL13"), "VO_INNKEEPER_Male_Dwarf_STANDARD_WELCOME3_14.prefab:51e1d835435b64542b9a77944e00cc19");
	}

	public void CheckClipboardAndPromptPlayerToPaste()
	{
		if (!CheckIfClipboardNotificationHasBeenShown())
		{
			return;
		}
		if (!CheckClipboardAndGetValidityMessaging(out var message))
		{
			if (message != string.Empty)
			{
				CollectionInputMgr.AlertPlayerOnInvalidDeckPaste(message);
			}
			return;
		}
		string popupBodyMessage = GameStrings.Get("GLUE_COLLECTION_DECK_VALID_PASTE_BODY");
		string popupBodyHeader = GameStrings.Get("GLUE_COLLECTION_DECK_VALID_PASTE_HEADER");
		if (CollectionManager.Get().IsInEditMode() && CollectionManager.Get().GetEditedDeck().GetTotalCardCount() > 0)
		{
			popupBodyMessage = GameStrings.Get("GLUE_COLLECTION_DECK_OVERWRITE_BODY");
			popupBodyHeader = GameStrings.Get("GLUE_COLLECTION_DECK_OVERWRITE_HEADER");
		}
		AlertPopup.PopupInfo popup = new AlertPopup.PopupInfo
		{
			m_headerText = popupBodyHeader,
			m_text = popupBodyMessage,
			m_cancelText = GameStrings.Get("GLUE_COLLECTION_DECK_SAVE_ANYWAY"),
			m_confirmText = GameStrings.Get("GLUE_COLLECTION_DECK_FINISH_FOR_ME"),
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_id = PASTE_DECK_POPUP_ID,
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CANCEL)
				{
					RejectDeckFromClipboard();
				}
				else
				{
					CreateDeckFromClipboard(m_cachedShareableDeck);
				}
			}
		};
		DialogManager.Get().ShowPopup(popup);
	}

	private bool CheckIfClipboardNotificationHasBeenShown()
	{
		if (PlatformSettings.OS == OSCategory.iOS && !Options.Get().GetBool(Option.HAS_SEEN_CLIPBOARD_NOTIFICATION, defaultVal: false))
		{
			if (DialogManager.Get().ShowingDialog())
			{
				return false;
			}
			string popupBodyHeader = GameStrings.Get("GLUE_COLLECTION_DECK_CLIPBOARD_ACCESS_HEADER");
			string popupBodyMessage = GameStrings.Get("GLUE_COLLECTION_DECK_CLIPBOARD_ACCESS_BODY");
			AlertPopup.PopupInfo popup = new AlertPopup.PopupInfo
			{
				m_headerText = popupBodyHeader,
				m_text = popupBodyMessage,
				m_showAlertIcon = false,
				m_responseCallback = delegate
				{
					Options.Get().SetBool(Option.HAS_SEEN_CLIPBOARD_NOTIFICATION, val: true);
				}
			};
			DialogManager.Get().ShowPopup(popup);
			return false;
		}
		return true;
	}

	public void PasteFromClipboardIfValidOrShowStatusMessage()
	{
		if (CheckIfClipboardNotificationHasBeenShown())
		{
			if (!CheckClipboardAndGetValidityMessaging(out var message))
			{
				UIStatus.Get().AddInfo(message);
				return;
			}
			ClipboardUtils.CopyToClipboard(string.Empty);
			CreateDeckFromClipboard(m_cachedShareableDeck);
		}
	}

	private bool CheckClipboardAndGetValidityMessaging(out string message)
	{
		message = string.Empty;
		ShareableDeck deckInClipboard = ShareableDeck.DeserializeFromClipboard();
		if (deckInClipboard == null)
		{
			return false;
		}
		if (DialogManager.Get().ShowingDialog())
		{
			if ((m_cachedShareableDeck != null && m_cachedShareableDeck.Equals(deckInClipboard)) || !CanPasteShareableDeck(deckInClipboard))
			{
				return false;
			}
			DialogManager.Get().ClearAllImmediately();
		}
		m_cachedShareableDeck = deckInClipboard;
		return CanPasteShareableDeck(m_cachedShareableDeck, out message);
	}

	private bool CanPasteShareableDeck(ShareableDeck shareableDeck)
	{
		string alertMessage;
		return CanPasteShareableDeck(shareableDeck, out alertMessage);
	}

	private bool CanPasteShareableDeck(ShareableDeck shareableDeck, out string alertMessage)
	{
		alertMessage = string.Empty;
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER && !CollectionManager.Get().IsInEditMode() && !CollectionDeckTray.Get().m_decksContent.CanShowNewDeckButton())
		{
			return false;
		}
		if (SceneMgr.Get().IsInTavernBrawlMode() && !TavernBrawlDisplay.Get().IsInDeckEditMode() && HeroPickerDisplay.Get() == null)
		{
			return false;
		}
		if (CraftingTray.Get().IsShown())
		{
			return false;
		}
		if (!CollectionManager.Get().ShouldAccountSeeStandardWild() && shareableDeck.FormatType == PegasusShared.FormatType.FT_WILD)
		{
			alertMessage = GameStrings.Get("GLUE_COLLECTION_DECK_WILD_NOT_UNLOCKED");
			return false;
		}
		string heroCardDesignerId = GameUtils.TranslateDbIdToCardId(shareableDeck.HeroCardDbId);
		if (string.IsNullOrEmpty(heroCardDesignerId))
		{
			return false;
		}
		List<TAG_CLASS> heroClasses = new List<TAG_CLASS>();
		DefLoader.Get().GetEntityDef(heroCardDesignerId).GetClasses(heroClasses);
		if (shareableDeck.FormatType == PegasusShared.FormatType.FT_TWIST)
		{
			if (!CollectionManager.Get().ShouldAccountSeeStandardWild())
			{
				alertMessage = GameStrings.Get("GLUE_COLLECTION_DECK_TWIST_NOT_UNLOCKED");
				return false;
			}
			if (!RankMgr.IsCurrentTwistSeasonActive())
			{
				alertMessage = GameStrings.Get("GLUE_COLLECTION_DECK_TWIST_NOT_ACTIVE");
				return false;
			}
			if (RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
			{
				alertMessage = GameStrings.Get("GLUE_COLLECTION_DECK_TWIST_INVALID_SEASON");
				return false;
			}
			if (RankMgr.IsClassLockedForTwist(heroClasses))
			{
				alertMessage = GameStrings.Get("GLUE_COLLECTION_DECK_TWIST_INVALID_CLASS");
				return false;
			}
		}
		if (shareableDeck.FormatType == PegasusShared.FormatType.FT_CLASSIC)
		{
			alertMessage = GameStrings.Get("GLUE_COLLECTION_DECK_CLASSIC_INVALID");
			return false;
		}
		bool isTavernBrawl = SceneMgr.Get().GetMode() == SceneMgr.Mode.TAVERN_BRAWL;
		ScenarioDbId scenarioId = ScenarioDbId.INVALID;
		if (isTavernBrawl)
		{
			scenarioId = (ScenarioDbId)TavernBrawlManager.Get().CurrentMission().missionId;
		}
		if (scenarioId != 0)
		{
			ScenarioDbfRecord scenarioIdRecord = GameDbf.Scenario.GetRecord((int)scenarioId);
			if (scenarioIdRecord != null)
			{
				foreach (ClassExclusionsDbfRecord classExclusion in scenarioIdRecord.ClassExclusions)
				{
					foreach (TAG_CLASS heroClass in heroClasses)
					{
						if (classExclusion.ClassId == (int)heroClass)
						{
							return false;
						}
					}
				}
			}
		}
		foreach (TAG_CLASS item in heroClasses)
		{
			if (!GameUtils.HasUnlockedClass(item))
			{
				alertMessage = GameStrings.Get("GLUE_COLLECTION_DECK_HERO_NOT_UNLOCKED");
				return false;
			}
		}
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER)
		{
			foreach (TAG_CLASS item2 in heroClasses)
			{
				if (!GameUtils.HasUnlockedClass(item2))
				{
					return false;
				}
			}
		}
		CollectionDeck deck = CollectionManager.Get().GetEditedDeck();
		if (CollectionManager.Get().IsInEditMode())
		{
			if (!IsValidHeroClassesForCollectionDeck(heroClasses, deck))
			{
				return false;
			}
			if (deck.GetShareableDeck().Equals(m_cachedShareableDeck))
			{
				return false;
			}
		}
		if (!isTavernBrawl && NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().ShouldPrevalidatePastedDeckCodes)
		{
			CollectionDeck tempCollectionDeck = new CollectionDeck();
			if (!tempCollectionDeck.FillFromShareableDeck(shareableDeck))
			{
				return false;
			}
			DeckRuleset deckRulesetToUse = CollectionManager.Get().GetDeckRuleset() ?? tempCollectionDeck.GetRuleset(null);
			if (deckRulesetToUse != null)
			{
				return deckRulesetToUse.IsDeckValid(tempCollectionDeck, CollectionDeck.DefaultIgnoreRules.ToArray());
			}
		}
		return true;
	}

	private void CreateDeckFromClipboard(ShareableDeck shareableDeck)
	{
		bool inCollectionManager = SceneMgr.Get().GetMode() == SceneMgr.Mode.COLLECTIONMANAGER;
		List<TAG_CLASS> heroClasses = new List<TAG_CLASS>();
		DefLoader.Get().GetEntityDef(shareableDeck.HeroCardDbId).GetClasses(heroClasses);
		if (heroClasses.Count == 0)
		{
			Debug.LogError($"CollectionManagerDisplay.CreateDeckFromClipboard(): no hero classes for hero card id; shareableDeck.HeroCardDbId={shareableDeck.HeroCardDbId}");
			return;
		}
		TAG_CLASS firstHeroClass = heroClasses[0];
		NetCache.CardDefinition favoriteHeroCardDef = CollectionManager.Get().GetRandomFavoriteHero(firstHeroClass, null);
		string heroCardDesignerId = ((favoriteHeroCardDef != null) ? favoriteHeroCardDef.Name : CollectionManager.GetVanillaHero(firstHeroClass));
		if (inCollectionManager)
		{
			PegasusShared.FormatType ft = Options.GetFormatType();
			if (ft == PegasusShared.FormatType.FT_UNKNOWN)
			{
				RankMgr.LogMessage("Options.GetFormatType() = FT_UNKOWN", "CreateDeckFromClipboard", "D:\\p4Workspace\\31.6.0\\Pegasus\\Client\\Assets\\Game\\CollectionManager\\CollectionManagerDisplay.cs", 3396);
				return;
			}
			CollectionManager.s_PreHeroPickerFormat = ft;
			Options.SetFormatType(shareableDeck.FormatType);
		}
		string deckName = null;
		if (!string.IsNullOrEmpty(shareableDeck.DeckName))
		{
			deckName = shareableDeck.DeckName;
		}
		if (!CollectionManager.Get().IsInEditMode())
		{
			if (!(CollectionDeckTray.Get() == null))
			{
				CollectionDeckTray.Get().GetDecksContent().CreateNewDeckFromUserSelection(firstHeroClass, heroCardDesignerId, deckName, DeckSourceType.DECK_SOURCE_TYPE_PASTED_DECK, shareableDeck.Serialize(includeComments: false));
				CollectionManager.Get().RegisterDeckCreatedListener(OnDeckCreatedFromClipboard);
				CollectionManager.Get().RemoveDeckCreatedListener(OnDeckCreatedByPlayer);
				if (HeroPickerDisplay.Get() != null && HeroPickerDisplay.Get().IsShown())
				{
					DeckPickerTrayDisplay.Get().SkipHeroSelectionAndCloseTray();
				}
			}
			return;
		}
		CollectionDeck editedDeck = CollectionManager.Get().GetEditedDeck();
		if (!IsValidHeroClassesForCollectionDeck(heroClasses, editedDeck))
		{
			AlertPopup.PopupInfo popup = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_COLLECTION_DECK_PASTE_TOOLTIP_HEADLINE"),
				m_text = GameStrings.Get("GLUE_COLLECTION_DECK_PASTE_INVALID_CLASS_BODY"),
				m_confirmText = GameStrings.Get("GLOBAL_OKAY"),
				m_showAlertIcon = true,
				m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM
			};
			DialogManager.Get().ShowPopup(popup);
		}
		else
		{
			OnDeckCreatedFromClipboard(editedDeck.ID, editedDeck.Name);
		}
	}

	private bool IsValidHeroClassesForCollectionDeck(List<TAG_CLASS> heroClasses, CollectionDeck deck)
	{
		if (heroClasses == null || deck == null)
		{
			return false;
		}
		List<TAG_CLASS> deckClasses = deck.GetClasses();
		foreach (TAG_CLASS heroClass in heroClasses)
		{
			if (!deckClasses.Contains(heroClass))
			{
				return false;
			}
		}
		return true;
	}

	private void OnDeckCreatedFromClipboard(long deckId, string name)
	{
		CollectionManager.Get().RemoveDeckCreatedListener(OnDeckCreatedFromClipboard);
		CollectionManager.Get().RegisterDeckCreatedListener(OnDeckCreatedByPlayer);
		bool deckWasPastedFromEditMode = CollectionManager.Get().IsInEditMode();
		if (GetViewMode() == CollectionUtils.ViewMode.DECK_TEMPLATE)
		{
			DeckTemplatePicker deckTemplatePicker = (UniversalInputManager.UsePhoneUI ? GetPhoneDeckTemplateTray() : m_pageManager.GetDeckTemplatePicker());
			if (deckTemplatePicker != null)
			{
				Navigation.RemoveHandler(deckTemplatePicker.OnNavigateBack);
			}
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				StartCoroutine(deckTemplatePicker.EnterDeckPhone());
			}
		}
		if (CollectionDeckTray.Get().GetCurrentContentType() != DeckTray.DeckContentTypes.Cards)
		{
			CollectionDeckTray.Get().RegisterModeSwitchedListener(OnCollectionDeckTrayModeSwitched);
			ShowDeck(deckId, !deckWasPastedFromEditMode, showDeckTemplatePage: false, CollectionUtils.ViewMode.CARDS);
		}
		else
		{
			ShowDeck(deckId, !deckWasPastedFromEditMode, showDeckTemplatePage: false, CollectionUtils.ViewMode.CARDS);
			OnCollectionDeckTrayModeSwitched();
		}
	}

	private void OnCollectionDeckTrayModeSwitched()
	{
		CollectionDeckTray.Get().UnregisterModeSwitchedListener(OnCollectionDeckTrayModeSwitched);
		if (m_cachedShareableDeck != null)
		{
			CollectionInputMgr.PasteDeckInEditModeFromShareableDeck(m_cachedShareableDeck);
		}
		else
		{
			CollectionInputMgr.PasteDeckFromClipboard();
		}
		ClipboardUtils.CopyToClipboard(string.Empty);
		m_cachedShareableDeck = null;
	}

	private void RejectDeckFromClipboard()
	{
		ClipboardUtils.CopyToClipboard(string.Empty);
		m_cachedShareableDeck = null;
	}

	public void SetHeroSkinClass(TAG_CLASS? newClass)
	{
		m_heroSkinClass = newClass;
	}

	public TAG_CLASS? GetHeroSkinClass()
	{
		return m_heroSkinClass;
	}

	public static bool ShouldShowDeckOptionsMenu()
	{
		return true;
	}

	public static bool ShouldShowDeckHeaderInfo()
	{
		return true;
	}

	public static bool IsSpecialOneDeckMode()
	{
		return SceneMgr.Get().IsInTavernBrawlMode();
	}

	private static bool ShouldSeeFilterButton()
	{
		return CollectionManager.Get().GetOwnedCards().Count > 0;
	}

	public static bool ShouldSeeCraftingButton()
	{
		return CollectionManager.Get().GetOwnedCards().Count > 0;
	}
}
