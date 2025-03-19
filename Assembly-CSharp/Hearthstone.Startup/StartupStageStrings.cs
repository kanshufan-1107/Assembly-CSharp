using System.Collections.Generic;

namespace Hearthstone.Startup;

public static class StartupStageStrings
{
	private static Dictionary<StartupStage, string> s_stageMap = new Dictionary<StartupStage, string>
	{
		{
			StartupStage.Start,
			"GLUE_STARTUP_START"
		},
		{
			StartupStage.VersioningAssets,
			"GLUE_STARTUP_VERSIONING"
		},
		{
			StartupStage.CheckingForUpdates,
			"GLUE_STARTUP_CHECKING_UPDATES"
		},
		{
			StartupStage.DownloadAPK,
			"GLUE_STARTUP_DOWNLOAD_APK"
		},
		{
			StartupStage.DownloadDbf,
			"GLUE_STARTUP_DOWNLOAD_DBF"
		},
		{
			StartupStage.NoAccountFlow,
			string.Empty
		},
		{
			StartupStage.ConnectingToBattlenet,
			"GLUE_STARTUP_CONNECTING_BNET"
		},
		{
			StartupStage.ConnectingToHearthstone,
			"GLUE_STARTUP_CONNECTING_HEARTHSTONE"
		},
		{
			StartupStage.RatingsScreen,
			string.Empty
		},
		{
			StartupStage.LaunchGame,
			"GLUE_STARTUP_LAUNCHING"
		}
	};

	private static string s_unknownStage = "GLUE_STARTUP_UNKNOWN_STAGE";

	public static string GetStringForStage(StartupStage stage)
	{
		return GameStrings.Get(GetStringKey(stage));
	}

	private static string GetStringKey(StartupStage stage)
	{
		return s_stageMap.GetValueOrDefault(stage, s_unknownStage);
	}
}
