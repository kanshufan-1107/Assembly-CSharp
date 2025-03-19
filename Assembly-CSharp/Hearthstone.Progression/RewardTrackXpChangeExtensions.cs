using Assets;
using PegasusUtil;

namespace Hearthstone.Progression;

public static class RewardTrackXpChangeExtensions
{
	public static Global.RewardTrackType GetRewardTrackType(this RewardTrackXpChange rewardTrackXpChange)
	{
		if (!rewardTrackXpChange.HasRewardSourceType)
		{
			Error.AddDevFatal("Attempted to get reward track type from an XP change with no reward source.");
			return Global.RewardTrackType.NONE;
		}
		switch ((RewardSourceType)rewardTrackXpChange.RewardSourceType)
		{
		default:
			Error.AddDevFatal("Attempted to get reward track type from an XP change with unknown reward source.");
			return Global.RewardTrackType.NONE;
		case RewardSourceType.QUEST:
			return QuestManager.GetRewardTrackType(rewardTrackXpChange.RewardSourceId);
		case RewardSourceType.ACHIEVEMENT:
			return (Global.RewardTrackType)GameDbf.Achievement.GetRecord(rewardTrackXpChange.RewardSourceId).RewardTrackType;
		case RewardSourceType.REWARD_TRACK_FREE:
		case RewardSourceType.REWARD_TRACK_PAID:
		case RewardSourceType.XP_PER_GAME:
		case RewardSourceType.CHEAT:
			return Global.RewardTrackType.GLOBAL;
		}
	}
}
