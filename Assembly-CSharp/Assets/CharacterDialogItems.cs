using Blizzard.T5.Core.Utils;

namespace Assets;

public static class CharacterDialogItems
{
	public enum CanvasAnchorType
	{
		CENTER,
		LEFT,
		RIGHT,
		BOTTOM,
		TOP,
		BOTTOM_LEFT,
		BOTTOM_RIGHT,
		TOP_LEFT,
		TOP_RIGHT
	}

	public static CanvasAnchorType ParseCanvasAnchorTypeValue(string value)
	{
		EnumUtils.TryGetEnum<CanvasAnchorType>(value, out var e);
		return e;
	}
}
