using System;
using Cysharp.Threading.Tasks;
using Hearthstone.Core.Streaming;

namespace Hearthstone.Streaming;

public interface IGameDownloadManager
{
	public delegate void ModuleInstallationStateChangeListener(DownloadTags.Content moduleTag, ModuleState state);

	InterruptionReason InterruptionReason { get; }

	bool IsAnyDownloadRequestedAndIncomplete { get; }

	bool IsInterrupted { get; }

	bool IsNewMobileVersionReleased { get; }

	bool IsDownloading { get; }

	bool IsReadyToPlay { get; }

	bool ShouldNotDownloadOptionalData { get; }

	bool IsVersionStepCompleted { get; }

	bool IsReadyToReadDbf { get; }

	bool ShouldDownloadLocalizedAssets { get; }

	double BytesPerSecond { get; }

	string VersionOverrideUrl { get; }

	string[] DisabledAdventuresForStreaming { get; }

	AssetDownloaderState AssetDownloaderState { get; }

	int MaxDownloadSpeed { get; set; }

	int InGameStreamingDefaultSpeed { get; set; }

	int DownloadSpeedInGame { get; set; }

	DownloadTags.Content[] DownloadableModuleTags { get; }

	event Action VersioningStarted;

	event Action InitialDownloadStarted;

	event Action<int> DbfDownloadProgress;

	event Action<int> ApkDownloadProgress;

	void StartContentDownload(DownloadTags.Content content);

	bool IsAssetDownloaded(string assetGuid);

	bool IsBundleDownloaded(string assetBundleName);

	bool IsBundleShouldAvailable(string assetBundleName);

	bool IsReadyAssetsInTags(string[] tags);

	bool IsCompletedInitialBaseDownload();

	bool IsCompletedInitialModulesDownload();

	bool IsOptionalDownloadCompleted();

	ModuleState GetModuleState(DownloadTags.Content moduleTag);

	bool IsModuleReadyToPlay(DownloadTags.Content moduleTag);

	bool IsModuleRequested(DownloadTags.Content moduleTag);

	bool IsModuleDownloading(DownloadTags.Content moduleTag);

	TagDownloadStatus GetTagDownloadStatus(string[] tags);

	TagDownloadStatus GetCurrentDownloadStatus();

	TagDownloadStatus GetOptionalDownloadStatus();

	TagDownloadStatus GetModuleDownloadStatus(DownloadTags.Content contentTag);

	ContentDownloadStatus GetContentDownloadStatus(DownloadTags.Content contentTag);

	void DownloadModule(DownloadTags.Content moduleTag);

	UniTask DeleteModuleAsync(DownloadTags.Content moduleTag);

	UniTask DeleteOptionalAssetsAsync();

	long GetModuleDownloadSize(DownloadTags.Content moduleTag);

	long GetOptionalAssetsDownloadSize();

	void StartUpdateProcess(bool localeChange, bool resetDownloadFinishReport);

	void StartUpdateProcessForOptional();

	void PauseAllDownloads(bool requestedByPlayer = false);

	void ResumeAllDownloads();

	void DeleteDownloadedData();

	void StopOptionalDownloads();

	void RegisterModuleInstallationStateChangeListener(ModuleInstallationStateChangeListener listener, bool invokeImmediately = true);

	void UnregisterModuleInstallationStateChangeListener(ModuleInstallationStateChangeListener listener);
}
