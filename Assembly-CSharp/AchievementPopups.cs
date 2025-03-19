using System;
using System.Collections.Generic;
using Assets;
using Hearthstone.Progression;

public class AchievementPopups : IDisposable
{
	private List<Achievement> m_progressedAchieves = new List<Achievement>();

	private List<Achievement> m_completedAchieves = new List<Achievement>();

	private Action<List<Achievement>> OnUpdateReward;

	private PopupDisplayManager m_popupDisplayManager;

	public List<Achievement> CompletedAchieves => m_completedAchieves;

	public List<Achievement> ProgressedAchieves => m_progressedAchieves;

	public AchievementPopups(PopupDisplayManager popupDisplayManager, Action<List<Achievement>> updateRewardCallback)
	{
		m_popupDisplayManager = popupDisplayManager;
		OnUpdateReward = updateRewardCallback;
		AchieveManager.Get().RegisterAchievesUpdatedListener(OnAchievesUpdated);
	}

	public void Dispose()
	{
		AchieveManager.Get().RemoveAchievesUpdatedListener(OnAchievesUpdated, null);
	}

	private void OnAchievesUpdated(List<Achievement> updatedAchieves, List<Achievement> completedAchieves, object userData)
	{
		HashSet<Achieve.RewardTiming> rewardTimings = new HashSet<Achieve.RewardTiming>
		{
			Achieve.RewardTiming.IMMEDIATE,
			Achieve.RewardTiming.OUT_OF_BAND
		};
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.ADVENTURE || !SceneMgr.Get().IsTransitioning())
		{
			rewardTimings.Add(Achieve.RewardTiming.ADVENTURE_CHEST);
		}
		PrepareNewlyProgressedAchievesToBeShown();
		PrepareNewlyCompletedAchievesToBeShown(rewardTimings);
	}

	private void PrepareNewlyProgressedAchievesToBeShown()
	{
		m_progressedAchieves = AchieveManager.Get().GetNewlyProgressedQuests();
	}

	public void PrepareNewlyCompletedAchievesToBeShown(HashSet<Achieve.RewardTiming> rewardTimings)
	{
		foreach (Achievement achieve in AchieveManager.Get().GetNewCompletedAchievesToShow())
		{
			if (m_completedAchieves.Find((Achievement obj) => achieve.ID == obj.ID) != null)
			{
				Log.Achievements.Print("PopupDisplayManager: skipping completed achievement already being processed: " + achieve);
			}
			else if (rewardTimings == null || !rewardTimings.Contains(achieve.RewardTiming))
			{
				Log.Achievements.PrintDebug("PopupDisplayManager: skipping completed achievement with {0} reward timing: {1}", achieve.RewardTiming, achieve);
			}
			else if (AchievementManager.Get().IsAchievementLocked(achieve.ID))
			{
				Log.Achievements.PrintDebug("PopupDisplayManager: skipping achievement {0} as it is locked (relevant module is not installed).", achieve);
			}
			else
			{
				Log.Achievements.Print("PopupDisplayManager: adding completed achievement " + achieve);
				m_completedAchieves.Add(achieve);
			}
		}
		OnUpdateReward?.Invoke(m_completedAchieves);
	}
}
