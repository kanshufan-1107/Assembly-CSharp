using UnityEngine;
using UnityEngine.Rendering;

public class FreezeFrame : MonoBehaviour
{
	private const int NO_WORK_FRAMES_BEFORE_DEACTIVATE = 2;

	public bool m_FrozenState;

	public bool m_CaptureFrozenImage;

	public int m_DeactivateFrameCount;

	public RenderTexture m_FrozenScreenTexture;

	private UniversalInputManager m_UniversalInputManager;

	private Camera m_Camera;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private void OnEnable()
	{
		RenderPipelineManager.endCameraRendering += EndCameraRendering;
	}

	protected void OnDisable()
	{
		if (m_FrozenState)
		{
			Unfreeze();
		}
		FullScreenFXMgr fXMgr = FullScreenFXMgr.Get();
		if (fXMgr != null && (bool)fXMgr.ActiveCameraFullScreenEffects)
		{
			fXMgr.ActiveCameraFullScreenEffects.Disable();
		}
		RenderPipelineManager.endCameraRendering -= EndCameraRendering;
	}

	protected void Awake()
	{
		m_Camera = GetComponent<Camera>();
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	protected void Start()
	{
		base.gameObject.GetComponent<Camera>().clearFlags = CameraClearFlags.Color;
	}

	public void Disable()
	{
		base.enabled = false;
		FullScreenFXMgr fxMgr = FullScreenFXMgr.Get();
		if (fxMgr != null)
		{
			if ((bool)fxMgr.ActiveCameraFullScreenEffects)
			{
				fxMgr.ActiveCameraFullScreenEffects.Disable();
			}
			fxMgr.ForceReset();
		}
	}

	[ContextMenu("Freeze")]
	public void Freeze()
	{
		base.enabled = true;
		if (!m_FrozenState)
		{
			FullScreenFXMgr fxMgr = FullScreenFXMgr.Get();
			if (fxMgr != null && (bool)fxMgr.ActiveCameraFullScreenEffects && !fxMgr.ActiveCameraFullScreenEffects.HasActiveEffects)
			{
				ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurDesaturatePerspective;
				screenEffectParameters.Time = 0f;
				screenEffectParameters.Blur = new BlurParameters(1.5f, 1f);
				m_screenEffectsHandle.StartEffect(screenEffectParameters);
			}
			m_CaptureFrozenImage = true;
			m_FrozenScreenTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			m_FrozenScreenTexture.filterMode = FilterMode.Point;
			m_FrozenScreenTexture.wrapMode = TextureWrapMode.Clamp;
		}
	}

	[ContextMenu("Unfreeze")]
	public void Unfreeze()
	{
		m_screenEffectsHandle.StopEffect();
		m_FrozenState = false;
		if (m_FrozenScreenTexture != null)
		{
			Object.DestroyImmediate(m_FrozenScreenTexture);
			m_FrozenScreenTexture = null;
		}
		Disable();
	}

	public bool isActive()
	{
		if (!base.enabled)
		{
			return false;
		}
		if (m_FrozenState)
		{
			return true;
		}
		return false;
	}

	private void EndCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		if (!(m_Camera != camera) && !m_FrozenState)
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
}
