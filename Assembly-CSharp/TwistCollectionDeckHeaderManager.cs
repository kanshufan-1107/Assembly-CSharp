using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class TwistCollectionDeckHeaderManager : MonoBehaviour
{
	public AsyncReference m_twistRulesDisplay;

	public AsyncReference m_twistInfoButton;

	private const string SHOW_TWIST_RULES = "SHOW_TWIST_RULES";

	private static TwistSeasonInfoDataModel m_twistSeasonInfoDataModel;

	private Widget m_twistRulesDisplayWidget;

	private RankedPlaySeason m_twistSeason;

	public void Awake()
	{
		if (m_twistRulesDisplay != null)
		{
			m_twistRulesDisplay.RegisterReadyListener<Widget>(OnTwistRulesDisplayReady);
		}
		if (m_twistInfoButton != null)
		{
			m_twistInfoButton.RegisterReadyListener<Widget>(OnTwistInfoButtonReady);
		}
		m_twistSeasonInfoDataModel = new TwistSeasonInfoDataModel();
		m_twistSeason = RankMgr.Get()?.GetCurrentTwistSeason();
	}

	private void OnTwistRulesDisplayReady(Widget widget)
	{
		m_twistRulesDisplayWidget = widget;
		if (m_twistSeason != null)
		{
			widget.BindDataModel(m_twistSeasonInfoDataModel);
			if (m_twistSeason.SeasonDesc != null)
			{
				m_twistSeasonInfoDataModel.SeasonDescription = GameStrings.Get(m_twistSeason.SeasonDesc);
			}
			else
			{
				Debug.LogError("Season record has no Desc, please update the SeasonDesc for season id" + m_twistSeason.ID);
			}
			if (m_twistSeason.SeasonTitle != null)
			{
				m_twistSeasonInfoDataModel.SeasonTitle = GameStrings.Get(m_twistSeason.SeasonTitle);
			}
			else
			{
				Debug.LogError("Season record has no Title, please update the SeasonTitle for season id" + m_twistSeason.ID);
			}
			List<TAG_CARD_SET> filteredSets = GameUtils.GetTwistSetsWithFilter(m_twistSeason.HiddenCardSets);
			GameUtils.FillTwistDataModelWithValidSets(m_twistSeasonInfoDataModel, filteredSets);
			GameUtils.FillTwistDataModelWithHeroicDecks(m_twistSeasonInfoDataModel);
		}
	}

	private void OnTwistInfoButtonReady(Widget widget)
	{
		widget.RegisterEventListener(ShowTwistRules);
	}

	public void ShowTwistRules(string eventName)
	{
		if (eventName != "SHOW_TWIST_RULES" || m_twistRulesDisplayWidget == null)
		{
			return;
		}
		if (m_twistSeason == null)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_headerText = GameStrings.Get("GLOBAL_TWIST_LOCKED");
			if (RankMgr.IsNextSeasonValid())
			{
				info.m_text = GameStrings.Format("GLOBAL_TWIST_LOCKED_DESC", TimeUtils.GetCountdownTimerString(RankMgr.GetTimeLeftInCurrentSeason(), getFinalSeconds: true));
			}
			else
			{
				info.m_text = GameStrings.Format("GLOBAL_TWIST_LOCKED_DESC", GameStrings.Get("GLUE_TIME_MORE_THAN_A_MONTH"));
			}
			info.m_showAlertIcon = false;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
			DialogManager.Get().ShowPopup(info);
		}
		else
		{
			m_twistRulesDisplayWidget.TriggerEvent("SHOW");
		}
	}
}
