using Blizzard.T5.Core;
using Hearthstone.Core.Streaming;
using PegasusShared;
using UnityEngine;

namespace Hearthstone.Streaming;

public static class DownloadUtils
{
	private static string[] s_suffixes = new string[4] { "GLOBAL_ASSET_DOWNLOAD_BYTE_SYMBOL", "GLOBAL_ASSET_DOWNLOAD_KILOBYTE_SYMBOL", "GLOBAL_ASSET_DOWNLOAD_MEGABYTE_SYMBOL", "GLOBAL_ASSET_DOWNLOAD_GIGABYTE_SYMBOL" };

	private static readonly Map<DownloadTags.Content, string> s_gameModeNameKeys = new Map<DownloadTags.Content, string>
	{
		{
			DownloadTags.Content.Adventure,
			"GLOBAL_MODULE_ADVENTURE"
		},
		{
			DownloadTags.Content.Merc,
			"GLOBAL_MODULE_MERC"
		},
		{
			DownloadTags.Content.Bgs,
			"GLOBAL_MODULE_BGS"
		}
	};

	public static string FormatBytesAsHumanReadable(long bytes)
	{
		int suffixIndex = 0;
		long mostSignificantBytes = 0L;
		long lessSignificantBytes = 0L;
		while (bytes > 0 && suffixIndex < s_suffixes.Length)
		{
			suffixIndex++;
			lessSignificantBytes = mostSignificantBytes;
			mostSignificantBytes = bytes % 1024;
			bytes /= 1024;
		}
		suffixIndex = Mathf.Max(1, suffixIndex);
		int roundedLessSignificantBytes = Mathf.RoundToInt((float)lessSignificantBytes * 10f / 1024f);
		if (roundedLessSignificantBytes == 10)
		{
			mostSignificantBytes++;
			roundedLessSignificantBytes = 0;
		}
		return string.Format(GameStrings.Get("GLOBAL_ASSET_DOWNLOAD_STATUS_DECIMAL_FORMAT"), mostSignificantBytes, roundedLessSignificantBytes, GameStrings.Get(s_suffixes[suffixIndex - 1]));
	}

	public static void CalculateModuleDownloadProgress(TagDownloadStatus status, long totalSize, out long downloadedBytes, out float progress)
	{
		long downloadedPreviously = ((status.BytesTotal > 0) ? (totalSize - status.BytesTotal) : 0);
		downloadedBytes = status.BytesDownloaded + downloadedPreviously;
		if (downloadedBytes < 0)
		{
			Log.Downloader.PrintWarning($"DownloadManagerDialog:CalculateProgress - Calculated downloadedByte is negative. DownloadStatus.BytesDownloaded: {status.BytesDownloaded}; DownloadStatus.BytesTotal: {status.BytesTotal}; TotalSize: {totalSize}");
			downloadedBytes = 0L;
		}
		progress = ((totalSize > 0) ? Mathf.Clamp01((float)downloadedBytes / (float)totalSize) : 0f);
	}

	public static string GetGameModeName(DownloadTags.Content moduleTag)
	{
		if (s_gameModeNameKeys.TryGetValue(moduleTag, out var key))
		{
			return GameStrings.Get(key);
		}
		return "";
	}

	public static bool HasNecessaryModeInstalled(NetCache.NetCacheDisconnectedGame dcGame, out DownloadTags.Content missingTag)
	{
		Log.Downloader.PrintDebug($"Disconnected game: Type({dcGame.GameType}) Format({dcGame.FormatType}) Mission({dcGame.ServerInfo.Mission})");
		missingTag = DownloadTags.Content.Unknown;
		switch (dcGame.GameType)
		{
		case GameType.GT_MERCENARIES_PVP:
		case GameType.GT_MERCENARIES_PVE:
		case GameType.GT_MERCENARIES_PVE_COOP:
		case GameType.GT_MERCENARIES_FRIENDLY:
			missingTag = DownloadTags.Content.Merc;
			break;
		case GameType.GT_VS_AI:
		{
			ScenarioDbId id = (ScenarioDbId)dcGame.ServerInfo.Mission;
			if (id == ScenarioDbId.TB_BACONSHOP_Tutorial || id == ScenarioDbId.TB_BACONSHOP_VS_AI || id == ScenarioDbId.TB_BACONSHOP_DUOS_VS_AI || id == ScenarioDbId.TB_BACONSHOP_DUOS_1_PLAYER_VS_AI)
			{
				missingTag = DownloadTags.Content.Bgs;
				break;
			}
			ScenarioDbfRecord scenario = GameDbf.Scenario.GetRecord(dcGame.ServerInfo.Mission);
			if (scenario != null && scenario.AdventureRecord != null && scenario.AdventureRecord.AdventureDefPrefab != null)
			{
				missingTag = DownloadTags.Content.Adventure;
			}
			break;
		}
		case GameType.GT_BATTLEGROUNDS:
			if (!GameDownloadManagerProvider.Get().IsModuleReadyToPlay(DownloadTags.Content.Bgs))
			{
				missingTag = DownloadTags.Content.Bgs;
			}
			break;
		}
		if (missingTag != 0)
		{
			return GameDownloadManagerProvider.Get().IsModuleReadyToPlay(missingTag);
		}
		return true;
	}
}
