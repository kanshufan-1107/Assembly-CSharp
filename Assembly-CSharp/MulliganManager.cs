using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Time;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using PegasusGame;
using UnityEngine;

[CustomEditClass(DefaultCollapsed = true)]
public class MulliganManager : MonoBehaviour
{
	[Serializable]
	public class TagConditionalVFX
	{
		[CustomEditField(SortPopupByName = true, Label = "Required Game Tag")]
		public GAME_TAG m_requiredTag;

		[CustomEditField(Label = "VFX Prefab")]
		public GameObject m_VFXPrefab;

		[CustomEditField(Label = "Banner Replacement Prefab")]
		public GameObject m_bannerReplacementPrefab;

		[CustomEditField(Label = "Banner Replacement Prefab Priority (Larger value wins)")]
		public int m_bannerReplacementPrefabPriority;
	}

	public AnimationClip cardAnimatesFromBoardToDeck;

	public AnimationClip cardAnimatesFromBoardToDeck_iPhone;

	public AnimationClip cardAnimatesFromTableToSky;

	public AnimationClip cardAnimatesFromDeckToBoard;

	public AnimationClip shuffleDeck;

	public AnimationClip myheroAnimatesToPosition;

	public AnimationClip hisheroAnimatesToPosition;

	public AnimationClip myheroAnimatesToPosition_iPhone;

	public AnimationClip hisheroAnimatesToPosition_iPhone;

	public GameObject coinPrefab;

	public GameObject weldPrefab;

	public GameObject mulliganChooseBannerPrefab;

	public GameObject mulliganDetailLabelPrefab;

	public GameObject mulliganKeepLabelPrefab;

	public MulliganReplaceLabel mulliganReplaceLabelPrefab;

	public GameObject mulliganXlabelPrefab;

	public GameObject mulliganTimerPrefab;

	public GameObject heroLabelPrefab;

	public MulliganButton mulliganButtonWidget;

	public MulliganButton mulliganCancelConfirmationButtonWidget;

	public UberText conditionalHelperTextLabel;

	public bool mulliganRefreshButtonEnabled;

	public float heroRerollButtonFTUEDuration = 10f;

	[CustomEditField(SearchField = "m_requiredTag", Label = "Tag Conditional VFX Prefabs")]
	public List<TagConditionalVFX> tagConditionalVFXPrefabs = new List<TagConditionalVFX>();

	private const float PHONE_HEIGHT_OFFSET = 7f;

	private const float PHONE_CARD_Z_OFFSET = 0.2f;

	private const float PHONE_CARD_SCALE = 0.9f;

	private const float PHONE_ZONE_SIZE_ADJUST = 0.55f;

	private const string MULLIGAN_BUTTON_PREFAB = "MulliganButton.prefab:f58c065fc711b604c891cefd1faf722a";

	private const float REFRESH_BUTTON_X_OFFSET = 2f;

	private const string DEFAULT_MULLIGAN_CONFIM_TEXT_KEY = "GLOBAL_CONFIRM";

	private const string MULLIGAN_CONFIM_WAITING_TEXT_KEY = "GAMEPLAY_MULLIGAN_WAITING_CONFIRMATION";

	private const float MULLIGAN_CONFIRM_CANCEL_BUTTON_OFFSET = 2f;

	private const string REROLL_BUTTON_FTUE_TOOLTIP_BONE = "FTUEBone";

	private const string REROLL_TOKEN_FTUE_TOOLTIP_BONE = "RerollTokenFTUETooltipBone";

	private readonly PlatformDependentValue<string> PLATFORM_DEPENDENT_BONE_SUFFIX = new PlatformDependentValue<string>(PlatformCategory.Screen)
	{
		PC = "PC",
		Tablet = "PC",
		Phone = "Phone"
	};

