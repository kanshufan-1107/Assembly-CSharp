using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.Streaming;
using Hearthstone.UI;
using PegasusShared;
using SpectatorProto;
using UnityEngine;

[CustomEditClass]
public class DialogManager : MonoBehaviour
{
	public delegate bool DialogProcessCallback(DialogBase dialog, object userData);

	public enum DialogType
	{
		ALERT,
		SEASON_END,
		FRIENDLY_CHALLENGE,
		TAVERN_BRAWL_CHALLENGE,
		EXISTING_ACCOUNT,
		CARD_LIST,
		STANDARD_COMING_SOON,
		ROTATION_TUTORIAL,
		HALL_OF_FAME,
		TAVERN_BRAWL_CHOICE,
		FIRESIDE_BRAWL_OK,
		FIRESIDE_GATHERING_JOIN,
		FIRESIDE_FIND_EVENT,
		FIRESIDE_LOCATION_HELPER,
		FIRESIDE_INNKEEPER_SETUP,
		RETURNING_PLAYER_OPT_OUT,
		OUTSTANDING_DRAFT_TICKETS,
		FREE_ARENA_WIN,
		ARENA_SEASON,
		ASSET_DOWNLOAD,
		LEAGUE_PROMOTE_SELF_MANUALLY,
		RECONNECT_HELPER,
		LOGIN_POPUP_SEQUENCE_BASIC,
		MULTI_PAGE_POPUP,
		GAME_MODES,
		BACON_CHALLENGE,
		PRIVACY_POLICY,
		MERCENARIES_COOP_CHALLENGE,
		MERCENARIES_FRIENDLY_CHALLENGE,
		MERCENARIES_SEASON_REWARDS,
		EXISTING_ACCOUNT_CN,
		MERCENARIES_ZONE_UNLOCK,
		BATTLEGROUNDS_SUGGESTION,
		BATTLEGROUNDS_LUCKYDRAW_END_SOON,
		INITIAL_DOWNLOAD,
		DOWNLOAD_MANAGER,
		UPDATED_QUESTS_COMPLETED,
		GENERIC_BASIC_POPUP
	}

	public class DialogRequest
	{
		public DialogType m_type;

		public UserAttentionBlocker m_attentionCategory;

		public object m_info;

		public DialogProcessCallback m_callback;

		public object m_userData;

		public string m_prefabAssetReferenceOverride;

		public bool m_isWidget;

		public IDataModel m_dataModel;

		public bool m_isFake;
	}

	[Serializable]
	public class DialogTypeMapping
	{
		public DialogType m_type;

		[CustomEditField(T = EditType.GAME_OBJECT)]
		public string m_prefabName;
	}

	private class SeasonEndDialogRequestInfo
	{
		public NetCache.ProfileNoticeMedal m_noticeMedal;
	}

	private class PopupCallbackSharedData
	{
		public readonly List<GameObject> m_loadedPrefabs = new List<GameObject>();

		public int m_remainingToLoad;

		public PopupCallbackSharedData(int count)
		{
			m_remainingToLoad = count;
		}
	}

	private struct PopupCallbackData
	{
		public PopupCallbackSharedData m_sharedData;

		public int m_index;

		public PopupCallbackData(PopupCallbackSharedData sharedData, int index)
		{
			m_sharedData = sharedData;
			m_index = index;
		}
	}

	private static DialogManager s_instance;

	private Queue<DialogRequest> m_dialogRequests = new Queue<DialogRequest>();

	private DialogBase m_currentDialog;

	private bool m_loadingDialog;

	private bool m_isReadyForSeasonEndPopup;

	private bool m_waitingToShowSeasonEndDialog;

	private bool m_waitingToDownloadInitial;

	private bool m_isReadyForDownloadInitialPopup;

	private List<long> m_handledMedalNoticeIDs = new List<long>();

	public List<DialogTypeMapping> m_typeMapping = new List<DialogTypeMapping>();

	public static event Action OnStarted;

	public event Action OnDialogShown;

	public event Action OnDialogHidden;

	private void Awake()
	{
		s_instance = this;
	}

	private void Start()
	{
		LoginManager.Get().OnInitialClientStateReceived += HandleSeasonEnd;
		if (DialogManager.OnStarted != null)
		{
			DialogManager.OnStarted();
		}
	}

