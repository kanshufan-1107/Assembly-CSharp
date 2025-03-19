using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class CardChange
{
	public enum ChangeType
	{
		[Description("Invalid")]
		INVALID,
		[Description("Buff")]
		BUFF,
		[Description("Nerf")]
		NERF,
		[Description("Addition")]
		ADDITION
	}

	public static ChangeType ParseChangeTypeValue(string value)
	{
		EnumUtils.TryGetEnum<ChangeType>(value, out var e);
		return e;
	}
}
