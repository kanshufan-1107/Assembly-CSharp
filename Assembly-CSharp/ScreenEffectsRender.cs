using UnityEngine;

[ExecuteAlways]
public class ScreenEffectsRender : MonoBehaviour
{
	private const int GLOW_RANDER_BUFFER_RESOLUTION = 512;

	public Camera m_EffectsObjectsCamera;

	public bool m_Debug;

	public RenderTexture m_MaskRenderTexture;

	private int m_width;

	private int m_height;

	private int m_previousWidth;

	private int m_previousHeight;

	private void Awake()
	{
		if (ScreenEffectsMgr.Get() == null)
		{
			base.enabled = false;
		}
		m_EffectsObjectsCamera = GetComponent<Camera>();
	}

	private void Update()
	{
		if (m_EffectsObjectsCamera == null)
		{
			base.enabled = false;
			return;
		}
		float ratio = (float)Screen.width / (float)Screen.height;
		int glowWidth = (int)(512f * ratio);
		int glowHeight = 512;
		if (glowWidth != m_previousWidth || glowHeight != m_previousHeight)
		{
			RenderTextureTracker.Get().DestroyRenderTexture(m_MaskRenderTexture);
			m_MaskRenderTexture = null;
		}
		if (m_MaskRenderTexture == null)
		{
			m_MaskRenderTexture = RenderTextureTracker.Get().CreateNewTexture(glowWidth, glowHeight, RenderTextureTracker.TEXTURE_DEPTH, RenderTextureFormat.ARGB32);
			m_MaskRenderTexture.filterMode = FilterMode.Bilinear;
			m_MaskRenderTexture.useMipMap = true;
			m_previousWidth = glowWidth;
			m_previousHeight = glowHeight;
		}
	}

	private void OnDisable()
	{
		if (m_MaskRenderTexture != null)
		{
			RenderTextureTracker.Get().DestroyRenderTexture(m_MaskRenderTexture);
			m_MaskRenderTexture = null;
		}
	}
}
