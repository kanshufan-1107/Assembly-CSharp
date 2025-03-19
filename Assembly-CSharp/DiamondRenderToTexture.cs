using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using UnityEngine;

public class DiamondRenderToTexture : MonoBehaviour
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

	public struct TransformData
	{
		public Vector3 position;

		public Vector3 localScale;

		public Quaternion rotation;

		public Vector3 up;

		public Vector3 forward;

		public int layer;

		public Transform objectParent;

		public Transform atlasedComponentParent;
	}

	private static readonly Vector3 ALPHA_OBJECT_OFFSET = new Vector3(0f, 1000f, 0f);

	private static readonly Color GIZMOS_COLOR = new Color(1f, 1f, 0f, 0.8f);

	public GameObject m_ObjectToRender;

	public GameObject m_AlphaObjectToRender;

	public bool m_AllowRepetition;

	public bool m_HideRenderObject = true;

	public bool m_RealtimeRender;

	public bool m_RealtimeTranslation;

	public bool m_OpaqueObjectAlphaFill;

	public RenderToTextureMaterial m_RenderMaterial;

	public Material m_Material;

	public bool m_CreateRenderPlane;

	public Color m_ClearColor = Color.clear;

	public GameObject m_RenderToObject;

	[Range(1f, 2048f)]
	public int m_Resolution = 128;

	public Vector3 m_bounds = Vector3.one;

	public bool m_UniformWorldScale;

	public Vector3 m_PositionOffset = Vector3.zero;

	private const string TRANSPARENT_SHADER_NAME = "R2TTransparent";

	private Shader m_TransparentShader;

	private Material m_TransparentMaterial;

	private bool m_isRegisteredToManager;

	private bool m_isDirty;

	private DiamondRenderToTextureService m_diamondRenderToTextureService;

	private Vector3 m_worldSize;

	private Vector3 m_worldScale;

	private TransformData m_transformSnapshot;

	private Bounds m_renderBounds = new Bounds(Vector3.zero, Vector3.zero);

	private Renderer m_outputRenderer;

	private TransformData m_atlasPositionSnapshot;

	private Transform m_selfOriginalParent;

	private Transform m_objectToRenderOriginalParent;

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

	public GameObject OffscreenGameObject { get; private set; }

	public Vector2Int TextureSize { get; private set; }

	public Bounds RendererBounds => m_renderBounds;

	public Vector3 PivotPosition => m_PositionOffset - Vector3.Scale(new Vector3(-1f, 1f, 1f), m_bounds / 2f);

	public Vector3 WorldPivotOffset => base.transform.TransformPoint(PivotPosition) - base.transform.position;

	public TransformData TransformSnapshot => m_transformSnapshot;

	public bool HasAtlasPosition { get; set; }

	public Vector3 WorldBounds => m_worldSize;

	public Vector3 ObjectToRenderOffset { get; private set; }

	public RenderCommandLists RenderCommands { get; private set; }

	private void Awake()
	{
		FetchOutputRenderer();
	}

	private void Start()
	{
		m_diamondRenderToTextureService = ServiceManager.Get<DiamondRenderToTextureService>();
		if (!m_ObjectToRender)
		{
			m_isDirty = true;
			return;
		}
		if (m_HideRenderObject)
		{
			m_ObjectToRender.SetActive(value: false);
		}
		FetchObjectRequiredData();
		RegisterToService();
	}

	private void Update()
	{
		if (base.transform.hasChanged)
		{
			m_isDirty = true;
		}
		if (m_isDirty)
		{
			FetchObjectRequiredData();
		}
		if (!m_isRegisteredToManager)
		{
			RegisterToService();
		}
	}

	private void OnValidate()
	{
		CalcWorldWidthHeightScale();
		m_isDirty = true;
	}

	private void OnDisable()
	{
		UnregisterFromService();
		ReleaseRenderCommands();
	}

	private void OnEnable()
	{
		FetchObjectRequiredData();
		RegisterToService();
	}

	private void OnDestroy()
	{
		UnregisterFromService();
		ReleaseRenderCommands();
	}

	private void OnDrawGizmosSelected()
	{
		if (base.enabled && (bool)m_ObjectToRender)
		{
			Gizmos.matrix = Matrix4x4.TRS(m_ObjectToRender.transform.position, base.transform.rotation, base.transform.lossyScale);
			Gizmos.color = GIZMOS_COLOR;
			Gizmos.DrawSphere(m_PositionOffset, 0.1f);
			Gizmos.DrawWireCube(m_PositionOffset, m_bounds);
			Gizmos.DrawSphere(PivotPosition, 0.1f);
			Vector3 pos = m_PositionOffset + new Vector3(0f, m_bounds.y / 2f, 0f);
			GizmosDrawArrow(pos, Vector3.forward, Color.blue);
			GizmosDrawArrow(pos, Vector3.up, Color.green);
			Gizmos.matrix = Matrix4x4.identity;
		}
	}

	public bool IsEqual(DiamondRenderToTexture other)
	{
		if (other.m_ObjectToRender.GetInstanceID() != m_ObjectToRender.GetInstanceID())
		{
			return false;
		}
		return true;
	}

	public void OnAddedToAtlas(RenderTexture atlasTexture, Rect atlasUV)
	{
		UpdatePlaneUVS(atlasUV);
		UpdateMaterial(atlasTexture);
	}

	public void PushTransform()
	{
		Transform objectTransform = m_ObjectToRender.transform;
		m_transformSnapshot.position = objectTransform.position;
		m_transformSnapshot.localScale = objectTransform.localScale;
		m_transformSnapshot.rotation = objectTransform.rotation;
		m_transformSnapshot.layer = m_ObjectToRender.layer;
		m_transformSnapshot.up = base.transform.up;
		m_transformSnapshot.forward = base.transform.forward;
		m_transformSnapshot.objectParent = objectTransform.parent;
		m_transformSnapshot.atlasedComponentParent = base.transform.parent;
	}

	public void ResetTransform(Vector3 position)
	{
		Transform obj = m_ObjectToRender.transform;
		obj.parent = null;
		obj.localScale = Vector3.one;
		obj.position = position;
		Transform obj2 = base.transform;
		obj2.parent = null;
		obj2.localScale = Vector3.one;
		obj2.position = position;
		CalcWorldWidthHeightScale();
	}

	public void RestoreParents()
	{
		m_ObjectToRender.transform.parent = m_transformSnapshot.objectParent;
		base.transform.parent = m_transformSnapshot.atlasedComponentParent;
	}

	public void PopTransform()
	{
		Transform obj = m_ObjectToRender.transform;
		obj.position = TransformSnapshot.position;
		obj.localScale = TransformSnapshot.localScale;
		obj.rotation = TransformSnapshot.rotation;
		m_ObjectToRender.layer = m_transformSnapshot.layer;
		base.transform.up = m_transformSnapshot.up;
		base.transform.forward = m_transformSnapshot.forward;
	}

	public void Refresh()
	{
		m_isDirty = true;
	}

	public void CaptureAtlasPosition()
	{
		HasAtlasPosition = true;
		Transform selfTransform = base.transform;
		Transform objectTransform = m_ObjectToRender.transform;
		m_atlasPositionSnapshot.position = objectTransform.position;
		m_atlasPositionSnapshot.localScale = objectTransform.localScale;
		m_atlasPositionSnapshot.rotation = objectTransform.rotation;
		m_atlasPositionSnapshot.up = selfTransform.up;
		m_atlasPositionSnapshot.forward = selfTransform.forward;
	}

	public bool MaintainsAtlasPosition()
	{
		Transform objectTransform = m_ObjectToRender.transform;
		if (!objectTransform.hasChanged)
		{
			return true;
		}
		bool num = m_atlasPositionSnapshot.position == objectTransform.position;
		bool hasSameScale = m_atlasPositionSnapshot.localScale == objectTransform.localScale;
		bool hasSameRotation = m_atlasPositionSnapshot.rotation == objectTransform.rotation;
		return num && hasSameScale && hasSameRotation;
	}

	public void RestoreAtlasPosition()
	{
		Transform obj = m_ObjectToRender.transform;
		obj.position = m_atlasPositionSnapshot.position;
		obj.localScale = m_atlasPositionSnapshot.localScale;
		obj.rotation = m_atlasPositionSnapshot.rotation;
		base.transform.position = m_atlasPositionSnapshot.position;
		base.transform.localScale = m_atlasPositionSnapshot.localScale;
		base.transform.rotation = Quaternion.LookRotation(m_atlasPositionSnapshot.forward, m_atlasPositionSnapshot.up);
	}

	public void RestoreOriginalParents()
	{
		if ((bool)m_objectToRenderOriginalParent && (bool)m_ObjectToRender)
		{
			m_ObjectToRender.transform.parent = m_objectToRenderOriginalParent;
		}
		if ((bool)m_selfOriginalParent && (bool)base.transform)
		{
			base.transform.parent = m_selfOriginalParent;
		}
	}

	private void FetchObjectRequiredData()
	{
		if ((bool)m_ObjectToRender)
		{
			CaptureOriginalParents();
			FetchOutputRenderer();
			CalculateObjectToRenderOffset();
			CalcTextureSize();
			Renderer[] capturedRenderers = m_ObjectToRender.GetComponentsInChildren<Renderer>(includeInactive: true);
			m_renderBounds = RenderToTextureUtils.CalcRendererBounds(capturedRenderers);
			if (RenderCommands == null)
			{
				RenderCommands = RenderToTextureUtils.RenderCommandListPool.Get(capturedRenderers);
			}
			else
			{
				RenderCommands.Clear();
				RenderCommands.AppendRenderCommands(capturedRenderers);
			}
			HasAtlasPosition = false;
			m_isDirty = false;
		}
	}

	private void ReleaseRenderCommands()
	{
		if (RenderCommands != null)
		{
			RenderToTextureUtils.RenderCommandListPool.Release(RenderCommands);
			RenderCommands = null;
		}
	}

	private void SetupAuxRenderObjects()
	{
		if (!m_ObjectToRender)
		{
			return;
		}
		if (m_RealtimeTranslation)
		{
			OffscreenGameObject = new GameObject("R2TOffsetRenderRoot_" + base.name);
			OffscreenGameObject.transform.position = base.transform.position;
			m_ObjectToRender.transform.SetParent(OffscreenGameObject.transform);
		}
		if (m_HideRenderObject)
		{
			if (m_RealtimeTranslation && (bool)m_AlphaObjectToRender)
			{
				m_AlphaObjectToRender.transform.SetParent(OffscreenGameObject.transform);
			}
			if ((bool)m_AlphaObjectToRender)
			{
				m_AlphaObjectToRender.transform.position = base.transform.position - ALPHA_OBJECT_OFFSET;
			}
		}
	}

	private void CalcWorldWidthHeightScale()
	{
		Transform obj = base.transform;
		Quaternion currentRot = obj.rotation;
		Vector3 currentScale = obj.localScale;
		Transform currentParent = obj.parent;
		obj.rotation = Quaternion.identity;
		Vector3 lossyScale = obj.lossyScale;
		bool zeroScale = false;
		if (lossyScale.magnitude == 0f)
		{
			base.transform.parent = null;
			base.transform.localScale = Vector3.one;
			zeroScale = true;
		}
		if (m_UniformWorldScale)
		{
			float uniformScale = Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z);
			m_worldScale = new Vector3(uniformScale, uniformScale, uniformScale);
		}
		else
		{
			m_worldScale = lossyScale;
		}
		m_worldSize = new Vector3(m_bounds.x * m_worldScale.x, m_bounds.y * m_worldScale.y, m_bounds.z * m_worldScale.z);
		if (zeroScale)
		{
			base.transform.parent = currentParent;
			base.transform.localScale = currentScale;
		}
		base.transform.rotation = currentRot;
		if (m_worldSize.x == 0f || m_worldSize.y == 0f)
		{
			Debug.LogError(string.Format(" \"{0}\": RenderToTexture has a world scale of zero. \nm_WorldWidth: {1},   m_WorldHeight: {2}", m_worldSize.x, m_worldSize.y));
		}
	}

	private void CalcTextureSize()
	{
		float aspectRatio = m_bounds.y / m_bounds.x;
		TextureSize = new Vector2Int(m_Resolution, Mathf.RoundToInt((float)m_Resolution * aspectRatio));
	}

	private void CalculateObjectToRenderOffset()
	{
		Vector3 renderOffset = base.transform.position - m_ObjectToRender.transform.position;
		renderOffset.z = 0f;
		ObjectToRenderOffset = renderOffset;
	}

	private void FetchOutputRenderer()
	{
		if ((bool)m_RenderToObject && !m_outputRenderer)
		{
			m_outputRenderer = m_RenderToObject.GetComponent<Renderer>();
			if (!m_outputRenderer)
			{
				Debug.LogError("RenderToObject should have a renderer!");
			}
			else
			{
				m_outputRenderer.enabled = false;
			}
		}
	}

	private void CaptureOriginalParents()
	{
		if ((bool)m_ObjectToRender && !m_objectToRenderOriginalParent)
		{
			m_objectToRenderOriginalParent = m_ObjectToRender.transform.parent;
		}
		if (!m_selfOriginalParent)
		{
			m_selfOriginalParent = base.transform.parent;
		}
	}

	private void RegisterToService()
	{
		if (!m_isRegisteredToManager && m_diamondRenderToTextureService != null && (bool)m_ObjectToRender && (bool)m_outputRenderer)
		{
			bool isRegistered = m_diamondRenderToTextureService.Register(this);
			if (isRegistered)
			{
				SetupAuxRenderObjects();
			}
			m_isRegisteredToManager = isRegistered;
		}
	}

	private void UnregisterFromService()
	{
		if (m_isRegisteredToManager)
		{
			m_diamondRenderToTextureService.Unregister(this);
			m_isRegisteredToManager = false;
		}
	}

	private void UpdatePlaneUVS(Rect atlasUV)
	{
		if ((bool)m_RenderToObject)
		{
			Mesh renderMesh = m_RenderToObject.GetComponent<MeshFilter>().mesh;
			Vector2[] meshUv = renderMesh.uv;
			Rect uvBounds = GetCurrentUVBounds(meshUv);
			Vector2 scaleFactor = new Vector2(atlasUV.width / uvBounds.width, atlasUV.height / uvBounds.height);
			Vector2 translateFactor = new Vector2(atlasUV.xMin - uvBounds.xMin, atlasUV.yMin - uvBounds.yMin);
			for (int i = 0; i < meshUv.Length; i++)
			{
				Vector2 uvCoordinate = meshUv[i];
				uvCoordinate.x = uvCoordinate.x * scaleFactor.x + translateFactor.x;
				uvCoordinate.y = uvCoordinate.y * scaleFactor.y + translateFactor.y;
				meshUv[i] = uvCoordinate;
			}
			renderMesh.uv = meshUv;
		}
	}

	private Rect GetCurrentUVBounds(Vector2[] currentUv)
	{
		Vector2 minUv = Vector2.one;
		Vector2 maxUv = Vector2.zero;
		for (int i = 0; i < currentUv.Length; i++)
		{
			Vector2 uv = currentUv[i];
			if (uv.x < minUv.x)
			{
				minUv.x = uv.x;
			}
			if (uv.y < minUv.y)
			{
				minUv.y = uv.y;
			}
			if (uv.x > maxUv.x)
			{
				maxUv.x = uv.x;
			}
			if (uv.y > maxUv.y)
			{
				maxUv.y = uv.y;
			}
		}
		return new Rect(minUv.x, minUv.y, maxUv.x - minUv.x, maxUv.y - minUv.y);
	}

	private void UpdateMaterial(RenderTexture atlasTexture)
	{
		if ((bool)m_outputRenderer)
		{
			if (m_RenderMaterial == RenderToTextureMaterial.Transparent)
			{
				TransparentMaterial.mainTexture = atlasTexture;
				m_outputRenderer.SetMaterial(TransparentMaterial);
				m_outputRenderer.enabled = true;
			}
			else
			{
				m_outputRenderer.GetMaterial().mainTexture = atlasTexture;
			}
		}
	}

	public void UpdateMaterialBlend(bool inPlay)
	{
		UpdateMaterialBlend(inPlay ? 1f : 0f);
	}

	public void UpdateMaterialBlend(float blendValue)
	{
		TransparentMaterial.SetFloat("_LightingBlend", blendValue);
	}

	private static void GizmosDrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20f)
	{
		Gizmos.color = color;
		Gizmos.DrawRay(pos, direction);
		Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
		Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
		Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
		Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
	}
}
