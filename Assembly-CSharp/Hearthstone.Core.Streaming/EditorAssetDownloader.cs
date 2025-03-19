using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blizzard.T5.Core.Utils;
using Cysharp.Threading.Tasks;
using Hearthstone.Util;
using UnityEngine;

namespace Hearthstone.Core.Streaming;

public class EditorAssetDownloader : IAssetDownloader
{
	public enum DownloadMode
	{
		None,
		Local,
		Build
	}

	[Serializable]
	public class AssetBundleDownloadProgress
	{
		public string Name;

		public string Path;

		public string SrcPath;

		public string[] Tags = new string[0];

		public long BytesTotal;

		public long BytesDownloaded;
	}

	[Serializable]
	public class DownloadProgress
	{
		public string BranchName;

		public int BuildNumber;

		public List<AssetBundleDownloadProgress> AssetBundles = new List<AssetBundleDownloadProgress>();
	}

	private class FileCopier
	{
		public enum State
		{
			INITIAL,
			DOWNLOADING,
			COMPLETED,
			FAILED
		}

		private readonly string m_srcPath;

		private readonly string m_dstPath;

		private readonly byte[] m_buffer;

		private readonly bool m_overwriteIfExists;

		private readonly Action<FileCopier> m_onProgressUpdated;

		private State m_state;

		private long m_totalBytes;

		private long m_downloadedBytes;

		public State CopyState => m_state;

		public string SrcPath => m_srcPath;

		public long DownloadedBytes => m_downloadedBytes;

		public FileCopier(string srcPath, string dstPath, Action<FileCopier> onProgressUpdated, bool overwriteIfExists = false, int bufferSize = 1048576)
		{
			m_srcPath = srcPath;
			m_dstPath = dstPath;
			m_onProgressUpdated = onProgressUpdated;
			m_buffer = new byte[bufferSize];
			m_overwriteIfExists = overwriteIfExists;
		}

		public void Execute()
		{
			if (m_state > State.INITIAL)
			{
				return;
			}
			m_state = State.DOWNLOADING;
			try
			{
				m_totalBytes = new FileInfo(m_srcPath).Length;
				if (File.Exists(m_dstPath))
				{
					if (!m_overwriteIfExists && m_totalBytes == new FileInfo(m_dstPath).Length)
					{
						m_downloadedBytes = m_totalBytes;
						m_state = State.COMPLETED;
						m_onProgressUpdated?.Invoke(this);
						return;
					}
					File.Delete(m_dstPath);
				}
				using FileStream srcFileStream = new FileStream(m_srcPath, FileMode.Open, FileAccess.Read);
				using FileStream dstFileStream = new FileStream(m_dstPath, FileMode.OpenOrCreate, FileAccess.Write);
				int bytesRead;
				do
				{
					bytesRead = srcFileStream.Read(m_buffer, 0, m_buffer.Length);
					if (bytesRead > 0)
					{
						dstFileStream.Write(m_buffer, 0, bytesRead);
						m_downloadedBytes += bytesRead;
						m_onProgressUpdated?.Invoke(this);
					}
				}
				while (bytesRead > 0);
				m_state = State.COMPLETED;
				m_onProgressUpdated?.Invoke(this);
			}
			catch (Exception ex)
			{
				Log.Downloader.PrintError("EditorAssetDownloader: Failed to download file from path = " + m_srcPath + " : Error = " + ex.Message);
				m_state = State.FAILED;
				m_onProgressUpdated?.Invoke(this);
			}
		}
	}

	private DownloadProgress m_downloadProgress = new DownloadProgress();

	private bool m_initializeCalled;

	private DownloadType m_downloadType;

	private float m_secondsUntilReady;

	private string[] m_requestedTags = new string[0];

	private IAssetManifest m_assetManifest;

	private string[] m_disabledAdventuresForStreaming = new string[0];

	private List<FileCopier> m_runningCopiers = new List<FileCopier>();

	private HashSet<string> m_copyingFilePaths = new HashSet<string>();

	private float m_updateStartTime;

	public static string DownloadProgressSerializedPath => Path.Combine(PlatformFilePaths.BasePersistentDataPath, "EditorAssetDownloader_DownloadProgress.json");

