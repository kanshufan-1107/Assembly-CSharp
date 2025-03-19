using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.InGameMessage.UI;
using Hearthstone.Login;
using Hearthstone.Progression;
using PegasusShared;
using UnityEngine;

public class PopupDisplayManager : IHasUpdate, IService
{
	private static AchievementPopups s_achievementPopups;

	private static QuestPopups s_questPopups;

	private RewardPopups m_rewardPopups;

	private CardPopups m_cardPopups;

	private LoginPopups m_loginPopups;

	private RedundantNDERerollPopups m_redundantNDERerollPopups;

	private static bool s_isShowing;

	private static bool s_shouldShowRankedIntro;

	private static bool s_hasPlayerReachedHub;

	private static bool s_canShowQuestsDuringLoanerDeckRailroading;

	private bool m_readyToShowPopups;

	private bool m_hasShownMetaShakeupEventPopups;

	private bool m_hasCheckedNewPlayerSetRotationPopup;

	private bool m_showMessagesForVillageMailbox;

	private static float m_timePlayerInHubAfterLogin;

	private BannerManager.DelOnCloseBanner m_delOnCloseBanner = OnPopupClosed;

	private Action m_popClosedCallback = OnPopupClosed;

	private Func<bool> m_nextCompletedQuestFunc = ShowNextCompletedQuest;

	private Func<bool> m_nextRankedIntroFunc = ShowNextRankedIntro;

	private bool m_receivedPlayerData;

	public static bool SuppressPopupsTemporarily;

	public RewardPopups RewardPopups => m_rewardPopups;

	public AchievementPopups AchievementPopups => s_achievementPopups;

	public QuestPopups QuestPopups => s_questPopups;

	public CardPopups CardPopups => m_cardPopups;

	public LoginPopups LoginPopups => m_loginPopups;

	public RedundantNDERerollPopups RedundantNDERerollPopups => m_redundantNDERerollPopups;

	public static bool SuppressPopupsForNewPlayer { get; private set; }

	public static bool SuppressPopupsForReturningPlayerWarmUp { get; private set; }

	public bool IsShowing
	{
		get
		{
			if (s_isShowing)
			{
				return true;
			}
			if (DialogManager.Get() != null && DialogManager.Get().ShowingDialog())
			{
				return true;
			}
			if (WelcomeQuests.Get() != null)
			{
				return true;
			}
			if (NarrativeManager.Get() != null && NarrativeManager.Get().IsShowingBlockingDialog())
			{
				return true;
			}
			if (BannerManager.Get().IsShowing)
			{
				return true;
			}
			if (RewardXpNotificationManager.Get().IsShowingXpGains)
			{
				return true;
			}
			if (InitialDownloadDialog.IsVisible)
			{
				return true;
			}
			return false;
		}
	}

	private event Action OnAllPopupsShown = delegate
	{
	};

