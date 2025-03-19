using UnityEngine;

public class CameraUtils
{
	private static Camera[] s_cameras = new Camera[20];

	public static Camera FindFirstByLayer(int layer)
	{
		return FindFirstByLayerMask(1 << layer);
	}

	public static Camera FindFirstByLayer(GameLayer layer)
	{
		return FindFirstByLayerMask(layer.LayerBit());
	}

	public static Camera FindFirstByLayerMask(LayerMask mask)
	{
		int num = s_cameras.Length;
		int allCameras = Camera.allCamerasCount;
		if (num < allCameras)
		{
			s_cameras = new Camera[allCameras];
		}
		int cameraCount = Camera.GetAllCameras(s_cameras);
		for (int i = 0; i < cameraCount; i++)
		{
			Camera camera = s_cameras[i];
			if ((camera.cullingMask & (int)mask) != 0)
			{
				return camera;
			}
		}
		return null;
	}

	public static Camera FindProjectionCameraForObject(GameObject obj)
	{
		if (OverlayUI.Get().HasObject(obj))
		{
			return OverlayUI.Get().m_UICamera;
		}
		return GetMainCamera();
	}

	public static Camera FindFullScreenEffectsCamera(bool activeOnly)
	{
		GameObject fxCameraObj = GameObject.FindGameObjectWithTag("MainCamera");
		if (fxCameraObj == null)
		{
			return null;
		}
		FullScreenEffects fullscreenFX = fxCameraObj.GetComponent<FullScreenEffects>();
		if (fullscreenFX == null)
		{
			return null;
		}
		if (!activeOnly || fullscreenFX.IsActive)
		{
			return fullscreenFX.Camera;
		}
		return null;
	}

	public static Plane CreateTopPlane(Camera camera)
	{
		Vector3 nearPointTL = camera.ViewportToWorldPoint(new Vector3(0f, 1f, camera.nearClipPlane));
		Vector3 nearPointTR = camera.ViewportToWorldPoint(new Vector3(1f, 1f, camera.nearClipPlane));
		Vector3 planeNormal = Vector3.Cross(camera.ViewportToWorldPoint(new Vector3(0f, 1f, camera.farClipPlane)) - nearPointTL, nearPointTR - nearPointTL);
		planeNormal.Normalize();
		return new Plane(planeNormal, nearPointTL);
	}

	public static Plane CreateBottomPlane(Camera camera)
	{
		Vector3 nearPointBL = camera.ViewportToWorldPoint(new Vector3(0f, 0f, camera.nearClipPlane));
		Vector3 nearPointBR = camera.ViewportToWorldPoint(new Vector3(1f, 0f, camera.nearClipPlane));
		Vector3 planeNormal = Vector3.Cross(camera.ViewportToWorldPoint(new Vector3(0f, 0f, camera.farClipPlane)) - nearPointBL, nearPointBR - nearPointBL);
		planeNormal.Normalize();
		return new Plane(planeNormal, nearPointBL);
	}

	public static Plane CreateLeftPlane(Camera camera)
	{
		Vector3 nearPointTL = camera.ViewportToWorldPoint(new Vector3(0f, 1f, camera.nearClipPlane));
		Vector3 nearPointBL = camera.ViewportToWorldPoint(new Vector3(0f, 0f, camera.nearClipPlane));
		Vector3 planeNormal = Vector3.Cross(camera.ViewportToWorldPoint(new Vector3(0f, 1f, camera.farClipPlane)) - nearPointTL, nearPointBL - nearPointTL);
		planeNormal.Normalize();
		return new Plane(planeNormal, nearPointTL);
	}

	public static Plane CreateRightPlane(Camera camera)
	{
		Vector3 nearPointBR = camera.ViewportToWorldPoint(new Vector3(1f, 0f, camera.nearClipPlane));
		Vector3 nearPointTR = camera.ViewportToWorldPoint(new Vector3(1f, 1f, camera.nearClipPlane));
		Vector3 planeNormal = Vector3.Cross(camera.ViewportToWorldPoint(new Vector3(1f, 0f, camera.farClipPlane)) - nearPointBR, nearPointTR - nearPointBR);
		planeNormal.Normalize();
		return new Plane(planeNormal, nearPointBR);
	}

