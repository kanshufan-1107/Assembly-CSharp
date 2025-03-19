using System;

[Flags]
public enum ScreenEffectType
{
	NONE = 0,
	BLUR = 1,
	VIGNETTE = 2,
	DESATURATE = 4,
	BLENDTOCOLOR = 8
}
