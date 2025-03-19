using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

namespace Hearthstone.UI;

public abstract class UIContext : MonoBehaviour
{
	public enum RenderCameraType
	{
		Box,
		Board,
		OrthographicUI,
		PerspectiveUI,
		HighPriorityUI,
		IgnoreFullScreenEffects,
		BackgroundUI
	}

	public enum BlurType
	{
		None,
		Layered,
		Legacy
	}

	public enum ProjectionType
	{
		Orthographic,
		Perspective
	}

	public class PopupRecord
	{
		public GameObject PopupInstance { get; private set; }

		public RenderCameraType CameraType { get; private set; }

		public Camera RenderCamera { get; private set; }

		public PopupRoot PopupRoot { get; private set; }

		public BlurType BlurType { get; private set; }

		public PopupRecord(GameObject popupInstance, PopupRoot popupRoot, RenderCameraType cameraType, Camera renderCamera, BlurType blurType)
		{
			PopupInstance = popupInstance;
			PopupRoot = popupRoot;
			CameraType = cameraType;
			RenderCamera = renderCamera;
			BlurType = blurType;
		}
	}

	private const float BlurFadeTime = 0.5f;

	private const float BlurFadeOutTime = 0.2f;

	private const float TopPopupStartingDistance = -50f;

	public const float DistanceBetweenPopups = 100f;

	private static readonly Map<GameLayer, RenderCameraType> m_layerToCameraTypeMap = new Map<GameLayer, RenderCameraType>
	{
		{
			GameLayer.UI,
			RenderCameraType.OrthographicUI
		},
		{
			GameLayer.HighPriorityUI,
			RenderCameraType.HighPriorityUI
		},
		{
			GameLayer.PerspectiveUI,
			RenderCameraType.PerspectiveUI
		},
		{
			GameLayer.BackgroundUI,
			RenderCameraType.BackgroundUI
		},
		{
			GameLayer.IgnoreFullScreenEffects,
			RenderCameraType.IgnoreFullScreenEffects
		}
	};

	private static UIContext s_uiContext;

	private List<PopupRecord> m_popupStack = new List<PopupRecord>();

	public static UIContext GetRoot()
	{
		if (s_uiContext == null)
		{
			GameObject uiContext = new GameObject("UIContext");
			uiContext.AddComponent<HSDontDestroyOnLoad>();
			if (!Application.isPlaying)
			{
				s_uiContext = uiContext.AddComponent<UIContextEditMode>();
			}
			else
			{
				s_uiContext = uiContext.AddComponent<UIContextPlayMode>();
			}
		}
		return s_uiContext;
	}

	public PopupRoot ShowPopup(GameObject popupInstance, BlurType blurType = BlurType.Layered, ProjectionType projection = ProjectionType.Orthographic)
	{
		if (popupInstance == null || !Application.IsPlaying(popupInstance))
		{
			return null;
		}
		PopupRoot popupRoot = popupInstance.GetComponent<PopupRoot>() ?? popupInstance.AddComponent<PopupRoot>();
		if (m_popupStack.Exists((PopupRecord s) => s.PopupRoot == popupRoot))
		{
			return popupRoot;
		}
		if (blurType != 0)
		{
			foreach (PopupRecord popup in m_popupStack)
			{
				if (popup.PopupRoot != null)
				{
					CustomViewEntryPoint preFxEntryPoint = GetPreFxEntryPointForPopup(popup.PopupRoot);
					popup.PopupRoot.ChangeRenderingSchedule(preFxEntryPoint);
					popup.PopupRoot.UpdateRenderingPassOverrides();
				}
			}
		}
		Camera popupCamera = GetPopupCamera(popupRoot);
		Transform popupBone = CreatePopupBone(popupInstance);
		popupRoot.PopupBone = popupBone;
		popupRoot.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
		popupRoot.OnDisabled -= HandlePopupDestroyedOrDisabled;
		popupRoot.OnDisabled += HandlePopupDestroyedOrDisabled;
		popupRoot.IsPerspectivePopup = !IsWidgetOnOrthographicCamera(popupRoot.gameObject);
		CameraOverridePass popupPass = CreateRenderPass(popupRoot);
		CustomViewEntryPoint entryPoint = GetPostFxEntryPointForPopup(popupRoot);
		popupRoot.ProjectionType = projection;
		popupRoot.Initialize(popupCamera, popupPass);
		popupRoot.ChangeRenderingSchedule(entryPoint);
		RegisterPopupInternal(popupInstance, popupRoot, GetCameraTypeFromLayer((GameLayer)popupInstance.gameObject.layer), popupCamera, blurType);
		UpdateStackPositions();
		popupRoot.UpdateRenderingPassOverrides();
		if (popupInstance.TryGetComponent<WidgetTemplate>(out var template))
		{
			template.SetPopupRoot(popupRoot);
		}
		return popupRoot;
	}

