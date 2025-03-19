public class MercenariesAbilityUnlockRewardData : RewardData
{
	public int MercenaryId { get; set; }

	public int AbilityId { get; set; }

	public MercenariesAbilityUnlockRewardData()
		: this(0, 0)
	{
	}

	public MercenariesAbilityUnlockRewardData(int mercenaryId, int abilityId)
		: base(Reward.Type.MERCENARY_ABILITY_UNLOCK)
	{
		MercenaryId = mercenaryId;
		AbilityId = abilityId;
	}

	public override string ToString()
	{
		return $"[MercenariesAbilityUnlockRewardData: MercenaryId={MercenaryId} MercenaryId={AbilityId} Origin={base.Origin} OriginData={base.OriginData}]";
	}

	protected override string GetAssetPath()
	{
		return "MercenariesAbilityUnlockReward.prefab:09a6c282da0ad3941b068ba387bbf4d1";
	}
}
