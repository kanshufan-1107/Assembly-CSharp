using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class LettuceMercenary
{
	public enum Acquiretype
	{
		[Description("none")]
		NONE,
		[Description("packs")]
		PACKS,
		[Description("progression")]
		PROGRESSION,
		[Description("rewards_track")]
		REWARDS_TRACK,
		[Description("event")]
		EVENT
	}

	public static Acquiretype ParseAcquiretypeValue(string value)
	{
		EnumUtils.TryGetEnum<Acquiretype>(value, out var e);
		return e;
	}
}
