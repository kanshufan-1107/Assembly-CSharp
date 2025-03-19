using System.Collections;
using System.Collections.Generic;
using Hearthstone.UI.Core;
using UnityEngine;
using UnityEngine.Video;

public class FinisherVideoCaptionDriver : MonoBehaviour
{
	[Tooltip("The video player to attach to for deriving timing information.")]
	public VideoPlayer VideoPlayer;

	[Tooltip("Ubertext that will display the caption titles.")]
	public UberText CaptionTitleText;

	[Tooltip("Ubertext that will display the caption Subtitles.")]
	public UberText CaptionSubtitleText;

	[Tooltip("Animation curve (range 0-1 for height and time) that drives fading in text. Should start at 0 and end at 1.")]
	public AnimationCurve FadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[Tooltip("Time for fade in transition for captions. If 0, transition is instant rather than fade.")]
	public double FadeInSeconds = 0.2;

	[Tooltip("Animation curve (range 0-1 for height and time) that drives fading out text. Should start at 1 and end at 0.")]
	public AnimationCurve FadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	[Tooltip("Time for fade out transition for captions. If 0, transition is instant rather than fade.")]
	public double FadeOutSeconds = 0.2;

	private int m_finisherId = -1;

	private IEnumerator m_showCaptionsCoroutine;

	private double m_aboutToLoopTime = double.MaxValue;

	[Overridable]
	public int FinisherID
	{
		get
		{
			return m_finisherId;
		}
		set
		{
			if (value == m_finisherId)
			{
				return;
			}
			if (value <= 0)
			{
				if (Application.isPlaying)
				{
					Error.AddDevWarning("Finisher Video Caption Drive", "Finisher Video Caption Driver instructed to play back video when no finisher was specified.");
				}
				return;
			}
			if (GameDbf.BattlegroundsFinisher.GetRecord(value) == null)
			{
				if (Application.isPlaying)
				{
					Error.AddDevWarning("Finisher Video Caption Drive", $"Finisher Video Caption Driver instructed to play back video for non-existent finisher ID {value}.");
				}
				return;
			}
			m_finisherId = value;
			if (VideoPlayer != null && VideoPlayer.isPlaying)
			{
				ShowCaptions();
			}
		}
	}

	private void Start()
	{
		if (CaptionTitleText == null)
		{
			Error.AddDevFatal("Finisher Video Caption Driver configured without target title ubertext.");
			return;
		}
		if (CaptionSubtitleText == null)
		{
			Error.AddDevFatal("Finisher Video Caption Driver configured without target subtitle ubertext.");
			return;
		}
		if (VideoPlayer == null)
		{
			Error.AddDevFatal("Finisher Video Caption Driver configured without target video player.");
			return;
		}
		VideoPlayer.started += OnVideoStarted;
		VideoPlayer.loopPointReached += OnVideoLooped;
	}

	public void OnClose()
	{
		m_finisherId = -1;
		StopAndHideCaptions();
	}

	private void OnVideoStarted(VideoPlayer source)
	{
		ShowCaptions();
	}

	private void OnVideoLooped(VideoPlayer source)
	{
		m_aboutToLoopTime = source.time;
		StopAndHideCaptions();
		ShowCaptions();
	}

	private void ShowCaptions()
	{
		if (CaptionTitleText == null)
		{
			Error.AddDevFatal("Finisher Video Caption Driver configured without target title ubertext.");
			return;
		}
		if (CaptionSubtitleText == null)
		{
			Error.AddDevFatal("Finisher Video Caption Driver configured without target subtitle ubertext.");
			return;
		}
		if (VideoPlayer == null)
		{
			Error.AddDevFatal("Finisher Video Caption Driver configured without target video player.");
			return;
		}
		if (m_finisherId <= 0)
		{
			Error.AddDevWarning("Finisher Video Caption Drive", "Finisher Video Caption Driver instructed to play back video when no finisher was specified.");
			return;
		}
		BattlegroundsFinisherDbfRecord finisherRecord = GameDbf.BattlegroundsFinisher.GetRecord(m_finisherId);
		if (finisherRecord == null)
		{
			Error.AddDevWarning("Finisher Video Caption Drive", $"Finisher Video Caption Driver instructed to play back video for non-existent finisher ID {m_finisherId}.");
			return;
		}
		List<DetailsVideoCueDbfRecord> cues = finisherRecord.VideoCues;
		if (cues == null || cues.Count == 0)
		{
			Error.AddDevWarning("Finisher Video Caption Drive", $"Finisher Video Caption Driver instructed to play back video for finisher ID {m_finisherId} which does not have cues specified.");
			return;
		}
		if (m_showCaptionsCoroutine != null)
		{
			StopCoroutine(m_showCaptionsCoroutine);
		}
		m_showCaptionsCoroutine = ShowCaptionsCoroutine();
		StartCoroutine(m_showCaptionsCoroutine);
	}

