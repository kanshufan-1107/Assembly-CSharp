public class MercenaryCoinRewardData : RewardData
{
	public int MercenaryId { get; set; }

	public int Quantity { get; set; }

	public MercenaryCoinRewardData()
		: this(0, 0)
	{
	}

	public MercenaryCoinRewardData(int mercenaryId, int quantity)
		: base(Reward.Type.MERCENARY_COIN)
	{
		MercenaryId = mercenaryId;
		Quantity = quantity;
	}

	public override string ToString()
	{
		string mercenaryNoteDesc = GameDbf.LettuceMercenary.GetRecord(MercenaryId).NoteDesc;
		return $"[MercenaryCoinRewardData: Mercenary={mercenaryNoteDesc} MercenaryId={MercenaryId} Quantity={Quantity} Origin={base.Origin} OriginData={base.OriginData}]";
	}

	protected override string GetAssetPath()
	{
		return "MercenaryCoinReward.prefab:27042e77df7ac724a8db9f3d2f69b6d4";
	}
}
