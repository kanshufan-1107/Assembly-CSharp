using System.Collections.Generic;
using System.IO;
using Blizzard.T5.Core;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;
using UnityEngine.Rendering;

public class HighlightRender : MonoBehaviour
{
	private readonly string MULTISAMPLE_SHADER_NAME = "Custom/Selection/HighlightMultiSample";

	private readonly string MULTISAMPLE_BLEND_SHADER_NAME = "Custom/Selection/HighlightMultiSampleBlend";

	private readonly string BLEND_SHADER_NAME = "Custom/Selection/HighlightMaskBlend";

	private readonly string HIGHLIGHT_SHADER_NAME = "Custom/Selection/Highlight";

	private readonly string UNLIT_COLOR_SHADER_NAME = "Custom/UnlitColor";

	private readonly string UNLIT_GREY_SHADER_NAME = "Custom/Unlit/Color/Grey";

	private readonly string UNLIT_LIGHTGREY_SHADER_NAME = "Custom/Unlit/Color/LightGrey";

	private readonly string UNLIT_DARKGREY_SHADER_NAME = "Custom/Unlit/Color/DarkGrey";

	private readonly string UNLIT_BLACK_SHADER_NAME = "Custom/Unlit/Color/BlackOverlay";

	private readonly string UNLIT_WHITE_SHADER_NAME = "Custom/Unlit/Color/White";

	private static Material s_whiteMaterial;

	private static Material s_lightGreyMaterial;

	private static Material s_greyMaterial;

	private static Material s_darkGreyMaterial;

	private static Material s_blurMaterial;

	private static Material s_blurBlendMaterial;

	private static Material s_maskBlendMaterial;

	private const float RENDER_SIZE1 = 0.3f;

	private const float RENDER_SIZE2 = 0.3f;

	private const float RENDER_SIZE3 = 0.5f;

	private const float RENDER_SIZE4 = 0.92f;

	private const float ORTHO_SIZE1 = 0.2f;

	private const float ORTHO_SIZE2 = 0.25f;

	private const float ORTHO_SIZE3 = 0.01f;

	private const float ORTHO_SIZE4 = -0.05f;

	private const float BLUR_BLEND1 = 1.25f;

	private const float BLUR_BLEND2 = 1.25f;

	private const float BLUR_BLEND3 = 1f;

	private const float BLUR_BLEND4 = 1.5f;

	private const int SILHOUETTE_RENDER_SIZE = 256;

	private const int MAX_HIGHLIGHT_EXCLUDE_PARENT_SEARCH = 25;

	private static readonly int s_sid0 = Shader.PropertyToID("temp_id_0");

	private static readonly int s_sid1 = Shader.PropertyToID("temp_id_1");

	private static readonly int s_sid2 = Shader.PropertyToID("temp_id_2");

	private static readonly int s_blurOffsetsId = Shader.PropertyToID("_BlurOffsets");

	private static readonly int s_blendTexId = Shader.PropertyToID("_BlendTex");

	private static List<Renderer> s_cachedRenderers = new List<Renderer>();

	private static List<RenderCommand> s_cachedCommands = new List<RenderCommand>();

	private static List<Material> s_cachedMaterials = new List<Material>();

	public Transform m_RootTransform;

	public float m_SilouetteRenderSize = 1f;

	public float m_SilouetteClipSize = 1f;

	public Renderer m_OverrideMeshRenderer;

	private GameObject m_RenderPlane;

	private float m_RenderScale = 1f;

	private Quaternion m_OrgRotation;

	private Vector3 m_OrgScale;

	private Shader m_MultiSampleShader;

	private Shader m_MultiSampleBlendShader;

	private Shader m_BlendShader;

	private Shader m_HighlightShader;

	private Shader m_UnlitColorShader;

	private Shader m_UnlitGreyShader;

	private Shader m_UnlitLightGreyShader;

	private Shader m_UnlitDarkGreyShader;

	private Shader m_UnlitBlackShader;

	private Shader m_UnlitWhiteShader;

	private RenderTexture m_CameraTexture;

	private float m_CameraOrthoSize;

	private Map<Renderer, bool> m_VisibilityStates;

