using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class OwnershipReqList
{
	public enum ReqTypes
	{
		[Description("None")]
		NONE,
		[Description("Card")]
		CARD
	}

	public static ReqTypes ParseReqTypesValue(string value)
	{
		EnumUtils.TryGetEnum<ReqTypes>(value, out var e);
		return e;
	}
}
