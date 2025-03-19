using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using PegasusUtil;

namespace Hearthstone.Progression;

public static class RewardTrackFactory
{
	public static DataModelList<RewardTrackNodeDataModel> CreateRewardTrackNodeDataModelList(RewardTrackDbfRecord trackRecord, RewardTrackDataModel trackDataModel, Func<int, PlayerRewardTrackLevelState> levelState, int startLevel, int endLevel)
	{
		if (trackRecord == null || endLevel <= 0)
		{
			return null;
		}
		List<RewardTrackNodeDataModel> selectedTrackNodes = new List<RewardTrackNodeDataModel>();
		int levelSoftCap = trackDataModel.LevelSoftCap;
		int cumulativeXpNeeded = 0;
		List<RewardTrackLevelDbfRecord> trackRecordLevels = GameUtils.GetRewardTrackLevelsForRewardTrack(trackRecord.ID);
		foreach (RewardTrackLevelDbfRecord record in trackRecordLevels)
		{
			int xpBeforeThisLevel = cumulativeXpNeeded;
			cumulativeXpNeeded += record.XpNeeded;
			int recordLevel = record.Level;
			bool isLevelInRange = recordLevel >= startLevel && recordLevel <= endLevel;
			if (levelSoftCap <= 0)
			{
				if (isLevelInRange)
				{
					selectedTrackNodes.Add(CreateRewardTrackNodeDataModel(trackRecord, record, trackDataModel, levelState, xpBeforeThisLevel));
				}
				continue;
			}
			bool isLevelBelowCap = recordLevel < levelSoftCap;
			if (isLevelInRange && isLevelBelowCap)
			{
				selectedTrackNodes.Add(CreateRewardTrackNodeDataModel(trackRecord, record, trackDataModel, levelState, xpBeforeThisLevel));
				continue;
			}
			bool isLevelRangePastCap = endLevel > levelSoftCap;
			bool isPlayersNextLevel = recordLevel == trackDataModel.Level + 1;
			bool isSoftCap = recordLevel == levelSoftCap;
			bool isBonusLevel = ((trackDataModel.Level >= levelSoftCap) ? isPlayersNextLevel : isSoftCap);
			if (isLevelRangePastCap && isBonusLevel)
			{
				selectedTrackNodes.Add(CreateRewardTrackNodeDataModel(trackRecord, record, trackDataModel, levelState, xpBeforeThisLevel));
				continue;
			}
			bool isPlayersMaxLevel = trackDataModel.Level == trackRecordLevels.Count && trackDataModel.Level == recordLevel;
			if (isLevelRangePastCap && isPlayersMaxLevel)
			{
				selectedTrackNodes.Add(CreateRewardTrackNodeDataModel(trackRecord, record, trackDataModel, levelState, xpBeforeThisLevel));
			}
		}
		return selectedTrackNodes.OrderBy((RewardTrackNodeDataModel node) => node.Level).ToDataModelList();
	}

	public static RewardTrackNodeDataModel CreateRewardTrackNodeDataModel(RewardTrackDbfRecord trackRecord, RewardTrackLevelDbfRecord record, RewardTrackDataModel TrackDataModel, Func<int, PlayerRewardTrackLevelState> levelState, int cumulativeXpNeeded)
	{
		RewardTrackNodeRewardsDataModel freeRewards = CreateRewardTrackNodeRewardsDataModel(trackRecord, record.FreeRewardListRecord, TrackDataModel, RewardTrackPaidType.RTPT_FREE, levelState(record.Level), cumulativeXpNeeded);
		RewardTrackNodeRewardsDataModel paidRewards = CreateRewardTrackNodeRewardsDataModel(trackRecord, record.PaidRewardListRecord, TrackDataModel, RewardTrackPaidType.RTPT_PAID, levelState(record.Level), cumulativeXpNeeded);
		RewardTrackNodeRewardsDataModel paidPremiumRewards = CreateRewardTrackNodeRewardsDataModel(trackRecord, record.PaidPremiumRewardListRecord, TrackDataModel, RewardTrackPaidType.RTPT_PAID_PREMIUM, levelState(record.Level), cumulativeXpNeeded);
		RewardTrackNodeAdditionalRewardsDataModel additionalRewards = CreateRewardTrackNodeAdditionalRewardsDataModel(record.FreeRewardListRecord, TrackDataModel, levelState(record.Level), freeRewards);
		return new RewardTrackNodeDataModel
		{
			Level = record.Level,
			StyleName = record.StyleName,
			FreeRewards = freeRewards,
			PaidRewards = paidRewards,
			PaidPremiumRewards = paidPremiumRewards,
			AdditionalRewards = additionalRewards,
			CumulativeXpNeeded = cumulativeXpNeeded
		};
	}