	private Map<Transform, Vector3> m_ObjectsOrginalPosition;

	private int m_RenderSizeX = 256;

	private int m_RenderSizeY = 256;

	private HighlightRenderOverrides m_renderOverrides;

	private bool m_Initialized;

	public RenderTexture SilhouetteTexture => m_CameraTexture;

	protected void OnDisable()
	{
		if (m_VisibilityStates != null)
		{
			m_VisibilityStates.Clear();
		}
		if (m_CameraTexture != null)
		{
			if (RenderTexture.active == m_CameraTexture)
			{
				RenderTexture.active = null;
			}
			RenderTextureTracker.Get().DestroyRenderTexture(m_CameraTexture);
			m_CameraTexture = null;
		}
		m_Initialized = false;
	}

	protected void Initialize()
	{
		if (m_Initialized)
		{
			return;
		}
		m_Initialized = true;
		if (m_HighlightShader == null)
		{
			m_HighlightShader = ShaderUtils.FindShader(HIGHLIGHT_SHADER_NAME);
		}
		if (!m_HighlightShader)
		{
			Debug.LogError("Failed to load Highlight Shader: " + HIGHLIGHT_SHADER_NAME);
			base.enabled = false;
		}
		GetComponent<Renderer>().GetMaterial().shader = m_HighlightShader;
		if (m_MultiSampleShader == null)
		{
			m_MultiSampleShader = ShaderUtils.FindShader(MULTISAMPLE_SHADER_NAME);
		}
		if (!m_MultiSampleShader)
		{
			Debug.LogError("Failed to load Highlight Shader: " + MULTISAMPLE_SHADER_NAME);
			base.enabled = false;
		}
		if (m_MultiSampleBlendShader == null)
		{
			m_MultiSampleBlendShader = ShaderUtils.FindShader(MULTISAMPLE_BLEND_SHADER_NAME);
		}
		if (!m_MultiSampleBlendShader)
		{
			Debug.LogError("Failed to load Highlight Shader: " + MULTISAMPLE_BLEND_SHADER_NAME);
			base.enabled = false;
		}
		if (m_BlendShader == null)
		{
			m_BlendShader = ShaderUtils.FindShader(BLEND_SHADER_NAME);
		}
		if (!m_BlendShader)
		{
			Debug.LogError("Failed to load Highlight Shader: " + BLEND_SHADER_NAME);
			base.enabled = false;
		}
		if (m_RootTransform == null)
		{
			Transform cardStateMgrTransform = base.transform.parent.parent;
			if ((bool)cardStateMgrTransform.GetComponent<ActorStateMgr>())
			{
				m_RootTransform = cardStateMgrTransform.parent;
			}
			else
			{
				m_RootTransform = cardStateMgrTransform;
			}
			if (m_RootTransform == null)
			{
				Debug.LogError("m_RootTransform is null. Highlighting disabled!");
				base.enabled = false;
			}
		}
		m_VisibilityStates = new Map<Renderer, bool>();
		HighlightSilhouetteInclude[] hsIncludes = m_RootTransform.GetComponentsInChildren<HighlightSilhouetteInclude>();
		if (hsIncludes != null)
		{
			HighlightSilhouetteInclude[] array = hsIncludes;
			for (int i = 0; i < array.Length; i++)
			{
				Renderer renderComp = array[i].gameObject.GetComponent<Renderer>();
				if (!(renderComp == null))
				{
					m_VisibilityStates.Add(renderComp, value: false);
				}
			}
		}
		m_UnlitColorShader = ShaderUtils.FindShader(UNLIT_COLOR_SHADER_NAME);
		if (!m_UnlitColorShader)
		{
			Debug.LogError("Failed to load Highlight Rendering Shader: " + UNLIT_COLOR_SHADER_NAME);
		}
		m_UnlitGreyShader = ShaderUtils.FindShader(UNLIT_GREY_SHADER_NAME);
		if (!m_UnlitGreyShader)
		{
			Debug.LogError("Failed to load Highlight Rendering Shader: " + UNLIT_GREY_SHADER_NAME);
		}
		m_UnlitLightGreyShader = ShaderUtils.FindShader(UNLIT_LIGHTGREY_SHADER_NAME);
		if (!m_UnlitLightGreyShader)
		{
			Debug.LogError("Failed to load Highlight Rendering Shader: " + UNLIT_LIGHTGREY_SHADER_NAME);
		}
		m_UnlitDarkGreyShader = ShaderUtils.FindShader(UNLIT_DARKGREY_SHADER_NAME);
		if (!m_UnlitDarkGreyShader)
		{
			Debug.LogError("Failed to load Highlight Rendering Shader: " + UNLIT_DARKGREY_SHADER_NAME);
		}
		m_UnlitBlackShader = ShaderUtils.FindShader(UNLIT_BLACK_SHADER_NAME);
		if (!m_UnlitBlackShader)
		{
			Debug.LogError("Failed to load Highlight Rendering Shader: " + UNLIT_BLACK_SHADER_NAME);
		}
		m_UnlitWhiteShader = ShaderUtils.FindShader(UNLIT_WHITE_SHADER_NAME);
		if (!m_UnlitWhiteShader)
		{
			Debug.LogError("Failed to load Highlight Rendering Shader: " + UNLIT_WHITE_SHADER_NAME);
		}
		if (s_whiteMaterial == null)
		{
			s_whiteMaterial = new Material(m_UnlitWhiteShader);
			s_lightGreyMaterial = new Material(m_UnlitLightGreyShader);
			s_greyMaterial = new Material(m_UnlitGreyShader);
			s_darkGreyMaterial = new Material(m_UnlitDarkGreyShader);
			s_blurMaterial = new Material(m_MultiSampleShader);
			s_blurBlendMaterial = new Material(m_MultiSampleBlendShader);
			s_maskBlendMaterial = new Material(m_BlendShader);
		}
	}

