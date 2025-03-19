using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class QuestChange
{
	public enum ChangeAttribute
	{
		[Description("Invalid")]
		INVALID,
		XP,
		[Description("Quota")]
		QUOTA,
		[Description("Trigger")]
		TRIGGER
	}

	public enum ChangeType
	{
		[Description("Invalid")]
		INVALID,
		[Description("Buff")]
		BUFF,
		[Description("Nerf")]
		NERF
	}

	public static ChangeAttribute ParseChangeAttributeValue(string value)
	{
		EnumUtils.TryGetEnum<ChangeAttribute>(value, out var e);
		return e;
	}

	public static ChangeType ParseChangeTypeValue(string value)
	{
		EnumUtils.TryGetEnum<ChangeType>(value, out var e);
		return e;
	}
}
