using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI;

[ExecuteAlways]
[AddComponentMenu("")]
public class PopupRoot : MonoBehaviour, IPopupRoot
{
	public CustomViewEntryPoint ViewEntryPoint = CustomViewEntryPoint.Count;

	public CameraOverridePass PrimaryRenderPass;

	public UIContext.ProjectionType ProjectionType;

	public ScreenEffectsHandle FullscreenEffectsHandle;

	private float m_perspectiveOverrideZNearLimit = 1f;

	private float m_perspectiveOverrideZFarLimit;

	private List<CameraOverridePass> m_maskRenderPasses = new List<CameraOverridePass>();

	private uint m_renderingLayerMask = 1u;

	private Camera m_popupCamera;

	private bool m_enabled;

	private HashSet<IPopupRendering> m_popupRenderingComponents = new HashSet<IPopupRendering>();

	private List<Action> m_popupRenderingDisabledListeners = new List<Action>();

	private bool m_resolutionChanged;

	private bool m_waitedFrameForResolutionChange;

	private IGraphicsManager m_graphicsManager;

	public Camera PopupCamera => m_popupCamera;

	public bool IsEnabled => m_enabled;

	public Transform PopupTransform => base.transform;

	public Transform PopupBone { get; set; }

	public uint RenderingLayerMask
	{
		get
		{
			return m_renderingLayerMask;
		}
		set
		{
			m_renderingLayerMask = value;
		}
	}

	public bool IsPerspectivePopup { get; set; }

	public bool IsBeingDisabledOrDestroyed { get; private set; }

	public event Action<PopupRoot> OnDestroyed;

	public event Action<PopupRoot> OnDisabled;

	private void Awake()
	{
		FullscreenEffectsHandle = new ScreenEffectsHandle(this);
	}

	public void Initialize(Camera popupCamera = null, CameraOverridePass popupRenderPass = null)
	{
		m_enabled = true;
		m_popupCamera = popupCamera;
		PrimaryRenderPass = popupRenderPass;
		ApplyPopupRendering(base.transform, m_popupRenderingComponents, overrideLayer: true);
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		if (m_graphicsManager != null)
		{
			m_graphicsManager.OnResolutionChangedEvent += OnResolutionChanged;
		}
	}

	public void ApplyPopupRendering(Transform parent, HashSet<IPopupRendering> popupRenderingComponents, bool overrideLayer = false, int layer = 29, bool isVisible = true)
	{
		if (popupRenderingComponents == null)
		{
			popupRenderingComponents = m_popupRenderingComponents;
		}
		List<MonoBehaviour> cachedComponentList = new List<MonoBehaviour>();
		GameObjectUtils.WalkSelfAndChildren(parent, delegate(Transform child)
		{
			bool flag = child == parent;
			bool flag2 = false;
			GetOrCreatePopupRenderer(child.gameObject, overrideLayer, layer, isVisible);
			if (overrideLayer)
			{
				child.gameObject.layer = layer;
			}
			if (!flag)
			{
				child.GetComponents(cachedComponentList);
				for (int i = 0; i < cachedComponentList.Count; i++)
				{
					if (overrideLayer && cachedComponentList[i] is ILayerOverridable layerOverridable)
					{
						layerOverridable.SetLayerOverride((GameLayer)layer);
					}
					if (cachedComponentList[i] is IPopupRendering popupRendering)
					{
						popupRendering.EnablePopupRendering(this);
						popupRenderingComponents.Add(popupRendering);
						flag2 |= popupRendering.HandlesChildPropagation();
					}
				}
			}
			cachedComponentList.Clear();
			return flag || !flag2;
		});
	}

	public void GetOrCreatePopupRenderer(GameObject objToAddRendererTo, bool overrideLayer = false, int layer = 1, bool isVisible = true)
	{
		if (objToAddRendererTo.TryGetComponent<Renderer>(out var _))
		{
			if (!objToAddRendererTo.TryGetComponent<PopupRenderer>(out var popupRenderer))
			{
				popupRenderer = objToAddRendererTo.AddComponent<PopupRenderer>();
			}
			popupRenderer.Initialize(this, isVisible, RenderingLayerMask);
		}
	}

