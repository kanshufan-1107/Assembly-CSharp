using System;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class SelectableMedal : MonoBehaviour
{
	[SerializeField]
	private Widget m_selectableMedalWidget;

	[SerializeField]
	private Widget m_rankedMedalWidget;

	[SerializeField]
	private Widget m_battlegroundsMedalWidget;

	private BattlegroundsMedalDataModel m_battlegroundsDataModel;

	private RankedPlayDataModel m_rankedDataModel;

	private bool m_bgDataModelBound;

	private bool m_rankedDataModelBound;

	private void Awake()
	{
		m_selectableMedalWidget.WillLoadSynchronously = true;
	}

	public void UpdateWidget(BnetPlayer player, Action onDisplayBgMedal = null, Action onDisplayRankedMedal = null, Action onDisplayNoMedal = null)
	{
		MedalInfoTranslator mit = RankMgr.Get().GetRankedMedalFromRankPresenceField(player);
		if (RankMgr.Get().GetBattlegroundsMedalFromRankPresenceField(player?.GetHearthstoneGameAccount(), out var bgRating, out var gameType))
		{
			if (m_battlegroundsDataModel == null)
			{
				m_battlegroundsDataModel = new BattlegroundsMedalDataModel();
			}
			m_battlegroundsDataModel.Rating = bgRating;
			m_battlegroundsDataModel.GameType = gameType;
			if (m_rankedDataModelBound)
			{
				m_rankedMedalWidget.gameObject.SetActive(value: false);
				m_selectableMedalWidget.UnbindDataModel(123);
				m_rankedDataModelBound = false;
			}
			if (!m_bgDataModelBound)
			{
				m_battlegroundsMedalWidget.gameObject.SetActive(value: true);
				m_selectableMedalWidget.BindDataModel(m_battlegroundsDataModel);
				m_bgDataModelBound = true;
			}
			m_selectableMedalWidget.Show();
			onDisplayBgMedal?.Invoke();
		}
		else if (mit != null && mit.IsDisplayable())
		{
			mit.CreateOrUpdateDataModel(mit.GetBestCurrentRankFormatType(), ref m_rankedDataModel, RankedMedal.DisplayMode.Default);
			if (m_bgDataModelBound)
			{
				m_battlegroundsMedalWidget.gameObject.SetActive(value: false);
				m_selectableMedalWidget.UnbindDataModel(999);
				m_bgDataModelBound = false;
			}
			if (!m_rankedDataModelBound)
			{
				m_rankedMedalWidget.gameObject.SetActive(value: true);
				m_selectableMedalWidget.BindDataModel(m_rankedDataModel);
				m_rankedDataModelBound = true;
			}
			m_selectableMedalWidget.Show();
			onDisplayRankedMedal?.Invoke();
		}
		else
		{
			m_selectableMedalWidget.Hide();
			onDisplayNoMedal?.Invoke();
		}
	}
}
