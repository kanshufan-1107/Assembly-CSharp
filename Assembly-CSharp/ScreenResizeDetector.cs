using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;
using UnityEngine.Rendering;

public class ScreenResizeDetector : MonoBehaviour
{
	public delegate void SizeChangedCallback(object userData);

	private class SizeChangedListener : EventListener<SizeChangedCallback>
	{
		public void Fire()
		{
			m_callback(m_userData);
		}
	}

	private const float _1x1 = 1f;

	private const float _5x4 = 1.25f;

	private const float _4x3 = 1.3333334f;

	private const float _3x2 = 1.5f;

	private const float _16x10 = 1.6f;

	private const float _16x9 = 1.7777778f;

	private const float _18x9 = 2f;

	private const float _19x9 = 2.1111112f;

	private const float _21x9 = 2.3333333f;

	private const float ExtraWide = 2.3703704f;

	private const float AspectRatioTolerance = 0.005f;

	private float m_screenWidth;

	private float m_screenHeight;

	private List<SizeChangedListener> m_sizeChangedListeners = new List<SizeChangedListener>();

	private void Awake()
	{
		SaveScreenSize();
		UpdateDeviceDataModel();
	}

	private void OnEnable()
	{
		RenderPipelineManager.beginFrameRendering += BeginFrameRendering;
	}

	private void OnDisable()
	{
		RenderPipelineManager.beginFrameRendering -= BeginFrameRendering;
	}

	private void BeginFrameRendering(ScriptableRenderContext context, Camera[] cameras)
	{
		float screenWidth = Screen.width;
		float screenHeight = Screen.height;
		if (!Mathf.Approximately(m_screenWidth, screenWidth) || !Mathf.Approximately(m_screenHeight, screenHeight))
		{
			SaveScreenSize();
			UpdateDeviceDataModel();
			FireSizeChangedEvent();
		}
	}

	public bool AddSizeChangedListener(SizeChangedCallback callback)
	{
		return AddSizeChangedListener(callback, null);
	}

	public bool AddSizeChangedListener(SizeChangedCallback callback, object userData)
	{
		SizeChangedListener listener = new SizeChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_sizeChangedListeners.Contains(listener))
		{
			return false;
		}
		m_sizeChangedListeners.Add(listener);
		return true;
	}

	public bool RemoveSizeChangedListener(SizeChangedCallback callback)
	{
		return RemoveSizeChangedListener(callback, null);
	}

	public bool RemoveSizeChangedListener(SizeChangedCallback callback, object userData)
	{
		SizeChangedListener listener = new SizeChangedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_sizeChangedListeners.Remove(listener);
	}

	private void SaveScreenSize()
	{
		m_screenWidth = Screen.width;
		m_screenHeight = Screen.height;
	}

	private void FireSizeChangedEvent()
	{
		SizeChangedListener[] listeners = m_sizeChangedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire();
		}
	}

	private void UpdateDeviceDataModel()
	{
		if (GlobalDataContext.Get().GetDataModel(0, out var datamodel))
		{
			((DeviceDataModel)datamodel).AspectRatio = GetNextBestAspectRatio();
		}
	}

	private AspectRatio GetNextBestAspectRatio()
	{
		float ratio = 1f;
		ratio = ((PlatformSettings.Screen != ScreenCategory.Phone) ? (m_screenWidth / m_screenHeight) : (Screen.safeArea.width / Screen.safeArea.height));
		if (NarrowerThanTargetRatio(ratio, 1f))
		{
			return AspectRatio.Unknown;
		}
		if (NarrowerThanTargetRatio(ratio, 1.25f))
		{
			return AspectRatio._1x1;
		}
		if (NarrowerThanTargetRatio(ratio, 1.3333334f))
		{
			return AspectRatio._5x4;
		}
		if (NarrowerThanTargetRatio(ratio, 1.5f))
		{
			return AspectRatio._4x3;
		}
		if (NarrowerThanTargetRatio(ratio, 1.6f))
		{
			return AspectRatio._3x2;
		}
		if (NarrowerThanTargetRatio(ratio, 1.7777778f))
		{
			return AspectRatio._16x10;
		}
		if (NarrowerThanTargetRatio(ratio, 2f))
		{
			return AspectRatio._16x9;
		}
		if (NarrowerThanTargetRatio(ratio, 2.1111112f))
		{
			return AspectRatio._18x9;
		}
		if (NarrowerThanTargetRatio(ratio, 2.3333333f))
		{
			return AspectRatio._19x9;
		}
		if (NarrowerThanTargetRatio(ratio, 2.3703704f))
		{
			return AspectRatio._21x9;
		}
		return AspectRatio.ExtraWide;
	}

	private bool NarrowerThanTargetRatio(float value, float target)
	{
		return value < target - 0.005f;
	}
}
