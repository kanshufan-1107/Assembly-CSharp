public class MercenaryExpRewardData : RewardData
{
	public int MercenaryId { get; set; }

	public int Amount { get; set; }

	public int InitialExperience { get; set; }

	public int FinalExperience { get; set; }

	public int NumberOfLevelUps { get; set; }

	public MercenaryExpRewardData()
		: this(0, 0, 0, 0)
	{
	}

	public MercenaryExpRewardData(int mercenaryId, int initialExperience, int finalExperience, int amount)
		: base(Reward.Type.MERCENARY_EXP)
	{
		MercenaryId = mercenaryId;
		InitialExperience = initialExperience;
		FinalExperience = finalExperience;
		Amount = amount;
		NumberOfLevelUps = GameUtils.GetMercenaryLevelFromExperience(FinalExperience) - GameUtils.GetMercenaryLevelFromExperience(InitialExperience);
	}

	public override string ToString()
	{
		string mercenaryNoteDesc = GameDbf.LettuceMercenary.GetRecord(MercenaryId).NoteDesc;
		return $"[MercenaryExpRewardData: Mercenary={mercenaryNoteDesc} MercenaryId={MercenaryId} InitialExperience={InitialExperience} FinalExperience={FinalExperience} Amount={Amount} NumberOfLevelUps={NumberOfLevelUps} Origin={base.Origin} OriginData={base.OriginData}]";
	}

	protected override string GetAssetPath()
	{
		return "MercenaryExpReward.prefab:89796271566f9d648a2c5be157d08510";
	}
}
