using System;
using UnityEngine;

namespace Hearthstone.Startup;

public class SplashLoadingText
{
	private readonly UberText m_loadingText;

	private readonly GameObject m_loadingBar;

	private readonly ProgressBar m_progressBar;

	private StartupStage m_lastStage;

	private int m_lastProgress = -1;

	public SplashLoadingText(UberText loadingText, GameObject loadingBar, ProgressBar progressBar)
	{
		m_loadingText = loadingText;
		m_loadingBar = loadingBar;
		m_progressBar = progressBar;
	}

	public void SetStartupStage(StartupStage stage)
	{
		if (m_lastStage != stage)
		{
			m_lastStage = stage;
			m_loadingBar.SetActive(value: false);
			SetText(StartupStageStrings.GetStringForStage(stage));
		}
	}

	public void UpdateStageProgress(StartupStage stage, int progress)
	{
		progress = Math.Clamp(progress, 0, 100);
		if (m_lastStage != stage)
		{
			m_lastStage = stage;
			m_lastProgress = -1;
			string text = StartupStageStrings.GetStringForStage(stage);
			SetText(text);
		}
		if (m_lastProgress != progress)
		{
			m_loadingBar.SetActive(value: true);
			m_progressBar.SetProgressBar(Mathf.Clamp01((float)progress / 100f));
			m_progressBar.SetLabel($"{progress}%");
			m_lastProgress = progress;
		}
	}

	private void SetText(string text)
	{
		m_loadingText.gameObject.SetActive(value: true);
		m_loadingText.Text = text;
		if (m_loadingText.isHidden())
		{
			m_loadingText.Show();
		}
	}
}
