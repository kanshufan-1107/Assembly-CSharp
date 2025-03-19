using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using agent;
using Blizzard.BlizzardErrorMobile;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using Blizzard.Telemetry;
using Blizzard.Telemetry.WTCG.Client;
using Cysharp.Threading.Tasks;
using Hearthstone.Devices;
using Hearthstone.Streaming;
using Hearthstone.Util;
using UnityEngine;

namespace Hearthstone.Core.Streaming;

public class RuntimeAssetDownloader : ICallbackHandler, IAssetDownloader
{
	private enum ApkInstallOperations
	{
		SAVE_BEFORE_APP_CLOSING = 1,
		LOAD_FROM_NEW_APP
	}

	private enum AgentInternalState
	{
		STATE_NONE = 0,
		STATE_STARTING = 1000,
		STATE_DOWNLOADING = 1001,
		STATE_INSTALLING = 1002,
		STATE_UPDATING = 1003,
		STATE_READY = 1004,
		STATE_RUNNING = 1005,
		STATE_CLOSING = 1006,
		STATE_VERSIONING = 1007,
		STATE_WAITING = 1008,
		STATE_FINISHED = 1009,
		STATE_CANCELED = 1010,
		STATE_IMPEDED = 1100,
		STATE_ERROR_START = 1200,
		STATE_FAILED = 1200,
		STATE_CANCELING = 1202,
		STATE_OUT_OF_DATE = 1203,
		STATE_ERROR_END = 1299
	}

	private enum AgentState
	{
		ERROR = -1,
		NONE,
		UNINITIALIZED,
		INSTALLED,
		VERSION,
		WAIT_SERVICE,
		UNKNOWN_APPS,
		ALLOW_NOTIFICATION,
		OPEN_APP_STORE,
		WARNING_MIN_SPEC,
		UPDATE_APK,
		UPDATE_MANIFEST,
		UPDATE_GLOBAL,
		UPDATE_OPTIONAL,
		UPDATE_LOCALIZED,
		AWAITING_WIFI,
		DISK_FULL,
		IMPEDED,
		UPDATE_MODULE
	}

	private enum TactProgressState
	{
		CL_NONE = 0,
		CL_FETCHING_BUILD_CONFIG = 1,
		CL_FETCHING_CDN_CONFIG = 2,
		CL_FETCHING_DOWNLOAD_MANIFEST = 3,
		CL_FETCHING_ENCODING_TABLE = 4,
		CL_FETCHING_INSTALLATION_MANIFEST = 5,
		CL_FETCHING_PATCH_MANIFEST = 6,
		CL_FETCHING_DATA_INDICES = 7,
		CL_FETCHING_PATCH_INDICES = 8,
		CL_CHECKING_RESIDENCY = 9,
		CL_INSTALLING = 10,
		CL_REPAIRING = 10,
		CL_FETCHFILE = 11,
		CL_POSTJOB = 12
	}

	private enum KindOfUpdate
	{
		VERSION,
		APK_UPDATE,
		MANIFEST_UPDATE,
		GLOBAL_UPDATE,
		OPTIONAL_UPDATE,
		LOCALE_UPDATE,
		MODULE_UPDATE
	}

	public enum FirstDownloadChoice
	{
		NO_SELECTION,
		WAIT,
		DOWNLOAD_NOW
	}

	[Serializable]
	public class DownloadProgress
	{
		public List<TagDownloadStatus> m_items = new List<TagDownloadStatus>();
	}

	public class BundleDataList
	{
		private struct DataInfo
		{
			public string m_md5;

			public long m_size;

			public DataInfo(string md5, long size)
			{
				m_md5 = md5;
				m_size = size;
			}
		}

		private Dictionary<string, DataInfo> m_dataFiles = new Dictionary<string, DataInfo>();

		private string m_dataFolder;

		public bool isEnabled => Vars.Key("Mobile.GenDataList").GetBool(def: false);

		public BundleDataList(string dataFolder)
		{
			m_dataFolder = dataFolder;
		}

		public void Clear()
		{
			if (isEnabled)
			{
				m_dataFiles.Clear();
			}
		}

		public IEnumerator Print()
		{
			if (!isEnabled)
			{
				yield break;
			}
			DirectoryInfo dataDirectory = new DirectoryInfo(m_dataFolder);
			FileInfo[] files = dataDirectory.GetFiles();
			foreach (FileInfo file in files)
			{
				if (!m_dataFiles.ContainsKey(file.Name))
				{
					m_dataFiles.Add(file.Name, new DataInfo(CalculateMD5(file.FullName), file.Length));
				}
				yield return null;
			}
			Log.Downloader.PrintInfo("== Data Directory Info ==");
			foreach (KeyValuePair<string, DataInfo> entry in m_dataFiles)
			{
				Log.Downloader.PrintInfo("{0}\t{1}\t{2}", entry.Key, entry.Value.m_size, entry.Value.m_md5);
			}
			Log.Downloader.PrintInfo("====================");
		}

		private static string CalculateMD5(string filename)
		{
			using MD5 md5 = MD5.Create();
			using FileStream stream = File.OpenRead(filename);
			return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
		}
	}

	private static readonly string[] s_dbfTags = new string[2]
	{
		DownloadTags.GetTagString(DownloadTags.Quality.Dbf),
		DownloadTags.GetTagString(DownloadTags.Content.Base)
	};

	private static readonly string[] s_stringsTags = new string[2]
	{
		DownloadTags.GetTagString(DownloadTags.Quality.Strings),
		DownloadTags.GetTagString(DownloadTags.Content.Base)
	};

	private static readonly Dictionary<AgentState, KindOfUpdate> s_updateOp = new Dictionary<AgentState, KindOfUpdate>
	{
		{
			AgentState.VERSION,
			KindOfUpdate.VERSION
		},
		{
			AgentState.UPDATE_APK,
			KindOfUpdate.APK_UPDATE
		},
		{
			AgentState.UPDATE_MANIFEST,
			KindOfUpdate.MANIFEST_UPDATE
		},
		{
			AgentState.UPDATE_GLOBAL,
			KindOfUpdate.GLOBAL_UPDATE
		},
		{
			AgentState.UPDATE_OPTIONAL,
			KindOfUpdate.OPTIONAL_UPDATE
		},
		{
			AgentState.UPDATE_LOCALIZED,
			KindOfUpdate.LOCALE_UPDATE
		},
		{
			AgentState.UPDATE_MODULE,
			KindOfUpdate.MODULE_UPDATE
		}
	};

	private bool m_allowUnknownApps;

	private bool m_versionCalled;

	private bool m_agentInit;

	private bool m_cancelledByUser;

	private bool m_askedCellAllowance;

	private bool m_pausedByNetwork;

	private bool m_showResumeDialog;

	private bool m_cellularEnabledSession;

	private bool m_retriedUpdateWithNoSpaceError;

	private bool m_optionalDownload;

	private bool m_localeDownload;

	private int[] m_disabledAPKUpdates;

	private string m_errorMsg = "GLUE_LOADINGSCREEN_ERROR_GENERIC";

	private string m_store;

	private string[] m_disabledAdventuresForStreaming = new string[0];

	private float m_progress;

	private float m_prevProgress;

	private long m_deviceSpace;

	private int m_prevInternalAgentError;

	private AgentState m_agentState;

	private AgentState m_savedAgentState;

	private ProductStatus m_agentStatus;

	private DownloadType m_downloadType = DownloadType.NONE;

	private IDisposable m_callbackDisposer;

	private float m_updateStartTime;

	private float m_apkUpdateStartTime;

	private float m_lastUpdateProgressReportTime;

	private DownloadProgress m_tagDownloads = new DownloadProgress();

	private Dictionary<int, int> m_tagLocations = new Dictionary<int, int>();

	private HashSet<string> m_tempUniqueTags = new HashSet<string>();

	private List<string> m_invalidBundles = new List<string>();

	private int m_instantSpeed;

	private int m_instantMaxSpeed;

	private int m_inGameStreamingDefaultSpeed = 512000;

	private bool m_inGameStreamingOff;

	private TagIndicatorManager m_tagIndicatorManager = new TagIndicatorManager();

	private BundleDataList m_bundleDataList;

	private ReactiveBoolOption m_isDownloadAllFinished = ReactiveBoolOption.CreateInstance(Option.DOWNLOAD_ALL_FINISHED);

	private InternetReachableController m_internetReachableController = new InternetReachableController();

	private DateTime m_lastAgentWorkTime = DateTime.UtcNow;

	private TactProgressState m_tactProgressState;

	private TactProgressState m_prevTactProgressState;

	private TagDownloadStatus m_curDownloadStatus;

	private string m_installDataPath;

	private int m_installedVersionCode;

	private string[] m_dataFolders = new string[4]
	{
		INSTALL_PATH + "/__agent__",
		INSTALL_PATH + "/caches",
		INSTALL_PATH + "/Data",
		INSTALL_PATH + "/Strings"
	};

	private HashSet<string> m_downloadedFilesList = new HashSet<string>();

	private static string INSTALL_PATH
	{
		get
		{
			if (PlatformSettings.RuntimeOS == OSCategory.Android)
			{
				return AndroidDeviceSettings.Get().assetBundleFolder;
			}
			return PlatformFilePaths.BasePersistentDataPath;
		}
	}

	private static VersionPipeline Pipeline => ServiceManager.Get<VersionConfigurationService>().GetPipeline();

	private static string ProductName => ServiceManager.Get<VersionConfigurationService>().GetProductName();

	private static string VersionToken => ServiceManager.Get<VersionConfigurationService>().GetClientToken();

	private static string EncrpytionKeyOption
	{
		get
		{
			string key = ServiceManager.Get<VersionConfigurationService>().GetEncrpytionKey();
			if (!string.IsNullOrEmpty(key))
			{
				return "encryption_key=" + key;
			}
			return string.Empty;
		}
	}

	private string UpdateState
	{
		get
		{
			return Options.Get().GetString(Option.NATIVE_UPDATE_STATE);
		}
		set
		{
			Options.Get().SetString(Option.NATIVE_UPDATE_STATE, value);
		}
	}

	private bool IsReportedApkInstallFailure
	{
		get
		{
			return Options.Get().GetBool(Option.APKINSTALL_FAILURE_REPORTED);
		}
		set
		{
			Options.Get().SetBool(Option.APKINSTALL_FAILURE_REPORTED, value);
		}
	}

	private static bool IsEnabledUpdate => UpdateUtils.AreUpdatesEnabledForCurrentPlatform;

	private bool AskUnkownApps => Options.Get().GetBool(Option.ASK_UNKNOWN_APPS);

	private int FirstInstallClientBuildNumber
	{
		get
		{
			return Options.Get().GetInt(Option.FIRST_CLIENT_BUILD_NUMBER);
		}
		set
		{
			Options.Get().SetInt(Option.FIRST_CLIENT_BUILD_NUMBER, value);
		}
	}

	private int MaxAgeInactiveLocales
	{
		get
		{
			return Options.Get().GetInt(Option.MAX_AGE_INACTIVE_LOCALES, 28);
		}
		set
		{
			Options.Get().SetInt(Option.MAX_AGE_INACTIVE_LOCALES, value);
		}
	}

	private long InActivityTimeout
	{
		get
		{
			return Options.Get().GetLong(Option.INACTIVITY_AGENT_TIMEOUT, 180L);
		}
		set
		{
			Options.Get().SetLong(Option.INACTIVITY_AGENT_TIMEOUT, value);
		}
	}

