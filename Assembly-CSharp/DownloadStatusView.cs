using System;
using System.Collections;
using System.Collections.Generic;
using Hearthstone.Core.Streaming;
using Hearthstone.DataModels;
using Hearthstone.Streaming;
using Hearthstone.UI;
using UnityEngine;
using UnityEngine.Events;

public class DownloadStatusView : MonoBehaviour
{
	private static Color s_normalColor = Color.white;

	private static Color s_warningColor = Color.yellow;

	private static Color s_errorColor = Color.red;

	private static Color s_buttonTextEnabledColor = new Color(209f / (326f * (float)Math.E), 0.189987f, 0.123487f);

	private static Color s_buttonTextDisabledColor = new Color(43f / 85f, 0.5019608f, 0.5019608f);

	private static readonly Dictionary<DownloadTags.Quality, string> s_downloadStatus = new Dictionary<DownloadTags.Quality, string>
	{
		{
			DownloadTags.Quality.Fonts,
			"GLOBAL_ASSET_INTENTION_DOWNLOADING_FONTS"
		},
		{
			DownloadTags.Quality.PortHigh,
			"GLOBAL_ASSET_INTENTION_DOWNLOADING_HIGH_RES_PORTRAITS"
		},
		{
			DownloadTags.Quality.PortPremium,
			"GLOBAL_ASSET_INTENTION_DOWNLOADING_PREMIUM_ANIMATIONS"
		},
		{
			DownloadTags.Quality.SoundSpell,
			"GLOBAL_ASSET_INTENTION_DOWNLOADING_SPELL_SOUNDS"
		},
		{
			DownloadTags.Quality.SoundLegend,
			"GLOBAL_ASSET_INTENTION_DOWNLOADING_LEGEND_STINGERS"
		},
		{
			DownloadTags.Quality.MusicExpansion,
			"GLOBAL_ASSET_INTENTION_DOWNLOADING_EXPANSION_MUSIC"
		},
		{
			DownloadTags.Quality.SoundOtherMinion,
			"GLOBAL_ASSET_INTENTION_DOWNLOADING_OTHER_MINION_SOUNDS"
		},
		{
			DownloadTags.Quality.PlaySounds,
			"GLOBAL_ASSET_INTENTION_DOWNLOADING_MINION_PLAY_SOUNDS"
		},
		{
			DownloadTags.Quality.SoundMission,
			"GLOBAL_ASSET_INTENTION_DOWNLOADING_MISSION_SOUNDS"
		},
		{
			DownloadTags.Quality.HeroMusic,
			"GLOBAL_ASSET_INTENTION_DOWNLOADING_HERO_MUSIC"
		}
	};

	[SerializeField]
	private UberText m_contentDetailsText;

	[SerializeField]
	private UberText m_transferDetailsText;

	[SerializeField]
	private ProgressBar m_progressBar;

	[SerializeField]
	private UIBButton m_button;

	[SerializeField]
	private TooltipZone m_tooltipZone;

	[SerializeField]
	private float m_crossfadeSeconds = 1f;

	[SerializeField]
	private float m_secondsUntilCrossfade = 2f;

	[SerializeField]
	private bool m_shortenText;

	[SerializeField]
	private UnityEvent m_onDownloadIncomplete = new UnityEvent();

	[SerializeField]
	private UnityEvent m_onDownloadCompleted = new UnityEvent();

	private string m_remaningBytesStr = string.Empty;

	private bool m_isShowingProgressPercentage = true;

	private Coroutine m_crossfadeCoroutine;

	private bool m_cachedHasDownloadCompleted;

	private float m_originalWiggleTime;

	private IGameDownloadManager DownloadManager => GameDownloadManagerProvider.Get();

	private void Start()
	{
		if (m_button != null)
		{
			m_originalWiggleTime = m_button.m_WiggleTime;
			m_button.AddEventListener(UIEventType.ROLLOVER, OnButtonRollOver);
			m_button.AddEventListener(UIEventType.ROLLOUT, OnButtonRollOut);
		}
	}

