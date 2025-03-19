using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

[CustomEditClass]
public class SlidingTray : MonoBehaviour
{
	public delegate void TrayToggledListener(bool shown);

	public const string SLIDING_TRAY_START_OPEN = "SLIDING_TRAY_START_OPEN";

	public const string SLIDING_TRAY_FINISH_CLOSE = "SLIDING_TRAY_FINISH_CLOSE";

	[CustomEditField(Sections = "Bones")]
	public Transform m_trayHiddenBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_trayShownBone;

	[CustomEditField(Sections = "Parameters")]
	public bool m_inactivateOnHide = true;

	[Tooltip("Useful to use (instead of 'inactivate On Hide') when the SlidingTray has Widgets on it that you want to load before it gets shown.")]
	[CustomEditField(Sections = "Parameters")]
	public bool m_invisibleOnHide;

	[CustomEditField(Sections = "Parameters")]
	public bool m_useNavigationBack;

	[CustomEditField(Sections = "Parameters")]
	public bool m_playAudioOnSlide = true;

	[CustomEditField(Sections = "Parameters")]
	public string m_SlideOnSFXAssetString = "choose_opponent_panel_slide_on.prefab:66491d3d01ed663429ab80daf6a5e880";

	[CustomEditField(Sections = "Parameters")]
	public string m_SlideOffSFXAssetString = "choose_opponent_panel_slide_off.prefab:3139d09eb94899d41b9bf612649f47bf";

	[CustomEditField(Sections = "Parameters")]
	public float m_traySlideDuration = 0.5f;

	[CustomEditField(Sections = "Parameters")]
	public bool m_animateBounce;

	[CustomEditField(Sections = "Parameters")]
	public float m_animateBlurInTime = 0.4f;

	[CustomEditField(Sections = "Parameters")]
	public float m_animateBlurOutTime = 0.2f;

	[CustomEditField(Sections = "Optional Features")]
	public PegUIElement m_offClickCatcher;

	[CustomEditField(Sections = "Optional Features")]
	public bool m_sendToForegroundForBlur;

	[CustomEditField(Sections = "Optional Features")]
	public MeshRenderer m_darkenQuad;

	[CustomEditField(Sections = "Optional Features")]
	public PegUIElement m_traySliderButton;

	[Tooltip("Objects to which this can send transition events to respond. Available events:\nSLIDING_TRAY_START_OPEN\nSLIDING_TRAY_FINISH_CLOSE")]
	[CustomEditField(Sections = "Optional Features")]
	public List<GameObject> m_widgetEventListeners = new List<GameObject>();

	private bool m_trayShown;

	private bool m_traySliderAnimating;

	private TrayToggledListener m_trayToggledListener;

	private bool m_startingPositionSet;

	private GameLayer m_hiddenLayer;

	private GameLayer m_shownLayer = GameLayer.IgnoreFullScreenEffects;

	private Color m_quadHiddenColor = Color.white;

	private Color m_quadShownColor = new Color(0.53f, 0.53f, 0.53f, 1f);

	private float m_currentQuadFade;

	private readonly Vector3 INVISIBLE_POSITION = new Vector3(0f, 0f, -500f);

	private SceneMgr.Mode m_sceneContext;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private PopupRoot m_popupRoot;

	private Coroutine m_blurCoroutine;

	[CustomEditField(Hide = true)]
	[Overridable]
	public bool PlayAudioOnSlide
	{
		get
		{
			return m_playAudioOnSlide;
		}
		set
		{
			m_playAudioOnSlide = value;
		}
	}

	public event Action OnTransitionComplete;

	private void Awake()
	{
		_ = (bool)UniversalInputManager.UsePhoneUI;
		if (m_traySliderButton != null)
		{
			m_traySliderButton.AddEventListener(UIEventType.RELEASE, OnTraySliderPressed);
		}
		if (m_offClickCatcher != null)
		{
			m_offClickCatcher.AddEventListener(UIEventType.RELEASE, OnClickCatcherPressed);
		}
		if (m_darkenQuad != null)
		{
			m_darkenQuad.gameObject.SetActive(value: false);
			m_darkenQuad.GetMaterial().color = m_quadHiddenColor;
		}
		if (m_invisibleOnHide)
		{
			base.transform.localPosition = INVISIBLE_POSITION;
		}
		if (SceneMgr.Get() != null)
		{
			m_sceneContext = SceneMgr.Get().GetMode();
		}
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
		LoanerDeckDisplay.LoanerDeckExpiredDisplayed += OnFreeDeckTrailExpired;
	}

