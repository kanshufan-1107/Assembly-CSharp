using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets;
using Blizzard.T5.Services;
using Hearthstone.Core.Streaming;
using Hearthstone.Login;
using Hearthstone.Progression;
using Hearthstone.Streaming;
using Hearthstone.UI;
using PegasusLettuce;
using PegasusShared;
using SpectatorProto;
using UnityEngine;

[CustomEditClass]
public class EndGameScreen : MonoBehaviour
{
	private class ShowNextCompletedQuestLock : IDisposable
	{
		private readonly EndGameScreen m_endGameScreen;

		public ShowNextCompletedQuestLock(EndGameScreen endGameScreen)
		{
			m_endGameScreen = endGameScreen;
			m_endGameScreen.m_showingNextCompletedQuest = true;
		}

		public void Dispose()
		{
			m_endGameScreen.m_showingNextCompletedQuest = false;
		}
	}

	public delegate void OnTwoScoopsShownHandler(bool shown, EndGameTwoScoop twoScoops);

	public EndGameTwoScoop m_twoScoop;

	public PegUIElement m_hitbox;

	public UberText m_noGoldRewardText;

	public UberText m_continueText;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string m_ScoreScreenPrefab;

	public static OnTwoScoopsShownHandler OnTwoScoopsShown;

	public static Action OnBackOutOfGameplay;

	private static EndGameScreen s_instance;

	private bool m_shown;

	private bool m_netCacheReady;

	private bool m_achievesReady;

	private bool m_heroRewardEventReady;

	private bool m_showingNextCompletedQuest;

	protected List<Achievement> m_completedQuests = new List<Achievement>();

	private bool m_isShowingFixedRewards;

	private List<Reward> m_rewards = new List<Reward>();

	private int m_numRewardsToLoad;

	private bool m_rewardsLoaded;

	private List<Reward> m_genericRewards = new List<Reward>();

	private HashSet<long> m_genericRewardChestNoticeIdsReady = new HashSet<long>();

	private Reward m_currentlyShowingReward;

	private bool m_haveShownTwoScoop;

	private bool m_hasAlreadySetMode;

	private int m_inputBlocker;

	private bool m_playingBlockingAnim;

	private bool m_doneDisplayingRewards;

	private bool m_showingScoreScreen;

	private ScoreScreen m_scoreScreen;

	private GameObject m_rankChangeTwoScoop;

	private bool m_rankChangeReady;

	private bool m_medalInfoUpdated;

	private const int MEDAL_INFO_RETRY_COUNT_MAX = 3;

	private const float MEDAL_INFO_RETRY_INITIAL_DELAY = 1f;

	private int m_medalInfoRetryCount;

	private float m_medalInfoRetryDelay;

	private bool m_shouldShowRankChange;

	private bool m_isShowingRankChange;

	private bool m_hasSentRankedInitTelemetry;

	private float m_endGameScreenStartTime;

	private Widget m_rankedRewardDisplayWidget;

	private RankedRewardDisplay m_rankedRewardDisplay;

	private bool m_isShowingRankedReward;

	private List<List<RewardData>> m_rankedRewardsToDisplay = new List<List<RewardData>>();

	private Widget m_rankedCardBackProgressWidget;

	private RankedCardBackProgressDisplay m_rankedCardBackProgress;

	private bool m_shouldShowRankedCardBackProgress;

	private bool m_isShowingRankedCardBackProgress;

	private bool m_isShowingTrackRewards;

	private bool m_shouldShowRewardXpGains;

	private bool m_isShowingMercenariesExperienceRewards;

	private bool m_finishedShowingMercenariesExperienceRewards;

	private bool m_hasTimedOutAndLogged;

	private float m_timeoutTimerStartTime;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private const float m_maxWaitTime = 5f;

