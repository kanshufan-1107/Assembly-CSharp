using Hearthstone.Core.Streaming;
using Hearthstone.Streaming;
using UnityEngine;

public class BoxDownloadProgress : MonoBehaviour
{
	[SerializeField]
	private ProgressBar m_ProgressBar;

	[SerializeField]
	private UberText m_ProgressBarText;

	private DownloadTags.Content m_moduleTag;

	private ModuleState m_moduleDownloadState = ModuleState.Unknown;

	private long m_totalBytes;

	private string m_totalBytesLabel;

	private IGameDownloadManager DownloadManager => GameDownloadManagerProvider.Get();

	public void Init(DownloadTags.Content moduleTag)
	{
		m_moduleTag = moduleTag;
		m_moduleDownloadState = DownloadManager.GetModuleState(m_moduleTag);
		m_ProgressBarText.Text = ((m_moduleDownloadState == ModuleState.Queued) ? GameStrings.Get("GLUE_GAME_MODE_DOWNLOAD_QUEUED") : "");
		m_ProgressBar.Progress = 0f;
		m_totalBytes = DownloadManager.GetModuleDownloadSize(m_moduleTag);
		m_totalBytesLabel = DownloadUtils.FormatBytesAsHumanReadable(m_totalBytes);
	}

	public void UpdateDownloadStateChange(ModuleState moduleState)
	{
		m_moduleDownloadState = moduleState;
		if (m_moduleDownloadState == ModuleState.Queued)
		{
			m_ProgressBarText.Text = GameStrings.Get("GLUE_GAME_MODE_DOWNLOAD_QUEUED");
		}
	}

	private void Update()
	{
		if (m_moduleTag != 0 && m_moduleDownloadState == ModuleState.Downloading)
		{
			DownloadUtils.CalculateModuleDownloadProgress(DownloadManager.GetModuleDownloadStatus(m_moduleTag), m_totalBytes, out var downloadedBytes, out var progress);
			if (m_ProgressBar != null)
			{
				m_ProgressBar.SetProgressBar(progress);
			}
			if (DownloadManager.IsInterrupted)
			{
				m_ProgressBarText.Text = GameStrings.Get("GLOBAL_ASSET_DOWNLOAD_PAUSED");
				return;
			}
			m_ProgressBarText.Text = GameStrings.Format("GLUE_GAME_MODE_DOWNLOAD_DOWNLOADING", DownloadUtils.FormatBytesAsHumanReadable(downloadedBytes), m_totalBytesLabel);
		}
	}
}
