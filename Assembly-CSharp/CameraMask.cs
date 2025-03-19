using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class CameraMask : MonoBehaviour
{
	public enum CAMERA_MASK_UP_VECTOR
	{
		Y,
		Z
	}

	[CustomEditField(Sections = "Mask Settings")]
	public GameObject m_ClipObjects;

	[CustomEditField(Sections = "Mask Settings")]
	public CAMERA_MASK_UP_VECTOR m_UpVector;

	[CustomEditField(Sections = "Mask Settings")]
	public float m_Width = 1f;

	[CustomEditField(Sections = "Mask Settings")]
	public float m_Height = 1f;

	[CustomEditField(Sections = "Mask Settings")]
	public bool m_RealtimeUpdate;

	[CustomEditField(Sections = "Render Camera")]
	public bool m_UseCameraFromLayer;

	[CustomEditField(Sections = "Render Camera", Parent = "m_UseCameraFromLayer")]
	public GameLayer m_CameraFromLayer;

	[CustomEditField(Sections = "Render Camera")]
	public List<GameLayer> m_CullingMasks = new List<GameLayer>
	{
		GameLayer.Default,
		GameLayer.IgnoreFullScreenEffects
	};

	[CustomEditField(Sections = "Render Camera")]
	public CustomViewEntryPoint RenderEntryPoint = CustomViewEntryPoint.PerspectivePostFullscreenFX;

	private Camera m_renderCamera;

	private CameraOverridePass m_cameraMaskPass;

	private void OnEnable()
	{
		Init();
		ActivateMask();
	}

	private void OnDisable()
	{
		DeactivateMask();
	}

	private void Update()
	{
		if (m_RealtimeUpdate)
		{
			UpdateCameraClipping();
		}
	}

	private void OnDrawGizmos()
	{
		Matrix4x4 m = default(Matrix4x4);
		if (m_UpVector == CAMERA_MASK_UP_VECTOR.Z)
		{
			m.SetTRS(base.transform.position, Quaternion.identity, base.transform.lossyScale);
		}
		else
		{
			m.SetTRS(base.transform.position, Quaternion.Euler(90f, 0f, 0f), base.transform.lossyScale);
		}
		Gizmos.matrix = m;
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(m_Width, m_Height, 0f));
		Gizmos.matrix = Matrix4x4.identity;
	}

	[ContextMenu("UpdateMask")]
	public void UpdateMask()
	{
		UpdateCameraClipping();
	}

	private bool Init()
	{
		if (m_UseCameraFromLayer)
		{
			m_renderCamera = CameraUtils.FindFirstByLayer(m_CameraFromLayer);
		}
		else
		{
			m_renderCamera = CameraUtils.FindProjectionCameraForObject(base.gameObject);
		}
		if (m_renderCamera == null)
		{
			return false;
		}
		int cullingMask = GameLayer.CameraMask.LayerBit();
		foreach (GameLayer mask in m_CullingMasks)
		{
			cullingMask |= mask.LayerBit();
		}
		m_cameraMaskPass = new CameraOverridePass("CameraMask: " + base.gameObject.name, cullingMask);
		return true;
	}

	private void UpdateCameraClipping()
	{
		if (!(m_renderCamera == null) || Init())
		{
			Vector3 p1 = Vector3.zero;
			Vector3 p2 = Vector3.zero;
			if (m_UpVector == CAMERA_MASK_UP_VECTOR.Y)
			{
				p1 = new Vector3(base.transform.position.x - m_Width * 0.5f * base.transform.lossyScale.x, base.transform.position.y, base.transform.position.z - m_Height * 0.5f * base.transform.lossyScale.z);
				p2 = new Vector3(base.transform.position.x + m_Width * 0.5f * base.transform.lossyScale.x, base.transform.position.y, base.transform.position.z + m_Height * 0.5f * base.transform.lossyScale.z);
			}
			else
			{
				p1 = new Vector3(base.transform.position.x - m_Width * 0.5f * base.transform.lossyScale.x, base.transform.position.y - m_Height * 0.5f * base.transform.lossyScale.y, base.transform.position.z);
				p2 = new Vector3(base.transform.position.x + m_Width * 0.5f * base.transform.lossyScale.x, base.transform.position.y + m_Height * 0.5f * base.transform.lossyScale.y, base.transform.position.z);
			}
			Vector3 vector = m_renderCamera.WorldToViewportPoint(p1);
			Vector3 v2 = m_renderCamera.WorldToViewportPoint(p2);
			float x1 = Mathf.Clamp(vector.x, 0f, 1f);
			float y1 = Mathf.Clamp(vector.y, 0f, 1f);
			float x2 = Mathf.Clamp(v2.x, 0f, 1f);
			float y2 = Mathf.Clamp(v2.y, 0f, 1f);
			Rect cameraRect = new Rect(x1, y1, x2 - x1, y2 - y1);
			if (!Mathf.Approximately(0f, cameraRect.height) && !Mathf.Approximately(0f, cameraRect.width))
			{
				cameraRect.Set(cameraRect.x * (float)m_renderCamera.pixelWidth, cameraRect.y * (float)m_renderCamera.pixelHeight, cameraRect.width * (float)m_renderCamera.pixelWidth, cameraRect.height * (float)m_renderCamera.pixelHeight);
				m_cameraMaskPass.OverrideScissor(cameraRect);
			}
		}
	}

	private void ActivateMask()
	{
		if (m_cameraMaskPass != null)
		{
			if (m_ClipObjects != null)
			{
				LayerUtils.SetLayer(m_ClipObjects, GameLayer.CameraMask);
			}
			m_cameraMaskPass.Schedule(RenderEntryPoint);
		}
	}

	private void DeactivateMask()
	{
		if (m_cameraMaskPass != null)
		{
			m_cameraMaskPass.Unschedule();
			m_cameraMaskPass = null;
		}
	}
}
