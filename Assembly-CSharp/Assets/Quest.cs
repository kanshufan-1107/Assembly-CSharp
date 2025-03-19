using Blizzard.T5.Core.Utils;

namespace Assets;

public static class Quest
{
	public enum RewardTrackType
	{
		NONE = 0,
		GLOBAL = 1,
		BATTLEGROUNDS = 2,
		EVENT = 7,
		APPRENTICE = 8
	}

	public static RewardTrackType ParseRewardTrackTypeValue(string value)
	{
		EnumUtils.TryGetEnum<RewardTrackType>(value, out var e);
		return e;
	}
}