	public static Bounds GetNearClipBounds(Camera camera)
	{
		Vector3 center = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.nearClipPlane));
		Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0f, 0f, camera.nearClipPlane));
		Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1f, 1f, camera.nearClipPlane));
		Vector3 size = new Vector3(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y, topRight.z - bottomLeft.z);
		return new Bounds(center, size);
	}

	public static Bounds GetFarClipBounds(Camera camera)
	{
		Vector3 center = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.farClipPlane));
		Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0f, 0f, camera.farClipPlane));
		Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1f, 1f, camera.farClipPlane));
		Vector3 size = new Vector3(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y, topRight.z - bottomLeft.z);
		return new Bounds(center, size);
	}

	public static Rect CreateGUIViewportRect(Camera camera, Component topLeft, Component bottomRight)
	{
		return CreateGUIViewportRect(camera, topLeft.transform.position, bottomRight.transform.position);
	}

	public static Rect CreateGUIViewportRect(Camera camera, Vector3 worldTopLeft, Vector3 worldBottomRight)
	{
		Vector3 topLeft = camera.WorldToViewportPoint(worldTopLeft);
		Vector3 bottomRight = camera.WorldToViewportPoint(worldBottomRight);
		return new Rect(topLeft.x, 1f - topLeft.y, bottomRight.x - topLeft.x, topLeft.y - bottomRight.y);
	}

	public static Rect CreateGUIScreenRect(Camera camera, Component topLeft, Component bottomRight)
	{
		return CreateGUIScreenRect(camera, topLeft.transform.position, bottomRight.transform.position);
	}

	public static Rect CreateGUIScreenRect(Camera camera, Vector3 worldTopLeft, Vector3 worldBottomRight)
	{
		Vector3 topLeft = camera.WorldToScreenPoint(worldTopLeft);
		Vector3 bottomRight = camera.WorldToScreenPoint(worldBottomRight);
		return new Rect(topLeft.x, bottomRight.y, bottomRight.x - topLeft.x, topLeft.y - bottomRight.y);
	}

	public static bool Raycast(Camera camera, Vector3 screenPoint, LayerMask layerMask, out RaycastHit hitInfo, CameraOverridePass cameraOverride = null)
	{
		if (camera == null)
		{
			hitInfo = default(RaycastHit);
			return false;
		}
		hitInfo = default(RaycastHit);
		if (cameraOverride != null && cameraOverride.toOverride.HasFlag(CameraOverridePass.OverrideFlags.Scissor))
		{
			if (!cameraOverride.scissorOverride.Contains(screenPoint))
			{
				return false;
			}
		}
		else if (!camera.pixelRect.Contains(screenPoint))
		{
			return false;
		}
		Ray ray;
		if (cameraOverride != null && cameraOverride.toOverride.HasFlag(CameraOverridePass.OverrideFlags.ProjectionMatrix))
		{
			Camera perspectiveCam = GetMainCamera();
			if (perspectiveCam == null)
			{
				hitInfo = default(RaycastHit);
				return false;
			}
			ray = perspectiveCam.ScreenPointToRay(screenPoint);
			ray.origin = camera.transform.position;
		}
		else
		{
			ray = camera.ScreenPointToRay(screenPoint);
		}
		return Physics.Raycast(ray, out hitInfo, camera.farClipPlane, layerMask);
	}

	public static int RaycastAll(Camera camera, Vector3 screenPoint, LayerMask layerMask, ref RaycastHit[] hitInfos)
	{
		if (!camera.pixelRect.Contains(screenPoint))
		{
			return 0;
		}
		Ray ray = camera.ScreenPointToRay(screenPoint);
		int hitNumber;
		for (hitNumber = Physics.RaycastNonAlloc(ray, hitInfos, camera.farClipPlane, layerMask); hitNumber == hitInfos.Length; hitNumber = Physics.RaycastNonAlloc(ray, hitInfos, camera.farClipPlane, layerMask))
		{
			int newSize = hitInfos.Length * 2;
			hitInfos = new RaycastHit[newSize];
		}
		return hitNumber;
	}

	public static GameObject CreateInputBlocker(Camera camera, string name)
	{
		return CreateInputBlocker(camera, name, null, null, 0f);
	}

	public static GameObject CreateInputBlocker(Camera camera, string name, Component parent)
	{
		return CreateInputBlocker(camera, name, parent, parent, 0f);
	}

	public static GameObject CreateInputBlocker(Camera camera, string name, Component parent, float worldOffset)
	{
		return CreateInputBlocker(camera, name, parent, parent, worldOffset);
	}

	public static GameObject CreateInputBlocker(Camera camera, string name, Component parent, Component relative, float worldOffset)
	{
		GameObject inputBlocker = new GameObject(name);
		inputBlocker.layer = camera.gameObject.layer;
		inputBlocker.transform.parent = ((parent == null) ? null : parent.transform);
		inputBlocker.transform.localScale = Vector3.one;
		inputBlocker.transform.rotation = Quaternion.Inverse(camera.transform.rotation);
		if (relative == null)
		{
			inputBlocker.transform.position = GetPosInFrontOfCamera(camera, camera.nearClipPlane + worldOffset);
		}
		else
		{
			inputBlocker.transform.position = GetPosInFrontOfCamera(camera, relative.transform.position, worldOffset);
		}
		Bounds farClipBounds = GetFarClipBounds(camera);
		Vector3 parentWorldScale = ((!(parent == null)) ? TransformUtil.ComputeWorldScale(parent) : Vector3.one);
		Vector3 colliderSize = default(Vector3);
		colliderSize.x = farClipBounds.size.x / parentWorldScale.x;
		if (farClipBounds.size.z > 0f)
		{
			colliderSize.y = farClipBounds.size.z / parentWorldScale.z;
		}
		else
		{
			colliderSize.y = farClipBounds.size.y / parentWorldScale.y;
		}
		inputBlocker.AddComponent<BoxCollider>().size = colliderSize;
		return inputBlocker;
	}

	public static float ScreenToWorldDist(Camera camera, float screenDist)
	{
		return ScreenToWorldDist(camera, screenDist, camera.nearClipPlane);
	}

	public static float ScreenToWorldDist(Camera camera, float screenDist, float worldDist)
	{
		Vector3 startPointWorld = camera.ScreenToWorldPoint(new Vector3(0f, 0f, worldDist));
		return camera.ScreenToWorldPoint(new Vector3(screenDist, 0f, worldDist)).x - startPointWorld.x;
	}

	public static float ScreenToWorldDist(Camera camera, float screenDist, Vector3 worldPoint)
	{
		float worldDist = Vector3.Distance(camera.transform.position, worldPoint);
		return ScreenToWorldDist(camera, screenDist, worldDist);
	}

	public static Vector3 GetPosInFrontOfCamera(Camera camera, float worldDistance)
	{
		Vector3 worldSourcePoint = camera.transform.position + new Vector3(0f, 0f, worldDistance);
		float localDistance = camera.transform.InverseTransformPoint(worldSourcePoint).magnitude;
		Vector3 localDestinationPoint = new Vector3(0f, 0f, localDistance);
		return camera.transform.TransformPoint(localDestinationPoint);
	}

	public static Vector3 GetPosInFrontOfCamera(Camera camera, Vector3 worldPoint, float worldOffset)
	{
		Vector3 camPosition = camera.transform.position;
		Vector3 camForward = camera.transform.forward;
		Vector3 offsetVector = (new Plane(-camForward, worldPoint).GetDistanceToPoint(camPosition) + worldOffset) * camForward;
		return camPosition + offsetVector;
	}

	public static Camera GetMainCamera()
	{
		if (Application.isPlaying && Box.Get() != null)
		{
			return Box.Get().GetCamera();
		}
		if (Application.isPlaying && BoardCameras.Get() != null)
		{
			return BoardCameras.Get().GetComponentInChildren<Camera>();
		}
		return Camera.main;
	}

	public static Ray ScreenPointToRayWithCameraPass(Camera cam, Vector2 mousePosition, CameraOverridePass cameraPass)
	{
		if (cameraPass != null && cameraPass.toOverride.HasFlag(CameraOverridePass.OverrideFlags.ProjectionMatrix))
		{
			Ray ray = GetMainCamera().ScreenPointToRay(mousePosition);
			ray.origin = cam.transform.position;
			return ray;
		}
		return cam.ScreenPointToRay(mousePosition);
	}
}
