using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("INTERNAL USE ONLY. Do not put this on your FSMs.")]
public abstract class CameraAction : FsmStateAction
{
	public enum WhichCamera
	{
		MAIN,
		SPECIFIC,
		NAMED
	}

	protected Camera m_namedCamera;

	protected Camera GetCamera(WhichCamera which, FsmGameObject specificCamera, FsmString namedCamera)
	{
		switch (which)
		{
		case WhichCamera.SPECIFIC:
			if (!specificCamera.IsNone)
			{
				return specificCamera.Value.GetComponent<Camera>();
			}
			break;
		case WhichCamera.NAMED:
		{
			string cameraName = (namedCamera.IsNone ? null : namedCamera.Value);
			if (string.IsNullOrEmpty(cameraName))
			{
				break;
			}
			if ((bool)m_namedCamera)
			{
				if (m_namedCamera.name == cameraName)
				{
					return m_namedCamera;
				}
				m_namedCamera = null;
			}
			Camera camera = null;
			GameObject namedObj = GameObject.Find(cameraName);
			if ((bool)namedObj)
			{
				camera = namedObj.GetComponent<Camera>();
			}
			if ((bool)camera)
			{
				m_namedCamera = camera;
				return m_namedCamera;
			}
			break;
		}
		}
		return Camera.main;
	}
}
