using System;
using System.Collections.Generic;
using System.Diagnostics;
using Blizzard.T5.Core.Utils;
using Hearthstone.UI.Core;
using Hearthstone.UI.Logging;
using UnityEngine;

namespace Hearthstone.UI;

[ExecuteAlways]
[AddComponentMenu("")]
[DisallowMultipleComponent]
public class Maskable : WidgetBehavior, IPopupRendering, IBoundsDependent, IVisibleWidgetComponent
{
	private bool m_initialized;

	private bool m_registeredComponents;

	private Camera m_renderCamera;

	private IPopupRoot m_popupRoot;

	private CameraOverridePass m_cameraMaskPass;

	private CustomViewEntryPoint? m_customvViewEntryPointOverride;

	private bool m_isVisible = true;

	private HashSet<IPopupRendering> m_popupRenderingComponents = new HashSet<IPopupRendering>();

	private bool m_hasAppliedPopupRendering;

	private bool Active
	{
		get
		{
			return base.IsActive;
		}
		set
		{
			if (base.enabled != value)
			{
				base.enabled = value;
			}
		}
	}

	[Overridable]
	public bool IsVisible
	{
		get
		{
			return m_isVisible;
		}
		set
		{
			if (m_isVisible != value)
			{
				SetVisibility(value, isInternal: false);
			}
		}
	}

	public bool NeedsBounds => true;

	public bool IsDesiredHidden => base.Owner.IsDesiredHidden;

	public bool IsDesiredHiddenInHierarchy
	{
		get
		{
			if (base.Owner.IsDesiredHiddenInHierarchy)
			{
				return true;
			}
			return false;
		}
	}

	public bool HandlesChildVisibility => false;

	public override bool IsChangingStates => false;

	public void EnablePopupRendering(IPopupRoot popupRoot)
	{
		if (m_popupRoot != popupRoot)
		{
			if (m_popupRoot != null && m_cameraMaskPass != null)
			{
				m_popupRoot.RemoveMaskPass(m_cameraMaskPass);
			}
			m_popupRoot = popupRoot;
			base.Owner.RegisterActivatedListener(ApplyPopupRendering, popupRoot);
		}
		base.Owner.RegisterReadyListener(ApplyPopupRendering, popupRoot);
	}