	protected void Update()
	{
		if ((bool)m_CameraTexture && m_Initialized && !m_CameraTexture.IsCreated())
		{
			CreateSilhouetteTexture();
		}
	}

	[ContextMenu("Export Silhouette Texture")]
	public void ExportSilhouetteTexture()
	{
		RenderTexture.active = m_CameraTexture;
		Texture2D highlightSilhouetteTexture = new Texture2D(m_RenderSizeX, m_RenderSizeY, TextureFormat.RGB24, mipChain: false);
		highlightSilhouetteTexture.ReadPixels(new Rect(0f, 0f, m_RenderSizeX, m_RenderSizeY), 0, 0, recalculateMipMaps: false);
		highlightSilhouetteTexture.Apply();
		string filePath = Application.dataPath + "/SilhouetteTexture.png";
		File.WriteAllBytes(filePath, highlightSilhouetteTexture.EncodeToPNG());
		RenderTexture.active = null;
		Debug.Log($"Silhouette Texture Created: {filePath}");
	}

	private static void DrawRenderers(CommandBuffer cmd, Material material)
	{
		foreach (RenderCommand command in s_cachedCommands)
		{
			cmd.DrawRenderer(command.Renderer, material, command.MeshIndex);
		}
	}

	private static void BlendBlit(CommandBuffer cmd, RenderTargetIdentifier src, RenderTargetIdentifier blend, RenderTargetIdentifier dst, float blur, Material material)
	{
		cmd.SetGlobalTexture(s_blendTexId, blend);
		cmd.SetGlobalVector(s_blurOffsetsId, Vector4.one * (0f - blur));
		cmd.Blit(src, dst, material);
	}

	private void SetProjectionMatrix(CommandBuffer cmd, float orthoSize)
	{
		Matrix4x4 proj = Matrix4x4.Ortho(0f - orthoSize, orthoSize, 0f - orthoSize, orthoSize, 0f - m_RenderScale + 1f, m_RenderScale + 1f);
		cmd.SetProjectionMatrix(proj);
	}

