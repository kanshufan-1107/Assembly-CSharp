using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using PegasusClient;
using PegasusShared;
using UnityEngine;

public class SocialToastMgr : MonoBehaviour
{
	public enum TOAST_TYPE
	{
		DEFAULT,
		FRIEND_ONLINE,
		FRIEND_OFFLINE,
		FRIEND_INVITE,
		HEALTHY_GAMING,
		HEALTHY_GAMING_OVER_THRESHOLD,
		FRIEND_ARENA_COMPLETE,
		SPECTATOR_INVITE_SENT,
		SPECTATOR_INVITE_RECEIVED,
		SPECTATOR_ADDED,
		SPECTATOR_REMOVED
	}

	private class ToastArgs
	{
		public string m_message;

		public float m_displayTime;

		public bool m_playSound;

		public ToastArgs(string message, float displayTime, bool playSound)
		{
			m_message = message;
			m_displayTime = displayTime;
			m_playSound = playSound;
		}
	}

	private class LastOnlineTracker
	{
		public float m_localLastOnlineTime;

		public Processor.ScheduledCallback m_callback;
	}

	private const float FADE_IN_TIME = 0.25f;

	private const float FADE_OUT_TIME = 0.5f;

	private const float HOLD_TIME = 2f;

	private const float SHUTDOWN_MESSAGE_TIME = 3.5f;

	private const float OFFLINE_TOAST_DELAY = 5f;

	private const int MAX_QUEUE_CAPACITY = 5;

	private const string BNET_TOAST_SOUND = "UI_BnetToast.prefab:b869739323d1fc241984f9f480fff8ef";

	public SocialToast m_defaultSocialToastPrefab;

	private static SocialToastMgr s_instance;

	private SocialToast m_defaultToast;

	private SocialToast m_currentToast;

	private Queue<ToastArgs> m_toastQueue = new Queue<ToastArgs>();

	private bool m_toastIsShown;

	private bool m_toastsEnabled;

