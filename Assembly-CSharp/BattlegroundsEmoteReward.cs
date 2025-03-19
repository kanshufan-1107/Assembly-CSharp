using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BattlegroundsEmoteReward : Reward
{
	[SerializeField]
	private WidgetInstance m_rewardItem;

	[SerializeField]
	private string m_emoteEventToTrigger = "DEFAULT_BOTTOM_LEFT";

	protected override void InitData()
	{
		SetData(new BattlegroundsEmoteRewardData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_root.SetActive(value: true);
		BattlegroundsEmoteDataModel dataModel = (base.Data as BattlegroundsEmoteRewardData).DataModel;
		m_rewardItem.BindDataModel(dataModel);
		m_rewardItem.TriggerEvent(m_emoteEventToTrigger);
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}
}