	public static string ModuleProgressSerializedPath => Path.Combine(PlatformFilePaths.BasePersistentDataPath, "downloadmanager");

	public AssetDownloaderState State { get; private set; }

	public bool IsReady { get; private set; }

	public bool IsNewMobileVersionReleased { get; private set; }

	public bool ShouldNotDownloadOptionalData { get; private set; }

	public bool IsVersionChanged { get; }

	public bool IsVersionStepCompleted { get; private set; }

	public bool IsDbfReady { get; private set; }

	public bool AreStringsReady { get; private set; }

	public bool DownloadAllFinished { get; set; }

	public string VersionOverrideUrl { get; private set; }

	public string[] DisabledAdventuresForStreaming => m_disabledAdventuresForStreaming;

	public double BytesPerSecond { get; private set; }

	public int MaxDownloadSpeed { get; set; }

	public int InGameStreamingDefaultSpeed { get; set; }

	public int DownloadSpeedInGame { get; set; }

	public static DownloadMode Mode => DownloadMode.None;

	public static int BuildNumber => 0;

	public static int FakeDownloadSpeedBytesPerSecond => 0;

	public static bool FakeDownloadSkipInitialBaseDownload => false;

	public event Action VersioningStarted;

	public event Action<int> ApkDownloadProgress;

	public event Action<int> DbfDownloadProgress;

	public bool Initialize()
	{
		VersionOverrideUrl = "Live";
		State = AssetDownloaderState.UNINITIALIZED;
		m_assetManifest = null;
		bool isFreshDownload = IsFreshDownload();
		switch (Mode)
		{
		case DownloadMode.Local:
			m_assetManifest = AssetManifest.Get();
			break;
		case DownloadMode.Build:
			DownloadAssetManifests(isFreshDownload);
			DownloadEssentialBundles(isFreshDownload);
			m_assetManifest = AssetManifest.Get();
			break;
		}
		if (m_assetManifest == null)
		{
			Log.Downloader.PrintError("EditorAssetDownloader: Failed to load asset manifest");
			StartupDialog.ShowStartupDialog(GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER"), GameStrings.Get("GLOBAL_ERROR_ASSET_MANIFEST"), GameStrings.Get("GLOBAL_QUIT"), delegate
			{
				ShutdownApplication();
			});
			return false;
		}
		m_initializeCalled = true;
		m_secondsUntilReady = 1f;
		IsNewMobileVersionReleased = false;
		IsReady = true;
		IsVersionStepCompleted = true;
		IsDbfReady = true;
		AreStringsReady = true;
		ShouldNotDownloadOptionalData = false;
		if (isFreshDownload)
		{
			ResetDownloadProgress();
		}
		DeserializeDownloadProgress();
		if (Mode == DownloadMode.Local && FakeDownloadSkipInitialBaseDownload)
		{
			foreach (AssetBundleDownloadProgress item in m_downloadProgress.AssetBundles.Where((AssetBundleDownloadProgress bundleProgress) => QueryTags(bundleProgress.Tags, new string[3]
			{
				DownloadTags.GetTagString(DownloadTags.Quality.Dbf),
				DownloadTags.GetTagString(DownloadTags.Quality.Initial),
				DownloadTags.GetTagString(DownloadTags.Content.Base)
			})))
			{
				item.BytesDownloaded = item.BytesTotal;
			}
		}
		TrySerializeDownloadProgress();
		return true;
	}