	private void Start()
	{
		if (!m_startingPositionSet)
		{
			if (m_invisibleOnHide)
			{
				base.transform.localPosition = INVISIBLE_POSITION;
			}
			else
			{
				base.transform.localPosition = m_trayHiddenBone.localPosition;
			}
			m_trayShown = false;
			if (m_inactivateOnHide)
			{
				base.gameObject.SetActive(value: false);
			}
			m_startingPositionSet = true;
		}
	}

	private void OnDestroy()
	{
		if (m_offClickCatcher != null)
		{
			m_offClickCatcher.RemoveEventListener(UIEventType.RELEASE, OnClickCatcherPressed);
		}
		if (m_traySliderButton != null)
		{
			m_traySliderButton.RemoveEventListener(UIEventType.RELEASE, OnTraySliderPressed);
		}
		if (FullScreenFXMgr.Get() != null && m_sceneContext != SceneMgr.Mode.GAME_MODE)
		{
			m_screenEffectsHandle.StopEffect(0f);
		}
		LoanerDeckDisplay.LoanerDeckExpiredDisplayed -= OnFreeDeckTrailExpired;
	}

	[ContextMenu("Show")]
	public void ShowTray()
	{
		ToggleTraySlider(show: true);
	}

	[ContextMenu("Hide")]
	public void HideTray()
	{
		ToggleTraySlider(show: false);
	}

	public void ToggleTraySlider(bool show, Transform target = null, bool animate = true)
	{
		if (m_trayShown != show)
		{
			if (show && target != null)
			{
				m_trayShownBone = target;
			}
			m_trayShown = show;
			if (show)
			{
				DoShowLogic(animate);
			}
			else
			{
				DoHideLogic(animate);
			}
			m_startingPositionSet = true;
			if (m_trayToggledListener != null)
			{
				m_trayToggledListener(show);
			}
		}
	}

	public bool TraySliderIsAnimating()
	{
		return m_traySliderAnimating;
	}

	public bool IsAnimatingToShow()
	{
		if (m_traySliderAnimating)
		{
			return m_trayShown;
		}
		return false;
	}

	public bool IsAnimatingToHide()
	{
		if (m_traySliderAnimating)
		{
			return !m_trayShown;
		}
		return false;
	}

	public bool IsTrayInShownPosition()
	{
		return base.gameObject.transform.localPosition == m_trayShownBone.localPosition;
	}

	public bool IsShown()
	{
		return m_trayShown;
	}

	public void RegisterTrayToggleListener(TrayToggledListener listener)
	{
		m_trayToggledListener = listener;
	}

	public void UnregisterTrayToggleListener(TrayToggledListener listener)
	{
		if (m_trayToggledListener == listener)
		{
			m_trayToggledListener = null;
		}
		else
		{
			Log.All.Print("Attempting to unregister a TrayToggleListener that has not been registered!");
		}
	}

	public void SetLayers(GameLayer visible, GameLayer hidden)
	{
		m_shownLayer = visible;
		m_hiddenLayer = hidden;
	}

	private void DoShowLogic(bool animate)
	{
		if (m_useNavigationBack)
		{
			Navigation.Push(BackPressed);
		}
		base.gameObject.SetActive(value: true);
		SendWidgetEvent("SLIDING_TRAY_START_OPEN");
		if (base.gameObject.activeInHierarchy && animate)
		{
			base.transform.localPosition = m_trayHiddenBone.localPosition;
			iTween.Stop(base.gameObject);
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("position", m_trayShownBone.localPosition);
			moveArgs.Add("islocal", true);
			moveArgs.Add("time", m_traySlideDuration);
			moveArgs.Add("oncomplete", "OnTraySliderAnimFinished");
			moveArgs.Add("oncompletetarget", base.gameObject);
			moveArgs.Add("easetype", m_animateBounce ? iTween.EaseType.easeOutBounce : iTween.Defaults.easeType);
			iTween.MoveTo(base.gameObject, moveArgs);
			m_traySliderAnimating = true;
			if (m_offClickCatcher != null)
			{
				if (m_darkenQuad != null)
				{
					m_darkenQuad.gameObject.SetActive(value: true);
					iTween.Stop(m_darkenQuad.gameObject);
					Hashtable fadeArgs = iTweenManager.Get().GetTweenHashTable();
					fadeArgs.Add("from", m_currentQuadFade);
					fadeArgs.Add("to", 1f);
					fadeArgs.Add("time", m_animateBlurInTime);
					fadeArgs.Add("onupdate", "DarkenQuadFade_Update");
					fadeArgs.Add("onupdatetarget", base.gameObject);
					iTween.ValueTo(m_darkenQuad.gameObject, fadeArgs);
				}
				else
				{
					FadeEffectsIn(m_animateBlurInTime);
				}
				m_offClickCatcher.gameObject.SetActive(value: true);
			}
			if (m_playAudioOnSlide)
			{
				SoundManager.Get().LoadAndPlay(m_SlideOnSFXAssetString, base.gameObject);
			}
		}
		else
		{
			iTween.Stop(base.gameObject);
			base.gameObject.transform.localPosition = m_trayShownBone.localPosition;
			if (m_darkenQuad != null)
			{
				iTween.Stop(m_darkenQuad.gameObject);
				m_currentQuadFade = 1f;
				m_darkenQuad.GetMaterial().color = m_quadShownColor;
			}
			OnTraySliderAnimFinished();
		}
	}

