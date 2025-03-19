public class DeckRewardData : RewardData
{
	public int DeckTemplateId { get; set; }

	public int DeckId { get; set; }

	public int ClassId { get; set; }

	public string DeckNameOverride { get; set; }

	public string PrefabAssetPath { get; set; }

	public DeckRewardData(int deckTemplateId, int deckId, int classId, string deckNameOverride)
		: base(Reward.Type.DECK)
	{
		DeckTemplateId = deckTemplateId;
		DeckId = deckId;
		ClassId = classId;
		DeckNameOverride = deckNameOverride;
		PrefabAssetPath = "DeckReward.prefab:8f715cc0ca835a444b1794ae1d5ab4a7";
	}

	public override string ToString()
	{
		return string.Format("[DeckReward: DeckId={0}]", base.RewardType, DeckId);
	}

	protected override string GetAssetPath()
	{
		return PrefabAssetPath;
	}
}
