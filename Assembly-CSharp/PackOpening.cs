using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.Progression;
using PegasusLettuce;
using PegasusUtil;
using UnityEngine;

public class PackOpening : MonoBehaviour
{
	public PackOpeningBones m_Bones;

	public PackOpeningDirector m_DirectorPrefab;

	public PackOpeningSocket m_Socket;

	public PackOpeningSocket m_SocketAccent;

	public UberText m_HeaderText;

	public UIBButton m_BackButton;

	public StoreButton m_StoreButton;

	public IGamemodeAvailabilityService.Gamemode m_StoreButtonGamemodeTarget = IGamemodeAvailabilityService.Gamemode.HEARTHSTONE;

	public Vector3 m_StoreButtonOffset;

	public GameObject m_NoPacksLabel;

	public GameObject m_DragPlane;

	public Vector3 m_DragTolerance;

	public GameObject m_InputBlocker;

	public UIBObjectSpacing m_UnopenedPackContainer;

	public UIBScrollable m_UnopenedPackScroller;

	public CameraMask m_PackTrayCameraMask;

	public float m_UnopenedPackPadding;

	public bool m_OnePackCentered = true;

	public GameObject m_centerSocketBone;

	private const int MAX_OPENED_PACKS_BEFORE_CARD_CACHE_RESET = 10;

	private static PackOpening s_instance;

	private bool m_waitingForInitialNetData = true;

	private bool m_waitingForInitialMercenaryData;

	private bool m_shown;

	private readonly Map<int, UnopenedPack> m_unopenedPacks = new Map<int, UnopenedPack>();

	private readonly Map<int, bool> m_unopenedPacksLoading = new Map<int, bool>();

	private PackOpeningDirector m_director;

	private UnopenedPack m_draggedPack;

	private UnopenedPack m_selectedPack;

	private GameObject m_centerPack;

	private Notification m_hintArrow;

	private GameObject m_PackOpeningCardFX;

	private GameObject m_PackOpeningPortraitFX;

	private GameObject m_PackOpeningCoinFX;

	private bool m_autoOpenPending;

	private int m_lastOpenedBoosterId;

	private bool m_enableBackButton;

	private bool m_entryTransitionFinished;

	private static bool m_hasAcknowledgedKoreanWarning;

	private Coroutine m_autoOpenPackCoroutine;

	private float m_packOpeningStartTime;

	private int m_packOpeningId;

	private VillagePackOpeningDisplay m_villageDisplay;

	private const float m_holdToOpenDelay = 0.5f;

	private bool m_holdToOpenPackReady = true;

	private bool m_isAutoOpenPackMode;

	private float m_inSpaceBarHeldDownModeTimer = -1f;

	private bool m_delayShowingNoPacksLabel;

	private List<int> m_sortedPackIds;

	[Header("Mass Pack Opening data")]
	private IEnumerator m_hooverCoroutine;

	[Tooltip("How far the pack can be dragged before hoovering is stopped.")]
	public float m_maxHooverRange = 20f;

	[Tooltip("The base rate at which hoovering occurs.")]
	public float m_hooverBaseRate = 1f;

	private float m_hooverBirthDealy = 1f;

	[Tooltip("The rate of change in which the hoovering speed increases. This multiplies m_hooverRate every time a pack is hoovered")]
	public float m_hooverRateMulitplier = 0.65f;

	[Tooltip("We cant hoover faster than this interval.")]
	public float m_hooverRateMin = 0.1f;

	[Tooltip("The actual value used to control hoover speed. Exposed for animation purposes.")]
	public float m_hooverRate = 1f;

	[Tooltip("When using space bar to hoover, we need to offset the dragged pack")]
	public Vector3 m_spaceToHooverOffset;

	private int m_massPackOpeningHooverChunkSize;

	[Tooltip("The PlayMaker script being used for hoovering")]
	[Header("Mass Pack Opening Hoovering PlayMaker Variable Names")]
	public PlayMakerFSM m_hooveringPlayMaker;

	[Tooltip("The PlayMaker script variable for the source stack")]
	public string m_hooveringSourceStackVarName;

	[Tooltip("The PlayMaker script variable for the held stack")]
	public string m_hooveringHeldStackVarName;

	[Tooltip("The PlayMaker script variable for the singe stack, used to get the pack mesh specifically")]
	public string m_hooveringSingleStackVarName;

	[Tooltip("The PlayMaker script variable for the delay between hoovering")]
	public string m_hooveringDelayVarName;

	[Tooltip("The PlayMaker script variable for the glow multi texture on the pack")]
	public string m_hooveringGlowMultiTextureVarName;

	[Tooltip("The PlayMaker script variable for the glow texture on the pack")]
	public string m_hooveringGlowTextureVarName;

	[Tooltip("The PlayMaker script variable for the birth delay")]
	public string m_hooveringBirthDelayVarName;

	private GameObject m_singleStack;

	private bool m_isDoingAnim;

	private const int m_massPackOpeningTooltipThreshold = 20;

	private PlatformDependentValue<bool> m_isOnMobile;

	private static LettucePackComponent m_mockLettucePackComponent;