	private void DoHideLogic(bool animate)
	{
		if (m_useNavigationBack)
		{
			Navigation.RemoveHandler(BackPressed);
		}
		if (this == null || base.gameObject == null || base.gameObject.transform == null || m_trayHiddenBone == null)
		{
			return;
		}
		if (base.gameObject.activeInHierarchy && animate)
		{
			iTween.Stop(base.gameObject);
			Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
			moveArgs.Add("position", m_trayHiddenBone.localPosition);
			moveArgs.Add("islocal", true);
			moveArgs.Add("oncomplete", "OnTraySliderAnimFinished");
			moveArgs.Add("oncompletetarget", base.gameObject);
			moveArgs.Add("time", m_animateBounce ? m_traySlideDuration : (m_traySlideDuration / 2f));
			moveArgs.Add("easetype", m_animateBounce ? iTween.EaseType.easeOutBounce : iTween.EaseType.linear);
			iTween.MoveTo(base.gameObject, moveArgs);
			m_traySliderAnimating = true;
			if (m_offClickCatcher != null && m_offClickCatcher.gameObject != null)
			{
				if (m_darkenQuad != null && m_darkenQuad.gameObject != null)
				{
					iTween.Stop(m_darkenQuad.gameObject);
					Hashtable fadeArgs = iTweenManager.Get().GetTweenHashTable();
					fadeArgs.Add("from", m_currentQuadFade);
					fadeArgs.Add("to", 0f);
					fadeArgs.Add("time", m_animateBlurOutTime);
					fadeArgs.Add("onupdate", "DarkenQuadFade_Update");
					fadeArgs.Add("onupdatetarget", base.gameObject);
					iTween.ValueTo(m_darkenQuad.gameObject, fadeArgs);
				}
				else
				{
					FadeEffectsOut(m_animateBlurOutTime);
				}
				m_offClickCatcher.gameObject.SetActive(value: false);
			}
			if (m_playAudioOnSlide)
			{
				SoundManager.Get()?.LoadAndPlay(m_SlideOffSFXAssetString, base.gameObject);
			}
			return;
		}
		iTween.Stop(base.gameObject);
		base.gameObject.transform.localPosition = m_trayHiddenBone.localPosition;
		if (m_darkenQuad != null && m_darkenQuad.gameObject != null)
		{
			iTween.Stop(m_darkenQuad.gameObject);
			m_currentQuadFade = 0f;
			Material darkenQuadMaterial = m_darkenQuad.GetMaterial();
			if (darkenQuadMaterial != null)
			{
				darkenQuadMaterial.color = m_quadHiddenColor;
			}
		}
		FadeEffectsOut(0f);
		OnTraySliderAnimFinished();
	}

	private bool BackPressed()
	{
		ToggleTraySlider(show: false);
		return true;
	}

	private void OnTraySliderAnimFinished()
	{
		m_traySliderAnimating = false;
		if (!m_trayShown)
		{
			if (m_inactivateOnHide)
			{
				base.gameObject.SetActive(value: false);
			}
			if (m_invisibleOnHide)
			{
				base.transform.localPosition = INVISIBLE_POSITION;
			}
			if (m_darkenQuad != null)
			{
				m_darkenQuad.gameObject.SetActive(value: false);
			}
			if (m_offClickCatcher != null)
			{
				m_offClickCatcher.gameObject.SetActive(value: false);
			}
			SendWidgetEvent("SLIDING_TRAY_FINISH_CLOSE");
		}
		if (this.OnTransitionComplete != null)
		{
			this.OnTransitionComplete();
		}
	}

