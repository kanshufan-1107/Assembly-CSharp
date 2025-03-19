using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Hearthstone.Core.Streaming;
using Hearthstone.DataModels;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone.Progression;

public static class ProgressUtils
{
	public static readonly RewardTrackPaidType[] AllValidRewardTrackPaidType = new RewardTrackPaidType[3]
	{
		RewardTrackPaidType.RTPT_FREE,
		RewardTrackPaidType.RTPT_PAID,
		RewardTrackPaidType.RTPT_PAID_PREMIUM
	};

	public static bool ShowDebugIds
	{
		get
		{
			return false;
		}
		set
		{
			Options.Get().SetBool(Option.PROG_TILE_DEBUG, value);
		}
	}

	public static bool ShowHiddenAchievements
	{
		get
		{
			return false;
		}
		set
		{
			Options.Get().SetBool(Option.PROG_HIDDEN_ACHIEVEMENTS, value);
		}
	}

	public static bool EarlyConcedeConfirmationDisabled
	{
		get
		{
			return false;
		}
		set
		{
			Options.Get().SetBool(Option.EARLY_CONCEDE_CONFIRMATION_DISABLED, value);
		}
	}

	public static void ShowOfflinePopup()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_OFFLINE_FEATURE_DISABLED_HEADER"),
			m_text = GameStrings.Get("GLUE_OFFLINE_FEATURE_DISABLED_BODY"),
			m_responseDisplay = AlertPopup.ResponseDisplay.OK,
			m_showAlertIcon = false
		};
		DialogManager.Get().ShowPopup(info);
	}

	public static string FormatCompletionPercentageMessage(int points, int available)
	{
		return GameStrings.Format("GLOBAL_PROGRESSION_ACHIEVEMENT_COMPLETION_PERCENTAGE", Mathf.FloorToInt((float)points * 100f / (float)available));
	}

	public static string FormatAchievementCompletionDate(long completionDate)
	{
		return GameStrings.Format("GLOBAL_PROGRESSION_ACHIEVEMENT_COMPLETION", GameStrings.Format("GLOBAL_PROGRESSION_ACHIEVEMENT_COMPLETION_DATE", TimeUtils.UnixTimeStampToDateTimeLocal(completionDate)));
	}

	public static string FormatAchievementSubcategoryFullName(AchievementCategoryDbfRecord categoryRecord, AchievementSubcategoryDbfRecord subcategoryRecord)
	{
		if (!string.IsNullOrEmpty(categoryRecord.Name) && !string.IsNullOrEmpty(subcategoryRecord.Name))
		{
			return GameStrings.Format("GLOBAL_PROGRESSION_ACHIEVEMENT_SUBCATEGORY_NAME", categoryRecord.Name.GetString(), subcategoryRecord.Name.GetString());
		}
		if (!string.IsNullOrEmpty(categoryRecord.Name))
		{
			return categoryRecord.Name;
		}
		if (!string.IsNullOrEmpty(subcategoryRecord.Name))
		{
			return subcategoryRecord.Name;
		}
		return "";
	}

	public static string FormatProgressMessage(int progress, int quota)
	{
		return GameStrings.Format("GLOBAL_PROGRESSION_PROGRESS_MESSAGE", progress, quota);
	}

	public static string FormatTierMessage(int tier, int maxTier)
	{
		return GameStrings.Format("GLOBAL_PROGRESSION_TIER_MESSAGE", tier, maxTier);
	}

	public static string FormatRewardsSummary(RewardListDataModel rewards, int xp, int points, bool isBoosted)
	{
		return string.Join("\n", new string[3]
		{
			FormatRewards(rewards),
			isBoosted ? FormatRewardTrackXPBoosted(xp) : FormatRewardTrackXP(xp),
			FormatAchievementPoints(points)
		}.Where((string text) => text != null).ToArray());
	}

	public static string FormatRewards(RewardListDataModel rewards)
	{
		return rewards?.Description;
	}

	public static string FormatRewardTrackXP(int xp)
	{
		if (xp <= 0)
		{
			return null;
		}
		return GameStrings.Format("GLOBAL_PROGRESSION_REWARD_TRACK_XP", xp);
	}

	public static string FormatRewardTrackXPBoosted(int xp)
	{
		if (xp <= 0)
		{
			return null;
		}
		return string.Format("<color=#24f104ff>{0}</color>", GameStrings.Format("GLOBAL_PROGRESSION_REWARD_TRACK_XP", xp));
	}

	public static string FormatAchievementPoints(int points)
	{
		if (points <= 0)
		{
			return null;
		}
		return GameStrings.Format("GLOBAL_PROGRESSION_POINTS", points);
	}

	public static string FormatDescription(string description, int quota)
	{
		if (string.IsNullOrEmpty(description))
		{
			return string.Empty;
		}
		string result = description.Replace("$q", "{0}");
		GameStrings.PluralNumber[] pluralNumbers = new GameStrings.PluralNumber[1]
		{
			new GameStrings.PluralNumber
			{
				m_index = 0,
				m_number = quota
			}
		};
		return GameStrings.FormatLocalizedStringWithPlurals(result, pluralNumbers, quota);
	}

	public static bool IsAchievementComplete(AchievementManager.AchievementStatus status)
	{
		if ((uint)(status - 2) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsAchievementClaimed(AchievementManager.AchievementStatus status)
	{
		if ((uint)(status - 3) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static DownloadTags.Content GetContentTagFromAchievementCategoryOrSubcategoryName(string name)
	{
		return name switch
		{
			"Mercenaries" => DownloadTags.Content.Merc, 
			"Adventures" => DownloadTags.Content.Adventure, 
			"Battlegrounds" => DownloadTags.Content.Bgs, 
			"Tavern Brawl" => DownloadTags.Content.Tb, 
			"Duels" => DownloadTags.Content.Duels, 
			"Arena" => DownloadTags.Content.Arena, 
			_ => DownloadTags.Content.Unknown, 
		};
	}

	public static void UpdateAchievementIsLocked(AchievementDataModel achievement)
	{
		achievement.UpdateIsLocked();
		if (achievement.IsLocked)
		{
			DownloadTags.Content contentTag = AchievementManager.Get().GetContentTagFromAchievement(achievement.ID);
			if (contentTag != 0)
			{
				achievement.LockedMessage = AchievementTile.GetLockedMessage(contentTag);
			}
		}
	}

	public static bool HasOwnedRewardTrackPaidType(RewardTrackDbfRecord rewardTrackRecord, RewardTrackPaidType paidType)
	{
		return paidType switch
		{
			RewardTrackPaidType.RTPT_FREE => true, 
			RewardTrackPaidType.RTPT_PAID => AccountLicenseMgr.Get().OwnsAccountLicense((rewardTrackRecord?.AccountLicenseRecord?.LicenseId).GetValueOrDefault()), 
			RewardTrackPaidType.RTPT_PAID_PREMIUM => AccountLicenseMgr.Get().OwnsAccountLicense((rewardTrackRecord?.PaidPremiumTier?.AccountLicenseRecord?.LicenseId).GetValueOrDefault()), 
			_ => false, 
		};
	}

	public static RewardTrack.RewardStatus GetRewardStatus(PlayerRewardTrackLevelState levelState, RewardTrackPaidType paidType)
	{
		switch (paidType)
		{
		case RewardTrackPaidType.RTPT_FREE:
			if (!levelState.HasFreeRewardStatus)
			{
				return RewardTrack.RewardStatus.UNKNOWN;
			}
			return (RewardTrack.RewardStatus)levelState.FreeRewardStatus;
		case RewardTrackPaidType.RTPT_PAID:
			if (!levelState.HasPaidRewardStatus)
			{
				return RewardTrack.RewardStatus.UNKNOWN;
			}
			return (RewardTrack.RewardStatus)levelState.PaidRewardStatus;
		case RewardTrackPaidType.RTPT_PAID_PREMIUM:
			if (!levelState.HasPaidPremiumRewardStatus)
			{
				return RewardTrack.RewardStatus.UNKNOWN;
			}
			return (RewardTrack.RewardStatus)levelState.PaidPremiumRewardStatus;
		default:
			return RewardTrack.RewardStatus.UNKNOWN;
		}
	}

	public static void SetRewardStatus(PlayerRewardTrackLevelState levelState, RewardTrackPaidType paidType, RewardTrack.RewardStatus status)
	{
		switch (paidType)
		{
		case RewardTrackPaidType.RTPT_FREE:
			levelState.FreeRewardStatus = (int)status;
			break;
		case RewardTrackPaidType.RTPT_PAID:
			levelState.PaidRewardStatus = (int)status;
			break;
		case RewardTrackPaidType.RTPT_PAID_PREMIUM:
			levelState.PaidPremiumRewardStatus = (int)status;
			break;
		}
	}

	public static int GetRewardListAssetId(RewardTrackLevelDbfRecord rewardTrackLevelAsset, RewardTrackPaidType paidType)
	{
		return paidType switch
		{
			RewardTrackPaidType.RTPT_FREE => rewardTrackLevelAsset.FreeRewardList, 
			RewardTrackPaidType.RTPT_PAID => rewardTrackLevelAsset.PaidRewardList, 
			RewardTrackPaidType.RTPT_PAID_PREMIUM => rewardTrackLevelAsset.PaidPremiumRewardList, 
			_ => 0, 
		};
	}

	public static RewardListDbfRecord GetRewardListRecord(RewardTrackLevelDbfRecord rewardTrackLevelAsset, RewardTrackPaidType paidType)
	{
		return paidType switch
		{
			RewardTrackPaidType.RTPT_FREE => rewardTrackLevelAsset.FreeRewardListRecord, 
			RewardTrackPaidType.RTPT_PAID => rewardTrackLevelAsset.PaidRewardListRecord, 
			RewardTrackPaidType.RTPT_PAID_PREMIUM => rewardTrackLevelAsset.PaidPremiumRewardListRecord, 
			_ => null, 
		};
	}

	public static List<RewardItemDbfRecord> GetRewardItemsForRewardList(RewardTrackLevelDbfRecord rewardTrackLevelAsset, HashSet<RewardTrackPaidType> paidTypes)
	{
		List<RewardItemDbfRecord> ret = null;
		if (paidTypes.Count > 1)
		{
			ret = new List<RewardItemDbfRecord>();
		}
		foreach (RewardTrackPaidType paidType in paidTypes)
		{
			if (GetRewardListAssetId(rewardTrackLevelAsset, paidType) != 0)
			{
				List<RewardItemDbfRecord> temp = GameUtils.GetRewardItemsForRewardList(GetRewardListAssetId(rewardTrackLevelAsset, paidType));
				if (ret == null)
				{
					return temp;
				}
				ret.AddRange(temp);
			}
		}
		return ret;
	}

	public static bool HasRewardLists(RewardTrackLevelDbfRecord rewardTrackLevelAsset, HashSet<RewardTrackPaidType> paidTypes)
	{
		foreach (RewardTrackPaidType paidType in paidTypes)
		{
			if (GetRewardListRecord(rewardTrackLevelAsset, paidType) != null)
			{
				return true;
			}
		}
		return false;
	}

	public static RewardTrackNodeRewardsDataModel GetNodeRewards(RewardTrackNodeDataModel nodeDatModel, RewardTrackPaidType paidType)
	{
		return paidType switch
		{
			RewardTrackPaidType.RTPT_FREE => nodeDatModel.FreeRewards, 
			RewardTrackPaidType.RTPT_PAID => nodeDatModel.PaidRewards, 
			RewardTrackPaidType.RTPT_PAID_PREMIUM => nodeDatModel.PaidPremiumRewards, 
			_ => null, 
		};
	}

	public static HashSet<RewardTrackPaidType> GetRewardTrackPaidTypesForProduct(RewardTrackDbfRecord record, int productId)
	{
		HashSet<RewardTrackPaidType> paidTypes = new HashSet<RewardTrackPaidType>();
		if (record != null && record.PaidPremiumTier?.ProductId == productId)
		{
			paidTypes.Add(RewardTrackPaidType.RTPT_PAID);
			paidTypes.Add(RewardTrackPaidType.RTPT_PAID_PREMIUM);
		}
		else if (record != null && record.PaidPremiumTier?.UpgradeProductId == productId)
		{
			paidTypes.Add(RewardTrackPaidType.RTPT_PAID_PREMIUM);
		}
		else
		{
			paidTypes.Add(RewardTrackPaidType.RTPT_PAID);
		}
		return paidTypes;
	}

	public static bool IsEventRewardTrackType(Global.RewardTrackType rewardTrackType)
	{
		return rewardTrackType == Global.RewardTrackType.EVENT;
	}

	public static bool HasClaimedRewardTrackReward(RewardTrack.RewardStatus status)
	{
		if ((uint)(status - 1) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool HasUnclaimedTrackReward(RewardTrack.RewardStatus status)
	{
		return !HasClaimedRewardTrackReward(status);
	}

	public static int CountUnclaimedRewards(List<RewardTrackLevelDbfRecord> levels, int trackLevel, HashSet<RewardTrackPaidType> ownedPaidTypes, Func<int, PlayerRewardTrackLevelState> levelState)
	{
		List<RewardTrackLevelDbfRecord> nodes = new List<RewardTrackLevelDbfRecord>();
		foreach (RewardTrackLevelDbfRecord node in levels)
		{
			if (node.Level <= trackLevel)
			{
				nodes.Add(node);
			}
		}
		int count = 0;
		foreach (RewardTrackPaidType paidType in ownedPaidTypes)
		{
			count += CountUnclaimedRewards(nodes, levelState, paidType);
		}
		return count;
	}

	public static int CountUnclaimedRewards(List<RewardTrackLevelDbfRecord> nodes, Func<int, PlayerRewardTrackLevelState> levelState, RewardTrackPaidType paidType)
	{
		int count = 0;
		foreach (RewardTrackLevelDbfRecord node in nodes)
		{
			RewardListDbfRecord rewardListRecord = GetRewardListRecord(node, paidType);
			if (rewardListRecord != null && rewardListRecord.RewardItems?.Count > 0 && HasUnclaimedTrackReward(GetRewardStatus(levelState(node.Level), paidType)))
			{
				count++;
			}
		}
		return count;
	}
}
