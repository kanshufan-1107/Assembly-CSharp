using Hearthstone.UI;
using UnityEngine;

public class MercenariesKnockoutReward : Reward
{
	[Header("Mercenaries")]
	public RewardBanner m_mercenariesRewardBannerPrefab;

	public WidgetInstance m_mercenaryRewardItem;

	public WidgetInstance m_knockoutRewardItem;

	protected override RewardBanner RewardBannerPrefab => m_mercenariesRewardBannerPrefab;

	protected override void InitData()
	{
	}

	protected override void OnDataSet(bool updateVisuals)
	{
		base.OnDataSet(updateVisuals);
		UpdateBannerObject();
	}

	protected override void ShowReward(bool updateCacheValues)
	{
		m_root.SetActive(value: true);
		if (base.Data is MercenariesKnockoutRewardData dataModel)
		{
			m_mercenaryRewardItem.BindDataModel(dataModel.MercenaryDataModel);
			m_knockoutRewardItem.BindDataModel(dataModel.KnockoutDataModel);
		}
		SetRewardText(GameStrings.Get("GLOBAL_LETTUCE_REWARD_BANNER_TEXT"), "", "");
	}

	protected override void HideReward()
	{
		base.HideReward();
		m_root.SetActive(value: false);
	}

	protected override void OnDestroy()
	{
		(base.Data as RewardItemRewardData)?.OnDestroyReward?.Invoke();
	}
}
