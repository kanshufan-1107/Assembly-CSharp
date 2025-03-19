using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Configuration;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.DungeonCrawl;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class AdventureDungeonCrawlDisplay : MonoBehaviour
{
	[Serializable]
	public class DungeonCrawlDisplayStyleOverride
	{
		public DungeonRunVisualStyle VisualStyle;

		public Material DungeonCrawlTrayMaterial;

		public Material PhoneDeckTrayMaterial;
	}

	public class PlayerHeroData
	{
		public delegate void DataChangedEventHandler();

		private readonly IDungeonCrawlData m_dungeonCrawlData;

		public List<TAG_CLASS> HeroClasses { get; private set; }

		public int HeroCardDbId { get; private set; }

		public string HeroCardId { get; private set; }

		public event DataChangedEventHandler OnHeroDataChanged;

		public PlayerHeroData(IDungeonCrawlData dungeonCrawlData)
		{
			m_dungeonCrawlData = dungeonCrawlData;
		}

		public void UpdateHeroDataFromHeroCardDbId(int heroCardDbId)
		{
			if (heroCardDbId == 0)
			{
				ClearHeroData();
				return;
			}
			HeroCardDbId = heroCardDbId;
			CardDbfRecord cardRecord = GameDbf.Card.GetRecord(heroCardDbId);
			if (cardRecord == null)
			{
				Debug.LogWarning($"AdventureDungeonCrawlDisplay.UpdateHeroDataFromHeroCardDbId: Unable to find hero for: heroCardDBId [{heroCardDbId}]");
				ClearHeroData();
				return;
			}
			HeroCardId = cardRecord.NoteMiniGuid;
			EntityDef entityDef = DefLoader.Get().GetEntityDef(heroCardDbId);
			if (entityDef == null)
			{
				Debug.LogWarning($"AdventureDungeonCrawlDisplay.UpdateHeroDataFromHeroCardDbId: No entity found for id: {heroCardDbId}");
				ClearHeroData();
				return;
			}
			HeroClasses = new List<TAG_CLASS>();
			entityDef.GetClasses(HeroClasses);
			if (HeroClasses == null || HeroClasses.Count < 1)
			{
				Debug.LogWarning($"AdventureDungeonCrawlDisplay.UpdateHeroDataFromHeroCardDbId: Unable to find classes for: heroCardDBId [{heroCardDbId}]");
				ClearHeroData();
			}
			else
			{
				this.OnHeroDataChanged?.Invoke();
			}
		}

		public void UpdateHeroDataFromClass(TAG_CLASS heroClass)
		{
			if (heroClass == TAG_CLASS.INVALID)
			{
				ClearHeroData();
				return;
			}
			HeroClasses = new List<TAG_CLASS> { heroClass };
			HeroCardId = AdventureUtils.GetHeroCardIdFromClassForDungeonCrawl(m_dungeonCrawlData, heroClass);
			HeroCardDbId = GameUtils.TranslateCardIdToDbId(HeroCardId);
			this.OnHeroDataChanged?.Invoke();
		}

		private void ClearHeroData()
		{
			HeroCardDbId = 0;
			HeroCardId = string.Empty;
			if (HeroClasses != null)
			{
				HeroClasses.Clear();
			}
			else
			{
				HeroClasses = new List<TAG_CLASS>();
			}
			this.OnHeroDataChanged?.Invoke();
		}
	}

	private enum DungeonRunLoadoutState
	{
		INVALID,
		HEROPOWER,
		TREASURE,
		DECKTEMPLATE,
		LOADOUTCOMPLETE
	}

	[CustomEditField(Sections = "UI")]
	public UberText m_AdventureTitle;

	[CustomEditField(Sections = "UI")]
	public UIBButton m_BackButton;

	[CustomEditField(Sections = "UI")]
	public AdventureDungeonCrawlDeckTray m_dungeonCrawlDeckTray;

	[CustomEditField(Sections = "UI")]
	public AsyncReference m_dungeonCrawlDeckSelectWidgetReference;

	[CustomEditField(Sections = "UI")]
	public AsyncReference m_dungeonCrawlPlayMatReference;

	[CustomEditField(Sections = "UI")]
	public AsyncReference m_heroClassIconsControllerReference;

	[CustomEditField(Sections = "UI")]
	public GameObject m_dungeonCrawlTray;

	[CustomEditField(Sections = "UI")]
	public DungeonCrawlBossKillCounter m_bossKillCounter;

	[CustomEditField(Sections = "UI")]
	public HighlightState m_backButtonHighlight;

	[CustomEditField(Sections = "UI")]
	public float m_RolloverTimeToHideBossHeroPowerTooltip = 0.35f;

	[CustomEditField(Sections = "UI")]
	public Material m_anomalyModeCardHighlightMaterial;

	[CustomEditField(Sections = "UI")]
	public float m_BigCardScale = 1f;

	[CustomEditField(Sections = "UI")]
	public AsyncReference m_retireButtonReference;

	[CustomEditField(Sections = "Animation")]
	public PlayMakerFSM m_HeroPowerPortraitPlayMaker;

	[CustomEditField(Sections = "Animation")]
	public string m_HeroPowerPotraitIntroStateName;

	[CustomEditField(Sections = "Bones")]
	public Transform m_socketHeroBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_heroPowerBone;

	[CustomEditField(Sections = "Bones")]
	public GameObject m_BossPowerBone;

	[CustomEditField(Sections = "Bones")]
	public GameObject m_HeroPowerBigCardBone;

	[CustomEditField(Sections = "Styles")]
	public List<DungeonCrawlDisplayStyleOverride> m_DungeonCrawlDisplayStyle;

	[CustomEditField(Sections = "Phone")]
	public UIBButton m_ShowDeckButton;

	[CustomEditField(Sections = "Phone")]
	public GameObject m_ShowDeckButtonFrame;

	[CustomEditField(Sections = "Phone")]
	public GameObject m_ShowDeckNoButtonFrame;

	[CustomEditField(Sections = "Phone")]
	public GameObject m_PhoneDeckTray;

	[CustomEditField(Sections = "Phone")]
	public GameObject m_DeckTrayRunCompleteBone;

	[CustomEditField(Sections = "Phone")]
	public GameObject m_DeckListHeaderRunCompleteBone;

	[CustomEditField(Sections = "Phone")]
	public TraySection m_DeckListHeaderPrefab;

	[CustomEditField(Sections = "Phone")]
	public GameObject m_TrayFrameDefault;

	[CustomEditField(Sections = "Phone")]
	public GameObject m_TrayFrameRunComplete;

	[CustomEditField(Sections = "Phone")]
	public GameObject m_AdventureTitleRunCompleteBone;

	[CustomEditField(Sections = "Phone")]
	public Vector3 m_DeckBigCardOffsetForRunCompleteState;

	[CustomEditField(Sections = "Phone")]
	public GameObject m_ViewDeckTrayMesh;

	public AdventureDungeonCrawlPlayMat m_playMat;

	public static bool s_shouldShowWelcomeBanner = true;

	private bool m_subsceneTransitionComplete;

	private CollectionDeck m_dungeonCrawlDeck;

	private DungeonCrawlDeckSelect m_dungeonCrawlDeckSelect;

	private Actor m_heroActor;

	private PlayerHeroData m_playerHeroData;

	private int m_numBossesDefeated;

	private List<long> m_defeatedBossIds;

	private long m_bossWhoDefeatedMeId;

	private long m_nextBossHealth;

	private string m_nextBossCardId;

	private long m_heroHealth;

	private bool m_isRunActive;

	private bool m_isRunRetired;

	private int m_selectedShrineIndex;

	private List<long> m_cardsAddedToDeckMap = new List<long>();

	private bool m_hasSeenLatestDungeonRunComplete;

	private List<long> m_shrineOptions;

	private long m_anomalyModeCardDbId;

	private long m_plotTwistCardDbId;

	private static GameSaveKeyId m_gameSaveDataServerKey;

	private static GameSaveKeyId m_gameSaveDataClientKey;

	private bool m_hasReceivedGameSaveDataServerKeyResponse;

	private bool m_hasReceivedGameSaveDataClientKeyResponse;

	private bool m_saveHeroDataUsingHeroId;

	private int m_numBossesInRun;

	private int m_bossCardBackId;

	private bool m_shouldSkipHeroSelect;

	private bool m_mustPickShrine;

	private bool m_mustSelectChapter;

	private Coroutine m_bossHeroPowerHideCoroutine;

	private IDungeonCrawlData m_dungeonCrawlData;

	private ISubsceneController m_subsceneController;

	private AssetLoadingHelper m_assetLoadingHelper;

	private Actor m_bossActor;

	private Actor m_bossPowerBigCard;

	private Actor m_heroPowerActor;

	private DefLoader.DisposableFullDef m_currentBossHeroPowerFullDef;

	private Actor m_heroPowerBigCard;

	private DefLoader.DisposableFullDef m_currentHeroPowerFullDef;

	private GameObject m_retireButton;

	private DungeonRunLoadoutState m_currentLoadoutState;

	private static AdventureDungeonCrawlDisplay m_instance;

	public static AdventureDungeonCrawlDisplay Get()
	{
		return m_instance;
	}

	private void Awake()
	{
		m_instance = this;
	}

	private void Start()
	{
		GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
	}

	public void StartRun(DungeonCrawlServices services)
	{
		m_dungeonCrawlData = services.DungeonCrawlData;
		m_subsceneController = services.SubsceneController;
		m_assetLoadingHelper = services.AssetLoadingHelper;
		services.AssetLoadingHelper.AssetLoadingComplete += OnSubSceneLoaded;
		m_subsceneController.TransitionComplete += OnSubSceneTransitionComplete;
		AdventureDbId selectedAdv = m_dungeonCrawlData.GetSelectedAdventure();
		AdventureModeDbId selectedMode = m_dungeonCrawlData.GetSelectedMode();
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)selectedAdv, (int)selectedMode);
		m_playerHeroData = new PlayerHeroData(m_dungeonCrawlData);
		m_playerHeroData.OnHeroDataChanged += delegate
		{
			m_playMat.SetPlayerHeroDbId(m_playerHeroData.HeroCardDbId);
		};
		m_AdventureTitle.Text = dataRecord.Name;
		m_gameSaveDataServerKey = (GameSaveKeyId)dataRecord.GameSaveDataServerKey;
		m_gameSaveDataClientKey = (GameSaveKeyId)dataRecord.GameSaveDataClientKey;
		if (m_gameSaveDataServerKey <= (GameSaveKeyId)0)
		{
			Debug.LogErrorFormat("Adventure {0} Mode {1} has no GameSaveDataKey set! This mode does not work without defining GAME_SAVE_DATA_SERVER_KEY in ADVENTURE.dbi!", selectedAdv, selectedMode);
		}
		if (m_gameSaveDataClientKey <= (GameSaveKeyId)0)
		{
			Debug.LogErrorFormat("Adventure {0} Mode {1} has no GameSaveDataKey set! This mode does not work without defining GAME_SAVE_DATA_CLIENT_KEY in ADVENTURE.dbi!", selectedAdv, selectedMode);
		}
		if (m_gameSaveDataClientKey == m_gameSaveDataServerKey)
		{
			Debug.LogErrorFormat("Adventure {0} Mode {1} has an equal GameSaveDataKey for Client and Server. These keys are not allowed to be equal!", selectedAdv, selectedMode);
		}
		m_bossCardBackId = dataRecord.BossCardBack;
		if (m_bossCardBackId == 0)
		{
			m_bossCardBackId = 0;
		}
		m_saveHeroDataUsingHeroId = dataRecord.DungeonCrawlSaveHeroUsingHeroDbId;
		List<ScenarioDbfRecord> scenarioRecords = GameDbf.Scenario.GetRecords((ScenarioDbfRecord r) => r.AdventureId == (int)selectedAdv && r.ModeId == (int)selectedMode);
		if (scenarioRecords == null || scenarioRecords.Count < 1)
		{
			Log.Adventures.PrintError("No Scenarios found for Adventure {0} and Mode {1}!", selectedAdv, selectedMode);
		}
		else if (scenarioRecords.Count == 1)
		{
			ScenarioDbfRecord scenarioRecord = scenarioRecords[0];
			m_dungeonCrawlData.SetMission((ScenarioDbId)scenarioRecord.ID, showDetails: false);
			Log.Adventures.Print("Owns wing for this Dungeon Run? {0}", AdventureProgressMgr.Get().OwnsWing(scenarioRecord.WingId));
		}
		else if (m_dungeonCrawlData.GetMission() == ScenarioDbId.INVALID)
		{
			Log.Adventures.Print("No selectedScenarioId currently set - this should come with the GameSaveData.");
		}
		else
		{
			ScenarioDbfRecord scenarioRecord2 = scenarioRecords.Find((ScenarioDbfRecord x) => x.ID == (int)m_dungeonCrawlData.GetMission());
			if (scenarioRecord2 == null)
			{
				Log.Adventures.PrintError("No matching Scenario for this Adventure has been set in AdventureConfig! AdventureConfig's mission: {0}", m_dungeonCrawlData.GetMission());
			}
			else
			{
				Log.Adventures.Print("Owns wing for this Dungeon Run? {0}", AdventureProgressMgr.Get().OwnsWing(scenarioRecord2.WingId));
			}
		}
		m_shouldSkipHeroSelect = dataRecord.DungeonCrawlSkipHeroSelect;
		m_mustPickShrine = dataRecord.DungeonCrawlMustPickShrine;
		m_mustSelectChapter = dataRecord.DungeonCrawlSelectChapter;
		m_anomalyModeCardDbId = dataRecord.AnomalyModeDefaultCardId;
		m_assetLoadingHelper.AddAssetToLoad();
		m_dungeonCrawlPlayMatReference.RegisterReadyListener<AdventureDungeonCrawlPlayMat>(OnPlayMatReady);
		bool retireButtonSupported = dataRecord.DungeonCrawlIsRetireSupported;
		m_assetLoadingHelper.AddAssetToLoad();
		m_retireButtonReference.RegisterReadyListener(delegate(Widget w)
		{
			w.RegisterEventListener(delegate(string eventName)
			{
				if (eventName == "Button_Framed_Clicked" && retireButtonSupported)
				{
					m_retireButton.SetActive(value: false);
					AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
					{
						m_headerText = GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_RETIRE_CONFIRMATION_HEADER"),
						m_text = GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_RETIRE_CONFIRMATION_BODY"),
						m_showAlertIcon = true,
						m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle,
						m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
						m_responseCallback = OnRetirePopupResponse
					};
					DialogManager.Get().ShowPopup(info);
				}
			});
			m_retireButton = w.gameObject;
			m_retireButton.SetActive(value: false);
			w.RegisterDoneChangingStatesListener(delegate
			{
				m_assetLoadingHelper.AssetLoadCompleted();
			}, null, callImmediatelyIfSet: true, doOnce: true);
		});
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_dungeonCrawlDeckSelectWidgetReference.RegisterReadyListener<DungeonCrawlDeckSelect>(OnDungeonCrawlDeckTrayReady);
		}
		if (m_dungeonCrawlDeckTray != null && m_dungeonCrawlDeckTray.m_deckBigCard != null)
		{
			m_dungeonCrawlDeckTray.m_deckBigCard.OnBigCardShown += OnDeckTrayBigCardShown;
		}
		EnableBackButton(enabled: true);
		Navigation.PushUnique(OnNavigateBack);
		m_BackButton.AddEventListener(UIEventType.RELEASE, OnBackButtonPress);
		if (m_ShowDeckButton != null)
		{
			m_ShowDeckButton.AddEventListener(UIEventType.RELEASE, OnShowDeckButtonPress);
		}
		DisableBackButtonIfInDemoMode();
		RequestOrLoadCachedGameSaveData();
		SetDungeonCrawlDisplayVisualStyle();
	}

	public void EnablePlayButton()
	{
		if (m_playMat != null)
		{
			m_playMat.PlayButton.Enable();
		}
	}

	public void DisablePlayButton()
	{
		if (m_playMat != null && m_playMat.PlayButton.IsEnabled())
		{
			m_playMat.PlayButton.Disable();
		}
	}

	public void EnableBackButton(bool enabled)
	{
		if (m_BackButton != null && m_BackButton.IsEnabled() != enabled)
		{
			m_BackButton.SetEnabled(enabled);
			m_BackButton.Flip(enabled);
		}
	}

	private void OnDeckTrayBigCardShown(Actor shownActor, EntityDef entityDef)
	{
		if (shownActor == null || entityDef == null)
		{
			return;
		}
		long cardId = GameUtils.TranslateCardIdToDbId(entityDef.GetCardId());
		if (m_anomalyModeCardDbId == cardId)
		{
			HighlightRender highlight = shownActor.GetComponentInChildren<HighlightRender>();
			MeshRenderer meshRenderer = ((highlight != null) ? highlight.GetComponent<MeshRenderer>() : null);
			if (meshRenderer != null && m_anomalyModeCardHighlightMaterial != null)
			{
				meshRenderer.SetSharedMaterial(m_anomalyModeCardHighlightMaterial);
				meshRenderer.enabled = true;
			}
		}
	}

	private void OnPlayMatPlayButtonReady(PlayButton playButton)
	{
		if (playButton == null)
		{
			Error.AddDevWarning("UI Error!", "PlayButtonReference is null, or does not have a PlayButton component on it!");
			return;
		}
		playButton.AddEventListener(UIEventType.RELEASE, OnPlayButtonPress);
		Widget playButtonWidget = playButton.GetComponentInParent<WidgetTemplate>();
		if (playButtonWidget != null)
		{
			playButtonWidget.RegisterDoneChangingStatesListener(delegate
			{
				m_assetLoadingHelper.AssetLoadCompleted();
			}, null, callImmediatelyIfSet: true, doOnce: true);
		}
		else
		{
			Error.AddDevWarning("UI Error!", "Could not find PlayMat PlayButton WidgetTemplate!");
			m_assetLoadingHelper.AssetLoadCompleted();
		}
	}

	private void OnDungeonCrawlDeckTrayReady(DungeonCrawlDeckSelect deckSelect)
	{
		m_dungeonCrawlDeckSelect = deckSelect;
		if (m_dungeonCrawlDeckSelect == null)
		{
			Error.AddDevWarning("UI Error!", "Could not find AdventureDungeonCrawlDeckTray in the AdventureDeckSelectWidget.");
			return;
		}
		if (m_dungeonCrawlDeckSelect == null)
		{
			Error.AddDevWarning("UI Error!", "Could not find SlidingTray in the AdventureDeckSelectWidget.");
			return;
		}
		deckSelect.playButton.AddEventListener(UIEventType.RELEASE, OnPlayButtonPress);
		deckSelect.heroDetails.AddHeroPowerListener(UIEventType.ROLLOVER, delegate
		{
			ShowBigCard(m_heroPowerBigCard, m_currentHeroPowerFullDef, m_HeroPowerBigCardBone);
		});
		deckSelect.heroDetails.AddHeroPowerListener(UIEventType.ROLLOUT, delegate
		{
			BigCardHelper.HideBigCard(m_heroPowerBigCard);
		});
		if (deckSelect.deckTray != null && deckSelect.deckTray.m_deckBigCard != null)
		{
			deckSelect.deckTray.m_deckBigCard.OnBigCardShown += OnDeckTrayBigCardShown;
		}
	}

	private void OnPlayMatReady(AdventureDungeonCrawlPlayMat playMat)
	{
		if (playMat == null)
		{
			Error.AddDevWarning("UI Error!", "m_dungeonCrawlPlayMatReference is null, or does not have a AdventureDungeonCrawlPlayMat component on it!");
			return;
		}
		m_playMat = playMat;
		m_playMat.SetCardBack(m_bossCardBackId);
		m_BossPowerBone = m_playMat.m_BossPowerBone;
		m_assetLoadingHelper.AddAssetToLoad();
		m_playMat.m_PlayButtonReference.RegisterReadyListener<PlayButton>(OnPlayMatPlayButtonReady);
		LoadInitialAssets();
		Widget playMatWidget = playMat.GetComponent<WidgetTemplate>();
		if (playMatWidget != null)
		{
			playMatWidget.RegisterDoneChangingStatesListener(delegate
			{
				m_assetLoadingHelper.AssetLoadCompleted();
			}, null, callImmediatelyIfSet: true, doOnce: true);
		}
		else
		{
			Error.AddDevWarning("UI Error!", "Could not find PlayMat WidgetTemplate!");
			m_assetLoadingHelper.AssetLoadCompleted();
		}
	}

	private void Update()
	{
		if (m_dungeonCrawlData != null && m_dungeonCrawlData.IsDevMode && InputCollection.GetKeyDown(KeyCode.Z) && !(m_playMat == null))
		{
			if (m_playMat.GetPlayMatState() == AdventureDungeonCrawlPlayMat.PlayMatState.SHOWING_BOSS_GRAVEYARD)
			{
				m_playMat.ShowNextBoss(GetPlayButtonTextForNextMission());
			}
			else if (m_playMat.GetPlayMatState() == AdventureDungeonCrawlPlayMat.PlayMatState.SHOWING_NEXT_BOSS)
			{
				ShowRunEnd(m_defeatedBossIds, m_bossWhoDefeatedMeId);
			}
		}
	}

	private void OnDestroy()
	{
		m_instance = null;
		m_currentBossHeroPowerFullDef?.Dispose();
		m_currentHeroPowerFullDef?.Dispose();
		if (m_playMat != null)
		{
			m_playMat.HideBossHeroPowerTooltip(immediate: true);
		}
		if (m_dungeonCrawlDeckTray != null && m_dungeonCrawlDeckTray.m_deckBigCard != null)
		{
			m_dungeonCrawlDeckTray.m_deckBigCard.OnBigCardShown -= OnDeckTrayBigCardShown;
		}
		if (m_dungeonCrawlDeckSelect != null && m_dungeonCrawlDeckSelect.deckTray != null && m_dungeonCrawlDeckSelect.deckTray.m_deckBigCard != null)
		{
			m_dungeonCrawlDeckSelect.deckTray.m_deckBigCard.OnBigCardShown -= OnDeckTrayBigCardShown;
		}
		GameMgr.Get()?.UnregisterFindGameEvent(OnFindGameEvent);
	}

	private void OnBossActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_bossActor = OnActorLoaded(assetRef, go, m_playMat.m_nextBossFaceBone.gameObject, withRotation: true);
		if (m_bossActor != null)
		{
			PegUIElement pegUIElement = m_bossActor.GetCollider().gameObject.AddComponent<PegUIElement>();
			pegUIElement.AddEventListener(UIEventType.ROLLOVER, delegate
			{
				m_bossActor.SetActorState(ActorStateType.CARD_MOUSE_OVER);
				ShowBigCard(m_bossPowerBigCard, m_currentBossHeroPowerFullDef, m_HeroPowerBigCardBone);
				m_bossHeroPowerHideCoroutine = StartCoroutine(HideBossHeroPowerTooltipAfterHover());
			});
			pegUIElement.AddEventListener(UIEventType.ROLLOUT, delegate
			{
				m_bossActor.SetActorState(ActorStateType.CARD_IDLE);
				BigCardHelper.HideBigCard(m_bossPowerBigCard);
				if (m_bossHeroPowerHideCoroutine != null)
				{
					StopCoroutine(m_bossHeroPowerHideCoroutine);
				}
			});
		}
		m_playMat.SetBossActor(m_bossActor);
		m_assetLoadingHelper.AssetLoadCompleted();
	}

	private void LoadInitialAssets()
	{
		AdventureDbId selectedAdventure = m_dungeonCrawlData.GetSelectedAdventure();
		AdventureModeDbId selectedMode = m_dungeonCrawlData.GetSelectedMode();
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)selectedAdventure, (int)selectedMode);
		if (dataRecord == null)
		{
			Log.Adventures.PrintError("Tried to load assets but data record not found!");
			return;
		}
		IAssetLoader assetLoader = AssetLoader.Get();
		m_assetLoadingHelper.AddAssetToLoad();
		assetLoader.InstantiatePrefab(dataRecord.DungeonCrawlBossCardPrefab, OnBossActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		m_assetLoadingHelper.AddAssetToLoad();
		assetLoader.InstantiatePrefab("History_HeroPower_Opponent.prefab:a99d23d6e8630f94b96a8e096fffb16f", OnBossPowerBigCardLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		m_assetLoadingHelper.AddAssetToLoad();
		assetLoader.InstantiatePrefab("Card_Dungeon_Play_Hero.prefab:183cb9cc59697844e911776ec349fe5e", OnHeroActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		m_assetLoadingHelper.AddAssetToLoad();
		assetLoader.InstantiatePrefab("History_HeroPower.prefab:e73edf8ccea2b11429093f7a448eef53", OnHeroPowerBigCardLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		m_assetLoadingHelper.AddAssetToLoad();
		assetLoader.InstantiatePrefab("Card_Play_HeroPower.prefab:a3794839abb947146903a26be13e09af", OnHeroPowerActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private IEnumerator HideBossHeroPowerTooltipAfterHover()
	{
		float timer = 0f;
		while (timer < m_RolloverTimeToHideBossHeroPowerTooltip)
		{
			timer += Time.unscaledDeltaTime;
			yield return new WaitForEndOfFrame();
		}
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSS_HERO_POWER_TUTORIAL_PROGRESS, out long heroPowerProgress);
		if (heroPowerProgress == 1)
		{
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSS_HERO_POWER_TUTORIAL_PROGRESS, 2L));
		}
		m_playMat.HideBossHeroPowerTooltip();
	}

	private void OnBossPowerBigCardLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_bossPowerBigCard = OnActorLoaded(assetRef, go, m_BossPowerBone);
		if (m_bossPowerBigCard != null)
		{
			m_bossPowerBigCard.TurnOffCollider();
		}
		m_assetLoadingHelper.AssetLoadCompleted();
	}

	private void OnHeroPowerBigCardLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_heroPowerBigCard = OnActorLoaded(assetRef, go, m_HeroPowerBigCardBone);
		if (m_heroPowerBigCard != null)
		{
			m_heroPowerBigCard.TurnOffCollider();
		}
		m_assetLoadingHelper.AssetLoadCompleted();
	}

	private void RequestOrLoadCachedGameSaveData()
	{
		if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY)
		{
			GameSaveDataManager.Get().ClearLocalData(m_gameSaveDataServerKey);
		}
		StartCoroutine(InitializeFromGameSaveDataWhenReady());
		if (!GameSaveDataManager.Get().IsDataReady(m_gameSaveDataServerKey))
		{
			GameSaveDataManager.Get().Request(m_gameSaveDataServerKey, OnRequestGameSaveDataServerResponse);
		}
		else
		{
			m_hasReceivedGameSaveDataServerKeyResponse = true;
		}
		if (!GameSaveDataManager.Get().IsDataReady(m_gameSaveDataClientKey))
		{
			GameSaveDataManager.Get().Request(m_gameSaveDataClientKey, OnRequestGameSaveDataClientResponse);
		}
		else
		{
			m_hasReceivedGameSaveDataClientKeyResponse = true;
		}
	}

	private void OnRequestGameSaveDataServerResponse(bool success)
	{
		if (!success)
		{
			Debug.LogError("OnRequestGameSaveDataResponse: Error requesting game save data for current adventure.");
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		}
		else
		{
			m_hasReceivedGameSaveDataServerKeyResponse = true;
		}
	}

	private void OnRequestGameSaveDataClientResponse(bool success)
	{
		if (!success)
		{
			Debug.LogError("OnRequestGameSaveDataResponse: Error requesting game save data for current adventure.");
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		}
		else
		{
			m_hasReceivedGameSaveDataClientKeyResponse = true;
		}
	}

	private IEnumerator InitializeFromGameSaveDataWhenReady()
	{
		while (m_playMat == null || !m_playMat.IsReady())
		{
			Log.Adventures.Print("Waiting for Play Mat to be initialized before handling new Game Save Data.");
			yield return null;
		}
		while (m_playMat.GetPlayMatState() == AdventureDungeonCrawlPlayMat.PlayMatState.TRANSITIONING_FROM_PREV_STATE)
		{
			Log.Adventures.Print("Waiting for Play Mat to be done transitioning before handling new Game Save Data.");
			yield return null;
		}
		while (!m_hasReceivedGameSaveDataClientKeyResponse || !m_hasReceivedGameSaveDataServerKeyResponse)
		{
			yield return null;
		}
		DungeonCrawlUtil.MigrateDungeonCrawlSubkeys(m_gameSaveDataClientKey, m_gameSaveDataServerKey);
		while (m_heroActor == null || m_heroPowerActor == null || m_heroPowerBigCard == null)
		{
			yield return null;
		}
		InitializeFromGameSaveData();
	}

	private bool IsScenarioValidForAdventureAndMode(ScenarioDbId selectedScenario)
	{
		if (!AdventureUtils.IsMissionValidForAdventureMode(m_dungeonCrawlData.GetSelectedAdventure(), m_dungeonCrawlData.GetSelectedMode(), selectedScenario))
		{
			Debug.LogErrorFormat("Scenario {0} is not a part of Adventure {1} and mode {2}! Something is probably wrong.", selectedScenario, m_dungeonCrawlData.GetSelectedAdventure(), m_dungeonCrawlData.GetSelectedMode());
			return false;
		}
		return true;
	}

	private void InitializeFromGameSaveData()
	{
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)m_dungeonCrawlData.GetSelectedAdventure(), (int)m_dungeonCrawlData.GetSelectedMode());
		m_playerHeroData.UpdateHeroDataFromClass(TAG_CLASS.INVALID);
		List<long> deckCardList = null;
		List<CardWithPremiumStatus> deckCardListPremium = null;
		List<long> treasureOptions = null;
		List<long> classLootOptionsA = null;
		List<long> classLootOptionsB = null;
		List<long> classLootOptionsC = null;
		List<long> nextBosses = null;
		long playerChosenLoot = 0L;
		long playerChosenTreasure = 0L;
		if (GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSSES_DEFEATED, out m_defeatedBossIds))
		{
			m_numBossesDefeated = m_defeatedBossIds.Count;
		}
		List<long> deckCardIndices = null;
		List<long> deckCardEnchantments = null;
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CARD_LIST, out deckCardList);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSS_LOST_TO, out m_bossWhoDefeatedMeId);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_OPTION_A, out classLootOptionsA);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_OPTION_B, out classLootOptionsB);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_OPTION_C, out classLootOptionsC);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_TREASURE_OPTION, out treasureOptions);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_SHRINE_OPTIONS, out m_shrineOptions);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEXT_BOSS_FIGHT, out nextBosses);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_LOOT, out playerChosenLoot);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_TREASURE, out playerChosenTreasure);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEXT_BOSS_HEALTH, out m_nextBossHealth);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_HEALTH, out long heroHealth);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_CARDS_ADDED_TO_DECK_MAP, out m_cardsAddedToDeckMap);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_SHRINE, out long selectedShrineIndex);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_SCENARIO_ID, out long selectedScenarioId);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_SCENARIO_ID, out long activeRunScenarioId);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_SCENARIO_OVERRIDE, out long scenarioOverrideId);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_LOADOUT_TREASURE_ID, out long selectedLoadoutTreasureDbId);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_POWER, out long selectedHeroPowerDbId);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_POWER, out long activeRunHeroPower);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_ANOMALY_MODE, out long selectedAnomalyMode);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_ANOMALY_MODE, out long activeRunAnomalyMode);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_DECK, out long selectedDeckId);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_ENCHANTMENT_INDICES, out deckCardIndices);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_ENCHANTMENTS, out deckCardEnchantments);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_CURRENT_ANOMALY_MODE_CARD, out long currentAnomalyModeCardDbId);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_ANOMALY_MODE_CARD_PREVIEW, out long anomalyModeCardPreviewDbId);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_LATEST_DUNGEON_RUN_COMPLETE, out long hasSeenLatestDungeonRunComplete);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CLASS, out long deckClass);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_CLASS, out long selectedHeroClass);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_CARD_DB_ID, out long heroCardDbId);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_CARD_DB_ID, out long selectedHeroCardDbId);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_RUN_RETIRED, out long isRunRetired);
		if (m_saveHeroDataUsingHeroId)
		{
			m_playerHeroData.UpdateHeroDataFromHeroCardDbId((int)heroCardDbId);
		}
		else
		{
			m_playerHeroData.UpdateHeroDataFromClass((TAG_CLASS)deckClass);
		}
		m_selectedShrineIndex = (int)selectedShrineIndex;
		if (deckCardList != null)
		{
			deckCardListPremium = CardWithPremiumStatus.ConvertList(deckCardList);
		}
		m_isRunRetired = isRunRetired > 0;
		m_isRunActive = DungeonCrawlUtil.IsDungeonRunActive(m_gameSaveDataServerKey);
		m_hasSeenLatestDungeonRunComplete = hasSeenLatestDungeonRunComplete > 0;
		bool useLoadoutOfActiveRun = m_isRunActive || ShouldShowRunCompletedScreen();
		Dictionary<string, SideboardDeck> sideboardMap = null;
		m_dungeonCrawlData.SelectedLoadoutTreasureDbId = selectedLoadoutTreasureDbId;
		m_dungeonCrawlData.SelectedHeroPowerDbId = selectedHeroPowerDbId;
		m_dungeonCrawlData.SelectedDeckId = selectedDeckId;
		if (m_saveHeroDataUsingHeroId && selectedHeroCardDbId != 0L)
		{
			m_dungeonCrawlData.SelectedHeroCardDbId = selectedHeroCardDbId;
		}
		else
		{
			TAG_CLASS heroClass = (TAG_CLASS)selectedHeroClass;
			if (heroClass != 0)
			{
				string cardId = AdventureUtils.GetHeroCardIdFromClassForDungeonCrawl(m_dungeonCrawlData, heroClass);
				m_dungeonCrawlData.SelectedHeroCardDbId = GameUtils.TranslateCardIdToDbId(cardId);
			}
		}
		ScenarioDbId scenarioOverride = (ScenarioDbId)scenarioOverrideId;
		if (scenarioOverride != 0 && !IsScenarioValidForAdventureAndMode(scenarioOverride))
		{
			scenarioOverride = ScenarioDbId.INVALID;
		}
		Log.Adventures.Print("Scenario Override set to {0}!", scenarioOverride);
		m_dungeonCrawlData.SetMissionOverride(scenarioOverride);
		ScenarioDbId scenarioDbId = (ScenarioDbId)(useLoadoutOfActiveRun ? activeRunScenarioId : selectedScenarioId);
		if (scenarioDbId != 0 && IsScenarioValidForAdventureAndMode(scenarioDbId))
		{
			m_dungeonCrawlData.SetMission(scenarioDbId);
		}
		bool hasValidLoadout = false;
		if (!useLoadoutOfActiveRun)
		{
			hasValidLoadout = m_dungeonCrawlData.HasValidLoadoutForSelectedAdventure();
			if (!hasValidLoadout)
			{
				ResetDungeonCrawlSelections(m_dungeonCrawlData);
			}
		}
		m_playMat.m_paperControllerReference.RegisterReadyListener<VisualController>(OnPlayMatPaperControllerReady);
		m_playMat.m_paperControllerReference_phone.RegisterReadyListener<VisualController>(OnPlayMatPaperControllerReady);
		if (useLoadoutOfActiveRun)
		{
			m_dungeonCrawlData.AnomalyModeActivated = activeRunAnomalyMode > 0;
		}
		else if (hasValidLoadout)
		{
			m_dungeonCrawlData.AnomalyModeActivated = selectedAnomalyMode > 0;
		}
		if (useLoadoutOfActiveRun)
		{
			m_heroHealth = heroHealth;
		}
		else
		{
			m_heroHealth = 0L;
		}
		if (HandleDemoModeReset())
		{
			return;
		}
		long anomalyModeCardDbId = (useLoadoutOfActiveRun ? currentAnomalyModeCardDbId : anomalyModeCardPreviewDbId);
		if (anomalyModeCardDbId > 0)
		{
			m_anomalyModeCardDbId = anomalyModeCardDbId;
		}
		if (m_isRunActive && deckCardListPremium != null)
		{
			if (playerChosenLoot != 0L)
			{
				List<long>[] lootChoices = new List<long>[3] { classLootOptionsA, classLootOptionsB, classLootOptionsC };
				int index = (int)playerChosenLoot - 1;
				if (index >= lootChoices.Length || lootChoices[index] == null)
				{
					Log.Adventures.PrintError("Attempting to add Loot choice {0} to the deck list, but there is not corresponding list of Loot!", index);
				}
				else
				{
					List<long> chosenLoot = lootChoices[index];
					for (int i = 1; i < chosenLoot.Count; i++)
					{
						deckCardListPremium.Add(new CardWithPremiumStatus(chosenLoot[i], TAG_PREMIUM.NORMAL));
					}
				}
			}
			if (playerChosenTreasure != 0L && treasureOptions != null)
			{
				int index2 = (int)playerChosenTreasure - 1;
				if (treasureOptions.Count <= index2)
				{
					Log.Adventures.PrintError("Attempting to add Treasure choice {0} to the deck list, but treasureLootOptions only has {1} options!", index2, treasureOptions.Count);
				}
				else
				{
					deckCardListPremium.Add(new CardWithPremiumStatus(treasureOptions[index2], TAG_PREMIUM.NORMAL));
				}
			}
		}
		ScenarioDbId scenarioId = m_dungeonCrawlData.GetMission();
		int chapterIndex = 0;
		WingDbfRecord wingRecord = GameUtils.GetWingRecordFromMissionId((int)scenarioId);
		m_numBossesInRun = m_dungeonCrawlData.GetAdventureBossesInRun(wingRecord);
		if (wingRecord != null)
		{
			chapterIndex = Mathf.Max(0, GameUtils.GetSortedWingUnlockIndex(wingRecord));
			m_plotTwistCardDbId = wingRecord.PlotTwistCardId;
		}
		int nextBossId = 0;
		if (nextBosses != null && nextBosses.Count > chapterIndex && !m_isRunRetired)
		{
			nextBossId = (int)nextBosses[chapterIndex];
		}
		if (nextBossId != 0)
		{
			m_nextBossCardId = GameUtils.TranslateDbIdToCardId(nextBossId);
		}
		else
		{
			m_nextBossCardId = GameUtils.GetMissionHeroCardId((int)scenarioId);
		}
		if (m_nextBossCardId == null)
		{
			Log.Adventures.PrintWarning("AdventureDungeonCrawlDisplay.OnGameSaveDataResponse() - No cardId for boss dbId {0}!", nextBossId);
		}
		else
		{
			m_assetLoadingHelper.AddAssetToLoad();
			DefLoader.Get().LoadFullDef(m_nextBossCardId, OnBossFullDefLoaded);
		}
		long heroPowerDbId = (useLoadoutOfActiveRun ? activeRunHeroPower : m_dungeonCrawlData.SelectedHeroPowerDbId);
		if (heroPowerDbId != 0L)
		{
			SetHeroPower(GameUtils.TranslateDbIdToCardId((int)heroPowerDbId));
		}
		if (m_isRunActive || ShouldShowRunCompletedScreen())
		{
			s_shouldShowWelcomeBanner = false;
		}
		InitializePlayMat();
		if (ShouldShowRunCompletedScreen())
		{
			ShowRunEnd(m_defeatedBossIds, m_bossWhoDefeatedMeId);
			SetUpBossKillCounter(m_playerHeroData.HeroCardDbId);
			SetUpDeckList(deckCardListPremium, useLoadoutOfActiveRun, playGlowAnimation: false, deckCardIndices, deckCardEnchantments, null, sideboardMap);
			SetUpHeroPortrait(m_playerHeroData);
			SetUpPhoneRunCompleteScreen();
		}
		else if (!m_isRunActive)
		{
			if (!m_dungeonCrawlData.HeroIsSelectedBeforeDungeonCrawlScreenForSelectedAdventure())
			{
				TryShowWelcomeBanner();
			}
			bool shouldShowNextBoss = true;
			if (m_mustPickShrine)
			{
				if (m_shrineOptions == null && m_dungeonCrawlData.GetSelectedAdventure() == AdventureDbId.TRL)
				{
					m_shrineOptions = GetDefaultStartingShrineOptions_TRL();
				}
				if (m_shrineOptions != null)
				{
					if (m_selectedShrineIndex == 0)
					{
						m_playerHeroData.UpdateHeroDataFromClass(TAG_CLASS.NEUTRAL);
						SetPlaymatStateForShrineSelection(m_shrineOptions);
						shouldShowNextBoss = false;
					}
					else
					{
						long shrineCardId = m_shrineOptions[m_selectedShrineIndex - 1];
						TAG_CLASS shrineClass = GetClassFromShrine(shrineCardId);
						m_playerHeroData.UpdateHeroDataFromClass(shrineClass);
						SetUpDeckListFromShrine(shrineCardId, playDeckGlowAnimation: false);
						if (m_dungeonCrawlData.SelectedHeroCardDbId == 0L)
						{
							m_dungeonCrawlData.SelectedHeroCardDbId = AdventureUtils.GetHeroCardDbIdFromClassForDungeonCrawl(m_dungeonCrawlData, shrineClass);
						}
					}
					SetUpHeroPortrait(m_playerHeroData);
				}
				SetUpBossKillCounter((int)m_dungeonCrawlData.SelectedHeroCardDbId);
			}
			else
			{
				if (m_dungeonCrawlData.HeroIsSelectedBeforeDungeonCrawlScreenForSelectedAdventure() && m_dungeonCrawlData.SelectedHeroCardDbId != 0L)
				{
					m_playerHeroData.UpdateHeroDataFromHeroCardDbId((int)m_dungeonCrawlData.SelectedHeroCardDbId);
					SetUpHeroPortrait(m_playerHeroData);
					SetUpBossKillCounter(m_playerHeroData.HeroCardDbId);
				}
				bool num = m_dungeonCrawlData.SelectableLoadoutTreasuresExist();
				bool hasSelectableHeroPowersAndDecks = m_dungeonCrawlData.SelectableHeroPowersAndDecksExist();
				if (num || hasSelectableHeroPowersAndDecks)
				{
					if (!m_dungeonCrawlData.HasValidLoadoutForSelectedAdventure())
					{
						m_currentLoadoutState = DungeonRunLoadoutState.INVALID;
						GoToNextLoadoutState();
						shouldShowNextBoss = false;
						if (m_plotTwistCardDbId != 0L || (m_anomalyModeCardDbId != 0L && m_dungeonCrawlData.AnomalyModeActivated))
						{
							SetUpDeckList(null, useLoadoutOfActiveRun);
						}
					}
					else if ((m_dungeonCrawlDeck == null || m_dungeonCrawlDeck.GetTotalCardCount() <= 0) && m_dungeonCrawlData.SelectedDeckId != 0L && m_playerHeroData.HeroClasses[0] != 0)
					{
						string deckName;
						string deckDescription;
						List<long> deckContents = CollectionManager.Get().LoadDeckFromDBF((int)m_dungeonCrawlData.SelectedDeckId, out deckName, out deckDescription);
						SetUpDeckList(CardWithPremiumStatus.ConvertList(deckContents), useLoadoutOfActiveRun);
					}
				}
				else if (dataRecord.DungeonCrawlDefaultToDeckFromUpcomingScenario)
				{
					SetUpDeckListFromScenario(m_dungeonCrawlData.GetMission(), useLoadoutOfActiveRun);
				}
				SetUpBossKillCounter((int)m_dungeonCrawlData.SelectedHeroCardDbId);
			}
			if (shouldShowNextBoss)
			{
				m_playMat.SetUpDefeatedBosses(null, m_numBossesInRun);
				m_playMat.SetShouldShowBossHeroPowerTooltip(ShouldShowBossHeroPowerTutorial());
				m_assetLoadingHelper.AddAssetToLoad();
				m_playMat.SetUpCardBacks(m_numBossesInRun - 1, m_assetLoadingHelper.AssetLoadCompleted);
				string playButtonText = "GLUE_CHOOSE";
				if (m_shouldSkipHeroSelect || m_dungeonCrawlData.HeroIsSelectedBeforeDungeonCrawlScreenForSelectedAdventure())
				{
					playButtonText = GetPlayButtonTextForNextMission();
				}
				m_playMat.ShowNextBoss(playButtonText);
				if (m_mustSelectChapter)
				{
					m_BackButton.SetText("GLOBAL_LEAVE");
				}
			}
			SetUpPhoneNewRunScreen();
		}
		else
		{
			SetUpBossKillCounter(m_playerHeroData.HeroCardDbId);
			if (dataRecord.DungeonCrawlDefaultToDeckFromUpcomingScenario && (deckCardListPremium == null || deckCardListPremium.Count == 0))
			{
				if ((deckCardIndices != null && deckCardIndices.Count > 0) || (deckCardEnchantments != null && deckCardEnchantments.Count > 0))
				{
					Debug.LogWarning("AdventureDungeonCrawlDisplay.InitializeFromGameSaveData() - Setting the deck list using the deck from upcoming scenario, but you have deck card enchantments! Something is probably wrong. Enchantments being ignored.");
				}
				SetUpDeckListFromScenario(m_dungeonCrawlData.GetMission(), useLoadoutOfActiveRun);
			}
			else
			{
				SetUpDeckList(deckCardListPremium, useLoadoutOfActiveRun, playGlowAnimation: false, deckCardIndices, deckCardEnchantments, null, sideboardMap);
			}
			SetUpHeroPortrait(m_playerHeroData);
			m_playMat.SetUpDefeatedBosses(m_defeatedBossIds, m_numBossesInRun);
			m_playMat.SetShouldShowBossHeroPowerTooltip(ShouldShowBossHeroPowerTutorial());
			m_assetLoadingHelper.AddAssetToLoad();
			int numDefeatedBosses = ((m_defeatedBossIds != null) ? m_defeatedBossIds.Count : 0);
			m_playMat.SetUpCardBacks(m_numBossesInRun - numDefeatedBosses - 1, m_assetLoadingHelper.AssetLoadCompleted);
			SetPlayMatStateFromGameSaveData();
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_dungeonCrawlDeckTray.gameObject.SetActive(value: false);
			}
			m_retireButton.SetActive(dataRecord.DungeonCrawlIsRetireSupported);
		}
		m_assetLoadingHelper.AssetLoadCompleted();
	}

	private void OnPlayMatPaperControllerReady(VisualController paperController)
	{
		if (paperController == null)
		{
			Debug.LogError("paperController was null in OnPlayMatPaperControllerReady!");
		}
		m_assetLoadingHelper.AssetLoadCompleted();
	}

	private void InitializePlayMat()
	{
		m_assetLoadingHelper.AddAssetToLoad();
		m_playMat.Initialize(m_dungeonCrawlData);
		Widget playMatWidget = m_playMat.GetComponent<WidgetTemplate>();
		if (playMatWidget != null)
		{
			playMatWidget.RegisterDoneChangingStatesListener(delegate
			{
				m_assetLoadingHelper.AssetLoadCompleted();
			}, null, callImmediatelyIfSet: true, doOnce: true);
		}
		else
		{
			Error.AddDevWarning("UI Error!", "Could not find PlayMat WidgetTemplate!");
			m_assetLoadingHelper.AssetLoadCompleted();
		}
	}

	private IEnumerator SetPlayMatStateFromGameSaveDataWhenReady()
	{
		while (GameSaveDataManager.Get().IsRequestPending(m_gameSaveDataServerKey) || GameSaveDataManager.Get().IsRequestPending(m_gameSaveDataClientKey) || m_playMat.GetPlayMatState() == AdventureDungeonCrawlPlayMat.PlayMatState.TRANSITIONING_FROM_PREV_STATE)
		{
			yield return null;
		}
		SetPlayMatStateFromGameSaveData();
	}

	private string GetPlayButtonTextForNextMission()
	{
		string playButtonTextOverride = "";
		if (GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.PLAY_BUTTON_TEXT_OVERRIDE, out playButtonTextOverride) && !string.IsNullOrEmpty(playButtonTextOverride))
		{
			return playButtonTextOverride;
		}
		return "GLOBAL_PLAY";
	}

	private bool IsNextMissionASpecialEncounter()
	{
		if (!m_hasReceivedGameSaveDataServerKeyResponse)
		{
			Debug.LogError("GetPlayButtonTextForNextMission() - this cannot be called before we've gotten the Game Save Data Server Key response!");
			return false;
		}
		return m_dungeonCrawlData.GetMissionOverride() != ScenarioDbId.INVALID;
	}

	private void SetPlayMatStateFromGameSaveData()
	{
		List<long> treasureOptions = null;
		List<long> classLootOptionsA = null;
		List<long> classLootOptionsB = null;
		List<long> classLootOptionsC = null;
		long playerChosenLoot = 0L;
		long playerChosenTreasure = 0L;
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_OPTION_A, out classLootOptionsA);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_OPTION_B, out classLootOptionsB);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_OPTION_C, out classLootOptionsC);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_TREASURE_OPTION, out treasureOptions);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_LOOT, out playerChosenLoot);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_TREASURE, out playerChosenTreasure);
		bool isRunActive = DungeonCrawlUtil.IsDungeonRunActive(m_gameSaveDataServerKey);
		m_playMat.IsNextMissionASpecialEncounter = IsNextMissionASpecialEncounter();
		if (m_backButtonHighlight != null)
		{
			m_backButtonHighlight.ChangeState(ActorStateType.NONE);
		}
		if (isRunActive && playerChosenTreasure == 0L && treasureOptions != null && treasureOptions.Count > 0)
		{
			m_playMat.ShowTreasureOptions(treasureOptions);
			return;
		}
		if (isRunActive && playerChosenLoot == 0L && ((classLootOptionsA != null && classLootOptionsA.Count > 0) || (classLootOptionsB != null && classLootOptionsB.Count > 0) || (classLootOptionsC != null && classLootOptionsC.Count > 0)))
		{
			m_playMat.ShowLootOptions(classLootOptionsA, classLootOptionsB, classLootOptionsC);
			return;
		}
		if (!isRunActive)
		{
			m_playMat.SetUpDefeatedBosses(null, m_numBossesInRun);
			m_playMat.SetShouldShowBossHeroPowerTooltip(ShouldShowBossHeroPowerTutorial());
			m_playMat.SetUpCardBacks(m_numBossesInRun - 1, null);
		}
		m_playMat.ShowNextBoss(GetPlayButtonTextForNextMission());
	}

	private void SetPlaymatStateForShrineSelection(List<long> shrineOptions)
	{
		if (shrineOptions == null || shrineOptions.Count == 0)
		{
			Log.Adventures.PrintError("SetPlaymatStateForShrineSelection: No shrine options found for adventure.");
			return;
		}
		SetShowDeckButtonEnabled(enabled: false);
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_DECK_SELECTION_TUTORIAL, out long hasSeenDeckSelectionTutorial);
		if (hasSeenDeckSelectionTutorial == 0L)
		{
			m_playMat.ShowEmptyState();
			StartCoroutine(ShowDeckSelectionTutorialPopupWhenReady(delegate
			{
				m_playMat.ShowShrineOptions(shrineOptions);
			}));
		}
		else
		{
			m_playMat.ShowShrineOptions(shrineOptions);
		}
	}

	private List<long> GetDefaultStartingShrineOptions_TRL()
	{
		return new List<long> { 52891L, 51920L, 53036L };
	}

	private IEnumerator ShowDeckSelectionTutorialPopupWhenReady(Action popupDismissedCallback)
	{
		while (!m_subsceneTransitionComplete)
		{
			yield return new WaitForEndOfFrame();
		}
		while (s_shouldShowWelcomeBanner)
		{
			yield return new WaitForEndOfFrame();
		}
		AdventureDef advDef = m_dungeonCrawlData.GetAdventureDef();
		if (advDef != null && !string.IsNullOrEmpty(advDef.m_AdventureDeckSelectionTutorialBannerPrefab))
		{
			BannerManager.Get().ShowBanner(advDef.m_AdventureDeckSelectionTutorialBannerPrefab, null, null, delegate
			{
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_DECK_SELECTION_TUTORIAL, 1L));
				popupDismissedCallback();
			});
		}
		else
		{
			popupDismissedCallback();
		}
	}

	private bool HandleDemoModeReset()
	{
		if (IsInDemoMode() && (m_numBossesDefeated >= 3 || m_bossWhoDefeatedMeId != 0L))
		{
			m_isRunActive = false;
			m_defeatedBossIds = null;
			m_bossWhoDefeatedMeId = 0L;
			m_numBossesDefeated = 0;
			StartCoroutine(ShowDemoThankQuote());
			s_shouldShowWelcomeBanner = false;
			return true;
		}
		return false;
	}

	private void TryShowWelcomeBanner()
	{
		if (!s_shouldShowWelcomeBanner)
		{
			return;
		}
		AdventureDef advDef = m_dungeonCrawlData.GetAdventureDef();
		if (advDef != null && !string.IsNullOrEmpty(advDef.m_AdventureIntroBannerPrefab))
		{
			BannerManager.Get().ShowBanner(advDef.m_AdventureIntroBannerPrefab, null, GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_INTRO_BANNER_BUTTON"), delegate
			{
				s_shouldShowWelcomeBanner = false;
			});
			WingDbId wingId = GameUtils.GetWingIdFromMissionId(m_dungeonCrawlData.GetMission());
			DungeonCrawlSubDef_VOLines.PlayVOLine(m_dungeonCrawlData.GetSelectedAdventure(), wingId, m_playerHeroData.HeroCardDbId, DungeonCrawlSubDef_VOLines.VOEventType.WELCOME_BANNER);
		}
		else
		{
			s_shouldShowWelcomeBanner = false;
		}
	}

	private bool ShouldShowBossHeroPowerTutorial()
	{
		GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSS_HERO_POWER_TUTORIAL_PROGRESS, out long heroPowerTutorialProgress);
		if (heroPowerTutorialProgress == 0L)
		{
			if (m_numBossesDefeated >= 2)
			{
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSS_HERO_POWER_TUTORIAL_PROGRESS, 1L));
				return true;
			}
			return false;
		}
		return heroPowerTutorialProgress == 1;
	}

	private void ShowRunEnd(List<long> defeatedBossIds, long bossWhoDefeatedMeId)
	{
		m_BackButton.Flip(faceUp: false, forceImmediate: true);
		m_BackButton.SetEnabled(enabled: false);
		m_assetLoadingHelper.AddAssetToLoad();
		m_playMat.ShowRunEnd(defeatedBossIds, bossWhoDefeatedMeId, m_numBossesInRun, HasCompletedAdventureWithAllClasses(), GetRunWinsForClass(m_playerHeroData.HeroClasses[0]) == 1, GetNumberOfClassesThatHaveCompletedAdventure(), m_gameSaveDataServerKey, m_gameSaveDataClientKey, m_assetLoadingHelper.AssetLoadCompleted, RunEndCompleted);
	}

	private int GetNumberOfClassesThatHaveCompletedAdventure()
	{
		int count = 0;
		foreach (TAG_CLASS tagClass in GameSaveDataManager.GetClassesFromDungeonCrawlProgressMap())
		{
			if (GetRunWinsForClass(tagClass) > 0)
			{
				count++;
			}
		}
		return count;
	}

	private bool HasCompletedAdventureWithAllClasses()
	{
		List<GuestHero> guestHeroes = m_dungeonCrawlData.GetGuestHeroesForCurrentAdventure();
		if (guestHeroes.Count > 0)
		{
			foreach (GuestHero item in guestHeroes)
			{
				TAG_CLASS guestHeroClass = GameUtils.GetTagClassFromCardDbId(item.cardDbId);
				if (GameSaveDataManager.GetClassesFromDungeonCrawlProgressMap().Contains(guestHeroClass) && !HasCompletedAdventureWithClass(guestHeroClass))
				{
					return false;
				}
			}
		}
		else
		{
			foreach (TAG_CLASS tagClass in GameSaveDataManager.GetClassesFromDungeonCrawlProgressMap())
			{
				if (!HasCompletedAdventureWithClass(tagClass))
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool HasCompletedAdventureWithClass(TAG_CLASS tagClass)
	{
		return GetRunWinsForClass(tagClass) > 0;
	}

	private void RunEndCompleted()
	{
		if (!(m_BackButton == null))
		{
			m_dungeonCrawlData.SelectedHeroCardDbId = 0L;
			m_BackButton.Flip(faceUp: true);
			m_BackButton.SetEnabled(enabled: true);
			if (m_backButtonHighlight != null)
			{
				m_backButtonHighlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
			}
		}
	}

	private void SetUpBossKillCounter(int heroCardDbId)
	{
		bool displayWinsAsTotal = m_shouldSkipHeroSelect;
		long wins = 0L;
		long runWins = 0L;
		m_bossKillCounter.SetDungeonRunData(m_dungeonCrawlData);
		TAG_CLASS deckClass = GameUtils.GetTagClassFromCardDbId(heroCardDbId);
		if (deckClass != 0 && !displayWinsAsTotal)
		{
			m_bossKillCounter.SetHeroClass(deckClass);
			AdventureDataDbfRecord adventureDataRecord = m_dungeonCrawlData.GetSelectedAdventureDataRecord();
			if (adventureDataRecord.DungeonCrawlSaveHeroUsingHeroDbId)
			{
				int currentGuestHero = AdventureUtils.GetGuestHeroIdFromHeroCardDbId(m_dungeonCrawlData, heroCardDbId);
				if (!GetBossWinsForGuestHero(currentGuestHero, adventureDataRecord.AdventureId, out wins))
				{
					wins = GetBossWinsForClass(deckClass);
				}
			}
			else
			{
				wins = GetBossWinsForClass(deckClass);
			}
			runWins = GetRunWinsForClass(deckClass);
		}
		else if (displayWinsAsTotal)
		{
			GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_ALL_CLASSES_TOTAL_BOSS_WINS, out wins);
			GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_ALL_CLASSES_TOTAL_RUN_WINS, out runWins);
		}
		m_bossKillCounter.SetBossWins(wins);
		if (runWins > 0)
		{
			m_bossKillCounter.SetRunWins(runWins);
		}
		m_bossKillCounter.UpdateLayout();
	}

	private long GetRunWinsForClass(TAG_CLASS tagClass)
	{
		long wins = 0L;
		if (GameSaveDataManager.GetProgressSubkeyForDungeonCrawlClass(tagClass, out var classSubkeys))
		{
			GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, classSubkeys.runWins, out wins);
		}
		return wins;
	}

	public bool IsCardLoadoutTreasureForCurrentHero(string cardID)
	{
		if (m_dungeonCrawlData == null)
		{
			return false;
		}
		List<AdventureLoadoutTreasuresDbfRecord> treasuresForDungeonCrawlHero = AdventureUtils.GetTreasuresForDungeonCrawlHero(m_dungeonCrawlData, (int)m_dungeonCrawlData.SelectedHeroCardDbId);
		int cardDbId = GameUtils.TranslateCardIdToDbId(cardID);
		foreach (AdventureLoadoutTreasuresDbfRecord item in treasuresForDungeonCrawlHero)
		{
			if (item.CardId == cardDbId)
			{
				return true;
			}
		}
		return false;
	}

	private bool GetBossWinsForGuestHero(int guestHeroId, int adventureId, out long wins)
	{
		int baseGuestHeroId = AdventureUtils.GetBaseGuestHeroIdForAdventure((AdventureDbId)adventureId, guestHeroId);
		if (GameSaveDataManager.GetBossWinsSubkeyForDungeonCrawlGuestHero((baseGuestHeroId > 0) ? baseGuestHeroId : guestHeroId, out var bossWinsSubkey))
		{
			GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, bossWinsSubkey, out wins);
			return true;
		}
		wins = 0L;
		return false;
	}

	private long GetBossWinsForClass(TAG_CLASS tagClass)
	{
		long wins = 0L;
		if (GameSaveDataManager.GetProgressSubkeyForDungeonCrawlClass(tagClass, out var classSubkeys))
		{
			GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataServerKey, classSubkeys.bossWins, out wins);
		}
		return wins;
	}

	private void SetUpDeckListFromShrine(long shrineCardId, bool playDeckGlowAnimation)
	{
		List<long> cardsInDeck = new List<long>();
		CardTagDbfRecord shrineDeckCardTagRecord = GameDbf.CardTag.GetRecord((CardTagDbfRecord r) => r.CardId == (int)shrineCardId && r.TagId == 1099);
		foreach (DeckCardDbfRecord card in GameDbf.DeckCard.GetRecords((DeckCardDbfRecord r) => r.DeckId == shrineDeckCardTagRecord.TagValue))
		{
			cardsInDeck.Add(card.CardId);
		}
		cardsInDeck.Add(shrineCardId);
		SetUpDeckList(CardWithPremiumStatus.ConvertList(cardsInDeck), useLoadoutOfActiveRun: false, playDeckGlowAnimation);
		SetShowDeckButtonEnabled(enabled: true);
	}

	private void SetUpDeckListFromScenario(ScenarioDbId scenario, bool useLoadoutOfActiveRun)
	{
		ScenarioDbfRecord scenarioRecord = GameDbf.Scenario.GetRecord((int)scenario);
		if (scenarioRecord != null)
		{
			string deckName;
			string deckDescription;
			List<long> deckContents = CollectionManager.Get().LoadDeckFromDBF(scenarioRecord.Player1DeckId, out deckName, out deckDescription);
			SetUpDeckList(CardWithPremiumStatus.ConvertList(deckContents), useLoadoutOfActiveRun);
		}
	}

	private void SetUpDeckList(List<CardWithPremiumStatus> deckCardList, bool useLoadoutOfActiveRun, bool playGlowAnimation = false, List<long> deckCardIndices = null, List<long> deckCardEnchantments = null, RuneType[] runeOrder = null, Dictionary<string, SideboardDeck> sideboardMap = null)
	{
		if (m_playerHeroData.HeroClasses.Count <= 0 || m_playerHeroData.HeroClasses[0] == TAG_CLASS.INVALID)
		{
			Log.Adventures.PrintError("AdventureDungeonCrawlDisplay.SetUpDeckList() - HeroClasses is INVALID!");
		}
		else
		{
			if (string.IsNullOrEmpty(m_playerHeroData.HeroCardId))
			{
				return;
			}
			m_dungeonCrawlDeck = new CollectionDeck
			{
				HeroCardID = m_playerHeroData.HeroCardId
			};
			m_dungeonCrawlDeck.FormatType = FormatType.FT_WILD;
			m_dungeonCrawlDeck.Type = DeckType.CLIENT_ONLY_DECK;
			if (m_anomalyModeCardDbId != 0L && m_dungeonCrawlData.AnomalyModeActivated)
			{
				string cardId = GameUtils.TranslateDbIdToCardId((int)m_anomalyModeCardDbId);
				if (cardId != null)
				{
					m_dungeonCrawlDeck.AddCard(cardId, TAG_PREMIUM.NORMAL, false, null);
				}
				else
				{
					Log.Adventures.PrintWarning("AdventureDungeonCrawlDisplay.SetUpDeckList() - No cardId for anomalyCardDbId {0}!", m_anomalyModeCardDbId);
				}
			}
			if (m_plotTwistCardDbId != 0L)
			{
				string cardId2 = GameUtils.TranslateDbIdToCardId((int)m_plotTwistCardDbId);
				if (cardId2 != null)
				{
					m_dungeonCrawlDeck.AddCard(cardId2, TAG_PREMIUM.NORMAL, false, null);
				}
				else
				{
					Log.Adventures.PrintWarning("AdventureDungeonCrawlDisplay.SetUpDeckList() - No cardId for m_plotTwistCardDbId {0}!", m_plotTwistCardDbId);
				}
			}
			if (!useLoadoutOfActiveRun && m_dungeonCrawlData.SelectedLoadoutTreasureDbId != 0L)
			{
				string cardId3 = GameUtils.TranslateDbIdToCardId((int)m_dungeonCrawlData.SelectedLoadoutTreasureDbId);
				if (!string.IsNullOrEmpty(cardId3))
				{
					CollectionDeckSlot slot = m_dungeonCrawlDeck.FindFirstSlotByCardId(cardId3);
					if (slot == null || slot.Count == 0)
					{
						m_dungeonCrawlDeck.AddCard(cardId3, TAG_PREMIUM.NORMAL, false, null);
					}
				}
				else
				{
					Log.Adventures.PrintWarning("AdventureDungeonCrawlDisplay.SetUpDeckList() - No cardId for SelectedLoadoutTreasureDbId {0}!", m_dungeonCrawlData.SelectedLoadoutTreasureDbId);
				}
			}
			if (deckCardList != null)
			{
				Dictionary<int, List<int>> enchantmentMap = new Dictionary<int, List<int>>();
				if (deckCardIndices != null && deckCardEnchantments != null && deckCardIndices.Count == deckCardEnchantments.Count)
				{
					for (int i = 0; i < deckCardIndices.Count; i++)
					{
						if (!enchantmentMap.TryGetValue((int)deckCardIndices[i], out var values))
						{
							values = new List<int>();
							enchantmentMap.Add((int)deckCardIndices[i], values);
						}
						values.Add((int)deckCardEnchantments[i]);
					}
				}
				for (int j = 0; j < deckCardList.Count; j++)
				{
					long dbId = deckCardList[j].cardId;
					TAG_PREMIUM premium = deckCardList[j].premium;
					if (dbId == 0L)
					{
						continue;
					}
					string cardId4 = GameUtils.TranslateDbIdToCardId((int)dbId);
					List<int> enchantmentCardIds;
					if (cardId4 == null)
					{
						Log.Adventures.PrintWarning("AdventureDungeonCrawlDisplay.SetUpDeckList() - No cardId for dbId {0}!", dbId);
					}
					else if (enchantmentMap.TryGetValue(j + 1, out enchantmentCardIds))
					{
						m_dungeonCrawlDeck.AddCard_DungeonCrawlBuff(cardId4, premium, enchantmentCardIds);
					}
					else
					{
						m_dungeonCrawlDeck.AddCard(cardId4, premium, false, null);
					}
					if (sideboardMap != null && sideboardMap.ContainsKey(cardId4))
					{
						List<CardWithPremiumStatus> sideboardDeckList = sideboardMap[cardId4].GetCardsWithPremiumStatus();
						for (int k = 0; k < sideboardDeckList.Count; k++)
						{
							string sideboardCardId = GameUtils.TranslateDbIdToCardId((int)sideboardDeckList[k].cardId);
							m_dungeonCrawlDeck.AddCardToSideboardPreferredPremium(sideboardCardId, (int)dbId, sideboardDeckList[k].premium, allowInvalid: false);
						}
					}
				}
			}
			m_dungeonCrawlDeckTray.SetDungeonCrawlDeck(m_dungeonCrawlDeck, playGlowAnimation);
			SetUpCardsCreatedByTreasures();
			SetUpPhoneNewRunScreen();
		}
	}

	private void SetUpHeroPortrait(PlayerHeroData playerHeroData)
	{
		if (m_heroActor == null)
		{
			Log.Adventures.PrintError("Unable to change hero portrait. No hero actor has been loaded.");
		}
		else
		{
			if (string.IsNullOrEmpty(playerHeroData.HeroCardId))
			{
				return;
			}
			bool isInDefeatScreen = IsInDefeatScreen();
			NetCache.CardDefinition favoriteHeroDef = CollectionManager.Get().GetRandomFavoriteHero(playerHeroData.HeroClasses[0], null);
			bool usingGuestHero = m_dungeonCrawlData.GuestHeroesExistForCurrentAdventure();
			TAG_PREMIUM heroActorPremium = TAG_PREMIUM.NORMAL;
			if (!isInDefeatScreen && !usingGuestHero && favoriteHeroDef != null && !GameUtils.IsVanillaHero(favoriteHeroDef.Name))
			{
				heroActorPremium = TAG_PREMIUM.GOLDEN;
			}
			SetHero(playerHeroData.HeroCardId, heroActorPremium);
			if (isInDefeatScreen)
			{
				m_heroActor.GetComponent<Animation>().Play("AllyDefeat_Desat");
			}
			if (m_heroHealth == 0L)
			{
				CardTagDbfRecord healthTag = GameDbf.CardTag.GetRecord((CardTagDbfRecord r) => r.CardId == playerHeroData.HeroCardDbId && r.TagId == 45);
				m_heroHealth = healthTag.TagValue;
			}
			SetHeroHealthVisual(m_heroActor, !isInDefeatScreen);
			if (m_dungeonCrawlDeckSelect != null)
			{
				SetHeroHealthVisual(m_dungeonCrawlDeckSelect.heroDetails.HeroActor, !isInDefeatScreen);
			}
		}
	}

	private void SetHero(string cardID, TAG_PREMIUM premium)
	{
		if (m_heroActor == null)
		{
			Log.Adventures.PrintError("AdventureDungeonCrawlDisplay.SetHero was called but m_heroActor was null");
			return;
		}
		using DefLoader.DisposableFullDef fullDef = DefLoader.Get().GetFullDef(cardID);
		if (!(fullDef?.CardDef == null) && fullDef.EntityDef != null)
		{
			m_heroActor.SetCardDef(fullDef.DisposableCardDef);
			m_heroActor.SetEntityDef(fullDef.EntityDef);
			fullDef.CardDef.m_AlwaysRenderPremiumPortrait = true;
			m_heroActor.SetPremium(premium);
			m_heroActor.UpdateAllComponents();
			m_heroActor.Show();
			m_heroClassIconsControllerReference.RegisterReadyListener<Widget>(OnHeroClassIconsControllerReady);
		}
	}

	private void SetHeroPower(string cardID)
	{
		if (m_heroPowerActor == null)
		{
			Log.Adventures.PrintError("AdventureDungeonCrawlDisplay.SetHeroPower was called but m_heroPowerActor was null.");
			return;
		}
		BoxCollider collider = m_heroPowerActor.GetComponent<BoxCollider>();
		if (collider != null)
		{
			collider.enabled = false;
		}
		if (string.IsNullOrEmpty(cardID))
		{
			m_heroPowerActor.Hide();
			return;
		}
		DefLoader.DisposableFullDef fullDef = DefLoader.Get().GetFullDef(cardID);
		if (!(fullDef?.CardDef == null) && fullDef?.EntityDef != null)
		{
			m_currentHeroPowerFullDef?.Dispose();
			m_currentHeroPowerFullDef = fullDef;
			m_heroPowerActor.SetFullDef(fullDef);
			m_heroPowerActor.UpdateAllComponents();
			m_heroPowerActor.Show();
			if (collider != null)
			{
				collider.enabled = true;
			}
		}
	}

	private void SetUpPhoneNewRunScreen()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_dungeonCrawlDeckTray.gameObject.SetActive(value: false);
			bool deckHasCards = m_dungeonCrawlDeck != null && m_dungeonCrawlDeck.GetTotalCardCount() > 0;
			SetShowDeckButtonEnabled(deckHasCards);
		}
	}

	public void SetShowDeckButtonEnabled(bool enabled)
	{
		if ((bool)UniversalInputManager.UsePhoneUI && enabled != m_ShowDeckButton.IsEnabled())
		{
			m_ShowDeckButton.SetEnabled(enabled);
			m_ShowDeckButton.Flip(enabled);
		}
	}

	private void SetUpPhoneRunCompleteScreen()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_ShowDeckButtonFrame.SetActive(value: false);
			m_ShowDeckNoButtonFrame.SetActive(value: false);
			m_TrayFrameDefault.SetActive(value: false);
			m_TrayFrameRunComplete.SetActive(value: true);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_dungeonCrawlDeckTray.gameObject.SetActive(value: false);
			}
			GameUtils.SetParent(m_AdventureTitle, m_AdventureTitleRunCompleteBone);
			m_PhoneDeckTray.SetActive(value: true);
			GameUtils.SetParent(m_PhoneDeckTray, m_DeckTrayRunCompleteBone);
			TraySection headerTraySection = (TraySection)GameUtils.Instantiate(m_DeckListHeaderPrefab, m_DeckListHeaderRunCompleteBone, withRotation: true);
			m_dungeonCrawlDeckTray.OffsetDeckBigCardByVector(m_DeckBigCardOffsetForRunCompleteState);
			headerTraySection.m_deckBox.m_neverUseGoldenPortraits = IsInDefeatScreen();
			headerTraySection.m_deckBox.SetHeroCardID(m_playerHeroData.HeroCardId, null);
			headerTraySection.m_deckBox.HideBanner();
			headerTraySection.m_deckBox.SetDeckName(GetClassNameFromDeckClass(m_playerHeroData.HeroClasses[0]));
			headerTraySection.m_deckBox.HideRenameVisuals();
			headerTraySection.m_deckBox.SetDeckNameAsSingleLine(forceSingleLine: true);
			if (IsInDefeatScreen())
			{
				headerTraySection.m_deckBox.PlayDesaturationAnimation();
			}
		}
	}

	private bool IsInDefeatScreen()
	{
		if (m_playMat.GetPlayMatState() == AdventureDungeonCrawlPlayMat.PlayMatState.SHOWING_BOSS_GRAVEYARD)
		{
			return m_numBossesDefeated < m_numBossesInRun;
		}
		return false;
	}

	private void SetUpCardsCreatedByTreasures()
	{
		if (m_cardsAddedToDeckMap != null && m_cardsAddedToDeckMap.Count != 0 && m_cardsAddedToDeckMap.Count % 2 != 1)
		{
			Dictionary<long, long> cardIdToCreatorMap = new Dictionary<long, long>();
			for (int i = 0; i < m_cardsAddedToDeckMap.Count; i += 2)
			{
				cardIdToCreatorMap[m_cardsAddedToDeckMap[i]] = m_cardsAddedToDeckMap[i + 1];
			}
			m_dungeonCrawlDeckTray.CardIdToCreatorMap = cardIdToCreatorMap;
		}
	}

	public static bool OnNavigateBack()
	{
		if (m_instance == null)
		{
			Debug.LogError("Trying to navigate back, but AdventureDungeonCrawlDisplay has been destroyed!");
			return false;
		}
		AdventureDungeonCrawlPlayMat playMat = m_instance.m_playMat;
		if (playMat != null)
		{
			if (playMat.GetPlayMatState() == AdventureDungeonCrawlPlayMat.PlayMatState.TRANSITIONING_FROM_PREV_STATE)
			{
				return false;
			}
			playMat.HideBossHeroPowerTooltip(immediate: true);
		}
		if ((bool)UniversalInputManager.UsePhoneUI && m_instance.m_dungeonCrawlDeckTray != null)
		{
			m_instance.m_dungeonCrawlDeckTray.gameObject.SetActive(value: false);
		}
		if (playMat != null && playMat.GetPlayMatState() == AdventureDungeonCrawlPlayMat.PlayMatState.SHOWING_BOSS_GRAVEYARD && m_instance.m_mustSelectChapter)
		{
			if (m_instance.m_subsceneController != null)
			{
				m_instance.m_subsceneController.ChangeSubScene(AdventureData.Adventuresubscene.LOCATION_SELECT, pushToBackStack: false);
			}
		}
		else if (m_instance.m_subsceneController != null)
		{
			m_instance.m_subsceneController.SubSceneGoBack();
		}
		return true;
	}

	private void OnBackButtonPress(UIEvent e)
	{
		EnableBackButton(enabled: false);
		Navigation.GoBack();
	}

	private void GoToHeroSelectSubscene()
	{
		bool num = m_dungeonCrawlData.GuestHeroesExistForCurrentAdventure();
		m_playMat.PlayButton.Disable();
		AdventureData.Adventuresubscene subscene = (num ? AdventureData.Adventuresubscene.ADVENTURER_PICKER : AdventureData.Adventuresubscene.MISSION_DECK_PICKER);
		if (m_subsceneController != null)
		{
			m_subsceneController.ChangeSubScene(subscene);
		}
	}

	private void GoBackToHeroPower()
	{
		m_dungeonCrawlData.SelectedHeroPowerDbId = 0L;
		SetHeroPower(null);
		StartCoroutine(ShowHeroPowerOptionsWhenReady());
	}

	private void GoBackFromHeroPower()
	{
		m_playMat.PlayHeroPowerOptionSelected();
	}

	private void GoBackToTreasureLoadoutSelection()
	{
		SetUpDeckList(new List<CardWithPremiumStatus>(), useLoadoutOfActiveRun: false, playGlowAnimation: true);
		m_dungeonCrawlData.SelectedLoadoutTreasureDbId = 0L;
		StartCoroutine(ShowTreasureSatchelWhenReady());
	}

	private void GoBackFromTreasureLoadoutSelection()
	{
		m_playMat.PlayTreasureSatchelOptionHidden();
	}

	private void GoBackFromDeckTemplateSelection()
	{
		m_dungeonCrawlData.SelectedDeckId = 0L;
		SetUpDeckList(new List<CardWithPremiumStatus>(), useLoadoutOfActiveRun: false, playGlowAnimation: true);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_dungeonCrawlDeckSelect == null || !m_dungeonCrawlDeckSelect.isReady)
			{
				Error.AddDevWarning("UI Error!", "AdventureDeckSelectWidget is not setup correctly or is not ready!");
				return;
			}
			m_dungeonCrawlDeckSelect.deckTray.SetDungeonCrawlDeck(m_dungeonCrawlDeck, playGlowAnimation: false);
			m_dungeonCrawlDeckSelect.playButton.Disable();
		}
		else
		{
			m_playMat.PlayButton.Disable();
		}
		m_playMat.DeselectAllDeckOptionsWithoutId(0);
		m_playMat.PlayDeckOptionSelected();
	}

	private static bool OnNavigateBackFromCurrentLoadoutState()
	{
		AdventureDungeonCrawlPlayMat playMat = m_instance.m_playMat;
		if (playMat != null && playMat.GetPlayMatState() == AdventureDungeonCrawlPlayMat.PlayMatState.TRANSITIONING_FROM_PREV_STATE)
		{
			return false;
		}
		if (m_instance == null)
		{
			Debug.LogError("Trying to navigate back to previous loadout selection, but AdventureDungeonCrawlDisplay has been destroyed!");
			return false;
		}
		switch (m_instance.m_currentLoadoutState)
		{
		case DungeonRunLoadoutState.HEROPOWER:
			m_instance.GoBackFromHeroPower();
			if (m_instance.m_dungeonCrawlData.SelectableLoadoutTreasuresExist())
			{
				m_instance.GoBackToTreasureLoadoutSelection();
			}
			break;
		case DungeonRunLoadoutState.TREASURE:
			m_instance.GoBackFromTreasureLoadoutSelection();
			break;
		case DungeonRunLoadoutState.DECKTEMPLATE:
			m_instance.GoBackFromDeckTemplateSelection();
			m_instance.GoBackToHeroPower();
			break;
		}
		return true;
	}

	private void GoToNextLoadoutState()
	{
		switch (m_currentLoadoutState)
		{
		case DungeonRunLoadoutState.HEROPOWER:
			StartCoroutine(ShowDeckOptionsWhenReady());
			break;
		case DungeonRunLoadoutState.TREASURE:
			StartCoroutine(ShowHeroPowerOptionsWhenReady());
			break;
		case DungeonRunLoadoutState.INVALID:
			if (m_dungeonCrawlData.SelectableLoadoutTreasuresExist())
			{
				StartCoroutine(ShowTreasureSatchelWhenReady());
			}
			else if (m_dungeonCrawlData.SelectableHeroPowersAndDecksExist())
			{
				StartCoroutine(ShowHeroPowerOptionsWhenReady());
			}
			break;
		case DungeonRunLoadoutState.DECKTEMPLATE:
			break;
		}
	}

	private void LockInDuelsLoadoutSelections()
	{
		Navigation.RemoveHandler(OnNavigateBackFromCurrentLoadoutState);
		Navigation.RemoveHandler(GuestHeroPickerTrayDisplay.OnNavigateBack);
		if (m_dungeonCrawlData.HasValidLoadoutForSelectedAdventure())
		{
			List<GameSaveDataManager.SubkeySaveRequest> requests = new List<GameSaveDataManager.SubkeySaveRequest>();
			m_dungeonCrawlData.GetSelectedAdventureDataRecord();
			requests.Add(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_POWER, m_dungeonCrawlData.SelectedHeroPowerDbId));
			requests.Add(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_LOADOUT_TREASURE_ID, m_dungeonCrawlData.SelectedLoadoutTreasureDbId));
			if (m_saveHeroDataUsingHeroId)
			{
				requests.Add(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_CARD_DB_ID, m_dungeonCrawlData.SelectedHeroCardDbId));
			}
			else
			{
				TAG_CLASS heroClass = AdventureUtils.GetHeroClassFromHeroId((int)m_dungeonCrawlData.SelectedHeroCardDbId);
				requests.Add(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_CLASS, (long)heroClass));
			}
			GameSaveDataManager.Get().SaveSubkeys(requests);
			m_BackButton.SetText("GLOBAL_LEAVE");
		}
	}

	private void LockInNewRunSelectionsAndTransition()
	{
		Navigation.RemoveHandler(OnNavigateBackFromCurrentLoadoutState);
		Navigation.RemoveHandler(GuestHeroPickerTrayDisplay.OnNavigateBack);
		Navigation.RemoveHandler(AdventureLocationSelectBook.OnNavigateBack);
		Navigation.RemoveHandler(AdventureBookPageManager.NavigateToMapPage);
		if (m_subsceneController != null)
		{
			m_subsceneController.RemoveSubScenesFromStackUntilTargetReached(AdventureData.Adventuresubscene.CHOOSER);
		}
		if (m_dungeonCrawlData.HasValidLoadoutForSelectedAdventure())
		{
			List<GameSaveDataManager.SubkeySaveRequest> requests = new List<GameSaveDataManager.SubkeySaveRequest>();
			AdventureDataDbfRecord dataRecord = m_dungeonCrawlData.GetSelectedAdventureDataRecord();
			if (dataRecord.DungeonCrawlSelectChapter)
			{
				requests.Add(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_SCENARIO_ID, (long)m_dungeonCrawlData.GetMission()));
			}
			if (dataRecord.DungeonCrawlPickHeroFirst)
			{
				if (m_saveHeroDataUsingHeroId)
				{
					requests.Add(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_CARD_DB_ID, m_dungeonCrawlData.SelectedHeroCardDbId));
				}
				else
				{
					TAG_CLASS heroClass = AdventureUtils.GetHeroClassFromHeroId((int)m_dungeonCrawlData.SelectedHeroCardDbId);
					requests.Add(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_CLASS, (long)heroClass));
				}
			}
			if (m_dungeonCrawlData.SelectableHeroPowersExist())
			{
				requests.Add(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_POWER, m_dungeonCrawlData.SelectedHeroPowerDbId));
			}
			if (m_dungeonCrawlData.SelectableDecksExist())
			{
				requests.Add(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_DECK, m_dungeonCrawlData.SelectedDeckId));
			}
			if (m_dungeonCrawlData.SelectableLoadoutTreasuresExist())
			{
				requests.Add(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_LOADOUT_TREASURE_ID, m_dungeonCrawlData.SelectedLoadoutTreasureDbId));
			}
			long isAnomalyModeSelected = (m_dungeonCrawlData.AnomalyModeActivated ? 1 : 0);
			requests.Add(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_ANOMALY_MODE, isAnomalyModeSelected));
			GameSaveDataManager.Get().SaveSubkeys(requests);
			SetShowDeckButtonEnabled(enabled: true);
			m_BackButton.SetText("GLOBAL_LEAVE");
			StartCoroutine(SetPlayMatStateFromGameSaveDataWhenReady());
		}
		else
		{
			Navigation.GoBack();
		}
	}

	private void OnPlayButtonPress(UIEvent e)
	{
		PlayButton playButton = e.GetElement() as PlayButton;
		if (playButton != null)
		{
			playButton.Disable();
		}
		m_playMat.HideBossHeroPowerTooltip(immediate: true);
		if (m_dungeonCrawlData.DoesSelectedMissionRequireDeck() && !m_dungeonCrawlData.HeroIsSelectedBeforeDungeonCrawlScreenForSelectedAdventure() && !m_shouldSkipHeroSelect && (m_numBossesDefeated == 0 || !m_isRunActive))
		{
			GoToHeroSelectSubscene();
		}
		else if (m_playMat.GetPlayMatState() == AdventureDungeonCrawlPlayMat.PlayMatState.SHOWING_OPTIONS && m_playMat.GetPlayMatOptionType() == AdventureDungeonCrawlPlayMat.OptionType.DECK)
		{
			m_playMat.PlayDeckOptionSelected();
			LockInNewRunSelectionsAndTransition();
		}
		else
		{
			QueueForGame();
		}
	}

	private int GetHeroCardIdToUse()
	{
		if (m_dungeonCrawlData.GetGuestHeroesForCurrentAdventure().Count > 0 && (!m_shouldSkipHeroSelect || m_mustPickShrine))
		{
			int heroToUse = (int)m_dungeonCrawlData.SelectedHeroCardDbId;
			if (m_isRunActive || m_mustPickShrine)
			{
				heroToUse = m_playerHeroData.HeroCardDbId;
			}
			return heroToUse;
		}
		return GameUtils.GetFavoriteHeroCardDBIdFromClass(m_playerHeroData.HeroClasses[0]);
	}

	private void QueueForGame()
	{
		int heroToQueueWith = GetHeroCardIdToUse();
		GameMgr.Get().FindGameWithHero(m_dungeonCrawlData.GameType, FormatType.FT_WILD, (int)m_dungeonCrawlData.GetMissionToPlay(), 0, heroToQueueWith, 0L);
	}

	private void OnShowDeckButtonPress(UIEvent e)
	{
		ShowMobileDeckTray(m_dungeonCrawlDeckTray.gameObject.GetComponent<SlidingTray>());
	}

	protected void OnSubSceneLoaded(object sender, EventArgs args)
	{
		m_playMat.OnSubSceneLoaded();
		m_playMat.SetRewardOptionSelectedCallback(OnRewardOptionSelected);
		m_playMat.SetTreasureSatchelOptionSelectedCallback(OnTreasureSatchelOptionSelected);
		m_playMat.SetHeroPowerOptionCallback(OnHeroPowerOptionSelected, OnHeroPowerOptionRollover, OnHeroPowerOptionRollout);
		m_playMat.SetDeckOptionSelectedCallback(OnDeckOptionSelected);
	}

	protected void OnSubSceneTransitionComplete(object sender, EventArgs args)
	{
		m_subsceneTransitionComplete = true;
		if (m_dungeonCrawlDeckTray != null)
		{
			m_dungeonCrawlDeckTray.gameObject.SetActive(value: true);
		}
		if (m_playMat != null)
		{
			m_playMat.OnSubSceneTransitionComplete();
		}
	}

	private void OnHeroActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"AdventureDungeonCrawlDisplay.OnHeroActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		m_heroActor = go.GetComponent<Actor>();
		if (m_heroActor == null)
		{
			Debug.LogWarning($"AdventureDungeonCrawlDisplay.OnHeroActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		m_heroActor.transform.parent = m_socketHeroBone.transform;
		m_heroActor.transform.localPosition = Vector3.zero;
		m_heroActor.transform.localScale = Vector3.one;
		m_heroActor.Hide();
	}

	private void OnHeroPowerActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"AdventureDungeonCrawlDisplay.OnHeroPowerActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		m_heroPowerActor = go.GetComponent<Actor>();
		if (m_heroPowerActor == null)
		{
			Debug.LogWarning($"AdventureDungeonCrawlDisplay.OnHeroPowerActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		m_heroPowerActor.transform.parent = m_heroPowerBone;
		m_heroPowerActor.transform.localPosition = Vector3.zero;
		m_heroPowerActor.transform.localRotation = Quaternion.identity;
		m_heroPowerActor.transform.localScale = Vector3.one;
		m_heroPowerActor.Hide();
		m_heroPowerActor.SetUnlit();
		PegUIElement pegUIElement = go.AddComponent<PegUIElement>();
		go.AddComponent<BoxCollider>().enabled = false;
		pegUIElement.AddEventListener(UIEventType.ROLLOVER, delegate
		{
			ShowBigCard(m_heroPowerBigCard, m_currentHeroPowerFullDef, m_HeroPowerBigCardBone);
		});
		pegUIElement.AddEventListener(UIEventType.ROLLOUT, delegate
		{
			BigCardHelper.HideBigCard(m_heroPowerBigCard);
		});
	}

	private void SetHeroHealthVisual(Actor actor, bool show)
	{
		if (actor == null)
		{
			Log.Adventures.PrintError("SetHeroHealthVisual: actor provided is null!");
			return;
		}
		actor.GetHealthObject().gameObject.SetActive(show);
		if (show)
		{
			actor.GetHealthText().gameObject.SetActive(value: true);
			actor.GetHealthText().Text = Convert.ToString(m_heroHealth);
			actor.GetHealthText().AmbientLightBlend = 0f;
		}
	}

	private IEnumerator ShowTreasureSatchelWhenReady()
	{
		while (m_playMat == null || m_playMat.GetPlayMatState() == AdventureDungeonCrawlPlayMat.PlayMatState.TRANSITIONING_FROM_PREV_STATE)
		{
			yield return null;
		}
		List<AdventureLoadoutTreasuresDbfRecord> adventureLoadoutTreasures = AdventureUtils.GetTreasuresForDungeonCrawlHero(m_dungeonCrawlData, (int)m_dungeonCrawlData.SelectedHeroCardDbId);
		m_playMat.ShowTreasureSatchel(adventureLoadoutTreasures, m_gameSaveDataServerKey, m_gameSaveDataClientKey);
		m_currentLoadoutState = DungeonRunLoadoutState.TREASURE;
		EnableBackButton(enabled: true);
	}

	private IEnumerator ShowHeroPowerOptionsWhenReady()
	{
		while (m_playMat == null || m_playMat.GetPlayMatState() == AdventureDungeonCrawlPlayMat.PlayMatState.TRANSITIONING_FROM_PREV_STATE)
		{
			yield return null;
		}
		List<AdventureHeroPowerDbfRecord> adventureHeroPowers = AdventureUtils.GetHeroPowersForDungeonCrawlHero(m_dungeonCrawlData, (int)m_dungeonCrawlData.SelectedHeroCardDbId);
		m_playMat.ShowHeroPowers(adventureHeroPowers, m_gameSaveDataServerKey, m_gameSaveDataClientKey);
		m_currentLoadoutState = DungeonRunLoadoutState.HEROPOWER;
		EnableBackButton(enabled: true);
		if (m_instance.m_dungeonCrawlData.SelectableLoadoutTreasuresExist())
		{
			Navigation.PushUnique(OnNavigateBackFromCurrentLoadoutState);
		}
	}

	private IEnumerator ShowDeckOptionsWhenReady()
	{
		while (m_playMat == null || m_playMat.GetPlayMatState() == AdventureDungeonCrawlPlayMat.PlayMatState.TRANSITIONING_FROM_PREV_STATE)
		{
			yield return null;
		}
		List<AdventureDeckDbfRecord> adventureDecks = m_dungeonCrawlData.GetDecksForClass(m_playerHeroData.HeroClasses[0]);
		m_playMat.ShowDecks(adventureDecks, m_gameSaveDataServerKey, m_gameSaveDataClientKey);
		m_currentLoadoutState = DungeonRunLoadoutState.DECKTEMPLATE;
		EnableBackButton(enabled: true);
		Navigation.PushUnique(OnNavigateBackFromCurrentLoadoutState);
	}

	private void OnBossFullDefLoaded(string cardId, DefLoader.DisposableFullDef def, object userData)
	{
		using (def)
		{
			if (def == null)
			{
				Log.Adventures.PrintError("Unable to load {0} hero def for Dungeon Crawl boss.", cardId);
				m_assetLoadingHelper.AssetLoadCompleted();
				return;
			}
			string heroPowerCardId = null;
			string heroCardId = def.EntityDef.GetCardId();
			if (GameUtils.IsModeHeroic(m_dungeonCrawlData.GetSelectedMode()))
			{
				int heroPowerId = GameUtils.GetCardTagValue(heroCardId, GAME_TAG.HEROIC_HERO_POWER);
				if (heroPowerId != 0)
				{
					heroPowerCardId = GameUtils.TranslateDbIdToCardId(heroPowerId);
				}
			}
			if (string.IsNullOrEmpty(heroPowerCardId))
			{
				heroPowerCardId = GameUtils.GetHeroPowerCardIdFromHero(heroCardId);
			}
			if (!string.IsNullOrEmpty(heroPowerCardId))
			{
				m_assetLoadingHelper.AddAssetToLoad();
				DefLoader.Get().LoadFullDef(heroPowerCardId, OnBossPowerFullDefLoaded);
			}
			EntityDef entityDef = def.EntityDef;
			if (entityDef != null && m_nextBossHealth != 0L && !m_isRunRetired)
			{
				entityDef = entityDef.Clone();
				entityDef.SetTag(GAME_TAG.HEALTH, m_nextBossHealth);
			}
			if (IsNextMissionASpecialEncounter() && m_bossActor != null && m_bossActor.GetHealthObject() != null)
			{
				m_bossActor.GetHealthObject().Hide();
			}
			m_playMat.SetBossFullDef(def.DisposableCardDef, entityDef);
			m_assetLoadingHelper.AssetLoadCompleted();
		}
	}

	private void OnBossPowerFullDefLoaded(string cardId, DefLoader.DisposableFullDef def, object userData)
	{
		if (def == null)
		{
			Debug.LogError($"Unable to load {cardId} hero power def for Dungeon Crawl boss.", base.gameObject);
			m_assetLoadingHelper.AssetLoadCompleted();
		}
		else
		{
			m_currentBossHeroPowerFullDef?.Dispose();
			m_currentBossHeroPowerFullDef = def;
			m_assetLoadingHelper.AssetLoadCompleted();
		}
	}

	private void OnTreasureSatchelOptionSelected(long treasureLoadoutDbId)
	{
		m_dungeonCrawlData.SelectedLoadoutTreasureDbId = treasureLoadoutDbId;
		AdventureConfig.Get().SelectedLoadoutTreasureDbId = treasureLoadoutDbId;
		if (m_dungeonCrawlData.SelectableHeroPowersAndDecksExist())
		{
			List<AdventureLoadoutTreasuresDbfRecord> treasureRecords = AdventureUtils.GetTreasuresForDungeonCrawlHero(m_dungeonCrawlData, (int)m_dungeonCrawlData.SelectedHeroCardDbId);
			int selectedTreasureIndex = treasureRecords.FindIndex((AdventureLoadoutTreasuresDbfRecord r) => r.CardId == treasureLoadoutDbId);
			AdventureDungeonCrawlRewardOption.OptionData optionData = new AdventureDungeonCrawlRewardOption.OptionData(AdventureDungeonCrawlPlayMat.OptionType.TREASURE_SATCHEL, new List<long> { treasureLoadoutDbId }, selectedTreasureIndex);
			if (selectedTreasureIndex >= 0 && selectedTreasureIndex < treasureRecords.Count)
			{
				_ = treasureRecords[selectedTreasureIndex];
			}
			OnRewardOptionSelected(optionData);
			m_playMat.PlayTreasureSatchelOptionSelected();
			GoToNextLoadoutState();
		}
		else
		{
			Debug.LogWarning("AdventureDungeonCrawlDisplay.OnTreasureLoadoutOptionSelected: Selecting a Treasure Loadout but no Hero Power or Deck is not supported!");
		}
	}

	private int GetGuestHeroCardDbIdForCurrentAdventure(int guestHeroId)
	{
		foreach (GuestHero guestHero in m_dungeonCrawlData.GetGuestHeroesForCurrentAdventure())
		{
			if (guestHero.guestHeroId == guestHeroId)
			{
				return guestHero.cardDbId;
			}
		}
		return 0;
	}

	private void UpdateHeroFromTreasure(AdventureLoadoutTreasuresDbfRecord selectedTreasure)
	{
		m_dungeonCrawlData.SelectedHeroCardDbId = GetGuestHeroCardDbIdForCurrentAdventure(selectedTreasure.GuestHeroVariantId);
		m_playerHeroData.UpdateHeroDataFromHeroCardDbId((int)m_dungeonCrawlData.SelectedHeroCardDbId);
		SetUpHeroPortrait(m_playerHeroData);
	}

	private void OnHeroPowerOptionSelected(long heroPowerDbId, bool isLocked)
	{
		if (!isLocked)
		{
			m_dungeonCrawlData.SelectedHeroPowerDbId = heroPowerDbId;
			AdventureConfig.Get().SelectedHeroPowerDbId = heroPowerDbId;
			SetHeroPower(GameUtils.TranslateDbIdToCardId((int)heroPowerDbId));
			if (m_HeroPowerPortraitPlayMaker != null && !string.IsNullOrEmpty(m_HeroPowerPotraitIntroStateName))
			{
				m_HeroPowerPortraitPlayMaker.SendEvent(m_HeroPowerPotraitIntroStateName);
			}
			m_playMat.PlayHeroPowerOptionSelected();
			GoToNextLoadoutState();
		}
	}

	private void OnHeroPowerOptionRollover(long heroPowerDbId, GameObject bone)
	{
		GameUtils.SetParent(m_heroPowerBigCard, bone);
		using DefLoader.DisposableFullDef heroPowerDef = DefLoader.Get().GetFullDef(GameUtils.TranslateDbIdToCardId((int)heroPowerDbId));
		ShowBigCard(m_heroPowerBigCard, heroPowerDef, m_HeroPowerBigCardBone);
	}

	private void OnHeroPowerOptionRollout(long heroPowerDbId, GameObject bone)
	{
		BigCardHelper.HideBigCard(m_heroPowerBigCard);
		GameUtils.SetParent(m_heroPowerBigCard, m_HeroPowerBigCardBone);
	}

	private void OnDeckOptionSelected(int deckId, List<long> deckContent, bool deckIsLocked)
	{
		m_playMat.DeselectAllDeckOptionsWithoutId(deckId);
		m_dungeonCrawlData.SelectedDeckId = deckId;
		SetUpDeckList(CardWithPremiumStatus.ConvertList(deckContent), useLoadoutOfActiveRun: false, playGlowAnimation: true);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_dungeonCrawlDeckSelect == null || !m_dungeonCrawlDeckSelect.isReady)
			{
				Error.AddDevWarning("UI Error!", "AdventureDeckSelectWidget is not setup correctly or is not ready!");
				return;
			}
			m_dungeonCrawlDeckSelect.deckTray.SetDungeonCrawlDeck(m_dungeonCrawlDeck, playGlowAnimation: false);
			using (DefLoader.DisposableCardDef heroDef = m_heroActor.ShareDisposableCardDef())
			{
				m_dungeonCrawlDeckSelect.heroDetails.UpdateHeroInfo(m_heroActor.GetEntityDef(), heroDef);
			}
			using (DefLoader.DisposableCardDef heroPowerDef = m_heroPowerActor.ShareDisposableCardDef())
			{
				m_dungeonCrawlDeckSelect.heroDetails.UpdateHeroPowerInfo(m_heroPowerActor.GetEntityDef(), heroPowerDef);
			}
			if (deckIsLocked)
			{
				m_dungeonCrawlDeckSelect.playButton.Disable();
			}
			else
			{
				m_dungeonCrawlDeckSelect.playButton.Enable();
			}
			ShowMobileDeckTray(m_dungeonCrawlDeckSelect.slidingTray);
		}
		else if (deckIsLocked)
		{
			m_playMat.PlayButton.Disable();
		}
		else
		{
			m_playMat.PlayButton.Enable();
		}
		GoToNextLoadoutState();
	}

	private void OnRewardOptionSelected(AdventureDungeonCrawlRewardOption.OptionData optionData)
	{
		if (!GameSaveDataManager.Get().IsDataReady(m_gameSaveDataServerKey))
		{
			Log.Adventures.PrintError("Attempting to make a selection, but no data is ready yet!");
			return;
		}
		if (m_playMat.GetPlayMatState() != AdventureDungeonCrawlPlayMat.PlayMatState.SHOWING_OPTIONS)
		{
			Log.Adventures.PrintError("Attempting to choose a reward, but the Play Mat is not currently in the 'SHOWING_OPTIONS' state!");
			return;
		}
		GameSaveKeySubkeyId subkeyForChoice = GameSaveKeySubkeyId.INVALID;
		switch (optionData.optionType)
		{
		case AdventureDungeonCrawlPlayMat.OptionType.TREASURE:
			subkeyForChoice = GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_TREASURE;
			break;
		case AdventureDungeonCrawlPlayMat.OptionType.LOOT:
			subkeyForChoice = GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_LOOT;
			break;
		case AdventureDungeonCrawlPlayMat.OptionType.SHRINE_TREASURE:
			subkeyForChoice = GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_SHRINE;
			break;
		}
		m_dungeonCrawlDeckTray.gameObject.SetActive(value: true);
		Action onCardAddedToDeckListCallback = null;
		if (optionData.optionType == AdventureDungeonCrawlPlayMat.OptionType.SHRINE_TREASURE)
		{
			if (m_shrineOptions == null || m_shrineOptions.Count == 0)
			{
				Log.Adventures.PrintError("OnRewardOptionSelected: Player selected a shrine, but there are no shrine options!");
				return;
			}
			long shrineCardId = m_shrineOptions[optionData.index];
			m_playerHeroData.UpdateHeroDataFromClass(GetClassFromShrine(shrineCardId));
			SetUpDeckList(new List<CardWithPremiumStatus>(), useLoadoutOfActiveRun: false);
			onCardAddedToDeckListCallback = delegate
			{
				SetUpDeckListFromShrine(shrineCardId, playDeckGlowAnimation: true);
				ChangeHeroPortrait(m_playerHeroData.HeroCardId, TAG_PREMIUM.NORMAL);
			};
		}
		for (int i = ((subkeyForChoice == GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_LOOT) ? 1 : 0); i < optionData.options.Count; i++)
		{
			string cardId = GameUtils.TranslateDbIdToCardId((int)optionData.options[i], showWarning: true);
			if (!string.IsNullOrEmpty(cardId))
			{
				Actor actorToAnimateFrom = null;
				if (!UniversalInputManager.UsePhoneUI)
				{
					actorToAnimateFrom = m_playMat.GetActorToAnimateFrom(cardId, optionData.index);
				}
				m_dungeonCrawlDeckTray.AddCard(cardId, actorToAnimateFrom, onCardAddedToDeckListCallback);
			}
		}
		TooltipPanelManager.Get().HideKeywordHelp();
		if (optionData.optionType != AdventureDungeonCrawlPlayMat.OptionType.TREASURE_SATCHEL)
		{
			List<GameSaveDataManager.SubkeySaveRequest> updates = new List<GameSaveDataManager.SubkeySaveRequest>();
			updates.Add(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, subkeyForChoice, optionData.index + 1));
			GameSaveDataManager.Get().SaveSubkeys(updates);
			m_playMat.PlayRewardOptionSelected(optionData);
			StartCoroutine(SetPlayMatStateFromGameSaveDataWhenReady());
		}
		PlayRewardSelectVO(optionData);
	}

	private void PlayRewardSelectVO(AdventureDungeonCrawlRewardOption.OptionData optionData)
	{
		if (optionData.optionType != AdventureDungeonCrawlPlayMat.OptionType.TREASURE && optionData.optionType != AdventureDungeonCrawlPlayMat.OptionType.SHRINE_TREASURE)
		{
			return;
		}
		WingDbId wingId = GameUtils.GetWingIdFromMissionId(m_dungeonCrawlData.GetMission());
		if (DungeonCrawlSubDef_VOLines.GetNextValidEventType(m_dungeonCrawlData.GetSelectedAdventure(), wingId, m_playerHeroData.HeroCardDbId, DungeonCrawlSubDef_VOLines.OFFER_LOOT_PACKS_EVENTS) == DungeonCrawlSubDef_VOLines.VOEventType.INVALID)
		{
			int treasureID = AdventureDungeonCrawlRewardOption.GetTreasureDatabaseID(optionData);
			if (treasureID != 47251 || Options.Get().GetBool(Option.HAS_JUST_SEEN_LOOT_NO_TAKE_CANDLE_VO))
			{
				DungeonCrawlSubDef_VOLines.PlayVOLine(m_dungeonCrawlData.GetSelectedAdventure(), wingId, m_playerHeroData.HeroCardDbId, DungeonCrawlSubDef_VOLines.VOEventType.TAKE_TREASURE_GENERAL, treasureID);
			}
		}
	}

	private bool ShouldShowRunCompletedScreen()
	{
		if (m_defeatedBossIds == null && m_bossWhoDefeatedMeId == 0L)
		{
			return false;
		}
		if (!m_isRunActive && !m_isRunRetired)
		{
			return !m_hasSeenLatestDungeonRunComplete;
		}
		return false;
	}

	private void ShowMobileDeckTray(SlidingTray tray)
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			return;
		}
		if (tray == null)
		{
			Debug.LogError("ToggleMobileDeckTray: Could not find SlidingTray on Dungeon Crawl Deck Tray.");
			return;
		}
		m_playMat.HideBossHeroPowerTooltip(immediate: true);
		SetHeroHealthVisual(m_heroActor, show: true);
		SlidingTray.TrayToggledListener trayListener = null;
		trayListener = delegate(bool shown)
		{
			OnMobileDeckTrayToggled(tray, shown, trayListener);
		};
		tray.RegisterTrayToggleListener(trayListener);
		tray.ToggleTraySlider(show: true);
	}

	private void OnMobileDeckTrayToggled(SlidingTray tray, bool shown, SlidingTray.TrayToggledListener trayListener)
	{
		if (!shown)
		{
			tray.UnregisterTrayToggleListener(trayListener);
			m_playMat.ShowBossHeroPowerTooltip();
		}
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		switch (eventData.m_state)
		{
		case FindGameState.CLIENT_CANCELED:
		case FindGameState.CLIENT_ERROR:
		case FindGameState.BNET_QUEUE_CANCELED:
		case FindGameState.BNET_ERROR:
		case FindGameState.SERVER_GAME_CANCELED:
			HandleGameStartupFailure();
			break;
		case FindGameState.SERVER_GAME_STARTED:
			if (m_subsceneController != null)
			{
				m_subsceneController.RemoveSubSceneIfOnTopOfStack(AdventureData.Adventuresubscene.ADVENTURER_PICKER);
				m_subsceneController.RemoveSubSceneIfOnTopOfStack(AdventureData.Adventuresubscene.MISSION_DECK_PICKER);
			}
			break;
		}
		return false;
	}

	private void HandleGameStartupFailure()
	{
		EnablePlayButton();
	}

	private IEnumerator ShowDemoThankQuote()
	{
		string thankQuote = Vars.Key("Demo.DungeonThankQuote").GetStr("");
		float delaySeconds = Vars.Key("Demo.DungeonThankQuoteDelaySeconds").GetFloat(1f);
		float blockSeconds = Vars.Key("Demo.DungeonThankQuoteDurationSeconds").GetFloat(5f);
		BannerPopup thankBanner = null;
		yield return new WaitForSeconds(delaySeconds);
		BannerManager.Get().ShowBanner("NewPopUp_LOOT.prefab:c1f1a158f539ad3428175ebcd948f138", null, thankQuote, delegate
		{
			s_shouldShowWelcomeBanner = true;
			TryShowWelcomeBanner();
		}, delegate(BannerPopup popup)
		{
			thankBanner = popup;
			GameObject obj = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(thankBanner.gameObject.layer), "BannerInputBlocker", thankBanner.transform);
			obj.transform.localPosition = new Vector3(0f, 100f, 0f);
			obj.layer = 17;
		});
		while (thankBanner == null)
		{
			yield return null;
		}
		yield return new WaitForSeconds(blockSeconds);
		thankBanner.Close();
	}

	private static bool IsInDemoMode()
	{
		return DemoMgr.Get().GetMode() == DemoMode.BLIZZCON_2017_ADVENTURE;
	}

	private void DisableBackButtonIfInDemoMode()
	{
		if (IsInDemoMode())
		{
			m_BackButton.SetEnabled(enabled: false);
			m_BackButton.Flip(faceUp: false);
		}
	}

	private void SetDungeonCrawlDisplayVisualStyle()
	{
		DungeonRunVisualStyle style = m_dungeonCrawlData.VisualStyle;
		foreach (DungeonCrawlDisplayStyleOverride displayStyle in m_DungeonCrawlDisplayStyle)
		{
			if (displayStyle.VisualStyle != style)
			{
				continue;
			}
			MeshRenderer meshRenderer = m_dungeonCrawlTray.GetComponent<MeshRenderer>();
			if (meshRenderer != null && displayStyle.DungeonCrawlTrayMaterial != null)
			{
				meshRenderer.SetMaterial(displayStyle.DungeonCrawlTrayMaterial);
			}
			if ((bool)UniversalInputManager.UsePhoneUI && m_ViewDeckTrayMesh != null)
			{
				MeshRenderer deckTrayMeshRenderer = m_ViewDeckTrayMesh.GetComponent<MeshRenderer>();
				if (deckTrayMeshRenderer != null && displayStyle.PhoneDeckTrayMaterial != null)
				{
					deckTrayMeshRenderer.SetMaterial(displayStyle.PhoneDeckTrayMaterial);
				}
			}
			break;
		}
	}

	private string GetClassNameFromDeckClass(TAG_CLASS deckClass)
	{
		List<GuestHero> guestHeroes = m_dungeonCrawlData.GetGuestHeroesForCurrentAdventure();
		if (guestHeroes.Count == 0)
		{
			return GameStrings.GetClassName(deckClass);
		}
		foreach (GuestHero guest in guestHeroes)
		{
			if (GameUtils.GetTagClassFromCardDbId(guest.cardDbId) == deckClass)
			{
				return GameDbf.GuestHero.GetRecord((GuestHeroDbfRecord r) => r.CardId == guest.cardDbId).Name;
			}
		}
		return string.Empty;
	}

	private TAG_CLASS GetClassFromShrine(long shrineCardId)
	{
		return GameUtils.GetTagClassFromCardDbId((int)shrineCardId);
	}

	private void ChangeHeroPortrait(string newHeroCardId, TAG_PREMIUM premium)
	{
		if (m_heroActor == null)
		{
			Log.Adventures.PrintError($"Unable to change hero portrait to cardId={newHeroCardId}. No actor has been loaded.");
			return;
		}
		DefLoader.Get().LoadFullDef(newHeroCardId, delegate(string cardId, DefLoader.DisposableFullDef fullDef, object userData)
		{
			using (fullDef)
			{
				m_heroActor.SetFullDef(fullDef);
				m_heroActor.SetPremium(premium);
				m_heroActor.GetComponent<PlayMakerFSM>().SendEvent(fullDef.EntityDef.GetClass().ToString());
			}
		});
	}

	public static Actor OnActorLoaded(string actorName, GameObject actorObject, GameObject container, bool withRotation = false)
	{
		Actor actor = actorObject.GetComponent<Actor>();
		if (actor != null)
		{
			if (container != null)
			{
				GameUtils.SetParent(actor, container, withRotation);
				LayerUtils.SetLayer(actor, container.layer);
			}
			actor.SetUnlit();
			actor.Hide();
		}
		else
		{
			Debug.LogWarning($"ERROR actor \"{actorName}\" has no Actor component");
		}
		return actor;
	}

	private void ShowBigCard(Actor heroPowerBigCard, DefLoader.DisposableFullDef heroPowerFullDef, GameObject bone)
	{
		Vector3? origin = null;
		if (m_heroPowerActor != null)
		{
			origin = m_heroPowerActor.gameObject.transform.position;
		}
		BigCardHelper.ShowBigCard(heroPowerBigCard, heroPowerFullDef, bone, m_BigCardScale, origin);
	}

	private void OnHeroClassIconsControllerReady(Widget widget)
	{
		if (widget == null)
		{
			Debug.LogWarning("AdventureDungeonCrawlDisplay.OnHeroIconsControllerReady - widget was null!");
			return;
		}
		if (m_heroActor == null)
		{
			Debug.LogWarning("AdventureDungeonCrawlDisplay.OnHeroIconsControllerReady - m_heroActor was null!");
			return;
		}
		HeroClassIconsDataModel dataModel = new HeroClassIconsDataModel();
		EntityDef entityDef = m_heroActor.GetEntityDef();
		if (entityDef == null)
		{
			Debug.LogWarning("AdventureDungeonCrawlDisplay.OnHeroIconsControllerReady - m_heroActor did not contain an entity def!");
			return;
		}
		dataModel.Classes.Clear();
		entityDef.GetClasses(dataModel.Classes);
		widget.BindDataModel(dataModel);
	}

	private static void ResetDungeonCrawlSelections(IDungeonCrawlData data)
	{
		data.SelectedLoadoutTreasureDbId = 0L;
		data.SelectedHeroPowerDbId = 0L;
		data.SelectedDeckId = 0L;
	}

	private void OnRetirePopupResponse(AlertPopup.Response response, object userData)
	{
		if (response == AlertPopup.Response.CANCEL)
		{
			m_retireButton.SetActive(value: true);
			return;
		}
		Navigation.GoBack();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			Navigation.GoBack();
		}
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_RUN_RETIRED, 1L), delegate(bool dataWrittenSuccessfully)
		{
			HandleRetireSuccessOrFail(dataWrittenSuccessfully);
		});
	}

	private void HandleRetireSuccessOrFail(bool success)
	{
		if (!success)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_RETIRE_CONFIRMATION_HEADER");
			info.m_text = GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_RETIRE_FAILURE_BODY");
			info.m_showAlertIcon = true;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			DialogManager.Get().ShowPopup(info);
			m_retireButton.SetActive(value: true);
		}
	}
}
