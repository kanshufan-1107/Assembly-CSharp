using Blizzard.T5.Services;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MainCamera : MonoBehaviour
{
	public Camera m_camera;

	private void Awake()
	{
		if (m_camera == null)
		{
			return;
		}
		Camera c = CameraUtils.FindFirstByLayer(GameLayer.BattleNet);
		if (c != null)
		{
			UniversalAdditionalCameraData data = m_camera.GetUniversalAdditionalCameraData();
			if (!data.cameraStack.Contains(c))
			{
				data.cameraStack.Add(c);
			}
		}
		m_camera.allowMSAA = ServiceManager.Get<IGraphicsManager>().AllowMSAA();
		CameraManager.Get().BaseCamera = m_camera;
	}

	private void OnDestroy()
	{
		if (CameraManager.IsInitialized())
		{
			CameraManager.Get().BaseCamera = null;
		}
	}
}
