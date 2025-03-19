using Blizzard.T5.Core.Utils;

namespace Assets;

public static class LettuceBounty
{
	public enum MercenariesBountyDifficulty
	{
		NONE,
		NORMAL,
		HEROIC,
		MYTHIC
	}

	public static MercenariesBountyDifficulty ParseMercenariesBountyDifficultyValue(string value)
	{
		EnumUtils.TryGetEnum<MercenariesBountyDifficulty>(value, out var e);
		return e;
	}
}