	public void CleanupPopupRendering(HashSet<IPopupRendering> popupRenderingComponents)
	{
		foreach (IPopupRendering popupRenderer in popupRenderingComponents)
		{
			if (popupRenderer != null && !(popupRenderer as UnityEngine.Object == null))
			{
				popupRenderer.DisablePopupRendering();
			}
		}
		popupRenderingComponents.Clear();
	}

	public void DisablePopupRendering()
	{
		m_enabled = false;
		if (PrimaryRenderPass != null)
		{
			PrimaryRenderPass.Unschedule();
			CameraPassProvider.ReleasePass(PrimaryRenderPass);
		}
		foreach (CameraOverridePass maskRenderPass in m_maskRenderPasses)
		{
			maskRenderPass.Unschedule();
		}
		PrimaryRenderPass = null;
		m_maskRenderPasses.Clear();
		CleanupPopupRendering(m_popupRenderingComponents);
		for (int i = m_popupRenderingDisabledListeners.Count - 1; i >= 0; i--)
		{
			m_popupRenderingDisabledListeners[i]?.Invoke();
		}
	}

	public void RegisterDisabledListener(Action callback)
	{
		m_popupRenderingDisabledListeners.Add(callback);
	}

	public void RemoveDisabledListener(Action callback)
	{
		if (m_popupRenderingDisabledListeners.Contains(callback))
		{
			m_popupRenderingDisabledListeners.Remove(callback);
		}
	}

	public void SetOverrideProjectionMatrixZRange(float allowedZNear, float allowedZFar)
	{
		m_perspectiveOverrideZNearLimit = allowedZNear;
		m_perspectiveOverrideZFarLimit = allowedZFar;
	}

	private void OnEnable()
	{
		IsBeingDisabledOrDestroyed = false;
	}

	private void OnDisable()
	{
		IsBeingDisabledOrDestroyed = true;
		DisablePopupRendering();
		if (this.OnDisabled != null)
		{
			this.OnDisabled(this);
		}
	}

	private void OnDestroy()
	{
		IsBeingDisabledOrDestroyed = true;
		if (m_graphicsManager != null)
		{
			m_graphicsManager.OnResolutionChangedEvent -= OnResolutionChanged;
		}
		if (this.OnDestroyed != null)
		{
			this.OnDestroyed(this);
		}
	}

	private void OnResolutionChanged(int width, int height)
	{
		UpdateRenderingPassOverrides();
		m_resolutionChanged = true;
		m_waitedFrameForResolutionChange = false;
	}

	private void LateUpdate()
	{
		if (m_resolutionChanged)
		{
			if (!m_waitedFrameForResolutionChange)
			{
				m_waitedFrameForResolutionChange = true;
				return;
			}
			UpdateRenderingPassOverrides();
			m_resolutionChanged = false;
			m_waitedFrameForResolutionChange = false;
		}
	}

	public void ChangeRenderingSchedule(CustomViewEntryPoint entryPoint)
	{
		ViewEntryPoint = entryPoint;
		if (PrimaryRenderPass != null && PrimaryRenderPass.isScheduled)
		{
			PrimaryRenderPass.ChangeSchedule(ViewEntryPoint);
		}
		foreach (CameraOverridePass maskPass in m_maskRenderPasses)
		{
			if (maskPass.isScheduled)
			{
				maskPass.ChangeSchedule(ViewEntryPoint);
			}
		}
	}

	public bool NeedsProjectionMatrixOverride()
	{
		if (ProjectionType == UIContext.ProjectionType.Perspective)
		{
			return !IsPerspectivePopup;
		}
		return false;
	}

	public void UpdateRenderingPassOverrides()
	{
		if (PrimaryRenderPass != null)
		{
			PrimaryRenderPass.OverrideRenderLayerMask(RenderingLayerMask);
			if (NeedsProjectionMatrixOverride())
			{
				SetMatrixProjection(PrimaryRenderPass);
			}
			else
			{
				PrimaryRenderPass.ClearOverrides(CameraOverridePass.OverrideFlags.ProjectionMatrix);
				PrimaryRenderPass.ClearOverrides(CameraOverridePass.OverrideFlags.ViewMatrix);
			}
		}
		foreach (CameraOverridePass maskPass in m_maskRenderPasses)
		{
			maskPass.OverrideRenderLayerMask(m_renderingLayerMask);
			if (NeedsProjectionMatrixOverride())
			{
				SetMatrixProjection(maskPass);
				continue;
			}
			maskPass.ClearOverrides(CameraOverridePass.OverrideFlags.ProjectionMatrix);
			maskPass.ClearOverrides(CameraOverridePass.OverrideFlags.ViewMatrix);
		}
	}

