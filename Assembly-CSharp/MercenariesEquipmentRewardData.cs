public class MercenariesEquipmentRewardData : RewardData
{
	public int MercenaryId { get; set; }

	public int EquipmentId { get; set; }

	public int EquipmentTier { get; set; }

	public MercenariesEquipmentRewardData()
		: this(0, 0, 0)
	{
	}

	public MercenariesEquipmentRewardData(int mercenaryId, int equipmentId, int equipmentTier)
		: base(Reward.Type.MERCENARY_EQUIPMENT)
	{
		MercenaryId = mercenaryId;
		EquipmentId = equipmentId;
		EquipmentTier = equipmentTier;
	}

	public override string ToString()
	{
		return $"[MercenariesEquipmentUnlockRewardData: MercenaryId={MercenaryId} EquipmentId={EquipmentId} EquipmentTier={EquipmentTier} Origin={base.Origin} OriginData={base.OriginData}]";
	}

	protected override string GetAssetPath()
	{
		return "MercenariesEquipmentReward.prefab:b28ad26bc9ddf6841aba85fe20f6f812";
	}
}
