using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class Trigger
{
	public enum Triggertype
	{
		[Description("lua")]
		LUA,
		[Description("collect")]
		COLLECT,
		[Description("collect_non_unique_cards")]
		COLLECT_NON_UNIQUE_CARDS,
		[Description("quest_completion")]
		QUEST_COMPLETION
	}

	public static Triggertype ParseTriggertypeValue(string value)
	{
		EnumUtils.TryGetEnum<Triggertype>(value, out var e);
		return e;
	}
}
