using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blizzard.T5.Configuration;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Cysharp.Threading.Tasks;
using Hearthstone.Core;
using Hearthstone.Core.Streaming;
using Hearthstone.Util;
using UnityEngine;

namespace Hearthstone.Streaming;

public class GameDownloadManager : IService, IGameDownloadManager, IHasUpdate
{
	[Serializable]
	private class SerializedState
	{
		public DownloadTags.Quality MaximumQualityTag;

		public string[] LastRequestedContentTags;

		public DownloadTags.Quality LastInprogressTag;

		public bool ReportInitialDataDownloadFinished;

		public bool ReportOptionalDataDownloadFinished;

		public bool IsInLocaleUpdate;

		public bool PlayerPausedDownload;

		public string[] RequestedModules = new string[0];

		public string[] RequestedModulesForToast = new string[0];
	}

	private const string UPDATE_PROCESS_JOB_NAME = "GameDownloadManager.UpdateProcess";

	private const float PROCESS_DELTA_TIME = 0.5f;

	private const int MIN_DOWNLOAD_SPEED = 51200;

	private const int MAX_DOWNLOAD_SPEED = 52428800;

	protected const float MINIMUM_TIME_BETWEEN_RETRY_ATTEMPTS = 5f;

	private const float DOWNLOAD_PROGRESS_REFRESH_INTERVAL = 1f;

	private const string AUTO_DOWNLOAD_MODULE_1ST_VERSION = "000000_28.2.0";

	private const string AUTO_DOWNLOAD_MODULE_2ND_VERSION = "000000_28.6.0";

	private static readonly DownloadTags.Quality s_startingOptionalQualityTag;

	private static readonly Dictionary<Locale, string> s_localeTags;

	protected IAssetDownloader m_assetDownloader;

	private SerializedState m_serializedState = new SerializedState();

	private int m_prevDownloadSpeed;

	private bool m_requestedInitialAssets;

	private bool m_requestedStreamingAssets;

	private bool m_bRegisterSceneLoadCallback;

	private bool m_refreshCachedModuleState;

	protected float m_timeOfLastAutoResumeAttempt;

	private ContentDownloadStatus m_curContentDownloadStatus = new ContentDownloadStatus();

	private LoginManager m_loginManager;

	private SceneMgr m_sceneMgr;

	private DownloadType m_currentDownloadType;

	private Dictionary<DownloadTags.Content, ModuleState> m_cachedModuleStates = new Dictionary<DownloadTags.Content, ModuleState>();

	private IGameDownloadManager.ModuleInstallationStateChangeListener m_moduleInstallationStateChangeListeners;

	private string[] m_initialBaseTags;

	private string[] m_initialQualityTags;

	private string[] m_optionalQualityTags;

	private readonly DownloadTags.Content[] m_downloadableModuleTags = new DownloadTags.Content[0];

	private int m_frames;

	private bool m_cachedReadyToPlay;

	private float m_refreshedTime = -1f;

	private bool m_cachedModulesDownloaded;

	private float m_modulesDownloadRefreshedTime = -1f;

	private string[] InitialBaseTags
	{
		get
		{
			if (m_initialBaseTags == null || m_initialBaseTags.Length == 0)
			{
				List<string> initialBaseTags = new List<string>();
				initialBaseTags.AddRange(InitialQualityTags);
				initialBaseTags.AddRange(InitialContentTags);
				m_initialBaseTags = initialBaseTags.ToArray();
			}
			return m_initialBaseTags;
		}
		set
		{
			m_initialBaseTags = value;
		}
	}

	private string[] InitialQualityTags
	{
		get
		{
			if (m_initialBaseTags == null)
			{
				List<string> tags = new List<string>();
				foreach (DownloadTags.Quality qualityTag in QualityTagsBetween(DownloadTags.Quality.Unknown, DownloadTags.Quality.Initial, PlatformSettings.ShouldFallbackToLowRes))
				{
					tags.Add(DownloadTags.GetTagString(qualityTag));
				}
				m_initialQualityTags = tags.ToArray();
			}
			return m_initialQualityTags;
		}
	}

	private string[] InitialContentTags
	{
		get
		{
			List<string> initialContentTags = new List<string>();
			if (DisabledAdventuresForStreaming.Length != 0)
			{
				if (AssetManifest.Get() != null)
				{
					AssetManifest.Get().GetTagsInTagGroup(DownloadTags.GetTagGroupString(DownloadTags.TagGroup.Content), ref initialContentTags);
				}
				else
				{
					initialContentTags.AddRange(DisabledAdventuresForStreaming);
				}
			}
			else
			{
				initialContentTags.Add(DownloadTags.GetTagString(DownloadTags.Content.Base));
			}
			return initialContentTags.ToArray();
		}
	}

	private string[] OptionalQualityTags
	{
		get
		{
			if (m_optionalQualityTags == null)
			{
				List<string> tags = new List<string>();
				foreach (DownloadTags.Quality qualityTag in QualityTagsAfter(s_startingOptionalQualityTag, PlatformSettings.ShouldFallbackToLowRes))
				{
					tags.Add(DownloadTags.GetTagString(qualityTag));
				}
				m_optionalQualityTags = tags.ToArray();
			}
			return m_optionalQualityTags;
		}
	}

	public DownloadTags.Content[] DownloadableModuleTags => m_downloadableModuleTags;

	private bool CanAskForCellularPermission => GetLoginManager()?.IsFullLoginFlowComplete ?? false;

	private string SerializedStatePath => Path.Combine(PlatformFilePaths.BasePersistentDataPath, "downloadmanager");

	private DownloadTags.Quality MaximumQualityTag
	{
		get
		{
			return GetSerializedValue(m_serializedState.MaximumQualityTag);
		}
		set
		{
			SetSerializedValue(ref m_serializedState.MaximumQualityTag, value);
		}
	}

	private string[] LastRequestedContentTags
	{
		get
		{
			return GetSerializedValue(m_serializedState.LastRequestedContentTags);
		}
		set
		{
			SetSerializedValue(ref m_serializedState.LastRequestedContentTags, value);
		}
	}

	private DownloadTags.Quality LastInprogressTag
	{
		get
		{
			return GetSerializedValue(m_serializedState.LastInprogressTag);
		}
		set
		{
			SetSerializedValue(ref m_serializedState.LastInprogressTag, value);
		}
	}

	private bool ReportInitialDataDownloadFinished
	{
		get
		{
			return GetSerializedValue(m_serializedState.ReportInitialDataDownloadFinished);
		}
		set
		{
			SetSerializedValue(ref m_serializedState.ReportInitialDataDownloadFinished, value);
		}
	}

	private bool ReportOptionalDataDownloadFinished
	{
		get
		{
			return GetSerializedValue(m_serializedState.ReportOptionalDataDownloadFinished);
		}
		set
		{
			SetSerializedValue(ref m_serializedState.ReportOptionalDataDownloadFinished, value);
		}
	}

	private bool IsInLocaleChange
	{
		get
		{
			return GetSerializedValue(m_serializedState.IsInLocaleUpdate);
		}
		set
		{
			SetSerializedValue(ref m_serializedState.IsInLocaleUpdate, value);
		}
	}

	private string[] RequestedModules
	{
		get
		{
			return GetSerializedValue(m_serializedState.RequestedModules);
		}
		set
		{
			SetSerializedValue(ref m_serializedState.RequestedModules, value);
		}
	}

	private string[] RequestedModulesForToast
	{
		get
		{
			return GetSerializedValue(m_serializedState.RequestedModulesForToast);
		}
		set
		{
			SetSerializedValue(ref m_serializedState.RequestedModulesForToast, value);
		}
	}

	private bool PlayerPausedDownload
	{
		get
		{
			return GetSerializedValue(m_serializedState.PlayerPausedDownload);
		}
		set
		{
			SetSerializedValue(ref m_serializedState.PlayerPausedDownload, value);
		}
	}

	private bool ShouldRunPostTask { get; set; }

	protected virtual bool IsInGame => GetSceneManager()?.IsInGame() ?? false;

	private float FPS { get; set; }

	private float UpdateInterval { get; set; } = 5f;

	private double LastInterval { get; set; }

