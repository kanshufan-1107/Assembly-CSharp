using Hearthstone.DataModels;

public class BattlegroundsBoardSkinRewardData : RewardData
{
	public BattlegroundsBoardSkinDataModel DataModel { get; private set; }

	public long BoardSkinId { get; set; }

	public BattlegroundsBoardSkinRewardData()
		: this(0L, new BattlegroundsBoardSkinDataModel())
	{
	}

	public BattlegroundsBoardSkinRewardData(long boardSkinId, BattlegroundsBoardSkinDataModel dataModel)
		: base(Reward.Type.BATTLEGROUNDS_BOARD_SKIN, showQuestToast: true)
	{
		BoardSkinId = boardSkinId;
		DataModel = dataModel;
	}

	public override string ToString()
	{
		return $"[BattlegroundsBoardSkinRewardData: BoardSkinId={BoardSkinId} Origin={base.Origin} OriginData={base.OriginData}]";
	}

	protected override string GetAssetPath()
	{
		return "BattlegroundsBoardSkinReward.prefab:f9cb78c7c8244924cac09826e6310962";
	}
}