	private void StopAndHideCaptions()
	{
		if (CaptionTitleText == null)
		{
			Error.AddDevFatal("Finisher Video Caption Driver configured without target title ubertext.");
			return;
		}
		if (CaptionSubtitleText == null)
		{
			Error.AddDevFatal("Finisher Video Caption Driver configured without target subtitle ubertext.");
			return;
		}
		if (VideoPlayer == null)
		{
			Error.AddDevFatal("Finisher Video Caption Driver configured without target video player.");
			return;
		}
		if (m_showCaptionsCoroutine != null)
		{
			StopCoroutine(m_showCaptionsCoroutine);
			m_showCaptionsCoroutine = null;
		}
		CaptionTitleText.TextAlpha = 0f;
		CaptionSubtitleText.TextAlpha = 0f;
	}

	private IEnumerator ShowCaptionsCoroutine()
	{
		BattlegroundsFinisherDbfRecord finisherRecord = GameDbf.BattlegroundsFinisher.GetRecord(m_finisherId);
		List<DetailsVideoCueDbfRecord> cues = finisherRecord.VideoCues;
		double videoPositionSeconds;
		for (videoPositionSeconds = VideoPlayer.time; videoPositionSeconds >= m_aboutToLoopTime; videoPositionSeconds = VideoPlayer.time)
		{
			yield return null;
		}
		m_aboutToLoopTime = double.MaxValue;
		for (int cueIdx = 0; cueIdx < cues.Count; cueIdx++)
		{
			DetailsVideoCueDbfRecord currentCue = cues[cueIdx];
			if (videoPositionSeconds < currentCue.StartSeconds)
			{
				yield return new WaitForSeconds((float)(currentCue.StartSeconds - videoPositionSeconds));
				videoPositionSeconds = VideoPlayer.time;
			}
			double cueFadeInEndSeconds = currentCue.StartSeconds + FadeInSeconds;
			double cueEndSeconds = VideoPlayer.length;
			if (cueIdx + 1 < cues.Count)
			{
				cueEndSeconds = cues[cueIdx + 1].StartSeconds;
			}
			double cueFadeOutStartSeconds = cueEndSeconds - FadeOutSeconds;
			if (cueFadeOutStartSeconds < cueFadeInEndSeconds)
			{
				Error.AddDevWarning("Finisher Video Caption Drive", $"Finisher Video Caption Driver for finisher ID {m_finisherId}, caption #{cueIdx} has a fade out start before a fade-in ends, which will lead to clipped fade-outs.");
			}
			CaptionTitleText.Text = currentCue.CaptionTitle;
			CaptionSubtitleText.Text = currentCue.CaptionSubtitle;
			CaptionTitleText.TextAlpha = 0f;
			CaptionSubtitleText.TextAlpha = 0f;
			while (videoPositionSeconds < cueFadeInEndSeconds && FadeInSeconds > 0.0)
			{
				float fadeInFraction = (float)((videoPositionSeconds - currentCue.StartSeconds) / FadeInSeconds);
				fadeInFraction = Mathf.Clamp01(fadeInFraction);
				float newAlpha = FadeInCurve.Evaluate(fadeInFraction);
				CaptionTitleText.TextAlpha = newAlpha;
				CaptionSubtitleText.TextAlpha = newAlpha;
				yield return null;
				videoPositionSeconds = VideoPlayer.time;
			}
			if (videoPositionSeconds < cueFadeOutStartSeconds)
			{
				CaptionTitleText.TextAlpha = 1f;
				CaptionSubtitleText.TextAlpha = 1f;
				yield return new WaitForSeconds((float)(cueFadeOutStartSeconds - videoPositionSeconds));
				videoPositionSeconds = VideoPlayer.time;
			}
			while (videoPositionSeconds < cueEndSeconds && FadeOutSeconds > 0.0)
			{
				float fadeOutFraction = (float)((videoPositionSeconds - cueFadeOutStartSeconds) / FadeOutSeconds);
				fadeOutFraction = Mathf.Clamp01(fadeOutFraction);
				float newAlpha2 = FadeOutCurve.Evaluate(fadeOutFraction);
				CaptionTitleText.TextAlpha = newAlpha2;
				CaptionSubtitleText.TextAlpha = newAlpha2;
				yield return null;
				videoPositionSeconds = VideoPlayer.time;
			}
		}
		m_showCaptionsCoroutine = null;
	}
}
