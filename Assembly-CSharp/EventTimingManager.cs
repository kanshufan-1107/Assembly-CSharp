using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using PegasusUtil;

public class EventTimingManager : IService
{
	public delegate void OnReceivedEventTimingsFromServerDelegate();

	private class EventTiming
	{
		[CompilerGenerated]
		private EventTimingType _003CType_003Ek__BackingField;

		public string Name { get; private set; }

		public long Id { get; private set; }

		private EventTimingType Type
		{
			[CompilerGenerated]
			set
			{
				_003CType_003Ek__BackingField = value;
			}
		}

		public DateTime? StartTimeUtc { get; private set; }

		public DateTime? EndTimeUtc { get; private set; }

		public EventTiming(string name, long id, EventTimingType eventType, DateTime? startTimeUtc, DateTime? endTimeUtc)
		{
			Id = id;
			Name = name;
			Type = eventType;
			StartTimeUtc = startTimeUtc;
			EndTimeUtc = endTimeUtc;
			if (StartTimeUtc.HasValue && StartTimeUtc.Value.Kind != DateTimeKind.Utc)
			{
				StartTimeUtc = StartTimeUtc.Value.ToUniversalTime();
			}
			if (EndTimeUtc.HasValue && EndTimeUtc.Value.Kind != DateTimeKind.Utc)
			{
				EndTimeUtc = EndTimeUtc.Value.ToUniversalTime();
			}
		}

		public bool HasStarted(DateTime utcTimestamp)
		{
			if (!StartTimeUtc.HasValue)
			{
				return true;
			}
			if (utcTimestamp.Kind != DateTimeKind.Utc)
			{
				utcTimestamp = utcTimestamp.ToUniversalTime();
			}
			return utcTimestamp >= StartTimeUtc.Value;
		}

		public bool HasEnded(DateTime utcTimestamp)
		{
			if (!EndTimeUtc.HasValue)
			{
				return false;
			}
			if (utcTimestamp.Kind != DateTimeKind.Utc)
			{
				utcTimestamp = utcTimestamp.ToUniversalTime();
			}
			return utcTimestamp > EndTimeUtc.Value;
		}

		public bool IsActiveNow(DateTime utcTimestamp)
		{
			if (StartTimeUtc.HasValue && EndTimeUtc.HasValue && EndTimeUtc.Value < StartTimeUtc.Value)
			{
				return false;
			}
			if (utcTimestamp.Kind != DateTimeKind.Utc)
			{
				utcTimestamp = utcTimestamp.ToUniversalTime();
			}
			if (HasStarted(utcTimestamp))
			{
				return !HasEnded(utcTimestamp);
			}
			return false;
		}

		public bool IsStartTimeInTheFuture(DateTime utcTimestamp)
		{
			if (utcTimestamp.Kind != DateTimeKind.Utc)
			{
				utcTimestamp = utcTimestamp.ToUniversalTime();
			}
			if (StartTimeUtc.HasValue)
			{
				return StartTimeUtc.Value > utcTimestamp;
			}
			return false;
		}
	}

	private class EventTimingTypeComparer : IEqualityComparer<EventTimingType>
	{
		public bool Equals(EventTimingType x, EventTimingType y)
		{
			return x == y;
		}

		public int GetHashCode(EventTimingType obj)
		{
			return (int)obj;
		}
	}

	public delegate void EventAddedCallback(object userData);

	private class EventAddedListener : EventListener<EventAddedCallback>
	{
		public void Fire()
		{
			m_callback(m_userData);
		}
	}

	public const long EVENT_TIMING_ID_INVALID = -1L;

	private const long EVENT_TIMING_ID_NEVER = 320L;

	private const long EVENT_TIMING_ID_ALWAYS = 650L;

	private Dictionary<EventTimingType, List<EventAddedListener>> m_allEventAddedListeners = new Dictionary<EventTimingType, List<EventAddedListener>>();

	private int m_nextEventTimingId = 10000000;

	private Dictionary<string, EventTimingType> m_eventTimingIdByEventName = new Dictionary<string, EventTimingType>();

	private Dictionary<EventTimingType, EventTiming> m_eventTimings = new Dictionary<EventTimingType, EventTiming>(new EventTimingTypeComparer());

	private HashSet<EventTimingType> m_forcedInactiveEvents;

	private HashSet<EventTimingType> m_forcedActiveEvents;

