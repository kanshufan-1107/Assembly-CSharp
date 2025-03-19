using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Assets;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.Store;
using Hearthstone.UI;
using PegasusLettuce;
using PegasusShared;
using UnityEngine;

public class LettuceVillageDataUtil
{
	public class AdditionalTaskModelInfo
	{
		public bool ValidMercenaryID => MercenaryID != -1;

		public int MercenaryID { get; set; } = -1;

		public int BountyID { get; set; } = -1;

		public List<int> AdditionalMercenaryIDs { get; set; }
	}

	private static readonly AssetReference TASK_TOAST_PREFAB = new AssetReference("LettuceTaskItemToast.prefab:4bd9a9a0603657a4d8948dfae543dcd4");

	public static int CurrentTaskContext;

	public static int RecentlyClaimedTaskId = 0;

	public static bool ZoneWasRecentlyUnlocked = false;

	private static DateTime m_LastRefreshTime;

	private static int m_prevPackCount = 0;

	public static bool Initialized => NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageInfo>()?.Initialized ?? false;

	public static List<MercenariesVisitorState> VisitorStates
	{
		get
		{
			NetCache.NetCacheMercenariesVillageVisitorInfo villageInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageVisitorInfo>();
			if (villageInfo != null)
			{
				return villageInfo.VisitorStates;
			}
			return new List<MercenariesVisitorState>();
		}
	}

	public static List<MercenariesCompletedVisitorState> CompletedVisitorStates
	{
		get
		{
			NetCache.NetCacheMercenariesVillageVisitorInfo villageInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageVisitorInfo>();
			if (villageInfo != null)
			{
				return villageInfo.CompletedVisitorStates;
			}
			return new List<MercenariesCompletedVisitorState>();
		}
	}

	public static int[] VisitingMercenaries
	{
		get
		{
			NetCache.NetCacheMercenariesVillageVisitorInfo villageInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageVisitorInfo>();
			if (villageInfo != null)
			{
				return villageInfo.VisitingMercenaries;
			}
			return new int[0];
		}
	}

	public static List<MercenariesBuildingState> BuildingStates
	{
		get
		{
			NetCache.NetCacheMercenariesVillageInfo villageInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageInfo>();
			if (villageInfo != null)
			{
				return villageInfo.BuildingStates;
			}
			return new List<MercenariesBuildingState>();
		}
	}

	public static List<MercenariesRenownOfferData> ActiveRenownStates
	{
		get
		{
			NetCache.NetCacheMercenariesVillageVisitorInfo villageInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageVisitorInfo>();
			if (villageInfo != null)
			{
				return villageInfo.ActiveRenownOffers;
			}
			return new List<MercenariesRenownOfferData>();
		}
	}

	public static List<MercenariesTaskState> GetTaskStates()
	{
		NetCache.NetCacheMercenariesVillageVisitorInfo villageInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageVisitorInfo>();
		List<MercenariesTaskState> visitorTasks = new List<MercenariesTaskState>();
		if (villageInfo != null)
		{
			foreach (MercenariesVisitorState visitorState in villageInfo.VisitorStates)
			{
				visitorTasks.Add(visitorState.ActiveTaskState);
			}
		}
		return visitorTasks;
	}

	public static void InitializeData()
	{
		NetCache.NetCacheMercenariesVillageInfo villageInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageInfo>();
		if (villageInfo == null || !villageInfo.Initialized)
		{
			Network.Get().MercenariesVillageStatusRequest();
		}
	}

	public static void RefreshDataIfNecessary()
	{
		if (DateTime.UtcNow.CompareTo(m_LastRefreshTime.AddSeconds(60.0)) >= 1)
		{
			RefreshData();
		}
	}

	public static void RefreshData()
	{
		Network.Get().MercenariesVillageRefreshRequest();
		m_LastRefreshTime = DateTime.UtcNow;
	}

	public static VisitorTaskDbfRecord GetTaskRecordByID(int taskId)
	{
		return GameDbf.VisitorTask.GetRecord(taskId);
	}

	public static MercenariesTaskState GetTaskStateByID(int taskId)
	{
		foreach (MercenariesTaskState state in GetTaskStates())
		{
			if (state.TaskId == taskId)
			{
				return state;
			}
		}
		return null;
	}

	public static MercenaryVisitorDbfRecord GetVisitorRecordByID(int visitorId)
	{
		return GameDbf.MercenaryVisitor.GetRecord(visitorId);
	}

	public static MercenariesVisitorState GetVisitorStateByID(int visitorId)
	{
		foreach (MercenariesVisitorState state in VisitorStates)
		{
			if (state.VisitorId == visitorId)
			{
				return state;
			}
		}
		return null;
	}

	public static MercenaryVisitorDbfRecord GetVisitorDbfRecordByMercenaryId(int mercenaryId)
	{
		return GameDbf.MercenaryVisitor.GetRecord((MercenaryVisitorDbfRecord r) => r.MercenaryId == mercenaryId);
	}

	public static VisitorTaskChainDbfRecord GetVisitorTaskChainByID(int taskChainId)
	{
		return GameDbf.VisitorTaskChain.GetRecord(taskChainId);
	}

	public static VisitorTaskChainDbfRecord GetCurrentTaskChainByVisitorState(MercenariesVisitorState visitorState)
	{
		if (visitorState == null)
		{
			return null;
		}
		if (visitorState.ActiveTaskState == null)
		{
			return null;
		}
		return GetVisitorTaskChain(GetVisitorRecordByID(visitorState.VisitorId));
	}

	public static VisitorTaskChainDbfRecord GetVisitorTaskChain(MercenaryVisitorDbfRecord visitorRecord)
	{
		if (visitorRecord == null || visitorRecord.VisitorTaskChains == null || visitorRecord.VisitorTaskChains.Count == 0)
		{
			return null;
		}
		return visitorRecord.VisitorTaskChains[visitorRecord.VisitorTaskChains.Count - 1];
	}

	public static bool IsBountyTutorial(LettuceBountyDbfRecord bountyRecord)
	{
		if (bountyRecord == null)
		{
			return false;
		}
		if (bountyRecord.BountySetRecord == null)
		{
			return false;
		}
		return bountyRecord.BountySetRecord.IsTutorial;
	}

