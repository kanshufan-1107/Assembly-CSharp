using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusUtil;

namespace Hearthstone.Progression;

public class SpecialEventManager : IService
{
	public delegate void EventChangedHandler(bool eventEnded);

	private SpecialEventDbfRecord m_currentSpecialEvent;

	private SpecialEventDataModel m_expiredSpecialEventDataModel;

	private bool m_hasReceivedInitialClientState;

	private bool m_hasReceivedEventTimings;

	private bool m_callbackScheduled;

	public event EventChangedHandler OnCurrentEventChanged;

	public static SpecialEventManager Get()
	{
		return ServiceManager.Get<SpecialEventManager>();
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		serviceLocator.Get<Network>().RegisterNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
		serviceLocator.Get<EventTimingManager>().OnReceivedEventTimingsFromServer += OnReceivedEventTimingsFromServer;
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[6]
		{
			typeof(EventTimingManager),
			typeof(Network),
			typeof(QuestManager),
			typeof(RewardTrackManager),
			typeof(SceneMgr),
			typeof(ReturningPlayerMgr)
		};
	}

	public void Shutdown()
	{
		if (ServiceManager.TryGet<Network>(out var net))
		{
			net.RemoveNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
		}
	}

	private void WillReset()
	{
		if (m_callbackScheduled)
		{
			Processor.CancelScheduledCallback(HandleEventChanged);
		}
		m_currentSpecialEvent = null;
		m_expiredSpecialEventDataModel = null;
		m_hasReceivedInitialClientState = false;
		m_hasReceivedEventTimings = false;
		m_callbackScheduled = false;
	}

	public SpecialEventDbfRecord GetUpcomingSpecialEvent(TimeSpan upcomingTimeSpan)
	{
		if (!GameUtils.HasCompletedApprentice())
		{
			return null;
		}
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		return GameDbf.SpecialEvent.GetRecord(delegate(SpecialEventDbfRecord asset)
		{
			EventTimingType eventTiming = asset.EventTiming;
			TimeSpan timeUntilEventStart = eventTimingManager.GetTimeUntilEventStart(eventTiming);
			return !(timeUntilEventStart <= TimeSpan.Zero) && timeUntilEventStart < upcomingTimeSpan;
		});
	}

	public SpecialEventDbfRecord GetCurrentSpecialEvent(bool forceGetRecords = false)
	{
		if (!GameUtils.HasCompletedApprentice())
		{
			return null;
		}
		if (m_currentSpecialEvent != null && !forceGetRecords)
		{
			return m_currentSpecialEvent;
		}
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		m_currentSpecialEvent = GameDbf.SpecialEvent.GetRecord(delegate(SpecialEventDbfRecord asset)
		{
			EventTimingType eventTiming = asset.EventTiming;
			return eventTimingManager.IsEventActive(eventTiming);
		});
		return m_currentSpecialEvent;
	}

	public SpecialEventDataModel GetEventDataModelForCurrentEvent()
	{
		SpecialEventDbfRecord currentSpecialEvent = GetCurrentSpecialEvent();
		if (currentSpecialEvent == null)
		{
			return null;
		}
		return GetEventDataModelFromSpecialEvent(currentSpecialEvent);
	}

	public SpecialEventDataModel GetEventDataModelFromSpecialEvent(SpecialEventDbfRecord specialEventRecord)
	{
		if (specialEventRecord == null)
		{
			return null;
		}
		SpecialEventDataModel eventDataModel = new SpecialEventDataModel();
		eventDataModel.ID = specialEventRecord.ID;
		eventDataModel.SpecialEventType = specialEventRecord.SpecialEventType;
		eventDataModel.Name = specialEventRecord.DisplayName?.GetString() ?? string.Empty;
		eventDataModel.ShortDescription = specialEventRecord.ShortDescription?.GetString() ?? string.Empty;
		eventDataModel.LongDescription = specialEventRecord.LongDescription?.GetString() ?? string.Empty;
		eventDataModel.ChooseTrackPrompt = specialEventRecord.ChooseATrackPrompt?.GetString() ?? string.Empty;
		eventDataModel.ActiveTrackId = RewardTrackManager.Get().GetCurrentEventRewardTrack()?.RewardTrackId ?? 0;
		eventDataModel.RewardTracks = new DataModelList<int>();
		foreach (EventRewardTrackDbfRecord eventTrack in specialEventRecord.RewardTracks)
		{
			eventDataModel.RewardTracks.Add(eventTrack.EventRewardTrackId);
			if (eventDataModel.ActiveTrackId == eventTrack.EventRewardTrackId)
			{
				eventDataModel.ShortConclusion = eventTrack.ShortConclusion?.GetString() ?? string.Empty;
				eventDataModel.LongConclusion = eventTrack.LongConclusion?.GetString() ?? string.Empty;
			}
		}
		return eventDataModel;
	}