	private void InitializeOverrideMeshRenderer()
	{
		Transform silhouetteParent = m_RootTransform;
		HighlightOverrideSilhouetteMeshParent highlightSilhouetteParent = m_RootTransform.gameObject.GetComponentInChildren<HighlightOverrideSilhouetteMeshParent>();
		if (highlightSilhouetteParent != null)
		{
			silhouetteParent = highlightSilhouetteParent.transform;
		}
		GameObject newGo = new GameObject("HighlightMesh");
		newGo.transform.SetParent(silhouetteParent.transform, worldPositionStays: false);
		m_OverrideMeshRenderer = newGo.AddComponent<MeshRenderer>();
		m_OverrideMeshRenderer.enabled = false;
		newGo.AddComponent<MeshFilter>().SetMesh(m_renderOverrides.OverrideSilouetteMesh);
		m_OverrideMeshRenderer.SetSharedMaterials(m_renderOverrides.OverrideSilouetteMeshMaterials);
	}

	public void CreateSilhouetteTexture(bool force = false)
	{
		Initialize();
		if (!VisibilityStatesChanged() && !force)
		{
			return;
		}
		SetupRenderObjects();
		if (m_RenderPlane == null || m_RenderSizeX < 1 || m_RenderSizeY < 1)
		{
			return;
		}
		Renderer renderPlaneRenderer = GetComponent<Renderer>();
		bool renderPlaneState = renderPlaneRenderer.enabled;
		renderPlaneRenderer.enabled = false;
		if (m_OverrideMeshRenderer == null && m_renderOverrides != null && m_renderOverrides.OverrideSilouetteMesh != null)
		{
			InitializeOverrideMeshRenderer();
		}
		if (m_OverrideMeshRenderer != null)
		{
			m_OverrideMeshRenderer.transform.localRotation = Quaternion.Euler(m_renderOverrides.OverrideSilouetteMeshOrientation);
			m_OverrideMeshRenderer.transform.localScale = new Vector3(m_renderOverrides.OverrideSilouetteScale, m_renderOverrides.OverrideSilouetteScale, m_renderOverrides.OverrideSilouetteScale);
			s_cachedRenderers.Add(m_OverrideMeshRenderer);
		}
		else
		{
			m_RootTransform.GetComponentsInChildren(s_cachedRenderers);
		}
		foreach (Renderer renderer in s_cachedRenderers)
		{
			if (!renderer.enabled && !m_OverrideMeshRenderer)
			{
				continue;
			}
			MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
			if (meshFilter == null || meshFilter.sharedMesh == null)
			{
				continue;
			}
			renderer.GetSharedMaterials(s_cachedMaterials);
			int meshCount = Mathf.Min(meshFilter.sharedMesh.subMeshCount, s_cachedMaterials.Count);
			for (int meshIndex = 0; meshIndex < meshCount; meshIndex++)
			{
				Material material = s_cachedMaterials[meshIndex];
				if (material != null && material.GetTag("Highlight", searchFallbacks: false) != "")
				{
					s_cachedCommands.Add(new RenderCommand
					{
						Renderer = renderer,
						MeshIndex = meshIndex
					});
				}
			}
		}
		s_cachedRenderers.Clear();
		s_cachedMaterials.Clear();
		RenderTargetIdentifier rt0 = new RenderTargetIdentifier(s_sid0);
		RenderTargetIdentifier rt1 = new RenderTargetIdentifier(s_sid1);
		RenderTargetIdentifier rt2 = new RenderTargetIdentifier(s_sid2);
		CommandBuffer cmd = CommandBufferPool.Get("Create Silhouette Texture");
		int rsx = m_RenderSizeX;
		int rsy = m_RenderSizeY;
		FilterMode mode = FilterMode.Bilinear;
		RenderTextureFormat format = RenderTextureFormat.R8;
		int depth = RenderTextureTracker.TEXTURE_DEPTH;
		m_CameraTexture.DiscardContents();
		Transform t = m_RenderPlane.transform;
		Vector3 siloutteRenderOffset = (m_renderOverrides ? m_renderOverrides.SilouetteRenderOffset : Vector3.zero);
		Matrix4x4 view = Matrix4x4.TRS(t.position + siloutteRenderOffset, t.rotation * Quaternion.Euler(90f, 180f, 0f), new Vector3(1f, 1f, -1f)).inverse;
		cmd.SetViewMatrix(view);
		float silouetteRenderSize = (m_renderOverrides ? m_renderOverrides.SilouetteRenderSize : m_SilouetteRenderSize);
		float silouetteClipSize = (m_renderOverrides ? m_renderOverrides.SilouetteClipSize : m_SilouetteClipSize);
		cmd.GetTemporaryRT(s_sid0, (int)((float)rsx * 0.3f), (int)((float)rsy * 0.3f), depth, mode, format);
		cmd.SetRenderTarget(rt0);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		SetProjectionMatrix(cmd, m_CameraOrthoSize - 0.2f * silouetteRenderSize);
		DrawRenderers(cmd, s_darkGreyMaterial);
		cmd.GetTemporaryRT(s_sid1, (int)((float)rsx * 0.3f), (int)((float)rsy * 0.3f), depth, mode, format);
		cmd.SetRenderTarget(rt1);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		SetProjectionMatrix(cmd, m_CameraOrthoSize - 0.25f * silouetteRenderSize);
		DrawRenderers(cmd, s_greyMaterial);
		cmd.GetTemporaryRT(s_sid2, (int)((float)rsx * 0.3f), (int)((float)rsy * 0.3f), 0, mode, format);
		cmd.SetRenderTarget(rt2);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		BlendBlit(cmd, rt0, rt1, rt2, 1.25f, s_blurBlendMaterial);
		cmd.ReleaseTemporaryRT(s_sid0);
		cmd.ReleaseTemporaryRT(s_sid1);
		cmd.GetTemporaryRT(s_sid0, (int)((float)rsx * 0.5f), (int)((float)rsy * 0.5f), depth, mode, format);
		cmd.SetRenderTarget(rt0);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		SetProjectionMatrix(cmd, m_CameraOrthoSize - 0.01f * silouetteRenderSize);
		DrawRenderers(cmd, s_lightGreyMaterial);
		cmd.GetTemporaryRT(s_sid1, (int)((float)rsx * 0.5f), (int)((float)rsy * 0.5f), 0, mode, format);
		cmd.SetRenderTarget(rt1);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		BlendBlit(cmd, rt2, rt0, rt1, 1.25f, s_blurBlendMaterial);
		cmd.ReleaseTemporaryRT(s_sid2);
		cmd.ReleaseTemporaryRT(s_sid0);
		cmd.GetTemporaryRT(s_sid0, rsx, rsy, depth, mode, format);
		cmd.SetRenderTarget(rt0);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		SetProjectionMatrix(cmd, m_CameraOrthoSize - -0.05f * silouetteRenderSize);
		DrawRenderers(cmd, s_lightGreyMaterial);
		cmd.GetTemporaryRT(s_sid2, rsx, rsy, 0, mode, format);
		cmd.SetRenderTarget(rt2);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		BlendBlit(cmd, rt1, rt0, rt2, 1f, s_blurBlendMaterial);
		cmd.ReleaseTemporaryRT(s_sid1);
		cmd.ReleaseTemporaryRT(s_sid0);
		cmd.SetGlobalVector(s_blurOffsetsId, Vector4.one * -1.5f);
		cmd.GetTemporaryRT(s_sid0, rsx, rsy, 0, mode, format);
		cmd.SetRenderTarget(rt0);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		cmd.Blit(rt2, rt0, s_blurMaterial);
		cmd.ReleaseTemporaryRT(s_sid2);
		cmd.GetTemporaryRT(s_sid1, (int)((float)rsx * 0.92f), (int)((float)rsy * 0.92f), depth, mode, format);
		cmd.SetRenderTarget(rt1);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		SetProjectionMatrix(cmd, m_CameraOrthoSize + 0.1f * silouetteClipSize);
		DrawRenderers(cmd, s_whiteMaterial);
		BlendBlit(cmd, rt0, rt1, m_CameraTexture, 0.8f, s_maskBlendMaterial);
		Graphics.ExecuteCommandBuffer(cmd);
		cmd.ReleaseTemporaryRT(s_sid0);
		cmd.ReleaseTemporaryRT(s_sid1);
		CommandBufferPool.Release(cmd);
		renderPlaneRenderer.enabled = renderPlaneState;
		RestoreRenderObjects();
		s_cachedCommands.Clear();
	}

