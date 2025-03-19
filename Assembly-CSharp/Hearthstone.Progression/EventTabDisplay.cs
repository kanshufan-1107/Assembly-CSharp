using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class EventTabDisplay : MonoBehaviour
{
	[SerializeField]
	private float m_timerUpdateSecondsDelay;

	[SerializeField]
	private ProgressBar m_xpProgressBar;

	[SerializeField]
	private UberText m_doorsTimerText;

	[SerializeField]
	private int m_doorsTimerThresholdHours;

	private Widget m_widget;

	private JournalMetaDataModel m_journalMeta;

	private Coroutine m_timerUpdateCoroutine;

	private SpecialEventDbfRecord m_specialEventRecord;

	private SpecialEventDataModel m_specialEventDataModel;

	private RewardTrack m_eventRewardTrack;

	private QuestListDataModel m_questList;

	private int m_maxRewardsToShow = 5;

	private Dictionary<int, float> m_barPositionXForRewardCount = new Dictionary<int, float>
	{
		{ 5, 0.192f },
		{ 4, 0.26f },
		{ 3, 0.4f }
	};

	private Dictionary<int, float> m_barScaleXForRewardCount = new Dictionary<int, float>
	{
		{ 5, 0.924462f },
		{ 4, 0.904392f },
		{ 3, 0.857981f }
	};

	private Dictionary<int, float> m_firstSegmentPercentForRewardCount = new Dictionary<int, float>
	{
		{ 5, 0.11838f },
		{ 4, 0.15582f },
		{ 3, 0.20747f }
	};

	private Dictionary<int, float> m_endcapPercentForRewardCount = new Dictionary<int, float>
	{
		{ 5, 0.02791f },
		{ 4, 0.03057f },
		{ 3, 0.03838f }
	};

	private const string CODE_EVENT_TYPE_LAST_SEEN = "CODE_EVENT_TYPE_LAST_SEEN";

	private const string CODE_TRIGGER_EVENT_COMPLETE = "CODE_TRIGGER_EVENT_COMPLETE";

	private const string CODE_PREVIEW_REWARD_TRACK = "CODE_PREVIEW_REWARD_TRACK";

	private const string CODE_REWARD_TRACK_CHOSEN = "CODE_REWARD_TRACK_CHOSEN";

	private const string CODE_REWARD_TRACK_UPDATED = "CODE_REWARD_TRACK_UPDATED";

	private const string HIDE_INFO_LETTER_INSTANT = "HIDE_INFO_LETTER_INSTANT";

	[Overridable]
	public int DebugMaxRewardsToShow
	{
		get
		{
			return m_maxRewardsToShow;
		}
		set
		{
			if (m_maxRewardsToShow != value)
			{
				UpdateRewardTrackNodes(value);
				UpdateProgressBar();
			}
			m_maxRewardsToShow = value;
		}
	}

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		CreateEventDataModels();
		m_widget.RegisterReadyListener(OnActivateOrReactivate);
		m_widget.RegisterActivatedListener(OnActivateOrReactivate);
		m_widget.RegisterDeactivatedListener(OnDeactivate);
		m_widget.RegisterEventListener(HandleEvent);
		RewardTrackManager rewardTrackManager = RewardTrackManager.Get();
		if (rewardTrackManager != null)
		{
			rewardTrackManager.OnRewardTracksReceived += OnRewardTracksReceived;
		}
		Network.Get()?.RegisterNetHandler(PlayerQuestStateUpdate.PacketID.ID, OnQuestStateUpdate);
	}

	private void OnActivateOrReactivate(object unused)
	{
		m_journalMeta = m_widget.GetDataModel<JournalMetaDataModel>();
		UpdateQuests();
		UpdateTimerCoroutines();
		UpdateProgressBar();
	}

	private void OnDeactivate(object unused)
	{
		StopAllCoroutines();
		m_widget.TriggerEvent("HIDE_INFO_LETTER_INSTANT");
	}

	private void HandleEvent(string eventName)
	{
		EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
		switch (eventName)
		{
		case "CODE_EVENT_TYPE_LAST_SEEN":
			UpdateEventSaveData();
			UpdateTimerCoroutines();
			break;
		case "CODE_PREVIEW_REWARD_TRACK":
			PreviewEventRewardTrack(Convert.ToInt32(eventDataModel.Payload));
			break;
		case "CODE_REWARD_TRACK_CHOSEN":
			SelectEventRewardTrack(Convert.ToInt32(eventDataModel.Payload));
			break;
		case "CODE_TRIGGER_EVENT_COMPLETE":
			HandleEventComplete();
			UpdateTimerCoroutines();
			break;
		case "CODE_REWARD_TRACK_UPDATED":
			UpdateProgressBar();
			break;
		}
	}

	private void OnRewardTracksReceived()
	{
		if ((m_journalMeta == null || !m_journalMeta.IsActivatingEventTrack) && (m_eventRewardTrack == null || m_eventRewardTrack.TrackDataModel.Level == 0))
		{
			CreateEventDataModels();
			UpdateQuests();
		}
	}

	private void OnQuestStateUpdate()
	{
		if (m_journalMeta != null && m_journalMeta.IsActivatingEventTrack)
		{
			CreateEventDataModels();
		}
		UpdateQuests();
	}

	private void CreateEventDataModels()
	{
		RewardTrackManager rewardTrackManager = RewardTrackManager.Get();
		SpecialEventManager specialEventManager = SpecialEventManager.Get();
		if (m_widget == null || rewardTrackManager == null)
		{
			return;
		}
		m_specialEventRecord = specialEventManager.GetCurrentSpecialEvent();
		if (m_specialEventRecord == null)
		{
			JournalPopup journalPopup = base.gameObject.GetComponentInParent<JournalPopup>();
			if (journalPopup != null)
			{
				TimeSpan eventComingSoonTimespan = journalPopup.GetEventComingSoonTimespan();
				m_specialEventRecord = specialEventManager.GetUpcomingSpecialEvent(eventComingSoonTimespan);
			}
		}
		if (m_specialEventRecord == null)
		{
			Debug.LogError("[Journal] Opening the Event tab without an event! This view will not behave as expected.");
			return;
		}
		m_specialEventDataModel = specialEventManager.GetEventDataModelFromSpecialEvent(m_specialEventRecord);
		m_widget.BindDataModel(m_specialEventDataModel);
		m_eventRewardTrack = rewardTrackManager.GetCurrentEventRewardTrack();
		if (m_eventRewardTrack != null && m_eventRewardTrack.TrackDataModel != null)
		{
			m_eventRewardTrack.TrackDataModel.TimeRemainingText = GetEventTimeRemainingString();
			m_widget.BindDataModel(m_eventRewardTrack.TrackDataModel);
			UpdateRewardTrackNodes(m_maxRewardsToShow);
		}
	}

	private void PreviewEventRewardTrack(int rewardTrackId)
	{
		RewardTrackDbfRecord previewTrackAsset = GameDbf.RewardTrack.GetRecord(rewardTrackId);
		if (previewTrackAsset != null)
		{
			RewardTrack tempRewardTrack = new RewardTrack(Global.RewardTrackType.EVENT);
			tempRewardTrack.TrackDataModel.RewardTrackId = rewardTrackId;
			tempRewardTrack.TrackDataModel.RewardTrackType = Global.RewardTrackType.EVENT;
			tempRewardTrack.TrackDataModel.Name = previewTrackAsset.Name;
			EventRewardTrackDbfRecord eventTrack = GameDbf.EventRewardTrack.GetRecord((EventRewardTrackDbfRecord track) => track.EventRewardTrackId == rewardTrackId);
			if (eventTrack != null)
			{
				tempRewardTrack.TrackDataModel.ChoiceConfirmationText = eventTrack.ChoiceConfirmationText;
				m_eventRewardTrack = tempRewardTrack;
				m_widget.BindDataModel(m_eventRewardTrack.TrackDataModel);
				UpdateRewardTrackNodes(m_maxRewardsToShow);
			}
		}
	}

	private void SelectEventRewardTrack(int rewardTrackId)
	{
		RewardTrackManager rewardTrackManager = RewardTrackManager.Get();
		if (rewardTrackManager == null)
		{
			Debug.LogError("[Journal] Attempting to activate a reward track without a valid RewardTrackManager.");
			return;
		}
		m_journalMeta.IsActivatingEventTrack = true;
		rewardTrackManager.SetActiveEventRewardTrack(rewardTrackId);
	}

	private void UpdateRewardTrackNodes(int maxRewardsToShow)
	{
		if (m_eventRewardTrack != null && m_eventRewardTrack.IsValid)
		{
			m_eventRewardTrack.SetEventRewardTrackNodePage(maxRewardsToShow);
			m_widget.BindDataModel(m_eventRewardTrack.NodesDataModel);
		}
	}

	private void UpdateQuests()
	{
		if (m_eventRewardTrack != null && m_eventRewardTrack.IsValid)
		{
			QuestManager questManager = QuestManager.Get();
			Global.RewardTrackType eventTrackType = m_eventRewardTrack.TrackDataModel.RewardTrackType;
			m_questList = questManager.CreateActiveQuestsDataModel(QuestPool.QuestPoolType.EVENT, (QuestPool.RewardTrackType)eventTrackType, appendTimeUntilNextQuest: true);
			m_questList.Quests.Sort(QuestManager.SortChainQuestsToFront);
			m_widget.BindDataModel(m_questList);
		}
	}

	private void UpdateProgressBar()
	{
		if (m_eventRewardTrack == null || !m_eventRewardTrack.IsValid)
		{
			return;
		}
		RewardTrackDataModel trackDatamodel = m_eventRewardTrack.TrackDataModel;
		int rewardCount = m_eventRewardTrack.NodesDataModel.Nodes.Count;
		if (trackDatamodel.Level == 0)
		{
			return;
		}
		TransformUtil.SetLocalScaleX(m_xpProgressBar, m_barScaleXForRewardCount[rewardCount]);
		TransformUtil.SetLocalPosX(m_xpProgressBar, m_barPositionXForRewardCount[rewardCount]);
		int maxLevel = Mathf.Min(GameUtils.GetRewardTrackLevelsForRewardTrack(m_eventRewardTrack.RewardTrackAsset.ID).Count, rewardCount + 1);
		float progress;
		if (trackDatamodel.Level >= maxLevel)
		{
			progress = 1f;
		}
		else
		{
			float totalSegments = Mathf.Max(0f, (float)maxLevel - 1f);
			float completeSegments = Mathf.Max(0f, (float)trackDatamodel.Level - 1f);
			float totalNormalSegments = Mathf.Max(0f, totalSegments - 1f);
			float completeNormalSegments = Mathf.Max(0f, completeSegments - 1f);
			float firstSegmentPercent = m_firstSegmentPercentForRewardCount[rewardCount];
			float endcapPercent = m_endcapPercentForRewardCount[rewardCount];
			float normalSegmentPercent = (1f - firstSegmentPercent - endcapPercent) / totalNormalSegments;
			float completeSegmentsProgress = normalSegmentPercent * completeNormalSegments;
			if (trackDatamodel.Level > 1)
			{
				completeSegmentsProgress += firstSegmentPercent;
			}
			float currentLevelPercent = (float)trackDatamodel.Xp / (float)trackDatamodel.XpNeeded;
			float partialSegmentProgress = currentLevelPercent * firstSegmentPercent;
			if (trackDatamodel.Level > 1)
			{
				partialSegmentProgress = currentLevelPercent * normalSegmentPercent;
			}
			progress = completeSegmentsProgress + partialSegmentProgress;
		}
		m_xpProgressBar.ResetMaterialReference();
		m_xpProgressBar.SetProgressBar(progress);
	}

	private void UpdateEventSaveData()
	{
		if (m_journalMeta == null || m_specialEventRecord == null)
		{
			Debug.LogError("[Journal] Attempting to save data with missing record or datamodel.");
			return;
		}
		m_journalMeta.IsActivatingEventTrack = false;
		if (m_journalMeta.EventIsNew)
		{
			SpecialEvent.SpecialEventType specialEventType = m_specialEventRecord.SpecialEventType;
			if (!GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PROGRESSION, GameSaveKeySubkeyId.PROGRESSION_EVENT_TAB_EVENT_TYPE_LAST_SEEN, (long)specialEventType)))
			{
				Debug.LogError($"[Journal] Failed to save last seen Special Event type, {specialEventType}");
				return;
			}
			if (m_journalMeta.EventActive)
			{
				long activeSpecialEventId = m_specialEventRecord.ID;
				if (!GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PROGRESSION, GameSaveKeySubkeyId.PROGRESSION_EVENT_TAB_ACTIVE_EVENT_LAST_SEEN, activeSpecialEventId)))
				{
					Debug.LogError($"[Journal] Failed to save last seen Special Event ID, {activeSpecialEventId}");
					return;
				}
			}
			m_journalMeta.EventIsNew = false;
			GameUtils.SetGSDFlag(GameSaveKeyId.PROGRESSION, GameSaveKeySubkeyId.PROGRESSION_EVENT_TAB_HAS_ACKNOWLEDGED_COMPLETION, enableFlag: false);
			m_journalMeta.EventCompleted = false;
		}
		if (m_journalMeta.EventIsEndingSoon)
		{
			long activeSpecialEventId2 = m_specialEventRecord.ID;
			if (!GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.PROGRESSION, GameSaveKeySubkeyId.PROGRESSION_EVENT_TAB_ENDING_SOON_EVENT_LAST_SEEN, activeSpecialEventId2)))
			{
				Debug.LogError($"[Journal] Failed to save ending soon Special Event ID, {activeSpecialEventId2}");
			}
			else
			{
				m_journalMeta.EventIsEndingSoon = false;
			}
		}
	}

	private void HandleEventComplete()
	{
		GameUtils.SetGSDFlag(GameSaveKeyId.PROGRESSION, GameSaveKeySubkeyId.PROGRESSION_EVENT_TAB_HAS_ACKNOWLEDGED_COMPLETION, enableFlag: true);
		m_journalMeta.EventCompleted = true;
	}

	private void UpdateTimerCoroutines()
	{
		if (m_timerUpdateCoroutine != null)
		{
			StopCoroutine(m_timerUpdateCoroutine);
		}
		if (m_eventRewardTrack != null && m_eventRewardTrack.IsActive)
		{
			m_timerUpdateCoroutine = StartCoroutine(UpdateTimeRemaining());
		}
		else
		{
			m_timerUpdateCoroutine = StartCoroutine(UpdateDoorsCountdown());
		}
	}

	private string GetEventTimeRemainingString()
	{
		if (m_eventRewardTrack == null || !m_eventRewardTrack.IsValid)
		{
			return "";
		}
		TimeSpan timeUntilEventEnd = m_eventRewardTrack.GetTimeRemaining();
		if (timeUntilEventEnd <= TimeSpan.Zero)
		{
			return GameStrings.Format("GLUE_PROGRESSION_EVENT_TAB_TIMER_OVER");
		}
		string timeLeftString = TimeUtils.GetCountdownTimerString(timeUntilEventEnd);
		return GameStrings.Format("GLUE_PROGRESSION_EVENT_TAB_TIMER", timeLeftString);
	}

	private IEnumerator UpdateTimeRemaining()
	{
		WaitForSeconds delay = new WaitForSeconds(Mathf.Max(1f, m_timerUpdateSecondsDelay));
		while (true)
		{
			if (m_widget.gameObject.activeSelf)
			{
				string timeRemainingString = GetEventTimeRemainingString();
				if (string.IsNullOrEmpty(timeRemainingString))
				{
					break;
				}
				m_eventRewardTrack.TrackDataModel.TimeRemainingText = timeRemainingString;
			}
			yield return delay;
		}
	}

	private IEnumerator UpdateDoorsCountdown()
	{
		if (m_specialEventRecord == null)
		{
			Debug.LogError("[Journal] Attempting to start doors countdown without Special Event record.");
			yield break;
		}
		WaitForSeconds delay = new WaitForSeconds(Mathf.Max(1f, m_timerUpdateSecondsDelay));
		while (true)
		{
			EventTimingType trackEventType = m_specialEventRecord.EventTiming;
			TimeSpan timeToStart = EventTimingManager.Get().GetTimeUntilEventStart(trackEventType);
			if (timeToStart <= TimeSpan.Zero)
			{
				break;
			}
			string timeToStartString = ((!(timeToStart.TotalHours > (double)m_doorsTimerThresholdHours)) ? TimeUtils.GetCountdownTimerString(timeToStart) : TimeUtils.GetElapsedTimeString((long)timeToStart.TotalSeconds, new TimeUtils.ElapsedStringSet
			{
				m_days = "GLOBAL_DATETIME_SPLASHSCREEN_DAYS"
			}));
			if (timeToStartString != "")
			{
				m_doorsTimerText.Text = GameStrings.Format("GLUE_PROGRESSION_EVENT_TAB_COMING_SOON", timeToStartString);
			}
			yield return delay;
		}
		m_doorsTimerText.Text = "";
	}
}