	protected virtual void Awake()
	{
		s_instance = this;
		if (GameMgr.Get().IsBattlegrounds())
		{
			m_netCacheReady = true;
		}
		StartCoroutine(WaitForAchieveManager());
		ProcessPreviousAchievements();
		AchieveManager.Get().RegisterAchievesUpdatedListener(OnAchievesUpdated);
		m_shouldShowRankChange = !GameMgr.Get().IsSpectator() && GameMgr.Get().IsPlay() && Options.Get().GetBool(Option.IN_RANKED_PLAY_MODE);
		m_hitbox.gameObject.SetActive(value: false);
		string continueText = "GLOBAL_CLICK_TO_CONTINUE";
		if (UniversalInputManager.Get().IsTouchMode())
		{
			continueText = "GLOBAL_CLICK_TO_CONTINUE_TOUCH";
		}
		m_continueText.Text = GameStrings.Get(continueText);
		m_continueText.gameObject.SetActive(value: false);
		m_noGoldRewardText.gameObject.SetActive(value: false);
		PegUI.Get().AddInputCamera(CameraUtils.FindFirstByLayer(GameLayer.IgnoreFullScreenEffects));
		LayerUtils.SetLayer(m_hitbox.gameObject, GameLayer.IgnoreFullScreenEffects);
		LayerUtils.SetLayer(m_continueText.gameObject, GameLayer.IgnoreFullScreenEffects);
		if (!Network.ShouldBeConnectedToAurora())
		{
			UpdateRewards();
		}
		m_genericRewardChestNoticeIdsReady = GenericRewardChestNoticeManager.Get().GetReadyGenericRewardChestNotices();
		GenericRewardChestNoticeManager.Get().RegisterRewardsUpdatedListener(OnGenericRewardUpdated);
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	protected virtual void OnDestroy()
	{
		if (NetCache.Get() != null)
		{
			NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheMedalInfo), OnMedalInfoUpdate);
		}
		if (OnTwoScoopsShown != null)
		{
			OnTwoScoopsShown(shown: false, m_twoScoop);
		}
		if (AchieveManager.Get() != null)
		{
			AchieveManager.Get().RemoveAchievesUpdatedListener(OnAchievesUpdated);
		}
		if (GenericRewardChestNoticeManager.Get() != null)
		{
			GenericRewardChestNoticeManager.Get().RemoveRewardsUpdatedListener(OnGenericRewardUpdated);
		}
		m_screenEffectsHandle.StopEffect();
		s_instance = null;
	}

	public static EndGameScreen Get()
	{
		return s_instance;
	}

	public virtual void Show()
	{
		if (GameState.Get() == null || !GameState.Get().WasRestartRequested())
		{
			m_shown = true;
			m_endGameScreenStartTime = Time.time;
			Network.Get().DisconnectFromGameServer(Network.DisconnectReason.EndGameScreen);
			InputManager.Get().DisableInput();
			m_hitbox.gameObject.SetActive(value: true);
			m_screenEffectsHandle.StartEffect(ScreenEffectParameters.BlurVignetteDesaturatePerspective);
			if (GameState.Get() != null && GameState.Get().GetFriendlySidePlayer() != null)
			{
				GameState.Get().GetFriendlySidePlayer().GetHandZone()
					.UpdateLayout(null);
			}
			ShowScoreScreen();
			StartCoroutine(ShowStandardFlowIfReady());
		}
	}

	public void SetPlayingBlockingAnim(bool set)
	{
		m_playingBlockingAnim = set;
	}

	public bool IsPlayingBlockingAnim()
	{
		return m_playingBlockingAnim;
	}

	public void AddInputBlocker()
	{
		m_inputBlocker++;
	}

	public void RemoveInputBlocker()
	{
		m_inputBlocker--;
	}

	private bool IsInputBlocked()
	{
		return m_inputBlocker > 0;
	}

	public bool IsScoreScreenShown()
	{
		return m_showingScoreScreen;
	}

	private void ShowTutorialProgress()
	{
		HideTwoScoop();
		if (GameMgr.Get().GetMissionId() == 5290 && TutorialProgressScreen.HasEverOpenedRewardChest() && NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>().CampaignProgress == TutorialProgress.LICH_KING_COMPLETE)
		{
			LoadingScreen.Get().SetFadeColor(Color.white);
			NavigateToHubForFirstTime();
		}
		else
		{
			StartCoroutine(LoadTutorialProgress());
		}
	}

	private IEnumerator LoadTutorialProgress()
	{
		yield return new WaitForSeconds(0.25f);
		AssetLoader.Get().InstantiatePrefab("TutorialInterstitialPopup.prefab:a5140e1dc7b29634cb548f42574bce5b", OnTutorialProgressScreenCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void OnTutorialProgressScreenCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		go.transform.parent = base.transform;
		TutorialProgressScreen tutorialProgressScreen = go.GetComponent<TutorialProgressScreen>();
		if (tutorialProgressScreen != null)
		{
			tutorialProgressScreen.SetEndGameScreen(this);
			tutorialProgressScreen.StartTutorialProgress();
		}
	}

	public void NavigateToHubForFirstTime()
	{
		ContinueButtonPress_FirstTimeHub(null);
	}

	protected void ContinueButtonPress_FirstTimeHub(UIEvent e)
	{
		if (!HasShownScoops())
		{
			return;
		}
		HideTwoScoop();
		if (ShowNextReward())
		{
			SoundManager.Get().LoadAndPlay("VO_INNKEEPER_TUT_COMPLETE_05.prefab:c8d19a552e18c7c429946f62102c9460");
		}
		else
		{
			if (ShowNextCompletedQuest())
			{
				return;
			}
			ContinueButtonPress_Common();
			m_hitbox.RemoveEventListener(UIEventType.RELEASE, ContinueButtonPress_FirstTimeHub);
			if (Network.ShouldBeConnectedToAurora())
			{
				if (!DialogManager.Get().ShowInitialDownloadPopupDuringDownload())
				{
					BackToMode(SceneMgr.Mode.HUB);
				}
			}
			else
			{
				NotificationManager.Get().CreateTutorialDialog("GLOBAL_MEDAL_REWARD_CONGRATULATIONS", "TUTORIAL_MOBILE_COMPLETE_CONGRATS", "GLOBAL_OKAY", UserPressedStartButton, new UnityEngine.Vector2(0.5f, 0f), swapMaterial: true);
				m_hitbox.gameObject.SetActive(value: false);
				m_continueText.gameObject.SetActive(value: false);
			}
		}
	}

	protected void UserPressedStartButton(UIEvent e)
	{
		ServiceManager.Get<ILoginService>()?.ClearAuthentication();
		BackToMode(SceneMgr.Mode.RESET);
	}

	protected void ContinueButtonPress_Common()
	{
		LoadingScreen.Get().AddTransitionObject(this);
	}

	protected void ContinueButtonPress_ProceedToError(UIEvent e)
	{
		if (!IsPlayingBlockingAnim())
		{
			HideScoreScreen();
			m_hitbox.RemoveEventListener(UIEventType.RELEASE, ContinueButtonPress_ProceedToError);
		}
	}

	protected void ContinueButtonPress_PrevMode(UIEvent e)
	{
		ContinueEvents();
	}

	public bool ContinueEvents()
	{
		if (!GameMgr.Get().IsBattlegrounds() && !GameMgr.Get().IsMercenaries() && GameMgr.Get().IsTraditionalTutorial())
		{
			if (!GameUtils.IsTraditionalTutorialComplete())
			{
				ContinueTutorialEvents();
				return true;
			}
			if (!TutorialProgressScreen.HasEverOpenedRewardChest())
			{
				ContinueTutorialEvents();
				return true;
			}
		}
		if (ContinueDefaultEvents())
		{
			return true;
		}
		if (m_twoScoop == null)
		{
			return false;
		}
		PlayMakerFSM pmDeath = m_twoScoop.GetComponent<PlayMakerFSM>();
		if (pmDeath != null)
		{
			pmDeath.SendEvent("Death");
		}
		ContinueButtonPress_Common();
		m_hitbox.RemoveEventListener(UIEventType.RELEASE, ContinueButtonPress_PrevMode);
		ReturnToPreviousMode();
		return false;
	}

	protected void ContinueButtonPress_TutorialProgress(UIEvent e)
	{
		ContinueTutorialEvents();
	}

	public void ContinueTutorialEvents()
	{
		if (!ContinueDefaultEvents())
		{
			ContinueButtonPress_Common();
			m_hitbox.RemoveEventListener(UIEventType.RELEASE, ContinueButtonPress_TutorialProgress);
			m_continueText.gameObject.SetActive(value: false);
			ShowTutorialProgress();
		}
	}

	private bool ContinueDefaultEvents()
	{
		if (IsPlayingBlockingAnim())
		{
			return true;
		}
		if (IsInputBlocked())
		{
			return true;
		}
		if (m_currentlyShowingReward != null)
		{
			m_currentlyShowingReward.Hide(animate: true);
			m_currentlyShowingReward = null;
		}
		HideScoreScreen();
		if (!m_haveShownTwoScoop)
		{
			return true;
		}
		HideTwoScoop();
		if (ShowHeroRewardEvent() && m_heroRewardEventReady)
		{
			return true;
		}
		if (ShowRewardTrackXpGains())
		{
			return true;
		}
		if (ShowNextRewardTrackAutoClaimedReward())
		{
			return true;
		}
		if (ShowFixedRewards())
		{
			return true;
		}
		if (ShowGoldReward())
		{
			return true;
		}
		if (ShowRankedCardBackProgress())
		{
			return true;
		}
		if (ShowRankChange())
		{
			return true;
		}
		if (ShowRankedRewards())
		{
			return true;
		}
		if (ShowNextProgressionQuestReward())
		{
			return true;
		}
		if (ShowMercenariesExperienceRewards())
		{
			return true;
		}
		if (ShowNextCompletedQuest())
		{
			return true;
		}
		if (ShowNextReward())
		{
			return true;
		}
		if (ShowNextGenericReward())
		{
			return true;
		}
		if (!SpectatorManager.Get().IsSpectatingOrWatching && TemporaryAccountManager.IsTemporaryAccount() && ShowHealUpDialog())
		{
			return true;
		}
		if (ShowPushNotificationPrompt())
		{
			return true;
		}
		ShowAppRatingPrompt();
		m_doneDisplayingRewards = true;
		return false;
	}

	protected virtual void OnTwoScoopShown()
	{
	}

	protected virtual void OnTwoScoopHidden()
	{
	}

	protected virtual void InitGoldRewardUI()
	{
	}

	private static string GetFriendlyChallengeRewardMessage(Achievement achieve)
	{
		if (DemoMgr.Get().IsDemo())
		{
			return null;
		}
		string goldRewardMsg = null;
		if (achieve.DbfRecord.MaxDefense > 0)
		{
			goldRewardMsg = GetFriendlyChallengeEarlyConcedeMessage(achieve.DbfRecord.MaxDefense);
			if (!string.IsNullOrEmpty(goldRewardMsg))
			{
				return goldRewardMsg;
			}
		}
		AchieveRegionDataDbfRecord regionData = achieve.GetCurrentRegionData();
		if (regionData != null && regionData.RewardableLimit > 0 && achieve.IntervalRewardStartDate > 0)
		{
			DateTime intervalStart = DateTime.FromFileTimeUtc(achieve.IntervalRewardStartDate);
			if ((DateTime.UtcNow - intervalStart).TotalDays < regionData.RewardableInterval && achieve.IntervalRewardCount >= regionData.RewardableLimit)
			{
				goldRewardMsg = GameStrings.Get("GLOBAL_FRIENDLYCHALLENGE_QUEST_REWARD_AT_LIMIT");
			}
		}
		if (string.IsNullOrEmpty(goldRewardMsg) && regionData != null && regionData.RewardableLimit > 0 && FriendChallengeMgr.Get().DidReceiveChallenge())
		{
			achieve.IncrementIntervalRewardCount();
		}
		return goldRewardMsg;
	}

	protected static string GetFriendlyChallengeRewardText()
	{
		if (!FriendChallengeMgr.Get().HasChallenge())
		{
			return null;
		}
		if (DemoMgr.Get().IsDemo())
		{
			return null;
		}
		string goldRewardMsg = null;
		AchieveManager achieveManager = AchieveManager.Get();
		PartyQuestInfo partyQuests = FriendChallengeMgr.Get().GetPartyQuestInfo();
		if (partyQuests != null)
		{
			bool num = FriendChallengeMgr.Get().DidSendChallenge();
			bool isChallengee = FriendChallengeMgr.Get().DidReceiveChallenge();
			PlayerType playerType = PlayerType.PT_ANY;
			if (num)
			{
				playerType = PlayerType.PT_FRIENDLY_CHALLENGER;
			}
			if (isChallengee)
			{
				playerType = PlayerType.PT_FRIENDLY_CHALLENGEE;
			}
			for (int i = 0; i < partyQuests.QuestIds.Count; i++)
			{
				Achievement achieve = achieveManager.GetAchievement(partyQuests.QuestIds[i]);
				if (achieve != null && achieve.IsValidFriendlyPlayerChallengeType(playerType))
				{
					goldRewardMsg = GetFriendlyChallengeRewardMessage(achieve);
				}
				if (string.IsNullOrEmpty(goldRewardMsg))
				{
					Achievement sharedAchieve = achieveManager.GetAchievement(achieve.DbfRecord.SharedAchieveId);
					if (sharedAchieve != null && sharedAchieve.IsValidFriendlyPlayerChallengeType(playerType))
					{
						goldRewardMsg = GetFriendlyChallengeRewardMessage(sharedAchieve);
					}
				}
			}
		}
		if (string.IsNullOrEmpty(goldRewardMsg) && EventTimingManager.Get().IsEventActive(EventTimingType.FRIEND_WEEK))
		{
			NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
			bool num2 = (from a in achieveManager.GetActiveQuests()
				where a.IsAffectedByFriendWeek && (a.AchieveTrigger == Achieve.Trigger.WIN || a.AchieveTrigger == Achieve.Trigger.FINISH) && a.GameModeRequiresNonFriendlyChallenge
				select a).Any();
			bool gameIsWorkingTowardTavernBrawlReward = false;
			if (FriendChallengeMgr.Get().IsChallengeTavernBrawl() && guardianVars != null && guardianVars.FriendWeekAllowsTavernBrawlRecordUpdate)
			{
				BrawlType brawlType = FriendChallengeMgr.Get().GetChallengeBrawlType();
				TavernBrawlMission mission = TavernBrawlManager.Get().GetMission(brawlType);
				TavernBrawlPlayerRecord record = TavernBrawlManager.Get().GetRecord(brawlType);
				bool hasWinOrFinishTrigger = mission != null && (mission.rewardTrigger == RewardTrigger.REWARD_TRIGGER_WIN_GAME || mission.rewardTrigger == RewardTrigger.REWARD_TRIGGER_FINISH_GAME);
				if (mission != null && mission.rewardType != RewardType.REWARD_UNKNOWN && hasWinOrFinishTrigger && record != null && record.RewardProgress < mission.RewardTriggerQuota)
				{
					gameIsWorkingTowardTavernBrawlReward = true;
				}
			}
			if (!num2 && !gameIsWorkingTowardTavernBrawlReward)
			{
				return null;
			}
			int concederMaxDefense = 0;
			if (guardianVars != null)
			{
				concederMaxDefense = guardianVars.FriendWeekConcederMaxDefense;
			}
			goldRewardMsg = GetFriendlyChallengeEarlyConcedeMessage(concederMaxDefense);
		}
		return goldRewardMsg;
	}

	private static string GetFriendlyChallengeEarlyConcedeMessage(int concederMaxDefense)
	{
		if (DemoMgr.Get().IsDemo())
		{
			return null;
		}
		int minTotalTurns = 0;
		NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (guardianVars != null)
		{
			minTotalTurns = guardianVars.FriendWeekConcededGameMinTotalTurns;
		}
		string stringKey = null;
		int remainingDefense = 0;
		GameState gameState = GameState.Get();
		bool isConcededGame = false;
		foreach (KeyValuePair<int, Player> item in gameState.GetPlayerMap())
		{
			Player player = item.Value;
			TAG_PLAYSTATE playState = player.GetPreGameOverPlayState();
			if (playState == TAG_PLAYSTATE.CONCEDED || playState == TAG_PLAYSTATE.DISCONNECTED)
			{
				isConcededGame = true;
				Entity hero = player.GetHero();
				if (hero != null)
				{
					remainingDefense = hero.GetCurrentDefense();
					stringKey = ((player.GetSide() != Player.Side.FRIENDLY) ? "GLOBAL_FRIENDLYCHALLENGE_REWARD_CONCEDED_YOUR_OPPONENT" : "GLOBAL_FRIENDLYCHALLENGE_REWARD_CONCEDED_YOURSELF");
					break;
				}
			}
		}
		bool checkConcederDefense = concederMaxDefense > 0;
		bool concededGameIsEligibleOnMaxDefense = !isConcededGame || (checkConcederDefense && remainingDefense <= concederMaxDefense);
		bool concededGameIsEligibleOnTurnNumber = !isConcededGame || gameState.GetTurn() >= minTotalTurns;
		if (!concededGameIsEligibleOnMaxDefense && !concededGameIsEligibleOnTurnNumber)
		{
			return GameStrings.Get(stringKey);
		}
		return null;
	}

	protected void BackToMode(SceneMgr.Mode mode)
	{
		AchieveManager.Get().RemoveAchievesUpdatedListener(OnAchievesUpdated);
		HideTwoScoop();
		if (OnBackOutOfGameplay != null)
		{
			OnBackOutOfGameplay();
		}
		if (!m_hasAlreadySetMode)
		{
			m_hasAlreadySetMode = true;
			StartCoroutine(ToMode(mode));
			Navigation.Clear();
		}
	}

	private IEnumerator ToMode(SceneMgr.Mode mode)
	{
		yield return new WaitForSeconds(0.5f);
		SceneMgr.Get().SetNextMode(mode);
	}

	private void ReturnToPreviousMode()
	{
		SceneMgr.Mode nextMode = GameMgr.Get().GetPostGameSceneMode();
		if (nextMode == SceneMgr.Mode.ADVENTURE && (!GameDownloadManagerProvider.Get().IsModuleReadyToPlay(DownloadTags.Content.Adventure) || !GameUtils.HasCompletedApprentice()))
		{
			nextMode = SceneMgr.Mode.HUB;
		}
		else if (nextMode == SceneMgr.Mode.BACON && !GameDownloadManagerProvider.Get().IsModuleReadyToPlay(DownloadTags.Content.Bgs))
		{
			nextMode = SceneMgr.Mode.HUB;
		}
		GameMgr.Get().PreparePostGameSceneMode(nextMode);
		BackToMode(nextMode);
	}

	private void ShowScoreScreen()
	{
		if (!GameState.Get().CanShowScoreScreen())
		{
			return;
		}
		m_scoreScreen = GameUtils.LoadGameObjectWithComponent<ScoreScreen>(m_ScoreScreenPrefab);
		if ((bool)m_scoreScreen)
		{
			TransformUtil.AttachAndPreserveLocalTransform(m_scoreScreen.transform, base.transform);
			LayerUtils.SetLayer(m_scoreScreen, GameLayer.IgnoreFullScreenEffects);
			m_scoreScreen.Show();
			m_showingScoreScreen = true;
			SetPlayingBlockingAnim(set: true);
			StartCoroutine(WaitThenSetPlayingBlockingAnim(0.65f, set: false));
			if (Gameplay.Get().HasBattleNetFatalError())
			{
				m_hitbox.AddEventListener(UIEventType.RELEASE, ContinueButtonPress_ProceedToError);
			}
		}
	}

	private void HideScoreScreen()
	{
		if ((bool)m_scoreScreen)
		{
			m_scoreScreen.Hide();
			m_showingScoreScreen = false;
			SetPlayingBlockingAnim(set: true);
			StartCoroutine(WaitThenSetPlayingBlockingAnim(0.25f, set: false));
		}
	}

	protected void HideTwoScoop()
	{
		if (m_twoScoop.IsShown())
		{
			m_twoScoop.Hide();
			m_noGoldRewardText.gameObject.SetActive(value: false);
			OnTwoScoopHidden();
			if (OnTwoScoopsShown != null)
			{
				OnTwoScoopsShown(shown: false, m_twoScoop);
			}
			if (InputManager.Get() != null)
			{
				InputManager.Get().EnableInput();
			}
		}
	}

	protected void ShowTwoScoop()
	{
		StartCoroutine(ShowTwoScoopWhenReady());
	}

	private IEnumerator ShowTwoScoopWhenReady()
	{
		while ((bool)m_scoreScreen)
		{
			SendTelemetryIfTimeout("ScoreScreen");
			yield return null;
		}
		while (!m_twoScoop.IsLoaded())
		{
			SendTelemetryIfTimeout("TwoScoop");
			yield return null;
		}
		while (JustEarnedHeroReward())
		{
			SendTelemetryIfTimeout("HeroReward");
			if (m_heroRewardEventReady)
			{
				break;
			}
			yield return null;
		}
		m_twoScoop.Show();
		if (!SpectatorManager.Get().IsSpectatingOrWatching && ShouldMakeUtilRequests())
		{
			InitGoldRewardUI();
		}
		OnTwoScoopShown();
		m_haveShownTwoScoop = true;
		if (OnTwoScoopsShown != null)
		{
			OnTwoScoopsShown(shown: true, m_twoScoop);
		}
	}

	protected IEnumerator WaitThenSetPlayingBlockingAnim(float sec, bool set)
	{
		yield return new WaitForSeconds(sec);
		SetPlayingBlockingAnim(set);
	}

	protected bool ShouldMakeUtilRequests()
	{
		if (!Network.ShouldBeConnectedToAurora())
		{
			return false;
		}
		return true;
	}

	public bool IsDoneDisplayingRewards()
	{
		return m_doneDisplayingRewards;
	}

	private IEnumerator ShowStandardFlowIfReady()
	{
		while (!m_shown)
		{
			yield return null;
		}
		while (ShouldMakeUtilRequests() && !m_netCacheReady && SendTelemetryIfTimeout("m_netCacheReady"))
		{
			yield return null;
		}
		while (!m_achievesReady)
		{
			SendTelemetryIfTimeout("m_achievesReady");
			yield return null;
		}
		while (!m_rewardsLoaded)
		{
			SendTelemetryIfTimeout("m_rewardsLoaded");
			yield return null;
		}
		while (m_shouldShowRankChange && (!m_rankChangeReady || !m_medalInfoUpdated))
		{
			SendTelemetryIfTimeout($"m_rankChangeReady: {m_rankChangeReady} m_medalInfoUpdated: {m_medalInfoUpdated}");
			yield return null;
		}
		while (m_shouldShowRankedCardBackProgress && m_rankedCardBackProgress == null)
		{
			SendTelemetryIfTimeout("m_rankedCardBackProgress");
			yield return null;
		}
		while (m_shouldShowRewardXpGains && !RewardXpNotificationManager.Get().IsReady)
		{
			SendTelemetryIfTimeout("RewardXpNotificationManager.Get().IsReady");
			yield return null;
		}
		SendRankedInitTelemetryIfNeeded();
		ShowStandardFlow();
	}

	protected virtual void ShowStandardFlow()
	{
		ShowTwoScoop();
		ShowRewardsXpGains();
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_continueText.gameObject.SetActive(value: true);
		}
	}

	protected void ShowRewardsXpGains()
	{
		if (RewardXpNotificationManager.Get().HasXpGainsToShow)
		{
			m_shouldShowRewardXpGains = true;
			RewardXpNotificationManager.Get().InitEndOfGameFlow(null);
			RewardXpNotificationManager.Get().ShowRewardTrackXpGains(delegate
			{
				ContinueEvents();
			}, justShowGameXp: true);
		}
	}

	protected virtual void OnNetCacheReady()
	{
		m_netCacheReady = true;
		NetCache.Get().UnregisterNetCacheHandler(OnNetCacheReady);
		if (m_shouldShowRankChange)
		{
			RetryMedalInfoRequestIfNeeded();
			LoadRankChange();
			LoadRankedRewardDisplay();
			LoadRankedCardBackProgress();
		}
		MaybeUpdateRewards();
	}

	private void RetryMedalInfoRequestIfNeeded()
	{
		if (IsMedalInfoRetryNeeded())
		{
			StartCoroutine(RetryMedalInfoRequest());
			return;
		}
		NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheMedalInfo), OnMedalInfoUpdate);
		m_medalInfoUpdated = true;
	}

	private bool IsMedalInfoRetryNeeded()
	{
		if (!ShouldMakeUtilRequests())
		{
			return false;
		}
		if (!m_shouldShowRankChange)
		{
			return false;
		}
		if (m_medalInfoRetryCount >= 3)
		{
			return false;
		}
		FormatType currentFormatType = Options.GetFormatType();
		MedalInfoTranslator mit = RankMgr.Get().GetLocalPlayerMedalInfo();
		if (mit != null)
		{
			return mit.GetChangeType(currentFormatType) == RankChangeType.NO_GAME_PLAYED;
		}
		return true;
	}

	private IEnumerator RetryMedalInfoRequest()
	{
		if (m_medalInfoRetryCount == 0)
		{
			m_medalInfoRetryDelay = 1f;
			NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheMedalInfo), OnMedalInfoUpdate);
		}
		else
		{
			m_medalInfoRetryDelay *= 2f;
		}
		m_medalInfoRetryCount++;
		yield return new WaitForSeconds(m_medalInfoRetryDelay);
		NetCache.Get().RefreshNetObject<NetCache.NetCacheMedalInfo>();
	}

	private void OnMedalInfoUpdate()
	{
		RetryMedalInfoRequestIfNeeded();
	}

	private void SendRankedInitTelemetryIfNeeded()
	{
		if (m_shouldShowRankChange && !m_hasSentRankedInitTelemetry)
		{
			m_hasSentRankedInitTelemetry = true;
			float elapsedTime = Time.time - m_endGameScreenStartTime;
			FormatType currentFormatType = Options.GetFormatType();
			MedalInfoTranslator mit = RankMgr.Get().GetLocalPlayerMedalInfo();
			bool medalInfoRetriesTimedOut = m_medalInfoRetryCount >= 3 && (mit == null || mit.GetChangeType(currentFormatType) == RankChangeType.NO_GAME_PLAYED);
			if (medalInfoRetriesTimedOut && mit != null)
			{
				Log.All.PrintError("EndGameScreen_MedalInfoTimeOut elapsedTime={0} retries={1} prev={2} curr={3}", elapsedTime, m_medalInfoRetryCount, mit.GetPreviousMedal(currentFormatType).ToString(), mit.GetCurrentMedal(currentFormatType).ToString());
			}
			bool showRankedReward = m_rankedRewardsToDisplay.Count > 0;
			TelemetryManager.Client().SendEndGameScreenInit(elapsedTime, m_medalInfoRetryCount, medalInfoRetriesTimedOut, showRankedReward, m_shouldShowRankedCardBackProgress, m_rewards.Count);
		}
	}

	private void LoadRankChange()
	{
		AssetReference rankChangePrefab = RankMgr.RANK_CHANGE_TWO_SCOOP_PREFAB_NEW;
		AssetLoader.Get().InstantiatePrefab(rankChangePrefab, OnRankChangeLoaded);
	}

	private void OnRankChangeLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_rankChangeTwoScoop = go;
		m_rankChangeTwoScoop.gameObject.SetActive(value: false);
		m_rankChangeReady = true;
	}

	private void OnRankChangeClosed()
	{
		m_isShowingRankChange = false;
		m_shouldShowRankChange = false;
		ContinueEvents();
	}

	private void LoadRankedRewardDisplay()
	{
		if (RankMgr.Get().GetLocalPlayerMedalInfo().GetRankedRewardsEarned(Options.GetFormatType(), ref m_rankedRewardsToDisplay) && m_rankedRewardsToDisplay.Count != 0)
		{
			m_rankedRewardDisplayWidget = WidgetInstance.Create(RankMgr.RANKED_REWARD_DISPLAY_PREFAB);
			m_rankedRewardDisplayWidget.RegisterReadyListener(delegate
			{
				OnRankedRewardDisplayWidgetReady();
			});
		}
	}

	private void OnRankedRewardDisplayWidgetReady()
	{
		m_rankedRewardDisplay = m_rankedRewardDisplayWidget.GetComponentInChildren<RankedRewardDisplay>();
	}

	private void LoadRankedCardBackProgress()
	{
		m_shouldShowRankedCardBackProgress = RankMgr.Get().GetLocalPlayerMedalInfo().ShouldShowCardBackProgress();
		if (m_shouldShowRankedCardBackProgress)
		{
			m_rankedCardBackProgressWidget = WidgetInstance.Create(RankMgr.RANKED_CARDBACK_PROGRESS_DISPLAY_PREFAB);
			m_rankedCardBackProgressWidget.RegisterReadyListener(delegate
			{
				OnRankedCardBackProgressWidgetReady();
			});
		}
	}

	private void OnRankedCardBackProgressWidgetReady()
	{
		m_rankedCardBackProgress = m_rankedCardBackProgressWidget.GetComponentInChildren<RankedCardBackProgressDisplay>();
	}

	private IEnumerator WaitForAchieveManager()
	{
		while (!AchieveManager.Get().IsReady())
		{
			yield return null;
		}
		m_achievesReady = true;
		MaybeUpdateRewards();
	}

	private void ProcessPreviousAchievements()
	{
		OnAchievesUpdated(new List<Achievement>(), new List<Achievement>(), null);
	}

	private void OnAchievesUpdated(List<Achievement> updatedAchieves, List<Achievement> completedAchieves, object userData)
	{
		List<Achievement> newCompletedAchievesToShow = AchieveManager.Get().GetNewCompletedAchievesToShow();
		bool shouldSuppressPopups = PopupDisplayManager.ShouldSuppressPopups();
		foreach (Achievement achieve in newCompletedAchievesToShow)
		{
			if ((!shouldSuppressPopups || achieve.Mode == Achieve.GameMode.MERCENARIES) && achieve.RewardTiming == Achieve.RewardTiming.IMMEDIATE && string.IsNullOrWhiteSpace(achieve.CustomVisualWidget) && m_completedQuests.Find((Achievement obj) => achieve.ID == obj.ID) == null)
			{
				m_completedQuests.Add(achieve);
			}
		}
	}

	private void OnGenericRewardUpdated(long rewardNoticeId, object userData)
	{
		m_genericRewardChestNoticeIdsReady.Add(rewardNoticeId);
		UpdateRewards();
	}

	protected bool HasShownScoops()
	{
		return m_haveShownTwoScoop;
	}

	protected void SetHeroRewardEventReady(bool isReady)
	{
		m_heroRewardEventReady = isReady;
	}

	private void MaybeUpdateRewards()
	{
		if (m_achievesReady && m_netCacheReady)
		{
			UpdateRewards();
		}
	}

	private void LoadRewards(List<RewardData> rewardsToLoad, Reward.DelOnRewardLoaded callback)
	{
		if (rewardsToLoad == null)
		{
			return;
		}
		foreach (RewardData rewardData in rewardsToLoad)
		{
			if (PopupDisplayManager.Get().RewardPopups.UpdateNoticesSeen(rewardData))
			{
				m_numRewardsToLoad++;
				rewardData.LoadRewardObject(callback);
			}
		}
	}

	private void UpdateRewards()
	{
		GameMgr gameMgr = GameMgr.Get();
		if (gameMgr == null)
		{
			Log.All.PrintError("EndGameScreen::UpdateRewards GameMgr object is null.");
			return;
		}
		bool includeRealRewards = true;
		if (gameMgr.IsTraditionalTutorial())
		{
			includeRealRewards = GameUtils.IsTraditionalTutorialComplete();
		}
		List<RewardData> rewardsToLoad = null;
		List<RewardData> genericRewardChestsToLoad = null;
		List<RewardData> purchasedCardRewardsToShow = null;
		if (includeRealRewards)
		{
			if (NetCache.Get() == null)
			{
				Log.All.PrintError("EndGameScreen::UpdateRewards NetCache object is null.");
				return;
			}
			List<NetCache.ProfileNotice> list = NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>().Notices.Where((NetCache.ProfileNotice n) => n.Type != NetCache.ProfileNotice.NoticeType.GENERIC_REWARD_CHEST || m_genericRewardChestNoticeIdsReady.Any((long r) => n.NoticeID == r)).ToList();
			list.RemoveAll((NetCache.ProfileNotice n) => n.Origin == NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_DUELS);
			List<RewardData> rewards = RewardUtils.GetRewards(list);
			HashSet<Achieve.RewardTiming> rewardTimings = new HashSet<Achieve.RewardTiming> { Achieve.RewardTiming.IMMEDIATE };
			RewardUtils.GetViewableRewards(rewards, rewardTimings, out rewardsToLoad, out genericRewardChestsToLoad, ref purchasedCardRewardsToShow, ref m_completedQuests);
		}
		else
		{
			rewardsToLoad = new List<RewardData>();
		}
		JustEarnedHeroReward();
		if (!gameMgr.IsSpectator())
		{
			GameState gameState = GameState.Get();
			if (gameState != null)
			{
				GameEntity gameEntity = gameState.GetGameEntity();
				if (gameEntity != null)
				{
					List<RewardData> missionSpecificRewards = gameEntity.GetCustomRewards();
					if (missionSpecificRewards != null)
					{
						rewardsToLoad.AddRange(missionSpecificRewards);
					}
				}
			}
		}
		LoadRewards(rewardsToLoad, OnRewardObjectLoaded);
		LoadRewards(genericRewardChestsToLoad, OnGenericRewardObjectLoaded);
		if (m_numRewardsToLoad == 0)
		{
			m_rewardsLoaded = true;
		}
	}

	private void OnRewardObjectLoaded(Reward reward, object callbackData)
	{
		LoadReward(reward, ref m_rewards);
	}

	private void OnGenericRewardObjectLoaded(Reward reward, object callbackData)
	{
		LoadReward(reward, ref m_genericRewards);
	}

	private void PositionReward(Reward reward)
	{
		reward.transform.parent = base.transform;
		reward.transform.localRotation = Quaternion.identity;
		reward.transform.localPosition = PopupDisplayManager.Get().RewardPopups.GetRewardLocalPos();
	}

	private void LoadReward(Reward reward, ref List<Reward> allRewards)
	{
		reward.Hide();
		PositionReward(reward);
		allRewards.Add(reward);
		m_numRewardsToLoad--;
		if (m_numRewardsToLoad <= 0)
		{
			RewardUtils.SortRewards(ref allRewards);
			m_rewardsLoaded = true;
		}
	}

	private void DisplayLoadedRewardObject(Reward reward, object callbackData)
	{
		if (m_currentlyShowingReward != null)
		{
			m_currentlyShowingReward.Hide(animate: true);
			m_currentlyShowingReward = null;
		}
		reward.Hide();
		PositionReward(reward);
		m_currentlyShowingReward = reward;
		SetPlayingBlockingAnim(set: true);
		LayerUtils.SetLayer(m_currentlyShowingReward.gameObject, GameLayer.IgnoreFullScreenEffects);
		ShowReward(m_currentlyShowingReward);
	}

	private void ShowReward(Reward reward)
	{
		bool updateCacheValues = !(reward is CardReward);
		RewardUtils.ShowReward(UserAttentionBlocker.NONE, reward, updateCacheValues, PopupDisplayManager.Get().RewardPopups.GetRewardPunchScale(), PopupDisplayManager.Get().RewardPopups.GetRewardScale());
		StartCoroutine(WaitThenSetPlayingBlockingAnim(0.35f, set: false));
	}

	protected virtual bool ShowHeroRewardEvent()
	{
		return false;
	}

	protected bool ShowFixedRewards()
	{
		if (m_isShowingFixedRewards)
		{
			return true;
		}
		if (PopupDisplayManager.SuppressPopupsTemporarily)
		{
			return false;
		}
		HashSet<Achieve.RewardTiming> rewardTimings = new HashSet<Achieve.RewardTiming> { Achieve.RewardTiming.IMMEDIATE };
		FixedRewardsMgr.DelOnAllFixedRewardsShown onAllFixedRewardsShown = delegate
		{
			m_isShowingFixedRewards = false;
			ContinueEvents();
		};
		m_isShowingFixedRewards = FixedRewardsMgr.Get().ShowFixedRewards(UserAttentionBlocker.NONE, rewardTimings, onAllFixedRewardsShown, null);
		return m_isShowingFixedRewards;
	}

	private bool ShowGoldReward()
	{
		int rewardIndex = m_rewards.FindIndex((Reward reward) => reward.Data is GoldRewardData { Origin: NetCache.ProfileNotice.NoticeOrigin.TOURNEY });
		if (rewardIndex < 0)
		{
			return false;
		}
		Reward goldReward = m_rewards[rewardIndex];
		m_rewards.RemoveAt(rewardIndex);
		m_rewards.Insert(0, goldReward);
		ShowNextReward();
		return true;
	}

	private bool ShowNextProgressionQuestReward()
	{
		if (!QuestManager.Get().ShowNextReward(delegate
		{
			ContinueEvents();
		}))
		{
			return false;
		}
		return true;
	}

	protected bool ShowNextCompletedQuest()
	{
		if (m_showingNextCompletedQuest)
		{
			StackTrace stackTrace = new StackTrace();
			Log.All.PrintError("[EndGameScreen] ShowNextCompletedQuest attempted to recursively call itself!\nStackTrace:\n{0}", stackTrace.ToString());
			return false;
		}
		using (new ShowNextCompletedQuestLock(this))
		{
			if (m_completedQuests.Count == 0)
			{
				return false;
			}
			if (QuestToast.IsQuestActive())
			{
				QuestToast.GetCurrentToast().CloseQuestToast();
			}
			Achievement completedQuest = m_completedQuests[0];
			m_completedQuests.RemoveAt(0);
			while (!string.IsNullOrEmpty(completedQuest.CustomVisualWidget))
			{
				if (m_completedQuests.Count == 0)
				{
					return false;
				}
				completedQuest = m_completedQuests[0];
				m_completedQuests.RemoveAt(0);
			}
			if (!completedQuest.UseGenericRewardVisual)
			{
				bool containsNonBasicCardReward = false;
				foreach (RewardData reward in completedQuest.Rewards)
				{
					if (reward.RewardType == Reward.Type.CARD && reward is CardRewardData cardRewardData)
					{
						TAG_CARD_SET cardSet = GameUtils.GetCardSetFromCardID(cardRewardData.CardID);
						containsNonBasicCardReward |= !GameDbf.GetIndex().GetCardSet(cardSet).IsCoreCardSet;
					}
				}
				bool updateCacheValues = !containsNonBasicCardReward;
				QuestToast.ShowQuestToast(UserAttentionBlocker.NONE, ShowQuestToastCallback, updateCacheValues, completedQuest);
				NarrativeManager.Get().OnQuestCompleteShown(completedQuest.ID);
			}
			else
			{
				completedQuest.AckCurrentProgressAndRewardNotices();
				if (completedQuest.Rewards.Count > 0)
				{
					completedQuest.Rewards[0].LoadRewardObject(DisplayLoadedRewardObject);
				}
			}
		}
		return true;
	}

	protected void ShowQuestToastCallback(object userData)
	{
		if (!(this == null))
		{
			ContinueEvents();
		}
	}

	protected bool ShowRewardTrackXpGains()
	{
		RewardXpNotificationManager rewardXpManager = RewardXpNotificationManager.Get();
		if (rewardXpManager.IsShowingXpGains && !rewardXpManager.JustShowGameXp)
		{
			rewardXpManager.TerminateEarly();
			return false;
		}
		if (!rewardXpManager.HasXpGainsToShow && !rewardXpManager.JustShowGameXp)
		{
			return false;
		}
		if (rewardXpManager.IsShowingXpGains && rewardXpManager.JustShowGameXp)
		{
			rewardXpManager.ContinueNotifications();
		}
		else
		{
			rewardXpManager.ShowRewardTrackXpGains(delegate
			{
				ContinueEvents();
			});
		}
		return true;
	}

	protected bool ShowNextRewardTrackAutoClaimedReward()
	{
		if (m_isShowingTrackRewards)
		{
			return true;
		}
		Action callback = delegate
		{
			m_isShowingTrackRewards = false;
			ContinueEvents();
		};
		if (!RewardTrackManager.Get().ShowNextReward(callback))
		{
			return false;
		}
		m_isShowingTrackRewards = true;
		return true;
	}

	protected bool ShowNextReward()
	{
		if (m_rewards.Count == 0)
		{
			return false;
		}
		SetPlayingBlockingAnim(set: true);
		m_currentlyShowingReward = m_rewards[0];
		m_rewards.RemoveAt(0);
		ShowReward(m_currentlyShowingReward);
		return true;
	}

	protected bool ShowNextGenericReward()
	{
		if (m_genericRewards.Count == 0)
		{
			return false;
		}
		SetPlayingBlockingAnim(set: true);
		m_currentlyShowingReward = m_genericRewards[0];
		m_genericRewards.RemoveAt(0);
		QuestToast.ShowGenericRewardQuestToast(UserAttentionBlocker.NONE, ShowQuestToastCallback, m_currentlyShowingReward.Data, m_currentlyShowingReward.Data.NameOverride, m_currentlyShowingReward.Data.DescriptionOverride);
		StartCoroutine(WaitThenSetPlayingBlockingAnim(0.35f, set: false));
		return true;
	}

	private bool ShowRankChange()
	{
		if (!m_shouldShowRankChange)
		{
			return false;
		}
		if (m_isShowingRankChange)
		{
			return true;
		}
		m_rankChangeTwoScoop.gameObject.SetActive(value: true);
		RankChangeTwoScoop_NEW component = m_rankChangeTwoScoop.GetComponent<RankChangeTwoScoop_NEW>();
		component.Initialize(RankMgr.Get().GetLocalPlayerMedalInfo(), Options.GetFormatType(), OnRankChangeClosed);
		component.Show();
		m_isShowingRankChange = true;
		return true;
	}

	private bool ShowRankedRewards()
	{
		if (m_rankedRewardsToDisplay.Count == 0)
		{
			return false;
		}
		if (m_isShowingRankedReward)
		{
			return true;
		}
		m_isShowingRankedReward = true;
		FormatType currentFormatType = Options.GetFormatType();
		TranslatedMedalInfo tmi = RankMgr.Get().GetLocalPlayerMedalInfo().GetCurrentMedal(currentFormatType);
		m_rankedRewardDisplay.Initialize(tmi, m_rankedRewardsToDisplay, OnRankedRewardsClosed);
		m_rankedRewardDisplay.Show();
		return true;
	}

	private void OnRankedRewardsClosed()
	{
		m_isShowingRankedReward = false;
		m_rankedRewardsToDisplay.Clear();
		UnityEngine.Object.Destroy(m_rankedRewardDisplayWidget.gameObject);
		ContinueEvents();
	}

	private bool ShowRankedCardBackProgress()
	{
		if (!m_shouldShowRankedCardBackProgress)
		{
			return false;
		}
		if (m_isShowingRankedCardBackProgress)
		{
			return true;
		}
		m_isShowingRankedCardBackProgress = true;
		m_rankedCardBackProgress.Initialize(RankMgr.Get().GetLocalPlayerMedalInfo(), OnRankedCardBackProgressClosed);
		m_rankedCardBackProgress.Show();
		return true;
	}

	private void OnRankedCardBackProgressClosed()
	{
		m_shouldShowRankedCardBackProgress = false;
		m_isShowingRankedCardBackProgress = false;
		UnityEngine.Object.Destroy(m_rankedCardBackProgressWidget.gameObject);
		if (FindRankedCardBackRewardAndMakeNext())
		{
			ShowNextReward();
		}
		else
		{
			ContinueEvents();
		}
	}

	private bool FindRankedCardBackRewardAndMakeNext()
	{
		int currentSeasonId = RankMgr.Get().GetLocalPlayerMedalInfo().GetCurrentSeasonId();
		int rankedCardBackId = RankMgr.Get().GetRankedCardBackIdForSeasonId(currentSeasonId);
		int rankedCardBackRewardIndex = m_rewards.FindIndex((Reward reward) => (reward.Data is CardBackRewardData cardBackRewardData && cardBackRewardData.CardBackID == rankedCardBackId) ? true : false);
		if (rankedCardBackRewardIndex < 0)
		{
			return false;
		}
		Reward rankedCardBackReward = m_rewards[rankedCardBackRewardIndex];
		m_rewards.RemoveAt(rankedCardBackRewardIndex);
		m_rewards.Insert(0, rankedCardBackReward);
		return true;
	}

	protected virtual bool JustEarnedHeroReward()
	{
		return false;
	}

	protected virtual bool ShowHealUpDialog()
	{
		return false;
	}

	protected virtual bool ShowPushNotificationPrompt()
	{
		return false;
	}

	protected virtual void ShowAppRatingPrompt()
	{
	}

	protected bool ShowMercenariesExperienceRewards()
	{
		if (m_isShowingMercenariesExperienceRewards)
		{
			return true;
		}
		if (m_finishedShowingMercenariesExperienceRewards)
		{
			return false;
		}
		LettuceMissionEntity lettuceGameEntity = null;
		if (GameState.Get().GetGameEntity() is LettuceMissionEntity)
		{
			lettuceGameEntity = (LettuceMissionEntity)GameState.Get().GetGameEntity();
			List<MercenaryExpRewardData> mercenariesExperienceRewards = new List<MercenaryExpRewardData>();
			foreach (MercenariesExperienceUpdate experienceUpdate in lettuceGameEntity.GetMercenaryExperienceUpdates())
			{
				if (experienceUpdate.PreExp != experienceUpdate.PostExp)
				{
					MercenaryExpRewardData rewardData = new MercenaryExpRewardData(experienceUpdate.MercenaryId, (int)experienceUpdate.PreExp, (int)experienceUpdate.PostExp, (int)experienceUpdate.ExpDelta);
					mercenariesExperienceRewards.Add(rewardData);
				}
			}
			if (mercenariesExperienceRewards.Count == 0)
			{
				m_finishedShowingMercenariesExperienceRewards = true;
				return false;
			}
			mercenariesExperienceRewards = mercenariesExperienceRewards.OrderByDescending((MercenaryExpRewardData r) => r.NumberOfLevelUps).ToList();
			if (!AssetLoader.Get().InstantiatePrefab("MercenariesExperienceTwoScoop.prefab:eb825692c63590b4d8a76def17e8aa3a", OnLoadMercenariesExperienceTwoScoopAttempted, mercenariesExperienceRewards))
			{
				OnLoadMercenariesExperienceTwoScoopAttempted("MercenariesExperienceTwoScoop.prefab:eb825692c63590b4d8a76def17e8aa3a", null, mercenariesExperienceRewards);
			}
			m_isShowingMercenariesExperienceRewards = true;
			return true;
		}
		m_finishedShowingMercenariesExperienceRewards = true;
		return false;
	}

	private void OnLoadMercenariesExperienceTwoScoopAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Log.Lettuce.PrintError("Failed to load Mercenaries Experience Two Scoop.");
			m_isShowingMercenariesExperienceRewards = false;
			m_finishedShowingMercenariesExperienceRewards = true;
			return;
		}
		MercenariesExperienceTwoScoop twoScoop = go.GetComponent<MercenariesExperienceTwoScoop>();
		if (twoScoop == null)
		{
			Log.Lettuce.PrintError("MercenariesExperienceTwoScoop game object had no script attached!");
			m_isShowingMercenariesExperienceRewards = false;
			m_finishedShowingMercenariesExperienceRewards = true;
		}
		else
		{
			List<MercenaryExpRewardData> rewards = (List<MercenaryExpRewardData>)callbackData;
			twoScoop.Initialize(rewards, OnMercenariesExperienceTwoScoopClosed);
		}
	}

	private void OnMercenariesExperienceTwoScoopClosed()
	{
		m_isShowingMercenariesExperienceRewards = false;
		m_finishedShowingMercenariesExperienceRewards = true;
		ContinueEvents();
	}

	private bool SendTelemetryIfTimeout(string culprit)
	{
		if (m_hasTimedOutAndLogged)
		{
			return false;
		}
		if (m_timeoutTimerStartTime == 0f)
		{
			m_timeoutTimerStartTime = Time.time;
		}
		float timeElapsed = Time.time - m_timeoutTimerStartTime;
		if (timeElapsed >= 5f)
		{
			TelemetryManager.Client().SendLiveIssue("EndGameScreen_NetCacheReadyTimeout", "Timeout occurred when waiting for " + culprit + " to be ready, " + $"time elapsed: {timeElapsed} while waiting for {culprit}.");
			Log.All.PrintError("Timeout occurred when waiting for " + culprit + " to be ready, " + $"time elapsed: {timeElapsed} while waiting for {culprit}.");
			m_hasTimedOutAndLogged = true;
			return false;
		}
		return true;
	}
}