	public bool isTextureCreated()
	{
		if ((bool)m_CameraTexture)
		{
			return m_CameraTexture.IsCreated();
		}
		return false;
	}

	private void SetupRenderObjects()
	{
		if (m_RootTransform == null)
		{
			m_RenderPlane = null;
			return;
		}
		m_OrgRotation = m_RootTransform.rotation;
		m_OrgScale = m_RootTransform.localScale;
		SetWorldScale(m_RootTransform, Vector3.one);
		m_RootTransform.rotation = Quaternion.identity;
		Bounds renderBounds = GetComponent<Renderer>().bounds;
		float width = renderBounds.size.x;
		float height = renderBounds.size.z;
		if (height < renderBounds.size.y)
		{
			height = renderBounds.size.y;
		}
		if (width > height)
		{
			m_RenderSizeX = 256;
			m_RenderSizeY = (int)(256f * (height / width));
		}
		else
		{
			m_RenderSizeX = (int)(256f * (width / height));
			m_RenderSizeY = 256;
		}
		m_CameraOrthoSize = height * 0.5f;
		if (m_CameraTexture == null)
		{
			if (m_RenderSizeX < 1 || m_RenderSizeY < 1)
			{
				m_RenderSizeX = 256;
				m_RenderSizeY = 256;
			}
			m_CameraTexture = RenderTextureTracker.Get().CreateNewTexture(m_RenderSizeX, m_RenderSizeY, RenderTextureTracker.TEXTURE_DEPTH, RenderTextureFormat.R8);
		}
		HighlightState hlsc = m_RootTransform.GetComponentInChildren<HighlightState>();
		if (hlsc == null)
		{
			Debug.LogError("Can not find Highlight(HighlightState component) object for selection highlighting.");
			m_RenderPlane = null;
			return;
		}
		hlsc.transform.localPosition = Vector3.zero;
		HighlightRender hlr = m_RootTransform.GetComponentInChildren<HighlightRender>();
		if (hlr == null)
		{
			Debug.LogError("Can not find render plane object(HighlightRender component) for selection highlighting.");
			m_RenderPlane = null;
		}
		else
		{
			m_RenderPlane = hlr.gameObject;
			m_RenderScale = GetWorldScale(m_RenderPlane.transform).x;
		}
	}

