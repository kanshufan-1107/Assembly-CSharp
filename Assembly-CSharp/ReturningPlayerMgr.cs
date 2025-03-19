using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Util;
using PegasusShared;
using PegasusUtil;

public class ReturningPlayerMgr : IService
{
	private ReturningPlayerStatus m_returningPlayerStatus;

	private int m_returningPlayerLoginCount;

	private bool m_hasSeenLoanerDecks;

	private bool m_isLoanerDeckRailroadingActive;

	public bool IsInReturningPlayerMode
	{
		get
		{
			if (m_returningPlayerStatus != ReturningPlayerStatus.RPS_ACTIVE)
			{
				return m_returningPlayerStatus == ReturningPlayerStatus.RPS_ACTIVE_WITH_MANY_LOSSES;
			}
			return true;
		}
	}

	public bool SuppressOldPopups
	{
		get
		{
			if (!IsInReturningPlayerMode)
			{
				return SuppressPopupsDuringWarmUpPeriod;
			}
			return true;
		}
	}

	public bool SuppressPopupsDuringWarmUpPeriod
	{
		get
		{
			if (!IsCNRPEActive)
			{
				return false;
			}
			if (m_returningPlayerStatus == ReturningPlayerStatus.RPS_NOT_RETURNING_PLAYER || m_returningPlayerStatus == ReturningPlayerStatus.RPS_COMPLETE || m_returningPlayerStatus == ReturningPlayerStatus.RPS_UNKNOWN)
			{
				return false;
			}
			NetCache.NetCacheFeatures guardianVars = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>();
			if (guardianVars == null || !guardianVars.ReturningPlayer.LoginCountNoticeSupressionEnabled)
			{
				return false;
			}
			if (m_returningPlayerLoginCount > guardianVars.ReturningPlayer.NoticeSuppressionLoginThreshold)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsCNRPEActive
	{
		get
		{
			if (!RegionUtils.IsCNLegalRegion)
			{
				return false;
			}
			return EventTimingManager.Get().IsEventActive(EventTimingType.CN_RPE);
		}
	}

	public bool IsLoanerDeckRailroadingActive
	{
		get
		{
			return m_isLoanerDeckRailroadingActive;
		}
		private set
		{
			m_isLoanerDeckRailroadingActive = value;
			PopupDisplayManager.SuppressPopupsTemporarily = value;
		}
	}

	public bool SuppressQuestPopupsDuringRailroading { get; private set; }

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		serviceLocator.Get<Network>().RegisterNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[4]
		{
			typeof(Network),
			typeof(EventTimingManager),
			typeof(SetRotationManager),
			typeof(SceneMgr)
		};
	}

	public void Shutdown()
	{
	}

	private void WillReset()
	{
		m_returningPlayerStatus = ReturningPlayerStatus.RPS_UNKNOWN;
		m_returningPlayerLoginCount = 0;
		m_hasSeenLoanerDecks = false;
	}

	public static ReturningPlayerMgr Get()
	{
		return ServiceManager.Get<ReturningPlayerMgr>();
	}

	public bool ShowReturningPlayerWelcomeBannerIfNeeded(Action callback)
	{
		if (!ShouldShowReturningPlayerWelcomeBanner())
		{
			return false;
		}
		BannerManager.Get().ShowBanner("WoodenSign_Paint_Welcome_Back.prefab:4cb64d2b8c67feb45b4e17042d58f1ba", null, GameStrings.Get("GLUE_RETURNING_PLAYER_WELCOME_DESC"), delegate
		{
			callback();
		});
		GameUtils.SetGSDFlag(GameSaveKeyId.RETURNING_PLAYER_EXPERIENCE, GameSaveKeySubkeyId.RETURNING_PLAYER_SEEN_BANNER, enableFlag: true);
		return true;
	}

	public bool PlayReturningPlayerInnkeeperGreetingIfNeeded()
	{
		if (!ShouldShowReturningPlayerWelcomeBanner())
		{
			return false;
		}
		SoundManager.Get().LoadAndPlay("VO_Innkeeper_Male_Dwarf_ReturningPlayers_01.prefab:cd3f8a594d06834408cb5a119aa33a21");
		return true;
	}

	public bool ShowRPELoanerDeckMessageIfNeeded(Action popupDismissedCallback)
	{
		if (!ShouldShowWelcomeLoanerDeckMessage())
		{
			return false;
		}
		BasicPopup.PopupInfo popupInfo = new BasicPopup.PopupInfo();
		popupInfo.m_prefabAssetRefs.Add("WelcomeBackLoanerPopup.prefab:39153054341f1b4499acff3f953e8c6f");
		popupInfo.m_disableBnetBar = true;
		popupInfo.m_blurWhenShown = true;
		popupInfo.m_responseCallback = delegate
		{
			SceneMgr.Get().RegisterSceneLoadedEvent(HandleSceneLoadedDuringLoanerDeckIntro);
			Box box = Box.Get();
			if (box != null && SetRotationManager.Get().ShouldShowSetRotationIntro())
			{
				box.UpdateStateForCurrentSceneMode();
			}
			if (box == null || !UserAttentionManager.IsBlockedBy(UserAttentionBlocker.SET_ROTATION_INTRO))
			{
				DeepLinkManager.ExecuteDeepLink(new string[2] { "ranked", "FT_STANDARD" }, DeepLinkManager.DeepLinkSource.IN_GAME_MESSAGE, 0);
			}
			GameUtils.SetGSDFlag(GameSaveKeyId.RETURNING_PLAYER_EXPERIENCE, GameSaveKeySubkeyId.RETURNING_PLAYER_SEEN_WELCOME_LOANER_MSG, enableFlag: true);
			UserAttentionManager.StopBlocking(UserAttentionBlocker.RPE_LOANER_MESSAGE);
			popupDismissedCallback?.Invoke();
		};
		UserAttentionManager.StartBlocking(UserAttentionBlocker.RPE_LOANER_MESSAGE);
		IsLoanerDeckRailroadingActive = true;
		SuppressQuestPopupsDuringRailroading = true;
		DialogManager.Get().ShowBasicPopup(UserAttentionBlocker.RPE_LOANER_MESSAGE, popupInfo);
		return true;
	}

	private void HandleSceneLoadedDuringLoanerDeckIntro(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (!IsLoanerDeckRailroadingActive)
		{
			SceneMgr.Get().UnregisterSceneUnloadedEvent(HandleSceneLoadedDuringLoanerDeckIntro);
			return;
		}
		if (mode == SceneMgr.Mode.TOURNAMENT && !m_hasSeenLoanerDecks)
		{
			m_hasSeenLoanerDecks = true;
			return;
		}
		if (m_hasSeenLoanerDecks && mode != SceneMgr.Mode.TOURNAMENT && mode != SceneMgr.Mode.COLLECTIONMANAGER)
		{
			SuppressQuestPopupsDuringRailroading = false;
		}
		if (m_hasSeenLoanerDecks && (SceneMgr.Get().GetMode() == SceneMgr.Mode.HUB || SceneMgr.Get().GetNextMode() == SceneMgr.Mode.HUB))
		{
			SceneMgr.Get().UnregisterSceneUnloadedEvent(HandleSceneLoadedDuringLoanerDeckIntro);
			IsLoanerDeckRailroadingActive = false;
		}
	}

	private bool ShouldShowWelcomeLoanerDeckMessage()
	{
		if (!IsCNRPEActive)
		{
			return false;
		}
		if (!IsInReturningPlayerMode && !RankMgr.Get().DidSkipApprenticeThisSession)
		{
			return false;
		}
		if (HasSeenWelcomeLoanerDeckMessage())
		{
			return false;
		}
		return true;
	}

	private bool HasSeenWelcomeLoanerDeckMessage()
	{
		if (m_hasSeenLoanerDecks)
		{
			return true;
		}
		return GameUtils.IsGSDFlagSet(GameSaveKeyId.RETURNING_PLAYER_EXPERIENCE, GameSaveKeySubkeyId.RETURNING_PLAYER_SEEN_WELCOME_LOANER_MSG);
	}

	private void OnInitialClientState()
	{
		InitialClientState packet = Network.Get().GetInitialClientState();
		if (packet != null)
		{
			if (packet.HasReturningPlayerInfo)
			{
				m_returningPlayerStatus = packet.ReturningPlayerInfo.Status;
				m_returningPlayerLoginCount = Convert.ToInt32(packet.ReturningPlayerInfo.LoginCount);
				Log.All.PrintInfo($"Received player state - RPStatus: {m_returningPlayerStatus}, LoginCount: {m_returningPlayerLoginCount}, Region: {RegionUtils.CurrentRegion}");
			}
			m_hasSeenLoanerDecks = HasSeenWelcomeLoanerDeckMessage();
		}
	}

	private bool ShouldShowReturningPlayerWelcomeBanner()
	{
		if (!IsInReturningPlayerMode)
		{
			return false;
		}
		if (GameUtils.IsGSDFlagSet(GameSaveKeyId.RETURNING_PLAYER_EXPERIENCE, GameSaveKeySubkeyId.RETURNING_PLAYER_SEEN_BANNER))
		{
			return false;
		}
		if (IsCNRPEActive)
		{
			if (!HasSeenWelcomeLoanerDeckMessage())
			{
				return false;
			}
			if (!m_hasSeenLoanerDecks)
			{
				return false;
			}
			if (IsLoanerDeckRailroadingActive)
			{
				return false;
			}
		}
		return true;
	}
}
