using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class DynamicVideoCaptionDriver : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The video player to attach to for deriving timing information.")]
	private VideoPlayer m_VideoPlayer;

	[SerializeField]
	[Tooltip("Ubertext that will display the caption titles.")]
	private UberText m_CaptionTitleText;

	[Tooltip("Ubertext that will display the caption Subtitles.")]
	[SerializeField]
	private UberText m_CaptionDescText;

	[SerializeField]
	[Tooltip("Animation curve (range 0-1 for height and time) that drives fading in text. Should start at 0 and end at 1.")]
	private AnimationCurve m_FadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	[Tooltip("Time for fade in transition for captions. If 0, transition is instant rather than fade.")]
	private double m_FadeInSeconds = 0.2;

	[SerializeField]
	[Tooltip("Animation curve (range 0-1 for height and time) that drives fading out text. Should start at 1 and end at 0.")]
	private AnimationCurve m_FadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	[SerializeField]
	[Tooltip("Time for fade out transition for captions. If 0, transition is instant rather than fade.")]
	private double m_FadeOutSeconds = 0.2;

	private List<VideoCaptionKey> m_captionKeys;

	private Coroutine m_showCaptionsCoroutine;

	private double m_aboutToLoopTime = double.MaxValue;

	private bool m_isInitialized;

	public List<VideoCaptionKey> VideoCaptionKeys
	{
		get
		{
			return m_captionKeys;
		}
		set
		{
			if (value == m_captionKeys)
			{
				return;
			}
			if (value == null || value.Count == 0)
			{
				m_captionKeys = null;
				HideCaptions();
				return;
			}
			m_captionKeys = value;
			if (m_VideoPlayer != null && m_VideoPlayer.isPlaying)
			{
				ShowCaptions();
			}
		}
	}

	public void StopCaptions()
	{
		HideCaptions();
	}

	private void Awake()
	{
		if (m_CaptionTitleText == null)
		{
			Error.AddDevFatal("DynamicVideoCaptionDriver configured without target title ubertext.");
			return;
		}
		if (m_CaptionDescText == null)
		{
			Error.AddDevFatal("DynamicVideoCaptionDriver configured without target subtitle ubertext.");
			return;
		}
		if (m_VideoPlayer == null)
		{
			Error.AddDevFatal("DynamicVideoCaptionDriver configured without target video player.");
			return;
		}
		m_isInitialized = true;
		HideCaptions();
		m_VideoPlayer.started += OnVideoStarted;
		m_VideoPlayer.loopPointReached += OnVideoLooped;
	}

	private void OnDestroy()
	{
		if (m_VideoPlayer != null)
		{
			m_VideoPlayer.started -= OnVideoStarted;
			m_VideoPlayer.loopPointReached -= OnVideoLooped;
		}
	}

	private void OnEnable()
	{
		if (m_isInitialized && m_captionKeys != null)
		{
			ShowCaptions();
		}
	}

	private void OnDisable()
	{
		HideCaptions();
	}

	private void OnVideoStarted(VideoPlayer source)
	{
		ShowCaptions();
	}

	private void OnVideoLooped(VideoPlayer source)
	{
		HideCaptions();
		m_aboutToLoopTime = source.time;
		ShowCaptions();
	}

	private void ShowCaptions()
	{
		if (!m_isInitialized)
		{
			Error.AddDevFatal("DynamicVideoCaptionDriver unable to show captions due to failure to initialize!");
			return;
		}
		if (m_captionKeys == null || m_captionKeys.Count == 0)
		{
			HideCaptions();
			return;
		}
		if (m_showCaptionsCoroutine != null)
		{
			StopCoroutine(m_showCaptionsCoroutine);
		}
		m_showCaptionsCoroutine = StartCoroutine(ShowCaptionsCoroutine());
	}

	private void HideCaptions()
	{
		if (m_isInitialized)
		{
			if (m_showCaptionsCoroutine != null)
			{
				StopCoroutine(m_showCaptionsCoroutine);
				m_showCaptionsCoroutine = null;
			}
			m_CaptionTitleText.Text = string.Empty;
			m_CaptionDescText.Text = string.Empty;
			m_CaptionTitleText.TextAlpha = 0f;
			m_CaptionDescText.TextAlpha = 0f;
		}
	}

	private IEnumerator ShowCaptionsCoroutine()
	{
		List<VideoCaptionKey> keys = m_captionKeys;
		double videoPositionSeconds;
		for (videoPositionSeconds = m_VideoPlayer.time; videoPositionSeconds >= m_aboutToLoopTime; videoPositionSeconds = m_VideoPlayer.time)
		{
			yield return null;
		}
		m_aboutToLoopTime = double.MaxValue;
		for (int cueIdx = 0; cueIdx < keys.Count; cueIdx++)
		{
			VideoCaptionKey currentKey = keys[cueIdx];
			if (videoPositionSeconds < (double)currentKey.TimeStampSeconds)
			{
				yield return new WaitForSeconds((float)((double)currentKey.TimeStampSeconds - videoPositionSeconds));
				videoPositionSeconds = m_VideoPlayer.time;
			}
			double cueFadeInEndSeconds = (double)currentKey.TimeStampSeconds + m_FadeInSeconds;
			double cueEndSeconds = m_VideoPlayer.length;
			if (cueIdx + 1 < keys.Count)
			{
				cueEndSeconds = keys[cueIdx + 1].TimeStampSeconds;
			}
			double cueFadeOutStartSeconds = cueEndSeconds - m_FadeOutSeconds;
			if (cueFadeOutStartSeconds < cueFadeInEndSeconds)
			{
				Error.AddDevWarning("Finisher Video Caption Drive", string.Format("{0} caption #{1} has a fade out start before a fade-in ends, which will lead to clipped fade-outs.", "DynamicVideoCaptionDriver", cueIdx));
			}
			m_CaptionTitleText.Text = currentKey.TitleLocalizedString;
			m_CaptionDescText.Text = currentKey.DescLocalizedString;
			m_CaptionTitleText.TextAlpha = 0f;
			m_CaptionDescText.TextAlpha = 0f;
			while (videoPositionSeconds < cueFadeInEndSeconds && m_FadeInSeconds > 0.0)
			{
				float fadeInFraction = (float)((videoPositionSeconds - (double)currentKey.TimeStampSeconds) / m_FadeInSeconds);
				fadeInFraction = Mathf.Clamp01(fadeInFraction);
				float newAlpha = m_FadeInCurve.Evaluate(fadeInFraction);
				m_CaptionTitleText.TextAlpha = newAlpha;
				m_CaptionDescText.TextAlpha = newAlpha;
				yield return null;
				videoPositionSeconds = m_VideoPlayer.time;
			}
			if (videoPositionSeconds < cueFadeOutStartSeconds)
			{
				m_CaptionTitleText.TextAlpha = 1f;
				m_CaptionDescText.TextAlpha = 1f;
				yield return new WaitForSeconds((float)(cueFadeOutStartSeconds - videoPositionSeconds));
				videoPositionSeconds = m_VideoPlayer.time;
			}
			while (videoPositionSeconds < cueEndSeconds && m_FadeOutSeconds > 0.0)
			{
				float fadeOutFraction = (float)((videoPositionSeconds - cueFadeOutStartSeconds) / m_FadeOutSeconds);
				fadeOutFraction = Mathf.Clamp01(fadeOutFraction);
				float newAlpha2 = m_FadeOutCurve.Evaluate(fadeOutFraction);
				m_CaptionTitleText.TextAlpha = newAlpha2;
				m_CaptionDescText.TextAlpha = newAlpha2;
				yield return null;
				videoPositionSeconds = m_VideoPlayer.time;
			}
		}
		m_showCaptionsCoroutine = null;
	}
}