	private void RestoreRenderObjects()
	{
		m_RootTransform.rotation = m_OrgRotation;
		m_RootTransform.localScale = m_OrgScale;
		m_RenderPlane = null;
	}

	private bool VisibilityStatesChanged()
	{
		bool changed = false;
		HighlightSilhouetteInclude[] componentsInChildren = m_RootTransform.GetComponentsInChildren<HighlightSilhouetteInclude>();
		List<Renderer> hiRenders = new List<Renderer>();
		HighlightSilhouetteInclude[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Renderer r = array[i].gameObject.GetComponent<Renderer>();
			if (r != null)
			{
				hiRenders.Add(r);
			}
		}
		foreach (Renderer hir in hiRenders)
		{
			bool visible = hir.enabled && hir.gameObject.activeInHierarchy;
			if (!m_VisibilityStates.ContainsKey(hir))
			{
				m_VisibilityStates.Add(hir, visible);
				if (visible)
				{
					changed = true;
				}
			}
			else if (m_VisibilityStates[hir] != visible)
			{
				m_VisibilityStates[hir] = visible;
				changed = true;
			}
		}
		return changed;
	}

	public static Vector3 GetWorldScale(Transform transform)
	{
		Vector3 worldScale = transform.localScale;
		Transform parent = transform.parent;
		while (parent != null)
		{
			worldScale = Vector3.Scale(worldScale, parent.localScale);
			parent = parent.parent;
		}
		return worldScale;
	}

	public void SetWorldScale(Transform xform, Vector3 scale)
	{
		GameObject obj = new GameObject();
		Transform tempXform = obj.transform;
		tempXform.parent = null;
		tempXform.localRotation = Quaternion.identity;
		tempXform.localScale = Vector3.one;
		Transform orgParent = xform.parent;
		xform.parent = tempXform;
		xform.localScale = scale;
		xform.parent = orgParent;
		Object.Destroy(obj);
	}

	public void SetRenderOverrides(HighlightRenderOverrides renderOverrides)
	{
		m_renderOverrides = renderOverrides;
	}
}
