using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Core;

namespace FixedReward;

public class RewardQueue
{
	private readonly Map<Achieve.RewardTiming, HashSet<RewardMapIDToShow>> m_rewardMap = new Map<Achieve.RewardTiming, HashSet<RewardMapIDToShow>>();

	public bool HasRewardsToShow(IEnumerable<Achieve.RewardTiming> timings)
	{
		foreach (KeyValuePair<Achieve.RewardTiming, HashSet<RewardMapIDToShow>> reward in m_rewardMap)
		{
			if (timings.Contains(reward.Key) && reward.Value.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetRewards(Achieve.RewardTiming timing, out HashSet<RewardMapIDToShow> rewards)
	{
		return m_rewardMap.TryGetValue(timing, out rewards);
	}

	public void Add(Achieve.RewardTiming timing, RewardMapIDToShow reward)
	{
		if (!m_rewardMap.ContainsKey(timing))
		{
			m_rewardMap[timing] = new HashSet<RewardMapIDToShow>();
		}
		m_rewardMap[timing].Add(reward);
	}

	public void Clear()
	{
		m_rewardMap.Clear();
	}

	public void Clear(Achieve.RewardTiming timing)
	{
		if (m_rewardMap.TryGetValue(timing, out var rewards))
		{
			rewards.Clear();
		}
	}
}
