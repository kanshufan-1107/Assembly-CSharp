using System;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusLettuce;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class ProfilePageGameMode : MonoBehaviour
{
	private enum GameModeIconID
	{
		ARENA,
		BATTLEGROUNDS,
		PVPDR_HEROIC,
		PVPDR_NORMAL,
		MERCENARIES
	}

	public static readonly AssetReference PROFILE_PAGE_GAME_MODE_PREFAB = new AssetReference("ProfilePageGameMode.prefab:f475550e593d4ac4d927f215596ff43d");

	private Widget m_widget;

	private RankedPlayListDataModel m_rankedPlayDataModels;

	private ProfileGameModeStatDataModel m_arenaDataModel;

	private ProfileGameModeStatDataModel m_bgDataModel;

	private ProfileGameModeStatDataModel m_mercenariesDataModel;

	private ProfileGameModeStatListDataModel m_gameModeStatDataModels;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_rankedPlayDataModels = new RankedPlayListDataModel();
		m_arenaDataModel = new ProfileGameModeStatDataModel();
		m_bgDataModel = new ProfileGameModeStatDataModel();
		m_gameModeStatDataModels = new ProfileGameModeStatListDataModel();
		m_mercenariesDataModel = new ProfileGameModeStatDataModel();
		if (NetCache.Get().GetNetObject<NetCache.NetCacheBaconRatingInfo>() == null)
		{
			Network.Get().RegisterNetHandler(BattlegroundsRatingInfoResponse.PacketID.ID, OnBattlegroundsRatingInfo);
			Network.Get().RequestBaconRatingInfo();
		}
		if (NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>() == null)
		{
			Network.Get().RegisterNetHandler(MercenariesPlayerInfoResponse.PacketID.ID, OnMercenariesPlayerInfoResponse);
			Network.Get().MercenariesPlayerInfoRequest();
		}
		List<GameSaveKeyId> bgKeyRequest = new List<GameSaveKeyId>();
		if (!GameSaveDataManager.Get().IsRequestPending(GameSaveKeyId.BACON) && !GameSaveDataManager.Get().IsDataReady(GameSaveKeyId.BACON))
		{
			bgKeyRequest.Add(GameSaveKeyId.BACON);
		}
		if (!GameSaveDataManager.Get().IsRequestPending(GameSaveKeyId.BACON_DUOS) && !GameSaveDataManager.Get().IsDataReady(GameSaveKeyId.BACON_DUOS))
		{
			bgKeyRequest.Add(GameSaveKeyId.BACON_DUOS);
		}
		GameSaveDataManager.Get().Request(bgKeyRequest, OnBgGameSaveDataReceived);
		InitRankAndStats();
	}

	public void Show()
	{
		m_widget.Show();
		UpdateData();
	}

	public void Hide()
	{
		m_widget.Hide();
	}

	private void OnBgGameSaveDataReceived(bool success)
	{
		if (success)
		{
			UpdateBgStatDataModel();
		}
	}

	private int GetTop4BgWins()
	{
		int result = 0;
		if (!GameSaveDataManager.Get().IsDataReady(GameSaveKeyId.BACON))
		{
			return result;
		}
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.BACON, GameSaveKeySubkeyId.BACON_TOP_4_FINISHES, out long value);
		return (int)value;
	}

	private void OnBattlegroundsRatingInfo()
	{
		Network.Get().RemoveNetHandler(BattlegroundsRatingInfoResponse.PacketID.ID, OnBattlegroundsRatingInfo);
		UpdateBgStatDataModel();
	}

	private void OnMercenariesPlayerInfoResponse()
	{
		Network.Get().RemoveNetHandler(MercenariesPlayerInfoResponse.PacketID.ID, OnMercenariesPlayerInfoResponse);
		GetTotalWins(out var _, out var _, out var totalMercWins);
		UpdateMercsStatDataModel(totalMercWins);
	}

	private void UpdateData()
	{
		GetTotalWins(out var totalRankedWins, out var totalArenaWins, out var totalMercWins);
		UpdateRankedMedals(totalRankedWins);
		UpdateGameModeStats(totalArenaWins, totalMercWins);
	}

	private void UpdateRankedMedals(int totalRankedWins)
	{
		m_rankedPlayDataModels.Items.Clear();
		MedalInfoTranslator mit = RankMgr.Get().GetLocalPlayerMedalInfo();
		foreach (FormatType enumValue in Enum.GetValues(typeof(FormatType)))
		{
			if (enumValue != 0 && RankMgr.Get().IsFormatAllowedInLeague(enumValue))
			{
				RankedPlayDataModel rankedDataModel = mit.CreateDataModel(enumValue, RankedMedal.DisplayMode.Default, isTooltipEnabled: true);
				m_rankedPlayDataModels.Items.Add(rankedDataModel);
			}
		}
		m_rankedPlayDataModels.TotalWins = totalRankedWins;
	}

	private void GetTotalWins(out int totalRankedWins, out int totalArenaWins, out int totalMercWins)
	{
		totalRankedWins = 0;
		totalArenaWins = 0;
		totalMercWins = 0;
		NetCache.NetCachePlayerRecords cachedPlayerRecords = NetCache.Get()?.GetNetObject<NetCache.NetCachePlayerRecords>();
		if (cachedPlayerRecords?.Records == null)
		{
			return;
		}
		foreach (NetCache.PlayerRecord record in cachedPlayerRecords.Records)
		{
			if (record != null && record.Data == 0)
			{
				switch (record.RecordType)
				{
				case GameType.GT_ARENA:
					totalArenaWins += record.Wins;
					break;
				case GameType.GT_RANKED:
					totalRankedWins += record.Wins;
					break;
				}
			}
		}
		NetCache.NetCacheMercenariesPlayerInfo mercPlayerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		if (mercPlayerInfo == null)
		{
			return;
		}
		foreach (KeyValuePair<int, NetCache.NetCacheMercenariesPlayerInfo.BountyInfo> item in mercPlayerInfo.BountyInfoMap)
		{
			totalMercWins += item.Value.Completions;
		}
	}

	private void InitRankAndStats()
	{
		GetTotalWins(out var totalRankedWins, out var totalArenaWins, out var totalMercWins);
		UpdateRankedMedals(totalRankedWins);
		InitArenaStatDataModel(totalArenaWins);
		m_gameModeStatDataModels.Items.Add(m_arenaDataModel);
		InitBgStatDataModel();
		m_gameModeStatDataModels.Items.Add(m_bgDataModel);
		InitMercsStatDataModel(totalMercWins);
		m_gameModeStatDataModels.Items.Add(m_mercenariesDataModel);
		m_widget.BindDataModel(m_rankedPlayDataModels);
		m_widget.BindDataModel(m_gameModeStatDataModels);
	}

	private void InitArenaStatDataModel(int totalArenaWins)
	{
		m_arenaDataModel.ModeIcon = 0;
		m_arenaDataModel.ModeName = GameStrings.Format("GLUE_QUEST_LOG_ARENA");
		m_arenaDataModel.StatName = GameStrings.Get("GLOBAL_PROGRESSION_PROFILE_ARENA_STAT_TOOLTIP_TITLE");
		m_arenaDataModel.StatDesc = GameStrings.Get("GLOBAL_PROGRESSION_PROFILE_ARENA_STAT_TOOLTIP");
		UpdateArenaStatDataModel(totalArenaWins);
	}

	private void UpdateArenaStatDataModel(int totalArenaWins)
	{
		m_arenaDataModel.StatValue.Clear();
		NetCache.NetCacheProfileProgress arenaProgress = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
		m_arenaDataModel.StatValue.Add(arenaProgress?.BestForgeWins ?? 0);
		m_arenaDataModel.StatValue.Add(totalArenaWins);
	}

	private void InitBgStatDataModel()
	{
		m_bgDataModel.ModeIcon = 1;
		m_bgDataModel.ModeName = GameStrings.Format("GLOBAL_BATTLEGROUNDS");
		m_bgDataModel.StatName = GameStrings.Get("GLOBAL_PROGRESSION_PROFILE_BATTLEGROUNDS_STAT_TOOLTIP_TITLE");
		m_bgDataModel.StatDesc = GameStrings.Get("GLOBAL_PROGRESSION_PROFILE_BATTLEGROUNDS_STAT_TOOLTIP");
		m_bgDataModel.StatValueDesc.Add("");
		UpdateBgStatDataModel();
	}

	private void UpdateBgStatDataModel()
	{
		m_bgDataModel.StatValue.Clear();
		NetCache.NetCacheBaconRatingInfo bgRating = NetCache.Get().GetNetObject<NetCache.NetCacheBaconRatingInfo>();
		m_bgDataModel.StatValue.Add(bgRating?.Rating ?? 0);
		int top4BGWins = GetTop4BgWins();
		m_bgDataModel.StatValue.Add(top4BGWins);
		GameStrings.PluralNumber plural = new GameStrings.PluralNumber
		{
			m_index = 0,
			m_number = top4BGWins
		};
		m_bgDataModel.StatValueDesc[0] = GameStrings.FormatPlurals("GLOBAL_PROGRESSION_PROFILE_BATTLEGROUNDS_TOP4", new GameStrings.PluralNumber[1] { plural }, top4BGWins);
	}

	private void InitMercsStatDataModel(int totalMercWins)
	{
		m_mercenariesDataModel.ModeIcon = 4;
		m_mercenariesDataModel.ModeName = GameStrings.Format("GLOBAL_MERCENARIES");
		m_mercenariesDataModel.StatName = GameStrings.Get("GLOBAL_PROGRESSION_PROFILE_MERCENARIES_STAT_TOOLTIP_TITLE");
		m_mercenariesDataModel.StatDesc = GameStrings.Get("GLOBAL_PROGRESSION_PROFILE_MERCENARIES_STAT_TOOLTIP");
		UpdateMercsStatDataModel(totalMercWins);
	}

	private void UpdateMercsStatDataModel(int totalMercWins)
	{
		m_mercenariesDataModel.StatValue.Clear();
		NetCache.NetCacheMercenariesPlayerInfo mercPlayerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		m_mercenariesDataModel.StatValue.Add(mercPlayerInfo?.PvpRating ?? 0);
		m_mercenariesDataModel.StatValue.Add(totalMercWins);
	}

	private void UpdateGameModeStats(int totalArenaWins, int totalMercWins)
	{
		UpdateArenaStatDataModel(totalArenaWins);
		UpdateBgStatDataModel();
		UpdateMercsStatDataModel(totalMercWins);
	}
}
