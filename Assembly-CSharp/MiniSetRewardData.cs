using Hearthstone.DataModels;

public class MiniSetRewardData : RewardData
{
	public ProductDataModel DataModel { get; private set; }

	public int MiniSetID { get; set; }

	public int Premium { get; set; }

	public MiniSetRewardData(int cardsRewardId, int premium)
		: base(Reward.Type.MINI_SET)
	{
		MiniSetID = cardsRewardId;
		Premium = premium;
		DataModel = new ProductDataModel();
		if (Premium == 1)
		{
			DataModel.Tags.Add("golden");
		}
	}

	public override string ToString()
	{
		return $"[MiniSetRewardData: CardsRewardID={MiniSetID} Origin={base.Origin} OriginData={base.OriginData}]";
	}

	protected override string GetAssetPath()
	{
		return "MiniSetReward.prefab:dc43a6807e16eb440a7db978dd95ab1f";
	}
}
