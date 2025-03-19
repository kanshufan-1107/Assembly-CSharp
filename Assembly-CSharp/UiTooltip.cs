using Blizzard.T5.Services;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public class UiTooltip : MonoBehaviour, IWidgetEventListener
{
	[SerializeField]
	private string m_title;

	[SerializeField]
	private string m_description;

	[SerializeField]
	private float m_scale = 0.5f;

	private TooltipZone m_tooltipZone;

	private WidgetTemplate m_widget;

	private IGameStringsService m_gameStringsService;

	private const string CODE_SHOW_TOOLTIP = "CODE_SHOW_TOOLTIP";

	private const string CODE_HIDE_TOOLTIP = "CODE_HIDE_TOOLTIP";

	[Overridable]
	public string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			m_title = value;
		}
	}

	[Overridable]
	public string Description
	{
		get
		{
			return m_description;
		}
		set
		{
			m_description = value;
		}
	}

	[Overridable]
	public float Scale
	{
		get
		{
			return m_scale;
		}
		set
		{
			m_scale = value;
		}
	}

	public WidgetTemplate OwningWidget => m_widget;

	private void Awake()
	{
		m_tooltipZone = GetComponent<TooltipZone>();
		m_widget = GetComponentInParent<WidgetTemplate>();
		m_widget.RegisterDeactivatedListener(OnDeactivate);
	}

	public WidgetEventListenerResponse EventReceived(string eventName, TriggerEventParameters parameters)
	{
		WidgetEventListenerResponse response = default(WidgetEventListenerResponse);
		if (!(eventName == "CODE_SHOW_TOOLTIP"))
		{
			if (eventName == "CODE_HIDE_TOOLTIP")
			{
				HideTooltip();
				response.Consumed = true;
			}
		}
		else
		{
			ShowTooltip();
			response.Consumed = true;
		}
		return response;
	}

	private void OnDeactivate(object unused)
	{
		HideTooltip();
	}

	private void ShowTooltip()
	{
		if (!(m_tooltipZone == null))
		{
			if (m_gameStringsService == null)
			{
				m_gameStringsService = ServiceManager.Get<IGameStringsService>();
			}
			m_tooltipZone.ShowLayerTooltip(m_gameStringsService.Get(m_title.Trim()), m_gameStringsService.Get(m_description.Trim()), m_scale);
		}
	}

	private void HideTooltip()
	{
		if (!(m_tooltipZone == null))
		{
			m_tooltipZone.HideTooltip();
		}
	}
}