	private void ShutdownApplication()
	{
		Log.Downloader.PrintInfo("ShutdownApplication");
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

	private static bool IsFreshDownload()
	{
		DownloadProgress downloadProgress = DeserializeProgress();
		if (downloadProgress != null && !(downloadProgress.BranchName != PegasusUtils.GetBranchName()))
		{
			return downloadProgress.BuildNumber != BuildNumber;
		}
		return true;
	}

	private void DownloadAssetManifests(bool isFreshDownload)
	{
		string assetBundlesPath = GetNetworkDriveAssetBundlesPath();
		if (!Directory.Exists(assetBundlesPath))
		{
			Log.Downloader.PrintError("EditorAssetDownloader: Failed to locate network drive asset bundle path " + assetBundlesPath);
			return;
		}
		Log.Downloader.PrintInfo("EditorAssetDownloader: Download asset manifests from network drive Start.");
		string assetBundleDir = PlatformFilePaths.CreateLocalFilePath("Data/" + AssetBundleInfo.BundlePathPlatformModifier());
		if (!Directory.Exists(assetBundleDir))
		{
			Directory.CreateDirectory(assetBundleDir);
		}
		IEnumerable<string> enumerable = Directory.EnumerateFiles(assetBundlesPath, "asset_manifest*.unity3d");
		int count = 0;
		foreach (string srcPath in enumerable)
		{
			string dstPath = AssetBundleInfo.GetAssetBundlePath(Path.GetFileName(srcPath));
			try
			{
				if (File.Exists(dstPath) && (isFreshDownload || new FileInfo(srcPath).Length != new FileInfo(dstPath).Length))
				{
					File.Delete(dstPath);
				}
				if (!File.Exists(dstPath))
				{
					File.Copy(srcPath, dstPath);
				}
			}
			catch (Exception arg)
			{
				Log.Downloader.PrintError($"EditorAssetDownloader: Failed to copy asset manifest file from {srcPath} to {dstPath}: {arg}");
			}
			count++;
		}
		Log.Downloader.PrintInfo("EditorAssetDownloader: Download asset manifests from network drive Finish.");
	}

	public static void DownloadEssentialBundles(bool isFreshDownload)
	{
		Log.Downloader.PrintInfo("Starting essential bundle download...");
		string assetBundlesPath = GetNetworkDriveAssetBundlesPath();
		if (!string.IsNullOrEmpty(assetBundlesPath))
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(assetBundlesPath);
			string assetBundleDir = PlatformFilePaths.CreateLocalFilePath("Data/" + AssetBundleInfo.BundlePathPlatformModifier());
			if (!Directory.Exists(assetBundleDir))
			{
				Directory.CreateDirectory(assetBundleDir);
			}
			IEnumerable<FileInfo> enumerable = from path in directoryInfo.GetFiles()
				where Path.GetFileName(path.FullName).StartsWith("essential")
				select path;
			int count = 0;
			foreach (FileInfo fileInfo in enumerable)
			{
				Log.Downloader.PrintInfo("Starting copy of essential bundle: " + fileInfo.Name);
				string dest = AssetBundleInfo.GetAssetBundlePath(fileInfo.Name);
				if (File.Exists(dest) && !isFreshDownload && fileInfo.Length == new FileInfo(dest).Length)
				{
					Log.Downloader.PrintInfo("Skipping " + fileInfo.Name + " already present on disk");
					continue;
				}
				fileInfo.CopyTo(dest, overwrite: true);
				Log.Downloader.PrintInfo("Copied essential bundle: " + fileInfo.Name);
				count++;
			}
		}
		Log.Downloader.PrintInfo("Finished essential bundle download...");
	}

	public void Update(bool firstCall)
	{
		if (!m_initializeCalled)
		{
			return;
		}
		switch (State)
		{
		case AssetDownloaderState.UNINITIALIZED:
			m_secondsUntilReady -= Time.deltaTime;
			if (m_secondsUntilReady <= 0f)
			{
				m_secondsUntilReady = 0f;
				State = AssetDownloaderState.IDLE;
			}
			break;
		case AssetDownloaderState.IDLE:
			if (m_requestedTags != null && m_requestedTags.Length != 0)
			{
				State = AssetDownloaderState.DOWNLOADING;
			}
			break;
		case AssetDownloaderState.DOWNLOADING:
			if (m_requestedTags == null || m_requestedTags.Length == 0)
			{
				State = AssetDownloaderState.IDLE;
				break;
			}
			switch (Mode)
			{
			case DownloadMode.Local:
				UpdateLocal();
				break;
			case DownloadMode.Build:
				UpdateBuild();
				break;
			}
			break;
		}
		TrySerializeDownloadProgress();
	}

