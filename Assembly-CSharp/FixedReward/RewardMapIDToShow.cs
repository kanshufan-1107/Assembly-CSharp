namespace FixedReward;

public class RewardMapIDToShow
{
	public static readonly int NoAchieveID;

	public readonly int rewardMapID;

	public readonly int achieveID;

	public readonly int sortOrder;

	public RewardMapIDToShow(int rewardMapID, int achieveID, int sortOrder)
	{
		this.rewardMapID = rewardMapID;
		this.achieveID = achieveID;
		this.sortOrder = sortOrder;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is RewardMapIDToShow other))
		{
			return false;
		}
		return rewardMapID == other.rewardMapID;
	}

	public override int GetHashCode()
	{
		int num = rewardMapID;
		return num.GetHashCode();
	}
}
