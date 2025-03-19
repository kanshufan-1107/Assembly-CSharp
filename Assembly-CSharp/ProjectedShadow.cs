using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class ProjectedShadow : MonoBehaviour
{
	private const int RENDER_SIZE = 64;

	private const string CONTACT_SHADER_NAME = "Custom/ContactShadow";

	private const string UNLIT_WHITE_SHADER_NAME = "Custom/Unlit/Color/White";

	private const string UNLIT_DARKGREY_SHADER_NAME = "Custom/Unlit/Color/DarkGrey";

	private const string MULTISAMPLE_SHADER_NAME = "Custom/Selection/HighlightMultiSample";

	private const float NEARCLIP_PLANE = 0f;

	private const float SHADOW_OFFSET_SCALE = 0.3f;

	private const float RENDERMASK_OFFSET = 0.11f;

	private const float RENDERMASK_BLUR = 0.6f;

	private const float RENDERMASK_BLUR2 = 0.8f;

	private const float CONTACT_SHADOW_SCALE = 0.98f;

	private const float CONTACT_SHADOW_FADE_IN_HEIGHT = 0.08f;

	private const float CONTACT_SHADOW_INTENSITY = 3.5f;

	private const float REDRAW_SHADOW_SCALE_DIFFERENCE = 0.1f;

	private static CommandBuffer s_commandBuffer;

	private static RenderTexture s_tempBuffer;

	private static readonly int BLUR_OFFSETS_ID = Shader.PropertyToID("_BlurOffsets");

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

	public float m_ShadowProjectorSize = 1.5f;

	public bool m_ShadowEnabled;

	public bool m_AutoBoardHeightDisable;

	public float m_AutoDisableHeight;

	public float m_ProjectionFarClip = 10f;

	public Vector3 m_ProjectionOffset;

	public bool m_ContactShadow;

	public Vector3 m_ContactOffset = Vector3.zero;

	public bool m_isDirtyContactShadow = true;

	public bool m_enabledAlongsideRealtimeShadows;

	private static float s_offset = -12000f;

	private static Color s_ShadowColor = new Color(0.098f, 0.098f, 0.235f, 0.45f);

	private GameObject m_RootObject;

	private GameObject m_ProjectorGameObject;

	private Transform m_ProjectorTransform;

	private DecalProjector m_Projector;

	private RenderTexture m_ShadowTexture;

	private RenderTexture m_ContactShadowTexture;

	private float m_AdjustedShadowProjectorSize = 1.5f;

	private float m_BoardHeight = 0.2f;

	private bool m_HasBoardHeight;

	private Mesh m_PlaneMesh;

	private GameObject m_PlaneGameObject;

	private CancellationTokenSource m_LoadTokenSource;

	private IGraphicsManager m_graphicsManager;

	private List<MeshRenderer> m_objectsToRender;

	private bool m_projectorShadowDelay;

	private float m_LastWorldScale;

	private List<Material> m_materialsTempList = new List<Material>();

	private Shader m_UnlitWhiteShader;

	private Shader m_UnlitDarkGreyShader;

	private Material m_ShadowMaterial;

	private Material m_WhiteMaterial;

	private Material m_UnlitDarkGreyMaterial;

	private Shader m_ContactShadowShader;

	private Material m_ContactShadowMaterial;

	private Shader m_MultiSampleShader;

	private Material m_MultiSampleMaterial;

	private Material WhiteMaterial => m_WhiteMaterial ?? (m_WhiteMaterial = new Material(m_UnlitWhiteShader));

	private Material UnlitDarkGreyMaterial => m_UnlitDarkGreyMaterial ?? (m_UnlitDarkGreyMaterial = new Material(m_UnlitDarkGreyShader));

	protected Material ContactShadowMaterial
	{
		get
		{
			if (m_ContactShadowMaterial == null)
			{
				m_ContactShadowMaterial = new Material(m_ContactShadowShader);
				m_ContactShadowMaterial.SetFloat("_Intensity", 3.5f);
				m_ContactShadowMaterial.SetColor("_Color", s_ShadowColor);
				GameObjectUtils.SetHideFlags(m_ContactShadowMaterial, HideFlags.DontSave);
			}
			return m_ContactShadowMaterial;
		}
	}

	protected Material MultiSampleMaterial
	{
		get
		{
			if (m_MultiSampleMaterial == null)
			{
				m_MultiSampleMaterial = new Material(m_MultiSampleShader);
				GameObjectUtils.SetHideFlags(m_MultiSampleMaterial, HideFlags.DontSave);
			}
			return m_MultiSampleMaterial;
		}
	}

	private void Awake()
	{
		if (s_commandBuffer == null)
		{
			s_commandBuffer = CommandBufferPool.Get("Render Shadow");
		}
		if (s_tempBuffer == null || !s_tempBuffer.IsCreated())
		{
			s_tempBuffer = RenderTextureTracker.Get().CreateNewTexture(64, 64, RenderTextureTracker.TEXTURE_DEPTH, RenderTextureFormat.R8);
		}
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		m_objectsToRender = new List<MeshRenderer>();
	}

	protected void Start()
	{
		if (m_graphicsManager != null && m_graphicsManager.RealtimeShadows && !m_enabledAlongsideRealtimeShadows)
		{
			base.enabled = false;
		}
		if (m_ContactShadowShader == null)
		{
			m_ContactShadowShader = ShaderUtils.FindShader("Custom/ContactShadow");
		}
		if (!m_ContactShadowShader)
		{
			Debug.LogError("Failed to load Projected Shadow Shader: Custom/ContactShadow");
			base.enabled = false;
		}
		if (m_MultiSampleShader == null)
		{
			m_MultiSampleShader = ShaderUtils.FindShader("Custom/Selection/HighlightMultiSample");
		}
		if (!m_MultiSampleShader)
		{
			Debug.LogError("Failed to load Projected Shadow Shader: Custom/Selection/HighlightMultiSample");
			base.enabled = false;
		}
		m_UnlitWhiteShader = ShaderUtils.FindShader("Custom/Unlit/Color/White");
		if (!m_UnlitWhiteShader)
		{
			Debug.LogError("Failed to load Projected Shadow Shader: Custom/Unlit/Color/White");
		}
		m_UnlitDarkGreyShader = ShaderUtils.FindShader("Custom/Unlit/Color/DarkGrey");
		if (!m_UnlitDarkGreyShader)
		{
			Debug.LogError("Failed to load Projected Shadow Shader: Custom/Unlit/Color/DarkGrey");
		}
		if (Board.Get() != null)
		{
			if (m_LoadTokenSource == null)
			{
				m_LoadTokenSource = new CancellationTokenSource();
			}
			AssignBoardHeight_WaitForBoardStandardGameLoaded(m_LoadTokenSource.Token).Forget();
		}
		Actor actor = GetComponent<Actor>();
		if (actor != null)
		{
			m_RootObject = actor.GetRootObject();
			return;
		}
		GameObject rootObject = GameObjectUtils.FindChildBySubstring(base.gameObject, "RootObject");
		if (rootObject != null)
		{
			m_RootObject = rootObject;
		}
		else
		{
			m_RootObject = base.gameObject;
		}
	}

	private async UniTaskVoid AssignBoardHeight_WaitForBoardStandardGameLoaded(CancellationToken token = default(CancellationToken))
	{
		if (!ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			return;
		}
		while (sceneMgr != null && sceneMgr.GetMode() == SceneMgr.Mode.GAMEPLAY && Gameplay.Get() != null && Gameplay.Get().GetBoardLayout() == null)
		{
			await UniTask.Yield(PlayerLoopTiming.Initialization, token);
		}
		if (sceneMgr != null && !(Board.Get() == null) && sceneMgr.GetMode() == SceneMgr.Mode.GAMEPLAY)
		{
			Transform centerBone = Board.Get().FindBone("CenterPointBone");
			if (centerBone != null)
			{
				m_BoardHeight = centerBone.position.y;
				m_HasBoardHeight = true;
			}
		}
	}

	protected void LateUpdate()
	{
		if (m_graphicsManager != null && m_graphicsManager.RealtimeShadows && !m_enabledAlongsideRealtimeShadows)
		{
			base.enabled = false;
			return;
		}
		Render();
		if (m_ContactShadow)
		{
			RenderContactShadow();
		}
	}

	private void OnDisable()
	{
		if ((bool)m_PlaneGameObject)
		{
			m_PlaneGameObject.SetActive(value: false);
		}
		if (m_Projector != null)
		{
			m_Projector.enabled = false;
		}
	}

	protected void OnDestroy()
	{
		if ((bool)m_ContactShadowMaterial)
		{
			Object.Destroy(m_ContactShadowMaterial);
		}
		if ((bool)m_ShadowMaterial)
		{
			Object.Destroy(m_ShadowMaterial);
		}
		if ((bool)m_MultiSampleMaterial)
		{
			Object.Destroy(m_MultiSampleMaterial);
		}
		if ((bool)m_ProjectorGameObject)
		{
			Object.Destroy(m_ProjectorGameObject);
		}
		if ((bool)m_ShadowTexture)
		{
			RenderTextureTracker.Get().DestroyRenderTexture(m_ShadowTexture);
			m_ShadowTexture = null;
		}
		if ((bool)m_ContactShadowTexture)
		{
			RenderTextureTracker.Get().DestroyRenderTexture(m_ContactShadowTexture);
			m_ContactShadowTexture = null;
		}
		if ((bool)m_PlaneMesh)
		{
			Object.DestroyImmediate(m_PlaneMesh);
			MeshFilter component = m_PlaneGameObject.GetComponent<MeshFilter>();
			Object.DestroyImmediate(component.mesh);
			component.mesh = null;
			m_PlaneMesh = null;
		}
		if ((bool)m_PlaneGameObject)
		{
			Object.DestroyImmediate(m_PlaneGameObject);
			m_PlaneGameObject = null;
		}
		m_LoadTokenSource?.Cancel();
		m_LoadTokenSource?.Dispose();
	}

	private void OnDrawGizmos()
	{
		float scaledSize = m_ShadowProjectorSize * TransformUtil.ComputeWorldScale(base.transform).x * 2f;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = new Color(0.6f, 0.15f, 0.6f);
		if (m_ContactShadow)
		{
			Gizmos.DrawWireCube(m_ContactOffset, new Vector3(scaledSize, 0f, scaledSize));
		}
		else
		{
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(scaledSize, 0f, scaledSize));
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

	public void Render()
	{
		if (!m_ShadowEnabled || ((bool)m_RootObject && !m_RootObject.activeSelf))
		{
			if ((bool)m_Projector && m_Projector.enabled)
			{
				m_Projector.enabled = false;
			}
			if ((bool)m_PlaneGameObject)
			{
				m_PlaneGameObject.SetActive(value: false);
			}
			return;
		}
		float worldScale = TransformUtil.ComputeWorldScale(base.transform).x;
		m_AdjustedShadowProjectorSize = m_ShadowProjectorSize * worldScale;
		if (m_AdjustedShadowProjectorSize == 0f)
		{
			return;
		}
		if (m_Projector == null)
		{
			CreateProjector();
		}
		float cardY = base.transform.position.y;
		float shadowSurfaceHeight = (m_HasBoardHeight ? m_BoardHeight : (cardY - Mathf.Max(0f, m_AutoDisableHeight) - float.Epsilon));
		float currHeight = (cardY - shadowSurfaceHeight) * 0.3f;
		m_AdjustedShadowProjectorSize += Mathf.Lerp(0f, 0.5f, currHeight * 0.5f);
		if (m_ContactShadow)
		{
			float contactFadeHeight = shadowSurfaceHeight + 0.08f;
			if (currHeight < contactFadeHeight)
			{
				if (m_PlaneGameObject == null)
				{
					m_isDirtyContactShadow = true;
				}
				else if (!m_PlaneGameObject.activeSelf)
				{
					m_isDirtyContactShadow = true;
				}
				float fade = Mathf.Clamp((contactFadeHeight - currHeight) / 0.08f, 0f, 1f);
				if ((bool)m_ContactShadowTexture && (bool)m_PlaneGameObject)
				{
					Renderer renderer = m_PlaneGameObject.GetComponent<Renderer>();
					Material material = (renderer ? renderer.GetSharedMaterial() : null);
					if ((bool)material)
					{
						material.mainTexture = m_ContactShadowTexture;
						material.color = s_ShadowColor;
						material.SetFloat("_Alpha", fade);
					}
				}
			}
			else if (m_PlaneGameObject != null)
			{
				m_PlaneGameObject.SetActive(value: false);
			}
		}
		if (currHeight < m_AutoDisableHeight && m_AutoBoardHeightDisable)
		{
			m_Projector.enabled = false;
			Object.DestroyImmediate(m_ShadowTexture);
			m_ShadowTexture = null;
			return;
		}
		m_Projector.enabled = true;
		float xOffset = 0f;
		if (m_projectorShadowDelay)
		{
			m_projectorShadowDelay = false;
			xOffset = 1000f;
		}
		else if (base.transform.parent != null)
		{
			xOffset = Mathf.Lerp(-0.7f, 1.8f, base.transform.parent.position.x / 17f * -1f) * currHeight;
		}
		Vector3 newPos = new Vector3(base.transform.position.x - xOffset - currHeight * 0.25f, base.transform.position.y, base.transform.position.z - currHeight * 0.8f);
		m_ProjectorTransform.position = newPos;
		m_ProjectorTransform.Translate(m_ProjectionOffset);
		Quaternion currentRotation = base.transform.rotation;
		float aspectRatio = (1f - currentRotation.z) * 0.5f + 0.5f;
		float xfactor = currentRotation.x * 0.5f;
		m_Projector.AspectRatio = aspectRatio - xfactor;
		m_Projector.OrthographicSize = m_AdjustedShadowProjectorSize + xfactor;
		m_ProjectorTransform.rotation = Quaternion.identity;
		m_ProjectorTransform.Rotate(90f, currentRotation.eulerAngles.y, 0f);
		bool needToRender = false;
		if (m_ShadowTexture == null || !m_ShadowTexture.IsCreated())
		{
			m_ShadowTexture = RenderTextureTracker.Get().CreateNewTexture(64, 64, RenderTextureTracker.TEXTURE_DEPTH, RenderTextureFormat.R8);
			needToRender = true;
		}
		if (needToRender || Mathf.Abs(m_LastWorldScale - worldScale) > 0.1f)
		{
			RenderShadowMask();
		}
	}

	public static void SetShadowColor(Color color)
	{
		s_ShadowColor = color;
	}

	public void EnableShadow()
	{
		m_ShadowEnabled = true;
	}

	public void EnableShadow(float FadeInTime)
	{
		m_ShadowEnabled = true;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("from", 0f);
		args.Add("to", 1f);
		args.Add("time", FadeInTime);
		args.Add("easetype", iTween.EaseType.easeInCubic);
		args.Add("onupdate", "UpdateShadowColor");
		args.Add("onupdatetarget", base.gameObject);
		args.Add("name", "ProjectedShadowFade");
		iTween.StopByName(base.gameObject, "ProjectedShadowFade");
		iTween.ValueTo(base.gameObject, args);
	}

	public void DisableShadow()
	{
		DisableShadowProjector();
	}

	public void DisableShadow(float FadeOutTime)
	{
		if (!(m_Projector == null) && m_ShadowEnabled)
		{
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("from", 1f);
			args.Add("to", 0f);
			args.Add("time", FadeOutTime);
			args.Add("easetype", iTween.EaseType.easeOutCubic);
			args.Add("onupdate", "UpdateShadowColor");
			args.Add("onupdatetarget", base.gameObject);
			args.Add("name", "ProjectedShadowFade");
			args.Add("oncomplete", "DisableShadowProjector");
			iTween.StopByName(base.gameObject, "ProjectedShadowFade");
			iTween.ValueTo(base.gameObject, args);
		}
	}

	public void UpdateContactShadow(Spell spell, SpellStateType prevStateType, object userData)
	{
		UpdateContactShadow();
	}

	public void UpdateContactShadow(Spell spell, object userData)
	{
		UpdateContactShadow();
	}

	public void UpdateContactShadow(Spell spell)
	{
		UpdateContactShadow();
	}

	public void UpdateContactShadow()
	{
		if (m_ContactShadow)
		{
			m_isDirtyContactShadow = true;
		}
	}

	private void DisableShadowProjector()
	{
		if (m_Projector != null)
		{
			m_Projector.enabled = false;
		}
		m_ShadowEnabled = false;
	}

	private void UpdateShadowColor(float val)
	{
		if (!(m_Projector == null) && !(m_Projector.Material == null))
		{
			Color color = Color.Lerp(new Color(0.5f, 0.5f, 0.5f, 0.5f), s_ShadowColor, val);
			m_Projector.Material.SetColor("_Color", color);
		}
	}

	private void RenderShadowMask()
	{
		Vector3 currentPosition = base.transform.position;
		s_offset -= 10f;
		if (s_offset < -19000f)
		{
			s_offset = -12000f;
		}
		Vector3 renderPosition = Vector3.left * s_offset;
		base.transform.position = renderPosition;
		float worldScale = TransformUtil.ComputeWorldScale(base.transform).x;
		float halfSize = m_ShadowProjectorSize * worldScale - 0.11f - 0.05f;
		m_LastWorldScale = worldScale;
		RenderToShadowTexture(m_ShadowTexture, renderPosition, halfSize, isContactShadow: false);
		m_ShadowMaterial.SetTexture("_MainTex", m_ShadowTexture);
		m_ShadowMaterial.SetColor("_Color", s_ShadowColor);
		base.transform.position = currentPosition;
	}

	private async UniTaskVoid DelayRenderContactShadow()
	{
		await UniTask.NextFrame();
		m_isDirtyContactShadow = true;
	}

	private void RenderContactShadow()
	{
		if (m_graphicsManager != null && m_graphicsManager.RealtimeShadows && !m_enabledAlongsideRealtimeShadows)
		{
			base.enabled = false;
		}
		if (!(m_ContactShadowTexture != null) || m_isDirtyContactShadow || !m_ContactShadowTexture.IsCreated())
		{
			if (m_PlaneGameObject == null)
			{
				CreateRenderPlane();
			}
			m_PlaneGameObject.SetActive(value: true);
			if (m_ContactShadowTexture == null)
			{
				m_ContactShadowTexture = RenderTextureTracker.Get().CreateNewTexture(64, 64, RenderTextureTracker.TEXTURE_DEPTH, RenderTextureFormat.R8);
			}
			Quaternion currentRotation = base.transform.localRotation;
			Vector3 currentPosition = base.transform.localPosition;
			Vector3 currentScale = base.transform.localScale;
			s_offset -= 10f;
			if (s_offset < -19000f)
			{
				s_offset = -12000f;
			}
			Vector3 renderPosition = Vector3.left * s_offset;
			base.transform.position = renderPosition;
			base.transform.rotation = Quaternion.identity;
			SetWorldScale(base.transform, Vector3.one);
			float halfSize = m_ShadowProjectorSize - 0.11f - 0.15f;
			RenderToShadowTexture(m_ContactShadowTexture, renderPosition, halfSize, isContactShadow: true);
			base.transform.localRotation = currentRotation;
			base.transform.localPosition = currentPosition;
			base.transform.localScale = currentScale;
			m_PlaneGameObject.GetComponent<Renderer>().GetSharedMaterial().mainTexture = m_ContactShadowTexture;
			m_isDirtyContactShadow = false;
		}
	}

	private void RenderToShadowTexture(RenderTexture destTexture, Vector3 renderPosition, float halfSize, bool isContactShadow)
	{
		Matrix4x4 view = Matrix4x4.TRS(renderPosition, Quaternion.Euler(90f, 0f, 0f), new Vector3(1f, 1f, -1f)).inverse;
		Matrix4x4 projection = Matrix4x4.Ortho(0f - halfSize, halfSize, 0f - halfSize, halfSize, -3f, 3f);
		s_commandBuffer.SetRenderTarget(destTexture);
		s_commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		s_commandBuffer.SetViewProjectionMatrices(view, projection);
		GetComponentsInChildren(includeInactive: false, m_objectsToRender);
		foreach (MeshRenderer renderer in m_objectsToRender)
		{
			if (!renderer.enabled)
			{
				continue;
			}
			renderer.GetSharedMaterials(m_materialsTempList);
			int numMaterials = m_materialsTempList.Count;
			for (int materialIdx = 0; materialIdx < numMaterials; materialIdx++)
			{
				Material material = m_materialsTempList[materialIdx];
				if (material != null && material.GetTag("Highlight", searchFallbacks: false) != "")
				{
					s_commandBuffer.DrawRenderer(renderer, isContactShadow ? UnlitDarkGreyMaterial : WhiteMaterial, materialIdx);
				}
			}
		}
		s_commandBuffer.SetGlobalVector(BLUR_OFFSETS_ID, Vector4.one * -0.6f);
		s_commandBuffer.Blit(destTexture, s_tempBuffer, MultiSampleMaterial);
		s_commandBuffer.SetGlobalVector(BLUR_OFFSETS_ID, Vector4.one * -0.8f);
		s_commandBuffer.Blit(s_tempBuffer, destTexture, MultiSampleMaterial);
		Graphics.ExecuteCommandBuffer(s_commandBuffer);
		s_commandBuffer.Clear();
	}

	private void CreateProjector()
	{
		if (m_ProjectorGameObject != null)
		{
			Object.Destroy(m_ProjectorGameObject);
			m_ProjectorGameObject = null;
			m_ProjectorTransform = null;
		}
		m_ProjectorGameObject = (GameObject)Object.Instantiate(Resources.Load("Prefabs/ShadowProjector"));
		m_Projector = m_ProjectorGameObject.GetComponent<DecalProjector>();
		m_ProjectorTransform = m_ProjectorGameObject.transform;
		m_ProjectorTransform.Rotate(90f, 0f, 0f);
		if (m_RootObject != null)
		{
			m_ProjectorTransform.parent = m_RootObject.transform;
		}
		m_Projector.NearClipPlane = 0f;
		m_Projector.FarClipPlane = m_ProjectionFarClip;
		m_Projector.OrthographicSize = m_AdjustedShadowProjectorSize;
		GameObjectUtils.SetHideFlags(m_Projector, HideFlags.HideAndDontSave);
		m_ShadowMaterial = m_Projector.Material;
		m_projectorShadowDelay = true;
	}

	private void CreateRenderPlane()
	{
		if (m_PlaneGameObject != null)
		{
			Object.DestroyImmediate(m_PlaneGameObject);
		}
		m_PlaneGameObject = new GameObject();
		m_PlaneGameObject.name = base.name + "_ContactShadowRenderPlane";
		if (m_RootObject != null)
		{
			m_PlaneGameObject.transform.parent = m_RootObject.transform;
		}
		m_PlaneGameObject.transform.localPosition = m_ContactOffset;
		m_PlaneGameObject.transform.localRotation = Quaternion.identity;
		m_PlaneGameObject.transform.localScale = new Vector3(0.98f, 1f, 0.98f);
		m_PlaneGameObject.AddComponent<MeshFilter>();
		m_PlaneGameObject.AddComponent<MeshRenderer>();
		GameObjectUtils.SetHideFlags(m_PlaneGameObject, HideFlags.HideAndDontSave);
		Mesh meshPlane = new Mesh();
		meshPlane.name = "ContactShadowMeshPlane";
		float halfWidth = m_ShadowProjectorSize;
		float halfHeight = m_ShadowProjectorSize;
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
		Mesh planeMesh = (m_PlaneGameObject.GetComponent<MeshFilter>().mesh = meshPlane);
		m_PlaneMesh = planeMesh;
		m_PlaneMesh.RecalculateBounds();
		m_ContactShadowMaterial = ContactShadowMaterial;
		m_ContactShadowMaterial.color = s_ShadowColor;
		if ((bool)m_ContactShadowMaterial)
		{
			m_PlaneGameObject.GetComponent<Renderer>().SetSharedMaterial(m_ContactShadowMaterial);
		}
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
}
