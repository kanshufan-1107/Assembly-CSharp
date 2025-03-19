using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class QuestPool
{
	public enum QuestPoolType
	{
		[Description("none")]
		NONE,
		[Description("daily")]
		DAILY,
		[Description("weekly")]
		WEEKLY,
		[Description("event")]
		EVENT
	}

	public enum RewardTrackType
	{
		NONE = 0,
		GLOBAL = 1,
		BATTLEGROUNDS = 2,
		EVENT = 7,
		APPRENTICE = 8
	}

	public static QuestPoolType ParseQuestPoolTypeValue(string value)
	{
		EnumUtils.TryGetEnum<QuestPoolType>(value, out var e);
		return e;
	}

	public static RewardTrackType ParseRewardTrackTypeValue(string value)
	{
		EnumUtils.TryGetEnum<RewardTrackType>(value, out var e);
		return e;
	}
}