	public void DismissPopup(GameObject popupInstance)
	{
		if (!(popupInstance == null) && Application.IsPlaying(popupInstance))
		{
			PopupRoot popupRoot = popupInstance.GetComponent<PopupRoot>();
			if (!(popupRoot == null))
			{
				DismissPopupInternal(popupRoot);
			}
		}
	}

	public void DismissPopupsRecursive(GameObject root)
	{
		PopupRoot[] componentsInChildren = root.GetComponentsInChildren<PopupRoot>();
		foreach (PopupRoot popup in componentsInChildren)
		{
			DismissPopupInternal(popup);
		}
	}

	public void RegisterPopup(GameObject popupInstance, RenderCameraType cameraType, BlurType blurType = BlurType.Layered)
	{
		Camera renderCamera = FindTemplateCamera(popupInstance, cameraType);
		if (renderCamera == null)
		{
			Log.UIFramework.PrintError("UIContext.RegisterPopup: Unable to find suitable camera.");
		}
		else
		{
			RegisterPopupInternal(popupInstance, null, cameraType, renderCamera, blurType);
		}
	}

	public void UnregisterPopup(GameObject popupInstance)
	{
		PopupRecord record = null;
		for (int i = 0; i < m_popupStack.Count; i++)
		{
			if (m_popupStack[i].PopupInstance == popupInstance)
			{
				record = m_popupStack[i];
				m_popupStack.RemoveAt(i);
				break;
			}
		}
		if (record != null && !(record.PopupRoot == null))
		{
			PopupRoot popupRoot = record.PopupRoot;
			Transform popupBone = popupRoot.PopupBone;
			Transform popupInstanceTransform = null;
			Transform originalParent = null;
			if (popupInstance != null)
			{
				popupInstanceTransform = popupInstance.transform;
			}
			if (popupBone != null)
			{
				originalParent = popupBone.parent;
			}
			if (!popupRoot.IsBeingDisabledOrDestroyed)
			{
				CleanupPopupBone(popupInstanceTransform, popupBone, originalParent);
			}
			else
			{
				StartCoroutine(DelayedCleanupPopupBone(popupInstanceTransform, popupBone, originalParent));
			}
			if (popupRoot.FullscreenEffectsHandle == null)
			{
				popupRoot.FullscreenEffectsHandle = new ScreenEffectsHandle(popupRoot);
			}
			if (record.BlurType != 0)
			{
				popupRoot.FullscreenEffectsHandle.StopEffect();
			}
		}
	}

	private IEnumerator DelayedCleanupPopupBone(Transform popupInstance, Transform popupBone, Transform originalParent)
	{
		yield return null;
		yield return new WaitForEndOfFrame();
		CleanupPopupBone(popupInstance, popupBone, originalParent);
	}

	private static void CleanupPopupBone(Transform popupInstance, Transform popupBone, Transform originalParent)
	{
		if (popupInstance != null)
		{
			Transform transformToReparent = popupInstance;
			PopupRoot popupRoot = popupInstance.GetComponent<PopupRoot>();
			if (popupRoot != null && popupRoot.PopupBone != null && popupRoot.PopupBone != popupBone)
			{
				transformToReparent = popupRoot.PopupBone;
			}
			Vector3 localPosition = transformToReparent.localPosition;
			if (originalParent != null)
			{
				transformToReparent.SetParent(originalParent, worldPositionStays: true);
				transformToReparent.localPosition = localPosition;
			}
		}
		if (popupBone != null)
		{
			Object.Destroy(popupBone.gameObject);
		}
	}

	public PopupRecord GetLatestPopup()
	{
		int i = m_popupStack.Count - 1;
		while (m_popupStack.Count > 0 && i >= 0)
		{
			if (m_popupStack[i].PopupInstance == null)
			{
				m_popupStack.RemoveAt(i);
				i--;
				continue;
			}
			return m_popupStack[i];
		}
		return null;
	}

