using System;

public class BattlegroundsTokenRewardData : RewardData
{
	public long Amount { get; set; }

	public DateTime? Date { get; set; }

	public BattlegroundsTokenRewardData()
		: this(0L)
	{
	}

	public BattlegroundsTokenRewardData(long amount)
		: this(amount, null)
	{
	}

	public BattlegroundsTokenRewardData(long amount, DateTime? date)
		: this(amount, date, "", "")
	{
	}

	public BattlegroundsTokenRewardData(long amount, DateTime? date, string nameOverride, string descriptionOverride)
		: base(Reward.Type.BATTLEGROUNDS_TOKEN)
	{
		Amount = amount;
		Date = date;
		base.NameOverride = nameOverride;
		base.DescriptionOverride = descriptionOverride;
	}

	public override string ToString()
	{
		return $"[BattlegroundsTokenRewardData: Amount={Amount} Origin={base.Origin} OriginData={base.OriginData}]";
	}

	protected override string GetAssetPath()
	{
		return "BattlegroundsTokenReward.prefab:3a267887951e67d4e8fa017070fa1dc0";
	}
}