	private void OnDestroy()
	{
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			netCache.RemoveNewNoticesListener(OnNewNotices);
		}
		if (LoginManager.Get() != null)
		{
			LoginManager.Get().OnInitialClientStateReceived -= HandleSeasonEnd;
		}
		s_instance = null;
	}

	public void HandleSeasonEnd()
	{
		NetCache.NetCacheProfileNotices netCacheProfileNotices = NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>();
		if (netCacheProfileNotices != null)
		{
			MaybeShowSeasonEndDialog(netCacheProfileNotices.Notices, fromOutOfBandNotice: false);
		}
		NetCache.Get().RegisterNewNoticesListener(OnNewNotices);
	}

	public static DialogManager Get()
	{
		return s_instance;
	}

	public void GoBack()
	{
		if ((bool)m_currentDialog)
		{
			m_currentDialog.GoBack();
		}
	}

	public void ReadyForSeasonEndPopup(bool ready)
	{
		m_isReadyForSeasonEndPopup = ready;
	}

	public void ClearHandledMedalNotices()
	{
		m_handledMedalNoticeIDs.Clear();
	}

	public bool HandleKeyboardInput()
	{
		if (InputCollection.GetKeyUp(KeyCode.Escape))
		{
			if (!m_currentDialog)
			{
				return false;
			}
			return m_currentDialog.HandleKeyboardInput();
		}
		return false;
	}

	public bool AddToQueue(DialogRequest request)
	{
		UserAttentionBlocker attentionCategory = request?.m_attentionCategory ?? UserAttentionBlocker.NONE;
		if (UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE) || !UserAttentionManager.CanShowAttentionGrabber(attentionCategory, "DialogManager.AddToQueue:" + ((request == null) ? "null" : request.m_type.ToString())))
		{
			return false;
		}
		m_dialogRequests.Enqueue(request);
		UpdateQueue();
		return true;
	}

	private void UpdateQueue()
	{
		if (UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE) || m_currentDialog != null || m_loadingDialog || m_dialogRequests.Count == 0)
		{
			return;
		}
		DialogRequest request = m_dialogRequests.Peek();
		if (!UserAttentionManager.CanShowAttentionGrabber(request.m_attentionCategory, "DialogManager.UpdateQueue:" + request.m_attentionCategory))
		{
			Processor.ScheduleCallback(0.5f, realTime: false, delegate
			{
				UpdateQueue();
			});
		}
		else
		{
			LoadPopup(request);
		}
	}

	public void ShowPopup(AlertPopup.PopupInfo info, DialogProcessCallback callback, object userData)
	{
		UserAttentionBlocker attentionCategory = info?.m_attentionCategory ?? UserAttentionBlocker.NONE;
		if (!UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE) && UserAttentionManager.CanShowAttentionGrabber(attentionCategory, "DialogManager.ShowPopup:" + ((info == null) ? "null" : (info.m_id + ":" + info.m_attentionCategory))))
		{
			DialogRequest request = new DialogRequest();
			request.m_type = DialogType.ALERT;
			request.m_attentionCategory = attentionCategory;
			request.m_info = info;
			request.m_callback = callback;
			request.m_userData = userData;
			AddToQueue(request);
		}
	}

	public void ShowPopup(AlertPopup.PopupInfo info, DialogProcessCallback callback)
	{
		ShowPopup(info, callback, null);
	}

	public void ShowPopup(AlertPopup.PopupInfo info)
	{
		ShowPopup(info, null, null);
	}

	public bool ShowUniquePopup(AlertPopup.PopupInfo info, DialogProcessCallback callback, object userData)
	{
		UserAttentionBlocker attentionCategory = info?.m_attentionCategory ?? UserAttentionBlocker.NONE;
		if (UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE) || !UserAttentionManager.CanShowAttentionGrabber(attentionCategory, "DialogManager.ShowUniquePopup:" + ((info == null) ? "null" : (info.m_id + ":" + info.m_attentionCategory))))
		{
			return false;
		}
		if (info != null && !string.IsNullOrEmpty(info.m_id))
		{
			foreach (DialogRequest request in m_dialogRequests)
			{
				if (request.m_type == DialogType.ALERT && ((AlertPopup.PopupInfo)request.m_info).m_id == info.m_id)
				{
					return false;
				}
			}
		}
		ShowPopup(info, callback, userData);
		return true;
	}

	public bool ShowUniquePopup(AlertPopup.PopupInfo info, DialogProcessCallback callback)
	{
		return ShowUniquePopup(info, callback, null);
	}

	public bool ShowUniquePopup(AlertPopup.PopupInfo info)
	{
		return ShowUniquePopup(info, null, null);
	}

	public void ShowMessageOfTheDay(string message)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_text = message;
		ShowPopup(info);
	}

	public void RemoveUniquePopupRequestFromQueue(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return;
		}
		foreach (DialogRequest request in m_dialogRequests)
		{
			if (request.m_info is AlertPopup.PopupInfo && ((AlertPopup.PopupInfo)request.m_info).m_id == id)
			{
				m_dialogRequests = new Queue<DialogRequest>(m_dialogRequests.Where((DialogRequest r) => r.m_info != null && r.m_info.GetType() == typeof(AlertPopup.PopupInfo) && ((AlertPopup.PopupInfo)r.m_info).m_id != id));
				break;
			}
		}
	}

	public void DismissAlertOrRemoveFromQueue(string id)
	{
		RemoveUniquePopupRequestFromQueue(id);
		if (m_currentDialog != null && m_currentDialog is AlertPopup)
		{
			AlertPopup.PopupInfo currentPopupInfo = ((AlertPopup)m_currentDialog).GetInfo();
			if (currentPopupInfo != null && currentPopupInfo.m_id == id)
			{
				m_currentDialog.Hide();
			}
		}
	}

	public bool WaitingToShowSeasonEndDialog()
	{
		if (m_waitingToShowSeasonEndDialog || (m_currentDialog != null && m_currentDialog is SeasonEndDialog))
		{
			return true;
		}
		return m_dialogRequests.FirstOrDefault((DialogRequest obj) => obj.m_type == DialogType.SEASON_END) != null;
	}

	public IEnumerator<IAsyncJobResult> Job_WaitForSeasonEndPopup()
	{
		ReadyForSeasonEndPopup(ready: true);
		while (WaitingToShowSeasonEndDialog())
		{
			yield return null;
		}
	}

	public void ShowFriendlyChallenge(FormatType formatType, BnetPlayer challenger, bool challengeIsTavernBrawl, PartyType partyType, PartyQuestInfo questInfo, FriendlyChallengeDialog.ResponseCallback responseCallback, DialogProcessCallback callback)
	{
		DialogRequest request = new DialogRequest();
		if (challengeIsTavernBrawl)
		{
			request.m_type = DialogType.TAVERN_BRAWL_CHALLENGE;
		}
		else
		{
			switch (partyType)
			{
			case PartyType.BATTLEGROUNDS_PARTY:
				request.m_type = DialogType.BACON_CHALLENGE;
				break;
			case PartyType.MERCENARIES_COOP_PARTY:
				request.m_type = DialogType.MERCENARIES_COOP_CHALLENGE;
				break;
			case PartyType.MERCENARIES_FRIENDLY_CHALLENGE:
				request.m_type = DialogType.MERCENARIES_FRIENDLY_CHALLENGE;
				break;
			default:
				request.m_type = DialogType.FRIENDLY_CHALLENGE;
				break;
			}
		}
		FriendlyChallengeDialog.Info info = new FriendlyChallengeDialog.Info();
		info.m_formatType = formatType;
		info.m_challenger = challenger;
		info.m_partyType = partyType;
		info.m_questInfo = questInfo;
		info.m_callback = responseCallback;
		request.m_info = info;
		request.m_callback = callback;
		AddToQueue(request);
	}

	public void ShowBattlegroundsSuggestion(BnetGameAccountId playerToInviteGameAccountId, string playerToInviteName, BnetGameAccountId suggesterGameAccountId, string suggesterName, BattlegroundsSuggestDialog.ResponseCallback responseCallback)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.BATTLEGROUNDS_SUGGESTION;
		BattlegroundsSuggestDialog.Info info = new BattlegroundsSuggestDialog.Info();
		info.PlayerToInviteGameAccountId = playerToInviteGameAccountId;
		info.PlayerToInviteName = playerToInviteName;
		info.SuggesterGameAccountId = suggesterGameAccountId;
		info.SuggesterName = suggesterName;
		info.Callback = responseCallback;
		info.m_id = $"partysuggestion_{playerToInviteGameAccountId.Low}";
		request.m_info = info;
		AddToQueue(request);
	}

	public void ShowBattlegroundsLuckyDrawEndSoonPopup(LuckyDrawDataModel dataModel, DialogProcessCallback cb)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.BATTLEGROUNDS_LUCKYDRAW_END_SOON;
		request.m_isWidget = true;
		request.m_dataModel = dataModel;
		request.m_callback = cb;
		AddToQueue(request);
	}

	public void ShowPrivacyPolicyPopup(PrivacyPolicyPopup.ResponseCallback responseCallback, DialogProcessCallback callback)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.PRIVACY_POLICY;
		PrivacyPolicyPopup.Info info = new PrivacyPolicyPopup.Info();
		info.m_callback = responseCallback;
		request.m_info = info;
		request.m_callback = callback;
		AddToQueue(request);
	}

	public void ShowExistingAccountPopup(ExistingAccountPopup.ResponseCallback responseCallback, DialogProcessCallback callback, bool useCNStyle)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = (useCNStyle ? DialogType.EXISTING_ACCOUNT_CN : DialogType.EXISTING_ACCOUNT);
		ExistingAccountPopup.Info info = new ExistingAccountPopup.Info();
		info.m_callback = responseCallback;
		request.m_info = info;
		request.m_callback = callback;
		AddToQueue(request);
	}

	public void ShowLeaguePromoteSelfManuallyDialog(LeaguePromoteSelfManuallyDialog.ResponseCallback callback)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.LEAGUE_PROMOTE_SELF_MANUALLY;
		LeaguePromoteSelfManuallyDialog.Info info = new LeaguePromoteSelfManuallyDialog.Info();
		info.m_callback = callback;
		request.m_info = info;
		request.m_callback = null;
		AddToQueue(request);
	}

	public void ShowCardListPopup(UserAttentionBlocker attentionCategory, CardListPopup.Info info)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.CARD_LIST;
		request.m_attentionCategory = attentionCategory;
		request.m_info = info;
		AddToQueue(request);
	}

	public void ShowSetRotationTutorialPopup(UserAttentionBlocker attentionCategory, SetRotationRotatedBoostersPopup.SetRotationRotatedBoostersPopupInfo info)
	{
		info.m_prefabAssetRefs.Add(new AssetReference("SetRotationRotatedBoostersPopup.prefab:2a1c1ce78c98c1e418039a479c8ddce4"));
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.GENERIC_BASIC_POPUP;
		request.m_attentionCategory = attentionCategory;
		request.m_info = info;
		request.m_isWidget = true;
		AddToQueue(request);
	}

	public void ShowOutstandingDraftTicketPopup(UserAttentionBlocker attentionCategory, OutstandingDraftTicketDialog.Info info)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.OUTSTANDING_DRAFT_TICKETS;
		request.m_attentionCategory = attentionCategory;
		request.m_info = info;
		AddToQueue(request);
	}

	public void ShowFreeArenaWinPopup(UserAttentionBlocker attentionCategory, FreeArenaWinDialog.Info info)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.FREE_ARENA_WIN;
		request.m_attentionCategory = attentionCategory;
		request.m_info = info;
		AddToQueue(request);
	}

	public bool ShowArenaSeasonPopup(UserAttentionBlocker attentionCategory, BasicPopup.PopupInfo info)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.ARENA_SEASON;
		request.m_attentionCategory = attentionCategory;
		request.m_info = info;
		return AddToQueue(request);
	}

	public void ShowLoginPopupSequenceBasicPopup(UserAttentionBlocker attentionCategory, LoginPopupSequencePopup.Info info)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.LOGIN_POPUP_SEQUENCE_BASIC;
		request.m_attentionCategory = attentionCategory;
		request.m_info = info;
		request.m_prefabAssetReferenceOverride = info.m_prefabAssetReference;
		AddToQueue(request);
	}

	public void ShowMultiPagePopup(UserAttentionBlocker attentionCategory, MultiPagePopup.Info info)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.MULTI_PAGE_POPUP;
		request.m_attentionCategory = attentionCategory;
		request.m_info = info;
		request.m_prefabAssetReferenceOverride = "MultiPagePopup.prefab:a9b6df0282662ed449031d34aa2ecfa7";
		AddToQueue(request);
	}

	public bool ShowBasicPopup(UserAttentionBlocker attentionCategory, BasicPopup.PopupInfo info)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.GENERIC_BASIC_POPUP;
		request.m_attentionCategory = attentionCategory;
		request.m_info = info;
		return AddToQueue(request);
	}

	public bool ShowAssetDownloadPopup(AssetDownloadDialog.Info info)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.ASSET_DOWNLOAD;
		request.m_attentionCategory = UserAttentionBlocker.NONE;
		request.m_info = info;
		return AddToQueue(request);
	}

	public bool ShowInitialDownloadPopup(InitialDownloadDialog.Info info)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.INITIAL_DOWNLOAD;
		request.m_attentionCategory = UserAttentionBlocker.INITIAL_DOWNLOAD;
		request.m_info = info;
		return AddToQueue(request);
	}

	public bool ShowCompletedQuestsUpdatedListPopup(UserAttentionBlocker attentionCategory, CompletedQuestsUpdatedPopup.Info info)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.UPDATED_QUESTS_COMPLETED;
		request.m_attentionCategory = attentionCategory;
		request.m_info = info;
		return AddToQueue(request);
	}

	public void AllowShowInitialDownloadPopup()
	{
		if (!GameDownloadManagerProvider.Get().IsCompletedInitialBaseDownload())
		{
			m_waitingToDownloadInitial = false;
			m_isReadyForDownloadInitialPopup = true;
		}
	}

	public bool ShowInitialDownloadPopupDuringDownload()
	{
		if (GameDownloadManagerProvider.Get().IsReadyToPlay)
		{
			return false;
		}
		if (!m_isReadyForDownloadInitialPopup)
		{
			return false;
		}
		if (m_waitingToDownloadInitial)
		{
			return true;
		}
		m_waitingToDownloadInitial = true;
		return ShowInitialDownloadPopup(new InitialDownloadDialog.Info());
	}

	public void ShowReconnectHelperDialog(Action reconnectSuccessCallback = null, Action goBackCallback = null)
	{
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.RECONNECT_HELPER;
		ReconnectHelperDialog.Info info = new ReconnectHelperDialog.Info();
		info.m_reconnectSuccessCallback = reconnectSuccessCallback;
		info.m_goBackCallback = goBackCallback;
		request.m_info = info;
		request.m_callback = null;
		AddToQueue(request);
	}

	public void ShowClassUpcomingPopup()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_showAlertIcon = false;
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		info.m_headerText = GameStrings.Get("GLUE_CLASS_UPCOMING_HEADER");
		info.m_text = GameStrings.Get("GLUE_CLASS_UPCOMING_DESC");
		Get().ShowPopup(info);
	}

	public void ShowBonusStarsPopup(RankedPlayDataModel dataModel, Action onHiddenCallback)
	{
		RankedBonusStarsPopup.BonusStarsPopupInfo info = new RankedBonusStarsPopup.BonusStarsPopupInfo
		{
			m_onHiddenCallback = onHiddenCallback
		};
		DialogRequest request = new DialogRequest
		{
			m_type = DialogType.GENERIC_BASIC_POPUP,
			m_dataModel = dataModel,
			m_info = info,
			m_isWidget = true
		};
		info.m_prefabAssetRefs.Add(RankMgr.BONUS_STAR_POPUP_PREFAB);
		AddToQueue(request);
	}

	public void ShowRankedIntroPopUp(Action onHiddenCallback)
	{
		RankedIntroPopup.RankedIntroPopupInfo info = new RankedIntroPopup.RankedIntroPopupInfo
		{
			m_onHiddenCallback = onHiddenCallback
		};
		DialogRequest request = new DialogRequest
		{
			m_type = DialogType.GENERIC_BASIC_POPUP,
			m_info = info,
			m_isWidget = true
		};
		info.m_prefabAssetRefs.Add(RankMgr.RANKED_INTRO_POPUP_PREFAB);
		AddToQueue(request);
	}

	public void ShowDeckArchetypesIntroPopup(Action onHiddenCallback)
	{
		DeckArchetypesIntroPopup.DeckArchetypesIntroPopupInfo info = new DeckArchetypesIntroPopup.DeckArchetypesIntroPopupInfo
		{
			m_prefabAssetRefs = { (string)DeckArchetypesIntroPopup.PrefabReference },
			m_onHiddenCallback = onHiddenCallback
		};
		DialogRequest request = new DialogRequest
		{
			m_type = DialogType.GENERIC_BASIC_POPUP,
			m_info = info,
			m_isWidget = true
		};
		AddToQueue(request);
	}

	public void ShowRewardTrackInfographic(Action onHiddenCallback)
	{
		RewardTrackInfographic.RewardTrackInfographicInfo info = new RewardTrackInfographic.RewardTrackInfographicInfo
		{
			m_prefabAssetRefs = { (string)RewardTrackInfographic.PrefabReference },
			m_onHiddenCallback = onHiddenCallback
		};
		DialogRequest request = new DialogRequest
		{
			m_type = DialogType.GENERIC_BASIC_POPUP,
			m_info = info,
			m_isWidget = true
		};
		AddToQueue(request);
	}

	public void ClearAllImmediately()
	{
		if (m_currentDialog != null)
		{
			UnityEngine.Object.DestroyImmediate(m_currentDialog.gameObject);
			m_currentDialog = null;
		}
		m_dialogRequests.Clear();
	}

	public void ClearAllImmediatelyDontDestroy()
	{
		if (m_currentDialog != null)
		{
			m_currentDialog.Hide();
			m_currentDialog = null;
		}
		m_dialogRequests.Clear();
	}

	public bool ShowingDialog()
	{
		if (!(m_currentDialog != null))
		{
			return m_dialogRequests.Count > 0;
		}
		return true;
	}

	public bool ShowingHighPriorityDialog()
	{
		if (m_currentDialog != null)
		{
			return m_currentDialog.gameObject.layer == 27;
		}
		return false;
	}

	private void OnNewNotices(List<NetCache.ProfileNotice> newNotices, bool isInitialNoticeList)
	{
		MaybeShowSeasonEndDialog(newNotices, !isInitialNoticeList);
	}

	private void MaybeShowSeasonEndDialog(List<NetCache.ProfileNotice> newNotices, bool fromOutOfBandNotice)
	{
		newNotices.Sort(delegate(NetCache.ProfileNotice a, NetCache.ProfileNotice b)
		{
			if (a.Type != b.Type)
			{
				return a.Type - b.Type;
			}
			if (a.Origin != b.Origin)
			{
				return a.Origin - b.Origin;
			}
			return (int)((a.OriginData != b.OriginData) ? (a.OriginData - b.OriginData) : (a.NoticeID - b.NoticeID));
		});
		NetCache.ProfileNotice notice = MaybeShowSeasonEndDialog_GetLatestMedalNotice(newNotices);
		if (notice == null || !(notice is NetCache.ProfileNoticeMedal medalNotice) || m_handledMedalNoticeIDs.Contains(medalNotice.NoticeID) || UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE) || !UserAttentionManager.CanShowAttentionGrabber("DialogManager.MaybeShowSeasonEndDialog"))
		{
			return;
		}
		m_handledMedalNoticeIDs.Add(medalNotice.NoticeID);
		if (ReturningPlayerMgr.Get().SuppressOldPopups)
		{
			Log.ReturningPlayer.Print("Suppressing popup for Season End Dialogue {0} due to being a Returning Player!");
			Network.Get().AckNotice(medalNotice.NoticeID);
			return;
		}
		if (fromOutOfBandNotice)
		{
			NetCache.Get().RefreshNetObject<NetCache.NetCacheMedalInfo>();
			NetCache.Get().ReloadNetObject<NetCache.NetCacheRewardProgress>();
		}
		SeasonEndDialogRequestInfo requestInfo = new SeasonEndDialogRequestInfo();
		requestInfo.m_noticeMedal = medalNotice;
		DialogRequest request = new DialogRequest();
		request.m_type = DialogType.SEASON_END;
		request.m_info = requestInfo;
		StartCoroutine(ShowSeasonEndDialogWhenReady(request));
	}

	private NetCache.ProfileNotice MaybeShowSeasonEndDialog_GetLatestMedalNotice(List<NetCache.ProfileNotice> newNotices)
	{
		List<NetCache.ProfileNotice> source = new List<NetCache.ProfileNotice>(newNotices);
		IEnumerable<NetCache.ProfileNotice> medalNotices = source.Where((NetCache.ProfileNotice notice) => notice.Type == NetCache.ProfileNotice.NoticeType.GAINED_MEDAL);
		IEnumerable<NetCache.ProfileNotice> enumerable = source.Where((NetCache.ProfileNotice notice) => notice.Type == NetCache.ProfileNotice.NoticeType.BONUS_STARS);
		if (medalNotices.Any())
		{
			long MINIMUM_SEASON_TO_SHOW = 52L;
			long maxSeason = Math.Max(MINIMUM_SEASON_TO_SHOW, medalNotices.Max((NetCache.ProfileNotice n) => n.OriginData));
			medalNotices.Where((NetCache.ProfileNotice notice) => notice.OriginData != maxSeason).ForEach(delegate(NetCache.ProfileNotice notice)
			{
				Network.Get().AckNotice(notice.NoticeID);
			});
			medalNotices = medalNotices.Where((NetCache.ProfileNotice notice) => notice.OriginData == maxSeason);
			medalNotices.Skip(1).ForEach(delegate(NetCache.ProfileNotice notice)
			{
				Network.Get().AckNotice(notice.NoticeID);
			});
		}
		enumerable.ForEach(delegate(NetCache.ProfileNotice notice)
		{
			Network.Get().AckNotice(notice.NoticeID);
		});
		return medalNotices.FirstOrDefault();
	}

	private void LoadPopup(DialogRequest request)
	{
		List<string> prefabAssetRefs = null;
		if (request.m_info is BasicPopup.PopupInfo)
		{
			prefabAssetRefs = ((BasicPopup.PopupInfo)request.m_info).m_prefabAssetRefs;
		}
		else
		{
			prefabAssetRefs = new List<string>();
			string prefabAssetReference = GetPrefabNameFromDialogRequest(request);
			prefabAssetRefs.Add(prefabAssetReference);
		}
		if (prefabAssetRefs == null || prefabAssetRefs.Count == 0 || string.IsNullOrEmpty(prefabAssetRefs[0]))
		{
			Error.AddDevFatal("DialogManager.LoadPopup() - no prefab to load for type={0} info={1} attnCategory={2} prefabName={3}", request.m_type, request.m_info, request.m_attentionCategory, (prefabAssetRefs == null) ? "<null>" : ((prefabAssetRefs.Count == 0) ? "<empty>" : (prefabAssetRefs[0] ?? "null")));
			return;
		}
		prefabAssetRefs.RemoveAll((string assetRef) => string.IsNullOrEmpty(assetRef));
		m_loadingDialog = true;
		PopupCallbackSharedData cbSharedData = new PopupCallbackSharedData(prefabAssetRefs.Count);
		for (int i = 0; i < prefabAssetRefs.Count; i++)
		{
			cbSharedData.m_loadedPrefabs.Add(null);
		}
		for (int j = 0; j < prefabAssetRefs.Count; j++)
		{
			PopupCallbackData cbData = new PopupCallbackData(cbSharedData, j);
			if (request.m_isWidget)
			{
				WidgetInstance widgetInstance = WidgetInstance.Create(prefabAssetRefs[j]);
				if (request.m_dataModel != null)
				{
					widgetInstance.BindDataModel(request.m_dataModel);
				}
				StartCoroutine(WaitForWidgetPopupReady(prefabAssetRefs[j], widgetInstance, cbData));
			}
			else
			{
				AssetLoader.Get().InstantiatePrefab(prefabAssetRefs[j], OnPopupLoaded, cbData);
			}
		}
	}

	private string GetPrefabNameFromDialogRequest(DialogRequest request)
	{
		if (!string.IsNullOrEmpty(request.m_prefabAssetReferenceOverride))
		{
			return request.m_prefabAssetReferenceOverride;
		}
		DialogTypeMapping asset = m_typeMapping.Find((DialogTypeMapping x) => x.m_type == request.m_type);
		if (asset == null || asset.m_prefabName == null)
		{
			Error.AddDevFatal("DialogManager.GetPrefabNameFromDialogRequest() - unhandled dialog type {0}", request.m_type);
			return null;
		}
		return asset.m_prefabName;
	}

	private IEnumerator WaitForWidgetPopupReady(AssetReference assetRef, WidgetInstance widgetInstance, object callbackData)
	{
		if (!(widgetInstance == null))
		{
			widgetInstance.Hide();
			while (!widgetInstance.IsReady || widgetInstance.IsChangingStates)
			{
				yield return null;
			}
			OnPopupLoaded(assetRef, widgetInstance.gameObject, callbackData);
			widgetInstance.Show();
		}
	}

	private void OnPopupLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		DialogRequest request = ((m_dialogRequests.Count == 0) ? null : m_dialogRequests.Peek());
		UserAttentionBlocker attentionCategory = request?.m_attentionCategory ?? UserAttentionBlocker.NONE;
		if (m_dialogRequests.Count == 0 || UserAttentionManager.IsBlockedBy(UserAttentionBlocker.FATAL_ERROR_SCENE) || !UserAttentionManager.CanShowAttentionGrabber(attentionCategory, "DialogManager.OnPopupLoaded:" + ((request == null) ? "null" : request.m_type.ToString())))
		{
			m_loadingDialog = false;
			UnityEngine.Object.DestroyImmediate(go);
			return;
		}
		PopupCallbackData cbData = (PopupCallbackData)callbackData;
		cbData.m_sharedData.m_loadedPrefabs[cbData.m_index] = go;
		if (--cbData.m_sharedData.m_remainingToLoad > 0)
		{
			return;
		}
		m_loadingDialog = false;
		request = m_dialogRequests.Dequeue();
		GameObject firstAssetLoaded = ((cbData.m_sharedData.m_loadedPrefabs.Count == 0) ? null : cbData.m_sharedData.m_loadedPrefabs[0]);
		DialogBase dialog = ((firstAssetLoaded == null) ? null : firstAssetLoaded.GetComponentInChildren<DialogBase>());
		if (dialog == null)
		{
			Debug.LogError($"DialogManager.OnPopupLoaded() - game object {go} has no DialogBase component (request_type={request.m_type} count prefabs loaded={cbData.m_sharedData.m_loadedPrefabs.Count})");
			UnityEngine.Object.DestroyImmediate(go);
			UpdateQueue();
			return;
		}
		for (int i = 1; i < cbData.m_sharedData.m_loadedPrefabs.Count; i++)
		{
			GameObject loadedPrefab = cbData.m_sharedData.m_loadedPrefabs[i];
			if (!(loadedPrefab == null))
			{
				loadedPrefab.transform.SetParent(firstAssetLoaded.transform, worldPositionStays: false);
			}
		}
		ProcessRequest(request, dialog);
	}

	private void ProcessRequest(DialogRequest request, DialogBase dialog)
	{
		if (request.m_callback != null && !request.m_callback(dialog, request.m_userData))
		{
			UpdateQueue();
			UnityEngine.Object.Destroy(dialog.gameObject);
			return;
		}
		m_currentDialog = dialog;
		m_currentDialog.SetReadyToDestroyCallback(OnCurrentDialogHidden);
		if (request.m_type == DialogType.ALERT)
		{
			ProcessAlertRequest(request, (AlertPopup)dialog);
		}
		else if (request.m_type == DialogType.SEASON_END)
		{
			ProcessMedalRequest(request, (SeasonEndDialog)dialog);
		}
		else if (request.m_type == DialogType.FRIENDLY_CHALLENGE || request.m_type == DialogType.TAVERN_BRAWL_CHALLENGE || request.m_type == DialogType.MERCENARIES_COOP_CHALLENGE || request.m_type == DialogType.MERCENARIES_FRIENDLY_CHALLENGE)
		{
			ProcessFriendlyChallengeRequest(request, (FriendlyChallengeDialog)dialog);
		}
		else if (request.m_type == DialogType.BACON_CHALLENGE)
		{
			ProcessBattlegroundsInviteRequest(request, (BattlegroundsInviteDialog)dialog);
		}
		else if (request.m_type == DialogType.BATTLEGROUNDS_SUGGESTION)
		{
			ProcessBattlegroundsSuggestionRequest(request, (BattlegroundsSuggestDialog)dialog);
		}
		else if (request.m_type == DialogType.EXISTING_ACCOUNT || request.m_type == DialogType.EXISTING_ACCOUNT_CN)
		{
			ProcessExistingAccountRequest(request, (ExistingAccountPopup)dialog);
		}
		else if (request.m_type == DialogType.CARD_LIST)
		{
			ProcessCardListRequest(request, (CardListPopup)dialog);
		}
		else if (request.m_type == DialogType.LEAGUE_PROMOTE_SELF_MANUALLY)
		{
			ProcessLeaguePromoteSelfManuallyRequest(request, (LeaguePromoteSelfManuallyDialog)dialog);
		}
		else if (request.m_type == DialogType.OUTSTANDING_DRAFT_TICKETS)
		{
			ProcessOutstandingDraftTicketDialog(request, (OutstandingDraftTicketDialog)dialog);
		}
		else if (request.m_type == DialogType.FREE_ARENA_WIN)
		{
			ProcessFreeArenaWinDialog(request, (FreeArenaWinDialog)dialog);
		}
		else if (request.m_type == DialogType.GENERIC_BASIC_POPUP || request.m_type == DialogType.ARENA_SEASON)
		{
			ProcessBasicPopupRequest(request, (BasicPopup)dialog);
		}
		else if (request.m_type == DialogType.ASSET_DOWNLOAD)
		{
			ProcessAssetDownloadRequest(request, (AssetDownloadDialog)dialog);
		}
		else if (request.m_type == DialogType.RECONNECT_HELPER)
		{
			ProcessReconnectRequest(request, (ReconnectHelperDialog)dialog);
		}
		else if (request.m_type == DialogType.LOGIN_POPUP_SEQUENCE_BASIC)
		{
			ProcessLoginPopupSequenceBasicPopupRequest(request, (LoginPopupSequencePopup)dialog);
		}
		else if (request.m_type == DialogType.MULTI_PAGE_POPUP)
		{
			ProcessMultiPagePopupRequest(request, (MultiPagePopup)dialog);
		}
		else if (request.m_type == DialogType.PRIVACY_POLICY)
		{
			ProcessPrivacyPolicyRequest(request, (PrivacyPolicyPopup)dialog);
		}
		else if (request.m_type == DialogType.MERCENARIES_SEASON_REWARDS)
		{
			ProcessMercenariesSeasonRewardsDialog(request, (MercenariesSeasonRewardsDialog)dialog);
		}
		else if (request.m_type == DialogType.MERCENARIES_ZONE_UNLOCK)
		{
			ProcessMercenariesZoneUnlockDialog(request, (MercenariesZoneUnlockDialog)dialog);
		}
		else if (request.m_type == DialogType.INITIAL_DOWNLOAD)
		{
			ProcessInitialDownloadRequest(request, (InitialDownloadDialog)dialog);
		}
		else if (request.m_type == DialogType.UPDATED_QUESTS_COMPLETED)
		{
			ProcessCompletedQuestsUpdatedRequest(request, (CompletedQuestsUpdatedPopup)dialog);
		}
		if (this.OnDialogShown != null)
		{
			this.OnDialogShown();
		}
	}

	private void ProcessExistingAccountRequest(DialogRequest request, ExistingAccountPopup exAcctPopup)
	{
		exAcctPopup.SetInfo((ExistingAccountPopup.Info)request.m_info);
		exAcctPopup.Show();
	}

	private void ProcessAlertRequest(DialogRequest request, AlertPopup alertPopup)
	{
		AlertPopup.PopupInfo info = (AlertPopup.PopupInfo)request.m_info;
		alertPopup.SetInfo(info);
		alertPopup.Show();
	}

	private void ProcessBasicPopupRequest(DialogRequest request, BasicPopup basicPopup)
	{
		BasicPopup.PopupInfo info = (BasicPopup.PopupInfo)request.m_info;
		basicPopup.SetInfo(info);
		basicPopup.Show();
	}

	private void ProcessAssetDownloadRequest(DialogRequest request, AssetDownloadDialog dialog)
	{
		dialog.Show();
	}

	private void ProcessReconnectRequest(DialogRequest request, ReconnectHelperDialog dialog)
	{
		dialog.SetInfo((ReconnectHelperDialog.Info)request.m_info);
		dialog.Show();
	}

	private void ProcessMedalRequest(DialogRequest request, SeasonEndDialog seasonEndDialog)
	{
		SeasonEndDialog.SeasonEndInfo seasonEndInfo = null;
		if (request.m_isFake)
		{
			seasonEndInfo = request.m_info as SeasonEndDialog.SeasonEndInfo;
			if (seasonEndInfo == null)
			{
				return;
			}
		}
		else
		{
			SeasonEndDialogRequestInfo requestInfo = request.m_info as SeasonEndDialogRequestInfo;
			if (PopupDisplayManager.ShouldDisableNotificationOnLogin())
			{
				Network.Get().AckNotice(requestInfo.m_noticeMedal.NoticeID);
				UIStatus.Get().AddInfo("Season Roll skipped due to disableLoginPopups", 5f);
				return;
			}
			seasonEndInfo = new SeasonEndDialog.SeasonEndInfo();
			seasonEndInfo.m_noticesToAck.Add(requestInfo.m_noticeMedal.NoticeID);
			seasonEndInfo.m_seasonID = (int)requestInfo.m_noticeMedal.OriginData;
			seasonEndInfo.m_leagueId = requestInfo.m_noticeMedal.LeagueId;
			seasonEndInfo.m_starLevelAtEndOfSeason = requestInfo.m_noticeMedal.StarLevel;
			seasonEndInfo.m_bestStarLevelAtEndOfSeason = requestInfo.m_noticeMedal.BestStarLevel;
			seasonEndInfo.m_legendIndex = requestInfo.m_noticeMedal.LegendRank;
			seasonEndInfo.m_rankedRewards = requestInfo.m_noticeMedal.Chest.Rewards;
			seasonEndInfo.m_formatType = requestInfo.m_noticeMedal.FormatType;
			seasonEndInfo.m_wasLimitedByBestEverStarLevel = requestInfo.m_noticeMedal.WasLimitedByBestEverStarLevel;
		}
		seasonEndDialog.Init(seasonEndInfo);
		seasonEndDialog.Show();
	}

	private void ProcessFriendlyChallengeRequest(DialogRequest request, FriendlyChallengeDialog friendlyChallengeDialog)
	{
		friendlyChallengeDialog.SetInfo((FriendlyChallengeDialog.Info)request.m_info);
		friendlyChallengeDialog.Show();
	}

	private void ProcessBattlegroundsInviteRequest(DialogRequest request, BattlegroundsInviteDialog battlegroundsInviteDialog)
	{
		battlegroundsInviteDialog.SetInfo((FriendlyChallengeDialog.Info)request.m_info);
		battlegroundsInviteDialog.Show();
	}

	private void ProcessBattlegroundsSuggestionRequest(DialogRequest request, BattlegroundsSuggestDialog battlegroundsSuggestDialog)
	{
		battlegroundsSuggestDialog.SetInfo((BattlegroundsSuggestDialog.Info)request.m_info);
		battlegroundsSuggestDialog.Show();
	}

	private void ProcessCardListRequest(DialogRequest request, CardListPopup cardListPopup)
	{
		CardListPopup.Info info = (CardListPopup.Info)request.m_info;
		cardListPopup.SetInfo(info);
		cardListPopup.Show();
	}

	private void ProcessSetRotationRotatedBoostersPopupRequest(DialogRequest request, SetRotationRotatedBoostersPopup setRotationTutorialDialog)
	{
		SetRotationRotatedBoostersPopup.SetRotationRotatedBoostersPopupInfo info = (SetRotationRotatedBoostersPopup.SetRotationRotatedBoostersPopupInfo)request.m_info;
		setRotationTutorialDialog.SetInfo(info);
		setRotationTutorialDialog.Show();
	}

	private void ProcessLeaguePromoteSelfManuallyRequest(DialogRequest request, LeaguePromoteSelfManuallyDialog leaguePromoteSelfManuallyDialog)
	{
		LeaguePromoteSelfManuallyDialog.Info info = (LeaguePromoteSelfManuallyDialog.Info)request.m_info;
		leaguePromoteSelfManuallyDialog.SetInfo(info);
		leaguePromoteSelfManuallyDialog.Show();
	}

	private void ProcessOutstandingDraftTicketDialog(DialogRequest request, OutstandingDraftTicketDialog outstandingDraftTicketDialog)
	{
		OutstandingDraftTicketDialog.Info info = (OutstandingDraftTicketDialog.Info)request.m_info;
		outstandingDraftTicketDialog.SetInfo(info);
		outstandingDraftTicketDialog.Show();
	}

	private void ProcessFreeArenaWinDialog(DialogRequest request, FreeArenaWinDialog freeArenaWinDialog)
	{
		FreeArenaWinDialog.Info info = (FreeArenaWinDialog.Info)request.m_info;
		freeArenaWinDialog.SetInfo(info);
		freeArenaWinDialog.Show();
	}

	private void ProcessLoginPopupSequenceBasicPopupRequest(DialogRequest request, LoginPopupSequencePopup loginPopupSequencePopup)
	{
		LoginPopupSequencePopup.Info info = (LoginPopupSequencePopup.Info)request.m_info;
		loginPopupSequencePopup.SetInfo(info);
		loginPopupSequencePopup.LoadAssetsAndShowWhenReady();
	}

	private void ProcessPrivacyPolicyRequest(DialogRequest request, PrivacyPolicyPopup privacyPolicyPopup)
	{
		privacyPolicyPopup.SetInfo((PrivacyPolicyPopup.Info)request.m_info);
		privacyPolicyPopup.Show();
	}

	private void ProcessMultiPagePopupRequest(DialogRequest request, MultiPagePopup multiPagePopup)
	{
		MultiPagePopup.Info info = (MultiPagePopup.Info)request.m_info;
		multiPagePopup.SetInfo(info);
		multiPagePopup.Show();
	}

	private void ProcessMercenariesSeasonRewardsDialog(DialogRequest request, MercenariesSeasonRewardsDialog dialog)
	{
		dialog.SetInfo((MercenariesSeasonRewardsDialog.Info)request.m_info);
		dialog.Show();
	}

	private void ProcessMercenariesZoneUnlockDialog(DialogRequest request, MercenariesZoneUnlockDialog dialog)
	{
		dialog.SetInfo((MercenariesZoneUnlockDialog.Info)request.m_info);
		dialog.Show();
	}

	private void ProcessInitialDownloadRequest(DialogRequest request, InitialDownloadDialog dialog)
	{
		dialog.Show();
	}

	private void ProcessCompletedQuestsUpdatedRequest(DialogRequest request, CompletedQuestsUpdatedPopup questListPopup)
	{
		CompletedQuestsUpdatedPopup.Info info = (CompletedQuestsUpdatedPopup.Info)request.m_info;
		questListPopup.SetInfo(info);
		questListPopup.Show();
	}

	private void OnCurrentDialogHidden(DialogBase dialog)
	{
		if (!(dialog != m_currentDialog))
		{
			UnityEngine.Object.Destroy(m_currentDialog.gameObject);
			m_currentDialog = null;
			UpdateQueue();
			if (this.OnDialogHidden != null)
			{
				this.OnDialogHidden();
			}
		}
	}

	private IEnumerator ShowSeasonEndDialogWhenReady(DialogRequest request)
	{
		m_waitingToShowSeasonEndDialog = true;
		while (!NetCache.Get().IsNetObjectAvailable<NetCache.NetCacheRewardProgress>() || !m_isReadyForSeasonEndPopup)
		{
			yield return null;
		}
		while (SceneMgr.Get().IsTransitioning())
		{
			yield return null;
		}
		while (SceneMgr.Get().GetMode() != SceneMgr.Mode.HUB)
		{
			if ((SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT || SceneMgr.Get().GetMode() == SceneMgr.Mode.LOGIN) && !SceneMgr.Get().IsTransitioning())
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
				break;
			}
			yield return null;
		}
		while (SceneMgr.Get().IsTransitioning())
		{
			yield return null;
		}
		AddToQueue(request);
		m_waitingToShowSeasonEndDialog = false;
	}

	public void ShowMercenariesSeasonRewardsDialog(NetCache.ProfileNoticeMercenariesSeasonRewards rewardNotice, Action doneCallback = null)
	{
		DialogRequest request = new DialogRequest
		{
			m_type = DialogType.MERCENARIES_SEASON_REWARDS,
			m_info = new MercenariesSeasonRewardsDialog.Info
			{
				m_noticeId = rewardNotice.NoticeID,
				m_rewards = Network.ConvertRewardChest(rewardNotice.Chest).Rewards,
				m_rewardAssetId = rewardNotice.RewardAssetId,
				m_doneCallback = doneCallback
			}
		};
		AddToQueue(request);
	}

	public void ShowMercenariesZoneUnlockDialog(int zoneId, Action onCompleteCallback)
	{
		DialogRequest request = new DialogRequest
		{
			m_type = DialogType.MERCENARIES_ZONE_UNLOCK,
			m_info = new MercenariesZoneUnlockDialog.Info
			{
				m_zoneId = zoneId,
				m_onCompleteCallback = onCompleteCallback
			}
		};
		AddToQueue(request);
	}
}
