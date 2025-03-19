using System;
using System.Collections;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class SetRotationClock : MonoBehaviour
{
	public delegate void DisableTheClockCallback();

	public Texture2D m_PreviousIcon;

	public Texture2D m_PreviousIconBlur;

	public Texture2D m_NewIcon;

	public Texture2D m_NewIconBlur;

	public Renderer m_GlassPanel;

	public float m_AnimationWaitTime = 5.5f;

	public GameObject m_CenterPanel;

	public float m_CenterPanelFlipTime = 1f;

	public GameObject m_SetRotationButton;

	public GameObject m_SetRotationButtonMesh;

	public GameObject m_SetRotationIconWidget;

	public float m_SetRotationButtonDelay = 0.75f;

	public float m_SetRotationButtonWobbleTime = 0.5f;

	public float m_ButtonRotationHoldTime = 1.5f;

	public GameObject m_ButtonRiseBone;

	public GameObject m_ButtonBanner;

	public UberText m_ButtonBannerStandard;

	public UberText m_ButtonBannerClassic;

	public Color m_ButtonBannerTextColor = Color.white;

	public float m_ButtonRiseTime = 1.75f;

	public float m_BlurScreenDelay = 0.5f;

	public float m_BlurScreenTime = 1f;

	public float m_MoveButtonUpZ = -0.1f;

	public float m_MoveButtonUpZphone = -0.3f;

	public float m_MoveButtonUpTime = 1f;

	public float m_ButtonFlipTime = 0.5f;

	public float m_ButtonToTrayAnimTime = 0.5f;

	public float m_EndBlurScreenDelay = 0.5f;

	public float m_EndBlurScreenTime = 1f;

	public float m_MoveButtonToTrayDelay = 1.5f;

	public float m_TextDelayTime = 1f;

	public float m_VeteranGhostedIconDelayTime = 3f;

	public ClockOverlayText m_overlayText;

	public GameObject m_ButtonGlowPlaneYellow;

	public GameObject m_ButtonGlowPlaneGreen;

	public ParticleSystem m_ImpactParticles;

	public AnimationCurve m_ButtonGlowAnimation;

	public PegUIElement m_clickCatcher;

	public AudioSource m_TheClockAmbientSound;

	public float m_TheClockAmbientSoundVolume = 1f;

	public float m_TheClockAmbientSoundFadeInTime = 2f;

	public float m_TheClockAmbientSoundFadeOutTime = 1f;

	public AudioSource m_ClickSound;

	public AudioSource m_Stage1Sound;

	public AudioSource m_Stage1Sound_Veteran;

	public AudioSource m_Stage2Sound;

	public AudioSource m_Stage2Sound_Veteran;

	public AudioSource m_Stage3Sound;

	public AudioSource m_Stage4Sound;

	public AudioSource m_Stage5Sound;

	public AudioSource m_Stage5Sound_Veteran;

	private bool m_clickCaptured;

	private Vector3 m_buttonBannerScale;

	private AudioSource m_ambientSound;

	private const float BUTTON_MESH_Z_ROTATION_FOR_CLASSIC = 0f;

	private const float BUTTON_MESH_Z_ROTATION_FOR_STANDARD = 180f;

	private static SetRotationClock s_instance;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private void Awake()
	{
		s_instance = this;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			base.transform.position = new Vector3(-60.7f, -18.939f, -43f);
			base.transform.localScale = new Vector3(9.043651f, 9.043651f, 9.043651f);
		}
		else
		{
			base.transform.position = new Vector3(-47.234f, -18.939f, -31.837f);
			base.transform.localScale = new Vector3(6.970411f, 6.970411f, 6.970411f);
		}
		m_overlayText.HideImmediate();
		m_clickCatcher.gameObject.SetActive(value: false);
		m_clickCatcher.AddEventListener(UIEventType.RELEASE, OnClick);
		m_buttonBannerScale = m_ButtonBanner.transform.localScale;
		m_ButtonBannerStandard.TextColor = m_ButtonBannerTextColor;
		m_ButtonBannerClassic.TextColor = m_ButtonBannerTextColor;
		m_ButtonBanner.SetActive(value: false);
		m_ButtonBannerStandard.gameObject.SetActive(value: false);
		m_ButtonBannerClassic.gameObject.SetActive(value: false);
		Material sharedMaterial = m_GlassPanel.GetSharedMaterial();
		sharedMaterial.SetTexture("_BlendImage1", m_PreviousIcon);
		sharedMaterial.SetTexture("_BlendImage2", m_PreviousIconBlur);
		sharedMaterial.SetFloat("_BlendTransparency", 1f);
		sharedMaterial.SetFloat("_DistortionAmountX", 0f);
		sharedMaterial.SetFloat("_DistortionAmountY", 0f);
		sharedMaterial.SetFloat("_BlendImageSizeX", 6.5f);
		sharedMaterial.SetFloat("_BlendImageSizeY", 6.5f);
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	public static SetRotationClock Get()
	{
		return s_instance;
	}

	public void StartTheClock()
	{
		m_SetRotationButton.SetActive(value: true);
		StartCoroutine(ClockAnimation());
	}

	public void ShakeCamera()
	{
		CameraShakeMgr.Shake(Camera.main, new Vector3(0.1f, 0.1f, 0.1f), 0.4f);
	}

	public void SwapSetIcons()
	{
		Material sharedMaterial = m_GlassPanel.GetSharedMaterial();
		sharedMaterial.SetTexture("_BlendImage1", m_NewIcon);
		sharedMaterial.SetTexture("_BlendImage2", m_NewIconBlur);
	}

	public IEnumerator ClockAnimation()
	{
		bool veteranFlow = SetRotationManager.HasSeenStandardModeTutorial();
		AudioSource clickSound = null;
		if (m_ClickSound != null)
		{
			clickSound = UnityEngine.Object.Instantiate(m_ClickSound);
		}
		while (!DeckPickerTrayDisplay.Get().IsLoaded())
		{
			yield return null;
		}
		DeckPickerTrayDisplay.Get().InitSetRotationTutorial(veteranFlow);
		if (!veteranFlow && SetRotationManager.Get().IsThisYearsSetRotationEventActive())
		{
			PlayClockAnimation();
			if (m_Stage1Sound != null)
			{
				SoundManager.Get().Play(UnityEngine.Object.Instantiate(m_Stage1Sound));
			}
			if (m_TheClockAmbientSound != null)
			{
				FadeInAmbientSound();
			}
			yield return new WaitForSeconds(m_AnimationWaitTime);
			VignetteBackground(0.5f);
			m_clickCatcher.gameObject.SetActive(value: true);
			m_clickCaptured = false;
			m_overlayText.UpdateText(0);
			m_overlayText.Show();
			yield return new WaitForSeconds(m_TextDelayTime);
			while (!m_clickCaptured)
			{
				yield return null;
			}
			if (m_Stage2Sound != null)
			{
				SoundManager.Get().Play(UnityEngine.Object.Instantiate(m_Stage2Sound));
			}
			if (clickSound != null)
			{
				SoundManager.Get().Play(clickSound);
			}
			StopVignetteBackground(0.5f);
			m_clickCatcher.gameObject.SetActive(value: false);
			m_overlayText.Hide();
			yield return new WaitForSeconds(m_TextDelayTime);
		}
		else
		{
			if (m_TheClockAmbientSound != null)
			{
				FadeInAmbientSound();
			}
			yield return new WaitForSeconds(m_VeteranGhostedIconDelayTime);
			if (m_Stage2Sound_Veteran != null)
			{
				SoundManager.Get().Play(UnityEngine.Object.Instantiate(m_Stage2Sound_Veteran));
			}
		}
		FlipCenterPanelButton();
		yield return new WaitForSeconds(m_ButtonRotationHoldTime);
		RaiseButton();
		yield return new WaitForSeconds(m_BlurScreenDelay);
		BlurBackground(m_BlurScreenTime);
		yield return new WaitForSeconds(m_BlurScreenTime);
		m_clickCatcher.gameObject.SetActive(value: true);
		m_clickCaptured = false;
		m_overlayText.UpdateText(1);
		m_overlayText.Show();
		yield return new WaitForSeconds(m_TextDelayTime);
		while (!m_clickCaptured)
		{
			yield return null;
		}
		if (clickSound != null)
		{
			SoundManager.Get().Play(clickSound);
		}
		m_clickCatcher.gameObject.SetActive(value: false);
		m_overlayText.Hide();
		if (m_Stage3Sound != null)
		{
			SoundManager.Get().Play(UnityEngine.Object.Instantiate(m_Stage3Sound));
		}
		MoveButtonUp();
		yield return new WaitForSeconds(m_TextDelayTime);
		m_clickCatcher.gameObject.SetActive(value: true);
		m_clickCaptured = false;
		ShowButtonBanner();
		ShowButtonYellowGlow();
		TournamentDisplay.Get().SetRotationSlideIn();
		FadeOutAmbientSound();
		while (!m_clickCaptured)
		{
			yield return null;
		}
		if (clickSound != null)
		{
			SoundManager.Get().Play(clickSound);
		}
		m_clickCatcher.gameObject.SetActive(value: false);
		if (m_Stage4Sound != null)
		{
			SoundManager.Get().Play(UnityEngine.Object.Instantiate(m_Stage4Sound));
		}
		while (TournamentDisplay.Get().SlidingInForSetRotation)
		{
			yield return null;
		}
		if (clickSound != null)
		{
			SoundManager.Get().Play(clickSound);
		}
		m_clickCatcher.gameObject.SetActive(value: false);
		HideButtonBanner();
		StopBlurBackground(m_EndBlurScreenTime);
		StopButtonDrift();
		EndClockStartTutorial();
	}

	private void FadeInAmbientSound()
	{
		if (!(m_TheClockAmbientSound == null))
		{
			m_ambientSound = UnityEngine.Object.Instantiate(m_TheClockAmbientSound);
			SoundManager.Get().SetVolume(m_ambientSound, 0.01f);
			Action<object> clockAmbientVolUpdate = delegate(object amount)
			{
				SoundManager.Get().SetVolume(m_ambientSound, (float)amount);
			};
			iTween.ValueTo(base.gameObject, iTween.Hash("name", "TheClockAmbientSound", "from", 0.01f, "to", m_TheClockAmbientSoundVolume, "time", m_TheClockAmbientSoundFadeInTime, "easetype", iTween.EaseType.linear, "onupdate", clockAmbientVolUpdate, "onupdatetarget", base.gameObject));
			SoundManager.Get().Play(m_ambientSound);
		}
	}

	private void FadeOutAmbientSound()
	{
		if (!(m_ambientSound == null))
		{
			Action<object> clockAmbientVolUpdate = delegate(object amount)
			{
				SoundManager.Get().SetVolume(m_ambientSound, (float)amount);
			};
			iTween.ValueTo(base.gameObject, iTween.Hash("name", "TheClockAmbientSound", "from", m_TheClockAmbientSoundVolume, "to", 0f, "time", m_TheClockAmbientSoundFadeOutTime, "easetype", iTween.EaseType.linear, "onupdate", clockAmbientVolUpdate, "onupdatetarget", base.gameObject, "oncompletetarget", base.gameObject, "oncomplete", "StopAmbientSound"));
		}
	}

	private void StopAmbientSound()
	{
		if (!(m_ambientSound == null))
		{
			SoundManager.Get().Stop(m_ambientSound);
		}
	}

	private void PlayClockAnimation()
	{
		Animator anim = GetComponent<Animator>();
		if (!(anim == null))
		{
			anim.SetTrigger("StartClock");
		}
	}

	private void AnimateButtonToTournamentTray()
	{
		TournamentDisplay.Get().SetRotationSlideIn();
	}

	private void FlipCenterPanelButton()
	{
		iTween.RotateTo(m_CenterPanel, iTween.Hash("z", 180f, "time", m_CenterPanelFlipTime, "islocal", true, "easetype", iTween.EaseType.easeOutBounce));
		m_SetRotationButton.transform.localEulerAngles = new Vector3(0f, 0f, -10f);
		iTween.RotateTo(m_SetRotationButton, iTween.Hash("z", 0f, "delay", m_SetRotationButtonDelay, "time", m_SetRotationButtonWobbleTime, "islocal", true, "easetype", iTween.EaseType.easeOutBounce));
	}

	private void RaiseButton()
	{
		GetComponent<Animator>().SetTrigger("RaiseButton");
		LayerUtils.SetLayer(m_SetRotationButton, GameLayer.IgnoreFullScreenEffects);
		iTween.MoveTo(m_SetRotationButton, iTween.Hash("position", m_ButtonRiseBone.transform.position, "delay", 0f, "time", m_ButtonRiseTime, "islocal", false, "easetype", iTween.EaseType.easeInOutQuint, "oncompletetarget", base.gameObject, "oncomplete", "RaiseButtonComplete"));
	}

	private void RaiseButtonComplete()
	{
		TokyoDrift drift = m_SetRotationButton.GetComponentInChildren<TokyoDrift>();
		if (!(drift == null))
		{
			drift.enabled = true;
		}
	}

	private void StopButtonDrift()
	{
		m_ButtonBanner.SetActive(value: false);
		m_ButtonBannerStandard.gameObject.SetActive(value: false);
		m_ButtonBannerClassic.gameObject.SetActive(value: false);
		TokyoDrift drift = m_SetRotationButton.GetComponentInChildren<TokyoDrift>();
		if (!(drift == null))
		{
			drift.enabled = false;
		}
	}

	private void ShowButtonBanner()
	{
		m_ButtonBanner.SetActive(value: true);
		m_ButtonBannerStandard.gameObject.SetActive(value: true);
		m_ButtonBanner.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("scale", m_buttonBannerScale);
		args.Add("time", 0.15f);
		args.Add("easetype", iTween.EaseType.easeOutQuad);
		iTween.ScaleTo(m_ButtonBanner, args);
	}

	private void ShowButtonYellowGlow()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("islocal", true);
		args.Add("from", 0f);
		args.Add("to", 1f);
		args.Add("time", 0.3f);
		args.Add("easetype", iTween.EaseType.easeOutExpo);
		args.Add("onupdate", (Action<object>)delegate(object value)
		{
			m_ButtonGlowPlaneYellow.GetComponent<Renderer>().GetMaterial().SetFloat("_Intensity", (float)value);
		});
		args.Add("onupdatetarget", base.gameObject);
		iTween.ValueTo(base.gameObject, args);
	}

	private void CrossFadeToGreenGlow()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("islocal", true);
		args.Add("from", 1f);
		args.Add("to", 0f);
		args.Add("time", 0.3f);
		args.Add("easetype", iTween.EaseType.easeOutExpo);
		args.Add("onupdate", (Action<object>)delegate(object value)
		{
			m_ButtonGlowPlaneYellow.GetComponent<Renderer>().GetMaterial().SetFloat("_Intensity", (float)value);
		});
		args.Add("onupdatetarget", m_ButtonGlowPlaneYellow);
		iTween.ValueTo(base.gameObject, args);
		Hashtable greenArgs = iTweenManager.Get().GetTweenHashTable();
		greenArgs.Add("islocal", true);
		greenArgs.Add("from", 0f);
		greenArgs.Add("to", 1f);
		greenArgs.Add("time", 0.3f);
		greenArgs.Add("easetype", iTween.EaseType.easeOutExpo);
		greenArgs.Add("onupdate", (Action<object>)delegate(object value)
		{
			m_ButtonGlowPlaneGreen.GetComponent<Renderer>().GetMaterial().SetFloat("_Intensity", (float)value);
		});
		greenArgs.Add("onupdatetarget", base.gameObject);
		iTween.ValueTo(m_ButtonGlowPlaneGreen, greenArgs);
	}

	private void ButtonBannerCrossFadeText()
	{
		m_ButtonBannerStandard.gameObject.SetActive(value: true);
		m_ButtonBannerClassic.gameObject.SetActive(value: true);
		Color classicColor = m_ButtonBannerClassic.TextColor;
		classicColor.a = 0f;
		m_ButtonBannerClassic.TextColor = classicColor;
		iTween.FadeTo(m_ButtonBannerStandard.gameObject, 0f, m_ButtonFlipTime * 0.1f);
		iTween.FadeTo(m_ButtonBannerClassic.gameObject, 1f, m_ButtonFlipTime * 0.1f);
	}

	private void ButtonBannerPunch()
	{
		Vector3 bannerScale = m_ButtonBanner.transform.localScale;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("scale", bannerScale * 1.5f);
		args.Add("time", 0.075f);
		args.Add("delay", m_ButtonFlipTime * 0.25f);
		args.Add("easetype", iTween.EaseType.easeOutQuad);
		args.Add("onupdatetarget", base.gameObject);
		iTween.ScaleTo(m_ButtonBanner, args);
		Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
		args2.Add("scale", bannerScale);
		args2.Add("time", 0.25f);
		args2.Add("delay", m_ButtonFlipTime * 0.25f + 0.075f);
		args2.Add("easetype", iTween.EaseType.easeInOutQuad);
		args2.Add("onupdatetarget", base.gameObject);
		iTween.ScaleTo(m_ButtonBanner, args2);
	}

	private void HideButtonBanner()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("scale", Vector3.zero);
		args.Add("time", 0.25f);
		args.Add("easetype", iTween.EaseType.easeInQuad);
		args.Add("oncompletetarget", base.gameObject);
		args.Add("oncomplete", "HideButtonBannerComplete");
		iTween.ScaleTo(m_ButtonBanner, args);
	}

	private void HideButtonBannerComplete()
	{
		m_ButtonBanner.SetActive(value: false);
	}

	private void FlipButton()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("z", 0f);
		args.Add("time", m_ButtonFlipTime);
		args.Add("islocal", true);
		args.Add("easetype", iTween.EaseType.easeOutElastic);
		iTween.RotateTo(m_SetRotationButtonMesh, args);
	}

	private void MoveButtonUp()
	{
		float z = m_MoveButtonUpZ;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			z = m_MoveButtonUpZphone;
		}
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("z", z);
		args.Add("delay", 0f);
		args.Add("time", m_MoveButtonUpTime);
		args.Add("islocal", true);
		args.Add("easetype", iTween.EaseType.easeInOutQuint);
		iTween.MoveTo(m_SetRotationButton, args);
	}

	private void VignetteBackground(float time)
	{
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.VignettePerspective;
		screenEffectParameters.Vignette.Amount = 0.99f;
		screenEffectParameters.EaseType = iTween.EaseType.easeOutCubic;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	private void StopVignetteBackground(float time)
	{
		m_screenEffectsHandle.StopEffect(time, iTween.EaseType.easeInCubic);
	}

	private void BlurBackground(float time)
	{
		ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.VignettePerspective;
		screenEffectParameters.Time = time;
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	private void StopBlurBackground(float time)
	{
		m_screenEffectsHandle.StopEffect(time);
	}

	private void MoveButtonToDeckPickerTray(bool socketAsClassic)
	{
		StopButtonDrift();
		Vector3 deckPickerButtonLocation = Vector3.zero;
		Vector3 deckPickerButtonScale = Vector3.one;
		GameObject trayButtonBone = DeckPickerTrayDisplay.Get().m_TheClockButtonBone;
		if (trayButtonBone != null)
		{
			deckPickerButtonLocation = trayButtonBone.transform.position;
			deckPickerButtonScale = trayButtonBone.transform.localScale;
		}
		Vector3 midPoint = Vector3.Lerp(m_SetRotationButton.transform.position, deckPickerButtonLocation, 0.75f);
		midPoint = new Vector3(midPoint.x + 7f, midPoint.y, midPoint.z);
		Vector3[] path = new Vector3[3]
		{
			m_SetRotationButton.transform.position,
			midPoint,
			deckPickerButtonLocation
		};
		GetComponent<Animator>().SetTrigger("SocketButton");
		Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
		moveArgs.Add("path", path);
		moveArgs.Add("delay", 0f);
		moveArgs.Add("time", m_ButtonToTrayAnimTime);
		moveArgs.Add("islocal", false);
		moveArgs.Add("easetype", iTween.EaseType.easeInOutQuint);
		moveArgs.Add("oncompletetarget", base.gameObject);
		moveArgs.Add("oncomplete", "ButtonImpactAndShutdownTheClock");
		iTween.MoveTo(m_SetRotationButton, moveArgs);
		Hashtable rotArgs = iTweenManager.Get().GetTweenHashTable();
		rotArgs.Add("rotation", new Vector3(0f, 0f, socketAsClassic ? 0f : 180f));
		rotArgs.Add("time", m_ButtonToTrayAnimTime);
		rotArgs.Add("islocal", true);
		rotArgs.Add("easetype", iTween.EaseType.easeInOutQuint);
		iTween.RotateTo(m_SetRotationButtonMesh, rotArgs);
		Hashtable rotArgs2 = iTweenManager.Get().GetTweenHashTable();
		rotArgs2.Add("rotation", Vector3.zero);
		rotArgs2.Add("time", m_ButtonToTrayAnimTime);
		rotArgs2.Add("islocal", true);
		rotArgs2.Add("easetype", iTween.EaseType.easeInOutQuint);
		iTween.RotateTo(m_SetRotationButton, rotArgs2);
		Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
		scaleArgs.Add("scale", deckPickerButtonScale);
		scaleArgs.Add("delay", 0f);
		scaleArgs.Add("time", m_ButtonToTrayAnimTime);
		scaleArgs.Add("easetype", iTween.EaseType.easeInOutQuint);
		iTween.ScaleTo(m_SetRotationButton, scaleArgs);
	}

	private void ButtonImpactAndShutdownTheClock()
	{
		ShakeCamera();
		m_ImpactParticles.Play();
		StartCoroutine(FinalGlowAndDisableTheClock());
	}

	private IEnumerator FinalGlowAndDisableTheClock()
	{
		EndClockStartTutorial();
		Renderer glowRenderer = (SetRotationManager.HasSeenStandardModeTutorial() ? m_ButtonGlowPlaneYellow.GetComponent<Renderer>() : m_ButtonGlowPlaneGreen.GetComponent<Renderer>());
		Material glowMat = glowRenderer.GetMaterial();
		float animLength = m_ButtonGlowAnimation[m_ButtonGlowAnimation.length - 1].time;
		float animTime = 0f;
		while (animTime < animLength)
		{
			animTime += Time.deltaTime;
			glowMat.SetFloat("_Intensity", m_ButtonGlowAnimation.Evaluate(animTime));
			yield return null;
		}
		yield return new WaitForSeconds(3f);
		base.gameObject.SetActive(value: false);
	}

	private void DisableTheClock()
	{
		base.gameObject.SetActive(value: false);
	}

	private void EndClockStartTutorial()
	{
		DisableTheClockCallback callback = DisableTheClock;
		GetComponent<Animator>().StopPlayback();
		DeckPickerTrayDisplay.Get().StartSetRotationTutorial(callback);
	}

	private void OnClick(UIEvent e)
	{
		m_clickCaptured = true;
	}
}
