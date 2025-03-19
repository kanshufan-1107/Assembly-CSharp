using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets;
using Hearthstone.Attribution;
using Hearthstone.DataModels;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone.Progression;

public class RewardTrack
{
	public enum RewardStatus
	{
		UNKNOWN,
		GRANTED,
		ACKED,
		RESET
	}

	private const RewardStatus DefaultStatus = RewardStatus.UNKNOWN;

	private readonly PlayerState<PlayerRewardTrackLevelState> m_rewardTrackLevelState = new PlayerState<PlayerRewardTrackLevelState>(CreateDefaultLevelState);

	private readonly Dictionary<(int, int, RewardTrackPaidType), int> m_pendingRewardClaimRequests = new Dictionary<(int, int, RewardTrackPaidType), int>();

	private int m_rewardTrackPageStartLevel;

	private int m_rewardTrackPageEndLevel;

	public RewardTrackDataModel TrackDataModel { get; private set; }

	public RewardTrackNodeListDataModel NodesDataModel { get; private set; }

	public PageInfoDataModel PageDataModel { get; private set; }

	public int RewardTrackId => TrackDataModel?.RewardTrackId ?? 0;

	public bool IsValid => RewardTrackId != 0;

	public bool IsActive { get; private set; }

	public RewardTrackDbfRecord RewardTrackAsset => GameDbf.RewardTrack.GetRecord(RewardTrackId);

	public RewardTrackLevelDbfRecord RewardTrackLevelAsset => GameUtils.GetRewardTrackLevelsForRewardTrack(RewardTrackId).Find((RewardTrackLevelDbfRecord r) => r.Level == TrackDataModel?.Level);

	public int CurrentPageNumber => PageDataModel?.PageNumber ?? 1;

	public int TotalPages => PageDataModel?.TotalPages ?? 1;

	public bool IsRewardClaimPending => m_pendingRewardClaimRequests.Count > 0;

	public bool HasOwnedRewardTrackPaidType(RewardTrackPaidType paidType)
	{
		return ProgressUtils.HasOwnedRewardTrackPaidType(RewardTrackAsset, paidType);
	}

	public HashSet<RewardTrackPaidType> GetOwnedRewardTrackPaidTypes()
	{
		RewardTrackDbfRecord rewardTrackDbfRecord = RewardTrackAsset;
		HashSet<RewardTrackPaidType> ret = new HashSet<RewardTrackPaidType>();
		RewardTrackPaidType[] allValidRewardTrackPaidType = ProgressUtils.AllValidRewardTrackPaidType;
		foreach (RewardTrackPaidType paidType in allValidRewardTrackPaidType)
		{
			if (ProgressUtils.HasOwnedRewardTrackPaidType(rewardTrackDbfRecord, paidType))
			{
				ret.Add(paidType);
			}
		}
		return ret;
	}

	public RewardTrack(Global.RewardTrackType rewardTrackType)
	{
		TrackDataModel = new RewardTrackDataModel
		{
			RewardTrackType = rewardTrackType
		};
		NodesDataModel = new RewardTrackNodeListDataModel();
		PageDataModel = new PageInfoDataModel();
		m_rewardTrackLevelState.OnStateChanged += OnLevelStateChanged;
	}

	public void Reset()
	{
		Global.RewardTrackType rewardTrackType = TrackDataModel.RewardTrackType;
		TrackDataModel = new RewardTrackDataModel
		{
			RewardTrackType = rewardTrackType
		};
		NodesDataModel = new RewardTrackNodeListDataModel();
		PageDataModel = new PageInfoDataModel();
		m_rewardTrackLevelState.Reset();
		m_pendingRewardClaimRequests.Clear();
	}

	public void InitializeClientState()
	{
		int seasonLastSeen = GetLastRewardTrackSeasonSeen();
		if (seasonLastSeen <= 0 && TrackDataModel.RewardTrackType == Global.RewardTrackType.GLOBAL)
		{
			TrackDataModel.SeasonLastSeen = 1;
		}
		else
		{
			TrackDataModel.SeasonLastSeen = seasonLastSeen;
		}
	}