	public bool IsCurrentEventEndingSoon()
	{
		SpecialEventDbfRecord currentSpecialEvent = GetCurrentSpecialEvent();
		if (currentSpecialEvent == null)
		{
			return false;
		}
		TimeSpan timeLeftForEvent = EventTimingManager.Get().GetTimeLeftForEvent(currentSpecialEvent.EventTiming);
		TimeSpan endingSoonThreshold = new TimeSpan(0, 48, 0, 0);
		return timeLeftForEvent < endingSoonThreshold;
	}

	public void UpdateJournalMetaWithSpecialEvent(JournalMetaDataModel journalMeta)
	{
		if (journalMeta == null)
		{
			return;
		}
		RewardTrackManager rewardTrackManager = RewardTrackManager.Get();
		GameSaveDataManager gsdManager = GameSaveDataManager.Get();
		SpecialEventDbfRecord currentSpecialEvent = GetCurrentSpecialEvent();
		bool isChooseOneEvent = false;
		if (currentSpecialEvent != null)
		{
			isChooseOneEvent = currentSpecialEvent.RewardTracks.Count > 1;
		}
		RewardTrack eventRewardTrack = rewardTrackManager.GetCurrentEventRewardTrack();
		journalMeta.EventActive = (eventRewardTrack != null || isChooseOneEvent) && currentSpecialEvent != null;
		journalMeta.EventIsEndingSoon = false;
		SpecialEventDbfRecord specialEventToShow = currentSpecialEvent;
		if (specialEventToShow == null)
		{
			TimeSpan timeSpanToCheck = new TimeSpan(7, 0, 0, 0);
			specialEventToShow = GetUpcomingSpecialEvent(timeSpanToCheck);
		}
		journalMeta.EventTabActive = specialEventToShow != null;
		if (!journalMeta.EventTabActive)
		{
			journalMeta.EventIsNew = false;
			return;
		}
		bool num = journalMeta.EventTabActive && !journalMeta.EventActive && specialEventToShow == currentSpecialEvent;
		journalMeta.EventLocked = false;
		journalMeta.EventLockedReason = string.Empty;
		if (num)
		{
			journalMeta.EventLocked = ShouldSpecialEventBeLocked(out var reason);
			journalMeta.EventLockedReason = reason;
			if (journalMeta.EventLocked && string.IsNullOrEmpty(journalMeta.EventLockedReason))
			{
				Log.All.PrintWarning("[SpecialEventManager] Event considered locked but no reason was found.");
			}
		}
		bool eventTrackNotChosen = eventRewardTrack == null && journalMeta.EventActive;
		bool eventTrackHasUnclaimed = eventRewardTrack != null && eventRewardTrack.TrackDataModel.Unclaimed > 0;
		journalMeta.EventHasUnclaimed = eventTrackNotChosen || eventTrackHasUnclaimed;
		if (!gsdManager.GetSubkeyValue(GameSaveKeyId.PROGRESSION, GameSaveKeySubkeyId.PROGRESSION_EVENT_TAB_EVENT_TYPE_LAST_SEEN, out long eventTypeLastSeen) || (SpecialEvent.SpecialEventType)eventTypeLastSeen != specialEventToShow.SpecialEventType)
		{
			journalMeta.EventIsNew = true;
		}
		if (journalMeta.EventActive)
		{
			if (!gsdManager.GetSubkeyValue(GameSaveKeyId.PROGRESSION, GameSaveKeySubkeyId.PROGRESSION_EVENT_TAB_ACTIVE_EVENT_LAST_SEEN, out long activeEventIdLastSeen) || activeEventIdLastSeen != currentSpecialEvent.ID)
			{
				journalMeta.EventIsNew = true;
			}
			long endingSoonEventIdLastSeen;
			bool subkeyValue = gsdManager.GetSubkeyValue(GameSaveKeyId.PROGRESSION, GameSaveKeySubkeyId.PROGRESSION_EVENT_TAB_ENDING_SOON_EVENT_LAST_SEEN, out endingSoonEventIdLastSeen);
			bool isEndingSoon = IsCurrentEventEndingSoon();
			bool hasSeenEndingSoonWarning = subkeyValue && endingSoonEventIdLastSeen == currentSpecialEvent.ID;
			if (isEndingSoon && !hasSeenEndingSoonWarning)
			{
				journalMeta.EventIsEndingSoon = true;
			}
		}
		if (!journalMeta.EventIsNew && !journalMeta.EventCompleted)
		{
			journalMeta.EventCompleted = GameUtils.IsGSDFlagSet(GameSaveKeyId.PROGRESSION, GameSaveKeySubkeyId.PROGRESSION_EVENT_TAB_HAS_ACKNOWLEDGED_COMPLETION);
		}
	}