	private PlatformDependentValue<Vector3> TOAST_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(235f, 1f, 235f),
		Phone = new Vector3(470f, 1f, 470f)
	};

	private Map<BnetGameAccountId, MedalInfoTranslator> m_lastKnownMedals = new Map<BnetGameAccountId, MedalInfoTranslator>();

	private Map<BnetGameAccountId, string> m_lastOpenedLegendary = new Map<BnetGameAccountId, string>();

	private Map<BnetGameAccountId, int> m_lastAchievementId = new Map<BnetGameAccountId, int>();

	private Map<int, LastOnlineTracker> m_lastOnlineTracker = new Map<int, LastOnlineTracker>();

	private void Awake()
	{
		s_instance = this;
		CreateSocialToastObjects();
		BnetPresenceMgr.Get().AddPlayersChangedListener(OnPlayersChanged);
		BnetPresenceMgr.Get().OnGameAccountPresenceChange += OnPresenceChanged;
		BnetFriendMgr.Get().AddChangeListener(OnFriendsChanged);
		Network.Get().SetShutdownHandler(ShutdownHandler);
		SoundManager.Get().Load("UI_BnetToast.prefab:b869739323d1fc241984f9f480fff8ef");
		LoginManager.Get().OnFullLoginFlowComplete += OnLoginCompleted;
	}

	private void OnDestroy()
	{
		if (BnetPresenceMgr.Get() != null)
		{
			BnetPresenceMgr.Get().OnGameAccountPresenceChange -= OnPresenceChanged;
			BnetPresenceMgr.Get().RemovePlayersChangedListener(OnPlayersChanged);
		}
		if (BnetFriendMgr.Get() != null)
		{
			BnetFriendMgr.Get().RemoveChangeListener(OnFriendsChanged);
		}
		if (LoginManager.Get() != null)
		{
			LoginManager.Get().OnFullLoginFlowComplete -= OnLoginCompleted;
		}
		m_lastKnownMedals.Clear();
		s_instance = null;
	}

	public static SocialToastMgr Get()
	{
		return s_instance;
	}

	public void Reset()
	{
		if (!(m_currentToast == null))
		{
			iTween.Stop(m_currentToast.gameObject, includechildren: true);
			iTween.Stop(base.gameObject, includechildren: true);
			RenderUtils.SetAlpha(m_currentToast.gameObject, 0f);
			m_toastQueue.Clear();
			DeactivateToast();
		}
	}

	public void AddToast(UserAttentionBlocker blocker, string textArg)
	{
		AddToast(blocker, textArg, TOAST_TYPE.DEFAULT, 2f, playSound: true);
	}

	public void AddToast(UserAttentionBlocker blocker, string textArg, TOAST_TYPE toastType)
	{
		AddToast(blocker, textArg, toastType, 2f, playSound: true);
	}

	public void AddToast(UserAttentionBlocker blocker, string textArg, TOAST_TYPE toastType, bool playSound)
	{
		AddToast(blocker, textArg, toastType, 2f, playSound);
	}

	public void AddToast(UserAttentionBlocker blocker, string textArg, TOAST_TYPE toastType, float displayTime)
	{
		AddToast(blocker, textArg, toastType, displayTime, playSound: true);
	}

	public void AddToast(UserAttentionBlocker blocker, string textArg, TOAST_TYPE toastType, float displayTime, bool playSound)
	{
		if (UserAttentionManager.CanShowAttentionGrabber(blocker, "SocialToastMgr.AddToast:" + toastType))
		{
			string message = toastType switch
			{
				TOAST_TYPE.DEFAULT => textArg, 
				TOAST_TYPE.FRIEND_OFFLINE => GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_OFFLINE", "999999ff", textArg), 
				TOAST_TYPE.FRIEND_ONLINE => GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_ONLINE", "5ecaf0ff", textArg), 
				TOAST_TYPE.FRIEND_INVITE => GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_REQUEST", "5ecaf0ff", textArg), 
				TOAST_TYPE.HEALTHY_GAMING => GameStrings.Format("GLOBAL_HEALTHY_GAMING_TOAST", textArg), 
				TOAST_TYPE.HEALTHY_GAMING_OVER_THRESHOLD => GameStrings.Format("GLOBAL_HEALTHY_GAMING_TOAST_OVER_THRESHOLD", textArg), 
				TOAST_TYPE.SPECTATOR_INVITE_SENT => GameStrings.Format("GLOBAL_SOCIAL_TOAST_SPECTATOR_INVITE_SENT", "5ecaf0ff", textArg), 
				TOAST_TYPE.SPECTATOR_INVITE_RECEIVED => GameStrings.Format("GLOBAL_SOCIAL_TOAST_SPECTATOR_INVITE_RECEIVED", "5ecaf0ff", textArg), 
				TOAST_TYPE.SPECTATOR_ADDED => GameStrings.Format("GLOBAL_SOCIAL_TOAST_SPECTATOR_ADDED", "5ecaf0ff", textArg), 
				TOAST_TYPE.SPECTATOR_REMOVED => GameStrings.Format("GLOBAL_SOCIAL_TOAST_SPECTATOR_REMOVED", "5ecaf0ff", textArg), 
				_ => "", 
			};
			CreateSocialToastObjects();
			m_currentToast = m_defaultToast;
			if (m_currentToast == null)
			{
				Log.All.PrintWarning("Toast design is not created yet");
			}
			else if (m_toastQueue.Count <= 5)
			{
				ToastArgs toastArgs = new ToastArgs(message, displayTime, playSound);
				m_toastQueue.Enqueue(toastArgs);
				CheckToastQueue();
			}
		}
	}

	private void OnLoginCompleted()
	{
		m_toastsEnabled = true;
		CheckToastQueue();
	}

	private void CreateSocialToastObjects()
	{
		if (!(BnetBar.Get() == null) && !(BnetBar.Get().m_socialToastBone == null) && m_defaultToast == null)
		{
			CreateSocialToastObject(m_defaultSocialToastPrefab, ref m_defaultToast);
			m_currentToast = m_defaultToast;
		}
	}

	private void CreateSocialToastObject(SocialToast prefab, ref SocialToast newToastReference, Transform parent = null)
	{
		if (parent == null && (BnetBar.Get() == null || BnetBar.Get().m_socialToastBone == null))
		{
			Debug.LogError("FAILED to create Social Toast Object, no parent transform found!");
			return;
		}
		newToastReference = UnityEngine.Object.Instantiate(prefab);
		RenderUtils.SetAlpha(newToastReference.gameObject, 0f);
		newToastReference.gameObject.SetActive(value: false);
		newToastReference.transform.parent = ((parent != null) ? parent : BnetBar.Get().m_socialToastBone.transform);
		newToastReference.transform.localRotation = Quaternion.Euler(new Vector3(90f, 180f, 0f));
		newToastReference.transform.localScale = TOAST_SCALE;
		newToastReference.transform.position = BnetBar.Get().m_socialToastBone.transform.position;
	}

	private void FadeInToast()
	{
		m_toastIsShown = true;
		ToastArgs toastArgs = m_toastQueue.Dequeue();
		m_currentToast.gameObject.SetActive(value: true);
		m_currentToast.SetText(toastArgs.m_message);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", 1f);
		args.Add("time", 0.25f);
		args.Add("easetype", iTween.EaseType.easeInCubic);
		args.Add("oncomplete", "FadeOutToast");
		args.Add("oncompletetarget", base.gameObject);
		args.Add("oncompleteparams", toastArgs.m_displayTime);
		args.Add("name", "fade");
		iTween.StopByName(base.gameObject, "fade");
		iTween.FadeTo(m_currentToast.gameObject, args);
		RenderUtils.SetAlpha(m_currentToast.gameObject, 1f);
		if (toastArgs.m_playSound)
		{
			PlayToastSound();
		}
	}

	public void PlayToastSound()
	{
		SoundManager.Get().LoadAndPlay("UI_BnetToast.prefab:b869739323d1fc241984f9f480fff8ef");
	}

	private void FadeOutToast(float displayTime)
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", 0f);
		args.Add("delay", displayTime);
		args.Add("time", 0.25f);
		args.Add("easetype", iTween.EaseType.easeInCubic);
		args.Add("oncomplete", "DeactivateToast");
		args.Add("oncompletetarget", base.gameObject);
		args.Add("name", "fade");
		iTween.FadeTo(m_currentToast.gameObject, args);
	}

	private void DeactivateToast()
	{
		m_currentToast.gameObject.SetActive(value: false);
		m_toastIsShown = false;
		CheckToastQueue();
	}

	public void CheckToastQueue()
	{
		if (!m_toastsEnabled || m_toastIsShown)
		{
			return;
		}
		AchievementManager achievementMgr = AchievementManager.Get();
		if (achievementMgr != null)
		{
			achievementMgr.CheckToastQueue();
			if (achievementMgr.IsShowingToast())
			{
				return;
			}
		}
		if (m_toastQueue.Count != 0)
		{
			FadeInToast();
		}
	}

	public bool IsShowingToast()
	{
		return m_toastIsShown;
	}

	private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		if (!DemoMgr.Get().IsSocialEnabled())
		{
			return;
		}
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		foreach (BnetPlayerChange change in changelist.GetChanges())
		{
			if (change.GetPlayer() == null || change.GetNewPlayer() == null || change == null || !change.GetPlayer().IsDisplayable() || change.GetPlayer() == myself || !BnetFriendMgr.Get().IsFriend(change.GetPlayer()))
			{
				continue;
			}
			BnetPlayer oldPlayer = change.GetOldPlayer();
			BnetPlayer newPlayer = change.GetNewPlayer();
			CheckForOnlineStatusChanged(oldPlayer, newPlayer);
			if (oldPlayer != null)
			{
				BnetGameAccount newPlayerAccount = newPlayer.GetHearthstoneGameAccount();
				BnetGameAccount oldPlayerAccount = oldPlayer.GetHearthstoneGameAccount();
				if (!(oldPlayerAccount == null) && !(newPlayerAccount == null))
				{
					CheckForCardOpened(oldPlayerAccount, newPlayerAccount, newPlayer);
					CheckForDruidLevelChanged(oldPlayerAccount, newPlayerAccount, newPlayer);
					CheckForHunterLevelChanged(oldPlayerAccount, newPlayerAccount, newPlayer);
					CheckForMageLevelChanged(oldPlayerAccount, newPlayerAccount, newPlayer);
					CheckForPaladinLevelChanged(oldPlayerAccount, newPlayerAccount, newPlayer);
					CheckForPriestLevelChanged(oldPlayerAccount, newPlayerAccount, newPlayer);
					CheckForRogueLevelChanged(oldPlayerAccount, newPlayerAccount, newPlayer);
					CheckForShamanLevelChanged(oldPlayerAccount, newPlayerAccount, newPlayer);
					CheckForWarlockLevelChanged(oldPlayerAccount, newPlayerAccount, newPlayer);
					CheckForWarriorLevelChanged(oldPlayerAccount, newPlayerAccount, newPlayer);
					CheckForDemonHunterLevelChanged(oldPlayerAccount, newPlayerAccount, newPlayer);
					CheckForDeathKnightLevelChanged(oldPlayerAccount, newPlayerAccount, newPlayer);
					CheckForAchievementCompleted(oldPlayerAccount, newPlayerAccount, newPlayer);
				}
			}
		}
	}

	private void OnPresenceChanged(PresenceUpdate[] updates)
	{
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		for (int i = 0; i < updates.Length; i++)
		{
			PresenceUpdate update = updates[i];
			if (update.programId != BnetProgramId.HEARTHSTONE)
			{
				continue;
			}
			BnetPlayer player = BnetUtils.GetPlayer(new BnetGameAccountId(update.entityId?.EntityId));
			if (player != null && player != myself && player.IsDisplayable() && BnetFriendMgr.Get().IsFriend(player))
			{
				switch (update.fieldId)
				{
				case 17u:
					CheckSessionGameStarted(player);
					break;
				case 22u:
					CheckSessionRecordChanged(player);
					break;
				case 18u:
					CheckForNewRank(player);
					break;
				}
			}
		}
	}

	private void CheckForOnlineStatusChanged(BnetPlayer oldPlayer, BnetPlayer newPlayer)
	{
		if (oldPlayer == null || newPlayer == null || oldPlayer.IsOnline() == newPlayer.IsOnline())
		{
			return;
		}
		long lastOnline = newPlayer.GetBestLastOnlineMicrosec();
		BnetPlayer myPlayer = BnetPresenceMgr.Get().GetMyPlayer();
		long myLastOnline = 0L;
		if (myPlayer != null)
		{
			myLastOnline = myPlayer.GetBestLastOnlineMicrosec();
		}
		if (lastOnline == 0L || myLastOnline == 0L || myLastOnline > lastOnline)
		{
			return;
		}
		LastOnlineTracker lastOnlineTime = null;
		float currTime = Time.fixedTime;
		int playerHashCode = newPlayer.GetAccountId().GetHashCode();
		if (!m_lastOnlineTracker.TryGetValue(playerHashCode, out lastOnlineTime))
		{
			lastOnlineTime = new LastOnlineTracker();
			m_lastOnlineTracker[playerHashCode] = lastOnlineTime;
		}
		if (newPlayer.IsOnline())
		{
			if (lastOnlineTime.m_callback != null)
			{
				Processor.CancelScheduledCallback(lastOnlineTime.m_callback);
			}
			lastOnlineTime.m_callback = null;
			if (currTime - lastOnlineTime.m_localLastOnlineTime >= 5f)
			{
				AddToast(UserAttentionBlocker.NONE, newPlayer.GetBestName(), TOAST_TYPE.FRIEND_ONLINE);
			}
			return;
		}
		lastOnlineTime.m_localLastOnlineTime = currTime;
		lastOnlineTime.m_callback = delegate
		{
			if (!newPlayer.IsOnline())
			{
				AddToast(UserAttentionBlocker.NONE, newPlayer.GetBestName(), TOAST_TYPE.FRIEND_OFFLINE, playSound: false);
			}
		};
		Processor.ScheduleCallback(5f, realTime: false, lastOnlineTime.m_callback);
	}

	private void CheckSessionGameStarted(BnetPlayer player)
	{
		if (PresenceMgr.Get().GetStatus(player) == Global.PresenceStatus.TAVERN_BRAWL_GAME)
		{
			if (!TavernBrawlManager.Get().IsCurrentSeasonSessionBased)
			{
				return;
			}
		}
		else if (PresenceMgr.Get().GetStatus(player) != Global.PresenceStatus.ARENA_GAME)
		{
			return;
		}
		BnetGameAccount playerAccount = player.GetHearthstoneGameAccount();
		if (playerAccount == null)
		{
			return;
		}
		SessionRecord record = playerAccount.GetSessionRecord();
		if (record != null && record.Wins >= 8 && !record.RunFinished)
		{
			string sessionToastGlueString = string.Empty;
			switch (record.SessionRecordType)
			{
			case SessionRecordType.ARENA:
				sessionToastGlueString = "GLOBAL_SOCIAL_TOAST_FRIEND_ARENA_START_WITH_MANY_WINS";
				break;
			case SessionRecordType.TAVERN_BRAWL:
				sessionToastGlueString = "GLOBAL_SOCIAL_TOAST_FRIEND_BRAWLISEUM_START_WITH_MANY_WINS";
				break;
			case SessionRecordType.HEROIC_BRAWL:
				sessionToastGlueString = "GLOBAL_SOCIAL_TOAST_FRIEND_HEROIC_BRAWL_START_WITH_MANY_WINS";
				break;
			}
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format(sessionToastGlueString, "5ecaf0ff", player.GetBestName(), record.Wins));
		}
	}

	private void CheckSessionRecordChanged(BnetPlayer player)
	{
		BnetGameAccount playerAccount = player.GetHearthstoneGameAccount();
		if (playerAccount == null)
		{
			return;
		}
		SessionRecord record = playerAccount.GetSessionRecord();
		if (record == null)
		{
			return;
		}
		string sessionToastGlueString = string.Empty;
		if (record.RunFinished)
		{
			if (record.Wins >= 3)
			{
				switch (record.SessionRecordType)
				{
				case SessionRecordType.ARENA:
					sessionToastGlueString = "GLOBAL_SOCIAL_TOAST_FRIEND_ARENA_COMPLETE";
					break;
				case SessionRecordType.TAVERN_BRAWL:
					sessionToastGlueString = "GLOBAL_SOCIAL_TOAST_FRIEND_BRAWLISEUM_COMPLETE";
					break;
				case SessionRecordType.HEROIC_BRAWL:
					sessionToastGlueString = "GLOBAL_SOCIAL_TOAST_FRIEND_HEROIC_BRAWL_COMPLETE";
					break;
				}
				AddToast(UserAttentionBlocker.NONE, GameStrings.Format(sessionToastGlueString, "5ecaf0ff", player.GetBestName(), record.Wins, record.Losses));
			}
		}
		else if (record.Wins == 0 && record.Losses == 0)
		{
			switch (record.SessionRecordType)
			{
			case SessionRecordType.ARENA:
				sessionToastGlueString = "GLOBAL_SOCIAL_TOAST_FRIEND_ARENA_START";
				break;
			case SessionRecordType.TAVERN_BRAWL:
				sessionToastGlueString = "GLOBAL_SOCIAL_TOAST_FRIEND_BRAWLISEUM_START";
				break;
			case SessionRecordType.HEROIC_BRAWL:
				sessionToastGlueString = "GLOBAL_SOCIAL_TOAST_FRIEND_HEROIC_BRAWL_START";
				break;
			}
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format(sessionToastGlueString, "5ecaf0ff", player.GetBestName()));
		}
	}

	private void CheckForCardOpened(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		string newCardsOpened = newPlayerAccount.GetCardsOpened();
		if (string.IsNullOrEmpty(newCardsOpened) || newCardsOpened == oldPlayerAccount.GetCardsOpened())
		{
			return;
		}
		BnetGameAccountId accountId = oldPlayerAccount.GetId();
		if (!m_lastOpenedLegendary.TryGetValue(accountId, out var lastOpenedCard))
		{
			m_lastOpenedLegendary[accountId] = newCardsOpened;
			return;
		}
		m_lastOpenedLegendary[accountId] = newCardsOpened;
		if (lastOpenedCard == newCardsOpened)
		{
			return;
		}
		string[] args = newCardsOpened.Split(',');
		if (args.Length != 2)
		{
			return;
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(args[0]);
		if (entityDef != null && Enum.TryParse<TAG_PREMIUM>(args[1], out var premiumTag))
		{
			if (premiumTag == TAG_PREMIUM.GOLDEN)
			{
				AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_GOLDEN_LEGENDARY", "5ecaf0ff", newPlayer.GetBestName(), entityDef.GetName(), "ffd200"));
			}
			else
			{
				AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_LEGENDARY", "5ecaf0ff", newPlayer.GetBestName(), entityDef.GetName(), "ff9c00"));
			}
		}
	}

	private bool CheckForHigherRankForFormat(FormatType format, MedalInfoTranslator currentMedalInfo, MedalInfoTranslator lastKnownMedalInfo, out TranslatedMedalInfo rankToShowToastFor)
	{
		rankToShowToastFor = null;
		TranslatedMedalInfo currMedal = currentMedalInfo.GetCurrentMedal(format);
		TranslatedMedalInfo lastMedal = lastKnownMedalInfo.GetCurrentMedal(format);
		if (!currMedal.IsValid() || !lastMedal.IsValid())
		{
			return false;
		}
		if (currMedal.LeagueConfig.LeagueLevel < lastMedal.LeagueConfig.LeagueLevel)
		{
			return false;
		}
		int lowestStarLevelToCheck = 1;
		if (currMedal.LeagueConfig.LeagueLevel == lastMedal.LeagueConfig.LeagueLevel)
		{
			if (currMedal.starLevel <= lastMedal.starLevel)
			{
				return false;
			}
			lowestStarLevelToCheck = lastMedal.starLevel + 1;
		}
		for (int starLevel = currMedal.starLevel; starLevel >= lowestStarLevelToCheck; starLevel--)
		{
			LeagueRankDbfRecord rankRecord = RankMgr.Get().GetLeagueRankRecord(currMedal.leagueId, starLevel);
			if (rankRecord == null)
			{
				return false;
			}
			if (rankRecord.ShowToastOnAttained)
			{
				rankToShowToastFor = MedalInfoTranslator.CreateTranslatedMedalInfo(format, rankRecord.LeagueId, rankRecord.StarLevel, 0);
				break;
			}
		}
		return true;
	}

	private void CheckForNewRank(BnetPlayer player)
	{
		MedalInfoTranslator currentMedalInfo = RankMgr.Get().GetRankedMedalFromRankPresenceField(player);
		if (currentMedalInfo == null || !currentMedalInfo.IsDisplayable())
		{
			return;
		}
		BnetGameAccountId accountId = player.GetHearthstoneGameAccountId();
		if (!m_lastKnownMedals.ContainsKey(accountId))
		{
			m_lastKnownMedals[accountId] = currentMedalInfo;
			return;
		}
		MedalInfoTranslator lastKnownMedalInfo = m_lastKnownMedals[accountId];
		TranslatedMedalInfo standardRankToShowToastFor;
		bool num = CheckForHigherRankForFormat(FormatType.FT_STANDARD, currentMedalInfo, lastKnownMedalInfo, out standardRankToShowToastFor);
		TranslatedMedalInfo wildRankToShowToastFor;
		bool higherWildRank = CheckForHigherRankForFormat(FormatType.FT_WILD, currentMedalInfo, lastKnownMedalInfo, out wildRankToShowToastFor);
		TranslatedMedalInfo classicRankToShowToastFor;
		bool higherClassicRank = CheckForHigherRankForFormat(FormatType.FT_CLASSIC, currentMedalInfo, lastKnownMedalInfo, out classicRankToShowToastFor);
		if (num || higherWildRank || higherClassicRank)
		{
			m_lastKnownMedals[accountId] = currentMedalInfo;
		}
		if (classicRankToShowToastFor != null)
		{
			if (classicRankToShowToastFor.IsLegendRank())
			{
				AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_RANK_LEGEND_CLASSIC", "5ecaf0ff", player.GetBestName()));
			}
			else
			{
				AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_RANK_EARNED_CLASSIC", "5ecaf0ff", player.GetBestName(), classicRankToShowToastFor.GetRankName()));
			}
		}
		else if (standardRankToShowToastFor != null)
		{
			if (standardRankToShowToastFor.IsLegendRank())
			{
				AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_RANK_LEGEND", "5ecaf0ff", player.GetBestName()));
			}
			else
			{
				AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_RANK_EARNED", "5ecaf0ff", player.GetBestName(), standardRankToShowToastFor.GetRankName()));
			}
		}
		else if (wildRankToShowToastFor != null)
		{
			if (wildRankToShowToastFor.IsLegendRank())
			{
				AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_RANK_LEGEND_WILD", "5ecaf0ff", player.GetBestName()));
			}
			else
			{
				AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_RANK_EARNED_WILD", "5ecaf0ff", player.GetBestName(), wildRankToShowToastFor.GetRankName()));
			}
		}
	}

	private void CheckForMageLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		if (ShouldToastThisLevel(oldPlayerAccount.GetMageLevel(), newPlayerAccount.GetMageLevel()))
		{
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_MAGE_LEVEL", "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetMageLevel()));
		}
	}

	private void CheckForPaladinLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		if (ShouldToastThisLevel(oldPlayerAccount.GetPaladinLevel(), newPlayerAccount.GetPaladinLevel()))
		{
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_PALADIN_LEVEL", "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetPaladinLevel()));
		}
	}

	private void CheckForDruidLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		if (ShouldToastThisLevel(oldPlayerAccount.GetDruidLevel(), newPlayerAccount.GetDruidLevel()))
		{
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_DRUID_LEVEL", "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetDruidLevel()));
		}
	}

	private void CheckForRogueLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		if (ShouldToastThisLevel(oldPlayerAccount.GetRogueLevel(), newPlayerAccount.GetRogueLevel()))
		{
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_ROGUE_LEVEL", "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetRogueLevel()));
		}
	}

	private void CheckForHunterLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		if (ShouldToastThisLevel(oldPlayerAccount.GetHunterLevel(), newPlayerAccount.GetHunterLevel()))
		{
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_HUNTER_LEVEL", "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetHunterLevel()));
		}
	}

	private void CheckForShamanLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		if (ShouldToastThisLevel(oldPlayerAccount.GetShamanLevel(), newPlayerAccount.GetShamanLevel()))
		{
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_SHAMAN_LEVEL", "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetShamanLevel()));
		}
	}

	private void CheckForWarriorLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		if (ShouldToastThisLevel(oldPlayerAccount.GetWarriorLevel(), newPlayerAccount.GetWarriorLevel()))
		{
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_WARRIOR_LEVEL", "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetWarriorLevel()));
		}
	}

	private void CheckForWarlockLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		if (ShouldToastThisLevel(oldPlayerAccount.GetWarlockLevel(), newPlayerAccount.GetWarlockLevel()))
		{
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_WARLOCK_LEVEL", "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetWarlockLevel()));
		}
	}

	private void CheckForPriestLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		if (ShouldToastThisLevel(oldPlayerAccount.GetPriestLevel(), newPlayerAccount.GetPriestLevel()))
		{
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_PRIEST_LEVEL", "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetPriestLevel()));
		}
	}

	private void CheckForDemonHunterLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		if (ShouldToastThisLevel(oldPlayerAccount.GetDemonHunterLevel(), newPlayerAccount.GetDemonHunterLevel()))
		{
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_DEMON_HUNTER_LEVEL", "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetDemonHunterLevel()));
		}
	}

	private void CheckForDeathKnightLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		if (ShouldToastThisLevel(oldPlayerAccount.GetDeathKnightLevel(), newPlayerAccount.GetDeathKnightLevel()))
		{
			AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_DEATH_KNIGHT_LEVEL", "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetDeathKnightLevel()));
		}
	}

	private void CheckForAchievementCompleted(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
	{
		BnetGameAccountId accountId = oldPlayerAccount.GetId();
		int newLastAchievementId = newPlayerAccount.GetLastAchievement();
		if (newLastAchievementId == 0)
		{
			return;
		}
		if (!m_lastAchievementId.TryGetValue(accountId, out var oldLastAchievementId))
		{
			m_lastAchievementId[accountId] = newLastAchievementId;
		}
		else if (oldLastAchievementId != newLastAchievementId)
		{
			m_lastAchievementId[accountId] = newLastAchievementId;
			AchievementDataModel achievementDataModel = AchievementManager.Get().GetAchievementDataModelFromSection(newLastAchievementId);
			if (achievementDataModel != null)
			{
				string toastMessage = ((achievementDataModel.Tier != 1 || achievementDataModel.NextTierID != 0) ? GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_ACHIEVEMENT_TIER", "5ecaf0ff", newPlayer.GetBestName(), achievementDataModel.Name, achievementDataModel.Tier) : GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_ACHIEVEMENT", "5ecaf0ff", newPlayer.GetBestName(), achievementDataModel.Name));
				AddToast(UserAttentionBlocker.NONE, toastMessage);
			}
		}
	}

	private bool ShouldToastThisLevel(int oldLevel, int newLevel)
	{
		if (oldLevel == newLevel)
		{
			return false;
		}
		if (newLevel == 20 || newLevel == 30 || newLevel == 40 || newLevel == 50 || newLevel == 60)
		{
			return true;
		}
		return false;
	}

	private void OnFriendsChanged(BnetFriendChangelist changelist, object userData)
	{
		if (!DemoMgr.Get().IsSocialEnabled())
		{
			return;
		}
		List<BnetInvitation> friendRequests = changelist.GetAddedReceivedInvites();
		if (friendRequests == null)
		{
			return;
		}
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (myself != null && myself.IsBusy())
		{
			return;
		}
		foreach (BnetInvitation invite in friendRequests)
		{
			BnetPlayer recentOpponent = FriendMgr.Get().GetRecentOpponent();
			if (recentOpponent != null && recentOpponent.HasAccount(invite.GetInviterId()))
			{
				AddToast(UserAttentionBlocker.NONE, GameStrings.Get("GLOBAL_SOCIAL_TOAST_RECENT_OPPONENT_FRIEND_REQUEST"));
			}
			else
			{
				AddToast(UserAttentionBlocker.NONE, invite.GetInviterName(), TOAST_TYPE.FRIEND_INVITE);
			}
		}
	}

	private void ShutdownHandler(int minutes)
	{
		AddToast(UserAttentionBlocker.ALL, GameStrings.Format("GLOBAL_SHUTDOWN_TOAST", "f61f1fff", minutes), TOAST_TYPE.DEFAULT, 3.5f);
	}
}
