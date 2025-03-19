using Hearthstone.DataModels;

public class BattlegroundsEmoteRewardData : RewardData
{
	public BattlegroundsEmoteDataModel DataModel { get; private set; }

	public long EmoteID { get; set; }

	public BattlegroundsEmoteRewardData()
		: this(0L, new BattlegroundsEmoteDataModel())
	{
	}

	public BattlegroundsEmoteRewardData(long emoteID, BattlegroundsEmoteDataModel dataModel)
		: base(Reward.Type.BATTLEGROUNDS_EMOTE, showQuestToast: true)
	{
		EmoteID = emoteID;
		DataModel = dataModel;
	}

	public override string ToString()
	{
		return $"[BattlegroundsEmoteRewardData: EmoteId={EmoteID} Origin={base.Origin} OriginData={base.OriginData}]";
	}

	protected override string GetAssetPath()
	{
		return "BattlegroundsEmoteReward.prefab:e199e14ef82bfc045bc4e4ed939daa08";
	}
}
