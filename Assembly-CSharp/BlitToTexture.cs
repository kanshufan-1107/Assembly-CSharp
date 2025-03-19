using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class BlitToTexture : MonoBehaviour
{
	[SerializeField]
	private Vector2Int m_textureSize = new Vector2Int(256, 256);

	[SerializeField]
	private RenderTextureFormat m_renderTextureFormat = RenderTextureFormat.Default;

	[SerializeField]
	private Renderer m_drawAfterBlit;

	public bool AssignTextureAsRendererMainTex;

	public Vector2 Offset;

	public bool CenteredOffset;

	public bool OffsetFollowPosition;

	protected Camera m_mainCamera;

	public float RotationDegrees;

	public float ZoomFactor = 1f;

	public bool ScaleZoomFrom1080P = true;

	private readonly BlitToTextureService.Request m_request = new BlitToTextureService.Request();

	public Renderer DrawAfterBlit
	{
		get
		{
			return m_drawAfterBlit;
		}
		set
		{
			m_drawAfterBlit = value;
			m_request.DrawAfterRenderer = m_drawAfterBlit;
		}
	}

	public RenderTexture TargetTexture { get; private set; }

	public BlitToTexture()
	{
	}

	public BlitToTexture(Vector2Int textureSize, RenderTextureFormat renderTextureFormat = RenderTextureFormat.Default)
	{
		m_textureSize = textureSize;
		m_renderTextureFormat = renderTextureFormat;
	}

	protected virtual void Awake()
	{
		TargetTexture = RenderTextureTracker.Get().CreateNewTexture(m_textureSize.x, m_textureSize.y, 0, m_renderTextureFormat);
		m_request.TargetTexture = TargetTexture;
		m_request.DrawAfterRenderer = m_drawAfterBlit;
		if (AssignTextureAsRendererMainTex && m_drawAfterBlit != null)
		{
			m_drawAfterBlit.GetMaterial().mainTexture = TargetTexture;
		}
	}

	private void OnDestroy()
	{
		RenderTextureTracker.Get().DestroyRenderTexture(TargetTexture);
	}

	private void OnEnable()
	{
		BlitToTextureService.AddPersistentRequest(m_request);
	}

	private void OnDisable()
	{
		BlitToTextureService.RemovePersistentRequest(m_request);
	}

	protected virtual void Update()
	{
		float resolutionScaleFactor = 1f;
		if (ScaleZoomFrom1080P)
		{
			resolutionScaleFactor = (float)((double)CameraUtils.GetMainCamera().scaledPixelHeight / 1080.0);
		}
		m_request.Size = new Vector2(m_textureSize.x, m_textureSize.y) * ZoomFactor * resolutionScaleFactor;
		m_request.Offset = Offset;
		if (CenteredOffset)
		{
			m_request.Offset -= m_request.Size / 2f;
		}
		if (OffsetFollowPosition)
		{
			if (m_mainCamera == null)
			{
				m_mainCamera = CameraUtils.GetMainCamera();
			}
			Vector2 viewportPosition = m_mainCamera.WorldToScreenPoint(base.transform.position);
			Offset = viewportPosition;
		}
		m_request.RotationDeg = RotationDegrees;
	}
}
