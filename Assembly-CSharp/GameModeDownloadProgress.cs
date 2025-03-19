using Hearthstone.Core.Streaming;
using Hearthstone.Streaming;
using UnityEngine;

public class GameModeDownloadProgress : MonoBehaviour
{
	[SerializeField]
	private UberText m_downloadPercentageProgressText;

	[SerializeField]
	private UberText m_downloadSizeProgressText;

	private DownloadTags.Content m_moduleTag;

	private ModuleState m_moduleDownloadState = ModuleState.Unknown;

	private long m_totalBytes;

	private string m_totalBytesLabel = "";

	private IGameDownloadManager DownloadManager => GameDownloadManagerProvider.Get();

	private void Awake()
	{
		DownloadManager.RegisterModuleInstallationStateChangeListener(OnModuleDownloadStateChange, invokeImmediately: false);
	}

	public void Init(DownloadTags.Content moduleTag)
	{
		m_moduleTag = moduleTag;
		m_moduleDownloadState = DownloadManager.GetModuleState(m_moduleTag);
		m_totalBytes = DownloadManager.GetModuleDownloadSize(m_moduleTag);
		m_totalBytesLabel = DownloadUtils.FormatBytesAsHumanReadable(m_totalBytes);
		InitTexts();
	}

	public void Update()
	{
		if (m_moduleTag != 0 && m_moduleDownloadState == ModuleState.Downloading)
		{
			DownloadUtils.CalculateModuleDownloadProgress(DownloadManager.GetModuleDownloadStatus(m_moduleTag), m_totalBytes, out var downloadedBytes, out var progress);
			m_downloadPercentageProgressText.Text = $"{progress * 100f:0.}%";
			if (DownloadManager.IsInterrupted)
			{
				m_downloadSizeProgressText.Text = GameStrings.Get("GLOBAL_ASSET_DOWNLOAD_PAUSED");
				return;
			}
			string bytesDownloaded = DownloadUtils.FormatBytesAsHumanReadable(downloadedBytes);
			m_downloadSizeProgressText.Text = bytesDownloaded + "/" + m_totalBytesLabel;
		}
	}

	private void InitTexts()
	{
		m_downloadPercentageProgressText.Text = $"{0f:0.}%";
		if (DownloadManager.IsInterrupted)
		{
			m_downloadSizeProgressText.Text = GameStrings.Get("GLOBAL_ASSET_DOWNLOAD_PAUSED");
		}
		else if (m_moduleDownloadState == ModuleState.Queued)
		{
			m_downloadSizeProgressText.Text = GameStrings.Get("GLUE_GAME_MODE_DOWNLOAD_QUEUED");
		}
		else
		{
			m_downloadSizeProgressText.Text = "";
		}
	}

	private void OnModuleDownloadStateChange(DownloadTags.Content moduleTag, ModuleState state)
	{
		if (moduleTag == m_moduleTag)
		{
			m_moduleDownloadState = state;
			if (m_moduleDownloadState == ModuleState.Queued)
			{
				m_downloadSizeProgressText.Text = GameStrings.Get("GLUE_GAME_MODE_DOWNLOAD_QUEUED");
			}
		}
	}

	private void OnDestroy()
	{
		DownloadManager.UnregisterModuleInstallationStateChangeListener(OnModuleDownloadStateChange);
	}
}