	public static bool TryGetBountyBossData(LettuceBountyDbfRecord bountyRecord, out string bossName, out List<string> bossCards)
	{
		bossCards = new List<string>();
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo != null && playerInfo.BountyInfoMap.TryGetValue(bountyRecord.ID, out var bountyInfo) && bountyInfo.BossCardIds != null)
		{
			foreach (uint cardId in bountyInfo.BossCardIds)
			{
				bossCards.Add(GameUtils.TranslateDbIdToCardId((int)cardId));
			}
		}
		if (bossCards.Count == 0)
		{
			bossCards.Add(GameUtils.TranslateDbIdToCardId(bountyRecord.FinalBossCardId));
		}
		List<string> bossNames = new List<string>();
		foreach (string bossCard in bossCards)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(bossCard);
			bossNames.Add(entityDef.GetName());
		}
		bossName = string.Empty;
		if (!string.IsNullOrWhiteSpace(bountyRecord.BossNameOverride))
		{
			bossName = bountyRecord.BossNameOverride.GetString();
		}
		else
		{
			switch (bossNames.Count)
			{
			case 1:
				bossName = bossNames[0];
				break;
			case 2:
				bossName = GameStrings.Format("GLUE_LETTUCE_BOUNTY_DOUBLE_BOSS_NAME", bossNames[0], bossNames[1]);
				break;
			}
		}
		if (string.IsNullOrEmpty(bossName))
		{
			Log.Lettuce.PrintError($"Unable to find valid boss data for bounty {bountyRecord.ID}");
			return false;
		}
		return true;
	}

	public static List<int> GetBountyBossIds(LettuceBountyDbfRecord bountyRecord, NetCache.NetCacheMercenariesPlayerInfo.BountyInfo bountyInfo = null)
	{
		if (bountyInfo == null)
		{
			NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
			if (playerInfo != null)
			{
				bountyInfo = (playerInfo.BountyInfoMap.ContainsKey(bountyRecord.ID) ? playerInfo.BountyInfoMap[bountyRecord.ID] : null);
			}
		}
		List<int> finalBossCardIds = new List<int>();
		if (bountyInfo != null && bountyInfo.BossCardIds != null && bountyInfo.BossCardIds.Count > 0)
		{
			foreach (uint bossCard in bountyInfo.BossCardIds)
			{
				finalBossCardIds.Add((int)bossCard);
			}
		}
		else
		{
			finalBossCardIds.Add(bountyRecord.FinalBossCardId);
		}
		finalBossCardIds.Sort();
		return finalBossCardIds;
	}

	public static List<RewardItemDataModel> GetFinalBossRewards(LettuceBountyDbfRecord bountyRecord, out string bossRewardDescription, out string additionRewardDescription)
	{
		List<RewardItemDataModel> rewards = new List<RewardItemDataModel>();
		additionRewardDescription = string.Empty;
		if (bountyRecord.FinalBossRewards.Count > 0 && bountyRecord.FinalBossRepresentiveRewards.Count == 0)
		{
			bossRewardDescription = "GLUE_LETTUCE_PVP_REWARD_POP_UP_TITLE";
		}
		else if (bountyRecord.DifficultyMode == LettuceBounty.MercenariesBountyDifficulty.MYTHIC)
		{
			bossRewardDescription = "GLUE_LETTUCE_PVP_REWARD_POP_UP_GENERIC_TITLE";
			additionRewardDescription = "GLUE_LETTUCE_REWARD_ADDITIONAL_RENOWN";
		}
		else
		{
			bossRewardDescription = "GLUE_LETTUCE_PVP_REWARD_POP_UP_GENERIC_TITLE";
		}
		foreach (LettuceBountyFinalRewardsDbfRecord reward in bountyRecord.FinalBossRewards)
		{
			string cardId = GameUtils.GetCardIdFromMercenaryId(reward.RewardMercenaryId);
			EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
			RewardItemDataModel rewardItem = new RewardItemDataModel
			{
				ItemType = RewardItemType.MERCENARY_COIN,
				MercenaryCoin = new LettuceMercenaryCoinDataModel
				{
					MercenaryId = reward.RewardMercenaryId,
					MercenaryName = entityDef.GetName(),
					Quantity = 0,
					GlowActive = false,
					NameActive = true
				}
			};
			rewards.Add(rewardItem);
		}
		foreach (LettuceBountyFinalRespresentiveRewardsDbfRecord finalBossRepresentiveReward in bountyRecord.FinalBossRepresentiveRewards)
		{
			RewardItemDataModel rewardItem2 = null;
			if (finalBossRepresentiveReward.RewardType == LettuceBountyFinalRespresentiveRewards.RewardType.RENOWN)
			{
				rewardItem2 = new RewardItemDataModel
				{
					ItemType = RewardItemType.MERCENARY_RENOWN,
					Currency = new PriceDataModel
					{
						Currency = CurrencyType.RENOWN
					}
				};
			}
			if (rewardItem2 != null)
			{
				rewards.Add(rewardItem2);
			}
		}
		return rewards;
	}

	public static string GenerateBountyName(LettuceBountyDbfRecord bountyRecord, bool includeZoneName = true, bool includeDifficulty = true)
	{
		if (bountyRecord == null)
		{
			return "";
		}
		string bountyName = null;
		if (bountyRecord.BountyNameOverride != null)
		{
			bountyName = bountyRecord.BountyNameOverride.GetString();
		}
		List<string> bossCards;
		if (string.IsNullOrEmpty(bountyName) && includeZoneName)
		{
			TryGetBountyBossData(bountyRecord, out var bossName, out bossCards);
			LettuceBountySetDbfRecord bountySetRecord = bountyRecord.BountySetRecord;
			if (bountySetRecord != null && bountySetRecord.Name != null)
			{
				bountyName = GameStrings.Format("GLUE_LETTUCE_BOUNTY_BOARD_BOUNTY_TITLE", bossName, bountyRecord.BountySetRecord.Name.GetString());
			}
		}
		if (string.IsNullOrEmpty(bountyName))
		{
			TryGetBountyBossData(bountyRecord, out bountyName, out bossCards);
		}
		if (includeDifficulty && !string.IsNullOrEmpty(bountyName) && bountyRecord.Heroic)
		{
			bountyName = GameStrings.Format("GLUE_LETTUCE_BOUNTY_BOARD_BOUNTY_TITLE_HEROIC_ADD", bountyName);
		}
		return bountyName;
	}

	public static string GeneratePosterName(LettuceBountyDbfRecord bountyRecord)
	{
		if (!string.IsNullOrEmpty(bountyRecord.PosterNameOverride))
		{
			return bountyRecord.PosterNameOverride;
		}
		return GameStrings.Format("GLUE_LETTUCE_BOUNTY_POSTER_TEXT", bountyRecord.BountyLevel);
	}

	public static void ApplyMercenaryBossCoinMaterials(AdventureMissionDataModel dataModel, IEnumerable<int> bossCardIds, List<DefLoader.DisposableCardDef> disposableDefList, string fallbackAssetId = "LOE_08CoinPortrait.mat:b5cdfac2e9672f9479083d73014858c6")
	{
		List<Material> bossCoinMaterials = new List<Material>();
		foreach (int bossCardId in bossCardIds)
		{
			Material bossCoinMaterial = null;
			if (bossCardId != 0)
			{
				DefLoader.Get().LoadCardDef(GameUtils.TranslateDbIdToCardId(bossCardId), delegate(string cardId, DefLoader.DisposableCardDef def, object userData)
				{
					if (def != null)
					{
						bossCoinMaterial = def.CardDef.m_MercenaryMapBossCoinPortrait;
						disposableDefList.Add(def);
					}
				});
				if (bossCoinMaterial == null)
				{
					bossCoinMaterial = AssetLoader.Get().LoadMaterial(fallbackAssetId);
				}
			}
			bossCoinMaterials.Add(bossCoinMaterial);
		}
		if (bossCoinMaterials.Count > 2)
		{
			Log.Lettuce.PrintError("Cannot handle more than 2 materials with boss coin.");
		}
		dataModel.CoinPortraitMaterial = ((bossCoinMaterials.Count > 0) ? bossCoinMaterials[0] : null);
		dataModel.SecondaryCoinPortraitMaterial = ((bossCoinMaterials.Count > 1) ? bossCoinMaterials[1] : null);
		dataModel.CoinPortraitMaterialCount = bossCoinMaterials.Count;
	}

	public static bool BuildingIsBuilt(MercenariesBuildingState bldg)
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageInfo>().BuildingIsBuilt(bldg);
	}

	public static List<int> GetNextTierListByTierId(int tierId)
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageInfo>().GetNextTierListByTierId(tierId);
	}

	public static BuildingTierDbfRecord GetTierRecordByTierId(int tierId)
	{
		return GameDbf.BuildingTier.GetRecord(tierId);
	}

	public static BuildingTierDbfRecord GetTierRecordByTierIndex(MercenaryBuilding.Mercenarybuildingtype buildingType, int tierIndex)
	{
		MercenaryBuildingDbfRecord buildingRecord = GetBuildingRecordByType(buildingType);
		if (buildingRecord == null)
		{
			return null;
		}
		if (tierIndex < buildingRecord.MercenaryBuildingTiers.Count)
		{
			return buildingRecord.MercenaryBuildingTiers[tierIndex];
		}
		return null;
	}

	public static MercenaryBuildingDbfRecord GetBuildingRecordByType(MercenaryBuilding.Mercenarybuildingtype buildingType)
	{
		return GameDbf.MercenaryBuilding.GetRecord((MercenaryBuildingDbfRecord r) => r.MercenaryBuildingType == buildingType);
	}

	public static MercenaryBuildingDbfRecord GetBuildingRecordByID(int buildingID)
	{
		return GameDbf.MercenaryBuilding.GetRecord((MercenaryBuildingDbfRecord r) => r.ID == buildingID);
	}

	public static MercenariesBuildingState GetBuildingStateByID(int buildingId)
	{
		foreach (MercenariesBuildingState state in BuildingStates)
		{
			if (state.BuildingId == buildingId)
			{
				return state;
			}
		}
		return null;
	}

	public static BuildingTierDbfRecord GetCurrentTierRecordFromBuilding(MercenaryBuilding.Mercenarybuildingtype buildingType)
	{
		MercenaryBuildingDbfRecord buildingRecord = GetBuildingRecordByType(buildingType);
		if (buildingRecord != null)
		{
			MercenariesBuildingState buildingState = GetBuildingStateByID(buildingRecord.ID);
			if (buildingState != null)
			{
				foreach (BuildingTierDbfRecord tier in buildingRecord.MercenaryBuildingTiers)
				{
					if (tier.ID == buildingState.CurrentTierId)
					{
						return tier;
					}
				}
			}
		}
		return null;
	}

	public static List<BuildingTierDbfRecord> GetTierRecordsThatCanBeBuilt()
	{
		List<BuildingTierDbfRecord> UpcomingTiers = new List<BuildingTierDbfRecord>();
		foreach (MercenaryBuilding.Mercenarybuildingtype value in Enum.GetValues(typeof(MercenaryBuilding.Mercenarybuildingtype)))
		{
			MercenaryBuildingDbfRecord buildingRecord = GetBuildingRecordByType(value);
			if (buildingRecord == null)
			{
				continue;
			}
			MercenariesBuildingState buildingState = GetBuildingStateByID(buildingRecord.ID);
			if (buildingState == null)
			{
				continue;
			}
			List<BuildingTierDbfRecord> mercenaryBuildingTiers = buildingRecord.MercenaryBuildingTiers;
			int currentStateIndex = 1000;
			int index = 0;
			foreach (BuildingTierDbfRecord tier in mercenaryBuildingTiers)
			{
				if (index > currentStateIndex)
				{
					UpcomingTiers.Add(tier);
				}
				if (tier.ID == buildingState.CurrentTierId)
				{
					currentStateIndex = index;
				}
				index++;
			}
		}
		return UpcomingTiers;
	}

	public static bool IsBuildingReadyToUpgrade(MercenaryBuilding.Mercenarybuildingtype buildingType, int targetTierID = 0)
	{
		MercenaryBuildingDbfRecord buildingRecord = GetBuildingRecordByType(buildingType);
		if (buildingRecord == null)
		{
			return false;
		}
		MercenariesBuildingState buildingState = GetBuildingStateByID(buildingRecord.ID);
		if (buildingState == null)
		{
			return false;
		}
		BuildingTierDbfRecord nextTierRecord = GetNextTierRecord(GetTierRecordByTierId(buildingState.CurrentTierId));
		if (nextTierRecord != null && IsBuildingTierAchievementComplete(nextTierRecord.ID) && (targetTierID == 0 || nextTierRecord.ID == targetTierID) && NetCache.Get().GetGoldBalance() >= nextTierRecord.UpgradeCost)
		{
			return true;
		}
		return false;
	}

	public static int GetCurrentTierPropertyForBuilding(MercenaryBuilding.Mercenarybuildingtype buildingType, TierProperties.Buildingtierproperty tierProperty, BuildingTierDbfRecord buildingTierRecord = null)
	{
		if (buildingTierRecord == null)
		{
			buildingTierRecord = GetCurrentTierRecordFromBuilding(buildingType);
		}
		if (buildingTierRecord != null)
		{
			foreach (TierPropertiesDbfRecord property in buildingTierRecord.MercenaryBuildingTierProperties)
			{
				if (property.TierPropertyType == tierProperty)
				{
					return property.TierPropertyValue;
				}
			}
		}
		return 0;
	}

	public static BuildingTierDbfRecord GetNextTierRecord(BuildingTierDbfRecord currentTierRecord)
	{
		if (currentTierRecord == null)
		{
			return null;
		}
		BuildingTierDbfRecord result = null;
		List<int> nextTiers = GetNextTierListByTierId(currentTierRecord.ID);
		if (nextTiers.Count != 0)
		{
			result = GetTierRecordByTierId(nextTiers[0]);
			if (IsBuildingTierUpgradeDisabled(result.ID))
			{
				return null;
			}
		}
		return result;
	}

	public static bool TryGetRenownConversionRate(TAG_RARITY rarity, out int conversionRate)
	{
		conversionRate = 0;
		return NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesVillageInfo>()?.TryGetRenownRate(rarity, out conversionRate) ?? false;
	}

	public static string FormatTaskStringForProceduralData(string taskString, int mercenaryId, int bountyId, List<int> additionalMercenaryIds)
	{
		if (string.IsNullOrEmpty(taskString))
		{
			return string.Empty;
		}
		return FormatTaskStringForBounty(FormatTaskStringForMercenaries(taskString, mercenaryId, additionalMercenaryIds), bountyId);
	}

	public static string FormatTaskStringForMercenaries(string taskString, int mercenaryId, List<int> additionalMercenaryIds)
	{
		LettuceMercenaryDbfRecord mercenary = GameDbf.LettuceMercenary.GetRecord(mercenaryId);
		if (mercenary == null)
		{
			return string.Empty;
		}
		string result = SetMercenaryNamesInTaskString("$owner_merc", taskString, new List<LettuceMercenaryDbfRecord> { mercenary }, (LettuceMercenaryDbfRecord merc, CardDbfRecord card) => string.IsNullOrWhiteSpace(card.ShortName) ? card.Name : card.ShortName);
		List<LettuceMercenaryDbfRecord> additionalMercenaries = new List<LettuceMercenaryDbfRecord>();
		if (additionalMercenaryIds != null)
		{
			foreach (int id in additionalMercenaryIds)
			{
				LettuceMercenaryDbfRecord record = GameDbf.LettuceMercenary.GetRecord(id);
				if (record != null)
				{
					additionalMercenaries.Add(record);
				}
			}
		}
		return SetMercenaryNamesInTaskString("$additional_mercs", result, additionalMercenaries, delegate(LettuceMercenaryDbfRecord merc, CardDbfRecord card)
		{
			string text = null;
			if (!string.IsNullOrWhiteSpace(card.ShortName))
			{
				text = card.ShortName;
			}
			if (text == null || text.Length == 0)
			{
				text = card.Name;
			}
			return text;
		});
	}

	public static string FormatTaskStringForBounty(string taskString, int bountyId)
	{
		LettuceBountyDbfRecord bountyRecord = GameDbf.LettuceBounty.GetRecord(bountyId);
		if (bountyRecord == null)
		{
			return taskString;
		}
		string result = SetBountyNameInTaskString("$bounty_nz", taskString, bountyRecord, includeZoneName: true, includeDifficulty: false);
		result = SetBountyNameInTaskString("$bounty_nd", result, bountyRecord, includeZoneName: false, includeDifficulty: true);
		result = SetBountyNameInTaskString("$bounty_n", result, bountyRecord, includeZoneName: false, includeDifficulty: false);
		LettuceBountySetDbfRecord bountySetRecord = bountyRecord.BountySetRecord;
		if (bountySetRecord != null)
		{
			result = SetBountySetInTaskString("$bounty_set", result, bountySetRecord);
		}
		result = SetBountyDifficultyInTaskString("$bounty_diff", result, bountyRecord);
		return SetBountyNameInTaskString("$bounty", result, bountyRecord, includeZoneName: true, includeDifficulty: true);
	}

	public static string FormatTaskDescriptionForAbility(string taskDescription, int quota, int mercenaryId)
	{
		if (string.IsNullOrEmpty(taskDescription))
		{
			return string.Empty;
		}
		return ProgressUtils.FormatDescription(SetEquipmentNameFromTaskDescription(SetAbilityNameFromTaskDescription(taskDescription, mercenaryId), mercenaryId), quota);
	}

	public static IEnumerator ShowTaskToast(List<MercenariesTaskState> CompletedTasks, bool useGeneric = false)
	{
		int maxDisplayed;
		Vector3 toastOffset;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			maxDisplayed = 2;
			toastOffset = LettuceVillageTaskToast.TASK_TOAST_OFFSET_PHONE;
		}
		else
		{
			maxDisplayed = 3;
			toastOffset = LettuceVillageTaskToast.TASK_TOAST_OFFSET;
		}
		for (int i = 0; i < CompletedTasks.Count; i++)
		{
			MercenaryVillageTaskItemDataModel model = null;
			if (useGeneric)
			{
				model = Dev_CreateGenericTaskModel(CompletedTasks[i].TaskId, CompletedTasks[i].Progress);
			}
			else
			{
				model = CreateTaskModelFromTaskState(CompletedTasks[i]);
			}
			if (model != null)
			{
				WidgetInstance taskItemWidget = WidgetInstance.Create(TASK_TOAST_PREFAB);
				Vector3 screenPosition = i % maxDisplayed * toastOffset;
				taskItemWidget.gameObject.transform.position = screenPosition;
				taskItemWidget.RegisterReadyListener(delegate
				{
					LettuceVillageTaskToast componentInChildren = taskItemWidget.GetComponentInChildren<LettuceVillageTaskToast>();
					componentInChildren.Initialize(model);
					componentInChildren.Show();
				});
			}
			if ((i + 1) % maxDisplayed == 0 || i == CompletedTasks.Count - 1)
			{
				yield return new WaitForSeconds(5f);
			}
		}
	}

	private static string SetMercenaryNamesInTaskString(string command, string taskDescription, List<LettuceMercenaryDbfRecord> mercenaries, Func<LettuceMercenaryDbfRecord, CardDbfRecord, string> getNameFromRecordFn)
	{
		Func<string> detailsFn = delegate
		{
			List<string> list = new List<string>();
			foreach (LettuceMercenaryDbfRecord current in mercenaries)
			{
				if (current.MercenaryArtVariations.Count != 0)
				{
					string item = getNameFromRecordFn(current, current.MercenaryArtVariations[0].CardRecord);
					list.Add(item);
				}
			}
			string key = $"GLUE_MERCENARIES_TASKBOARD_PROC_TASK_MERCENARY_LIST_{list.Count}";
			object[] args = list.ToArray();
			return GameStrings.Format(key, args);
		};
		return ReplaceCommandWithDetails(command, taskDescription, detailsFn);
	}

	private static string SetBountyNameInTaskString(string command, string taskDescription, LettuceBountyDbfRecord bountyRecord, bool includeZoneName, bool includeDifficulty)
	{
		return ReplaceCommandWithDetails(command, taskDescription, () => GenerateBountyName(bountyRecord, includeZoneName, includeDifficulty));
	}

	private static string SetBountySetInTaskString(string command, string taskDescription, LettuceBountySetDbfRecord bountySetRecord)
	{
		return ReplaceCommandWithDetails(command, taskDescription, () => bountySetRecord.Name);
	}

	private static string SetBountyDifficultyInTaskString(string command, string taskDescription, LettuceBountyDbfRecord bountyRecord)
	{
		Func<string> detailsFn = delegate
		{
			if (bountyRecord.DifficultyMode == LettuceBounty.MercenariesBountyDifficulty.HEROIC || bountyRecord.Heroic)
			{
				return GameStrings.Get("GLUE_MERCENARIES_TASKBOARD_PROC_TASK_HEROIC_DIFFICULTY");
			}
			return (bountyRecord.DifficultyMode == LettuceBounty.MercenariesBountyDifficulty.NORMAL) ? GameStrings.Get("GLUE_MERCENARIES_TASKBOARD_PROC_TASK_NORMAL_DIFFICULTY") : "";
		};
		return ReplaceCommandWithDetails(command, taskDescription, detailsFn);
	}

	private static string ReplaceCommandWithDetails(string command, string taskDescription, Func<string> detailsFn)
	{
		return Regex.Replace(taskDescription, "\\" + command, detailsFn());
	}

	private static string SetAbilityNameFromTaskDescription(string taskDescription, int mercenaryId)
	{
		int startPos = taskDescription.IndexOf("$ability(");
		if (startPos == -1)
		{
			return taskDescription;
		}
		startPos += "$ability(".Length;
		int endPos = taskDescription.IndexOf(")", startPos);
		if (endPos == -1)
		{
			Debug.LogError("Incorrect format for $ability command for " + taskDescription);
			return taskDescription;
		}
		string[] abilityParams = taskDescription.Substring(startPos, endPos - startPos).Split(',');
		int specializationSlot = 0;
		int abilitySlot = 0;
		if (!int.TryParse(abilityParams[0], out specializationSlot) || !int.TryParse(abilityParams[1], out abilitySlot))
		{
			Debug.LogError("Error in params for task description for" + taskDescription);
			return taskDescription;
		}
		LettuceMercenaryDbfRecord mercRecord = GameDbf.LettuceMercenary.GetRecord(mercenaryId);
		if (specializationSlot >= mercRecord.LettuceMercenarySpecializations.Count)
		{
			Debug.LogError("Error in params,invalid specialization slot in task description, received " + specializationSlot);
			return taskDescription;
		}
		LettuceMercenarySpecializationDbfRecord specialization = mercRecord.LettuceMercenarySpecializations[specializationSlot];
		if (abilitySlot >= specialization.LettuceMercenaryAbilities.Count)
		{
			Debug.LogError("Error in params,invalid ability slot in task description, received " + abilitySlot);
			return taskDescription;
		}
		int abilityId = specialization.LettuceMercenaryAbilities[abilitySlot].LettuceAbilityId;
		LettuceAbilityDbfRecord ability = GameDbf.LettuceAbility.GetRecord(abilityId);
		return Regex.Replace(taskDescription, "\\$ability(.*?)\\)", ability.AbilityName, RegexOptions.IgnoreCase);
	}

	private static string SetEquipmentNameFromTaskDescription(string taskDescription, int mercenaryId)
	{
		int startPos = taskDescription.IndexOf("$equipment(");
		if (startPos == -1)
		{
			return taskDescription;
		}
		startPos += "$equipment(".Length;
		int endPos = taskDescription.IndexOf(")", startPos);
		if (endPos == -1)
		{
			Debug.LogError("Incorrect format for $equipment command for " + taskDescription);
			return taskDescription;
		}
		string[] abilityParams = taskDescription.Substring(startPos, endPos - startPos).Split(',');
		int equipmentSlot = 0;
		int equipmentTierSlot = 0;
		if (!int.TryParse(abilityParams[0], out equipmentSlot) || !int.TryParse(abilityParams[1], out equipmentTierSlot))
		{
			Debug.LogError("Error in params for task description for" + taskDescription);
			return taskDescription;
		}
		LettuceMercenaryDbfRecord mercRecord = GameDbf.LettuceMercenary.GetRecord(mercenaryId);
		if (equipmentSlot >= mercRecord.LettuceMercenaryEquipment.Count)
		{
			Debug.LogError("Error in params,invalid equipment slot in task description, received " + equipmentSlot);
			return taskDescription;
		}
		LettuceMercenaryEquipmentDbfRecord equipment = mercRecord.LettuceMercenaryEquipment[equipmentSlot];
		if (equipmentTierSlot >= equipment.LettuceEquipmentRecord.LettuceEquipmentTiers.Count)
		{
			Debug.LogError("Error in params,invalid equipment tier slot in task description, received " + equipmentTierSlot);
			return taskDescription;
		}
		int abilityCardId = equipment.LettuceEquipmentRecord.LettuceEquipmentTiers[equipmentTierSlot].CardId;
		CardDbfRecord equipmentCard = GameDbf.Card.GetRecord(abilityCardId);
		return Regex.Replace(taskDescription, "\\$equipment(.*?)\\)", equipmentCard.Name, RegexOptions.IgnoreCase);
	}

	public static List<MercenaryBuilding.Mercenarybuildingtype> GetAvailableBuildingsForCurrentFTUEState(bool isUIContext = false)
	{
		List<MercenaryBuilding.Mercenarybuildingtype> availableBuildings;
		if (GameUtils.IsMercenariesVillageTutorialComplete())
		{
			availableBuildings = new List<MercenaryBuilding.Mercenarybuildingtype>
			{
				MercenaryBuilding.Mercenarybuildingtype.PVEZONES,
				MercenaryBuilding.Mercenarybuildingtype.PVP,
				MercenaryBuilding.Mercenarybuildingtype.COLLECTION,
				MercenaryBuilding.Mercenarybuildingtype.SHOP,
				MercenaryBuilding.Mercenarybuildingtype.MAILBOX,
				MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL,
				MercenaryBuilding.Mercenarybuildingtype.TASKBOARD,
				MercenaryBuilding.Mercenarybuildingtype.BUILDINGMANAGER
			};
		}
		else
		{
			availableBuildings = new List<MercenaryBuilding.Mercenarybuildingtype>();
			availableBuildings.Add(MercenaryBuilding.Mercenarybuildingtype.COLLECTION);
			if (!isUIContext)
			{
				availableBuildings.Add(MercenaryBuilding.Mercenarybuildingtype.BUILDINGMANAGER);
			}
			if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_UPGRADE_ABILITY_END))
			{
				availableBuildings.Add(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD);
			}
			if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_TASK_BOARD_END))
			{
				availableBuildings.Add(MercenaryBuilding.Mercenarybuildingtype.PVEZONES);
			}
			if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_SHOP_BUILD_START))
			{
				availableBuildings.Add(MercenaryBuilding.Mercenarybuildingtype.SHOP);
			}
			if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_SHOP_BUILD_END))
			{
				availableBuildings.Add(MercenaryBuilding.Mercenarybuildingtype.MAILBOX);
			}
		}
		return availableBuildings;
	}

	public static bool IsBuildingAvailableInTutorial(int buildingId, List<MercenaryBuilding.Mercenarybuildingtype> availableBuildings)
	{
		MercenaryBuildingDbfRecord buildingRecord = GetBuildingRecordByID(buildingId);
		if (buildingRecord != null)
		{
			return availableBuildings.Contains(buildingRecord.MercenaryBuildingType);
		}
		return false;
	}

	public static bool IsBuildingTierAchievementComplete(int nextTierId)
	{
		BuildingTierDbfRecord record = GetTierRecordByTierId(nextTierId);
		if (record == null)
		{
			Debug.LogError("LettuceVillageDataUtil.IsBuildingAchievementComplete: No record exists for the CurrentTierId.");
			return false;
		}
		if (record.UnlockAchievement == 0)
		{
			return true;
		}
		AchievementDataModel achievement = AchievementManager.Get().GetAchievementDataModel(record.UnlockAchievement);
		if (achievement == null)
		{
			Debug.LogError("LettuceVillageDataUtil.IsBuildingAchievementComplete: No record exists for this achievement.");
			return false;
		}
		AchievementManager.AchievementStatus status = achievement.Status;
		if ((uint)(status - 2) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static List<MercenariesVisitorState> GetVisitorsByType(MercenaryVisitor.VillageVisitorType visitorType)
	{
		List<MercenariesVisitorState> visitors = new List<MercenariesVisitorState>();
		foreach (MercenariesVisitorState state in VisitorStates)
		{
			MercenaryVisitorDbfRecord record = GetVisitorRecordByID(state.VisitorId);
			if (record != null && record.VisitorType == visitorType)
			{
				visitors.Add(state);
			}
		}
		return visitors;
	}

	public static List<MercenariesCompletedVisitorState> GetCompletedVisitorsByType(MercenaryVisitor.VillageVisitorType visitorType)
	{
		List<MercenariesCompletedVisitorState> visitors = new List<MercenariesCompletedVisitorState>();
		foreach (MercenariesCompletedVisitorState state in CompletedVisitorStates)
		{
			MercenaryVisitorDbfRecord record = GetVisitorRecordByID(state.VisitorId);
			if (record != null && record.VisitorType == visitorType)
			{
				visitors.Add(state);
			}
		}
		return visitors;
	}

	public static int GetNumberOfMercPacksToOpen()
	{
		int currentPackCount = 0;
		if (LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_SHOP_CLAIM_PACK_POPUP))
		{
			currentPackCount = BoosterPackUtils.GetBoosterCount(629);
		}
		return currentPackCount;
	}

	public static bool DidPackCountChangeFromZero(int newCount)
	{
		if (m_prevPackCount == 0)
		{
			return newCount > 0;
		}
		return false;
	}

	public static void UpdatePrevPackCount(int newCount)
	{
		m_prevPackCount = newCount;
	}

	public static bool IsDifficultyUnlocked(LettuceBounty.MercenariesBountyDifficulty difficulty)
	{
		NetCache netCache = NetCache.Get();
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (difficulty == LettuceBounty.MercenariesBountyDifficulty.MYTHIC && !features.Games.MercenariesMythic)
		{
			return false;
		}
		NetCache.NetCacheMercenariesVillageInfo villageInfo = netCache.GetNetObject<NetCache.NetCacheMercenariesVillageInfo>();
		if (villageInfo == null)
		{
			return false;
		}
		return villageInfo.UnlockedBountyDifficultyLevel >= (int)difficulty;
	}

	public static GameSaveKeySubkeyId GetNotificationSubkeyIdForBuilding(MercenaryBuilding.Mercenarybuildingtype buildingType)
	{
		GameSaveKeySubkeyId subkeyId = GameSaveKeySubkeyId.INVALID;
		switch (buildingType)
		{
		case MercenaryBuilding.Mercenarybuildingtype.BUILDINGMANAGER:
			subkeyId = GameSaveKeySubkeyId.MERCENARIES_SHOULD_SEE_WORKSHOP_NOTIFICATION;
			break;
		case MercenaryBuilding.Mercenarybuildingtype.TASKBOARD:
			subkeyId = GameSaveKeySubkeyId.MERCENARIES_SHOULD_SEE_TASKBOARD_NOTIFICATION;
			break;
		case MercenaryBuilding.Mercenarybuildingtype.PVEZONES:
			subkeyId = GameSaveKeySubkeyId.MERCENARIES_SHOULD_SEE_PVEZONES_NOTIFICATION;
			break;
		case MercenaryBuilding.Mercenarybuildingtype.PVP:
			subkeyId = GameSaveKeySubkeyId.MERCENARIES_SHOULD_SEE_ARENA_NOTIFICATION;
			break;
		case MercenaryBuilding.Mercenarybuildingtype.SHOP:
			subkeyId = GameSaveKeySubkeyId.MERCENARIES_SHOULD_SEE_SHOP_NOTIFICATION;
			break;
		case MercenaryBuilding.Mercenarybuildingtype.COLLECTION:
			subkeyId = GameSaveKeySubkeyId.MERCENARIES_SHOULD_SEE_COLLECTION_NOTIFICATION;
			break;
		case MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL:
			subkeyId = GameSaveKeySubkeyId.MERCENARIES_SHOULD_SEE_TRAININGHALL_NOTIFICATION;
			break;
		}
		return subkeyId;
	}

	public static bool GetNotificationStatusForBuilding(MercenaryBuilding.Mercenarybuildingtype buildingType)
	{
		if (buildingType == MercenaryBuilding.Mercenarybuildingtype.COLLECTION)
		{
			return GetNotificationStatusForCollectionBuilding();
		}
		GameSaveKeySubkeyId subkeyId = GetNotificationSubkeyIdForBuilding(buildingType);
		if (subkeyId != GameSaveKeySubkeyId.INVALID)
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, subkeyId, out long value);
			switch (buildingType)
			{
			case MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL:
				if (value <= 0)
				{
					return GetNotificationStatusForTrainingGrounds();
				}
				return true;
			case MercenaryBuilding.Mercenarybuildingtype.PVEZONES:
				if (value <= 0)
				{
					return GetNotificationStatusForTravelPoint();
				}
				return true;
			case MercenaryBuilding.Mercenarybuildingtype.TASKBOARD:
				if (value <= 0)
				{
					return GetNotificationStatusForTaskBoard();
				}
				return true;
			default:
				return value > 0;
			}
		}
		return false;
	}

	public static bool GetNotificationStatusForCollectionBuilding()
	{
		return CollectionManager.Get().DoesAnyMercenaryNeedToBeAcknowledged();
	}

	public static bool GetNotificationStatusForTrainingGrounds()
	{
		return IsAnyMercenaryFinishedTraining();
	}

	public static bool GetNotificationStatusForTravelPoint()
	{
		if (IsDifficultyUnlocked(LettuceBounty.MercenariesBountyDifficulty.HEROIC))
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_HAS_SEEN_HEROIC_PVE_ZONE_FANFARE, out long value);
			if (value == 0L)
			{
				return true;
			}
		}
		if (IsDifficultyUnlocked(LettuceBounty.MercenariesBountyDifficulty.MYTHIC))
		{
			GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_HAS_SEEN_MYTHIC_PVE_ZONE_FANFARE, out long value2);
			if (value2 == 0L)
			{
				return true;
			}
		}
		return ZoneWasRecentlyUnlocked;
	}

	public static bool GetNotificationStatusForTaskBoard()
	{
		foreach (MercenariesTaskState taskState in GetTaskStates())
		{
			if (taskState != null && taskState.Status_ == MercenariesTaskState.Status.COMPLETE)
			{
				return true;
			}
		}
		return false;
	}

	public static bool MarkNotificationAsSeenForBuilding(MercenaryBuilding.Mercenarybuildingtype buildingType)
	{
		GameSaveKeySubkeyId subkeyId = GetNotificationSubkeyIdForBuilding(buildingType);
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, subkeyId, out long value);
		if (value == 0L)
		{
			return false;
		}
		if (subkeyId != GameSaveKeySubkeyId.INVALID)
		{
			switch (buildingType)
			{
			case MercenaryBuilding.Mercenarybuildingtype.COLLECTION:
			{
				List<GameSaveDataManager.SubkeySaveRequest> saveDataRequests = new List<GameSaveDataManager.SubkeySaveRequest>();
				saveDataRequests.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, subkeyId, default(long)));
				saveDataRequests.Add(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_SHOULD_SEE_ABILITYUPGRADE_NOTIFICATION, default(long)));
				return GameSaveDataManager.Get().SaveSubkeys(saveDataRequests);
			}
			case MercenaryBuilding.Mercenarybuildingtype.PVEZONES:
				ZoneWasRecentlyUnlocked = false;
				break;
			}
			return GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(GameSaveKeyId.MERCENARIES, subkeyId, default(long)));
		}
		return false;
	}

	public static void RemoveCompletedOrDismissedTaskDialogue(int taskId)
	{
		VisitorTaskDbfRecord taskRecord = GetTaskRecordByID(taskId);
		if (taskRecord != null && taskRecord.OnAssignedDialog != 0)
		{
			GameSaveDataManager.SubkeySaveRequest saveRequest = GameSaveDataManager.Get().GenerateSaveRequestToRemoveValueFromSubkeyIfItExists(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_VILLAGE_RECENTLY_PLAYED_TASK_DIALOGS, taskRecord.OnAssignedDialog);
			if (saveRequest != null)
			{
				GameSaveDataManager.Get().SaveSubkey(saveRequest);
			}
		}
	}

	public static MercenaryVillageTaskItemDataModel CreateTaskModelFromActiveVisitorState(MercenariesVisitorState visitorState)
	{
		if (visitorState == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageTaskBoard.CreateTaskItemFromVisitorState: visitor state was null");
			return null;
		}
		MercenariesTaskState taskState = visitorState.ActiveTaskState;
		if (taskState == null)
		{
			Debug.LogErrorFormat($"error in LettuceVillageTaskBoard.CreateTaskItemFromVisitorState: active task state null for Visitor id: {visitorState.VisitorId}");
			return null;
		}
		return CreateTaskModelFromTaskState(taskState, visitorState);
	}

	public static bool IsDataEqual(MercenariesVisitorState visitorData, MercenaryVillageTaskItemDataModel dataModel)
	{
		if (visitorData == null)
		{
			return false;
		}
		if (visitorData.ActiveTaskState == null)
		{
			return false;
		}
		if (dataModel.TaskId != visitorData.ActiveTaskState.TaskId)
		{
			return false;
		}
		if (dataModel.Progress != visitorData.ActiveTaskState.Progress)
		{
			return false;
		}
		if (dataModel.TaskStatus != visitorData.ActiveTaskState.Status_)
		{
			return false;
		}
		return true;
	}

	public static bool IsDataEqual(MercenariesCompletedVisitorState completedVisitorData, MercenaryVillageTaskItemDataModel dataModel)
	{
		if (completedVisitorData == null)
		{
			return false;
		}
		if (completedVisitorData.CompletedTaskChainId != dataModel.TaskChainId)
		{
			return false;
		}
		if (dataModel.TaskStatus != MercenariesTaskState.Status.CLAIMED)
		{
			return false;
		}
		return true;
	}

	public static MercenaryVillageTaskItemDataModel CreateTaskModelFromTaskState(MercenariesTaskState taskState, MercenariesVisitorState visitorState = null)
	{
		if (taskState == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageDataUtil.CreateTaskModelFromTaskState: task state is null");
			return null;
		}
		VisitorTaskDbfRecord taskRecord = GetTaskRecordByID(taskState.TaskId);
		if (taskRecord == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageDataUtil.CreateTaskModelFromTaskState: task record is null for task id {0}", taskState.TaskId);
			return null;
		}
		if (GetVisitorRecordByID(taskRecord.MercenaryVisitorId) == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageDataUtil.CreateTaskModelFromTaskState: visitor record is null for mercenary visitor id: {0}", taskRecord.MercenaryVisitorId);
			return null;
		}
		if (visitorState == null)
		{
			foreach (MercenariesVisitorState state in VisitorStates)
			{
				if (state != null && state.ActiveTaskState != null && state.ActiveTaskState.TaskId == taskState.TaskId)
				{
					visitorState = state;
					break;
				}
			}
		}
		if (visitorState == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageDataUtil.CreateTaskModelFromTaskState: unable to find visitor state for visitor id: {0}", taskRecord.MercenaryVisitorId);
			return null;
		}
		AdditionalTaskModelInfo additionalTaskInfo = new AdditionalTaskModelInfo();
		if (visitorState.HasProceduralMercenaryId)
		{
			additionalTaskInfo.MercenaryID = visitorState.ProceduralMercenaryId;
		}
		if (taskState.HasProceduralBountyId)
		{
			additionalTaskInfo.BountyID = taskState.ProceduralBountyId;
		}
		additionalTaskInfo.AdditionalMercenaryIDs = taskState.AdditionalMercenaryId;
		return CreateTaskModel(taskRecord, taskState.Progress, visitorState.TaskChainProgress, taskState.Status_, additionalTaskInfo, setTaskContext: true);
	}

	public static MercenaryVillageTaskItemDataModel CreateTaskModelFromCompletedVisitorState(MercenariesCompletedVisitorState completedState)
	{
		if (completedState == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageDataUtil.CreateTaskModelFromCompletedVisitorState: completed state is null");
			return null;
		}
		VisitorTaskChainDbfRecord taskChain = GetVisitorTaskChainByID(completedState.CompletedTaskChainId);
		int taskListCount = taskChain.TaskList.Count;
		if (taskListCount == 0)
		{
			Debug.LogErrorFormat("error in LettuceVillageDataUtil.CreateTaskModelFromCompletedVisitorState: no tasks in related task chain");
			return null;
		}
		VisitorTaskDbfRecord taskRecord = taskChain.TaskList[taskListCount - 1]?.TaskRecord;
		if (taskRecord == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageDataUtil.CreateTaskModelFromCompletedVisitorState: task record is null");
			return null;
		}
		AdditionalTaskModelInfo additionalTaskInfo = new AdditionalTaskModelInfo();
		if (completedState.HasProceduralMercenaryId)
		{
			additionalTaskInfo.MercenaryID = completedState.ProceduralMercenaryId;
		}
		return CreateTaskModel(taskRecord, taskRecord.Quota, taskListCount - 1, MercenariesTaskState.Status.CLAIMED, additionalTaskInfo, setTaskContext: true);
	}

	public static MercenaryVillageTaskItemDataModel CreateTaskModel(VisitorTaskDbfRecord taskRecord, int progress, int taskChainProgress, MercenariesTaskState.Status taskStatus, AdditionalTaskModelInfo additionalTaskInfo = null, bool setTaskContext = false, bool ShowEquipmentRewardsAsIcon = false)
	{
		MercenaryVisitorDbfRecord visitorRecord = GetVisitorRecordByID(taskRecord.MercenaryVisitorId);
		if (visitorRecord == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageDataUtil.CreateTaskModel: visitor record is null for mercenary visitor id: {0}", taskRecord.MercenaryVisitorId);
			return null;
		}
		LettuceMercenary merc = null;
		int mercId = -1;
		if (additionalTaskInfo != null && additionalTaskInfo.ValidMercenaryID)
		{
			mercId = additionalTaskInfo.MercenaryID;
			merc = CollectionManager.Get().GetMercenary(mercId, AttemptToGenerate: true);
		}
		if (merc == null)
		{
			mercId = GetMercenaryIdForVisitor(visitorRecord, taskRecord);
			merc = CollectionManager.Get().GetMercenary(mercId, AttemptToGenerate: true);
		}
		if (merc == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageDataUtil.CreateTaskModel: merc is null for mercenary id: {0}", mercId);
			return null;
		}
		MercenaryVillageTaskItemDataModel task = new MercenaryVillageTaskItemDataModel();
		string formatedTaskDescription = FormatTaskDescriptionForAbility(taskRecord.TaskDescription, taskRecord.Quota, mercId);
		int bountyId = -1;
		List<int> additionalMercenaryIds = null;
		if (additionalTaskInfo != null)
		{
			bountyId = additionalTaskInfo.BountyID;
			additionalMercenaryIds = additionalTaskInfo.AdditionalMercenaryIDs;
		}
		formatedTaskDescription = FormatTaskStringForProceduralData(formatedTaskDescription, mercId, bountyId, additionalMercenaryIds);
		string taskTitle = FormatTaskStringForProceduralData(taskRecord.TaskTitle, mercId, bountyId, additionalMercenaryIds);
		task.TaskId = taskRecord.ID;
		task.Title = taskTitle;
		task.Description = formatedTaskDescription;
		task.Progress = progress;
		task.ProgressNeeded = taskRecord.Quota;
		task.ProgressMessage = string.Format(GameStrings.Get("GLOBAL_PROGRESSION_PROGRESS_MESSAGE"), progress, taskRecord.Quota);
		task.MercenaryId = mercId;
		task.TaskType = visitorRecord.VisitorType;
		task.TaskChainIndex = taskChainProgress;
		task.MercShoutOut = taskRecord.MercenaryQuote;
		task.IsTimedEvent = false;
		task.TaskStatus = taskStatus;
		VisitorTaskChainDbfRecord taskChain = GetVisitorTaskChain(visitorRecord);
		if (taskChain != null)
		{
			task.TaskChainId = taskChain.ID;
			task.TaskChainLength = taskChain.TaskList.Count;
		}
		if (setTaskContext)
		{
			CurrentTaskContext = mercId;
		}
		RewardListDataModel rewardList = (task.RewardList = RewardUtils.CreateRewardListDataModelFromRewardListRecord(taskRecord.RewardListRecord));
		if (rewardList != null && rewardList.Items != null)
		{
			foreach (RewardItemDataModel rewardDM in rewardList.Items)
			{
				if (taskStatus == MercenariesTaskState.Status.CLAIMED)
				{
					rewardDM.IsClaimed = true;
				}
				if (rewardDM.ItemType == RewardItemType.MERCENARY_EQUIPMENT)
				{
					rewardDM.ItemType = RewardItemType.MERCENARY_EQUIPMENT_ICON;
				}
			}
		}
		switch (visitorRecord.VisitorType)
		{
		case MercenaryVisitor.VillageVisitorType.SPECIAL:
			task.TaskStyle = LettuceVillageTaskBoard.TaskStyle.LEGENDARY;
			break;
		case MercenaryVisitor.VillageVisitorType.EVENT:
		{
			task.TaskStyle = LettuceVillageTaskBoard.TaskStyle.NORMAL;
			EventTimingType eventType = visitorRecord.Event;
			if (eventType != EventTimingType.SPECIAL_EVENT_ALWAYS && eventType != EventTimingType.UNKNOWN && eventType != EventTimingType.SPECIAL_EVENT_NEVER && eventType != 0)
			{
				task.IsTimedEvent = true;
				task.TaskStyle = LettuceVillageTaskBoard.TaskStyle.LEGENDARY;
				TimeSpan timeLeftForEvent = EventTimingManager.Get().GetTimeLeftForEvent(eventType);
				if (timeLeftForEvent.Days > 0)
				{
					task.RemainingEventTime = GameStrings.Format("GLUE_MERCENARIES_TASKBOARD_EVENT_TIME_REM_DAYS", timeLeftForEvent.Days, timeLeftForEvent.Hours);
				}
				else if (timeLeftForEvent.Hours > 1)
				{
					task.RemainingEventTime = GameStrings.Format("GLUE_MERCENARIES_TASKBOARD_EVENT_TIME_REM_HOURS", timeLeftForEvent.Hours);
				}
				else
				{
					task.RemainingEventTime = GameStrings.Get("GLUE_MERCENARIES_TASKBOARD_EVENT_TIME_REM_HOUR_OR_LESS");
				}
			}
			break;
		}
		case MercenaryVisitor.VillageVisitorType.STANDARD:
		case MercenaryVisitor.VillageVisitorType.PROCEDURAL:
			task.TaskStyle = LettuceVillageTaskBoard.TaskStyle.NORMAL;
			break;
		}
		task.MercenaryName = merc.m_mercName;
		task.MercenaryShortName = merc.m_mercShortName;
		task.MercenaryRole = merc.m_role;
		task.MercenaryLevel = merc.m_level;
		CardDbfRecord record = merc.GetCardRecord();
		task.MercenaryCard = new CardDataModel
		{
			CardId = record.NoteMiniGuid,
			Premium = TAG_PREMIUM.NORMAL,
			FlavorText = record?.FlavorText
		};
		return task;
	}

	public static MercenaryVillageTaskItemDataModel CreateTaskModelFromRenownOffer(MercenariesRenownOfferData renownOffer)
	{
		LettuceMercenary merc = CollectionManager.Get().GetMercenary(renownOffer.MercenaryId, AttemptToGenerate: true);
		if (merc == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageDataUtil.CreateTaskModel: merc is null for mercenary id: {0}", renownOffer.MercenaryId);
			return null;
		}
		RewardListDataModel rewards = new RewardListDataModel
		{
			Items = new DataModelList<RewardItemDataModel>()
		};
		if (renownOffer.CoinAmount > 0)
		{
			rewards.Items.Add(new RewardItemDataModel
			{
				Quantity = 1,
				ItemType = RewardItemType.MERCENARY_COIN,
				MercenaryCoin = new LettuceMercenaryCoinDataModel
				{
					MercenaryId = merc.ID,
					MercenaryName = merc.m_mercName,
					Quantity = renownOffer.CoinAmount,
					GlowActive = false,
					NameActive = false
				}
			});
		}
		if (renownOffer.PortraitId > 0)
		{
			MercenaryArtVariationPremiumDbfRecord artVariationPremium = GameDbf.MercenaryArtVariationPremium.GetRecord(renownOffer.PortraitId);
			if (artVariationPremium != null)
			{
				LettuceMercenaryDataModel lettuceMercenaryDataModel = MercenaryFactory.CreateMercenaryDataModel(renownOffer.MercenaryId, artVariationPremium.MercenaryArtVariationId, (TAG_PREMIUM)artVariationPremium.Premium, merc);
				lettuceMercenaryDataModel.Owned = true;
				rewards.Items.Add(new RewardItemDataModel
				{
					Quantity = 1,
					ItemType = RewardItemType.MERCENARY,
					Mercenary = lettuceMercenaryDataModel,
					IsMercenaryPortrait = RewardUtils.IsMercenaryRewardPortrait(lettuceMercenaryDataModel)
				});
			}
			else
			{
				Debug.LogErrorFormat("error in LettuceVillageDataUtil.CreateTaskModel: merc portrait is null for mercenary id: {0}", renownOffer.PortraitId);
			}
		}
		MercenaryVillageTaskItemDataModel obj = new MercenaryVillageTaskItemDataModel
		{
			TaskId = (int)renownOffer.RenownOfferId,
			Title = string.Format(GameStrings.Get("GLUE_MERCENARIES_TASKBOARD_RENOWN_OFFER_TITLE"), merc.m_mercShortName),
			Description = string.Empty,
			TaskStyle = LettuceVillageTaskBoard.TaskStyle.RENOWN,
			TaskStatus = MercenariesTaskState.Status.ACTIVE,
			TaskType = MercenaryVisitor.VillageVisitorType.PROCEDURAL,
			Progress = 0,
			ProgressNeeded = 1,
			MercenaryName = merc.m_mercName,
			MercenaryShortName = merc.m_mercShortName,
			MercenaryRole = merc.m_role,
			MercenaryLevel = merc.m_level,
			RewardList = rewards,
			TaskChainIndex = 0,
			TaskChainId = 0,
			TaskChainLength = 0,
			MercShoutOut = string.Empty,
			IsTimedEvent = false,
			IsRenownOffer = true
		};
		CardDbfRecord record = merc.GetCardRecord();
		obj.MercenaryCard = new CardDataModel
		{
			CardId = record.NoteMiniGuid,
			Premium = TAG_PREMIUM.NORMAL,
			FlavorText = record?.FlavorText
		};
		return obj;
	}

	public static MercenaryVillageTaskItemDataModel Dev_CreateGenericTaskModel(int taskId, int progress, bool ShowEquipmentRewardsAsIcon = false)
	{
		VisitorTaskDbfRecord taskRecord = GetTaskRecordByID(taskId);
		if (taskRecord == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageDataUtil.Dev_CreateGenericTaskModel: task record is null for task id {0}", taskId);
			return null;
		}
		return CreateTaskModel(taskRecord, progress, 1, MercenariesTaskState.Status.ACTIVE, null, setTaskContext: false, ShowEquipmentRewardsAsIcon);
	}

	public static int GetCurrentProgressForTaskRecord(VisitorTaskDbfRecord taskRecord, MercenariesVisitorState visitorState)
	{
		if (taskRecord == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageTaskBoard.CreateTaskItemFromTaskRecord: task record was null");
			return 0;
		}
		if (visitorState == null)
		{
			return 0;
		}
		MercenariesTaskState taskState = visitorState.ActiveTaskState;
		if (taskState == null)
		{
			return 0;
		}
		if (taskState.TaskId == taskRecord.ID)
		{
			return taskState.Progress;
		}
		return 0;
	}

	public static MercenariesTaskState.Status GetCurrentTaskStatusForTaskRecord(VisitorTaskDbfRecord taskRecord, int taskChainIndex, MercenariesVisitorState visitorState)
	{
		if (taskRecord == null)
		{
			Debug.LogErrorFormat("error in LettuceVillageTaskBoard.GetCurrentTaskStatusForTaskRecord: task record was null");
			return MercenariesTaskState.Status.INVALID;
		}
		if (visitorState == null)
		{
			return MercenariesTaskState.Status.CLAIMED;
		}
		MercenariesTaskState taskState = visitorState.ActiveTaskState;
		if (taskState == null)
		{
			return MercenariesTaskState.Status.INVALID;
		}
		if (taskState.TaskId == taskRecord.ID)
		{
			return taskState.Status_;
		}
		if (taskChainIndex < visitorState.TaskChainProgress)
		{
			return MercenariesTaskState.Status.CLAIMED;
		}
		_ = visitorState.TaskChainProgress;
		return MercenariesTaskState.Status.INVALID;
	}

	public static bool IsMercShopAvailable()
	{
		if (!StoreManager.Get().IsOpen())
		{
			return false;
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService) || dataService.HasStoreLoaded())
		{
			return false;
		}
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo == null)
		{
			Debug.LogError("LettuceVillageDataUtil.IsMercShopAvailable - Can't access NetCacheMercenariesPlayerInfo");
			return false;
		}
		if (playerInfo.BuildingEnabledMap.TryGetValue(MercenaryBuilding.Mercenarybuildingtype.SHOP, out var enabled) && !enabled)
		{
			return false;
		}
		return true;
	}

	public static int GetMercenaryIdForVisitor(MercenaryVisitorDbfRecord visitorRecord, VisitorTaskDbfRecord taskRecord = null)
	{
		if (visitorRecord == null)
		{
			Debug.LogError("LettuceVillageDataUtil.GetMercenaryIdForVisitor - Visitor record not provided");
			return 0;
		}
		if (taskRecord == null)
		{
			MercenariesVisitorState state = GetVisitorStateByID(visitorRecord.ID);
			if (state == null)
			{
				Debug.LogError("LettuceVillageDataUtil.GetMercenaryIdForVisitor - Can't find visitor state");
				return visitorRecord.MercenaryId;
			}
			if (state.HasProceduralMercenaryId)
			{
				return state.ProceduralMercenaryId;
			}
			taskRecord = GetTaskRecordByID(state.ActiveTaskState.TaskId);
			if (taskRecord == null)
			{
				Debug.LogError("LettuceVillageDataUtil.GetMercenaryIdForVisitor - Can't find task record");
				return visitorRecord.MercenaryId;
			}
		}
		if (taskRecord.MercenaryOverride != 0)
		{
			return taskRecord.MercenaryOverride;
		}
		return visitorRecord.MercenaryId;
	}

	public static List<int> GetDisabledMercenaryList()
	{
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get()?.GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo == null)
		{
			Log.Lettuce.PrintError("GetDisabledMercenaryList - Can't access NetCacheMercenariesPlayerInfo");
			return null;
		}
		return playerInfo.DisabledMercenaryList;
	}

	public static bool IsMercenaryDisabled(int mercenaryId, List<int> disabledMercList = null)
	{
		if (disabledMercList == null)
		{
			disabledMercList = GetDisabledMercenaryList();
		}
		if (disabledMercList.Contains(mercenaryId))
		{
			return true;
		}
		return false;
	}

	public static bool IsBuildingTierUpgradeDisabled(int buildingTierId)
	{
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get()?.GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo == null)
		{
			Log.Lettuce.PrintError("GetDisabledMercenaryList - Can't access NetCacheMercenariesPlayerInfo");
			return false;
		}
		return playerInfo.DisabledBuildingTierUpgradeList.Contains(buildingTierId);
	}

	public static bool IsAnyMercenaryFinishedTraining()
	{
		BuildingTierDbfRecord currTrainingHallTier = GetCurrentTierRecordFromBuilding(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL);
		int maxExpGained = GetCurrentTierPropertyForBuilding(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL, TierProperties.Buildingtierproperty.TRAININGXPPOOLSIZE, currTrainingHallTier);
		int expPerHour = GetCurrentTierPropertyForBuilding(MercenaryBuilding.Mercenarybuildingtype.TRAININGHALL, TierProperties.Buildingtierproperty.TRAININGXPPERHOUR, currTrainingHallTier);
		(LettuceMercenary, LettuceMercenary) mercsInTraining = CollectionManager.Get().GetMercenariesInTraining();
		bool merc1Finished = false;
		bool merc2Finished = false;
		if (mercsInTraining.Item1 != null)
		{
			int expGained = CalculateExpGainedFromTimestamp(mercsInTraining.Item1.m_trainingStartDate, expPerHour);
			merc1Finished = mercsInTraining.Item1.IsMaxLevel() || expGained >= maxExpGained;
		}
		if (mercsInTraining.Item2 != null)
		{
			int expGained = CalculateExpGainedFromTimestamp(mercsInTraining.Item2.m_trainingStartDate, expPerHour);
			merc2Finished = mercsInTraining.Item2.IsMaxLevel() || expGained >= maxExpGained;
		}
		return merc1Finished || merc2Finished;
	}

	public static int GetTimeTrainingInSeconds(Date startDate)
	{
		if (startDate == null)
		{
			return 0;
		}
		DateTime utcNow = DateTime.UtcNow;
		DateTime trainingStartTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hours, startDate.Min, startDate.Sec);
		return (int)(utcNow - trainingStartTime).TotalSeconds;
	}

	public static int CalculateExpGainedFromTimestamp(Date startDate, int expPerHour)
	{
		if (startDate == null)
		{
			return 0;
		}
		DateTime utcNow = DateTime.UtcNow;
		DateTime trainingStartTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hours, startDate.Min, startDate.Sec);
		TimeSpan difference = utcNow - trainingStartTime;
		int num = (int)difference.TotalHours * expPerHour;
		int secondsExp = (int)((float)difference.TotalSeconds % 3600f * (float)expPerHour / 3600f);
		return num + secondsExp;
	}

	public static bool TryGetCurrentSavedMythicBountyLevel(out int mythicLevel)
	{
		mythicLevel = 0;
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo == null)
		{
			return false;
		}
		int currentWeekNumber = GetMythicWeekNumber(playerInfo.GeneratedBountyResetTime);
		if (!TryGetSavedMythicBountyLevel(currentWeekNumber, out mythicLevel))
		{
			mythicLevel = playerInfo.CurrentMythicBountyLevel;
			UpdateSavedMythicBountyLevel(currentWeekNumber, mythicLevel);
		}
		return true;
	}

	public static bool TryGetSavedMythicBountyLevel(int weekNumber, out int mythicLevel)
	{
		mythicLevel = 0;
		long mythicLevelSaved = Options.Get().GetLong(Option.MYTHIC_LEVEL_CHOICE);
		if ((int)((mythicLevelSaved >> 32) & 0xFFFFFFFFu) == weekNumber)
		{
			mythicLevel = (int)(mythicLevelSaved & 0xFFFFFFFFu);
			return true;
		}
		return false;
	}

	public static void UpdateCurrentSavedMythicBountyLevel(int mythicLevel)
	{
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (playerInfo != null)
		{
			UpdateSavedMythicBountyLevel(GetMythicWeekNumber(playerInfo.GeneratedBountyResetTime), mythicLevel);
		}
	}

	public static void UpdateSavedMythicBountyLevel(int weekNumber, int mythicLevel)
	{
		long optionSave = ((long)weekNumber << 32) | (uint)mythicLevel;
		Options.Get().SetLong(Option.MYTHIC_LEVEL_CHOICE, optionSave);
	}

	public static void ResetSavedMythicBountyLevel()
	{
		Options.Get().SetOption(Option.MYTHIC_LEVEL_CHOICE, 0L);
	}

	public static int GetMythicWeekNumber(DateTime resetTime)
	{
		DateTime minValue = DateTime.MinValue;
		DateTime date = minValue.AddDays(1.0);
		int days = (resetTime.DayOfWeek - date.DayOfWeek + 7) % 7;
		return (int)(resetTime - date.AddDays(days)).TotalDays / 7;
	}

	public static bool HasGeneratedBountyWeeklyResetElapsed()
	{
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		BnetBar bnetBar = BnetBar.Get();
		if (playerInfo == null || bnetBar == null)
		{
			return false;
		}
		if (!bnetBar.TryGetServerTimeUTC(out var serverNow))
		{
			return false;
		}
		return playerInfo.GeneratedBountyResetTime <= serverNow;
	}

	public static void RefreshIfPassedWeeklyGeneratedBountyReset()
	{
		if (HasGeneratedBountyWeeklyResetElapsed())
		{
			RefreshDataIfNecessary();
		}
	}
}
