using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class MercenaryExpReward : Reward
{
	public AsyncReference m_mercenaryExpRewardReference;

	protected Widget m_mercenaryCardWidget;

	protected bool m_hidden;

	protected override void Start()
	{
		base.Start();
		m_mercenaryExpRewardReference.RegisterReadyListener<Widget>(OnWidgetReady);
	}

	private void OnWidgetReady(Widget widget)
	{
		m_mercenaryCardWidget = widget;
		if (!(m_mercenaryCardWidget == null) && m_hidden)
		{
			m_mercenaryCardWidget.Hide();
		}
	}

	private LettuceMercenaryExpRewardDataModel GetMercenaryExpRewardDataModel()
	{
		if (m_mercenaryCardWidget == null)
		{
			return null;
		}
		if (!m_mercenaryCardWidget.GetDataModel(251, out var dataModel))
		{
			dataModel = new LettuceMercenaryExpRewardDataModel();
			m_mercenaryCardWidget.BindDataModel(dataModel);
		}
		return dataModel as LettuceMercenaryExpRewardDataModel;
	}

	protected override void InitData()
	{
		SetData(new MercenaryExpRewardData(), updateVisuals: false);
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		if (!updateVisuals || m_hidden || m_mercenaryCardWidget == null)
		{
			return;
		}
		if (!(base.Data is MercenaryExpRewardData expRewardData))
		{
			Debug.LogWarning($"MercenaryExpReward.OnDataSet() - data {base.Data} is not MercenaryExpRewardData");
			return;
		}
		LettuceMercenaryDbfRecord mercenaryRecord = GameDbf.LettuceMercenary.GetRecord(expRewardData.MercenaryId);
		if (mercenaryRecord == null)
		{
			Debug.LogWarning($"MercenaryExpReward.OnDataSet() - data {expRewardData.MercenaryId} has invalid mercenary id");
			return;
		}
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(mercenaryRecord.ID);
		LettuceMercenaryExpRewardDataModel dataModel = GetMercenaryExpRewardDataModel();
		dataModel.Mercenary = MercenaryFactory.CreateMercenaryDataModel(merc);
		dataModel.Mercenary.ExperienceFinal = expRewardData.FinalExperience;
		dataModel.Mercenary.ExperienceInitial = expRewardData.InitialExperience;
		dataModel.Mercenary.Owned = true;
		dataModel.Mercenary.Label = GameStrings.Get("MERCENARY_CARD_LABEL_XP");
		int level = GameUtils.GetMercenaryLevelFromExperience(expRewardData.InitialExperience);
		CollectionUtils.PopulateMercenaryCardDataModel(dataModel.Mercenary, merc.GetEquippedArtVariation());
		CollectionUtils.SetMercenaryStatsByLevel(dataModel.Mercenary, merc.ID, level, merc.m_isFullyUpgraded);
		dataModel.ExperienceDeltaText = GameStrings.Format("GLUE_LETTUCE_MERCENARY_EXP_GAIN", expRewardData.Amount);
		dataModel.LeveledUp = expRewardData.NumberOfLevelUps > 0;
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_hidden = false;
		m_root.SetActive(value: true);
		if (m_mercenaryCardWidget != null)
		{
			m_mercenaryCardWidget.Show();
			OnDataSet(updateVisuals: true);
		}
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
		m_hidden = true;
		if (m_mercenaryCardWidget != null)
		{
			m_mercenaryCardWidget.Hide();
		}
	}
}
