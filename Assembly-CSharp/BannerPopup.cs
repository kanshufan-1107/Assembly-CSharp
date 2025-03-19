using System.Collections;
using UnityEngine;

[CustomEditClass]
public class BannerPopup : MonoBehaviour
{
	public GameObject m_root;

	public UberText m_header;

	public UberText m_text;

	public UIBButton m_dismissButton;

	public Spell m_ShowSpell;

	public Spell m_LoopingSpell;

	public Spell m_HideSpell;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_showSound;

	private BannerManager.DelOnCloseBanner m_onCloseBannerPopup;

	private PegUIElement m_inputBlocker;

	private bool m_showSpellComplete = true;

	private bool m_onCloseCallbackCalled;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private void Awake()
	{
		base.gameObject.SetActive(value: false);
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void Start()
	{
		if (m_ShowSpell == null)
		{
			OnShowSpellFinished(null, null);
			return;
		}
		m_ShowSpell.AddFinishedCallback(OnShowSpellFinished);
		m_ShowSpell.Activate();
	}

	private void OnDestroy()
	{
		if (!m_onCloseCallbackCalled)
		{
			m_onCloseCallbackCalled = true;
			if (m_onCloseBannerPopup != null)
			{
				m_onCloseBannerPopup();
			}
		}
	}

	public void Show(string headerText, string bannerText, BannerManager.DelOnCloseBanner onCloseCallback = null)
	{
		OverlayUI.Get().AddGameObject(base.gameObject);
		if (m_header != null && headerText != null)
		{
			m_header.Text = headerText;
		}
		if (m_text != null && bannerText != null)
		{
			m_text.Text = bannerText;
		}
		m_onCloseBannerPopup = onCloseCallback;
		base.gameObject.SetActive(value: true);
		Animation anim = ((m_root == null) ? null : m_root.GetComponent<Animation>());
		if (anim != null)
		{
			anim.Play();
		}
		if (!string.IsNullOrEmpty(m_showSound))
		{
			SoundManager.Get().LoadAndPlay(m_showSound);
		}
		GameObject inputBlockerObject = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(base.gameObject.layer), "ClosedSignInputBlocker", this);
		LayerUtils.SetLayer(inputBlockerObject, base.gameObject.layer, null);
		m_inputBlocker = inputBlockerObject.AddComponent<PegUIElement>();
		iTween.ScaleFrom(base.gameObject, iTween.Hash("scale", new Vector3(0.01f, 0.01f, 0.01f), "time", 0.25f, "oncompletetarget", base.gameObject, "oncomplete", "EnableClickHandler"));
		FadeEffectsIn();
		if (m_dismissButton != null)
		{
			m_dismissButton.AddEventListener(UIEventType.RELEASE, CloseBannerPopup);
		}
		m_showSpellComplete = false;
	}

	private void FadeEffectsIn()
	{
		m_screenEffectsHandle.StartEffect(ScreenEffectParameters.BlurVignetteDesaturatePerspective);
	}

	private void FadeEffectsOut()
	{
		m_screenEffectsHandle.StopEffect();
	}

	private void CloseBannerPopup(UIEvent e)
	{
		m_inputBlocker.RemoveEventListener(UIEventType.RELEASE, CloseBannerPopup);
		Close();
	}

	public void Close()
	{
		FadeEffectsOut();
		iTween.ScaleTo(base.gameObject, iTween.Hash("scale", Vector3.zero, "time", 0.5f, "oncompletetarget", base.gameObject, "oncomplete", "DestroyBannerPopup"));
		SoundManager.Get().LoadAndPlay("new_quest_click_and_shrink.prefab:601ba6676276eab43947e38f110f7b99");
		ParticleSystem[] particles = base.gameObject.GetComponentsInChildren<ParticleSystem>();
		if (particles != null)
		{
			ParticleSystem[] array = particles;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
			}
		}
		if (m_LoopingSpell != null)
		{
			m_LoopingSpell.AddFinishedCallback(OnLoopingSpellFinished);
			m_LoopingSpell.ActivateState(SpellStateType.DEATH);
		}
		else if (m_HideSpell != null)
		{
			m_HideSpell.Activate();
		}
	}

	private void EnableClickHandler()
	{
		if (m_dismissButton == null)
		{
			m_inputBlocker.AddEventListener(UIEventType.RELEASE, CloseBannerPopup);
		}
	}

	private void DestroyBannerPopup()
	{
		m_onCloseCallbackCalled = true;
		if (m_onCloseBannerPopup != null)
		{
			m_onCloseBannerPopup();
		}
		StartCoroutine(DestroyPopupObject());
	}

	private IEnumerator DestroyPopupObject()
	{
		while (!m_showSpellComplete)
		{
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}

	private void OnShowSpellFinished(Spell spell, object userData)
	{
		m_showSpellComplete = true;
		if (m_LoopingSpell == null)
		{
			OnLoopingSpellFinished(null, null);
		}
		else
		{
			m_LoopingSpell.ActivateState(SpellStateType.ACTION);
		}
	}

	private void OnLoopingSpellFinished(Spell spell, object userData)
	{
		if (m_HideSpell != null)
		{
			m_HideSpell.Activate();
		}
	}
}
