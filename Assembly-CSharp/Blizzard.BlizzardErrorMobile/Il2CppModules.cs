using System;
using System.Collections.Generic;

namespace Blizzard.BlizzardErrorMobile;

internal static class Il2CppModules
{
	public static readonly Lazy<List<DebugImage>> DebugImages = new Lazy<List<DebugImage>>(LoadDebugImages);

	private static List<DebugImage> LoadDebugImages()
	{
		return new List<DebugImage>();
	}
}
