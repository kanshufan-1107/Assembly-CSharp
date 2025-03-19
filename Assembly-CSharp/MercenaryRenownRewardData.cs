public class MercenaryRenownRewardData : RewardData
{
	public int Amount { get; set; }

	public MercenaryRenownRewardData(int amount)
		: base(Reward.Type.MERCENARY_RENOWN)
	{
		Amount = amount;
	}

	public override string ToString()
	{
		return $"[MercenaryRenownRewardData: Quantity={Amount} Origin={base.Origin} OriginData={base.OriginData}]";
	}

	protected override string GetAssetPath()
	{
		return "MercenaryRenownReward.prefab:f119250af6921d04c8e23ab1e3cb73b9";
	}
}
