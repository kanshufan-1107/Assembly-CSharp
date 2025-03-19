using Blizzard.T5.Services;
using Hearthstone;
using UnityEngine;

[ExecuteAlways]
public class ShowFPS : MonoBehaviour
{
	private float m_UpdateInterval = 0.5f;

	private double m_LastInterval;

	private int frames;

	private bool m_FrameCountActive;

	private float m_FrameCountTime;

	private float m_FrameCountLastTime;

	private int m_FrameCount;

	private bool m_verbose;

	private string m_fpsText;

	private const int MAX_CAMERA_NUM = 20;

	private Camera[] m_cameras;

	private static ShowFPS s_instance;

	private void Awake()
	{
		s_instance = this;
		if (HearthstoneApplication.IsPublic())
		{
			Object.DestroyImmediate(base.gameObject);
		}
		m_cameras = new Camera[20];
	}

	private void OnDestroy()
	{
		m_cameras = null;
		s_instance = null;
	}

	public static ShowFPS Get()
	{
		return s_instance;
	}

	[ContextMenu("Start Frame Count")]
	public void StartFrameCount()
	{
		m_FrameCountLastTime = Time.realtimeSinceStartup;
		m_FrameCountTime = 0f;
		m_FrameCount = 0;
		m_FrameCountActive = true;
	}

	[ContextMenu("Stop Frame Count")]
	public void StopFrameCount()
	{
		m_FrameCountActive = false;
	}

	[ContextMenu("Clear Frame Count")]
	public void ClearFrameCount()
	{
		m_FrameCountLastTime = 0f;
		m_FrameCountTime = 0f;
		m_FrameCount = 0;
		m_FrameCountActive = false;
	}

	private void Start()
	{
		m_LastInterval = Time.realtimeSinceStartup;
		frames = 0;
		UpdateEnabled();
		Options.Get().RegisterChangedListener(Option.HUD, OnHudOptionChanged);
	}

	private void OnDisable()
	{
		Time.captureFramerate = 0;
	}

	private void Update()
	{
		frames++;
		float currentTime = Time.realtimeSinceStartup;
		if ((double)currentTime > m_LastInterval + (double)m_UpdateInterval)
		{
			float fps = (float)frames / (float)((double)currentTime - m_LastInterval);
			if (m_verbose)
			{
				m_fpsText = $"{fps:f2} - {frames} frames over {m_UpdateInterval}sec";
			}
			else
			{
				m_fpsText = $"{fps:f2}";
			}
			frames = 0;
			m_LastInterval = currentTime;
		}
		if ((m_FrameCountActive || m_FrameCount > 0) && m_FrameCountActive)
		{
			m_FrameCountTime += (currentTime - m_FrameCountLastTime) / 60f * Time.timeScale;
			if (m_FrameCountLastTime == 0f)
			{
				m_FrameCountLastTime = currentTime;
			}
			m_FrameCount = Mathf.CeilToInt(m_FrameCountTime * 60f);
		}
	}

	private void OnGUI()
	{
		int numFullScreenEffectsOn = 0;
		if (m_cameras.Length < Camera.allCamerasCount)
		{
			m_cameras = new Camera[Camera.allCamerasCount];
		}
		int camerasCount = Camera.GetAllCameras(m_cameras);
		for (int i = 0; i < camerasCount; i++)
		{
			if (m_cameras[i].TryGetComponent<FullScreenEffects>(out var fse) && fse.IsActive)
			{
				numFullScreenEffectsOn++;
			}
		}
		string fpsTextRT = m_fpsText;
		if (m_FrameCountActive || m_FrameCount > 0)
		{
			fpsTextRT = $"{fpsTextRT} - Frame Count: {m_FrameCount}";
		}
		if (numFullScreenEffectsOn > 0)
		{
			fpsTextRT = $"{fpsTextRT} - FSE (x{numFullScreenEffectsOn})";
		}
		if (ServiceManager.TryGet<ScreenEffectsMgr>(out var screenEffectsMgr))
		{
			int screenEffectsCount = screenEffectsMgr.GetActiveScreenEffectsCount();
			if (screenEffectsCount > 0 && screenEffectsMgr.IsActive)
			{
				fpsTextRT = $"{fpsTextRT} - ScreenEffects Active: {screenEffectsCount}";
			}
		}
		GUI.Box(new Rect((float)Screen.width * 0.75f, Screen.height - 20, (float)Screen.width * 0.25f, 20f), fpsTextRT);
	}

	private void OnHudOptionChanged(Option option, object prevValue, bool existed, object userData)
	{
		UpdateEnabled();
	}

	private void UpdateEnabled()
	{
		base.enabled = Options.Get().GetBool(Option.HUD);
	}
}
