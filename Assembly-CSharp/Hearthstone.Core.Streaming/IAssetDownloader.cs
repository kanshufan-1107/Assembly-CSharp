using System;

namespace Hearthstone.Core.Streaming;

public interface IAssetDownloader
{
	AssetDownloaderState State { get; }

	bool IsReady { get; }

	bool IsNewMobileVersionReleased { get; }

	bool ShouldNotDownloadOptionalData { get; }

	bool IsVersionChanged { get; }

	bool IsVersionStepCompleted { get; }

	bool IsDbfReady { get; }

	bool AreStringsReady { get; }

	bool DownloadAllFinished { get; set; }

	double BytesPerSecond { get; }

	int MaxDownloadSpeed { get; set; }

	int InGameStreamingDefaultSpeed { get; set; }

	int DownloadSpeedInGame { get; set; }

	string VersionOverrideUrl { get; }

	string[] DisabledAdventuresForStreaming { get; }

	event Action VersioningStarted;

	event Action<int> ApkDownloadProgress;

	event Action<int> DbfDownloadProgress;

	bool Initialize();

	void Update(bool firstCall);

	void Shutdown();

	TagDownloadStatus GetDownloadStatus(string[] tags);

	TagDownloadStatus GetCurrentDownloadStatus();

	void StartDownload(string[] tags, DownloadType downloadType, bool localeChanged);

	void PauseAllDownloads();

	void DeleteDownloadedData();

	long DeleteDownloadedData(string[] tags);

	bool IsBundleDownloaded(string bundleName);

	void OnSceneLoad(SceneMgr.Mode prevMode, SceneMgr.Mode nextMode, object userData);

	void PrepareRestart();

	void DoPostTasksAfterDownload(DownloadType downloadType);

	void SendDownloadStartedTelemetryMessage(DownloadType downloadType, bool localeUpdate, DownloadTags.Content moduleTag = DownloadTags.Content.Unknown);

	void SendDownloadFinishedTelemetryMessage(DownloadType downloadType, bool localeUpdate, DownloadTags.Content moduleTag = DownloadTags.Content.Unknown);

	void SendDownloadStoppedTelemetryMessage(DownloadType downloadType, bool localeUpdate, bool byUser, DownloadTags.Content moduleTag = DownloadTags.Content.Unknown);

	void SendDeleteModuleTelemetryMessage(DownloadTags.Content moduleTag, long deletedSize);

	void SendDeleteOptionalDataTelemetryMessage(long deletedSize);

	void EnterBackgroundMode();

	void ExitFromBackgroundMode();

	void UnknownSourcesListener(string onOff);

	void InstallAPKListener(string status);

	void AllowNotificationListener(string onOff);

	bool IsCurrentVersionHigherOrEqual(string versionStr);
}
