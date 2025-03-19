using Blizzard.GameService.SDK.Client.Integration;
using UnityEngine;

namespace Hearthstone.Util;

public static class RegionUtils
{
	private static Region s_clientRegion;

	private static BnetRegion LaunchRegion
	{
		get
		{
			BnetRegion retRegion = BnetRegion.REGION_UNKNOWN;
			string regionLaunchOption = BattleNet.GetLaunchOption("REGION", encrypted: false);
			if (!string.IsNullOrEmpty(regionLaunchOption))
			{
				switch (regionLaunchOption)
				{
				case "ST-1":
				case "ST-21":
				case "ST-81":
				case "US":
					retRegion = BnetRegion.REGION_US;
					break;
				case "ST-2":
				case "ST-22":
				case "ST-82":
				case "EU":
					retRegion = BnetRegion.REGION_EU;
					break;
				case "ST-3":
				case "ST-23":
				case "ST-83":
				case "KR":
					retRegion = BnetRegion.REGION_KR;
					break;
				case "TW":
					retRegion = BnetRegion.REGION_TW;
					break;
				case "ST-5":
				case "ST-25":
				case "ST-85":
				case "CN":
					retRegion = BnetRegion.REGION_CN;
					break;
				}
			}
			return retRegion;
		}
	}

	public static bool IsCNLegalRegion => CurrentRegion == Region.CN;

	public static Region CurrentRegion
	{
		get
		{
			if (s_clientRegion == Region.All)
			{
				BnetRegion bnetRegion = BattleNet.GetCurrentRegion();
				if (bnetRegion != BnetRegion.REGION_UNINITIALIZED && bnetRegion != 0)
				{
					s_clientRegion = ConvertBNetRegionToGlobalRegion(bnetRegion);
				}
				if (s_clientRegion == Region.All)
				{
					s_clientRegion = ConvertBNetRegionToGlobalRegion(LaunchRegion);
				}
			}
			return s_clientRegion;
		}
		set
		{
			s_clientRegion = value;
		}
	}

	public static Region ConvertBNetRegionToGlobalRegion(BnetRegion region)
	{
		return region switch
		{
			BnetRegion.REGION_US => Region.US, 
			BnetRegion.REGION_EU => Region.EU, 
			BnetRegion.REGION_KR => Region.KR, 
			BnetRegion.REGION_TW => Region.TW, 
			BnetRegion.REGION_CN => Region.CN, 
			_ => Region.All, 
		};
	}

	public static void ResetRegion()
	{
		CurrentRegion = Region.All;
		Debug.Log("Region: " + CurrentRegion);
	}
}