	public HashSet<EventTimingType> AllKnownEvents => new HashSet<EventTimingType>(m_eventTimings.Keys);

	public bool HasReceivedEventTimingsFromServer { get; private set; }

	public long DevTimeOffsetSeconds { get; private set; }

	public EventTimingVisualMgr Visuals { get; private set; }

	public event OnReceivedEventTimingsFromServerDelegate OnReceivedEventTimingsFromServer = delegate
	{
	};

	public bool AddEventAddedListener(EventAddedCallback callback, EventTimingType eventType, object userData = null)
	{
		if (callback == null)
		{
			return false;
		}
		EventAddedListener listener = new EventAddedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_allEventAddedListeners.ContainsKey(eventType))
		{
			m_allEventAddedListeners[eventType] = new List<EventAddedListener>();
		}
		m_allEventAddedListeners[eventType].Add(listener);
		return true;
	}

	private void FireEventAddedEvents(EventTimingType eventType)
	{
		if (!m_allEventAddedListeners.TryGetValue(eventType, out var listeners))
		{
			return;
		}
		foreach (EventAddedListener item in listeners)
		{
			item.Fire();
		}
	}

	private EventTimingType RegisterEventTimingName(string eventName)
	{
		EventTimingType eventType = GetEventType(eventName);
		if (eventType != EventTimingType.UNKNOWN)
		{
			return eventType;
		}
		eventType = (EventTimingType)(++m_nextEventTimingId);
		m_eventTimingIdByEventName[eventName] = eventType;
		return eventType;
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		InstantiatePrefab loadVisuals = new InstantiatePrefab("EventTimingVisualMgr.prefab:9e2d0e3e4eb236f418ecaf0fa12732e4");
		yield return loadVisuals;
		Visuals = loadVisuals.InstantiatedPrefab.GetComponent<EventTimingVisualMgr>();
		HearthstoneApplication.Get().WillReset += WillReset;
		InitializeHardcodedEvents();
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(IAssetLoader) };
	}

	public void Shutdown()
	{
	}

	private void WillReset()
	{
		m_eventTimingIdByEventName.Clear();
		m_eventTimings.Clear();
		if (m_forcedInactiveEvents != null)
		{
			m_forcedInactiveEvents.Clear();
		}
		if (m_forcedActiveEvents != null)
		{
			m_forcedActiveEvents.Clear();
		}
		HasReceivedEventTimingsFromServer = false;
		m_allEventAddedListeners.Clear();
		InitializeEventNames();
		InitializeHardcodedEvents();
	}

	public static EventTimingManager Get()
	{
		return ServiceManager.Get<EventTimingManager>();
	}

	private void InitializeEventNames()
	{
		EventTimingMap eventTimingMap = DbfShared.GetEventMap();
		for (int i = 0; i < eventTimingMap.Keys.Count; i++)
		{
			if (!m_eventTimingIdByEventName.ContainsKey(eventTimingMap.Keys[i]))
			{
				m_eventTimingIdByEventName.Add(eventTimingMap.Keys[i], (EventTimingType)eventTimingMap.Values[i]);
			}
		}
		m_nextEventTimingId = eventTimingMap.CurrentId;
	}

	private void InitializeHardcodedEvents()
	{
		DateTime utcNow = DateTime.UtcNow;
		m_eventTimings[EventTimingType.SPECIAL_EVENT_ALWAYS] = new EventTiming(EnumUtils.GetString(EventTimingType.SPECIAL_EVENT_ALWAYS), 650L, EventTimingType.SPECIAL_EVENT_ALWAYS, null, null);
		m_eventTimings[EventTimingType.SPECIAL_EVENT_NEVER] = new EventTiming(EnumUtils.GetString(EventTimingType.SPECIAL_EVENT_NEVER), 320L, EventTimingType.SPECIAL_EVENT_NEVER, utcNow.AddSeconds(-1.0), utcNow.AddSeconds(-2.0));
	}

	public void InitEventTimingsFromServer(long devTimeOffsetSeconds, IList<SpecialEventTiming> serverEventTimingList)
	{
		m_forcedActiveEvents = (m_forcedInactiveEvents = null);
		DevTimeOffsetSeconds = devTimeOffsetSeconds;
		m_eventTimingIdByEventName.Clear();
		m_eventTimings.Clear();
		InitializeEventNames();
		InitializeHardcodedEvents();
		List<EventTimingType> addedEventTypes = new List<EventTimingType>();
		DateTime utcNow = DateTime.UtcNow;
		for (int i = 0; i < serverEventTimingList.Count; i++)
		{
			SpecialEventTiming eventTimingRecord = serverEventTimingList[i];
			EventTimingType eventType = RegisterEventTimingName(eventTimingRecord.EventName);
			DateTime? startDate = null;
			if (eventTimingRecord.HasSecondsToStart)
			{
				startDate = utcNow.AddSeconds(eventTimingRecord.SecondsToStart);
			}
			DateTime? endDate = null;
			if (eventTimingRecord.HasSecondsToEnd)
			{
				endDate = utcNow.AddSeconds(eventTimingRecord.SecondsToEnd);
			}
			m_eventTimings[eventType] = new EventTiming(eventTimingRecord.EventName, eventTimingRecord.EventId, eventType, startDate, endDate);
			addedEventTypes.Add(eventType);
		}
		HasReceivedEventTimingsFromServer = true;
		this.OnReceivedEventTimingsFromServer?.Invoke();
		foreach (EventTimingType eventType2 in addedEventTypes)
		{
			FireEventAddedEvents(eventType2);
		}
	}

	public DateTime? GetEventStartTimeUtc(EventTimingType eventType)
	{
		if (m_eventTimings.TryGetValue(eventType, out var eventTiming) && eventTiming != null)
		{
			return eventTiming.StartTimeUtc;
		}
		return null;
	}

	public DateTime? GetEventEndTimeUtc(EventTimingType eventType)
	{
		if (m_eventTimings.TryGetValue(eventType, out var eventTiming) && eventTiming != null)
		{
			return eventTiming.EndTimeUtc;
		}
		return null;
	}

	public TimeSpan GetTimeUntilEventStart(EventTimingType eventType)
	{
		DateTime? startTime = GetEventStartTimeUtc(eventType);
		if (!startTime.HasValue)
		{
			return TimeSpan.Zero;
		}
		return startTime.Value - DateTime.UtcNow;
	}

	public TimeSpan GetTimeLeftForEvent(EventTimingType eventType)
	{
		m_eventTimings.TryGetValue(eventType, out var eventTiming);
		TimeSpan timeLeft = default(TimeSpan);
		if (eventTiming != null && eventTiming.EndTimeUtc.HasValue)
		{
			DateTime currentDate = DateTime.UtcNow;
			return eventTiming.EndTimeUtc.Value - currentDate;
		}
		return timeLeft;
	}

	public bool GetEventRangeUtc(EventTimingType eventType, out DateTime? start, out DateTime? end)
	{
		if (m_eventTimings.TryGetValue(eventType, out var eventTiming) && eventTiming != null)
		{
			start = eventTiming.StartTimeUtc;
			end = eventTiming.EndTimeUtc;
			return true;
		}
		start = null;
		end = null;
		return false;
	}

	public bool HasEventStarted(EventTimingType eventType)
	{
		if (IsEventForcedInactive(eventType))
		{
			return false;
		}
		if (IsEventForcedActive(eventType))
		{
			return true;
		}
		if (!m_eventTimings.TryGetValue(eventType, out var eventTiming))
		{
			return false;
		}
		return eventTiming.HasStarted(DateTime.UtcNow);
	}

	public bool IsStartTimeInTheFuture(EventTimingType eventType)
	{
		if (eventType == EventTimingType.UNKNOWN)
		{
			return false;
		}
		if (IsEventForcedInactive(eventType))
		{
			return false;
		}
		if (IsEventForcedActive(eventType))
		{
			return false;
		}
		if (!m_eventTimings.TryGetValue(eventType, out var eventTiming))
		{
			return false;
		}
		return eventTiming.IsStartTimeInTheFuture(DateTime.UtcNow);
	}

	public bool HasEventEnded(EventTimingType eventType)
	{
		if (IsEventForcedInactive(eventType))
		{
			return false;
		}
		if (IsEventForcedActive(eventType))
		{
			return false;
		}
		if (!m_eventTimings.TryGetValue(eventType, out var eventTiming))
		{
			return false;
		}
		return eventTiming.HasEnded(DateTime.UtcNow);
	}

	public bool IsEventActive(EventTimingType eventType)
	{
		return IsEventActive(eventType, DateTime.UtcNow);
	}

	public bool IsEventActive(EventTimingType eventType, DateTime utcTimestamp)
	{
		return IsEventActive_Impl(eventType, utcTimestamp);
	}

	public bool IsEventActive(string eventName)
	{
		return IsEventActive(eventName, DateTime.UtcNow);
	}

	public bool IsEventActive(string eventName, DateTime utcTimestamp)
	{
		if (string.IsNullOrEmpty(eventName))
		{
			return false;
		}
		EventTimingType eventTimingType = GetEventType(eventName);
		if (eventTimingType == EventTimingType.UNKNOWN)
		{
			return false;
		}
		return IsEventActive(eventTimingType, utcTimestamp);
	}

	public EventTimingType GetEventType(string eventName)
	{
		if (eventName == null)
		{
			return EventTimingType.UNKNOWN;
		}
		if (m_eventTimingIdByEventName.TryGetValue(eventName, out var eventType))
		{
			return eventType;
		}
		return EventTimingType.UNKNOWN;
	}

	public string GetName(EventTimingType eventType)
	{
		if (m_eventTimings.TryGetValue(eventType, out var eventTiming) && eventTiming != null)
		{
			return eventTiming.Name;
		}
		return EnumUtils.GetString(eventType);
	}

	private bool IsEventActive_Impl(EventTimingType eventType, DateTime localTimestamp)
	{
		switch (eventType)
		{
		case EventTimingType.SPECIAL_EVENT_ALWAYS:
			return true;
		case EventTimingType.SPECIAL_EVENT_NEVER:
			return false;
		default:
		{
			if (IsEventForcedInactive(eventType))
			{
				return false;
			}
			if (IsEventForcedActive(eventType))
			{
				return true;
			}
			if (!m_eventTimings.TryGetValue(eventType, out var eventTiming))
			{
				return false;
			}
			return eventTiming.IsActiveNow(localTimestamp);
		}
		}
	}

	public bool IsEventForcedInactive(EventTimingType eventType)
	{
		return IsEventTimingForced(eventType, "Events.ForceInactive", ref m_forcedInactiveEvents);
	}

	public bool IsEventForcedActive(EventTimingType eventType)
	{
		return IsEventTimingForced(eventType, "Events.ForceActive", ref m_forcedActiveEvents);
	}

	public EventTimingType GetActiveEventType()
	{
		if (IsEventActive(EventTimingType.GVG_PROMOTION))
		{
			return EventTimingType.GVG_PROMOTION;
		}
		if (IsEventActive(EventTimingType.SPECIAL_EVENT_PRE_TAVERN_BRAWL))
		{
			return EventTimingType.SPECIAL_EVENT_PRE_TAVERN_BRAWL;
		}
		return EventTimingType.IGNORE;
	}

	private bool IsEventTimingForced(EventTimingType eventType, string clientConfigVarKey, ref HashSet<EventTimingType> forcedEventSet)
	{
		if (!HearthstoneApplication.IsInternal())
		{
			return false;
		}
		if (forcedEventSet == null)
		{
			forcedEventSet = new HashSet<EventTimingType>(new EventTimingTypeComparer());
			string activeEventsOverride = Vars.Key(clientConfigVarKey).GetStr(null);
			if (string.IsNullOrEmpty(activeEventsOverride))
			{
				return false;
			}
			string[] eventStrings = activeEventsOverride.Split(new char[3] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < eventStrings.Length; i++)
			{
				EventTimingType parsedEvt = GetEventType(eventStrings[i]);
				if (parsedEvt != EventTimingType.UNKNOWN)
				{
					forcedEventSet.Add(parsedEvt);
				}
			}
		}
		return forcedEventSet.Contains(eventType);
	}

	public long GetEventIdFromEventName(EventTimingType eventName)
	{
		if (m_eventTimings.TryGetValue(eventName, out var value))
		{
			return value.Id;
		}
		return -1L;
	}

	public long GetEventIdFromEventName(string eventName)
	{
		foreach (KeyValuePair<EventTimingType, EventTiming> eventPair in m_eventTimings)
		{
			if (eventPair.Value.Name.Equals(eventName, StringComparison.OrdinalIgnoreCase))
			{
				return eventPair.Value.Id;
			}
		}
		return -1L;
	}
}