	public List<PopupRecord> GetPopupsDescendingOrder()
	{
		List<PopupRecord> popups = new List<PopupRecord>(m_popupStack.Count);
		for (int i = m_popupStack.Count - 1; i >= 0; i--)
		{
			if (m_popupStack[i].PopupInstance == null)
			{
				m_popupStack.RemoveAt(i);
			}
			else
			{
				popups.Add(m_popupStack[i]);
			}
		}
		return popups;
	}

	private void HandlePopupDestroyedOrDisabled(PopupRoot popupRoot)
	{
		DismissPopupInternal(popupRoot);
	}

	private void DismissPopupInternal(PopupRoot popupRoot)
	{
		if (!(popupRoot == null) && IsPopupRegistered(popupRoot.gameObject))
		{
			popupRoot.DisablePopupRendering();
			UnregisterPopup(popupRoot.gameObject);
			if (PegUI.Get() != null)
			{
				PegUI.Get().UnregisterFromRenderPassPriorityHitTest(popupRoot);
			}
			if (OverlayUI.Get() != null && popupRoot.gameObject != null && IsWidgetOnOrthographicCamera(popupRoot.gameObject))
			{
				UpdateStackPositions();
			}
			if (!popupRoot.IsBeingDisabledOrDestroyed)
			{
				Object.DestroyImmediate(popupRoot);
			}
			if (m_popupStack.Count != 0)
			{
				PopupRoot topMostPopup = m_popupStack[m_popupStack.Count - 1].PopupRoot;
				CustomViewEntryPoint entryPoint = GetPostFxEntryPointForPopup(topMostPopup);
				topMostPopup.ChangeRenderingSchedule(entryPoint);
				topMostPopup.UpdateRenderingPassOverrides();
			}
		}
	}

	private void RegisterPopupInternal(GameObject popupInstance, PopupRoot popupRoot, RenderCameraType cameraType, Camera camera, BlurType blurType)
	{
		if (IsPopupRegistered(popupInstance))
		{
			return;
		}
		PopupRecord popupRecord = new PopupRecord(popupInstance, popupRoot, cameraType, camera, blurType);
		m_popupStack.Add(popupRecord);
		if (!(popupRoot == null))
		{
			if (popupRoot.FullscreenEffectsHandle == null)
			{
				popupRoot.FullscreenEffectsHandle = new ScreenEffectsHandle(popupRoot);
			}
			if (PegUI.Get() != null)
			{
				PegUI.Get().RegisterForRenderPassPriorityHitTest(popupRoot);
			}
			if (blurType != 0)
			{
				ScreenEffectParameters parameters = (popupRoot.IsPerspectivePopup ? ScreenEffectParameters.BlurVignettePerspective : ScreenEffectParameters.BlurVignetteOrthographic);
				popupRoot.FullscreenEffectsHandle.StartEffect(parameters);
			}
		}
	}

	private void OnDisable()
	{
		for (int i = m_popupStack.Count - 1; i >= 0; i--)
		{
			PopupRecord record = m_popupStack[i];
			if (record.PopupRoot != null)
			{
				record.PopupRoot.OnDisabled -= HandlePopupDestroyedOrDisabled;
			}
		}
	}

	private bool IsPopupRegistered(GameObject popupInstance)
	{
		foreach (PopupRecord item in m_popupStack)
		{
			if (item.PopupInstance == popupInstance)
			{
				return true;
			}
		}
		return false;
	}

	private int CountPopupsWithBlurInstances(BlurType blurType)
	{
		int count = 0;
		foreach (PopupRecord item in m_popupStack)
		{
			if (item.BlurType == blurType)
			{
				count++;
			}
		}
		return count;
	}

	private float GetOrthoZBufferValue(float distance, float zNear, float zFar)
	{
		return (distance - zFar) / (zNear - zFar);
	}

	private void UpdateStackPositions()
	{
		for (int i = 0; i < m_popupStack.Count; i++)
		{
			if (IsWidgetOnOrthographicCamera(m_popupStack[i].PopupInstance))
			{
				PopupRoot popupRoot = m_popupStack[i].PopupRoot;
				Vector3 position = popupRoot.PopupBone.position;
				position.y = -50f + (float)(i - m_popupStack.Count) * 100f;
				popupRoot.PopupBone.position = position;
				if (popupRoot.NeedsProjectionMatrixOverride())
				{
					Camera cam = m_popupStack[i].RenderCamera;
					float distance = Vector3.Magnitude(position - cam.transform.position);
					float nearPlane = GetOrthoZBufferValue(distance - 50f, cam.nearClipPlane, cam.farClipPlane);
					float farPlane = GetOrthoZBufferValue(distance + 50f, cam.nearClipPlane, cam.farClipPlane);
					popupRoot.SetOverrideProjectionMatrixZRange(nearPlane, farPlane);
				}
				popupRoot.UpdateRenderingPassOverrides();
			}
		}
	}