	public void DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.CleanupPopupRendering(m_popupRenderingComponents);
			m_popupRoot.RemoveMaskPass(m_cameraMaskPass);
		}
		if (base.Owner != null)
		{
			base.Owner.RemoveReadyListener(ApplyPopupRendering);
		}
		m_popupRoot = null;
		RemovePopupRendering(null);
	}

	public bool HandlesChildPropagation()
	{
		return true;
	}

	private void ApplyPopupRendering(object payload)
	{
		if (!(payload is IPopupRoot popupRoot))
		{
			return;
		}
		SetupMaskingCamera();
		popupRoot.AddMaskablePass(m_cameraMaskPass);
		if (!IsDesiredHiddenInHierarchy && Active && IsVisible)
		{
			CustomViewEntryPoint entryPoint = (CustomViewEntryPoint)popupRoot.GetViewEntryPoint();
			if (m_customvViewEntryPointOverride.HasValue)
			{
				entryPoint = m_customvViewEntryPointOverride.Value;
			}
			else if (entryPoint == CustomViewEntryPoint.Count)
			{
				entryPoint = CustomViewEntryPoint.PerspectivePostFullscreenFX;
			}
			EnableMaskRendering(enable: true, entryPoint);
		}
		popupRoot.ApplyPopupRendering(base.transform, m_popupRenderingComponents, overrideLayer: true, 31, m_isVisible);
		m_hasAppliedPopupRendering = true;
		base.Owner.RemoveReadyListener(ApplyPopupRendering);
	}

	private void RemovePopupRendering(object unused)
	{
		m_hasAppliedPopupRendering = false;
		SetupMaskingCamera();
	}

	public void SetVisibility(bool isVisible, bool isInternal)
	{
		if (!m_initialized || m_cameraMaskPass == null)
		{
			return;
		}
		bool hasVisibilityChanged = m_isVisible != isVisible;
		if (!isInternal)
		{
			m_isVisible = isVisible;
		}
		if (isVisible && Active && !IsDesiredHiddenInHierarchy)
		{
			if (m_popupRoot == null)
			{
				EnableMaskRendering(enable: true, CustomViewEntryPoint.PerspectivePostFullscreenFX);
				return;
			}
			if (hasVisibilityChanged && m_hasAppliedPopupRendering)
			{
				ApplyPopupRendering(m_popupRoot);
			}
			CustomViewEntryPoint entryPoint = (CustomViewEntryPoint)m_popupRoot.GetViewEntryPoint();
			EnableMaskRendering(enable: true, entryPoint);
		}
		else
		{
			EnableMaskRendering(enable: false);
		}
	}

	protected override void OnInitialize()
	{
		if (Application.IsPlaying(this))
		{
			m_initialized = true;
			SetupMaskingCamera();
			if (Active)
			{
				ActivateMask();
			}
		}
	}

	private void LateUpdate()
	{
		UpdateCameraClipping();
	}

	public void Hide()
	{
		SetVisibility(isVisible: false, isInternal: false);
	}

	public void Show()
	{
		SetVisibility(isVisible: true, isInternal: false);
	}

	public override bool GetIsChangingStates(Func<GameObject, bool> includeGameObject)
	{
		return false;
	}

	public void OverrideRenderPassEntryPoint(CustomViewEntryPoint entry)
	{
		m_customvViewEntryPointOverride = entry;
		if (m_cameraMaskPass != null && m_cameraMaskPass.isScheduled)
		{
			EnableMaskRendering(enable: true, m_customvViewEntryPointOverride.Value);
		}
	}

	private void SetupMaskingCamera()
	{
		m_renderCamera = GetRenderCamera();
		if (!(m_renderCamera == null))
		{
			if (m_cameraMaskPass == null)
			{
				m_cameraMaskPass = new CameraOverridePass("UI_Maskable_Camera (Widget: " + base.Owner.name + ")", GetCullingMask());
			}
			else
			{
				m_cameraMaskPass.layerMask = GetCullingMask();
			}
			UpdateCameraClipping();
		}
	}

	public void UpdateCameraClipping()
	{
		WidgetTransform widgetTransform = GetComponent<WidgetTransform>();
		if (!(widgetTransform == null) && m_cameraMaskPass != null && !(m_renderCamera == null))
		{
			Rect wtRect = widgetTransform.Rect;
			Vector3 vector = WorldToViewportPoint(base.transform.TransformPoint(wtRect.xMin, 0f, wtRect.yMin));
			Vector3 v2 = WorldToViewportPoint(base.transform.TransformPoint(wtRect.xMax, 0f, wtRect.yMax));
			float x1 = Mathf.Clamp(vector.x, 0f, 1f);
			float y1 = Mathf.Clamp(vector.y, 0f, 1f);
			float x2 = Mathf.Clamp(v2.x, 0f, 1f);
			float y2 = Mathf.Clamp(v2.y, 0f, 1f);
			Rect cameraRect = new Rect(x1, y1, x2 - x1, y2 - y1);
			if (Mathf.Approximately(0f, cameraRect.height))
			{
				cameraRect.height = Mathf.Epsilon;
			}
			if (Mathf.Approximately(0f, cameraRect.width))
			{
				cameraRect.width = Mathf.Epsilon;
			}
			cameraRect.Set(cameraRect.x * (float)m_renderCamera.pixelWidth, cameraRect.y * (float)m_renderCamera.pixelHeight, cameraRect.width * (float)m_renderCamera.pixelWidth, cameraRect.height * (float)m_renderCamera.pixelHeight);
			m_cameraMaskPass.OverrideScissor(cameraRect);
		}
	}

	private Vector3 WorldToViewportPoint(Vector3 point)
	{
		if (!m_cameraMaskPass.toOverride.HasFlag(CameraOverridePass.OverrideFlags.ProjectionMatrix | CameraOverridePass.OverrideFlags.ViewMatrix))
		{
			return m_renderCamera.WorldToViewportPoint(point);
		}
		Matrix4x4 projectionMatrix = m_renderCamera.projectionMatrix;
		Matrix4x4 viewMatrix = m_renderCamera.worldToCameraMatrix;
		if (m_cameraMaskPass.toOverride.HasFlag(CameraOverridePass.OverrideFlags.ProjectionMatrix))
		{
			projectionMatrix = m_cameraMaskPass.projectionOverride;
		}
		if (m_cameraMaskPass.toOverride.HasFlag(CameraOverridePass.OverrideFlags.ViewMatrix))
		{
			viewMatrix = m_cameraMaskPass.viewMatrixOverride;
		}
		Vector3 screenPosition = (projectionMatrix * viewMatrix).MultiplyPoint(point);
		return new Vector3(screenPosition.x + 1f, screenPosition.y + 1f, screenPosition.z + 1f) / 2f;
	}

	private int GetCullingMask()
	{
		int cullingMask = GameLayer.CameraMask.LayerBit();
		if (m_popupRoot == null)
		{
			cullingMask |= GameLayer.Default.LayerBit();
		}
		return cullingMask;
	}

	private Camera GetRenderCamera()
	{
		if (m_popupRoot != null)
		{
			return m_popupRoot.PopupCamera;
		}
		return CameraUtils.FindFirstByLayer(base.gameObject.layer);
	}

	private void ActivateMask()
	{
		if (!m_initialized)
		{
			return;
		}
		if (!base.Owner.IsInitialized || base.Owner.IsChangingStates)
		{
			base.Owner.RegisterDoneChangingStatesListener(delegate
			{
				ApplyMaskingLayer(base.transform);
				if (!m_registeredComponents)
				{
					RegisterStatefulComponents();
				}
			}, null, callImmediatelyIfSet: true, doOnce: true);
		}
		else
		{
			ApplyMaskingLayer(base.transform);
			if (!m_registeredComponents)
			{
				RegisterStatefulComponents();
			}
		}
		if (!IsDesiredHiddenInHierarchy && IsVisible && Active)
		{
			CustomViewEntryPoint entryPoint = CustomViewEntryPoint.PerspectivePostFullscreenFX;
			if (m_customvViewEntryPointOverride.HasValue)
			{
				entryPoint = m_customvViewEntryPointOverride.Value;
			}
			EnableMaskRendering(enable: true, entryPoint);
		}
		if (m_popupRoot != null)
		{
			m_popupRoot.AddMaskablePass(m_cameraMaskPass);
		}
		FlagForInputDetectionByRenderPass(enable: true);
	}

	private void DeactivateMask()
	{
		EnableMaskRendering(enable: false);
		if (m_popupRoot != null)
		{
			m_popupRoot.RemoveMaskPass(m_cameraMaskPass);
		}
		RemoveMaskingLayer(base.transform);
		FlagForInputDetectionByRenderPass(enable: false);
	}

	private void ApplyMaskingLayer(object root, bool ignoreChildren = false)
	{
		if (!(this == null))
		{
			base.Owner.SetLayerOverrideForObject(GameLayer.CameraMask, base.gameObject);
		}
	}

	private void RemoveMaskingLayer(object root, bool ignoreChildren = false)
	{
		if (!(this == null))
		{
			base.Owner.SetLayerOverrideForObject(GameLayer.Default, base.gameObject);
		}
	}

	private void RegisterStatefulComponents()
	{
		GameObjectUtils.WalkSelfAndChildren(base.transform, delegate(Transform current)
		{
			IStatefulWidgetComponent[] components = current.GetComponents<IStatefulWidgetComponent>();
			bool result = true;
			if (components != null)
			{
				IStatefulWidgetComponent[] array = components;
				foreach (IStatefulWidgetComponent component in array)
				{
					if (!((UnityEngine.Object)component == this) && !(component is VisualController) && !(component is Clickable))
					{
						component.RegisterDoneChangingStatesListener(delegate
						{
							HandleComponentDoneChangingStates(component);
						});
						result = false;
					}
				}
			}
			return result;
		});
		m_registeredComponents = true;
	}

	private void HandleComponentDoneChangingStates(IStatefulWidgetComponent statefulComponent)
	{
		if (Active)
		{
			Component component = (Component)statefulComponent;
			if (component != null)
			{
				ApplyMaskingLayer(component.gameObject);
			}
		}
	}

	private void EnableMaskRendering(bool enable, CustomViewEntryPoint entryPoint = CustomViewEntryPoint.OrthographicsPostFullscreenFX)
	{
		if (enable)
		{
			m_cameraMaskPass?.ChangeSchedule(entryPoint);
		}
		else
		{
			m_cameraMaskPass?.Unschedule();
		}
	}

	private void FlagForInputDetectionByRenderPass(bool enable)
	{
		PegUI pegUI = PegUI.Get();
		if (pegUI != null)
		{
			if (enable)
			{
				pegUI.RegisterForRenderPassPriorityHitTest(this);
			}
			else
			{
				pegUI.UnregisterFromRenderPassPriorityHitTest(this);
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (m_initialized)
		{
			UpdateCameraClipping();
			ActivateMask();
		}
		if (m_popupRoot != null && m_cameraMaskPass != null && m_cameraMaskPass.isScheduled)
		{
			EnableMaskRendering(enable: true, (CustomViewEntryPoint)m_popupRoot.GetViewEntryPoint());
		}
	}

	protected override void OnDisable()
	{
		DeactivateMask();
		base.OnDisable();
	}

	[Conditional("UNITY_EDITOR")]
	private void Log(string message, string type)
	{
		Hearthstone.UI.Logging.Log.Get().AddMessage(message, this, LogLevel.Info, type);
	}
}
