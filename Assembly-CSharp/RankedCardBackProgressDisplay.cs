using System;
using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class RankedCardBackProgressDisplay : MonoBehaviour
{
	[CustomEditField(Sections = "Animate In")]
	public Vector3_MobileOverride m_startScale;

	[CustomEditField(Sections = "Animate In")]
	public Vector3_MobileOverride m_punchScale;

	[CustomEditField(Sections = "Animate In")]
	public Vector3_MobileOverride m_afterPunchScale;

	[CustomEditField(Sections = "Progress Bar")]
	public float m_progressBarAnimTime = 2f;

	public PlayMakerFSM m_fsm;

	public PegUIElement m_debugClickCatcher;

	public UberText m_footerText;

	private Widget m_widget;

	private ProgressBar m_progressBar;

	private MedalInfoTranslator m_medalInfo;

	private int m_winsNeeded;

	private int m_prevWins;

	private int m_currWins;

	private Action m_closedCallback;

	private bool m_isDebugShow;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private bool IsReady
	{
		get
		{
			if (m_widget != null && m_widget.IsReady)
			{
				return !m_widget.IsChangingStates;
			}
			return false;
		}
	}

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
		Reset();
	}

	private void OnDestroy()
	{
		if (EndGameScreen.Get() != null)
		{
			EndGameScreen.Get().m_hitbox.RemoveEventListener(UIEventType.RELEASE, OnClick);
		}
	}

	public void Initialize(MedalInfoTranslator medalInfo, Action callback)
	{
		if (medalInfo != null)
		{
			m_medalInfo = medalInfo;
			m_closedCallback = callback;
			int seasonId = m_medalInfo.GetCurrentSeasonId();
			m_winsNeeded = m_medalInfo.GetSeasonCardBackMinWins();
			m_prevWins = Mathf.Min(m_medalInfo.TotalRankedWinsPrevious, m_winsNeeded);
			m_currWins = Mathf.Min(m_medalInfo.TotalRankedWins, m_winsNeeded);
			FormatType currentFormatType = Options.GetFormatType();
			bool isTooltipEnabled = false;
			bool hasEarnedCardback = m_medalInfo.HasEarnedSeasonCardBack();
			RankedPlayDataModel rankedPlayDataModel = m_medalInfo.CreateDataModel(currentFormatType, RankedMedal.DisplayMode.Default, isTooltipEnabled, hasEarnedCardback);
			m_widget.BindDataModel(rankedPlayDataModel);
			CardBackDataModel cardBackDataModel = new CardBackDataModel
			{
				CardBackId = RankMgr.Get().GetRankedCardBackIdForSeasonId(seasonId)
			};
			m_widget.BindDataModel(cardBackDataModel);
			m_widget.Hide();
		}
	}

	[ContextMenu("Reset")]
	public void Reset()
	{
		m_debugClickCatcher.gameObject.SetActive(value: false);
		m_widget.Hide();
		m_fsm.SendEvent("Reset");
	}

	public void Show()
	{
		StartCoroutine(ShowWhenReady());
	}

	private IEnumerator ShowWhenReady()
	{
		while (!IsReady)
		{
			yield return null;
		}
		if (m_isDebugShow)
		{
			PositionForDebugShow();
		}
		float prevProgress = (float)m_prevWins / (float)m_winsNeeded;
		m_progressBar = m_widget.GetComponentInChildren<ProgressBar>();
		if (m_progressBar != null)
		{
			m_progressBar.SetLabel(GameStrings.Format("GLOBAL_REWARD_PROGRESS", m_prevWins, m_winsNeeded));
			m_progressBar.SetProgressBar(prevProgress);
		}
		m_footerText.Text = GameStrings.Format("GLOBAL_REMINDER_CARDBACK_SEASON_END_DIALOG", m_medalInfo.GetSeasonCardBackWinsRemaining());
		m_widget.Show();
		AnimationUtil.ShowWithPunch(base.gameObject, m_startScale, m_punchScale, m_afterPunchScale, "OnShown", noFade: true);
		m_fsm.SendEvent("Birth");
	}

	private void OnShown()
	{
		if (EndGameScreen.Get() != null)
		{
			EndGameScreen.Get().m_hitbox.AddEventListener(UIEventType.RELEASE, OnClick);
		}
		if (m_currWins > m_prevWins)
		{
			if (m_progressBar != null)
			{
				float currProgress = (float)m_currWins / (float)m_winsNeeded;
				m_progressBar.m_increaseAnimTime = m_progressBarAnimTime;
				m_progressBar.AnimateProgress(m_progressBar.Progress, currProgress, iTween.EaseType.easeInOutQuad);
			}
			float actualAnimTime = m_progressBarAnimTime / (float)m_winsNeeded;
			StartCoroutine(WaitThenTriggerPlayMaker(actualAnimTime));
		}
	}

	private IEnumerator WaitThenTriggerPlayMaker(float delay)
	{
		yield return new WaitForSeconds(delay);
		m_progressBar.SetLabel(GameStrings.Format("GLOBAL_REWARD_PROGRESS", m_currWins, m_winsNeeded));
		if (m_currWins > m_prevWins && m_currWins >= m_winsNeeded)
		{
			m_fsm.SendEvent("StartAnim");
		}
	}

	private void OnPlayMakerFinished()
	{
		Hide();
	}

	private void OnClick(UIEvent e)
	{
		m_fsm.SendEvent("Death");
		m_widget.TriggerEvent("HIDE_FOOTER_TEXT");
	}

	private void Hide()
	{
		if (EndGameScreen.Get() != null)
		{
			EndGameScreen.Get().m_hitbox.RemoveEventListener(UIEventType.RELEASE, OnClick);
		}
		if (base.gameObject != null)
		{
			AnimationUtil.ScaleFade(base.gameObject, new Vector3(0.01f, 0.01f, 0.01f), "OnClosed");
		}
	}

	private void OnClosed()
	{
		m_closedCallback?.Invoke();
	}

	public static void DebugShowFake(MedalInfoTranslator medalInfo)
	{
		Widget widget = WidgetInstance.Create(RankMgr.RANKED_CARDBACK_PROGRESS_DISPLAY_PREFAB);
		widget.RegisterReadyListener(delegate
		{
			RankedCardBackProgressDisplay componentInChildren = widget.GetComponentInChildren<RankedCardBackProgressDisplay>();
			componentInChildren.ActivateDebugEquivalentsOfEndGameScreen();
			componentInChildren.Initialize(medalInfo, componentInChildren.OnDebugShowComplete);
			componentInChildren.Show();
		});
	}

	private void ActivateDebugEquivalentsOfEndGameScreen()
	{
		m_screenEffectsHandle.StartEffect(ScreenEffectParameters.BlurVignetteDesaturatePerspective);
		m_debugClickCatcher.gameObject.SetActive(value: true);
		m_debugClickCatcher.AddEventListener(UIEventType.RELEASE, OnClick);
		m_isDebugShow = true;
	}

	private void PositionForDebugShow()
	{
		Camera camera = CameraUtils.GetMainCamera();
		base.transform.localPosition = camera.transform.position + (camera.nearClipPlane + 0.04f * (camera.farClipPlane - camera.nearClipPlane)) * camera.transform.forward;
	}

	private void OnDebugShowComplete()
	{
		m_screenEffectsHandle.StopEffect();
		UnityEngine.Object.Destroy(base.transform.parent.gameObject);
	}
}
