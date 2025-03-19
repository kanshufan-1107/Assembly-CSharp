using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BattlegroundsFinisherReward : Reward
{
	[SerializeField]
	private WidgetInstance m_rewardItem;

	protected override void InitData()
	{
		SetData(new BattlegroundsFinisherRewardData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_root.SetActive(value: true);
		BattlegroundsFinisherDataModel dataModel = (base.Data as BattlegroundsFinisherRewardData).DataModel;
		m_rewardItem.BindDataModel(dataModel);
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}
}
