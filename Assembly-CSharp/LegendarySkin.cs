using System;
using System.Collections.Generic;
using Hearthstone;
using LegendarySkins;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class LegendarySkin : MonoBehaviour
{
	public enum AntiAliasingSetting
	{
		Off,
		Two,
		Four,
		Eight
	}

	[Flags]
	public enum SuspendUpdateReason
	{
		DynamicResolution = 1,
		UpdatePriority = 2
	}

	public enum CameraBoneUsage
	{
		LookAt,
		Parent
	}

	private struct RenderCommandWithPass
	{
		public RenderCommand Command;

		public int ShaderPass;
	}

	[Header("Camera")]
	public Camera RTTCamera;

	public int TextureSize;

	[FormerlySerializedAs("LookAtJoint")]
	public GameObject CameraBone;

	public CameraBoneUsage BoneUsage;

	public bool AnimateFOV;

	[Header("Lighting")]
	public LegendarySkinLight DirectionalLight;

	public bool ShadowPassEnabled;

	public int ShadowTextureSize;

	public PortraitLighting LightSettings;

	public LegendarySkinShadowVolume ShadowVolume;

	[Header("Render Texture Settings")]
	public AntiAliasingSetting AntiAliasingLevel;

	[Range(0f, 1f)]
	public float ScissorRegion = 1f;

	[Min(0f)]
	public int UpdatePriority;

	[Header("Baking pose")]
	public AnimationClip PoseAnimation;

	public float ClipTime;

	public float m_RimLightColorMult = 1f;

	private RenderTexture m_renderTexture;

	private Animator m_animator;

	private HashSet<LegendarySkinDynamicResController> m_dynamicResolutionControllers = new HashSet<LegendarySkinDynamicResController>();

	private static Stack<int> s_freeSlots = new Stack<int>();

	private static int s_nextFreeSlot = 0;

	private int m_slot;

	private bool m_renderersDirty;

	private bool m_commandBuffersDirty;

	private bool m_renderCommandBuffer;

	private int m_dynamicResolution;

	private float m_viewportFraction = 1f;

	private SuspendUpdateReason m_suspendUpdateReason;

	private Renderer[] m_allRenderers;

	private List<RenderCommandWithPass> m_shadowRenderCommands;

	private List<RenderCommand> m_forwardRenderCommands;

	private CommandBuffer m_forwardCommandBuffer;

	private Matrix4x4 m_projectionMatrix;

	private Matrix4x4 m_viewMatrix;

	private Vector3 m_cameraPosition;

	private static readonly int s_MainTexID = Shader.PropertyToID("_MainTex");

	private static readonly int s_PortraitShadowMatrixID = Shader.PropertyToID("_PortraitShadowMatrix");

	private static readonly int s_PortraitLightDirectionID = Shader.PropertyToID("_PortraitLightDirection");

	private static readonly int s_PortraitLightColourID = Shader.PropertyToID("_PortraitLightColour");

	private static readonly int s_PortraitShadowMapID = Shader.PropertyToID("_PortraitShadowMap");

	private static readonly int s_PortraitRimLightColorID = Shader.PropertyToID("_PortraitRimLightColor");

	private static readonly int s_PortraitHairRimLightColorID = Shader.PropertyToID("_PortraitHairRimLightColor");

	private static readonly int s_PortraitShadowColorID = Shader.PropertyToID("_PortraitShadowColor");

	private static readonly int s_PortraitCameraPositionID = Shader.PropertyToID("_PortraitCameraPosition");

	private static readonly int s_SoftnessID = Shader.PropertyToID("_Softness");

	private static readonly int s_SoftnessFalloffID = Shader.PropertyToID("_SoftnessFalloff");

	private static readonly int s_SSSLightDirID = Shader.PropertyToID("_SSSLightDir");

	private static readonly int s_ViewDirID = Shader.PropertyToID("_ViewDir");

	private static readonly int s_CubemapRotationID = Shader.PropertyToID("_CubemapRotationMatrix");

	private static readonly int s_CubemapID = Shader.PropertyToID("_Cubemap");

	private static readonly int s_RimLightConeID = Shader.PropertyToID("_RimLightCone");

	private static readonly int s_RimLightConeDirectionID = Shader.PropertyToID("_RimLightConeDirection");

	private static readonly int s_RimLightFalloffID = Shader.PropertyToID("_RimLightFalloff");

	private static readonly int s_ShadowRenderTextureID = Shader.PropertyToID("_ShadowRenderTexture");

	private static readonly int s_InvResolutionID = Shader.PropertyToID("_InvResolution");

	private static readonly int s_CameraFocalLengthID = Animator.StringToHash("Focal Length");

	public static bool DynamicResolutionEnabled = false;

	public static float DynamicResolutionScale = 1f;

	public Texture PortraitTexture => m_renderTexture;

	public bool CanUpdateRender => m_suspendUpdateReason == (SuspendUpdateReason)0;

	public bool HasObservers
	{
		get
		{
			if (m_dynamicResolutionControllers.Count <= 0)
			{
				return !DynamicResolutionEnabled;
			}
			return true;
		}
	}

	private void Awake()
	{
		if (RTTCamera != null && CameraBone != null)
		{
			switch (BoneUsage)
			{
			case CameraBoneUsage.LookAt:
				RTTCamera.transform.LookAt(CameraBone.transform, RTTCamera.transform.up);
				RTTCamera.transform.SetParent(CameraBone.transform, worldPositionStays: true);
				break;
			case CameraBoneUsage.Parent:
				RTTCamera.transform.SetParent(CameraBone.transform, worldPositionStays: false);
				RTTCamera.transform.localPosition = Vector3.zero;
				RTTCamera.transform.localScale = Vector3.one;
				break;
			}
		}
		int textureSize = Mathf.NextPowerOfTwo(TextureSize);
		int antiAliasing = 1;
		if ((bool)UniversalInputManager.UsePhoneUI && textureSize > 512)
		{
			textureSize = Mathf.Max(512, textureSize / 2);
		}
		switch (AntiAliasingLevel)
		{
		case AntiAliasingSetting.Off:
			antiAliasing = 1;
			break;
		case AntiAliasingSetting.Two:
			antiAliasing = 2;
			break;
		case AntiAliasingSetting.Four:
			antiAliasing = 4;
			break;
		case AntiAliasingSetting.Eight:
			antiAliasing = 8;
			break;
		}
		m_animator = GetComponentInChildren<Animator>();
		m_renderTexture = new RenderTexture(textureSize, textureSize, 24, RenderTextureFormat.ARGB32);
		m_renderTexture.antiAliasing = antiAliasing;
		m_renderTexture.filterMode = FilterMode.Bilinear;
		m_dynamicResolution = textureSize;
		m_allRenderers = GetComponentsInChildren<Renderer>(includeInactive: true);
		Renderer[] allRenderers = m_allRenderers;
		foreach (Renderer renderer in allRenderers)
		{
			if (renderer is SkinnedMeshRenderer)
			{
				(renderer as SkinnedMeshRenderer).updateWhenOffscreen = true;
			}
			else
			{
				renderer.enabled = false;
			}
		}
		RTTCamera.aspect = 1f;
		RTTCamera.enabled = false;
		Camera[] componentsInChildren = base.gameObject.GetComponentsInChildren<Camera>();
		foreach (Camera camera in componentsInChildren)
		{
			if (camera != RTTCamera)
			{
				camera.enabled = false;
			}
		}
	}

	private void OnEnable()
	{
		if (s_freeSlots.Count == 0)
		{
			s_freeSlots.Push(s_nextFreeSlot++);
		}
		m_slot = s_freeSlots.Pop();
		base.transform.SetPositionAndRotation(new Vector3(100f * (float)m_slot, -200f, -1000f), Quaternion.identity);
		CreateRenderCommands();
		m_commandBuffersDirty = true;
		RenderPipelineManager.beginFrameRendering += OnBeginFrameRendering;
	}

	private void OnDisable()
	{
		s_freeSlots.Push(m_slot);
		m_slot = -1;
		RenderPipelineManager.beginFrameRendering -= OnBeginFrameRendering;
	}

	private void OnDestroy()
	{
		if ((bool)RTTCamera)
		{
			RTTCamera.targetTexture = null;
		}
		if ((bool)m_renderTexture)
		{
			UnityEngine.Object.Destroy(m_renderTexture);
			m_renderTexture = null;
		}
	}

	private void Update()
	{
		if (AnimateFOV && CameraBone != null && m_animator != null)
		{
			RTTCamera.focalLength = m_animator.GetFloat(s_CameraFocalLengthID);
		}
	}

	private void LateUpdate()
	{
		if (CameraBone != null || AnimateFOV)
		{
			m_renderersDirty = true;
		}
		if (m_renderersDirty)
		{
			CreateRenderCommands();
			m_commandBuffersDirty = true;
			m_renderersDirty = false;
		}
		UpdateDynamicResolutionAndRender(Camera.allCameras);
	}

	public void SetSuspendFlag(SuspendUpdateReason reason)
	{
		m_suspendUpdateReason |= reason;
		if (m_suspendUpdateReason != 0 && m_animator != null)
		{
			m_animator.enabled = false;
		}
	}

	public void ClearSuspendFlag(SuspendUpdateReason reason)
	{
		m_suspendUpdateReason &= ~reason;
		if (m_suspendUpdateReason == (SuspendUpdateReason)0 && m_animator != null)
		{
			m_animator.enabled = true;
		}
	}

	private void UpdateDynamicResolutionAndRender(Camera[] cameras)
	{
		if (cameras == null || cameras.Length == 0)
		{
			return;
		}
		int maxWidth = m_renderTexture.width;
		int previousResolution = m_dynamicResolution;
		int targetResolution = 0;
		if (HasObservers)
		{
			targetResolution = maxWidth;
			if (DynamicResolutionEnabled)
			{
				bool useEstimatedSize = true;
				float estimatedPixelsHigh = 0f;
				using (HashSet<LegendarySkinDynamicResController>.Enumerator enumerator = m_dynamicResolutionControllers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						float sizeInPixels;
						switch (enumerator.Current.GetSize(cameras, out sizeInPixels))
						{
						case LegendarySkinDynamicResController.SizeResult.Bounded:
							estimatedPixelsHigh = Mathf.Max(estimatedPixelsHigh, sizeInPixels);
							break;
						case LegendarySkinDynamicResController.SizeResult.MaxSize:
							useEstimatedSize = false;
							break;
						}
					}
				}
				if (useEstimatedSize)
				{
					estimatedPixelsHigh *= DynamicResolutionScale;
					targetResolution = Mathf.RoundToInt(Mathf.Min(estimatedPixelsHigh, maxWidth));
				}
			}
		}
		if (targetResolution == 0)
		{
			SetSuspendFlag(SuspendUpdateReason.DynamicResolution);
		}
		else
		{
			ClearSuspendFlag(SuspendUpdateReason.DynamicResolution);
		}
		if (!CanUpdateRender)
		{
			return;
		}
		m_dynamicResolution = targetResolution;
		if (previousResolution != m_dynamicResolution)
		{
			m_commandBuffersDirty = true;
		}
		if (m_commandBuffersDirty)
		{
			float newViewportFraction = (float)m_dynamicResolution / (float)maxWidth;
			if (!Mathf.Approximately(m_viewportFraction, newViewportFraction))
			{
				m_viewportFraction = newViewportFraction;
				foreach (LegendarySkinDynamicResController dynamicResolutionController in m_dynamicResolutionControllers)
				{
					dynamicResolutionController.UpdateMaterial(m_viewportFraction);
				}
			}
			BuildCommandBuffers();
			m_commandBuffersDirty = false;
		}
		m_renderCommandBuffer = true;
	}

	private void OnBeginFrameRendering(ScriptableRenderContext renderContext, Camera[] cameras)
	{
		if (m_renderCommandBuffer)
		{
			m_renderCommandBuffer = false;
			Camera.SetupCurrent(RTTCamera);
			Graphics.ExecuteCommandBuffer(m_forwardCommandBuffer);
		}
	}

	public void SetDirty()
	{
		m_renderersDirty = true;
	}

	private void BuildCommandBuffers(RenderTexture renderTextureOverride = null, int shadowTextureOverride = 0)
	{
		m_forwardCommandBuffer = new CommandBuffer
		{
			name = "PortraitRender"
		};
		Vector3 shadowFocus = Vector3.zero;
		float shadowRange = 0f;
		if (ShadowVolume != null)
		{
			shadowFocus = ShadowVolume.transform.position;
			shadowRange = ShadowVolume.Radius;
		}
		else
		{
			Bounds shadowBounds = default(Bounds);
			foreach (RenderCommandWithPass command in m_shadowRenderCommands)
			{
				if (shadowBounds.extents.sqrMagnitude > 0f)
				{
					shadowBounds.Encapsulate(command.Command.Renderer.bounds);
				}
				else
				{
					shadowBounds = command.Command.Renderer.bounds;
				}
			}
			shadowFocus = shadowBounds.center;
			shadowRange = shadowBounds.extents.magnitude;
		}
		Vector3 lightDirection = Vector3.forward;
		Vector3 lightColourVector = Vector3.one;
		Matrix4x4 viewMatrix = Matrix4x4.identity;
		if ((bool)DirectionalLight)
		{
			Color lightColour = DirectionalLight.color;
			lightColourVector = new Vector3(lightColour.r, lightColour.g, lightColour.b) * DirectionalLight.intensity;
			shadowRange = Mathf.Max(0.1f, Mathf.Abs(shadowRange));
			lightDirection = DirectionalLight.transform.forward;
			viewMatrix = Matrix4x4.TRS(shadowFocus, Quaternion.FromToRotation(Vector3.forward, -lightDirection), Vector3.one * shadowRange).inverse;
		}
		Matrix4x4 orthoProjection = Matrix4x4.Ortho(-1f, 1f, -1f, 1f, -1f, 1f);
		Matrix4x4 shadowProjection = GL.GetGPUProjectionMatrix(orthoProjection, renderIntoTexture: true) * viewMatrix;
		bool addShadowToCommandBuffer = ShadowPassEnabled && m_shadowRenderCommands.Count > 0;
		if (addShadowToCommandBuffer)
		{
			int shadowTextureSize = Mathf.NextPowerOfTwo(ShadowTextureSize);
			if ((bool)UniversalInputManager.UsePhoneUI && shadowTextureSize > 512)
			{
				shadowTextureSize = Mathf.Max(512, shadowTextureSize / 2);
			}
			if (shadowTextureOverride > 0)
			{
				shadowTextureSize = shadowTextureOverride;
			}
			m_forwardCommandBuffer.GetTemporaryRT(s_ShadowRenderTextureID, shadowTextureSize, shadowTextureSize, 24, FilterMode.Bilinear, RenderTextureFormat.Depth);
			m_forwardCommandBuffer.SetRenderTarget(s_ShadowRenderTextureID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
			m_forwardCommandBuffer.SetViewport(new Rect(0f, 0f, shadowTextureSize, shadowTextureSize));
			m_forwardCommandBuffer.ClearRenderTarget(clearDepth: true, clearColor: false, Color.clear);
			m_forwardCommandBuffer.SetViewProjectionMatrices(viewMatrix, orthoProjection);
			m_forwardCommandBuffer.SetGlobalMatrix(s_PortraitShadowMatrixID, shadowProjection);
			foreach (RenderCommandWithPass command2 in m_shadowRenderCommands)
			{
				m_forwardCommandBuffer.DrawRenderer(command2.Command.Renderer, command2.Command.Material, command2.Command.MeshIndex, 1);
			}
		}
		RenderTexture renderTexture = renderTextureOverride ?? m_renderTexture;
		m_forwardCommandBuffer.SetRenderTarget(renderTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
		int viewportSize = ((renderTextureOverride == null) ? m_dynamicResolution : renderTexture.width);
		m_forwardCommandBuffer.SetViewport(new Rect(0f, 0f, viewportSize, viewportSize));
		m_forwardCommandBuffer.SetViewProjectionMatrices(m_viewMatrix, m_projectionMatrix);
		m_forwardCommandBuffer.SetGlobalMatrix(s_PortraitShadowMatrixID, shadowProjection);
		m_forwardCommandBuffer.SetGlobalVector(s_PortraitLightDirectionID, lightDirection);
		m_forwardCommandBuffer.SetGlobalVector(s_PortraitLightColourID, lightColourVector);
		m_forwardCommandBuffer.SetGlobalVector(s_PortraitCameraPositionID, m_cameraPosition);
		if (addShadowToCommandBuffer)
		{
			m_forwardCommandBuffer.SetGlobalTexture(s_PortraitShadowMapID, s_ShadowRenderTextureID);
		}
		else
		{
			m_forwardCommandBuffer.SetGlobalTexture(s_PortraitShadowMapID, Texture2D.blackTexture);
		}
		m_forwardCommandBuffer.SetGlobalVector(s_SSSLightDirID, lightDirection);
		m_forwardCommandBuffer.SetGlobalVector(s_ViewDirID, RTTCamera.transform.forward);
		m_forwardCommandBuffer.SetGlobalFloat(s_InvResolutionID, renderTexture.width);
		if (LightSettings != null)
		{
			m_forwardCommandBuffer.SetGlobalVector(s_PortraitRimLightColorID, LightSettings.RimLightColor * m_RimLightColorMult);
			m_forwardCommandBuffer.SetGlobalVector(s_PortraitHairRimLightColorID, LightSettings.HairRimLightColor * m_RimLightColorMult);
			m_forwardCommandBuffer.SetGlobalVector(s_PortraitShadowColorID, LightSettings.ShadowColor);
			m_forwardCommandBuffer.SetGlobalFloat(s_SoftnessID, LightSettings.DepthBias / 64f);
			m_forwardCommandBuffer.SetGlobalFloat(s_SoftnessFalloffID, Mathf.Exp(LightSettings.SoftnessFalloff));
			Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, LightSettings.CubemapRotation * 360f, 0f), Vector3.one);
			m_forwardCommandBuffer.SetGlobalMatrix(s_CubemapRotationID, rotationMatrix);
			m_forwardCommandBuffer.SetGlobalTexture(s_CubemapID, LightSettings.Cubemap);
			Vector4 rimLightCone = default(Vector4);
			float rimLightDirection = (float)Math.PI / 180f * LightSettings.RimLightDirection;
			rimLightCone.x = Mathf.Cos(rimLightDirection);
			rimLightCone.y = Mathf.Sin(rimLightDirection);
			float num = (float)Math.PI / 180f * Mathf.Clamp(LightSettings.RimLightAngle, 0f, 360f) * 0.5f;
			float coneMin = Mathf.Cos(num);
			float coneMax = Mathf.Cos(num * (1f - LightSettings.RimLightAngleSoftness));
			rimLightCone.z = Mathf.Min(coneMin, coneMax - Mathf.Epsilon);
			rimLightCone.w = coneMax;
			m_forwardCommandBuffer.SetGlobalVector(s_RimLightConeID, rimLightCone);
			Vector3 rimLightConeDirection = new Vector3(rimLightCone.x, rimLightCone.y, 0f);
			rimLightConeDirection = RTTCamera.cameraToWorldMatrix * rimLightConeDirection;
			m_forwardCommandBuffer.SetGlobalVector(s_RimLightConeDirectionID, rimLightConeDirection);
			Vector2 rimFalloff = default(Vector2);
			rimFalloff.x = LightSettings.RimLightMinNormal - Mathf.Epsilon;
			rimFalloff.y = LightSettings.RimLightMaxNormal + Mathf.Epsilon;
			m_forwardCommandBuffer.SetGlobalVector(s_RimLightFalloffID, rimFalloff);
		}
		else
		{
			m_forwardCommandBuffer.SetGlobalVector(s_PortraitRimLightColorID, Color.white);
			m_forwardCommandBuffer.SetGlobalVector(s_PortraitHairRimLightColorID, Color.white);
			m_forwardCommandBuffer.SetGlobalVector(s_PortraitShadowColorID, Color.black);
			m_forwardCommandBuffer.SetGlobalFloat(s_SoftnessID, 1f / 64f);
			m_forwardCommandBuffer.SetGlobalFloat(s_SoftnessFalloffID, Mathf.Exp(4f));
			m_forwardCommandBuffer.SetGlobalMatrix(s_CubemapRotationID, Matrix4x4.identity);
			m_forwardCommandBuffer.SetGlobalTexture(s_CubemapID, Texture2D.blackTexture);
			m_forwardCommandBuffer.SetGlobalVector(s_RimLightConeID, new Vector4(1f, 0f, -0.1f, 0.1f));
			m_forwardCommandBuffer.SetGlobalVector(s_RimLightConeDirectionID, new Vector4(1f, 0f, 0f, 0f));
			m_forwardCommandBuffer.SetGlobalVector(s_RimLightFalloffID, new Vector2(0.85f, 0.95f));
		}
		bool applyScissorRect = ScissorRegion < 1f - Mathf.Epsilon;
		m_forwardCommandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		if (applyScissorRect)
		{
			m_forwardCommandBuffer.EnableScissorRect(new Rect(0f, 0f, viewportSize, (float)viewportSize * ScissorRegion));
		}
		foreach (RenderCommand command3 in m_forwardRenderCommands)
		{
			m_forwardCommandBuffer.DrawRenderer(command3.Renderer, command3.Material, command3.MeshIndex, 0);
		}
		if (applyScissorRect)
		{
			m_forwardCommandBuffer.DisableScissorRect();
		}
	}

	private void CreateRenderCommands(ISkinMaterialProcessor materialProcessor = null)
	{
		m_projectionMatrix = RTTCamera.projectionMatrix;
		m_viewMatrix = RTTCamera.worldToCameraMatrix;
		m_cameraPosition = RTTCamera.transform.position;
		m_shadowRenderCommands = new List<RenderCommandWithPass>();
		m_forwardRenderCommands = new List<RenderCommand>();
		Renderer[] allRenderers = m_allRenderers;
		foreach (Renderer renderer in allRenderers)
		{
			if (!renderer.gameObject.activeInHierarchy)
			{
				continue;
			}
			List<Material> sharedMaterials = new List<Material>();
			renderer.GetSharedMaterials(sharedMaterials);
			MeshRenderer obj = renderer as MeshRenderer;
			SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
			int totalMeshCount = 1;
			if ((bool)obj)
			{
				MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
				if (meshFilter == null || meshFilter.sharedMesh == null)
				{
					continue;
				}
				totalMeshCount = meshFilter.sharedMesh.subMeshCount;
			}
			if ((bool)skinnedMeshRenderer)
			{
				totalMeshCount = skinnedMeshRenderer.sharedMesh.subMeshCount;
			}
			for (int meshIndex = 0; meshIndex < totalMeshCount; meshIndex++)
			{
				int materialIndex = meshIndex;
				if (materialIndex >= sharedMaterials.Count)
				{
					materialIndex = 0;
				}
				Material material = sharedMaterials[materialIndex];
				if (material == null)
				{
					Debug.LogError($"Material on mesh '{renderer.gameObject}' at index {materialIndex} is null", renderer);
					continue;
				}
				if (materialProcessor != null)
				{
					material = materialProcessor.ProcessMaterial(material);
				}
				m_forwardRenderCommands.Add(new RenderCommand
				{
					Renderer = renderer,
					Material = material,
					MeshIndex = meshIndex
				});
				if (!ShadowPassEnabled || !(material.GetTag("RenderType", searchFallbacks: false) == "LegendaryPortrait") || renderer.shadowCastingMode == ShadowCastingMode.Off)
				{
					continue;
				}
				int passCount = material.passCount;
				for (int passIndex = 0; passIndex < passCount; passIndex++)
				{
					if (material.GetPassName(passIndex) == "Shadow Pass")
					{
						m_shadowRenderCommands.Add(new RenderCommandWithPass
						{
							Command = new RenderCommand
							{
								Renderer = renderer,
								Material = material,
								MeshIndex = meshIndex
							},
							ShaderPass = passIndex
						});
						break;
					}
				}
			}
		}
		m_forwardRenderCommands.Sort(SortRenderCommands);
		m_shadowRenderCommands.Sort(SortRenderCommands);
	}

	public void AddDynamicResController(LegendarySkinDynamicResController controller)
	{
		m_dynamicResolutionControllers.Add(controller);
		controller.UpdateMaterial(m_viewportFraction);
	}

	public void RemoveDynamicResController(LegendarySkinDynamicResController controller)
	{
		m_dynamicResolutionControllers.Remove(controller);
	}

	public void UpdateDynamicResControllers()
	{
		foreach (LegendarySkinDynamicResController dynamicResolutionController in m_dynamicResolutionControllers)
		{
			dynamicResolutionController.UpdateMaterial(m_viewportFraction);
		}
	}

	private static int SortRenderCommands(RenderCommand a, RenderCommand b)
	{
		if (a.Material.renderQueue == b.Material.renderQueue)
		{
			return a.Renderer.rendererPriority - b.Renderer.rendererPriority;
		}
		return a.Material.renderQueue - b.Material.renderQueue;
	}

	private static int SortRenderCommands(RenderCommandWithPass a, RenderCommandWithPass b)
	{
		return SortRenderCommands(a.Command, b.Command);
	}
}