	private void OnTraySliderPressed(UIEvent e)
	{
		if (!m_useNavigationBack || !m_trayShown)
		{
			ToggleTraySlider(!m_trayShown);
		}
	}

	private void OnClickCatcherPressed(UIEvent e)
	{
		ToggleTraySlider(show: false);
	}

	private void FadeEffectsIn(float time)
	{
		LayerUtils.SetLayer(base.gameObject, m_shownLayer);
		if (m_shownLayer == GameLayer.IgnoreFullScreenEffects)
		{
			LayerUtils.SetLayer(Box.Get().m_letterboxingContainer, m_shownLayer);
		}
		SceneMgr sceneMgr = ServiceManager.Get<SceneMgr>();
		if (sceneMgr != null && sceneMgr.IsTransitioning())
		{
			m_blurCoroutine = StartCoroutine(BlurScreenAfterTransition(sceneMgr, time, this));
		}
		else
		{
			DoBlur(this, m_sendToForegroundForBlur, time);
		}
	}

	private IEnumerator BlurScreenAfterTransition(SceneMgr sceneMgr, float time, SlidingTray tray)
	{
		while (sceneMgr.IsTransitioning())
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		DoBlur(tray, m_sendToForegroundForBlur, time);
	}

	private void DoBlur(SlidingTray tray, bool sendToForeground, float time = 0f)
	{
		if (sendToForeground)
		{
			UIContext.GetRoot().ShowPopup(tray.gameObject, UIContext.BlurType.Layered, UIContext.ProjectionType.Perspective);
			PopupRoot popupRoot = tray.GetComponent<PopupRoot>();
			if (popupRoot != null)
			{
				tray.m_popupRoot = popupRoot;
			}
		}
		else
		{
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = time;
			tray.m_screenEffectsHandle.StartEffect(screenEffectParameters);
		}
		if (m_blurCoroutine != null)
		{
			StopCoroutine(m_blurCoroutine);
			m_blurCoroutine = null;
		}
	}

	private void RemoveSendToForegroundRendering()
	{
		if (m_sendToForegroundForBlur)
		{
			UIContext.GetRoot().DismissPopup(base.gameObject);
		}
		OnTransitionComplete -= RemoveSendToForegroundRendering;
	}

	private void FadeEffectsOut(float time)
	{
		if (m_sendToForegroundForBlur)
		{
			bool registeredCallback = false;
			if (m_popupRoot != null)
			{
				m_popupRoot.FullscreenEffectsHandle.SetFinishedCallback(OnFadeFinished);
				registeredCallback = true;
			}
			if (m_traySliderAnimating)
			{
				OnTransitionComplete += RemoveSendToForegroundRendering;
			}
			else
			{
				RemoveSendToForegroundRendering();
			}
			if (!registeredCallback)
			{
				m_screenEffectsHandle.StopEffect(time, OnFadeFinished);
			}
		}
		else
		{
			m_screenEffectsHandle.StopEffect(time, OnFadeFinished);
		}
		if (m_blurCoroutine != null)
		{
			StopCoroutine(m_blurCoroutine);
			m_blurCoroutine = null;
		}
	}

	private void OnFadeFinished()
	{
		if (!(base.gameObject == null))
		{
			if (m_popupRoot != null)
			{
				m_popupRoot.FullscreenEffectsHandle.ClearCallbacks();
			}
			LayerUtils.SetLayer(base.gameObject, m_shownLayer);
			if (m_hiddenLayer == GameLayer.Default)
			{
				LayerUtils.SetLayer(Box.Get().m_letterboxingContainer, m_hiddenLayer);
			}
		}
	}

	private void DarkenQuadFade_Update(float fade)
	{
		m_currentQuadFade = fade;
		Color color = Color.Lerp(m_quadHiddenColor, m_quadShownColor, m_currentQuadFade);
		m_darkenQuad.GetMaterial().color = color;
	}

	private void OnFreeDeckTrailExpired()
	{
		ToggleTraySlider(show: false, null, m_traySliderAnimating);
	}

	private void SendWidgetEvent(string eventName)
	{
		foreach (GameObject asyncRef in m_widgetEventListeners)
		{
			if (!(asyncRef == null))
			{
				EventFunctions.TriggerEvent(parameters: new TriggerEventParameters(eventName, null, noDownwardPropagation: true, ignorePlaymaker: true), target: asyncRef.transform, eventName: eventName);
			}
		}
	}
}
