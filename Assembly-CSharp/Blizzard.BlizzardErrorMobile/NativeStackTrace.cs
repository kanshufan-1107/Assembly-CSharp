using System;

namespace Blizzard.BlizzardErrorMobile;

internal class NativeStackTrace
{
	public IntPtr[] Frames { get; set; } = Array.Empty<IntPtr>();

	public string ImageUuid { get; set; }

	public string ImageName { get; set; }
}