	private void Update()
	{
		if (m_button != null)
		{
			bool buttonState = ShouldEnableButton();
			SetButtonState(buttonState);
		}
		if (DownloadManager == null)
		{
			return;
		}
		bool hasDownloadCompleted = !DownloadManager.IsInterrupted && !DownloadManager.IsAnyDownloadRequestedAndIncomplete;
		UpdateHasDownloadCompleted(hasDownloadCompleted);
		if (DownloadManager.IsInterrupted)
		{
			StartCrossfade();
			if (m_transferDetailsText != null)
			{
				switch (DownloadManager.InterruptionReason)
				{
				case InterruptionReason.Paused:
					m_transferDetailsText.TextColor = s_normalColor;
					m_transferDetailsText.Text = GameStrings.Get("GLOBAL_ASSET_DOWNLOAD_PAUSED");
					break;
				case InterruptionReason.Disabled:
					m_transferDetailsText.TextColor = s_errorColor;
					m_transferDetailsText.Text = GameStrings.Get("GLOBAL_ASSET_DOWNLOAD_ERROR_DOWNLOAD_DISABLED");
					break;
				case InterruptionReason.AgentImpeded:
					m_transferDetailsText.TextColor = s_errorColor;
					m_transferDetailsText.Text = GameStrings.Format("GLOBAL_ASSET_DOWNLOAD_ERROR_AGENT_IMPEDED", m_remaningBytesStr);
					break;
				case InterruptionReason.AwaitingWifi:
					m_transferDetailsText.TextColor = s_warningColor;
					m_transferDetailsText.Text = GameStrings.Format("GLOBAL_ASSET_DOWNLOAD_ERROR_CELLULAR_DISABLED");
					break;
				case InterruptionReason.DiskFull:
					m_transferDetailsText.TextColor = s_warningColor;
					m_transferDetailsText.Text = GameStrings.Format("GLOBAL_ASSET_DOWNLOAD_ERROR_OUT_OF_STORAGE");
					break;
				case InterruptionReason.Fetching:
					m_transferDetailsText.TextColor = s_warningColor;
					m_transferDetailsText.Text = GameStrings.Format("GLOBAL_ASSET_DOWNLOAD_AWAITING_FETCH");
					break;
				}
			}
		}
		TagDownloadStatus optionalDownloadStatus = DownloadManager.GetOptionalDownloadStatus();
		if (!HasDownloadStarted(optionalDownloadStatus))
		{
			SetStartingProgressAndText();
			return;
		}
		float currentProgress = optionalDownloadStatus.Progress;
		m_remaningBytesStr = DownloadUtils.FormatBytesAsHumanReadable(optionalDownloadStatus.BytesTotal - optionalDownloadStatus.BytesDownloaded);
		if (!DownloadManager.IsInterrupted)
		{
			if (!DownloadManager.IsAnyDownloadRequestedAndIncomplete)
			{
				StopCrossfade();
				if (m_contentDetailsText != null)
				{
					m_contentDetailsText.Text = GameStrings.Get("GLOBAL_ASSET_DOWNLOAD_COMPLETE");
					m_contentDetailsText.TextAlpha = 1f;
				}
				if (m_transferDetailsText != null)
				{
					m_transferDetailsText.TextColor = s_normalColor;
					m_transferDetailsText.Text = string.Empty;
				}
			}
			else
			{
				StartCrossfade();
				if (m_transferDetailsText != null)
				{
					string transferRate = DownloadUtils.FormatBytesAsHumanReadable((long)DownloadManager.BytesPerSecond);
					m_transferDetailsText.TextColor = s_normalColor;
					string formatString = (m_shortenText ? "GLOBAL_ASSET_DOWNLOAD_STATUS_SHORT" : "GLOBAL_ASSET_DOWNLOAD_STATUS");
					m_transferDetailsText.Text = GameStrings.Format(formatString, m_remaningBytesStr, transferRate);
				}
			}
		}
		double progress = Mathf.Clamp01(currentProgress);
		if (m_progressBar != null)
		{
			m_progressBar.SetProgressBar((float)progress);
		}
		if (m_contentDetailsText != null && DownloadManager.IsAnyDownloadRequestedAndIncomplete)
		{
			string pausedIntentionFormatString = GameStrings.Get("GLOBAL_ASSET_DOWNLOAD_INTENTION_PAUSED");
			string mainText = ((!m_isShowingProgressPercentage) ? GameStrings.Get(LocalizedDescriptionForDownloadStatus(optionalDownloadStatus)) : $"{progress * 100.0:0.}%");
			m_contentDetailsText.Text = (DownloadManager.IsInterrupted ? string.Format(pausedIntentionFormatString, mainText) : mainText);
		}
	}

	private static bool HasDownloadStarted(TagDownloadStatus baseContentStatus)
	{
		if (baseContentStatus != null)
		{
			return baseContentStatus.BytesTotal > 0;
		}
		return false;
	}

	private void SetStartingProgressAndText()
	{
		SetProgressBarToZero();
		SetStartingContentDetailsText();
	}

	private void SetStartingContentDetailsText()
	{
		if (!(m_contentDetailsText == null))
		{
			m_contentDetailsText.Text = GetStartingTextForContentDetails();
		}
	}

	private string GetStartingTextForContentDetails()
	{
		if (DownloadManager.InterruptionReason == InterruptionReason.Disabled)
		{
			return string.Empty;
		}
		return GameStrings.Format("GLOBAL_ASSET_INTENTION_UNINITIALIZED");
	}

	private void SetProgressBarToZero()
	{
		if (!(m_progressBar == null))
		{
			m_progressBar.SetProgressBar(0f);
		}
	}

