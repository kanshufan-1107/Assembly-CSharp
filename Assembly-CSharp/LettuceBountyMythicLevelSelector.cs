using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LettuceBountyMythicLevelSelector : MonoBehaviour
{
	[SerializeField]
	private ScrollbarControl m_levelScroller;

	[SerializeField]
	private AsyncReference m_leftLevelTunerReference;

	[SerializeField]
	private AsyncReference m_rightLevelTunerReference;

	private Widget m_widget;

	private Widget m_leftLevelTunerWidget;

	private Widget m_rightLevelTunerWidget;

	private bool m_leftLevelTunerWidgetHidden;

	private bool m_rightLevelTunerWidgetHidden;

	private bool m_leftLevelTunerHeld;

	private bool m_rightLevelTunerHeld;

	private static readonly float TUNER_HOLD_ADVANCE_SPEED = 0.1f;

	private float m_timeToNextTunerAdvance;

	private MercenaryMythicLevelSelectorDataModel m_dataModel;

	private const int LevelStepValue = 5;

	public int CurrentMythicLevel
	{
		get
		{
			return m_dataModel.CurrentMythicLevel;
		}
		private set
		{
			if (value != m_dataModel.CurrentMythicLevel)
			{
				m_dataModel.CurrentMythicLevel = value;
				OnCurrentMythicLevelUpdated(value);
			}
		}
	}

	private int MinMythicLevel => m_dataModel.MinMythicLevel;

	private int MaxMythicLevel => m_dataModel.MaxMythicLevel;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		if (!(m_widget == null))
		{
			m_widget.RegisterEventListener(HandleEvent);
			WidgetUtils.BindorCreateDataModel(m_widget, 782, ref m_dataModel);
			m_levelScroller.SetUpdateHandler(OnSliderUpdated);
			m_leftLevelTunerReference.RegisterReadyListener<Widget>(OnLeftLevelTunerWidgetReady);
			m_rightLevelTunerReference.RegisterReadyListener<Widget>(OnRightLevelTunerWidgetReady);
		}
	}

	private void Update()
	{
		if (!m_leftLevelTunerHeld && !m_rightLevelTunerHeld)
		{
			return;
		}
		m_timeToNextTunerAdvance -= Time.deltaTime;
		if (m_timeToNextTunerAdvance <= 0f)
		{
			if (m_leftLevelTunerHeld)
			{
				int newMythicLevel = Mathf.Max(m_dataModel.MinMythicLevel, CurrentMythicLevel - 5);
				SetMythicLevel(newMythicLevel);
			}
			else
			{
				int newMythicLevel2 = Mathf.Min(m_dataModel.MaxMythicLevel, CurrentMythicLevel + 5);
				SetMythicLevel(newMythicLevel2);
			}
			m_timeToNextTunerAdvance = TUNER_HOLD_ADVANCE_SPEED + m_timeToNextTunerAdvance;
		}
	}

	private void OnLeftLevelTunerWidgetReady(Widget widget)
	{
		m_leftLevelTunerWidget = widget;
		m_leftLevelTunerWidget.TriggerEvent("LEFT");
		Clickable clickable = m_leftLevelTunerWidget.GetComponentInChildren<Clickable>();
		if (clickable != null)
		{
			clickable.AddEventListener(UIEventType.TAP, OnLeftLevelTunerEvent);
			clickable.AddEventListener(UIEventType.HOLD, OnLeftLevelTunerEvent);
			clickable.AddEventListener(UIEventType.RELEASE, OnTunerButtonReleased);
			clickable.AddEventListener(UIEventType.ROLLOUT, OnTunerButtonReleased);
		}
	}

	private void OnRightLevelTunerWidgetReady(Widget widget)
	{
		m_rightLevelTunerWidget = widget;
		m_rightLevelTunerWidget.TriggerEvent("RIGHT");
		Clickable clickable = m_rightLevelTunerWidget.GetComponentInChildren<Clickable>();
		if (clickable != null)
		{
			clickable.AddEventListener(UIEventType.TAP, OnRightLevelTunerEvent);
			clickable.AddEventListener(UIEventType.HOLD, OnRightLevelTunerEvent);
			clickable.AddEventListener(UIEventType.RELEASE, OnTunerButtonReleased);
			clickable.AddEventListener(UIEventType.ROLLOUT, OnTunerButtonReleased);
		}
	}

	private void OnLeftLevelTunerEvent(UIEvent e)
	{
		if (e.GetEventType() == UIEventType.TAP)
		{
			int newMythicLevel = Mathf.Max(m_dataModel.MinMythicLevel, CurrentMythicLevel - 5);
			SetMythicLevel(newMythicLevel);
		}
		else
		{
			m_leftLevelTunerHeld = true;
		}
	}

	private void OnRightLevelTunerEvent(UIEvent e)
	{
		if (e.GetEventType() == UIEventType.TAP)
		{
			int newMythicLevel = Mathf.Min(m_dataModel.MaxMythicLevel, CurrentMythicLevel + 5);
			SetMythicLevel(newMythicLevel);
		}
		else
		{
			m_rightLevelTunerHeld = true;
		}
	}

	private void OnTunerButtonReleased(UIEvent e)
	{
		m_leftLevelTunerHeld = false;
		m_rightLevelTunerHeld = false;
		m_timeToNextTunerAdvance = 0f;
	}

	public void SetMythicLevelLimits(int min, int max)
	{
		m_dataModel.MinMythicLevel = ((min == 1) ? min : ((int)(Mathf.Ceil((float)min / 5f) * 5f)));
		m_dataModel.MaxMythicLevel = (int)(Mathf.Floor((float)max / 5f) * 5f);
		m_levelScroller.SetValue(LevelToPercentage(CurrentMythicLevel));
	}

	public void SetMythicLevel(int mythicLevel)
	{
		if (mythicLevel == 1)
		{
			CurrentMythicLevel = mythicLevel;
		}
		else
		{
			CurrentMythicLevel = (int)Mathf.Round(mythicLevel / 5) * 5;
		}
		m_levelScroller.SetValue(LevelToPercentage(mythicLevel));
	}

	private void HandleEvent(string eventName)
	{
		if (eventName == "ON_OPEN")
		{
			LayerUtils.SetLayer(m_levelScroller.m_DragCollider, GameLayer.Default);
			CurrentMythicLevel = Mathf.Clamp(CurrentMythicLevel, MinMythicLevel, MaxMythicLevel);
			SetMythicLevel(CurrentMythicLevel);
		}
	}

	private void OnSliderUpdated(float val)
	{
		CurrentMythicLevel = Mathf.Clamp(PercentageToLevel(val), MinMythicLevel, MaxMythicLevel);
	}

	private float LevelToPercentage(int mythicLevel)
	{
		return Mathf.InverseLerp(MinMythicLevel, MaxMythicLevel, mythicLevel);
	}

	private int PercentageToLevel(float percent)
	{
		if (MaxMythicLevel <= 0)
		{
			return 0;
		}
		if (percent != 0f)
		{
			if (percent == 1f)
			{
				return MaxMythicLevel;
			}
			return (int)Mathf.Round(Mathf.Lerp(MinMythicLevel, MaxMythicLevel, percent) / 5f) * 5;
		}
		return MinMythicLevel;
	}

	private void OnCurrentMythicLevelUpdated(int mythicLevel)
	{
		if (m_leftLevelTunerWidget != null)
		{
			if (mythicLevel <= m_dataModel.MinMythicLevel)
			{
				SetTunerWidgetVisibility(m_leftLevelTunerWidget, visible: false);
				m_leftLevelTunerWidgetHidden = true;
			}
			else if (m_leftLevelTunerWidgetHidden)
			{
				SetTunerWidgetVisibility(m_leftLevelTunerWidget, visible: true);
				m_leftLevelTunerWidgetHidden = false;
			}
		}
		if (m_rightLevelTunerWidget != null)
		{
			if (mythicLevel >= m_dataModel.MaxMythicLevel)
			{
				SetTunerWidgetVisibility(m_rightLevelTunerWidget, visible: false);
				m_rightLevelTunerWidgetHidden = true;
			}
			else if (m_rightLevelTunerWidgetHidden)
			{
				SetTunerWidgetVisibility(m_rightLevelTunerWidget, visible: true);
				m_rightLevelTunerWidgetHidden = false;
			}
		}
	}

	private void SetTunerWidgetVisibility(Widget arrowWidget, bool visible)
	{
		Vector3 scale;
		iTween.EaseType easeType;
		if (visible)
		{
			scale = Vector3.one;
			if (arrowWidget == m_leftLevelTunerWidget)
			{
				scale.x *= -1f;
			}
			easeType = iTween.EaseType.easeOutBack;
		}
		else
		{
			scale = Vector3.zero;
			easeType = iTween.EaseType.easeInBack;
		}
		Hashtable args = iTween.Hash("scale", scale, "time", 0.5f, "easeintype", easeType, "islocal", true, "name", "TunerWidgetScale");
		iTween.StopByName(arrowWidget.gameObject, "TunerWidgetScale");
		iTween.ScaleTo(arrowWidget.gameObject, args);
	}
}