	private event Action OnPopupShown = delegate
	{
	};

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		m_rewardPopups = new RewardPopups(this, SetPopupShowingFlag, this.OnPopupShown, OnPopupClosed);
		s_achievementPopups = new AchievementPopups(this, m_rewardPopups.UpdateRewards);
		s_questPopups = new QuestPopups(SetPopupShowingFlag, this.OnPopupShown, OnPopupClosed, m_rewardPopups.DisplayLoadedRewardObject);
		m_cardPopups = new CardPopups();
		m_loginPopups = new LoginPopups();
		m_redundantNDERerollPopups = new RedundantNDERerollPopups(SetPopupShowingFlag, this.OnPopupShown, OnPopupClosed);
		HearthstoneApplication.Get().WillReset += WillReset;
		LoginManager.Get().OnFullLoginFlowComplete += InitializePlayerTimeInHubAfterLogin;
		m_timePlayerInHubAfterLogin = 0f;
		s_hasPlayerReachedHub = false;
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[7]
		{
			typeof(AssetLoader),
			typeof(Network),
			typeof(NetCache),
			typeof(AchieveManager),
			typeof(ReturningPlayerMgr),
			typeof(LoginManager),
			typeof(SetRotationManager)
		};
	}

	public void Shutdown()
	{
		if (ServiceManager.TryGet<LoginManager>(out var loginManager))
		{
			loginManager.OnFullLoginFlowComplete -= InitializePlayerTimeInHubAfterLogin;
		}
		HearthstoneApplication.Get().WillReset -= WillReset;
		m_rewardPopups.Dispose();
		s_achievementPopups.Dispose();
		s_questPopups.Dispose();
		m_loginPopups.Dispose();
		m_delOnCloseBanner = null;
		m_popClosedCallback = null;
		m_nextCompletedQuestFunc = null;
		m_nextRankedIntroFunc = null;
	}

	public void RegisterAllPopupsShownListener(Action callback)
	{
		if (callback != null)
		{
			OnAllPopupsShown -= callback;
			OnAllPopupsShown += callback;
		}
	}

	public void AddPopupShownListener(Action callback)
	{
		if (callback != null)
		{
			OnPopupShown -= callback;
			OnPopupShown += callback;
		}
	}

	public void RemovePopupShownListener(Action callback)
	{
		if (callback != null)
		{
			OnPopupShown -= callback;
		}
	}

	public void Update()
	{
		if (!m_receivedPlayerData)
		{
			Box box = Box.Get();
			if (NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>() != null && box != null && box.GetRailroadManager() != null && box.GetRailroadManager().IsRailroadStateReady)
			{
				m_receivedPlayerData = true;
				SuppressPopupsForNewPlayer = box.GetRailroadManager().ShouldSuppressPopups();
			}
		}
		if (SceneMgr.Get() == null)
		{
			return;
		}
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		if ((mode == SceneMgr.Mode.HUB && GameUtils.IsTraditionalTutorialComplete() && DialogManager.Get().ShowInitialDownloadPopupDuringDownload()) || !m_readyToShowPopups || mode == SceneMgr.Mode.GAMEPLAY || mode == SceneMgr.Mode.STARTUP || mode == SceneMgr.Mode.FATAL_ERROR)
		{
			return;
		}
		if (SuppressPopupsForNewPlayer && mode == SceneMgr.Mode.HUB)
		{
			Box box2 = Box.Get();
			if (box2 != null && box2.GetRailroadManager() != null)
			{
				SuppressPopupsForNewPlayer = box2.GetRailroadManager().ShouldSuppressPopups();
			}
		}
		if ((SceneMgr.Get().GetScene() != null && SceneMgr.Get().GetScene().IsBlockingPopupDisplayManager()) || (StoreManager.Get() != null && StoreManager.Get().IsPurchaseAuthViewShown()) || IsShowing)
		{
			return;
		}
		s_canShowQuestsDuringLoanerDeckRailroading = false;
		SuppressPopupsForReturningPlayerWarmUp = false;
		ReturningPlayerMgr returningPlayerMgr = ReturningPlayerMgr.Get();
		if (returningPlayerMgr != null && returningPlayerMgr.IsCNRPEActive)
		{
			SuppressPopupsForReturningPlayerWarmUp = returningPlayerMgr.SuppressPopupsDuringWarmUpPeriod;
			s_canShowQuestsDuringLoanerDeckRailroading = returningPlayerMgr.IsLoanerDeckRailroadingActive && !returningPlayerMgr.SuppressQuestPopupsDuringRailroading;
			if (returningPlayerMgr.ShowRPELoanerDeckMessageIfNeeded(OnPopupClosed))
			{
				this.OnPopupShown();
				s_isShowing = true;
				return;
			}
		}
		if (!SuppressPopupsTemporarily || s_canShowQuestsDuringLoanerDeckRailroading)
		{
			s_questPopups.ShowQuestProgressToasts(s_achievementPopups.ProgressedAchieves);
		}
		if (GameUtils.IsAnyTransitionActive())
		{
			return;
		}
		if (!m_hasCheckedNewPlayerSetRotationPopup)
		{
			m_hasCheckedNewPlayerSetRotationPopup = true;
			if (!SuppressPopupsForReturningPlayerWarmUp && SetRotationManager.Get().ShowNewPlayerSetRotationPopupIfNeeded())
			{
				return;
			}
		}
		if (returningPlayerMgr != null && returningPlayerMgr.ShowReturningPlayerWelcomeBannerIfNeeded(OnPopupClosed))
		{
			this.OnPopupShown();
			s_isShowing = true;
			return;
		}
		if (ShouldDisableNotificationOnLogin())
		{
			BannerManager.Get().AutoAcknowledgeOutstandingBanner();
		}
		else if (BannerManager.Get().ShowOutstandingBannerEvent(m_delOnCloseBanner))
		{
			this.OnPopupShown();
			s_isShowing = true;
			return;
		}
		if (DraftManager.Get() != null && DraftManager.Get().ShowNextArenaPopup(m_popClosedCallback))
		{
			this.OnPopupShown();
			s_isShowing = true;
		}
		else
		{
			if (!SuppressPopupsForReturningPlayerWarmUp && m_cardPopups.ShowChangedCards(shouldDisableNotificationOnLogin: false, UserAttentionBlocker.SET_ROTATION_INTRO))
			{
				return;
			}
			if (!m_hasShownMetaShakeupEventPopups && m_loginPopups.ShowLoginPopupSequence(SuppressPopupsForNewPlayer || SuppressPopupsForReturningPlayerWarmUp, ShouldDisableNotificationOnLogin(), m_cardPopups))
			{
				m_hasShownMetaShakeupEventPopups = true;
			}
			else if ((ShouldSuppressQuestPopups() || !s_questPopups.ShowAdjustedQuestsCompletedPopup()) && (SuppressPopupsTemporarily || !ShowRewardAndOtherPopups()) && (ShouldSuppressQuestPopups() || !s_questPopups.ShowNextQuestNotification()) && (ShouldSuppressInGameMessagePopups(mode) || !ShowInGameMessagePopups()) && (mode != SceneMgr.Mode.HUB || !ShowSkipHealupDialog()) && !ShowAppRatingPopup())
			{
				NarrativeManager.Get().OnAllPopupsShown();
				if (!IsShowing)
				{
					this.OnAllPopupsShown();
					ClearAllPopupsShownListeners();
				}
			}
		}
	}

	public static PopupDisplayManager Get()
	{
		return ServiceManager.Get<PopupDisplayManager>();
	}

	public void SetVillageMailboxMessageShouldShow(bool show)
	{
		m_showMessagesForVillageMailbox = show;
	}

	private void WillReset()
	{
		m_readyToShowPopups = false;
		s_isShowing = false;
		m_receivedPlayerData = false;
		m_hasShownMetaShakeupEventPopups = false;
		m_hasCheckedNewPlayerSetRotationPopup = false;
		SuppressPopupsForNewPlayer = false;
		SuppressPopupsTemporarily = false;
		SuppressPopupsForReturningPlayerWarmUp = false;
		m_showMessagesForVillageMailbox = false;
		if (ServiceManager.TryGet<LoginManager>(out var loginManager))
		{
			loginManager.OnFullLoginFlowComplete -= InitializePlayerTimeInHubAfterLogin;
		}
		if (ServiceManager.TryGet<UniversalInputManager>(out var inputMgr))
		{
			inputMgr.SetGameDialogActive(active: false);
		}
		DialogManager dialogManager = DialogManager.Get();
		if (dialogManager != null)
		{
			dialogManager.ReadyForSeasonEndPopup(ready: false);
			dialogManager.ClearHandledMedalNotices();
		}
		ClearAllPopupsShownListeners();
	}

	public void ReadyToShowPopups()
	{
		if (Network.IsLoggedIn())
		{
			m_readyToShowPopups = true;
			Update();
		}
	}

	public void ResetReadyToShowPopups()
	{
		m_readyToShowPopups = false;
	}

	public IEnumerator WaitForAllPopups()
	{
		bool allPopupsShown = false;
		RegisterAllPopupsShownListener(delegate
		{
			allPopupsShown = true;
		});
		while (!allPopupsShown)
		{
			yield return null;
		}
	}

	public IEnumerator<IAsyncJobResult> Job_WaitForAllPopups()
	{
		ReadyToShowPopups();
		bool allPopupsShown = false;
		RegisterAllPopupsShownListener(delegate
		{
			allPopupsShown = true;
		});
		while (!allPopupsShown)
		{
			yield return null;
		}
	}

	private void SetPopupShowingFlag(bool isShowing)
	{
		s_isShowing = isShowing;
	}

	private static void OnPopupClosed()
	{
		s_isShowing = false;
	}

	public void OnRewardPresenterScrollQueued(int rewardItemId)
	{
		m_redundantNDERerollPopups.OnRewardPresenterScrollQueued(rewardItemId);
	}

	public bool CanShowPopups()
	{
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY && (EndGameScreen.Get() == null || !EndGameScreen.Get().IsDoneDisplayingRewards()))
		{
			return false;
		}
		return true;
	}

	public static bool ShouldSuppressPopups()
	{
		if (!SuppressPopupsForNewPlayer)
		{
			return SuppressPopupsTemporarily;
		}
		return true;
	}

	private static bool ShouldSuppressQuestPopups()
	{
		if (ShouldSuppressPopups())
		{
			return !s_canShowQuestsDuringLoanerDeckRailroading;
		}
		return false;
	}

	public void ShowAnyOutstandingPopups()
	{
		ShowAnyOutstandingPopups(null);
	}

	public void ShowAnyOutstandingPopups(Action callback)
	{
		HashSet<Achieve.RewardTiming> rewardTimings = new HashSet<Achieve.RewardTiming>
		{
			Achieve.RewardTiming.IMMEDIATE,
			Achieve.RewardTiming.OUT_OF_BAND,
			Achieve.RewardTiming.ADVENTURE_CHEST
		};
		ShowAnyOutstandingPopups(rewardTimings, callback);
	}

	private void ShowAnyOutstandingPopups(HashSet<Achieve.RewardTiming> rewardTimings, Action callback)
	{
		s_achievementPopups.PrepareNewlyCompletedAchievesToBeShown(rewardTimings);
		if (callback != null)
		{
			RegisterAllPopupsShownListener(callback);
		}
		ReadyToShowPopups();
	}

	private void ClearAllPopupsShownListeners()
	{
		this.OnAllPopupsShown = delegate
		{
		};
	}

	public static bool CanShowRankedIntroForNewPlayer()
	{
		if (!GameUtils.HasCompletedApprentice())
		{
			return false;
		}
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.TOURNAMENT || !Options.GetInRankedPlayMode())
		{
			return false;
		}
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.RANKED_PLAY, GameSaveKeySubkeyId.RANKED_PLAY_INTRO_SEEN_COUNT, out long rankedIntroSeenCount);
		if (rankedIntroSeenCount > 0)
		{
			return false;
		}
		return true;
	}

	public void ShowRankedIntro()
	{
		s_shouldShowRankedIntro = true;
	}

	private static bool ShowNextCompletedQuest()
	{
		return s_questPopups.ShowNextCompletedQuest(s_achievementPopups.CompletedAchieves, ShouldSuppressQuestPopups());
	}

	private static bool ShowNextRankedIntro()
	{
		if (!s_shouldShowRankedIntro)
		{
			return false;
		}
		if (UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE) || !UserAttentionManager.CanShowAttentionGrabber("PopupDisplayManager.ShowNextRankedIntro"))
		{
			return false;
		}
		s_shouldShowRankedIntro = false;
		s_isShowing = true;
		MedalInfoTranslator mit = RankMgr.Get().GetLocalPlayerMedalInfo();
		DialogManager.Get().ShowBonusStarsPopup(mit.CreateDataModel(FormatType.FT_STANDARD, RankedMedal.DisplayMode.Default), OnPopupClosed);
		return true;
	}

	private bool ShowRewardAndOtherPopups()
	{
		if (m_rewardPopups.ShowRewardPopups(s_achievementPopups.CompletedAchieves, ShouldSuppressPopups(), m_nextRankedIntroFunc, m_nextCompletedQuestFunc))
		{
			return true;
		}
		if (m_redundantNDERerollPopups.ShowRerollPopup())
		{
			return true;
		}
		return false;
	}

	public static bool ShouldDisableNotificationOnLogin()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return false;
		}
		if (StoreManager.Get().IsShown())
		{
			return false;
		}
		if (!Options.Get().GetBool(Option.DISABLE_LOGIN_POPUPS))
		{
			return false;
		}
		if (!s_hasPlayerReachedHub)
		{
			return true;
		}
		return Time.realtimeSinceStartup - m_timePlayerInHubAfterLogin < 20f;
	}

	private void InitializePlayerTimeInHubAfterLogin()
	{
		s_hasPlayerReachedHub = true;
		m_timePlayerInHubAfterLogin = Time.realtimeSinceStartup;
	}

	private bool ShouldSuppressInGameMessagePopups(SceneMgr.Mode mode)
	{
		if (!ServiceManager.TryGet<MessagePopupDisplay>(out var messageUIDisplay))
		{
			return true;
		}
		if (JournalPopup.s_isShowing)
		{
			return true;
		}
		if (m_showMessagesForVillageMailbox)
		{
			return false;
		}
		if (ShouldSuppressPopups())
		{
			return true;
		}
		if (SuppressPopupsForReturningPlayerWarmUp && messageUIDisplay.CurrentPopupEvent != PopupEvent.OnBGLobby && messageUIDisplay.CurrentPopupEvent != PopupEvent.OnBGShop)
		{
			return true;
		}
		return false;
	}

	private bool ShowInGameMessagePopups()
	{
		if (ServiceManager.TryGet<MessagePopupDisplay>(out var messageUIDisplay))
		{
			if (messageUIDisplay.IsDisplayingMessage)
			{
				return true;
			}
			if (messageUIDisplay.HasMessageToDisplay)
			{
				s_isShowing = true;
				this.OnPopupShown();
				messageUIDisplay.DisplayIGMMessage(delegate
				{
					s_isShowing = false;
				});
				return true;
			}
		}
		return false;
	}

	private bool ShowAppRatingPopup()
	{
		if (MobileCallbackManager.IsAppRatingPromptQueued)
		{
			s_isShowing = true;
			this.OnPopupShown();
			MobileCallbackManager.m_onAppRatingPromptHidden = (Action)Delegate.Combine(MobileCallbackManager.m_onAppRatingPromptHidden, (Action)delegate
			{
				s_isShowing = false;
			});
			MobileCallbackManager.DisplayAppRatingPopup();
			return true;
		}
		return false;
	}

	private bool ShowSkipHealupDialog()
	{
		if (CreateSkipHelper.ShouldShowSkipScreenAtBox && CreateSkipHelper.ShowCreateSkipDialog(delegate
		{
			s_isShowing = false;
		}))
		{
			s_isShowing = true;
			this.OnPopupShown();
			return true;
		}
		return false;
	}
}