	private string LocalizedDescriptionForDownloadStatus(TagDownloadStatus optionalDownloadStatus)
	{
		string[] tags = optionalDownloadStatus.Tags;
		for (int i = 0; i < tags.Length; i++)
		{
			DownloadTags.Content contentTag = DownloadTags.GetContentTag(tags[i]);
			if (contentTag == DownloadTags.Content.Unknown)
			{
				continue;
			}
			ContentDownloadStatus contentDownloadStatus = DownloadManager.GetContentDownloadStatus(contentTag);
			if (contentDownloadStatus.Progress < 1f)
			{
				if (s_downloadStatus.TryGetValue(contentDownloadStatus.InProgressQualityTag, out var key))
				{
					return key;
				}
				return "";
			}
		}
		return "";
	}

	private void OnDisable()
	{
		m_crossfadeCoroutine = null;
	}

	private void StartCrossfade()
	{
		if (m_crossfadeCoroutine == null && m_contentDetailsText != null)
		{
			m_crossfadeCoroutine = StartCoroutine(CrossfadeBetweenProgressAndContentDetailsText());
		}
	}

	private void StopCrossfade()
	{
		if (m_crossfadeCoroutine != null)
		{
			StopCoroutine(m_crossfadeCoroutine);
			m_crossfadeCoroutine = null;
		}
	}

	private IEnumerator CrossfadeBetweenProgressAndContentDetailsText()
	{
		m_contentDetailsText.TextAlpha = 0f;
		while (true)
		{
			m_isShowingProgressPercentage = !m_isShowingProgressPercentage;
			yield return LerpBetweenValues(m_crossfadeSeconds, 0f, 1f, delegate(float a)
			{
				m_contentDetailsText.TextAlpha = a;
			});
			yield return new WaitForSeconds(m_secondsUntilCrossfade);
			yield return LerpBetweenValues(m_crossfadeSeconds, 1f, 0f, delegate(float a)
			{
				m_contentDetailsText.TextAlpha = a;
			});
		}
	}

	private IEnumerator LerpBetweenValues(float duration, float from, float to, Action<float> onUpdate)
	{
		float timeLeft = duration;
		while (timeLeft >= 0f)
		{
			onUpdate(Mathf.Lerp(to, from, timeLeft / duration));
			timeLeft -= Time.deltaTime;
			yield return null;
		}
	}

	private void SetButtonState(bool state)
	{
		if (!(m_button == null) && state != m_button.IsEnabled(UIEventType.RELEASE))
		{
			m_button.SetEnabled(UIEventType.PRESS, state);
			m_button.SetEnabled(UIEventType.RELEASE, state);
			m_button.SetEnabled(UIEventType.RELEASEALL, state);
			m_button.SetEnabled(UIEventType.RIGHTCLICK, state);
			m_button.SetEnabled(UIEventType.DOUBLECLICK, state);
			m_button.SetEnabled(UIEventType.TAP, state);
			m_button.Flip(state, forceImmediate: true);
			m_button.m_WiggleTime = (state ? m_originalWiggleTime : 0f);
			m_button.m_ButtonText.TextColor = (state ? s_buttonTextEnabledColor : s_buttonTextDisabledColor);
		}
	}

	private void UpdateHasDownloadCompleted(bool hasDownloadCompleted)
	{
		if (m_cachedHasDownloadCompleted != hasDownloadCompleted)
		{
			m_cachedHasDownloadCompleted = hasDownloadCompleted;
			if (m_cachedHasDownloadCompleted)
			{
				m_onDownloadCompleted?.Invoke();
			}
			else
			{
				m_onDownloadIncomplete?.Invoke();
			}
		}
	}

	private void OnButtonRollOver(UIEvent e)
	{
		if (!(m_tooltipZone == null) && !m_button.IsEnabled(UIEventType.RELEASE))
		{
			m_tooltipZone.ShowBoxTooltip(GameStrings.Get("GLOBAL_GAME_MENU_BUTTON_DOWNLOAD_MANAGER_LOCKED_TOOLTIP_TITLE"), GameStrings.Get("GLOBAL_GAME_MENU_BUTTON_DOWNLOAD_MANAGER_LOCKED_TOOLTIP"));
			m_tooltipZone.LayerOverride = GameLayer.HighPriorityUI;
			float scale = 0.5f;
			if (GlobalDataContext.Get().GetDataModel(0, out var datamodel) && ((DeviceDataModel)datamodel).Screen < ScreenCategory.MiniTablet)
			{
				scale = 0.65f;
			}
			m_tooltipZone.Scale = scale;
		}
	}

	private void OnButtonRollOut(UIEvent e)
	{
		if (!(m_tooltipZone == null))
		{
			m_tooltipZone.HideTooltip();
		}
	}

	private bool ShouldEnableButton()
	{
		if (DownloadManager == null)
		{
			return false;
		}
		SceneMgr sceneMgr = SceneMgr.Get();
		if (sceneMgr == null || sceneMgr.GetMode() != SceneMgr.Mode.HUB || sceneMgr.IsTransitionNowOrPending())
		{
			return false;
		}
		return true;
	}
}