	private void UpdateLocal()
	{
		BytesPerSecond = 0.0;
		BytesPerSecond = FakeDownloadSpeedBytesPerSecond;
		AssetBundleDownloadProgress[] allProgress = GetAllProgressForRequestedTags();
		long allByteDownloaded = 0L;
		long allBytesTotal = 0L;
		long downloadedBytes = (long)((float)FakeDownloadSpeedBytesPerSecond * Time.deltaTime);
		AssetBundleDownloadProgress[] array = allProgress;
		foreach (AssetBundleDownloadProgress progress in array)
		{
			if (progress.BytesDownloaded < progress.BytesTotal && downloadedBytes > 0)
			{
				long amount = Math.Min(progress.BytesTotal - progress.BytesDownloaded, downloadedBytes);
				progress.BytesDownloaded += amount;
				downloadedBytes -= amount;
			}
			allByteDownloaded += progress.BytesDownloaded;
			allBytesTotal += progress.BytesTotal;
		}
		if (m_downloadType != DownloadType.NONE && allProgress.Length == 0)
		{
			if (m_downloadType == DownloadType.OPTIONAL_DOWNLOAD)
			{
				DownloadAllFinished = true;
			}
			string timeTaken = GetTimeSpentFormatted(Time.realtimeSinceStartup - m_updateStartTime);
			Log.Downloader.PrintInfo($"EditorAssetDownloader : Fake download for download type : {m_downloadType} took : {timeTaken}");
			m_downloadType = DownloadType.NONE;
		}
	}

	private void UpdateBuild()
	{
		for (int i = m_runningCopiers.Count - 1; i >= 0; i--)
		{
			if (m_runningCopiers[i].CopyState == FileCopier.State.COMPLETED || m_runningCopiers[i].CopyState == FileCopier.State.FAILED)
			{
				m_copyingFilePaths.Remove(m_runningCopiers[i].SrcPath);
				m_runningCopiers.RemoveAt(i);
			}
		}
		if (5 <= m_runningCopiers.Count)
		{
			return;
		}
		AssetBundleDownloadProgress[] allProgressForRequestedTags = GetAllProgressForRequestedTags();
		foreach (AssetBundleDownloadProgress progress in allProgressForRequestedTags)
		{
			if (progress.BytesDownloaded < progress.BytesTotal && !m_copyingFilePaths.Contains(progress.SrcPath))
			{
				FileCopier newCopier = new FileCopier(progress.SrcPath, progress.Path, delegate(FileCopier copier)
				{
					progress.BytesDownloaded = copier.DownloadedBytes;
				});
				m_runningCopiers.Add(newCopier);
				m_copyingFilePaths.Add(newCopier.SrcPath);
				UniTask.RunOnThreadPool(newCopier.Execute);
				if (5 <= m_runningCopiers.Count)
				{
					return;
				}
			}
		}
		if (m_downloadType != DownloadType.NONE && m_runningCopiers.Count == 0)
		{
			if (m_downloadType == DownloadType.OPTIONAL_DOWNLOAD)
			{
				DownloadAllFinished = true;
			}
			string timeTaken = GetTimeSpentFormatted(Time.realtimeSinceStartup - m_updateStartTime);
			Log.Downloader.PrintInfo($"EditorAssetDownloader : Copying assets from network drive for download type : {m_downloadType} took : {timeTaken}");
			m_downloadType = DownloadType.NONE;
		}
	}

	private string GetTimeSpentFormatted(float duration)
	{
		return TimeSpan.FromSeconds(duration).ToString("hh'h:'mm'm:'ss's'");
	}

	private List<string> FindFirstIncompleteTagSet(List<string> parentTags, List<List<string>> requestedTagGroups, int tagGroupIndex)
	{
		if (tagGroupIndex == requestedTagGroups.Count)
		{
			if (!GetDownloadStatus(parentTags.ToArray()).Complete)
			{
				return parentTags;
			}
			return null;
		}
		foreach (string tag in requestedTagGroups[tagGroupIndex])
		{
			List<string> thisTags = new List<string>(parentTags) { tag };
			List<string> incompleteSet = FindFirstIncompleteTagSet(thisTags, requestedTagGroups, tagGroupIndex + 1);
			if (incompleteSet != null)
			{
				return incompleteSet;
			}
		}
		return null;
	}

