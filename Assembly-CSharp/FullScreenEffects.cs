using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class FullScreenEffects : MonoBehaviour
{
	private const int NO_WORK_FRAMES_BEFORE_DEACTIVATE = 2;

	public Texture2D m_VignettingMask;

	[Header("Cached Rendering")]
	public bool m_disablePreFXRenderFeatures;

	public ScriptableRendererFeature[] m_preFXRenderFeatures;

	private bool m_usingCachedResult;

	private int m_DeactivateFrameCount;

	private ScreenEffectParameters m_toParameters;

	private ScreenEffectParameters m_startParameters;

	private ScreenEffectParameters m_currentParameters;

	private HashSet<CustomViewPass> m_preFXPasses = new HashSet<CustomViewPass>();

	private float m_effectTime;

	private float m_effectProgress;

	private ProfilerMarker m_updateProfilerMarker;

	private bool m_invokedOnFinishCallbacks;

	private bool m_overridingBlendToColor;

	public bool UsingCachedResult
	{
		get
		{
			return m_usingCachedResult;
		}
		set
		{
			if (value != m_usingCachedResult)
			{
				m_usingCachedResult = value;
				if (m_usingCachedResult)
				{
					this.OnResultCached?.Invoke();
				}
				else
				{
					this.OnResultReleased?.Invoke();
				}
			}
		}
	}

	public FullScreenFXMgr.ScreenEffectsInstance ActiveEffectsInstance { get; private set; }

	public Camera Camera { get; private set; }

	public bool BlurEnabled
	{
		get
		{
			return (m_currentParameters.Type & ScreenEffectType.BLUR) != 0;
		}
		set
		{
			if (value)
			{
				m_currentParameters.Type |= ScreenEffectType.BLUR;
				base.enabled = true;
			}
			else
			{
				m_currentParameters.Type &= ~ScreenEffectType.BLUR;
			}
		}
	}

	public float BlurBlend
	{
		get
		{
			return m_currentParameters.Blur.Blend;
		}
		set
		{
			BlurEnabled = true;
			m_currentParameters.Blur.Blend = value;
		}
	}

	public bool VignettingEnable
	{
		get
		{
			return (m_currentParameters.Type & ScreenEffectType.VIGNETTE) != 0;
		}
		set
		{
			if (value)
			{
				m_currentParameters.Type |= ScreenEffectType.VIGNETTE;
				base.enabled = true;
			}
			else
			{
				m_currentParameters.Type &= ~ScreenEffectType.VIGNETTE;
			}
		}
	}

	public float VignettingIntensity
	{
		get
		{
			return m_currentParameters.Vignette.Amount;
		}
		set
		{
			VignettingEnable = true;
			m_currentParameters.Type |= ScreenEffectType.VIGNETTE;
			m_currentParameters.Vignette.Amount = value;
		}
	}

	public bool BlendToColorEnable
	{
		get
		{
			return (m_currentParameters.Type & ScreenEffectType.BLENDTOCOLOR) != 0;
		}
		set
		{
			if (value)
			{
				m_currentParameters.Type |= ScreenEffectType.BLENDTOCOLOR;
				base.enabled = true;
			}
			else
			{
				m_currentParameters.Type &= ~ScreenEffectType.BLENDTOCOLOR;
			}
		}
	}

	public Color BlendColor
	{
		get
		{
			return m_currentParameters.BlendToColor.BlendColor;
		}
		set
		{
			BlendToColorEnable = true;
			m_currentParameters.Type |= ScreenEffectType.BLENDTOCOLOR;
			m_currentParameters.BlendToColor.BlendColor = value;
		}
	}

	public float BlendToColorAmount
	{
		get
		{
			return m_currentParameters.BlendToColor.Amount;
		}
		set
		{
			BlendToColorEnable = true;
			m_currentParameters.Type |= ScreenEffectType.BLENDTOCOLOR;
			m_currentParameters.BlendToColor.Amount = value;
		}
	}

	public bool DesaturationEnabled
	{
		get
		{
			return (m_currentParameters.Type & ScreenEffectType.DESATURATE) != 0;
		}
		set
		{
			if (value)
			{
				m_currentParameters.Type |= ScreenEffectType.DESATURATE;
				base.enabled = true;
			}
			else
			{
				m_currentParameters.Type &= ~ScreenEffectType.DESATURATE;
			}
		}
	}

	public float Desaturation
	{
		get
		{
			return m_currentParameters.Desaturate.Amount;
		}
		set
		{
			DesaturationEnabled = true;
			m_currentParameters.Type |= ScreenEffectType.DESATURATE;
			m_currentParameters.Desaturate.Amount = value;
		}
	}

	public bool IsActive
	{
		get
		{
			if (!base.gameObject.activeInHierarchy || !base.enabled)
			{
				return false;
			}
			return HasActiveEffects;
		}
	}

	public bool HasActiveEffects => m_currentParameters.Type != ScreenEffectType.NONE;

	public bool DisablePreFullscreenRenderFeatures
	{
		get
		{
			if (BlurEnabled && BlurBlend >= 1f)
			{
				return m_disablePreFXRenderFeatures;
			}
			return false;
		}
	}

	public event Action OnResultCached;

	public event Action OnResultReleased;

	protected void Awake()
	{
		Camera = GetComponent<Camera>();
		m_updateProfilerMarker = new ProfilerMarker("FullScreenEffects.DoUpdate");
		m_currentParameters = ScreenEffectParameters.None;
		if (m_preFXRenderFeatures == null || m_preFXRenderFeatures.Length == 0)
		{
			m_disablePreFXRenderFeatures = false;
		}
		RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
		ScreenResizeDetector screenResizeDetector = GetComponent<ScreenResizeDetector>();
		if (screenResizeDetector != null)
		{
			screenResizeDetector.AddSizeChangedListener(delegate
			{
				UsingCachedResult = false;
			});
		}
	}

	private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		if (!(camera == Camera))
		{
			return;
		}
		HashSet<CustomViewPass> preFXPasses = new HashSet<CustomViewPass>();
		preFXPasses.UnionWith(CustomViewPass.GetQueue(CustomViewEntryPoint.OrthographicPreFullscreenFX));
		preFXPasses.UnionWith(CustomViewPass.GetQueue(CustomViewEntryPoint.PerspectivePreFullscreenFX));
		if (!m_preFXPasses.SetEquals(preFXPasses))
		{
			UsingCachedResult = false;
			m_preFXPasses = preFXPasses;
		}
		bool preFXPassesActive = !DisablePreFullscreenRenderFeatures || !UsingCachedResult;
		if (m_preFXRenderFeatures != null)
		{
			ScriptableRendererFeature[] preFXRenderFeatures = m_preFXRenderFeatures;
			for (int i = 0; i < preFXRenderFeatures.Length; i++)
			{
				preFXRenderFeatures[i].SetActive(preFXPassesActive);
			}
		}
	}

	private void Update()
	{
		if (!IsActive)
		{
			if (m_DeactivateFrameCount > 2)
			{
				m_DeactivateFrameCount = 0;
				Disable();
			}
			else
			{
				m_DeactivateFrameCount++;
			}
		}
	}

	private void LateUpdate()
	{
		if (IsActive)
		{
			DoToUpdate();
		}
	}

	public void SetBlendToColorOverride(float amount, Color color)
	{
		m_currentParameters.Type |= ScreenEffectType.BLENDTOCOLOR;
		m_currentParameters.BlendToColor = new BlendToColorParameters(color, amount);
		m_toParameters.Type |= ScreenEffectType.BLENDTOCOLOR;
		m_toParameters.BlendToColor = m_currentParameters.BlendToColor;
		m_overridingBlendToColor = true;
		base.enabled = true;
	}

	public void DisableBlendToColorOverride()
	{
		m_currentParameters.Type &= ~ScreenEffectType.BLENDTOCOLOR;
		m_toParameters.Type &= ~ScreenEffectType.BLENDTOCOLOR;
		m_overridingBlendToColor = false;
	}

	private void SetDefaults()
	{
		m_currentParameters = ScreenEffectParameters.None;
		m_toParameters = ScreenEffectParameters.None;
		ActiveEffectsInstance = null;
	}

	public void Disable()
	{
		if (ActiveEffectsInstance != null)
		{
			FullScreenFXMgr.Get().OnFinishedEffect(ActiveEffectsInstance);
		}
		base.enabled = false;
		SetDefaults();
	}

	public void StartEffect(FullScreenFXMgr.ScreenEffectsInstance screenEffectsInstance)
	{
		ScreenEffectParameters screenEffectParameters = screenEffectsInstance.Parameters;
		m_currentParameters.Type = screenEffectParameters.Type;
		m_currentParameters.PassLocation = screenEffectParameters.PassLocation;
		m_startParameters = m_currentParameters;
		m_toParameters = screenEffectParameters;
		m_effectTime = Time.time;
		m_effectProgress = 0f;
		if (!m_invokedOnFinishCallbacks && ActiveEffectsInstance != null && ActiveEffectsInstance.OnFinishedCallback != null)
		{
			ActiveEffectsInstance.OnFinishedCallback();
		}
		ActiveEffectsInstance = screenEffectsInstance;
		m_invokedOnFinishCallbacks = false;
		base.enabled = true;
	}

	public void CleanupEffects(float time)
	{
		m_toParameters = ScreenEffectParameters.None;
		m_toParameters.Time = time;
		m_invokedOnFinishCallbacks = false;
		base.enabled = true;
	}

	private void DoToUpdate()
	{
		using (m_updateProfilerMarker.Auto())
		{
			float time = Time.time - m_effectTime;
			if (time > m_toParameters.Time && m_effectProgress >= 1f)
			{
				FinishEffect();
				return;
			}
			iTween.EasingFunction easingFunction = iTween.GetEasingFunction(m_toParameters.EaseType);
			float progress = Mathf.Clamp01(time / m_toParameters.Time);
			m_currentParameters.Blur.Blend = easingFunction(m_startParameters.Blur.Blend, m_toParameters.Blur.Blend, progress);
			m_currentParameters.Vignette.Amount = easingFunction(m_startParameters.Vignette.Amount, m_toParameters.Vignette.Amount, progress);
			m_currentParameters.Desaturate.Amount = easingFunction(m_startParameters.Desaturate.Amount, m_toParameters.Desaturate.Amount, progress);
			if (!m_overridingBlendToColor)
			{
				m_currentParameters.BlendToColor.Amount = easingFunction(m_startParameters.BlendToColor.Amount, m_toParameters.BlendToColor.Amount, progress);
			}
			m_effectProgress = progress;
		}
	}

	private void FinishEffect()
	{
		m_effectProgress = 0f;
		if (m_invokedOnFinishCallbacks)
		{
			return;
		}
		if (ActiveEffectsInstance == null)
		{
			m_currentParameters = m_toParameters;
			m_invokedOnFinishCallbacks = true;
			return;
		}
		if (ActiveEffectsInstance.OnFinishedCallback != null)
		{
			ActiveEffectsInstance.OnFinishedCallback();
		}
		if (ActiveEffectsInstance != null && ActiveEffectsInstance.Released)
		{
			m_toParameters = ScreenEffectParameters.None;
			m_currentParameters = ScreenEffectParameters.None;
			FullScreenFXMgr.Get().OnFinishedEffect(ActiveEffectsInstance);
			ActiveEffectsInstance = null;
		}
		else
		{
			m_currentParameters = m_toParameters;
		}
		m_invokedOnFinishCallbacks = true;
	}
}
