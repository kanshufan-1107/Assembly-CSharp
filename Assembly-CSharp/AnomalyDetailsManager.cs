using System;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class AnomalyDetailsManager : MonoBehaviour
{
	public AsyncReference m_anomalyPopup;

	private static AnomalyInfoDataModel m_anomalyInfoDataModel;

	private const string DISMISS_MODE_INTRO_GLOW = "DISMISS_MODE_INTRO_GLOW";

	private static AnomalyDetailsManager m_instance;

	private DateTime m_anomalyEndTime = DateTime.MinValue;

	public static AnomalyInfoDataModel AnomalyInfoModel
	{
		get
		{
			if (m_anomalyInfoDataModel == null)
			{
				m_anomalyInfoDataModel = new AnomalyInfoDataModel();
			}
			return m_anomalyInfoDataModel;
		}
	}

	public void Awake()
	{
		if (m_instance == null)
		{
			m_instance = this;
		}
		if (m_anomalyPopup != null)
		{
			m_anomalyPopup.RegisterReadyListener<Widget>(OnAnomalyPopupWidgetReady);
		}
		InitializeDataModel();
		if (!IsAnomalyEventActive())
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		AnomalyInfoModel.RemainingTime = GetTimeLeftInAnomalyEvent();
		if (m_anomalyEndTime < DateTime.UtcNow)
		{
			if (IsAnomalyEventActive())
			{
				InitializeDataModel();
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private void OnDestroy()
	{
		m_anomalyInfoDataModel = null;
	}

	private bool HasSeenAnomalyGlow()
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_ANOMALY_GLOW, out long result);
		return result > 0;
	}

	private void SetSeenAnomalyGlow(bool hasSeen)
	{
		long value = (hasSeen ? 1 : 0);
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.HAS_SEEN_ANOMALY_GLOW, value));
		AnomalyInfoModel.HasSeenAnomalyModeGlow = hasSeen;
	}

	private void OnAnomalyPopupWidgetReady(Widget widget)
	{
		widget.BindDataModel(AnomalyInfoModel);
		widget.RegisterEventListener(DismissNewModeGlow);
	}

	private bool IsAnomalyEventActive()
	{
		if (!EventTimingManager.Get().IsEventActive(EventTimingType.SPECIAL_EVENT_HEARTHSTONE_ANOMALIES))
		{
			return EventTimingManager.Get().IsEventActive(EventTimingType.SPECIAL_EVENT_HEARTHSTONE_ANOMALIES_AFTERWARDS);
		}
		return true;
	}

	private string GetTimeLeftInAnomalyEvent()
	{
		if (m_anomalyEndTime < DateTime.UtcNow)
		{
			m_anomalyEndTime = DateTime.UtcNow;
		}
		TimeSpan span = m_anomalyEndTime - DateTime.UtcNow;
		TimeUtils.ElapsedStringSet timeStringSet = new TimeUtils.ElapsedStringSet
		{
			m_seconds = "GLUE_TAVERN_BRAWL_LABEL_ENDING_SECONDS",
			m_minutes = "GLUE_TAVERN_BRAWL_LABEL_ENDING_MINUTES",
			m_hours = "GLUE_TAVERN_BRAWL_LABEL_ENDING_HOURS",
			m_yesterday = null,
			m_days = "GLUE_TAVERN_BRAWL_LABEL_ENDING_DAYS",
			m_weeks = "GLUE_TAVERN_BRAWL_LABEL_ENDING_WEEKS",
			m_monthAgo = "GLUE_TAVERN_BRAWL_LABEL_ENDING_OVER_1_MONTH"
		};
		return TimeUtils.GetElapsedTimeString((int)span.TotalSeconds, timeStringSet, roundUp: true);
	}

	private void InitializeDataModel()
	{
		InitializeAnomalyEndTime();
		AnomalyInfoModel.HasSeenAnomalyModeGlow = HasSeenAnomalyGlow();
		AnomalyInfoModel.RemainingTime = GetTimeLeftInAnomalyEvent();
		AnomalyInfoModel.RulesText = GetRulesText();
		AnomalyInfoModel.AlwaysActive = IsAlwaysActive();
		AnomalyInfoModel.EventActive = IsEventActive();
		AnomalyInfoModel.CurrentSceneMode = SceneMgr.Get().GetMode().ToString();
	}

	private void InitializeAnomalyEndTime()
	{
		DateTime? endTime = EventTimingManager.Get().GetEventEndTimeUtc(EventTimingType.SPECIAL_EVENT_HEARTHSTONE_ANOMALIES);
		if (endTime.HasValue && endTime.Value > DateTime.UtcNow)
		{
			m_anomalyEndTime = endTime.Value;
			return;
		}
		endTime = EventTimingManager.Get().GetEventEndTimeUtc(EventTimingType.SPECIAL_EVENT_HEARTHSTONE_ANOMALIES_AFTERWARDS);
		if (endTime.HasValue && endTime.Value > DateTime.UtcNow)
		{
			m_anomalyEndTime = endTime.Value;
		}
		else
		{
			m_anomalyEndTime = DateTime.UtcNow;
		}
	}

	private string GetRulesText()
	{
		if (EventTimingManager.Get().IsEventActive(EventTimingType.SPECIAL_EVENT_HEARTHSTONE_ANOMALIES))
		{
			return GameStrings.Get("GLUE_ANOMALIES_EVENT_RULES_ALL");
		}
		return GameStrings.Get("GLUE_ANOMALIES_EVENT_RULES_SOME");
	}

	private bool IsAlwaysActive()
	{
		return EventTimingManager.Get().IsEventActive(EventTimingType.SPECIAL_EVENT_HEARTHSTONE_ANOMALIES);
	}

	private bool IsEventActive()
	{
		if (!EventTimingManager.Get().IsEventActive(EventTimingType.SPECIAL_EVENT_HEARTHSTONE_ANOMALIES))
		{
			return EventTimingManager.Get().IsEventActive(EventTimingType.SPECIAL_EVENT_HEARTHSTONE_ANOMALIES_AFTERWARDS);
		}
		return true;
	}

	public static AnomalyDetailsManager Get()
	{
		return m_instance;
	}

	public static void DismissNewModeGlow(string eventName)
	{
		if (!(eventName != "DISMISS_MODE_INTRO_GLOW"))
		{
			m_instance.SetSeenAnomalyGlow(hasSeen: true);
		}
	}
}