	private AssetBundleDownloadProgress[] GetAllProgressForRequestedTags()
	{
		List<List<string>> requestedTagGroups = new List<List<string>>();
		List<string> tagsInGroup = new List<string>();
		string[] tagGroups = m_assetManifest.GetTagGroups();
		foreach (string tagGroupName in tagGroups)
		{
			List<string> tags = new List<string>();
			m_assetManifest.GetTagsInTagGroup(tagGroupName, ref tagsInGroup);
			foreach (string tagGroupTag in tagsInGroup)
			{
				if (m_requestedTags.Contains(tagGroupTag))
				{
					tags.Add(tagGroupTag);
				}
			}
			if (tags.Count > 0)
			{
				requestedTagGroups.Add(tags);
			}
		}
		requestedTagGroups.Add(new List<string> { Localization.GetLocale().ToString() });
		List<string> requestedTags = FindFirstIncompleteTagSet(new List<string>(), requestedTagGroups, 0);
		if (requestedTags == null)
		{
			return new AssetBundleDownloadProgress[0];
		}
		return m_downloadProgress.AssetBundles.Where((AssetBundleDownloadProgress bundleProgress) => QueryTags(bundleProgress.Tags, requestedTags)).ToArray();
	}

	private bool QueryTags(ICollection<string> targetTags, ICollection<string> queryTags)
	{
		if (targetTags == null || targetTags.Count == 0)
		{
			return false;
		}
		HashSet<string> groups = new HashSet<string>();
		foreach (string tag in targetTags)
		{
			groups.Add(m_assetManifest.GetTagGroupForTag(tag));
		}
		foreach (string tag2 in queryTags)
		{
			string group = m_assetManifest.GetTagGroupForTag(tag2);
			if (targetTags.Contains(tag2))
			{
				groups.Remove(group);
			}
		}
		return groups.Count == 0;
	}

	private void DeserializeDownloadProgress()
	{
		_ = AssetLoaderPrefs.AssetLoadingMethod;
		_ = Mode;
		float startTime = Time.realtimeSinceStartup;
		m_downloadProgress = DeserializeProgress();
		if (m_downloadProgress == null)
		{
			m_downloadProgress = new DownloadProgress();
			m_downloadProgress.BranchName = PegasusUtils.GetBranchName();
			m_downloadProgress.BuildNumber = BuildNumber;
		}
		DownloadMode mode = Mode;
		if ((uint)(mode - 1) <= 1u)
		{
			string[] assetBundleNames = m_assetManifest.GetAllAssetBundleNames();
			m_downloadProgress.AssetBundles = m_downloadProgress.AssetBundles.Where((AssetBundleDownloadProgress a) => assetBundleNames.Contains(a.Name)).ToList();
			int count = 0;
			string[] array = assetBundleNames;
			foreach (string bundleName in array)
			{
				string bundlePath = AssetBundleInfo.GetAssetBundlePath(bundleName);
				AssetBundleDownloadProgress bundleProgress = m_downloadProgress.AssetBundles.FirstOrDefault((AssetBundleDownloadProgress a) => a.Path == bundlePath);
				if (bundleProgress == null)
				{
					bundleProgress = new AssetBundleDownloadProgress();
					bundleProgress.Name = bundleName;
					bundleProgress.Path = bundlePath;
					bundleProgress.SrcPath = ((Mode == DownloadMode.Build) ? Path.Combine(GetNetworkDriveAssetBundlesPath(), Path.GetFileName(bundlePath)) : bundlePath);
					m_downloadProgress.AssetBundles.Add(bundleProgress);
					bundleProgress.BytesTotal = m_assetManifest.GetBundleSize(bundleName);
					if (bundleName.StartsWith("essential"))
					{
						bundleProgress.BytesDownloaded = bundleProgress.BytesTotal;
					}
				}
				List<string> tagList = new List<string>();
				m_assetManifest.GetTagsFromAssetBundle(bundleProgress.Name, tagList);
				string locale = UpdateUtils.GetLocaleFromAssetBundle(bundleProgress.Name);
				if (!string.IsNullOrEmpty(locale))
				{
					tagList.Add(locale);
				}
				bundleProgress.Tags = tagList.ToArray();
				if (Mode == DownloadMode.Local)
				{
					if (bundleName.StartsWith("local"))
					{
						bundleProgress.BytesDownloaded = bundleProgress.BytesTotal;
					}
				}
				else if (bundleProgress.BytesDownloaded > 0 && !File.Exists(bundleProgress.Path))
				{
					bundleProgress.BytesDownloaded = 0L;
				}
				else
				{
					long bytesDownloaded = bundleProgress.BytesDownloaded;
					if (bytesDownloaded <= 0 && File.Exists(bundleProgress.Path))
					{
						bytesDownloaded = new FileInfo(bundleProgress.Path).Length;
					}
					bundleProgress.BytesDownloaded = Math.Min(bytesDownloaded, bundleProgress.BytesTotal);
				}
				count++;
			}
			m_downloadProgress.AssetBundles.Sort((AssetBundleDownloadProgress a, AssetBundleDownloadProgress b) => a.Name.CompareTo(b.Name));
		}
		string timeTaken = GetTimeSpentFormatted(Time.realtimeSinceStartup - startTime);
		Log.Downloader.PrintInfo("EditorAssetDownloader: DeserializeDownloadProgress took : (" + timeTaken + ")");
	}