	public void AddMaskablePass(object cameraOverridePass)
	{
		if (cameraOverridePass is CameraOverridePass passToSet && !m_maskRenderPasses.Contains(passToSet))
		{
			m_maskRenderPasses.Add(passToSet);
		}
	}

	public void RemoveMaskPass(object cameraOverridePass)
	{
		if (cameraOverridePass is CameraOverridePass passToSet && m_maskRenderPasses.Contains(passToSet))
		{
			m_maskRenderPasses.Remove(passToSet);
		}
	}

	public int GetViewEntryPoint()
	{
		return (int)ViewEntryPoint;
	}

	private Matrix4x4 PerspectiveMtxZRange(float fovy, float aspect, float zNear, float zFar, float allowedNear, float allowedFar)
	{
		allowedFar = Mathf.Clamp(allowedFar, 0f, 1f);
		allowedNear = Mathf.Clamp(allowedNear, allowedFar + 0.0001f, 1f);
		float d = Mathf.Lerp(1f, -1f, allowedFar);
		float c = Mathf.Lerp(1f, -1f, allowedNear);
		float radians = (float)Math.PI / 180f * (fovy / 2f);
		float cotangent = Mathf.Cos(radians) / Mathf.Sin(radians);
		Matrix4x4 toReturn = default(Matrix4x4);
		float zValA = (c - d) * zNear * zFar * -1f / (zFar - zNear);
		float zValB = (d * zFar - c * zNear) / (zFar - zNear);
		toReturn.m00 = cotangent / aspect;
		toReturn.m01 = 0f;
		toReturn.m02 = 0f;
		toReturn.m03 = 0f;
		toReturn.m10 = 0f;
		toReturn.m11 = cotangent;
		toReturn.m12 = 0f;
		toReturn.m13 = 0f;
		toReturn.m20 = 0f;
		toReturn.m21 = 0f;
		toReturn.m22 = 0f - zValB;
		toReturn.m23 = 0f - zValA;
		toReturn.m30 = 0f;
		toReturn.m31 = 0f;
		toReturn.m32 = -1f;
		toReturn.m33 = 0f;
		return toReturn;
	}

	private void SetMatrixProjection(CameraOverridePass renderPass)
	{
		Camera defaultCamera = CameraUtils.GetMainCamera();
		Camera orthoCamera = CameraUtils.FindFirstByLayer(GameLayer.UI);
		if (!(defaultCamera == null) && !(orthoCamera == null) && !(this == null))
		{
			RectTransform rectTransform = (base.transform.IsChildOf(OverlayUI.Get().m_widthScale.m_Center.parent) ? ((RectTransform)OverlayUI.Get().m_widthScale.m_Center.parent) : ((RectTransform)OverlayUI.Get().m_heightScale.m_Center.parent));
			float distance = orthoCamera.orthographicSize * rectTransform.localScale.y * 0.5f / Mathf.Tan(defaultCamera.fieldOfView * 0.5f * ((float)Math.PI / 180f));
			Vector3 relativePosition = PopupBone.position + Vector3.one * (distance + 100f);
			Matrix4x4 viewMatrix = Matrix4x4.Inverse(Matrix4x4.TRS(new Vector3(orthoCamera.transform.position.x, relativePosition.y, orthoCamera.transform.position.z), orthoCamera.transform.rotation, new Vector3(1f, 1f, -1f)));
			renderPass.OverrideViewMatrix(viewMatrix);
			Matrix4x4 perspectiveMatrix = PerspectiveMtxZRange(defaultCamera.fieldOfView, defaultCamera.aspect, distance - 100f, distance + 100f, m_perspectiveOverrideZNearLimit, m_perspectiveOverrideZFarLimit);
			renderPass.OverrideProjectionMatrix(perspectiveMatrix);
		}
	}
}
