using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraManager : IService
{
	private static CameraManager s_instance;

	private Camera m_pegUICamera;

	private UniversalAdditionalCameraData m_pegUICameraData;

	private bool m_haveUICamera;

	private int m_cameraCount;

	private Camera m_baseCamera;

	public Camera BaseCamera
	{
		set
		{
			m_baseCamera = value;
			if (m_baseCamera != null)
			{
				UniversalAdditionalCameraData data = m_baseCamera.GetUniversalAdditionalCameraData();
				if (data != null && !data.cameraStack.Contains(m_pegUICamera))
				{
					data.cameraStack.Add(m_pegUICamera);
				}
				m_cameraCount++;
			}
			else
			{
				m_cameraCount--;
			}
			if (m_haveUICamera)
			{
				if (m_cameraCount > 0)
				{
					m_pegUICameraData.renderType = CameraRenderType.Overlay;
				}
				else
				{
					m_pegUICameraData.renderType = CameraRenderType.Base;
				}
			}
		}
	}

	public static CameraManager Get()
	{
		if (s_instance == null)
		{
			s_instance = ServiceManager.Get<CameraManager>();
		}
		return s_instance;
	}

	public static bool IsInitialized()
	{
		return s_instance != null;
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		PegUI pegUI = PegUI.Get();
		if (pegUI != null && pegUI.orthographicUICam != null)
		{
			m_pegUICamera = pegUI.orthographicUICam;
			UniversalAdditionalCameraData data = pegUI.orthographicUICam.GetUniversalAdditionalCameraData();
			if (data != null)
			{
				m_pegUICameraData = data;
				m_haveUICamera = true;
			}
		}
		if (!m_haveUICamera)
		{
			Debug.LogError("Couldn't find orthographic UI camera");
		}
		yield break;
	}

	public Type[] GetDependencies()
	{
		return null;
	}

	public void Shutdown()
	{
		s_instance = null;
	}
}
