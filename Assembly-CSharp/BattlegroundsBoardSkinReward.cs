using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BattlegroundsBoardSkinReward : Reward
{
	[SerializeField]
	private WidgetInstance m_rewardItem;

	protected override void InitData()
	{
		SetData(new BattlegroundsBoardSkinRewardData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_root.SetActive(value: true);
		BattlegroundsBoardSkinDataModel dataModel = (base.Data as BattlegroundsBoardSkinRewardData).DataModel;
		m_rewardItem.BindDataModel(dataModel);
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}
}
