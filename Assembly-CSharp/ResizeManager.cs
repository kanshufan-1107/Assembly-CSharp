using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using UnityEngine;

public class ResizeManager
{
	private Action<int, int> m_onResolutionChanged;

	private bool m_lastFullScreen;

	private int m_lastWindowedWidth;

	private int m_lastWindowedHeight;

	private int m_lastWidth;

	private int m_lastHeight;

	private float m_lastDpi;

	private float m_lastChangedResolutionTime = float.MinValue;

	public ResizeManager(Action<int, int> onResolutionChanged)
	{
		m_onResolutionChanged = onResolutionChanged;
		bool fullScreen = (m_lastFullScreen = Options.Get().GetBool(Option.GFX_FULLSCREEN, defaultVal: true));
		int width;
		int height;
		if (fullScreen)
		{
			width = Options.Get().GetInt(Option.GFX_WIDTH, Screen.currentResolution.width);
			height = Options.Get().GetInt(Option.GFX_HEIGHT, Screen.currentResolution.height);
			m_lastWindowedWidth = (int)((float)Screen.currentResolution.width * 0.75f);
			m_lastWindowedHeight = (int)((float)Screen.currentResolution.height * 0.75f);
			if (IsNewIntelLowResolutionDevice() && Screen.currentResolution.height > 1080)
			{
				width = 1920;
				height = 1080;
			}
			if (width == Screen.currentResolution.width && height == Screen.currentResolution.height && fullScreen == Screen.fullScreen)
			{
				return;
			}
		}
		else
		{
			width = Options.Get().GetInt(Option.GFX_WIDTH, (int)((float)Screen.currentResolution.width * 0.75f));
			height = Options.Get().GetInt(Option.GFX_HEIGHT, (int)((float)Screen.currentResolution.height * 0.75f));
			m_lastWindowedWidth = width;
			m_lastWindowedHeight = height;
		}
		SetScreenResolution(width, height, fullScreen, fadeToBlack: true);
	}

	public void Update()
	{
		if (Screen.fullScreen)
		{
			return;
		}
		if (!IsNewWindowResolution(Screen.width, Screen.height))
		{
			m_lastChangedResolutionTime = Time.time;
			return;
		}
		int[] newWindowedResolution = ForceScreenResolutionAspectRatio(Screen.width, Screen.height, isWindowed: true);
		if (m_lastChangedResolutionTime + 1f < Time.time && m_lastChangedResolutionTime + 1f > Time.time - Time.deltaTime)
		{
			SetScreenResolution(newWindowedResolution[0], newWindowedResolution[1], Screen.fullScreen);
		}
	}

	private int[] ForceScreenResolutionAspectRatio(int width, int height, bool isWindowed)
	{
		if (!GraphicsResolution.IsAspectRatioWithinLimit(width, height, isWindowed))
		{
			int[] array = GraphicsResolution.CalcAspectRatioLimit(width, height);
			width = array[0];
			height = array[1];
		}
		return new int[2] { width, height };
	}

	private bool IsNewWindowResolution(int width, int height)
	{
		if (m_lastWidth != width || m_lastHeight != height)
		{
			return m_lastDpi == Screen.dpi;
		}
		return false;
	}

	public void SetScreenResolution(int width, int height, bool fullscreen)
	{
		SetScreenResolution(width, height, fullscreen, fadeToBlack: false);
	}

	public void SetScreenResolution(int width, int height, bool fullscreen, bool fadeToBlack)
	{
		if (height > Screen.currentResolution.height && !fullscreen)
		{
			height = Screen.currentResolution.height;
		}
		if (width > Screen.currentResolution.width && !fullscreen)
		{
			width = Screen.currentResolution.width;
		}
		if (fullscreen && fullscreen != m_lastFullScreen)
		{
			height = Screen.currentResolution.height;
			width = Screen.currentResolution.width;
		}
		Processor.QueueJob("ResizeManager.SetRes", Job_SetRes(width, height, fullscreen, fadeToBlack));
	}

	private IEnumerator<IAsyncJobResult> Job_SetRes(int width, int height, bool fullscreen, bool fadeToBlack)
	{
		yield return ServiceManager.CreateServiceSoftDependency(typeof(SceneMgr));
		SceneMgr sceneMgr;
		LoadingScreen loadingScreen = ((!ServiceManager.TryGet<SceneMgr>(out sceneMgr)) ? UnityEngine.Object.FindObjectOfType<LoadingScreen>() : sceneMgr.LoadingScreen);
		CameraFade cameraFade = loadingScreen.GetCameraFade();
		Camera camera = loadingScreen.GetFxCamera();
		float prevDepth = camera.depth;
		Color prevColor = cameraFade.m_Color;
		float prevFade = cameraFade.m_Fade;
		if (!fadeToBlack)
		{
			cameraFade.m_Color = Color.black;
			cameraFade.m_Fade = 1f;
		}
		yield return null;
		if (fullscreen != m_lastFullScreen)
		{
			if (fullscreen)
			{
				width = Screen.currentResolution.width;
				height = Screen.currentResolution.height;
			}
			else
			{
				width = m_lastWindowedWidth;
				height = m_lastWindowedHeight;
			}
		}
		else
		{
			int[] newWindowedResolution = ForceScreenResolutionAspectRatio(width, height, !fullscreen);
			width = newWindowedResolution[0];
			height = newWindowedResolution[1];
		}
		m_lastFullScreen = fullscreen;
		Screen.SetResolution(width, height, fullscreen);
		yield return null;
		Screen.SetResolution(width, height, fullscreen);
		Options.Get().SetBool(Option.GFX_FULLSCREEN, fullscreen);
		Options.Get().SetInt(Option.GFX_WIDTH, width);
		Options.Get().SetInt(Option.GFX_HEIGHT, height);
		m_lastWidth = Screen.width;
		m_lastHeight = Screen.height;
		m_lastDpi = Screen.dpi;
		if (!fullscreen)
		{
			m_lastWindowedWidth = width;
			m_lastWindowedHeight = height;
		}
		camera.depth = prevDepth;
		cameraFade.m_Color = prevColor;
		cameraFade.m_Fade = prevFade;
		m_onResolutionChanged(width, height);
	}

	private static bool IsNewIntelLowResolutionDevice()
	{
		if (Options.Get().HasOption(Option.GFX_WIDTH) && Options.Get().HasOption(Option.GFX_HEIGHT))
		{
			return false;
		}
		string deviceName = ServiceManager.Get<ITouchScreenService>().GetIntelDeviceName();
		if (string.IsNullOrEmpty(deviceName))
		{
			return false;
		}
		if ((deviceName.Contains("Haswell") && deviceName.Contains("Y6W")) || (deviceName.Contains("Haswell") && deviceName.Contains("U15W")))
		{
			return true;
		}
		return false;
	}

	public void SetWindowedScreen(int width, int height)
	{
		SetScreenResolution(width, height, fullscreen: false);
	}

	public void SetFullScreen()
	{
		SetScreenResolution(0, 0, fullscreen: true);
	}
}
