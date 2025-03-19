using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class League
{
	public enum LeagueType
	{
		[Description("unknown")]
		UNKNOWN,
		[Description("normal")]
		NORMAL,
		[Description("new_player")]
		NEW_PLAYER
	}

	public static LeagueType ParseLeagueTypeValue(string value)
	{
		EnumUtils.TryGetEnum<LeagueType>(value, out var e);
		return e;
	}
}
