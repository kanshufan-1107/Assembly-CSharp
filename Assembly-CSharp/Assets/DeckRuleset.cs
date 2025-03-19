using System;
using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class DeckRuleset
{
	[Flags]
	public enum AssetFlags
	{
		[Description("none")]
		NONE = 0,
		[Description("dev_only")]
		DEV_ONLY = 1,
		[Description("not_packaged_in_client")]
		NOT_PACKAGED_IN_CLIENT = 2,
		[Description("force_do_not_localize")]
		FORCE_DO_NOT_LOCALIZE = 4
	}

	public static AssetFlags ParseAssetFlagsValue(string value)
	{
		EnumUtils.TryGetEnum<AssetFlags>(value, out var e);
		return e;
	}
}
