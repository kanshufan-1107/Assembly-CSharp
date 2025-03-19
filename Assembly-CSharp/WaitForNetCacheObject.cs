using Blizzard.T5.Jobs;
using Blizzard.T5.Services;

public class WaitForNetCacheObject<T> : IJobDependency, IAsyncJobResult
{
	public bool IsReady()
	{
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			return netCache.GetNetObject<T>() != null;
		}
		return false;
	}
}