	private string ApkInstallStart
	{
		get
		{
			return Options.Get().GetString(Option.APKINSTALL_START);
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				Options.Get().SetString(Option.APKINSTALL_START, value);
			}
			else
			{
				Options.Get().DeleteOption(Option.APKINSTALL_START);
			}
		}
	}

	private long BGRemainingBytes
	{
		get
		{
			return Options.Get().GetLong(Option.BG_REMAINING_BYTES);
		}
		set
		{
			if (value != 0L)
			{
				Options.Get().SetLong(Option.BG_REMAINING_BYTES, value);
			}
			else
			{
				Options.Get().DeleteOption(Option.BG_REMAINING_BYTES);
			}
		}
	}

	private ulong BGRemainingBytesStartTime
	{
		get
		{
			return Options.Get().GetULong(Option.BG_REMAINING_BYTES_STARTTIME_NEW);
		}
		set
		{
			if ((float)value != 0f)
			{
				Options.Get().SetULong(Option.BG_REMAINING_BYTES_STARTTIME_NEW, value);
			}
			else
			{
				Options.Get().DeleteOption(Option.BG_REMAINING_BYTES_STARTTIME_NEW);
			}
		}
	}

	private bool IsInGame
	{
		get
		{
			if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
			{
				return sceneMgr.IsInGame();
			}
			return false;
		}
	}

	private bool IsDOPCase { get; set; }

	protected bool InternetAvailable => m_internetReachableController.InternetAvailable;

	protected bool IsEnabledDownload
	{
		get
		{
			if (m_downloadType >= DownloadType.OPTIONAL_DOWNLOAD)
			{
				return DownloadPermissionManager.DownloadEnabled;
			}
			return true;
		}
	}

	protected bool CanDownload
	{
		get
		{
			if (IsEnabledDownload && InternetAvailable && !BlockedByFirstDownloadPrompt)
			{
				if (BlockedByCellPermission)
				{
					return TotalBytes < Options.Get().GetInt(Option.CELL_PROMPT_THRESHOLD);
				}
				return true;
			}
			return false;
		}
	}

	private bool BlockedByDiskFull { get; set; }

	private bool BlockedByCellPermission
	{
		get
		{
			if (NetworkReachabilityManager.OnCellular)
			{
				return !m_cellularEnabledSession;
			}
			return false;
		}
	}

	private FirstDownloadChoice FirstDownloadUserChoice
	{
		get
		{
			return Options.Get().GetEnum(Option.FIRST_DOWNLOAD_USER_CHOICE, FirstDownloadChoice.NO_SELECTION);
		}
		set
		{
			Options.Get().SetEnum(Option.FIRST_DOWNLOAD_USER_CHOICE, value);
		}
	}

	private bool IsEnabledFirstDownloadPrompt => Options.Get().GetBool(Option.FIRST_DOWNLOAD_ENABLED, defaultVal: false);

	private bool BlockedByFirstDownloadPrompt
	{
		get
		{
			if (PlatformSettings.RuntimeOS == OSCategory.Android)
			{
				return false;
			}
			if (!IsDbfReady)
			{
				return false;
			}
			if (m_downloadType != 0 || FirstDownloadUserChoice != 0)
			{
				return false;
			}
			if (TotalBytes < Options.Get().GetInt(Option.FIRST_DOWNLOAD_THRESHOLD))
			{
				return false;
			}
			if (!IsEnabledFirstDownloadPrompt && !GameUtils.IsTraditionalTutorialComplete())
			{
				return false;
			}
			return true;
		}
	}

	private bool RestartOnFalseHDDFullEnabled => Vars.Key("Mobile.RestartOnFalseHDDFull").GetBool(def: true);

	private bool CheckLowerVersion
	{
		get
		{
			return Vars.Key("Mobile.CheckLowerVersion").GetBool(def: true);
		}
		set
		{
			Vars.Key("Mobile.CheckLowerVersion").Set(value.ToString(), permanent: false);
		}
	}

	private bool CheckStoreVersionCode
	{
		get
		{
			return Vars.Key("Mobile.CheckStoreVersionCode").GetBool(def: true);
		}
		set
		{
			Vars.Key("Mobile.CheckStoreVersionCode").Set(value.ToString(), permanent: false);
		}
	}

	private bool NewMinSpecCheck
	{
		get
		{
			return Vars.Key("Mobile.CheckNewMinSpec").GetBool(def: true);
		}
		set
		{
			Vars.Key("Mobile.CheckNewMinSpec").Set(value.ToString(), permanent: false);
		}
	}

	private bool ShownMinSpecWarningInThisSession
	{
		get
		{
			return Vars.Key("Mobile.ShownMinSpecWarning").GetBool(def: false);
		}
		set
		{
			Vars.Key("Mobile.ShownMinSpecWarning").Set(value.ToString(), permanent: false);
		}
	}

	private string MobileMode => EnumUtils.GetString(HearthstoneApplication.GetMobileEnvironment());

	private string InternalVersionService => Vars.Key("Mobile.VersionService").GetStr("https://t5.corp.blizzard.net/mobile");

	private long TotalBytes { get; set; }

	private long DownloadedBytes { get; set; }

	private long RemainingDownloadBytes => TotalBytes - DownloadedBytes;

	private long PrevDownloadedBytes { get; set; }

	private long RequiredBytes { get; set; }

	private float ProgressPercent
	{
		get
		{
			if (m_cancelledByUser || TotalBytes == 0L)
			{
				return m_prevProgress;
			}
			return m_progress;
		}
	}

	private string InstallDataPath
	{
		get
		{
			if (string.IsNullOrEmpty(m_installDataPath))
			{
				m_installDataPath = INSTALL_PATH + "/Data";
				if (PlatformSettings.RuntimeOS == OSCategory.Android)
				{
					m_installDataPath = m_installDataPath + "/" + AndroidDeviceSettings.Get().InstalledTexture;
				}
			}
			return m_installDataPath;
		}
	}

	private int GetInstalledVersionCode
	{
		get
		{
			if (m_installedVersionCode != 0)
			{
				return m_installedVersionCode;
			}
			try
			{
				string[] versionStrSplit = Application.version.Split('.');
				m_installedVersionCode = int.Parse(versionStrSplit[2]);
				if (m_installedVersionCode != 216423)
				{
					Log.Downloader.PrintError("Application.version is different from our setting");
					m_installedVersionCode = 216423;
				}
			}
			catch (Exception ex)
			{
				Error.AddDevFatal("Failed to read the installed version: {0}", ex.Message);
			}
			return m_installedVersionCode;
		}
	}

	private int InternalAgentError
	{
		get
		{
			if (m_agentState != AgentState.VERSION)
			{
				return m_agentStatus.m_cachedState.m_updateProgress.m_progressDetails.m_error;
			}
			return m_agentStatus.m_cachedState.m_versionProgress.m_error;
		}
		set
		{
			if (m_agentState == AgentState.VERSION)
			{
				m_agentStatus.m_cachedState.m_versionProgress.m_error = value;
			}
			else
			{
				m_agentStatus.m_cachedState.m_updateProgress.m_progressDetails.m_error = value;
			}
		}
	}

	private AgentInternalState InternalAgentState
	{
		get
		{
			if (m_agentState != AgentState.VERSION)
			{
				return (AgentInternalState)m_agentStatus.m_cachedState.m_updateProgress.m_progressDetails.m_agentState;
			}
			return (AgentInternalState)m_agentStatus.m_cachedState.m_versionProgress.m_agentState;
		}
	}

	private bool SentStartedTelemetryMessage { get; set; }

	private bool ShouldShowPauseState { get; set; }

	private bool IsSameMajorMinorVersions { get; set; }

	private bool NeedToRedownload { get; set; }

	private bool IsStoreShowingUpdateButton
	{
		get
		{
			if (string.Compare(m_store, "Google") != 0 || !CheckStoreVersionCode)
			{
				return true;
			}
			return string.Compare(StrVersionCodeInStore, "0") != 0;
		}
	}

	private string StrVersionCodeInStore => Vars.Key("Debug.FakeStoreVersionCode").GetStr(MobileCallbackManager.VersionCodeInStore);

	private string CurrentVersion { get; set; } = string.Empty;

	private string LiveVersion { get; set; } = string.Empty;

	private bool ShouldReportBGDownloadResult { get; set; }

	private bool PrintStageDownloadInfo => Vars.Key("Debug.PrintStageDownloadInfo").GetBool(def: true);

	public AssetDownloaderState State
	{
		get
		{
			switch (m_agentState)
			{
			case AgentState.AWAITING_WIFI:
				return AssetDownloaderState.AWAITING_WIFI;
			case AgentState.ERROR:
				if (!BlockedByDiskFull)
				{
					return AssetDownloaderState.ERROR;
				}
				return AssetDownloaderState.DISK_FULL;
			case AgentState.INSTALLED:
			case AgentState.VERSION:
			case AgentState.UNKNOWN_APPS:
			case AgentState.OPEN_APP_STORE:
				return AssetDownloaderState.VERSIONING;
			case AgentState.UPDATE_APK:
			case AgentState.UPDATE_MANIFEST:
			case AgentState.UPDATE_GLOBAL:
			case AgentState.UPDATE_OPTIONAL:
			case AgentState.UPDATE_LOCALIZED:
			case AgentState.UPDATE_MODULE:
				return AssetDownloaderState.DOWNLOADING;
			case AgentState.WAIT_SERVICE:
				return AssetDownloaderState.UNINITIALIZED;
			case AgentState.IMPEDED:
				return AssetDownloaderState.AGENT_IMPEDED;
			case AgentState.NONE:
				if (FirstDownloadUserChoice == FirstDownloadChoice.WAIT)
				{
					return AssetDownloaderState.FIRST_DOWNLOAD_WAIT;
				}
				return AssetDownloaderState.IDLE;
			default:
				return AssetDownloaderState.IDLE;
			}
		}
	}

	public bool IsReady { get; private set; }

	public bool ShouldNotDownloadOptionalData { get; private set; }

	public bool IsVersionChanged { get; private set; }

	public bool IsVersionStepCompleted { get; private set; }

	public bool IsDbfReady { get; private set; }

	public bool AreStringsReady { get; private set; }

	public bool DownloadAllFinished
	{
		get
		{
			return m_isDownloadAllFinished.Value;
		}
		set
		{
			m_isDownloadAllFinished.Set(value);
		}
	}

	public bool IsNewMobileVersionReleased
	{
		get
		{
			if (m_agentStatus == null || m_agentState == AgentState.ERROR)
			{
				return true;
			}
			return GetInstalledVersionCode < GetVersionCode(m_agentStatus.m_configuration.m_liveDisplayVersion);
		}
	}

	public string VersionOverrideUrl { get; private set; }

	public string[] DisabledAdventuresForStreaming => m_disabledAdventuresForStreaming;

	public double BytesPerSecond { get; private set; }

	public int MaxDownloadSpeed
	{
		get
		{
			if (m_instantMaxSpeed != 0)
			{
				return m_instantMaxSpeed;
			}
			return Options.Get().GetInt(Option.MAX_DOWNLOAD_SPEED);
		}
		set
		{
			if (m_instantMaxSpeed == value)
			{
				Log.Downloader.PrintDebug($"Skip to update the max download speed: {m_instantMaxSpeed}");
			}
			if (IsUpdating())
			{
				m_instantMaxSpeed = value;
				ResetDownloadSpeed(value);
			}
		}
	}

	public int InGameStreamingDefaultSpeed
	{
		get
		{
			return m_inGameStreamingDefaultSpeed;
		}
		set
		{
			if (value < 0)
			{
				m_inGameStreamingOff = true;
				return;
			}
			m_inGameStreamingOff = false;
			m_inGameStreamingDefaultSpeed = value;
		}
	}

	public int DownloadSpeedInGame
	{
		get
		{
			if (m_instantSpeed != 0)
			{
				return m_instantSpeed;
			}
			return Options.Get().GetInt(Option.STREAMING_SPEED_IN_GAME, InGameStreamingDefaultSpeed);
		}
		set
		{
			m_instantSpeed = value;
			if (State == AssetDownloaderState.DOWNLOADING && IsInGame)
			{
				ResetDownloadSpeed(m_instantSpeed);
			}
		}
	}

	private static string DownloadStatsPath => Path.Combine(INSTALL_PATH, "DownloadProgress.json");

	private int VersionRetryCount { get; set; }

	private bool IsInitialBackgroundDownloading => !GameUtils.IsTraditionalTutorialComplete();

	protected bool ShouldShowCellPopup
	{
		get
		{
			if (!m_askedCellAllowance)
			{
				return RemainingDownloadBytes > Options.Get().GetInt(Option.CELL_PROMPT_THRESHOLD);
			}
			return false;
		}
	}

	public event Action VersioningStarted;

	public event Action<int> ApkDownloadProgress;

	public event Action<int> DbfDownloadProgress;

	public void OnTelemetry(TelemetryMessage msg)
	{
		MessageOptions options = null;
		if (!string.IsNullOrEmpty(msg.m_component))
		{
			options = new MessageOptions
			{
				Context = new Context
				{
					Program = new Context.ProgramInfo
					{
						Id = msg.m_component
					}
				}
			};
		}
		if (TelemetryManager.Client() != null)
		{
			TelemetryManager.Client().EnqueueMessage(msg.m_packageName, msg.m_messageName, msg.m_payload, options);
		}
	}

	public void OnPatchOverrideUrlChanged(OverrideUrlChangedMessage msg)
	{
		Log.Downloader.PrintDebug("Retired PatchUrl override feature. Ignored.");
	}

	public void OnVersionServiceOverrideUrlChanged(OverrideUrlChangedMessage msg)
	{
		Log.Downloader.PrintInfo("OnVersionServiceOverrideUrlChanged: {0} -- {1}", msg.m_product, msg.m_overrideUrl);
		if (!msg.m_product.Equals(ProductName))
		{
			Log.Downloader.PrintError("Unknown product name for {0}", msg.m_product);
		}
		else if ((string.IsNullOrEmpty(msg.m_overrideUrl) || !msg.m_overrideUrl.Contains(InternalVersionService)) && !VersionOverrideUrl.Equals("Live"))
		{
			VersionOverrideUrl = "Back to Live";
		}
	}

	public void OnNetworkStatusChangedMessage(NetworkStatusChangedMessage msg)
	{
		Log.Downloader.PrintInfo("OnNetworkStatusChangedMessage - cell {0}, wifi {1}, isCellAllowed {2}", msg.m_hasCell, msg.m_hasWifi, msg.m_isCellAllowed);
	}

	public void OnDownloadPausedDueToNetworkStatusChange(NetworkStatusChangedMessage msg)
	{
		Log.Downloader.PrintInfo("OnDownloadPausedDueToNetworkStatusChange - cell {0}, wifi {1}, isCellAllowed {2}", msg.m_hasCell, msg.m_hasWifi, msg.m_isCellAllowed);
		m_pausedByNetwork = true;
	}

	public void OnDownloadResumedDueToNetworkStatusChange(NetworkStatusChangedMessage msg)
	{
		Log.Downloader.PrintInfo("OnDownloadResumedDueToNetworkStatusChange - cell {0}, wifi {1}, isCellAllowed {2}", msg.m_hasCell, msg.m_hasWifi, msg.m_isCellAllowed);
		if (msg.m_isCellAllowed && msg.m_hasCell && !msg.m_hasWifi)
		{
			Log.Downloader.PrintInfo("User allowed to resume by using Cellular");
		}
		m_cellularEnabledSession = msg.m_isCellAllowed;
		m_pausedByNetwork = false;
	}

	public void OnDownloadPausedByUser()
	{
		Log.Downloader.PrintInfo("OnDownloadPausedByUser");
		if (State == AssetDownloaderState.DOWNLOADING && m_optionalDownload && DownloadPermissionManager.DownloadEnabled)
		{
			ShouldShowPauseState = true;
		}
	}

	public void OnDownloadResumedByUser()
	{
		Log.Downloader.PrintInfo("OnDownloadResumedByUser");
	}

	public bool Initialize()
	{
		if (!m_internetReachableController.Initialize())
		{
			return false;
		}
		HandleApkInstallationSuccess(ApkInstallOperations.LOAD_FROM_NEW_APP);
		m_agentState = AgentState.UNINITIALIZED;
		return SetInitialState();
	}

	public void Update(bool firstCall)
	{
		AgentInitializeWhenReady();
		ProcessMobile();
	}

	public void Shutdown()
	{
		Log.Downloader.PrintInfo("Shutdown");
		if (PlatformSettings.IsMobileRuntimeOS && !Application.isEditor)
		{
			AgentEmbeddedAPI.Shutdown();
		}
		Log.Downloader.PrintInfo("Disposed listeners for Agent");
		if (m_callbackDisposer != null)
		{
			m_callbackDisposer.Dispose();
		}
	}

	public TagDownloadStatus GetDownloadStatus(string[] tags)
	{
		tags = RemoveRepeatedTags(tags);
		TagDownloadStatus status = CreateDownloadStatusIfNotExist(tags);
		if (!status.Complete)
		{
			if (!m_tagIndicatorManager.IsInitialized)
			{
				status.Complete = false;
			}
			else
			{
				status.Complete = m_tagIndicatorManager.IsReady(status.Tags);
			}
		}
		return status;
	}

	public TagDownloadStatus GetCurrentDownloadStatus()
	{
		return m_curDownloadStatus;
	}

	public void StartDownload(string[] tags, DownloadType downloadType, bool localeChanged)
	{
		KindOfUpdate kind;
		if (downloadType == DownloadType.INITIAL_DOWNLOAD)
		{
			m_optionalDownload = false;
			if (localeChanged)
			{
				kind = KindOfUpdate.LOCALE_UPDATE;
				m_askedCellAllowance = false;
				Options.Get().SetULong(Option.CURRENT_LOCALE_TIME_STAMP, TimeUtils.DateTimeToUnixTimeStampMilliseconds(DateTime.UtcNow));
			}
			else
			{
				if (UpdateState == "Updated")
				{
					GoToIdleState();
					return;
				}
				kind = KindOfUpdate.GLOBAL_UPDATE;
			}
		}
		else
		{
			kind = ((downloadType == DownloadType.MODULE_DOWNLOAD) ? KindOfUpdate.MODULE_UPDATE : KindOfUpdate.OPTIONAL_UPDATE);
			m_optionalDownload = true;
			m_askedCellAllowance = false;
			ShouldShowPauseState = false;
		}
		m_localeDownload = localeChanged;
		m_downloadType = downloadType;
		StartDownloadInternal(tags, kind);
	}

	public void PauseAllDownloads()
	{
		m_agentState = AgentState.NONE;
		AgentEmbeddedAPI.CancelAllOperations();
	}

	public void DeleteDownloadedData()
	{
		string[] dataFolders = m_dataFolders;
		foreach (string p in dataFolders)
		{
			try
			{
				DirectoryInfo dir = new DirectoryInfo(p);
				if (dir.Exists)
				{
					dir.Delete(recursive: true);
				}
			}
			catch (Exception ex)
			{
				Error.AddDevFatal("Failed to delete a folder({0}): {1}", p, ex.Message);
			}
		}
		Log.Downloader.PrintInfo("ClearDownloadedData");
	}

	public long DeleteDownloadedData(string[] tags)
	{
		long deletedDataSize = 0L;
		foreach (string bundleName in AssetManifest.Get().GetAssetBundleNamesForTags(tags))
		{
			string bundlePath = AssetBundleInfo.GetAssetBundlePath(bundleName);
			try
			{
				if (File.Exists(bundlePath))
				{
					deletedDataSize += new FileInfo(bundlePath).Length;
					File.Delete(bundlePath);
					Log.Downloader.PrintInfo("Deleted bundle : '" + bundlePath + "'");
				}
				else
				{
					Log.Downloader.PrintDebug("Trying to delete bundle : '" + bundlePath + "', But bundle file not found on disk.");
				}
				m_tagIndicatorManager.RemoveAvailableBundle(bundleName);
			}
			catch (Exception ex)
			{
				Error.AddDevFatal("Failed to delete the bundle : '" + bundlePath + "' : Error = " + ex.Message);
			}
		}
		m_tagIndicatorManager.DeleteIndicatorsForTags(tags);
		ResetTagDownloadStatus(tags);
		Log.Downloader.PrintInfo($"RuntimeAssetDownloader.DeleteDownloadedData complete. Deleted data for tags : {string.Join(',', tags)} : deletedDataSize = {deletedDataSize}");
		return deletedDataSize;
	}

	private void ResetTagDownloadStatus(string[] deletedTags)
	{
		string[] deletedContentTags = deletedTags.Where((string t) => DownloadTags.GetContentTag(t) != DownloadTags.Content.Unknown).ToArray();
		string[] deletedQualityTags = deletedTags.Where((string t) => DownloadTags.GetQualityTag(t) != DownloadTags.Quality.Unknown).ToArray();
		if (deletedContentTags == null || deletedContentTags.Length == 0 || deletedQualityTags == null || deletedQualityTags.Length == 0)
		{
			return;
		}
		foreach (TagDownloadStatus tagDownloadStatus in m_tagDownloads.m_items)
		{
			if (tagDownloadStatus.Tags.Intersect(deletedContentTags).Any() && tagDownloadStatus.Tags.Intersect(deletedQualityTags).Any())
			{
				Log.Downloader.PrintInfo("Resetting progress for tag download status because one of it's content-quality combination was deleted : tagDownloadStatus.Tags = " + string.Join(',', tagDownloadStatus.Tags) + " : deleted tags : " + string.Join(',', deletedTags));
				tagDownloadStatus.ResetProgress();
			}
		}
		SerializeDownloadStats();
	}

	public bool IsBundleDownloaded(string bundleName)
	{
		if (!PlatformSettings.IsMobileRuntimeOS)
		{
			return true;
		}
		return m_tagIndicatorManager.IsReady(bundleName);
	}

	public void OnSceneLoad(SceneMgr.Mode prevMode, SceneMgr.Mode nextMode, object userData)
	{
		if (State == AssetDownloaderState.DOWNLOADING && !m_inGameStreamingOff)
		{
			if (prevMode != SceneMgr.Mode.GAMEPLAY && nextMode == SceneMgr.Mode.GAMEPLAY)
			{
				ResetDownloadSpeed(DownloadSpeedInGame);
			}
			if (prevMode == SceneMgr.Mode.GAMEPLAY && nextMode != SceneMgr.Mode.GAMEPLAY)
			{
				ResetDownloadSpeed(MaxDownloadSpeed);
			}
		}
	}

	public void PrepareRestart()
	{
		GameStrings.LoadNative();
		ResetToUpdate(clearEssentialLooseFiles: false);
	}

	public async void DoPostTasksAfterDownload(DownloadType downloadType)
	{
		Log.Downloader.PrintInfo($"Process the post tasks after downloading the data: isInitial={downloadType}");
		FilterForiCloud();
		if (downloadType == DownloadType.INITIAL_DOWNLOAD)
		{
			await DeleteInvalidBundlesAsync();
		}
	}

	private async UniTask DeleteInvalidBundlesAsync()
	{
		List<string> list = AddtionalLocales();
		StringBuilder regLocales = new StringBuilder();
		foreach (string locale in list)
		{
			regLocales.AppendFormat("{0}_{1}", (regLocales.Length > 0) ? "|" : "", locale.ToLower());
		}
		Regex reg = new Regex(regLocales.ToString());
		await UniTask.RunOnThreadPool(delegate
		{
			string[] allAssetBundleNames = AssetManifest.Get().GetAllAssetBundleNames();
			Array.Sort(allAssetBundleNames);
			FileInfo[] files = new DirectoryInfo(InstallDataPath).GetFiles("*.unity3d");
			foreach (FileInfo fileInfo in files)
			{
				if (!reg.Match(fileInfo.Name).Success && !fileInfo.Name.StartsWith("dbf") && Array.BinarySearch(allAssetBundleNames, fileInfo.Name) < 0)
				{
					try
					{
						Log.Downloader.PrintWarning("Found the invalid asset bundle(" + fileInfo.Name + "), Deleting");
						m_invalidBundles.Add(fileInfo.Name);
						fileInfo.Delete();
					}
					catch (Exception ex)
					{
						Log.Downloader.PrintError("Failed to delete the invalid bundle(" + fileInfo.Name + "): " + ex.Message);
					}
				}
			}
			if (m_invalidBundles.Count > 0)
			{
				TelemetryManager.Client().SendDeletedInvalidBundles(m_invalidBundles, m_invalidBundles.Count);
			}
		});
	}

	public bool IsCurrentVersionHigherOrEqual(string versionToCheckStr)
	{
		if (!UpdateUtils.GetSplitVersion(CurrentVersion, out var currentVersionInt))
		{
			Log.Downloader.PrintDebug("IsCurrentVersionHigherOrEqual : Failed to get split version for CurrentVersion = " + CurrentVersion);
			return false;
		}
		if (!UpdateUtils.GetSplitVersion(versionToCheckStr, out var versionToCheckInt))
		{
			Log.Downloader.PrintDebug("IsCurrentVersionHigherOrEqual : Failed to get split version for VersionToCheckStr = " + versionToCheckStr);
			return false;
		}
		return CompareVersions(currentVersionInt, versionToCheckInt, compareMajorMinorOnly: true) >= 0;
	}

	public void SendDownloadStartedTelemetryMessage(DownloadType downloadType, bool localeUpdate, DownloadTags.Content moduleTag = DownloadTags.Content.Unknown)
	{
		SentStartedTelemetryMessage = true;
		switch (downloadType)
		{
		case DownloadType.INITIAL_DOWNLOAD:
			if (localeUpdate)
			{
				TelemetryManager.Client().SendLocaleDataUpdateStarted(Localization.GetLocale().ToString());
			}
			else
			{
				TelemetryManager.Client().SendDataUpdateStarted();
			}
			break;
		case DownloadType.MODULE_DOWNLOAD:
			if (localeUpdate)
			{
				TelemetryManager.Client().SendLocaleModuleDataUpdateStarted(Localization.GetLocale().ToString(), moduleTag.ToString());
			}
			else
			{
				TelemetryManager.Client().SendModuleDataUpdateStarted(moduleTag.ToString());
			}
			break;
		case DownloadType.OPTIONAL_DOWNLOAD:
			if (localeUpdate)
			{
				TelemetryManager.Client().SendLocaleOptionalDataUpdateStarted(Localization.GetLocale().ToString());
			}
			else
			{
				TelemetryManager.Client().SendOptionalDataUpdateStarted();
			}
			break;
		}
	}

	public void SendDownloadFinishedTelemetryMessage(DownloadType downloadType, bool localeUpdate, DownloadTags.Content moduleTag = DownloadTags.Content.Unknown)
	{
		if (!SentStartedTelemetryMessage)
		{
			return;
		}
		SentStartedTelemetryMessage = false;
		float duration = ElapsedTimeFromStart(m_updateStartTime);
		switch (downloadType)
		{
		case DownloadType.INITIAL_DOWNLOAD:
			if (localeUpdate)
			{
				TelemetryManager.Client().SendLocaleDataUpdateFinished(duration, DownloadedBytes, TotalBytes);
			}
			else
			{
				TelemetryManager.Client().SendDataUpdateFinished(duration, DownloadedBytes, TotalBytes);
			}
			break;
		case DownloadType.MODULE_DOWNLOAD:
			if (localeUpdate)
			{
				TelemetryManager.Client().SendLocaleModuleDataUpdateFinished(duration, DownloadedBytes, TotalBytes, moduleTag.ToString());
			}
			else
			{
				TelemetryManager.Client().SendModuleDataUpdateFinished(duration, DownloadedBytes, TotalBytes, moduleTag.ToString());
			}
			break;
		case DownloadType.OPTIONAL_DOWNLOAD:
			if (localeUpdate)
			{
				TelemetryManager.Client().SendLocaleOptionalDataUpdateFinished(duration, DownloadedBytes, TotalBytes);
			}
			else
			{
				TelemetryManager.Client().SendOptionalDataUpdateFinished(duration, DownloadedBytes, TotalBytes);
			}
			break;
		}
		PrintDownloadSpeedInfo(downloadType, localeUpdate, DownloadedBytes, duration);
		PrintStageDownloadedInfo(downloadType, localeUpdate, duration);
		ReportBGDownloadResult(completed: true);
	}

	public void SendDownloadStoppedTelemetryMessage(DownloadType downloadType, bool localeUpdate, bool byUser, DownloadTags.Content moduleTag = DownloadTags.Content.Unknown)
	{
		if (!SentStartedTelemetryMessage)
		{
			return;
		}
		SentStartedTelemetryMessage = false;
		if (m_agentState == AgentState.UPDATE_APK)
		{
			m_deviceSpace = FreeSpace.Measure();
			TelemetryManager.Client().SendUpdateStopped(m_agentStatus.m_cachedState.m_baseState.m_currentVersionStr, (float)m_deviceSpace / 1048576f, ElapsedTimeFromStart(m_apkUpdateStartTime), byUser);
			return;
		}
		float duration = ElapsedTimeFromStart(m_updateStartTime);
		switch (downloadType)
		{
		case DownloadType.INITIAL_DOWNLOAD:
			if (localeUpdate)
			{
				TelemetryManager.Client().SendLocaleDataUpdateStopped(duration, DownloadedBytes, TotalBytes, byUser);
			}
			else
			{
				TelemetryManager.Client().SendDataUpdateStopped(duration, DownloadedBytes, TotalBytes, byUser);
			}
			break;
		case DownloadType.MODULE_DOWNLOAD:
			if (localeUpdate)
			{
				TelemetryManager.Client().SendLocaleModuleDataUpdateStopped(duration, DownloadedBytes, TotalBytes, byUser, moduleTag.ToString());
			}
			else
			{
				TelemetryManager.Client().SendModuleDataUpdateStopped(duration, DownloadedBytes, TotalBytes, byUser, moduleTag.ToString());
			}
			break;
		case DownloadType.OPTIONAL_DOWNLOAD:
			if (localeUpdate)
			{
				TelemetryManager.Client().SendLocaleOptionalDataUpdateStopped(duration, DownloadedBytes, TotalBytes, byUser);
			}
			else
			{
				TelemetryManager.Client().SendOptionalDataUpdateStopped(duration, DownloadedBytes, TotalBytes, byUser);
			}
			break;
		}
		PrintDownloadSpeedInfo(downloadType, localeUpdate, DownloadedBytes, duration);
	}

	public void SendDownloadFailedTelemetryMessage(DownloadType downloadType, bool localeUpdate, DownloadTags.Content moduleTag = DownloadTags.Content.Unknown)
	{
		if (!SentStartedTelemetryMessage)
		{
			return;
		}
		SentStartedTelemetryMessage = false;
		if (m_agentState == AgentState.UPDATE_APK)
		{
			TelemetryManager.Client().SendUpdateError((uint)InternalAgentError, ElapsedTimeFromStart(m_apkUpdateStartTime));
			return;
		}
		float duration = ElapsedTimeFromStart(m_updateStartTime);
		switch (downloadType)
		{
		case DownloadType.INITIAL_DOWNLOAD:
			if (localeUpdate)
			{
				TelemetryManager.Client().SendLocaleDataUpdateFailed(duration, DownloadedBytes, TotalBytes, InternalAgentError);
			}
			else
			{
				TelemetryManager.Client().SendDataUpdateFailed(duration, DownloadedBytes, TotalBytes, InternalAgentError);
			}
			break;
		case DownloadType.MODULE_DOWNLOAD:
			if (localeUpdate)
			{
				TelemetryManager.Client().SendLocaleModuleDataUpdateFailed(duration, DownloadedBytes, TotalBytes, InternalAgentError, moduleTag.ToString());
			}
			else
			{
				TelemetryManager.Client().SendModuleDataUpdateFailed(duration, DownloadedBytes, TotalBytes, InternalAgentError, moduleTag.ToString());
			}
			break;
		case DownloadType.OPTIONAL_DOWNLOAD:
			if (localeUpdate)
			{
				TelemetryManager.Client().SendLocaleOptionalDataUpdateFailed(duration, DownloadedBytes, TotalBytes, InternalAgentError);
			}
			else
			{
				TelemetryManager.Client().SendOptionalDataUpdateFailed(duration, DownloadedBytes, TotalBytes, InternalAgentError);
			}
			break;
		}
		PrintDownloadSpeedInfo(downloadType, localeUpdate, DownloadedBytes, duration);
	}

	public void SendDeleteModuleTelemetryMessage(DownloadTags.Content moduleTag, long deletedSize)
	{
		TelemetryManager.Client().SendDeleteModuleData(moduleTag.ToString(), deletedSize);
	}

	public void SendDeleteOptionalDataTelemetryMessage(long deletedSize)
	{
		TelemetryManager.Client().SendDeleteOptionalData(deletedSize);
	}

	private void PrintDownloadSpeedInfo(DownloadType downloadType, bool localeUpdate, long downloadedBytes, float duration)
	{
		long speed = ((duration > 0f) ? Convert.ToInt64((float)downloadedBytes / duration) : 0);
		Log.Downloader.PrintInfo("Downloaded {0}{1}, Speed: {2} bytes / {3} = {4}", downloadType, localeUpdate ? " locale update" : "", downloadedBytes, duration, DownloadUtils.FormatBytesAsHumanReadable(speed));
	}

	private void PrintStageDownloadedInfo(DownloadType downloadType, bool localeUpdate, float duration)
	{
		if (!PrintStageDownloadInfo || !Directory.Exists(InstallDataPath))
		{
			return;
		}
		try
		{
			IEnumerable<string> enumerable = Directory.EnumerateFiles(InstallDataPath, "*", SearchOption.AllDirectories);
			StringBuilder filesInfoBuilder = new StringBuilder();
			long downloadSize = 0L;
			int filesCount = 0;
			IAssetManifest assetManifest = AssetManifest.Get();
			List<string> tagsForBundle = new List<string>();
			foreach (string item in enumerable)
			{
				FileInfo fileInfo = new FileInfo(item);
				if (!m_downloadedFilesList.Contains(fileInfo.FullName))
				{
					assetManifest.GetTagsFromAssetBundle(fileInfo.Name, tagsForBundle);
					filesInfoBuilder.AppendLine(fileInfo.FullName + " : " + string.Join(",", tagsForBundle));
					downloadSize += fileInfo.Length;
					m_downloadedFilesList.Add(fileInfo.FullName);
					filesCount++;
				}
			}
			Log.Downloader.PrintDebug(string.Format("Downloaded {0}, {1}", downloadType, localeUpdate ? " locale update" : "") + $" : m_downloadType = {m_downloadType}" + " : TagsDownloaded = " + string.Join(",", m_curDownloadStatus?.Tags) + $" : DownloadSize = {downloadSize}" + $" : FilesDownloaded = {filesCount}" + $" : TimeTaken = {duration}" + " : DownloadedFiles = \n" + filesInfoBuilder.ToString());
		}
		catch (Exception ex)
		{
			Log.Downloader.PrintError("Failed to print stage downloaded files info : Error = " + ex.Message);
		}
	}

	public void EnterBackgroundMode()
	{
		UpdateStatusOnce();
		UpdateDownloadedBytes();
		if (InternalAgentState != AgentInternalState.STATE_UPDATING)
		{
			Log.Downloader.PrintInfo("Do not need to record the status for Background downloading!");
			return;
		}
		BGRemainingBytes = GetCurrentDownloadStatus().BytesRemaining;
		BGRemainingBytesStartTime = TimeUtils.DateTimeToUnixTimeStampMilliseconds(DateTime.UtcNow);
	}

	public void OnForeground()
	{
		if (BGRemainingBytes > 0)
		{
			ShouldReportBGDownloadResult = true;
		}
	}

	public void ExitFromBackgroundMode()
	{
		OnForeground();
	}

	public void UnknownSourcesListener(string onOff)
	{
		Log.Downloader.PrintInfo("Unknown sources: " + onOff);
		m_allowUnknownApps = onOff == "on";
		StartApkDownload();
		Log.Downloader.PrintInfo("Start to update APK");
	}

	public void InstallAPKListener(string status)
	{
		Log.Downloader.PrintInfo("install APK: " + status);
		if (!(status == "success"))
		{
			TelemetryManager.Client().SendApkInstallFailure(m_agentStatus.m_cachedState.m_baseState.m_currentVersionStr, "exception: " + status);
			IsReportedApkInstallFailure = true;
			OpenAppStoreAlert();
		}
		else
		{
			StartupDialog.ShowStartupDialog(GameStrings.Get("GLOBAL_PROGRAMNAME_HEARTHSTONE"), GameStrings.Get("GLOBAL_RELAUNCH_APPLICATION_AFTER_INSTALLAPK"), GameStrings.Get("GLOBAL_QUIT"), delegate
			{
				ShutdownApplication();
			});
		}
	}

	public void AllowNotificationListener(string onOff)
	{
		Log.Downloader.PrintInfo("Allow notification: " + onOff);
	}

	private void HandleApkInstallationSuccess(ApkInstallOperations op)
	{
		if (PlatformSettings.RuntimeOS != OSCategory.Android)
		{
			return;
		}
		switch (op)
		{
		case ApkInstallOperations.SAVE_BEFORE_APP_CLOSING:
			ApkInstallStart = ApkInstallConvertToString(GetInstalledVersionCode, m_agentStatus.m_cachedState.m_baseState.m_currentVersionStr, TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now.AddSeconds(ElapsedTimeFromStart(m_apkUpdateStartTime) * -1f)));
			break;
		case ApkInstallOperations.LOAD_FROM_NEW_APP:
		{
			int oldVersionCode;
			string updatedVersionStr;
			ulong apkUpdateStartTime;
			bool num = ApkInstallConvertFromString(ApkInstallStart, out oldVersionCode, out updatedVersionStr, out apkUpdateStartTime);
			ApkInstallStart = null;
			if (num)
			{
				if (IsReportedApkInstallFailure)
				{
					Log.Downloader.PrintDebug("ApkInstallFailure is already reported. No need to process.");
					IsReportedApkInstallFailure = false;
				}
				else if (oldVersionCode == GetInstalledVersionCode)
				{
					Log.Downloader.PrintInfo("Report as ApkInstallFailure: same version {0}", oldVersionCode);
					TelemetryManager.Client().SendApkInstallFailure(updatedVersionStr, "same version code");
				}
				else
				{
					Log.Downloader.PrintDebug("Report ApkInstallSuccess");
					m_deviceSpace = FreeSpace.Measure();
					TelemetryManager.Client().SendApkInstallSuccess(updatedVersionStr, (float)m_deviceSpace / 1048576f, TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now) - apkUpdateStartTime);
				}
			}
			break;
		}
		}
	}

	protected string ApkInstallConvertToString(int oldVersionCode, string updatedVersionStr, ulong updateStartTime)
	{
		return string.Join("|", oldVersionCode.ToString(), updatedVersionStr, updateStartTime.ToString());
	}

	protected bool ApkInstallConvertFromString(string buf, out int oldVersionCode, out string updatedVersionStr, out ulong updateStartTime)
	{
		oldVersionCode = 0;
		updatedVersionStr = null;
		updateStartTime = 0uL;
		string[] values = buf.Split("|"[0]);
		if (values.Length != 3)
		{
			return false;
		}
		if (IsReportedApkInstallFailure)
		{
			Log.Downloader.PrintDebug("ApkInstallFailure is already reported. No need to process.");
			IsReportedApkInstallFailure = false;
			return false;
		}
		if (!int.TryParse(values[0], out oldVersionCode))
		{
			Log.Downloader.PrintError("Failed to convert the version code: {0}", values[0]);
			return false;
		}
		updatedVersionStr = values[1];
		if (!ulong.TryParse(values[2], out updateStartTime))
		{
			Log.Downloader.PrintError("Failed to convert the start time: {0}", values[2]);
			return false;
		}
		return true;
	}

	private float ElapsedTimeFromStart(float startTime)
	{
		return Time.realtimeSinceStartup - startTime;
	}

	private static int GetTagsHashCode(string[] tags)
	{
		int hash = 17;
		foreach (string tag in tags)
		{
			hash ^= tag.GetHashCode();
		}
		return hash ^ Localization.GetLocaleHashCode();
	}

	private TagDownloadStatus CreateDownloadStatusIfNotExist(string[] tags)
	{
		int hash = GetTagsHashCode(tags);
		TagDownloadStatus status;
		if (m_tagLocations.TryGetValue(hash, out var pos))
		{
			status = m_tagDownloads.m_items[pos];
		}
		else
		{
			status = new TagDownloadStatus();
			status.Tags = tags;
			m_tagLocations[hash] = m_tagDownloads.m_items.Count;
			m_tagDownloads.m_items.Add(status);
		}
		return status;
	}

	private string[] RemoveRepeatedTags(string[] tags)
	{
		m_tempUniqueTags.Clear();
		bool foundRepeated = false;
		string[] array = tags;
		foreach (string tag in array)
		{
			if (!m_tempUniqueTags.Add(tag))
			{
				foundRepeated = true;
			}
		}
		if (foundRepeated)
		{
			tags = m_tempUniqueTags.ToArray();
		}
		return tags;
	}

	private void UpdateStatusOnce()
	{
		m_agentStatus = (m_agentInit ? AgentEmbeddedAPI.GetStatus() : null);
	}

	private void StartDownloadInternal(string[] tags, KindOfUpdate kind)
	{
		Log.Downloader.PrintInfo("StartDownload with {0}", string.Join(", ", tags));
		m_curDownloadStatus = CreateDownloadStatusIfNotExist(tags);
		if (!m_curDownloadStatus.Complete)
		{
			DownloadAllFinished = false;
			DoUpdate(kind);
		}
		else if (m_downloadType == DownloadType.OPTIONAL_DOWNLOAD)
		{
			DownloadAllFinished = true;
			m_downloadType = DownloadType.NONE;
		}
	}

	private void StartApkDownload()
	{
		StartDownloadInternal(new string[1] { "apk" }, KindOfUpdate.APK_UPDATE);
	}

	private void DeserializeDownloadStats()
	{
		if (!File.Exists(DownloadStatsPath))
		{
			return;
		}
		try
		{
			string json = File.ReadAllText(DownloadStatsPath);
			m_tagDownloads = JsonUtility.FromJson<DownloadProgress>(json);
			int pos = 0;
			m_tagDownloads.m_items.ForEach(delegate(TagDownloadStatus i)
			{
				m_tagLocations[GetTagsHashCode(i.Tags)] = pos++;
			});
		}
		catch (Exception ex)
		{
			Log.Downloader.PrintError("Unable to deserialize {0}: {1}", DownloadStatsPath, ex);
		}
	}

	private void SerializeDownloadStats()
	{
		try
		{
			string json = JsonUtility.ToJson(m_tagDownloads, !HearthstoneApplication.IsPublic());
			File.WriteAllText(DownloadStatsPath, json);
		}
		catch (Exception ex)
		{
			Log.Downloader.PrintError("Unable to serialize {0}: {1}", DownloadStatsPath, ex);
		}
	}

	private void DeleteDownloadStats()
	{
		if (File.Exists(DownloadStatsPath))
		{
			try
			{
				File.Delete(DownloadStatsPath);
				m_tagDownloads.m_items.Clear();
				m_tagLocations.Clear();
			}
			catch (Exception ex)
			{
				Error.AddDevFatal("Failed to delete the stats file({0}): {1}", DownloadStatsPath, ex.Message);
			}
		}
	}

	private IEnumerator CallVersioningWithDelay(float waitseconds)
	{
		Log.Downloader.PrintDebug($"Retry Versioning after {waitseconds} second(s)");
		yield return new WaitForSecondsRealtime(waitseconds);
		StartVersionAndError("force_refresh=true");
	}

	private void CanRetryVersion(string liveVer)
	{
		m_agentState = AgentState.NONE;
		TelemetryManager.Client().SendWrongVersionFixed(liveVer, VersionRetryCount, succeeded: false);
		if (VersionRetryCount < 2)
		{
			VersionRetryCount++;
			Processor.RunCoroutine(CallVersioningWithDelay(1f * (float)VersionRetryCount));
			return;
		}
		ExceptionReporter.Get().ReportCaughtException(new Exception("Reported WrongVersion"));
		if (IsSameMajorMinorVersions)
		{
			TelemetryManager.Client().SendWrongVersionContinued(liveVer, UpdateState);
			IsVersionChanged = true;
			ResetToUpdate(clearEssentialLooseFiles: false);
			GoToIdleState();
			Log.Downloader.PrintInfo("Keep downloading the live version data");
			return;
		}
		Log.Downloader.PrintInfo("Show wrong version window");
		StartupDialog.ShowStartupDialog(GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER"), GameStrings.Get("GLUE_WRONG_VERSION_MESSAGE"), GameStrings.Get("GLOBAL_QUIT"), delegate
		{
			Log.Downloader.PrintInfo("Delete Agent folder for next clean try");
			DeleteAgentFolder();
			ResetToUpdate(clearEssentialLooseFiles: true);
			ShutdownApplication();
		});
	}

	private void DeleteAgentFolder()
	{
		try
		{
			Directory.Delete(Path.Combine(INSTALL_PATH, "__agent__"), recursive: true);
		}
		catch (Exception ex)
		{
			Error.AddDevFatal("Failed to clear Agent folder: {0}", ex.Message);
		}
	}

	private void DeleteInvalidAssetManifest()
	{
		FileInfo[] files = new DirectoryInfo(InstallDataPath).GetFiles("asset_manifest*.unity3d");
		foreach (FileInfo file in files)
		{
			try
			{
				Log.Downloader.PrintWarning("Found the invalid asset bundle(" + file.Name + "), Deleting");
				file.Delete();
				m_invalidBundles.Add(file.Name);
			}
			catch (Exception ex)
			{
				Log.Downloader.PrintError("Failed to delete the invalid bundle(" + file.Name + "): " + ex.Message);
			}
		}
	}

	private void DeleteAgentLogFolder()
	{
		try
		{
			if (Directory.Exists(PlatformFilePaths.OldAgentLogPath))
			{
				Directory.Delete(PlatformFilePaths.OldAgentLogPath, recursive: true);
			}
		}
		catch (Exception ex)
		{
			Error.AddDevFatal("Failed to clear the old Agent Log folder: {0}", ex.Message);
		}
	}

	private void AgentInitializeWhenReady()
	{
		if (AndroidDeviceSettings.Get().m_determineSDCard && !m_agentInit)
		{
			m_tagIndicatorManager.Initialize(InstallDataPath);
			if (NeedToRedownload)
			{
				ResetToUpdate(clearEssentialLooseFiles: true);
				NeedToRedownload = false;
			}
			AndroidDeviceSettings.Get().DeleteOldNotificationChannels();
			Log.Downloader.PrintInfo("Set listeners for Agent");
			m_callbackDisposer = AgentEmbeddedAPI.Subscribe(this);
			if (!string.IsNullOrEmpty(VersionToken))
			{
				Log.Downloader.PrintInfo("Token is specified: {0}", VersionToken);
			}
			string region = GetNGDPRegion();
			m_agentState = AgentState.WAIT_SERVICE;
			Log.Downloader.PrintInfo("Initialization: ***, region: " + region);
			if (!AgentEmbeddedAPI.Initialize(INSTALL_PATH, PlatformFilePaths.AgentLogPath, VersionToken, region, allowVersionRefresh: true))
			{
				Error.AddDevFatal("Failed to initialize Agent service");
				m_agentState = AgentState.ERROR;
				return;
			}
			m_bundleDataList = new BundleDataList(InstallDataPath);
			m_agentInit = true;
			AgentEmbeddedAPI.SetTelemetry(enable: true);
			DeserializeDownloadStats();
			OnForeground();
			StartVersion();
			PrintStageDownloadedInfo(DownloadType.NONE, localeUpdate: false, -1f);
		}
	}

	private bool IsAgentNonResponsible()
	{
		if (InActivityTimeout < 0)
		{
			return false;
		}
		if (InternalAgentState == AgentInternalState.STATE_VERSIONING || (InternalAgentState == AgentInternalState.STATE_UPDATING && DownloadedBytes == PrevDownloadedBytes))
		{
			if ((long)DateTime.UtcNow.Subtract(m_lastAgentWorkTime).TotalSeconds > InActivityTimeout)
			{
				return true;
			}
		}
		else
		{
			m_lastAgentWorkTime = DateTime.UtcNow;
		}
		return false;
	}

	private bool ProcessMobile()
	{
		if (!IsEnabledDownload && !m_cancelledByUser)
		{
			return false;
		}
		UpdateStatusOnce();
		if (m_agentState == AgentState.ERROR && (m_agentStatus == null || !m_agentStatus.m_cachedState.m_baseState.m_playable))
		{
			if (IsInitialBackgroundDownloading)
			{
				return false;
			}
			if (!m_optionalDownload)
			{
				StartupDialog.ShowStartupDialog(GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER"), GameStrings.Get(m_errorMsg), GameStrings.Get("GLOBAL_QUIT"), delegate
				{
					ShutdownApplication();
				});
			}
			return true;
		}
		if (!m_agentInit || m_agentState == AgentState.NONE)
		{
			return false;
		}
		if (m_agentStatus == null)
		{
			return false;
		}
		if (IsWaitingAction())
		{
			return false;
		}
		if (m_agentState == AgentState.AWAITING_WIFI)
		{
			if (!Network.IsLoggedIn())
			{
				return false;
			}
			if (CanDownload)
			{
				if (m_cancelledByUser)
				{
					if (!m_showResumeDialog)
					{
						ShowResumeMessage();
						m_showResumeDialog = true;
					}
				}
				else
				{
					Log.Downloader.PrintInfo("Download start again with " + m_savedAgentState);
					StartupDialog.Destroy();
					SendResumedTelemetryMessage();
					DoUpdate(s_updateOp[m_savedAgentState]);
				}
			}
			else if (!IsInitialBackgroundDownloading && !StartupDialog.IsShown)
			{
				ShowNetworkDialog();
			}
			return false;
		}
		if (!IsUpdating())
		{
			Log.Downloader.PrintWarning("Not updating message!!!" + m_agentState);
			return true;
		}
		if (m_agentState != AgentState.VERSION && (InternalAgentState == AgentInternalState.STATE_UPDATING || InternalAgentState == AgentInternalState.STATE_READY || InternalAgentState == AgentInternalState.STATE_FINISHED))
		{
			UpdateDownloadedBytes();
		}
		if (IsAgentNonResponsible())
		{
			m_agentState = AgentState.ERROR;
			m_errorMsg = "GLUE_LOADINGSCREEN_TIMEOUT_AGENT";
			AgentEmbeddedAPI.CancelAllOperations();
		}
		switch (InternalAgentState)
		{
		case AgentInternalState.STATE_READY:
		case AgentInternalState.STATE_FINISHED:
			Log.Downloader.PrintInfo("Done!!! state=" + InternalAgentState);
			m_tagIndicatorManager.Check();
			if (m_agentState == AgentState.VERSION)
			{
				ProcessInBlobGameSettings();
				CurrentVersion = m_agentStatus.m_cachedState.m_baseState.m_currentVersionStr;
				LiveVersion = m_agentStatus.m_configuration.m_liveDisplayVersion;
				LaunchArguments.LiveBuildVersion = m_agentStatus.m_configuration.m_liveDisplayVersion;
				TelemetryManager.Client().SendVersionFinished(CurrentVersion, LiveVersion);
				Log.Downloader.PrintInfo("Current version: " + CurrentVersion + ", Live version: " + LiveVersion);
				if (LiveVersion.IndexOf('.') == -1)
				{
					VersionFetchFailed();
					break;
				}
				bool hasNewBinary = false;
				bool isChangedVersion = false;
				try
				{
					isChangedVersion = ShouldCheckBinaryUpdate(out hasNewBinary);
				}
				catch (InvalidOperationException ex)
				{
					Log.Downloader.PrintError("Wrong Version Error: {0}", ex.Message);
					CanRetryVersion(LiveVersion);
					break;
				}
				AssetBundleInfo.GetAssetBundlePath("dbf.unity3d");
				Log.Downloader.PrintInfo("Extracted DBF bundle");
				Log.Downloader.PrintInfo("IsVersionStepCompleted set.");
				IsVersionStepCompleted = true;
				if (VersionRetryCount > 0)
				{
					TelemetryManager.Client().SendWrongVersionFixed(LiveVersion, VersionRetryCount, succeeded: true);
				}
				if (NewMinSpecCheck && !ShownMinSpecWarningInThisSession)
				{
					Log.Downloader.PrintDebug($"MinSpec checking with [isChangedVersion = {isChangedVersion}, hasNewBinary = {hasNewBinary}]");
					List<MinSpecManager.MinSpecKind> warnings = MinSpecManager.Get().GetNotEnoughSpecs(isChangedVersion && !hasNewBinary, LiveVersion);
					if (ShowMinSpecWarning(isChangedVersion && !hasNewBinary, warnings))
					{
						ShownMinSpecWarningInThisSession = true;
						break;
					}
				}
				if (isChangedVersion)
				{
					FirstDownloadUserChoice = FirstDownloadChoice.DOWNLOAD_NOW;
					if (IsStoreShowingUpdateButton || hasNewBinary)
					{
						IsVersionChanged = true;
						ResetToUpdate(hasNewBinary);
						if (PlatformSettings.RuntimeOS == OSCategory.Android && (m_store == "CN" || m_store == "CN_Dashen") && NeedBinaryUpdate(report: false))
						{
							if (!AndroidDeviceSettings.Get().AllowUnknownApps())
							{
								if (AskUnkownApps)
								{
									UpdateState = "UnknownApps";
									m_agentState = AgentState.UNKNOWN_APPS;
									Log.Downloader.PrintInfo("Processing OS Unknown Sources approval");
									AndroidDeviceSettings.Get().TriggerUnknownSources("MobileCallbackManager.UnknownSourcesListener");
								}
								else
								{
									StartApkDownload();
								}
							}
							else
							{
								m_allowUnknownApps = true;
								StartApkDownload();
							}
						}
						else if (hasNewBinary)
						{
							GoToIdleState(checkDbfReady: false);
						}
						else
						{
							OpenAppStore();
						}
					}
					else if (UpdateState == "Updated")
					{
						ShouldNotDownloadOptionalData = true;
						TelemetryManager.Client().SendOldVersionInStore(LiveVersion, UpdateState, silentGo: true);
						GoToIdleState();
					}
					else
					{
						OpenAppStore();
					}
				}
				else if (m_agentState != AgentState.ERROR)
				{
					if (!IsDOPCase)
					{
						SetDbfReady();
					}
					GoToIdleState();
				}
			}
			else if (m_agentState == AgentState.UPDATE_APK)
			{
				this.ApkDownloadProgress?.Invoke(100);
				Log.Downloader.PrintInfo("UPDATE_APK Done!!!");
				m_deviceSpace = FreeSpace.Measure();
				TelemetryManager.Client().SendUpdateFinished(m_agentStatus.m_cachedState.m_baseState.m_currentVersionStr, (float)m_deviceSpace / 1048576f, ElapsedTimeFromStart(m_apkUpdateStartTime));
				TryToInstallAPK();
			}
			else if (m_agentState == AgentState.UPDATE_MANIFEST)
			{
				GoToIdleState();
			}
			else if (m_agentState == AgentState.UPDATE_GLOBAL)
			{
				if (m_curDownloadStatus.Tags.Contains(DownloadTags.GetTagString(DownloadTags.Quality.Initial)))
				{
					Log.Downloader.PrintInfo("UPDATE_GLOBAL Done!!!");
					UpdateState = "Updated";
					GoToIdleState();
				}
				else
				{
					m_agentState = AgentState.NONE;
				}
			}
			else if (m_agentState == AgentState.UPDATE_OPTIONAL)
			{
				Log.Downloader.PrintInfo("UPDATE_OPTIONAL Done!!!");
				TelemetryManager.Client().SendRuntimeUpdate(ElapsedTimeFromStart(m_updateStartTime), RuntimeUpdate.Intention.DONE);
				GoToIdleState();
				DownloadAllFinished = true;
				m_downloadType = DownloadType.NONE;
				PrintDataList();
			}
			else if (m_agentState == AgentState.UPDATE_LOCALIZED)
			{
				Log.Downloader.PrintInfo("UPDATE_LOCALIZED Done!!!");
				UpdateState = "Updated";
				GoToIdleState();
			}
			else if (m_agentState == AgentState.UPDATE_MODULE)
			{
				Log.Downloader.PrintInfo("UPDATE_MODULE Done!!!");
				GoToIdleState();
			}
			else
			{
				Log.Downloader.PrintError("State error: " + m_agentState);
				m_errorMsg = "GLUE_LOADINGSCREEN_ERROR_UPDATE";
				m_agentState = AgentState.ERROR;
			}
			break;
		case AgentInternalState.STATE_IMPEDED:
			m_agentState = AgentState.IMPEDED;
			Log.Downloader.PrintInfo("Impeded!!!");
			break;
		case AgentInternalState.STATE_CANCELED:
			if (m_pausedByNetwork && !CanDownload)
			{
				Log.Downloader.PrintInfo("Circumstances have changed from Agent.  Stopping download.");
				m_cancelledByUser = false;
				StopDownloading();
			}
			else if (!m_cancelledByUser)
			{
				Log.Downloader.PrintInfo("Canceled!!!");
				m_prevProgress = ProgressPercent;
				m_savedAgentState = m_agentState;
				m_cancelledByUser = true;
				if (ShouldShowPauseState)
				{
					DownloadPermissionManager.DownloadEnabled = false;
					ShouldShowPauseState = false;
				}
				SendDownloadStoppedTelemetryMessage(m_downloadType, m_localeDownload, byUser: true, GetCurrentDownloadingModuleTag());
				ShowResumeMessage();
			}
			break;
		case AgentInternalState.STATE_ERROR_START:
			if (m_agentState == AgentState.VERSION)
			{
				if (UpdateState == "Updated")
				{
					Log.Downloader.PrintInfo("Version failure but allow to play");
					GoToIdleState();
				}
				else
				{
					VersionFetchFailed();
				}
			}
			else
			{
				if (InternalAgentError == m_prevInternalAgentError)
				{
					break;
				}
				m_deviceSpace = FreeSpace.Measure();
				Log.Downloader.PrintInfo("Measured free space: {0}", m_deviceSpace);
				if (m_deviceSpace < RequiredBytes - DownloadedBytes)
				{
					Log.Downloader.PrintWarning("Agent might fail because of low disk space. AgentErrorCode({0})", InternalAgentError);
					InternalAgentError = 2101;
				}
				int internalAgentError = InternalAgentError;
				if (internalAgentError == 801 || internalAgentError == 2101)
				{
					if (RestartOnFalseHDDFullEnabled && m_deviceSpace > RequiredBytes - DownloadedBytes)
					{
						if (!m_retriedUpdateWithNoSpaceError)
						{
							SendResumedTelemetryMessage();
							DoUpdate(s_updateOp[m_agentState]);
							m_retriedUpdateWithNoSpaceError = true;
							Log.Downloader.PrintWarning("Received a false 'out of space' error. Retrying.");
						}
						else
						{
							Log.Downloader.PrintError("Received a false 'out of space' error again. Stop.");
							InternalAgentError = 2100;
							StopDownloading();
						}
					}
					else
					{
						Log.Downloader.PrintWarning("Out of space!");
						BlockedByDiskFull = true;
						StopDownloading();
					}
				}
				else
				{
					Error.AddDevFatal("Unidentified error Error={0}", InternalAgentError);
					if (!CanDownload)
					{
						Log.Downloader.PrintError("failed to download.  Stopping download.");
					}
					else
					{
						m_agentState = AgentState.ERROR;
						m_errorMsg = GetAgentErrorMessage(InternalAgentError);
					}
					StopDownloading();
				}
			}
			break;
		case AgentInternalState.STATE_UPDATING:
			m_tagIndicatorManager.Check();
			if (m_agentState == AgentState.VERSION)
			{
				break;
			}
			if (m_agentStatus.m_cachedState.m_updateProgress.m_progressDetails.m_impediment == 811)
			{
				SetEncryptionKey();
				return false;
			}
			if (!IsDbfReady && GetDownloadStatus(s_dbfTags).Complete)
			{
				SetDbfReady();
				FilterForiCloud();
			}
			if (!AreStringsReady && GetDownloadStatus(s_stringsTags).Complete)
			{
				SetStringsReady();
			}
			ReportBGDownloadResult(completed: false);
			if (m_cancelledByUser)
			{
				SendResumedTelemetryMessage();
				m_cancelledByUser = false;
				StartupDialog.Destroy();
				if (!CanDownload && m_optionalDownload)
				{
					Log.Downloader.PrintInfo("Agent Service has been resumed.");
					DownloadPermissionManager.DownloadEnabled = true;
				}
			}
			else if (!CanDownload)
			{
				Log.Downloader.PrintInfo("Circumstances have changed.  Stopping download.");
				StopDownloading();
			}
			if (ElapsedTimeFromStart(m_lastUpdateProgressReportTime) > 15f)
			{
				if (m_agentState == AgentState.UPDATE_APK)
				{
					TelemetryManager.Client().SendUpdateProgress(ElapsedTimeFromStart(m_apkUpdateStartTime), DownloadedBytes, TotalBytes);
					m_lastUpdateProgressReportTime = Time.realtimeSinceStartup;
				}
				else if (m_agentState == AgentState.UPDATE_GLOBAL)
				{
					TelemetryManager.Client().SendDataUpdateProgress(ElapsedTimeFromStart(m_updateStartTime), DownloadedBytes, TotalBytes);
					m_lastUpdateProgressReportTime = Time.realtimeSinceStartup;
				}
				Log.Downloader.PrintInfo("Downloading: {0} / {1}", DownloadedBytes, TotalBytes);
			}
			break;
		}
		m_prevInternalAgentError = InternalAgentError;
		return false;
	}

	private void SetDbfReady()
	{
		Log.Downloader.PrintInfo("DBF is ready!");
		IsDbfReady = true;
		m_tagIndicatorManager.Check();
	}

	private void SetStringsReady()
	{
		Log.Downloader.PrintInfo("Strings are ready!");
		AreStringsReady = true;
		m_tagIndicatorManager.Check();
	}

	private bool ShouldCheckBinaryUpdate(out bool hasNewBinary)
	{
		hasNewBinary = false;
		int[] binaryVersionInt = new int[4] { 31, 6, 0, GetInstalledVersionCode };
		int diff = 0;
		try
		{
			if (GetInstalledVersionCode > 0 && UpdateUtils.GetSplitVersion(LiveVersion, out var liveVersionsionInt))
			{
				diff = CompareVersions(liveVersionsionInt, binaryVersionInt);
				IsSameMajorMinorVersions = liveVersionsionInt[0] == binaryVersionInt[0] && liveVersionsionInt[1] == binaryVersionInt[1];
				IsDOPCase = liveVersionsionInt.Length > 4;
			}
		}
		catch (Exception ex)
		{
			Error.AddDevFatal("Failed to check the binary version with CurrentVersion:{0} LiveVersion:{1}: {2}", CurrentVersion, LiveVersion, ex.Message);
		}
		Log.Downloader.PrintDebug($"diff = {diff}, CurrentVersion = {CurrentVersion}, LiveVersion = {LiveVersion}");
		if (UpdateState == "UpdateAPK")
		{
			hasNewBinary = diff == 0;
			return true;
		}
		if (diff < 0)
		{
			Log.Downloader.PrintError("The binary is newer than the live version: LiveVersion:{0} binaryVer:{1}", LiveVersion, binaryVersionInt[0] + "." + binaryVersionInt[1] + ".?." + binaryVersionInt[3]);
			if (CheckLowerVersion)
			{
				throw new InvalidOperationException("Binary Version is greater than the live version");
			}
			if (string.IsNullOrEmpty(CurrentVersion))
			{
				return false;
			}
		}
		if (string.IsNullOrEmpty(CurrentVersion))
		{
			if (diff > 0)
			{
				Log.Downloader.PrintInfo("The binary version is older than the live version: LiveVersion:{0} binaryVer:{1}", LiveVersion, binaryVersionInt[0] + "." + binaryVersionInt[1] + ".?." + binaryVersionInt[3]);
				return true;
			}
			UpdateState = "Update";
			if (GetInstalledVersionCode != 0 && GetInstalledVersionCode != FirstInstallClientBuildNumber)
			{
				DeleteExtractedBundles();
			}
			FirstInstallClientBuildNumber = GetInstalledVersionCode;
		}
		else
		{
			if (CurrentVersion != LiveVersion)
			{
				Log.Downloader.PrintInfo("New version is detected");
				if (diff == 0)
				{
					Log.Downloader.PrintInfo("It's new version already. {0}", GetInstalledVersionCode);
					hasNewBinary = true;
				}
				return true;
			}
			if (UpdateUtils.GetSplitVersion(CurrentVersion, out var curVersionInt) && CompareVersions(curVersionInt, binaryVersionInt, compareMajorMinorOnly: true) > 0)
			{
				Log.Downloader.PrintError("Agent already has the new version strangely, let's try to update the binary.");
				return true;
			}
		}
		return false;
	}

	private void ResetToUpdate(bool clearEssentialLooseFiles)
	{
		m_tagIndicatorManager.ClearAllIndicators();
		DeleteDownloadStats();
		DownloadAllFinished = false;
		IsDbfReady = false;
		AreStringsReady = false;
		UpdateState = "Update";
		ClearBGStatus();
		ClearDataList();
		if (clearEssentialLooseFiles)
		{
			DeleteExtractedBundles();
		}
	}

	private void DeleteExtractedBundles()
	{
		if (PlatformSettings.RuntimeOS != OSCategory.Android || !Directory.Exists(InstallDataPath))
		{
			return;
		}
		try
		{
			Log.Downloader.PrintInfo("Delete the extracted bundles.");
			(from file in Directory.EnumerateFiles(InstallDataPath)
				where Path.GetFileName(file).StartsWith("essential")
				select file).ToList().ForEach(File.Delete);
		}
		catch (Exception ex)
		{
			Error.AddDevFatal("Failed to delete the extracted asset bundles: {0}", ex.Message);
		}
	}

	protected int CompareVersions(int[] version1, int[] version2, bool compareMajorMinorOnly = false)
	{
		try
		{
			int diffMajor = version1[0] - version2[0];
			int diffMinor = version1[1] - version2[1];
			int diffVersionCode = GetBinaryVersionCode(version1) - GetBinaryVersionCode(version2);
			if (compareMajorMinorOnly)
			{
				diffVersionCode = 0;
			}
			if (diffMajor < 0 || (diffMajor == 0 && diffMinor < 0) || (diffMajor == 0 && diffMinor == 0 && diffVersionCode < 0))
			{
				return -1;
			}
			if (diffMajor > 0 || diffMinor > 0 || diffVersionCode > 0)
			{
				return 1;
			}
		}
		catch (Exception ex)
		{
			Error.AddDevFatal("Failed to compare two version array: {0}", ex.Message);
		}
		return 0;
	}

	private void FilterForiCloud()
	{
		if (PlatformSettings.RuntimeOS != OSCategory.iOS)
		{
			return;
		}
		string[] tempPaths = new string[5]
		{
			Log.ConfigPath,
			Log.LogsPath,
			PlatformFilePaths.CachePath,
			INSTALL_PATH + "/Unity",
			DownloadStatsPath
		};
		string[] excludePaths = new string[tempPaths.Length + m_dataFolders.Length];
		tempPaths.CopyTo(excludePaths, 0);
		m_dataFolders.CopyTo(excludePaths, tempPaths.Length);
		string[] array = excludePaths;
		foreach (string p in array)
		{
			if (!File.Exists(p) && !Directory.Exists(p))
			{
				Log.Downloader.PrintDebug("No file or directory: " + p);
			}
			else if (!UpdateUtils.addSkipBackupAttributeToItemAtPath(p))
			{
				Log.Downloader.PrintError("Failed to exclude from iCloud - " + Path.GetFileName(p));
			}
		}
		Log.Downloader.PrintInfo("Excluded game data folders from iCloud");
	}

	private bool SetInitialState()
	{
		VersionOverrideUrl = "Live";
		m_store = AndroidDeviceSettings.Get().m_HSStore;
		if (IsEnabledUpdate)
		{
			GameStrings.LoadNative();
			SanitizeDataFolder();
			AndroidDeviceSettings.Get().AskForSDCard();
		}
		else
		{
			if (Application.isEditor && PlatformSettings.IsEmulating)
			{
				GameStrings.LoadNative();
				m_agentState = AgentState.VERSION;
				m_versionCalled = true;
				return true;
			}
			m_agentState = AgentState.NONE;
		}
		return true;
	}

	private void SanitizeDataFolder()
	{
		Log.Downloader.PrintInfo("SanitizeDataFolder");
		try
		{
			if (INSTALL_PATH == null)
			{
				Log.Downloader.PrintDebug("No INSTALL_PATH yet!");
				return;
			}
			string baseDataFolder = Path.Combine(INSTALL_PATH, "Data");
			if (!Directory.Exists(baseDataFolder))
			{
				Log.Downloader.PrintDebug("No Data folder yet: " + baseDataFolder);
				if (!string.IsNullOrEmpty(UpdateState))
				{
					Log.Downloader.PrintInfo("It looks like to be restored from backup, need to download...");
					NeedToRedownload = true;
				}
				return;
			}
			DeleteInvalidAssetManifest();
			DeleteAgentLogFolder();
			int redownload = Vars.Key("Debug.Redownload").GetInt(0);
			if (redownload > 0)
			{
				Log.Downloader.PrintDebug($"Debug.Redownload set with '{redownload}");
				if (redownload >= 3)
				{
					DeleteDownloadedData();
					Log.Downloader.PrintDebug("Deleted Data folder");
				}
				if (redownload >= 2)
				{
					DeleteAgentFolder();
					Log.Downloader.PrintDebug("Deleted Agent folder");
				}
				NeedToRedownload = true;
			}
			else
			{
				CleanupUnusedLanguages();
			}
		}
		catch (Exception ex)
		{
			Error.AddDevFatal("Failed to sanitize Data folder: {0}", ex.Message);
		}
	}

	private List<string> AddtionalLocales()
	{
		List<string> list = Options.Get().GetString(Option.INSTALLED_LOCALES).Split(',')
			.ToList();
		string currentLoc = Localization.GetLocaleName();
		if (currentLoc.Equals("enGB"))
		{
			currentLoc = "enUS";
		}
		list.RemoveAll((string l) => l.Equals(currentLoc));
		return list;
	}

	private void CleanupUnusedLanguages()
	{
		if (MaxAgeInactiveLocales < 0)
		{
			Log.Downloader.PrintInfo("Skip to check inactive locale data");
			return;
		}
		List<string> additionalLocales = AddtionalLocales();
		if (additionalLocales.Count() == 0)
		{
			Log.Downloader.PrintDebug("There is no inactive locale data.");
			return;
		}
		string addtionalLocaleStr = string.Join(",", additionalLocales);
		if (!Options.Get().HasOption(Option.CURRENT_LOCALE_TIME_STAMP))
		{
			Options.Get().SetULong(Option.CURRENT_LOCALE_TIME_STAMP, TimeUtils.DateTimeToUnixTimeStampMilliseconds(DateTime.UtcNow));
		}
		DateTime saveTimeStamp = TimeUtils.UnixTimeStampMillisecondsToDateTimeUtc(Convert.ToInt64(Options.Get().GetULong(Option.CURRENT_LOCALE_TIME_STAMP)));
		int days = (int)DateTime.UtcNow.Subtract(saveTimeStamp).TotalDays;
		if (days <= MaxAgeInactiveLocales)
		{
			Log.Downloader.PrintDebug("Inactive locales('{0}') are detected but no need to delete yet: {1} <= {2}", addtionalLocaleStr, days, MaxAgeInactiveLocales);
			return;
		}
		Log.Downloader.PrintInfo("Deleting the inactive 'String' folder: " + addtionalLocaleStr);
		StringBuilder regLocales = new StringBuilder();
		string errorMsg = string.Empty;
		foreach (string locale in additionalLocales)
		{
			try
			{
				string localeStringPath = Path.Combine(INSTALL_PATH, "Strings", locale);
				if (Directory.Exists(localeStringPath))
				{
					Log.Downloader.PrintInfo("Deleting " + localeStringPath);
					Directory.Delete(localeStringPath, recursive: true);
				}
				regLocales.AppendFormat("{0}_{1}", (regLocales.Length > 0) ? "|" : "", locale.ToLower());
			}
			catch (Exception ex)
			{
				Log.Downloader.PrintError("Failed to delete the '{0}/Strings' folder: {1}", locale, ex.Message);
				if (!string.IsNullOrEmpty(errorMsg))
				{
					errorMsg += "|";
				}
				errorMsg += ex.Message;
			}
		}
		Log.Downloader.PrintInfo("Deleting the asset bundles of '{0}'", addtionalLocaleStr);
		Regex reg = new Regex(regLocales.ToString());
		try
		{
			(from file in Directory.EnumerateFiles(InstallDataPath)
				where reg.Match(file).Success
				select file).ToList().ForEach(File.Delete);
		}
		catch (Exception ex2)
		{
			Log.Downloader.PrintError("Failed to delete the asset bundles of inactive languages: {0}", ex2.Message);
			if (!string.IsNullOrEmpty(errorMsg))
			{
				errorMsg += "|";
			}
			errorMsg += ex2.Message;
		}
		TelemetryManager.Client().SendClearInactiveLocales(additionalLocales, string.IsNullOrEmpty(errorMsg), errorMsg);
		if (string.IsNullOrEmpty(errorMsg))
		{
			DeleteAgentFolder();
			NeedToRedownload = true;
		}
	}

	private bool IsWaitingAction()
	{
		if (m_agentState != AgentState.UNKNOWN_APPS && m_agentState != AgentState.WAIT_SERVICE && m_agentState != AgentState.OPEN_APP_STORE && m_agentState != AgentState.WARNING_MIN_SPEC)
		{
			return m_agentState == AgentState.ALLOW_NOTIFICATION;
		}
		return true;
	}

	private bool IsUpdating()
	{
		if (m_agentState != 0)
		{
			return m_agentState != AgentState.ERROR;
		}
		return false;
	}

	private void GoToIdleState(bool checkDbfReady = true)
	{
		m_agentState = AgentState.NONE;
		IsReady = true;
		if (checkDbfReady && GetDownloadStatus(s_dbfTags).Complete)
		{
			SetDbfReady();
		}
	}

	private string GetAdditionalTags(KindOfUpdate kind)
	{
		switch (kind)
		{
		case KindOfUpdate.VERSION:
			return "";
		case KindOfUpdate.APK_UPDATE:
			if (!m_store.Equals("CN") && !m_store.Equals("CN_Dashen"))
			{
				return "";
			}
			if (!m_allowUnknownApps)
			{
				return "";
			}
			return MobileMode + "? " + m_store + "?";
		default:
		{
			string tags = "";
			string[] tags2 = m_curDownloadStatus.Tags;
			foreach (string t in tags2)
			{
				tags += $" {t}?";
			}
			if (PlatformSettings.RuntimeOS == OSCategory.Android)
			{
				tags += $" {AndroidDeviceSettings.Get().InstalledTexture}?";
			}
			return tags;
		}
		}
	}

	private int DoUpdate(KindOfUpdate kind)
	{
		m_showResumeDialog = false;
		int ret = ((kind == KindOfUpdate.VERSION) ? AgentStartVersion("") : AgentStartUpdateGlobal(kind));
		if (ret != 0)
		{
			Error.AddDevFatal("DoUpdate({0}) Error={1}", kind.ToString(), ret);
			m_errorMsg = ((ret == 2410) ? "GLUE_LOADINGSCREEN_ERROR_UPDATE_CONFLICT" : "GLUE_LOADINGSCREEN_ERROR_UPDATE");
			m_agentState = AgentState.ERROR;
			return ret;
		}
		if (!m_optionalDownload)
		{
			UpdateState = "Update";
		}
		switch (kind)
		{
		case KindOfUpdate.VERSION:
			m_agentState = AgentState.VERSION;
			UpdateState = "Version";
			break;
		case KindOfUpdate.APK_UPDATE:
			m_agentState = AgentState.UPDATE_APK;
			UpdateState = "UpdateAPK";
			ReportUpdateStarted();
			break;
		case KindOfUpdate.MANIFEST_UPDATE:
			m_agentState = AgentState.UPDATE_MANIFEST;
			break;
		case KindOfUpdate.GLOBAL_UPDATE:
			m_agentState = AgentState.UPDATE_GLOBAL;
			m_updateStartTime = Time.realtimeSinceStartup;
			m_lastUpdateProgressReportTime = m_updateStartTime;
			break;
		case KindOfUpdate.OPTIONAL_UPDATE:
			m_agentState = AgentState.UPDATE_OPTIONAL;
			m_updateStartTime = Time.realtimeSinceStartup;
			break;
		case KindOfUpdate.LOCALE_UPDATE:
			m_agentState = AgentState.UPDATE_LOCALIZED;
			m_updateStartTime = Time.realtimeSinceStartup;
			break;
		case KindOfUpdate.MODULE_UPDATE:
			m_agentState = AgentState.UPDATE_MODULE;
			m_updateStartTime = Time.realtimeSinceStartup;
			break;
		}
		return ret;
	}

	private void ReportUpdateStarted()
	{
		m_apkUpdateStartTime = Time.realtimeSinceStartup;
		m_lastUpdateProgressReportTime = m_apkUpdateStartTime;
		m_deviceSpace = FreeSpace.Measure();
		TelemetryManager.Client().SendUpdateStarted(m_agentStatus.m_cachedState.m_baseState.m_currentVersionStr, AndroidDeviceSettings.Get().InstalledTexture, InstallDataPath, (float)m_deviceSpace / 1048576f);
	}

	private int AgentStartUpdateGlobal(KindOfUpdate kind)
	{
		m_lastAgentWorkTime = DateTime.UtcNow;
		UserSettings userSettings = default(UserSettings);
		userSettings.m_region = GetNGDPRegion();
		userSettings.m_languages = Localization.GetLocale().ToString();
		userSettings.m_branch = GetBranch();
		userSettings.m_additionalTags = GetAdditionalTags(kind);
		UserSettings settings = userSettings;
		Log.Downloader.PrintInfo("ModifyProductInstall with locale: {0}, tags: {1}, branch: {2}", settings.m_languages, settings.m_additionalTags, settings.m_branch);
		int ret = AgentEmbeddedAPI.ModifyProductInstall(ref settings);
		if (ret != 0)
		{
			Log.Downloader.PrintWarning("1st ModifyProductInstall Error={0}", ret);
			if (ret != 2410)
			{
				return ret;
			}
			AgentEmbeddedAPI.CancelAllOperations();
			ret = AgentEmbeddedAPI.ModifyProductInstall(ref settings);
			if (ret != 0)
			{
				Error.AddDevFatal("2nd ModifyProductInstall Error={0}", ret);
				return ret;
			}
		}
		if (m_curDownloadStatus != null)
		{
			m_curDownloadStatus.StartProgress = m_curDownloadStatus.Progress;
		}
		TotalBytes = 0L;
		DownloadedBytes = 0L;
		RequiredBytes = 0L;
		BytesPerSecond = 0.0;
		m_deviceSpace = -1L;
		m_cancelledByUser = false;
		NotificationUpdateSettings notificationSettings = new NotificationUpdateSettings
		{
			m_cellDataThreshold = Options.Get().GetInt(Option.CELL_PROMPT_THRESHOLD),
			m_isCellDataAllowed = m_cellularEnabledSession
		};
		ret = AgentEmbeddedAPI.StartUpdate(EncrpytionKeyOption, notificationSettings);
		ResetDownloadSpeed(MaxDownloadSpeed);
		return ret;
	}

	private int ResetDownloadSpeed(int speed)
	{
		Log.Downloader.PrintInfo("Set the download speed to {0}", speed);
		int success = AgentEmbeddedAPI.SetUpdateParams($"download_limit={speed}");
		if (success != 0)
		{
			Log.Downloader.PrintError("SetUpdateParams Error={0}", success);
		}
		return success;
	}

	private void SetEncryptionKey()
	{
		Log.Downloader.PrintDebug("Set the encryption key");
		int success = AgentEmbeddedAPI.SetUpdateParams(EncrpytionKeyOption);
		if (success != 0)
		{
			Log.Downloader.PrintError("SetUpdateParams Error={0}", success);
		}
	}

	private void UpdateDownloadedBytes()
	{
		if (m_agentStatus == null)
		{
			return;
		}
		TotalBytes = m_agentStatus.m_cachedState.m_updateProgress.m_downloadDetails.m_expectedDownloadBytes;
		PrevDownloadedBytes = DownloadedBytes;
		DownloadedBytes = m_agentStatus.m_cachedState.m_updateProgress.m_downloadDetails.m_realDownloadedBytes;
		RequiredBytes = m_agentStatus.m_cachedState.m_updateProgress.m_downloadDetails.m_expectedOriginalBytes;
		BytesPerSecond = m_agentStatus.m_cachedState.m_updateProgress.m_downloadDetails.m_downloadRate;
		m_progress = (float)m_agentStatus.m_cachedState.m_updateProgress.m_progressDetails.m_progress;
		m_tactProgressState = (TactProgressState)m_agentStatus.m_cachedState.m_updateProgress.m_progressDetails.m_state;
		PrintStatus(m_agentStatus);
		m_curDownloadStatus.BytesDownloaded = DownloadedBytes;
		m_curDownloadStatus.BytesTotal = TotalBytes;
		if (TotalBytes > 0 && m_deviceSpace == -1)
		{
			Log.Downloader.PrintInfo("Total: {0}, Downloaded: {1}, Required: {2}, Speed: {3}, Progress: {4}", TotalBytes, DownloadedBytes, RequiredBytes, BytesPerSecond, m_progress);
			m_deviceSpace = FreeSpace.Measure();
			Log.Downloader.PrintInfo("Measured free space: {0}", m_deviceSpace);
			if (RequiredBytes > m_deviceSpace)
			{
				Log.Downloader.PrintError("Device will run out of space during download.  {0} / {1}", RequiredBytes, m_deviceSpace);
				BlockedByDiskFull = true;
				StopDownloading();
			}
		}
		SerializeDownloadStats();
	}

	private void ShowStartupTextOnScreen(string msg, bool show = true)
	{
		if (HearthstoneApplication.IsInternal() && UIStatus.Get() != null)
		{
			if (show && (SceneMgr.Get() == null || SceneMgr.Get().GetMode() == SceneMgr.Mode.STARTUP))
			{
				UIStatus.Get().AddInfo(msg);
			}
			else
			{
				UIStatus.Get().StopText();
			}
		}
	}

	private void PrintStatus(ProductStatus agentStatus)
	{
		ProgressDetails p = agentStatus.m_cachedState.m_updateProgress.m_progressDetails;
		string devMsg = string.Empty;
		switch (m_tactProgressState)
		{
		case TactProgressState.CL_FETCHING_DATA_INDICES:
			devMsg = string.Format(GameStrings.Get("GLUE_LOADINGSCREEN_M_DATA_INDICES"), $"{p.m_current}/{p.m_total}");
			break;
		case TactProgressState.CL_FETCHING_PATCH_INDICES:
			devMsg = string.Format(GameStrings.Get("GLUE_LOADINGSCREEN_M_PATCH_INDICES"), $"{p.m_current}/{p.m_total}");
			break;
		case TactProgressState.CL_FETCHING_ENCODING_TABLE:
			devMsg = GameStrings.Get("GLUE_LOADINGSCREEN_M_ENCODING_TABLE");
			break;
		case TactProgressState.CL_FETCHING_INSTALLATION_MANIFEST:
			devMsg = GameStrings.Get("GLUE_LOADINGSCREEN_M_INSTALL_MANIFEST");
			break;
		case TactProgressState.CL_CHECKING_RESIDENCY:
			devMsg = GameStrings.Get("GLUE_LOADINGSCREEN_M_INSTALLATION_MANIFEST");
			break;
		case TactProgressState.CL_INSTALLING:
			if (m_agentState == AgentState.UPDATE_APK)
			{
				this.ApkDownloadProgress?.Invoke((int)(m_progress * 100f));
			}
			else if (m_agentState == AgentState.UPDATE_GLOBAL && !IsDbfReady)
			{
				int dbf_progress = (int)(GetDownloadStatus(s_dbfTags).Progress * 100f);
				this.DbfDownloadProgress?.Invoke(dbf_progress);
			}
			break;
		default:
			if (m_prevTactProgressState != m_tactProgressState)
			{
				ShowStartupTextOnScreen("", show: false);
			}
			break;
		}
		m_prevTactProgressState = (TactProgressState)p.m_state;
		if (!string.IsNullOrEmpty(devMsg))
		{
			ShowStartupTextOnScreen(devMsg);
		}
	}

	private void VersionFetchFailed()
	{
		Error.AddDevFatal("Agent: Failed to get Version! - {0}", m_agentStatus.m_cachedState.m_versionProgress.m_error);
		m_errorMsg = "GLUE_LOADINGSCREEN_ERROR_VERSION";
		m_agentState = AgentState.ERROR;
		TelemetryManager.Client().SendVersionError((uint)m_agentStatus.m_cachedState.m_versionProgress.m_error, (uint)m_agentStatus.m_cachedState.m_versionProgress.m_agentState, m_agentStatus.m_settings.m_languages, m_agentStatus.m_settings.m_region, m_agentStatus.m_settings.m_branch, m_agentStatus.m_settings.m_additionalTags);
	}

	private void ProcessInBlobGameSettings()
	{
		Log.Downloader.PrintInfo("Processing blob data...");
		string disabledApkInstall = AgentEmbeddedAPI.GetOpaqueString("disabled_apk_update");
		if (!string.IsNullOrEmpty(disabledApkInstall))
		{
			Log.Downloader.PrintInfo("Blob - disabled_apk_update: " + disabledApkInstall);
			try
			{
				m_disabledAPKUpdates = Array.ConvertAll(disabledApkInstall.Split(','), (string s) => int.Parse(s.Trim()));
			}
			catch (Exception ex)
			{
				Log.Downloader.PrintError("Failed to parse the 'disabled_apk_update' - {0}: {1}", disabledApkInstall, ex.Message);
			}
		}
		string inGameSpeed = AgentEmbeddedAPI.GetOpaqueString("in_game_streaming_speed");
		if (!string.IsNullOrEmpty(inGameSpeed))
		{
			Log.Downloader.PrintInfo("Blob - in_game_streaming_speed: " + inGameSpeed);
			try
			{
				int value = int.Parse(inGameSpeed);
				InGameStreamingDefaultSpeed = value;
			}
			catch (Exception ex2)
			{
				Log.Downloader.PrintError("Failed to parse the 'in_game_streaming_speed' - {0}: {1}", inGameSpeed, ex2.Message);
			}
		}
		string adventures = AgentEmbeddedAPI.GetOpaqueString("disabled_adventures_for_streaming");
		if (!string.IsNullOrEmpty(adventures))
		{
			Log.Downloader.PrintInfo("Blob - disabled_adventure_streaming: " + adventures);
			m_disabledAdventuresForStreaming = adventures.Split(',');
		}
		string anrThrottle = AgentEmbeddedAPI.GetOpaqueString("anr_throttle");
		if (!string.IsNullOrEmpty(anrThrottle))
		{
			Log.Downloader.PrintInfo("Blob - anr_throttle: " + anrThrottle);
			string[] values = anrThrottle.Split(',');
			float throttle = Options.Get().GetFloat(Option.ANR_THROTTLE);
			if (float.TryParse(values[0], out var new_throttle) && new_throttle != throttle)
			{
				throttle = new_throttle;
				Options.Get().SetFloat(Option.ANR_THROTTLE, throttle);
			}
			float seconds = Options.Get().GetFloat(Option.ANR_WAIT_SECONDS);
			if (values.Length > 1 && float.TryParse(values[1], out var new_seconds) && new_seconds != seconds)
			{
				seconds = new_seconds;
				Options.Get().SetFloat(Option.ANR_WAIT_SECONDS, seconds);
			}
		}
		if (UpdateUtils.ProcessBlobKey("use_ipv4_only", "Debug.FakeBlobUseIPv4Only", "IPv4 only", defaultValue: false))
		{
			Vars.Key("Mobile.IPv6Conversion").Set("false", permanent: false);
		}
		if (UpdateUtils.ProcessBlobKey("allow_lower_version", "Debug.FakeAllowLowerVersion", "allowLowerVersion", defaultValue: false))
		{
			CheckLowerVersion = false;
		}
		if (UpdateUtils.ProcessBlobKey("no_store_version_code", "Debug.FakeNoStoreVersionCode", "NoStoreVersionCode", defaultValue: false))
		{
			CheckStoreVersionCode = false;
		}
		if (UpdateUtils.ProcessBlobKey("legacy_minspec", "Debug.FakeLegacyMinSpec", null, defaultValue: false))
		{
			NewMinSpecCheck = false;
		}
		if (NewMinSpecCheck)
		{
			string minSpecBlob = UpdateUtils.ProcessBlobKey("min_spec", "Debug.FakeMinSpecBlob", null, string.Empty);
			Log.Downloader.PrintDebug("Loaded MinSpec blob:\n" + minSpecBlob);
			if (!MinSpecManager.Get().LoadJsonData(minSpecBlob))
			{
				Log.Downloader.PrintInfo("New MinSpec warning has been disabled because it failed to load.");
				NewMinSpecCheck = false;
			}
		}
		int days = UpdateUtils.ProcessBlobKey("maxage_inactive_locale", "Debug.FakeMaxAgeInactiveLocale", null, 0);
		if (days != 0)
		{
			MaxAgeInactiveLocales = days;
		}
		int inActivityTimeout = UpdateUtils.ProcessBlobKey("agent_inactivity_timeout", "Debug.FakenActivityTimeout", null, 0);
		if (inActivityTimeout != 0)
		{
			InActivityTimeout = inActivityTimeout;
		}
		string storeURIBlob = UpdateUtils.ProcessBlobKey("store_uri", "Debug.FakeStoreURI", null, string.Empty);
		Log.Downloader.PrintDebug("Loaded StoreURI blob:\n" + storeURIBlob);
		if (!UpdateUtils.LoadStoreURIJsonData(storeURIBlob))
		{
			Log.Downloader.PrintInfo("No store URI override information. Use the internal setting.");
		}
	}

	private int InCreateProduct(Func<int> preFunc = null)
	{
		UserSettings userSettings = default(UserSettings);
		userSettings.m_region = GetNGDPRegion();
		userSettings.m_languages = Localization.GetLocale().ToString();
		userSettings.m_branch = GetBranch();
		UserSettings settings = userSettings;
		preFunc?.Invoke();
		int ret = AgentEmbeddedAPI.CreateProductInstall(ProductName, ref settings);
		if (ret == 0)
		{
			Log.Downloader.PrintInfo("Installation is done! Version...");
			m_agentState = AgentState.INSTALLED;
		}
		else
		{
			Log.Downloader.PrintWarning("CreateProductInstall Error={0}", ret.ToString());
		}
		return ret;
	}

	private void CreateProduct()
	{
		int ret = InCreateProduct();
		switch (ret)
		{
		case 2410:
			ret = InCreateProduct(() => AgentEmbeddedAPI.StartUninstall(""));
			break;
		case 0:
			return;
		}
		if (ret != 0)
		{
			m_errorMsg = GetAgentErrorMessage(ret);
			m_agentState = AgentState.ERROR;
		}
	}

	private void StartVersion()
	{
		if (!m_versionCalled)
		{
			m_versionCalled = true;
			this.VersioningStarted?.Invoke();
			SetVersionOverrideUrl();
			m_agentStatus = AgentEmbeddedAPI.GetStatus();
			if (m_agentStatus == null || string.IsNullOrEmpty(m_agentStatus.m_product))
			{
				Log.Downloader.PrintInfo("initialization succeeded! Create product...");
				CreateProduct();
			}
			else
			{
				string locale = m_agentStatus.m_settings.m_languages;
				Log.Downloader.PrintInfo("initialization succeeded! Ready to update locale=" + locale);
				m_agentState = AgentState.INSTALLED;
			}
			if (m_agentState == AgentState.INSTALLED)
			{
				StartVersionAndError("");
			}
		}
	}

	private int AgentStartVersion(string options)
	{
		Log.Downloader.PrintInfo("StartVersion options=" + options);
		m_lastAgentWorkTime = DateTime.UtcNow;
		TelemetryManager.Client().SendVersionStarted(0);
		return AgentEmbeddedAPI.StartVersion(options);
	}

	private int StartVersionAndError(string options)
	{
		int ret = AgentStartVersion(options);
		if (ret != 0)
		{
			m_agentState = AgentState.ERROR;
			Error.AddDevFatal("[Downloader] StartVersion Error={0}", ret.ToString());
		}
		else
		{
			m_agentState = AgentState.VERSION;
		}
		return ret;
	}

	private void SetVersionOverrideUrl()
	{
		if (!UpdateUtils.AreUpdatesEnabledForCurrentPlatform)
		{
			return;
		}
		if (string.IsNullOrEmpty(VersionToken))
		{
			Log.Downloader.PrintError("No version override");
			return;
		}
		if (ServiceManager.Get<VersionConfigurationService>().IsInternalVersionService())
		{
			string token = string.Format(EnumUtils.GetString(Pipeline) + ":" + VersionToken);
			Log.Downloader.PrintInfo("Setting version override to token:{0}, url:{1}", token, InternalVersionService);
			AgentEmbeddedAPI.SetVersionServiceUrlOverride(ProductName, InternalVersionService, token);
		}
		else if (!Vars.Key("Debug.StopOverride").GetBool(def: false))
		{
			string url = $"https://{GetNGDPRegion()}.version.battle.net";
			if (GetNGDPRegion() == "cn")
			{
				url = $"https://{GetNGDPRegion()}.version.battlenet.com.cn";
			}
			Log.Downloader.PrintInfo("Setting version override to token for " + EnumUtils.GetString(Pipeline) + ":" + VersionToken);
			AgentEmbeddedAPI.SetVersionServiceUrlOverride(ProductName, url, VersionToken);
		}
		VersionOverrideUrl = EnumUtils.GetString(Pipeline);
	}

	private string GetNGDPRegion()
	{
		return DeviceLocaleHelper.GetCurrentRegionId() switch
		{
			BnetRegion.REGION_US => "us", 
			BnetRegion.REGION_EU => "eu", 
			BnetRegion.REGION_KR => "kr", 
			BnetRegion.REGION_TW => "kr", 
			BnetRegion.REGION_CN => "cn", 
			_ => "us", 
		};
	}

	private string GetBranch()
	{
		if (PlatformSettings.RuntimeOS == OSCategory.Android)
		{
			return "android_" + m_store.ToLower();
		}
		if (PlatformSettings.RuntimeOS == OSCategory.iOS)
		{
			if (PlatformSettings.LocaleVariant == LocaleVariant.China)
			{
				return "ios_cn";
			}
			return "ios";
		}
		return string.Empty;
	}

	private void ShutdownApplication()
	{
		Log.Downloader.PrintInfo("ShutdownApplication");
		Shutdown();
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.Exit();
		}
		else
		{
			GeneralUtils.ExitApplication();
		}
	}

	private bool ShowMinSpecWarning(bool isNextVersionWarning, List<MinSpecManager.MinSpecKind> warnings)
	{
		Option hasShownOption = (isNextVersionWarning ? Option.HAS_SHOWN_MINSPEC_NEXT_VERSION_WARNING : Option.HAS_SHOWN_DEVICE_PERFORMANCE_WARNING);
		if (warnings.Count == 0)
		{
			Log.Downloader.PrintInfo($"MinSpec warning is gone. Clear the option: {hasShownOption}");
			Options.Get().DeleteOption(hasShownOption);
			return false;
		}
		if (Options.Get().GetBool(hasShownOption))
		{
			return false;
		}
		Log.Downloader.PrintInfo("Show MinSpec warning popup window");
		m_agentState = AgentState.WARNING_MIN_SPEC;
		List<string> minWarnings = new List<string>();
		minWarnings = warnings.ConvertAll((MinSpecManager.MinSpecKind e) => e.ToString());
		TelemetryManager.Client().SendMinSpecWarning(isNextVersionWarning, minWarnings);
		bool showAlways = false;
		string keyBody;
		if (warnings.Contains(MinSpecManager.MinSpecKind.OS_SPEC))
		{
			keyBody = "GLUE_DEVICE_OSVERSION_NEXT_VERSION_WARNING";
			showAlways = true;
		}
		else if (!warnings.Contains(MinSpecManager.MinSpecKind.OPENGL_SPEC))
		{
			keyBody = ((!isNextVersionWarning) ? "GLUE_DEVICE_PERFORMANCE_WARNING_2" : "GLUE_DEVICE_PERFORMANCE_NEXT_VERSION_WARNING");
		}
		else
		{
			keyBody = "GLUE_DEVICE_OPENGLVERSION_NEXT_VERSION_WARNING";
			showAlways = true;
		}
		StartupDialog.ShowStartupDialog(GameStrings.Get("GLOBAL_WARNING_GENERIC_HEADER"), GameStrings.Get(keyBody), GameStrings.Get("GLOBAL_OKAY"), delegate
		{
			StartVersionAndError("");
		}, closeAtClick1: true, GameStrings.Get("GLOBAL_MORE"), delegate
		{
			Application.OpenURL((DeviceLocaleHelper.GetCurrentRegionId() == BnetRegion.REGION_CN) ? "https://support.blizzardgames.cn/article/hearthstone-system-requirements" : "https://support.blizzard.com/article/hearthstone-system-requirements");
		}, closeAtClick2: false);
		if (!showAlways)
		{
			Options.Get().SetBool(hasShownOption, val: true);
		}
		return true;
	}

	private void OpenAppStore()
	{
		m_agentState = AgentState.OPEN_APP_STORE;
		if (IsStoreShowingUpdateButton)
		{
			OpenAppStoreAlert();
			return;
		}
		TelemetryManager.Client().SendOldVersionInStore(LiveVersion, UpdateState, silentGo: false);
		StartupDialog.ShowStartupDialog(GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER"), GameStrings.Get("GLUE_LOADINGSCREEN_NO_NEW_BINARY_IN_STORE"), GameStrings.Get("GLUE_LOADINGSCREEN_OPEN_APP_STORE"), delegate
		{
			HandleOpenAppStore();
		});
	}

	private bool InstallAPK()
	{
		if (!NeedBinaryUpdate(report: true))
		{
			return false;
		}
		if (SendToAppStore())
		{
			OpenAppStore();
		}
		else
		{
			string downloadedApkPath = Path.Combine(Path.Combine(INSTALL_PATH, "apk"), $"Hearthstone_{m_store}_{MobileMode}.apk");
			Log.Downloader.PrintInfo("ApkPath: " + downloadedApkPath);
			if (AndroidDeviceSettings.Get().m_AndroidSDKVersion < 24)
			{
				string apkPath = Path.Combine(PlatformFilePaths.BaseExternalDataPath, "Hearthstone.apk");
				Log.Downloader.PrintInfo("Copy apk: " + downloadedApkPath + " -> " + apkPath);
				try
				{
					File.Delete(apkPath);
					File.Copy(downloadedApkPath, apkPath);
					downloadedApkPath = apkPath;
				}
				catch (Exception ex)
				{
					Error.AddDevFatal("Failed to copy APK, Open app store instead: {0}", ex.Message);
					m_agentState = AgentState.OPEN_APP_STORE;
					OpenAppStore();
					return true;
				}
			}
			HandleApkInstallationSuccess(ApkInstallOperations.SAVE_BEFORE_APP_CLOSING);
			ShowStartupTextOnScreen("", show: false);
			AndroidDeviceSettings.Get().ProcessInstallAPK(downloadedApkPath, "MobileCallbackManager.InstallAPKListener");
		}
		return true;
	}

	private void TryToInstallAPK()
	{
		if (PlatformSettings.RuntimeOS == OSCategory.Android && InstallAPK())
		{
			m_agentState = AgentState.NONE;
		}
		else
		{
			GoToIdleState(checkDbfReady: false);
		}
	}

	private bool SendToAppStore()
	{
		if (m_store == "Amazon" || m_store == "Google" || m_store == "CN_Huawei" || m_store == "OneStore")
		{
			return true;
		}
		if (m_store == "CN" || m_store == "CN_Dashen")
		{
			if (m_allowUnknownApps && AndroidDeviceSettings.Get().AllowUnknownApps() && !IsDisabledAPKUpdateVersion())
			{
				return false;
			}
		}
		else if (AndroidDeviceSettings.Get().IsNonStoreAppAllowed())
		{
			return false;
		}
		return true;
	}

	private bool IsDisabledAPKUpdateVersion()
	{
		bool disabled = m_disabledAPKUpdates != null && Array.Exists(m_disabledAPKUpdates, (int s) => s == 216423);
		if (disabled)
		{
			Log.Downloader.PrintInfo("The current version-{0} is disabled for APK update.", 216423);
		}
		return disabled;
	}

	private bool NeedBinaryUpdate(bool report)
	{
		int binaryVersionFromLive = GetBinaryVersionCode(LiveVersion);
		if (report)
		{
			Log.Downloader.PrintInfo("InstalledVersion: {0}, BinaryVersionFromLive: {1}", GetInstalledVersionCode, binaryVersionFromLive);
			int downloadedVersionFromAgent = GetVersionCode(m_agentStatus.m_cachedState.m_baseState.m_currentVersionStr);
			TelemetryManager.Client().SendApkUpdate(GetInstalledVersionCode, binaryVersionFromLive, downloadedVersionFromAgent);
		}
		return binaryVersionFromLive != GetInstalledVersionCode;
	}

	protected int GetVersionCode(string versionString)
	{
		if (UpdateUtils.GetSplitVersion(versionString, out var versionInt))
		{
			return versionInt[3];
		}
		return 0;
	}

	protected int GetBinaryVersionCode(string versionString)
	{
		if (UpdateUtils.GetSplitVersion(versionString, out var versionInt))
		{
			return GetBinaryVersionCode(versionInt);
		}
		return 0;
	}

	protected int GetBinaryVersionCode(int[] versionInt)
	{
		if (versionInt.Length <= 4)
		{
			return versionInt[3];
		}
		return versionInt[4];
	}

	private void ShowResumeMessage()
	{
		if (!m_optionalDownload && !IsInitialBackgroundDownloading)
		{
			StartupDialog.ShowStartupDialog(GameStrings.Get("GLUE_LOADINGSCREEN_UPDATE_HEADER"), GameStrings.Get("GLUE_LOADINGSCREEN_RESUME"), GameStrings.Get("GLOBAL_RESUME_GAME"), delegate
			{
				Log.Downloader.PrintInfo("Resume the update which has stopped.");
				SendResumedTelemetryMessage();
				DoUpdate(s_updateOp[m_savedAgentState]);
			});
		}
	}

	private void SendResumedTelemetryMessage()
	{
		Log.Downloader.PrintDebug("SendResume");
		if (m_savedAgentState == AgentState.UPDATE_APK)
		{
			ReportUpdateStarted();
		}
		else
		{
			SendDownloadStartedTelemetryMessage(m_downloadType, m_localeDownload, GetCurrentDownloadingModuleTag());
		}
	}

	private DownloadTags.Content GetCurrentDownloadingModuleTag()
	{
		if (m_downloadType != DownloadType.MODULE_DOWNLOAD)
		{
			return DownloadTags.Content.Unknown;
		}
		string[] array = m_curDownloadStatus?.Tags;
		for (int i = 0; i < array.Length; i++)
		{
			DownloadTags.Content moduleTag = DownloadTags.GetContentTag(array[i]);
			if (moduleTag != 0)
			{
				return moduleTag;
			}
		}
		return DownloadTags.Content.Unknown;
	}

	private void OpenAppStoreAlert()
	{
		Log.Downloader.PrintInfo("Show App store popup window");
		m_deviceSpace = FreeSpace.Measure();
		TelemetryManager.Client().SendOpeningAppStore(m_agentStatus.m_cachedState.m_baseState.m_currentVersionStr, (float)m_deviceSpace / 1048576f, ElapsedTimeFromStart(m_apkUpdateStartTime), StrVersionCodeInStore);
		string appStoreDialogMessage = ((m_store == "CN") ? "GLUE_LOADINGSCREEN_APK_UPDATE_FROM_WEBSITE" : "GLUE_LOADINGSCREEN_APK_UPDATE_FROM_APP_STORE");
		StartupDialog.ShowStartupDialog(GameStrings.Get("GLUE_LOADINGSCREEN_UPDATE_HEADER"), GameStrings.Get(appStoreDialogMessage), GameStrings.Get("GLUE_LOADINGSCREEN_OPEN_APP_STORE"), delegate
		{
			HandleOpenAppStore();
		});
	}

	private void HandleOpenAppStore()
	{
		Log.Downloader.PrintInfo("Open App store");
		if (!UpdateUtils.OpenAppStore())
		{
			StartupDialog.ShowStartupDialog(GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER"), GameStrings.Get("GLUE_LOADINGSCREEN_ERROR_OPENING_STORE_MESSAGE"), GameStrings.Get("GLOBAL_QUIT"), delegate
			{
				ShutdownApplication();
			});
			Error.AddDevFatal("Failed to open Store.");
		}
		ShutdownApplication();
	}

	private string GetAgentErrorMessage(int errorCode)
	{
		switch (errorCode)
		{
		case 2100:
		case 2101:
		case 2110:
		case 2111:
		case 2112:
		case 2113:
		case 2115:
		case 2116:
		case 2118:
		case 2120:
		case 2121:
		case 2122:
		case 2123:
		case 2126:
			return "GLUE_LOADINGSCREEN_ERROR_FILESYSTEM_MESSAGE";
		default:
			return "GLUE_LOADINGSCREEN_ERROR_UPDATE";
		}
	}

	protected void StopDownloading()
	{
		if (IsUpdating())
		{
			m_savedAgentState = m_agentState;
			Log.Downloader.PrintInfo("Calling CancelAllOperations!");
			if (InternalAgentError != 0)
			{
				SendDownloadFailedTelemetryMessage(m_downloadType, m_localeDownload, GetCurrentDownloadingModuleTag());
			}
			else
			{
				SendDownloadStoppedTelemetryMessage(m_downloadType, m_localeDownload, !DownloadPermissionManager.DownloadEnabled, GetCurrentDownloadingModuleTag());
			}
			m_agentState = AgentState.NONE;
			AgentEmbeddedAPI.CancelAllOperations();
		}
		if (BlockedByDiskFull)
		{
			m_errorMsg = "GLUE_LOADINGSCREEN_ERROR_DISK_SPACE";
			m_agentState = AgentState.ERROR;
			m_deviceSpace = FreeSpace.Measure();
			TelemetryManager.Client().SendNotEnoughSpaceError((ulong)m_deviceSpace, (ulong)TotalBytes, PlatformFilePaths.BaseExternalDataPath, !m_optionalDownload, m_localeDownload);
		}
		else
		{
			ShowNetworkDialog();
		}
	}

	private void ShowNetworkDialog()
	{
		Log.Downloader.PrintInfo("ShowNetworkDialog");
		StartupDialog.Destroy();
		if (BlockedByCellPermission)
		{
			Log.Downloader.PrintInfo("Block by cell permission");
			if (ShouldShowCellPopup)
			{
				ShowCellularAllowance();
			}
			else
			{
				ShowAwaitingWifi();
			}
		}
		else if (BlockedByFirstDownloadPrompt)
		{
			Log.Downloader.PrintInfo("Block by the first download");
			if (FirstDownloadUserChoice == FirstDownloadChoice.NO_SELECTION)
			{
				ShowFirstDownloadPrompt();
			}
		}
		else if (!InternetAvailable)
		{
			ShowAwaitingWifi();
		}
		else if (!m_optionalDownload)
		{
			m_errorMsg = "GLUE_LOADINGSCREEN_ERROR_UPDATE";
			m_agentState = AgentState.ERROR;
		}
	}

	private void ShowFirstDownloadPrompt()
	{
		Log.Downloader.PrintInfo("Asking for the first download");
		string formattedBytes = DownloadUtils.FormatBytesAsHumanReadable(RemainingDownloadBytes);
		StartupDialog.ShowStartupDialog(GameStrings.Get("GLUE_LOADINGSCREEN_UPDATE_HEADER"), string.Format(GameStrings.Get("GLOBAL_ASSET_DOWNLOAD_FIRST_DOWNLOAD_BODY"), formattedBytes), GameStrings.Get("GLOBAL_BUTTON_OK"), delegate
		{
			Log.Downloader.PrintInfo("User said 'download'.");
			FirstDownloadUserChoice = FirstDownloadChoice.DOWNLOAD_NOW;
			DoUpdate(s_updateOp[m_savedAgentState]);
		});
	}

	private void ShowCellularAllowance()
	{
		Log.Downloader.PrintInfo("Asking for cell permission");
		string formattedBytes = DownloadUtils.FormatBytesAsHumanReadable(RemainingDownloadBytes);
		StartupDialog.ShowStartupDialog(GameStrings.Get("GLOBAL_ASSET_DOWNLOAD_CELLULAR_POPUP_HEADER"), string.Format(GameStrings.Get(m_optionalDownload ? "GLOBAL_ASSET_DOWNLOAD_CELLULAR_POPUP_ADDITIONAL_BODY" : "GLOBAL_ASSET_DOWNLOAD_CELLULAR_POPUP_INITIAL_BODY"), formattedBytes), GameStrings.Get("GLOBAL_BUTTON_YES"), delegate
		{
			m_deviceSpace = FreeSpace.Measure();
			TelemetryManager.Client().SendUsingCellularData(m_agentStatus.m_cachedState.m_baseState.m_currentVersionStr, (float)m_deviceSpace / 1048576f, ElapsedTimeFromStart(m_apkUpdateStartTime), !m_optionalDownload, m_localeDownload);
			Log.Downloader.PrintInfo("User said yes to cell prompt.");
			m_cellularEnabledSession = true;
			SendResumedTelemetryMessage();
			DoUpdate(s_updateOp[m_savedAgentState]);
			m_askedCellAllowance = true;
		}, GameStrings.Get("GLOBAL_BUTTON_NO"), delegate
		{
			Log.Downloader.PrintInfo("User said no to cell prompt.");
			ShowAwaitingWifi();
			m_askedCellAllowance = true;
		});
		m_agentState = AgentState.AWAITING_WIFI;
	}

	private void ShowAwaitingWifi()
	{
		Log.Downloader.PrintInfo("Awaiting Wifi");
		m_deviceSpace = FreeSpace.Measure();
		TelemetryManager.Client().SendNoWifi(m_agentStatus.m_cachedState.m_baseState.m_currentVersionStr, (float)m_deviceSpace / 1048576f, ElapsedTimeFromStart(m_apkUpdateStartTime), !m_optionalDownload, m_localeDownload);
		if (!m_optionalDownload && !IsInitialBackgroundDownloading)
		{
			Log.Downloader.PrintInfo("Showing the Wifi awaiting");
			StartupDialog.ShowStartupDialog(GameStrings.Get("GLUE_LOADINGSCREEN_UPDATE_HEADER"), GameStrings.Get("GLUE_LOADINGSCREEN_ERROR_CHECK_SETTINGS"), GameStrings.Get("GLUE_LOADINGSCREEN_BUTTON_SETTINGS"), delegate
			{
				Log.Downloader.PrintInfo("Check your wireless settings.");
				UpdateUtils.ShowWirelessSettings();
			}, closeAtClick: false);
		}
		m_agentState = AgentState.AWAITING_WIFI;
	}

	private void ReportBGDownloadResult(bool completed)
	{
		if (ShouldReportBGDownloadResult && (completed || TotalBytes != 0L) && RemainingDownloadBytes != BGRemainingBytes)
		{
			DateTime t = TimeUtils.UnixTimeStampMillisecondsToDateTimeUtc(Convert.ToInt64(BGRemainingBytesStartTime));
			float duration = (float)DateTime.UtcNow.Subtract(t).TotalSeconds;
			if (!(duration <= 0f) && BGRemainingBytes > 0)
			{
				ShouldReportBGDownloadResult = false;
				Log.Downloader.PrintDebug($"BGDownloadResult: {duration}, {BGRemainingBytes}, {RemainingDownloadBytes}, downloaded: {BGRemainingBytes - RemainingDownloadBytes}");
				TelemetryManager.Client().SendBGDownloadResult(duration, BGRemainingBytes, BGRemainingBytes - RemainingDownloadBytes);
				ClearBGStatus();
			}
		}
	}

	private void ClearBGStatus()
	{
		BGRemainingBytes = 0L;
		BGRemainingBytesStartTime = 0uL;
	}

	private void ClearDataList()
	{
		if (m_bundleDataList != null)
		{
			m_bundleDataList.Clear();
		}
		if (m_downloadedFilesList != null)
		{
			m_downloadedFilesList.Clear();
		}
	}

	private void PrintDataList()
	{
		if (m_bundleDataList != null)
		{
			Processor.RunCoroutine(m_bundleDataList.Print());
			m_bundleDataList.Clear();
		}
	}
}