	public bool AckReward(int rewardTrackId, int level, RewardTrackPaidType paidType)
	{
		if (RewardTrackId == rewardTrackId)
		{
			if (!RewardExistsAtLevel(level, paidType))
			{
				return false;
			}
			if (GetRewardStatus(level, paidType) != RewardStatus.GRANTED)
			{
				return false;
			}
			PlayerRewardTrackLevelState state = m_rewardTrackLevelState.GetState(level);
			if (state == null)
			{
				return false;
			}
			ProgressUtils.SetRewardStatus(state, paidType, RewardStatus.ACKED);
		}
		Network.Get().AckRewardTrackReward(rewardTrackId, level, paidType);
		return true;
	}

	public bool ClaimReward(int rewardTrackId, int level, RewardTrackPaidType paidType, int chooseOneRewardItemId = 0)
	{
		if (rewardTrackId == RewardTrackId)
		{
			if (!RewardExistsAtLevel(level, paidType))
			{
				return false;
			}
			if (TrackDataModel.Level < level)
			{
				return false;
			}
			if (ProgressUtils.HasClaimedRewardTrackReward(GetRewardStatus(level, paidType)))
			{
				return false;
			}
			if (!HasOwnedRewardTrackPaidType(paidType))
			{
				return false;
			}
		}
		if (m_pendingRewardClaimRequests.ContainsKey((rewardTrackId, level, paidType)))
		{
			return false;
		}
		m_pendingRewardClaimRequests[(rewardTrackId, level, paidType)] = chooseOneRewardItemId;
		Network.Get().ClaimRewardTrackReward(rewardTrackId, level, paidType, chooseOneRewardItemId);
		return true;
	}

	public void SetRewardTrackNodes(int startLevel, int endLevel, int itemsPerPage, int pageNum)
	{
		List<RewardTrackLevelDbfRecord> rewardTrackDbfRecords = GameUtils.GetRewardTrackLevelsForRewardTrack(RewardTrackId);
		if (rewardTrackDbfRecords == null || rewardTrackDbfRecords.Count == 0)
		{
			Log.All.PrintError("SetRewardTrackNodes: RewardTrackAsset is missing or incomplete!");
			return;
		}
		int maxLevel = rewardTrackDbfRecords.Count;
		int cappedLevel = Mathf.Min(((RewardTrackAsset.LevelCapSoft > 0) ? RewardTrackAsset.LevelCapSoft : maxLevel) + 1, maxLevel);
		int totalPages = ((maxLevel <= 0) ? 1 : Mathf.Max(1, Mathf.CeilToInt((float)cappedLevel / (float)itemsPerPage)));
		pageNum = Mathf.Clamp(pageNum, 1, totalPages);
		m_rewardTrackPageStartLevel = Mathf.Clamp(startLevel, 1, maxLevel);
		m_rewardTrackPageEndLevel = Mathf.Clamp(endLevel, 1, maxLevel);
		TrackDataModel.LevelSoftCap = ((RewardTrackAsset.LevelCapSoft > 0) ? cappedLevel : RewardTrackAsset.LevelCapSoft);
		TrackDataModel.LevelHardCap = maxLevel;
		PageDataModel.PageNumber = pageNum;
		PageDataModel.TotalPages = totalPages;
		PageDataModel.ItemsPerPage = itemsPerPage;
		PageDataModel.InfoText = GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_PAGE_NUMBER", pageNum, totalPages);
		CreateRewardTrackNodes();
		ApplyRewardTrackStateToNodes();
	}

	public void SetRewardTrackNodePage(int pageNum, int itemsPerPage)
	{
		int startLevel = itemsPerPage * (pageNum - 1) + 1;
		int endLevel = startLevel + itemsPerPage - 1;
		SetRewardTrackNodes(startLevel, endLevel, itemsPerPage, pageNum);
	}

	public void SetApprenticeTrackNodePage(int pageNum, int itemsPerPage)
	{
		int startLevel = itemsPerPage * (pageNum - 1);
		int endLevel = startLevel + itemsPerPage - 1;
		SetRewardTrackNodes(startLevel, endLevel, itemsPerPage, pageNum);
	}

	public void SetEventRewardTrackNodePage(int itemsPerPage)
	{
		SetRewardTrackNodes(2, itemsPerPage + 1, itemsPerPage, 1);
	}