	private void Awake()
	{
		s_instance = this;
		m_delayShowingNoPacksLabel = false;
		m_isOnMobile = new PlatformDependentValue<bool>(PlatformCategory.Screen)
		{
			Phone = true,
			Tablet = true,
			PC = false
		};
		m_hooverRate = m_hooverBaseRate;
		if (m_hooveringPlayMaker != null)
		{
			m_hooverBirthDealy = m_hooveringPlayMaker.FsmVariables.GetFsmFloat(m_hooveringBirthDelayVarName).Value;
		}
		m_massPackOpeningHooverChunkSize = MassPackOpeningHooverChunkSize();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			AssetLoader.Get().InstantiatePrefab("PackOpeningCardFX_Phone.prefab:0ef32a20a9e7843c3ba360e49527dbfa", OnPackOpeningCardFXLoaded);
			AssetLoader.Get().InstantiatePrefab("PackOpeningPortraitFX_Phone.prefab:0608d65f5479515409f3ff3e905a7343", OnPackOpeningPortraitFXLoaded);
			AssetLoader.Get().InstantiatePrefab("PackOpeningCoinFX_Phone.prefab:1da77448f9a695f4faa3101c383c7770", OnPackOpeningCoinFXLoaded);
		}
		else
		{
			AssetLoader.Get().InstantiatePrefab("PackOpeningCardFX.prefab:b32177fb14f134edfb891dc93501b1ce", OnPackOpeningCardFXLoaded);
			AssetLoader.Get().InstantiatePrefab("PackOpeningPortraitFX.prefab:abb645e2796976845ab335be65d26618", OnPackOpeningPortraitFXLoaded);
			AssetLoader.Get().InstantiatePrefab("PackOpeningCoinFX.prefab:a3ff192999fcb13478dd78b1e5c80576", OnPackOpeningCoinFXLoaded);
		}
		TAG_CARDTYPE[] array = new TAG_CARDTYPE[5]
		{
			TAG_CARDTYPE.MINION,
			TAG_CARDTYPE.WEAPON,
			TAG_CARDTYPE.SPELL,
			TAG_CARDTYPE.HERO,
			TAG_CARDTYPE.LOCATION
		};
		foreach (TAG_CARDTYPE cardType in array)
		{
			AssetLoader.Get().LoadAsset<GameObject>(ActorNames.GetHandActor(cardType));
		}
		AssetLoader.Get().LoadAsset<GameObject>("GhostStyleDef.prefab:932fbc50238e04673aeb0f59c9cfaed1");
		AssetLoader.Get().LoadAsset<GameObject>("Card_Hand_Ability_SpellTable.prefab:62c19ebc0789b4f00b9f393b17349cb2");
		InitializeNet();
		InitializeUI();
		if (StoreButtonOnPackOpeningScreenEnabled())
		{
			m_StoreButton.AddEventListener(UIEventType.RELEASE, OnStoreButtonReleased);
			TelemetryWatcher.WatchFor(TelemetryWatcherWatchType.StoreFromPackOpening);
		}
		else
		{
			m_StoreButton.gameObject.SetActive(value: false);
		}
		if (SceneMgr.Get() != null && SceneMgr.Get().GetMode() != SceneMgr.Mode.LETTUCE_PACK_OPENING)
		{
			Box.Get().AddTransitionFinishedListener(OnBoxTransitionFinished);
			SceneMgr.Get().RegisterScenePreUnloadEvent(OnScenePreUnload);
		}
		GameSaveDataManager.Get().Request(GameSaveKeyId.COLLECTION_MANAGER, OnGameSaveDataReady);
	}

	private void Start()
	{
		Navigation.Push(OnNavigateBack);
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
	}

	private void Update()
	{
		if (!IsReady())
		{
			return;
		}
		UpdateDraggedPack();
		if (StoreButtonOnPackOpeningScreenEnabled())
		{
			StartCoroutine(SetNoPacksLabel());
		}
		if (!HoldSpaceToOpenPacksEnabled())
		{
			return;
		}
		if (m_inSpaceBarHeldDownModeTimer >= 0f && Time.realtimeSinceStartup - m_inSpaceBarHeldDownModeTimer >= 0.5f)
		{
			m_isAutoOpenPackMode = true;
			m_inSpaceBarHeldDownModeTimer = -1f;
		}
		if (m_isAutoOpenPackMode)
		{
			if (!Application.isFocused)
			{
				ResetAfterLosingFocusOfGame();
			}
			else if (m_holdToOpenPackReady)
			{
				StartCoroutine(StartHoldToOpenCooldown());
				SpaceToOpenPack();
			}
		}
	}

	private void OnDestroy()
	{
		if (m_draggedPack != null && PegCursor.Get() != null)
		{
			PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
		}
		ShutdownNet();
		s_instance = null;
		if (SceneMgr.Get() != null)
		{
			SceneMgr.Get().UnregisterScenePreUnloadEvent(OnScenePreUnload);
		}
		if (Box.Get() != null)
		{
			Box.Get().RemoveTransitionFinishedListener(OnBoxTransitionFinished);
		}
		FatalErrorMgr.Get().RemoveErrorListener(OnFatalError);
		m_director.OnDoneOpeningPack -= OnDonePackOpening;
	}

	public static PackOpening Get()
	{
		return s_instance;
	}

	public GameObject GetPackOpeningCardEffects()
	{
		return m_PackOpeningCardFX;
	}

	public GameObject GetPackOpeningPortraitEffects()
	{
		return m_PackOpeningPortraitFX;
	}

	public GameObject GetPackOpeningCoinEffects()
	{
		return m_PackOpeningCoinFX;
	}

	private bool SpaceToOpenPack()
	{
		if (m_director == null)
		{
			return false;
		}
		if (m_draggedPack != null && m_selectedPack != null && (m_draggedPack.GetCount() >= MassPackOpeningPackLimit(m_draggedPack.GetBoosterId()) || m_selectedPack.GetCount() == 0))
		{
			TriggerMassPackOpeningWithSpaceBar();
			return true;
		}
		if (CanOpenPackAutomatically())
		{
			if (PopupDisplayManager.Get().RedundantNDERerollPopups.IsWaitingToShow())
			{
				m_director.FinishPackOpen();
			}
			else
			{
				UnopenedPack openPack = null;
				if (!m_unopenedPacks.TryGetValue(m_lastOpenedBoosterId, out openPack) || openPack.GetCount() == 0)
				{
					foreach (int id in m_sortedPackIds)
					{
						if (m_unopenedPacks.ContainsKey(id))
						{
							UnopenedPack pack = m_unopenedPacks[id];
							if (!(pack == null) && pack.GetCount() > 0)
							{
								openPack = pack;
								break;
							}
						}
					}
				}
				m_autoOpenPending = true;
				m_director.FinishPackOpen();
				m_autoOpenPackCoroutine = StartCoroutine(OpenNextPackWhenReady(openPack));
			}
		}
		else if (PackOpeningDirector.QuickPackOpeningAllowed)
		{
			if (!m_director.IsMassPackOpening() && !m_director.IsCatchupPackOpening())
			{
				m_director.ForceRevealRandomCard();
			}
			else
			{
				if (m_director.IsMassPackOpeningHighlightsContinueButtonShowing())
				{
					m_director.MassPackOpeningContinuePressed();
					m_inSpaceBarHeldDownModeTimer = -1f;
					return true;
				}
				if (m_director.IsMassPackOpeningSummaryDoneButtonShowing())
				{
					m_director.MassPackOpeningDonePressed();
					m_inSpaceBarHeldDownModeTimer = -1f;
					return true;
				}
				if (m_director.IsCatchupPackOpeningDoneButtonShowing())
				{
					m_director.TriggerCatchupPackOpeningDoneButtonPressed();
					return true;
				}
				m_director.MassPackOpeningRevealRandomCard();
			}
		}
		return true;
	}

	public bool HandleKeyboardInput()
	{
		if (HoldSpaceToOpenPacksEnabled())
		{
			if (InputCollection.GetKeyUp(KeyCode.Space))
			{
				if (m_draggedPack != null && (m_isDoingAnim || m_draggedPack.GetCount() >= m_massPackOpeningHooverChunkSize))
				{
					TriggerMassPackOpeningWithSpaceBar();
				}
				if (m_director != null && m_director.IsMassPackOpening())
				{
					if (m_director.IsMassPackOpeningHighlightsContinueButtonShowing())
					{
						m_director.MassPackOpeningContinuePressed();
						m_inSpaceBarHeldDownModeTimer = -1f;
						return true;
					}
					if (m_director.IsMassPackOpeningSummaryDoneButtonShowing())
					{
						m_director.MassPackOpeningDonePressed();
						m_inSpaceBarHeldDownModeTimer = -1f;
						return true;
					}
				}
				TriggerHooverDeath();
				bool ret = false;
				if (!m_isAutoOpenPackMode)
				{
					StopCoroutine(StartHoldToOpenCooldown());
					m_holdToOpenPackReady = true;
					ret = SpaceToOpenPack();
				}
				m_isAutoOpenPackMode = false;
				m_inSpaceBarHeldDownModeTimer = -1f;
				return ret;
			}
			if (InputCollection.GetKeyDown(KeyCode.Space))
			{
				m_inSpaceBarHeldDownModeTimer = Time.realtimeSinceStartup;
			}
		}
		else if (InputCollection.GetKeyUp(KeyCode.Space))
		{
			return SpaceToOpenPack();
		}
		return false;
	}

	public void PreUnload()
	{
		FullScreenFXMgr.Get().StopAllEffects();
		if (m_director != null)
		{
			m_director.HideCardsAndDoneButton();
		}
		Hide();
	}

	public bool CreateMockLettucePackComponent(int mercId, int artVariantId, int currenyAmount, bool acquired, TAG_PREMIUM premium)
	{
		if (artVariantId != 0 && !LettuceMercenary.GetArtVariations(mercId).Exists((MercenaryArtVariationDbfRecord variation) => variation.ID == artVariantId))
		{
			return false;
		}
		m_mockLettucePackComponent = new LettucePackComponent();
		m_mockLettucePackComponent.MercenaryId = mercId;
		m_mockLettucePackComponent.HasMercenaryId = mercId != 0;
		m_mockLettucePackComponent.MercenaryArtVariationId = artVariantId;
		m_mockLettucePackComponent.HasMercenaryArtVariationId = artVariantId != 0;
		m_mockLettucePackComponent.MercenaryArtVariationPremium = (int)premium;
		m_mockLettucePackComponent.HasMercenaryArtVariationPremium = premium != TAG_PREMIUM.NORMAL;
		m_mockLettucePackComponent.CurrencyAmount = currenyAmount;
		m_mockLettucePackComponent.HasCurrencyAmount = currenyAmount != 0;
		m_mockLettucePackComponent.MercenaryAlreadyAcquired = acquired;
		m_mockLettucePackComponent.HasMercenaryAlreadyAcquired = acquired;
		return true;
	}

	private void OnScenePreUnload(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		PreUnload();
	}

	public bool IsReady()
	{
		return !m_waitingForInitialNetData;
	}

	public void SetVillageDisplay(VillagePackOpeningDisplay display)
	{
		m_villageDisplay = display;
	}

	public bool HoldSpaceToOpenPacksEnabled()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().ContinuousQuickOpenEnabled;
	}

	public bool StoreButtonOnPackOpeningScreenEnabled()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().ShopButtonOnPackOpeningScreenEnabled;
	}

	public IEnumerator StartHoldToOpenCooldown()
	{
		m_holdToOpenPackReady = false;
		yield return new WaitForSeconds(0.5f);
		m_holdToOpenPackReady = true;
	}

	public bool MassPackOpeningEnabled()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().MassPackOpeningEnabled;
	}

	public int MassPackOpeningPackLimit(int boosterId)
	{
		BoosterDbfRecord boosterRecord = GameDbf.Booster.GetRecord(boosterId);
		NetCache.NetCacheFeatures featuresNetCache = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (boosterRecord.IsCatchupPack)
		{
			return Math.Min(featuresNetCache.MassCatchupPackOpeningPackLimit, featuresNetCache.MassPackOpeningPackLimit);
		}
		if (boosterRecord.Premium == 1)
		{
			return Math.Min(featuresNetCache.MassPackOpeningGoldenPackLimit, featuresNetCache.MassPackOpeningPackLimit);
		}
		return featuresNetCache.MassPackOpeningPackLimit;
	}

	public int MassPackOpeningHooverChunkSize()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().MassPackOpeningHooverChunkSize;
	}

	public bool MassCatchupPackOpeningEnabled()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().MassCatchupPackOpeningEnabled;
	}

	private void OnBoxTransitionFinished(object userData)
	{
		Box.Get().RemoveTransitionFinishedListener(OnBoxTransitionFinished);
		Show();
		m_entryTransitionFinished = true;
	}

	public void Show()
	{
		if (m_shown)
		{
			return;
		}
		m_shown = true;
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_PACK_OPENING)
		{
			m_entryTransitionFinished = true;
			MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_MercenariesPackOpening);
		}
		else
		{
			if (!Options.Get().GetBool(Option.HAS_SEEN_PACK_OPENING, defaultVal: false) && BoosterPackUtils.GetTotalBoosterCount() > 0)
			{
				Options.Get().SetBool(Option.HAS_SEEN_PACK_OPENING, val: true);
			}
			MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_PackOpening);
		}
		PresenceMgr.Get().SetStatus(Global.PresenceStatus.PACKOPENING);
		CreateDirector();
		BnetBar.Get().RefreshCurrency();
		if (BoosterPackUtils.GetBoosterStackCount() < 2 || SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_PACK_OPENING)
		{
			ShowHintOnUnopenedPack();
		}
		UpdateUIEvents();
		DisablePackTrayMask();
	}

	private void Hide()
	{
		if (m_shown)
		{
			m_shown = false;
			DestroyHint();
			m_StoreButton.Unload();
			m_InputBlocker.SetActive(value: false);
			EnablePackTrayMask();
			UnregisterUIEvents();
			ShutdownNet();
		}
	}

	private bool OnNavigateBack()
	{
		if (!m_enableBackButton || m_InputBlocker.activeSelf)
		{
			return false;
		}
		GoBack();
		return true;
	}

	private void GoBack()
	{
		Hide();
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_PACK_OPENING && m_villageDisplay != null)
		{
			m_villageDisplay.NavigateBack();
		}
		else
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		}
	}

	private void InitializeNet()
	{
		m_waitingForInitialNetData = true;
		m_waitingForInitialMercenaryData = BoosterPackUtils.LoadMercenaryCollectionIfRequired();
		NetCache.Get().RegisterScreenPackOpening(OnNetDataReceived, OnBoosterOpenFailed);
		Network.Get().RegisterNetHandler(BoosterContent.PacketID.ID, OnBoosterOpened);
		Network.Get().RegisterNetHandler(OpenMercenariesPackResponse.PacketID.ID, OnMercenariesBoosterOpened);
		Network.Get().RegisterNetHandler(DBAction.PacketID.ID, OnDBAction);
		LoginManager.Get().OnAchievesLoaded += OnReloginComplete;
	}

	private void ShutdownNet()
	{
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			netCache.UnregisterNetCacheHandler(OnNetDataReceived);
		}
		if (ServiceManager.TryGet<Network>(out var network))
		{
			network.RemoveNetHandler(BoosterContent.PacketID.ID, OnBoosterOpened);
			network.RemoveNetHandler(OpenMercenariesPackResponse.PacketID.ID, OnMercenariesBoosterOpened);
			network.RemoveNetHandler(DBAction.PacketID.ID, OnDBAction);
		}
		if (ServiceManager.TryGet<LoginManager>(out var _))
		{
			LoginManager.Get().OnAchievesLoaded -= OnReloginComplete;
		}
	}

	private void OnNetDataReceived()
	{
		if (m_waitingForInitialNetData)
		{
			m_waitingForInitialNetData = false;
			StartCoroutine(WaitForMercenaryData());
		}
		UpdatePacks();
		UpdateUIEvents();
	}

	private IEnumerator WaitForMercenaryData()
	{
		while (m_waitingForInitialMercenaryData && !CollectionManager.Get().IsLettuceLoaded())
		{
			yield return null;
		}
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.LETTUCE_PACK_OPENING)
		{
			SceneMgr.Get().NotifySceneLoaded();
		}
	}

	private void OnReloginComplete()
	{
		UpdatePacks();
		UpdateUIEvents();
	}

	private void UpdatePacks()
	{
		NetCache.NetCacheBoosters netBoosters = NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>();
		if (netBoosters == null)
		{
			Debug.LogError($"PackOpening.UpdatePacks() - boosters are null");
			return;
		}
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_PACK_OPENING)
		{
			UpdateBoosterPacks(netBoosters, mercPacksOnly: true);
		}
		else
		{
			UpdateBoosterPacks(netBoosters);
		}
		if (m_director == null || !m_director.IsPlaying())
		{
			LayoutPacks();
		}
	}

	private void UpdateBoosterPacks(NetCache.NetCacheBoosters netBoosters, bool mercPacksOnly = false)
	{
		foreach (NetCache.BoosterStack netCacheBooster in netBoosters.BoosterStacks)
		{
			if (mercPacksOnly && netCacheBooster.Id != 629)
			{
				continue;
			}
			int id = netCacheBooster.Id;
			if (m_unopenedPacks.ContainsKey(id) && m_unopenedPacks[id] != null)
			{
				if (netBoosters.GetBoosterStack(id) == null)
				{
					UnityEngine.Object.Destroy(m_unopenedPacks[id]);
					m_unopenedPacks[id] = null;
				}
				else
				{
					UpdatePack(m_unopenedPacks[id], netBoosters.GetBoosterStack(id));
				}
			}
			else
			{
				if (netBoosters.GetBoosterStack(id) == null || netBoosters.GetBoosterStack(id).Count <= 0 || (m_unopenedPacksLoading.ContainsKey(id) && m_unopenedPacksLoading[id]))
				{
					continue;
				}
				m_unopenedPacksLoading[id] = true;
				BoosterDbfRecord record = GameDbf.Booster.GetRecord(id);
				if (record == null)
				{
					Debug.LogErrorFormat("PackOpening.UpdatePacks() - No DBF record for booster {0}", id);
					continue;
				}
				if (string.IsNullOrEmpty(record.PackOpeningPrefab))
				{
					Debug.LogError($"PackOpening.UpdatePacks() - no prefab found for booster {id}!");
					continue;
				}
				AssetReference assetReference = record.PackOpeningPrefab;
				if (!AssetLoader.Get().InstantiatePrefab(assetReference, OnUnopenedPackLoadAttempted, netCacheBooster, AssetLoadingOptions.IgnorePrefabPosition))
				{
					OnUnopenedPackLoadAttempted(assetReference, null, netCacheBooster);
				}
			}
		}
	}

	private void OnBoosterOpenFailed(NetCache.ErrorInfo info)
	{
		TelemetryManager.Client().SendPackOpening(-1f, -1f, -1, -1);
		NetCache.DefaultErrorHandler(info);
	}

	private void OnBoosterOpened()
	{
		TriggerHooverDeath();
		float packOpeningDuration = Time.realtimeSinceStartup - m_packOpeningStartTime;
		if (m_centerPack != null)
		{
			UnityEngine.Object.Destroy(m_centerPack);
			m_centerPack = null;
		}
		bool isCatchup = false;
		List<NetCache.BoosterCard> cardInstances = Network.Get().OpenedBooster(out isCatchup);
		if (Network.Get().GetNumPacksOpened(out var packsOpened))
		{
			m_director.SetNumPacksOpened(packsOpened);
		}
		UpdateMassPackOpeningEnabledIfCacheIsStale();
		m_director.OnBoosterOpened(cardInstances, isCatchup);
		m_director.Play(m_lastOpenedBoosterId, packOpeningDuration, m_packOpeningId);
		m_autoOpenPending = false;
	}

	private void UpdateMassPackOpeningEnabledIfCacheIsStale()
	{
		if (Network.Get().GetMassPackOpeningEnabledFromResponsePacket(out var enabled) && MassPackOpeningEnabled() != enabled)
		{
			NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().MassPackOpeningEnabled = enabled;
		}
	}

	private void OnMercenariesBoosterOpened()
	{
		OpenMercenariesPackResponse response = Network.Get().OpenMercenariesPackResponse();
		if (response.ErrorCode != 0)
		{
			Error.AddWarning(GameStrings.Get("GLUE_LETTUCE_ERROR_OPENING_PACK_TITLE"), GameStrings.Get("GLUE_LETTUCE_ERROR_OPENING_PACK_DESCRIPTION"));
			RevertBoosterOpeningAfterFailure();
			return;
		}
		if (m_centerPack != null)
		{
			UnityEngine.Object.Destroy(m_centerPack);
			m_centerPack = null;
		}
		m_director.Play(m_lastOpenedBoosterId, Time.realtimeSinceStartup - m_packOpeningStartTime, m_packOpeningId);
		m_autoOpenPending = false;
		if (m_mockLettucePackComponent != null)
		{
			response.PackContents.Components[0] = m_mockLettucePackComponent;
			m_mockLettucePackComponent = null;
		}
		if (response?.PackContents?.Components != null)
		{
			m_director.OnMercenariesBoosterOpened(response.PackContents.Components);
		}
	}

	private void OnDBAction()
	{
		Network.DBAction response = Network.Get().GetDbAction();
		if (response.Action == Network.DBAction.ActionType.OPEN_BOOSTER && response.Result != Network.DBAction.ResultType.SUCCESS)
		{
			OnDBAction_OpenBoosterFailed(response);
		}
	}

	private void OnDBAction_OpenBoosterFailed(Network.DBAction response)
	{
		Debug.LogError($"PackOpening.OnDBAction_OpenBoosterFailed - Error while opening packs: {response}");
		RevertBoosterOpeningAfterFailure();
	}

	private void RevertBoosterOpeningAfterFailure()
	{
		m_UnopenedPackScroller.Pause(pause: false);
		m_InputBlocker.SetActive(value: false);
		m_autoOpenPending = false;
		m_unopenedPacks[m_lastOpenedBoosterId].AddBoosters(m_director.NumPacksOpened);
		m_unopenedPacksLoading[m_lastOpenedBoosterId] = false;
		TriggerHooverDeath();
		if (m_centerPack != null)
		{
			UnityEngine.Object.Destroy(m_centerPack);
			m_centerPack = null;
		}
		BnetBar.Get().RefreshCurrency();
	}

	private void OnGameSaveDataReady(bool dataLoadSuccess)
	{
		if (!dataLoadSuccess)
		{
			Log.CollectionManager.PrintError("Error retrieving Game Save Key for Collection Manager!");
		}
		else
		{
			CardBackManager.Get().LoadRandomCardBackIntoFavoriteSlot(updateScene: true);
		}
	}

	private void CreateDirector()
	{
		GameObject instance = UnityEngine.Object.Instantiate(m_DirectorPrefab.gameObject);
		m_director = instance.GetComponent<PackOpeningDirector>();
		instance.transform.parent = base.transform;
		TransformUtil.CopyWorld(m_director, m_Bones.m_Director);
		m_director.OnDoneOpeningPack += OnDonePackOpening;
	}

	private void PickUpBooster()
	{
		UnopenedPack creatorPack = m_draggedPack.GetCreatorPack();
		int numPacks = 1;
		if (IsMassPackOpenable(creatorPack))
		{
			if (MassPackOpeningEnabled() && m_hooveringPlayMaker != null && creatorPack.GetCount() >= m_massPackOpeningHooverChunkSize)
			{
				TriggerHooverBirth();
			}
			if (Options.Get().GetBool(Option.MPO_DEBUG, defaultVal: false))
			{
				numPacks = Math.Min(creatorPack.GetCount(), MassPackOpeningPackLimit(creatorPack.GetBoosterId()));
			}
		}
		creatorPack.RemoveBooster(numPacks);
		m_draggedPack.SetBoosterId(creatorPack.GetBoosterId());
		m_draggedPack.SetCount(numPacks);
	}

	private bool IsMassPackOpenable(UnopenedPack pack)
	{
		if (pack == null)
		{
			return false;
		}
		int boosterId = pack.GetBoosterId();
		if (boosterId == 629)
		{
			return false;
		}
		if ((!MassCatchupPackOpeningEnabled() || MassPackOpeningPackLimit(boosterId) <= 1) && GameDbf.Booster.GetRecord(boosterId).IsCatchupPack)
		{
			return false;
		}
		return true;
	}

	private IEnumerator HooverBooster(UnopenedPack sourceStack, UnopenedPack heldStack)
	{
		if (sourceStack == null || heldStack == null)
		{
			yield break;
		}
		if (sourceStack.GetCount() > 0 && heldStack.GetCount() < MassPackOpeningPackLimit(sourceStack.GetBoosterId()))
		{
			if (BoosterIsWithinHooverRange(sourceStack, heldStack) || m_isAutoOpenPackMode)
			{
				if (sourceStack.GetCount() >= m_massPackOpeningHooverChunkSize && heldStack.GetCount() < m_massPackOpeningHooverChunkSize && !m_isDoingAnim)
				{
					yield return new WaitForSeconds(m_hooverBirthDealy);
					TransferPacksBetweenStacks(sourceStack, m_draggedPack, m_massPackOpeningHooverChunkSize - 1);
				}
				else
				{
					yield return new WaitForSeconds(m_hooverRate);
					TransferPacksBetweenStacks(sourceStack, heldStack, m_massPackOpeningHooverChunkSize);
					m_hooverRate *= m_hooverRateMulitplier;
					if (m_hooverRate < m_hooverRateMin)
					{
						m_hooverRate = m_hooverRateMin;
					}
					if (m_hooveringPlayMaker != null)
					{
						m_hooveringPlayMaker.FsmVariables.GetFsmFloat(m_hooveringDelayVarName).Value = m_hooverRate;
					}
				}
			}
			else if (m_hooveringPlayMaker != null)
			{
				TriggerHooverDeath();
				yield break;
			}
			m_hooverCoroutine = HooverBooster(sourceStack, heldStack);
			StartCoroutine(m_hooverCoroutine);
			yield return null;
		}
		else
		{
			TriggerHooverDeath();
		}
	}

	private bool BoosterIsWithinHooverRange(UnopenedPack sourceStack, UnopenedPack heldStack)
	{
		if (sourceStack == null || heldStack == null)
		{
			return false;
		}
		Vector3 position = sourceStack.transform.position;
		Vector3 heldPos = heldStack.transform.position;
		if ((position - heldPos).sqrMagnitude <= Mathf.Pow(m_maxHooverRange, 2f))
		{
			return true;
		}
		return false;
	}

	private void OpenBooster(UnopenedPack pack, int numPacks = 1)
	{
		AchievementManager.Get().PauseToastNotifications();
		PopupDisplayManager.SuppressPopupsTemporarily = true;
		PopupDisplayManager.Get().RedundantNDERerollPopups.SuppressNDEPopups = true;
		if (MassPackOpeningEnabled() && numPacks >= m_massPackOpeningHooverChunkSize)
		{
			CardBackManager.Get().LoadRandomCardBackIntoFavoriteSlot(updateScene: true);
		}
		int boosterId = 1;
		if (!GameUtils.IsFakePackOpeningEnabled())
		{
			boosterId = pack.GetBoosterId();
			m_packOpeningStartTime = Time.realtimeSinceStartup;
			m_packOpeningId = boosterId;
			m_director.SetNumPacksOpened(numPacks);
			BoosterPackUtils.OpenBooster(boosterId, numPacks);
		}
		m_InputBlocker.SetActive(value: true);
		if (m_autoOpenPackCoroutine != null)
		{
			StopCoroutine(m_autoOpenPackCoroutine);
			m_autoOpenPackCoroutine = null;
		}
		m_director.OnFinishedEvent += OnDirectorFinished;
		m_lastOpenedBoosterId = boosterId;
		BnetBar.Get().HideCurrencyFrames();
		if (GameUtils.IsFakePackOpeningEnabled())
		{
			StartCoroutine(OnFakeBoosterOpened());
		}
		m_UnopenedPackScroller.Pause(pause: true);
	}

	private IEnumerator OnFakeBoosterOpened()
	{
		float fakeNetDelay = UnityEngine.Random.Range(0f, 1f);
		yield return new WaitForSeconds(fakeNetDelay);
		List<NetCache.BoosterCard> cards = new List<NetCache.BoosterCard>();
		NetCache.BoosterCard boosterCard = new NetCache.BoosterCard();
		boosterCard.Def.Name = "CS1_042";
		boosterCard.Def.Premium = TAG_PREMIUM.NORMAL;
		cards.Add(boosterCard);
		boosterCard = new NetCache.BoosterCard();
		boosterCard.Def.Name = "CS1_129";
		boosterCard.Def.Premium = TAG_PREMIUM.NORMAL;
		cards.Add(boosterCard);
		boosterCard = new NetCache.BoosterCard();
		boosterCard.Def.Name = "EX1_050";
		boosterCard.Def.Premium = TAG_PREMIUM.NORMAL;
		cards.Add(boosterCard);
		boosterCard = new NetCache.BoosterCard();
		boosterCard.Def.Name = "EX1_105";
		boosterCard.Def.Premium = TAG_PREMIUM.NORMAL;
		cards.Add(boosterCard);
		boosterCard = new NetCache.BoosterCard();
		boosterCard.Def.Name = "EX1_350";
		boosterCard.Def.Premium = TAG_PREMIUM.NORMAL;
		cards.Add(boosterCard);
		m_director.OnBoosterOpened(cards, isCatchupPack: false);
	}

	private void PutBackBooster()
	{
		if (!(m_draggedPack == null))
		{
			UnopenedPack creatorPack = m_draggedPack.GetCreatorPack();
			int numPacks = m_draggedPack.GetCount();
			m_draggedPack.RemoveBooster(numPacks);
			creatorPack.AddBoosters(numPacks);
		}
	}

	private void UpdatePack(UnopenedPack pack, NetCache.BoosterStack boosterStack)
	{
		pack.SetBoosterId(boosterStack.Id);
		pack.SetCount(boosterStack.Count);
	}

	private void OnUnopenedPackLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		int boosterId = 0;
		if (callbackData is NetCache.BoosterStack)
		{
			boosterId = ((NetCache.BoosterStack)callbackData).Id;
			m_unopenedPacksLoading[boosterId] = false;
			if (go == null)
			{
				Debug.LogError($"PackOpening.OnUnopenedPackLoaded() - FAILED to load {assetRef}");
				return;
			}
			UnopenedPack pack = go.GetComponent<UnopenedPack>();
			go.SetActive(value: false);
			if (pack == null)
			{
				Debug.LogError($"PackOpening.OnUnopenedPackLoaded() - asset {base.name} did not have a {typeof(UnopenedPack)} script on it");
				return;
			}
			m_unopenedPacks.Add(boosterId, pack);
			pack.gameObject.SetActive(value: true);
			GameUtils.SetParent(pack, m_UnopenedPackContainer);
			pack.transform.localScale = Vector3.one;
			pack.SetDragTolerance(m_DragTolerance);
			if ((bool)m_isOnMobile)
			{
				pack.AddEventListener(UIEventType.PRESS, OnUnopenedPackPress);
				pack.AddEventListener(UIEventType.DRAG, OnUnopenedPackDrag);
			}
			else
			{
				pack.AddEventListener(UIEventType.PRESS, OnUnopenedPackDrag);
			}
			pack.AddEventListener(UIEventType.ROLLOVER, OnUnopenedPackRollover);
			pack.AddEventListener(UIEventType.ROLLOUT, OnUnopenedPackRollout);
			pack.AddEventListener(UIEventType.RELEASEALL, OnUnopenedPackReleaseAll);
			UpdatePack(pack, (NetCache.BoosterStack)callbackData);
			AchieveManager.Get().NotifyOfPacksReadyToOpen(pack);
			if (BoosterPackUtils.GetBoosterStackCount() < 2 || SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_PACK_OPENING)
			{
				LayoutPacks();
				ShowHintOnUnopenedPack();
			}
			else
			{
				StopHintOnUnopenedPack();
				LayoutPacks();
			}
			UpdateUIEvents();
		}
		else
		{
			Debug.LogErrorFormat("OnUnopenedPackLoaded() - Unable to get booster id for gameobject {0}", go?.name);
		}
	}

	private void LayoutPacks(bool animate = false)
	{
		m_sortedPackIds = GameUtils.GetSortedPackIds(ascending: false);
		m_UnopenedPackContainer.ClearObjects();
		if (!m_entryTransitionFinished)
		{
			DisablePackTrayMask();
		}
		int signupIncentiveBoosterDbId = 18;
		if (TemporaryAccountManager.IsTemporaryAccount())
		{
			m_unopenedPacks.TryGetValue(signupIncentiveBoosterDbId, out var pack);
			if (pack != null && pack.GetCount() > 0)
			{
				pack.gameObject.SetActive(value: true);
				m_UnopenedPackContainer.AddObject(pack);
			}
		}
		foreach (int packId in m_sortedPackIds)
		{
			if (signupIncentiveBoosterDbId != packId)
			{
				m_unopenedPacks.TryGetValue(packId, out var pack2);
				if (!(pack2 == null) && pack2.GetCount() != 0)
				{
					pack2.gameObject.SetActive(value: true);
					m_UnopenedPackContainer.AddObject(pack2);
				}
			}
		}
		if (m_OnePackCentered && m_UnopenedPackContainer.m_Objects.Count < 1)
		{
			m_UnopenedPackContainer.AddSpace(0);
		}
		m_UnopenedPackContainer.AddObject(m_StoreButton, m_StoreButtonOffset);
		if (animate)
		{
			m_UnopenedPackContainer.AnimateUpdatePositions(0.25f);
		}
		else
		{
			m_UnopenedPackContainer.UpdatePositions();
		}
		if (!m_entryTransitionFinished)
		{
			EnablePackTrayMask();
		}
	}

	private void CreateDraggedPack(UnopenedPack creatorPack)
	{
		m_draggedPack = creatorPack.AcquireDraggedPack();
		Vector3 pos = m_draggedPack.transform.position;
		if (UniversalInputManager.Get().GetInputHitInfo(GameLayer.DragPlane.LayerBit(), out var hit))
		{
			pos = hit.point;
		}
		float dot = Vector3.Dot(Camera.main.transform.forward, Vector3.up);
		float dir = (0f - dot) / Mathf.Abs(dot);
		Bounds bounds = m_draggedPack.GetComponent<Collider>().bounds;
		pos.y += dir * bounds.extents.y * m_draggedPack.transform.lossyScale.y;
		m_draggedPack.transform.position = pos;
	}

	private void DestroyDraggedPack()
	{
		if (!(m_draggedPack == null))
		{
			m_UnopenedPackScroller.Pause(pause: false);
			m_draggedPack.GetCreatorPack().ReleaseDraggedPack();
			m_draggedPack = null;
		}
	}

	private void UpdateDraggedPack()
	{
		if (m_draggedPack == null)
		{
			return;
		}
		if (MassPackOpeningEnabled() && IsMassPackOpenable(m_draggedPack))
		{
			UnopenedPack creatorPack = m_draggedPack.GetCreatorPack();
			if (creatorPack != null && creatorPack.GetCount() > 0 && creatorPack.GetCount() + m_draggedPack.GetCount() >= m_massPackOpeningHooverChunkSize && m_draggedPack.GetCount() < MassPackOpeningPackLimit(m_draggedPack.GetBoosterId()) && BoosterIsWithinHooverRange(creatorPack, m_draggedPack))
			{
				if (m_draggedPack.GetCount() < m_massPackOpeningHooverChunkSize && creatorPack.GetCount() >= m_massPackOpeningHooverChunkSize)
				{
					TriggerHooverBirth();
				}
				else
				{
					TriggerHooverDoAnim();
				}
			}
			else
			{
				TriggerHooverDeath();
				StopHooverCoroutine();
			}
		}
		Vector3 pos = m_draggedPack.transform.position;
		if (!m_isAutoOpenPackMode && UniversalInputManager.Get().GetInputHitInfo(GameLayer.DragPlane.LayerBit(), out var hit))
		{
			pos.x = hit.point.x;
			pos.z = hit.point.z;
			m_draggedPack.transform.position = pos;
		}
		if (InputCollection.GetMouseButtonUp(0) && !m_isAutoOpenPackMode)
		{
			DropPack();
		}
	}

	private void SetPackVariablesInPlayMaker(UnopenedPack sourceStack, UnopenedPack heldStack)
	{
		if (sourceStack == null || heldStack == null || m_hooveringPlayMaker == null)
		{
			return;
		}
		SetUpSingleStack(sourceStack);
		m_hooveringPlayMaker.FsmVariables.GetFsmGameObject(m_hooveringSourceStackVarName).Value = sourceStack.gameObject;
		m_hooveringPlayMaker.FsmVariables.GetFsmGameObject(m_hooveringHeldStackVarName).Value = heldStack.gameObject;
		m_hooveringPlayMaker.FsmVariables.GetFsmGameObject(m_hooveringSingleStackVarName).Value = m_singleStack;
		m_hooveringPlayMaker.FsmVariables.GetFsmFloat(m_hooveringDelayVarName).Value = m_hooverRate;
		HighlightState highlight = sourceStack.GetComponentInChildren<HighlightState>();
		if (highlight != null)
		{
			if (highlight.m_BannerStaticSilouetteTexture != null)
			{
				m_hooveringPlayMaker.FsmVariables.GetFsmTexture(m_hooveringGlowMultiTextureVarName).Value = highlight.m_BannerStaticSilouetteTexture;
			}
			if (highlight.m_StaticSilouetteTexture != null)
			{
				m_hooveringPlayMaker.FsmVariables.GetFsmTexture(m_hooveringGlowTextureVarName).Value = highlight.m_StaticSilouetteTexture;
			}
		}
	}

	private void SetUpSingleStack(UnopenedPack sourceStack)
	{
		DestroySingleStack();
		m_singleStack = UnityEngine.Object.Instantiate(sourceStack.m_SingleStack.m_RootObject);
		m_singleStack.transform.parent = sourceStack.gameObject.transform;
		m_singleStack.transform.position = new Vector3(-999999f, -999999f, -999999f);
		m_singleStack.SetActive(value: true);
	}

	private void DestroySingleStack()
	{
		if (m_singleStack != null)
		{
			UnityEngine.Object.Destroy(m_singleStack);
			m_singleStack = null;
		}
	}

	private void TriggerHooverBirth()
	{
		if (!(m_draggedPack == null) && !(m_selectedPack == null) && !m_isDoingAnim)
		{
			if (m_hooveringPlayMaker != null)
			{
				SetPackVariablesInPlayMaker(m_selectedPack, m_draggedPack);
				m_hooveringPlayMaker.SendEvent("Birth");
			}
			if (!Options.Get().GetBool(Option.MPO_DEBUG) && m_hooverCoroutine == null && m_selectedPack.GetCount() >= m_massPackOpeningHooverChunkSize)
			{
				StartHooverCoroutine();
			}
			m_isDoingAnim = true;
		}
	}

	private void TriggerHooverDoAnim()
	{
		if (!(m_draggedPack == null) && !(m_selectedPack == null) && !m_isDoingAnim)
		{
			if (m_hooveringPlayMaker != null)
			{
				SetPackVariablesInPlayMaker(m_selectedPack, m_draggedPack);
				m_hooveringPlayMaker.SendEvent("DoAnim");
			}
			if (!Options.Get().GetBool(Option.MPO_DEBUG) && m_hooverCoroutine == null && m_selectedPack.GetCount() > 0)
			{
				StartHooverCoroutine();
			}
			m_isDoingAnim = true;
		}
	}

	private void StartHooverCoroutine()
	{
		StopHooverCoroutine();
		m_hooverCoroutine = HooverBooster(m_selectedPack, m_draggedPack);
		StartCoroutine(m_hooverCoroutine);
	}

	private void StopHooverCoroutine()
	{
		if (m_hooverCoroutine != null)
		{
			StopCoroutine(m_hooverCoroutine);
			m_hooverRate = m_hooverBaseRate;
			m_hooverCoroutine = null;
		}
	}

	private void TriggerHooverDeath()
	{
		if (m_hooveringPlayMaker != null && m_isDoingAnim)
		{
			m_hooveringPlayMaker.SendEvent("Death");
			m_isDoingAnim = false;
			DestroySingleStack();
		}
		else
		{
			StopHooverCoroutine();
		}
	}

	private void OnDirectorFinished(object sender, EventArgs eventArgs)
	{
		m_UnopenedPackScroller.Pause(pause: false);
		int boosterCount = 0;
		foreach (KeyValuePair<int, UnopenedPack> entry in m_unopenedPacks)
		{
			if (!(entry.Value == null))
			{
				int count = entry.Value.GetCount();
				boosterCount += count;
				entry.Value.gameObject.SetActive(count > 0);
			}
		}
		m_InputBlocker.SetActive(value: false);
		CreateDirector();
		LayoutPacks(animate: true);
		BnetBar.Get().RefreshCurrency();
	}

	private void ShowHintOnUnopenedPack()
	{
		if (!m_shown || Options.Get().GetBool(Option.HAS_OPENED_BOOSTER, defaultVal: false) || !UserAttentionManager.CanShowAttentionGrabber("PackOpening.ShowHintOnUnopenedPack"))
		{
			return;
		}
		List<UnopenedPack> packs = new List<UnopenedPack>();
		foreach (KeyValuePair<int, UnopenedPack> entry in m_unopenedPacks)
		{
			if (!(entry.Value == null) && entry.Value.CanOpenPack() && entry.Value.GetCount() > 0)
			{
				packs.Add(entry.Value);
			}
		}
		if (packs.Count >= 1 && !(packs[0] == null) && packs[0].GetBoosterId() != 18 && m_hintArrow == null)
		{
			Vector3 arrowPosition = packs[0].GetComponent<Collider>().bounds.center;
			m_hintArrow = NotificationManager.Get().CreateBouncingArrow(UserAttentionBlocker.NONE, arrowPosition, new Vector3(0f, 90f, 0f), addToList: false);
			if (m_hintArrow != null)
			{
				FixArrowScale(packs[0].transform);
				Vector3 localPos = new Vector3(m_hintArrow.bounceObject.GetComponent<Renderer>().bounds.extents.x, arrowPosition.y, 0f);
				m_hintArrow.gameObject.transform.SetParent(packs[0].gameObject.transform);
				m_hintArrow.transform.localPosition = localPos;
			}
		}
	}

	private void StopHintOnUnopenedPack()
	{
		foreach (KeyValuePair<int, UnopenedPack> entry in m_unopenedPacks)
		{
			if (!(entry.Value == null) && entry.Value.CanOpenPack() && entry.Value.GetCount() > 0)
			{
				entry.Value.StopAlert();
				break;
			}
		}
	}

	private void ShowHintOnSlot()
	{
		if (!Options.Get().GetBool(Option.HAS_OPENED_BOOSTER, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("PackOpening.ShowHintOnSlot"))
		{
			if (m_hintArrow == null)
			{
				m_hintArrow = NotificationManager.Get().CreateBouncingArrow(UserAttentionBlocker.NONE, addToList: false);
			}
			if (m_hintArrow != null)
			{
				FixArrowScale(m_draggedPack.transform);
				Bounds arrowBounds = m_hintArrow.bounceObject.GetComponent<Renderer>().bounds;
				Vector3 arrowPosition = m_Bones.m_Hint.position;
				arrowPosition.z += arrowBounds.extents.z;
				m_hintArrow.transform.position = arrowPosition;
			}
		}
	}

	private void FixArrowScale(Transform parent)
	{
		Transform prevParent = m_hintArrow.transform.parent;
		m_hintArrow.transform.parent = parent;
		m_hintArrow.transform.localScale = Vector3.one;
		m_hintArrow.transform.parent = prevParent;
	}

	private void HideHint()
	{
		if (!(m_hintArrow == null))
		{
			Options.Get().SetBool(Option.HAS_OPENED_BOOSTER, val: true);
			UnityEngine.Object.Destroy(m_hintArrow.gameObject);
			m_hintArrow = null;
		}
	}

	private void DestroyHint()
	{
		if (!(m_hintArrow == null))
		{
			UnityEngine.Object.DestroyImmediate(m_hintArrow.gameObject);
			m_hintArrow = null;
		}
	}

	private void InitializeUI()
	{
		m_HeaderText.Text = GameStrings.Get("GLUE_PACK_OPENING_HEADER");
		m_BackButton.AddEventListener(UIEventType.RELEASE, OnBackButtonPressed);
		m_DragPlane.SetActive(value: false);
		m_InputBlocker.SetActive(value: false);
	}

	private void UpdateUIEvents()
	{
		if (!m_shown)
		{
			UnregisterUIEvents();
			return;
		}
		if (m_draggedPack != null)
		{
			UnregisterUIEvents();
			return;
		}
		m_enableBackButton = true;
		m_BackButton.SetEnabled(enabled: true);
		m_StoreButton.SetEnabled(enabled: true);
		foreach (KeyValuePair<int, UnopenedPack> entry in m_unopenedPacks)
		{
			if (entry.Value != null)
			{
				entry.Value.SetEnabled(enabled: true);
			}
		}
	}

	private void UnregisterUIEvents()
	{
		m_enableBackButton = false;
		m_BackButton.SetEnabled(enabled: false);
		m_StoreButton.SetEnabled(enabled: false);
		foreach (KeyValuePair<int, UnopenedPack> entry in m_unopenedPacks)
		{
			if (entry.Value != null)
			{
				entry.Value.SetEnabled(enabled: false);
			}
		}
	}

	private void OnBackButtonPressed(UIEvent e)
	{
		Navigation.GoBack();
	}

	private void HoldPack(UnopenedPack selectedPack)
	{
		bool overPack = UniversalInputManager.Get().InputIsOver(selectedPack.gameObject);
		if (!selectedPack.CanOpenPack() || !overPack)
		{
			return;
		}
		m_selectedPack = selectedPack;
		DestroyHint();
		HideUnopenedPackTooltip();
		PegCursor.Get().SetMode(PegCursor.Mode.DRAG);
		m_DragPlane.SetActive(value: true);
		CreateDraggedPack(selectedPack);
		if (m_draggedPack != null)
		{
			TooltipPanel clonedKeyword = m_draggedPack.GetComponentInChildren<TooltipPanel>();
			if (clonedKeyword != null)
			{
				UnityEngine.Object.Destroy(clonedKeyword.gameObject);
			}
		}
		PickUpBooster();
		selectedPack.StopAlert();
		ShowHintOnSlot();
		m_Socket.OnPackHeld();
		m_SocketAccent.OnPackHeld();
		UpdateUIEvents();
		m_UnopenedPackScroller.Pause(pause: true);
	}

	private void TransferPacksBetweenStacks(UnopenedPack sourceStack, UnopenedPack heldStack, int numPacks)
	{
		if (!(sourceStack == null) && !(heldStack == null))
		{
			if (sourceStack.GetCount() - numPacks < 0)
			{
				numPacks = sourceStack.GetCount();
			}
			sourceStack.RemoveBooster(numPacks);
			heldStack.SetCount(heldStack.GetCount() + numPacks);
		}
	}

	private void ResetAfterLosingFocusOfGame()
	{
		m_isAutoOpenPackMode = false;
		m_inSpaceBarHeldDownModeTimer = -1f;
		m_holdToOpenPackReady = true;
		DropPack();
		m_autoOpenPending = false;
		if (m_autoOpenPackCoroutine != null)
		{
			StopCoroutine(m_autoOpenPackCoroutine);
			m_autoOpenPackCoroutine = null;
		}
		StopHooverCoroutine();
	}

	private void DropPack()
	{
		m_selectedPack = null;
		StopHooverCoroutine();
		if (MassPackOpeningEnabled())
		{
			TriggerHooverDeath();
		}
		PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);
		m_Socket.OnPackReleased();
		m_SocketAccent.OnPackReleased();
		bool num = UniversalInputManager.Get().InputIsOver(m_Socket.gameObject);
		if (num)
		{
			if (BattleNet.GetAccountCountry() == "KOR")
			{
				m_hasAcknowledgedKoreanWarning = true;
			}
			OpenBooster(m_draggedPack, m_draggedPack.GetCount());
			HideHint();
		}
		else
		{
			PutBackBooster();
			DestroyHint();
		}
		if (num && m_centerSocketBone != null && m_draggedPack != null)
		{
			SetUpCenterPack();
		}
		DestroyDraggedPack();
		UpdateUIEvents();
		m_DragPlane.SetActive(value: false);
		ShowHintOnUnopenedPack();
	}

	private void SetUpCenterPack()
	{
		if (!(m_draggedPack == null))
		{
			if (m_centerPack != null)
			{
				UnityEngine.Object.Destroy(m_centerPack);
			}
			m_centerPack = UnityEngine.Object.Instantiate(m_draggedPack.gameObject, m_centerSocketBone.transform);
			UnopenedPack centerPack = m_centerPack.GetComponent<UnopenedPack>();
			if (centerPack != null)
			{
				centerPack.SetUpToBeCenterPack(m_draggedPack.GetCount(), m_centerSocketBone.transform);
			}
			m_centerPack.SetActive(value: true);
		}
	}

	private bool ShowKoreanWarningTooltip(UIEvent e)
	{
		if (m_hasAcknowledgedKoreanWarning || BattleNet.GetAccountCountry() != "KOR")
		{
			return false;
		}
		TooltipZone tooltipZone = (e.GetElement() as UnopenedPack).GetComponent<TooltipZone>();
		if (tooltipZone == null)
		{
			return false;
		}
		tooltipZone.ShowTooltip(string.Empty, GameStrings.Get("GLUE_PACK_OPENING_TOOLTIP"), 5f);
		return true;
	}

	private bool ShowMassPackOpeningTooltip(UIEvent e)
	{
		if (HasSeenMassPackOpeningTooltip())
		{
			return false;
		}
		UnopenedPack unopenedPack = e.GetElement() as UnopenedPack;
		if (unopenedPack.GetBoosterId() == 629)
		{
			return false;
		}
		if (unopenedPack.GetCount() < 20)
		{
			return false;
		}
		TooltipZone tooltipZone = unopenedPack.GetComponent<TooltipZone>();
		if (tooltipZone == null)
		{
			return false;
		}
		string tooltipText = (UniversalInputManager.Get().IsTouchMode() ? GameStrings.Get("GLUE_MASS_PACK_OPEN_TOOLTIP_MOBILE") : GameStrings.Get("GLUE_MASS_PACK_OPEN_TOOLTIP_PC"));
		tooltipZone.ShowTooltip(string.Empty, tooltipText, 5f);
		return true;
	}

	private void AutomaticallyOpenPack()
	{
		if (PopupDisplayManager.Get().IsShowing || PopupDisplayManager.Get().RedundantNDERerollPopups.IsWaitingToShow())
		{
			m_autoOpenPending = false;
			return;
		}
		HideUnopenedPackTooltip();
		UnopenedPack openPack = null;
		if (!m_unopenedPacks.TryGetValue(m_lastOpenedBoosterId, out openPack) || openPack.GetCount() == 0)
		{
			foreach (int id in m_sortedPackIds)
			{
				if (m_unopenedPacks.ContainsKey(id))
				{
					UnopenedPack pack = m_unopenedPacks[id];
					if (!(pack == null) && pack.GetCount() > 0)
					{
						openPack = pack;
						break;
					}
				}
			}
		}
		if (!(openPack == null) && openPack.CanOpenPack())
		{
			if (m_draggedPack != null || m_InputBlocker.activeSelf)
			{
				m_autoOpenPending = false;
				return;
			}
			m_draggedPack = openPack.AcquireDraggedPack();
			PickUpBooster();
			openPack.StopAlert();
			OpenBooster(m_draggedPack);
			DestroyDraggedPack();
			UpdateUIEvents();
			m_DragPlane.SetActive(value: false);
		}
	}

	private void SpaceToHoover(UnopenedPack packToOpen)
	{
		if (!MassPackOpeningEnabled())
		{
			return;
		}
		HideUnopenedPackTooltip();
		if (!(packToOpen == null) && packToOpen.CanOpenPack() && IsMassPackOpenable(packToOpen))
		{
			if (m_draggedPack != null || m_InputBlocker.activeSelf)
			{
				m_autoOpenPending = false;
			}
			else if (packToOpen.GetCount() >= m_massPackOpeningHooverChunkSize)
			{
				m_selectedPack = packToOpen;
				m_draggedPack = packToOpen.AcquireDraggedPack();
				SetDraggedPackPositionForSpaceToHoover();
				PickUpBooster();
				packToOpen.StopAlert();
				UpdateUIEvents();
				m_DragPlane.SetActive(value: false);
			}
		}
	}

	private void SetDraggedPackPositionForSpaceToHoover()
	{
		if (!(m_selectedPack == null) && !(m_draggedPack == null))
		{
			Vector3 selectedPos = m_selectedPack.transform.position;
			Vector3 offset = new Vector3(selectedPos.x + m_spaceToHooverOffset.x, selectedPos.y + m_spaceToHooverOffset.y, selectedPos.z + m_spaceToHooverOffset.z);
			m_draggedPack.transform.position = offset;
		}
	}

	private void TriggerMassPackOpeningWithSpaceBar()
	{
		if (!(m_draggedPack == null))
		{
			if (BattleNet.GetAccountCountry() == "KOR")
			{
				m_hasAcknowledgedKoreanWarning = true;
			}
			OpenBooster(m_draggedPack, m_draggedPack.GetCount());
			HideHint();
			if (m_centerSocketBone != null && m_draggedPack != null)
			{
				SetUpCenterPack();
			}
			DestroyDraggedPack();
			UpdateUIEvents();
			m_DragPlane.SetActive(value: false);
		}
	}

	private void OnUnopenedPackPress(UIEvent e)
	{
		if ((e.GetElement() as UnopenedPack).GetBoosterId() == 18)
		{
			TemporaryAccountManager.Get().ShowHealUpDialog(GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_HEADER_01"), GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_BODY_03"), TemporaryAccountManager.HealUpReason.LOCKED_PACK, userTriggered: true, null);
		}
	}

	private void OnUnopenedPackDrag(UIEvent e)
	{
		HoldPack(e.GetElement() as UnopenedPack);
	}

	private void OnUnopenedPackRollover(UIEvent e)
	{
		if (!ShowKoreanWarningTooltip(e))
		{
			ShowMassPackOpeningTooltip(e);
		}
	}

	private void OnUnopenedPackRollout(UIEvent e)
	{
		HideUnopenedPackTooltip();
	}

	private void OnUnopenedPackReleaseAll(UIEvent e)
	{
		if (m_draggedPack == null)
		{
			if (!UniversalInputManager.Get().IsTouchMode() && ((UIReleaseAllEvent)e).GetMouseIsOver())
			{
				if ((e.GetElement() as UnopenedPack).GetBoosterId() == 18)
				{
					TemporaryAccountManager.Get().ShowHealUpDialog(GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_HEADER_01"), GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_DIALOG_BODY_03"), TemporaryAccountManager.HealUpReason.LOCKED_PACK, userTriggered: true, null);
				}
				else
				{
					HoldPack(e.GetElement() as UnopenedPack);
				}
			}
		}
		else
		{
			DropPack();
		}
	}

	private void HideUnopenedPackTooltip()
	{
		foreach (KeyValuePair<int, UnopenedPack> entry in m_unopenedPacks)
		{
			if (!(entry.Value == null))
			{
				entry.Value.GetComponent<TooltipZone>().HideTooltip();
			}
		}
	}

	private bool CanOpenPackAutomatically()
	{
		if (!Application.isFocused)
		{
			return false;
		}
		if (PopupDisplayManager.Get().IsShowing)
		{
			return false;
		}
		if (m_autoOpenPending)
		{
			return false;
		}
		if (!m_shown)
		{
			return false;
		}
		if (BoosterPackUtils.GetTotalBoosterCount() == 0)
		{
			return false;
		}
		if (m_director.IsPlaying() && !m_director.IsDoneButtonShown)
		{
			return false;
		}
		if (m_DragPlane.activeSelf)
		{
			return false;
		}
		if (StoreManager.Get().IsShownOrWaitingToShow())
		{
			return false;
		}
		if (m_hooverCoroutine != null)
		{
			return false;
		}
		return true;
	}

	private IEnumerator OpenNextPackWhenReady(UnopenedPack packToOpen)
	{
		float waitTime = 0f;
		if (m_director.IsPlaying())
		{
			waitTime = 1f;
		}
		while (m_director.IsPlaying())
		{
			yield return null;
		}
		yield return new WaitForSeconds(waitTime);
		if (packToOpen.GetCount() < m_massPackOpeningHooverChunkSize)
		{
			AutomaticallyOpenPack();
		}
		else if (m_isAutoOpenPackMode)
		{
			SpaceToHoover(packToOpen);
		}
		else
		{
			AutomaticallyOpenPack();
		}
	}

	private IEnumerator SetNoPacksLabel()
	{
		if ((m_draggedPack != null && m_draggedPack.GetCount() > 0) || !(m_NoPacksLabel != null))
		{
			yield break;
		}
		if (!m_NoPacksLabel.activeInHierarchy)
		{
			if (!HasOpenablePacks())
			{
				if (m_delayShowingNoPacksLabel)
				{
					yield return new WaitForSeconds(1f);
				}
				m_NoPacksLabel.SetActive(value: true);
				m_delayShowingNoPacksLabel = true;
			}
		}
		else if (HasOpenablePacks())
		{
			m_NoPacksLabel.SetActive(value: false);
		}
	}

	private bool HasOpenablePacks()
	{
		if (BoosterPackUtils.GetTotalBoosterCount() == 0)
		{
			return false;
		}
		if (m_unopenedPacks != null)
		{
			foreach (UnopenedPack pack in m_unopenedPacks.Values)
			{
				if (pack.GetCount() != 0 && pack.CanOpenPack())
				{
					return true;
				}
			}
		}
		return false;
	}

	private void OnPackOpeningCardFXLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_PackOpeningCardFX = go;
	}

	private void OnPackOpeningPortraitFXLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_PackOpeningPortraitFX = go;
	}

	private void OnPackOpeningCoinFXLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_PackOpeningCoinFX = go;
	}

	private void OnStoreButtonReleased(UIEvent e)
	{
		if (!m_StoreButton.IsVisualClosed())
		{
			TelemetryWatcher.StopWatchingFor(TelemetryWatcherWatchType.StoreFromPackOpening);
			TelemetryManager.Client().SendPackOpenToStore(PackOpenToStore.Path.PACK_OPENING_BUTTON);
			string targetTabId;
			switch (m_StoreButtonGamemodeTarget)
			{
			case IGamemodeAvailabilityService.Gamemode.HEARTHSTONE:
				targetTabId = "standard";
				break;
			case IGamemodeAvailabilityService.Gamemode.MERCENARIES:
				targetTabId = "mercs";
				break;
			default:
				targetTabId = string.Empty;
				Debug.LogWarning(string.Format("{0} Unhandled target gamemode for store button open action. Value: {1}", "PackOpening", m_StoreButtonGamemodeTarget));
				break;
			}
			StoreManager.Get().StartGeneralTransaction(targetTabId);
		}
	}

	private void EnablePackTrayMask()
	{
		if (!(m_PackTrayCameraMask == null))
		{
			m_PackTrayCameraMask.enabled = true;
		}
	}

	private void DisablePackTrayMask()
	{
		if (m_PackTrayCameraMask == null)
		{
			return;
		}
		Transform[] componentsInChildren = m_PackTrayCameraMask.m_ClipObjects.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			GameObject go = componentsInChildren[i].gameObject;
			if (!(go == null))
			{
				LayerUtils.SetLayer(go, GameLayer.Default);
			}
		}
		m_PackTrayCameraMask.enabled = false;
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		if (!m_director.IsPlaying())
		{
			NavigateToBoxAfterDisconnect();
		}
		else
		{
			m_director.OnDoneOpeningPack += OnDonePackOpening_FatalError;
		}
	}

	private void OnDonePackOpening_FatalError()
	{
		m_director.OnDoneOpeningPack -= OnDonePackOpening_FatalError;
		if (!Network.IsLoggedIn())
		{
			NavigateToBoxAfterDisconnect();
		}
	}

	private void OnDonePackOpening()
	{
		PopupDisplayManager.Get().RedundantNDERerollPopups.SuppressNDEPopups = false;
	}

	private void NavigateToBoxAfterDisconnect()
	{
		SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
		DialogManager.Get().ShowReconnectHelperDialog();
		Navigation.Clear();
	}

	public void OpenFakeCatchupPack(int numCards)
	{
		StartCoroutine(OpenFakeCatchupPackImpl(numCards));
	}

	private IEnumerator OpenFakeCatchupPackImpl(int numCards)
	{
		m_director.OnFinishedEvent += OnDirectorFinished;
		float fakeNetDelay = UnityEngine.Random.Range(0f, 1f);
		yield return new WaitForSeconds(fakeNetDelay);
		List<NetCache.BoosterCard> cards = new List<NetCache.BoosterCard>
		{
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "ETC_424",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "REV_750",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "CORE_ICC_097",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "JAM_011",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "TTN_960",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "ETC_424",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "REV_750",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "CORE_ICC_097",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "JAM_011",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "TTN_960",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "ETC_424",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "REV_750",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "CORE_ICC_097",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "JAM_011",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "TTN_960",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "ETC_424",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "REV_750",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "CORE_ICC_097",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "JAM_011",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "TTN_960",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "ETC_424",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "REV_750",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "CORE_ICC_097",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "JAM_011",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "TTN_960",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "ETC_424",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "REV_750",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "CORE_ICC_097",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "JAM_011",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "TTN_960",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "ETC_424",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "REV_750",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "CORE_ICC_097",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "JAM_011",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "TTN_960",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "ETC_424",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "REV_750",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "CORE_ICC_097",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "JAM_011",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "TTN_960",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "ETC_424",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "REV_750",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "CORE_ICC_097",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "JAM_011",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "TTN_960",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "ETC_424",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "REV_750",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "CORE_ICC_097",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "JAM_011",
					Premium = TAG_PREMIUM.NORMAL
				}
			},
			new NetCache.BoosterCard
			{
				Def = new NetCache.CardDefinition
				{
					Name = "TTN_960",
					Premium = TAG_PREMIUM.NORMAL
				}
			}
		};
		if (numCards > 0)
		{
			int numToRemove = cards.Count - numCards;
			if (numToRemove > 0)
			{
				cards.RemoveRange(numCards, numToRemove);
			}
		}
		if (cards.Count > 0)
		{
			m_director.OnBoosterOpened(cards, isCatchupPack: true);
		}
	}

	public static bool HasSeenMassPackOpeningTooltip()
	{
		if (!GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_FLAGS, GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_SEEN_MASS_PACK_OPENING_TOOLTIP, out long hasSeenMassPackOpeningTooltip))
		{
			return false;
		}
		return hasSeenMassPackOpeningTooltip != 0;
	}

	public static void MarkMassPackOpeningTooltipSeen()
	{
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_FLAGS, GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_SEEN_MASS_PACK_OPENING_TOOLTIP, 1L));
	}
}
