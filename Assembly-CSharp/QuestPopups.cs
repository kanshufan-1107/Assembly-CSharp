using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Hearthstone.Progression;
using UnityEngine;

public class QuestPopups : IDisposable
{
	private Action OnPopupShown;

	private Action OnPopupClosed;

	private Action<bool> SetIsShowing;

	private Dictionary<EventTimingType, List<QuestChangeDbfRecord>> m_loginQuestChangesByEventTiming = new Dictionary<EventTimingType, List<QuestChangeDbfRecord>>();

	private Dictionary<EventTimingType, List<QuestChangeDbfRecord>> m_journalQuestChangesByEventTiming = new Dictionary<EventTimingType, List<QuestChangeDbfRecord>>();

	private bool m_areQuestChangesInitialized;

	public Reward.DelOnRewardLoaded OnRewardLoadedCallback { get; private set; }

	private event Action<int> OnQuestCompletedShown = delegate
	{
	};

	public QuestPopups(Action<bool> setIsShowing, Action onPopupShown, Action onPopupClosed, Reward.DelOnRewardLoaded onRewardLoadedCallback)
	{
		SetIsShowing = setIsShowing;
		OnPopupShown = onPopupShown;
		OnPopupClosed = onPopupClosed;
		OnRewardLoadedCallback = onRewardLoadedCallback;
	}

	public void Dispose()
	{
	}

	public void RegisterCompletedQuestShownListener(Action<int> callback)
	{
		if (callback != null)
		{
			OnQuestCompletedShown -= callback;
			OnQuestCompletedShown += callback;
		}
	}

	public void RemoveCompletedQuestShownListener(Action<int> callback)
	{
		if (callback != null)
		{
			OnQuestCompletedShown -= callback;
		}
	}

	public bool ShowNextCompletedQuest(List<Achievement> completedAchieves, bool suppressRewardPopupsForNewPlayer)
	{
		if (completedAchieves.Count == 0)
		{
			return false;
		}
		if (suppressRewardPopupsForNewPlayer)
		{
			return false;
		}
		if (QuestToast.IsQuestActive())
		{
			QuestToast.GetCurrentToast().CloseQuestToast();
		}
		Achievement completedAchieve = completedAchieves[0];
		SetIsShowing?.Invoke(obj: true);
		OnPopupShown?.Invoke();
		UserAttentionBlocker blocker = completedAchieve.GetUserAttentionBlocker();
		if (ReturningPlayerMgr.Get() != null && ReturningPlayerMgr.Get().SuppressOldPopups && completedAchieve.ShowToReturningPlayer == Achieve.ShowToReturningPlayer.SUPPRESSED)
		{
			completedAchieves.Remove(completedAchieve);
			completedAchieve.AckCurrentProgressAndRewardNotices();
			SetIsShowing?.Invoke(obj: false);
			return true;
		}
		if (AssetLoader.Get() != null && !string.IsNullOrEmpty(completedAchieve.CustomVisualWidget))
		{
			AssetLoader.Get().InstantiatePrefab(completedAchieve.CustomVisualWidget, ONAssetLoad);
		}
		else if (!completedAchieve.UseGenericRewardVisual)
		{
			completedAchieves.Remove(completedAchieve);
			QuestToast.ShowQuestToast(blocker, delegate
			{
				SetIsShowing?.Invoke(obj: false);
			}, updateCacheValues: false, completedAchieve);
			this.OnQuestCompletedShown(completedAchieve.ID);
		}
		else
		{
			completedAchieves.Remove(completedAchieve);
			completedAchieve.AckCurrentProgressAndRewardNotices();
			completedAchieve.Rewards[0].LoadRewardObject(OnRewardLoadedCallback);
		}
		return true;
		void ONAssetLoad(AssetReference assetRef, GameObject go, object callbackData)
		{
			OverlayUI.Get().AddGameObject(go);
			CustomVisualReward component = go.GetComponent<CustomVisualReward>();
			component.AssociatedAchievement = completedAchieve;
			component.SetCompleteCallback(delegate
			{
				SetIsShowing?.Invoke(obj: false);
				completedAchieves.Remove(completedAchieve);
				completedAchieve.AckCurrentProgressAndRewardNotices();
			});
		}
	}

	public bool ShowNextQuestNotification()
	{
		if (JournalPopup.s_isShowing)
		{
			return false;
		}
		QuestManager questManager = QuestManager.Get();
		if (questManager == null || !questManager.HasQuestNotificationToShow() || !questManager.ShowQuestNotification(OnPopupClosed))
		{
			return false;
		}
		OnPopupShown?.Invoke();
		SetIsShowing?.Invoke(obj: true);
		return true;
	}

