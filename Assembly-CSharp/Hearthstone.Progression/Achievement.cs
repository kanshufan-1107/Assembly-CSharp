using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusUtil;

namespace Hearthstone.Progression;

public static class Achievement
{
	private static readonly AchievementManager.AchievementStatus[] s_statusOrder = new AchievementManager.AchievementStatus[6]
	{
		AchievementManager.AchievementStatus.COMPLETED,
		AchievementManager.AchievementStatus.ACTIVE,
		AchievementManager.AchievementStatus.REWARD_GRANTED,
		AchievementManager.AchievementStatus.REWARD_ACKED,
		AchievementManager.AchievementStatus.UNKNOWN,
		AchievementManager.AchievementStatus.RESET
	};

	public static DataModelList<AchievementDataModel> GetCurrentSortedAchievements(this DataModelList<AchievementDataModel> source)
	{
		return source.Where((AchievementDataModel achievement) => achievement.IsCurrentTierInList(source)).Sorted().ToDataModelList();
	}

	public static void UpdateProgress(this AchievementDataModel source)
	{
		source.ProgressMessage = ProgressUtils.FormatProgressMessage(source.Progress, source.Quota);
	}

	public static void UpdateIsLocked(this AchievementDataModel source)
	{
		source.IsLocked = AchievementManager.Get()?.IsAchievementLocked(source.ID) ?? false;
	}

	public static void UpdateTiers(this DataModelList<AchievementDataModel> source)
	{
		int i = 0;
		for (int iMax = source.Count; i < iMax; i++)
		{
			AchievementDataModel achievement = source[i];
			int reverseChainLength = achievement.CountReverseChain(source);
			int forwardChainLength = achievement.CountForwardChain(source);
			achievement.Tier = reverseChainLength;
			achievement.MaxTier = reverseChainLength + forwardChainLength - 1;
			achievement.TierMessage = ProgressUtils.FormatTierMessage(achievement.Tier, achievement.MaxTier);
		}
	}

	public static int CountReverseChain(this AchievementDataModel source, DataModelList<AchievementDataModel> list)
	{
		int count = 0;
		while (source != null)
		{
			source = source.FindPreviousAchievement(list);
			count++;
		}
		return count;
	}

	public static int CountForwardChain(this AchievementDataModel source, DataModelList<AchievementDataModel> list)
	{
		int count = 0;
		while (source != null)
		{
			source = source.FindNextAchievement(list);
			count++;
		}
		return count;
	}

	public static bool IsCurrentTierInList(this AchievementDataModel source, DataModelList<AchievementDataModel> list)
	{
		AchievementDataModel previous = source.FindPreviousAchievement(list);
		if (previous != null && !ProgressUtils.IsAchievementClaimed(previous.Status))
		{
			return false;
		}
		if (!ProgressUtils.IsAchievementClaimed(source.Status))
		{
			return true;
		}
		return source.FindNextAchievement(list) == null;
	}

	public static AchievementDataModel FindPreviousAchievement(this AchievementDataModel source, DataModelList<AchievementDataModel> list)
	{
		int sourceID = source.ID;
		int i = 0;
		for (int iMax = list.Count; i < iMax; i++)
		{
			AchievementDataModel achievementDataModel = list[i];
			if (achievementDataModel.NextTierID == sourceID)
			{
				return achievementDataModel;
			}
		}
		return null;
	}

	public static AchievementDataModel FindNextAchievement(this AchievementDataModel source, DataModelList<AchievementDataModel> list)
	{
		int sourceTierID = source.NextTierID;
		int i = 0;
		for (int iMax = list.Count; i < iMax; i++)
		{
			AchievementDataModel achievementDataModel = list[i];
			if (achievementDataModel.ID == sourceTierID)
			{
				return achievementDataModel;
			}
		}
		return null;
	}

	public static IEnumerable<AchievementDataModel> Sorted(this IEnumerable<AchievementDataModel> source)
	{
		return from element in source
			orderby Array.IndexOf(s_statusOrder, element.Status), element.SortOrder, (float)element.Progress / (float)element.Quota descending, element.Name
			select element;
	}

	public static IEnumerable<AchievementDataModel> SortByStatusThenClaimedDate(this IEnumerable<AchievementDataModel> source)
	{
		return from element in source
			orderby Array.IndexOf(s_statusOrder, element.Status), AchievementManager.Get().GetClaimedDate(element.ID) descending
			select element;
	}

	public static int CountPoints(this AchievementDbfRecord source, Func<int, PlayerAchievementState> playerState)
	{
		if (!ProgressUtils.IsAchievementClaimed((AchievementManager.AchievementStatus)playerState(source.ID).Status))
		{
			return 0;
		}
		return source.Points;
	}

	public static bool IsCompleted(this AchievementDbfRecord source, Func<int, PlayerAchievementState> playerState)
	{
		return ProgressUtils.IsAchievementComplete((AchievementManager.AchievementStatus)playerState(source.ID).Status);
	}

	public static bool IsUnclaimed(this AchievementDbfRecord source, Func<int, PlayerAchievementState> playerState)
	{
		return playerState(source.ID).Status == 2;
	}
}
