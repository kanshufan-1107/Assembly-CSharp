using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.UI;

public static class CameraPassProvider
{
	private const int MaxPasses = 31;

	private const int ReservedRenderingLayerMasks = 9;

	private static List<CameraOverridePass> s_passesAllocated = new List<CameraOverridePass>();

	public static CameraOverridePass RequestPass(string name, LayerMask layers, CustomViewEntryPoint initialEntryPoint)
	{
		if (s_passesAllocated.Count + 9 >= 31)
		{
			Log.UIFramework.PrintError("Exceeded maximum camera pass limit!");
			return null;
		}
		CameraOverridePass pass = new CameraOverridePass(name, layers);
		pass.OverrideRenderLayerMask(GetFirstAvailableMask());
		pass.Schedule(initialEntryPoint);
		s_passesAllocated.Add(pass);
		return pass;
	}

	private static uint GetFirstAvailableMask()
	{
		uint mask = 512u;
		int i;
		for (i = 9; i < 31; i++)
		{
			if (!s_passesAllocated.Exists((CameraOverridePass p) => p.renderLayerMaskOverride == 1 << i))
			{
				mask = (uint)(1 << i);
				break;
			}
		}
		return mask;
	}

	public static void ReleasePass(CameraOverridePass passToRelease)
	{
		if (!s_passesAllocated.Contains(passToRelease))
		{
			Log.UIFramework.PrintError("Attemped to release pass that has not been allocated by CameraPassProvider");
		}
		else
		{
			s_passesAllocated.Remove(passToRelease);
		}
	}
}
