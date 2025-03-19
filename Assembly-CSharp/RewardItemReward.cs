using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class RewardItemReward : Reward
{
	private static readonly HashSet<RewardItemType> s_battlegroundsRewards = new HashSet<RewardItemType>
	{
		RewardItemType.BATTLEGROUNDS_GUIDE_SKIN,
		RewardItemType.BATTLEGROUNDS_HERO_SKIN
	};

	private static readonly HashSet<RewardItemType> s_mercenariesRewards = new HashSet<RewardItemType>
	{
		RewardItemType.MERCENARY,
		RewardItemType.MERCENARY_COIN,
		RewardItemType.MERCENARY_BOOSTER,
		RewardItemType.MERCENARY_RANDOM_MERCENARY,
		RewardItemType.MERCENARY_EQUIPMENT,
		RewardItemType.MERCENARY_EQUIPMENT_ICON,
		RewardItemType.MERCENARY_XP
	};

	public WidgetInstance m_rewardItem;

	[Header("Battlegrounds")]
	public RewardBanner m_battlegroundsRewardBannerPrefab;

	[Header("Mercenaries")]
	public RewardBanner m_mercenariesRewardBannerPrefab;

	protected override RewardBanner RewardBannerPrefab
	{
		get
		{
			RewardItemDataModel dataModel = (base.Data as RewardItemRewardData)?.DataModel;
			if (dataModel != null)
			{
				if (s_mercenariesRewards.Contains(dataModel.ItemType))
				{
					return m_mercenariesRewardBannerPrefab;
				}
				if (s_battlegroundsRewards.Contains(dataModel.ItemType))
				{
					return m_battlegroundsRewardBannerPrefab;
				}
			}
			return null;
		}
	}

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
		RewardItemDataModel dataModel = (base.Data as RewardItemRewardData).DataModel;
		m_rewardItem.BindDataModel(dataModel);
		if (s_mercenariesRewards.Contains(dataModel.ItemType))
		{
			SetRewardText(GameStrings.Get("GLOBAL_LETTUCE_REWARD_BANNER_TEXT"), "", "");
		}
		else
		{
			SetRewardText(GameStrings.Get("GLOBAL_REWARD_CARD_HEADLINE"), "", "");
		}
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
