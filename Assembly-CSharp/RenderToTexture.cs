using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone.UI.Core;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderToTexture : MonoBehaviour, IPopupRendering
{
	public enum RenderToTextureMaterial
	{
		Custom,
		Transparent,
		Additive,
		Bloom,
		AlphaClip,
		AlphaClipBloom
	}

	public enum AlphaClipShader
	{
		Standard,
		ColorGradient
	}

	public enum BloomRenderType
	{
		Color,
		Alpha
	}

	public enum BloomBlendType
	{
		Additive,
		Transparent
	}

	private const string BLUR_SHADER_NAME = "Hidden/R2TBlur";

	private const string BLUR_ALPHA_SHADER_NAME = "Hidden/R2TAlphaBlur";

	private const string ALPHA_BLEND_SHADER_NAME = "Hidden/R2TColorAlphaCombine";

	private const string ALPHA_BLEND_ADD_SHADER_NAME = "Hidden/R2TColorAlphaCombineAdd";

	private const string ALPHA_FILL_SHADER_NAME = "Custom/AlphaFillOpaque";

	private const string BLOOM_SHADER_NAME = "Hidden/R2TBloom";

	private const string BLOOM_ALPHA_SHADER_NAME = "Hidden/R2TBloomAlpha";

	private const string ADDITIVE_SHADER_NAME = "Hidden/R2TAdditive";

	private const string TRANSPARENT_SHADER_NAME = "R2TTransparent";

	private const string ALPHA_CLIP_SHADER_NAME = "Hidden/R2TAlphaClip";

	private const string ALPHA_CLIP_BLOOM_SHADER_NAME = "Hidden/R2TAlphaClipBloom";

	private const string ALPHA_CLIP_GRADIENT_SHADER_NAME = "Hidden/R2TAlphaClipGradient";

	private const RenderTextureFormat ALPHA_TEXTURE_FORMAT = RenderTextureFormat.R8;

	private const float OFFSET_DISTANCE = 300f;

	private const float MIN_OFFSET_DISTANCE = -4000f;

	private const float MAX_OFFSET_DISTANCE = -90000f;

	private readonly Vector3 ALPHA_OBJECT_OFFSET = new Vector3(0f, 1000f, 0f);

	private const float RENDER_SIZE_QUALITY_LOW = 0.75f;

	private const float RENDER_SIZE_QUALITY_MEDIUM = 1f;

	private const float RENDER_SIZE_QUALITY_HIGH = 1.5f;

	private readonly Vector2[] PLANE_UVS = new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f)
	};

	private readonly Vector3[] PLANE_NORMALS = new Vector3[4]
	{
		Vector3.up,
		Vector3.up,
		Vector3.up,
		Vector3.up
	};

	private readonly int[] PLANE_TRIANGLES = new int[6] { 3, 1, 2, 2, 1, 0 };

	public GameObject m_ObjectToRender;

	public GameObject m_AlphaObjectToRender;

	public bool m_HideRenderObject = true;

	public bool m_RealtimeRender;

	public bool m_RealtimeTranslation;

	public bool m_RenderMeshAsAlpha;

	public bool m_OpaqueObjectAlphaFill;

	public RenderToTextureMaterial m_RenderMaterial;

	public Material m_Material;

	public bool m_CreateRenderPlane = true;

	public GameObject m_RenderToObject;

	public string m_ShaderTextureName = string.Empty;

	public int m_Resolution = 128;

	public float m_Width = 1f;

	public float m_Height = 1f;

	public float m_NearClip = -0.1f;

	public float m_FarClip = 0.5f;

	public float m_BloomIntensity;

	public float m_BloomThreshold = 0.7f;

	public float m_BloomBlur = 0.3f;

	public float m_BloomResolutionRatio = 0.5f;

	public BloomRenderType m_BloomRenderType;

	public float m_BloomAlphaIntensity = 1f;

	public BloomBlendType m_BloomBlend;

	public Color m_BloomColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public AlphaClipShader m_AlphaClipRenderStyle;

	public float m_AlphaClip = 15f;

	public float m_AlphaClipIntensity = 1.5f;

	public float m_AlphaClipAlphaIntensity = 1f;

	public Texture2D m_AlphaClipGradientMap;

	public float m_BlurAmount;

	public bool m_BlurAlphaOnly;

	public Color m_TintColor = Color.white;

	public int m_RenderQueueOffset = 3000;

	public int m_RenderQueue;

	public Color m_ClearColor = Color.clear;

	public Shader m_ReplacmentShader;

	public string m_ReplacmentTag;

	public string m_AlphaReplacementTag;

	public RenderTextureFormat m_RenderTextureFormat = RenderTextureFormat.Default;

	public Vector3 m_PositionOffset = Vector3.zero;

	public Vector3 m_CameraOffset = Vector3.zero;

	public LayerMask m_LayerMask = -1;

	public bool m_UniformWorldScale;

	public float m_OverrideCameraSize;

	public bool m_LateUpdate;

	public bool m_RenderOnStart = true;

	public bool m_RenderOnEnable = true;

	private bool m_renderEnabled = true;

	private bool m_init;

	private float m_WorldWidth;

	private float m_WorldHeight;

	private Vector3 m_WorldScale;

	private GameObject m_OffscreenGameObject;

	private GameObject m_CameraGO;

	private RenderToTextureUtils.LightWeightCamera m_CameraData;

	private GameObject m_AlphaCameraGO;

	private RenderToTextureUtils.LightWeightCamera m_AlphaCameraData;

	private GameObject m_BloomCaptureCameraGO;

	private RenderToTextureUtils.LightWeightCamera m_BloomCameraData;

	private Camera m_Camera;

	private string m_RttCommandBufferName;

	private string m_BloomCommandbufferName;

	private RenderTexture m_RenderTexture;

	private RenderTexture m_BloomRenderTexture;

	private RenderTexture m_BloomRenderBuffer1;

	private RenderTexture m_BloomRenderBuffer2;

	private GameObject m_PlaneGameObject;

	private GameObject m_BloomPlaneGameObject;

	private GameObject m_BloomCapturePlaneGameObject;

	private bool m_ObjectToRenderOrgPositionStored;

	private Transform m_ObjectToRenderOrgParent;

	private Vector3 m_ObjectToRenderOrgPosition = Vector3.zero;

	private Vector3 m_OriginalRenderPosition = Vector3.zero;

	private bool m_isDirty;

	private Shader m_AlphaFillShader;

	private Vector3 m_OffscreenPos;

	private Vector3 m_ObjectToRenderOffset = Vector3.zero;

	private Vector3 m_AlphaObjectToRenderOffset = Vector3.zero;

	private RenderToTextureMaterial m_PreviousRenderMaterial;

	private int m_previousRenderQueue;

	private List<Renderer> m_OpaqueObjectAlphaFillTransparent;

	private List<UberText> m_OpaqueObjectAlphaFillUberText;

	private bool m_hasMaterialInstance;

	private static IMaterialService s_materialService;

	private RenderCommandLists.MatOverrideDictionary m_materialOverrides;

	private Vector3 m_Offset = Vector3.zero;

	private static Vector3 s_offset = new Vector3(-4000f, -4000f, -4000f);

	private IGraphicsManager m_graphicsManager;

	private static ProfilerMarker s_RenderTexInit = new ProfilerMarker("RTT_Init");

	private static ProfilerMarker s_RenderTex = new ProfilerMarker("RTT_RenderTex");

	private static ProfilerMarker s_RenderTexBloom = new ProfilerMarker("RTT_RenderBloom");

	private Shader m_AlphaBlendShader;

	private Material m_AlphaBlendMaterial;

	private Shader m_AlphaBlendAddShader;

	private Material m_AlphaBlendAddMaterial;

	private Shader m_AdditiveShader;

	private Material m_AdditiveMaterial;

	private Shader m_BloomShader;

	private Material m_BloomMaterial;

	private Shader m_BloomShaderAlpha;

	private Material m_BloomMaterialAlpha;

	private Shader m_BlurShader;

	private Material m_BlurMaterial;

	private Shader m_AlphaBlurShader;

	private Material m_AlphaBlurMaterial;

	private Shader m_TransparentShader;

	private Material m_TransparentMaterial;

	private Shader m_AlphaClipShader;

	private Material m_AlphaClipMaterial;

	private Shader m_AlphaClipBloomShader;

	private Material m_AlphaClipBloomMaterial;

	private Shader m_AlphaClipGradientShader;

	private Material m_AlphaClipGradientMaterial;

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupRenderers = new HashSet<IPopupRendering>();

	protected Vector3 Offset
	{
		get
		{
			if (m_Offset == Vector3.zero)
			{
				s_offset.x -= 300f;
				if (s_offset.x < -90000f)
				{
					s_offset.x = -4000f;
					s_offset.y -= 300f;
					if (s_offset.y < -90000f)
					{
						s_offset.y = -4000f;
						s_offset.z -= 300f;
						if (s_offset.z < -90000f)
						{
							s_offset.z = -4000f;
						}
					}
				}
				m_Offset = s_offset;
			}
			return m_Offset;
		}
	}

	protected Material AlphaBlendMaterial
	{
		get
		{
			if (m_AlphaBlendMaterial == null)
			{
				if (m_AlphaBlendShader == null)
				{
					m_AlphaBlendShader = ShaderUtils.FindShader("Hidden/R2TColorAlphaCombine");
					if (!m_AlphaBlendShader)
					{
						Debug.LogError("Failed to load RenderToTexture Shader: Hidden/R2TColorAlphaCombine");
					}
				}
				m_AlphaBlendMaterial = new Material(m_AlphaBlendShader);
				GameObjectUtils.SetHideFlags(m_AlphaBlendMaterial, HideFlags.DontSave);
			}
			return m_AlphaBlendMaterial;
		}
	}

	protected Material AlphaBlendAddMaterial
	{
		get
		{
			if (m_AlphaBlendAddMaterial == null)
			{
				if (m_AlphaBlendAddShader == null)
				{
					m_AlphaBlendAddShader = ShaderUtils.FindShader("Hidden/R2TColorAlphaCombineAdd");
					if (!m_AlphaBlendAddShader)
					{
						Debug.LogError("Failed to load RenderToTexture Shader: Hidden/R2TColorAlphaCombineAdd");
					}
				}
				m_AlphaBlendAddMaterial = new Material(m_AlphaBlendAddShader);
				GameObjectUtils.SetHideFlags(m_AlphaBlendAddMaterial, HideFlags.DontSave);
			}
			return m_AlphaBlendAddMaterial;
		}
	}

	protected Material AdditiveMaterial
	{
		get
		{
			if (m_AdditiveMaterial == null)
			{
				if (m_AdditiveShader == null)
				{
					m_AdditiveShader = ShaderUtils.FindShader("Hidden/R2TAdditive");
					if (!m_AdditiveShader)
					{
						Debug.LogError("Failed to load RenderToTexture Shader: Hidden/R2TAdditive");
					}
				}
				m_AdditiveMaterial = new Material(m_AdditiveShader);
				GameObjectUtils.SetHideFlags(m_AdditiveMaterial, HideFlags.DontSave);
			}
			return m_AdditiveMaterial;
		}
	}

	protected Material BloomMaterial
	{
		get
		{
			if (m_BloomMaterial == null)
			{
				if (m_BloomShader == null)
				{
					m_BloomShader = ShaderUtils.FindShader("Hidden/R2TBloom");
					if (!m_BloomShader)
					{
						Debug.LogError("Failed to load RenderToTexture Shader: Hidden/R2TBloom");
					}
				}
				m_BloomMaterial = new Material(m_BloomShader);
				GameObjectUtils.SetHideFlags(m_BloomMaterial, HideFlags.DontSave);
			}
			return m_BloomMaterial;
		}
	}

	protected Material BloomMaterialAlpha
	{
		get
		{
			if (m_BloomMaterialAlpha == null)
			{
				if (m_BloomShaderAlpha == null)
				{
					m_BloomShaderAlpha = ShaderUtils.FindShader("Hidden/R2TBloomAlpha");
					if (!m_BloomShaderAlpha)
					{
						Debug.LogError("Failed to load RenderToTexture Shader: Hidden/R2TBloomAlpha");
					}
				}
				m_BloomMaterialAlpha = new Material(m_BloomShaderAlpha);
				GameObjectUtils.SetHideFlags(m_BloomMaterialAlpha, HideFlags.DontSave);
			}
			return m_BloomMaterialAlpha;
		}
	}

	protected Material BlurMaterial
	{
		get
		{
			if (m_BlurMaterial == null)
			{
				if (m_BlurShader == null)
				{
					m_BlurShader = ShaderUtils.FindShader("Hidden/R2TBlur");
					if (!m_BlurShader)
					{
						Debug.LogError("Failed to load RenderToTexture Shader: Hidden/R2TBlur");
					}
				}
				m_BlurMaterial = new Material(m_BlurShader);
				GameObjectUtils.SetHideFlags(m_BlurMaterial, HideFlags.DontSave);
			}
			return m_BlurMaterial;
		}
	}

	protected Material AlphaBlurMaterial
	{
		get
		{
			if (m_AlphaBlurMaterial == null)
			{
				if (m_AlphaBlurShader == null)
				{
					m_AlphaBlurShader = ShaderUtils.FindShader("Hidden/R2TAlphaBlur");
					if (!m_AlphaBlurShader)
					{
						Debug.LogError("Failed to load RenderToTexture Shader: Hidden/R2TAlphaBlur");
					}
				}
				m_AlphaBlurMaterial = new Material(m_AlphaBlurShader);
				GameObjectUtils.SetHideFlags(m_AlphaBlurMaterial, HideFlags.DontSave);
			}
			return m_AlphaBlurMaterial;
		}
	}

	protected Material TransparentMaterial
	{
		get
		{
			if (m_TransparentMaterial == null)
			{
				if (m_TransparentShader == null)
				{
					m_TransparentShader = ShaderUtils.FindShader("R2TTransparent");
					if (!m_TransparentShader)
					{
						Debug.LogError("Failed to load RenderToTexture Shader: R2TTransparent");
					}
				}
				m_TransparentMaterial = new Material(m_TransparentShader);
				GameObjectUtils.SetHideFlags(m_TransparentMaterial, HideFlags.DontSave);
			}
			return m_TransparentMaterial;
		}
	}

	protected Material AlphaClipMaterial
	{
		get
		{
			if (m_AlphaClipMaterial == null)
			{
				if (m_AlphaClipShader == null)
				{
					m_AlphaClipShader = ShaderUtils.FindShader("Hidden/R2TAlphaClip");
					if (!m_AlphaClipShader)
					{
						Debug.LogError("Failed to load RenderToTexture Shader: Hidden/R2TAlphaClip");
					}
				}
				m_AlphaClipMaterial = new Material(m_AlphaClipShader);
				GameObjectUtils.SetHideFlags(m_AlphaClipMaterial, HideFlags.DontSave);
			}
			return m_AlphaClipMaterial;
		}
	}

	protected Material AlphaClipBloomMaterial
	{
		get
		{
			if (m_AlphaClipBloomMaterial == null)
			{
				if (m_AlphaClipBloomShader == null)
				{
					m_AlphaClipBloomShader = ShaderUtils.FindShader("Hidden/R2TAlphaClipBloom");
					if (!m_AlphaClipBloomShader)
					{
						Debug.LogError("Failed to load RenderToTexture Shader: Hidden/R2TAlphaClipBloom");
					}
				}
				m_AlphaClipBloomMaterial = new Material(m_AlphaClipBloomShader);
				GameObjectUtils.SetHideFlags(m_AlphaClipBloomMaterial, HideFlags.DontSave);
			}
			return m_AlphaClipBloomMaterial;
		}
	}

	protected Material AlphaClipGradientMaterial
	{
		get
		{
			if (m_AlphaClipGradientMaterial == null)
			{
				if (m_AlphaClipGradientShader == null)
				{
					m_AlphaClipGradientShader = ShaderUtils.FindShader("Hidden/R2TAlphaClipGradient");
					if (!m_AlphaClipGradientShader)
					{
						Debug.LogError("Failed to load RenderToTexture Shader: Hidden/R2TAlphaClipGradient");
					}
				}
				m_AlphaClipGradientMaterial = new Material(m_AlphaClipGradientShader);
				GameObjectUtils.SetHideFlags(m_AlphaClipGradientMaterial, HideFlags.DontSave);
			}
			return m_AlphaClipGradientMaterial;
		}
	}

	public bool DontRefreshOnFocus { get; set; }

	public void SetMaterialOverrides(RenderCommandLists.MatOverrideDictionary arg)
	{
		m_materialOverrides = arg;
	}

	public void ClearMaterialOverrides()
	{
		m_materialOverrides = null;
	}

	private void Awake()
	{
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		m_AlphaFillShader = ShaderUtils.FindShader("Custom/AlphaFillOpaque");
		if (!m_AlphaFillShader)
		{
			Debug.LogError("Failed to load RenderToTexture Shader: Custom/AlphaFillOpaque");
		}
		m_OffscreenPos = Offset;
		if (m_Material != null)
		{
			m_Material = Object.Instantiate(m_Material);
			m_hasMaterialInstance = true;
			GetMaterialService().IgnoreMaterial(m_Material);
		}
	}

	private void Start()
	{
		if (m_RenderOnStart)
		{
			m_isDirty = true;
		}
		Init();
	}

	private void Update()
	{
		if (!m_renderEnabled)
		{
			return;
		}
		if ((bool)m_RenderTexture && !m_RenderTexture.IsCreated())
		{
			Log.Graphics.Print("RenderToTexture Texture lost. Render Called");
			m_isDirty = true;
			RenderTex();
		}
		else if (!m_LateUpdate)
		{
			if (m_HideRenderObject && (bool)m_ObjectToRender)
			{
				PositionHiddenObjectsAndCameras();
			}
			if (m_RealtimeRender || m_isDirty)
			{
				RenderTex();
			}
		}
	}

	private void LateUpdate()
	{
		if (!m_renderEnabled)
		{
			return;
		}
		if (m_LateUpdate)
		{
			if (m_HideRenderObject && (bool)m_ObjectToRender)
			{
				PositionHiddenObjectsAndCameras();
			}
			if (m_RealtimeRender || m_isDirty)
			{
				RenderTex();
			}
		}
		else if (m_RenderMaterial == RenderToTextureMaterial.AlphaClipBloom || m_RenderMaterial == RenderToTextureMaterial.Bloom)
		{
			RenderBloom();
		}
		else if ((bool)m_BloomPlaneGameObject)
		{
			Object.DestroyImmediate(m_BloomPlaneGameObject);
		}
	}

	private void OnApplicationFocus(bool state)
	{
		if (!DontRefreshOnFocus && (bool)m_RenderTexture && state)
		{
			m_isDirty = true;
			RenderTex();
		}
	}

	private void OnDrawGizmos()
	{
		if (base.enabled)
		{
			if (m_FarClip < 0f)
			{
				m_FarClip = 0f;
			}
			if (m_NearClip > 0f)
			{
				m_NearClip = 0f;
			}
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Vector3 vector = new Vector3(0f, (0f - m_NearClip) * 0.5f, 0f);
			Gizmos.color = new Color(0.1f, 0.5f, 0.7f, 0.8f);
			Gizmos.DrawWireCube(vector + m_PositionOffset, new Vector3(m_Width, 0f - m_NearClip, m_Height));
			Gizmos.color = new Color(0.2f, 0.2f, 0.9f, 0.8f);
			Gizmos.DrawWireCube(new Vector3(0f, (0f - m_FarClip) * 0.5f, 0f) + m_PositionOffset, new Vector3(m_Width, 0f - m_FarClip, m_Height));
			Gizmos.color = new Color(0.8f, 0.8f, 1f, 1f);
			Gizmos.DrawWireCube(m_PositionOffset, new Vector3(m_Width, 0f, m_Height));
			Gizmos.matrix = Matrix4x4.identity;
		}
	}

	private void OnDisable()
	{
		RestoreAfterRender();
		if ((bool)m_ObjectToRender)
		{
			if (m_ObjectToRenderOrgParent != null)
			{
				m_ObjectToRender.transform.parent = m_ObjectToRenderOrgParent;
			}
			m_ObjectToRender.transform.localPosition = m_ObjectToRenderOrgPosition;
		}
		if ((bool)m_PlaneGameObject)
		{
			Object.Destroy(m_PlaneGameObject);
		}
		if ((bool)m_BloomPlaneGameObject)
		{
			Object.Destroy(m_BloomPlaneGameObject);
		}
		if ((bool)m_BloomCapturePlaneGameObject)
		{
			Object.Destroy(m_BloomCapturePlaneGameObject);
		}
		if ((bool)m_BloomCaptureCameraGO)
		{
			Object.Destroy(m_BloomCaptureCameraGO);
		}
		ReleaseTexture();
		m_init = false;
		m_isDirty = true;
	}

	private void OnDestroy()
	{
		CleanUp();
	}

	private void OnEnable()
	{
		if (m_RenderOnEnable)
		{
			RenderTex();
		}
	}

	public RenderTexture Render()
	{
		m_isDirty = true;
		return m_RenderTexture;
	}

	public RenderTexture RenderNow()
	{
		RenderTex();
		return m_RenderTexture;
	}

	public void ForceTextureRebuild()
	{
		if (base.enabled)
		{
			ReleaseTexture();
			m_isDirty = true;
			RenderTex();
		}
	}

	public void Show()
	{
		Show(render: false);
	}

	public void Show(bool render)
	{
		m_renderEnabled = true;
		if ((bool)m_RenderToObject)
		{
			m_RenderToObject.GetComponent<Renderer>().enabled = true;
		}
		else if ((bool)m_PlaneGameObject)
		{
			m_PlaneGameObject.GetComponent<Renderer>().enabled = true;
			if ((bool)m_BloomPlaneGameObject)
			{
				m_BloomPlaneGameObject.GetComponent<Renderer>().enabled = true;
			}
		}
		if (render)
		{
			Render();
		}
	}

	public void Hide()
	{
		m_renderEnabled = false;
		if ((bool)m_RenderToObject)
		{
			m_RenderToObject.GetComponent<Renderer>().enabled = false;
		}
		else if ((bool)m_PlaneGameObject)
		{
			m_PlaneGameObject.GetComponent<Renderer>().enabled = false;
			if ((bool)m_BloomPlaneGameObject)
			{
				m_BloomPlaneGameObject.GetComponent<Renderer>().enabled = false;
			}
		}
	}

	public void SetDirty()
	{
		m_init = false;
		m_isDirty = true;
	}

	public Material GetRenderMaterial()
	{
		if ((bool)m_RenderToObject)
		{
			return m_RenderToObject.GetComponent<Renderer>().GetMaterial();
		}
		if ((bool)m_PlaneGameObject)
		{
			return m_PlaneGameObject.GetComponent<Renderer>().GetMaterial();
		}
		return m_Material;
	}

	public GameObject GetRenderToObject()
	{
		if ((bool)m_RenderToObject)
		{
			return m_RenderToObject;
		}
		return m_PlaneGameObject;
	}

	public RenderTexture GetRenderTexture()
	{
		return m_RenderTexture;
	}

	public Vector3 GetOffscreenPosition()
	{
		return m_OffscreenPos;
	}

	public Vector3 GetOffscreenPositionOffset()
	{
		return m_OffscreenPos - base.transform.position;
	}

	private void Init()
	{
		if (m_init)
		{
			return;
		}
		using (s_RenderTexInit.Auto())
		{
			if (m_RealtimeTranslation)
			{
				m_OffscreenGameObject = new GameObject();
				m_OffscreenGameObject.name = $"R2TOffsetRenderRoot_{base.name}";
				m_OffscreenGameObject.transform.position = base.transform.position;
			}
			if ((bool)m_ObjectToRender)
			{
				if (!m_ObjectToRenderOrgPositionStored)
				{
					m_ObjectToRenderOrgParent = m_ObjectToRender.transform.parent;
					m_ObjectToRenderOrgPosition = m_ObjectToRender.transform.localPosition;
					m_ObjectToRenderOrgPositionStored = true;
				}
				if (m_HideRenderObject)
				{
					if (m_RealtimeTranslation)
					{
						m_ObjectToRender.transform.parent = m_OffscreenGameObject.transform;
						if ((bool)m_AlphaObjectToRender)
						{
							m_AlphaObjectToRender.transform.parent = m_OffscreenGameObject.transform;
						}
					}
					if ((bool)m_RenderToObject)
					{
						m_OriginalRenderPosition = m_RenderToObject.transform.position;
					}
					else
					{
						m_OriginalRenderPosition = base.transform.position;
					}
					if ((bool)m_ObjectToRender && m_ObjectToRenderOffset == Vector3.zero)
					{
						m_ObjectToRenderOffset = base.transform.position - m_ObjectToRender.transform.position;
					}
					if ((bool)m_AlphaObjectToRender && m_AlphaObjectToRenderOffset == Vector3.zero)
					{
						m_AlphaObjectToRenderOffset = base.transform.position - m_AlphaObjectToRender.transform.position;
					}
				}
			}
			else if (!m_ObjectToRenderOrgPositionStored)
			{
				m_ObjectToRenderOrgPosition = base.transform.localPosition;
				if (m_OffscreenGameObject != null)
				{
					m_OffscreenGameObject.transform.position = base.transform.position;
				}
				m_ObjectToRenderOrgPositionStored = true;
			}
			if (m_HideRenderObject)
			{
				if (m_RealtimeTranslation)
				{
					if (m_OffscreenGameObject != null)
					{
						m_OffscreenGameObject.transform.position = m_OffscreenPos;
					}
				}
				else if ((bool)m_ObjectToRender)
				{
					m_ObjectToRender.transform.position = m_OffscreenPos;
				}
				else
				{
					base.transform.position = m_OffscreenPos;
				}
			}
			if (m_ObjectToRender == null)
			{
				m_ObjectToRender = base.gameObject;
			}
			CalcWorldWidthHeightScale();
			CreateTexture();
			CreateCamera();
			if (m_OpaqueObjectAlphaFill || m_RenderMeshAsAlpha || m_AlphaObjectToRender != null)
			{
				CreateAlphaCamera();
			}
			if (!m_RenderToObject && m_CreateRenderPlane)
			{
				CreateRenderPlane();
			}
			if ((bool)m_RenderToObject)
			{
				m_RenderToObject.GetComponent<Renderer>().GetMaterial().renderQueue = m_RenderQueueOffset + m_RenderQueue;
			}
			SetupMaterial();
			m_RttCommandBufferName = "RenderToTexture " + base.name;
			m_BloomCommandbufferName = "RenderToTexture Bloom " + base.name;
			m_init = true;
		}
	}

	private void RenderTex()
	{
		using (s_RenderTex.Auto())
		{
			if (!m_renderEnabled)
			{
				return;
			}
			Init();
			if (!m_init)
			{
				return;
			}
			SetupForRender();
			if (m_RenderMaterial != m_PreviousRenderMaterial || m_RenderQueue != m_previousRenderQueue)
			{
				SetupMaterial();
			}
			if (m_HideRenderObject && (bool)m_ObjectToRender)
			{
				PositionHiddenObjectsAndCameras();
			}
			if ((bool)m_PlaneGameObject && !m_HideRenderObject)
			{
				m_PlaneGameObject.GetComponent<Renderer>().enabled = false;
				if ((bool)m_BloomPlaneGameObject)
				{
					m_BloomPlaneGameObject.GetComponent<Renderer>().enabled = false;
				}
			}
			bool num = m_OpaqueObjectAlphaFill || m_RenderMeshAsAlpha || m_AlphaObjectToRender != null;
			bool renderWithBlur = m_BlurAmount > 0f;
			RenderCommandLists RenderCommands = RenderToTextureUtils.RenderCommandListPool.Get(m_ObjectToRender, includeInactiveRenderers: false, m_materialOverrides);
			RenderCommandLists alphaRenderCommands = ((m_AlphaObjectToRender == null) ? null : RenderToTextureUtils.RenderCommandListPool.Get(m_AlphaObjectToRender, includeInactiveRenderers: false, m_materialOverrides));
			CommandBuffer cmd = CommandBufferPool.Get(m_RttCommandBufferName);
			RenderTexture colorBuffer = RenderTexture.GetTemporary(m_RenderTexture.width, m_RenderTexture.height, m_RenderTexture.depth, m_RenderTexture.format);
			RenderTexture alphaBuffer = (num ? RenderTexture.GetTemporary(m_RenderTexture.width, m_RenderTexture.height, 16, RenderTextureFormat.R8) : null);
			RenderTexture blurBuffer = (renderWithBlur ? RenderTexture.GetTemporary(m_RenderTexture.width, m_RenderTexture.height, m_RenderTexture.depth, m_RenderTexture.format) : null);
			m_RenderTexture.DiscardContents();
			m_CameraData.SetOrthoProjectionMatrix(OrthoSize(), m_NearClip * m_WorldScale.z, m_FarClip * m_WorldScale.z);
			m_CameraData.SetWorldToCameraMatrix(m_CameraGO.transform);
			m_Camera.orthographicSize = OrthoSize();
			m_Camera.farClipPlane = m_FarClip * m_WorldScale.z;
			m_Camera.nearClipPlane = m_NearClip * m_WorldScale.z;
			Camera.SetupCurrent(m_Camera);
			if (num)
			{
				RenderToTextureUtils.RenderCamera(cmd, colorBuffer, m_CameraData, RenderCommands, m_ReplacmentShader, m_ReplacmentTag);
				m_AlphaCameraData.SetOrthoProjectionMatrix(OrthoSize(), m_NearClip * m_WorldScale.z, m_FarClip * m_WorldScale.z);
				m_AlphaCameraData.SetWorldToCameraMatrix(m_AlphaCameraGO.transform);
				AlphaCameraRender(cmd, alphaBuffer, m_AlphaCameraData, RenderCommands, alphaRenderCommands);
				if (m_OpaqueObjectAlphaFill)
				{
					AlphaBlendAddMaterial.SetTexture("_AlphaTex", alphaBuffer);
				}
				else
				{
					AlphaBlendMaterial.SetTexture("_AlphaTex", alphaBuffer);
				}
				if (renderWithBlur)
				{
					if (m_OpaqueObjectAlphaFill)
					{
						cmd.Blit(colorBuffer, blurBuffer, AlphaBlendAddMaterial);
					}
					else
					{
						cmd.Blit(colorBuffer, blurBuffer, AlphaBlendMaterial);
					}
					Material blurMat = (m_BlurAlphaOnly ? BlurMaterial : AlphaBlurMaterial);
					blurMat.SetVector("_BlurOffsets", new Vector4(m_BlurAmount, m_BlurAmount, m_BlurAmount, m_BlurAmount));
					blurMat.SetVector("_MainTex_TexelSize", new Vector4(1f / (float)blurBuffer.width, 1f / (float)blurBuffer.height, 0f, 0f));
					cmd.Blit(blurBuffer, m_RenderTexture, blurMat);
				}
				else if (m_OpaqueObjectAlphaFill)
				{
					cmd.Blit(colorBuffer, m_RenderTexture, AlphaBlendAddMaterial);
				}
				else
				{
					cmd.Blit(colorBuffer, m_RenderTexture, AlphaBlendMaterial);
				}
			}
			else if (renderWithBlur)
			{
				RenderToTextureUtils.RenderCamera(cmd, blurBuffer, m_CameraData, RenderCommands, m_ReplacmentShader, m_ReplacmentTag);
				Material blurMat2 = BlurMaterial;
				if (m_BlurAlphaOnly)
				{
					blurMat2 = m_AlphaBlurMaterial;
				}
				blurMat2.SetVector("_BlurOffsets", new Vector4(m_BlurAmount, m_BlurAmount, m_BlurAmount, m_BlurAmount));
				blurMat2.SetVector("_MainTex_TexelSize", new Vector4(1f / (float)blurBuffer.width, 1f / (float)blurBuffer.height, 0f, 0f));
				cmd.Blit(blurBuffer, m_RenderTexture, blurMat2);
			}
			else
			{
				RenderToTextureUtils.RenderCamera(cmd, m_RenderTexture, m_CameraData, RenderCommands, m_ReplacmentShader, m_ReplacmentTag);
			}
			Graphics.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
			RenderToTextureUtils.RenderCommandListPool.Release(RenderCommands);
			RenderToTextureUtils.RenderCommandListPool.Release(alphaRenderCommands);
			RenderTexture.ReleaseTemporary(colorBuffer);
			if ((bool)alphaBuffer)
			{
				RenderTexture.ReleaseTemporary(alphaBuffer);
			}
			if ((bool)blurBuffer)
			{
				RenderTexture.ReleaseTemporary(blurBuffer);
			}
			if ((bool)m_RenderToObject)
			{
				Renderer renderer = m_RenderToObject.GetComponent<Renderer>();
				if (renderer == null)
				{
					renderer = m_RenderToObject.GetComponentInChildren<Renderer>();
				}
				if (m_ShaderTextureName != string.Empty)
				{
					renderer.GetMaterial().SetTexture(m_ShaderTextureName, m_RenderTexture);
				}
				else
				{
					renderer.GetMaterial().mainTexture = m_RenderTexture;
				}
			}
			else if ((bool)m_PlaneGameObject)
			{
				if (m_ShaderTextureName != string.Empty)
				{
					m_PlaneGameObject.GetComponent<Renderer>().GetMaterial().SetTexture(m_ShaderTextureName, m_RenderTexture);
				}
				else
				{
					m_PlaneGameObject.GetComponent<Renderer>().GetMaterial().mainTexture = m_RenderTexture;
				}
			}
			if (m_RenderMaterial == RenderToTextureMaterial.AlphaClip || m_RenderMaterial == RenderToTextureMaterial.AlphaClipBloom)
			{
				GameObject renderObj = m_PlaneGameObject;
				if ((bool)m_RenderToObject)
				{
					renderObj = m_RenderToObject;
				}
				Material alphaClipMaterial = renderObj.GetComponent<Renderer>().GetMaterial();
				alphaClipMaterial.SetFloat("_Cutoff", m_AlphaClip);
				alphaClipMaterial.SetFloat("_Intensity", m_AlphaClipIntensity);
				alphaClipMaterial.SetFloat("_AlphaIntensity", m_AlphaClipAlphaIntensity);
				if (m_AlphaClipRenderStyle == AlphaClipShader.ColorGradient)
				{
					alphaClipMaterial.SetTexture("_GradientTex", m_AlphaClipGradientMap);
				}
			}
			if ((bool)m_PlaneGameObject && !m_HideRenderObject)
			{
				m_PlaneGameObject.GetComponent<Renderer>().enabled = true;
				if ((bool)m_BloomPlaneGameObject)
				{
					m_BloomPlaneGameObject.GetComponent<Renderer>().enabled = true;
				}
			}
			if (!m_RealtimeRender)
			{
				RestoreAfterRender();
			}
			if (m_popupRoot != null && (m_PlaneGameObject != null || m_BloomPlaneGameObject != null || m_BloomCapturePlaneGameObject != null))
			{
				m_popupRoot.ApplyPopupRendering(base.transform, m_popupRenderers, overrideLayer: true, base.gameObject.layer);
			}
			m_isDirty = false;
			Camera.SetupCurrent(CameraUtils.GetMainCamera());
		}
	}

	private void RenderBloom()
	{
		using (s_RenderTexBloom.Auto())
		{
			if (m_BloomIntensity == 0f)
			{
				if ((bool)m_BloomPlaneGameObject)
				{
					Object.DestroyImmediate(m_BloomPlaneGameObject);
				}
				return;
			}
			Camera.SetupCurrent(Camera.main);
			int bloomWidth = (int)((float)m_RenderTexture.width * Mathf.Clamp01(m_BloomResolutionRatio));
			int bloomHeight = (int)((float)m_RenderTexture.height * Mathf.Clamp01(m_BloomResolutionRatio));
			RenderTexture bloomTexture = m_RenderTexture;
			if (m_RenderMaterial == RenderToTextureMaterial.AlphaClipBloom)
			{
				if (!m_BloomPlaneGameObject)
				{
					CreateBloomPlane();
				}
				if (!m_BloomRenderTexture)
				{
					m_BloomRenderTexture = RenderTextureTracker.Get().CreateNewTexture(bloomWidth, bloomHeight, RenderTextureTracker.TEXTURE_DEPTH, RenderTextureFormat.ARGB32);
				}
			}
			if (!m_BloomRenderBuffer1)
			{
				m_BloomRenderBuffer1 = RenderTextureTracker.Get().CreateNewTexture(bloomWidth, bloomHeight, RenderTextureTracker.TEXTURE_DEPTH, RenderTextureFormat.ARGB32);
			}
			if (!m_BloomRenderBuffer2)
			{
				m_BloomRenderBuffer2 = RenderTextureTracker.Get().CreateNewTexture(bloomWidth, bloomHeight, RenderTextureTracker.TEXTURE_DEPTH, RenderTextureFormat.ARGB32);
			}
			Material bloomMat = BloomMaterial;
			if (m_RenderMaterial == RenderToTextureMaterial.AlphaClipBloom)
			{
				bloomMat = AlphaClipBloomMaterial;
				bloomTexture = m_BloomRenderTexture;
				if (!m_BloomCaptureCameraGO)
				{
					CreateBloomCaptureCamera();
				}
				m_BloomCameraData.SetWorldToCameraMatrix(m_BloomCaptureCameraGO.transform);
				bloomMat.SetFloat("_Cutoff", m_AlphaClip);
				bloomMat.SetFloat("_Intensity", m_AlphaClipIntensity);
				bloomMat.SetFloat("_AlphaIntensity", m_AlphaClipAlphaIntensity);
				RenderCommandLists renderCommands = RenderToTextureUtils.RenderCommandListPool.Get(m_ObjectToRender, includeInactiveRenderers: false, m_materialOverrides);
				CommandBuffer commandBuffer = CommandBufferPool.Get(m_BloomCommandbufferName);
				RenderToTextureUtils.RenderCamera(commandBuffer, bloomTexture, m_BloomCameraData, renderCommands);
				Graphics.ExecuteCommandBuffer(commandBuffer);
				CommandBufferPool.Release(commandBuffer);
				RenderToTextureUtils.RenderCommandListPool.Release(renderCommands);
			}
			if (m_BloomRenderType == BloomRenderType.Alpha)
			{
				bloomMat = BloomMaterialAlpha;
				bloomMat.SetFloat("_AlphaIntensity", m_BloomAlphaIntensity);
			}
			float oow = 1f / (float)bloomWidth;
			float ooh = 1f / (float)bloomHeight;
			bloomMat.SetFloat("_Threshold", m_BloomThreshold);
			bloomMat.SetFloat("_Intensity", m_BloomIntensity / (1f - m_BloomThreshold));
			bloomMat.SetVector("_OffsetA", new Vector4(1.5f * oow, 1.5f * ooh, -1.5f * oow, 1.5f * ooh));
			bloomMat.SetVector("_OffsetB", new Vector4(-1.5f * oow, -1.5f * ooh, 1.5f * oow, -1.5f * ooh));
			m_BloomRenderBuffer2.DiscardContents();
			Graphics.Blit(bloomTexture, m_BloomRenderBuffer2, bloomMat, 1);
			oow *= 4f * m_BloomBlur;
			ooh *= 4f * m_BloomBlur;
			bloomMat.SetVector("_OffsetA", new Vector4(1.5f * oow, 0f, -1.5f * oow, 0f));
			bloomMat.SetVector("_OffsetB", new Vector4(0.5f * oow, 0f, -0.5f * oow, 0f));
			m_BloomRenderBuffer1.DiscardContents();
			Graphics.Blit(m_BloomRenderBuffer2, m_BloomRenderBuffer1, bloomMat, 2);
			bloomMat.SetVector("_OffsetA", new Vector4(0f, 1.5f * ooh, 0f, -1.5f * ooh));
			bloomMat.SetVector("_OffsetB", new Vector4(0f, 0.5f * ooh, 0f, -0.5f * ooh));
			bloomTexture.DiscardContents();
			Graphics.Blit(m_BloomRenderBuffer1, bloomTexture, bloomMat, 2);
			Material planeMaterial = m_PlaneGameObject.GetComponent<Renderer>().GetMaterial();
			if (m_RenderMaterial == RenderToTextureMaterial.AlphaClipBloom)
			{
				Material bloomPlaneMaterial = m_BloomPlaneGameObject.GetComponent<Renderer>().GetMaterial();
				bloomPlaneMaterial.color = m_BloomColor;
				bloomPlaneMaterial.mainTexture = bloomTexture;
				if ((bool)m_PlaneGameObject)
				{
					bloomPlaneMaterial.renderQueue = planeMaterial.renderQueue + 1;
				}
			}
			else if ((bool)m_RenderToObject)
			{
				Material material = m_RenderToObject.GetComponent<Renderer>().GetMaterial();
				material.color = m_BloomColor;
				material.mainTexture = bloomTexture;
			}
			else
			{
				planeMaterial.color = m_BloomColor;
				planeMaterial.mainTexture = bloomTexture;
			}
		}
	}

	private void SetupForRender()
	{
		CalcWorldWidthHeightScale();
		if (!m_RenderTexture)
		{
			CreateTexture();
		}
		if (m_HideRenderObject)
		{
			if ((bool)m_PlaneGameObject)
			{
				m_PlaneGameObject.transform.localPosition = m_PositionOffset;
				m_PlaneGameObject.layer = base.gameObject.layer;
			}
			m_CameraData.backgroundColor = m_ClearColor;
		}
	}

	private void SetupMaterial()
	{
		GameObject renderObj = m_PlaneGameObject;
		if ((bool)m_RenderToObject)
		{
			renderObj = m_RenderToObject;
			if (m_RenderMaterial == RenderToTextureMaterial.Custom)
			{
				return;
			}
		}
		if (renderObj == null)
		{
			return;
		}
		Renderer renderer = renderObj.GetComponent<Renderer>();
		switch (m_RenderMaterial)
		{
		case RenderToTextureMaterial.Additive:
			renderer.SetMaterial(AdditiveMaterial);
			break;
		case RenderToTextureMaterial.Transparent:
			renderer.SetMaterial(TransparentMaterial);
			break;
		case RenderToTextureMaterial.AlphaClip:
		{
			Material alphaClipMaterial = ((m_AlphaClipRenderStyle != 0) ? AlphaClipGradientMaterial : AlphaClipMaterial);
			renderer.SetMaterial(alphaClipMaterial);
			alphaClipMaterial.SetFloat("_Cutoff", m_AlphaClip);
			alphaClipMaterial.SetFloat("_Intensity", m_AlphaClipIntensity);
			alphaClipMaterial.SetFloat("_AlphaIntensity", m_AlphaClipAlphaIntensity);
			if (m_AlphaClipRenderStyle == AlphaClipShader.ColorGradient)
			{
				alphaClipMaterial.SetTexture("_GradientTex", m_AlphaClipGradientMap);
			}
			break;
		}
		case RenderToTextureMaterial.AlphaClipBloom:
		{
			Material alphaClipBloomMaterial = ((m_AlphaClipRenderStyle != 0) ? AlphaClipGradientMaterial : AlphaClipMaterial);
			renderer.SetMaterial(alphaClipBloomMaterial);
			alphaClipBloomMaterial.SetFloat("_Cutoff", m_AlphaClip);
			alphaClipBloomMaterial.SetFloat("_Intensity", m_AlphaClipIntensity);
			alphaClipBloomMaterial.SetFloat("_AlphaIntensity", m_AlphaClipAlphaIntensity);
			if (m_AlphaClipRenderStyle == AlphaClipShader.ColorGradient)
			{
				alphaClipBloomMaterial.SetTexture("_GradientTex", m_AlphaClipGradientMap);
			}
			break;
		}
		case RenderToTextureMaterial.Bloom:
			if (m_BloomBlend == BloomBlendType.Additive)
			{
				renderer.SetMaterial(AdditiveMaterial);
			}
			else if (m_BloomBlend == BloomBlendType.Transparent)
			{
				renderer.SetMaterial(TransparentMaterial);
			}
			break;
		default:
			if (m_Material != null)
			{
				renderer.SetMaterial(m_Material);
			}
			break;
		}
		Material rendererMaterial = renderer.GetMaterial();
		if (rendererMaterial != null)
		{
			rendererMaterial.color *= m_TintColor;
		}
		if (m_BloomIntensity > 0f && (bool)m_BloomPlaneGameObject)
		{
			m_BloomPlaneGameObject.GetComponent<Renderer>().GetMaterial().color = m_BloomColor;
		}
		renderer.sortingOrder = m_RenderQueue;
		rendererMaterial.renderQueue = m_RenderQueueOffset + m_RenderQueue;
		if ((bool)m_BloomPlaneGameObject)
		{
			Renderer component = m_BloomPlaneGameObject.GetComponent<Renderer>();
			component.sortingOrder = m_RenderQueue + 1;
			component.GetMaterial().renderQueue = m_RenderQueueOffset + m_RenderQueue + 1;
		}
		m_PreviousRenderMaterial = m_RenderMaterial;
		m_previousRenderQueue = m_RenderQueue;
	}

	private void PositionHiddenObjectsAndCameras()
	{
		Vector3 posDelta = Vector3.zero;
		posDelta = ((!m_RenderToObject) ? (base.transform.position - m_OriginalRenderPosition) : (m_RenderToObject.transform.position - m_OriginalRenderPosition));
		if (m_RealtimeTranslation)
		{
			m_OffscreenGameObject.transform.position = m_OffscreenPos + posDelta;
			m_OffscreenGameObject.transform.rotation = base.transform.rotation;
			if ((bool)m_AlphaObjectToRender)
			{
				m_AlphaObjectToRender.transform.position = m_OffscreenPos - ALPHA_OBJECT_OFFSET - m_AlphaObjectToRenderOffset + posDelta;
				m_AlphaObjectToRender.transform.rotation = base.transform.rotation;
			}
			return;
		}
		if ((bool)m_ObjectToRender)
		{
			m_ObjectToRender.transform.rotation = Quaternion.identity;
			m_ObjectToRender.transform.position = m_OffscreenPos - m_ObjectToRenderOffset + m_PositionOffset;
			m_ObjectToRender.transform.rotation = base.transform.rotation;
		}
		if ((bool)m_AlphaObjectToRender)
		{
			m_AlphaObjectToRender.transform.position = m_OffscreenPos - ALPHA_OBJECT_OFFSET;
			m_AlphaObjectToRender.transform.rotation = base.transform.rotation;
		}
		if (!(m_CameraGO == null))
		{
			m_CameraGO.transform.rotation = Quaternion.identity;
			if ((bool)m_ObjectToRender)
			{
				m_CameraGO.transform.position = m_ObjectToRender.transform.position + m_CameraOffset;
			}
			else
			{
				m_CameraGO.transform.position = m_OffscreenPos + m_PositionOffset + m_CameraOffset;
			}
			m_CameraGO.transform.rotation = m_ObjectToRender.transform.rotation;
			m_CameraGO.transform.Rotate(90f, 0f, 0f);
		}
	}

	private void RestoreAfterRender()
	{
		if (m_HideRenderObject)
		{
			return;
		}
		if ((bool)m_ObjectToRender)
		{
			if (m_ObjectToRenderOrgParent != null)
			{
				m_ObjectToRender.transform.parent = m_ObjectToRenderOrgParent;
			}
			m_ObjectToRender.transform.localPosition = m_ObjectToRenderOrgPosition;
		}
		else
		{
			base.transform.localPosition = m_ObjectToRenderOrgPosition;
		}
	}

	private void CreateTexture()
	{
		if (m_RenderTexture != null)
		{
			return;
		}
		Vector2 size = CalcTextureSize();
		if (m_graphicsManager != null)
		{
			if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.Low)
			{
				size *= 0.75f;
			}
			else if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.Medium)
			{
				size *= 1f;
			}
			else if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.High)
			{
				size *= 1.5f;
			}
		}
		m_RenderTexture = RenderTextureTracker.Get().CreateNewTexture((int)size.x, (int)size.y, RenderTextureTracker.TEXTURE_DEPTH, m_RenderTextureFormat);
		m_RenderTexture.Create();
	}

	private void ReleaseTexture()
	{
		if (RenderTexture.active == m_RenderTexture)
		{
			RenderTexture.active = null;
		}
		if (RenderTexture.active == m_BloomRenderTexture)
		{
			RenderTexture.active = null;
		}
		if (m_RenderTexture != null)
		{
			RenderTextureTracker.Get().DestroyRenderTexture(m_RenderTexture);
			m_RenderTexture = null;
		}
		if (m_BloomRenderTexture != null)
		{
			RenderTextureTracker.Get().DestroyRenderTexture(m_BloomRenderTexture);
			m_BloomRenderTexture = null;
		}
		if (m_BloomRenderBuffer1 != null)
		{
			RenderTextureTracker.Get().DestroyRenderTexture(m_BloomRenderBuffer1);
			m_BloomRenderBuffer1 = null;
		}
		if (m_BloomRenderBuffer2 != null)
		{
			RenderTextureTracker.Get().DestroyRenderTexture(m_BloomRenderBuffer2);
			m_BloomRenderBuffer2 = null;
		}
	}

	private void CreateCamera()
	{
		if (m_CameraGO != null)
		{
			return;
		}
		m_CameraGO = new GameObject(base.name + "_R2TRenderCamera", typeof(Camera));
		m_CameraGO.TryGetComponent<Camera>(out m_Camera);
		if (m_HideRenderObject)
		{
			if (m_RealtimeTranslation)
			{
				m_OffscreenGameObject.transform.position = base.transform.position;
				m_CameraGO.transform.parent = m_OffscreenGameObject.transform;
				m_CameraGO.transform.localPosition = Vector3.zero + m_PositionOffset + m_CameraOffset;
				m_CameraGO.transform.rotation = base.transform.rotation;
				m_OffscreenGameObject.transform.position = m_OffscreenPos;
			}
			else
			{
				m_CameraGO.transform.parent = null;
				m_CameraGO.transform.position = m_OffscreenPos + m_PositionOffset + m_CameraOffset;
				m_CameraGO.transform.rotation = base.transform.rotation;
			}
		}
		else
		{
			m_CameraGO.transform.parent = base.transform;
			m_CameraGO.transform.position = base.transform.position + m_PositionOffset + m_CameraOffset;
			m_CameraGO.transform.rotation = base.transform.rotation;
		}
		m_CameraGO.transform.Rotate(90f, 0f, 0f);
		if (m_FarClip < 0f)
		{
			m_FarClip = 0f;
		}
		if (m_NearClip > 0f)
		{
			m_NearClip = 0f;
		}
		m_Camera.orthographic = true;
		m_Camera.nearClipPlane = m_NearClip * m_WorldScale.y;
		m_Camera.farClipPlane = m_FarClip * m_WorldScale.y;
		m_Camera.clearFlags = CameraClearFlags.Color;
		m_Camera.backgroundColor = m_ClearColor;
		m_Camera.depthTextureMode = DepthTextureMode.None;
		m_Camera.renderingPath = RenderingPath.Forward;
		m_Camera.cullingMask = m_LayerMask;
		m_Camera.allowHDR = false;
		m_Camera.enabled = false;
		m_Camera.targetTexture = m_RenderTexture;
		m_CameraData = default(RenderToTextureUtils.LightWeightCamera);
		m_CameraData.cullingMask = m_LayerMask;
		m_CameraData.backgroundColor = m_ClearColor;
		m_CameraData.aspectRatio = (float)m_RenderTexture.width / (float)m_RenderTexture.height;
	}

	private float OrthoSize()
	{
		if (m_OverrideCameraSize > 0f)
		{
			return m_OverrideCameraSize;
		}
		float orthographicSize = 0f;
		if (m_WorldWidth > m_WorldHeight)
		{
			return Mathf.Min(m_WorldWidth * 0.5f, m_WorldHeight * 0.5f);
		}
		return m_WorldHeight * 0.5f;
	}

	private void CreateAlphaCamera()
	{
		if (!(m_AlphaCameraGO != null))
		{
			m_AlphaCameraGO = new GameObject(base.name + "_R2TAlphaRenderCamera");
			m_AlphaCameraGO.transform.parent = m_CameraGO.transform;
			if ((bool)m_AlphaObjectToRender)
			{
				m_AlphaCameraGO.transform.position = m_CameraGO.transform.position - ALPHA_OBJECT_OFFSET;
			}
			else
			{
				m_AlphaCameraGO.transform.position = m_CameraGO.transform.position;
			}
			m_AlphaCameraGO.transform.localRotation = Quaternion.identity;
			m_AlphaCameraData = new RenderToTextureUtils.LightWeightCamera(m_CameraData);
			m_AlphaCameraData.backgroundColor = Color.clear;
		}
	}

	private void AlphaCameraRender(CommandBuffer cmd, RenderTexture targetTexture, RenderToTextureUtils.LightWeightCamera alphaCamera, RenderCommandLists objectToRender, RenderCommandLists alphaObjectToRender)
	{
		if (m_OpaqueObjectAlphaFill)
		{
			RenderToTextureUtils.RenderCamera(cmd, targetTexture, alphaCamera, objectToRender, m_AlphaFillShader, "RenderType");
		}
		else if (m_AlphaObjectToRender == null)
		{
			string replacementTag = m_AlphaReplacementTag;
			if (replacementTag == string.Empty)
			{
				replacementTag = m_ReplacmentTag;
			}
			RenderToTextureUtils.RenderCamera(cmd, targetTexture, alphaCamera, objectToRender, m_AlphaFillShader, replacementTag);
		}
		else
		{
			RenderToTextureUtils.RenderCamera(cmd, targetTexture, alphaCamera, alphaObjectToRender);
		}
	}

	private void CreateBloomCaptureCamera()
	{
		if (!(m_BloomCaptureCameraGO != null))
		{
			m_BloomCaptureCameraGO = new GameObject(base.name + "_R2TBloomRenderCamera");
			m_BloomCaptureCameraGO.transform.parent = m_CameraGO.transform;
			m_BloomCaptureCameraGO.transform.localPosition = Vector3.zero;
			m_BloomCaptureCameraGO.transform.localRotation = Quaternion.identity;
			m_BloomCameraData = new RenderToTextureUtils.LightWeightCamera(m_CameraData);
		}
	}

	private Vector2 CalcTextureSize()
	{
		Vector2 size = new Vector2(m_Resolution, m_Resolution);
		if (m_WorldWidth > m_WorldHeight)
		{
			size.x = m_Resolution;
			size.y = (float)m_Resolution * (m_WorldHeight / m_WorldWidth);
		}
		else
		{
			size.x = (float)m_Resolution * (m_WorldWidth / m_WorldHeight);
			size.y = m_Resolution;
		}
		return size;
	}

	private void CreateRenderPlane()
	{
		if (m_PlaneGameObject != null)
		{
			Object.DestroyImmediate(m_PlaneGameObject);
		}
		m_PlaneGameObject = CreateMeshPlane($"{base.name}_RenderPlane", m_Material);
		GameObjectUtils.SetHideFlags(m_PlaneGameObject, HideFlags.DontSave);
	}

	private void CreateBloomPlane()
	{
		if (m_BloomPlaneGameObject != null)
		{
			Object.DestroyImmediate(m_BloomPlaneGameObject);
		}
		Material bloomMat = AdditiveMaterial;
		if (m_BloomBlend == BloomBlendType.Transparent)
		{
			bloomMat = TransparentMaterial;
		}
		m_BloomPlaneGameObject = CreateMeshPlane($"{base.name}_BloomRenderPlane", bloomMat);
		m_BloomPlaneGameObject.transform.parent = m_PlaneGameObject.transform;
		m_BloomPlaneGameObject.transform.localPosition = new Vector3(0f, 0.15f, 0f);
		m_BloomPlaneGameObject.transform.localRotation = Quaternion.identity;
		m_BloomPlaneGameObject.transform.localScale = Vector3.one;
		m_BloomPlaneGameObject.GetComponent<Renderer>().GetMaterial().color = m_BloomColor;
	}

	private void CreateBloomCapturePlane()
	{
		if (m_BloomCapturePlaneGameObject != null)
		{
			Object.DestroyImmediate(m_BloomCapturePlaneGameObject);
		}
		Material bloomMat = AdditiveMaterial;
		if (m_BloomBlend == BloomBlendType.Transparent)
		{
			bloomMat = TransparentMaterial;
		}
		m_BloomCapturePlaneGameObject = CreateMeshPlane($"{base.name}_BloomCaptureRenderPlane", bloomMat);
		m_BloomCapturePlaneGameObject.transform.parent = m_BloomCaptureCameraGO.transform;
		m_BloomCapturePlaneGameObject.transform.localPosition = Vector3.zero;
		m_BloomCapturePlaneGameObject.transform.localRotation = Quaternion.identity;
		m_BloomCapturePlaneGameObject.transform.Rotate(-90f, 0f, 0f);
		m_BloomCapturePlaneGameObject.transform.localScale = m_WorldScale;
		if ((bool)m_Material)
		{
			m_BloomCapturePlaneGameObject.GetComponent<Renderer>().SetMaterial(m_PlaneGameObject.GetComponent<Renderer>().GetMaterial());
		}
		if ((bool)m_RenderTexture)
		{
			m_BloomCapturePlaneGameObject.GetComponent<Renderer>().GetMaterial().mainTexture = m_RenderTexture;
		}
	}

	private GameObject CreateMeshPlane(string name, Material material)
	{
		GameObject planeGO = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
		planeGO.transform.parent = base.transform;
		planeGO.transform.localPosition = m_PositionOffset;
		planeGO.transform.localRotation = Quaternion.identity;
		planeGO.transform.localScale = Vector3.one;
		Mesh meshPlane = new Mesh();
		float halfWidth = m_Width * 0.5f;
		float halfHeight = m_Height * 0.5f;
		meshPlane.vertices = new Vector3[4]
		{
			new Vector3(0f - halfWidth, 0f, 0f - halfHeight),
			new Vector3(halfWidth, 0f, 0f - halfHeight),
			new Vector3(0f - halfWidth, 0f, halfHeight),
			new Vector3(halfWidth, 0f, halfHeight)
		};
		meshPlane.uv = PLANE_UVS;
		meshPlane.normals = PLANE_NORMALS;
		meshPlane.triangles = PLANE_TRIANGLES;
		Mesh mesh2 = (planeGO.GetComponent<MeshFilter>().mesh = meshPlane);
		mesh2.RecalculateBounds();
		Renderer planeRenderer = planeGO.GetComponent<Renderer>();
		if ((bool)material)
		{
			planeRenderer.SetMaterial(material);
		}
		planeRenderer.sortingOrder = m_RenderQueue;
		planeRenderer.GetMaterial().renderQueue = m_RenderQueueOffset + m_RenderQueue;
		m_previousRenderQueue = m_RenderQueue;
		return planeGO;
	}

	public void EnablePopupRendering(IPopupRoot popupRoot)
	{
		m_popupRoot = popupRoot;
	}

	public void DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.CleanupPopupRendering(m_popupRenderers);
		}
		m_popupRoot = null;
	}

	public bool HandlesChildPropagation()
	{
		return false;
	}

	private void CalcWorldWidthHeightScale()
	{
		Quaternion currentRot = base.transform.rotation;
		Vector3 currentScale = base.transform.localScale;
		Transform currentParent = base.transform.parent;
		base.transform.rotation = Quaternion.identity;
		bool zeroScale = false;
		if (base.transform.lossyScale.magnitude == 0f)
		{
			base.transform.parent = null;
			base.transform.localScale = Vector3.one;
			zeroScale = true;
		}
		if (m_UniformWorldScale)
		{
			float uniformScale = Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.y, base.transform.lossyScale.z);
			m_WorldScale = new Vector3(uniformScale, uniformScale, uniformScale);
		}
		else
		{
			m_WorldScale = base.transform.lossyScale;
		}
		m_WorldWidth = m_Width * m_WorldScale.x;
		m_WorldHeight = m_Height * m_WorldScale.y;
		if (zeroScale)
		{
			base.transform.parent = currentParent;
			base.transform.localScale = currentScale;
		}
		base.transform.rotation = currentRot;
		if (m_WorldWidth == 0f || m_WorldHeight == 0f)
		{
			Debug.LogError(string.Format(" \"{0}\": RenderToTexture has a world scale of zero. \nm_WorldWidth: {1},   m_WorldHeight: {2}", m_WorldWidth, m_WorldHeight));
		}
	}

	private void CleanUp()
	{
		ReleaseTexture();
		if (m_hasMaterialInstance)
		{
			if (!GetMaterialService().CanIgnoreMaterial(m_Material))
			{
				Object.Destroy(m_Material);
			}
			m_hasMaterialInstance = false;
		}
		if ((bool)m_CameraGO)
		{
			Object.Destroy(m_CameraGO);
		}
		if ((bool)m_AlphaCameraGO)
		{
			Object.Destroy(m_AlphaCameraGO);
		}
		if ((bool)m_PlaneGameObject)
		{
			Object.Destroy(m_PlaneGameObject);
		}
		if ((bool)m_BloomPlaneGameObject)
		{
			Object.Destroy(m_BloomPlaneGameObject);
		}
		if ((bool)m_BloomCaptureCameraGO)
		{
			Object.Destroy(m_BloomCaptureCameraGO);
		}
		if ((bool)m_BloomCapturePlaneGameObject)
		{
			Object.Destroy(m_BloomCapturePlaneGameObject);
		}
		if ((bool)m_OffscreenGameObject)
		{
			Object.Destroy(m_OffscreenGameObject);
		}
		if (m_ObjectToRender != null)
		{
			if (m_ObjectToRenderOrgParent != null)
			{
				m_ObjectToRender.transform.parent = m_ObjectToRenderOrgParent;
			}
			m_ObjectToRender.transform.localPosition = m_ObjectToRenderOrgPosition;
		}
		m_init = false;
		m_isDirty = true;
	}

	private static IMaterialService GetMaterialService()
	{
		if (s_materialService == null)
		{
			s_materialService = ServiceManager.Get<IMaterialService>();
		}
		return s_materialService;
	}
}
