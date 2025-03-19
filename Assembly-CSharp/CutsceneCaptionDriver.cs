using System;
using System.Collections;
using Hearthstone.UI.Core;
using UnityEngine;

public class CutsceneCaptionDriver : MonoBehaviour
{
	[Tooltip("Ubertext that will display the caption titles.")]
	[SerializeField]
	private UberText m_captionText;

	[SerializeField]
	[Tooltip("Animation curve (range 0-1 for height and time) that drives fading in text. Should start at 0 and end at 1.")]
	private AnimationCurve m_fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	[Tooltip("Time for fade in transition for captions. If 0, transition is instant rather than fade.")]
	private float m_fadeInSeconds = 0.2f;

	[SerializeField]
	[Tooltip("Animation curve (range 0-1 for height and time) that drives fading out text. Should start at 1 and end at 0.")]
	private AnimationCurve m_fadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	[SerializeField]
	[Tooltip("Time for fade out transition for captions. If 0, transition is instant rather than fade.")]
	private float m_fadeOutSeconds = 0.2f;

	private Coroutine m_captionFaderCoroutine;

	private string m_currentCaptionText = string.Empty;

	private bool m_isShowing;

	[Overridable]
	public string CaptionText
	{
		get
		{
			return m_currentCaptionText;
		}
		set
		{
			UpdateCaptionText(value);
		}
	}

	private void Awake()
	{
		if (m_captionText == null)
		{
			Log.CosmeticPreview.PrintError("CutsceneCaptionDriver is missing uber text reference - component will be disabled!");
			base.enabled = false;
		}
	}

	public void SetCaptionText(string text, bool shouldShowImmediately = false)
	{
		UpdateCaptionText(text, shouldShowImmediately);
	}

	public void ClearCaptionText(bool shouldHideImmediately = false)
	{
		UpdateCaptionText(string.Empty, shouldHideImmediately);
	}

	private void UpdateCaptionText(string newCaption, bool shouldShowImmediately = false)
	{
		if (!m_currentCaptionText.Equals(newCaption, StringComparison.OrdinalIgnoreCase))
		{
			m_currentCaptionText = newCaption;
			if (string.IsNullOrEmpty(m_currentCaptionText))
			{
				HideCaption(shouldShowImmediately);
			}
			else
			{
				ShowCaption(shouldShowImmediately);
			}
		}
	}

	private void ShowCaption(bool showImmediate = false)
	{
		bool wasShowing = m_isShowing;
		m_isShowing = true;
		if (showImmediate)
		{
			if (m_captionFaderCoroutine != null)
			{
				StopCoroutine(m_captionFaderCoroutine);
				m_captionFaderCoroutine = null;
			}
			SetCaptionAbsoluteValue(isShown: true);
		}
		else
		{
			if (m_captionFaderCoroutine != null)
			{
				StopCoroutine(m_captionFaderCoroutine);
				SetCaptionAbsoluteValue(isShown: false);
			}
			m_captionFaderCoroutine = StartCoroutine(CaptionFaderCoroutine(isRequestedToShow: true, wasShowing));
		}
	}

	private void HideCaption(bool hideImmediate = false)
	{
		bool wasShowing = m_isShowing;
		m_isShowing = false;
		if (hideImmediate)
		{
			if (m_captionFaderCoroutine != null)
			{
				StopCoroutine(m_captionFaderCoroutine);
				m_captionFaderCoroutine = null;
			}
			SetCaptionAbsoluteValue(isShown: false);
		}
		else
		{
			if (m_captionFaderCoroutine != null)
			{
				StopCoroutine(m_captionFaderCoroutine);
			}
			m_captionFaderCoroutine = StartCoroutine(CaptionFaderCoroutine(isRequestedToShow: false, wasShowing));
		}
	}

	private void SetCaptionAbsoluteValue(bool isShown)
	{
		if (m_captionFaderCoroutine != null)
		{
			StopCoroutine(m_captionFaderCoroutine);
			m_captionFaderCoroutine = null;
		}
		m_captionText.Text = (isShown ? m_currentCaptionText : string.Empty);
		m_captionText.TextAlpha = (isShown ? 1 : 0);
	}

	private IEnumerator CaptionFaderCoroutine(bool isRequestedToShow, bool wasShowing)
	{
		float duration = (isRequestedToShow ? m_fadeInSeconds : m_fadeOutSeconds);
		if (isRequestedToShow)
		{
			if (wasShowing)
			{
				yield return RunAndWaitForFade(m_fadeOutSeconds, shouldShow: false);
			}
			m_captionText.Text = m_currentCaptionText;
		}
		yield return RunAndWaitForFade(duration, isRequestedToShow);
		SetCaptionAbsoluteValue(isRequestedToShow);
		m_captionFaderCoroutine = null;
	}

	private IEnumerator RunAndWaitForFade(float duration, bool shouldShow)
	{
		if (!(duration <= 0f))
		{
			float elapsedTime = 0f;
			while (elapsedTime < duration)
			{
				float fadeFraction = elapsedTime / duration;
				fadeFraction = Mathf.Clamp01(fadeFraction);
				float newAlpha = (shouldShow ? m_fadeInCurve.Evaluate(fadeFraction) : m_fadeOutCurve.Evaluate(fadeFraction));
				m_captionText.TextAlpha = newAlpha;
				elapsedTime += Time.deltaTime;
				yield return null;
			}
		}
	}
}