	public void ShowQuestProgressToasts(List<Achievement> progressedAchieves)
	{
		if (UserAttentionManager.CanShowAttentionGrabber("ShowQuestProgressToasts") && (SceneMgr.Get().GetMode() != SceneMgr.Mode.ADVENTURE || !UniversalInputManager.UsePhoneUI))
		{
			if (QuestManager.Get() != null && QuestToastManager.Get() != null)
			{
				QuestToastManager.Get()?.ShowNextQuestProgress();
			}
			else if (progressedAchieves.Count > 0)
			{
				GameToastMgr.Get().ShowQuestProgressToasts(progressedAchieves);
				progressedAchieves.Clear();
			}
		}
	}

	public bool ShowAdjustedQuestsCompletedPopup()
	{
		if (!m_areQuestChangesInitialized)
		{
			InitializeQuestChangeEvents(ignoreSeen: true);
			m_areQuestChangesInitialized = true;
		}
		if (m_loginQuestChangesByEventTiming.Count > 0)
		{
			HashSet<int> questIds = new HashSet<int>();
			Dictionary<int, List<QuestChangeDbfRecord>> questChangesByQuestId = new Dictionary<int, List<QuestChangeDbfRecord>>();
			foreach (List<QuestChangeDbfRecord> value in m_loginQuestChangesByEventTiming.Values)
			{
				foreach (QuestChangeDbfRecord loginQuestChange in value)
				{
					questIds.Add(loginQuestChange.QuestId);
					if (!questChangesByQuestId.TryGetValue(loginQuestChange.QuestId, out var questChanges))
					{
						questChanges = new List<QuestChangeDbfRecord>();
						questChangesByQuestId[loginQuestChange.QuestId] = questChanges;
					}
					questChanges.Add(loginQuestChange);
				}
			}
			CompletedQuestsUpdatedPopup.Info info = new CompletedQuestsUpdatedPopup.Info
			{
				m_quests = questIds.ToList(),
				m_changes = questChangesByQuestId,
				m_callbackOnHide = delegate
				{
					MarkLoginQuestChangeEventAsSeen(m_loginQuestChangesByEventTiming.Keys.ToList());
					m_loginQuestChangesByEventTiming.Clear();
				}
			};
			return DialogManager.Get().ShowCompletedQuestsUpdatedListPopup(UserAttentionBlocker.NONE, info);
		}
		return false;
	}

	public bool ShouldShowAdjustedQuestsPopupOnJournal()
	{
		return m_journalQuestChangesByEventTiming.Count > 0;
	}

	public void OnAdjustedQuestsPopupShownOnJournal()
	{
		MarkJournalQuestChangeEventAsSeen(m_journalQuestChangesByEventTiming.Keys.ToList());
		m_journalQuestChangesByEventTiming.Clear();
	}

	public void MarkLoginQuestChangeEventAsSeen(List<EventTimingType> specialEventType)
	{
		MarkQuestChangeEventAsSeen(specialEventType, GameSaveKeySubkeyId.LIST_OF_SEEN_QUEST_CHANGES_ON_LOGIN);
	}

	public void MarkJournalQuestChangeEventAsSeen(List<EventTimingType> specialEventType)
	{
		MarkQuestChangeEventAsSeen(specialEventType, GameSaveKeySubkeyId.LIST_OF_SEEN_QUEST_CHANGES_ON_JOURNAL);
	}