	public static RewardTrackNodeRewardsDataModel CreateRewardTrackNodeRewardsDataModel(RewardTrackDbfRecord trackRecord, RewardListDbfRecord record, RewardTrackDataModel TrackDataModel, RewardTrackPaidType paidType, PlayerRewardTrackLevelState levelState, int cumulativeXpNeeded)
	{
		RewardListDataModel items = ((record != null) ? RewardUtils.CreateRewardListDataModelFromRewardListId(record.ID) : new RewardListDataModel());
		if (record != null && record.ChooseOne)
		{
			items.Items = items.Items.OrderBy((RewardItemDataModel item) => item, new RewardUtils.RewardOwnedItemComparer()).ToDataModelList();
		}
		RewardTrack.RewardStatus status = ProgressUtils.GetRewardStatus(levelState, paidType);
		bool num = ProgressUtils.HasOwnedRewardTrackPaidType(trackRecord, paidType);
		bool isLocked = record?.Locked ?? false;
		bool isClaimable = num && !isLocked && TrackDataModel.Level >= levelState.Level;
		bool num2 = TrackDataModel.LevelSoftCap > 0 && levelState.Level >= TrackDataModel.LevelSoftCap;
		string summary = null;
		if (!num2)
		{
			summary = (isClaimable ? ((!ProgressUtils.IsEventRewardTrackType(TrackDataModel.RewardTrackType)) ? GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_POPUP_COMPLETE_LEVEL", levelState.Level) : GameStrings.Format("GLUE_PROGRESSION_EVENT_TAB_REWARD_POPUP_COMPLETE_LEVEL", cumulativeXpNeeded)) : ((!ProgressUtils.IsEventRewardTrackType(TrackDataModel.RewardTrackType)) ? GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_POPUP_INCOMPLETE_LEVEL", levelState.Level) : GameStrings.Format("GLUE_PROGRESSION_EVENT_TAB_REWARD_POPUP_INCOMPLETE_LEVEL", cumulativeXpNeeded)));
		}
		else
		{
			RewardItemDataModel firstRewardItem = items.Items.FirstOrDefault();
			summary = GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_POPUP_BONUS_DESCRIPTION", firstRewardItem?.Quantity.ToString() ?? string.Empty, TrackDataModel.LevelSoftCap, TrackDataModel.LevelHardCap);
		}
		string passLabel = null;
		if (isLocked)
		{
			passLabel = "GLUE_PROGRESSION_REWARD_TRACK_LOCKED_LABEL";
		}
		else
		{
			switch (paidType)
			{
			case RewardTrackPaidType.RTPT_FREE:
				passLabel = "GLUE_PROGRESSION_REWARD_TRACK_FREE_LABEL";
				break;
			case RewardTrackPaidType.RTPT_PAID_PREMIUM:
				passLabel = "GLUE_PROGRESSION_REWARD_TRACK_PAID_PREMIUM_LABEL";
				break;
			}
		}
		return new RewardTrackNodeRewardsDataModel
		{
			Level = levelState.Level,
			Summary = summary,
			IsPremium = (paidType != RewardTrackPaidType.RTPT_FREE),
			PaidType = paidType,
			Items = items,
			IsClaimed = ProgressUtils.HasClaimedRewardTrackReward(status),
			IsClaimable = isClaimable,
			IsUnrevealedAndLocked = isLocked,
			RewardTrackType = (int)TrackDataModel.RewardTrackType,
			RewardTrackId = TrackDataModel.RewardTrackId,
			PassLabel = passLabel
		};
	}

	public static RewardListDataModel CreatePaidRewardListDataModel(RewardTrackDbfRecord record, int productId)
	{
		HashSet<RewardTrackPaidType> paidTypes = ProgressUtils.GetRewardTrackPaidTypesForProduct(record, productId);
		return new RewardListDataModel
		{
			Items = (from element in GameUtils.GetRewardTrackLevelsForRewardTrack(record.ID)
				where ProgressUtils.HasRewardLists(element, paidTypes)
				select element).SelectMany((RewardTrackLevelDbfRecord element) => ProgressUtils.GetRewardItemsForRewardList(element, paidTypes)).SelectMany((RewardItemDbfRecord element) => RewardFactory.CreateRewardItemDataModel(element)).Consolidate()
				.OrderBy((RewardItemDataModel item) => item, new RewardUtils.RewardItemComparer())
				.ToDataModelList()
		};
	}

	public static RewardTrackNodeAdditionalRewardsDataModel CreateRewardTrackNodeAdditionalRewardsDataModel(RewardListDbfRecord record, RewardTrackDataModel TrackDataModel, PlayerRewardTrackLevelState levelState, RewardTrackNodeRewardsDataModel freeRewardNode)
	{
		if (record == null)
		{
			return null;
		}
		List<RewardTrackNodeRewardsDataModel> rewardLists = new List<RewardTrackNodeRewardsDataModel>();
		foreach (RewardItemDbfRecord reward in GameUtils.GetRewardItemsForRewardList(record.ID))
		{
			if (RewardUtils.IsAdditionalRewardType(reward.RewardType))
			{
				List<RewardItemDataModel> singleItemList = RewardFactory.CreateRewardItemDataModel(reward);
				RewardListDataModel items = new RewardListDataModel
				{
					Items = singleItemList.ToDataModelList()
				};
				RewardTrackNodeRewardsDataModel additionalRewardList = new RewardTrackNodeRewardsDataModel
				{
					Level = levelState.Level,
					Summary = freeRewardNode.Summary,
					Items = items,
					IsClaimed = freeRewardNode.IsClaimed,
					IsClaimable = freeRewardNode.IsClaimable,
					RewardTrackType = (int)TrackDataModel.RewardTrackType,
					RewardTrackId = TrackDataModel.RewardTrackId,
					PaidType = RewardTrackPaidType.RTPT_FREE
				};
				rewardLists.Add(additionalRewardList);
			}
		}
		return new RewardTrackNodeAdditionalRewardsDataModel
		{
			RewardLists = rewardLists.ToDataModelList()
		};
	}

	public static int CalculateCumulativeXpForLevel(RewardTrackDbfRecord trackRecord, int level)
	{
		int cumulativeXpNeeded = 0;
		GameUtils.GetRewardTrackLevelsForRewardTrack(trackRecord.ID);
		foreach (RewardTrackLevelDbfRecord record in trackRecord.Levels)
		{
			if (record.Level >= level)
			{
				break;
			}
			cumulativeXpNeeded += record.XpNeeded;
		}
		return cumulativeXpNeeded;
	}

	public static RewardScrollDataModel CreateRewardScrollDataModel(int rewardListId, int level, string displayName, int chooseOneRewardItemId = 0, List<RewardItemOutput> rewardItemOutputs = null)
	{
		RewardListDbfRecord rewardListRecord = GameDbf.RewardList.GetRecord(rewardListId);
		RewardListDataModel rewardListDataModel = RewardUtils.CreateRewardListDataModelFromRewardListRecord(rewardListRecord, chooseOneRewardItemId, rewardItemOutputs);
		if (rewardListDataModel.Items.Count == 0)
		{
			return null;
		}
		string description2;
		if (rewardListRecord != null && rewardListRecord.ChooseOne)
		{
			List<RewardItemDbfRecord> rewardItemsForRewardList = GameUtils.GetRewardItemsForRewardList(rewardListRecord.ID);
			RewardItemDbfRecord rewardItemRecord = null;
			foreach (RewardItemDbfRecord rewardItem in rewardItemsForRewardList)
			{
				if (rewardItem != null && rewardItem.ID == chooseOneRewardItemId)
				{
					rewardItemRecord = rewardItem;
					break;
				}
			}
			if (rewardItemRecord == null || rewardItemRecord.Card == 0)
			{
				DbfLocValue description = rewardListRecord.Description;
				description2 = ((description != null) ? ((string)description) : string.Empty);
			}
			else
			{
				object obj = ((IList<RewardItemDataModel>)(rewardListDataModel?.Items))?[0]?.Card?.Name;
				if (obj == null)
				{
					DbfLocValue description = rewardListRecord.Description;
					obj = ((description != null) ? ((string)description) : string.Empty);
				}
				description2 = (string)obj;
			}
		}
		else
		{
			DbfLocValue description = rewardListRecord?.Description;
			description2 = ((description != null) ? ((string)description) : string.Empty);
		}
		return new RewardScrollDataModel
		{
			DisplayName = displayName,
			Description = description2,
			RewardList = rewardListDataModel
		};
	}

	public static RewardScrollDataModel CreateAdditionalRewardScrollDataModel(int rewardListId, int level, string displayName, int chooseOneRewardItemId = 0, List<RewardItemOutput> rewardItemOutputs = null)
	{
		RewardListDataModel rewardListDataModel = RewardUtils.CreateRewardListDataModelFromRewardListRecord(GameDbf.RewardList.GetRecord(rewardListId), chooseOneRewardItemId, rewardItemOutputs, includeAdditionalRewards: true);
		rewardListDataModel.Items = rewardListDataModel.Items.Where((RewardItemDataModel reward) => reward.ItemType != RewardItemType.HERO_CLASS).ToDataModelList();
		if (rewardListDataModel.Items.Count == 0)
		{
			return null;
		}
		string description = rewardListDataModel.Items[0].StandaloneDescription;
		return new RewardScrollDataModel
		{
			DisplayName = displayName,
			Description = description,
			RewardList = rewardListDataModel
		};
	}
}
