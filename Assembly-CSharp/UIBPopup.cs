using System;
using System.Collections;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class UIBPopup : MonoBehaviour
{
	public delegate void OnAnimationComplete();

	[CustomEditField(Sections = "Animation & Positioning")]
	public bool m_useStartingPositionForShow;

	[CustomEditField(Sections = "Animation & Positioning")]
	public Vector3 m_showPosition = Vector3.zero;

	[CustomEditField(Sections = "Animation & Positioning")]
	public bool m_useStartingScaleForShow;

	[CustomEditField(Sections = "Animation & Positioning")]
	public Vector3 m_showScale = Vector3.one;

	[CustomEditField(Sections = "Animation & Positioning")]
	public float m_showAnimTime = 0.5f;

	[CustomEditField(Sections = "Animation & Positioning")]
	public Vector3 m_hidePosition = new Vector3(-1000f, 0f, 0f);

	[CustomEditField(Sections = "Animation & Positioning")]
	public float m_hideAnimTime = 0.1f;

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public string m_showAnimationSound = "Expand_Up.prefab:775d97ea42498c044897f396362b9db3";

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public bool m_playShowSoundWithNoAnimation;

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public string m_hideAnimationSound = "Shrink_Down.prefab:a6d5184049ac041418cd5896e7d9a87a";

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public bool m_playHideSoundWithNoAnimation;

	[CustomEditField(Sections = "Click Blockers")]
	public BoxCollider m_animationClickBlocker;

	[CustomEditField(Sections = "Click Blockers")]
	public GameObject m_inputBlockerRenderer;

	private const string s_ShowiTweenAnimationName = "SHOW_ANIMATION";

	protected bool m_shown;

	protected CanvasScaleMode m_scaleMode = CanvasScaleMode.HEIGHT;

	protected bool m_destroyOnSceneLoad = true;

	protected bool m_useOverlayUI = true;

	protected virtual void Awake()
	{
		if (m_useStartingPositionForShow)
		{
			m_showPosition = base.transform.localPosition;
		}
		if (m_useStartingScaleForShow)
		{
			m_showScale = base.transform.localScale;
		}
		WidgetTemplate widget = GetComponent<WidgetTemplate>();
		if (widget != null)
		{
			m_useOverlayUI = false;
			m_destroyOnSceneLoad = false;
		}
		widget = widget ?? GetComponentInParent<WidgetTemplate>();
		if (widget != null)
		{
			widget.RegisterEventListener(HandleWidgetEvent);
		}
	}

	protected virtual void Start()
	{
	}

	public virtual bool IsShown()
	{
		return m_shown;
	}

	public virtual void Show()
	{
		Show(useOverlayUI: true);
	}

	public virtual void Show(bool useOverlayUI)
	{
		if (m_shown)
		{
			return;
		}
		m_useOverlayUI = useOverlayUI;
		if (m_useOverlayUI)
		{
			OverlayUI overlayUI = OverlayUI.Get();
			if (overlayUI != null)
			{
				overlayUI.AddGameObject(base.gameObject, CanvasAnchor.CENTER, scaleMode: m_scaleMode, destroyOnSceneLoad: m_destroyOnSceneLoad);
			}
		}
		m_shown = true;
		DoShowAnimation();
	}

	public virtual void Hide()
	{
		Hide(animate: false);
	}

	protected virtual void Hide(bool animate)
	{
		if (m_shown)
		{
			m_shown = false;
			DoHideAnimation(!animate, OnHidden);
		}
	}

	protected virtual void OnHidden()
	{
	}

	protected void DoShowAnimation(OnAnimationComplete animationDoneCallback = null)
	{
		DoShowAnimation(disableAnimation: false, animationDoneCallback);
	}

	protected void DoShowAnimation(bool disableAnimation, OnAnimationComplete animationDoneCallback = null)
	{
		base.transform.localPosition = m_showPosition;
		if (m_useOverlayUI)
		{
			OverlayUI overlayUI = OverlayUI.Get();
			if (overlayUI != null)
			{
				overlayUI.AddGameObject(base.gameObject, CanvasAnchor.CENTER, scaleMode: m_scaleMode, destroyOnSceneLoad: m_destroyOnSceneLoad);
			}
		}
		EnableAnimationClickBlocker(enable: true);
		if (!disableAnimation && m_showAnimTime > 0f)
		{
			base.transform.localScale = m_showScale * 0.01f;
			if (!string.IsNullOrEmpty(m_showAnimationSound))
			{
				SoundManager.Get().LoadAndPlay(m_showAnimationSound);
			}
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", m_showScale);
			scaleArgs.Add("islocal", false);
			scaleArgs.Add("time", m_showAnimTime);
			scaleArgs.Add("easetype", iTween.EaseType.easeOutBounce);
			scaleArgs.Add("name", "SHOW_ANIMATION");
			if (animationDoneCallback != null)
			{
				scaleArgs.Add("oncomplete", (Action<object>)delegate
				{
					EnableAnimationClickBlocker(enable: false);
					animationDoneCallback();
				});
			}
			iTween.StopByName(base.gameObject, "SHOW_ANIMATION");
			iTween.ScaleTo(base.gameObject, scaleArgs);
		}
		else
		{
			if (m_playShowSoundWithNoAnimation && !string.IsNullOrEmpty(m_showAnimationSound))
			{
				SoundManager.Get().LoadAndPlay(m_showAnimationSound);
			}
			base.transform.localScale = m_showScale;
			if (animationDoneCallback != null)
			{
				EnableAnimationClickBlocker(enable: false);
				animationDoneCallback();
			}
		}
		WidgetTemplate widget = GetComponentInParent<WidgetTemplate>();
		if (widget != null)
		{
			widget.TriggerEvent("POPUP_SHOWN");
		}
	}

	protected void DoHideAnimation(OnAnimationComplete animationDoneCallback = null)
	{
		DoHideAnimation(disableAnimation: false, animationDoneCallback);
	}

	protected void DoHideAnimation(bool disableAnimation, OnAnimationComplete animationDoneCallback = null)
	{
		Action setHidePosition = delegate
		{
			if (base.transform != null)
			{
				base.transform.localPosition = m_hidePosition;
				base.transform.localScale = m_showScale;
			}
		};
		if (!disableAnimation && m_hideAnimTime > 0f)
		{
			if (!string.IsNullOrEmpty(m_hideAnimationSound))
			{
				SoundManager.Get().LoadAndPlay(m_hideAnimationSound);
			}
			Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
			scaleArgs.Add("scale", m_showScale * 0.01f);
			scaleArgs.Add("islocal", true);
			scaleArgs.Add("time", m_hideAnimTime);
			scaleArgs.Add("easetype", iTween.EaseType.linear);
			scaleArgs.Add("name", "SHOW_ANIMATION");
			if (animationDoneCallback != null)
			{
				scaleArgs.Add("oncomplete", (Action<object>)delegate
				{
					setHidePosition();
					animationDoneCallback();
				});
			}
			else
			{
				scaleArgs.Add("oncomplete", (Action<object>)delegate
				{
					setHidePosition();
				});
			}
			iTween.StopByName(base.gameObject, "SHOW_ANIMATION");
			iTween.ScaleTo(base.gameObject, scaleArgs);
		}
		else
		{
			if (m_playHideSoundWithNoAnimation && !string.IsNullOrEmpty(m_hideAnimationSound))
			{
				SoundManager.Get().LoadAndPlay(m_hideAnimationSound);
			}
			setHidePosition();
			if (animationDoneCallback != null)
			{
				animationDoneCallback();
			}
		}
		WidgetTemplate widget = GetComponentInParent<WidgetTemplate>();
		if (widget != null)
		{
			widget.TriggerEvent("POPUP_HIDDEN");
		}
	}

	public void DarkenInputBlocker(GameObject inputBlockerObject, float alpha)
	{
		inputBlockerObject.AddComponent<MeshRenderer>().SetMaterial(m_inputBlockerRenderer.GetComponent<MeshRenderer>().GetMaterial());
		inputBlockerObject.AddComponent<MeshFilter>().SetMesh(m_inputBlockerRenderer.GetComponent<MeshFilter>().GetMesh());
		BoxCollider inputCollider = inputBlockerObject.GetComponent<BoxCollider>();
		TransformUtil.SetLocalScaleXY(inputBlockerObject, inputCollider.size.x, inputCollider.size.y);
		inputCollider.size = new Vector3(1f, 1f, 0f);
		TransformUtil.SetLocalEulerAngleX(inputBlockerObject, 90f);
		RenderUtils.SetAlpha(inputBlockerObject, alpha);
	}

	private void EnableAnimationClickBlocker(bool enable)
	{
		if (m_animationClickBlocker != null)
		{
			m_animationClickBlocker.gameObject.SetActive(enable);
		}
	}

	public void HandleWidgetEvent(string eventName)
	{
		if (!(eventName == "CODE_SHOW_POPUP"))
		{
			if (eventName == "CODE_HIDE_POPUP")
			{
				Hide();
			}
		}
		else
		{
			Show(useOverlayUI: false);
		}
	}
}
