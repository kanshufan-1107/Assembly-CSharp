using System;
using Hearthstone.DataModels;

public class RewardItemRewardData : RewardData
{
	public RewardItemDataModel DataModel { get; private set; }

	public Action OnDestroyReward { get; private set; }

	public RewardItemRewardData(RewardItemDataModel dataModel, bool showQuestToast, Reward.Type rewardType = Reward.Type.REWARD_ITEM, Action onDestroyReward = null)
		: base(rewardType, showQuestToast)
	{
		DataModel = dataModel;
		OnDestroyReward = onDestroyReward;
	}

	protected override string GetAssetPath()
	{
		return "RewardItemReward.prefab:dd30749fc49afda46b59f7c48d47522c";
	}
}
