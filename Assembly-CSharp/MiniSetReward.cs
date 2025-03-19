using Hearthstone.DataModels;
using Hearthstone.UI;

public class MiniSetReward : Reward
{
	public WidgetInstance m_rewardItem;

	protected override void InitData()
	{
		SetData(new CardBackRewardData(), updateVisuals: false);
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_root.SetActive(value: true);
		ProductDataModel dataModel = (base.Data as MiniSetRewardData).DataModel;
		m_rewardItem.BindDataModel(dataModel);
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}
}