	public bool ShowEventEndedPopup(Action callback)
	{
		if (m_expiredSpecialEventDataModel == null)
		{
			return false;
		}
		CreateEventEndWidget(m_expiredSpecialEventDataModel, callback);
		m_expiredSpecialEventDataModel = null;
		return true;
	}

	private void OnInitialClientState()
	{
		m_hasReceivedInitialClientState = true;
		ScheduleCheckForEventChange();
	}

	private void OnReceivedEventTimingsFromServer()
	{
		m_hasReceivedEventTimings = true;
		ScheduleCheckForEventChange();
	}

	private TimeSpan? GetTimeUntilNextEventChange()
	{
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		SpecialEventDbfRecord currentSpecialEvent = GetCurrentSpecialEvent();
		if (currentSpecialEvent != null)
		{
			return eventTimingManager.GetTimeLeftForEvent(currentSpecialEvent.EventTiming);
		}
		TimeSpan timeSpanToCheckForNextEvent = new TimeSpan(2, 0, 0, 0);
		SpecialEventDbfRecord upcomingSpecialEvent = GetUpcomingSpecialEvent(timeSpanToCheckForNextEvent);
		if (upcomingSpecialEvent != null)
		{
			return eventTimingManager.GetTimeUntilEventStart(upcomingSpecialEvent.EventTiming);
		}
		return null;
	}

	private void ScheduleCheckForEventChange()
	{
		if (m_hasReceivedInitialClientState && m_hasReceivedEventTimings && !m_callbackScheduled)
		{
			TimeSpan? timeToEventChange = GetTimeUntilNextEventChange();
			if (timeToEventChange.HasValue)
			{
				float secondsToWait = (float)timeToEventChange.Value.TotalSeconds + 2f;
				m_callbackScheduled = true;
				Processor.ScheduleCallback(secondsToWait, realTime: true, HandleEventChanged);
			}
		}
	}

	private void HandleEventChanged(object unused)
	{
		m_callbackScheduled = false;
		SpecialEventDataModel expiringSpecialEventDataModel = GetEventDataModelFromSpecialEvent(m_currentSpecialEvent);
		bool eventWasActive = expiringSpecialEventDataModel != null;
		bool eventIsActive = GetCurrentSpecialEvent(forceGetRecords: true) != null;
		if (eventWasActive == eventIsActive)
		{
			ScheduleCheckForEventChange();
			return;
		}
		bool eventEnded = eventWasActive && !eventIsActive;
		if (eventEnded)
		{
			m_expiredSpecialEventDataModel = expiringSpecialEventDataModel;
		}
		this.OnCurrentEventChanged?.Invoke(eventEnded);
	}

	private bool ShouldSpecialEventBeLocked(out string reason)
	{
		reason = string.Empty;
		bool shouldBeLocked = false;
		ReturningPlayerMgr returningPlayerMgr = ReturningPlayerMgr.Get();
		if (returningPlayerMgr == null || !returningPlayerMgr.IsCNRPEActive)
		{
			return false;
		}
		if (!GameUtils.IsGSDFlagSet(GameSaveKeyId.RETURNING_PLAYER_EXPERIENCE, GameSaveKeySubkeyId.RETURNING_PLAYER_INTRO_QUESTS_COMPLETE))
		{
			QuestDbfRecord quest = GameDbf.Quest.GetRecord(973);
			if (quest == null)
			{
				return false;
			}
			shouldBeLocked = true;
			string questName = quest.Name?.GetString();
			reason = ((!string.IsNullOrEmpty(questName)) ? GameStrings.Format("GLUE_PROGRESSION_EVENT_TAB_LOCK_DESCRIPTION", questName) : string.Empty);
		}
		return shouldBeLocked;
	}

	private void CreateEventEndWidget(SpecialEventDataModel SpecialEventDataModel, Action callback)
	{
		Widget widget = WidgetInstance.Create(EventEndedPopup.EVENT_ENDED_POPUP_PREFAB);
		widget.RegisterReadyListener(delegate
		{
			EventEndedPopup componentInChildren = widget.GetComponentInChildren<EventEndedPopup>();
			componentInChildren.Initialize(callback, SpecialEventDataModel);
			componentInChildren.Show();
		});
	}
}
