using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core;
using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class AdventureMissionDisplay : AdventureSubSceneDisplay
{
	protected class BossCreateParams
	{
		public ScenarioDbfRecord m_ScenarioRecord;

		public ScenarioDbId m_MissionId;

		public string m_CardDefId;
	}

	protected class WingCreateParams
	{
		public AdventureWingDef m_WingDef;

		[CustomEditField(ListTable = true)]
		public List<BossCreateParams> m_BossCreateParams = new List<BossCreateParams>();
	}

	protected class BossInfo
	{
		public string m_Title;

		public string m_Description;
	}

	public enum ProgressStep
	{
		INVALID,
		WING_COINS_AND_CHESTS_UPDATED
	}

	public delegate void ProgressStepCompletedCallback(ProgressStep progress);

	[CustomEditField(Sections = "Boss Layout Settings")]
	public GameObject m_BossWingContainer;

	[CustomEditField(Sections = "Boss Info")]
	public UberText m_BossTitle;

	[CustomEditField(Sections = "Boss Info")]
	public UberText m_BossDescription;

	[CustomEditField(Sections = "UI")]
	public UberText m_AdventureTitle;

	[CustomEditField(Sections = "UI")]
	public PegUIElement m_BackButton;

	[CustomEditField(Sections = "UI")]
	public PlayButton m_ChooseButton;

	[CustomEditField(Sections = "UI")]
	public GameObject m_BossPortraitContainer;

	[CustomEditField(Sections = "UI")]
	public GameObject m_BossPowerContainer;

	[CustomEditField(Sections = "UI")]
	public PegUIElement m_BossPowerHoverArea;

	[CustomEditField(Sections = "UI", T = EditType.GAME_OBJECT)]
	public MeshRenderer m_WatermarkIcon;

	[CustomEditField(Sections = "UI")]
	public AdventureRewardsDisplayArea m_RewardsDisplay;

	[CustomEditField(Sections = "UI")]
	public GameObject m_ClickBlocker;

	[CustomEditField(Sections = "UI/Animation")]
	public float m_CoinFlipDelayTime = 1.25f;

	[CustomEditField(Sections = "UI/Animation")]
	public float m_CoinFlipAnimationTime = 1f;

	[CustomEditField(Sections = "UI/Scroll Bar")]
	public UIBScrollable m_ScrollBar;

	[CustomEditField(Sections = "UI/Preview Pane")]
	public AdventureRewardsPreview m_PreviewPane;

	public AdventureMissionDisplayTray m_advMissionDisplayTray;

	[SerializeField]
	private Vector3 m_BossWingOffset = Vector3.zero;

	private static AdventureMissionDisplay s_instance;

	private AdventureWingProgressDisplay m_progressDisplay;

	private List<AdventureWing> m_BossWings = new List<AdventureWing>();

	private GameObject m_BossWingBorder;

	private AdventureBossCoin m_SelectedCoin;

	private Map<ScenarioDbId, DefLoader.DisposableFullDef> m_BossPortraitDefCache = new Map<ScenarioDbId, DefLoader.DisposableFullDef>();

	private Map<ScenarioDbId, DefLoader.DisposableFullDef> m_BossPowerDefCache = new Map<ScenarioDbId, DefLoader.DisposableFullDef>();

	private int m_DisableSelectionCount;

	private List<AdventureWing> m_WingsToGiveBigChest = new List<AdventureWing>();

	private bool m_ShowingRewardsPreview;

	private int m_TotalBosses;

	private int m_TotalBossesDefeated;

	private bool m_BossJustDefeated;

	private bool m_WaitingForClassChallengeUnlocks;

	private int m_ClassChallengeUnlockShowing;

	private Map<ScenarioDbId, BossInfo> m_BossInfoCache = new Map<ScenarioDbId, BossInfo>();

	private MusicPlaylistType m_mainMusic;

	private List<AdventureWing> m_wingsToFocus = new List<AdventureWing>();

	private Coroutine m_scheduledBringWingsToFocusCallback;

	private AssetHandle<Texture> m_watermarkTexture;

	private List<ProgressStepCompletedCallback> m_ProgressStepCompletedListeners = new List<ProgressStepCompletedCallback>();

	private bool m_waitingOnExternalLock;

	private const float s_ScreenBackTransitionDelay = 1.8f;

	private bool m_showingAdventureCompletePopup;

	private static int s_cheat_nextWingToGrantChest;

	[CustomEditField(Sections = "Boss Layout Settings")]
	public Vector3 BossWingOffset
	{
		get
		{
			return m_BossWingOffset;
		}
		set
		{
			m_BossWingOffset = value;
			UpdateWingPositions();
		}
	}

	public static AdventureMissionDisplay Get()
	{
		return s_instance;
	}

	private void Awake()
	{
		s_instance = this;
		m_mainMusic = MusicManager.Get().GetCurrentPlaylist();
		AdventureConfig ac = AdventureConfig.Get();
		AdventureDbId selectedAdv = ac.GetSelectedAdventure();
		AdventureModeDbId selectedMode = ac.GetSelectedMode();
		string dataName = GameUtils.GetAdventureDataRecord((int)selectedAdv, (int)selectedMode).Name;
		m_AdventureTitle.Text = dataName;
		List<WingCreateParams> wingCreateParamsList = BuildWingCreateParamsList();
		m_WingsToGiveBigChest.Clear();
		AdventureDef advDef = AdventureScene.Get().GetAdventureDef(selectedAdv);
		AdventureSubDef advSubDef = advDef.GetSubDef(selectedMode);
		if (!string.IsNullOrEmpty(advDef.m_WingBottomBorderPrefab))
		{
			m_BossWingBorder = AssetLoader.Get().InstantiatePrefab(advDef.m_WingBottomBorderPrefab);
			GameUtils.SetParent(m_BossWingBorder, m_BossWingContainer);
		}
		AddAssetToLoad(3);
		foreach (WingCreateParams wingCreateParams in wingCreateParamsList)
		{
			AddAssetToLoad(wingCreateParams.m_BossCreateParams.Count * 2);
		}
		m_TotalBosses = 0;
		m_TotalBossesDefeated = 0;
		if (!string.IsNullOrEmpty(advDef.m_ProgressDisplayPrefab))
		{
			m_progressDisplay = GameUtils.LoadGameObjectWithComponent<AdventureWingProgressDisplay>(advDef.m_ProgressDisplayPrefab);
			if (m_progressDisplay != null && m_BossWingContainer != null)
			{
				GameUtils.SetParent(m_progressDisplay, m_BossWingContainer);
			}
		}
		foreach (WingCreateParams wingCreateParams2 in wingCreateParamsList)
		{
			WingDbId wingId = wingCreateParams2.m_WingDef.GetWingId();
			AdventureWingDef wingDef = wingCreateParams2.m_WingDef;
			AdventureWing wing = GameUtils.LoadGameObjectWithComponent<AdventureWing>(wingDef.m_WingPrefab);
			if (wing == null)
			{
				continue;
			}
			if (m_BossWingContainer != null)
			{
				GameUtils.SetParent(wing, m_BossWingContainer);
			}
			wing.Initialize(wingDef);
			wing.SetBigChestRewards(wingId);
			wing.AddBossSelectedListener(delegate(AdventureBossCoin c, ScenarioDbId m)
			{
				OnBossSelected(c, m, playerSelected: true);
			});
			wing.AddOpenPlateStartListener(OnStartUnlockPlate);
			wing.AddOpenPlateEndListener(OnEndUnlockPlate);
			wing.AddTryPurchaseWingListener(delegate
			{
				ShowAdventureStore(wing);
			});
			wing.AddShowRewardsListener(delegate(List<RewardData> r, Vector3 o)
			{
				m_RewardsDisplay.ShowRewards(r, o, null);
			});
			wing.AddHideRewardsListener(delegate
			{
				m_RewardsDisplay.HideRewards();
			});
			List<int> wingScenarios = new List<int>();
			int bossesDefeated = 0;
			foreach (BossCreateParams bossCreateParams in wingCreateParams2.m_BossCreateParams)
			{
				bool enabled = AdventureConfig.IsMissionAvailable((int)bossCreateParams.m_MissionId) || wingCreateParams2.m_WingDef.CoinsStartFaceUp;
				AdventureBossCoin coin = wing.CreateBoss(wingDef.m_CoinPrefab, wingDef.m_RewardsPrefab, bossCreateParams.m_MissionId, enabled);
				AdventureConfig.Get().LoadBossDef(bossCreateParams.m_MissionId, delegate(AdventureBossDef bossDef, bool y)
				{
					if (bossDef != null)
					{
						coin.SetPortraitMaterial(bossDef);
					}
					AssetLoadCompleted();
				});
				if (AdventureConfig.Get().GetLastSelectedMission() == bossCreateParams.m_MissionId)
				{
					StartCoroutine(RememberLastBossSelection(coin, bossCreateParams.m_MissionId));
				}
				if (AdventureProgressMgr.Get().HasDefeatedScenario((int)bossCreateParams.m_MissionId))
				{
					bossesDefeated++;
					m_TotalBossesDefeated++;
				}
				m_TotalBosses++;
				DefLoader.Get().LoadFullDef(bossCreateParams.m_CardDefId, OnHeroFullDefLoaded, bossCreateParams.m_MissionId);
				wingScenarios.Add((int)bossCreateParams.m_MissionId);
			}
			bool thisWingBossJustDefeated = ac.GetWingBossesDefeated(selectedAdv, selectedMode, wingId, bossesDefeated) != bossesDefeated;
			if (thisWingBossJustDefeated)
			{
				m_BossJustDefeated = true;
			}
			if (wing.m_BigChest != null)
			{
				bool completeWing = bossesDefeated == wingCreateParams2.m_BossCreateParams.Count;
				if (!wing.HasBigChestRewards())
				{
					wing.HideBigChest();
				}
				else if (completeWing)
				{
					if (thisWingBossJustDefeated)
					{
						m_WingsToGiveBigChest.Add(wing);
					}
					else
					{
						wing.BigChestStayOpen();
					}
				}
			}
			if (m_progressDisplay != null)
			{
				bool isWingComplete = AdventureProgressMgr.Get().IsWingComplete(selectedAdv, AdventureModeDbId.LINEAR, wingId);
				m_progressDisplay.UpdateProgress(wingCreateParams2.m_WingDef.GetWingId(), isWingComplete);
			}
			ac.UpdateWingBossesDefeated(selectedAdv, selectedMode, wingId, bossesDefeated);
			wing.AddShowRewardsPreviewListeners(delegate
			{
				ShowRewardsPreview(wing, wingScenarios.ToArray(), wing.GetBigChestRewards(), wing.GetWingName());
			});
			wing.UpdateRewardsPreviewCover();
			wing.RandomizeBackground();
			wing.SetBringToFocusCallback(BatchBringWingToFocus);
			m_BossWings.Add(wing);
			if (AdventureScene.Get().IsDevMode)
			{
				wing.InitializeDevMode();
			}
		}
		AssetLoader.Get().InstantiatePrefab("Card_Play_Hero.prefab:42cbbd2c4969afb46b3887bb628de19d", OnHeroActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		AssetLoader.Get().InstantiatePrefab("Card_Play_HeroPower.prefab:a3794839abb947146903a26be13e09af", OnHeroPowerActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		AssetLoader.Get().InstantiatePrefab("History_HeroPower_Opponent.prefab:a99d23d6e8630f94b96a8e096fffb16f", OnHeroPowerBigCardLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		if (m_BossPowerHoverArea != null)
		{
			m_BossPowerHoverArea.AddEventListener(UIEventType.ROLLOVER, delegate
			{
				ShowBossPowerBigCard();
			});
			m_BossPowerHoverArea.AddEventListener(UIEventType.ROLLOUT, delegate
			{
				HideBossPowerBigCard();
			});
		}
		m_BackButton.AddEventListener(UIEventType.RELEASE, OnBackButtonPress);
		m_ChooseButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			ChangeToDeckPicker();
		});
		UpdateWingPositions();
		m_ChooseButton.Disable();
		StoreManager.Get().RegisterStoreShownListener(OnStoreShown);
		StoreManager.Get().RegisterStoreHiddenListener(OnStoreHidden);
		AdventureProgressMgr.Get().RegisterProgressUpdatedListener(OnAdventureProgressUpdate);
		if (m_WatermarkIcon != null && !string.IsNullOrEmpty(advSubDef.m_WatermarkTexture))
		{
			AssetLoader.Get().LoadAsset(ref m_watermarkTexture, advSubDef.m_WatermarkTexture);
			if (m_watermarkTexture != null)
			{
				m_WatermarkIcon.GetMaterial().mainTexture = m_watermarkTexture;
			}
			else
			{
				Debug.LogWarning($"Adventure Watermark texture is null: {advSubDef.m_WatermarkTexture}");
			}
		}
		else
		{
			Debug.LogWarning($"Adventure Watermark texture is null: m_WatermarkIcon: {m_WatermarkIcon},  advSubDef.m_WatermarkTexture: {advSubDef.m_WatermarkTexture}");
		}
		m_BackButton.gameObject.SetActive(value: true);
		m_PreviewPane.AddHideListener(OnHideRewardsPreview);
		AdventureDataDbfRecord record = AdventureConfig.Get().GetSelectedAdventureDataRecord();
		if (record.GameSaveDataServerKey != 0)
		{
			AddAssetToLoad();
			GameSaveDataManager.Get().Request((GameSaveKeyId)record.GameSaveDataServerKey, OnGameSaveDataReceived);
		}
	}

	private void Start()
	{
		Navigation.PushUnique(OnNavigateBack);
		AdventureWing firstUnlockedWing = null;
		foreach (AdventureWing wing in m_BossWings)
		{
			if (firstUnlockedWing == null || wing.GetWingDef().GetUnlockOrder() < firstUnlockedWing.GetWingDef().GetUnlockOrder())
			{
				firstUnlockedWing = wing;
			}
		}
		if (m_ScrollBar != null)
		{
			m_ScrollBar.UpdateScroll();
			if (firstUnlockedWing != null && firstUnlockedWing != m_BossWings[0])
			{
				firstUnlockedWing.BringToFocus();
			}
			m_ScrollBar.LoadScroll(AdventureConfig.Get().GetSelectedAdventureAndModeString(), snap: false);
		}
		AdventureConfig.Get().OnAdventureSceneUnloadEvent += OnAdventureSceneUnloaded;
	}

	protected override void OnDestroy()
	{
		AdventureProgressMgr.Get()?.RemoveProgressUpdatedListener(OnAdventureProgressUpdate);
		StoreManager.Get()?.RemoveStoreHiddenListener(OnStoreHidden);
		StoreManager.Get()?.RemoveStoreShownListener(OnStoreShown);
		if (AdventureConfig.Get() != null)
		{
			AdventureConfig.Get().OnAdventureSceneUnloadEvent -= OnAdventureSceneUnloaded;
		}
		SaveScrollbarValue();
		s_instance = null;
		m_BossPortraitDefCache.DisposeValuesAndClear();
		m_BossPowerDefCache.DisposeValuesAndClear();
		AssetHandle.SafeDispose(ref m_watermarkTexture);
		base.OnDestroy();
	}

	private void OnAdventureSceneUnloaded()
	{
		SaveScrollbarValue();
	}

	private void Update()
	{
		if (AdventureScene.Get().IsDevMode)
		{
			if (InputCollection.GetKeyDown(KeyCode.Z))
			{
				StartCoroutine(AnimateAdventureCompleteCheckmarksAndPopups(forceAnimation: true));
			}
			if (InputCollection.GetKeyDown(KeyCode.X))
			{
				ShowAdventureCompletePopup();
			}
			if (InputCollection.GetKeyDown(KeyCode.C))
			{
				Cheat_OpenNextWing();
			}
			if (InputCollection.GetKeyDown(KeyCode.V))
			{
				Cheat_OpenNextChest();
			}
		}
	}

	public bool IsDisabledSelection()
	{
		return m_DisableSelectionCount > 0;
	}

	public void AddProgressStepCompletedListener(ProgressStepCompletedCallback callback)
	{
		m_ProgressStepCompletedListeners.Add(callback);
	}

	private void FireProgressStepCompletedListeners(ProgressStep progress)
	{
		ProgressStepCompletedCallback[] array = m_ProgressStepCompletedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](progress);
		}
	}

	private void UpdateWingPositions()
	{
		float offset = 0f;
		if (m_progressDisplay != null)
		{
			m_progressDisplay.transform.localPosition = m_BossWingOffset;
			TransformUtil.SetLocalPosZ(m_progressDisplay, m_BossWingOffset.z - offset);
			offset += HeightForScrollableItem(m_progressDisplay.gameObject);
		}
		foreach (AdventureWing wing in m_BossWings)
		{
			wing.transform.localPosition = m_BossWingOffset;
			TransformUtil.SetLocalPosZ(wing, m_BossWingOffset.z - offset);
			offset += HeightForScrollableItem(wing.gameObject);
		}
		if (m_BossWingBorder != null)
		{
			m_BossWingBorder.transform.localPosition = m_BossWingOffset;
			TransformUtil.SetLocalPosZ(m_BossWingBorder, m_BossWingOffset.z - offset);
		}
	}

	private float HeightForScrollableItem(GameObject go)
	{
		UIBScrollableItem scrollableItem = go.GetComponent<UIBScrollableItem>();
		if (scrollableItem == null)
		{
			Log.All.PrintError("No UIBScrollableItem component on the GameObject {0}!", go);
			return 0f;
		}
		return scrollableItem.m_size.z;
	}

	private void OnHeroFullDefLoaded(string cardId, DefLoader.DisposableFullDef def, object userData)
	{
		if (def == null)
		{
			Debug.LogError($"Unable to load {cardId} hero def for Adventure boss.", base.gameObject);
			AssetLoadCompleted();
			return;
		}
		ScenarioDbId mission = (ScenarioDbId)userData;
		m_BossPortraitDefCache.SetOrReplaceDisposable(mission, def);
		string heroPowerID = GameUtils.GetMissionHeroPowerCardId((int)mission);
		if (!string.IsNullOrEmpty(heroPowerID))
		{
			AddAssetToLoad();
			DefLoader.Get().LoadFullDef(heroPowerID, OnHeroPowerFullDefLoaded, mission);
		}
		AssetLoadCompleted();
	}

	private void OnHeroPowerFullDefLoaded(string cardId, DefLoader.DisposableFullDef def, object userData)
	{
		if (def == null)
		{
			Debug.LogError($"Unable to load {cardId} hero power def for Adventure boss.", base.gameObject);
			AssetLoadCompleted();
		}
		else
		{
			ScenarioDbId mission = (ScenarioDbId)userData;
			m_BossPowerDefCache.SetOrReplaceDisposable(mission, def);
			AssetLoadCompleted();
		}
	}

	private void OnHeroActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_BossActor = AdventureSubSceneDisplay.OnActorLoaded(assetRef, go, m_BossPortraitContainer);
		if (m_BossActor != null && m_BossActor.GetHealthObject() != null)
		{
			m_BossActor.GetHealthObject().Hide();
		}
		if (m_BossActor != null && m_BossActor.GetLoanerFlag() != null)
		{
			m_BossActor.GetLoanerFlag().Hide();
		}
		AssetLoadCompleted();
	}

	private void OnHeroPowerActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_HeroPowerActor = AdventureSubSceneDisplay.OnActorLoaded(assetRef, go, m_BossPowerContainer);
		AssetLoadCompleted();
	}

	private void OnHeroPowerBigCardLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_BossPowerBigCard = AdventureSubSceneDisplay.OnActorLoaded(assetRef, go, (m_HeroPowerActor == null) ? null : m_HeroPowerActor.gameObject);
		if (m_BossPowerBigCard != null)
		{
			m_BossPowerBigCard.TurnOffCollider();
		}
		AssetLoadCompleted();
	}

	private void OnGameSaveDataReceived(bool success)
	{
		AssetLoadCompleted();
		UpdateCoinsWithGameSaveData();
	}

	private void UpdateCoinsWithGameSaveData()
	{
		foreach (AdventureWing bossWing in m_BossWings)
		{
			bossWing.UpdateAllBossCoinChests();
		}
	}

	protected override void OnSubSceneLoaded()
	{
		if (m_BossPowerBigCard != null && m_HeroPowerActor != null && m_BossPowerBigCard.transform.parent != m_HeroPowerActor.transform)
		{
			GameUtils.SetParent(m_BossPowerBigCard, m_HeroPowerActor.gameObject);
		}
		base.OnSubSceneLoaded();
	}

	protected override void OnSubSceneTransitionComplete()
	{
		base.OnSubSceneTransitionComplete();
		TryShowWelcomeBanner(delegate
		{
			StartCoroutine(UpdateAndAnimateProgress(m_BossWings, scrollToCoin: true));
		});
	}

	private static bool OnNavigateBack()
	{
		AdventureConfig.Get().SubSceneGoBack();
		return true;
	}

	private void OnBackButtonPress(UIEvent e)
	{
		foreach (AdventureWing bossWing in m_BossWings)
		{
			bossWing.NavigateBackCleanup();
		}
		Navigation.GoBack();
	}

	private void OnZeroCostTransactionStoreExit(bool authorizationBackButtonPressed, object userData)
	{
		if (authorizationBackButtonPressed)
		{
			OnBackButtonPress(null);
		}
	}

	private void OnBossSelected(AdventureBossCoin coin, ScenarioDbId mission, bool playerSelected)
	{
		if (IsDisabledSelection())
		{
			return;
		}
		if (m_SelectedCoin != null)
		{
			m_SelectedCoin.Select(selected: false);
		}
		m_SelectedCoin = coin;
		m_SelectedCoin.Select(selected: true);
		if (m_ChooseButton != null)
		{
			if (!m_ChooseButton.IsEnabled())
			{
				m_ChooseButton.Enable();
			}
			string chooseButtonText = GameStrings.Get(AdventureConfig.DoesMissionRequireDeck(mission) ? "GLUE_CHOOSE" : "GLOBAL_PLAY");
			m_ChooseButton.SetText(chooseButtonText);
		}
		ShowBossFrame(mission);
		AdventureConfig.Get().SetMission(mission, playerSelected);
		AdventureBossDef bossDef = AdventureConfig.Get().GetBossDef(mission);
		if (bossDef.m_MissionMusic != 0 && !MusicManager.Get().StartPlaylist(bossDef.m_MissionMusic))
		{
			ResumeMainMusic();
		}
		if (playerSelected && bossDef.m_IntroLinePlayTime == AdventureBossDef.IntroLinePlayTime.MissionSelect)
		{
			AdventureUtils.PlayMissionQuote(bossDef, DetermineCharacterQuotePos(coin.gameObject));
		}
	}

	private Vector3 DetermineCharacterQuotePos(GameObject coin)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return NotificationManager.PHONE_CHARACTER_POS;
		}
		Bounds coinBounds = TransformUtil.GetBoundsOfChildren(coin);
		Vector3 coinBottomWorld = coinBounds.center;
		coinBottomWorld.y -= coinBounds.extents.y;
		Camera cam = Box.Get().GetCamera();
		Vector3 vector = cam.WorldToScreenPoint(coinBottomWorld);
		float screenY = 0.4f * (float)cam.pixelHeight;
		if (vector.y < screenY)
		{
			return NotificationManager.ALT_ADVENTURE_SCREEN_POS;
		}
		return NotificationManager.DEFAULT_CHARACTER_POS;
	}

	private void ShowBossFrame(ScenarioDbId mission)
	{
		if (m_BossInfoCache.TryGetValue(mission, out var bossinfo))
		{
			m_BossTitle.Text = bossinfo.m_Title;
			if (m_BossDescription != null)
			{
				m_BossDescription.Text = bossinfo.m_Description;
			}
		}
		if (m_BossPortraitDefCache.TryGetValue(mission, out var def))
		{
			m_BossActor.SetPremium(TAG_PREMIUM.NORMAL);
			m_BossActor.SetEntityDef(def.EntityDef);
			m_BossActor.SetCardDef(def.DisposableCardDef);
			m_BossActor.UpdateAllComponents();
			m_BossActor.SetUnlit();
			m_BossActor.Show();
		}
		if (m_BossPowerDefCache.TryGetValue(mission, out def))
		{
			m_HeroPowerActor.SetPremium(TAG_PREMIUM.NORMAL);
			m_HeroPowerActor.SetEntityDef(def.EntityDef);
			m_HeroPowerActor.SetCardDef(def.DisposableCardDef);
			m_HeroPowerActor.UpdateAllComponents();
			m_HeroPowerActor.SetUnlit();
			m_HeroPowerActor.Show();
			m_CurrentBossHeroPowerFullDef?.Dispose();
			m_CurrentBossHeroPowerFullDef = def?.Share();
			ScenarioDbfRecord scenarioRecord = GameDbf.Scenario.GetRecord((int)mission);
			if (m_BossPowerContainer != null && !m_BossPowerContainer.activeSelf && !scenarioRecord.HideBossHeroPowerInUi)
			{
				m_BossPowerContainer.SetActive(value: true);
			}
		}
		else if (m_BossPowerContainer != null)
		{
			m_BossPowerContainer.SetActive(value: false);
		}
	}

	private void UnselectBoss()
	{
		if (m_BossTitle != null)
		{
			m_BossTitle.Text = string.Empty;
		}
		if (m_BossDescription != null)
		{
			m_BossDescription.Text = string.Empty;
		}
		m_BossActor.Hide();
		if (m_BossPowerContainer != null)
		{
			m_BossPowerContainer.SetActive(value: false);
		}
		if (m_SelectedCoin != null)
		{
			m_SelectedCoin.Select(selected: false);
		}
		m_SelectedCoin = null;
		AdventureConfig.Get().SetMission(ScenarioDbId.INVALID);
		if (m_ChooseButton.IsEnabled())
		{
			m_ChooseButton.Disable();
		}
	}

	private void ChangeToDeckPicker()
	{
		ScenarioDbId selectedMission = AdventureConfig.Get().GetMission();
		AdventureBossDef bossDef = AdventureConfig.Get().GetBossDef(selectedMission);
		if (bossDef != null && bossDef.m_IntroLinePlayTime == AdventureBossDef.IntroLinePlayTime.MissionStart)
		{
			AdventureUtils.PlayMissionQuote(bossDef, DetermineCharacterQuotePos(m_ChooseButton.gameObject));
		}
		if (AdventureConfig.Get().DoesSelectedMissionRequireDeck())
		{
			m_ChooseButton.Disable();
			DisableSelection(yes: true);
			AdventureConfig.Get().ChangeSubScene(AdventureData.Adventuresubscene.MISSION_DECK_PICKER);
			return;
		}
		if (m_advMissionDisplayTray != null)
		{
			m_advMissionDisplayTray.EnableRewardsChest(enabled: false);
		}
		GameMgr.Get().FindGame(GameType.GT_VS_AI, FormatType.FT_WILD, (int)AdventureConfig.Get().GetMission(), 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
	}

	public void GetExternalUILock()
	{
		m_waitingOnExternalLock = true;
		DisableSelection(yes: true);
	}

	public void ReleaseExternalUILock()
	{
		m_waitingOnExternalLock = false;
		DisableSelection(yes: false);
	}

	private void DisableSelection(bool yes)
	{
		if (!(m_ClickBlocker == null))
		{
			m_DisableSelectionCount += (yes ? 1 : (-1));
			bool disable = IsDisabledSelection();
			if (m_ClickBlocker.gameObject.activeSelf != disable)
			{
				m_ClickBlocker.gameObject.SetActive(disable);
				m_ScrollBar.Enable(!disable);
			}
		}
	}

	private void TryShowWelcomeBanner(BannerManager.DelOnCloseBanner OnCloseBanner)
	{
		bool shouldShowBanner = true;
		foreach (AdventureWing wing in m_BossWings)
		{
			AdventureProgressMgr.Get().GetWingAck((int)wing.GetWingId(), out var wingAck);
			if (wingAck > 0)
			{
				shouldShowBanner = false;
				break;
			}
		}
		if (shouldShowBanner)
		{
			AdventureDef advDef = AdventureScene.Get().GetAdventureDef(AdventureConfig.Get().GetSelectedAdventure());
			if (advDef != null && !string.IsNullOrEmpty(advDef.m_AdventureEntryQuotePrefab) && !string.IsNullOrEmpty(advDef.m_AdventureEntryQuoteVOLine))
			{
				string quoteString = new AssetReference(advDef.m_AdventureEntryQuoteVOLine).GetLegacyAssetName();
				NotificationManager.Get().CreateCharacterQuote(advDef.m_AdventureEntryQuotePrefab, GameStrings.Get(quoteString), advDef.m_AdventureEntryQuoteVOLine);
			}
			if (advDef != null && !string.IsNullOrEmpty(advDef.m_AdventureIntroBannerPrefab))
			{
				BannerManager.Get().ShowBanner(advDef.m_AdventureIntroBannerPrefab, null, null, OnCloseBanner);
				return;
			}
		}
		OnCloseBanner();
	}

	private IEnumerator UpdateAndAnimateProgress(List<AdventureWing> wings, bool scrollToCoin, bool forceCoinAnimation = false)
	{
		yield return StartCoroutine(UpdateAndAnimateWingCoinsAndChests(wings, scrollToCoin, forceCoinAnimation));
		FireProgressStepCompletedListeners(ProgressStep.WING_COINS_AND_CHESTS_UPDATED);
		while (m_waitingOnExternalLock)
		{
			yield return null;
		}
		yield return StartCoroutine(AnimateWingCompleteBigChests());
		yield return StartCoroutine(AnimateProgressDisplay());
		yield return StartCoroutine(AnimateAdventureCompleteCheckmarksAndPopups());
		CheckForWingUnlocks();
	}

	public bool HasWingJustAckedRequiredProgress(int wingId, int requiredProgress)
	{
		foreach (AdventureWing wing in m_BossWings)
		{
			if (wing.GetWingId() == (WingDbId)wingId)
			{
				return wing.HasJustAckedRequiredProgress(requiredProgress);
			}
		}
		return false;
	}

	public void SetWingHasJustAckedProgress(int wingId, bool hasJustAckedProgress)
	{
		foreach (AdventureWing wing in m_BossWings)
		{
			if (wing.GetWingId() == (WingDbId)wingId)
			{
				wing.SetHasJustAckedProgress(hasJustAckedProgress);
				break;
			}
		}
	}

	private IEnumerator UpdateAndAnimateWingCoinsAndChests(List<AdventureWing> wings, bool scrollToCoin, bool forceCoinAnimation)
	{
		DisableSelection(yes: true);
		if (AdventureScene.Get().IsInitialScreen())
		{
			yield return new WaitForSeconds(1.8f);
		}
		int wingsUpdated = 0;
		foreach (AdventureWing wing in wings)
		{
			AdventureWing.DelOnCoinAnimateCallback func = null;
			if (scrollToCoin)
			{
				AdventureWing thisWing = wing;
				func = delegate
				{
					thisWing.BringToFocus();
				};
			}
			if (wing.UpdateAndAnimateCoinsAndChests(m_CoinFlipDelayTime * (float)wingsUpdated, forceCoinAnimation, func))
			{
				wingsUpdated++;
			}
		}
		if (wingsUpdated > 0)
		{
			yield return new WaitForSeconds(m_CoinFlipDelayTime * (float)wingsUpdated + m_CoinFlipAnimationTime);
		}
		DisableSelection(yes: false);
	}

	private IEnumerator AnimateWingCompleteBigChests()
	{
		if (m_WingsToGiveBigChest.Count == 0)
		{
			yield break;
		}
		DisableSelection(yes: true);
		if (AdventureScene.Get().IsInitialScreen())
		{
			yield return new WaitForSeconds(1.8f);
		}
		int animDone = 0;
		foreach (AdventureWing item in m_WingsToGiveBigChest)
		{
			int num = animDone + 1;
			animDone = num;
			item.m_WingEventTable.AddOpenChestEndEventListener(delegate
			{
				int num2 = animDone - 1;
				animDone = num2;
			}, once: true);
			item.OpenBigChest();
		}
		while (animDone > 0)
		{
			yield return null;
		}
		StartCoroutine(PlayWingNotifications());
		List<int> wingIds = new List<int>();
		foreach (AdventureWing wing in m_WingsToGiveBigChest)
		{
			List<AdventureMissionDbfRecord> unlockedMissions = ClassChallengeUnlock.AdventureMissionsUnlockedByWingId((int)wing.GetWingId());
			if (unlockedMissions != null && unlockedMissions.Count > 0)
			{
				wingIds.Add((int)wing.GetWingId());
			}
		}
		if (UserAttentionManager.CanShowAttentionGrabber("AdventureMissionDisplay.ShowFixedRewards"))
		{
			m_WaitingForClassChallengeUnlocks = true;
			PopupDisplayManager.Get().ShowAnyOutstandingPopups(delegate
			{
				ShowClassChallengeUnlock(wingIds);
			});
		}
		while (m_WaitingForClassChallengeUnlocks)
		{
			yield return null;
		}
		foreach (AdventureWing wing2 in m_WingsToGiveBigChest)
		{
			AdventureWing nextWing = GetNextUnlockedWing(wing2.GetWingDef());
			wing2.GetCompleteQuoteAssetsFromTargetWingEventTiming((int)((!(nextWing == null)) ? nextWing.GetWingDef().GetWingId() : WingDbId.INVALID), out var completeQuotePrefab, out var completeQuoteVOLine);
			string gameString = new AssetReference(completeQuoteVOLine).GetLegacyAssetName();
			if (!string.IsNullOrEmpty(completeQuotePrefab) && !string.IsNullOrEmpty(completeQuoteVOLine))
			{
				NotificationManager.Get().CreateCharacterQuote(completeQuotePrefab, GameStrings.Get(gameString), completeQuoteVOLine);
			}
			wing2.BigChestStayOpen();
		}
		m_WingsToGiveBigChest.Clear();
		DisableSelection(yes: false);
	}

	private IEnumerator AnimateProgressDisplay()
	{
		if (!(m_progressDisplay != null))
		{
			yield break;
		}
		while (m_progressDisplay.HasProgressAnimationToPlay())
		{
			m_ScrollBar.SetScroll(0f);
			DisableSelection(yes: true);
			bool isAnimComplete = false;
			m_progressDisplay.PlayProgressAnimation(delegate
			{
				DisableSelection(yes: false);
				isAnimComplete = true;
			});
			while (!isAnimComplete)
			{
				yield return null;
			}
		}
	}

	private void CheckForWingUnlocks()
	{
		foreach (AdventureWing bossWing in m_BossWings)
		{
			bossWing.UpdatePlateState();
		}
	}

	private IEnumerator AnimateAdventureCompleteCheckmarksAndPopups(bool forceAnimation = false)
	{
		if ((m_TotalBosses != m_TotalBossesDefeated || !m_BossJustDefeated) && !forceAnimation)
		{
			yield break;
		}
		List<KeyValuePair<AdventureRewardsChest, float>> chestAnimates = new List<KeyValuePair<AdventureRewardsChest, float>>();
		float animateTime = 0.7f;
		float totalAnimTime = 0f;
		List<AdventureWing> list = new List<AdventureWing>(m_BossWings);
		list.Sort(WingUnlockOrderSortComparison);
		foreach (AdventureWing item in list)
		{
			foreach (AdventureRewardsChest chest in item.GetChests())
			{
				animateTime *= 0.9f;
				if (animateTime < 0.1f)
				{
					animateTime = 0.1f;
				}
				totalAnimTime += animateTime;
				chestAnimates.Add(new KeyValuePair<AdventureRewardsChest, float>(chest, animateTime));
			}
		}
		DisableSelection(yes: true);
		float startScroll = 0f;
		float endScroll = 1f;
		if (m_progressDisplay != null)
		{
			totalAnimTime -= animateTime;
			startScroll = 1f / (float)m_BossWings.Count;
		}
		else if (m_BossWings[0].GetWingDef().GetUnlockOrder() > m_BossWings[m_BossWings.Count - 1].GetWingDef().GetUnlockOrder())
		{
			startScroll = 1f;
			endScroll = 0f;
		}
		m_ScrollBar.SetScroll(startScroll, iTween.EaseType.easeOutSine, 0.25f, blockInputWhileScrolling: true);
		yield return new WaitForSeconds(0.3f);
		m_ScrollBar.SetScroll(endScroll, iTween.EaseType.easeInQuart, totalAnimTime - 0.1f, blockInputWhileScrolling: true);
		foreach (KeyValuePair<AdventureRewardsChest, float> kvpair in chestAnimates)
		{
			kvpair.Key.BurstCheckmark();
			yield return new WaitForSeconds(kvpair.Value);
		}
		DisableSelection(yes: false);
		ShowAdventureCompletePopup();
		while (m_showingAdventureCompletePopup)
		{
			yield return null;
		}
	}

	public void ShowClassChallengeUnlock(List<int> classChallengeUnlocks)
	{
		if (classChallengeUnlocks == null || classChallengeUnlocks.Count == 0)
		{
			m_WaitingForClassChallengeUnlocks = false;
			return;
		}
		foreach (int classChallengeUnlock in classChallengeUnlocks)
		{
			m_ClassChallengeUnlockShowing++;
			new ClassChallengeUnlockData(classChallengeUnlock).LoadRewardObject(delegate(Reward reward, object data)
			{
				reward.RegisterHideListener(delegate
				{
					m_ClassChallengeUnlockShowing--;
					if (m_ClassChallengeUnlockShowing == 0)
					{
						m_WaitingForClassChallengeUnlocks = false;
					}
				});
				OnRewardObjectLoaded(reward, data);
			});
		}
	}

	private void ShowAdventureCompletePopup()
	{
		AdventureDbId adventure = AdventureConfig.Get().GetSelectedAdventure();
		AdventureModeDbId mode = AdventureConfig.Get().GetSelectedMode();
		DisableSelection(yes: true);
		AdventureDef advDef = AdventureScene.Get().GetAdventureDef(adventure);
		AdventureSubDef advSubDef = advDef.GetSubDef(mode);
		switch (advDef.m_BannerRewardType)
		{
		case AdventureDef.BannerRewardType.AdventureCompleteReward:
			m_showingAdventureCompletePopup = true;
			new AdventureCompleteRewardData(mode, advDef.m_BannerRewardPrefab, advSubDef.GetCompleteBannerText()).LoadRewardObject(delegate(Reward reward, object data)
			{
				reward.RegisterHideListener(AdventureCompletePopupDismissed);
				OnRewardObjectLoaded(reward, data);
			});
			break;
		case AdventureDef.BannerRewardType.BannerManagerPopup:
			m_showingAdventureCompletePopup = true;
			BannerManager.Get().ShowBanner(advDef.m_BannerRewardPrefab, null, advSubDef.GetCompleteBannerText(), delegate
			{
				AdventureCompletePopupDismissed(null);
			});
			break;
		}
		if (!string.IsNullOrEmpty(advDef.m_AdventureCompleteQuotePrefab) && !string.IsNullOrEmpty(advDef.m_AdventureCompleteQuoteVOLine))
		{
			string gameString = new AssetReference(advDef.m_AdventureCompleteQuoteVOLine).GetLegacyAssetName();
			NotificationManager.Get().CreateCharacterQuote(advDef.m_AdventureCompleteQuotePrefab, GameStrings.Get(gameString), advDef.m_AdventureCompleteQuoteVOLine);
		}
	}

	private void AdventureCompletePopupDismissed(object userData)
	{
		m_showingAdventureCompletePopup = false;
		DisableSelection(yes: false);
	}

	private void PositionReward(Reward reward)
	{
		GameUtils.SetParent(reward, base.transform);
	}

	private void OnRewardObjectLoaded(Reward reward, object callbackData)
	{
		PositionReward(reward);
		reward.Show(updateCacheValues: false);
	}

	private List<WingCreateParams> BuildWingCreateParamsList()
	{
		AdventureConfig adventureConfig = AdventureConfig.Get();
		AdventureDbId adventureId = adventureConfig.GetSelectedAdventure();
		AdventureModeDbId modeId = adventureConfig.GetSelectedMode();
		int adventureDbId = (int)adventureId;
		int modeDbId = (int)modeId;
		List<WingCreateParams> wingCreateParamsList = new List<WingCreateParams>();
		int bossesAdded = 0;
		foreach (ScenarioDbfRecord scenarioRecord in GameDbf.Scenario.GetRecords((ScenarioDbfRecord r) => r.AdventureId == adventureDbId && r.ModeId == modeDbId))
		{
			ScenarioDbId missionId = (ScenarioDbId)scenarioRecord.ID;
			WingDbId wingId = (WingDbId)scenarioRecord.WingId;
			int cardDbId = scenarioRecord.ClientPlayer2HeroCardId;
			if (cardDbId == 0)
			{
				cardDbId = scenarioRecord.Player2HeroCardId;
			}
			AdventureWingDef wingDef = AdventureScene.Get().GetWingDef(wingId);
			if (wingDef == null)
			{
				Debug.LogError($"Unable to find wing record for scenario {missionId} with ID: {wingId}");
				continue;
			}
			CardDbfRecord cardRecord = GameDbf.Card.GetRecord(cardDbId);
			WingCreateParams wingCreateParams = wingCreateParamsList.Find((WingCreateParams currParams) => wingId == currParams.m_WingDef.GetWingId());
			if (wingCreateParams == null)
			{
				wingCreateParams = new WingCreateParams();
				wingCreateParams.m_WingDef = wingDef;
				if (wingCreateParams.m_WingDef == null)
				{
					Error.AddDevFatal("AdventureDisplay.BuildWingCreateParamsMap() - failed to find a WingDef for adventure {0} wing {1}", adventureId, wingId);
					continue;
				}
				wingCreateParamsList.Add(wingCreateParams);
			}
			BossCreateParams boss = new BossCreateParams();
			boss.m_ScenarioRecord = scenarioRecord;
			boss.m_MissionId = missionId;
			boss.m_CardDefId = cardRecord.NoteMiniGuid;
			if (!m_BossInfoCache.ContainsKey(missionId))
			{
				BossInfo newbossinfo = new BossInfo
				{
					m_Title = scenarioRecord.ShortName,
					m_Description = (((bool)UniversalInputManager.UsePhoneUI && !string.IsNullOrEmpty(scenarioRecord.ShortDescription)) ? scenarioRecord.ShortDescription : scenarioRecord.Description)
				};
				m_BossInfoCache[missionId] = newbossinfo;
			}
			wingCreateParams.m_BossCreateParams.Add(boss);
			bossesAdded++;
		}
		if (bossesAdded == 0)
		{
			Debug.LogError($"Unable to find any bosses associated with wing {adventureId} and mode {modeId}.\nCheck if the scenario DBF has valid entries!");
		}
		wingCreateParamsList.Sort(WingCreateParamsSortComparison);
		foreach (WingCreateParams item in wingCreateParamsList)
		{
			item.m_BossCreateParams.Sort(BossCreateParamsSortComparison);
		}
		return wingCreateParamsList;
	}

	private int WingCreateParamsSortComparison(WingCreateParams params1, WingCreateParams params2)
	{
		return params1.m_WingDef.GetSortOrder() - params2.m_WingDef.GetSortOrder();
	}

	private int BossCreateParamsSortComparison(BossCreateParams params1, BossCreateParams params2)
	{
		return GameUtils.MissionSortComparison(params1.m_ScenarioRecord, params2.m_ScenarioRecord);
	}

	private int WingUnlockOrderSortComparison(AdventureWing wing1, AdventureWing wing2)
	{
		return wing1.GetWingDef().GetUnlockOrder() - wing2.GetWingDef().GetUnlockOrder();
	}

	private void ShowAdventureStore(AdventureWing selectedWing)
	{
		if (!SetRotationManager.Get().CheckForSetRotationRollover() && (PlayerMigrationManager.Get() == null || !PlayerMigrationManager.Get().CheckForPlayerMigrationRequired()))
		{
			StoreManager.Get().StartAdventureTransaction(selectedWing.GetProductType(), selectedWing.GetProductData(), null, null, ShopType.ADVENTURE_STORE, 1);
		}
	}

	private void OnStoreShown()
	{
		DisableSelection(yes: true);
	}

	private void OnStoreHidden()
	{
		DisableSelection(yes: false);
	}

	private void OnAdventureProgressUpdate(bool isStartupAction, AdventureMission.WingProgress oldProgress, AdventureMission.WingProgress newProgress, object userData)
	{
		bool num = oldProgress?.IsOwned() ?? false;
		bool newOwned = newProgress?.IsOwned() ?? false;
		if (num != newOwned)
		{
			StartCoroutine(UpdateWingPlateStates());
		}
	}

	private IEnumerator UpdateWingPlateStates()
	{
		while (StoreManager.Get().IsShown())
		{
			yield return null;
		}
		foreach (AdventureWing bossWing in m_BossWings)
		{
			bossWing.UpdatePlateState();
		}
	}

	private void ShowRewardsPreview(AdventureWing wing, int[] scenarioids, List<RewardData> wingRewards, string wingName)
	{
		if (m_ShowingRewardsPreview)
		{
			return;
		}
		if (m_ClickBlocker != null)
		{
			m_ClickBlocker.SetActive(value: true);
		}
		m_ShowingRewardsPreview = true;
		m_PreviewPane.Reset();
		m_PreviewPane.SetHeaderText(wingName);
		List<string> specificCardRewards = wing.GetWingDef().m_SpecificRewardsPreviewCards;
		List<int> specificCardBackRewards = wing.GetWingDef().m_SpecificRewardsPreviewCardBacks;
		List<BoosterDbId> specificBoosterRewards = wing.GetWingDef().m_SpecificRewardsPreviewBoosters;
		int hiddenCards = wing.GetWingDef().m_HiddenRewardsPreviewCount;
		bool num = specificCardRewards != null && specificCardRewards.Count > 0;
		bool hasSpecificCardBackRewards = specificCardBackRewards != null && specificCardBackRewards.Count > 0;
		bool hasSpecificBoosterRewards = specificBoosterRewards != null && specificBoosterRewards.Count > 0;
		if (num)
		{
			m_PreviewPane.AddSpecificCards(specificCardRewards);
		}
		if (hasSpecificCardBackRewards)
		{
			m_PreviewPane.AddSpecificCardBacks(specificCardBackRewards);
		}
		if (hasSpecificBoosterRewards)
		{
			m_PreviewPane.AddSpecificBoosters(specificBoosterRewards);
		}
		if (!num && !hasSpecificCardBackRewards && !hasSpecificBoosterRewards)
		{
			foreach (int id in scenarioids)
			{
				m_PreviewPane.AddRewardBatch(id);
			}
			if (wingRewards != null && wingRewards.Count > 0)
			{
				m_PreviewPane.AddRewardBatch(wingRewards);
			}
		}
		m_PreviewPane.SetHiddenCardCount(hiddenCards);
		m_PreviewPane.Init();
		m_PreviewPane.Show(show: true);
	}

	private void OnHideRewardsPreview()
	{
		if (m_ClickBlocker != null)
		{
			m_ClickBlocker.SetActive(value: false);
		}
		m_ShowingRewardsPreview = false;
	}

	private void OnStartUnlockPlate(AdventureWing wing)
	{
		if (!wing.ContainsBossCoin(m_SelectedCoin))
		{
			UnselectBoss();
		}
		DisableSelection(yes: true);
	}

	private void OnEndUnlockPlate(AdventureWing wing)
	{
		DisableSelection(yes: false);
		if (!string.IsNullOrEmpty(wing.GetWingDef().m_WingOpenPopup))
		{
			AdventureWingOpenBanner wingOpenBanner = GameUtils.LoadGameObjectWithComponent<AdventureWingOpenBanner>(wing.GetWingDef().m_WingOpenPopup);
			if (wingOpenBanner != null)
			{
				wingOpenBanner.ShowBanner(delegate
				{
					StartCoroutine(UpdateAndAnimateProgress(new List<AdventureWing> { wing }, scrollToCoin: false));
				});
			}
		}
		else
		{
			StartCoroutine(UpdateAndAnimateProgress(new List<AdventureWing> { wing }, scrollToCoin: false));
		}
	}

	private void BatchBringWingToFocus(AdventureWing wing)
	{
		if (!m_wingsToFocus.Contains(wing))
		{
			m_wingsToFocus.Add(wing);
		}
		if (m_scheduledBringWingsToFocusCallback == null)
		{
			m_scheduledBringWingsToFocusCallback = StartCoroutine(WaitThenBringWingsToFocus());
		}
	}

	private IEnumerator WaitThenBringWingsToFocus()
	{
		yield return new WaitForEndOfFrame();
		if (m_wingsToFocus.Count != 0)
		{
			m_wingsToFocus.Sort(WingUnlockOrderSortComparison);
			BringWingToFocus(m_wingsToFocus[0]);
			m_scheduledBringWingsToFocusCallback = null;
			m_wingsToFocus.Clear();
		}
	}

	private void BringWingToFocus(AdventureWing wing)
	{
		if (!(m_ScrollBar == null))
		{
			float positionOffset = 0f;
			UIBScrollableItem scrollableItem = wing.GetComponent<UIBScrollableItem>();
			if (scrollableItem != null)
			{
				positionOffset = scrollableItem.m_offset.z * wing.gameObject.transform.lossyScale.z;
			}
			m_ScrollBar.CenterObjectInView(wing.gameObject, positionOffset, null, m_ScrollBar.m_ScrollEaseType, m_ScrollBar.m_ScrollTweenTime, blockInputWhileScrolling: true);
		}
	}

	private IEnumerator RememberLastBossSelection(AdventureBossCoin coin, ScenarioDbId mission)
	{
		while (base.AssetLoadingHelper.AssetsLoading > 0)
		{
			yield return null;
		}
		OnBossSelected(coin, mission, playerSelected: false);
	}

	private IEnumerator PlayWingNotifications()
	{
		yield return new WaitForSeconds(3f);
		foreach (AdventureWing wing in m_WingsToGiveBigChest)
		{
			if (wing.GetAdventureId() == AdventureDbId.NAXXRAMAS && wing.GetWingId() == WingDbId.NAXX_ARACHNID)
			{
				NotificationManager.Get().CreateKTQuote("VO_KT_MAEXXNA5_50", "VO_KT_MAEXXNA5_50.prefab:71879e77d87e0e745be507be968067bf");
			}
		}
	}

	private void ResumeMainMusic()
	{
		if (m_mainMusic != 0)
		{
			MusicManager.Get().StartPlaylist(m_mainMusic);
		}
	}

	private AdventureWing GetNextUnlockedWing(AdventureWingDef currentWingDef)
	{
		AdventureWing nextWingToOpen = null;
		foreach (AdventureWing wing in m_BossWings)
		{
			if (wing.GetWingDef().GetUnlockOrder() > currentWingDef.GetUnlockOrder() && (nextWingToOpen == null || wing.GetWingDef().GetUnlockOrder() < nextWingToOpen.GetWingDef().GetUnlockOrder()))
			{
				nextWingToOpen = wing;
			}
		}
		return nextWingToOpen;
	}

	private void SaveScrollbarValue()
	{
		if (m_ScrollBar != null && AdventureConfig.Get() != null)
		{
			m_ScrollBar.SaveScroll(AdventureConfig.Get().GetSelectedAdventureAndModeString());
		}
	}

	private void Cheat_OpenNextWing()
	{
		if (!AdventureScene.Get().IsDevMode)
		{
			return;
		}
		AdventureWing nextWingToOpen = null;
		foreach (AdventureWing wing in m_BossWings)
		{
			if (wing.m_WingEventTable.IsPlateInOrGoingToAnActiveState() && (nextWingToOpen == null || wing.GetWingDef().GetUnlockOrder() < nextWingToOpen.GetWingDef().GetUnlockOrder()))
			{
				nextWingToOpen = wing;
			}
		}
		if (nextWingToOpen != null)
		{
			nextWingToOpen.UnlockPlate();
		}
	}

	private void Cheat_OpenNextChest()
	{
		if (AdventureScene.Get().IsDevMode)
		{
			m_WingsToGiveBigChest.Clear();
			if (s_cheat_nextWingToGrantChest >= m_BossWings.Count)
			{
				s_cheat_nextWingToGrantChest = 0;
			}
			AdventureWing wingToOpenChest = m_BossWings[s_cheat_nextWingToGrantChest];
			m_WingsToGiveBigChest.Add(wingToOpenChest);
			StartCoroutine(UpdateAndAnimateProgress(new List<AdventureWing> { wingToOpenChest }, scrollToCoin: false, forceCoinAnimation: true));
			s_cheat_nextWingToGrantChest++;
		}
	}

	public bool Cheat_AdventureEvent(string eventName)
	{
		if (!AdventureScene.Get().IsDevMode)
		{
			return false;
		}
		foreach (AdventureWing bossWing in m_BossWings)
		{
			bossWing.m_WingEventTable.TriggerState(eventName);
		}
		return true;
	}
}
