namespace Hearthstone.Startup;

public enum StartupStage
{
	None,
	Start,
	VersioningAssets,
	CheckingForUpdates,
	DownloadAPK,
	DownloadDbf,
	NoAccountFlow,
	ConnectingToBattlenet,
	ConnectingToHearthstone,
	RatingsScreen,
	LaunchGame
}