	public bool HasUnclaimedRewardsForLevel(int level)
	{
		return HasUnclaimedRewardsForLevel(GetRewardTrackLevelRecord(level));
	}

	public bool HasUnclaimedRewardsForLevel(RewardTrackLevelDbfRecord record)
	{
		PlayerRewardTrackLevelState state = m_rewardTrackLevelState.GetState(record.Level);
		RewardTrackPaidType[] allValidRewardTrackPaidType = ProgressUtils.AllValidRewardTrackPaidType;
		foreach (RewardTrackPaidType paidType in allValidRewardTrackPaidType)
		{
			if (HasOwnedRewardTrackPaidType(paidType))
			{
				int? rewardListCount = ProgressUtils.GetRewardListRecord(record, paidType)?.RewardItems?.Count;
				if (rewardListCount.HasValue && rewardListCount.Value != 0 && !ProgressUtils.HasClaimedRewardTrackReward(ProgressUtils.GetRewardStatus(state, paidType)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public RewardTrackLevelDbfRecord GetRewardTrackLevelRecord(int level)
	{
		return GameUtils.GetRewardTrackLevelsForRewardTrack(RewardTrackId).Find((RewardTrackLevelDbfRecord cur) => cur.Level == level);
	}

	public bool SetRewardTrackSeasonLastSeen(int seasonLastSeen)
	{
		GameSaveKeyId key = GameSaveKeyId.PROGRESSION;
		GameSaveKeySubkeyId subkey = GameSaveKeySubkeyId.PROGRESSION_REWARD_TRACK_SEASON_LAST_SEEN;
		switch (TrackDataModel.RewardTrackType)
		{
		case Global.RewardTrackType.NONE:
			Error.AddDevFatal("Attempted to set last season seen of reward track type NONE.");
			return false;
		case Global.RewardTrackType.BATTLEGROUNDS:
			key = GameSaveKeyId.BACON;
			subkey = GameSaveKeySubkeyId.BATTLEGROUNDS_REWARD_TRACK_LAST_SEASON_SEEN;
			break;
		default:
			subkey = GameSaveKeySubkeyId.PROGRESSION_EVENT_TAB_EVENT_TRACK_SEASON_LAST_SEEN;
			break;
		case Global.RewardTrackType.GLOBAL:
			break;
		}
		if (GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(key, subkey, seasonLastSeen)))
		{
			TrackDataModel.SeasonLastSeen = seasonLastSeen;
			return true;
		}
		return false;
	}

	public int ApplyXpBonusPercent(int xp)
	{
		float multiplier = 1f + (float)TrackDataModel.XpBonusPercent / 100f;
		return (int)Math.Round((float)xp * multiplier, MidpointRounding.AwayFromZero);
	}

	public void UpdateRewardsAndBonuses()
	{
		UpdatePremiumRewardsUnlocked();
		UpdateXpBonusPercent();
		UpdateUnclaimedRewards();
	}

	public void UpdateLevelHardCap()
	{
		List<RewardTrackLevelDbfRecord> rewardTrackDbfRecords = GameUtils.GetRewardTrackLevelsForRewardTrack(RewardTrackId);
		if (rewardTrackDbfRecords == null || rewardTrackDbfRecords.Count == 0)
		{
			Log.All.PrintError("UpdateLevelHardCap: RewardTrackAsset is missing or incomplete!");
		}
		else
		{
			TrackDataModel.LevelHardCap = rewardTrackDbfRecords.Count;
		}
	}

	public void HandleRewardTrackStateUpdate(PlayerRewardTrackState stateUpdate)
	{
		if (stateUpdate == null)
		{
			return;
		}
		if (!stateUpdate.HasRewardTrackId || stateUpdate.RewardTrackId == 0)
		{
			Log.All.PrintWarning("Received update for an invalid reward track, ignoring.");
			return;
		}
		bool activeTrackChanged = false;
		if (TrackDataModel.RewardTrackId != stateUpdate.RewardTrackId)
		{
			if (!stateUpdate.HasIsActiveRewardTrack && !stateUpdate.HasLevel && !stateUpdate.HasXp)
			{
				foreach (PlayerRewardTrackLevelState levelState in stateUpdate.TrackLevel)
				{
					OnLevelStateChangedForTrack(stateUpdate.RewardTrackId, null, levelState);
				}
				return;
			}
			activeTrackChanged = true;
			TrackDataModel.RewardTrackId = stateUpdate.RewardTrackId;
			TrackDataModel.Season = RewardTrackAsset.Season;
			TrackDataModel.Name = RewardTrackAsset.Name?.GetString() ?? string.Empty;
			UpdateLevelHardCap();
			m_rewardTrackLevelState.Reset();
			UpdatePremiumRewardsUnlocked();
			CreateRewardTrackNodes();
		}
		if (stateUpdate.HasIsActiveRewardTrack)
		{
			if (IsActive != stateUpdate.IsActiveRewardTrack && RewardTrackManager.Get().HasReceivedRewardTracksFromServer)
			{
				RewardTrackManager.Get().OnSeasonRollDuringClientSession(TrackDataModel, stateUpdate.IsActiveRewardTrack);
			}
			IsActive = stateUpdate.IsActiveRewardTrack;
		}
		if (stateUpdate.HasLevel)
		{
			if (TrackDataModel.Level != stateUpdate.Level)
			{
				TrackDataModel.Level = stateUpdate.Level;
				UpdateXpBonusPercent();
				UpdateUnclaimedRewards();
				BlizzardAttributionManager.Get().SendEvent_RewardTrackUpdate(stateUpdate.Level);
			}
			TrackDataModel.XpNeeded = RewardTrackLevelAsset?.XpNeeded ?? 0;
		}
		if (stateUpdate.HasXp)
		{
			TrackDataModel.Xp = stateUpdate.Xp;
		}
		if (stateUpdate.HasLevel || stateUpdate.HasXp)
		{
			TrackDataModel.TotalXp = GetTotalXpFromCompletedNodes();
			TrackDataModel.XpProgress = GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_XP_PROGRESS", TrackDataModel.Xp, TrackDataModel.XpNeeded);
		}
		if (stateUpdate.TrackLevel.Count > 0)
		{
			foreach (PlayerRewardTrackLevelState levelState2 in stateUpdate.TrackLevel)
			{
				m_rewardTrackLevelState.UpdateState(levelState2.Level, levelState2);
			}
			ApplyRewardTrackStateToNodes();
		}
		if (stateUpdate.TrackLevel.Count > 0 || activeTrackChanged)
		{
			UpdateUnclaimedRewards();
		}
	}

	public int GetLastRewardTrackSeasonSeen()
	{
		GameSaveKeyId key = GameSaveKeyId.PROGRESSION;
		GameSaveKeySubkeyId subkey = GameSaveKeySubkeyId.PROGRESSION_REWARD_TRACK_SEASON_LAST_SEEN;
		switch (TrackDataModel.RewardTrackType)
		{
		case Global.RewardTrackType.NONE:
			Error.AddDevFatal("Attempted to get last season seen of reward track type NONE.");
			return 0;
		case Global.RewardTrackType.BATTLEGROUNDS:
			key = GameSaveKeyId.BACON;
			subkey = GameSaveKeySubkeyId.BATTLEGROUNDS_REWARD_TRACK_LAST_SEASON_SEEN;
			break;
		default:
			subkey = GameSaveKeySubkeyId.PROGRESSION_EVENT_TAB_EVENT_TRACK_SEASON_LAST_SEEN;
			break;
		case Global.RewardTrackType.GLOBAL:
			break;
		}
		if (GameSaveDataManager.Get().GetSubkeyValue(key, subkey, out long rewardTrackSeasonLastSeen))
		{
			return (int)rewardTrackSeasonLastSeen;
		}
		return 0;
	}

	public bool SeasonNewerThanLastSeen()
	{
		return RewardTrackAsset.Season > GetLastRewardTrackSeasonSeen();
	}

	public TimeSpan GetTimeRemaining()
	{
		return EventTimingManager.Get().GetTimeLeftForEvent(RewardTrackAsset.Event);
	}

	private static PlayerRewardTrackLevelState CreateDefaultLevelState(int id)
	{
		return new PlayerRewardTrackLevelState
		{
			Level = id,
			FreeRewardStatus = 0,
			PaidRewardStatus = 0,
			PaidPremiumRewardStatus = 0
		};
	}

	private void UpdatePremiumRewardsUnlocked()
	{
		TrackDataModel.PaidRewardsUnlocked = HasOwnedRewardTrackPaidType(RewardTrackPaidType.RTPT_PAID);
		TrackDataModel.PaidPremiumRewardsUnlocked = HasOwnedRewardTrackPaidType(RewardTrackPaidType.RTPT_PAID_PREMIUM);
	}

	private void UpdateXpBonusPercent()
	{
		float xpMult = CalcRewardTrackXpMult();
		TrackDataModel.XpBonusPercent = (int)Math.Round((xpMult - 1f) * 100f);
		TrackDataModel.XpBoostText = GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_XP_TOOLTIP_BOOST_BODY", TrackDataModel.XpBonusPercent);
	}

	private void CreateRewardTrackNodes()
	{
		NodesDataModel.Nodes = RewardTrackFactory.CreateRewardTrackNodeDataModelList(RewardTrackAsset, TrackDataModel, m_rewardTrackLevelState.GetState, m_rewardTrackPageStartLevel, m_rewardTrackPageEndLevel);
	}

	private int GetTotalXpFromCompletedNodes()
	{
		int cumulativeXp = TrackDataModel.Xp;
		List<RewardTrackLevelDbfRecord> rewardTrackLevels = GameUtils.GetRewardTrackLevelsForRewardTrack(RewardTrackId);
		for (int i = 1; i < TrackDataModel.Level; i++)
		{
			RewardTrackLevelDbfRecord level = rewardTrackLevels[i - 1];
			cumulativeXp += level.XpNeeded;
		}
		return cumulativeXp;
	}

	private void SetChooseOneRewardItemAsOwned(int level, RewardTrackPaidType paidType, int chooseOneRewardItemId)
	{
		if (chooseOneRewardItemId > 0 && NodesDataModel != null && NodesDataModel.Nodes != null)
		{
			RewardItemDataModel rewardItem = (from item in NodesDataModel.Nodes.Where((RewardTrackNodeDataModel node) => node != null && node.Level == level).SelectMany((RewardTrackNodeDataModel node) => ProgressUtils.GetNodeRewards(node, paidType)?.Items?.Items)
				where item != null && item.AssetId == chooseOneRewardItemId
				select item).FirstOrDefault();
			if (rewardItem?.Card != null)
			{
				rewardItem.Card.Owned = true;
			}
		}
	}

	private void ApplyRewardTrackStateToNodes()
	{
		if (NodesDataModel.Nodes == null)
		{
			return;
		}
		foreach (RewardTrackNodeDataModel node in NodesDataModel.Nodes)
		{
			PlayerRewardTrackLevelState state = m_rewardTrackLevelState.GetState(node.Level);
			RewardTrackPaidType[] allValidRewardTrackPaidType = ProgressUtils.AllValidRewardTrackPaidType;
			foreach (RewardTrackPaidType paidType in allValidRewardTrackPaidType)
			{
				RewardTrackNodeRewardsDataModel nodeRewardsDataModel = ProgressUtils.GetNodeRewards(node, paidType);
				if (nodeRewardsDataModel == null)
				{
					continue;
				}
				bool claimed = (nodeRewardsDataModel.IsClaimed = ProgressUtils.HasClaimedRewardTrackReward(ProgressUtils.GetRewardStatus(state, paidType)));
				if (paidType != RewardTrackPaidType.RTPT_FREE || node.AdditionalRewards?.RewardLists == null)
				{
					continue;
				}
				foreach (RewardTrackNodeRewardsDataModel rewardList in node.AdditionalRewards.RewardLists)
				{
					rewardList.IsClaimed = claimed;
				}
			}
		}
	}

	private void UpdateUnclaimedRewards()
	{
		List<RewardTrackLevelDbfRecord> levels = GameUtils.GetRewardTrackLevelsForRewardTrack(RewardTrackId);
		if (levels == null || levels.Count == 0)
		{
			Log.All.PrintError("UpdateUnclaimedRewards: RewardTrackAsset is missing or incomplete!");
		}
		else
		{
			TrackDataModel.Unclaimed = ProgressUtils.CountUnclaimedRewards(levels, TrackDataModel.Level, GetOwnedRewardTrackPaidTypes(), m_rewardTrackLevelState.GetState);
		}
	}

	private void OnLevelStateChanged(PlayerRewardTrackLevelState oldState, PlayerRewardTrackLevelState newState)
	{
		OnLevelStateChangedForTrack(TrackDataModel.RewardTrackId, oldState, newState);
	}

	private void OnLevelStateChangedForTrack(int rewardTrackId, PlayerRewardTrackLevelState oldState, PlayerRewardTrackLevelState newState)
	{
		if (newState == null)
		{
			return;
		}
		RewardTrackPaidType[] allValidRewardTrackPaidType = ProgressUtils.AllValidRewardTrackPaidType;
		foreach (RewardTrackPaidType paidType in allValidRewardTrackPaidType)
		{
			if ((oldState == null || ProgressUtils.GetRewardStatus(oldState, paidType) != ProgressUtils.GetRewardStatus(newState, paidType)) && ProgressUtils.GetRewardStatus(newState, paidType) == RewardStatus.GRANTED)
			{
				HandleRewardGranted(rewardTrackId, newState.Level, paidType, newState.RewardItemOutput);
			}
		}
	}

	private void HandleRewardGranted(int rewardTrackId, int level, RewardTrackPaidType paidType, List<RewardItemOutput> rewardItemOutput)
	{
		RewardTrackDbfRecord rewardTrackAsset = GameDbf.RewardTrack.GetRecord(rewardTrackId);
		if (rewardTrackAsset == null)
		{
			return;
		}
		RewardTrackLevelDbfRecord rewardTrackLevelAsset = GameUtils.GetRewardTrackLevelsForRewardTrack(rewardTrackId).Find((RewardTrackLevelDbfRecord r) => r.Level == level);
		if (rewardTrackLevelAsset == null)
		{
			return;
		}
		int rewardListId = ProgressUtils.GetRewardListAssetId(rewardTrackLevelAsset, paidType);
		(int, int, RewardTrackPaidType) pendingClaimKey = (rewardTrackLevelAsset.RewardTrackId, level, paidType);
		if (m_pendingRewardClaimRequests.TryGetValue(pendingClaimKey, out var chooseOneRewardItemId))
		{
			m_pendingRewardClaimRequests.Remove(pendingClaimKey);
		}
		string displayName;
		if (ProgressUtils.IsEventRewardTrackType((Global.RewardTrackType)rewardTrackAsset.RewardTrackType))
		{
			int xpNeeded = RewardTrackFactory.CalculateCumulativeXpForLevel(rewardTrackAsset, level);
			displayName = GameStrings.Format("GLUE_PROGRESSION_EVENT_TAB_REWARD_SCROLL_TITLE", xpNeeded);
		}
		else
		{
			displayName = GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_REWARD_SCROLL_TITLE", level);
		}
		RewardScrollDataModel additionalRewardsScroll = RewardTrackFactory.CreateAdditionalRewardScrollDataModel(rewardListId, level, displayName, chooseOneRewardItemId, rewardItemOutput);
		if (additionalRewardsScroll != null)
		{
			RewardTrackManager.Get().GetRewardPresenter().EnqueueReward(additionalRewardsScroll, null);
		}
		RewardScrollDataModel rewardScroll = RewardTrackFactory.CreateRewardScrollDataModel(rewardListId, level, displayName, chooseOneRewardItemId, rewardItemOutput);
		if (rewardScroll != null)
		{
			RewardTrackManager.Get().GetRewardPresenter().EnqueueReward(rewardScroll, delegate
			{
				AckReward(rewardTrackId, level, paidType);
			});
		}
		if (rewardTrackId == TrackDataModel.RewardTrackId)
		{
			SetChooseOneRewardItemAsOwned(level, paidType, chooseOneRewardItemId);
		}
	}

	private RewardStatus GetRewardStatus(int level, RewardTrackPaidType paidType)
	{
		return ProgressUtils.GetRewardStatus(m_rewardTrackLevelState.GetState(level), paidType);
	}

	private bool RewardExistsAtLevel(int level, RewardTrackPaidType paidType)
	{
		RewardTrackLevelDbfRecord rewardTrackLevelAsset = GameUtils.GetRewardTrackLevelsForRewardTrack(RewardTrackId).Find((RewardTrackLevelDbfRecord r) => r.Level == level);
		if (rewardTrackLevelAsset == null)
		{
			return false;
		}
		return ProgressUtils.GetRewardListAssetId(rewardTrackLevelAsset, paidType) != 0;
	}

	private float CalcRewardTrackXpMult()
	{
		float xpMult = 1f;
		HashSet<RewardTrackPaidType> unlockedPaidTypes = GetOwnedRewardTrackPaidTypes();
		unlockedPaidTypes.Remove(RewardTrackPaidType.RTPT_FREE);
		if (unlockedPaidTypes.Count == 0)
		{
			return xpMult;
		}
		List<RewardTrackLevelDbfRecord> rewardTrackLevelAssets = GameUtils.GetRewardTrackLevelsForRewardTrack(RewardTrackId);
		if (rewardTrackLevelAssets != null)
		{
			foreach (RewardTrackLevelDbfRecord rewardTrackLevelAsset in rewardTrackLevelAssets)
			{
				if (rewardTrackLevelAsset.Level > TrackDataModel.Level)
				{
					continue;
				}
				foreach (RewardTrackPaidType paidType in unlockedPaidTypes)
				{
					List<RewardItemDbfRecord> rewardItems = ProgressUtils.GetRewardListRecord(rewardTrackLevelAsset, paidType)?.RewardItems;
					if (rewardItems == null || rewardItems.Count == 0)
					{
						continue;
					}
					foreach (RewardItemDbfRecord rewardItemAsset in rewardItems)
					{
						if (rewardItemAsset.RewardType == RewardItem.RewardType.REWARD_TRACK_XP_BOOST)
						{
							float levelXpMult = 1f + (float)rewardItemAsset.Quantity / 100f;
							if (levelXpMult > xpMult)
							{
								xpMult = levelXpMult;
							}
						}
					}
				}
			}
		}
		return xpMult;
	}

	public string GetRewardTrackDebugHudString()
	{
		StringBuilder sb = new StringBuilder();
		string eventName = RewardTrackAsset?.Event.ToString();
		int season = RewardTrackAsset?.Season ?? 0;
		int xpNeeded = RewardTrackLevelAsset?.XpNeeded ?? 0;
		sb.AppendLine($"ID={TrackDataModel.RewardTrackId} SEASON={season} EVENT={eventName}");
		sb.AppendLine($"LEVEL: {TrackDataModel.Level}");
		sb.AppendLine($"XP: {TrackDataModel.Xp} / {xpNeeded}  (BOOST: {TrackDataModel.XpBonusPercent}%)");
		sb.AppendLine();
		sb.AppendLine("---------- XP Gains ----------");
		sb.AppendLine(RewardXpNotificationManager.Get().GetRewardTrackDebugHudString());
		sb.AppendLine();
		sb.AppendLine("---------- Level States ----------");
		sb.AppendLine($"Unclaimed: {TrackDataModel.Unclaimed}");
		foreach (PlayerRewardTrackLevelState rewardTrackLevelState in m_rewardTrackLevelState.OrderBy((PlayerRewardTrackLevelState r) => r.Level))
		{
			sb.AppendFormat("{0} Free={1} Paid={2} Paid Premium={3}", rewardTrackLevelState.Level, GetRewardStatusString(rewardTrackLevelState, RewardTrackPaidType.RTPT_FREE), GetRewardStatusString(rewardTrackLevelState, RewardTrackPaidType.RTPT_PAID), GetRewardStatusString(rewardTrackLevelState, RewardTrackPaidType.RTPT_PAID_PREMIUM));
			sb.AppendLine();
		}
		return sb.ToString();
	}

	private string GetRewardStatusString(PlayerRewardTrackLevelState rewardTrackLevelState, RewardTrackPaidType paidType)
	{
		if (RewardExistsAtLevel(rewardTrackLevelState.Level, paidType))
		{
			return Enum.GetName(typeof(RewardStatus), ProgressUtils.GetRewardStatus(rewardTrackLevelState, paidType));
		}
		return "n/a";
	}
}
