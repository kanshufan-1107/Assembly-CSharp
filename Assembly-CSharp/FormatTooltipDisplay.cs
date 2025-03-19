using Hearthstone.UI;
using UnityEngine;

public class FormatTooltipDisplay : MonoBehaviour
{
	public Widget m_twistFormatWidget;

	public TooltipZone m_twistToolTipZone;

	public TooltipZone m_standardToolTipZone;

	public TooltipZone m_wildToolTipZone;

	public TooltipZone m_casualToolTipZone;

	public float tooltipScale = 7f;

	private const string SHOW_TWIST_TOOLTIP = "SHOW_TWIST_TOOLTIP";

	private const string HIDE_ALL_TOOLTIPS = "HIDE_ALL_TOOLTIPS";

	private const string SHOW_STANDARD_TOOLTIP = "SHOW_STANDARD_TOOLTIP";

	private const string SHOW_WILD_TOOLTIP = "SHOW_WILD_TOOLTIP";

	private const string SHOW_CASUAL_TOOLTIP = "SHOW_CASUAL_TOOLTIP";

	private void Awake()
	{
		m_twistFormatWidget.RegisterEventListener(HandleTooltipEvent);
	}

	public void HandleTooltipEvent(string eventName)
	{
		switch (eventName)
		{
		case "SHOW_TWIST_TOOLTIP":
			if (RankMgr.IsCurrentTwistSeasonActive())
			{
				ShowToolTipForFormat(m_twistToolTipZone, "GLOBAL_TWIST", "GLOBAL_TOOLTIP_MODE_TWIST");
			}
			else
			{
				ShowToolTipForFormat(m_twistToolTipZone, "GLOBAL_TWIST_LOCKED", "GLOBAL_TOOLTIP_MODE_TWIST_SEASON_DISABLED");
			}
			break;
		case "SHOW_STANDARD_TOOLTIP":
			ShowToolTipForFormat(m_standardToolTipZone, "GLOBAL_STANDARD", "GLOBAL_TOOLTIP_MODE_STANDARD");
			break;
		case "SHOW_CASUAL_TOOLTIP":
			ShowToolTipForFormat(m_casualToolTipZone, "GLUE_TOURNAMENT_CASUAL", "GLOBAL_TOOLTIP_MODE_CASUAL");
			break;
		case "SHOW_WILD_TOOLTIP":
			ShowToolTipForFormat(m_wildToolTipZone, "GLOBAL_WILD", "GLOBAL_TOOLTIP_MODE_WILD");
			break;
		case "HIDE_ALL_TOOLTIPS":
			if ((bool)m_twistFormatWidget)
			{
				m_twistToolTipZone.HideTooltip();
			}
			if ((bool)m_standardToolTipZone)
			{
				m_standardToolTipZone.HideTooltip();
			}
			if ((bool)m_casualToolTipZone)
			{
				m_casualToolTipZone.HideTooltip();
			}
			if ((bool)m_wildToolTipZone)
			{
				m_wildToolTipZone.HideTooltip();
			}
			break;
		}
	}

	private void ShowToolTipForFormat(TooltipZone tooltipZone, string title, string description)
	{
		if (!(tooltipZone == null))
		{
			tooltipZone.ShowTooltip(GameStrings.Get(title), GameStrings.Get(description), tooltipScale);
		}
	}
}