	public static DownloadProgress DeserializeProgress()
	{
		if (File.Exists(DownloadProgressSerializedPath))
		{
			return JsonUtility.FromJson<DownloadProgress>(File.ReadAllText(DownloadProgressSerializedPath));
		}
		return null;
	}

	public static void ResetDownloadProgress()
	{
		if (File.Exists(ModuleProgressSerializedPath))
		{
			File.Delete(ModuleProgressSerializedPath);
		}
		if (File.Exists(DownloadProgressSerializedPath))
		{
			File.Delete(DownloadProgressSerializedPath);
		}
		Log.Downloader.PrintInfo("EditorAssetDownloader: ResetDownloadProgress.");
	}

	private void TrySerializeDownloadProgress()
	{
		if (State != AssetDownloaderState.UNINITIALIZED && m_downloadProgress.AssetBundles.Any((AssetBundleDownloadProgress a) => a.BytesDownloaded < a.BytesTotal) && m_requestedTags != null && m_requestedTags.Length != 0)
		{
			string json = JsonUtility.ToJson(m_downloadProgress, prettyPrint: true);
			File.WriteAllText(DownloadProgressSerializedPath, json);
		}
	}

	private static string GetNetworkDriveAssetBundlesPath()
	{
		return Path.Combine("\\\\corp.blizzard.net\\Teams\\Team5\\Builds\\Hearthstone", $"{BuildNumber}_{PegasusUtils.GetBranchName()}", GetPlatformPath());
		static string GetPlatformPath()
		{
			switch (PlatformSettings.RuntimeOS)
			{
			case OSCategory.Android:
				return "Client.Android/Data/astc";
			case OSCategory.iOS:
				return "Client.iOS/Data";
			case OSCategory.Mac:
				return "Client.OSX/Data/OSX";
			case OSCategory.PC:
				return "Client/Data/Win";
			default:
				Log.Downloader.PrintError($"EditorAssetDownloader: Unknown PlatformSettings.RuntimeOS ({PlatformSettings.RuntimeOS})");
				return "";
			}
		}
	}

	public void Shutdown()
	{
		m_requestedTags = new string[0];
		m_initializeCalled = false;
		State = AssetDownloaderState.UNINITIALIZED;
	}

	public TagDownloadStatus GetDownloadStatus(string[] tags)
	{
		TagDownloadStatus status = new TagDownloadStatus();
		if (!tags.Contains(Localization.GetLocale().ToString()))
		{
			status.Tags = new string[tags.Length + 1];
			tags.CopyTo(status.Tags, 0);
			status.Tags[tags.Length] = Localization.GetLocale().ToString();
		}
		else
		{
			status.Tags = (string[])tags.Clone();
		}
		switch (Mode)
		{
		case DownloadMode.None:
			status.BytesTotal = (status.BytesDownloaded = 1L);
			break;
		case DownloadMode.Local:
		case DownloadMode.Build:
			foreach (AssetBundleDownloadProgress bundleProgress in m_downloadProgress.AssetBundles)
			{
				if (QueryTags(bundleProgress.Tags, status.Tags))
				{
					status.BytesTotal += bundleProgress.BytesTotal;
					status.BytesDownloaded += bundleProgress.BytesDownloaded;
				}
			}
			break;
		}
		if (status.BytesTotal == 0L)
		{
			status.Complete = true;
		}
		return status;
	}