	public InterruptionReason InterruptionReason
	{
		get
		{
			if (!IsAnyDownloadRequestedAndIncomplete)
			{
				return InterruptionReason.None;
			}
			if (m_currentDownloadType == DownloadType.OPTIONAL_DOWNLOAD && !GetDownloadEnabled())
			{
				return InterruptionReason.Disabled;
			}
			if (PlayerPausedDownload)
			{
				return InterruptionReason.Paused;
			}
			switch (m_assetDownloader.State)
			{
			case AssetDownloaderState.AGENT_IMPEDED:
				return InterruptionReason.AgentImpeded;
			case AssetDownloaderState.DISK_FULL:
				return InterruptionReason.DiskFull;
			case AssetDownloaderState.FETCHING_SIZE:
				return InterruptionReason.Fetching;
			case AssetDownloaderState.AWAITING_WIFI:
				return InterruptionReason.AwaitingWifi;
			case AssetDownloaderState.ERROR:
				return InterruptionReason.Error;
			case AssetDownloaderState.IDLE:
			case AssetDownloaderState.DOWNLOADING:
				return InterruptionReason.None;
			default:
				return InterruptionReason.Unknown;
			}
		}
	}

	public bool IsAnyDownloadRequestedAndIncomplete
	{
		get
		{
			if (LastRequestedContentTags == null || LastRequestedContentTags.Length == 0 || m_assetDownloader == null)
			{
				return false;
			}
			return !m_assetDownloader.GetDownloadStatus(LastRequestedContentTags).Complete;
		}
	}

	public bool IsInterrupted => InterruptionReason != InterruptionReason.None;

	public bool IsNewMobileVersionReleased
	{
		get
		{
			if (m_assetDownloader != null)
			{
				return m_assetDownloader.IsNewMobileVersionReleased;
			}
			return false;
		}
	}

	public bool IsDownloading => m_assetDownloader.State == AssetDownloaderState.DOWNLOADING;

	public bool IsReadyToPlay
	{
		get
		{
			if (GetRealtimeSinceStartup() - m_refreshedTime > 1f)
			{
				if (m_assetDownloader == null)
				{
					m_cachedReadyToPlay = true;
				}
				else if (!m_assetDownloader.IsReady)
				{
					m_cachedReadyToPlay = false;
				}
				else
				{
					m_cachedReadyToPlay = IsCompletedInitialBaseDownload() && !UserAttentionManager.IsBlockedBy(UserAttentionBlocker.INITIAL_DOWNLOAD);
				}
				m_refreshedTime = GetRealtimeSinceStartup();
			}
			return m_cachedReadyToPlay;
		}
	}

	public bool AreModulesReadyToPlay
	{
		get
		{
			if (GetRealtimeSinceStartup() - m_modulesDownloadRefreshedTime > 1f)
			{
				if (m_assetDownloader == null)
				{
					m_cachedModulesDownloaded = true;
				}
				else if (!m_assetDownloader.IsReady)
				{
					m_cachedModulesDownloaded = false;
				}
				else
				{
					m_cachedModulesDownloaded = IsCompletedInitialModulesDownload();
				}
				m_modulesDownloadRefreshedTime = GetRealtimeSinceStartup();
			}
			return m_cachedModulesDownloaded;
		}
	}

