public static class BoosterPackUtils
{
	public static int GetBoosterCount(int boosterDbId)
	{
		NetCache.NetCacheBoosters netBoosters = NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>();
		if (netBoosters == null)
		{
			return 0;
		}
		return netBoosters.GetBoosterStack(boosterDbId)?.Count ?? 0;
	}

	public static int GetBoosterOpenedCount(int boosterDbId)
	{
		NetCache.NetCacheBoosters netBoosters = NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>();
		if (netBoosters == null)
		{
			return 0;
		}
		NetCache.BoosterStack stack = netBoosters.GetBoosterStack(boosterDbId);
		if (stack == null)
		{
			return 0;
		}
		return stack.EverGrantedCount - stack.Count;
	}

	public static int GetTotalBoosterCount()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>().GetTotalNumBoosters();
	}

	public static int GetBoosterStackCount()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>().BoosterStacks.Count;
	}

	public static void OpenBooster(int boosterDbId, int numPacks)
	{
		if (boosterDbId == 629)
		{
			Network.Get().OpenMercenariesPackRequest();
		}
		else
		{
			Network.Get().OpenBooster(boosterDbId, numPacks);
		}
	}

	public static bool LoadMercenaryCollectionIfRequired()
	{
		if (CollectionManager.Get().IsLettuceLoaded())
		{
			return false;
		}
		foreach (NetCache.BoosterStack booster in NetCache.Get().GetNetObject<NetCache.NetCacheBoosters>().BoosterStacks)
		{
			if (booster.Id == 629 && booster.Count > 0)
			{
				CollectionManager.Get().StartInitialMercenaryLoadIfRequired();
				return true;
			}
		}
		return false;
	}
}
