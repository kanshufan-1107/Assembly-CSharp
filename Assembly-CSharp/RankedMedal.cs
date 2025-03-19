using Blizzard.T5.Core;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class RankedMedal : MonoBehaviour
{
	public enum DisplayMode
	{
		Default,
		Stars,
		Chest
	}

	private TooltipZone m_tooltipZone;

	private Widget m_widget;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(WidgetEventListener);
		m_tooltipZone = GetComponent<TooltipZone>();
	}

	public void BindRankedPlayDataModel(RankedPlayDataModel dataModel)
	{
		if (dataModel != GetRankedPlayDataModel())
		{
			m_widget.BindDataModel(dataModel);
		}
	}

	private RankedPlayDataModel GetRankedPlayDataModel()
	{
		IDataModel dataModel = null;
		m_widget.GetDataModel(123, out dataModel);
		return dataModel as RankedPlayDataModel;
	}

	private void WidgetEventListener(string eventName)
	{
		if (eventName.Equals("RollOver"))
		{
			OnRollOver();
		}
		else if (eventName.Equals("RollOut"))
		{
			OnRollOut();
		}
	}

	private void OnRollOver()
	{
		RankedPlayDataModel rankedPlayData = GetRankedPlayDataModel();
		if (rankedPlayData != null && rankedPlayData.IsTooltipEnabled)
		{
			string bodyText = "";
			string titleText = "";
			if (Options.Get().GetBool(Option.IN_RANKED_PLAY_MODE) || SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
			{
				bodyText = (rankedPlayData.IsLegend ? GameStrings.Format("GLOBAL_MEDAL_TOOLTIP_BODY_LEGEND") : ((!new Map<FormatType, string>
				{
					{
						FormatType.FT_STANDARD,
						"GLOBAL_MEDAL_TOOLTIP_BODY_STANDARD"
					},
					{
						FormatType.FT_WILD,
						"GLOBAL_MEDAL_TOOLTIP_BODY_WILD"
					},
					{
						FormatType.FT_CLASSIC,
						"GLOBAL_MEDAL_TOOLTIP_BODY_CLASSIC"
					},
					{
						FormatType.FT_TWIST,
						"GLOBAL_MEDAL_TOOLTIP_BODY_TWIST"
					}
				}.TryGetValue(rankedPlayData.FormatType, out var bodyGlueString)) ? ("UNKNOWN FORMAT TYPE " + rankedPlayData.FormatType) : GameStrings.Format(bodyGlueString)));
				titleText = ((!new Map<FormatType, string>
				{
					{
						FormatType.FT_STANDARD,
						"GLOBAL_MEDAL_TOOLTIP_BEST_RANK_STANDARD"
					},
					{
						FormatType.FT_WILD,
						"GLOBAL_MEDAL_TOOLTIP_BEST_RANK_WILD"
					},
					{
						FormatType.FT_CLASSIC,
						"GLOBAL_MEDAL_TOOLTIP_BEST_RANK_CLASSIC"
					},
					{
						FormatType.FT_TWIST,
						"GLOBAL_MEDAL_TOOLTIP_BEST_RANK_TWIST"
					}
				}.TryGetValue(rankedPlayData.FormatType, out var titleGlueString)) ? ("UNKNOWN FORMAT TYPE " + rankedPlayData.FormatType) : GameStrings.Format(titleGlueString, rankedPlayData.RankName));
			}
			m_tooltipZone.ShowLayerTooltip(titleText, bodyText);
			TooltipPanel tooltipPanel = m_tooltipZone.GetTooltipPanel();
			if ((bool)tooltipPanel)
			{
				tooltipPanel.m_name.WordWrap = false;
				tooltipPanel.m_name.Cache = false;
				tooltipPanel.m_name.UpdateNow();
			}
		}
	}

	private void OnRollOut()
	{
		m_tooltipZone.HideTooltip();
	}
}