	private readonly PlatformDependentValue<float> PLATFORM_DEPENDENT_Z_OFFSET = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 0f,
		Tablet = 0f,
		Phone = -0.4f
	};

	private readonly PlatformDependentValue<float> PLATFORM_DEPENDENT_FTUE_ADJUSTMENT_SLOPE = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 0.202f,
		Tablet = 0.202f,
		Phone = 0.10516f
	};

	private readonly PlatformDependentValue<float> PLATFORM_DEPENDENT_FTUE_ADJUSTMENT_OFFSET = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 0.306f,
		Tablet = 0.306f,
		Phone = 0.115717f
	};

	public const float BATTLEGROUNDS_HERO_ENDING_POSITION_X = -7.7726f;

	public const float BATTLEGROUNDS_HERO_ENDING_POSITION_Y = 0.0055918f;

	public const float BATTLEGROUNDS_HERO_ENDING_POSITION_Z = -8.054f;

	public const float BATTLEGROUNDS_HERO_ENDING_SCALE = 1.134f;

	public static readonly PlatformDependentValue<Vector3> FRIENDLY_PLAYER_CARD_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(1.1f, 0.28f, 1.1f),
		Phone = new Vector3(0.9f, 0.28f, 0.9f)
	};

	private static MulliganManager s_instance;

	private bool mulliganActive;

	private MulliganTimer m_mulliganTimer;

	private NormalButton mulliganButton;

	private NormalButton m_refreshButton;

	private GameObject myWeldEffect;

	private GameObject hisWeldEffect;

	private GameObject coinObject;

	private GameObject startingHandZone;

	private GameObject coinTossText;

	private ZoneHand friendlySideHandZone;

	private ZoneHand opposingSideHandZone;

	private ZoneDeck friendlySideDeck;

	private ZoneDeck opposingSideDeck;

	private Actor myHeroCardActor;

	private Actor hisHeroCardActor;

	private Actor myHeroPowerCardActor;

	private Actor hisHeroPowerCardActor;

	private Map<Card, Actor> opponentHeroActors = new Map<Card, Actor>();

	private Map<Card, Actor> choiceHeroActors = new Map<Card, Actor>();

	private Map<Card, Actor> teammateHeroActors = new Map<Card, Actor>();

	private List<Actor> fakeCardsOnLeft = new List<Actor>();

	private List<Actor> fakeCardsOnRight = new List<Actor>();

	private Card m_chosenHero;

	private bool waitingForVersusText;

	private GameStartVsLetters versusText;

	private bool waitingForVersusVo;

	private AudioSource versusVo;

	private bool introComplete;

	private bool skipCardChoosing;

	private List<Card> m_startingCards;

	private List<Card> m_startingOppCards;

	private int m_coinCardIndex = -1;

	private int m_bonusCardIndex = -1;

	private GameObject mulliganChooseBanner;

	private GameObject mulliganDetailLabel;

	private GameObject anomalyIcon;

	private List<MulliganReplaceLabel> m_replaceLabels;

	private GameObject[] m_xLabels;

	private List<GameObject> m_tagConditionalVFXs;

	private GameObject m_overrideMulliganChooseBannerPrefab;

	private Notification m_RerollButtonFTUENotification;

	private Notification m_RerollTokenFTUENotification;

	private float m_lastBnetBarX;

	private bool[] m_handCardsMarkedForReplace = new bool[4];

	private bool[] m_handCardsMarkedForTentativeConfirm = new bool[4];

	private Vector3 coinLocation;

	private bool friendlyPlayerGoesFirst;

	private HeroLabel myheroLabel;

	private HeroLabel hisheroLabel;

	private Spell m_MyCustomSocketInSpell;

	private Spell m_HisCustomSocketInSpell;

	private bool m_isLoadingMyCustomSocketIn;

	private bool m_isLoadingHisCustomSocketIn;

	private int pendingHeroCount;

	private int pendingFakeHeroCount;

	private int tentativeConfirmedChooseOneEntityId;

	private int tentativeFallbackChooseOneEntityId;

	public static readonly float ANIMATION_TIME_DEAL_CARD = 1.5f;

	private bool friendlyPlayerHasReplacementCards;

	private bool opponentPlayerHasReplacementCards;

	private Actor m_teammateActor;

	private bool m_waitingForUserInput;

	private Notification innkeeperMulliganDialog;

	private bool m_resuming;

	private Coroutine m_customIntroCoroutine;

	private IEnumerator m_DimLightsOnceBoardLoads;

	private IEnumerator m_WaitForBoardThenLoadButton;

	private IEnumerator m_WaitForHeroesAndStartAnimations;

	private IEnumerator m_ResumeMulligan;

	private IEnumerator m_DealStartingCards;

	private IEnumerator m_ShowMultiplayerWaitingArea;

	private IEnumerator m_RemoveOldCardsAnimation;

	private IEnumerator m_PlayStartingTaunts;

	private IEnumerator m_Spectator_WaitForFriendlyPlayerThenProcessEntitiesChosen;

	private IEnumerator m_ContinueMulliganWhenBoardLoads;

	private IEnumerator m_WaitAFrameBeforeSendingEventToMulliganButton;

	private IEnumerator m_WaitAFrameBeforeSendingEventToMulliganRefreshButton;

	private IEnumerator m_ShrinkStartingHandBanner;

	private IEnumerator m_AnimateCoinTossText;

	private IEnumerator m_UpdateChooseBanner;

	private IEnumerator m_RemoveUIButtons;

	private IEnumerator m_WaitForOpponentToFinishMulligan;

	private IEnumerator m_EndMulliganWithTiming;

	private IEnumerator m_HandleCoinCard;

	private IEnumerator m_EnableHandCollidersAfterCardsAreDealt;

	private IEnumerator m_SkipMulliganForResume;

	private IEnumerator m_SkipMulliganWhenIntroComplete;

	private IEnumerator m_WaitForBoardAnimToCompleteThenStartTurn;

	private void Awake()
	{
		s_instance = this;
	}

	private void OnDestroy()
	{
		if (GameState.Get() != null)
		{
			GameState.Get().UnregisterCreateGameListener(OnCreateGame);
			GameState.Get().UnregisterMulliganTimerUpdateListener(OnMulliganTimerUpdate);
			GameState.Get().UnregisterEntitiesChosenReceivedListener(OnEntitiesChosenReceived);
			GameState.Get().UnregisterGameOverListener(OnGameOver);
		}
		if (NetCache.Get() != null)
		{
			NetCache.Get().RemoveBattlegroundsTokenBalanceListener(OnBattlegroundsTokenBalanceUpdate);
		}
		s_instance = null;
	}

	private void Start()
	{
		if (GameState.Get() == null)
		{
			Debug.LogError($"MulliganManager.Start() - GameState already Shutdown before MulliganManager was loaded.");
			return;
		}
		if (GameState.Get().IsGameCreatedOrCreating())
		{
			HandleGameStart();
		}
		else
		{
			GameState.Get().RegisterCreateGameListener(OnCreateGame);
		}
		GameState.Get().RegisterMulliganTimerUpdateListener(OnMulliganTimerUpdate);
		GameState.Get().RegisterEntitiesChosenReceivedListener(OnEntitiesChosenReceived);
		GameState.Get().RegisterGameOverListener(OnGameOver);
		NetCache.Get().RegisterBattlegroundsTokenBalanceListener(OnBattlegroundsTokenBalanceUpdate);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			myheroAnimatesToPosition = myheroAnimatesToPosition_iPhone;
			hisheroAnimatesToPosition = hisheroAnimatesToPosition_iPhone;
			cardAnimatesFromBoardToDeck = cardAnimatesFromBoardToDeck_iPhone;
		}
	}

	public static MulliganManager Get()
	{
		return s_instance;
	}

	private void OnBattlegroundsTokenBalanceUpdate(NetCache.NetCacheBattlegroundsTokenBalance bgTokenBalance)
	{
		SpectatorManager spectatorManager = SpectatorManager.Get();
		if ((spectatorManager != null && spectatorManager.IsSpectatingOpposingSide()) || GameState.Get() == null || m_startingCards == null)
		{
			return;
		}
		foreach (Card card in m_startingCards)
		{
			if (card == null)
			{
				continue;
			}
			Actor actor = card.GetActor();
			if (!(actor == null))
			{
				PlayerLeaderboardMainCardActor baconHeroActor = actor.GetComponent<PlayerLeaderboardMainCardActor>();
				if (baconHeroActor != null)
				{
					baconHeroActor.UpdateMulliganRerollButton(null);
				}
			}
		}
	}

	public bool IsCustomIntroActive()
	{
		return m_customIntroCoroutine != null;
	}

	public bool IsMulliganActive()
	{
		return mulliganActive;
	}

	public bool IsMulliganIntroActive()
	{
		return !introComplete;
	}

	public void OnRealtimeAvailableRaces()
	{
		if (GameState.Get().IsInChoiceMode())
		{
			ShowMulliganDetail();
		}
	}

	private void EnableDamageCapFX(bool enable)
	{
		PlayerLeaderboardManager manager = PlayerLeaderboardManager.Get();
		if (manager != null)
		{
			manager.EnableDamageCapFX(enable);
		}
	}

	public void ForceMulliganActive(bool active)
	{
		mulliganActive = active;
		if (mulliganActive)
		{
			GameState.Get().HideZzzEffects();
			if (!skipCardChoosing)
			{
				EnableDamageCapFX(enable: false);
			}
		}
		else
		{
			GameState.Get().UnhideZzzEffects();
			EnableDamageCapFX(enable: true);
			DisableBaconMulliganElements();
			HideRerollPopups();
		}
		BnetBar.Get().RefreshCurrency();
	}

	public void LoadMulliganButton()
	{
		if (m_WaitForBoardThenLoadButton != null)
		{
			StopCoroutine(m_WaitForBoardThenLoadButton);
		}
		m_WaitForBoardThenLoadButton = WaitForBoardThenLoadButton();
		StartCoroutine(m_WaitForBoardThenLoadButton);
	}

	private IEnumerator WaitForBoardThenLoadButton()
	{
		while (Gameplay.Get().GetBoardLayout() == null)
		{
			yield return null;
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_REQUIRES_CONFIRMATION))
		{
			AssetLoader.Get().InstantiatePrefab("MulliganButton.prefab:f58c065fc711b604c891cefd1faf722a", OnMulliganButtonLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
		{
			AssetLoader.Get().InstantiatePrefab("MulliganButton.prefab:f58c065fc711b604c891cefd1faf722a", OnMulliganRefreshButtonLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
		if (conditionalHelperTextLabel != null)
		{
			conditionalHelperTextLabel.transform.position = Board.Get().FindBone("MulliganHelperTextPosition").position;
		}
	}

	private void OnMulliganButtonLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogError($"MulliganManager.OnMulliganButtonLoaded() - FAILED to load \"{assetRef}\"");
			return;
		}
		mulliganButton = go.GetComponent<NormalButton>();
		if (mulliganButton == null)
		{
			Debug.LogError($"MulliganManager.OnMulliganButtonLoaded() - ERROR \"{assetRef}\" has no {typeof(NormalButton)} component");
			return;
		}
		mulliganButton.SetText(GameStrings.Get("GLOBAL_CONFIRM"));
		mulliganButtonWidget.SetText(GameStrings.Get("GLOBAL_CONFIRM"));
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
		{
			mulliganButton.SetEnabled(enabled: false);
			mulliganButton.gameObject.SetActive(value: false);
			mulliganButtonWidget.SetEnabled(active: false);
		}
		else
		{
			mulliganButtonWidget.gameObject.SetActive(value: false);
		}
	}

	private void OnMulliganRefreshButtonLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogError($"MulliganManager.OnMulliganRefreshButtonLoaded() - FAILED to load \"{assetRef}\"");
			return;
		}
		m_refreshButton = go.GetComponent<NormalButton>();
		if (m_refreshButton == null)
		{
			Debug.LogError($"MulliganManager.OnMulliganRefreshButtonLoaded() - ERROR \"{assetRef}\" has no {typeof(NormalButton)} component");
			return;
		}
		m_refreshButton.SetText(GameStrings.Get("GLOBAL_REFRESH"));
		UpdateBGRefreshButton();
	}

	public void OnFriendlyPlayerNumberRefreshAvailableChanged(int newValue)
	{
		UpdateBGRefreshButton(newValue);
	}

	private void UpdateBGRefreshButton(int numRefreshesAvail = -1)
	{
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE) || !mulliganRefreshButtonEnabled)
		{
			m_refreshButton.gameObject.SetActive(value: false);
			return;
		}
		if (numRefreshesAvail == -1)
		{
			Player solitairePlayer = GameState.Get()?.GetFriendlySidePlayer();
			if (solitairePlayer != null)
			{
				numRefreshesAvail = solitairePlayer.GetTag(GAME_TAG.BACON_NUMBER_HERO_REFRESH_AVAILABLE);
			}
		}
		m_refreshButton.gameObject.SetActive(numRefreshesAvail > 0);
		if (numRefreshesAvail > 0)
		{
			m_WaitAFrameBeforeSendingEventToMulliganRefreshButton = WaitAFrameBeforeSendingEventToMulliganButton(m_refreshButton);
			StartCoroutine(m_WaitAFrameBeforeSendingEventToMulliganRefreshButton);
		}
	}

	private void DisableBaconMulliganElements()
	{
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
		{
			return;
		}
		HideRerollPopups();
		if (m_startingCards == null)
		{
			return;
		}
		foreach (Card card in m_startingCards)
		{
			if (card == null)
			{
				continue;
			}
			Actor actor = card.GetActor();
			if (!(actor == null))
			{
				PlayerLeaderboardMainCardActor baconHeroActor = actor.GetComponent<PlayerLeaderboardMainCardActor>();
				if (baconHeroActor != null)
				{
					baconHeroActor.DisableMulliganOnlyElements();
				}
			}
		}
	}

	private void OnVersusVoLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		waitingForVersusVo = false;
		if (go == null)
		{
			Debug.LogError($"MulliganManager.OnVersusVoLoaded() - FAILED to load \"{assetRef}\"");
			return;
		}
		versusVo = go.GetComponent<AudioSource>();
		if (versusVo == null)
		{
			Debug.LogError($"MulliganManager.OnVersusVoLoaded() - ERROR \"{assetRef}\" has no {typeof(AudioSource)} component");
		}
	}

	private void OnVersusTextLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		waitingForVersusText = false;
		if (go == null)
		{
			Debug.LogError($"MulliganManager.OnVersusTextLoadAttempted() - FAILED to load \"{assetRef}\"");
			return;
		}
		versusText = go.GetComponent<GameStartVsLetters>();
		if (versusText == null)
		{
			Log.All.PrintError("MulliganManager.OnVersusTextLoadAttempted() object loaded does not have a GameStartVsLetters component");
		}
	}

	private IEnumerator WaitForHeroesAndStartAnimations()
	{
		Log.LoadingScreen.Print("MulliganManager.WaitForHeroesAndStartAnimations()");
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.SKIP_HERO_LOAD))
		{
			while (gameEntity.IsPreloadingAssets())
			{
				yield return null;
			}
			gameEntity.NotifyOfMulliganInitialized();
			GameState.Get().GetGameEntity().DoAlternateMulliganIntro();
			introComplete = true;
			yield break;
		}
		Player friendlyPlayer;
		for (friendlyPlayer = GameState.Get().GetFriendlySidePlayer(); friendlyPlayer == null; friendlyPlayer = GameState.Get().GetFriendlySidePlayer())
		{
			yield return null;
		}
		Player opposingPlayer;
		for (opposingPlayer = GameState.Get().GetOpposingSidePlayer(); opposingPlayer == null; opposingPlayer = GameState.Get().GetOpposingSidePlayer())
		{
			yield return null;
		}
		Card myHeroCard = null;
		while (myHeroCardActor == null)
		{
			myHeroCard = friendlyPlayer.GetHeroCard();
			if (myHeroCard != null)
			{
				myHeroCardActor = myHeroCard.GetActor();
			}
			yield return null;
		}
		Card hisHeroCard = null;
		while (hisHeroCardActor == null)
		{
			hisHeroCard = opposingPlayer.GetHeroCard();
			if (hisHeroCard != null)
			{
				hisHeroCardActor = hisHeroCard.GetActor();
			}
			yield return null;
		}
		while (friendlyPlayer.GetHeroPower() != null && myHeroPowerCardActor == null)
		{
			Card heroPowerCard = friendlyPlayer.GetHeroPowerCard();
			if (heroPowerCard != null)
			{
				myHeroPowerCardActor = heroPowerCard.GetActor();
				if (myHeroPowerCardActor != null)
				{
					myHeroPowerCardActor.TurnOffCollider();
				}
			}
			yield return null;
		}
		while (opposingPlayer.GetHeroPower() != null && hisHeroPowerCardActor == null)
		{
			Card heroPowerCard2 = opposingPlayer.GetHeroPowerCard();
			if (heroPowerCard2 != null)
			{
				hisHeroPowerCardActor = heroPowerCard2.GetActor();
				if (hisHeroPowerCardActor != null)
				{
					hisHeroPowerCardActor.TurnOffCollider();
				}
			}
			yield return null;
		}
		while (GameState.Get() == null || GameState.Get().GetGameEntity().IsPreloadingAssets())
		{
			yield return null;
		}
		while (!myHeroCardActor.HasCardDef)
		{
			yield return null;
		}
		while (!hisHeroCardActor.HasCardDef)
		{
			yield return null;
		}
		LoadMyHeroSkinSocketInEffect(myHeroCardActor);
		LoadHisHeroSkinSocketInEffect(hisHeroCardActor);
		LoadStartOfGameHeroTrayEffects();
		while (m_isLoadingMyCustomSocketIn || m_isLoadingHisCustomSocketIn)
		{
			yield return null;
		}
		List<Material> myHeroActorMaterials = myHeroCardActor.m_portraitMesh.GetComponent<Renderer>().GetMaterials();
		Material myHeroMat = myHeroActorMaterials[myHeroCardActor.m_portraitMatIdx];
		CustomHeroFrameBehaviour customFrame = myHeroCardActor.gameObject.GetComponent<CustomHeroFrameBehaviour>();
		Material myHeroFrameMat;
		if (customFrame != null)
		{
			List<Material> frameMaterials = customFrame.GetMeshObject().GetComponentInChildren<Renderer>().GetMaterials();
			myHeroFrameMat = ((frameMaterials.Count > 0) ? frameMaterials[0] : null);
		}
		else
		{
			myHeroFrameMat = myHeroActorMaterials[myHeroCardActor.m_portraitFrameMatIdx];
		}
		if (myHeroMat != null && myHeroMat.HasProperty("_LightingBlend"))
		{
			myHeroMat.SetFloat("_LightingBlend", 0f);
		}
		if (myHeroFrameMat != null && myHeroFrameMat.HasProperty("_LightingBlend"))
		{
			myHeroFrameMat.SetFloat("_LightingBlend", 0f);
		}
		myHeroCardActor.UpdateCustomFrameLightingBlend(0f);
		float hisLightBlend = (GameState.Get().GetBooleanGameOption(GameEntityOption.DIM_OPPOSING_HERO_DURING_MULLIGAN) ? 1f : 0f);
		List<Material> hisHeroActorMaterials = hisHeroCardActor.m_portraitMesh.GetComponent<Renderer>().GetMaterials();
		Material hisHeroMat = hisHeroActorMaterials[hisHeroCardActor.m_portraitMatIdx];
		Material hisHeroFrameMat = hisHeroActorMaterials[hisHeroCardActor.m_portraitFrameMatIdx];
		if (hisHeroMat != null && hisHeroMat.HasProperty("_LightingBlend"))
		{
			hisHeroMat.SetFloat("_LightingBlend", hisLightBlend);
		}
		if (hisHeroFrameMat != null && hisHeroFrameMat.HasProperty("_LightingBlend"))
		{
			hisHeroFrameMat.SetFloat("_LightingBlend", hisLightBlend);
		}
		hisHeroCardActor.UpdateCustomFrameLightingBlend(hisLightBlend);
		if (myHeroPowerCardActor != null && myHeroPowerCardActor.m_portraitMesh != null)
		{
			List<Material> materials = myHeroPowerCardActor.m_portraitMesh.GetComponent<Renderer>().GetMaterials();
			Material myPowerMat = materials[myHeroPowerCardActor.m_portraitMatIdx];
			if (myPowerMat != null && myPowerMat.HasProperty("_LightingBlend"))
			{
				myPowerMat.SetFloat("_LightingBlend", 1f);
			}
			Material myPowerMatFrame = materials[myHeroPowerCardActor.m_portraitFrameMatIdx];
			if (myPowerMatFrame != null && myPowerMatFrame.HasProperty("_LightingBlend"))
			{
				myPowerMatFrame.SetFloat("_LightingBlend", 1f);
			}
		}
		if (hisHeroPowerCardActor != null && hisHeroPowerCardActor.m_portraitMesh != null)
		{
			List<Material> materials2 = hisHeroPowerCardActor.m_portraitMesh.GetComponent<Renderer>().GetMaterials();
			Material hisPowerMat = materials2[hisHeroPowerCardActor.m_portraitMatIdx];
			if (hisPowerMat != null && hisPowerMat.HasProperty("_LightingBlend"))
			{
				hisPowerMat.SetFloat("_LightingBlend", 1f);
			}
			Material hisPowerMatFrame = materials2[hisHeroPowerCardActor.m_portraitFrameMatIdx];
			if (hisPowerMatFrame != null && hisPowerMatFrame.HasProperty("_LightingBlend"))
			{
				hisPowerMatFrame.SetFloat("_LightingBlend", 1f);
			}
		}
		myHeroCardActor.TurnOffCollider();
		hisHeroCardActor.TurnOffCollider();
		myHeroCardActor.UpdateCustomFrameDiamondMaterial();
		hisHeroCardActor.UpdateCustomFrameDiamondMaterial();
		gameEntity.NotifyOfMulliganInitialized();
		if (GameState.Get().GetGameEntity().DoAlternateMulliganIntro())
		{
			if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_HAS_HERO_LOBBY))
			{
				myHeroCardActor.Hide();
			}
			introComplete = true;
			yield break;
		}
		while (waitingForVersusText || waitingForVersusVo)
		{
			yield return null;
		}
		Log.LoadingScreen.Print("MulliganManager.WaitForHeroesAndStartAnimations() - NotifySceneLoaded()");
		SceneMgr.Get().NotifySceneLoaded();
		while (LoadingScreen.Get().IsPreviousSceneActive() || LoadingScreen.Get().IsFadingOut())
		{
			yield return null;
		}
		GameMgr.Get().UpdatePresence();
		GameObject myHero = myHeroCardActor.gameObject;
		GameObject hisHero = hisHeroCardActor.gameObject;
		myHeroCardActor.GetHealthObject().Hide();
		hisHeroCardActor.GetHealthObject().Hide();
		if (myHeroCardActor.GetAttackObject() != null)
		{
			myHeroCardActor.GetAttackObject().Hide();
		}
		if (hisHeroCardActor.GetAttackObject() != null)
		{
			hisHeroCardActor.GetAttackObject().Hide();
		}
		if ((bool)versusText)
		{
			versusText.transform.position = Board.Get().FindBone("VS_Position").position;
		}
		GameObject heroLabelCopy = UnityEngine.Object.Instantiate(heroLabelPrefab);
		myheroLabel = heroLabelCopy.GetComponent<HeroLabel>();
		myheroLabel.transform.parent = myHeroCardActor.GetMeshRenderer().transform;
		myheroLabel.transform.localPosition = new Vector3(0f, 0f, 0f);
		myheroLabel.transform.localPosition = myHeroCardActor.GetHeroLabelOffset(Player.Side.FRIENDLY);
		TAG_CLASS classTag = myHeroCardActor.GetEntity().GetClass();
		string className = "";
		if (classTag != TAG_CLASS.NEUTRAL && gameEntity.ShouldShowHeroClassDuringMulligan(Player.Side.FRIENDLY))
		{
			className = GameStrings.GetClassName(classTag).ToUpper();
		}
		myheroLabel.UpdateText(myHeroCardActor.GetEntity().GetName(), className);
		heroLabelCopy = UnityEngine.Object.Instantiate(heroLabelPrefab);
		hisheroLabel = heroLabelCopy.GetComponent<HeroLabel>();
		hisheroLabel.transform.parent = hisHeroCardActor.GetMeshRenderer().transform;
		hisheroLabel.transform.localPosition = hisHeroCardActor.GetHeroLabelOffset(Player.Side.OPPOSING);
		classTag = hisHeroCardActor.GetEntity().GetClass();
		className = "";
		if (classTag != TAG_CLASS.NEUTRAL && gameEntity.ShouldShowHeroClassDuringMulligan(Player.Side.OPPOSING))
		{
			className = GameStrings.GetClassName(classTag).ToUpper();
		}
		hisheroLabel.UpdateText(hisHeroCardActor.GetEntity().GetName(), className);
		if (GameState.Get().WasConcedeRequested())
		{
			yield break;
		}
		gameEntity.StartMulliganSoundtracks(soft: false);
		Animation cardAnim = myHero.GetComponent<Animation>();
		if (cardAnim == null)
		{
			cardAnim = myHero.AddComponent<Animation>();
		}
		cardAnim.AddClip(hisheroAnimatesToPosition, "hisHeroAnimateToPosition");
		StartCoroutine(SampleAnimFrame(cardAnim, "hisHeroAnimateToPosition", 0f));
		Animation oppCardAnim = hisHero.GetComponent<Animation>();
		if (oppCardAnim == null)
		{
			oppCardAnim = hisHero.AddComponent<Animation>();
		}
		oppCardAnim.AddClip(myheroAnimatesToPosition, "myHeroAnimateToPosition");
		StartCoroutine(SampleAnimFrame(oppCardAnim, "myHeroAnimateToPosition", 0f));
		m_customIntroCoroutine = StartCoroutine(GameState.Get().GetGameEntity().DoCustomIntro(myHeroCard, hisHeroCard, myheroLabel, hisheroLabel, versusText));
		yield return m_customIntroCoroutine;
		m_customIntroCoroutine = null;
		myHero.transform.position += myHeroCardActor.GetHeroOffset(Player.Side.FRIENDLY);
		hisHero.transform.position += hisHeroCardActor.GetHeroOffset(Player.Side.OPPOSING);
		while (LoadingScreen.Get().IsTransitioning())
		{
			yield return null;
		}
		AudioSource myHeroLine = gameEntity.GetAnnouncerLine(myHeroCard, Card.AnnouncerLineType.BEFORE_VERSUS);
		AudioSource hisHeroLine = gameEntity.GetAnnouncerLine(hisHeroCard, Card.AnnouncerLineType.AFTER_VERSUS);
		if ((bool)versusVo && (bool)myHeroLine && (bool)hisHeroLine)
		{
			if (myHeroCard != null)
			{
				myHeroCard.ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.FriendlyAnnounceVO);
			}
			SoundManager.Get().Play(myHeroLine);
			while (SoundManager.Get().IsActive(myHeroLine) && !SoundManager.Get().IsPlaybackFinished(myHeroLine))
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.05f);
			SoundManager.Get().PlayPreloaded(versusVo);
			while (SoundManager.Get().IsActive(versusVo) && !SoundManager.Get().IsPlaybackFinished(versusVo))
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.05f);
			if (hisHeroCard != null)
			{
				hisHeroCard.ActivateLegendaryHeroAnimEvent(LegendaryHeroAnimations.OpponentAnnounceVO);
			}
			if (hisHeroLine != null && hisHeroLine.clip != null)
			{
				SoundManager.Get().Play(hisHeroLine);
				while (SoundManager.Get().IsActive(hisHeroLine) && !SoundManager.Get().IsPlaybackFinished(hisHeroLine))
				{
					yield return null;
				}
			}
		}
		else
		{
			yield return new WaitForSeconds(0.6f);
		}
		yield return StartCoroutine(GameState.Get().GetGameEntity().PlayMissionIntroLineAndWait());
		myheroLabel.transform.parent = null;
		hisheroLabel.transform.parent = null;
		myheroLabel.FadeOut();
		hisheroLabel.FadeOut();
		yield return new WaitForSeconds(0.5f);
		if (m_MyCustomSocketInSpell != null)
		{
			m_MyCustomSocketInSpell.Location = SpellLocation.NONE;
			m_MyCustomSocketInSpell.gameObject.SetActive(value: true);
			if (myHeroCardActor.SocketInParentEffectToHero)
			{
				Vector3 myActorScale = myHeroCardActor.transform.localScale;
				myHeroCardActor.transform.localScale = Vector3.one;
				m_MyCustomSocketInSpell.transform.parent = myHeroCardActor.transform;
				m_MyCustomSocketInSpell.transform.localPosition = Vector3.zero;
				myHeroCardActor.transform.localScale = myActorScale;
			}
			m_MyCustomSocketInSpell.SetSource(myHeroCardActor.GetCard().gameObject);
			m_MyCustomSocketInSpell.RemoveAllTargets();
			GameObject myHeroSocketBone = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.FRIENDLY).gameObject;
			m_MyCustomSocketInSpell.AddTarget(myHeroSocketBone);
			m_MyCustomSocketInSpell.ActivateState(SpellStateType.BIRTH);
			m_MyCustomSocketInSpell.AddStateFinishedCallback(delegate
			{
				myHeroCardActor.transform.position = myHeroSocketBone.transform.position;
				myHeroCardActor.transform.localScale = Vector3.one;
			});
			if (!myHeroCardActor.SocketInOverrideHeroAnimation)
			{
				cardAnim.Play("hisHeroAnimateToPosition");
			}
		}
		else
		{
			cardAnim.Play("hisHeroAnimateToPosition");
		}
		if (m_HisCustomSocketInSpell != null)
		{
			if ((bool)m_MyCustomSocketInSpell)
			{
				SoundUtils.SetSourceVolumes(m_HisCustomSocketInSpell, 0f);
			}
			m_HisCustomSocketInSpell.Location = SpellLocation.NONE;
			if (hisHeroCardActor.SocketInOverrideHeroAnimation)
			{
				yield return new WaitForSeconds(0.25f);
			}
			m_HisCustomSocketInSpell.gameObject.SetActive(value: true);
			if (hisHeroCardActor.SocketInParentEffectToHero)
			{
				Vector3 hisActorScale = hisHeroCardActor.transform.localScale;
				hisHeroCardActor.transform.localScale = Vector3.one;
				m_HisCustomSocketInSpell.transform.parent = hisHeroCardActor.transform;
				m_HisCustomSocketInSpell.transform.localPosition = Vector3.zero;
				hisHeroCardActor.transform.localScale = hisActorScale;
			}
			m_HisCustomSocketInSpell.SetSource(hisHeroCardActor.GetCard().gameObject);
			m_HisCustomSocketInSpell.RemoveAllTargets();
			GameObject hisHeroSocketBone = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.OPPOSING).gameObject;
			m_HisCustomSocketInSpell.AddTarget(hisHeroSocketBone);
			m_HisCustomSocketInSpell.ActivateState(SpellStateType.BIRTH);
			m_HisCustomSocketInSpell.AddStateFinishedCallback(delegate
			{
				hisHeroCardActor.transform.position = hisHeroSocketBone.transform.position;
				hisHeroCardActor.transform.localScale = Vector3.one;
			});
			if (!hisHeroCardActor.SocketInOverrideHeroAnimation)
			{
				oppCardAnim.Play("myHeroAnimateToPosition");
			}
		}
		else
		{
			oppCardAnim.Play("myHeroAnimateToPosition");
		}
		SoundManager.Get().LoadAndPlay("FX_MulliganCoin01_HeroCoinDrop.prefab:c46488739eda9f94eb0160290e35f321", hisHeroCardActor.GetCard().gameObject);
		if ((bool)versusText)
		{
			yield return new WaitForSeconds(0.1f);
			versusText.FadeOut();
			yield return new WaitForSeconds(0.32f);
		}
		if (m_MyCustomSocketInSpell == null)
		{
			myWeldEffect = UnityEngine.Object.Instantiate(weldPrefab);
			myWeldEffect.transform.position = myHero.transform.position;
			if ((bool)m_HisCustomSocketInSpell)
			{
				SoundUtils.SetSourceVolumes(myWeldEffect, 0f);
			}
			myWeldEffect.GetComponent<HeroWeld>().DoAnim();
		}
		if (m_HisCustomSocketInSpell == null)
		{
			hisWeldEffect = UnityEngine.Object.Instantiate(weldPrefab);
			hisWeldEffect.transform.position = hisHero.transform.position;
			if ((bool)m_MyCustomSocketInSpell)
			{
				SoundUtils.SetSourceVolumes(hisWeldEffect, 0f);
			}
			hisWeldEffect.GetComponent<HeroWeld>().DoAnim();
		}
		yield return new WaitForSeconds(0.05f);
		iTween.ShakePosition(Camera.main.gameObject, new Vector3(0.03f, 0.01f, 0.03f), 0.6f);
		Action<object> OnMyLightBlendUpdate = delegate(object amount)
		{
			if (myHeroMat != null)
			{
				myHeroMat.SetFloat("_LightingBlend", (float)amount);
			}
			if (myHeroFrameMat != null)
			{
				myHeroFrameMat.SetFloat("_LightingBlend", (float)amount);
			}
			myHeroCardActor.UpdateCustomFrameLightingBlend((float)amount);
		};
		OnMyLightBlendUpdate(0f);
		Hashtable myLightBlendArgs = iTweenManager.Get().GetTweenHashTable();
		myLightBlendArgs.Add("time", 1f);
		myLightBlendArgs.Add("from", 0f);
		myLightBlendArgs.Add("to", 1f);
		myLightBlendArgs.Add("delay", 2f);
		myLightBlendArgs.Add("onupdate", OnMyLightBlendUpdate);
		myLightBlendArgs.Add("onupdatetarget", base.gameObject);
		myLightBlendArgs.Add("name", "MyHeroLightBlend");
		iTween.ValueTo(base.gameObject, myLightBlendArgs);
		Action<object> OnHisLightBlendUpdate = delegate(object amount)
		{
			if (hisHeroMat != null)
			{
				hisHeroMat.SetFloat("_LightingBlend", (float)amount);
			}
			if (hisHeroFrameMat != null)
			{
				hisHeroFrameMat.SetFloat("_LightingBlend", (float)amount);
			}
			hisHeroCardActor.UpdateCustomFrameLightingBlend((float)amount);
		};
		OnHisLightBlendUpdate(0f);
		Hashtable hisLightBlendArgs = iTweenManager.Get().GetTweenHashTable();
		hisLightBlendArgs.Add("time", 1f);
		hisLightBlendArgs.Add("from", 0f);
		hisLightBlendArgs.Add("to", 1f);
		hisLightBlendArgs.Add("delay", 2f);
		hisLightBlendArgs.Add("onupdate", OnHisLightBlendUpdate);
		hisLightBlendArgs.Add("onupdatetarget", base.gameObject);
		hisLightBlendArgs.Add("name", "HisHeroLightBlend");
		iTween.ValueTo(base.gameObject, hisLightBlendArgs);
		yield return GameState.Get().GetGameEntity().DoGameSpecificPostIntroActions();
		introComplete = true;
		GameState.Get().GetGameEntity().NotifyOfHeroesFinishedAnimatingInMulligan();
		ScreenEffectsMgr.Get().SetActive(enabled: true);
		myHeroCardActor.UpdateCustomFrameDiamondMaterial();
		hisHeroCardActor.UpdateCustomFrameDiamondMaterial();
	}

	public void OnMulliganChooseOneTentativeSelection(Network.MulliganChooseOneTentativeSelection selection)
	{
		if (selection.IsFromTeammate)
		{
			TeammateBoardViewer.Get().UpdateHeroHighlightedState(selection.EntityId, selection.IsConfirmation);
		}
		else
		{
			SetHoldState(selection.EntityId, selection.IsConfirmation);
		}
	}

	public void BeginMulligan()
	{
		bool wasMulliganAlreadyActive = mulliganActive;
		ForceMulliganActive(active: true);
		if (GameState.Get().WasConcedeRequested())
		{
			HandleGameOverDuringMulligan();
		}
		else if (!wasMulliganAlreadyActive || !SpectatorManager.Get().IsSpectatingOpposingSide())
		{
			m_ContinueMulliganWhenBoardLoads = ContinueMulliganWhenBoardLoads();
			StartCoroutine(m_ContinueMulliganWhenBoardLoads);
		}
	}

	private void OnCreateGame(GameState.CreateGamePhase phase, object userData)
	{
		GameState.Get().UnregisterCreateGameListener(OnCreateGame);
		HandleGameStart();
	}

	private void HandleGameStart()
	{
		Log.LoadingScreen.Print("MulliganManager.HandleGameStart() - IsPastBeginPhase()={0}", GameState.Get().IsPastBeginPhase());
		bool shouldSkipPuzzleIntro = GameMgr.Get().IsSpectator() && GameState.Get().GetGameEntity().HasTag(GAME_TAG.PUZZLE_MODE);
		if (GameState.Get().IsPastBeginPhase() || shouldSkipPuzzleIntro)
		{
			m_SkipMulliganForResume = SkipMulliganForResume();
			StartCoroutine(m_SkipMulliganForResume);
			return;
		}
		InitZones();
		m_DimLightsOnceBoardLoads = DimLightsOnceBoardLoads();
		StartCoroutine(m_DimLightsOnceBoardLoads);
		AssetReference versusTextReference = "GameStart_VS_Letters.prefab:3cb2cbed6d44a694eb23fb8791684003";
		AssetReference announcerVoReference = "VO_ANNOUNCER_VERSUS_21.prefab:acc34acb15f07ff4ba08025a57a9a458";
		if (!GameState.Get().GetGameEntity().ShouldDoAlternateMulliganIntro())
		{
			m_xLabels = new GameObject[4];
			coinObject = UnityEngine.Object.Instantiate(coinPrefab);
			coinObject.SetActive(value: false);
			if (!Cheats.Get().ShouldSkipMulligan())
			{
				if (Cheats.Get().IsLaunchingQuickGame())
				{
					TimeScaleMgr.Get().SetTimeScaleMultiplier(SceneDebugger.GetDevTimescaleMultiplier());
				}
				waitingForVersusVo = true;
				SoundLoader.LoadSound(announcerVoReference, OnVersusVoLoaded);
			}
			waitingForVersusText = true;
			if (!AssetLoader.Get().InstantiatePrefab(versusTextReference, OnVersusTextLoadAttempted))
			{
				OnVersusTextLoadAttempted(versusTextReference, null, null);
			}
			if (m_WaitForBoardThenLoadButton != null)
			{
				StopCoroutine(m_WaitForBoardThenLoadButton);
			}
			m_WaitForBoardThenLoadButton = WaitForBoardThenLoadButton();
			StartCoroutine(m_WaitForBoardThenLoadButton);
		}
		else
		{
			if (!GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
			{
				waitingForVersusVo = true;
				SoundLoader.LoadSound(announcerVoReference, OnVersusVoLoaded);
			}
			waitingForVersusText = true;
			if (!AssetLoader.Get().InstantiatePrefab(versusTextReference, OnVersusTextLoadAttempted))
			{
				OnVersusTextLoadAttempted(versusTextReference, null, null);
			}
		}
		m_WaitForHeroesAndStartAnimations = WaitForHeroesAndStartAnimations();
		if (m_WaitForHeroesAndStartAnimations != null)
		{
			StartCoroutine(m_WaitForHeroesAndStartAnimations);
		}
		Log.LoadingScreen.Print("MulliganManager.HandleGameStart() - IsMulliganPhase()={0}", GameState.Get().IsMulliganPhase());
		if (GameState.Get().IsMulliganPhase())
		{
			m_ResumeMulligan = ResumeMulligan();
			StartCoroutine(m_ResumeMulligan);
		}
	}

	private IEnumerator DimLightsOnceBoardLoads()
	{
		while (Board.Get() == null)
		{
			yield return null;
		}
		Board.Get().SetMulliganLighting();
	}

	private IEnumerator ResumeMulligan()
	{
		m_resuming = true;
		foreach (Player player in GameState.Get().GetPlayerMap().Values)
		{
			if (player.GetTag<TAG_MULLIGAN>(GAME_TAG.MULLIGAN_STATE) == TAG_MULLIGAN.DONE)
			{
				if (player.IsFriendlySide())
				{
					friendlyPlayerHasReplacementCards = true;
				}
				else
				{
					opponentPlayerHasReplacementCards = true;
				}
			}
		}
		if (friendlyPlayerHasReplacementCards)
		{
			SkipCardChoosing();
		}
		else
		{
			while (GameState.Get().GetResponseMode() != GameState.ResponseMode.CHOICE)
			{
				yield return null;
			}
		}
		BeginMulligan();
	}

	private void OnMulliganTimerUpdate(TurnTimerUpdate update, object userData)
	{
		if (update.GetSecondsRemaining() > Mathf.Epsilon)
		{
			if (update.ShouldShow())
			{
				BeginMulliganCountdown(update.GetEndTimestamp());
			}
			else
			{
				StopMulliganCountdown();
			}
		}
		else
		{
			GameState.Get().UnregisterMulliganTimerUpdateListener(OnMulliganTimerUpdate);
			AutomaticContinueMulligan();
		}
	}

	private bool OnEntitiesChosenReceived(Network.EntitiesChosen chosen, object userData)
	{
		if (!GameMgr.Get().IsSpectator())
		{
			if (GameMgr.Get().IsBattlegroundDuoGame() && tentativeConfirmedChooseOneEntityId != 0)
			{
				tentativeConfirmedChooseOneEntityId = 0;
				AutomaticContinueMulligan(destroyTimer: true);
				return false;
			}
			return false;
		}
		int playerId = chosen.PlayerId;
		int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
		if (playerId == friendlyPlayerId)
		{
			m_Spectator_WaitForFriendlyPlayerThenProcessEntitiesChosen = Spectator_WaitForFriendlyPlayerThenProcessEntitiesChosen(chosen);
			StartCoroutine(m_Spectator_WaitForFriendlyPlayerThenProcessEntitiesChosen);
			return true;
		}
		return false;
	}

	private void OnGameOver(TAG_PLAYSTATE playState, object userData)
	{
		HandleGameOverDuringMulligan();
	}

	private IEnumerator Spectator_WaitForFriendlyPlayerThenProcessEntitiesChosen(Network.EntitiesChosen chosen)
	{
		while (!m_waitingForUserInput)
		{
			if (GameState.Get().IsGameOver() || skipCardChoosing)
			{
				yield break;
			}
			yield return null;
		}
		for (int cardIndex = 0; cardIndex < m_startingCards.Count; cardIndex++)
		{
			int entityId = m_startingCards[cardIndex].GetEntity().GetEntityId();
			bool spectateeMarkedReplaced = !chosen.Entities.Contains(entityId);
			if (m_handCardsMarkedForReplace[cardIndex] != spectateeMarkedReplaced)
			{
				ToggleHoldState(cardIndex);
			}
		}
		GameState.Get().OnEntitiesChosenProcessed(chosen);
		BeginDealNewCards();
	}

	private IEnumerator ContinueMulliganWhenBoardLoads()
	{
		while (ZoneMgr.Get() == null)
		{
			yield return null;
		}
		Board board = Board.Get();
		InitStartingHandZone();
		InitZones();
		if (m_resuming)
		{
			while (ShouldWaitForMulliganCardsToBeProcessed())
			{
				yield return null;
			}
		}
		SortHand(friendlySideHandZone);
		SortHand(opposingSideHandZone);
		board.CombinedSurface();
		board.FindCollider("DragPlane").enabled = false;
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_HAS_HERO_LOBBY))
		{
			m_ShowMultiplayerWaitingArea = ShowMultiplayerWaitingArea();
			StartCoroutine(m_ShowMultiplayerWaitingArea);
		}
		else
		{
			m_DealStartingCards = DealStartingCards();
			StartCoroutine(m_DealStartingCards);
		}
	}

	private bool InitStartingHandZone()
	{
		if (startingHandZone == null)
		{
			Board board = Board.Get();
			if (board == null)
			{
				return false;
			}
			startingHandZone = board.FindBone("StartingHandZone").gameObject;
		}
		return startingHandZone != null;
	}

	private void InitZones()
	{
		foreach (Zone zone in ZoneMgr.Get().GetZones())
		{
			if (zone is ZoneHand)
			{
				if (zone.m_Side == Player.Side.FRIENDLY)
				{
					friendlySideHandZone = (ZoneHand)zone;
				}
				else
				{
					opposingSideHandZone = (ZoneHand)zone;
				}
			}
			if (zone is ZoneDeck)
			{
				if (zone.m_Side == Player.Side.FRIENDLY)
				{
					friendlySideDeck = (ZoneDeck)zone;
					friendlySideDeck.SetSuppressEmotes(suppress: true);
					friendlySideDeck.UpdateLayout();
				}
				else
				{
					opposingSideDeck = (ZoneDeck)zone;
					opposingSideDeck.SetSuppressEmotes(suppress: true);
					opposingSideDeck.UpdateLayout();
				}
			}
		}
	}

	private bool ShouldWaitForMulliganCardsToBeProcessed()
	{
		PowerProcessor powerProcessor = GameState.Get().GetPowerProcessor();
		bool receivedEndOfMulligan = false;
		powerProcessor.ForEachTaskList(delegate(int index, PowerTaskList taskList)
		{
			if (IsTaskListPuttingUsPastMulligan(taskList))
			{
				receivedEndOfMulligan = true;
			}
		});
		if (receivedEndOfMulligan)
		{
			return false;
		}
		return powerProcessor.HasTaskLists();
	}

	private bool IsTaskListPuttingUsPastMulligan(PowerTaskList taskList)
	{
		foreach (PowerTask task in taskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type == Network.PowerType.TAG_CHANGE)
			{
				Network.HistTagChange tagChange = power as Network.HistTagChange;
				if (tagChange.Tag == 198 && GameUtils.IsPastBeginPhase((TAG_STEP)tagChange.Value))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void GetStartingLists()
	{
		List<Card> cards = friendlySideHandZone.GetCards();
		List<Card> opponentCards = opposingSideHandZone.GetCards();
		int friendlyCardCountExcludingCoin;
		if (ShouldHandleCoinCard())
		{
			if (friendlyPlayerGoesFirst)
			{
				friendlyCardCountExcludingCoin = cards.Count;
				m_bonusCardIndex = opponentCards.Count - 2;
				m_coinCardIndex = opponentCards.Count - 1;
			}
			else
			{
				friendlyCardCountExcludingCoin = cards.Count - 1;
				m_bonusCardIndex = cards.Count - 2;
			}
		}
		else
		{
			friendlyCardCountExcludingCoin = cards.Count;
			if (friendlyPlayerGoesFirst)
			{
				m_bonusCardIndex = opponentCards.Count - 1;
			}
			else
			{
				m_bonusCardIndex = cards.Count - 1;
			}
		}
		m_startingCards = new List<Card>();
		for (int i = 0; i < friendlyCardCountExcludingCoin; i++)
		{
			m_startingCards.Add(cards[i]);
		}
		m_startingOppCards = new List<Card>();
		for (int j = 0; j < opponentCards.Count; j++)
		{
			m_startingOppCards.Add(opponentCards[j]);
		}
	}

	private IEnumerator PlayStartingTaunts()
	{
		return EmoteHandler.Get().PlayStartingTaunts(base.gameObject);
	}

	private void SetupCardActor(ref List<Card> cards)
	{
		bool shouldSendTelemetry = false;
		foreach (Card card in cards)
		{
			if (card != null && card.GetActor() != null)
			{
				card.GetActor().SetActorState(ActorStateType.CARD_IDLE);
				card.GetActor().TurnOffCollider();
				card.GetActor().GetMeshRenderer().gameObject.layer = 8;
				if (card.GetActor().m_nameTextMesh != null)
				{
					card.GetActor().m_nameTextMesh.UpdateNow();
				}
			}
			else if (card == null)
			{
				shouldSendTelemetry = true;
			}
			if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS) && card != null && card.GetActor() != null)
			{
				pendingHeroCount++;
				card.GetActor().gameObject.SetActive(value: false);
				AssetReference assetReference = GameState.Get().GetStringGameOption(GameEntityOption.ALTERNATE_MULLIGAN_ACTOR_NAME);
				if (!AssetLoader.Get().InstantiatePrefab(assetReference, OnHeroActorLoadAttempted, card, AssetLoadingOptions.IgnorePrefabPosition))
				{
					OnHeroActorLoadAttempted(assetReference, null, card);
				}
			}
		}
		if (shouldSendTelemetry)
		{
			string telemetryString = "SetupCardActor - Found a null card within starting hero cards during initialization. Starting Cards: ";
			for (int i = 0; i < m_startingCards.Count; i++)
			{
				telemetryString += ((m_startingCards[i] == null) ? "NULL" : m_startingCards[i].GetEntity().GetName());
				telemetryString += ((i == m_startingCards.Count - 1) ? "." : ", ");
			}
			TelemetryManager.Client().SendLiveIssue("Gameplay_MulliganManager", telemetryString);
			Log.MulliganManager.PrintWarning(telemetryString);
		}
	}

	private void SetupCardCollider(ref List<Card> cards)
	{
		bool shouldSendTelemetry = false;
		foreach (Card startCard in cards)
		{
			if (startCard != null)
			{
				if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS) && choiceHeroActors.ContainsKey(startCard))
				{
					choiceHeroActors[startCard].TurnOnCollider();
				}
				else
				{
					startCard.GetActor().TurnOnCollider();
				}
			}
			else
			{
				shouldSendTelemetry = true;
			}
		}
		if (shouldSendTelemetry)
		{
			string telemetryString = "SetupCardCollider - Found a null card in starting cards while enabling colliders. Starting cards: ";
			for (int i = 0; i < m_startingCards.Count; i++)
			{
				telemetryString += ((m_startingCards[i] == null) ? "NULL" : m_startingCards[i].GetEntity().GetName());
				telemetryString += ((i == m_startingCards.Count - 1) ? "." : ", ");
			}
			TelemetryManager.Client().SendLiveIssue("Gameplay_MulliganManager", telemetryString);
			Log.MulliganManager.PrintWarning(telemetryString);
		}
	}

	private void CreateBannerIfNeeded()
	{
		if (mulliganChooseBanner == null)
		{
			mulliganChooseBanner = UnityEngine.Object.Instantiate(GetChooseBannerPrefab(), Board.Get().FindBone("ChoiceBanner").position, new Quaternion(0f, 0f, 0f, 0f));
		}
		string bannerText = GameState.Get().GetGameEntity().GetMulliganBannerText();
		string bannerSubtitleText = GameState.Get().GetGameEntity().GetMulliganBannerSubtitleText();
		SetMulliganBannerText(bannerText, bannerSubtitleText);
		ShowMulliganDetail();
	}

	private void CreateBannerShrinking()
	{
		mulliganChooseBanner = UnityEngine.Object.Instantiate(GetChooseBannerPrefab());
		SetMulliganBannerText(GameStrings.Get("GAMEPLAY_MULLIGAN_STARTING_HAND"));
		Vector3 mulliganChooseBannerPosition = Board.Get().FindBone("ChoiceBanner").position;
		mulliganChooseBanner.transform.position = mulliganChooseBannerPosition;
		Vector3 startingScale = mulliganChooseBanner.transform.localScale;
		mulliganChooseBanner.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
		iTween.ScaleTo(mulliganChooseBanner, startingScale, 0.5f);
		m_ShrinkStartingHandBanner = ShrinkStartingHandBanner(mulliganChooseBanner);
		StartCoroutine(m_ShrinkStartingHandBanner);
	}

	public List<Actor> GetFakeCardsOnLeft()
	{
		return fakeCardsOnLeft;
	}

	public List<Actor> GetFakeCardsOnRight()
	{
		return fakeCardsOnRight;
	}

	public List<Card> GetStartingCards()
	{
		return m_startingCards;
	}

	public Card GetSelectedHero()
	{
		return m_chosenHero;
	}

	public void SetConfirmationButtonVisibility(bool show)
	{
		if (mulliganButtonWidget != null)
		{
			mulliganButtonWidget.gameObject.SetActive(show);
		}
		if (mulliganButton != null)
		{
			mulliganButton.gameObject.SetActive(show);
		}
	}

	public Vector3 GetHeroSelectFinalPosition(int index, int heroCount, ZoneHand zoneHandFriendly)
	{
		InitStartingHandZone();
		float zoneWidth = startingHandZone.GetComponent<Collider>().bounds.size.x;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			zoneWidth *= 0.55f;
		}
		float spacingToUse = zoneWidth / (float)heroCount;
		float num = startingHandZone.transform.position.x - zoneWidth / 2f;
		float cardHeightOffset = 0f;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			cardHeightOffset = 7f;
		}
		float cardZpos = startingHandZone.transform.position.z - 0.3f;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			cardZpos = startingHandZone.transform.position.z - 0.2f;
		}
		float xOffset = spacingToUse / 2f + (float)index * spacingToUse;
		return new Vector3(num + xOffset, zoneHandFriendly.transform.position.y + cardHeightOffset, cardZpos);
	}

	public Vector3 GetHeroWaitPosition(ZoneHand zoneHandFriendly)
	{
		if (!InitStartingHandZone())
		{
			return Vector3.zero;
		}
		float zoneWidth = startingHandZone.GetComponent<Collider>().bounds.size.x;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			zoneWidth *= 0.55f;
		}
		float spacingToUse = zoneWidth;
		float num = startingHandZone.transform.position.x - zoneWidth / 2f;
		float cardHeightOffset = 0f;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			cardHeightOffset = 7f;
		}
		float cardZpos = startingHandZone.transform.position.z - 0.3f;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			cardZpos = startingHandZone.transform.position.z - 0.2f;
		}
		float xOffset = spacingToUse / 2f;
		return new Vector3(num + xOffset, zoneHandFriendly.transform.position.y + cardHeightOffset, cardZpos);
	}

	private IEnumerator ShowMultiplayerWaitingArea()
	{
		yield return new WaitForSeconds(1f);
		while (!introComplete)
		{
			yield return null;
		}
		yield return StartCoroutine(GameState.Get().GetGameEntity().DoActionsAfterIntroBeforeMulligan());
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.DO_OPENING_TAUNTS) && !Cheats.Get().ShouldSkipMulligan())
		{
			m_PlayStartingTaunts = PlayStartingTaunts();
			StartCoroutine(m_PlayStartingTaunts);
		}
		while (ZoneMgr.Get().HasPendingServerChange() || ZoneMgr.Get().HasActiveServerChange())
		{
			yield return null;
		}
		if (GameMgr.Get().IsBattlegrounds())
		{
			CreateBannerIfNeeded();
			if (mulliganChooseBanner == null)
			{
				yield break;
			}
			Banner banner = mulliganChooseBanner.GetComponent<Banner>();
			if (IsBaconAnomalyActive())
			{
				yield return banner.ShowBaconAnomalyIntro();
			}
			if (IsBaconQuestsActive())
			{
				banner.SetBaconQuestsBanner();
			}
			if (IsBaconBuddiesActive())
			{
				banner.SetBaconBuddiesBanner();
			}
			if (IsBaconTrinketActive())
			{
				banner.SetBaconTrinketBanner();
			}
		}
		Player friendlyPlayer = GameState.Get().GetFriendlySidePlayer();
		friendlyPlayerGoesFirst = friendlyPlayer.HasTag(GAME_TAG.FIRST_PLAYER);
		GetStartingLists();
		bool isMulliganOver = false;
		bool shouldSendTelemetry = true;
		if (m_startingCards.Count == 0)
		{
			while (GameState.Get().GetFriendlySidePlayer().GetHeroCard() == null)
			{
				if (shouldSendTelemetry)
				{
					TelemetryManager.Client().SendLiveIssue("Gameplay_MulliganManager", "No hero card set for friendly side player");
					shouldSendTelemetry = false;
				}
				yield return null;
			}
			m_startingCards.Add(GameState.Get().GetFriendlySidePlayer().GetHeroCard());
			isMulliganOver = true;
		}
		foreach (Card card in m_startingCards)
		{
			if (card != null && card.IsActorLoading())
			{
				yield return null;
			}
		}
		SetupCardActor(ref m_startingCards);
		while (pendingHeroCount > 0)
		{
			yield return null;
		}
		float zoneWidth = startingHandZone.GetComponent<Collider>().bounds.size.x;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			zoneWidth *= 0.55f;
		}
		int numFakeCardsOnLeft = GameState.Get().GetGameEntity().GetNumberOfFakeMulliganCardsToShowOnLeft(m_startingCards.Count);
		int numFakeCardsOnRight = GameState.Get().GetGameEntity().GetNumberOfFakeMulliganCardsToShowOnRight(m_startingCards.Count);
		if (!isMulliganOver)
		{
			if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
			{
				pendingFakeHeroCount = numFakeCardsOnLeft + numFakeCardsOnRight;
				for (int i = 0; i < numFakeCardsOnLeft; i++)
				{
					AssetLoader.Get().InstantiatePrefab(GameState.Get().GetStringGameOption(GameEntityOption.ALTERNATE_MULLIGAN_ACTOR_NAME), OnFakeHeroActorLoaded, fakeCardsOnLeft, AssetLoadingOptions.IgnorePrefabPosition);
				}
				for (int j = 0; j < numFakeCardsOnRight; j++)
				{
					AssetLoader.Get().InstantiatePrefab(GameState.Get().GetStringGameOption(GameEntityOption.ALTERNATE_MULLIGAN_ACTOR_NAME), OnFakeHeroActorLoaded, fakeCardsOnRight, AssetLoadingOptions.IgnorePrefabPosition);
				}
			}
			while (pendingFakeHeroCount > 0)
			{
				yield return null;
			}
		}
		else
		{
			numFakeCardsOnLeft = 0;
			numFakeCardsOnRight = 0;
		}
		float spaceForEachCard = zoneWidth / (float)Mathf.Max(m_startingCards.Count + numFakeCardsOnLeft + numFakeCardsOnRight, 1);
		float spacingToUse = spaceForEachCard;
		float leftSideOfZone = startingHandZone.transform.position.x - zoneWidth / 2f;
		float rightSideOfZone = startingHandZone.transform.position.x + zoneWidth / 2f;
		float timingBonus = 0.1f;
		int numCardsToDealExcludingBonusCard = m_startingCards.Count;
		opposingSideHandZone.SetDoNotUpdateLayout(enable: false);
		opposingSideHandZone.UpdateLayout(null, forced: true, 3);
		float cardHeightOffset = 0f;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			cardHeightOffset = 7f;
		}
		float zOffset = ((!UniversalInputManager.UsePhoneUI) ? (GameMgr.Get().IsBattlegrounds() ? 0.2f : (-0.3f)) : (GameMgr.Get().IsBattlegrounds() ? 0.3f : (-0.2f)));
		float cardZpos = startingHandZone.transform.position.z + zOffset;
		float xOffset = spacingToUse / 2f;
		foreach (Actor cardActor in fakeCardsOnLeft)
		{
			if (cardActor != null)
			{
				GameObject card2 = cardActor.gameObject;
				iTween.Stop(card2);
				Vector3[] drawPath = new Vector3[3]
				{
					card2.transform.position,
					new Vector3(card2.transform.position.x, card2.transform.position.y + 3.6f, card2.transform.position.z),
					new Vector3(leftSideOfZone + xOffset, friendlySideHandZone.transform.position.y + cardHeightOffset, cardZpos)
				};
				Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
				moveArgs.Add("path", drawPath);
				moveArgs.Add("time", ANIMATION_TIME_DEAL_CARD);
				moveArgs.Add("easetype", iTween.EaseType.easeInSineOutExpo);
				iTween.MoveTo(card2, moveArgs);
				if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
				{
					iTween.ScaleTo(card2, GameState.Get().GetGameEntity().GetAlternateMulliganActorScale(), ANIMATION_TIME_DEAL_CARD);
				}
				else
				{
					iTween.ScaleTo(card2, FRIENDLY_PLAYER_CARD_SCALE, ANIMATION_TIME_DEAL_CARD);
				}
				Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
				rotateArgs.Add("rotation", new Vector3(0f, 0f, 0f));
				rotateArgs.Add("time", ANIMATION_TIME_DEAL_CARD);
				rotateArgs.Add("delay", ANIMATION_TIME_DEAL_CARD / 16f);
				iTween.RotateTo(card2, rotateArgs);
				yield return new WaitForSeconds(0.04f);
				SoundManager.Get().LoadAndPlay("FX_GameStart09_CardsOntoTable.prefab:da502e035813b5742a04d2ef4f588255", card2);
				xOffset += spacingToUse;
				yield return new WaitForSeconds(0.05f + timingBonus);
				timingBonus = 0f;
			}
		}
		for (int k = 0; k < numCardsToDealExcludingBonusCard; k++)
		{
			if (!(m_startingCards[k] == null))
			{
				GameObject card2 = m_startingCards[k].gameObject;
				if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS) && choiceHeroActors.ContainsKey(m_startingCards[k]))
				{
					card2 = choiceHeroActors[m_startingCards[k]].transform.parent.gameObject;
				}
				iTween.Stop(card2);
				Vector3[] drawPath2 = new Vector3[3]
				{
					card2.transform.position,
					new Vector3(card2.transform.position.x, card2.transform.position.y + 3.6f, card2.transform.position.z),
					new Vector3(leftSideOfZone + xOffset, friendlySideHandZone.transform.position.y + cardHeightOffset, cardZpos)
				};
				Hashtable moveArgs2 = iTweenManager.Get().GetTweenHashTable();
				moveArgs2.Add("path", drawPath2);
				moveArgs2.Add("time", ANIMATION_TIME_DEAL_CARD);
				moveArgs2.Add("easetype", iTween.EaseType.easeInSineOutExpo);
				iTween.MoveTo(card2, moveArgs2);
				if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
				{
					iTween.ScaleTo(card2, GameState.Get().GetGameEntity().GetAlternateMulliganActorScale(), ANIMATION_TIME_DEAL_CARD);
				}
				else
				{
					iTween.ScaleTo(card2, FRIENDLY_PLAYER_CARD_SCALE, ANIMATION_TIME_DEAL_CARD);
				}
				Hashtable rotateArgs2 = iTweenManager.Get().GetTweenHashTable();
				rotateArgs2.Add("rotation", new Vector3(0f, 0f, 0f));
				rotateArgs2.Add("time", ANIMATION_TIME_DEAL_CARD);
				rotateArgs2.Add("delay", ANIMATION_TIME_DEAL_CARD / 16f);
				iTween.RotateTo(card2, rotateArgs2);
				yield return new WaitForSeconds(0.04f);
				SoundManager.Get().LoadAndPlay("FX_GameStart09_CardsOntoTable.prefab:da502e035813b5742a04d2ef4f588255", card2);
				xOffset += spacingToUse;
				yield return new WaitForSeconds(0.05f + timingBonus);
				timingBonus = 0f;
			}
		}
		foreach (Actor cardActor2 in fakeCardsOnRight)
		{
			if (cardActor2 != null)
			{
				GameObject card2 = cardActor2.gameObject;
				iTween.Stop(card2);
				Vector3[] drawPath3 = new Vector3[3]
				{
					card2.transform.position,
					new Vector3(card2.transform.position.x, card2.transform.position.y + 3.6f, card2.transform.position.z),
					new Vector3(leftSideOfZone + xOffset, friendlySideHandZone.transform.position.y + cardHeightOffset, cardZpos)
				};
				Hashtable moveArgs3 = iTweenManager.Get().GetTweenHashTable();
				moveArgs3.Add("path", drawPath3);
				moveArgs3.Add("time", ANIMATION_TIME_DEAL_CARD);
				moveArgs3.Add("easetype", iTween.EaseType.easeInSineOutExpo);
				iTween.MoveTo(card2, moveArgs3);
				if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
				{
					iTween.ScaleTo(card2, GameState.Get().GetGameEntity().GetAlternateMulliganActorScale(), ANIMATION_TIME_DEAL_CARD);
				}
				else
				{
					iTween.ScaleTo(card2, FRIENDLY_PLAYER_CARD_SCALE, ANIMATION_TIME_DEAL_CARD);
				}
				Hashtable rotateArgs3 = iTweenManager.Get().GetTweenHashTable();
				rotateArgs3.Add("rotation", new Vector3(0f, 0f, 0f));
				rotateArgs3.Add("time", ANIMATION_TIME_DEAL_CARD);
				rotateArgs3.Add("delay", ANIMATION_TIME_DEAL_CARD / 16f);
				iTween.RotateTo(card2, rotateArgs3);
				yield return new WaitForSeconds(0.04f);
				SoundManager.Get().LoadAndPlay("FX_GameStart09_CardsOntoTable.prefab:da502e035813b5742a04d2ef4f588255", card2);
				xOffset += spacingToUse;
				yield return new WaitForSeconds(0.05f + timingBonus);
				timingBonus = 0f;
			}
		}
		if (skipCardChoosing)
		{
			CreateBannerShrinking();
			ShowMulliganDetail();
		}
		yield return new WaitForSeconds(1.1f);
		while (GameState.Get().IsBusy())
		{
			yield return null;
		}
		if (friendlyPlayerGoesFirst)
		{
			xOffset = 0f;
			for (int i2 = m_startingCards.Count - 1; i2 >= 0; i2--)
			{
				if (m_startingCards[i2] != null)
				{
					GameObject topCard = m_startingCards[i2].gameObject;
					if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS) && choiceHeroActors.ContainsKey(m_startingCards[i2]))
					{
						topCard = choiceHeroActors[m_startingCards[i2]].gameObject;
					}
					iTween.Stop(topCard);
					Hashtable moveArgs4 = iTweenManager.Get().GetTweenHashTable();
					moveArgs4.Add("position", new Vector3(rightSideOfZone - spaceForEachCard - xOffset + spaceForEachCard / 2f, friendlySideHandZone.transform.position.y + cardHeightOffset, cardZpos));
					moveArgs4.Add("time", 14f / 15f);
					moveArgs4.Add("easetype", iTween.EaseType.easeInOutCubic);
					iTween.MoveTo(topCard, moveArgs4);
					xOffset += spaceForEachCard;
				}
			}
		}
		GameState.Get().GetGameEntity().OnMulliganCardsDealt(m_startingCards);
		ShowRerollButtonFTUENotificationIfNeeded(m_startingCards);
		yield return new WaitForSeconds(0.6f);
		if (skipCardChoosing)
		{
			if (GameState.Get().IsMulliganPhase() || GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_HAS_HERO_LOBBY))
			{
				if (GameState.Get().IsFriendlySidePlayerTurn())
				{
					TurnStartManager.Get().BeginListeningForTurnEvents();
				}
				m_WaitForOpponentToFinishMulligan = WaitForOpponentToFinishMulligan();
				StartCoroutine(m_WaitForOpponentToFinishMulligan);
			}
			else
			{
				yield return new WaitForSeconds(2f);
				EndMulligan();
			}
			yield break;
		}
		SetupCardCollider(ref m_startingCards);
		CreateBannerIfNeeded();
		CreateTagConditionalVFXs(Board.Get().FindBone("ChoiceBanner").position);
		if (GameState.Get().IsInChoiceMode() && GameMgr.Get().IsSpectator())
		{
			m_replaceLabels = new List<MulliganReplaceLabel>();
			for (int l = 0; l < m_startingCards.Count; l++)
			{
				if (m_startingCards[l] != null)
				{
					InputManager.Get().DoNetworkResponse(m_startingCards[l].GetEntity());
				}
				m_replaceLabels.Add(null);
			}
		}
		while (mulliganButton == null && GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_REQUIRES_CONFIRMATION))
		{
			yield return null;
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_REQUIRES_CONFIRMATION))
		{
			mulliganButton.transform.position = new Vector3(startingHandZone.transform.position.x, friendlySideHandZone.transform.position.y, myHeroCardActor.transform.position.z + (float)PLATFORM_DEPENDENT_Z_OFFSET);
			mulliganButton.transform.localEulerAngles = new Vector3(90f, 90f, 90f);
			mulliganButton.AddEventListener(UIEventType.RELEASE, OnMulliganButtonReleased);
			mulliganButtonWidget.transform.position = new Vector3(startingHandZone.transform.position.x, friendlySideHandZone.transform.position.y, myHeroCardActor.transform.position.z + (float)PLATFORM_DEPENDENT_Z_OFFSET);
			mulliganButtonWidget.AddEventListener(UIEventType.RELEASE, OnMulliganButtonReleased);
			mulliganCancelConfirmationButtonWidget.transform.position = new Vector3(startingHandZone.transform.position.x + 2f, friendlySideHandZone.transform.position.y, myHeroCardActor.transform.position.z);
			mulliganCancelConfirmationButtonWidget.AddEventListener(UIEventType.RELEASE, OnMulliganCancelConfirmationButtonReleased);
			m_WaitAFrameBeforeSendingEventToMulliganButton = WaitAFrameBeforeSendingEventToMulliganButton(mulliganButton);
			StartCoroutine(m_WaitAFrameBeforeSendingEventToMulliganButton);
			if (!GameMgr.Get().IsSpectator() && !Options.Get().GetBool(Option.HAS_SEEN_MULLIGAN, defaultVal: false) && !GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE) && UserAttentionManager.CanShowAttentionGrabber("MulliganManager.DealStartingCards:" + Option.HAS_SEEN_MULLIGAN))
			{
				innkeeperMulliganDialog = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, new Vector3(155.3f, NotificationManager.DEPTH, 34.5f), GameStrings.Get("VO_INNKEEPER_MULLIGAN_13"), "VO_INNKEEPER_MULLIGAN_13.prefab:3ec6b2e741ac16d4ca519bdfd26d10e3");
				Options.Get().SetBool(Option.HAS_SEEN_MULLIGAN, val: true);
				mulliganButton.GetComponent<Collider>().enabled = false;
			}
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE) && mulliganRefreshButtonEnabled)
		{
			while (m_refreshButton == null)
			{
				yield return null;
			}
			m_refreshButton.transform.position = new Vector3(mulliganButton.transform.position.x + 2f, mulliganButton.transform.position.y, mulliganButton.transform.position.z);
			m_refreshButton.transform.localEulerAngles = new Vector3(90f, 90f, 90f);
			m_refreshButton.AddEventListener(UIEventType.RELEASE, OnMulliganRefreshButtonReleased);
			m_WaitAFrameBeforeSendingEventToMulliganRefreshButton = WaitAFrameBeforeSendingEventToMulliganButton(m_refreshButton);
			StartCoroutine(m_WaitAFrameBeforeSendingEventToMulliganRefreshButton);
			m_refreshButton.GetComponent<Collider>().enabled = true;
		}
		GameState.Get().GetGameEntity().NotifyMulliganButtonReady();
		GameState.Get().GetGameEntity().StartMulliganSoundtracks(soft: true);
		m_waitingForUserInput = true;
		while (innkeeperMulliganDialog != null)
		{
			yield return null;
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_REQUIRES_CONFIRMATION))
		{
			mulliganButton.GetComponent<Collider>().enabled = true;
		}
		if (skipCardChoosing || Cheats.Get().ShouldSkipMulligan())
		{
			BeginDealNewCards();
		}
	}

	private Transform CalculateRerollTokenFTUETransform()
	{
		GameObject rerollTokenPopupBone = GameObjectUtils.FindChildBySubstring(base.gameObject, "RerollTokenFTUETooltipBone" + PLATFORM_DEPENDENT_BONE_SUFFIX);
		if (rerollTokenPopupBone == null)
		{
			return null;
		}
		Transform obj = rerollTokenPopupBone.transform;
		float xOffset = 0f;
		if (BnetBar.Get().TryGetRelevantCurrencyFrame(CurrencyType.BG_TOKEN, out var bgTokenFrame))
		{
			xOffset = (bgTokenFrame.gameObject.transform.position.x / bgTokenFrame.gameObject.transform.position.z - (float)PLATFORM_DEPENDENT_FTUE_ADJUSTMENT_OFFSET) / (float)PLATFORM_DEPENDENT_FTUE_ADJUSTMENT_SLOPE - rerollTokenPopupBone.transform.localPosition.x;
		}
		obj.position = new Vector3(rerollTokenPopupBone.transform.position.x + xOffset, rerollTokenPopupBone.transform.position.y, rerollTokenPopupBone.transform.position.z);
		return obj;
	}

	private void ShowRerollButtonFTUENotificationIfNeeded(List<Card> startingCards)
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_BG_REROLL_TOOLTIP, out long hasSeenBGRerollFTUE);
		if (hasSeenBGRerollFTUE > 0 || !GameState.Get().GetGameEntity().HasTag(GAME_TAG.BACON_MULLIGAN_HERO_REROLL_ACTIVE))
		{
			return;
		}
		int num;
		switch (startingCards.Count)
		{
		case 0:
			return;
		default:
			num = 0;
			break;
		case 4:
			num = 1;
			break;
		}
		int cardIdx = num;
		PlayerLeaderboardMainCardActor actor = startingCards[cardIdx].GetActor() as PlayerLeaderboardMainCardActor;
		if (!(actor == null) && !(actor.GetHeroRerollButton() == null))
		{
			string tokenMessage = GameStrings.Get("GLUE_BACON_TOOLTIP_REROLL_TOKEN_FTUE");
			string buttonMessage = GameStrings.Get("GLUE_BACON_TOOLTIP_REROLL_BUTTON_FTUE");
			GameObject rerollButtonPopupBone = GameObjectUtils.FindChildBySubstring(actor.GetHeroRerollButton().gameObject, "FTUEBone" + PLATFORM_DEPENDENT_BONE_SUFFIX);
			NotificationManager notificationManager = NotificationManager.Get();
			if (rerollButtonPopupBone != null)
			{
				m_RerollButtonFTUENotification = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, rerollButtonPopupBone.transform.position, rerollButtonPopupBone.transform.localScale, GameStrings.Get(buttonMessage));
				m_RerollButtonFTUENotification.ShowPopUpArrow(UniversalInputManager.UsePhoneUI ? Notification.PopUpArrowDirection.Down : Notification.PopUpArrowDirection.Up);
			}
			else
			{
				Log.MulliganManager.PrintWarning("MulliganManager.ShowRerollButtonFTUENotificationIfNeeded() - failed to show reroll button FTUE");
			}
			Transform rerollFTUETransform = CalculateRerollTokenFTUETransform();
			if (rerollFTUETransform != null)
			{
				m_RerollTokenFTUENotification = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, rerollFTUETransform.position, rerollFTUETransform.localScale, GameStrings.Get(tokenMessage));
				m_RerollTokenFTUENotification.ShowPopUpArrow(UniversalInputManager.UsePhoneUI ? Notification.PopUpArrowDirection.Up : Notification.PopUpArrowDirection.Down);
			}
			else
			{
				Log.MulliganManager.PrintWarning("MulliganManager.ShowRerollButtonFTUENotificationIfNeeded() - failed to show reroll token FTUE");
			}
			StartCoroutine(WaitThenHideRerollPopups());
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_BG_REROLL_TOOLTIP, 1L));
		}
	}

	private IEnumerator WaitThenHideRerollPopups(bool animate = false)
	{
		yield return new WaitForSecondsRealtime(heroRerollButtonFTUEDuration);
		HideRerollPopups(animate);
	}

	private void HideRerollPopups(bool animate = false)
	{
		if (m_RerollButtonFTUENotification != null)
		{
			if (animate)
			{
				NotificationManager.Get()?.DestroyNotification(m_RerollButtonFTUENotification, 0f);
			}
			else
			{
				NotificationManager.Get()?.DestroyNotificationNowWithNoAnim(m_RerollButtonFTUENotification);
			}
		}
		if (m_RerollTokenFTUENotification != null)
		{
			if (animate)
			{
				NotificationManager.Get()?.DestroyNotification(m_RerollTokenFTUENotification, 0f);
			}
			else
			{
				NotificationManager.Get()?.DestroyNotificationNowWithNoAnim(m_RerollTokenFTUENotification);
			}
		}
	}

	private IEnumerator DealStartingCards()
	{
		yield return new WaitForSeconds(1f);
		while (!introComplete)
		{
			yield return null;
		}
		if (IsHearthstoneAnomaliesActive())
		{
			CreateBannerIfNeeded();
			ShowHearthstoneAnomaliesDetail();
			if (mulliganChooseBanner != null)
			{
				Banner banner = mulliganChooseBanner.GetComponent<Banner>();
				yield return banner.ShowHearthstoneAnomalyIntro(GetAnomalies());
				CreateBannerIfNeeded();
			}
		}
		yield return StartCoroutine(GameState.Get().GetGameEntity().DoActionsAfterIntroBeforeMulligan());
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.DO_OPENING_TAUNTS) && !Cheats.Get().ShouldSkipMulligan())
		{
			m_PlayStartingTaunts = PlayStartingTaunts();
			StartCoroutine(m_PlayStartingTaunts);
		}
		Player friendlyPlayer = GameState.Get().GetFriendlySidePlayer();
		friendlyPlayerGoesFirst = friendlyPlayer.HasTag(GAME_TAG.FIRST_PLAYER);
		GetStartingLists();
		if (m_startingCards.Count == 0)
		{
			SkipCardChoosing();
		}
		foreach (Card startingCard in m_startingCards)
		{
			startingCard.GetActor().SetActorState(ActorStateType.CARD_IDLE);
			startingCard.GetActor().TurnOffCollider();
			startingCard.GetActor().GetMeshRenderer().gameObject.layer = 8;
			startingCard.GetActor().m_nameTextMesh.UpdateNow();
		}
		float zoneWidth = startingHandZone.GetComponent<Collider>().bounds.size.x;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			zoneWidth *= 0.55f;
		}
		float spaceForEachCard = zoneWidth / (float)m_startingCards.Count;
		float spaceForEachCardPre4th = zoneWidth / (float)(m_startingCards.Count + 1);
		float spacingToUse = spaceForEachCardPre4th;
		float leftSideOfZone = startingHandZone.transform.position.x - zoneWidth / 2f;
		float rightSideOfZone = startingHandZone.transform.position.x + zoneWidth / 2f;
		float timingBonus = 0.1f;
		int numCardsToDealExcludingBonusCard = m_startingCards.Count;
		if (!friendlyPlayerGoesFirst)
		{
			numCardsToDealExcludingBonusCard = m_bonusCardIndex;
			spacingToUse = spaceForEachCard;
		}
		else if (m_startingOppCards.Count > 0)
		{
			m_startingOppCards[m_bonusCardIndex].SetDoNotSort(on: true);
			if (m_coinCardIndex >= 0)
			{
				m_startingOppCards[m_coinCardIndex].SetDoNotSort(on: true);
			}
		}
		opposingSideHandZone.SetDoNotUpdateLayout(enable: false);
		opposingSideHandZone.UpdateLayout(null, forced: true, 3);
		float cardHeightOffset = 0f;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			cardHeightOffset = 7f;
		}
		float cardZpos = startingHandZone.transform.position.z - 0.3f;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			cardZpos = startingHandZone.transform.position.z - 0.2f;
		}
		yield return StartCoroutine(GameState.Get().GetGameEntity().DoActionsBeforeDealingBaseMulliganCards());
		float xOffset = spacingToUse / 2f;
		for (int i = 0; i < numCardsToDealExcludingBonusCard; i++)
		{
			GameObject topCard = m_startingCards[i].gameObject;
			iTween.Stop(topCard);
			Vector3[] drawPath = new Vector3[3]
			{
				topCard.transform.position,
				new Vector3(topCard.transform.position.x, topCard.transform.position.y + 3.6f, topCard.transform.position.z),
				new Vector3(leftSideOfZone + xOffset, friendlySideHandZone.transform.position.y + cardHeightOffset, cardZpos)
			};
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("path", drawPath);
			moveArgs.Add("time", ANIMATION_TIME_DEAL_CARD);
			moveArgs.Add("easetype", iTween.EaseType.easeInSineOutExpo);
			iTween.MoveTo(topCard, moveArgs);
			iTween.ScaleTo(topCard, FRIENDLY_PLAYER_CARD_SCALE, ANIMATION_TIME_DEAL_CARD);
			Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
			rotateArgs.Add("rotation", new Vector3(0f, 0f, 0f));
			rotateArgs.Add("time", ANIMATION_TIME_DEAL_CARD);
			rotateArgs.Add("delay", ANIMATION_TIME_DEAL_CARD / 16f);
			iTween.RotateTo(topCard, rotateArgs);
			yield return new WaitForSeconds(0.04f);
			SoundManager.Get().LoadAndPlay("FX_GameStart09_CardsOntoTable.prefab:da502e035813b5742a04d2ef4f588255", topCard);
			xOffset += spacingToUse;
			yield return new WaitForSeconds(0.05f + timingBonus);
			timingBonus = 0f;
		}
		if (skipCardChoosing)
		{
			CreateBannerShrinking();
		}
		yield return new WaitForSeconds(1.1f);
		yield return StartCoroutine(GameState.Get().GetGameEntity().DoActionsAfterDealingBaseMulliganCards());
		yield return StartCoroutine(GameState.Get().GetGameEntity().DoActionsBeforeCoinFlip());
		if (coinObject != null)
		{
			Transform coinSpawnLocation = Board.Get().FindBone("MulliganCoinPosition");
			coinObject.transform.position = coinSpawnLocation.position;
			coinObject.transform.localEulerAngles = coinSpawnLocation.localEulerAngles;
			coinObject.SetActive(value: true);
			coinObject.GetComponent<CoinEffect>().DoAnim(friendlyPlayerGoesFirst);
			SoundManager.Get().LoadAndPlay("FX_MulliganCoin03_CoinFlip.prefab:07015cb3f02713a45aa03fc3aa798778", coinObject);
			coinLocation = coinSpawnLocation.position;
			AssetLoader.Get().InstantiatePrefab("MulliganResultText.prefab:0369b435afd2e344db21e58648f8636c", CoinTossTextCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
			yield return new WaitForSeconds(2f);
		}
		yield return StartCoroutine(GameState.Get().GetGameEntity().DoActionsAfterCoinFlip());
		if (!friendlyPlayerGoesFirst)
		{
			GameObject topCard = m_startingCards[m_bonusCardIndex].gameObject;
			Vector3[] drawPath2 = new Vector3[3]
			{
				topCard.transform.position,
				new Vector3(topCard.transform.position.x, topCard.transform.position.y + 3.6f, topCard.transform.position.z),
				new Vector3(leftSideOfZone + xOffset, friendlySideHandZone.transform.position.y + cardHeightOffset, cardZpos)
			};
			Hashtable moveArgs2 = iTweenManager.Get().GetTweenHashTable();
			moveArgs2.Add("path", drawPath2);
			moveArgs2.Add("time", ANIMATION_TIME_DEAL_CARD);
			moveArgs2.Add("easetype", iTween.EaseType.easeInSineOutExpo);
			iTween.MoveTo(topCard, moveArgs2);
			iTween.ScaleTo(topCard, FRIENDLY_PLAYER_CARD_SCALE, ANIMATION_TIME_DEAL_CARD);
			Hashtable rotateArgs2 = iTweenManager.Get().GetTweenHashTable();
			rotateArgs2.Add("rotation", new Vector3(0f, 0f, 0f));
			rotateArgs2.Add("time", ANIMATION_TIME_DEAL_CARD);
			rotateArgs2.Add("delay", ANIMATION_TIME_DEAL_CARD / 8f);
			iTween.RotateTo(topCard, rotateArgs2);
			yield return new WaitForSeconds(0.04f);
			SoundManager.Get().LoadAndPlay("FX_GameStart20_CardDealSingle.prefab:0da693603ca05d846b9cfe26e9f0e3c7", topCard);
		}
		else if (m_startingOppCards.Count > 0)
		{
			m_startingOppCards[m_bonusCardIndex].SetDoNotSort(on: false);
			opposingSideHandZone.UpdateLayout(null, forced: true, 4);
		}
		yield return StartCoroutine(GameState.Get().GetGameEntity().DoActionsAfterDealingBonusCard());
		yield return new WaitForSeconds(1.75f);
		while (GameState.Get().IsBusy())
		{
			yield return null;
		}
		yield return StartCoroutine(GameState.Get().GetGameEntity().DoActionsBeforeSpreadingMulliganCards());
		if (friendlyPlayerGoesFirst)
		{
			xOffset = 0f;
			for (int i2 = m_startingCards.Count - 1; i2 >= 0; i2--)
			{
				GameObject target = m_startingCards[i2].gameObject;
				iTween.Stop(target);
				Hashtable moveArgs3 = iTweenManager.Get().GetTweenHashTable();
				moveArgs3.Add("position", new Vector3(rightSideOfZone - spaceForEachCard - xOffset + spaceForEachCard / 2f, friendlySideHandZone.transform.position.y + cardHeightOffset, cardZpos));
				moveArgs3.Add("time", 14f / 15f);
				moveArgs3.Add("easetype", iTween.EaseType.easeInOutCubic);
				iTween.MoveTo(target, moveArgs3);
				xOffset += spaceForEachCard;
			}
		}
		GameState.Get().GetGameEntity().OnMulliganCardsDealt(m_startingCards);
		yield return new WaitForSeconds(0.6f);
		yield return StartCoroutine(GameState.Get().GetGameEntity().DoActionsAfterSpreadingMulliganCards());
		if (skipCardChoosing)
		{
			if (GameState.Get().IsMulliganPhase())
			{
				if (GameState.Get().IsFriendlySidePlayerTurn())
				{
					TurnStartManager.Get().BeginListeningForTurnEvents();
				}
				m_WaitForOpponentToFinishMulligan = WaitForOpponentToFinishMulligan();
				StartCoroutine(m_WaitForOpponentToFinishMulligan);
			}
			else
			{
				yield return new WaitForSeconds(2f);
				EndMulligan();
			}
			yield break;
		}
		foreach (Card startingCard2 in m_startingCards)
		{
			startingCard2.GetActor().TurnOnCollider();
		}
		CreateBannerIfNeeded();
		CreateTagConditionalVFXs(Board.Get().FindBone("ChoiceBanner").position);
		if (GameState.Get().IsInChoiceMode())
		{
			m_replaceLabels = new List<MulliganReplaceLabel>();
			for (int j = 0; j < m_startingCards.Count; j++)
			{
				InputManager.Get().DoNetworkResponse(m_startingCards[j].GetEntity());
				m_replaceLabels.Add(null);
			}
		}
		while (mulliganButton == null && !GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_REQUIRES_CONFIRMATION))
		{
			yield return null;
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_REQUIRES_CONFIRMATION))
		{
			mulliganButton.transform.position = new Vector3(startingHandZone.transform.position.x, friendlySideHandZone.transform.position.y, myHeroCardActor.transform.position.z);
			mulliganButton.transform.localEulerAngles = new Vector3(90f, 90f, 90f);
			mulliganButton.AddEventListener(UIEventType.RELEASE, OnMulliganButtonReleased);
			mulliganButtonWidget.transform.position = new Vector3(startingHandZone.transform.position.x, friendlySideHandZone.transform.position.y, myHeroCardActor.transform.position.z);
			mulliganButtonWidget.AddEventListener(UIEventType.RELEASE, OnMulliganButtonReleased);
			mulliganCancelConfirmationButtonWidget.transform.position = new Vector3(startingHandZone.transform.position.x + 2f, friendlySideHandZone.transform.position.y, myHeroCardActor.transform.position.z);
			mulliganCancelConfirmationButtonWidget.AddEventListener(UIEventType.RELEASE, OnMulliganCancelConfirmationButtonReleased);
			m_WaitAFrameBeforeSendingEventToMulliganButton = WaitAFrameBeforeSendingEventToMulliganButton(mulliganButton);
			StartCoroutine(m_WaitAFrameBeforeSendingEventToMulliganButton);
			if (!GameMgr.Get().IsSpectator() && !Options.Get().GetBool(Option.HAS_SEEN_MULLIGAN, defaultVal: false) && !GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE) && UserAttentionManager.CanShowAttentionGrabber("MulliganManager.DealStartingCards:" + Option.HAS_SEEN_MULLIGAN))
			{
				innkeeperMulliganDialog = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, new Vector3(155.3f, NotificationManager.DEPTH, 34.5f), GameStrings.Get("VO_INNKEEPER_MULLIGAN_13"), "VO_INNKEEPER_MULLIGAN_13.prefab:3ec6b2e741ac16d4ca519bdfd26d10e3");
				Options.Get().SetBool(Option.HAS_SEEN_MULLIGAN, val: true);
				mulliganButton.GetComponent<Collider>().enabled = false;
			}
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
		{
			while (m_refreshButton == null && GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
			{
				yield return null;
			}
			m_refreshButton.transform.position = new Vector3(mulliganButton.transform.position.x + 2f, mulliganButton.transform.position.y, mulliganButton.transform.position.z);
			m_refreshButton.transform.localEulerAngles = new Vector3(90f, 90f, 90f);
			m_refreshButton.AddEventListener(UIEventType.RELEASE, OnMulliganRefreshButtonReleased);
			m_WaitAFrameBeforeSendingEventToMulliganRefreshButton = WaitAFrameBeforeSendingEventToMulliganButton(m_refreshButton);
			StartCoroutine(m_WaitAFrameBeforeSendingEventToMulliganRefreshButton);
			m_refreshButton.GetComponent<Collider>().enabled = true;
		}
		GameState.Get().GetGameEntity().StartMulliganSoundtracks(soft: true);
		m_waitingForUserInput = true;
		while (innkeeperMulliganDialog != null)
		{
			yield return null;
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_REQUIRES_CONFIRMATION))
		{
			mulliganButton.GetComponent<Collider>().enabled = true;
		}
		if (skipCardChoosing || Cheats.Get().ShouldSkipMulligan())
		{
			BeginDealNewCards();
		}
	}

	private IEnumerator WaitAFrameBeforeSendingEventToMulliganButton(NormalButton button)
	{
		yield return null;
		button.m_button.GetComponent<PlayMakerFSM>().SendEvent("Birth");
	}

	public bool IsMulliganTimerActive()
	{
		return m_mulliganTimer != null;
	}

	public bool IsMulliganTimerLeftExceedBaconRerollButtonCutoff()
	{
		if (m_mulliganTimer != null)
		{
			return m_mulliganTimer.ComputeCountdownRemainingSec() <= MulliganTimer.m_baconRerollButtonCutOffSecond;
		}
		return false;
	}

	public void OnMulliganTimerExceedBaconRerollButtonCutoff()
	{
		EnableMulliganCardRefreshButton(enable: false);
	}

	private void BeginMulliganCountdown(float endTimeStamp)
	{
		if (!m_waitingForUserInput && !GameState.Get().GetBooleanGameOption(GameEntityOption.ALWAYS_SHOW_MULLIGAN_TIMER))
		{
			return;
		}
		if (m_mulliganTimer == null)
		{
			GameObject newTimerObject = UnityEngine.Object.Instantiate(mulliganTimerPrefab);
			m_mulliganTimer = newTimerObject.GetComponent<MulliganTimer>();
			if (m_mulliganTimer == null)
			{
				UnityEngine.Object.Destroy(newTimerObject);
				return;
			}
		}
		m_mulliganTimer.SetEndTime(endTimeStamp);
	}

	private void StopMulliganCountdown()
	{
		DestroyMulliganTimer();
	}

	public GameObject GetMulliganBanner()
	{
		return mulliganChooseBanner;
	}

	public GameObject GetMulliganButton()
	{
		if (mulliganButton != null)
		{
			return mulliganButton.gameObject;
		}
		return null;
	}

	public GameObject GetMulliganRefreshButton()
	{
		if (m_refreshButton != null)
		{
			return m_refreshButton.gameObject;
		}
		return null;
	}

	public Vector3 GetMulliganTimerPosition()
	{
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_TIMER_HAS_ALTERNATE_POSITION))
		{
			return GameState.Get().GetGameEntity().GetMulliganTimerAlternatePosition();
		}
		if (!(mulliganButton != null))
		{
			if (!(m_mulliganTimer != null))
			{
				return new Vector3(0f, 0f, 0f);
			}
			return m_mulliganTimer.transform.position;
		}
		return mulliganButton.transform.position;
	}

	private void CoinTossTextCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		coinTossText = go;
		RenderUtils.SetAlpha(go, 1f);
		go.transform.position = coinLocation + new Vector3(0f, 0f, -1f);
		go.transform.eulerAngles = new Vector3(90f, 0f, 0f);
		UberText componentInChildren = go.transform.GetComponentInChildren<UberText>();
		string tossText = ((!friendlyPlayerGoesFirst) ? GameStrings.Get("GAMEPLAY_COIN_TOSS_LOST") : GameStrings.Get("GAMEPLAY_COIN_TOSS_WON"));
		componentInChildren.Text = tossText;
		GameState.Get().GetGameEntity().NotifyOfCoinFlipResult();
		m_AnimateCoinTossText = AnimateCoinTossText();
		StartCoroutine(m_AnimateCoinTossText);
	}

	private IEnumerator AnimateCoinTossText()
	{
		yield return new WaitForSeconds(1.8f);
		if (!(coinTossText == null))
		{
			iTween.FadeTo(coinTossText, 1f, 0.25f);
			iTween.MoveTo(coinTossText, coinTossText.transform.position + new Vector3(0f, 0.5f, 0f), 2f);
			yield return new WaitForSeconds(1.9f);
			while (GameState.Get().IsBusy())
			{
				yield return null;
			}
			if (!(coinTossText == null))
			{
				iTween.FadeTo(coinTossText, 0f, 1f);
				yield return new WaitForSeconds(0.1f);
				UnityEngine.Object.Destroy(coinTossText);
			}
		}
	}

	private MulliganReplaceLabel CreateNewUILabelAtCardPosition(MulliganReplaceLabel prefab, int cardPosition)
	{
		MulliganReplaceLabel newButton = UnityEngine.Object.Instantiate(prefab);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			newButton.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
			newButton.transform.position = new Vector3(m_startingCards[cardPosition].transform.position.x, m_startingCards[cardPosition].transform.position.y + 0.1f, m_startingCards[cardPosition].transform.position.z - 1.1f);
		}
		else
		{
			newButton.transform.position = new Vector3(m_startingCards[cardPosition].transform.position.x, m_startingCards[cardPosition].transform.position.y + 0.1f, m_startingCards[cardPosition].transform.position.z - startingHandZone.GetComponent<Collider>().bounds.size.z / 2.6f);
		}
		return newButton;
	}

	public void SetAllMulliganCardsToHold()
	{
		foreach (Card card in friendlySideHandZone.GetCards())
		{
			GameState.Get().AddChosenEntity(card.GetEntity());
		}
	}

	private void ToggleHoldState(int startingCardsIndex, bool forceDisable = false, bool forceEnable = false)
	{
		if (!GameState.Get().IsInChoiceMode() || startingCardsIndex >= m_startingCards.Count)
		{
			return;
		}
		if (forceDisable && forceEnable)
		{
			forceEnable = false;
		}
		if ((!forceDisable || (forceDisable && m_handCardsMarkedForReplace[startingCardsIndex])) && (!forceEnable || (forceEnable && !m_handCardsMarkedForReplace[startingCardsIndex])) && !InputManager.Get().DoNetworkResponse(m_startingCards[startingCardsIndex].GetEntity()))
		{
			return;
		}
		bool oldValue = m_handCardsMarkedForReplace[startingCardsIndex];
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
		{
			if (forceDisable)
			{
				m_handCardsMarkedForReplace[startingCardsIndex] = false;
			}
			else
			{
				m_handCardsMarkedForReplace[startingCardsIndex] = !m_handCardsMarkedForReplace[startingCardsIndex];
			}
			if (!m_handCardsMarkedForReplace[startingCardsIndex])
			{
				SoundManager.Get().LoadAndPlay("GM_ChatWarning.prefab:41baa28576a71664eabd8712a198b67f");
				if (m_xLabels != null && m_xLabels[startingCardsIndex] != null)
				{
					UnityEngine.Object.Destroy(m_xLabels[startingCardsIndex]);
				}
				UnityEngine.Object.Destroy(m_replaceLabels[startingCardsIndex].gameObject);
			}
			else
			{
				SoundManager.Get().LoadAndPlay("HeroDropItem1.prefab:587232e6704b20942af1205d00cfc0f9");
				if (m_xLabels != null && m_xLabels[startingCardsIndex] != null)
				{
					UnityEngine.Object.Destroy(m_xLabels[startingCardsIndex]);
				}
				GameObject newX = UnityEngine.Object.Instantiate(mulliganXlabelPrefab);
				newX.transform.position = m_startingCards[startingCardsIndex].transform.position;
				newX.transform.rotation = m_startingCards[startingCardsIndex].transform.rotation;
				if (m_xLabels != null)
				{
					m_xLabels[startingCardsIndex] = newX;
				}
				if (m_replaceLabels != null)
				{
					m_replaceLabels[startingCardsIndex] = CreateNewUILabelAtCardPosition(mulliganReplaceLabelPrefab, startingCardsIndex);
				}
			}
		}
		else
		{
			if (forceDisable)
			{
				m_handCardsMarkedForReplace[startingCardsIndex] = false;
			}
			else if (forceEnable)
			{
				m_handCardsMarkedForReplace[startingCardsIndex] = true;
			}
			else
			{
				m_handCardsMarkedForReplace[startingCardsIndex] = !m_handCardsMarkedForReplace[startingCardsIndex];
			}
			if (oldValue != m_handCardsMarkedForReplace[startingCardsIndex])
			{
				if (!m_handCardsMarkedForReplace[startingCardsIndex])
				{
					SoundManager.Get().LoadAndPlay("GM_ChatWarning.prefab:41baa28576a71664eabd8712a198b67f");
				}
				else
				{
					SoundManager.Get().LoadAndPlay("HeroDropItem1.prefab:587232e6704b20942af1205d00cfc0f9");
				}
			}
			bool anySelected = false;
			bool[] handCardsMarkedForReplace = m_handCardsMarkedForReplace;
			for (int i = 0; i < handCardsMarkedForReplace.Length; i++)
			{
				if (handCardsMarkedForReplace[i])
				{
					anySelected = true;
					break;
				}
			}
			if (mulliganButton != null)
			{
				mulliganButton.SetEnabled(anySelected);
			}
			if (mulliganButtonWidget != null)
			{
				mulliganButtonWidget.SetEnabled(anySelected);
			}
			if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
			{
				GameState.Get().GetGameEntity().ToggleAlternateMulliganActorHighlight(m_startingCards[startingCardsIndex], m_handCardsMarkedForReplace[startingCardsIndex] && !m_handCardsMarkedForTentativeConfirm[startingCardsIndex]);
				GameState.Get().GetGameEntity().ToggleAlternateMulliganActorConfirmHighlight(m_startingCards[startingCardsIndex], m_handCardsMarkedForTentativeConfirm[startingCardsIndex]);
			}
		}
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_REQUIRES_CONFIRMATION))
		{
			BeginDealNewCards();
		}
	}

	private void DestroyXobjects()
	{
		if (m_xLabels != null)
		{
			for (int i = 0; i < m_xLabels.Length; i++)
			{
				UnityEngine.Object.Destroy(m_xLabels[i]);
			}
			m_xLabels = null;
		}
	}

	private void DestroyChooseBanner()
	{
		if (!(mulliganChooseBanner == null))
		{
			UnityEngine.Object.Destroy(mulliganChooseBanner);
		}
	}

	private GameObject GetChooseBannerPrefab()
	{
		if (m_overrideMulliganChooseBannerPrefab == null)
		{
			OverrideChooseBannerPrefab();
		}
		if (!(m_overrideMulliganChooseBannerPrefab != null))
		{
			return mulliganChooseBannerPrefab;
		}
		return m_overrideMulliganChooseBannerPrefab;
	}

	private void OverrideChooseBannerPrefab()
	{
		m_overrideMulliganChooseBannerPrefab = null;
		if (GameState.Get() == null)
		{
			return;
		}
		int currentPriority = -1;
		for (int i = 0; i < tagConditionalVFXPrefabs.Count; i++)
		{
			TagConditionalVFX current = tagConditionalVFXPrefabs[i];
			if (GameState.Get().GetGameEntity().GetTag(current.m_requiredTag) != 0 && current.m_bannerReplacementPrefabPriority > currentPriority)
			{
				m_overrideMulliganChooseBannerPrefab = current.m_bannerReplacementPrefab;
				currentPriority = current.m_bannerReplacementPrefabPriority;
			}
		}
	}

	private void CreateTagConditionalVFXs(Vector3 position)
	{
		if (GameState.Get() == null)
		{
			return;
		}
		m_tagConditionalVFXs = new List<GameObject>();
		for (int i = 0; i < tagConditionalVFXPrefabs.Count; i++)
		{
			TagConditionalVFX current = tagConditionalVFXPrefabs[i];
			if (GameState.Get().GetGameEntity().GetTag(current.m_requiredTag) != 0)
			{
				GameObject tagVFX = UnityEngine.Object.Instantiate(current.m_VFXPrefab, position, Quaternion.identity);
				m_tagConditionalVFXs.Add(tagVFX);
			}
		}
	}

	private void DestroyTagConditionalVFXs()
	{
		if (m_tagConditionalVFXs == null)
		{
			return;
		}
		for (int i = 0; i < m_tagConditionalVFXs.Count; i++)
		{
			if (m_tagConditionalVFXs[i] != null)
			{
				UnityEngine.Object.Destroy(m_tagConditionalVFXs[i]);
			}
		}
		m_tagConditionalVFXs.Clear();
	}

	private void DestroyDetailLabel()
	{
		if (mulliganDetailLabel != null)
		{
			UnityEngine.Object.Destroy(mulliganDetailLabel);
			mulliganDetailLabel = null;
		}
	}

	private void DestroyAnomalyIcon()
	{
		if (anomalyIcon != null)
		{
			UnityEngine.Object.Destroy(anomalyIcon);
			anomalyIcon = null;
		}
	}

	private void DestroyMulliganTimer()
	{
		if (!(m_mulliganTimer == null))
		{
			m_mulliganTimer.SelfDestruct();
			m_mulliganTimer = null;
		}
	}

	public void ToggleHoldState(Actor toggleActor)
	{
		bool anySelected = false;
		List<Actor> fakeCards = new List<Actor>(fakeCardsOnLeft.Count + fakeCardsOnRight.Count);
		fakeCards.AddRange(fakeCardsOnLeft);
		fakeCards.AddRange(fakeCardsOnRight);
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
		{
			foreach (Actor fakeCard in fakeCards)
			{
				if (toggleActor == fakeCard)
				{
					anySelected = GameState.Get().GetGameEntity().ToggleAlternateMulliganActorHighlight(fakeCard, null);
				}
				else
				{
					GameState.Get().GetGameEntity().ToggleAlternateMulliganActorHighlight(fakeCard, false);
				}
			}
		}
		if (anySelected)
		{
			for (int i = 0; i < m_startingCards.Count; i++)
			{
				ToggleHoldState(i, forceDisable: true);
			}
			if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
			{
				if (mulliganButtonWidget != null)
				{
					mulliganButtonWidget.SetEnabled(active: false);
					mulliganButtonWidget.gameObject.SetActive(value: false);
				}
				Network.Get().SendMulliganChooseOneTentativeSelect(0, isConfirmation: false);
			}
			else if (mulliganButton != null)
			{
				mulliganButton.SetEnabled(enabled: false);
				mulliganButton.gameObject.SetActive(value: false);
			}
			if (conditionalHelperTextLabel != null)
			{
				conditionalHelperTextLabel.gameObject.SetActive(value: true);
			}
			ShowSeasonPassPurchaseFTUEIfNeeded();
			return;
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
		{
			if (mulliganButtonWidget != null)
			{
				mulliganButtonWidget.gameObject.SetActive(value: true);
				mulliganButtonWidget.SetEnabled(active: false);
			}
		}
		else if (mulliganButton != null)
		{
			mulliganButton.gameObject.SetActive(value: true);
		}
		if (conditionalHelperTextLabel != null)
		{
			conditionalHelperTextLabel.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (BnetBar.Get().TryGetRelevantCurrencyFrame(CurrencyType.BG_TOKEN, out var bgTokenFrame) && Mathf.Abs(bgTokenFrame.gameObject.transform.position.x - m_lastBnetBarX) > 0.001f)
		{
			m_lastBnetBarX = bgTokenFrame.gameObject.transform.position.x;
			RepositionNotificationsIfNeeded();
		}
	}

	private void RepositionNotificationsIfNeeded()
	{
		if (m_RerollTokenFTUENotification != null)
		{
			Transform updatedTransform = CalculateRerollTokenFTUETransform();
			if (updatedTransform != null)
			{
				Camera unityCamera = ((SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY) ? Box.Get().GetBoxCamera().GetComponent<Camera>() : BoardCameras.Get().GetComponentInChildren<Camera>());
				Vector3 popupPosition = OverlayUI.Get().GetRelativePosition(updatedTransform.position, unityCamera, OverlayUI.Get().m_heightScale.m_Center);
				m_RerollTokenFTUENotification.gameObject.transform.localPosition = popupPosition;
			}
		}
	}

	private int GetStartingCardIndexOfEntity(int entityId)
	{
		for (int i = 0; i < m_startingCards.Count; i++)
		{
			if (m_startingCards[i].GetEntity().GetEntityId() == entityId)
			{
				return i;
			}
		}
		return -1;
	}

	private void SetHoldState(int entityId, bool isConfirmation)
	{
		int entityIndex = GetStartingCardIndexOfEntity(entityId);
		bool anyTentativeConfirmed = false;
		if (entityIndex == -1)
		{
			int index = (isConfirmation ? GetStartingCardIndexOfEntity(tentativeConfirmedChooseOneEntityId) : GetStartingCardIndexOfEntity(tentativeFallbackChooseOneEntityId));
			if (isConfirmation && tentativeConfirmedChooseOneEntityId != tentativeFallbackChooseOneEntityId)
			{
				tentativeConfirmedChooseOneEntityId = 0;
				for (int i = 0; i < m_startingCards.Count; i++)
				{
					m_handCardsMarkedForTentativeConfirm[i] = false;
				}
				if (index >= 0)
				{
					ToggleHoldState(index, forceDisable: true);
				}
			}
			else if (!isConfirmation)
			{
				tentativeFallbackChooseOneEntityId = 0;
				if (index >= 0)
				{
					ToggleHoldState(index, forceDisable: true);
				}
			}
		}
		else
		{
			if (isConfirmation)
			{
				tentativeConfirmedChooseOneEntityId = entityId;
				m_handCardsMarkedForTentativeConfirm[entityIndex] = true;
			}
			else
			{
				tentativeFallbackChooseOneEntityId = entityId;
			}
			ToggleHoldState(entityIndex, forceDisable: false, forceEnable: true);
			for (int j = 0; j < m_startingCards.Count; j++)
			{
				if (j != entityIndex)
				{
					ToggleHoldState(j, forceDisable: true);
				}
				if (m_handCardsMarkedForTentativeConfirm[j])
				{
					anyTentativeConfirmed = true;
				}
			}
		}
		if (mulliganCancelConfirmationButtonWidget != null)
		{
			mulliganCancelConfirmationButtonWidget.gameObject.SetActive(anyTentativeConfirmed);
		}
		if (anyTentativeConfirmed)
		{
			if (mulliganButtonWidget != null)
			{
				mulliganButtonWidget.SetEnabled(active: false);
				mulliganButtonWidget.SetText(GameStrings.Get("GAMEPLAY_MULLIGAN_WAITING_CONFIRMATION"));
			}
			return;
		}
		mulliganButtonWidget.SetEnabled(active: true);
		string heroCardConfirmButtonTextKey = GameState.Get().GetGameEntity().GetMultiStepMulliganConfirmButtonText(GameState.Get().GetEntity(tentativeFallbackChooseOneEntityId));
		if (heroCardConfirmButtonTextKey != null)
		{
			mulliganButtonWidget.SetText(GameStrings.Get(heroCardConfirmButtonTextKey));
		}
		else
		{
			mulliganButtonWidget.SetText(GameStrings.Get("GLOBAL_CONFIRM"));
		}
	}

	private void ShowSeasonPassPurchaseFTUEIfNeeded()
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_BG_SEASON_PASS_POPUP, out long hasSeenBGSeasonPassFTUE);
		if (hasSeenBGSeasonPassFTUE == 0L)
		{
			DialogManager.Get().ClearAllImmediatelyDontDestroy();
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_SEASONPASS_FTUE_HEADER"),
				m_text = GameStrings.Get("GLUE_SEASONPASS_FTUE_BODY"),
				m_confirmText = GameStrings.Get("GLUE_SEASONPASS_FTUE_CONFIRM"),
				m_showAlertIcon = false,
				m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle,
				m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
				m_responseCallback = OnBGSeasonPassFTUEPopupResponse
			};
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_BG_SEASON_PASS_POPUP, 1L));
			DialogManager.Get().ShowPopup(info);
		}
	}

	private void OnBGSeasonPassFTUEPopupResponse(AlertPopup.Response response, object userData)
	{
		if (response != AlertPopup.Response.CANCEL)
		{
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_BG_SEASON_PASS_POPUP, 2L));
		}
	}

	public void ToggleHoldState(Card toggleCard)
	{
		Entity entity = toggleCard.GetEntity();
		if (entity != null && entity.HasTag(GAME_TAG.BACON_LOCKED_MULLIGAN_HERO))
		{
			for (int i = 0; i < m_startingCards.Count; i++)
			{
				ToggleHoldState(i, forceDisable: true);
			}
			if (mulliganButtonWidget != null)
			{
				mulliganButtonWidget.SetEnabled(active: false);
				mulliganButtonWidget.gameObject.SetActive(value: false);
			}
			Network.Get().SendMulliganChooseOneTentativeSelect(0, isConfirmation: false);
			if (conditionalHelperTextLabel != null)
			{
				conditionalHelperTextLabel.gameObject.SetActive(value: true);
			}
			ShowSeasonPassPurchaseFTUEIfNeeded();
			return;
		}
		bool anySelected = false;
		bool anyTentativeConfirmed = false;
		bool mulliganIsChooseOne = GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE);
		int selectedEntityId = 0;
		for (int j = 0; j < m_startingCards.Count; j++)
		{
			if (m_startingCards[j] == toggleCard)
			{
				ToggleHoldState(j);
				selectedEntityId = m_startingCards[j].GetEntity().GetEntityId();
			}
			else if (mulliganIsChooseOne)
			{
				ToggleHoldState(j, forceDisable: true);
			}
			anySelected |= m_handCardsMarkedForReplace[j];
			anyTentativeConfirmed |= m_handCardsMarkedForTentativeConfirm[j];
		}
		List<Actor> fakeCards = new List<Actor>(fakeCardsOnLeft.Count + fakeCardsOnRight.Count);
		fakeCards.AddRange(fakeCardsOnLeft);
		fakeCards.AddRange(fakeCardsOnRight);
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
		{
			Network.Get().SendMulliganChooseOneTentativeSelect(selectedEntityId, isConfirmation: false);
			mulliganButtonWidget.gameObject.SetActive(value: true);
		}
		else
		{
			if (mulliganButton != null)
			{
				mulliganButton.gameObject.SetActive(value: true);
			}
			if (m_refreshButton != null)
			{
				m_refreshButton.gameObject.SetActive(value: true);
			}
		}
		if (conditionalHelperTextLabel != null)
		{
			conditionalHelperTextLabel.gameObject.SetActive(value: false);
		}
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
		{
			return;
		}
		foreach (Actor fakeCard in fakeCards)
		{
			GameState.Get().GetGameEntity().ToggleAlternateMulliganActorHighlight(fakeCard, false);
		}
	}

	public void ServerHasDealtReplacementCards(bool isFriendlySide)
	{
		if (isFriendlySide)
		{
			friendlyPlayerHasReplacementCards = true;
			if (GameState.Get().IsFriendlySidePlayerTurn())
			{
				TurnStartManager.Get().BeginListeningForTurnEvents();
			}
		}
		else
		{
			opponentPlayerHasReplacementCards = true;
		}
	}

	public void AutomaticContinueMulligan(bool destroyTimer = false)
	{
		if (m_waitingForUserInput)
		{
			if (mulliganButton != null)
			{
				mulliganButton.SetEnabled(enabled: false);
			}
			if (mulliganButtonWidget != null)
			{
				mulliganButtonWidget.SetEnabled(active: false);
			}
			if (m_refreshButton != null)
			{
				m_refreshButton.SetEnabled(enabled: false);
			}
			if (mulliganCancelConfirmationButtonWidget != null)
			{
				mulliganCancelConfirmationButtonWidget.SetEnabled(active: false);
			}
			ShowMulliganCardRefreshButton(show: false);
			if (destroyTimer)
			{
				DestroyMulliganTimer();
			}
			BeginDealNewCards();
		}
		else
		{
			SkipCardChoosing();
		}
	}

	private Entity GetCurrentSelectedChooseOneEntity()
	{
		Entity entity = null;
		for (int cardIndex = 0; cardIndex < m_startingCards.Count; cardIndex++)
		{
			if (m_handCardsMarkedForReplace[cardIndex])
			{
				return m_startingCards[cardIndex].GetEntity();
			}
		}
		return entity;
	}

	public void RequestHeroReroll(Entity entity)
	{
		if (entity != null)
		{
			int entityIndex = GetStartingCardIndexOfEntity(entity.GetEntityId());
			if (m_handCardsMarkedForReplace[entityIndex])
			{
				ToggleHoldState(entityIndex);
			}
			HideRerollPopups();
			Network.Get().RequestReplaceBattlegroundsMulliganHero(entity.GetEntityId());
		}
	}

	public void ReplaceMulliganHero(ReplaceBattlegroundMulliganHero packet)
	{
		if (GameMgr.Get().IsBattlegroundDuoGame() && TeammateBoardViewer.Get() != null)
		{
			TeammateBoardViewer.Get().UpdateTeammateMulliganHero(packet);
		}
	}

	private void ShowMulliganCardRefreshButton(bool show)
	{
		if (m_startingCards == null)
		{
			return;
		}
		foreach (Card card in m_startingCards)
		{
			if (card == null)
			{
				continue;
			}
			Actor actor = card.GetActor();
			if (!(actor == null))
			{
				PlayerLeaderboardMainCardActor baconHeroActor = actor.GetComponent<PlayerLeaderboardMainCardActor>();
				if (baconHeroActor != null)
				{
					baconHeroActor.SetShowHeroRerollButton(show, null);
				}
			}
		}
	}

	private void EnableMulliganCardRefreshButton(bool enable)
	{
		foreach (Card card in m_startingCards)
		{
			if (card == null)
			{
				continue;
			}
			Actor actor = card.GetActor();
			if (!(actor == null))
			{
				PlayerLeaderboardMainCardActor baconHeroActor = actor.GetComponent<PlayerLeaderboardMainCardActor>();
				if (baconHeroActor != null)
				{
					baconHeroActor.SetShowHeroRerollButton(baconHeroActor.ShowHeroRerollButton(), enable);
				}
			}
		}
	}

	private void OnMulliganButtonReleased(UIEvent e)
	{
		if (!InputManager.Get().PermitDecisionMakingInput())
		{
			return;
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
		{
			Entity selectedEntity = GetCurrentSelectedChooseOneEntity();
			if (GameState.Get().GetGameEntity().HeroRequiresDoubleConfirmation(selectedEntity))
			{
				Network.Get().SendMulliganChooseOneTentativeSelect(selectedEntity.GetEntityId(), isConfirmation: true);
				return;
			}
		}
		if (mulliganButton != null)
		{
			mulliganButton.SetEnabled(enabled: false);
		}
		if (mulliganButtonWidget != null)
		{
			mulliganButtonWidget.SetEnabled(active: false);
		}
		ShowMulliganCardRefreshButton(show: false);
		BeginDealNewCards();
	}

	private void OnMulliganCancelConfirmationButtonReleased(UIEvent e)
	{
		if (InputManager.Get().PermitDecisionMakingInput() && GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
		{
			Network.Get().SendMulliganChooseOneTentativeSelect(0, isConfirmation: true);
		}
	}

	private void BeginDealNewCards(bool isBGRefresing = false)
	{
		if (!isBGRefresing)
		{
			GameState.Get().GetGameEntity().OnMulliganBeginDealNewCards();
		}
		ShowMulliganCardRefreshButton(show: false);
		if (m_waitingForUserInput)
		{
			m_waitingForUserInput = isBGRefresing;
			m_RemoveOldCardsAnimation = RemoveOldCardsAnimation(isBGRefresing);
			StartCoroutine(m_RemoveOldCardsAnimation);
		}
		EnableDamageCapFX(!isBGRefresing);
	}

	private void OnMulliganRefreshButtonReleased(UIEvent e)
	{
		if (InputManager.Get().PermitDecisionMakingInput())
		{
			if (m_refreshButton != null)
			{
				m_refreshButton.gameObject.SetActive(value: false);
			}
			List<int> herosIDsToKeep = new List<int>();
			Entity selectedEntity = GetCurrentSelectedChooseOneEntity();
			if (selectedEntity != null)
			{
				herosIDsToKeep.Add(selectedEntity.GetEntityId());
			}
			Network.Get().RequestReplaceAllBattlegroundsMulliganHeroExcept(herosIDsToKeep);
		}
	}

	private void RefreshBGHeroes()
	{
		if (InputManager.Get().PermitDecisionMakingInput())
		{
			Network.Get().SendPreRefreshBGHeroes();
			GameState.Get().ClearFriendlyChoicesList();
			if (m_startingCards.Count > 0 && InputManager.Get().DoNetworkResponse(m_startingCards[0].GetEntity()))
			{
				GameState.Get().SendChoices();
				GameState.Get().ClearFriendlyChoicesList();
				ClearHandCardsMarkedForReplace();
			}
		}
	}

	private void ClearHandCardsMarkedForReplace()
	{
		for (int i = 0; i < m_handCardsMarkedForReplace.Length; i++)
		{
			m_handCardsMarkedForReplace[i] = false;
		}
	}

	private IEnumerator RemoveOldCardsAnimation(bool isBGRefreshing = false)
	{
		m_waitingForUserInput = isBGRefreshing;
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_HAS_HERO_LOBBY))
		{
			DestroyMulliganTimer();
		}
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
		{
			SoundManager.Get().LoadAndPlay("FX_GameStart28_CardDismissWoosh2_v2.prefab:6eb21cb332351ea419772cb5ae32772a");
			DestroyXobjects();
		}
		else
		{
			SoundManager.Get().LoadAndPlay("BG_SelectHero.prefab:40cb8c418fca5f44391df4df2e9660cd");
		}
		Vector3 mulliganedCardsPosition = Board.Get().FindBone("MulliganedCardsPosition").position;
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_HAS_HERO_LOBBY))
		{
			DestroyChooseBanner();
			DestroyDetailLabel();
			DestroyTagConditionalVFXs();
		}
		else
		{
			m_UpdateChooseBanner = UpdateChooseBanner();
			StartCoroutine(m_UpdateChooseBanner);
		}
		if (!UniversalInputManager.UsePhoneUI || GameState.Get().GetBooleanGameOption(GameEntityOption.SUPPRESS_CLASS_NAMES))
		{
			Gameplay.Get().RemoveClassNames();
		}
		foreach (Card startingCard in m_startingCards)
		{
			startingCard.GetActor().SetActorState(ActorStateType.CARD_IDLE);
			startingCard.GetActor().ToggleForceIdle(bOn: true);
			startingCard.GetActor().TurnOffCollider();
		}
		hisHeroCardActor.SetActorState(ActorStateType.CARD_IDLE);
		hisHeroCardActor.ToggleForceIdle(bOn: true);
		Card friendlyHeroPower = GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard();
		if (friendlyHeroPower != null && friendlyHeroPower.GetActor() != null)
		{
			friendlyHeroPower.GetActor().SetActorState(ActorStateType.CARD_IDLE);
			friendlyHeroPower.GetActor().ToggleForceIdle(bOn: true);
		}
		if (!isBGRefreshing)
		{
			if (m_RemoveUIButtons != null)
			{
				StopCoroutine(m_RemoveUIButtons);
			}
			m_RemoveUIButtons = RemoveUIButtons();
			StartCoroutine(m_RemoveUIButtons);
		}
		float TO_DECK_ANIMATION_TIME = 1.5f;
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE))
		{
			for (int i = 0; i < m_startingCards.Count; i++)
			{
				if (m_handCardsMarkedForReplace[i])
				{
					GameObject cardObject = m_startingCards[i].gameObject;
					Vector3[] drawPath = new Vector3[4]
					{
						cardObject.transform.position,
						new Vector3(cardObject.transform.position.x + 2f, cardObject.transform.position.y - 1.7f, cardObject.transform.position.z),
						new Vector3(mulliganedCardsPosition.x, mulliganedCardsPosition.y, mulliganedCardsPosition.z),
						friendlySideDeck.transform.position
					};
					Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
					moveArgs.Add("path", drawPath);
					moveArgs.Add("time", TO_DECK_ANIMATION_TIME);
					moveArgs.Add("easetype", iTween.EaseType.easeOutCubic);
					iTween.MoveTo(cardObject, moveArgs);
					Animation cardAnim = cardObject.GetComponent<Animation>();
					if (cardAnim == null)
					{
						cardAnim = cardObject.AddComponent<Animation>();
					}
					cardAnim.AddClip(cardAnimatesFromBoardToDeck, "putCardBack");
					cardAnim.Play("putCardBack");
					yield return new WaitForSeconds(0.5f);
				}
			}
		}
		else if (isBGRefreshing)
		{
			for (int i = 0; i < m_startingCards.Count; i++)
			{
				GameObject cardObject2 = m_startingCards[i].gameObject;
				Vector3[] drawPath2 = new Vector3[4]
				{
					cardObject2.transform.position,
					new Vector3(cardObject2.transform.position.x + 2f, cardObject2.transform.position.y - 1.7f, cardObject2.transform.position.z),
					new Vector3(mulliganedCardsPosition.x, mulliganedCardsPosition.y, mulliganedCardsPosition.z),
					friendlySideDeck.transform.position
				};
				Hashtable moveArgs2 = iTweenManager.Get().GetTweenHashTable();
				moveArgs2.Add("path", drawPath2);
				moveArgs2.Add("time", TO_DECK_ANIMATION_TIME);
				moveArgs2.Add("easetype", iTween.EaseType.easeOutCubic);
				iTween.MoveTo(cardObject2, moveArgs2);
				Animation cardAnim2 = cardObject2.GetComponent<Animation>();
				if (cardAnim2 == null)
				{
					cardAnim2 = cardObject2.AddComponent<Animation>();
				}
				cardAnim2.AddClip(cardAnimatesFromBoardToDeck, "putCardBack");
				cardAnim2.Play("putCardBack");
				yield return new WaitForSeconds(0.5f);
			}
		}
		if (isBGRefreshing)
		{
			RefreshBGHeroes();
		}
		else if (!EndTurnButton.Get().IsDisabled)
		{
			InputManager.Get().DoEndTurnButton();
		}
		else
		{
			GameState.Get().SendChoices();
		}
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_IS_CHOOSE_ONE) || isBGRefreshing)
		{
			friendlySideHandZone.AddInputBlocker();
			while (!friendlyPlayerHasReplacementCards)
			{
				yield return null;
			}
			friendlySideHandZone.RemoveInputBlocker();
			SortHand(friendlySideHandZone);
			List<Card> handZoneCards = friendlySideHandZone.GetCards();
			if (!isBGRefreshing)
			{
				foreach (Card card in handZoneCards)
				{
					if (!IsCoinCard(card))
					{
						card.GetActor().SetActorState(ActorStateType.CARD_IDLE);
						card.GetActor().ToggleForceIdle(bOn: true);
						card.GetActor().TurnOffCollider();
					}
				}
			}
			else
			{
				GetStartingLists();
				SetupCardActor(ref m_startingCards);
				while (pendingHeroCount > 0)
				{
					yield return null;
				}
				SetupCardCollider(ref m_startingCards);
			}
			float zoneWidth = startingHandZone.GetComponent<Collider>().bounds.size.x;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				zoneWidth *= 0.55f;
			}
			float spaceForEachCard = zoneWidth / (float)m_startingCards.Count;
			float leftSideOfZone = startingHandZone.transform.position.x - zoneWidth / 2f;
			float xOffset = 0f;
			if (isBGRefreshing)
			{
				int numFakeCardsOnLeft = GameState.Get().GetGameEntity().GetNumberOfFakeMulliganCardsToShowOnLeft(m_startingCards.Count);
				int numFakeCardsOnRight = GameState.Get().GetGameEntity().GetNumberOfFakeMulliganCardsToShowOnRight(m_startingCards.Count);
				spaceForEachCard = zoneWidth / (float)(m_startingCards.Count + numFakeCardsOnLeft + numFakeCardsOnRight);
				xOffset += (float)numFakeCardsOnLeft * spaceForEachCard;
			}
			float cardHeightOffset = 0f;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				cardHeightOffset = 7f;
			}
			float cardZpos = startingHandZone.transform.position.z - 0.3f;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				cardZpos = startingHandZone.transform.position.z - 0.2f;
			}
			for (int i = 0; i < m_startingCards.Count; i++)
			{
				if (m_handCardsMarkedForReplace[i] || isBGRefreshing)
				{
					GameObject topCard = (isBGRefreshing ? m_startingCards[i].gameObject : handZoneCards[i].gameObject);
					iTween.Stop(topCard);
					Hashtable moveArgs3 = iTweenManager.Get().GetTweenHashTable();
					moveArgs3.Add("position", new Vector3(leftSideOfZone + spaceForEachCard + xOffset - spaceForEachCard / 2f, friendlySideHandZone.GetComponent<Collider>().bounds.center.y, startingHandZone.transform.position.z));
					moveArgs3.Add("time", 3f);
					iTween.MoveTo(topCard, moveArgs3);
					Vector3[] drawPath3 = new Vector3[4];
					drawPath3[0] = topCard.transform.position;
					drawPath3[1] = new Vector3(mulliganedCardsPosition.x, mulliganedCardsPosition.y, mulliganedCardsPosition.z);
					drawPath3[3] = new Vector3(leftSideOfZone + spaceForEachCard + xOffset - spaceForEachCard / 2f, friendlySideHandZone.GetComponent<Collider>().bounds.center.y + cardHeightOffset, cardZpos);
					drawPath3[2] = new Vector3(drawPath3[3].x + 2f, drawPath3[3].y - 1.7f, drawPath3[3].z);
					Hashtable moveArgs4 = iTweenManager.Get().GetTweenHashTable();
					moveArgs4.Add("path", drawPath3);
					moveArgs4.Add("time", TO_DECK_ANIMATION_TIME);
					moveArgs4.Add("easetype", iTween.EaseType.easeInCubic);
					iTween.MoveTo(topCard, moveArgs4);
					if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
					{
						iTween.ScaleTo(topCard, GameState.Get().GetGameEntity().GetAlternateMulliganActorScale(), ANIMATION_TIME_DEAL_CARD);
					}
					else
					{
						iTween.ScaleTo(topCard, FRIENDLY_PLAYER_CARD_SCALE, ANIMATION_TIME_DEAL_CARD);
					}
					Animation cardAnim3 = topCard.GetComponent<Animation>();
					if (cardAnim3 == null)
					{
						cardAnim3 = topCard.AddComponent<Animation>();
					}
					string animName = "putCardBack";
					cardAnim3.AddClip(cardAnimatesFromBoardToDeck, animName);
					cardAnim3[animName].normalizedTime = 1f;
					cardAnim3[animName].speed = -1f;
					cardAnim3.Play(animName);
					yield return new WaitForSeconds(0.5f);
					if (topCard != null)
					{
						if (topCard.GetComponent<AudioSource>() == null)
						{
							topCard.AddComponent<AudioSource>();
						}
						SoundManager.Get().LoadAndPlay("FX_GameStart30_CardReplaceSingle.prefab:aa2b215965bf6484da413a795c17e995", topCard);
					}
				}
				xOffset += spaceForEachCard;
			}
			yield return new WaitForSeconds(1f);
			ShuffleDeck();
			yield return new WaitForSeconds(1.5f);
		}
		if (!isBGRefreshing)
		{
			if (opponentPlayerHasReplacementCards && !GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_HAS_HERO_LOBBY))
			{
				EndMulligan();
				yield break;
			}
			m_WaitForOpponentToFinishMulligan = WaitForOpponentToFinishMulligan();
			StartCoroutine(WaitForOpponentToFinishMulligan());
		}
	}

	private IEnumerator UpdateChooseBanner()
	{
		yield break;
	}

	private Actor ShowTeammateHeroActor(Actor yourHeroActor, int teammateId)
	{
		Actor teammateHeroActor = null;
		foreach (Actor value in teammateHeroActors.Values)
		{
			GameObject topCard = value.gameObject.transform.parent.gameObject;
			teammateHeroActor = value;
			value.Show();
			topCard.transform.localScale = new Vector3(1.2f, 1.1f, 1.2f);
			topCard.transform.localRotation = yourHeroActor.transform.parent.localRotation;
			float zoneWidth = startingHandZone.GetComponent<Collider>().bounds.size.x;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				zoneWidth *= 0.55f;
				topCard.transform.localScale = new Vector3(0.9f, 1.1f, 0.9f);
			}
			float spacingToUse = zoneWidth;
			float leftSideOfZone = startingHandZone.transform.position.x - zoneWidth / 2f;
			float cardHeightOffset = 0f;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				cardHeightOffset = 7f;
			}
			float cardZpos = startingHandZone.transform.position.z - 0.3f;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				cardZpos = startingHandZone.transform.position.z - 0.2f;
			}
			float xOffset = 2f * spacingToUse / 3f;
			topCard.transform.position = new Vector3(leftSideOfZone + xOffset, friendlySideHandZone.transform.position.y + cardHeightOffset, cardZpos);
			((PlayerLeaderboardMainCardActor)value).SetAlternateNameTextActive(active: false);
			((PlayerLeaderboardMainCardActor)value).UpdatePlayerNameText(GameState.Get().GetGameEntity().GetBestNameForPlayer(teammateId));
			value.GetComponent<PlayMakerFSM>().SendEvent("SpawnTeammateFromPortal");
		}
		if (teammateHeroActor != null && teammateHeroActor.GetCard() != null)
		{
			teammateHeroActor.GetMeshRenderer().gameObject.layer = 8;
			teammateHeroActor.GetCard().SetAlwaysShowCardsInTooltip(show: true);
			teammateHeroActor.GetCard().SetCardInTooltipDisplaySide(forceLeft: true);
		}
		return teammateHeroActor;
	}

	private IEnumerator WaitForOpponentToFinishMulligan()
	{
		if (!GameMgr.Get().IsBattlegrounds())
		{
			DestroyChooseBanner();
			DestroyDetailLabel();
		}
		else
		{
			HideRerollPopups();
		}
		DestroyTagConditionalVFXs();
		Vector3 mulliganBannerPosition = Board.Get().FindBone("ChoiceBanner").position;
		Vector3 position;
		Vector3 endScale;
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_HAS_HERO_LOBBY))
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				position = new Vector3(mulliganBannerPosition.x, friendlySideHandZone.transform.position.y + 1f, myHeroCardActor.transform.position.z + 6.8f);
				endScale = new Vector3(2.5f, 2.5f, 2.5f);
			}
			else
			{
				position = new Vector3(mulliganBannerPosition.x, friendlySideHandZone.transform.position.y, myHeroCardActor.transform.position.z + 0.4f);
				endScale = new Vector3(1.4f, 1.4f, 1.4f);
			}
		}
		else
		{
			position = mulliganBannerPosition;
			endScale = new Vector3(1.4f, 1.4f, 1.4f);
		}
		if (!GameMgr.Get().IsBattlegrounds())
		{
			mulliganChooseBanner = UnityEngine.Object.Instantiate(GetChooseBannerPrefab(), position, new Quaternion(0f, 0f, 0f, 0f));
			mulliganChooseBanner.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			iTween.ScaleTo(mulliganChooseBanner, endScale, 0.4f);
		}
		CreateTagConditionalVFXs(position);
		Actor yourHeroActor = null;
		Actor teammateHeroActor = null;
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_HAS_HERO_LOBBY))
		{
			GameState.Get().GetGameEntity().GetMulliganWaitingText();
			GameState.Get().GetGameEntity().GetMulliganWaitingSubtitleText();
			while (GameState.Get().GetPlayerInfoMap()[GameState.Get().GetFriendlyPlayerId()].GetPlayerHero() == null)
			{
				string mulliganWaitingText = GameState.Get().GetGameEntity().GetMulliganWaitingText();
				string mulliganWaitingSubtitleText = GameState.Get().GetGameEntity().GetMulliganWaitingSubtitleText();
				SetMulliganBannerText(mulliganWaitingText, mulliganWaitingSubtitleText);
				yield return new WaitForSeconds(0.5f);
			}
			int teammateId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_DUO_TEAMMATE_PLAYER_ID);
			if (m_startingCards.Count == 0)
			{
				m_startingCards.Add(GameState.Get().GetFriendlySidePlayer().GetHeroCard());
				foreach (Card card in m_startingCards)
				{
					card.GetActor().SetActorState(ActorStateType.CARD_IDLE);
					card.GetActor().TurnOffCollider();
					card.GetActor().GetMeshRenderer().gameObject.layer = 8;
					if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
					{
						pendingHeroCount++;
						card.GetActor().gameObject.SetActive(value: false);
						AssetReference assetReference = GameState.Get().GetStringGameOption(GameEntityOption.ALTERNATE_MULLIGAN_ACTOR_NAME);
						if (!AssetLoader.Get().InstantiatePrefab(assetReference, OnHeroActorLoadAttempted, card, AssetLoadingOptions.IgnorePrefabPosition))
						{
							OnHeroActorLoadAttempted(assetReference, null, card);
						}
					}
				}
				while (pendingHeroCount > 0)
				{
					yield return null;
				}
			}
			foreach (Card card2 in choiceHeroActors.Keys)
			{
				if (card2.GetEntity().GetCardId() == GameState.Get().GetPlayerInfoMap()[GameState.Get().GetFriendlyPlayerId()].GetPlayerHero().GetCardId())
				{
					float zoneWidth = startingHandZone.GetComponent<Collider>().bounds.size.x;
					if ((bool)UniversalInputManager.UsePhoneUI)
					{
						zoneWidth *= 0.55f;
					}
					float spacingToUse = zoneWidth;
					float leftSideOfZone = startingHandZone.transform.position.x - zoneWidth / 2f;
					float cardHeightOffset = 0f;
					if ((bool)UniversalInputManager.UsePhoneUI)
					{
						cardHeightOffset = 7f;
					}
					float cardZpos = startingHandZone.transform.position.z - 0.3f;
					if ((bool)UniversalInputManager.UsePhoneUI)
					{
						cardZpos = startingHandZone.transform.position.z - 0.2f;
					}
					float xOffset = spacingToUse / 2f;
					if (GameMgr.Get().IsBattlegroundDuoGame())
					{
						xOffset = spacingToUse / 3f;
					}
					GameObject topCard = card2.gameObject;
					if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
					{
						topCard = choiceHeroActors[card2].gameObject.transform.parent.gameObject;
						yourHeroActor = choiceHeroActors[card2];
						yourHeroActor.GetCard().SetActor(yourHeroActor);
						yourHeroActor.GetCard().GetActor().Show();
						GameState.Get().GetGameEntity().ApplyMulliganActorLobbyStateChanges(yourHeroActor);
						GameState.Get().GetGameEntity().NotifyMulligainHeroSelected(yourHeroActor);
						((PlayerLeaderboardMainCardActor)yourHeroActor).UpdatePlayerNameText(GameState.Get().GetGameEntity().GetBestNameForPlayer(GameState.Get().GetFriendlySidePlayer().GetPlayerId()));
						myHeroCardActor = yourHeroActor;
						m_chosenHero = yourHeroActor.GetCard();
						choiceHeroActors.Values.ForEach(delegate(Actor actor)
						{
							actor.RemovePing();
						});
						myHeroCardActor.BlockPings(block: true);
					}
					iTween.Stop(topCard);
					Vector3[] drawPath = new Vector3[3]
					{
						topCard.transform.position,
						new Vector3(topCard.transform.position.x, topCard.transform.position.y + 3.6f, topCard.transform.position.z),
						new Vector3(leftSideOfZone + xOffset, friendlySideHandZone.transform.position.y + cardHeightOffset, cardZpos)
					};
					Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
					moveArgs.Add("path", drawPath);
					moveArgs.Add("time", ANIMATION_TIME_DEAL_CARD);
					moveArgs.Add("easetype", iTween.EaseType.easeInSineOutExpo);
					moveArgs.Add("oncomplete", "OnHeroDoneMovingAfterSelection");
					moveArgs.Add("oncompletetarget", base.gameObject);
					iTween.MoveTo(topCard, moveArgs);
					if ((bool)UniversalInputManager.UsePhoneUI)
					{
						iTween.ScaleTo(topCard, new Vector3(0.9f, 1.1f, 0.9f), ANIMATION_TIME_DEAL_CARD);
					}
					else
					{
						iTween.ScaleTo(topCard, new Vector3(1.2f, 1.1f, 1.2f), ANIMATION_TIME_DEAL_CARD);
					}
					Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
					rotateArgs.Add("rotation", new Vector3(0f, 0f, 0f));
					rotateArgs.Add("time", ANIMATION_TIME_DEAL_CARD);
					rotateArgs.Add("delay", ANIMATION_TIME_DEAL_CARD / 16f);
					iTween.RotateTo(topCard, rotateArgs);
				}
				else if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
				{
					if (!choiceHeroActors.ContainsKey(card2))
					{
						Debug.LogError("MulliganManager - ChoiceHeroActors doesn't contain card: " + card2.name);
						continue;
					}
					choiceHeroActors[card2].ActivateSpellBirthState(SpellType.DEATH);
					((PlayerLeaderboardMainCardActor)choiceHeroActors[card2]).m_fullSelectionHighlight.SetActive(value: false);
					((PlayerLeaderboardMainCardActor)choiceHeroActors[card2]).m_confirmSelectionHighlight.SetActive(value: false);
				}
				else
				{
					card2.FakeDeath();
				}
			}
			CleanupFakeCards();
			Transform rootTransform = yourHeroActor.gameObject.transform.parent.parent;
			bool heroPowerCreated = false;
			bool teammateActorCreated = false;
			do
			{
				yield return null;
				if (!heroPowerCreated)
				{
					Card friendlyHeroPower = GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard();
					if (friendlyHeroPower != null && friendlyHeroPower.GetActor() != null)
					{
						friendlyHeroPower.GetActor().SetActorState(ActorStateType.CARD_IDLE);
						friendlyHeroPower.GetActor().ToggleForceIdle(bOn: true);
						friendlyHeroPower.GetActor().TurnOffCollider();
						heroPowerCreated = true;
					}
				}
				string mulliganWaitingText = GameState.Get().GetGameEntity().GetMulliganWaitingText();
				string mulliganWaitingSubtitleText = GameState.Get().GetGameEntity().GetMulliganWaitingSubtitleText();
				SetMulliganBannerText(mulliganWaitingText, mulliganWaitingSubtitleText);
				if (!GameState.Get().GetGameEntity().IsTeammateHeroMulliganFinished())
				{
					continue;
				}
				if (!teammateActorCreated)
				{
					Entity teammateEntity = GameState.Get().GetGameEntity().GetFriendlyTeammateHeroEntity();
					if (teammateEntity != null)
					{
						teammateActorCreated = true;
						pendingHeroCount++;
						AssetReference assetReference2 = GameState.Get().GetStringGameOption(GameEntityOption.ALTERNATE_MULLIGAN_LOBBY_ACTOR_NAME);
						object callbackData = teammateEntity.GetCard();
						if (!AssetLoader.Get().InstantiatePrefab(assetReference2, OnTeammateHeroActorLoadAttempted, callbackData, AssetLoadingOptions.IgnorePrefabPosition))
						{
							OnTeammateHeroActorLoadAttempted(assetReference2, null, callbackData);
						}
					}
				}
				if (pendingHeroCount == 0 && teammateActorCreated && teammateHeroActor == null)
				{
					teammateHeroActor = ShowTeammateHeroActor(yourHeroActor, teammateId);
				}
			}
			while (!GameState.Get().GetGameEntity().IsHeroMulliganLobbyFinished() || !GameState.Get().GetGameEntity().IsTeammateHeroMulliganFinished());
			while (pendingHeroCount > 0)
			{
				yield return null;
			}
			if (teammateActorCreated && teammateHeroActor == null)
			{
				teammateHeroActor = ShowTeammateHeroActor(yourHeroActor, teammateId);
			}
			m_teammateActor = teammateHeroActor;
			m_chosenHero.SetAlwaysShowCardsInTooltip(show: false);
			if (m_teammateActor != null && m_teammateActor.GetCard() != null)
			{
				m_teammateActor.GetCard().SetAlwaysShowCardsInTooltip(show: false);
			}
			GameState.Get().GetGameEntity().NotifyHeroMulliganLobbyFinished();
			DisableBaconMulliganElements();
			foreach (SharedPlayerInfo sph in GameState.Get().GetPlayerInfoMap().Values)
			{
				if (sph.GetPlayerId() != GameState.Get().GetFriendlyPlayerId() && sph.GetPlayerId() != teammateId)
				{
					while (sph.GetPlayerHero() == null)
					{
						yield return null;
					}
					pendingHeroCount++;
					AssetReference assetReference3 = GameState.Get().GetStringGameOption(GameEntityOption.ALTERNATE_MULLIGAN_LOBBY_ACTOR_NAME);
					object callbackData2 = sph.GetPlayerHero().GetCard();
					if (!AssetLoader.Get().InstantiatePrefab(assetReference3, OnOpponentHeroActorLoadAttempted, callbackData2, AssetLoadingOptions.IgnorePrefabPosition))
					{
						OnOpponentHeroActorLoadAttempted(assetReference3, null, callbackData2);
					}
				}
			}
			while (pendingHeroCount > 0)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.5f);
			DestroyMulliganTimer();
			DestroyChooseBanner();
			DestroyDetailLabel();
			DestroyAnomalyIcon();
			DestroyTagConditionalVFXs();
			Transform yourHeroRoot = yourHeroActor.transform.parent;
			Vector3 vsPosition = Board.Get().FindBone("VS_Position").position;
			yield return new WaitForSeconds(1f);
			iTween.Stop(yourHeroRoot.gameObject);
			if (teammateHeroActor != null && teammateHeroActor.transform.parent != null)
			{
				iTween.Stop(teammateHeroActor.transform.parent.gameObject);
			}
			List<Actor> opponentActorList = new List<Actor>(opponentHeroActors.Values);
			if (GameMgr.Get().IsBattlegroundDuoGame())
			{
				opponentActorList.Sort((Actor a, Actor b) => a.GetEntity().GetTag(GAME_TAG.BACON_DUO_TEAM_ID).CompareTo(b.GetEntity().GetTag(GAME_TAG.BACON_DUO_TEAM_ID)));
			}
			int HeroLineupPlayerId = 1;
			foreach (Actor opponentHeroCard in opponentActorList)
			{
				opponentHeroCard.gameObject.transform.parent = rootTransform;
				opponentHeroCard.gameObject.transform.localScale = new Vector3(1.0506f, 1.0506f, 1.0506f);
				opponentHeroCard.gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
				Vector3 heroCardStartingPosition = Board.Get().FindBone("HeroSpawnLineUp0" + HeroLineupPlayerId++).position;
				opponentHeroCard.gameObject.transform.position = heroCardStartingPosition;
				((PlayerLeaderboardMainCardActor)opponentHeroCard).SetAlternateNameTextActive(active: false);
				SharedPlayerInfo sph2 = GetPlayerForCard(opponentHeroCard.GetCard());
				if (sph2 != null)
				{
					((PlayerLeaderboardMainCardActor)opponentHeroCard).UpdatePlayerNameText(GameState.Get().GetGameEntity().GetBestNameForPlayer(sph2.GetPlayerId()));
				}
			}
			yourHeroActor.transform.parent = null;
			yourHeroRoot.position = new Vector3(-7.7726f, 0.0055918f, -8.054f);
			yourHeroRoot.localScale = new Vector3(1.134f, 1.134f, 1.134f);
			yourHeroActor.transform.parent = yourHeroRoot;
			if (teammateHeroActor != null)
			{
				Transform teammateHeroRoot = teammateHeroActor.transform.parent;
				teammateHeroActor.transform.parent = null;
				if (teammateHeroRoot != null)
				{
					teammateHeroRoot.position = new Vector3(-7.7726f, 0.0055918f, -8.054f);
					teammateHeroRoot.localScale = new Vector3(1.134f, 1.134f, 1.134f);
				}
				teammateHeroActor.transform.parent = teammateHeroRoot;
			}
			PlayMakerFSM component = yourHeroActor.GetComponent<PlayMakerFSM>();
			component.FsmVariables.GetFsmBool("Duos").Value = GameMgr.Get().IsBattlegroundDuoGame();
			component.SendEvent(UniversalInputManager.UsePhoneUI ? "SlotInHeroAfterFlyIn_Phone" : "SlotInHeroAfterFlyIn");
			if (teammateHeroActor != null)
			{
				teammateHeroActor.GetComponent<PlayMakerFSM>().SendEvent(UniversalInputManager.UsePhoneUI ? "TeammatePortalAfterFlyIn" : "TeammatePortalAfterFlyIn");
			}
			yield return new WaitForSeconds(1f);
			if ((bool)versusText)
			{
				versusText.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
				versusText.transform.position = vsPosition;
			}
			yield return new WaitForSeconds(1.5f);
			int fsmPlayerId = 1;
			foreach (Actor item in opponentActorList)
			{
				PlayMakerFSM component2 = item.GetComponent<PlayMakerFSM>();
				component2.FsmVariables.GetFsmInt("Player").Value = fsmPlayerId++;
				component2.FsmVariables.GetFsmBool("Duos").Value = GameMgr.Get().IsBattlegroundDuoGame();
				component2.SendEvent(UniversalInputManager.UsePhoneUI ? "Spawn_Phone" : "Spawn");
			}
			yield return new WaitForSeconds(1.5f);
			if ((bool)versusText)
			{
				yield return new WaitForSeconds(0.1f);
				versusText.FadeOut();
				yield return new WaitForSeconds(0.32f);
			}
			foreach (Actor item2 in opponentActorList)
			{
				PlayMakerFSM component3 = item2.GetComponent<PlayMakerFSM>();
				component3.FsmVariables.GetFsmBool("Duos").Value = GameMgr.Get().IsBattlegroundDuoGame();
				component3.SendEvent(UniversalInputManager.UsePhoneUI ? "FlyIn_Phone" : "FlyIn");
			}
			if (PlayerLeaderboardManager.Get() != null)
			{
				PlayerLeaderboardManager.Get().UpdateLayout(animate: false);
			}
			yield return new WaitForSeconds(1.5f);
			if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
			{
				GameState.Get().GetGameEntity().ClearMulliganActorStateChanges(yourHeroActor);
			}
			foreach (Actor item3 in opponentActorList)
			{
				item3.gameObject.SetActive(value: false);
			}
		}
		else
		{
			SetMulliganBannerText(GameStrings.Get("GAMEPLAY_MULLIGAN_WAITING"));
			mulliganChooseBanner.GetComponent<Banner>().MoveGlowForBottomPlacement();
			while (!opponentPlayerHasReplacementCards && !GameState.Get().IsGameOver())
			{
				yield return null;
			}
		}
		EndMulligan();
	}

	private SharedPlayerInfo GetPlayerForCard(Card card)
	{
		foreach (SharedPlayerInfo sph in GameState.Get().GetPlayerInfoMap().Values)
		{
			if (card.GetEntity() != null && card.GetEntity().GetCardId() == sph.GetPlayerHero().GetCardId())
			{
				return sph;
			}
		}
		return null;
	}

	private void SetMulliganBannerText(string title)
	{
		SetMulliganBannerText(title, null);
	}

	private void SetMulliganBannerText(string title, string subtitle)
	{
		if (mulliganChooseBanner == null)
		{
			return;
		}
		if (GameMgr.Get().IsBattlegrounds())
		{
			Banner banner = mulliganChooseBanner.GetComponent<Banner>();
			if (banner != null)
			{
				if (GameMgr.Get().IsBattlegrounds() && GetBaconAnomaly() != 0)
				{
					banner.SetBaconAnomalyBannerText(title, subtitle, GetBaconAnomaly());
				}
				else
				{
					banner.SetBaconBannerText(title, subtitle);
				}
			}
		}
		else if (subtitle != null)
		{
			mulliganChooseBanner.GetComponent<Banner>().SetText(title, subtitle);
		}
		else
		{
			mulliganChooseBanner.GetComponent<Banner>().SetText(title);
		}
	}

	private void SetMulliganDetailLabelText(string title)
	{
		if (!(mulliganDetailLabel == null))
		{
			mulliganDetailLabel.GetComponent<UberText>().Text = title;
		}
	}

	private bool IsBaconBuddiesActive()
	{
		return GetBaconBuddies() > 0;
	}

	private bool IsBaconQuestsActive()
	{
		return GetBaconQuests() > 0;
	}

	private int GetBaconBuddies()
	{
		return GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_BUDDY_ENABLED);
	}

	private int GetBaconQuests()
	{
		return GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_QUESTS_ACTIVE);
	}

	private bool IsBaconTrinketActive()
	{
		return GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_TRINKETS_ACTIVE) > 0;
	}

	private bool IsBaconAnomalyActive()
	{
		return GetBaconAnomaly() > 0;
	}

	private int GetBaconAnomaly()
	{
		return GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_GLOBAL_ANOMALY_DBID);
	}

	private bool IsHearthstoneAnomaliesActive()
	{
		return GetAnomalies().Count > 0;
	}

	private List<int> GetAnomalies()
	{
		List<int> anomalies = new List<int>();
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		int anomalyEntID = 0;
		anomalyEntID = gameEntity.GetTag(GAME_TAG.ANOMALY1);
		if (anomalyEntID != 0)
		{
			anomalies.Add(anomalyEntID);
		}
		anomalyEntID = gameEntity.GetTag(GAME_TAG.ANOMALY2);
		if (anomalyEntID != 0)
		{
			anomalies.Add(anomalyEntID);
		}
		return anomalies;
	}

	private void ShowHearthstoneAnomaliesDetail()
	{
		if (IsHearthstoneAnomaliesActive())
		{
			Banner banner = mulliganChooseBanner.GetComponent<Banner>();
			if (!(banner == null))
			{
				List<int> anomalies = GetAnomalies();
				string title = ((anomalies.Count == 1) ? "GAMEPLAY_MULLIGAN_ANOMALY" : "GAMEPLAY_MULLIGAN_ANOMALIES");
				string subtitle = "GAMEPLAY_MULLIGAN_ANOMALY_SUBTITLE";
				banner.SetHearthstoneAnomaliesBannerText(GameStrings.Get(title), GameStrings.Get(subtitle), anomalies);
			}
		}
	}

	private void ShowMulliganDetail()
	{
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.DISPLAY_MULLIGAN_DETAIL_LABEL))
		{
			return;
		}
		if (GameMgr.Get().IsBattlegrounds())
		{
			if (mulliganChooseBanner == null)
			{
				return;
			}
			Banner banner = mulliganChooseBanner.GetComponent<Banner>();
			if (!(banner == null))
			{
				GameEntity game = GameState.Get().GetGameEntity();
				if (IsBaconAnomalyActive())
				{
					banner.SetBaconAnomalyBannerText(game.GetMulliganBannerText(), game.GetMulliganDetailText(), GetBaconAnomaly());
				}
				else
				{
					banner.SetBaconBannerText(game.GetMulliganBannerText(), game.GetMulliganDetailText());
				}
			}
			return;
		}
		string mulliganDetailText = GameState.Get().GetGameEntity().GetMulliganDetailText();
		if (mulliganDetailText != null)
		{
			if (mulliganDetailLabel == null)
			{
				mulliganDetailLabel = UnityEngine.Object.Instantiate(mulliganDetailLabelPrefab);
			}
			if (!(mulliganDetailLabel == null))
			{
				mulliganDetailLabel.transform.position = Board.Get().FindBone("MulliganDetail").position;
				SetMulliganDetailLabelText(mulliganDetailText);
			}
		}
	}

	private IEnumerator RemoveUIButtons()
	{
		if (mulliganButton != null)
		{
			mulliganButton.m_button.GetComponent<PlayMakerFSM>().SendEvent("Death");
		}
		if (mulliganButtonWidget != null)
		{
			mulliganButtonWidget.gameObject.SetActive(value: false);
		}
		if (m_refreshButton != null)
		{
			m_refreshButton.m_button.GetComponent<PlayMakerFSM>().SendEvent("Death");
		}
		if (mulliganCancelConfirmationButtonWidget != null)
		{
			mulliganCancelConfirmationButtonWidget.gameObject.SetActive(value: false);
		}
		if (m_replaceLabels != null)
		{
			for (int i = 0; i < m_replaceLabels.Count; i++)
			{
				if (m_replaceLabels[i] != null)
				{
					Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
					rotateArgs.Add("rotation", new Vector3(0f, 0f, 0f));
					rotateArgs.Add("time", 0.5f);
					rotateArgs.Add("easetype", iTween.EaseType.easeInExpo);
					iTween.RotateTo(m_replaceLabels[i].gameObject, rotateArgs);
					Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
					scaleArgs.Add("scale", new Vector3(0.001f, 0.001f, 0.001f));
					scaleArgs.Add("time", 0.5f);
					scaleArgs.Add("easetype", iTween.EaseType.easeInExpo);
					scaleArgs.Add("oncomplete", "DestroyButton");
					scaleArgs.Add("oncompletetarget", base.gameObject);
					scaleArgs.Add("oncompleteparams", m_replaceLabels[i]);
					iTween.ScaleTo(m_replaceLabels[i].gameObject, scaleArgs);
					yield return new WaitForSeconds(0.05f);
				}
			}
		}
		yield return new WaitForSeconds(3.5f);
		if (mulliganButton != null)
		{
			UnityEngine.Object.Destroy(mulliganButton.gameObject);
		}
		if (mulliganButtonWidget != null)
		{
			UnityEngine.Object.Destroy(mulliganButtonWidget.gameObject);
		}
		if (m_refreshButton != null)
		{
			UnityEngine.Object.Destroy(m_refreshButton.gameObject);
		}
		if (mulliganCancelConfirmationButtonWidget != null)
		{
			UnityEngine.Object.Destroy(mulliganCancelConfirmationButtonWidget.gameObject);
		}
	}

	private void DestroyButton(UnityEngine.Object buttonToDestroy)
	{
		UnityEngine.Object.Destroy(buttonToDestroy);
	}

	private void HandleGameOverDuringMulligan()
	{
		if (m_WaitForBoardThenLoadButton != null)
		{
			StopCoroutine(m_WaitForBoardThenLoadButton);
		}
		m_WaitForBoardThenLoadButton = null;
		if (m_WaitForHeroesAndStartAnimations != null)
		{
			StopCoroutine(m_WaitForHeroesAndStartAnimations);
		}
		m_WaitForHeroesAndStartAnimations = null;
		if (m_ResumeMulligan != null)
		{
			StopCoroutine(m_ResumeMulligan);
		}
		m_ResumeMulligan = null;
		if (m_DealStartingCards != null)
		{
			StopCoroutine(m_DealStartingCards);
		}
		m_DealStartingCards = null;
		if (m_ShowMultiplayerWaitingArea != null)
		{
			StopCoroutine(m_ShowMultiplayerWaitingArea);
		}
		m_ShowMultiplayerWaitingArea = null;
		if (m_RemoveOldCardsAnimation != null)
		{
			StopCoroutine(m_RemoveOldCardsAnimation);
		}
		m_RemoveOldCardsAnimation = null;
		if (m_PlayStartingTaunts != null)
		{
			StopCoroutine(m_PlayStartingTaunts);
		}
		m_PlayStartingTaunts = null;
		if (m_Spectator_WaitForFriendlyPlayerThenProcessEntitiesChosen != null)
		{
			StopCoroutine(m_Spectator_WaitForFriendlyPlayerThenProcessEntitiesChosen);
		}
		m_Spectator_WaitForFriendlyPlayerThenProcessEntitiesChosen = null;
		if (m_ContinueMulliganWhenBoardLoads != null)
		{
			StopCoroutine(m_ContinueMulliganWhenBoardLoads);
		}
		m_ContinueMulliganWhenBoardLoads = null;
		if (m_WaitAFrameBeforeSendingEventToMulliganButton != null)
		{
			StopCoroutine(m_WaitAFrameBeforeSendingEventToMulliganButton);
		}
		m_WaitAFrameBeforeSendingEventToMulliganButton = null;
		if (m_WaitAFrameBeforeSendingEventToMulliganRefreshButton != null)
		{
			StopCoroutine(m_WaitAFrameBeforeSendingEventToMulliganRefreshButton);
		}
		m_WaitAFrameBeforeSendingEventToMulliganRefreshButton = null;
		if (m_ShrinkStartingHandBanner != null)
		{
			StopCoroutine(m_ShrinkStartingHandBanner);
		}
		m_ShrinkStartingHandBanner = null;
		if (m_AnimateCoinTossText != null)
		{
			StopCoroutine(m_AnimateCoinTossText);
		}
		m_AnimateCoinTossText = null;
		if (m_WaitForOpponentToFinishMulligan != null)
		{
			StopCoroutine(m_WaitForOpponentToFinishMulligan);
		}
		m_WaitForOpponentToFinishMulligan = null;
		if (m_EndMulliganWithTiming != null)
		{
			StopCoroutine(m_EndMulliganWithTiming);
		}
		m_EndMulliganWithTiming = null;
		if (m_HandleCoinCard != null)
		{
			StopCoroutine(m_HandleCoinCard);
		}
		m_HandleCoinCard = null;
		if (m_EnableHandCollidersAfterCardsAreDealt != null)
		{
			StopCoroutine(m_EnableHandCollidersAfterCardsAreDealt);
		}
		m_EnableHandCollidersAfterCardsAreDealt = null;
		if (m_SkipMulliganForResume != null)
		{
			StopCoroutine(m_SkipMulliganForResume);
		}
		m_SkipMulliganForResume = null;
		if (m_SkipMulliganWhenIntroComplete != null)
		{
			StopCoroutine(m_SkipMulliganWhenIntroComplete);
		}
		m_SkipMulliganWhenIntroComplete = null;
		if (m_WaitForBoardAnimToCompleteThenStartTurn != null)
		{
			StopCoroutine(m_WaitForBoardAnimToCompleteThenStartTurn);
		}
		m_WaitForBoardAnimToCompleteThenStartTurn = null;
		if (m_customIntroCoroutine != null)
		{
			StopCoroutine(m_customIntroCoroutine);
			GameState.Get().GetGameEntity().OnCustomIntroCancelled(myHeroCardActor.GetCard(), hisHeroCardActor.GetCard(), myheroLabel, hisheroLabel, versusText);
			m_customIntroCoroutine = null;
		}
		m_waitingForUserInput = false;
		DestroyXobjects();
		DestroyChooseBanner();
		DisableBaconMulliganElements();
		DestroyDetailLabel();
		DestroyAnomalyIcon();
		DestroyMulliganTimer();
		DestroyTagConditionalVFXs();
		if (coinObject != null)
		{
			UnityEngine.Object.Destroy(coinObject);
		}
		if (versusText != null)
		{
			UnityEngine.Object.Destroy(versusText.gameObject);
		}
		if (versusVo != null)
		{
			SoundManager.Get().Destroy(versusVo);
		}
		if (coinTossText != null)
		{
			UnityEngine.Object.Destroy(coinTossText);
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			Gameplay.Get().RemoveNameBanners();
		}
		else
		{
			Gameplay.Get().RemoveClassNames();
		}
		if (m_RemoveUIButtons != null)
		{
			StopCoroutine(m_RemoveUIButtons);
		}
		m_RemoveUIButtons = RemoveUIButtons();
		StartCoroutine(m_RemoveUIButtons);
		if (mulliganButton != null)
		{
			mulliganButton.SetEnabled(enabled: false);
		}
		if (mulliganButtonWidget != null)
		{
			mulliganButtonWidget.SetEnabled(active: false);
		}
		if (m_refreshButton != null)
		{
			m_refreshButton.SetEnabled(enabled: false);
		}
		ShowMulliganCardRefreshButton(show: false);
		DestoryHeroSkinSocketInEffects();
		if (myheroLabel != null && myheroLabel.isActiveAndEnabled)
		{
			myheroLabel.FadeOut();
		}
		if (hisheroLabel != null && hisheroLabel.isActiveAndEnabled)
		{
			hisheroLabel.FadeOut();
		}
		if (m_startingCards != null && GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS) && GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_HAS_HERO_LOBBY))
		{
			foreach (Card card in m_startingCards)
			{
				GameState.Get().GetGameEntity().ToggleAlternateMulliganActorHighlight(card, highlighted: false);
				GameState.Get().GetGameEntity().ToggleAlternateMulliganActorConfirmHighlight(card, highlighted: false);
			}
		}
		if (friendlySideHandZone != null)
		{
			foreach (Card card2 in friendlySideHandZone.GetCards())
			{
				Actor actor = card2.GetActor();
				actor.SetActorState(ActorStateType.CARD_IDLE);
				actor.ToggleForceIdle(bOn: true);
				if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS) && GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_HAS_HERO_LOBBY))
				{
					GameState.Get().GetGameEntity().ToggleAlternateMulliganActorHighlight(card2, highlighted: false);
					GameState.Get().GetGameEntity().ToggleAlternateMulliganActorConfirmHighlight(card2, highlighted: false);
					if (actor is PlayerLeaderboardMainCardActor)
					{
						actor.ActivateSpellBirthState(SpellType.DEATH);
					}
				}
			}
			if (hisHeroCardActor != null)
			{
				hisHeroCardActor.SetActorState(ActorStateType.CARD_IDLE);
				hisHeroCardActor.ToggleForceIdle(bOn: true);
			}
			Card friendlyHeroPower = GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard();
			if (friendlyHeroPower != null && friendlyHeroPower.GetActor() != null)
			{
				friendlyHeroPower.GetActor().SetActorState(ActorStateType.CARD_IDLE);
				friendlyHeroPower.GetActor().ToggleForceIdle(bOn: true);
			}
			if (!friendlyPlayerGoesFirst && ShouldHandleCoinCard())
			{
				Card coinCard = GetCoinCardFromFriendlyHand();
				coinCard.SetDoNotSort(on: false);
				coinCard.SetTransitionStyle(ZoneTransitionStyle.NORMAL);
				PutCoinCardInSpawnPosition(coinCard);
				coinCard.GetActor().Show();
			}
			friendlySideHandZone.ForceStandInUpdate();
			friendlySideHandZone.SetDoNotUpdateLayout(enable: false);
			friendlySideHandZone.UpdateLayout();
		}
		CleanupFakeCards();
		Board board = Board.Get();
		if (board != null)
		{
			board.RaiseTheLightsQuickly();
		}
		if (myHeroCardActor != null)
		{
			Animation cardAnim = myHeroCardActor.gameObject.GetComponent<Animation>();
			if (cardAnim != null)
			{
				cardAnim.Stop();
			}
			myHeroCardActor.transform.localScale = Vector3.one;
			myHeroCardActor.transform.rotation = Quaternion.identity;
			myHeroCardActor.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.FRIENDLY).transform.position;
		}
		if (hisHeroCardActor != null)
		{
			Animation oppCardAnim = hisHeroCardActor.gameObject.GetComponent<Animation>();
			if (oppCardAnim != null)
			{
				oppCardAnim.Stop();
			}
			hisHeroCardActor.transform.localScale = Vector3.one;
			hisHeroCardActor.transform.rotation = Quaternion.identity;
			hisHeroCardActor.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.OPPOSING).transform.position;
		}
	}

	private void CleanupFakeCards()
	{
		List<Actor> list = new List<Actor>(fakeCardsOnLeft.Count + fakeCardsOnRight.Count);
		list.AddRange(fakeCardsOnLeft);
		list.AddRange(fakeCardsOnRight);
		foreach (Actor fakeCard in list)
		{
			fakeCard.ActivateSpellBirthState(SpellType.DEATH);
			fakeCard.TurnOffCollider();
			if (GameState.Get().GetBooleanGameOption(GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS))
			{
				GameState.Get().GetGameEntity().ConfigureFakeMulliganCardActor(fakeCard, shown: false);
			}
		}
		if (conditionalHelperTextLabel != null)
		{
			conditionalHelperTextLabel.gameObject.SetActive(value: false);
		}
	}

	public void EndMulligan()
	{
		m_waitingForUserInput = false;
		if (m_replaceLabels != null)
		{
			for (int i = 0; i < m_replaceLabels.Count; i++)
			{
				UnityEngine.Object.Destroy(m_replaceLabels[i]);
			}
		}
		if (mulliganButton != null)
		{
			UnityEngine.Object.Destroy(mulliganButton.gameObject);
		}
		if (mulliganButtonWidget != null)
		{
			UnityEngine.Object.Destroy(mulliganButtonWidget.gameObject);
		}
		if (m_refreshButton != null)
		{
			UnityEngine.Object.Destroy(m_refreshButton.gameObject);
		}
		if (mulliganCancelConfirmationButtonWidget != null)
		{
			UnityEngine.Object.Destroy(mulliganCancelConfirmationButtonWidget.gameObject);
		}
		DestroyXobjects();
		DestroyChooseBanner();
		DisableBaconMulliganElements();
		DestroyDetailLabel();
		DestroyAnomalyIcon();
		DestroyTagConditionalVFXs();
		if (versusText != null)
		{
			UnityEngine.Object.Destroy(versusText.gameObject);
		}
		if (versusVo != null)
		{
			SoundManager.Get().Destroy(versusVo);
		}
		if (coinTossText != null)
		{
			UnityEngine.Object.Destroy(coinTossText);
		}
		if (hisheroLabel != null)
		{
			hisheroLabel.FadeOut();
		}
		if (myheroLabel != null)
		{
			myheroLabel.FadeOut();
		}
		DestoryHeroSkinSocketInEffects();
		myHeroCardActor.transform.localPosition = new Vector3(0f, 0f, 0f);
		hisHeroCardActor.transform.localPosition = new Vector3(0f, 0f, 0f);
		myHeroCardActor.Show();
		if (!GameState.Get().IsGameOver())
		{
			myHeroCardActor.GetHealthObject().Show();
			hisHeroCardActor.GetHealthObject().Show();
			if (myHeroCardActor.GetAttackObject() != null)
			{
				myHeroCardActor.GetAttackObject().Show();
			}
			if (hisHeroCardActor.GetAttackObject() != null)
			{
				hisHeroCardActor.GetAttackObject().Show();
			}
			friendlySideHandZone.ForceStandInUpdate();
			friendlySideHandZone.SetDoNotUpdateLayout(enable: false);
			friendlySideHandZone.UpdateLayout();
			if (m_startingOppCards != null && m_startingOppCards.Count > 0)
			{
				m_startingOppCards[m_startingOppCards.Count - 1].SetDoNotSort(on: false);
			}
			opposingSideHandZone.SetDoNotUpdateLayout(enable: false);
			opposingSideHandZone.UpdateLayout();
			friendlySideDeck.SetSuppressEmotes(suppress: false);
			opposingSideDeck.SetSuppressEmotes(suppress: false);
			Board.Get().SplitSurface();
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				Gameplay.Get().RemoveNameBanners();
				Gameplay.Get().AddGamePlayNameBannerPhone();
			}
			if (m_MyCustomSocketInSpell != null)
			{
				UnityEngine.Object.Destroy(m_MyCustomSocketInSpell);
			}
			if (m_HisCustomSocketInSpell != null)
			{
				UnityEngine.Object.Destroy(m_HisCustomSocketInSpell);
			}
			m_EndMulliganWithTiming = EndMulliganWithTiming();
			StartCoroutine(m_EndMulliganWithTiming);
		}
	}

	private IEnumerator EndMulliganWithTiming()
	{
		if (ShouldHandleCoinCard())
		{
			m_HandleCoinCard = HandleCoinCard();
			yield return StartCoroutine(m_HandleCoinCard);
		}
		else
		{
			UnityEngine.Object.Destroy(coinObject);
		}
		myHeroCardActor.TurnOnCollider();
		hisHeroCardActor.TurnOnCollider();
		FadeOutMulliganMusicAndStartGameplayMusic();
		foreach (Card card in friendlySideHandZone.GetCards())
		{
			card.GetActor().TurnOnCollider();
			card.GetActor().ToggleForceIdle(bOn: false);
		}
		myHeroCardActor.ToggleForceIdle(bOn: false);
		hisHeroCardActor.ToggleForceIdle(bOn: false);
		Card friendlyHeroPower = GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard();
		if (friendlyHeroPower != null && friendlyHeroPower.GetActor() != null)
		{
			friendlyHeroPower.GetActor().ToggleForceIdle(bOn: false);
		}
		if (!friendlyPlayerHasReplacementCards)
		{
			m_EnableHandCollidersAfterCardsAreDealt = EnableHandCollidersAfterCardsAreDealt();
			StartCoroutine(m_EnableHandCollidersAfterCardsAreDealt);
		}
		Board.Get().FindCollider("DragPlane").enabled = true;
		ForceMulliganActive(active: false);
		Board.Get().RaiseTheLights();
		FadeHeroPowerIn(GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard());
		FadeHeroPowerIn(GameState.Get().GetOpposingSidePlayer().GetHeroPowerCard());
		InputManager.Get().OnMulliganEnded();
		EndTurnButton.Get().OnMulliganEnded();
		GameState.Get().GetGameEntity().NotifyOfMulliganEnded();
		m_WaitForBoardAnimToCompleteThenStartTurn = WaitForBoardAnimToCompleteThenStartTurn();
		StartCoroutine(m_WaitForBoardAnimToCompleteThenStartTurn);
	}

	private IEnumerator HandleCoinCard()
	{
		if (!friendlyPlayerGoesFirst)
		{
			if (coinObject != null && coinObject.activeSelf)
			{
				yield return new WaitForSeconds(0.5f);
				coinObject.GetComponentInChildren<PlayMakerFSM>().SendEvent("Birth");
				yield return new WaitForSeconds(0.1f);
			}
			if (!GameMgr.Get().IsSpectator() && !Options.Get().GetBool(Option.HAS_SEEN_THE_COIN, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("MulliganManager.HandleCoinCard:" + Option.HAS_SEEN_THE_COIN))
			{
				NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, new Vector3(155.3f, NotificationManager.DEPTH, 34.5f), GameStrings.Get("VO_INNKEEPER_COIN_INTRO"), "VO_INNKEEPER_COIN_INTRO.prefab:6fb1b3b124d474c4c84e392646caada4");
				Options.Get().SetBool(Option.HAS_SEEN_THE_COIN, val: true);
			}
			Card coinCard = GetCoinCardFromFriendlyHand();
			PutCoinCardInSpawnPosition(coinCard);
			coinCard.ActivateActorSpell(SpellType.SUMMON_IN, CoinCardSummonFinishedCallback);
			yield return new WaitForSeconds(1f);
		}
		else
		{
			UnityEngine.Object.Destroy(coinObject);
			if (m_coinCardIndex >= 0)
			{
				m_startingOppCards[m_coinCardIndex].SetDoNotSort(on: false);
			}
			opposingSideHandZone.UpdateLayout();
		}
	}

	private bool IsCoinCard(Card card)
	{
		string cardId = card.GetEntity().GetCardId();
		return CosmeticCoinManager.Get().IsFavoriteCoinCard(cardId);
	}

	private Card GetCoinCardFromFriendlyHand()
	{
		List<Card> cards = friendlySideHandZone.GetCards();
		if (cards.Count > 0)
		{
			return cards[cards.Count - 1];
		}
		Debug.LogError("GetCoinCardFromFriendlyHand() failed. friendlySideHandZone is empty.");
		return null;
	}

	private void PutCoinCardInSpawnPosition(Card coinCard)
	{
		coinCard.transform.position = Board.Get().FindBone("MulliganCoinCardSpawnPosition").position;
		coinCard.transform.localScale = Board.Get().FindBone("MulliganCoinCardSpawnPosition").localScale;
	}

	private bool ShouldHandleCoinCard()
	{
		if (!GameState.Get().IsMulliganPhase())
		{
			return false;
		}
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.HANDLE_COIN))
		{
			return false;
		}
		return true;
	}

	private void CoinCardSummonFinishedCallback(Spell spell, object userData)
	{
		Card card = GameObjectUtils.FindComponentInParents<Card>(spell);
		card.RefreshActor();
		card.UpdateActorComponents();
		card.SetDoNotSort(on: false);
		UnityEngine.Object.Destroy(coinObject);
		card.SetTransitionStyle(ZoneTransitionStyle.VERY_SLOW);
		friendlySideHandZone.UpdateLayout(null, forced: true);
	}

	private IEnumerator EnableHandCollidersAfterCardsAreDealt()
	{
		while (!friendlyPlayerHasReplacementCards)
		{
			yield return null;
		}
		foreach (Card card in friendlySideHandZone.GetCards())
		{
			card.GetActor().TurnOnCollider();
		}
	}

	public void SkipCardChoosing()
	{
		skipCardChoosing = true;
		EnableDamageCapFX(enable: true);
	}

	public void SkipMulliganForDev()
	{
		if (m_WaitForBoardThenLoadButton != null)
		{
			StopCoroutine(m_WaitForBoardThenLoadButton);
		}
		m_WaitForBoardThenLoadButton = null;
		if (m_WaitForHeroesAndStartAnimations != null)
		{
			StopCoroutine(m_WaitForHeroesAndStartAnimations);
		}
		m_WaitForHeroesAndStartAnimations = null;
		if (m_DealStartingCards != null)
		{
			StopCoroutine(m_DealStartingCards);
		}
		m_DealStartingCards = null;
		if (m_ShowMultiplayerWaitingArea != null)
		{
			StopCoroutine(m_ShowMultiplayerWaitingArea);
		}
		m_ShowMultiplayerWaitingArea = null;
		EndMulligan();
	}

	private IEnumerator SkipMulliganForResume()
	{
		introComplete = true;
		ForceMulliganActive(active: false);
		SoundDucker ducker = null;
		while (GameMgr.Get() == null)
		{
			yield return null;
		}
		if (!GameMgr.Get().IsSpectator())
		{
			ducker = base.gameObject.AddComponent<SoundDucker>();
			ducker.m_DuckedCategoryDefs = new List<SoundDuckedCategoryDef>();
			foreach (Global.SoundCategory soundCategory in Enum.GetValues(typeof(Global.SoundCategory)))
			{
				if (soundCategory != Global.SoundCategory.AMBIENCE && soundCategory != Global.SoundCategory.MUSIC)
				{
					SoundDuckedCategoryDef categoryDef = new SoundDuckedCategoryDef();
					categoryDef.m_Category = soundCategory;
					categoryDef.m_Volume = 0f;
					categoryDef.m_RestoreSec = 5f;
					categoryDef.m_BeginSec = 0f;
					ducker.m_DuckedCategoryDefs.Add(categoryDef);
				}
			}
			ducker.StartDucking();
		}
		while (Board.Get() == null)
		{
			yield return null;
		}
		Board.Get().RaiseTheLightsQuickly();
		while (ZoneMgr.Get() == null)
		{
			yield return null;
		}
		InitZones();
		Collider dragPlane = Board.Get().FindCollider("DragPlane");
		friendlySideHandZone.SetDoNotUpdateLayout(enable: false);
		opposingSideHandZone.SetDoNotUpdateLayout(enable: false);
		dragPlane.enabled = false;
		friendlySideHandZone.AddInputBlocker();
		opposingSideHandZone.AddInputBlocker();
		while (GameState.Get() == null || !GameState.Get().IsGameCreated())
		{
			yield return null;
		}
		while (ZoneMgr.Get().HasActiveServerChange())
		{
			yield return null;
		}
		GameState.Get().GetGameEntity().NotifyOfMulliganInitialized();
		SceneMgr.Get().NotifySceneLoaded();
		while (LoadingScreen.Get().IsPreviousSceneActive() || LoadingScreen.Get().IsFadingOut())
		{
			yield return null;
		}
		if (ducker != null)
		{
			ducker.StopDucking();
			UnityEngine.Object.Destroy(ducker);
		}
		if (SceneMgr.Get().GetPrevMode() != SceneMgr.Mode.GAMEPLAY)
		{
			FadeOutMulliganMusicAndStartGameplayMusic();
		}
		dragPlane.enabled = true;
		friendlySideHandZone.RemoveInputBlocker();
		opposingSideHandZone.RemoveInputBlocker();
		friendlySideDeck.SetSuppressEmotes(suppress: false);
		opposingSideDeck.SetSuppressEmotes(suppress: false);
		if (GameState.Get().GetResponseMode() == GameState.ResponseMode.CHOICE)
		{
			GameState.Get().UpdateChoiceHighlights();
		}
		else if (GameState.Get().GetResponseMode() == GameState.ResponseMode.OPTION)
		{
			GameState.Get().UpdateOptionHighlights();
		}
		GameMgr.Get().UpdatePresence();
		while (InputManager.Get() == null)
		{
			yield return null;
		}
		InputManager.Get().OnMulliganEnded();
		if (EndTurnButton.Get() != null)
		{
			EndTurnButton.Get().OnMulliganEnded();
		}
		GameState.Get().GetGameEntity().NotifyOfMulliganEnded();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void SkipMulligan()
	{
		Gameplay.Get().RemoveClassNames();
		m_SkipMulliganWhenIntroComplete = SkipMulliganWhenIntroComplete();
		StartCoroutine(m_SkipMulliganWhenIntroComplete);
	}

	private IEnumerator SkipMulliganWhenIntroComplete()
	{
		m_waitingForUserInput = false;
		while (!introComplete)
		{
			yield return null;
		}
		myHeroCardActor?.TurnOnCollider();
		hisHeroCardActor?.TurnOnCollider();
		FadeOutMulliganMusicAndStartGameplayMusic();
		myHeroCardActor?.GetHealthObject().Show();
		hisHeroCardActor?.GetHealthObject().Show();
		myHeroCardActor?.GetAttackObject().Show();
		hisHeroCardActor?.GetAttackObject().Show();
		Board.Get().FindCollider("DragPlane").enabled = true;
		Board.Get().SplitSurface();
		Board.Get().RaiseTheLights();
		FadeHeroPowerIn(GameState.Get().GetFriendlySidePlayer().GetHeroPowerCard());
		FadeHeroPowerIn(GameState.Get().GetOpposingSidePlayer().GetHeroPowerCard());
		ForceMulliganActive(active: false);
		InitZones();
		friendlySideHandZone.SetDoNotUpdateLayout(enable: false);
		friendlySideHandZone.UpdateLayout();
		opposingSideHandZone.SetDoNotUpdateLayout(enable: false);
		opposingSideHandZone.UpdateLayout();
		friendlySideDeck.SetSuppressEmotes(suppress: false);
		opposingSideDeck.SetSuppressEmotes(suppress: false);
		InputManager.Get().OnMulliganEnded();
		EndTurnButton.Get().OnMulliganEnded();
		GameState.Get().GetGameEntity().NotifyOfMulliganEnded();
		m_WaitForBoardAnimToCompleteThenStartTurn = WaitForBoardAnimToCompleteThenStartTurn();
		StartCoroutine(m_WaitForBoardAnimToCompleteThenStartTurn);
	}

	private void FadeOutMulliganMusicAndStartGameplayMusic()
	{
		GameState.Get().GetGameEntity().StartGameplaySoundtracks();
	}

	private IEnumerator WaitForBoardAnimToCompleteThenStartTurn()
	{
		yield return new WaitForSeconds(1.5f);
		GameState.Get().SetMulliganBusy(busy: false);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void ShuffleDeck()
	{
		SoundManager.Get().LoadAndPlay("FX_MulliganCoin09_DeckShuffle.prefab:e80f93eec961ec24485521285a8addf7", friendlySideDeck.gameObject);
		Animation deckShuffle = friendlySideDeck.gameObject.GetComponent<Animation>();
		if (deckShuffle == null)
		{
			deckShuffle = friendlySideDeck.gameObject.AddComponent<Animation>();
		}
		deckShuffle.AddClip(shuffleDeck, "shuffleDeckAnim");
		deckShuffle.Play("shuffleDeckAnim");
		deckShuffle = opposingSideDeck.gameObject.GetComponent<Animation>();
		if (deckShuffle == null)
		{
			deckShuffle = opposingSideDeck.gameObject.AddComponent<Animation>();
		}
		deckShuffle.AddClip(shuffleDeck, "shuffleDeckAnim");
		deckShuffle.Play("shuffleDeckAnim");
	}

	private void SlideCard(GameObject topCard)
	{
		iTween.MoveTo(topCard, iTween.Hash("position", new Vector3(topCard.transform.position.x - 0.5f, topCard.transform.position.y, topCard.transform.position.z), "time", 0.5f, "easetype", iTween.EaseType.linear));
	}

	private IEnumerator SampleAnimFrame(Animation animToUse, string animName, float startSec)
	{
		AnimationState state = animToUse[animName];
		state.enabled = true;
		state.time = startSec;
		animToUse.Play(animName);
		yield return null;
		state.enabled = false;
	}

	private void SortHand(Zone zone)
	{
		zone.GetCards().Sort(Zone.CardSortComparison);
	}

	private IEnumerator ShrinkStartingHandBanner(GameObject banner)
	{
		yield return new WaitForSeconds(4f);
		if (!(banner == null))
		{
			iTween.ScaleTo(banner, new Vector3(0f, 0f, 0f), 0.5f);
			yield return new WaitForSeconds(0.5f);
			UnityEngine.Object.Destroy(banner);
		}
	}

	private void FadeHeroPowerIn(Card heroPowerCard)
	{
		if (!(heroPowerCard == null))
		{
			Actor heroPowerActor = heroPowerCard.GetActor();
			if (!(heroPowerActor == null))
			{
				heroPowerActor.TurnOnCollider();
			}
		}
	}

	private void LoadMyHeroSkinSocketInEffect(Actor myHero)
	{
		if ((!string.IsNullOrEmpty(myHero.SocketInEffectFriendly) || (bool)UniversalInputManager.UsePhoneUI) && (!string.IsNullOrEmpty(myHero.SocketInEffectFriendlyPhone) || !UniversalInputManager.UsePhoneUI))
		{
			m_isLoadingMyCustomSocketIn = true;
			string socketEffectPath = myHero.SocketInEffectFriendly;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				socketEffectPath = myHero.SocketInEffectFriendlyPhone;
			}
			if (!AssetLoader.Get().InstantiatePrefab(socketEffectPath, OnMyHeroSkinSocketInEffectLoadAttempted))
			{
				OnMyHeroSkinSocketInEffectLoadAttempted(socketEffectPath, null, null);
			}
		}
	}

	private void OnMyHeroSkinSocketInEffectLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogError("Failed to load My custom hero socket in effect!");
			m_isLoadingMyCustomSocketIn = false;
			return;
		}
		go.transform.position = Board.Get().FindBone("CustomSocketIn_Friendly").position;
		Spell socketInSpell = go.GetComponent<Spell>();
		if (socketInSpell == null)
		{
			Debug.LogError("Faild to locate Spell on custom socket in effect!");
			m_isLoadingMyCustomSocketIn = false;
			return;
		}
		m_MyCustomSocketInSpell = socketInSpell;
		if (m_MyCustomSocketInSpell.HasUsableState(SpellStateType.IDLE))
		{
			m_MyCustomSocketInSpell.ActivateState(SpellStateType.IDLE);
		}
		else
		{
			m_MyCustomSocketInSpell.gameObject.SetActive(value: false);
		}
		m_isLoadingMyCustomSocketIn = false;
	}

	private void LoadHisHeroSkinSocketInEffect(Actor hisHero)
	{
		if ((!string.IsNullOrEmpty(hisHero.SocketInEffectOpponent) || (bool)UniversalInputManager.UsePhoneUI) && (!string.IsNullOrEmpty(hisHero.SocketInEffectOpponentPhone) || !UniversalInputManager.UsePhoneUI))
		{
			m_isLoadingHisCustomSocketIn = true;
			string socketEffectPath = hisHero.SocketInEffectOpponent;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				socketEffectPath = hisHero.SocketInEffectOpponentPhone;
			}
			if (!AssetLoader.Get().InstantiatePrefab(socketEffectPath, OnHisHeroSkinSocketInEffectLoadAttempted))
			{
				OnHisHeroSkinSocketInEffectLoadAttempted(socketEffectPath, null, null);
			}
		}
	}

	private void OnHisHeroSkinSocketInEffectLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogError("Failed to load His custom hero socket in effect!");
			m_isLoadingHisCustomSocketIn = false;
			return;
		}
		go.transform.position = Board.Get().FindBone("CustomSocketIn_Opposing").position;
		Spell socketInSpell = go.GetComponent<Spell>();
		if (socketInSpell == null)
		{
			Debug.LogError("Faild to locate Spell on custom socket in effect!");
			m_isLoadingHisCustomSocketIn = false;
			return;
		}
		m_HisCustomSocketInSpell = socketInSpell;
		if (m_HisCustomSocketInSpell.HasUsableState(SpellStateType.IDLE))
		{
			m_HisCustomSocketInSpell.ActivateState(SpellStateType.IDLE);
		}
		else
		{
			m_HisCustomSocketInSpell.gameObject.SetActive(value: false);
		}
		m_isLoadingHisCustomSocketIn = false;
	}

	private void LoadStartOfGameHeroTrayEffects()
	{
		if (Board.Get().m_FriendlyHeroTray != null)
		{
			PlayMakerFSM trayFSM = Board.Get().m_FriendlyHeroTray.GetComponent<PlayMakerFSM>();
			if (trayFSM != null)
			{
				trayFSM.SendEvent("Birth");
			}
		}
		if (Board.Get().m_OpponentHeroTray != null)
		{
			PlayMakerFSM trayFSM2 = Board.Get().m_OpponentHeroTray.GetComponent<PlayMakerFSM>();
			if (trayFSM2 != null)
			{
				trayFSM2.SendEvent("Birth");
			}
		}
	}

	private void DestoryHeroSkinSocketInEffects()
	{
		if (m_MyCustomSocketInSpell != null)
		{
			UnityEngine.Object.Destroy(m_MyCustomSocketInSpell.gameObject);
		}
		if (m_HisCustomSocketInSpell != null)
		{
			UnityEngine.Object.Destroy(m_HisCustomSocketInSpell.gameObject);
		}
	}

	private void OnFakeHeroActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Log.MulliganManager.PrintWarning($"MulliganManager.OnFakeHeroActorLoaded() - FAILED to load actor \"{assetRef}\"");
			pendingFakeHeroCount--;
			return;
		}
		Actor heroCardActor = go.GetComponent<Actor>();
		if (heroCardActor == null)
		{
			Log.MulliganManager.PrintWarning($"MulliganManager.OnFakeHeroActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			pendingFakeHeroCount--;
			return;
		}
		((List<Actor>)callbackData).Add(heroCardActor);
		heroCardActor.SetUnlit();
		LayerUtils.SetLayer(heroCardActor.gameObject, base.gameObject.layer, null);
		heroCardActor.GetMeshRenderer().gameObject.layer = 8;
		GameState.Get().GetGameEntity().ConfigureFakeMulliganCardActor(heroCardActor, shown: true);
		if (m_startingCards.Count > 0)
		{
			heroCardActor.gameObject.transform.position = new Vector3(m_startingCards[0].transform.position.x, m_startingCards[0].transform.position.y, m_startingCards[0].transform.position.z);
		}
		pendingFakeHeroCount--;
	}

	public void UpdateBaconRerollButtons(bool resetForceDisabledState = false)
	{
		foreach (Card card in m_startingCards)
		{
			if (card == null)
			{
				continue;
			}
			Actor actor = card.GetActor();
			if (!(actor == null))
			{
				PlayerLeaderboardMainCardActor baconHeroActor = actor.GetComponent<PlayerLeaderboardMainCardActor>();
				if (baconHeroActor != null)
				{
					baconHeroActor.UpdateMulliganRerollButton(null, resetForceDisabledState);
				}
			}
		}
	}

	private void OnHeroActorLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Log.MulliganManager.PrintWarning($"MulliganManager.OnHeroActorLoadAttempted() - FAILED to load actor \"{assetRef}\"");
			pendingHeroCount--;
			return;
		}
		Actor heroCardActor = go.GetComponent<Actor>();
		if (heroCardActor == null)
		{
			Log.MulliganManager.PrintWarning($"MulliganManager.OnHeroActorLoadAttempted() - ERROR actor \"{assetRef}\" has no Actor component");
			pendingHeroCount--;
			return;
		}
		Card refCard = (Card)callbackData;
		Entity entity = refCard.GetEntity();
		if (entity != null)
		{
			GameState.Get().GetGameEntity().ConfigureLockedMulliganCardActor(heroCardActor, entity.HasTag(GAME_TAG.BACON_LOCKED_MULLIGAN_HERO));
		}
		heroCardActor.SetCard(refCard);
		heroCardActor.SetCardDefFromCard(refCard);
		heroCardActor.SetPremium(refCard.GetPremium());
		heroCardActor.UpdateAllComponents();
		if (refCard.GetActor() != null)
		{
			refCard.GetActor().Destroy();
		}
		refCard.SetActor(heroCardActor);
		heroCardActor.SetEntity(refCard.GetEntity());
		heroCardActor.UpdateAllComponents();
		heroCardActor.SetUnlit();
		LayerUtils.SetLayer(heroCardActor.gameObject, base.gameObject.layer, null);
		heroCardActor.GetMeshRenderer().gameObject.layer = 8;
		heroCardActor.GetHealthObject().Hide();
		GameState.Get().GetGameEntity().ApplyMulliganActorStateChanges(heroCardActor);
		PlayerLeaderboardMainCardActor baconHeroActor = heroCardActor.GetComponent<PlayerLeaderboardMainCardActor>();
		if (baconHeroActor != null)
		{
			Entity heroEntity = heroCardActor.GetEntity();
			baconHeroActor.SetShowHeroRerollButton(heroEntity?.ShouldShowHeroRerollButton() ?? false, heroEntity != null && heroEntity.ShouldEnableRerollButton(null, null) <= Entity.RerollButtonEnableResult.UNLOCK);
		}
		choiceHeroActors.Add(refCard, heroCardActor);
		pendingHeroCount--;
	}

	private void OnOpponentHeroActorLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Log.MulliganManager.PrintWarning($"MulliganManager.OnOpponentHeroActorLoadAttempted() - FAILED to load actor \"{assetRef}\"");
			pendingHeroCount--;
			return;
		}
		Actor heroCardActor = go.GetComponent<Actor>();
		if (heroCardActor == null)
		{
			Log.MulliganManager.PrintWarning($"MulliganManager.OnOpponentHeroActorLoadAttempted() - ERROR actor \"{assetRef}\" has no Actor component");
			pendingHeroCount--;
			return;
		}
		Card refCard = (Card)callbackData;
		heroCardActor.SetCard(refCard);
		heroCardActor.SetCardDefFromCard(refCard);
		heroCardActor.SetPremium(refCard.GetPremium());
		heroCardActor.UpdateAllComponents();
		if (refCard.GetActor() != null)
		{
			refCard.GetActor().Destroy();
		}
		refCard.SetActor(heroCardActor);
		heroCardActor.SetEntity(refCard.GetEntity());
		heroCardActor.UpdateAllComponents();
		heroCardActor.SetUnlit();
		heroCardActor.transform.localPosition = new Vector3(heroCardActor.transform.localPosition.x + 1000f, heroCardActor.transform.localPosition.y, heroCardActor.transform.localPosition.z);
		LayerUtils.SetLayer(heroCardActor.gameObject, base.gameObject.layer, null);
		UnityEngine.Object.Destroy(heroCardActor.m_healthObject);
		UnityEngine.Object.Destroy(heroCardActor.m_attackObject);
		GameState.Get().GetGameEntity().ApplyMulliganActorLobbyStateChanges(heroCardActor);
		opponentHeroActors.Add(refCard, heroCardActor);
		pendingHeroCount--;
	}

	private void OnTeammateHeroActorLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Log.MulliganManager.PrintWarning($"MulliganManager.OnTeammateHeroActorLoadAttempted() - FAILED to load actor \"{assetRef}\"");
			pendingHeroCount--;
			return;
		}
		Actor heroCardActor = go.GetComponent<Actor>();
		if (heroCardActor == null)
		{
			Log.MulliganManager.PrintWarning($"MulliganManager.OnTeammateHeroActorLoadAttempted() - ERROR actor \"{assetRef}\" has no Actor component");
			pendingHeroCount--;
			return;
		}
		Card refCard = (Card)callbackData;
		heroCardActor.SetCard(refCard);
		heroCardActor.SetCardDefFromCard(refCard);
		heroCardActor.SetPremium(refCard.GetPremium());
		heroCardActor.UpdateAllComponents();
		if (refCard.GetActor() != null)
		{
			refCard.GetActor().Destroy();
		}
		refCard.SetActor(heroCardActor);
		heroCardActor.SetEntity(refCard.GetEntity());
		heroCardActor.UpdateAllComponents();
		heroCardActor.SetUnlit();
		heroCardActor.transform.localPosition = Vector3.zero;
		LayerUtils.SetLayer(heroCardActor.gameObject, base.gameObject.layer, null);
		UnityEngine.Object.Destroy(heroCardActor.m_healthObject);
		UnityEngine.Object.Destroy(heroCardActor.m_attackObject);
		heroCardActor.transform.localPosition = new Vector3(10000f, 0f, 0f);
		GameState.Get().GetGameEntity().ApplyMulliganActorLobbyStateChanges(heroCardActor);
		teammateHeroActors.Add(refCard, heroCardActor);
		pendingHeroCount--;
	}

	private void OnHeroDoneMovingAfterSelection()
	{
		if (!GameState.Get().GetGameEntity().IsHeroMulliganLobbyFinished() || !GameState.Get().GetGameEntity().IsTeammateHeroMulliganFinished())
		{
			m_chosenHero.SetAlwaysShowCardsInTooltip(show: true);
			m_chosenHero.CreateCardsInTooltip();
			myHeroCardActor.TurnOnCollider();
		}
	}
}