	private void MarkQuestChangeEventAsSeen(List<EventTimingType> specialEventType, GameSaveKeySubkeyId gameSaveKeySubkeyId)
	{
		if (specialEventType.Count == 0)
		{
			return;
		}
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_OPTIONS, gameSaveKeySubkeyId, out List<long> seenQuestChangeEventIds);
		if (seenQuestChangeEventIds == null)
		{
			seenQuestChangeEventIds = new List<long>();
		}
		while (seenQuestChangeEventIds.Count + specialEventType.Count >= 100)
		{
			seenQuestChangeEventIds.RemoveAt(0);
		}
		seenQuestChangeEventIds.Capacity = seenQuestChangeEventIds.Count + specialEventType.Count;
		foreach (EventTimingType eventTiming in specialEventType)
		{
			long eventIdToAddToGSD = EventTimingManager.Get().GetEventIdFromEventName(eventTiming);
			seenQuestChangeEventIds.Add(eventIdToAddToGSD);
		}
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PLAYER_OPTIONS, gameSaveKeySubkeyId, seenQuestChangeEventIds.ToArray()));
	}

	private void InitializeQuestChangeEvents(bool ignoreSeen)
	{
		HashSet<long> seenLoginEventSet = new HashSet<long>();
		HashSet<long> seenJournalEventSet = new HashSet<long>();
		if (ignoreSeen)
		{
			seenLoginEventSet = GetSeenLoginEvents();
			seenJournalEventSet = GetSeenJournalEvents();
		}
		Dictionary<EventTimingType, List<QuestChangeDbfRecord>> allUnseenLoginChanges = new Dictionary<EventTimingType, List<QuestChangeDbfRecord>>();
		Dictionary<EventTimingType, List<QuestChangeDbfRecord>> allUnseenJournalChanges = new Dictionary<EventTimingType, List<QuestChangeDbfRecord>>();
		foreach (QuestChangeDbfRecord questChangeRecord in GameDbf.QuestChange.GetRecords())
		{
			long eventId = EventTimingManager.Get().GetEventIdFromEventName(questChangeRecord.Event);
			if (!EventTimingManager.Get().IsEventActive(questChangeRecord.Event))
			{
				continue;
			}
			bool addToLoginList = true;
			bool addToJournalList = true;
			if (ignoreSeen)
			{
				if (seenLoginEventSet.Contains(eventId))
				{
					addToLoginList = false;
				}
				if (seenJournalEventSet.Contains(eventId))
				{
					addToJournalList = false;
				}
			}
			if (addToLoginList)
			{
				AddQuestChangeRecordToDictionary(questChangeRecord, allUnseenLoginChanges);
			}
			if (addToJournalList)
			{
				AddQuestChangeRecordToDictionary(questChangeRecord, allUnseenJournalChanges);
			}
		}
		List<EventTimingType> loginChangesToMarkAsSeen = new List<EventTimingType>();
		List<EventTimingType> journalChangesToMarkAsSeen = new List<EventTimingType>();
		QuestManager questManager = QuestManager.Get();
		EventTimingType key;
		List<QuestChangeDbfRecord> value;
		foreach (KeyValuePair<EventTimingType, List<QuestChangeDbfRecord>> item in allUnseenLoginChanges)
		{
			item.Deconstruct(out key, out value);
			EventTimingType eventTimingType = key;
			foreach (QuestChangeDbfRecord questChangeRecord2 in value)
			{
				if (QuestManager.IsQuestComplete(questManager.GetPlayerQuestStateById(questChangeRecord2.QuestId)))
				{
					AddQuestChangeRecordToDictionary(questChangeRecord2, m_loginQuestChangesByEventTiming);
				}
			}
			if (!m_loginQuestChangesByEventTiming.ContainsKey(eventTimingType))
			{
				loginChangesToMarkAsSeen.Add(eventTimingType);
			}
		}
		foreach (KeyValuePair<EventTimingType, List<QuestChangeDbfRecord>> item2 in allUnseenJournalChanges)
		{
			item2.Deconstruct(out key, out value);
			EventTimingType eventTimingType2 = key;
			foreach (QuestChangeDbfRecord questChangeRecord3 in value)
			{
				if (QuestManager.IsQuestActive(questManager.GetPlayerQuestStateById(questChangeRecord3.QuestId)))
				{
					AddQuestChangeRecordToDictionary(questChangeRecord3, m_journalQuestChangesByEventTiming);
				}
			}
			if (!m_journalQuestChangesByEventTiming.ContainsKey(eventTimingType2))
			{
				journalChangesToMarkAsSeen.Add(eventTimingType2);
			}
		}
		MarkLoginQuestChangeEventAsSeen(loginChangesToMarkAsSeen);
		MarkJournalQuestChangeEventAsSeen(journalChangesToMarkAsSeen);
	}

	private static void AddQuestChangeRecordToDictionary(QuestChangeDbfRecord questChangeRecord, Dictionary<EventTimingType, List<QuestChangeDbfRecord>> dictionaryToAddTo)
	{
		if (dictionaryToAddTo.TryGetValue(questChangeRecord.Event, out var questsForEvent))
		{
			questsForEvent.Add(questChangeRecord);
			return;
		}
		questsForEvent = new List<QuestChangeDbfRecord>();
		questsForEvent.Add(questChangeRecord);
		dictionaryToAddTo.Add(questChangeRecord.Event, questsForEvent);
	}

	private HashSet<long> GetSeenLoginEvents()
	{
		return GetSeenEventsImpl(GameSaveKeySubkeyId.LIST_OF_SEEN_QUEST_CHANGES_ON_LOGIN);
	}

	private HashSet<long> GetSeenJournalEvents()
	{
		return GetSeenEventsImpl(GameSaveKeySubkeyId.LIST_OF_SEEN_QUEST_CHANGES_ON_JOURNAL);
	}

	private HashSet<long> GetSeenEventsImpl(GameSaveKeySubkeyId gameSaveKeySubkeyId)
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_OPTIONS, gameSaveKeySubkeyId, out List<long> seenQuestChangeEventIds);
		if (seenQuestChangeEventIds == null)
		{
			seenQuestChangeEventIds = new List<long>();
		}
		return new HashSet<long>(seenQuestChangeEventIds);
	}
}