	private bool ShouldDownloadOptional
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return false;
			}
			if (m_assetDownloader.DownloadAllFinished || m_assetDownloader.ShouldNotDownloadOptionalData || !DownloadPermissionManager.DownloadEnabled)
			{
				return false;
			}
			return true;
		}
	}

	public bool ShouldNotDownloadOptionalData
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return false;
			}
			return m_assetDownloader.ShouldNotDownloadOptionalData;
		}
	}

	public bool IsVersionStepCompleted
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return true;
			}
			return m_assetDownloader.IsVersionStepCompleted;
		}
	}

	public bool IsReadyToReadDbf
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return true;
			}
			return m_assetDownloader.IsDbfReady;
		}
	}

	public bool IsReadyToReadStrings
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return true;
			}
			return m_assetDownloader.AreStringsReady;
		}
	}

	public double BytesPerSecond
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return 0.0;
			}
			return m_assetDownloader.BytesPerSecond;
		}
	}

	public string VersionOverrideUrl
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return string.Empty;
			}
			return m_assetDownloader.VersionOverrideUrl;
		}
	}

	public string[] DisabledAdventuresForStreaming
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return new string[0];
			}
			return m_assetDownloader.DisabledAdventuresForStreaming;
		}
	}

	public AssetDownloaderState AssetDownloaderState
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return AssetDownloaderState.UNINITIALIZED;
			}
			return m_assetDownloader.State;
		}
	}

	public int MaxDownloadSpeed
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return 0;
			}
			return m_assetDownloader.MaxDownloadSpeed;
		}
		set
		{
			if (m_assetDownloader != null)
			{
				m_assetDownloader.MaxDownloadSpeed = value;
			}
		}
	}

	public int InGameStreamingDefaultSpeed
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return 0;
			}
			return m_assetDownloader.InGameStreamingDefaultSpeed;
		}
		set
		{
			if (m_assetDownloader != null)
			{
				m_assetDownloader.InGameStreamingDefaultSpeed = value;
			}
		}
	}

	public int DownloadSpeedInGame
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return 0;
			}
			return m_assetDownloader.DownloadSpeedInGame;
		}
		set
		{
			if (m_assetDownloader != null)
			{
				m_assetDownloader.DownloadSpeedInGame = value;
			}
		}
	}

	public bool ShouldDownloadLocalizedAssets
	{
		get
		{
			if (m_assetDownloader == null)
			{
				return false;
			}
			if (PlatformSettings.IsMobileRuntimeOS)
			{
				if (m_assetDownloader.ShouldNotDownloadOptionalData)
				{
					return false;
				}
				PauseAllDownloads();
			}
			Log.Downloader.PrintInfo("Begin to download new locale data");
			DeleteSerializedState();
			m_assetDownloader.PrepareRestart();
			m_cachedReadyToPlay = false;
			m_refreshedTime = 0f;
			return true;
		}
	}

	public event Action VersioningStarted
	{
		add
		{
			if (m_assetDownloader != null)
			{
				m_assetDownloader.VersioningStarted += value;
			}
		}
		remove
		{
			if (m_assetDownloader != null)
			{
				m_assetDownloader.VersioningStarted -= value;
			}
		}
	}

	public event Action InitialDownloadStarted;

	public event Action<int> DbfDownloadProgress
	{
		add
		{
			if (m_assetDownloader != null)
			{
				m_assetDownloader.DbfDownloadProgress += value;
			}
		}
		remove
		{
			if (m_assetDownloader != null)
			{
				m_assetDownloader.DbfDownloadProgress -= value;
			}
		}
	}

	public event Action<int> ApkDownloadProgress
	{
		add
		{
			if (m_assetDownloader != null)
			{
				m_assetDownloader.ApkDownloadProgress += value;
			}
		}
		remove
		{
			if (m_assetDownloader != null)
			{
				m_assetDownloader.ApkDownloadProgress -= value;
			}
		}
	}

	static GameDownloadManager()
	{
		s_startingOptionalQualityTag = DownloadTags.Quality.SoundMission;
		s_localeTags = new Dictionary<Locale, string>();
		foreach (Locale locale in Enum.GetValues(typeof(Locale)))
		{
			s_localeTags[locale] = Enum.GetName(typeof(Locale), locale);
		}
	}

	protected IEnumerator<IAsyncJobResult> Job_UpdateProcess()
	{
		bool done = false;
		bool firstCall = true;
		while (!done)
		{
			if (m_assetDownloader != null)
			{
				done = StartContentDownloadWhenReady();
				m_assetDownloader.Update(firstCall);
				ResumeStalledDownloadsIfAble();
				firstCall = false;
			}
			yield return new WaitForDuration(GetUpdateWaitTime());
		}
		Log.Downloader.PrintDebug("UpdateProcess is done");
	}

	private float GetUpdateWaitTime()
	{
		if (!DownloadPermissionManager.DownloadEnabled || IsInGame)
		{
			return 1.5f;
		}
		if (m_assetDownloader == null || !m_assetDownloader.DownloadAllFinished)
		{
			DownloadType currentDownloadType = m_currentDownloadType;
			if (((currentDownloadType != 0 && currentDownloadType != DownloadType.NONE) || !IsReadyToPlay) && (m_currentDownloadType != DownloadType.MODULE_DOWNLOAD || !AreModulesReadyToPlay))
			{
				return 0.5f;
			}
		}
		return 0.05f;
	}

	private bool StartContentDownloadWhenReady()
	{
		if (!IsVersionStepCompleted)
		{
			return false;
		}
		if (!m_assetDownloader.IsReady)
		{
			return false;
		}
		if (!m_refreshCachedModuleState)
		{
			RefreshCachedModuleStates(forceNotify: true);
			m_refreshCachedModuleState = true;
		}
		switch (m_currentDownloadType)
		{
		case DownloadType.NONE:
			m_currentDownloadType = DownloadType.INITIAL_DOWNLOAD;
			if (!IsReadyToPlay && !m_requestedInitialAssets)
			{
				StartInitialDownload();
				m_requestedInitialAssets = true;
			}
			break;
		case DownloadType.INITIAL_DOWNLOAD:
			if (!IsCompletedInitialBaseDownload())
			{
				break;
			}
			UserAttentionManager.StopBlocking(UserAttentionBlocker.INITIAL_DOWNLOAD);
			if (m_requestedInitialAssets)
			{
				HandleInitialDownloadComplete();
			}
			if (PlayerPausedDownload)
			{
				return true;
			}
			if (ShouldDownloadModules())
			{
				m_currentDownloadType = DownloadType.MODULE_DOWNLOAD;
				StartModuleDownload();
				break;
			}
			m_currentDownloadType = DownloadType.OPTIONAL_DOWNLOAD;
			if (ShouldDownloadOptional)
			{
				StartOptionalDownload();
			}
			break;
		case DownloadType.MODULE_DOWNLOAD:
			if (!AreModulesReadyToPlay)
			{
				break;
			}
			if (ShouldDownloadModules())
			{
				StartModuleDownload();
				break;
			}
			m_currentDownloadType = DownloadType.OPTIONAL_DOWNLOAD;
			if (ShouldDownloadOptional)
			{
				StartOptionalDownload();
			}
			break;
		case DownloadType.OPTIONAL_DOWNLOAD:
		{
			if (!m_assetDownloader.DownloadAllFinished && !m_assetDownloader.ShouldNotDownloadOptionalData && !m_requestedStreamingAssets)
			{
				break;
			}
			UserAttentionManager.StopBlocking(UserAttentionBlocker.INITIAL_DOWNLOAD);
			SceneMgr sceneMgr = GetSceneManager();
			if (!m_bRegisterSceneLoadCallback && sceneMgr != null)
			{
				sceneMgr.RegisterScenePreLoadEvent(OnSceneLoad);
				m_bRegisterSceneLoadCallback = true;
			}
			if (Vars.Key("Mobile.StopDownloadAfter").HasValue && m_assetDownloader.DownloadAllFinished)
			{
				DownloadPermissionManager.DownloadEnabled = false;
				m_assetDownloader.DownloadAllFinished = false;
			}
			if (ShouldRunPostTask && (m_assetDownloader.DownloadAllFinished || IsInterrupted))
			{
				ExceptionReporterControl.Get()?.ControlANRMonitor(on: true);
				m_assetDownloader.DoPostTasksAfterDownload(DownloadType.OPTIONAL_DOWNLOAD);
				MobileCallbackManager.SetUpdateCompleted(finished: true);
				ShouldRunPostTask = false;
			}
			if (m_assetDownloader.DownloadAllFinished)
			{
				if (!ReportOptionalDataDownloadFinished)
				{
					m_assetDownloader.SendDownloadFinishedTelemetryMessage(DownloadType.OPTIONAL_DOWNLOAD, IsInLocaleChange);
					ReportOptionalDataDownloadFinished = true;
				}
				IsInLocaleChange = false;
			}
			if (m_assetDownloader.DownloadAllFinished)
			{
				return m_bRegisterSceneLoadCallback;
			}
			return false;
		}
		}
		return false;
	}

	private void StartInitialDownload()
	{
		Log.Downloader.PrintInfo("Starting Initial Download!");
		this.InitialDownloadStarted?.Invoke();
		if (m_assetDownloader.IsVersionChanged)
		{
			DeleteSerializedState();
			IsInLocaleChange = false;
			AutoDownloadModulesForExistingPlayers();
		}
		MaximumQualityTag = DownloadTags.Quality.Initial;
		string qualityStr = Vars.Key("Mobile.StopDownloadAfter").GetStr(string.Empty);
		if (!string.IsNullOrEmpty(qualityStr))
		{
			Log.Downloader.PrintInfo("The downloading will be stopped after " + qualityStr + ".");
			DownloadTags.Quality quality = DownloadTags.GetQualityTag(qualityStr);
			if (quality == DownloadTags.Quality.Strings || quality == DownloadTags.Quality.Dbf || quality == DownloadTags.Quality.Essential)
			{
				MaximumQualityTag = quality;
			}
		}
		HearthstoneApplication.SendStartupTimeTelemetry("GameDownloadManager.StartInitialBaseDownload");
		StartContentDownload(InitialContentTags);
		ProcessPostDownloadStart();
	}

	private void HandleInitialDownloadComplete()
	{
		Log.Downloader.PrintInfo("Initial Download Finished!");
		m_assetDownloader.DoPostTasksAfterDownload(DownloadType.INITIAL_DOWNLOAD);
		if (!ReportInitialDataDownloadFinished)
		{
			m_assetDownloader.SendDownloadFinishedTelemetryMessage(DownloadType.INITIAL_DOWNLOAD, IsInLocaleChange);
			ReportInitialDataDownloadFinished = true;
		}
	}

	private void StartModuleDownload()
	{
		Log.Downloader.PrintInfo(string.Format("Starting Module Download. Modules Requested = {0} : PlayerPausedDownload = {1}", string.Join(", ", RequestedModules.Select((string t) => $"{t} : {GetModuleState(DownloadTags.GetContentTag(t))}").ToArray()), PlayerPausedDownload));
		if (PlayerPausedDownload || RequestedModules.Any((string tagString) => GetModuleState(DownloadTags.GetContentTag(tagString)) == ModuleState.Downloading))
		{
			return;
		}
		MaximumQualityTag = DownloadTags.Quality.Initial;
		string[] requestedModules = RequestedModules;
		foreach (string tagString2 in requestedModules)
		{
			DownloadTags.Content moduleTag = DownloadTags.GetContentTag(tagString2);
			if (GetModuleState(moduleTag) == ModuleState.Queued)
			{
				StartContentDownload(new string[1] { tagString2 });
				ProcessPostDownloadStart();
				SetModuleState(moduleTag, ModuleState.Downloading);
				Log.Downloader.PrintInfo("Started Module Download. Module = " + tagString2);
				break;
			}
		}
	}

	private void HandleModuleDownloadComplete(DownloadTags.Content moduleTag)
	{
		SetModuleState(moduleTag, ModuleState.ReadyToPlay);
		int indexToRemove = Array.IndexOf(RequestedModulesForToast, DownloadTags.GetTagString(moduleTag));
		if (indexToRemove != -1)
		{
			SocialToastMgr.Get()?.AddToast(UserAttentionBlocker.NONE, GameStrings.Format("GLUE_GAME_MODE_MODULE_DOWNLOADED", DownloadUtils.GetGameModeName(moduleTag)));
			RequestedModulesForToast = RequestedModulesForToast.Where((string source, int index) => index != indexToRemove).ToArray();
		}
		m_assetDownloader.SendDownloadFinishedTelemetryMessage(DownloadType.MODULE_DOWNLOAD, IsInLocaleChange, moduleTag);
	}

	private void StartOptionalDownload()
	{
		Log.Downloader.PrintInfo($"Starting Optional Download! : PlayerPausedDownload = {PlayerPausedDownload}");
		if (PlayerPausedDownload)
		{
			return;
		}
		LastInprogressTag = GetNextTag(DownloadTags.Quality.Initial);
		if (!Vars.Key("Mobile.StopDownloadAfter").HasValue)
		{
			MaximumQualityTag = DownloadTags.GetLastEnum<DownloadTags.Quality>();
		}
		else
		{
			string qualityStr = Vars.Key("Mobile.StopDownloadAfter").GetStr(string.Empty);
			DownloadTags.Quality quality = DownloadTags.GetQualityTag(qualityStr);
			if (quality != 0)
			{
				Log.Downloader.PrintInfo("Optional data will be stopped after downloading '{0}'", qualityStr);
				MaximumQualityTag = quality;
			}
			else
			{
				Log.Downloader.PrintError("Unknown quality tag name has been used from deeplink: {0}", qualityStr);
				MaximumQualityTag = DownloadTags.GetLastEnum<DownloadTags.Quality>();
			}
		}
		List<string> contentTags = InitialContentTags.ToList();
		contentTags.AddRange(RequestedModules);
		StartContentDownload(contentTags.ToArray());
		ProcessPostDownloadStart();
		m_requestedStreamingAssets = true;
	}

	private void ProcessPostDownloadStart()
	{
		MobileCallbackManager.SetUpdateCompleted(finished: false);
		ShouldRunPostTask = true;
	}

	private string[] CombineWithAllAppropriateTags(params string[] tags)
	{
		List<string> list = new List<string>(AppropriateQualityTags());
		list.AddRange(tags);
		return list.ToArray();
	}

	private string[] AppropriateQualityTags()
	{
		return QualityTagsBetween(LastInprogressTag, MaximumQualityTag, PlatformSettings.ShouldFallbackToLowRes).Select(DownloadTags.GetTagString).ToArray();
	}

	private string[] AppropriateLocaleTags()
	{
		List<string> localeTags = new List<string> { DownloadTags.GetTagString(DownloadTags.Locale.EnUS) };
		Locale selectedLocale = Localization.GetLocale();
		if (selectedLocale != Locale.UNKNOWN && selectedLocale != 0)
		{
			localeTags.Add(s_localeTags[selectedLocale]);
		}
		return localeTags.ToArray();
	}

	private void Paused()
	{
		if (PlatformSettings.IsMobileRuntimeOS && m_assetDownloader.State == AssetDownloaderState.DOWNLOADING)
		{
			if (PlatformSettings.RuntimeOS == OSCategory.iOS)
			{
				m_assetDownloader.SendDownloadStoppedTelemetryMessage(m_currentDownloadType, IsInLocaleChange, byUser: false, GetCurrentDownloadingModuleTag());
			}
			m_prevDownloadSpeed = MaxDownloadSpeed;
			MaxDownloadSpeed = 0;
			m_assetDownloader.EnterBackgroundMode();
		}
	}

	private void Unpaused()
	{
		if (PlatformSettings.IsMobileRuntimeOS && IsAnyDownloadRequestedAndIncomplete && !IsInterrupted)
		{
			if (PlatformSettings.RuntimeOS == OSCategory.iOS)
			{
				m_assetDownloader.SendDownloadStartedTelemetryMessage(m_currentDownloadType, IsInLocaleChange, GetCurrentDownloadingModuleTag());
			}
			MaxDownloadSpeed = m_prevDownloadSpeed;
			m_assetDownloader.ExitFromBackgroundMode();
		}
	}

	private void SetSerializedValue<T>(ref T dest, T value)
	{
		bool num = !object.Equals(dest, value);
		dest = value;
		if (num)
		{
			SerializeState();
		}
	}

	private T GetSerializedValue<T>(T value)
	{
		if (m_serializedState == null)
		{
			TryDeserializeState();
		}
		return value;
	}

	private void SerializeState()
	{
		try
		{
			File.WriteAllText(SerializedStatePath, JsonUtility.ToJson(m_serializedState, !HearthstoneApplication.IsPublic()));
		}
		catch (Exception ex)
		{
			Log.Downloader.PrintError("Failed to serialize state: " + ex);
		}
	}

	private void TryDeserializeState()
	{
		string path = SerializedStatePath;
		if (!File.Exists(path))
		{
			m_serializedState = new SerializedState();
			return;
		}
		try
		{
			m_serializedState = JsonUtility.FromJson<SerializedState>(File.ReadAllText(path));
		}
		catch (Exception ex)
		{
			Log.Downloader.PrintError("Failed to deserialize state: " + ex);
		}
	}

	private void DeleteSerializedState()
	{
		string[] requestedModules = RequestedModules;
		string path = SerializedStatePath;
		if (File.Exists(path))
		{
			try
			{
				File.Delete(path);
				TryDeserializeState();
			}
			catch (Exception ex)
			{
				Error.AddDevFatal("Failed to delete the downloader state file({0}): {1}", path, ex.Message);
			}
		}
		RequestedModules = requestedModules;
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		yield return HearthstoneApplication.Get().DataTransferDependency;
		if (!MobileCallbackManager.IsReadyVersionCodeInStore)
		{
			yield return new WaitForDuration(0.5f);
		}
		bool isDemo = serviceLocator.Get<DemoMgr>().IsDemo();
		if (Initialize(isDemo))
		{
			StartUpdateProcess(localeChange: false);
			HearthstoneApplication.Get().Resetting += CreateUpdateProcessJob;
			HearthstoneApplication.Get().Paused += Paused;
			HearthstoneApplication.Get().Unpaused += Unpaused;
		}
	}

	public Type[] GetDependencies()
	{
		return new Type[3]
		{
			typeof(DemoMgr),
			typeof(VersionConfigurationService),
			typeof(MobileCallbackManager)
		};
	}

	public void Update()
	{
		if (!PlatformSettings.IsMobileRuntimeOS || AssetDownloaderState != AssetDownloaderState.DOWNLOADING || BytesPerSecond == 0.0 || !IsInGame)
		{
			return;
		}
		if (LastInterval == 0.0)
		{
			LastInterval = Time.realtimeSinceStartup;
			return;
		}
		m_frames++;
		float currentTime = Time.realtimeSinceStartup;
		if ((double)currentTime > LastInterval + (double)UpdateInterval)
		{
			FPS = (float)m_frames / (float)((double)currentTime - LastInterval);
			bool speedDown = FPS < (float)(GraphicsManager.DefaultFPS - 5);
			m_frames = 0;
			LastInterval = Time.realtimeSinceStartup;
			AdjustDownloadSpeed(speedDown);
		}
	}

	private void AdjustDownloadSpeed(bool speedDown)
	{
		Log.Downloader.PrintDebug($"FPS:{FPS}, BPS:{BytesPerSecond}");
		if (speedDown)
		{
			if (MaxDownloadSpeed != 51200)
			{
				int nextSpeed = (int)(Math.Min(BytesPerSecond, 52428800.0) * 0.9);
				if (nextSpeed < 51200)
				{
					nextSpeed = 51200;
				}
				MaxDownloadSpeed = nextSpeed;
			}
		}
		else if (MaxDownloadSpeed != 0)
		{
			int nextSpeed2 = (int)(Math.Max(BytesPerSecond, 52428800.0) * 1.1);
			if (nextSpeed2 > 52428800)
			{
				nextSpeed2 = 0;
			}
			MaxDownloadSpeed = nextSpeed2;
		}
	}

	public void Shutdown()
	{
		if (m_requestedStreamingAssets && PlatformSettings.IsMobileRuntimeOS)
		{
			GetSceneManager()?.UnregisterScenePreLoadEvent(OnSceneLoad);
		}
		if (m_assetDownloader != null)
		{
			m_assetDownloader.Shutdown();
		}
		RemoveDownloadCheats();
	}

	private bool Initialize(bool isDemo)
	{
		bool isInitialized = false;
		if (ShouldInitialize())
		{
			AddDownloadCheats();
			TryDeserializeState();
			IAssetDownloader assetDownloader2;
			if (!Application.isEditor)
			{
				IAssetDownloader assetDownloader = new RuntimeAssetDownloader();
				assetDownloader2 = assetDownloader;
			}
			else
			{
				IAssetDownloader assetDownloader = new EditorAssetDownloader();
				assetDownloader2 = assetDownloader;
			}
			m_assetDownloader = assetDownloader2;
			if (!m_assetDownloader.Initialize())
			{
				return false;
			}
			isInitialized = true;
		}
		return isInitialized;
		bool ShouldInitialize()
		{
			if (isDemo)
			{
				return false;
			}
			if (Application.isEditor)
			{
				if (PlatformSettings.IsEmulating)
				{
					return EditorAssetDownloader.Mode != EditorAssetDownloader.DownloadMode.None;
				}
				return false;
			}
			return PlatformSettings.IsMobileRuntimeOS;
		}
	}

	private void RefreshCachedModuleStates(bool forceNotify)
	{
		DownloadTags.Content[] downloadableModuleTags = DownloadableModuleTags;
		foreach (DownloadTags.Content moduleTag in downloadableModuleTags)
		{
			ModuleState state = GetRefreshedModuleState(moduleTag);
			SetModuleState(moduleTag, state, forceNotify);
			Option optionKey = Option.INVALID;
			if (forceNotify && (state == ModuleState.ReadyToPlay || state == ModuleState.FullyInstalled))
			{
				switch (moduleTag)
				{
				case DownloadTags.Content.Bgs:
					optionKey = Option.BATTLEGROUND_DATA_EXISTS;
					break;
				case DownloadTags.Content.Merc:
					optionKey = Option.MERCENARY_DATA_EXISTS;
					break;
				case DownloadTags.Content.Adventure:
					optionKey = Option.ADVENTURE_DATA_EXISTS;
					break;
				}
				if (optionKey != 0)
				{
					Options.Get().SetBool(optionKey, val: true);
				}
			}
		}
	}

	private void AutoDownloadModulesForExistingPlayers()
	{
		if (m_assetDownloader.IsCurrentVersionHigherOrEqual("000000_28.6.0"))
		{
			return;
		}
		bool bgs_only = m_assetDownloader.IsCurrentVersionHigherOrEqual("000000_28.2.0");
		List<string> requestedModulesList = RequestedModules.ToList();
		DownloadTags.Content[] downloadableModuleTags = DownloadableModuleTags;
		foreach (DownloadTags.Content moduleTag in downloadableModuleTags)
		{
			if (!bgs_only || moduleTag == DownloadTags.Content.Bgs)
			{
				string moduleTagStr = DownloadTags.GetTagString(moduleTag);
				if (!requestedModulesList.Contains(moduleTagStr))
				{
					DownloadModule(moduleTag);
				}
			}
		}
		Log.Downloader.PrintDebug("Will auto download modules = " + string.Join(", ", RequestedModules.Select((string t) => t ?? "").ToArray()));
	}

	private ModuleState GetRefreshedModuleState(DownloadTags.Content moduleTag)
	{
		if (!IsModuleRequestedInternal(moduleTag))
		{
			return ModuleState.NotRequested;
		}
		if (!IsModuleReadyToPlayInternal(moduleTag))
		{
			return ModuleState.Queued;
		}
		if (!IsModuleFullyInstalledInternal(moduleTag))
		{
			return ModuleState.ReadyToPlay;
		}
		return ModuleState.FullyInstalled;
	}

	protected virtual bool GetDownloadEnabled()
	{
		return DownloadPermissionManager.DownloadEnabled;
	}

	public void StartContentDownload(DownloadTags.Content contentTag)
	{
		StartContentDownload(new string[1] { DownloadTags.GetTagString(contentTag) });
	}

	public void DownloadModule(DownloadTags.Content moduleTag)
	{
		if (!DownloadableModuleTags.Contains(moduleTag))
		{
			Log.Downloader.PrintWarning(string.Format("{0}: Trying to download content : {1} which is not modularized.", "GameDownloadManager", moduleTag));
			return;
		}
		List<string> modules = RequestedModules.ToList();
		string moduleTagStr = DownloadTags.GetTagString(moduleTag);
		if (modules.Contains(moduleTagStr))
		{
			return;
		}
		modules.Add(moduleTagStr);
		RequestedModules = modules.ToArray();
		if (!RequestedModulesForToast.Contains(moduleTagStr))
		{
			RequestedModulesForToast = new List<string>(RequestedModulesForToast) { moduleTagStr }.ToArray();
		}
		SetModuleState(moduleTag, ModuleState.Queued);
		if (m_currentDownloadType != 0 && m_currentDownloadType != DownloadType.MODULE_DOWNLOAD)
		{
			if (m_currentDownloadType == DownloadType.OPTIONAL_DOWNLOAD)
			{
				StopOptionalDownloads();
			}
			StartUpdateProcess(localeChange: false, resetDownloadFinishReport: true);
		}
	}

	public async UniTask DeleteModuleAsync(DownloadTags.Content moduleTag)
	{
		if (m_assetDownloader == null)
		{
			Log.Downloader.PrintWarning("GameDownloadManager: Skipping DeleteModule as m_assetDownloader is null.");
			return;
		}
		Log.Downloader.PrintDebug(string.Format("{0}: DeleteModule. moduleTag = {1}", "GameDownloadManager", moduleTag));
		string moduleTagStr = DownloadTags.GetTagString(moduleTag);
		List<string> tags = new List<string> { moduleTagStr };
		foreach (DownloadTags.Quality qualityTag in QualityTagsAfter(DownloadTags.Quality.Initial, PlatformSettings.ShouldFallbackToLowRes))
		{
			tags.Add(DownloadTags.GetTagString(qualityTag));
		}
		long deletedDataSize = await UniTask.RunOnThreadPool(() => m_assetDownloader.DeleteDownloadedData(tags.ToArray()));
		bool num = !PlayerPausedDownload && IsAnyDownloadRequestedAndIncomplete && LastRequestedContentTags.Contains(moduleTagStr);
		RemoveModuleTag(moduleTag);
		SetModuleState(moduleTag, ModuleState.NotRequested);
		if (moduleTag == DownloadTags.Content.Bgs && Options.Get() != null)
		{
			Options.Get().SetBool(Option.HAS_SEEN_BG_DOWNLOAD_FINISHED_POPUP, val: false);
		}
		if (num)
		{
			if (m_currentDownloadType == DownloadType.MODULE_DOWNLOAD)
			{
				PauseAllDownloads();
				StartUpdateJobIfNotRunning();
			}
			else if (m_currentDownloadType == DownloadType.OPTIONAL_DOWNLOAD)
			{
				StopOptionalDownloads();
				if (ShouldDownloadOptional)
				{
					StartOptionalDownload();
				}
			}
		}
		m_assetDownloader.SendDeleteModuleTelemetryMessage(moduleTag, deletedDataSize);
	}

	public async UniTask DeleteOptionalAssetsAsync()
	{
		if (m_assetDownloader == null)
		{
			Log.Downloader.PrintWarning("GameDownloadManager: Skipping DeleteOptionalAssets as m_assetDownloader is null.");
			return;
		}
		Log.Downloader.PrintDebug("GameDownloadManager: DeleteOptionalAssets");
		StopOptionalDownloads();
		List<string> tags = new List<string>();
		tags.Add(DownloadTags.GetTagString(DownloadTags.Content.Base));
		tags.AddRange(RequestedModules);
		tags.AddRange(OptionalQualityTags);
		long deletedDataSize = await UniTask.RunOnThreadPool(() => m_assetDownloader.DeleteDownloadedData(tags.ToArray()));
		m_assetDownloader.DownloadAllFinished = false;
		if (LastRequestedContentTags.Intersect(OptionalQualityTags).Any())
		{
			LastRequestedContentTags = Array.Empty<string>();
		}
		m_assetDownloader.SendDeleteOptionalDataTelemetryMessage(deletedDataSize);
	}

	public TagDownloadStatus GetModuleDownloadStatus(DownloadTags.Content moduleTag)
	{
		List<string> tags = InitialQualityTags.ToList();
		tags.Add(DownloadTags.GetTagString(moduleTag));
		return GetTagDownloadStatus(tags.ToArray());
	}

	public long GetModuleDownloadSize(DownloadTags.Content moduleTag)
	{
		long downloadSize = 0L;
		List<string> tags = InitialQualityTags.ToList();
		tags.Add(DownloadTags.GetTagString(moduleTag));
		IAssetManifest manifest = AssetManifest.Get();
		foreach (string bundle in manifest.GetAssetBundleNamesForTags(tags.ToArray()))
		{
			downloadSize += manifest.GetBundleSize(bundle);
		}
		return downloadSize;
	}

	public long GetOptionalAssetsDownloadSize()
	{
		long downloadSize = 0L;
		List<string> tags = new List<string>();
		tags.Add(DownloadTags.GetTagString(DownloadTags.Content.Base));
		tags.AddRange(RequestedModules);
		tags.AddRange(OptionalQualityTags);
		IAssetManifest manifest = AssetManifest.Get();
		foreach (string bundle in manifest.GetAssetBundleNamesForTags(tags.ToArray()))
		{
			downloadSize += manifest.GetBundleSize(bundle);
		}
		return downloadSize;
	}

	private void RemoveModuleTag(DownloadTags.Content moduleTag)
	{
		List<string> requestedModuleTags = RequestedModules.ToList();
		requestedModuleTags.Remove(DownloadTags.GetTagString(moduleTag));
		RequestedModules = requestedModuleTags.ToArray();
		List<string> lastRequestedTags = LastRequestedContentTags.ToList();
		lastRequestedTags.Remove(DownloadTags.GetTagString(moduleTag));
		LastRequestedContentTags = lastRequestedTags.ToArray();
	}

	public void StopOptionalDownloads()
	{
		bool assetDownloaderReady = m_assetDownloader?.IsReady ?? false;
		if (assetDownloaderReady && IsCompletedInitialBaseDownload() && IsCompletedInitialModulesDownload())
		{
			Log.Downloader.PrintDebug("Requesting optional downloads to stop");
			PauseAllDownloads();
		}
		else
		{
			Log.Downloader.PrintDebug("Could not stop optional downloads: {0}", (!assetDownloaderReady) ? "Asset downloader not ready" : "Initial downloads not complete");
		}
	}

	public void RegisterModuleInstallationStateChangeListener(IGameDownloadManager.ModuleInstallationStateChangeListener listener, bool invokeImmediately = true)
	{
		m_moduleInstallationStateChangeListeners = (IGameDownloadManager.ModuleInstallationStateChangeListener)Delegate.Combine(m_moduleInstallationStateChangeListeners, listener);
		if (invokeImmediately)
		{
			DownloadTags.Content[] downloadableModuleTags = DownloadableModuleTags;
			foreach (DownloadTags.Content tag in downloadableModuleTags)
			{
				listener?.Invoke(tag, GetModuleState(tag));
			}
		}
	}

	public void UnregisterModuleInstallationStateChangeListener(IGameDownloadManager.ModuleInstallationStateChangeListener listener)
	{
		m_moduleInstallationStateChangeListeners = (IGameDownloadManager.ModuleInstallationStateChangeListener)Delegate.Remove(m_moduleInstallationStateChangeListeners, listener);
	}

	public ModuleState GetModuleState(DownloadTags.Content moduleTag)
	{
		if (!DownloadableModuleTags.Contains(moduleTag))
		{
			return ModuleState.FullyInstalled;
		}
		if (!m_cachedModuleStates.TryGetValue(moduleTag, out var state))
		{
			if (m_cachedModuleStates.Count == 0)
			{
				Log.Downloader.PrintWarning("GameDownloadManager: GetModuleState is accessed when module states is not initialized!");
			}
			return ModuleState.Unknown;
		}
		return state;
	}

	private void SetModuleState(DownloadTags.Content moduleTag, ModuleState state, bool forceNotify = false)
	{
		ModuleState oldState;
		bool notify = forceNotify || !m_cachedModuleStates.TryGetValue(moduleTag, out oldState) || oldState != state;
		Log.Downloader.PrintDebug(string.Format("SetModuleState moduleTag = {0} : oldState = {1} : NewState = {2} : forceNotify = {3} : m_assetDownloader.IsReady = {4}", moduleTag, m_cachedModuleStates.ContainsKey(moduleTag) ? m_cachedModuleStates[moduleTag].ToString() : "Null", state, forceNotify, m_assetDownloader?.IsReady));
		m_cachedModuleStates[moduleTag] = state;
		m_modulesDownloadRefreshedTime = -1f;
		if (notify)
		{
			NotifyModuleStateChanged(moduleTag);
		}
	}

	private void NotifyModuleStateChanged(DownloadTags.Content moduleTag)
	{
		m_moduleInstallationStateChangeListeners?.Invoke(moduleTag, GetModuleState(moduleTag));
	}

	public bool IsCompletedInitialBaseDownload()
	{
		if (m_assetDownloader == null || m_assetDownloader.DownloadAllFinished || m_assetDownloader.ShouldNotDownloadOptionalData)
		{
			return true;
		}
		if (m_assetDownloader.IsReady)
		{
			return GetTagDownloadStatus(InitialBaseTags).Complete;
		}
		return false;
	}

	public bool IsCompletedInitialModulesDownload()
	{
		if (m_assetDownloader == null || RequestedModules == null || RequestedModules.Length == 0 || m_assetDownloader.ShouldNotDownloadOptionalData)
		{
			return true;
		}
		bool modulesDownloaded = true;
		string[] requestedModules = RequestedModules;
		for (int i = 0; i < requestedModules.Length; i++)
		{
			DownloadTags.Content tag = DownloadTags.GetContentTag(requestedModules[i]);
			ModuleState moduleState = GetModuleState(tag);
			if (moduleState >= ModuleState.Downloading && moduleState < ModuleState.ReadyToPlay)
			{
				if (!IsModuleReadyToPlayInternal(tag))
				{
					modulesDownloaded = false;
				}
				else
				{
					HandleModuleDownloadComplete(tag);
				}
			}
		}
		return modulesDownloaded;
	}

	private bool ShouldDownloadModules()
	{
		if (ShouldNotDownloadOptionalData)
		{
			return false;
		}
		string[] requestedModules = RequestedModules;
		for (int i = 0; i < requestedModules.Length; i++)
		{
			DownloadTags.Content moduleTag = DownloadTags.GetContentTag(requestedModules[i]);
			if (!IsModuleReadyToPlay(moduleTag))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsOptionalDownloadCompleted()
	{
		if (m_assetDownloader == null)
		{
			return true;
		}
		if (m_assetDownloader.IsReady)
		{
			return GetOptionalDownloadStatus().Complete;
		}
		return false;
	}

	public bool IsModuleRequested(DownloadTags.Content moduleTag)
	{
		if (m_assetDownloader == null)
		{
			return true;
		}
		return GetModuleState(moduleTag) > ModuleState.NotRequested;
	}

	private bool IsModuleRequestedInternal(DownloadTags.Content moduleTag)
	{
		if (m_assetDownloader == null)
		{
			return true;
		}
		return RequestedModules.Contains(DownloadTags.GetTagString(moduleTag));
	}

	public bool IsModuleDownloading(DownloadTags.Content moduleTag)
	{
		ModuleState moduleState = GetModuleState(moduleTag);
		if (moduleState > ModuleState.NotRequested)
		{
			return moduleState < ModuleState.ReadyToPlay;
		}
		return false;
	}

	public bool IsModuleReadyToPlay(DownloadTags.Content moduleTag)
	{
		if (m_assetDownloader == null)
		{
			return true;
		}
		return GetModuleState(moduleTag) >= ModuleState.ReadyToPlay;
	}

	private bool IsModuleReadyToPlayInternal(DownloadTags.Content moduleTag)
	{
		if (m_assetDownloader == null)
		{
			return true;
		}
		if (!DownloadableModuleTags.Contains(moduleTag))
		{
			return true;
		}
		if (m_assetDownloader.IsReady)
		{
			return GetModuleDownloadStatus(moduleTag).Complete;
		}
		return false;
	}

	private bool IsModuleFullyInstalledInternal(DownloadTags.Content moduleTag)
	{
		if (m_assetDownloader == null)
		{
			return true;
		}
		List<string> tags = new List<string>();
		tags.AddRange(InitialQualityTags);
		tags.AddRange(OptionalQualityTags);
		tags.Add(DownloadTags.GetTagString(moduleTag));
		return GetTagDownloadStatus(tags.ToArray()).Complete;
	}

	private void StartContentDownload(string[] contentTag)
	{
		if (m_assetDownloader == null)
		{
			Log.Downloader.PrintError("StartContentDownload: AssetDownloader is not ready yet!");
			return;
		}
		string[] tags = (LastRequestedContentTags = CombineWithAllAppropriateTags(contentTag));
		Log.Downloader.PrintInfo(string.Format("Start content download : downloadType = {0} : tags = {1}", m_currentDownloadType, string.Join(", ", tags)));
		ExceptionReporterControl.Get()?.ControlANRMonitor(on: false);
		m_assetDownloader.SendDownloadStartedTelemetryMessage(m_currentDownloadType, IsInLocaleChange, GetCurrentDownloadingModuleTag());
		m_assetDownloader.StartDownload(tags, m_currentDownloadType, IsInLocaleChange);
	}

	private DownloadTags.Content GetCurrentDownloadingModuleTag()
	{
		if (m_currentDownloadType != DownloadType.MODULE_DOWNLOAD)
		{
			return DownloadTags.Content.Unknown;
		}
		string[] requestedModules = RequestedModules;
		for (int i = 0; i < requestedModules.Length; i++)
		{
			DownloadTags.Content moduleTag = DownloadTags.GetContentTag(requestedModules[i]);
			if (GetModuleState(moduleTag) == ModuleState.Downloading)
			{
				return moduleTag;
			}
		}
		return DownloadTags.Content.Unknown;
	}

	protected void ResumeStalledDownloadsIfAble()
	{
		if (ShouldRestartStalledDownloads())
		{
			m_timeOfLastAutoResumeAttempt = GetRealtimeSinceStartup();
			ResumeAllDownloadsInternal();
		}
	}

	protected virtual float GetRealtimeSinceStartup()
	{
		return Time.realtimeSinceStartup;
	}

	private bool ShouldRestartStalledDownloads()
	{
		if (m_assetDownloader != null && !IsAutoResumeThrottled())
		{
			if (!AreInitialDownloadsStalled() && !AreOptionalDownloadsStalled())
			{
				return AreModulesDownloadStalled();
			}
			return true;
		}
		return false;
	}

	private bool AreInitialDownloadsStalled()
	{
		if (!IsReadyToPlay && InitialDownloadDialog.IsVisible && InterruptionReason == InterruptionReason.None && m_assetDownloader.State == AssetDownloaderState.FIRST_DOWNLOAD_WAIT)
		{
			return true;
		}
		return false;
	}

	private bool AreOptionalDownloadsStalled()
	{
		if (MaximumQualityTag != DownloadTags.Quality.Initial && IsAnyDownloadRequestedAndIncomplete && m_assetDownloader.State != AssetDownloaderState.DOWNLOADING && IsReadyToPlay && m_requestedStreamingAssets)
		{
			InterruptionReason interruptionReason = InterruptionReason;
			if (IsInterrupted && interruptionReason != InterruptionReason.Error)
			{
				return interruptionReason == InterruptionReason.Unknown;
			}
			return true;
		}
		return false;
	}

	private bool AreModulesDownloadStalled()
	{
		if (MaximumQualityTag == DownloadTags.Quality.Initial && IsAnyDownloadRequestedAndIncomplete && m_assetDownloader.State != AssetDownloaderState.DOWNLOADING && IsReadyToPlay && !AreModulesReadyToPlay)
		{
			InterruptionReason interruptionReason = InterruptionReason;
			if (IsInterrupted && interruptionReason != InterruptionReason.Error)
			{
				return interruptionReason == InterruptionReason.Unknown;
			}
			return true;
		}
		return false;
	}

	private bool IsAutoResumeThrottled()
	{
		return GetRealtimeSinceStartup() - m_timeOfLastAutoResumeAttempt < 5f;
	}

	public bool IsAssetDownloaded(string assetGuid)
	{
		if (m_assetDownloader == null)
		{
			if (string.IsNullOrEmpty(assetGuid))
			{
				return false;
			}
			return true;
		}
		if (!string.IsNullOrEmpty(assetGuid) && AssetManifest.Get().TryGetDirectBundleFromGuid(assetGuid, out var bundleName))
		{
			if (!PlatformFilePaths.IsBundleInBinary(bundleName))
			{
				return IsBundleDownloaded(bundleName);
			}
			return true;
		}
		return false;
	}

	public bool IsBundleDownloaded(string bundleName)
	{
		if (m_assetDownloader == null)
		{
			return true;
		}
		return m_assetDownloader.IsBundleDownloaded(bundleName);
	}

	public bool IsBundleShouldAvailable(string assetBundleName)
	{
		List<string> bundleTags = new List<string>();
		AssetManifest.Get()?.GetTagsFromAssetBundle(assetBundleName, bundleTags);
		if (bundleTags.Count <= 0)
		{
			return true;
		}
		return IsReadyAssetsInTags(bundleTags.ToArray());
	}

	public bool IsReadyAssetsInTags(string[] tags)
	{
		if (m_assetDownloader == null)
		{
			return true;
		}
		return m_assetDownloader.GetDownloadStatus(tags).Complete;
	}

	public TagDownloadStatus GetTagDownloadStatus(string[] tags)
	{
		if (m_assetDownloader == null)
		{
			return new TagDownloadStatus
			{
				Complete = true
			};
		}
		return m_assetDownloader.GetDownloadStatus(tags);
	}

	public TagDownloadStatus GetOptionalDownloadStatus()
	{
		if (m_assetDownloader == null || m_assetDownloader.ShouldNotDownloadOptionalData)
		{
			return new TagDownloadStatus
			{
				Complete = false
			};
		}
		List<string> tags = new List<string>();
		tags.Add(DownloadTags.GetTagString(DownloadTags.Content.Base));
		tags.AddRange(RequestedModules);
		tags.AddRange(OptionalQualityTags);
		return GetTagDownloadStatus(tags.ToArray());
	}

	public TagDownloadStatus GetCurrentDownloadStatus()
	{
		if (m_assetDownloader == null)
		{
			return null;
		}
		return m_assetDownloader.GetCurrentDownloadStatus();
	}

	public ContentDownloadStatus GetContentDownloadStatus(DownloadTags.Content contentTag)
	{
		return GetContentDownloadStatus(DownloadTags.GetTagString(contentTag));
	}

	protected IEnumerable<DownloadTags.Quality> QualityTagsAfter(DownloadTags.Quality startTag, bool fallbackToLowRes)
	{
		return QualityTagsBetween(startTag, DownloadTags.GetLastEnum<DownloadTags.Quality>(), fallbackToLowRes);
	}

	protected IEnumerable<DownloadTags.Quality> QualityTagsBetween(DownloadTags.Quality startTag, DownloadTags.Quality endTag, bool fallbackToLowRes)
	{
		bool started = false;
		string prevTag = string.Empty;
		foreach (DownloadTags.Quality qualityTag in Enum.GetValues(typeof(DownloadTags.Quality)))
		{
			if (!started && qualityTag == startTag)
			{
				started = true;
			}
			if (!started || qualityTag == DownloadTags.Quality.Essential || (qualityTag == DownloadTags.Quality.PortHigh && fallbackToLowRes))
			{
				continue;
			}
			if (AssetManifest.Get() != null)
			{
				string overrideTag = AssetManifest.Get().ConvertToOverrideTag(DownloadTags.GetTagString(qualityTag), DownloadTags.GetTagGroupString(DownloadTags.TagGroup.Quality));
				if (overrideTag != prevTag)
				{
					yield return qualityTag;
				}
				prevTag = overrideTag;
			}
			else if (qualityTag != 0)
			{
				yield return qualityTag;
			}
			if (qualityTag == endTag)
			{
				yield break;
			}
		}
	}

	private DownloadTags.Quality GetNextTag(DownloadTags.Quality startTag)
	{
		foreach (DownloadTags.Quality tag in QualityTagsAfter(startTag, PlatformSettings.ShouldFallbackToLowRes))
		{
			if (tag != startTag)
			{
				return tag;
			}
		}
		return startTag;
	}

	private ContentDownloadStatus GetContentDownloadStatus(string contentTag)
	{
		if (m_assetDownloader == null)
		{
			return m_curContentDownloadStatus;
		}
		string[] tags = CombineWithAllAppropriateTags(contentTag);
		TagDownloadStatus status = m_assetDownloader.GetDownloadStatus(tags.ToArray());
		m_curContentDownloadStatus.BytesTotal = status.BytesTotal;
		m_curContentDownloadStatus.BytesDownloaded = status.BytesDownloaded;
		m_curContentDownloadStatus.Progress = status.Progress;
		m_curContentDownloadStatus.ContentTag = contentTag;
		foreach (DownloadTags.Quality qualityTag in QualityTagsAfter(m_curContentDownloadStatus.InProgressQualityTag, PlatformSettings.ShouldFallbackToLowRes))
		{
			m_curContentDownloadStatus.InProgressQualityTag = qualityTag;
			if (!m_assetDownloader.GetDownloadStatus(new string[2]
			{
				DownloadTags.GetTagString(m_curContentDownloadStatus.InProgressQualityTag),
				contentTag
			}).Complete)
			{
				break;
			}
		}
		return m_curContentDownloadStatus;
	}

	public void StartUpdateProcess(bool localeChange, bool resetDownloadFinishReport = false)
	{
		Log.Downloader.PrintInfo($"StartUpdate locale={Localization.GetLocale().ToString()} : resetDownloadFinishReport = {resetDownloadFinishReport}");
		m_requestedInitialAssets = false;
		m_requestedStreamingAssets = false;
		m_bRegisterSceneLoadCallback = false;
		if (localeChange)
		{
			IsInLocaleChange = true;
			RefreshCachedModuleStates(forceNotify: false);
		}
		if (resetDownloadFinishReport)
		{
			ReportInitialDataDownloadFinished = false;
			ReportOptionalDataDownloadFinished = false;
		}
		LastInprogressTag = DownloadTags.Quality.Unknown;
		InitialBaseTags = null;
		m_currentDownloadType = DownloadType.NONE;
		StartUpdateJobIfNotRunning();
	}

	protected virtual void StartUpdateJobIfNotRunning()
	{
		Processor.QueueJobIfNotExist("GameDownloadManager.UpdateProcess", Job_UpdateProcess());
	}

	public void StartUpdateProcessForOptional()
	{
		Log.Downloader.PrintDebug($"StartUpdateProcessForOptional :  m_currentDownloadType = {m_currentDownloadType} : ShouldDownloadOptional = {ShouldDownloadOptional} : ShouldNotDownloadOptionalData = {ShouldNotDownloadOptionalData} : PlayerPausedDownload = {PlayerPausedDownload}");
		if (m_currentDownloadType != 0 && m_currentDownloadType != DownloadType.MODULE_DOWNLOAD && ShouldDownloadOptional && !PlayerPausedDownload)
		{
			MaximumQualityTag = DownloadTags.GetLastEnum<DownloadTags.Quality>();
			StartOptionalDownload();
			StartUpdateJobIfNotRunning();
		}
	}

	protected void SetPlayerPausedDownload()
	{
		PlayerPausedDownload = true;
	}

	public void PauseAllDownloads(bool requestedByPlayer = false)
	{
		Log.Downloader.PrintDebug($"PauseAllDownloads : requestedByPlayer = {requestedByPlayer}");
		if (requestedByPlayer)
		{
			PlayerPausedDownload = true;
		}
		PauseAllDownloadsInternal();
	}

	private void PauseAllDownloadsInternal()
	{
		if (m_assetDownloader != null)
		{
			m_assetDownloader.SendDownloadStoppedTelemetryMessage(m_currentDownloadType, IsInLocaleChange, PlayerPausedDownload, GetCurrentDownloadingModuleTag());
			m_assetDownloader.PauseAllDownloads();
		}
	}

	private void CreateUpdateProcessJob()
	{
		if (IsAnyDownloadRequestedAndIncomplete)
		{
			Processor.QueueJobIfNotExist("GameDownloadManager.UpdateProcess", Job_UpdateProcess());
		}
	}

	public void ResumeAllDownloads()
	{
		PlayerPausedDownload = false;
		m_timeOfLastAutoResumeAttempt = GetRealtimeSinceStartup();
		StartUpdateJobIfNotRunning();
	}

	private void ResumeAllDownloadsInternal()
	{
		if (IsAnyDownloadRequestedAndIncomplete)
		{
			Log.Downloader.PrintInfo(string.Format("Resume download :  downloadType = {0} : tags = {1}", m_currentDownloadType, string.Join(", ", LastRequestedContentTags)));
			ExceptionReporterControl.Get()?.ControlANRMonitor(on: false);
			m_assetDownloader.SendDownloadStartedTelemetryMessage(m_currentDownloadType, IsInLocaleChange, GetCurrentDownloadingModuleTag());
			m_assetDownloader.StartDownload(LastRequestedContentTags, m_currentDownloadType, IsInLocaleChange);
		}
	}

	public void DeleteDownloadedData()
	{
		if (m_assetDownloader != null)
		{
			m_assetDownloader.PauseAllDownloads();
			m_assetDownloader.DeleteDownloadedData();
		}
	}

	public void OnSceneLoad(SceneMgr.Mode prevMode, SceneMgr.Mode nextMode, object userData)
	{
		Log.Downloader.PrintDebug("OnSceneLoad: prev {0}, next {1}, DownloadAllFinished {2}, DownloadEnabled {3}", prevMode, nextMode, m_assetDownloader.DownloadAllFinished, DownloadPermissionManager.DownloadEnabled);
		if (prevMode != SceneMgr.Mode.GAMEPLAY && nextMode == SceneMgr.Mode.GAMEPLAY)
		{
			LastInterval = 0.0;
		}
		m_assetDownloader?.OnSceneLoad(prevMode, nextMode, userData);
	}

	private LoginManager GetLoginManager()
	{
		if (m_loginManager == null)
		{
			m_loginManager = ServiceManager.Get<LoginManager>();
		}
		return m_loginManager;
	}

	private SceneMgr GetSceneManager()
	{
		if (m_sceneMgr == null)
		{
			m_sceneMgr = ServiceManager.Get<SceneMgr>();
		}
		return m_sceneMgr;
	}

	public void UnknownSourcesListener(string onOff)
	{
		m_assetDownloader.UnknownSourcesListener(onOff);
	}

	public void InstallAPKListener(string status)
	{
		m_assetDownloader.InstallAPKListener(status);
	}

	public void AllowNotificationListener(string onOff)
	{
		m_assetDownloader.AllowNotificationListener(onOff);
	}

	private void AddDownloadCheats()
	{
	}

	private void RemoveDownloadCheats()
	{
	}

	private bool OnProcessCheat_StartModuleDownload(string func, string[] args, string rawArgs)
	{
		if (args.Length != 0)
		{
			string moduleName = args[0];
			if (string.IsNullOrEmpty(moduleName))
			{
				return false;
			}
			DownloadTags.Content moduleTag = DownloadTags.GetContentTag(moduleName);
			if (moduleTag != 0)
			{
				DownloadModule(moduleTag);
				return true;
			}
		}
		return false;
	}

	private bool OnProcessCheat_DeleteModule(string func, string[] args, string rawArgs)
	{
		if (args.Length != 0)
		{
			string moduleName = args[0];
			if (string.IsNullOrEmpty(moduleName))
			{
				return false;
			}
			DownloadTags.Content moduleTag = DownloadTags.GetContentTag(moduleName);
			if (moduleTag != 0)
			{
				DeleteModuleAsync(moduleTag).GetAwaiter().GetResult();
				return true;
			}
		}
		return false;
	}

	private bool OnProcessCheat_ResetRequestedModules(string func, string[] args, string rawArgs)
	{
		RequestedModules = new string[0];
		List<string> lastRequestedContentTags = new List<string>(LastRequestedContentTags);
		DownloadTags.Content[] downloadableModuleTags = DownloadableModuleTags;
		foreach (DownloadTags.Content tag in downloadableModuleTags)
		{
			lastRequestedContentTags.Remove(DownloadTags.GetTagString(tag));
		}
		LastRequestedContentTags = lastRequestedContentTags.ToArray();
		return true;
	}

	private bool OnProcessCheat_GetModuleDownloadSize(string func, string[] args, string rawArgs)
	{
		if (args.Length != 0)
		{
			string moduleName = args[0];
			if (string.IsNullOrEmpty(moduleName))
			{
				return false;
			}
			DownloadTags.Content moduleTag = DownloadTags.GetContentTag(moduleName);
			if (moduleTag != 0)
			{
				long downloadSize = GetModuleDownloadSize(moduleTag);
				Log.Downloader.PrintDebug($"OnProcessCheat_GetModuleDownloadSize : moduleTag = {moduleTag} : downloadSize = {downloadSize}");
				return true;
			}
		}
		return false;
	}
}
