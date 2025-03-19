using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class GuestHeroPickerTrayDisplay : AbsDeckPickerTrayDisplay
{
	public delegate void GuestHeroSelectedCallback(TAG_CLASS classId, GuestHeroDbfRecord record);

	private struct GuestHeroRecordContainer
	{
		public GuestHeroDbfRecord GuestHeroRecord;

		public AdventureGuestHeroesDbfRecord AdventureGuestHeroRecord;

		public ScenarioGuestHeroesDbfRecord ScenarioGuestHeroRecord;
	}

	public UberText m_heroDescription;

	public UberText m_chooseHeroLabel;

	[CustomEditField(Sections = "Hero Divot Textures")]
	public Texture m_divotTextureDefault;

	[CustomEditField(Sections = "Hero Divot Textures")]
	public Texture m_divotTextureDalaran;

	[CustomEditField(Sections = "Hero Divot Textures")]
	public Texture m_divotTextureUldum;

	[CustomEditField(Sections = "Hero Divot Textures")]
	public Texture m_divotTexturePVPDR;

	private static GuestHeroPickerTrayDisplay s_instance;

	public override void Awake()
	{
		base.Awake();
		s_instance = this;
		HeroPickerDataModel dataModel = GetHeroPickerDataModel();
		if (dataModel != null)
		{
			dataModel.HasGuestHeroes = true;
		}
		VisualController visualController = GetComponent<VisualController>();
		if (visualController != null)
		{
			visualController.Owner.RegisterReadyListener(delegate
			{
				OnHeroPickerWidgetReady(visualController.Owner);
			});
		}
		else
		{
			Debug.LogError("AbsDeckPickerTrayDisplay.Awake - could not find visual controller. Ensure that this component is on the same object as the visual controller.");
		}
	}

	private void Start()
	{
		Navigation.PushIfNotOnTop(OnNavigateBack);
	}

	private void Update()
	{
		if (!(AdventureScene.Get() == null) && AdventureScene.Get().IsDevMode && InputCollection.GetKeyDown(KeyCode.Z))
		{
			Cheat_LockAllButtons();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Get() == this)
		{
			s_instance = null;
		}
	}

	public static bool OnNavigateBack()
	{
		if (Get() != null)
		{
			return Get().OnNavigateBackImplementation();
		}
		Debug.LogError("GuestHeroPickerTrayDisplay: tried to navigate back but had null instance!");
		return false;
	}

	protected override IEnumerator InitModeWhenReady()
	{
		yield return StartCoroutine(base.InitModeWhenReady());
		ShowFirstPage();
	}

	protected override void InitForMode(SceneMgr.Mode mode)
	{
		GetComponent<VisualController>();
		switch (mode)
		{
		case SceneMgr.Mode.ADVENTURE:
		{
			AdventureConfig adventureConfig = AdventureConfig.Get();
			AdventureDbId selectedAdv = adventureConfig.GetSelectedAdventure();
			AdventureModeDbId selectedMode = adventureConfig.GetSelectedMode();
			AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)selectedAdv, (int)selectedMode);
			SetHeaderText(dataRecord.Name);
			AdventureSubScene subscene = GuestHeroPickerDisplay.Get().GetComponent<AdventureSubScene>();
			if (subscene != null)
			{
				subscene.SetIsLoaded(loaded: true);
			}
			break;
		}
		case SceneMgr.Mode.FRIENDLY:
		case SceneMgr.Mode.TAVERN_BRAWL:
		{
			string glueHeader = ((TavernBrawlManager.Get().CurrentSeasonBrawlMode == TavernBrawlMode.TB_MODE_HEROIC) ? "GLOBAL_HEROIC_BRAWL" : "GLOBAL_TAVERN_BRAWL");
			TavernBrawlMission currentMission = TavernBrawlManager.Get().CurrentMission();
			ScenarioDbfRecord scenarioRecord = GameDbf.Scenario.GetRecord(currentMission.missionId);
			SetHeaderText(GameStrings.Get(glueHeader));
			if (scenarioRecord.ChooseHeroText != null)
			{
				SetChooseHeroText(scenarioRecord.ChooseHeroText);
			}
			if (GuestHeroPickerDisplay.Get() != null && mode != SceneMgr.Mode.FRIENDLY)
			{
				GuestHeroPickerDisplay.Get().ShowTray();
			}
			break;
		}
		}
		SetPlayButtonText(GameStrings.Get("GLOBAL_PLAY"));
		base.InitForMode(mode);
	}

	protected override void InitHeroPickerButtons()
	{
		base.InitHeroPickerButtons();
		List<GuestHeroRecordContainer> guestHeroContainers = GetGuestHeroes();
		if (guestHeroContainers == null)
		{
			Debug.LogError("InitHeroPickerButtons: Unable to get guest heroes to display.");
			return;
		}
		if (!GlobalDataContext.Get().GetDataModel(7, out var dataModel))
		{
			dataModel = new AdventureDataModel();
			GlobalDataContext.Get().BindDataModel(dataModel);
		}
		AdventureDataModel obj = dataModel as AdventureDataModel;
		if (obj == null)
		{
			Log.Adventures.PrintWarning("AdventureDataModel is null!");
		}
		Texture divotTexture = obj.SelectedAdventure switch
		{
			AdventureDbId.DALARAN => m_divotTextureDalaran, 
			AdventureDbId.ULDUM => m_divotTextureUldum, 
			_ => m_divotTextureDefault, 
		};
		m_heroDefsLoading = guestHeroContainers.Count;
		for (int i = 0; i < guestHeroContainers.Count; i++)
		{
			if (i >= m_heroButtons.Count || m_heroButtons[i] == null)
			{
				Debug.LogWarning("InitHeroPickerButtons: not enough buttons for total guest heroes.");
				break;
			}
			GuestHeroPickerButton button = m_heroButtons[i] as GuestHeroPickerButton;
			if (button == null)
			{
				Debug.LogWarning("InitHeroPickerButtons: attempted to display null button.");
				m_heroDefsLoading--;
				continue;
			}
			GuestHeroDbfRecord guestHero = guestHeroContainers[i].GuestHeroRecord;
			if (guestHero == null)
			{
				button.Lock();
				button.Activate(enable: false);
			}
			else
			{
				button.Unlock();
				button.Activate(enable: true);
			}
			long preconDeckID = 0L;
			TAG_CLASS heroClass = ((guestHero != null) ? GameUtils.GetTagClassFromCardDbId(guestHero.CardId) : TAG_CLASS.INVALID);
			if (heroClass != 0)
			{
				CollectionManager.PreconDeck preconDeck = CollectionManager.Get().GetPreconDeck(heroClass);
				if (preconDeck != null)
				{
					preconDeckID = preconDeck.ID;
				}
			}
			button.SetPreconDeckID(preconDeckID);
			button.SetGuestHero(guestHero);
			AdventureGuestHeroesDbfRecord adventureGuestHero = guestHeroContainers[i].AdventureGuestHeroRecord;
			if (adventureGuestHero != null)
			{
				HandleAdventureGuestHeroUnlockData(adventureGuestHero, button);
			}
			if (guestHero == null)
			{
				m_heroDefsLoading--;
				button.UpdateDisplay(null, TAG_PREMIUM.NORMAL);
			}
			else
			{
				string heroCardId = GameUtils.TranslateDbIdToCardId(guestHero.CardId);
				HeroFullDefLoadedCallbackData callbackData = new HeroFullDefLoadedCallbackData(button, TAG_PREMIUM.NORMAL);
				DefLoader.Get().LoadFullDef(heroCardId, OnHeroFullDefLoaded, callbackData);
			}
			button.SetDivotTexture(divotTexture);
		}
		if (IsChoosingHeroForDungeonCrawlAdventure())
		{
			SetUpHeroCrowns();
		}
	}

	protected override int ValidateHeroCount()
	{
		return GetGuestHeroes().Count;
	}

	protected override bool OnNavigateBackImplementation()
	{
		if (!base.OnNavigateBackImplementation())
		{
			return false;
		}
		switch ((SceneMgr.Get() != null) ? SceneMgr.Get().GetMode() : SceneMgr.Mode.INVALID)
		{
		case SceneMgr.Mode.ADVENTURE:
			if (AdventureConfig.Get() != null)
			{
				AdventureConfig.Get().SubSceneGoBack();
			}
			break;
		case SceneMgr.Mode.FRIENDLY:
		case SceneMgr.Mode.TAVERN_BRAWL:
			if (GuestHeroPickerDisplay.Get() != null)
			{
				GuestHeroPickerDisplay.Get().HideTray();
			}
			break;
		}
		return true;
	}

	public override void PreUnload()
	{
		if (m_randomDeckPickerTray.activeSelf)
		{
			m_randomDeckPickerTray.SetActive(value: false);
		}
	}

	protected override void UpdateHeroInfo(HeroPickerButton button)
	{
		using DefLoader.DisposableFullDef fullDef = button.ShareFullDef();
		string heroName = fullDef.EntityDef.GetName();
		string heroDescription = string.Empty;
		GuestHeroDbfRecord guestHero = button.GetGuestHero();
		if (guestHero != null)
		{
			heroDescription = guestHero.FlavorText;
		}
		TAG_PREMIUM premium = TAG_PREMIUM.NORMAL;
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE)
		{
			premium = TAG_PREMIUM.GOLDEN;
		}
		UpdateHeroInfo(fullDef, heroName, heroDescription, premium);
	}

	protected override void PlayGame()
	{
		base.PlayGame();
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE)
		{
			int myHeroId = GetSelectedHeroID();
			AdventureConfig ac = AdventureConfig.Get();
			if (OnPlayButtonPressed_SaveHeroAndAdvanceToDungeonRunIfNecessary())
			{
				GuestHeroDbfRecord guestHero = m_selectedHeroButton.GetGuestHero();
				if (guestHero != null)
				{
					AdventureGuestHeroesDbfRecord adventureGuestHero = GameDbf.AdventureGuestHeroes.GetRecord((AdventureGuestHeroesDbfRecord r) => r.AdventureId == (int)ac.GetSelectedAdventure() && r.GuestHeroId == guestHero.ID);
					if (adventureGuestHero != null && adventureGuestHero.CustomScenario != 0)
					{
						ac.SetMission((ScenarioDbId)adventureGuestHero.CustomScenario);
					}
				}
			}
			else
			{
				ac.SubSceneGoBack(fireevent: false);
				ScenarioDbId mission = ac.GetMissionToPlay();
				GameMgr.Get().FindGameWithHero(GameType.GT_VS_AI, FormatType.FT_WILD, (int)mission, 0, myHeroId, 0L);
			}
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_slidingTray.ToggleTraySlider(show: false);
		}
	}

	protected override void ShowHero()
	{
		UpdateHeroInfo(m_selectedHeroButton);
		base.ShowHero();
	}

	protected override void SelectHero(HeroPickerButton button, bool showTrayForPhone = true)
	{
		base.SelectHero(button, showTrayForPhone);
		UpdateSelectedHeroClasses(button);
		if (SceneMgr.Get() == null || SceneMgr.Get().GetMode() != SceneMgr.Mode.ADVENTURE)
		{
			return;
		}
		HeroPickerOptionDataModel buttonDataModel = button.GetDataModel();
		bool num = buttonDataModel?.IsTimelocked ?? false;
		bool isUnowned = buttonDataModel?.IsUnowned ?? false;
		if (num)
		{
			string desc = $"{buttonDataModel.ComingSoonText} ({buttonDataModel.UnlockCriteriaText})";
			AddHeroLockedTooltip(GameStrings.Get("GLOBAL_NOT_AVAILABLE"), desc);
		}
		else if (isUnowned)
		{
			AddHeroLockedTooltip(GameStrings.Get("GLUE_HERO_LOCKED_NAME"), buttonDataModel.UnlockCriteriaText);
		}
		else if (!button.IsLocked())
		{
			AdventureDataDbfRecord adventureData = AdventureConfig.Get().GetSelectedAdventureDataRecord();
			List<long> seenUnlockedHeroIds = null;
			if (!GameSaveDataManager.Get().GetSubkeyValue((GameSaveKeyId)adventureData.GameSaveDataClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_UNLOCKED_HEROES, out seenUnlockedHeroIds))
			{
				seenUnlockedHeroIds = new List<long>();
			}
			if (!seenUnlockedHeroIds.Contains(button.GetGuestHero().CardId))
			{
				seenUnlockedHeroIds.Add(m_selectedHeroButton.GetGuestHero().CardId);
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest((GameSaveKeyId)adventureData.GameSaveDataClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_UNLOCKED_HEROES, seenUnlockedHeroIds.ToArray()));
				button.GetDataModel().IsNewlyUnlocked = false;
			}
		}
	}

	protected override bool IsHeroPlayable(HeroPickerButton button)
	{
		HeroPickerOptionDataModel buttonDataModel = button.GetDataModel();
		if (!button.IsLocked())
		{
			if (buttonDataModel != null)
			{
				return !buttonDataModel.IsUnowned;
			}
			return true;
		}
		return false;
	}

	protected override bool ShouldShowHeroPower()
	{
		return true;
	}

	protected override void GoBackUntilOnNavigateBackCalled()
	{
		Navigation.GoBackUntilOnNavigateBackCalled(OnNavigateBack);
	}

	public void EnableBackButton(bool enabled)
	{
		if (m_backButton != null)
		{
			m_backButton.SetEnabled(enabled);
			m_backButton.Flip(enabled);
		}
	}

	private void OnHeroPickerWidgetReady(WidgetTemplate widget)
	{
		if (m_collectionButton != null)
		{
			SetCollectionButtonEnabled(enable: false);
		}
	}

	private void HandleAdventureGuestHeroUnlockData(AdventureGuestHeroesDbfRecord adventureGuestHeroRecord, HeroPickerButton button)
	{
		if (adventureGuestHeroRecord == null)
		{
			Debug.LogError("HandleGuestHeroUnlockEvents: No adventure guest hero passed in.");
			return;
		}
		WingDbfRecord wingRecord = GameDbf.Wing.GetRecord(adventureGuestHeroRecord.WingId);
		bool isWingUnlockEventActive = AdventureProgressMgr.IsWingEventActive(adventureGuestHeroRecord.WingId);
		string lockText = GetButtonLockedReasonText(wingRecord);
		button.SetLockReasonText(lockText);
		List<long> seenUnlockedHeroIds = null;
		AdventureDataDbfRecord adventureData = AdventureConfig.Get().GetSelectedAdventureDataRecord();
		GameSaveDataManager.Get().GetSubkeyValue((GameSaveKeyId)adventureData.GameSaveDataClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_UNLOCKED_HEROES, out seenUnlockedHeroIds);
		HeroPickerOptionDataModel dataModel = button.GetDataModel();
		dataModel.IsTimelocked = !isWingUnlockEventActive;
		dataModel.TimeLockInfoText = lockText;
		dataModel.ComingSoonText = adventureGuestHeroRecord.ComingSoonText;
		dataModel.UnlockCriteriaText = adventureGuestHeroRecord.UnlockCriteriaText;
		dataModel.IsUnowned = !AdventureProgressMgr.Get().OwnsWing(adventureGuestHeroRecord.WingId);
		dataModel.IsNewlyUnlocked = AdventureUtils.DoesAdventureShowNewlyUnlockedGuestHeroTreatment((AdventureDbId)adventureData.AdventureId) && isWingUnlockEventActive && !dataModel.IsUnowned && (seenUnlockedHeroIds == null || (button.GetGuestHero() != null && !seenUnlockedHeroIds.Contains(button.GetGuestHero().CardId)));
		if (!isWingUnlockEventActive)
		{
			button.Lock();
			button.Activate(enable: false);
		}
	}

	private List<GuestHeroRecordContainer> GetGuestHeroes()
	{
		List<GuestHeroRecordContainer> guestHeroContainers = null;
		List<AdventureGuestHeroesDbfRecord> adventureGuestHeroes = null;
		List<ScenarioGuestHeroesDbfRecord> scenarioGuestHeroes = null;
		switch (SceneMgr.Get().GetMode())
		{
		case SceneMgr.Mode.ADVENTURE:
			adventureGuestHeroes = GetSortedAdventureGuestHeroes();
			guestHeroContainers = GetGuestHeroes(adventureGuestHeroes);
			break;
		case SceneMgr.Mode.FRIENDLY:
		case SceneMgr.Mode.TAVERN_BRAWL:
			scenarioGuestHeroes = GetScenarioGuestHeroes();
			guestHeroContainers = GetGuestHeroes(scenarioGuestHeroes);
			break;
		}
		return guestHeroContainers;
	}

	private List<GuestHeroRecordContainer> GetGuestHeroes(List<AdventureGuestHeroesDbfRecord> adventureGuestHeroes)
	{
		List<GuestHeroRecordContainer> guestHeroContainers = new List<GuestHeroRecordContainer>();
		foreach (AdventureGuestHeroesDbfRecord adventureGuestHero in adventureGuestHeroes)
		{
			GuestHeroDbfRecord guestHero = GameDbf.GuestHero.GetRecord(adventureGuestHero.GuestHeroId);
			guestHeroContainers.Add(new GuestHeroRecordContainer
			{
				GuestHeroRecord = guestHero,
				AdventureGuestHeroRecord = adventureGuestHero
			});
		}
		return guestHeroContainers;
	}

	private List<GuestHeroRecordContainer> GetGuestHeroes(List<ScenarioGuestHeroesDbfRecord> scenarioGuestHeroes)
	{
		List<GuestHeroRecordContainer> guestHeroContainers = new List<GuestHeroRecordContainer>();
		foreach (ScenarioGuestHeroesDbfRecord scenarioGuestHero in scenarioGuestHeroes)
		{
			GuestHeroDbfRecord guestHero = GameDbf.GuestHero.GetRecord(scenarioGuestHero.GuestHeroId);
			guestHeroContainers.Add(new GuestHeroRecordContainer
			{
				GuestHeroRecord = guestHero,
				ScenarioGuestHeroRecord = scenarioGuestHero
			});
		}
		return guestHeroContainers;
	}

	private List<AdventureGuestHeroesDbfRecord> GetSortedAdventureGuestHeroes()
	{
		AdventureConfig ac = AdventureConfig.Get();
		AdventureDbId currentAdventure = ac.GetSelectedAdventure();
		List<AdventureGuestHeroesDbfRecord> records = GameDbf.AdventureGuestHeroes.GetRecords((AdventureGuestHeroesDbfRecord r) => r.AdventureId == (int)currentAdventure);
		records.Sort((AdventureGuestHeroesDbfRecord a, AdventureGuestHeroesDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder));
		return records;
	}

	private List<ScenarioGuestHeroesDbfRecord> GetScenarioGuestHeroes()
	{
		TavernBrawlMission currentMission = TavernBrawlManager.Get().CurrentMission();
		List<ScenarioGuestHeroesDbfRecord> records = GameDbf.ScenarioGuestHeroes.GetRecords((ScenarioGuestHeroesDbfRecord r) => r.ScenarioId == currentMission.missionId);
		records.Sort((ScenarioGuestHeroesDbfRecord a, ScenarioGuestHeroesDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder));
		return records;
	}

	private List<ScenarioGuestHeroesDbfRecord> GetPvPDungeonRunGuestHeroes()
	{
		int missionId = (int)AdventureConfig.Get().GetMission();
		List<ScenarioGuestHeroesDbfRecord> records = GameDbf.ScenarioGuestHeroes.GetRecords((ScenarioGuestHeroesDbfRecord r) => r.ScenarioId == missionId);
		records.Sort((ScenarioGuestHeroesDbfRecord a, ScenarioGuestHeroesDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder));
		return records;
	}

	private string GetButtonLockedReasonText(WingDbfRecord wingRecord)
	{
		if (wingRecord.UseUnlockCountdown)
		{
			EventTimingType wingEventTiming = AdventureProgressMgr.GetWingEventTiming(wingRecord.ID);
			TimeSpan? timeUntilUnlock = EventTimingManager.Get().GetEventStartTimeUtc(wingEventTiming) - DateTime.UtcNow;
			if (!timeUntilUnlock.HasValue)
			{
				return GameStrings.Get("GLOBAL_DATETIME_COMING_SOON");
			}
			TimeUtils.ElapsedStringSet stringSet = new TimeUtils.ElapsedStringSet
			{
				m_weeks = "GLOBAL_DATETIME_UNLOCKS_SOON_WEEKS"
			};
			return TimeUtils.GetElapsedTimeString((long)timeUntilUnlock.Value.TotalSeconds, stringSet, roundUp: true);
		}
		return wingRecord.ComingSoonLabel;
	}

	private void ShowFirstPage()
	{
		if (iTween.Count(m_randomDeckPickerTray) <= 0)
		{
			m_randomDeckPickerTray.SetActive(value: true);
			ShowPreconHighlights();
		}
	}

	private void ShowPreconHighlights()
	{
		if (!AbsDeckPickerTrayDisplay.HIGHLIGHT_SELECTED_DECK)
		{
			return;
		}
		foreach (HeroPickerButton button in m_heroButtons)
		{
			if (button == m_selectedHeroButton)
			{
				button.SetHighlightState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
			}
		}
	}

	private void UpdateHeroInfo(DefLoader.DisposableFullDef fullDef, string heroName, string heroDescription, TAG_PREMIUM premium)
	{
		m_heroName.Text = heroName;
		if (m_heroDescription != null)
		{
			m_heroDescription.Text = heroDescription;
		}
		m_selectedHeroName = fullDef.EntityDef.GetName();
		m_heroActor.SetPremium(premium);
		m_heroActor.SetEntityDef(fullDef.EntityDef);
		m_heroActor.SetCardDef(fullDef.DisposableCardDef);
		m_heroActor.UpdateAllComponents();
		m_heroActor.SetUnlit();
		string heroPowerID = GameUtils.GetHeroPowerCardIdFromHero(fullDef.EntityDef.GetCardId());
		if (!string.IsNullOrEmpty(heroPowerID))
		{
			UpdateHeroPowerInfo(fullDef, m_heroPowerDefs[heroPowerID], premium);
			return;
		}
		SetHeroPowerActorColliderEnabled(enable: false);
		HideHeroPowerActor();
	}

	private void Cheat_LockAllButtons()
	{
		foreach (HeroPickerButton heroButton in m_heroButtons)
		{
			heroButton.Lock();
			heroButton.Activate(enable: false);
		}
	}

	public static GuestHeroPickerTrayDisplay Get()
	{
		return s_instance;
	}

	public void SetChooseHeroText(string text)
	{
		if (m_chooseHeroLabel != null)
		{
			m_chooseHeroLabel.Text = text;
		}
	}

	private void UpdateSelectedHeroClasses(HeroPickerButton button)
	{
		VisualController visualController = GetComponent<VisualController>();
		if (visualController == null)
		{
			return;
		}
		HeroClassIconsDataModel dataModel = new HeroClassIconsDataModel();
		using DefLoader.DisposableFullDef def = button.ShareFullDef();
		EntityDef entityDef = def?.EntityDef;
		if (entityDef == null)
		{
			Debug.LogWarning("GuestHeroPickerTrayDisplay.UpdateSelectedHeroClasses - button did not contain an entity def!");
			return;
		}
		dataModel.Classes.Clear();
		entityDef.GetClasses(dataModel.Classes);
		dataModel.Classes.Sort((TAG_CLASS a, TAG_CLASS b) => (a == TAG_CLASS.NEUTRAL) ? 1 : (-1));
		visualController.Owner.BindDataModel(dataModel);
	}
}
