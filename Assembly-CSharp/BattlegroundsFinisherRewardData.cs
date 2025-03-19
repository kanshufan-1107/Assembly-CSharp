using Hearthstone.DataModels;

public class BattlegroundsFinisherRewardData : RewardData
{
	public BattlegroundsFinisherDataModel DataModel { get; private set; }

	public long FinisherId { get; set; }

	public BattlegroundsFinisherRewardData()
		: this(0L, new BattlegroundsFinisherDataModel())
	{
	}

	public BattlegroundsFinisherRewardData(long finisherId, BattlegroundsFinisherDataModel dataModel)
		: base(Reward.Type.BATTLEGROUNDS_FINISHER, showQuestToast: true)
	{
		FinisherId = finisherId;
		DataModel = dataModel;
	}

	public override string ToString()
	{
		return $"[BattlegroundsFinisherRewardData: FinisherId={FinisherId} Origin={base.Origin} OriginData={base.OriginData}]";
	}

	protected override string GetAssetPath()
	{
		return "BattlegroundsFinisherReward.prefab:1ccdb05bb23b23648afdd2a9989a7629";
	}
}
