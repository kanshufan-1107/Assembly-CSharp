using System;
using System.Collections;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Hearthstone.Core.Streaming;
using Hearthstone.Streaming;
using UnityEngine;

public class InitialDownloadDialog : DialogBase
{
	public class Info
	{
	}

	[SerializeField]
	private UberText m_downloadProgressText;

	[SerializeField]
	private UberText m_flavorStringText;

	[SerializeField]
	private ProgressBar m_progressBar;

	[SerializeField]
	private float m_secondsUntilFlavorCrossfade = 8f;

	[SerializeField]
	private float m_flavorCrossfadeSeconds = 1f;

	private const int NUM_LOADING_FLAVOR_STRINGS = 5;

	private JobDefinition m_dbfReloadingJob;

	private Coroutine m_flavorTextAnimationCoroutine;

	private PegUIElement m_inputBlocker;

	private IGameDownloadManager DownloadManager => GameDownloadManagerProvider.Get();

	public static bool IsVisible { get; private set; }

	public override void Show()
	{
		InitializeUI();
		base.Show();
		IsVisible = true;
		DoShowAnimation();
		DialogBase.DoBlur(isUseOrthographic: true);
		StartFlavorTextAnimation();
		CreateInputBlocker();
		ChatMgr.Get()?.CloseChatUI();
		BnetBarFriendButton.Get().SetEnabled(enabled: false);
		Log.Downloader.Log(LogLevel.Information, "Showing initial download popup.");
	}

	private void CreateInputBlocker()
	{
		GameObject inputBlocker = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(base.gameObject.layer), "InitialDownloadInputBlocker", this, null, 10f);
		inputBlocker.transform.localPosition = new Vector3(0f, 10f, inputBlocker.transform.position.z);
		inputBlocker.GetComponent<BoxCollider>().size /= 10f;
		inputBlocker.layer = base.gameObject.layer;
		m_inputBlocker = inputBlocker.AddComponent<PegUIElement>();
	}

	public override void Hide()
	{
		base.Hide();
		IsVisible = false;
		DialogBase.EndBlur();
		StopFlavorTextAnimation();
		BnetBarFriendButton.Get().SetEnabled(enabled: true);
		Log.Downloader.Log(LogLevel.Information, "Hiding initial download popup.");
	}

	protected override void OnDestroy()
	{
		IsVisible = false;
		base.OnDestroy();
	}

	private void InitializeUI()
	{
		if (m_progressBar == null)
		{
			throw new UnassignedReferenceException("m_progressBar");
		}
		if (m_downloadProgressText == null)
		{
			throw new UnassignedReferenceException("m_downloadProgressText");
		}
		if (m_flavorStringText == null)
		{
			throw new UnassignedReferenceException("m_flavorStringText");
		}
		m_progressBar.SetProgressBar(0f);
		m_downloadProgressText.Text = string.Empty;
		m_flavorStringText.Text = string.Empty;
		m_flavorStringText.TextAlpha = 0f;
	}

	private void Update()
	{
		if (DownloadManager == null || !IsVisible)
		{
			return;
		}
		ContentDownloadStatus downloadStatus = DownloadManager.GetContentDownloadStatus(DownloadTags.Content.Base);
		if (downloadStatus == null)
		{
			return;
		}
		if (DownloadManager.InterruptionReason == InterruptionReason.AgentImpeded)
		{
			m_progressBar.SetLabel(GameStrings.Get("GLUE_LOADINGSCREEN_PROGRESS_IMPEDED"));
			return;
		}
		if (DownloadManager.InterruptionReason == InterruptionReason.AwaitingWifi)
		{
			m_progressBar.SetLabel(GameStrings.Get("GLUE_LOADINGSCREEN_BUTTON_WAIT"));
			return;
		}
		if (DownloadManager.InterruptionReason == InterruptionReason.DiskFull)
		{
			m_progressBar.SetLabel(GameStrings.Get("GLUE_LOADINGSCREEN_ERROR_DISK_SPACE"));
			return;
		}
		if (DownloadManager.InterruptionReason == InterruptionReason.Error)
		{
			m_progressBar.SetLabel(GameStrings.Get("GLUE_LOADINGSCREEN_ERROR_DOWNLOADING_TITLE"));
			return;
		}
		bool isReadyToPlay = DownloadManager.IsReadyToPlay;
		bool didDownloadJustFinish = isReadyToPlay && m_dbfReloadingJob == null;
		long totalMegabytes = GetMegabytes(downloadStatus.BytesTotal);
		string megabytesSymbol = GameStrings.Get("GLOBAL_ASSET_DOWNLOAD_MEGABYTE_SYMBOL");
		if (!isReadyToPlay)
		{
			float downloadProgress = Mathf.Clamp01(downloadStatus.Progress);
			m_progressBar.SetProgressBar(downloadProgress);
			long downloadedMegabytes = GetMegabytes(downloadStatus.BytesDownloaded);
			m_downloadProgressText.Text = $"{downloadedMegabytes} / {totalMegabytes} {megabytesSymbol}";
		}
		else if (didDownloadJustFinish)
		{
			Log.Downloader.Log(LogLevel.Information, "Initial download is done.");
			m_progressBar.SetProgressBar(1f);
			m_downloadProgressText.Text = $"{totalMegabytes} / {totalMegabytes} {megabytesSymbol}";
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			Hide();
		}
	}

	private void StartFlavorTextAnimation()
	{
		if (m_flavorTextAnimationCoroutine == null && m_flavorStringText != null)
		{
			m_flavorTextAnimationCoroutine = StartCoroutine(Co_FlavorTextAnimation());
		}
	}

	private void StopFlavorTextAnimation()
	{
		if (m_flavorTextAnimationCoroutine != null)
		{
			StopCoroutine(m_flavorTextAnimationCoroutine);
			m_flavorTextAnimationCoroutine = null;
		}
	}

	private IEnumerator Co_FlavorTextAnimation()
	{
		int messageIndex = 0;
		m_flavorStringText.TextAlpha = 0f;
		while (IsVisible)
		{
			string flavorText = GameStrings.Get("GLUE_LOADINGSCREEN_PROGRESS_FLAVOR_" + messageIndex);
			m_flavorStringText.Text = flavorText;
			yield return SmoothLerpBetweenValues(m_flavorCrossfadeSeconds, 0f, 1f, delegate(float a)
			{
				m_flavorStringText.TextAlpha = a;
			});
			yield return new WaitForSeconds(m_secondsUntilFlavorCrossfade);
			yield return SmoothLerpBetweenValues(m_flavorCrossfadeSeconds, 1f, 0f, delegate(float a)
			{
				m_flavorStringText.TextAlpha = a;
			});
			int num = messageIndex + 1;
			messageIndex = num;
			if (5 <= messageIndex)
			{
				messageIndex = 0;
			}
		}
	}

	private IEnumerator SmoothLerpBetweenValues(float duration, float from, float to, Action<float> onUpdate)
	{
		float timeLeft = duration;
		while (0f <= timeLeft)
		{
			onUpdate(Mathf.Lerp(to, from, timeLeft / duration));
			timeLeft -= Time.smoothDeltaTime;
			yield return null;
		}
		onUpdate(to);
	}

	private static long GetMegabytes(long bytes)
	{
		return bytes / 1048576;
	}
}
