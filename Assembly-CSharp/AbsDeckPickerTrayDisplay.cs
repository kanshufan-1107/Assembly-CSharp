using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Assets;
using Blizzard.T5.Core;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public abstract class AbsDeckPickerTrayDisplay : MonoBehaviour
{
	public delegate void DeckTrayLoaded();

	public delegate void FormatTypePickerClosed();

	public enum HeroPickerLockedStatus
	{
		UNLOCKED = 1,
		LOCKED_IN_TWIST_SCENARIO,
		LOCKED_HERO_UNLOCK_NOT_DONE
	}

	protected class MythicHeroDeckData
	{
		public string heroPowerCardId;

		public CornerReplacementSpellType mythicHeroType;

		public TAG_PREMIUM premium;

		public Actor heroPowerActor;

		public Actor heroPowerBigCardActor;

		public PegUIElement heroPower;

		public MythicHeroDeckData(string heroPowerCardId, CornerReplacementSpellType mythicHeroType, TAG_PREMIUM premium)
		{
			this.heroPowerCardId = heroPowerCardId;
			this.mythicHeroType = mythicHeroType;
			this.premium = premium;
		}
	}

	private class CardCountWithPremium
	{
		public TAG_PREMIUM premium;

		public int count;

		public CardCountWithPremium(TAG_PREMIUM tag, int amount)
		{
			premium = tag;
			count = amount;
		}
	}

	protected class HeroFullDefLoadedCallbackData
	{
		[CompilerGenerated]
		private TAG_PREMIUM _003CPremium_003Ek__BackingField;

		public HeroPickerButton HeroPickerButton { get; private set; }

		private TAG_PREMIUM Premium
		{
			[CompilerGenerated]
			set
			{
				_003CPremium_003Ek__BackingField = value;
			}
		}

		public HeroFullDefLoadedCallbackData(HeroPickerButton button, TAG_PREMIUM premium)
		{
			HeroPickerButton = button;
			Premium = premium;
		}
	}

	protected static readonly PlatformDependentValue<bool> HIGHLIGHT_SELECTED_DECK = new PlatformDependentValue<bool>(PlatformCategory.Screen)
	{
		Phone = false,
		Tablet = true,
		PC = true
	};

	protected static readonly TAG_PREMIUM[] m_premiumHeirarchy = new TAG_PREMIUM[4]
	{
		TAG_PREMIUM.NORMAL,
		TAG_PREMIUM.GOLDEN,
		TAG_PREMIUM.DIAMOND,
		TAG_PREMIUM.SIGNATURE
	};

	public GameObject m_randomDeckPickerTray;

	public Transform m_Hero_Bone;

	public Transform m_Hero_BoneDown;

	public Transform m_HeroName_Bone;

	public Transform m_Ranked_Hero_Bone;

	public Transform m_Ranked_Hero_BoneDown;

	public Transform m_Ranked_HeroName_Bone;

	public Transform m_HeroPower_Bone;

	public Transform m_HeroPower_BoneDown;

	public Transform m_playButtonRoot;

	public AsyncReference m_playButtonWidgetReference;

	public UberText m_heroName;

	[CustomEditField(Sections = "Hero Button Placement")]
	public List<GameObject> m_heroPickerButtonBonesByHeroCount;

	public float m_heroPickerButtonHorizontalSpacing;

	public float m_heroPickerButtonVerticalSpacing;

	public GameObject m_hierarchyDetails;

	public GameObject m_hierarchyDetailsHeroRoot;

	public GameObject m_deckPickerFrame;

	public AsyncReference m_alternateDeckFrameTwist;

	public GameObject m_basicDeckPageContainer;

	public GameObject m_tooltipPrefab;

	public Transform m_tooltipBone;

	public UberText m_modeName;

	public UIBButton m_backButton;

	public UIBButton m_collectionButton;

	public GameObject m_basicDeckPage;

	public GameObject m_trayFrame;

	public GameObject m_randomDecksShownBone;

	public GameObject m_heroPowerContainer;

	public GameObject m_heroPowerShadowQuad;

	[CustomEditField(Sections = "Prefab References", T = EditType.GAME_OBJECT)]
	public string m_heroButtonWidgetPrefab;

	[CustomEditField(Sections = "Prefab References", T = EditType.GAME_OBJECT)]
	public string m_heroPickerCrownPrefab;

	protected FormatType m_PreviousFormatType;

	protected bool m_PreviousInRankedPlayMode;

	protected bool m_isMouseOverHeroPower;

	private bool m_playButtonEnabled;

	private bool m_heroRaised = true;

	protected int m_heroDefsLoading = int.MaxValue;

	protected int m_HeroPickerButtonCount;

	protected List<HeroPickerButton> m_heroButtons = new List<HeroPickerButton>();

	protected Map<string, DefLoader.DisposableFullDef> m_heroPowerDefs = new Map<string, DefLoader.DisposableFullDef>();

	protected List<DeckTrayLoaded> m_DeckTrayLoadedListeners = new List<DeckTrayLoaded>();

	protected List<FormatTypePickerClosed> m_FormatTypePickerClosedListeners = new List<FormatTypePickerClosed>();

	protected string m_selectedHeroName;

	protected bool m_Loaded;

	protected LockedHeroTooltipPanel m_heroLockedTooltip;

	protected DefLoader.DisposableFullDef m_selectedHeroPowerFullDef;

	protected HeroPickerButton m_selectedHeroButton;

	protected SlidingTray m_slidingTray;

	protected PlayButton m_playButton;

	private AudioSource m_lastPickLine;

	protected PegUIElement m_heroPower;

	protected PegUIElement m_goldenHeroPower;

	protected Actor m_heroActor;

	protected Actor m_heroPowerActor;

	protected Actor m_goldenHeroPowerActor;

	protected Actor m_heroPowerBigCard;

	protected Actor m_goldenHeroPowerBigCard;

	protected Actor m_mythicHeroPowerActor;

	protected Actor m_mythicHeroPowerBigCard;

	protected PegUIElement m_mythicHeroPower;

	protected Dictionary<string, MythicHeroDeckData> m_mythicHeroDeckData = new Dictionary<string, MythicHeroDeckData>();

	protected List<Transform> m_heroBones;

	protected List<TAG_CLASS> m_validClasses = new List<TAG_CLASS>();

	protected Widget m_alternateDeckFrameWidget;

	private Dictionary<TAG_PREMIUM, int> m_premiumPriority = new Dictionary<TAG_PREMIUM, int>();

	public Transform empty;

	public virtual void Awake()
	{
		m_randomDeckPickerTray.transform.localPosition = m_randomDecksShownBone.transform.localPosition;
		DeckPickerTray.Get().SetDeckPickerTrayDisplayReference(this);
		DeckPickerTray.Get().RegisterHandlers();
		if (m_backButton != null)
		{
			m_backButton.SetText(GameStrings.Get("GLOBAL_BACK"));
			m_backButton.AddEventListener(UIEventType.RELEASE, OnBackButtonReleased);
		}
		m_playButtonWidgetReference.RegisterReadyListener<VisualController>(OnPlayButtonWidgetReady);
		if (m_alternateDeckFrameTwist != null)
		{
			m_alternateDeckFrameTwist.RegisterReadyListener<WidgetInstance>(OnAlternateDeckFrameTwistWidgetReady);
		}
		if (m_heroPowerShadowQuad != null)
		{
			m_heroPowerShadowQuad.SetActive(value: false);
		}
		if (m_heroName != null)
		{
			m_heroName.RichText = false;
			m_heroName.Text = "";
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_slidingTray = base.gameObject.GetComponentInChildren<SlidingTray>();
			m_slidingTray.RegisterTrayToggleListener(OnSlidingTrayToggled);
		}
		PopupDisplayManager.Get().AddPopupShownListener(OnPopupShown);
		for (int i = 0; i < m_premiumHeirarchy.Length; i++)
		{
			m_premiumPriority.Add(m_premiumHeirarchy[i], i);
		}
	}

	protected virtual void OnDestroy()
	{
		PopupDisplayManager.Get()?.RemovePopupShownListener(OnPopupShown);
		if (DeckPickerTray.IsInitialized())
		{
			DeckPickerTray.Get().UnregisterHandlers();
		}
		m_heroPowerDefs.DisposeValuesAndClear();
		m_selectedHeroPowerFullDef?.Dispose();
		m_selectedHeroPowerFullDef = null;
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (GameMgr.Get().IsFindingGame())
		{
			GameMgr.Get().CancelFindGame();
		}
	}

	private static TAG_PREMIUM GetHighestOwnedPremium(string cardId)
	{
		int premiumNormalCount = 0;
		int premiumGoldCount = 0;
		int premiumSignatureCount = 0;
		int premiumDiamondCount = 0;
		TAG_PREMIUM highestOwnedPremium = TAG_PREMIUM.NORMAL;
		Dictionary<TAG_PREMIUM, int> premiumCount = new Dictionary<TAG_PREMIUM, int>();
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager != null)
		{
			collectionManager.GetOwnedCardCount(cardId, out premiumNormalCount, out premiumGoldCount, out premiumSignatureCount, out premiumDiamondCount);
			premiumCount.Add(TAG_PREMIUM.SIGNATURE, premiumSignatureCount);
			premiumCount.Add(TAG_PREMIUM.GOLDEN, premiumGoldCount);
			premiumCount.Add(TAG_PREMIUM.DIAMOND, premiumDiamondCount);
			premiumCount.Add(TAG_PREMIUM.NORMAL, premiumNormalCount);
			foreach (CounterpartCardsDbfRecord counterpartCard in GameUtils.GetCounterpartCards(cardId))
			{
				string counterpartCardId = GameUtils.TranslateDbIdToCardId(counterpartCard.DeckEquivalentCardId);
				collectionManager.GetOwnedCardCount(counterpartCardId, out premiumNormalCount, out premiumGoldCount, out premiumSignatureCount, out premiumDiamondCount);
				premiumCount[TAG_PREMIUM.SIGNATURE] += premiumSignatureCount;
				premiumCount[TAG_PREMIUM.GOLDEN] += premiumGoldCount;
				premiumCount[TAG_PREMIUM.DIAMOND] += premiumDiamondCount;
				premiumCount[TAG_PREMIUM.NORMAL] += premiumNormalCount;
			}
			for (int i = m_premiumHeirarchy.Length - 1; i >= 0; i--)
			{
				if (premiumCount[m_premiumHeirarchy[i]] > 0)
				{
					return m_premiumHeirarchy[i];
				}
			}
		}
		return highestOwnedPremium;
	}

	public virtual void HandleGameStartupFailure()
	{
		SetPlayButtonEnabled(enable: true);
		SetBackButtonEnabled(enable: true);
		SetHeroButtonsEnabled(enable: true);
		SetHeroRaised(raised: true);
	}

	public virtual void OnServerGameStarted()
	{
		FriendChallengeMgr.Get().RemoveChangedListener(OnFriendChallengeChanged);
	}

	public virtual void OnServerGameCanceled()
	{
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.FRIENDLY && !TavernBrawlManager.IsInTavernBrawlFriendlyChallenge())
		{
			HandleGameStartupFailure();
		}
	}

	public bool IsChoosingHero()
	{
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		if (mode != SceneMgr.Mode.COLLECTIONMANAGER && mode != SceneMgr.Mode.TAVERN_BRAWL && !IsChoosingHeroForTavernBrawlChallenge())
		{
			return IsChoosingHeroForDungeonCrawlAdventure();
		}
		return true;
	}

	protected virtual bool OnNavigateBackImplementation()
	{
		if (!m_backButton.IsEnabled())
		{
			return false;
		}
		if (((SceneMgr.Get() != null) ? SceneMgr.Get().GetMode() : SceneMgr.Mode.INVALID) == SceneMgr.Mode.FRIENDLY)
		{
			BackOutToHub();
			if (FriendChallengeMgr.Get() != null)
			{
				FriendChallengeMgr.Get().CancelChallenge();
			}
		}
		SetPlayButtonEnabled(enable: false);
		SetBackButtonEnabled(enable: false);
		SetHeroButtonsEnabled(enable: false);
		GameMgr.Get().CancelFindGame();
		SoundManager.Get().Stop(m_lastPickLine);
		return true;
	}

	protected virtual void OnHeroActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"AbsDeckPickerTrayDisplay.OnHeroActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		m_heroActor = go.GetComponent<Actor>();
		if (m_heroActor == null)
		{
			Debug.LogWarning($"AbsDeckPickerTrayDisplay.OnHeroActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		if (m_hierarchyDetailsHeroRoot != null)
		{
			go.transform.parent = m_hierarchyDetailsHeroRoot.transform;
		}
		else
		{
			go.transform.parent = m_hierarchyDetails.transform;
		}
		UpdateHeroActorOrientation();
		m_heroActor.SetUnlit();
		Object.Destroy(m_heroActor.m_attackObject);
		m_heroActor.Hide();
	}

	protected virtual void OnHeroFullDefLoaded(string cardId, DefLoader.DisposableFullDef fullDef, object userData)
	{
		using (fullDef)
		{
			EntityDef entityDef = fullDef?.EntityDef;
			if (entityDef != null)
			{
				HeroFullDefLoadedCallbackData callbackData = userData as HeroFullDefLoadedCallbackData;
				TAG_PREMIUM premium = ((!GameUtils.IsVanillaHero(cardId)) ? TAG_PREMIUM.GOLDEN : CollectionManager.Get().GetBestCardPremium(cardId));
				callbackData.HeroPickerButton.UpdateDisplay(fullDef, premium);
				Vector3 originalLocalPos = ((callbackData.HeroPickerButton.m_raiseAndLowerRoot != null) ? callbackData.HeroPickerButton.m_raiseAndLowerRoot.transform.localPosition : callbackData.HeroPickerButton.transform.localPosition);
				callbackData.HeroPickerButton.SetOriginalLocalPosition(originalLocalPos);
				if (entityDef.GetClass() != TAG_CLASS.WHIZBANG)
				{
					string heroPowerCardId = GameUtils.GetHeroPowerCardIdFromHero(entityDef.GetCardId());
					if (!string.IsNullOrEmpty(heroPowerCardId))
					{
						LoadHeroPowerDef(heroPowerCardId, premium);
					}
					else
					{
						Debug.LogErrorFormat("No hero power set up for hero {0}", entityDef.GetCardId());
					}
				}
			}
			m_heroDefsLoading--;
		}
	}

	protected virtual void OnSlidingTrayToggled(bool isShowing)
	{
		if (!isShowing && PracticePickerTrayDisplay.Get() != null && PracticePickerTrayDisplay.Get().IsShown())
		{
			Navigation.GoBack();
		}
	}

	public virtual void ResetCurrentMode()
	{
		if (m_selectedHeroButton != null)
		{
			SetPlayButtonEnabled(enable: true);
			SetHeroRaised(raised: true);
		}
		else
		{
			SetPlayButtonEnabled(enable: false);
		}
		SetHeroButtonsEnabled(enable: true);
	}

	public virtual void PreUnload()
	{
	}

	public virtual void InitAssets()
	{
		Log.PlayModeInvestigation.PrintInfo("AbsDeckPickerTrayDisplay.InitAssets() called");
		if (!GameUtils.HasCompletedApprentice() && Options.GetInRankedPlayMode())
		{
			Options.SetInRankedPlayMode(inRankedPlayMode: false);
		}
		else if (GameUtils.HasCompletedApprentice())
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_INTRO_SEEN_COUNT, out long rankedIntroSeenCount);
			if (rankedIntroSeenCount == 0L)
			{
				Options.SetInRankedPlayMode(inRankedPlayMode: true);
			}
		}
		m_PreviousFormatType = Options.GetFormatType();
		m_PreviousInRankedPlayMode = Options.GetInRankedPlayMode();
		m_HeroPickerButtonCount = ValidateHeroCount();
		SetupHeroLayout();
		if (IsChoosingHeroForHeroicDeck() && (bool)UniversalInputManager.UsePhoneUI)
		{
			ApplyDeckPickerTrayConfig();
		}
		LoadHero();
		if (ShouldShowHeroPower())
		{
			LoadHeroPower();
			LoadGoldenHeroPower();
		}
		StartCoroutine(LoadHeroButtons(null));
		StartCoroutine(InitModeWhenReady());
	}

	protected virtual IEnumerator WaitForHeroPickerButtonsLoaded()
	{
		while (m_heroButtons.Count < m_HeroPickerButtonCount)
		{
			yield return null;
		}
		foreach (HeroPickerButton button in m_heroButtons)
		{
			while (button.GetComponent<WidgetTemplate>().IsChangingStates)
			{
				yield return null;
			}
		}
	}

	protected virtual IEnumerator InitDeckDependentElements()
	{
		yield return StartCoroutine(WaitForHeroPickerButtonsLoaded());
		InitForMode(SceneMgr.Get().GetMode());
	}

	protected virtual IEnumerator InitHeroPickerElements()
	{
		yield return StartCoroutine(WaitForHeroPickerButtonsLoaded());
		InitHeroPickerButtons();
	}

	protected virtual IEnumerator InitModeWhenReady()
	{
		while (m_heroDefsLoading > 0 || m_heroActor == null || ((m_heroPowerActor == null || m_goldenHeroPowerActor == null) && ShouldShowHeroPower()))
		{
			yield return null;
		}
		m_Loaded = true;
		PlayGameScene scene = SceneMgr.Get().GetScene() as PlayGameScene;
		if (scene != null)
		{
			scene.OnDeckPickerLoaded(this);
		}
		FireDeckTrayLoadedEvent();
		InitRichPresence(null);
		SetBackButtonEnabled(enable: true);
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.FRIENDLY || TavernBrawlManager.IsInTavernBrawlFriendlyChallenge())
		{
			if (FriendChallengeMgr.Get().HasChallenge())
			{
				FriendChallengeMgr.Get().AddChangedListener(OnFriendChallengeChanged);
			}
			else
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Get().GetPrevMode());
			}
		}
	}

	protected virtual void InitForMode(SceneMgr.Mode mode)
	{
		switch (mode)
		{
		case SceneMgr.Mode.ADVENTURE:
			if (AdventureConfig.Get() != null && AdventureConfig.Get().IsHeroSelectedBeforeDungeonCrawlScreenForSelectedAdventure())
			{
				SetPlayButtonText(GameStrings.Get("GLUE_CHOOSE"));
			}
			break;
		case SceneMgr.Mode.COLLECTIONMANAGER:
			SetPlayButtonText(GameStrings.Get("GLUE_CHOOSE"));
			break;
		case SceneMgr.Mode.FRIENDLY:
		{
			string playButtonText = "GLUE_CHOOSE";
			SetPlayButtonText(GameStrings.Get(playButtonText));
			break;
		}
		}
	}

	protected virtual void InitHeroPickerButtons()
	{
	}

	protected virtual void InitRichPresence(Global.PresenceStatus? newStatus = null)
	{
		switch (SceneMgr.Get().GetMode())
		{
		case SceneMgr.Mode.ADVENTURE:
			if (AdventureConfig.Get().CurrentSubScene == AdventureData.Adventuresubscene.PRACTICE)
			{
				newStatus = Global.PresenceStatus.PRACTICE_DECKPICKER;
			}
			break;
		case SceneMgr.Mode.FRIENDLY:
			newStatus = Global.PresenceStatus.FRIENDLY_DECKPICKER;
			if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
			{
				newStatus = Global.PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING;
			}
			break;
		case SceneMgr.Mode.TAVERN_BRAWL:
			if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
			{
				newStatus = Global.PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING;
			}
			break;
		}
		if (newStatus.HasValue)
		{
			PresenceMgr.Get().SetStatus(newStatus.Value);
		}
	}

	protected virtual void TransitionToFormatType(FormatType formatType, bool inRankedPlayMode, float transitionSpeed = 2f)
	{
		if (formatType == FormatType.FT_UNKNOWN)
		{
			RankMgr.LogMessage("formatType being passed in = FT_UNKOWN", "TransitionToFormatType", "D:\\p4Workspace\\31.4.0\\Pegasus\\Client\\Assets\\Game\\DeckPickerTray\\AbsDeckPickerTrayDisplay.cs", 731);
			return;
		}
		Options.SetFormatType(formatType);
		Options.SetInRankedPlayMode(inRankedPlayMode);
		UpdateHeroActorOrientation();
	}

	protected virtual void PlayGame()
	{
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		if ((mode == SceneMgr.Mode.FRIENDLY || mode == SceneMgr.Mode.TAVERN_BRAWL) && TavernBrawlManager.Get().SelectHeroBeforeMission())
		{
			if (m_selectedHeroButton == null)
			{
				Debug.LogError("Trying to play Tavern Brawl game with no m_selectedHeroButton!");
				return;
			}
			int heroCardDbId = GameUtils.TranslateCardIdToDbId(m_selectedHeroButton.GetEntityDef().GetCardId());
			if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
			{
				FriendChallengeMgr.Get().SelectHero(heroCardDbId);
				FriendlyChallengeHelper.Get().StartChallengeOrWaitForOpponent("GLOBAL_FRIEND_CHALLENGE_TAVERN_BRAWL_OPPONENT_WAITING_READY", OnFriendChallengeWaitingForOpponentDialogResponse);
			}
			else
			{
				TavernBrawlManager.Get().StartGameWithHero(heroCardDbId);
			}
		}
		SoundManager.Get().Stop(m_lastPickLine);
	}

	protected virtual void ShowHero()
	{
		m_heroActor.Show();
		string heroPowerID = GameUtils.GetHeroPowerCardIdFromHero(m_heroActor.GetEntityDef().GetCardId());
		if (ShouldShowHeroPower() && !string.IsNullOrEmpty(heroPowerID))
		{
			if (IsMythicHero(m_heroActor.GetEntityDef()))
			{
				ShowMythicHeroPower(heroPowerID);
			}
			else
			{
				TAG_PREMIUM premium = m_heroActor.GetPremium();
				ShowHeroPower(premium);
			}
		}
		else
		{
			m_heroPowerShadowQuad.SetActive(value: false);
			if (m_heroPowerActor != null)
			{
				m_heroPowerActor.Hide();
			}
			if (m_goldenHeroPower != null)
			{
				m_goldenHeroPowerActor.Hide();
			}
			if (m_mythicHeroPowerActor != null)
			{
				m_mythicHeroPowerActor.Hide();
			}
		}
		if (m_selectedHeroName == null)
		{
			m_heroName.Text = "";
		}
	}

	protected virtual void SelectHero(HeroPickerButton button, bool showTrayForPhone = true)
	{
		if (!(button == m_selectedHeroButton) || (bool)UniversalInputManager.UsePhoneUI)
		{
			DeselectLastSelectedHero();
			if ((bool)HIGHLIGHT_SELECTED_DECK)
			{
				button.SetHighlightState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
			}
			else
			{
				button.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
			}
			m_selectedHeroButton = button;
			UpdateHeroInfo(button);
			button.SetSelected(isSelected: true);
			ShowPreconHero(show: true);
			RemoveHeroLockedTooltip();
			if ((bool)UniversalInputManager.UsePhoneUI && showTrayForPhone)
			{
				m_slidingTray.ToggleTraySlider(show: true);
			}
			bool isHeroPlayable = IsHeroPlayable(button);
			if (isHeroPlayable && !NotificationManager.Get().IsQuotePlaying && button.HasCardDef)
			{
				SoundManager.Get().LoadAndPlay(button.HeroPickerSelectedPrefab, button.gameObject, 1f, OnLastPickLineLoaded);
			}
			SetPlayButtonEnabled(isHeroPlayable);
		}
	}

	protected virtual void UpdateHeroInfo(HeroPickerButton button)
	{
	}

	protected virtual void BackOutToHub()
	{
		if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
		{
			if (FriendChallengeMgr.Get() != null)
			{
				FriendChallengeMgr.Get().RemoveChangedListener(OnFriendChallengeChanged);
			}
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		}
	}

	protected void UpdateValidHeroClasses()
	{
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		if (Options.GetFormatType() == FormatType.FT_CLASSIC && mode != SceneMgr.Mode.ADVENTURE)
		{
			m_validClasses = new List<TAG_CLASS>(GameUtils.CLASSIC_ORDERED_HERO_CLASSES);
		}
		else
		{
			m_validClasses = new List<TAG_CLASS>(GameUtils.ORDERED_HERO_CLASSES);
		}
		if (!IsChoosingHero())
		{
			m_validClasses.Add(TAG_CLASS.WHIZBANG);
		}
		List<ClassExclusionsDbfRecord> classExclusionList = GetClassExclusions(mode);
		for (int i = 0; i < classExclusionList.Count; i++)
		{
			m_validClasses.Remove((TAG_CLASS)classExclusionList[i].ClassId);
		}
	}

	protected List<ClassExclusionsDbfRecord> GetClassExclusions(SceneMgr.Mode mode)
	{
		List<ClassExclusionsDbfRecord> exclusionList = new List<ClassExclusionsDbfRecord>();
		ScenarioDbId? scenarioId = null;
		if (mode == SceneMgr.Mode.ADVENTURE)
		{
			scenarioId = AdventureConfig.Get().GetMission();
		}
		if (mode == SceneMgr.Mode.TAVERN_BRAWL || FriendChallengeMgr.Get().IsChallengeTavernBrawl())
		{
			scenarioId = (ScenarioDbId)TavernBrawlManager.Get().CurrentMission().missionId;
		}
		if (scenarioId.HasValue && scenarioId != ScenarioDbId.INVALID)
		{
			ScenarioDbfRecord scenarioIdRecord = GameDbf.Scenario.GetRecord((int)scenarioId.Value);
			exclusionList.AddRange(scenarioIdRecord.ClassExclusions);
		}
		return exclusionList;
	}

	protected virtual int ValidateHeroCount()
	{
		UpdateValidHeroClasses();
		return m_validClasses.Count;
	}

	protected virtual bool ShouldShowHeroPower()
	{
		return false;
	}

	private bool ShouldUseRankedModeLayoutForHero()
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT)
		{
			if (!Options.GetInRankedPlayMode())
			{
				return !GameUtils.HasCompletedApprentice();
			}
			return true;
		}
		return false;
	}

	private Transform GetActiveHeroBone()
	{
		bool useRankedBones = ShouldUseRankedModeLayoutForHero();
		if (m_heroRaised)
		{
			if (!useRankedBones)
			{
				return m_Hero_Bone;
			}
			return m_Ranked_Hero_Bone;
		}
		if (!useRankedBones)
		{
			return m_Hero_BoneDown;
		}
		return m_Ranked_Hero_BoneDown;
	}

	private Transform GetActiveHeroNameBone()
	{
		if (ShouldUseRankedModeLayoutForHero())
		{
			return m_Ranked_HeroName_Bone;
		}
		return m_HeroName_Bone;
	}

	private void UpdateHeroActorOrientation()
	{
		if (m_heroActor != null)
		{
			iTween.Stop(m_heroActor.gameObject);
			Transform activeHeroBone = GetActiveHeroBone();
			if (activeHeroBone != null)
			{
				m_heroActor.transform.localScale = activeHeroBone.localScale;
				m_heroActor.transform.localPosition = activeHeroBone.localPosition;
			}
		}
		if (m_heroName != null)
		{
			Transform activeHeroNameBone = GetActiveHeroNameBone();
			if (activeHeroNameBone != null)
			{
				m_heroName.transform.localScale = activeHeroNameBone.localScale;
				m_heroName.transform.localPosition = activeHeroNameBone.localPosition;
			}
		}
	}

	protected virtual void SetHeroRaised(bool raised)
	{
		m_heroRaised = raised;
		Transform activeHeroBone = GetActiveHeroBone();
		if (activeHeroBone == null || m_HeroPower_Bone == null || m_HeroPower_BoneDown == null)
		{
			Debug.LogWarning("SetHeroRaised tried using transforms that were undefined!");
			return;
		}
		Vector3 heroPosition = activeHeroBone.localPosition;
		Vector3 heroPowerPosition = (raised ? m_HeroPower_Bone.localPosition : m_HeroPower_BoneDown.localPosition);
		MoveToRaisedPosition(heroPosition, m_heroActor, raised);
		if (ShouldShowHeroPower())
		{
			m_heroPowerShadowQuad.SetActive(raised);
			MoveToRaisedPosition(heroPowerPosition, m_heroPowerActor, raised, m_heroPower);
			MoveToRaisedPosition(heroPowerPosition, m_goldenHeroPowerActor, raised, m_goldenHeroPower);
			MoveToRaisedPosition(heroPowerPosition, m_mythicHeroPowerActor, raised, m_mythicHeroPower);
		}
	}

	private void MoveToRaisedPosition(Vector3 position, Actor actor, bool raised, PegUIElement pegUiElement = null)
	{
		if (actor == null)
		{
			return;
		}
		Hashtable iTweenHash = iTweenManager.Get().GetTweenHashTable();
		iTweenHash.Add("position", position);
		iTweenHash.Add("time", 0.25f);
		iTweenHash.Add("easetype", iTween.EaseType.easeOutExpo);
		iTweenHash.Add("islocal", true);
		iTween.MoveTo(actor.gameObject, iTweenHash);
		if (pegUiElement != null)
		{
			Collider collider = pegUiElement.GetComponent<Collider>();
			if (collider != null)
			{
				collider.enabled = raised;
			}
			else
			{
				Debug.LogWarning("Could not locate Collider for " + pegUiElement.name + " when trying to SetHeroRaised");
			}
		}
	}

	protected virtual void SetPlayButtonEnabled(bool enable)
	{
		if (enable && SceneMgr.Get().GetMode() == SceneMgr.Mode.FRIENDLY && !FriendChallengeMgr.Get().HasChallenge())
		{
			return;
		}
		m_playButtonEnabled = enable;
		if (m_playButton != null && m_playButton.IsEnabled() != enable)
		{
			if (enable)
			{
				m_playButton.Enable();
			}
			else
			{
				m_playButton.Disable();
			}
		}
	}

	protected virtual void SetCollectionButtonEnabled(bool enable)
	{
		if (m_collectionButton != null)
		{
			m_collectionButton.SetEnabled(enable);
			m_collectionButton.Flip(enable);
		}
	}

	protected virtual void SetHeroButtonsEnabled(bool enable)
	{
		foreach (HeroPickerButton button in m_heroButtons)
		{
			if (!button.IsLocked() || !enable)
			{
				button.SetEnabled(enable);
			}
		}
	}

	protected virtual void SetHeaderForTavernBrawl()
	{
		string buttonLabelKey = "GLUE_CHOOSE";
		if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
		{
			buttonLabelKey = "GLUE_BRAWL_FRIEND";
		}
		else if (TavernBrawlManager.Get().SelectHeroBeforeMission())
		{
			buttonLabelKey = "GLUE_BRAWL";
		}
		SetPlayButtonText(GameStrings.Get(buttonLabelKey));
	}

	protected virtual bool IsHeroPlayable(HeroPickerButton button)
	{
		return !button.IsLocked();
	}

	public virtual int GetSelectedHeroID()
	{
		if (m_selectedHeroButton != null)
		{
			GuestHeroDbfRecord guestHeroRecord = m_selectedHeroButton.GetGuestHero();
			if (guestHeroRecord != null)
			{
				return guestHeroRecord.CardId;
			}
		}
		return 0;
	}

	public virtual long GetSelectedDeckID()
	{
		if (!(m_selectedHeroButton == null))
		{
			return m_selectedHeroButton.GetPreconDeckID();
		}
		return 0L;
	}

	protected abstract void GoBackUntilOnNavigateBackCalled();

	protected virtual void OnBackButtonReleased(UIEvent e)
	{
		Navigation.GoBack();
		TwistDetailsDisplayManager manager = TwistDetailsDisplayManager.Get();
		if (manager != null)
		{
			manager.ToggleTwistDeckDisplayAnimationStatePC(shouldBeOpen: false);
			manager.ToggleTwistDeckDisplayPC(showDeckDisplay: false);
		}
	}

	protected virtual void OnPlayGameButtonReleased(UIEvent e)
	{
		if (!Network.IsLoggedIn() && SceneMgr.Get().GetMode() != SceneMgr.Mode.COLLECTIONMANAGER)
		{
			DialogManager.Get().ShowReconnectHelperDialog();
		}
		else if (!SetRotationManager.Get().CheckForSetRotationRollover() && (PlayerMigrationManager.Get() == null || !PlayerMigrationManager.Get().CheckForPlayerMigrationRequired()))
		{
			SetPlayButtonEnabled(enable: false);
			SetHeroButtonsEnabled(enable: false);
			PlayGame();
		}
	}

	protected virtual void OnHeroButtonReleased(UIEvent e)
	{
		HeroPickerButton button = (HeroPickerButton)e.GetElement();
		SelectHero(button);
		SoundManager.Get().LoadAndPlay("tournament_screen_select_hero.prefab:2b9bdf587ac07084b8f7d5c4bce33ecf");
	}

	protected virtual void OnHeroMouseOver(UIEvent e)
	{
		if (e != null)
		{
			((HeroPickerButton)e.GetElement()).SetHighlightState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
			SoundManager.Get().LoadAndPlay("collection_manager_hero_mouse_over.prefab:653cc8000b988cd468d2210a209adce6");
		}
	}

	protected virtual void OnHeroMouseOut(UIEvent e)
	{
		HeroPickerButton button = (HeroPickerButton)e.GetElement();
		if (!UniversalInputManager.UsePhoneUI || !button.IsSelected())
		{
			button.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
		}
	}

	protected virtual void OnHeroPowerMouseOver(UIEvent e)
	{
		m_isMouseOverHeroPower = true;
		EntityDef heroDef = m_heroActor.GetEntityDef();
		TAG_PREMIUM premium = m_heroActor.GetPremium();
		bool num = IsMythicHero(heroDef);
		string heroPowerID = ((heroDef != null) ? GameUtils.GetHeroPowerCardIdFromHero(heroDef.GetCardId()) : null);
		if (num && !string.IsNullOrEmpty(heroPowerID))
		{
			if (!m_mythicHeroDeckData.TryGetValue(heroPowerID, out var mythicData))
			{
				Debug.LogWarningFormat("OnHeroPowerMouseOver - Attempted Mouse Over before setting up Hero data for {0}", heroDef);
			}
			else if (m_mythicHeroPowerBigCard == null)
			{
				LoadMythicHeroPowerBigCard(mythicData);
			}
			else
			{
				ShowHeroPowerBigCard(m_mythicHeroPowerBigCard);
			}
		}
		else if (premium == TAG_PREMIUM.GOLDEN)
		{
			if (m_goldenHeroPowerBigCard == null)
			{
				AssetLoader.Get().InstantiatePrefab(ActorNames.GetNameWithPremiumType(ActorNames.ACTOR_ASSET.HISTORY_HERO_POWER, TAG_PREMIUM.GOLDEN), OnGoldenHeroPowerLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
			}
			else
			{
				ShowHeroPowerBigCard(m_goldenHeroPowerBigCard);
			}
		}
		else if (m_heroPowerBigCard == null)
		{
			AssetLoader.Get().InstantiatePrefab("History_HeroPower.prefab:e73edf8ccea2b11429093f7a448eef53", OnHeroPowerLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
		else
		{
			ShowHeroPowerBigCard(m_heroPowerBigCard);
		}
	}

	protected virtual void OnHeroPowerMouseOut(UIEvent e)
	{
		m_isMouseOverHeroPower = false;
		if (m_heroPowerBigCard != null)
		{
			iTween.Stop(m_heroPowerBigCard.gameObject);
			m_heroPowerBigCard.Hide();
		}
		if (m_goldenHeroPowerBigCard != null)
		{
			iTween.Stop(m_goldenHeroPowerBigCard.gameObject);
			m_goldenHeroPowerBigCard.Hide();
		}
		if (m_mythicHeroPowerBigCard != null)
		{
			iTween.Stop(m_mythicHeroPowerBigCard.gameObject);
			m_mythicHeroPowerBigCard.Hide();
		}
	}

	protected void LoadHeroPowerDef(string heroPowerCardId, TAG_PREMIUM premium = TAG_PREMIUM.NORMAL)
	{
		DefLoader.DisposableFullDef def = DefLoader.Get().GetFullDef(heroPowerCardId, new CardPortraitQuality(3, premium));
		m_heroPowerDefs.SetOrReplaceDisposable(heroPowerCardId, def);
	}

	protected void OnPlayButtonWidgetReady(VisualController visualController)
	{
		if (visualController == null)
		{
			Error.AddDevWarning("UI Error!", "PlayButton could not be found! You will not be able to click 'Play'!");
			return;
		}
		m_playButton = visualController.GetComponent<PlayButton>();
		if (!(m_playButton == null))
		{
			m_playButton.AddEventListener(UIEventType.RELEASE, OnPlayGameButtonReleased);
			SetPlayButtonEnabled(m_playButtonEnabled);
		}
	}

	protected void OnAlternateDeckFrameTwistWidgetReady(WidgetInstance widget)
	{
		if (!(widget == null))
		{
			m_alternateDeckFrameWidget = widget;
			widget.gameObject.SetActive(value: false);
			if (IsChoosingHeroForHeroicDeck())
			{
				ToggleAlternateDeckFrame(showNewDeckPickerFrame: true);
			}
		}
	}

	protected virtual void ToggleAlternateDeckFrame(bool showNewDeckPickerFrame)
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (showNewDeckPickerFrame)
			{
				if (m_deckPickerFrame != null)
				{
					m_deckPickerFrame.GetComponent<MeshRenderer>().enabled = false;
				}
				if (m_alternateDeckFrameWidget != null)
				{
					m_alternateDeckFrameWidget.gameObject.SetActive(value: true);
				}
				ApplyDeckPickerTrayConfig();
			}
			else
			{
				if (m_deckPickerFrame != null)
				{
					m_deckPickerFrame.GetComponent<MeshRenderer>().enabled = true;
				}
				if (m_alternateDeckFrameWidget != null)
				{
					m_alternateDeckFrameWidget.gameObject.SetActive(value: false);
				}
				RemoveDeckPickerTrayConfig();
			}
		}
		else
		{
			TwistDetailsDisplayManager manager = TwistDetailsDisplayManager.Get();
			if (manager != null)
			{
				manager.ToggleTwistDeckDisplayAnimationStatePC(shouldBeOpen: false);
				manager.ToggleTwistDeckDisplayPC(showNewDeckPickerFrame);
			}
		}
	}

	protected virtual void RemoveDeckPickerTrayConfig()
	{
	}

	protected virtual void ApplyDeckPickerTrayConfig()
	{
	}

	protected virtual void SetupCardListForSelectedDeck(List<CardWithPremiumStatus> deckContent, LoanerDeckDetailsController deckDetailsController)
	{
		if (deckDetailsController == null)
		{
			return;
		}
		TwistDetailsDisplayManager twistDetailsManager = TwistDetailsDisplayManager.Get();
		if (twistDetailsManager != null)
		{
			twistDetailsManager.ToggleTwistHeroicDeckCardPanel(openPanel: false);
		}
		List<CardTileDataModel> cardTileList = new List<CardTileDataModel>();
		Dictionary<int, CardCountWithPremium> cardCountPairings = new Dictionary<int, CardCountWithPremium>();
		for (int i = 0; i < deckContent.Count; i++)
		{
			if (cardCountPairings.ContainsKey((int)deckContent[i].cardId))
			{
				cardCountPairings[(int)deckContent[i].cardId].count++;
			}
			else
			{
				cardCountPairings.Add((int)deckContent[i].cardId, new CardCountWithPremium(deckContent[i].premium, 1));
			}
		}
		foreach (KeyValuePair<int, CardCountWithPremium> pair in cardCountPairings)
		{
			string cardId = GameUtils.TranslateDbIdToCardId(pair.Key);
			CardTileDataModel newTile = new CardTileDataModel
			{
				CardId = cardId,
				Count = pair.Value.count,
				Premium = pair.Value.premium
			};
			cardTileList.Add(newTile);
		}
		deckDetailsController.ShowDeckCardList(cardTileList);
	}

	protected virtual void SetupCardListForSelectedDeck(List<CardWithPremiumStatus> deckContent, int deckTemplateId, LoanerDeckDetailsController deckDetailsController)
	{
		DeckTemplateDbfRecord deckTemplateRecord = GameDbf.DeckTemplate.GetRecord(deckTemplateId);
		if (deckTemplateRecord == null)
		{
			return;
		}
		TAG_PREMIUM highestPremiumOfUnlockReq = TAG_PREMIUM.NORMAL;
		TAG_PREMIUM currentPremium = TAG_PREMIUM.NORMAL;
		for (int i = 0; i < deckTemplateRecord.OwnershipReqList.Count; i++)
		{
			currentPremium = GetHighestOwnedPremium(GameUtils.TranslateDbIdToCardId(deckTemplateRecord.OwnershipReqList[i].ReqCardId));
			if (m_premiumPriority[currentPremium] > m_premiumPriority[highestPremiumOfUnlockReq])
			{
				highestPremiumOfUnlockReq = currentPremium;
			}
		}
		for (int j = 0; j < deckContent.Count; j++)
		{
			string cardId = GameUtils.TranslateDbIdToCardId((int)deckContent[j].cardId);
			TAG_PREMIUM highestOwnedPremium = GetHighestOwnedPremium(cardId);
			if (m_premiumPriority[highestPremiumOfUnlockReq] <= m_premiumPriority[highestOwnedPremium])
			{
				deckContent[j].premium = highestOwnedPremium;
				continue;
			}
			TAG_PREMIUM premiumToUse = TAG_PREMIUM.GOLDEN;
			DefLoader loader = DefLoader.Get();
			if (loader != null)
			{
				EntityDef entityDef = loader.GetEntityDef(cardId);
				bool cardHasDiamondVersion = entityDef.HasTag(GAME_TAG.HAS_DIAMOND_QUALITY);
				bool num = entityDef.HasTag(GAME_TAG.HAS_SIGNATURE_QUALITY);
				if (cardHasDiamondVersion && m_premiumPriority[highestPremiumOfUnlockReq] >= m_premiumPriority[TAG_PREMIUM.DIAMOND])
				{
					premiumToUse = TAG_PREMIUM.DIAMOND;
				}
				if (num && m_premiumPriority[highestPremiumOfUnlockReq] >= m_premiumPriority[TAG_PREMIUM.SIGNATURE])
				{
					premiumToUse = TAG_PREMIUM.SIGNATURE;
				}
			}
			deckContent[j].premium = premiumToUse;
		}
		SetupCardListForSelectedDeck(deckContent, deckDetailsController);
	}

	protected virtual void UpdateDeckDisplayDataForHeroicDecks(CollectionDeck deck, TwistHeroicDeckDataModel deckDetailsDataModel)
	{
		LoanerDeckDetailsController deckDetailsController = null;
		TwistDetailsDisplayManager twistDisplay = TwistDetailsDisplayManager.Get();
		List<CardWithPremiumStatus> deckContent = deck.GetCardsWithPremiumStatus();
		int deckTemplateId = deck.DeckTemplateId;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_alternateDeckFrameWidget != null)
			{
				deckDetailsController = m_alternateDeckFrameWidget.GetComponentInChildren<LoanerDeckDetailsController>();
			}
			if (twistDisplay != null)
			{
				twistDisplay.ToggleRankedHeaderDisplayMobile(!deck.DeckTemplate_HasUnownedRequirements(out var _));
			}
		}
		else if (twistDisplay != null && twistDisplay.TwistDeckTrayDisplayWidget != null)
		{
			deckDetailsController = twistDisplay.TwistDeckTrayDisplayWidget.GetComponentInChildren<LoanerDeckDetailsController>();
		}
		if (!(deckDetailsController == null))
		{
			Widget widget = deckDetailsController.GetComponent<Widget>();
			if (widget != null && deckDetailsDataModel != null)
			{
				widget.BindDataModel(deckDetailsDataModel);
			}
			if (deckContent != null && deckContent.Count > 0)
			{
				SetupCardListForSelectedDeck(deckContent, deckTemplateId, deckDetailsController);
			}
		}
	}

	protected void OnHeroPickerButtonWidgetReady(WidgetInstance widget)
	{
		HeroPickerButton button = widget.GetComponentInChildren<HeroPickerButton>();
		m_heroButtons.Add(button);
		SetUpHeroPickerButton(button, m_heroButtons.Count - 1);
		button.Lock();
		button.Activate(enable: false);
		button.AddEventListener(UIEventType.RELEASE, OnHeroButtonReleased);
		button.AddEventListener(UIEventType.ROLLOVER, OnHeroMouseOver);
		button.AddEventListener(UIEventType.ROLLOUT, OnHeroMouseOut);
		Vector3 originalLocalPos = ((button.m_raiseAndLowerRoot != null) ? button.m_raiseAndLowerRoot.transform.localPosition : base.transform.localPosition);
		button.SetOriginalLocalPosition(originalLocalPos);
	}

	protected void OnHeroPowerActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"AbsDeckPickerTrayDisplay.OnHeroPowerActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		m_heroPowerActor = go.GetComponent<Actor>();
		if (m_heroPowerActor == null)
		{
			Debug.LogWarning($"AbsDeckPickerTrayDisplay.OnHeroPowerActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		m_heroPower = go.AddComponent<PegUIElement>();
		go.AddComponent<BoxCollider>();
		GameUtils.SetParent(go, m_heroPowerContainer);
		go.transform.localScale = m_HeroPower_Bone.localScale;
		go.transform.localPosition = m_HeroPower_Bone.localPosition;
		if (m_heroPowerShadowQuad != null)
		{
			m_heroPowerShadowQuad.transform.localPosition = m_HeroPower_Bone.localPosition;
		}
		m_heroPowerActor.SetUnlit();
		m_heroPower.AddEventListener(UIEventType.ROLLOVER, OnHeroPowerMouseOver);
		m_heroPower.AddEventListener(UIEventType.ROLLOUT, OnHeroPowerMouseOut);
		m_heroPowerActor.Hide();
		m_heroPower.GetComponent<Collider>().enabled = false;
		StartCoroutine(UpdateHeroSkinHeroPower());
	}

	protected void OnGoldenHeroPowerActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"AbsDeckPickerTrayDisplay.OnHeroPowerActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		m_goldenHeroPowerActor = go.GetComponent<Actor>();
		if (m_goldenHeroPowerActor == null)
		{
			Debug.LogWarning($"AbsDeckPickerTrayDisplay.OnHeroPowerActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		m_goldenHeroPower = go.AddComponent<PegUIElement>();
		go.AddComponent<BoxCollider>();
		GameUtils.SetParent(go, m_heroPowerContainer);
		go.transform.localScale = m_HeroPower_Bone.localScale;
		go.transform.localPosition = m_HeroPower_Bone.localPosition;
		if (m_heroPowerShadowQuad != null)
		{
			m_heroPowerShadowQuad.transform.localPosition = m_HeroPower_Bone.localPosition;
		}
		m_goldenHeroPowerActor.SetUnlit();
		m_goldenHeroPowerActor.SetPremium(TAG_PREMIUM.GOLDEN);
		m_goldenHeroPower.AddEventListener(UIEventType.ROLLOVER, OnHeroPowerMouseOver);
		m_goldenHeroPower.AddEventListener(UIEventType.ROLLOUT, OnHeroPowerMouseOut);
		m_goldenHeroPowerActor.Hide();
		m_goldenHeroPower.GetComponent<Collider>().enabled = false;
	}

	protected void OnHeroPowerLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"AbsDeckPickerTrayDisplay.LoadHeroPowerCallback() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarning($"AbsDeckPickerTrayDisplay.LoadHeroPowerCallback() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		actor.transform.parent = m_heroPower.transform;
		actor.TurnOffCollider();
		LayerUtils.SetLayer(actor.gameObject, m_heroPower.gameObject.layer, null);
		if (m_heroPowerBigCard != null && m_heroPowerBigCard.IsShown())
		{
			m_heroPowerBigCard.Hide();
		}
		m_heroPowerBigCard = actor;
		if ((bool)UniversalInputManager.UsePhoneUI && m_slidingTray != null)
		{
			PopupRoot popupRoot = m_slidingTray.GetComponent<PopupRoot>();
			if (popupRoot != null)
			{
				popupRoot.ApplyPopupRendering(actor.transform, null, overrideLayer: true);
			}
		}
		if (m_isMouseOverHeroPower)
		{
			ShowHeroPowerBigCard(m_heroPowerBigCard);
		}
	}

	protected void OnGoldenHeroPowerLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"AbsDeckPickerTrayDisplay.LoadHeroPowerCallback() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarning($"AbsDeckPickerTrayDisplay.LoadHeroPowerCallback() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		actor.transform.parent = m_heroPower.transform;
		actor.TurnOffCollider();
		LayerUtils.SetLayer(actor.gameObject, m_heroPower.gameObject.layer, null);
		m_goldenHeroPowerBigCard = actor;
		if ((bool)UniversalInputManager.UsePhoneUI && m_slidingTray != null)
		{
			PopupRoot popupRoot = m_slidingTray.GetComponent<PopupRoot>();
			if (popupRoot != null)
			{
				popupRoot.ApplyPopupRendering(actor.transform, null, overrideLayer: true);
			}
		}
		if (m_isMouseOverHeroPower)
		{
			ShowHeroPowerBigCard(m_goldenHeroPowerBigCard);
		}
	}

	protected void OnPopupShown()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_slidingTray.ToggleTraySlider(show: false, null, animate: false);
		}
	}

	private void OnLastPickLineLoaded(AudioSource source, object callbackData)
	{
		SoundManager.Get().Stop(m_lastPickLine);
		m_lastPickLine = source;
	}

	protected virtual void OnFriendChallengeWaitingForOpponentDialogResponse(AlertPopup.Response response, object userData)
	{
		if (response == AlertPopup.Response.CANCEL && !FriendChallengeMgr.Get().AmIInGameState())
		{
			ResetCurrentMode();
			FriendChallengeMgr.Get().DeselectDeckOrHero();
			FriendlyChallengeHelper.Get().StopWaitingForFriendChallenge();
		}
	}

	protected virtual void OnFriendChallengeChanged(FriendChallengeEvent challengeEvent, BnetPlayer player, FriendlyChallengeData challengeData, object userData)
	{
		switch (challengeEvent)
		{
		case FriendChallengeEvent.SELECTED_DECK_OR_HERO:
			if (!SceneMgr.Get().IsInTavernBrawlMode() && player != BnetPresenceMgr.Get().GetMyPlayer() && FriendChallengeMgr.Get().DidISelectDeckOrHero())
			{
				FriendlyChallengeHelper.Get().HideFriendChallengeWaitingForOpponentDialog();
			}
			break;
		case FriendChallengeEvent.OPPONENT_CANCELED_CHALLENGE:
		case FriendChallengeEvent.OPPONENT_REMOVED_FROM_FRIENDS:
		case FriendChallengeEvent.QUEUE_CANCELED:
			FriendlyChallengeHelper.Get().StopWaitingForFriendChallenge();
			GoBackUntilOnNavigateBackCalled();
			break;
		case FriendChallengeEvent.DESELECTED_DECK_OR_HERO:
			if (player != BnetPresenceMgr.Get().GetMyPlayer())
			{
				if (FriendChallengeMgr.Get().DidISelectDeckOrHero())
				{
					FriendlyChallengeHelper.Get().StartChallengeOrWaitForOpponent("GLOBAL_FRIEND_CHALLENGE_OPPONENT_WAITING_DECK", OnFriendChallengeWaitingForOpponentDialogResponse);
					break;
				}
				ResetCurrentMode();
				SetBackButtonEnabled(enable: true);
			}
			break;
		}
	}

	protected IEnumerator LoadHeroButtons(int? m_cheatOverrideHeroPickerButtonCount = null)
	{
		if (m_cheatOverrideHeroPickerButtonCount.HasValue)
		{
			m_HeroPickerButtonCount = m_cheatOverrideHeroPickerButtonCount.Value;
		}
		else
		{
			m_HeroPickerButtonCount = ValidateHeroCount();
		}
		SetupHeroLayout();
		foreach (HeroPickerButton heroButton in m_heroButtons)
		{
			Object.Destroy(heroButton.gameObject);
		}
		m_heroButtons.Clear();
		HeroPickerDataModel dataModel = GetHeroPickerDataModel();
		for (int i = 0; i < m_HeroPickerButtonCount; i++)
		{
			WidgetInstance heroPickerButtonWidget = WidgetInstance.Create(m_heroButtonWidgetPrefab);
			if (dataModel != null)
			{
				heroPickerButtonWidget.BindDataModel(dataModel);
			}
			heroPickerButtonWidget.RegisterReadyListener(delegate
			{
				OnHeroPickerButtonWidgetReady(heroPickerButtonWidget);
			});
		}
		yield return StartCoroutine(InitDeckDependentElements());
		StartCoroutine(InitHeroPickerElements());
	}

	protected void SetupHeroLayout()
	{
		if (m_HeroPickerButtonCount <= 0 || m_HeroPickerButtonCount > m_heroPickerButtonBonesByHeroCount.Count || m_heroPickerButtonBonesByHeroCount[m_HeroPickerButtonCount] == null)
		{
			Log.Adventures.PrintWarning("Deck/Class Picker Instantiated with an unsupported amount of heroes: " + m_HeroPickerButtonCount);
			return;
		}
		GameObject layout = m_heroPickerButtonBonesByHeroCount[m_HeroPickerButtonCount];
		m_heroBones = new List<Transform>();
		m_heroBones.AddRange(layout.GetComponentsInChildren<Transform>());
		m_heroBones.RemoveAt(0);
		if (m_heroBones.Count != m_HeroPickerButtonCount)
		{
			Log.Adventures.PrintWarning("Layout for {0} heroes yielded an incorrect amount of transforms. This will result in errors when displaying heroes!", m_HeroPickerButtonCount);
		}
	}

	protected void LoadHero()
	{
		if (m_heroActor == null)
		{
			AssetLoader.Get().InstantiatePrefab("Card_Play_Hero.prefab:42cbbd2c4969afb46b3887bb628de19d", OnHeroActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
	}

	protected void LoadHeroPower()
	{
		if (m_heroPowerActor == null)
		{
			AssetLoader.Get().InstantiatePrefab("Card_Play_HeroPower.prefab:a3794839abb947146903a26be13e09af", OnHeroPowerActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
	}

	protected void LoadGoldenHeroPower()
	{
		if (m_goldenHeroPowerActor == null)
		{
			AssetLoader.Get().InstantiatePrefab(ActorNames.GetNameWithPremiumType(ActorNames.ACTOR_ASSET.PLAY_HERO_POWER, TAG_PREMIUM.GOLDEN), OnGoldenHeroPowerActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
	}

	protected void LoadMythicHeroPower(MythicHeroDeckData mythicHeroData)
	{
		if (mythicHeroData != null && mythicHeroData.mythicHeroType != 0)
		{
			CornerReplacementSpellType mythicHeroType = mythicHeroData.mythicHeroType;
			TAG_PREMIUM premium = mythicHeroData.premium;
			string replacementActorName = CornerReplacementConfig.Get().GetActor(mythicHeroType, ActorNames.ACTOR_ASSET.PLAY_HERO_POWER, premium);
			if (string.IsNullOrEmpty(replacementActorName))
			{
				Debug.LogWarningFormat("AbsDeckPickerTrayDisplay.onMythicHeroPowerLoaded() - Unable to find override actor for {0}", mythicHeroType);
			}
			AssetLoader.Get().InstantiatePrefab(replacementActorName, onMythicHeroPowerLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
		void onMythicHeroPowerLoaded(AssetReference assetRef, GameObject go, object callbackData)
		{
			if (go == null)
			{
				Debug.LogWarning($"AbsDeckPickerTrayDisplay.onMythicHeroPowerLoaded() - FAILED to load actor \"{assetRef}\"");
			}
			else
			{
				mythicHeroData.heroPowerActor = go.GetComponent<Actor>();
				if (mythicHeroData.heroPowerActor == null)
				{
					Debug.LogWarning($"AbsDeckPickerTrayDisplay.onMythicHeroPowerLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
				}
				else
				{
					mythicHeroData.heroPower = go.AddComponent<PegUIElement>();
					go.AddComponent<BoxCollider>();
					GameUtils.SetParent(go, m_heroPowerContainer);
					go.transform.localScale = m_HeroPower_Bone.localScale;
					go.transform.localPosition = m_HeroPower_Bone.localPosition;
					if (m_heroPowerShadowQuad != null)
					{
						m_heroPowerShadowQuad.transform.localPosition = m_HeroPower_Bone.localPosition;
					}
					mythicHeroData.heroPowerActor.SetUnlit();
					mythicHeroData.heroPowerActor.SetPremium(mythicHeroData.premium);
					mythicHeroData.heroPowerActor.SetFullDef(m_heroPowerDefs[mythicHeroData.heroPowerCardId]);
					mythicHeroData.heroPowerActor.SetUnlit();
					mythicHeroData.heroPowerActor.UpdateAllComponents();
					mythicHeroData.heroPower.AddEventListener(UIEventType.ROLLOVER, OnHeroPowerMouseOver);
					mythicHeroData.heroPower.AddEventListener(UIEventType.ROLLOUT, OnHeroPowerMouseOut);
					mythicHeroData.heroPowerActor.Hide();
					mythicHeroData.heroPower.GetComponent<Collider>().enabled = false;
					m_mythicHeroDeckData[mythicHeroData.heroPowerCardId] = mythicHeroData;
					if (GameUtils.GetHeroPowerCardIdFromHero(m_heroActor.GetEntityDef().GetCardId()) == mythicHeroData.heroPowerCardId)
					{
						ShowMythicHeroPower(mythicHeroData.heroPowerCardId);
					}
				}
			}
		}
	}

	protected void LoadMythicHeroPowerBigCard(MythicHeroDeckData mythicHeroData)
	{
		if (mythicHeroData != null && mythicHeroData.mythicHeroType != 0)
		{
			CornerReplacementSpellType mythicHeroType = mythicHeroData.mythicHeroType;
			TAG_PREMIUM premium = mythicHeroData.premium;
			string replacementActorName = CornerReplacementConfig.Get().GetActor(mythicHeroType, ActorNames.ACTOR_ASSET.HISTORY_HERO_POWER, premium);
			if (string.IsNullOrEmpty(replacementActorName))
			{
				Debug.LogWarningFormat("AbsDeckPickerTrayDisplay.LoadMythicHeroPowerBigCard() - Unable to find override actor for {0}", mythicHeroType);
			}
			AssetLoader.Get().InstantiatePrefab(replacementActorName, onMythicHeroPowerBigCardLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
		void onMythicHeroPowerBigCardLoaded(AssetReference assetRef, GameObject go, object callbackData)
		{
			if (go == null)
			{
				Debug.LogWarning($"AbsDeckPickerTrayDisplay.onMythicHeroPowerBigCardLoaded() - FAILED to load actor \"{assetRef}\"");
			}
			else
			{
				Actor actor = go.GetComponent<Actor>();
				if (actor == null)
				{
					Debug.LogWarning($"AbsDeckPickerTrayDisplay.onMythicHeroPowerBigCardLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
				}
				else
				{
					actor.transform.parent = m_heroPower.transform;
					actor.TurnOffCollider();
					LayerUtils.SetLayer(actor.gameObject, m_heroPower.gameObject.layer, null);
					mythicHeroData.heroPowerBigCardActor = actor;
					m_mythicHeroPowerBigCard = mythicHeroData.heroPowerBigCardActor;
					m_mythicHeroDeckData[mythicHeroData.heroPowerCardId] = mythicHeroData;
					if ((bool)UniversalInputManager.UsePhoneUI && m_slidingTray != null)
					{
						PopupRoot popupRoot = m_slidingTray.GetComponent<PopupRoot>();
						if (popupRoot != null)
						{
							popupRoot.ApplyPopupRendering(actor.transform, null, overrideLayer: true);
						}
					}
					if (m_isMouseOverHeroPower)
					{
						ShowHeroPowerBigCard(m_mythicHeroPowerBigCard);
					}
				}
			}
		}
	}

	protected IEnumerator UpdateHeroSkinHeroPower()
	{
		while (m_heroActor == null || !m_heroActor.HasCardDef)
		{
			yield return null;
		}
		HeroSkinHeroPower hshp = m_heroPowerActor.gameObject.GetComponentInChildren<HeroSkinHeroPower>();
		if (!(hshp == null))
		{
			hshp.m_Actor.AlwaysRenderPremiumPortrait = !GameUtils.IsVanillaHero(m_heroActor.GetEntityDef().GetCardId());
			hshp.m_Actor.UpdateMaterials();
			hshp.m_Actor.UpdateTextures();
		}
	}

	protected MythicHeroDeckData LoadMythicHeroDeckData(EntityDef entityDef, TAG_PREMIUM premium)
	{
		string heroPowerID = ((entityDef != null) ? GameUtils.GetHeroPowerCardIdFromHero(entityDef.GetCardId()) : null);
		if (string.IsNullOrEmpty(heroPowerID))
		{
			Debug.LogErrorFormat("LoadMythicHeroDeckData - No hero power set up for hero {0}", entityDef.GetCardId());
			return null;
		}
		CornerReplacementSpellType mythicType = entityDef.GetTag<CornerReplacementSpellType>(GAME_TAG.CORNER_REPLACEMENT_TYPE);
		if (mythicType == CornerReplacementSpellType.NONE)
		{
			Debug.LogErrorFormat("LoadMythicHeroDeckData - {0} is not a mythic hero type", entityDef.GetCardId());
			return null;
		}
		if (!m_heroPowerDefs.ContainsKey(heroPowerID))
		{
			LoadHeroPowerDef(heroPowerID, premium);
		}
		if (!m_mythicHeroDeckData.TryGetValue(heroPowerID, out var mythicHeroData))
		{
			mythicHeroData = new MythicHeroDeckData(heroPowerID, mythicType, premium);
			m_mythicHeroDeckData[heroPowerID] = mythicHeroData;
			LoadMythicHeroPower(mythicHeroData);
		}
		return mythicHeroData;
	}

	protected bool IsMythicHero(EntityDef entityDef)
	{
		if (entityDef == null)
		{
			return false;
		}
		return entityDef.GetTag<CornerReplacementSpellType>(GAME_TAG.CORNER_REPLACEMENT_TYPE) != CornerReplacementSpellType.NONE;
	}

	protected void UpdateHeroPowerInfo(DefLoader.DisposableFullDef fullDef, DefLoader.DisposableFullDef heroPowerDef, TAG_PREMIUM premium)
	{
		SetHeroPowerActorColliderEnabled();
		m_selectedHeroPowerFullDef?.Dispose();
		m_selectedHeroPowerFullDef = heroPowerDef?.Share();
		if (m_heroPowerActor != null)
		{
			m_heroPowerActor.SetFullDef(heroPowerDef);
			m_heroPowerActor.SetUnlit();
			m_heroPowerActor.UpdateAllComponents();
		}
		if (m_goldenHeroPowerActor != null)
		{
			m_goldenHeroPowerActor.SetFullDef(heroPowerDef);
			m_goldenHeroPowerActor.UpdateAllComponents();
			m_goldenHeroPowerActor.SetUnlit();
		}
		if (IsMythicHero(fullDef.EntityDef))
		{
			MythicHeroDeckData mythicData = LoadMythicHeroDeckData(fullDef.EntityDef, premium);
			if (mythicData != null)
			{
				ShowMythicHeroPower(mythicData.heroPowerCardId);
			}
		}
		else
		{
			ShowHeroPower(premium);
			if (premium != TAG_PREMIUM.GOLDEN)
			{
				StartCoroutine(UpdateHeroSkinHeroPower());
			}
		}
	}

	protected void UpdateCustomHeroPowerBigCard(GameObject heroPowerBigCard)
	{
		if (!m_heroActor.HasCardDef)
		{
			Debug.LogWarning("AbsDeckPickerTrayDisplay.UpdateCustomHeroPowerBigCard heroCardDef = null!");
			return;
		}
		Actor hpBigCardActor = heroPowerBigCard.GetComponentInChildren<Actor>();
		if (hpBigCardActor != null)
		{
			hpBigCardActor.AlwaysRenderPremiumPortrait = ShouldRenderPremiumPortrait();
			hpBigCardActor.UpdateMaterials();
		}
	}

	protected bool ShouldRenderPremiumPortrait()
	{
		if (m_heroActor == null)
		{
			return false;
		}
		EntityDef entityDef = m_heroActor.GetEntityDef();
		if (entityDef == null)
		{
			return false;
		}
		TAG_CARD_SET cardSet = entityDef.GetCardSet();
		TAG_PREMIUM premium = m_heroActor.GetPremium();
		string cardId = entityDef.GetCardId();
		bool isGolden = premium == TAG_PREMIUM.GOLDEN;
		bool isVanilla = GameUtils.IsVanillaHero(cardId);
		if (cardSet == TAG_CARD_SET.HERO_SKINS)
		{
			if (!isGolden)
			{
				return !isVanilla;
			}
			return true;
		}
		return false;
	}

	protected void ShowHeroPowerBigCard(Actor heroPowerBigCardToShow)
	{
		if (m_heroPowerBigCard != null)
		{
			m_heroPowerBigCard.Hide();
		}
		if (m_goldenHeroPowerBigCard != null)
		{
			m_goldenHeroPowerBigCard.Hide();
		}
		if (m_mythicHeroPowerBigCard != null)
		{
			m_mythicHeroPowerBigCard.Hide();
		}
		if (!(heroPowerBigCardToShow == null) && !(m_selectedHeroPowerFullDef?.CardDef == null))
		{
			heroPowerBigCardToShow.SetCardDef(m_selectedHeroPowerFullDef.DisposableCardDef);
			heroPowerBigCardToShow.SetEntityDef(m_selectedHeroPowerFullDef.EntityDef);
			heroPowerBigCardToShow.UpdateAllComponents();
			heroPowerBigCardToShow.Show();
			UpdateCustomHeroPowerBigCard(heroPowerBigCardToShow.gameObject);
			float BIG_SCALE_VAL = 1f;
			float SCALE_VAL = 1.5f;
			Vector3 START_POSITION = (UniversalInputManager.Get().IsTouchMode() ? new Vector3(0.019f, 0.54f, 3f) : new Vector3(0.019f, 0.54f, -1.12f));
			GameObject go = heroPowerBigCardToShow.gameObject;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				go.transform.localPosition = new Vector3(-7.5f, 0.6f, -2.14f);
				go.transform.localScale = Vector3.one * 2.9f;
				AnimationUtil.GrowThenDrift(go, m_HeroPower_Bone.transform.position, 2f);
				return;
			}
			Vector3 ANIM_OFFSET = (PlatformSettings.IsTablet ? new Vector3(0f, 0.1f, 0.1f) : new Vector3(0.1f, 0.1f, 0.1f));
			go.transform.localPosition = START_POSITION;
			go.transform.localScale = Vector3.one * BIG_SCALE_VAL;
			iTween.ScaleTo(go, Vector3.one * SCALE_VAL, 0.15f);
			iTween.MoveTo(go, iTween.Hash("position", START_POSITION + ANIM_OFFSET, "islocal", true, "time", 10f));
		}
	}

	protected void ShowHeroPower(TAG_PREMIUM premium)
	{
		if (m_heroPowerShadowQuad != null)
		{
			m_heroPowerShadowQuad.SetActive(value: true);
		}
		if ((bool)m_mythicHeroPowerActor)
		{
			m_mythicHeroPowerActor.Hide();
		}
		if (premium == TAG_PREMIUM.GOLDEN)
		{
			if ((bool)m_heroPowerActor)
			{
				m_heroPowerActor.Hide();
			}
			if ((bool)m_goldenHeroPowerActor)
			{
				m_goldenHeroPowerActor.Show();
			}
			if ((bool)m_goldenHeroPower)
			{
				m_goldenHeroPower.GetComponent<Collider>().enabled = true;
			}
		}
		else
		{
			if ((bool)m_goldenHeroPowerActor)
			{
				m_goldenHeroPowerActor.Hide();
			}
			if ((bool)m_heroPowerActor)
			{
				m_heroPowerActor.Show();
			}
			if ((bool)m_heroPower)
			{
				m_heroPower.GetComponent<Collider>().enabled = true;
			}
		}
	}

	protected void ShowMythicHeroPower(string heroPowerId)
	{
		if (m_mythicHeroDeckData.TryGetValue(heroPowerId, out var mythicData))
		{
			if (m_heroPowerShadowQuad != null)
			{
				m_heroPowerShadowQuad.SetActive(value: true);
			}
			if ((bool)m_heroPowerActor)
			{
				m_heroPowerActor.Hide();
			}
			if ((bool)m_goldenHeroPowerActor)
			{
				m_goldenHeroPowerActor.Hide();
			}
			if ((bool)m_mythicHeroPowerActor)
			{
				m_mythicHeroPowerActor.Hide();
			}
			m_mythicHeroPowerActor = mythicData.heroPowerActor;
			m_mythicHeroPowerBigCard = mythicData.heroPowerBigCardActor;
			if ((bool)m_mythicHeroPowerActor)
			{
				m_mythicHeroPowerActor.Show();
			}
		}
	}

	protected void ShowPreconHero(bool show)
	{
		if (show && SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE && AdventureConfig.Get().CurrentSubScene == AdventureData.Adventuresubscene.PRACTICE && PracticePickerTrayDisplay.Get() != null && PracticePickerTrayDisplay.Get().IsShown())
		{
			return;
		}
		if (show)
		{
			ShowHero();
			return;
		}
		if ((bool)m_heroActor)
		{
			m_heroActor.Hide();
		}
		if ((bool)m_heroPowerActor)
		{
			m_heroPowerActor.Hide();
		}
		if ((bool)m_goldenHeroPowerActor)
		{
			m_goldenHeroPowerActor.Hide();
		}
		if ((bool)m_mythicHeroPowerActor)
		{
			m_mythicHeroPowerActor.Hide();
		}
		if ((bool)m_heroPower)
		{
			m_heroPower.GetComponent<Collider>().enabled = false;
		}
		if ((bool)m_goldenHeroPower)
		{
			m_goldenHeroPower.GetComponent<Collider>().enabled = false;
		}
		if ((bool)m_mythicHeroPower)
		{
			m_mythicHeroPower.GetComponent<Collider>().enabled = false;
		}
		m_heroName.Text = "";
	}

	protected void HideHeroPowerActor()
	{
		m_heroPowerShadowQuad.SetActive(value: false);
		if (m_heroPowerActor != null)
		{
			m_heroPowerActor.Hide();
		}
		if (m_goldenHeroPower != null)
		{
			m_goldenHeroPowerActor.Hide();
		}
		if (m_mythicHeroPowerActor != null)
		{
			m_mythicHeroPowerActor.Hide();
		}
	}

	protected void SetUpHeroPickerButton(HeroPickerButton button, int heroCount)
	{
		GameObject go = button.gameObject;
		Transform parent = go.transform.parent;
		go.name = $"{go.name}_{heroCount}";
		parent.transform.SetParent(m_heroBones[heroCount], worldPositionStays: false);
		parent.transform.localScale = Vector3.one;
		parent.transform.localPosition = Vector3.zero;
		parent.SetParent(m_basicDeckPageContainer.transform, worldPositionStays: true);
	}

	protected void AddHeroLockedTooltip(string name, string description, TAG_CLASS lockedClass = TAG_CLASS.INVALID)
	{
		RemoveHeroLockedTooltip();
		GameObject tipObject = Object.Instantiate(m_tooltipPrefab);
		if (!(tipObject == null))
		{
			LayerUtils.SetLayer(tipObject, UniversalInputManager.UsePhoneUI ? GameLayer.IgnoreFullScreenEffects : GameLayer.Default);
			m_heroLockedTooltip = tipObject.GetComponent<LockedHeroTooltipPanel>();
			if (m_heroLockedTooltip != null)
			{
				m_heroLockedTooltip.Reset();
				m_heroLockedTooltip.Initialize(name, description);
				m_heroLockedTooltip.SetLockedClass(lockedClass);
				GameUtils.SetParent(m_heroLockedTooltip, m_tooltipBone);
			}
			else
			{
				Object.DestroyImmediate(tipObject);
			}
		}
	}

	protected void RemoveHeroLockedTooltip()
	{
		if (m_heroLockedTooltip != null)
		{
			Object.DestroyImmediate(m_heroLockedTooltip.gameObject);
		}
	}

	protected void DeselectLastSelectedHero()
	{
		if (!(m_selectedHeroButton == null))
		{
			m_selectedHeroButton.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
			m_selectedHeroButton.SetSelected(isSelected: false);
		}
	}

	protected void FireDeckTrayLoadedEvent()
	{
		DeckTrayLoaded[] array = m_DeckTrayLoadedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}

	protected void FireFormatTypePickerClosedEvent()
	{
		FormatTypePickerClosed[] array = m_FormatTypePickerClosedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}

	protected bool IsChoosingHeroForTavernBrawlChallenge()
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.FRIENDLY)
		{
			return FriendChallengeMgr.Get().IsChallengeTavernBrawl();
		}
		return false;
	}

	protected bool IsChoosingHeroForDungeonCrawlAdventure()
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.ADVENTURE)
		{
			return GameUtils.DoesAdventureModeUseDungeonCrawlFormat(AdventureConfig.Get().GetSelectedMode());
		}
		return false;
	}

	protected bool IsChoosingHeroForHeroicDeck()
	{
		if (!RankMgr.IsCurrentTwistSeasonUsingHeroicDecks())
		{
			return false;
		}
		VisualsFormatType currentVisualsFormatType = VisualsFormatTypeExtensions.GetCurrentVisualsFormatType();
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT)
		{
			return currentVisualsFormatType == VisualsFormatType.VFT_TWIST;
		}
		return false;
	}

	protected bool OnPlayButtonPressed_SaveHeroAndAdvanceToDungeonRunIfNecessary()
	{
		AdventureConfig ac = AdventureConfig.Get();
		AdventureDataDbfRecord adventureData = ac.GetSelectedAdventureDataRecord();
		if (GameUtils.DoesAdventureModeUseDungeonCrawlFormat(AdventureConfig.Get().GetSelectedMode()) && adventureData.DungeonCrawlPickHeroFirst)
		{
			ac.SelectedHeroCardDbId = m_selectedHeroButton.HeroCardDbId;
			ac.ChangeSubScene(AdventureData.Adventuresubscene.DUNGEON_CRAWL);
			return true;
		}
		return false;
	}

	protected void SetBackButtonEnabled(bool enable)
	{
		if (DemoMgr.Get().IsExpoDemo())
		{
			if (enable)
			{
				return;
			}
			enable = false;
		}
		if (m_backButton != null && m_backButton.IsEnabled() != enable)
		{
			m_backButton.SetEnabled(enable);
			m_backButton.Flip(enable);
		}
	}

	protected void SetHeroPowerActorColliderEnabled(bool enable = true)
	{
		if (m_heroPowerActor != null)
		{
			m_heroPowerActor.GetComponent<Collider>().enabled = enable;
		}
		if (m_goldenHeroPower != null)
		{
			m_goldenHeroPowerActor.GetComponent<Collider>().enabled = enable;
		}
		if (m_mythicHeroPower != null)
		{
			m_mythicHeroPower.GetComponent<Collider>().enabled = enable;
		}
	}

	protected void SetUpHeroCrowns()
	{
		AdventureDataDbfRecord adventureDataRecord = GameUtils.GetAdventureDataRecord((int)AdventureConfig.Get().GetSelectedAdventure(), (int)AdventureConfig.Get().GetSelectedMode());
		GameSaveKeyId adventureServerKey = (GameSaveKeyId)adventureDataRecord.GameSaveDataServerKey;
		if (!GameSaveDataManager.Get().ValidateIfKeyCanBeAccessed(adventureServerKey, adventureDataRecord.Name))
		{
			return;
		}
		if (adventureDataRecord != null && adventureDataRecord.DungeonCrawlDisplayHeroWinsPerChapter)
		{
			WingDbfRecord wingRecord = GameUtils.GetWingRecordFromMissionId((int)AdventureConfig.Get().GetMission());
			GameSaveDataManager.AdventureDungeonCrawlWingProgressSubkeys wingSubkeys;
			List<long> heroCardWins;
			if (wingRecord == null)
			{
				Log.Adventures.PrintError("SetUpHeroCrowns() - No WingRecord found for mission {0}, so cannot set up hero crowns.", AdventureConfig.Get().GetMission());
			}
			else if (!GameSaveDataManager.GetProgressSubkeysForDungeonCrawlWing(wingRecord, out wingSubkeys))
			{
				Log.Adventures.PrintError("GetProgressSubkeysForDungeonCrawlWing could not find progress subkeys for Wing {0}, so we don't know which Heroes to show crowns over.", wingRecord.ID);
			}
			else if (GameSaveDataManager.Get().GetSubkeyValue(adventureServerKey, wingSubkeys.heroCardWins, out heroCardWins) && heroCardWins != null)
			{
				ActivateCrownsForHeroCardDbIds(heroCardWins);
			}
			return;
		}
		long runsComplete = 0L;
		List<TAG_CLASS> crownClasses = new List<TAG_CLASS>();
		List<long> heroCardIds = new List<long>();
		foreach (TAG_CLASS deckClass in GameSaveDataManager.GetClassesFromDungeonCrawlProgressMap())
		{
			if (GameSaveDataManager.GetProgressSubkeyForDungeonCrawlClass(deckClass, out var classSubkeys) && GameSaveDataManager.Get().GetSubkeyValue(adventureServerKey, classSubkeys.runWins, out runsComplete) && runsComplete > 0)
			{
				crownClasses.Add(deckClass);
			}
		}
		ActivateCrownsForClasses(crownClasses);
		foreach (long heroCardId in GameSaveDataManager.GetHeroCardIdFromDungeonCrawlProgressMap())
		{
			if (GameSaveDataManager.Get().GetSubkeyValue(adventureServerKey, GameSaveDataManager.AdventureDungeonCrawlHeroToSubkeyMapping[heroCardId], out runsComplete))
			{
				if (runsComplete > 0)
				{
					heroCardIds.Add(heroCardId);
				}
			}
			else
			{
				DeactivateCrownForHeroCardDbId(heroCardId);
			}
		}
		ActivateCrownsForHeroCardDbIds(heroCardIds);
	}

	protected List<Transform> ActivateCrownsForClasses(List<TAG_CLASS> classes)
	{
		List<Transform> crownBones = new List<Transform>();
		foreach (HeroPickerButton heroButton in m_heroButtons)
		{
			if (classes.Contains(heroButton.m_heroClass))
			{
				heroButton.m_crown.SetActive(value: true);
			}
		}
		return crownBones;
	}

	protected void ActivateCrownsForHeroCardDbIds(List<long> cardDbIds)
	{
		foreach (HeroPickerButton heroButton in m_heroButtons)
		{
			EntityDef entityDef = heroButton.GetEntityDef();
			if (entityDef != null)
			{
				int heroDbId = GameUtils.TranslateCardIdToDbId(entityDef.GetCardId());
				if (cardDbIds.Contains(heroDbId))
				{
					heroButton.m_crown.SetActive(value: true);
				}
			}
		}
	}

	protected void DeactivateCrownForHeroCardDbId(long heroDbId)
	{
		foreach (HeroPickerButton heroButton in m_heroButtons)
		{
			EntityDef entityDef = heroButton.GetEntityDef();
			if (entityDef != null)
			{
				int dbId = GameUtils.TranslateCardIdToDbId(entityDef.GetCardId());
				if (heroDbId == dbId)
				{
					heroButton.m_crown.SetActive(value: false);
				}
			}
		}
	}

	public void Unload()
	{
		DeckPickerTray.Get().UnregisterHandlers();
		if (FriendChallengeMgr.Get() != null)
		{
			FriendChallengeMgr.Get().RemoveChangedListener(OnFriendChallengeChanged);
		}
	}

	public bool IsLoaded()
	{
		return m_Loaded;
	}

	public void AddDeckTrayLoadedListener(DeckTrayLoaded dlg)
	{
		m_DeckTrayLoadedListeners.Add(dlg);
	}

	public void RemoveDeckTrayLoadedListener(DeckTrayLoaded dlg)
	{
		m_DeckTrayLoadedListeners.Remove(dlg);
	}

	public void AddFormatTypePickerClosedListener(FormatTypePickerClosed dlg)
	{
		m_FormatTypePickerClosedListeners.Add(dlg);
	}

	public void RemoveFormatTypePickerClosedListener(FormatTypePickerClosed dlg)
	{
		m_FormatTypePickerClosedListeners.Remove(dlg);
	}

	public void SetPlayButtonText(string text)
	{
		if (m_playButton != null)
		{
			m_playButton.SetText(text);
		}
	}

	public void SetPlayButtonTextAlpha(float alpha)
	{
		if (m_playButton != null)
		{
			m_playButton.m_newPlayButtonText.TextAlpha = alpha;
		}
	}

	public void AddPlayButtonListener(UIEventType type, UIEvent.Handler handler)
	{
		if (m_playButton != null)
		{
			m_playButton.AddEventListener(type, handler);
		}
	}

	public void RemovePlayButtonListener(UIEventType type, UIEvent.Handler handler)
	{
		if (m_playButton != null)
		{
			m_playButton.RemoveEventListener(type, handler);
		}
	}

	public void SetHeaderText(string text)
	{
		if (m_modeName != null)
		{
			m_modeName.Text = text;
		}
	}

	public HeroPickerDataModel GetHeroPickerDataModel()
	{
		VisualController visualController = GetComponent<VisualController>();
		if (visualController == null)
		{
			return null;
		}
		Widget owner = visualController.Owner;
		if (!owner.GetDataModel(13, out var dataModel))
		{
			dataModel = new HeroPickerDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as HeroPickerDataModel;
	}

	public void CheatLoadHeroButtons(int buttonsToDisplay)
	{
		StartCoroutine(LoadHeroButtons(buttonsToDisplay));
	}
}