	private bool IsWidgetOnOrthographicCamera(GameObject widget)
	{
		return OverlayUI.Get().HasObject(widget);
	}

	private CustomViewEntryPoint GetPreFxEntryPointForPopup(PopupRoot popupRoot)
	{
		if (!popupRoot.IsPerspectivePopup)
		{
			return CustomViewEntryPoint.OrthographicPreFullscreenFX;
		}
		return CustomViewEntryPoint.PerspectivePreFullscreenFX;
	}

	private CustomViewEntryPoint GetPostFxEntryPointForPopup(PopupRoot popupRoot)
	{
		if (!popupRoot.IsPerspectivePopup)
		{
			return CustomViewEntryPoint.OrthographicsPostFullscreenFX;
		}
		return CustomViewEntryPoint.PerspectivePostFullscreenFX;
	}

	private Transform CreatePopupBone(GameObject popupInstance)
	{
		Transform rootParent = popupInstance.transform.parent;
		GameObject newRoot = new GameObject(popupInstance.name + " Popup Bone");
		newRoot.transform.SetParent(rootParent, worldPositionStays: true);
		if (rootParent != null)
		{
			newRoot.transform.position = rootParent.transform.position;
			newRoot.transform.rotation = rootParent.transform.rotation;
			newRoot.transform.localScale = Vector3.one;
		}
		popupInstance.transform.SetParent(newRoot.transform, worldPositionStays: true);
		return newRoot.transform;
	}

	private CameraOverridePass CreateRenderPass(PopupRoot popupRoot)
	{
		if (popupRoot.PrimaryRenderPass != null)
		{
			return popupRoot.PrimaryRenderPass;
		}
		CustomViewEntryPoint initialEntryPoint = CustomViewEntryPoint.OrthographicsPostFullscreenFX;
		CameraOverridePass popupPass = CameraPassProvider.RequestPass("UI_Popup_Camera (Widget: " + base.name + ")", 536870944, initialEntryPoint);
		if (popupPass == null)
		{
			return null;
		}
		popupRoot.RenderingLayerMask = popupPass.renderLayerMaskOverride;
		return popupPass;
	}

	private Camera FindTemplateCamera(GameObject popupInstance, RenderCameraType cameraType)
	{
		if (Application.IsPlaying(popupInstance))
		{
			OverlayUI overlayUI = OverlayUI.Get();
			if (overlayUI != null && overlayUI.HasObject(popupInstance))
			{
				return overlayUI.m_UICamera;
			}
			if (cameraType == RenderCameraType.Box)
			{
				Box box = Box.Get();
				if (box != null && box.GetCamera() != null)
				{
					return box.GetCamera();
				}
			}
			if (cameraType == RenderCameraType.Board)
			{
				BoardCameras boardCameras = BoardCameras.Get();
				if (boardCameras != null)
				{
					Camera boardCamera = boardCameras.GetComponentInChildren<Camera>();
					if (boardCamera != null)
					{
						return boardCamera;
					}
				}
			}
			if (cameraType == RenderCameraType.IgnoreFullScreenEffects)
			{
				return CameraUtils.FindFirstByLayer(GameLayer.IgnoreFullScreenEffects);
			}
		}
		return Camera.main;
	}

	private RenderCameraType GetCameraTypeFromLayer(GameLayer gameLayer)
	{
		if (m_layerToCameraTypeMap.TryGetValue(gameLayer, out var cameraType))
		{
			return cameraType;
		}
		if (SceneMgr.Get() != null && SceneMgr.Get().IsInGame())
		{
			return RenderCameraType.Board;
		}
		return RenderCameraType.Box;
	}

	private Camera GetPopupCamera(PopupRoot popupRoot)
	{
		return CameraUtils.FindFirstByLayer(IsWidgetOnOrthographicCamera(popupRoot.gameObject) ? GameLayer.BattleNet : GameLayer.Default);
	}
}
