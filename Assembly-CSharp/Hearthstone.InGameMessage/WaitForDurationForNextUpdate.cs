using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using UnityEngine;

namespace Hearthstone.InGameMessage;

public class WaitForDurationForNextUpdate : IJobDependency, IAsyncJobResult
{
	private float m_targetTime;

	public WaitForDurationForNextUpdate(float seconds)
	{
		m_targetTime = Time.realtimeSinceStartup + seconds;
	}

	public bool IsReady()
	{
		if (!ServiceManager.TryGet<InGameMessageScheduler>(out var scheduler))
		{
			return false;
		}
		if (!scheduler.IsTerminated && !(Time.realtimeSinceStartup >= m_targetTime))
		{
			if (InGameMessageScheduler.Get() != null)
			{
				return InGameMessageScheduler.Get().HasNewRegisteredType;
			}
			return false;
		}
		return true;
	}
}
