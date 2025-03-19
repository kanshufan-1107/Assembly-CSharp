using System;
using System.Runtime.CompilerServices;
using Hearthstone.DataModels;

public class MercenariesKnockoutRewardData : RewardData
{
	[CompilerGenerated]
	private Action _003COnDestroyReward_003Ek__BackingField;

	public RewardItemDataModel MercenaryDataModel { get; private set; }

	public RewardItemDataModel KnockoutDataModel { get; private set; }

	private Action OnDestroyReward
	{
		[CompilerGenerated]
		set
		{
			_003COnDestroyReward_003Ek__BackingField = value;
		}
	}

	public MercenariesKnockoutRewardData(RewardItemDataModel mercenaryDataModel, RewardItemDataModel knockoutDataModel, Action onDestroyReward = null)
		: base(Reward.Type.MERCENARY_KNOCKOUT, showQuestToast: true)
	{
		MercenaryDataModel = mercenaryDataModel;
		KnockoutDataModel = knockoutDataModel;
		OnDestroyReward = onDestroyReward;
	}

	protected override string GetAssetPath()
	{
		return "MercenariesKnockoutReward.prefab:5d024a3773d16e44da415b6693266fdc";
	}
}
