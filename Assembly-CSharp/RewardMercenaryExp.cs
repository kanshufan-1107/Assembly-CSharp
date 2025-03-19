using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class RewardMercenaryExp : MonoBehaviour
{
	public AsyncReference m_mercenaryRewardReference;

	protected Widget m_mercenaryCardWidget;

	private MercenaryExpRewardData m_rewardData;

	public void Initialize(MercenaryExpRewardData rewardData)
	{
		m_rewardData = rewardData;
	}

	private void Start()
	{
		m_mercenaryRewardReference.RegisterReadyListener<Widget>(OnWidgetReady);
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

	private void OnWidgetReady(Widget widget)
	{
		m_mercenaryCardWidget = widget;
		if (!(m_mercenaryCardWidget == null))
		{
			LettuceMercenary merc = CollectionManager.Get().GetMercenary(m_rewardData.MercenaryId);
			LettuceMercenaryExpRewardDataModel dataModel = GetMercenaryExpRewardDataModel();
			dataModel.Mercenary = MercenaryFactory.CreateMercenaryDataModel(merc);
			dataModel.Mercenary.ExperienceFinal = (int)merc.m_experience;
			dataModel.Mercenary.ExperienceInitial = (int)merc.m_experience - m_rewardData.Amount;
			dataModel.Mercenary.Owned = true;
			dataModel.Mercenary.Label = GameStrings.Get("MERCENARY_CARD_LABEL_XP");
			GameUtils.GetMercenaryLevelFromExperience(dataModel.Mercenary.ExperienceInitial);
			CollectionUtils.PopulateMercenaryCardDataModel(dataModel.Mercenary, merc.GetEquippedArtVariation());
			CollectionUtils.SetMercenaryStatsByLevel(dataModel.Mercenary, merc.ID, merc.m_level, merc.m_isFullyUpgraded);
			dataModel.ExperienceDeltaText = GameStrings.Format("GLUE_LETTUCE_MERCENARY_EXP_GAIN", m_rewardData.Amount);
			dataModel.LeveledUp = m_rewardData.NumberOfLevelUps > 0;
		}
	}
}