	public TagDownloadStatus GetCurrentDownloadStatus()
	{
		return GetDownloadStatus(m_requestedTags);
	}

	public void StartDownload(string[] tags, DownloadType downloadType, bool localeChanged)
	{
		m_requestedTags = tags;
		m_downloadType = downloadType;
		DownloadAllFinished = false;
		m_updateStartTime = Time.realtimeSinceStartup;
	}

	public void PauseAllDownloads()
	{
		m_requestedTags = new string[0];
	}

	public void DeleteDownloadedData()
	{
		if (Directory.Exists("Final/Data"))
		{
			Directory.Delete("Final/Data", recursive: true);
		}
	}

	public long DeleteDownloadedData(string[] tags)
	{
		long deletedDataSize = 0L;
		foreach (string bundleName in m_assetManifest.GetAssetBundleNamesForTags(tags))
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
					Log.Downloader.PrintInfo("Trying to delete bundle : '" + bundlePath + "', But bundle file not found on disk.");
				}
				m_downloadProgress.AssetBundles.FirstOrDefault((AssetBundleDownloadProgress a) => a.Path == bundlePath).BytesDownloaded = 0L;
			}
			catch (Exception ex)
			{
				Error.AddDevFatal("Failed to delete the bundle : '" + bundlePath + "' : Error = " + ex.Message);
			}
		}
		TrySerializeDownloadProgress();
		Log.Downloader.PrintInfo($"EditorAssetDownloader.DeleteDownloadedData complete. tags deleted = {string.Join(',', tags)} : deletedDataSize = {deletedDataSize}");
		return deletedDataSize;
	}

	public bool IsFileDownloaded(string filePath)
	{
		switch (Mode)
		{
		case DownloadMode.None:
			return true;
		case DownloadMode.Local:
		case DownloadMode.Build:
		{
			AssetBundleDownloadProgress progress = m_downloadProgress.AssetBundles.FirstOrDefault((AssetBundleDownloadProgress a) => a.Path == filePath);
			if (progress != null)
			{
				return progress.BytesTotal == progress.BytesDownloaded;
			}
			return false;
		}
		default:
			return false;
		}
	}

	public bool IsBundleDownloaded(string bundleName)
	{
		string filePath = AssetBundleInfo.GetAssetBundlePath(bundleName);
		return IsFileDownloaded(filePath);
	}

	public void OnSceneLoad(SceneMgr.Mode prevMode, SceneMgr.Mode nextMode, object userData)
	{
	}

	public void PrepareRestart()
	{
	}

	public void DoPostTasksAfterDownload(DownloadType downloadType)
	{
	}

	public void SendDownloadStartedTelemetryMessage(DownloadType downloadType, bool localeUpdate, DownloadTags.Content moduleTag = DownloadTags.Content.Unknown)
	{
	}

	public void SendDownloadFinishedTelemetryMessage(DownloadType downloadType, bool localeUpdate, DownloadTags.Content moduleTag = DownloadTags.Content.Unknown)
	{
	}

	public void SendDownloadStoppedTelemetryMessage(DownloadType downloadType, bool localeUpdate, bool byUser, DownloadTags.Content moduleTag = DownloadTags.Content.Unknown)
	{
	}

	public void SendDeleteModuleTelemetryMessage(DownloadTags.Content moduleTag, long deletedSize)
	{
	}

	public void SendDeleteOptionalDataTelemetryMessage(long deletedSize)
	{
	}

	public void EnterBackgroundMode()
	{
	}

	public void ExitFromBackgroundMode()
	{
	}

	public void UnknownSourcesListener(string onOff)
	{
	}

	public void InstallAPKListener(string onOff)
	{
	}

	public void AllowNotificationListener(string onOff)
	{
	}

	public bool IsCurrentVersionHigherOrEqual(string versionStr)
	{
		return false;
	}
}
