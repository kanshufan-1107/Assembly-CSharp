using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class LoginPopupSequencePopup
{
	public enum LoginPopupSequencePopupType
	{
		[Description("invalid")]
		INVALID,
		[Description("basic")]
		BASIC,
		[Description("featured_cards")]
		FEATURED_CARDS
	}

	public static LoginPopupSequencePopupType ParseLoginPopupSequencePopupTypeValue(string value)
	{
		EnumUtils.TryGetEnum<LoginPopupSequencePopupType>(value, out var e);
		return e;
	}
}
