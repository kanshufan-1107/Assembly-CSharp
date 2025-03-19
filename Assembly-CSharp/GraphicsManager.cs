using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Util;
using UnityEngine;

public class GraphicsManager : IGraphicsManager, IHasUpdate, IService
{
	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	private struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private const int ANDROID_MIN_DPI_HIGH_RES_TEXTURES = 180;

	private const int DRAGGING_TARGET_FRAMERATE = 60;

	private const int MIN_VIDMEM_FOR_HIGHQUALITY = 500;

	public const int DEFAULT_TARGET_FRAME_RATE = 60;

	private GraphicsQuality m_GraphicsQuality;

	private bool m_RealtimeShadows;

	private List<GameObject> m_DisableLowQualityObjects;

	private int m_targetFramerate = 60;

	private int m_winPosX;

	private int m_winPosY;

	private bool m_initialPositionSet;

	private bool m_DynamicFps = true;

	private ResizeManager m_resizeManager;

	private bool m_allowMSAA = true;

	public GraphicsQuality RenderQualityLevel
	{
		get
		{
			return m_GraphicsQuality;
		}
		set
		{
			m_GraphicsQuality = value;
			Options.Get().SetInt(Option.GFX_QUALITY, (int)m_GraphicsQuality);
			UpdateQualitySettings();
		}
	}

	public bool RealtimeShadows => m_RealtimeShadows;

	public static int DefaultFPS => 60;

	private event Action<int, int> m_onResolutionChangedEvent;

	public event Action<int, int> OnResolutionChangedEvent
	{
		add
		{
			m_onResolutionChangedEvent -= value;
			m_onResolutionChangedEvent += value;
		}
		remove
		{
			m_onResolutionChangedEvent -= value;
		}
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		InitializeResolution();
		m_DisableLowQualityObjects = new List<GameObject>();
		if (!Options.Get().HasOption(Option.GFX_QUALITY))
		{
			string deviceName = serviceLocator.Get<ITouchScreenService>().GetIntelDeviceName();
			Log.Graphics.Print("Intel Device Name = {0}", deviceName);
			if (deviceName != null && deviceName.Contains("Haswell") && deviceName.Contains("U28W"))
			{
				if (Screen.currentResolution.height > 1080)
				{
					Options.Get().SetInt(Option.GFX_QUALITY, 0);
				}
			}
			else if (deviceName != null && deviceName.Contains("Crystal-Well"))
			{
				Options.Get().SetInt(Option.GFX_QUALITY, 2);
			}
			else if (deviceName != null && deviceName.Contains("BayTrail"))
			{
				Options.Get().SetInt(Option.GFX_QUALITY, 0);
			}
		}
		m_GraphicsQuality = (GraphicsQuality)Options.Get().GetInt(Option.GFX_QUALITY);
		m_resizeManager = new ResizeManager(OnResolutionChanged);
		InitializeScreen();
		UpdateQualitySettings();
		UpdateFramerateSettings();
		LogSystemInfo();
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(ITouchScreenService) };
	}

	public void Shutdown()
	{
		if (!Screen.fullScreen)
		{
			Options.Get().SetInt(Option.GFX_WIDTH, Screen.width);
			Options.Get().SetInt(Option.GFX_HEIGHT, Screen.height);
			int[] currentPos = GetWindowPosition();
			Options.Get().SetInt(Option.GFX_WIN_POSX, currentPos[0]);
			Options.Get().SetInt(Option.GFX_WIN_POSY, currentPos[1]);
		}
		this.m_onResolutionChangedEvent = null;
	}

	public void Update()
	{
		m_resizeManager.Update();
	}

	public void SetDraggingFramerate(bool isDragging)
	{
		if (!m_DynamicFps)
		{
			return;
		}
		if (isDragging)
		{
			if (Application.targetFrameRate < 60)
			{
				Application.targetFrameRate = 60;
			}
		}
		else
		{
			Application.targetFrameRate = m_targetFramerate;
		}
	}

	public void RegisterLowQualityDisableObject(GameObject lowQualityObject)
	{
		if (!m_DisableLowQualityObjects.Contains(lowQualityObject))
		{
			m_DisableLowQualityObjects.Add(lowQualityObject);
		}
	}

	public void DeregisterLowQualityDisableObject(GameObject lowQualityObject)
	{
		if (m_DisableLowQualityObjects.Contains(lowQualityObject))
		{
			m_DisableLowQualityObjects.Remove(lowQualityObject);
		}
	}

	public bool isVeryLowQualityDevice()
	{
		return false;
	}

	public void UpdateTargetFramerate(int rate)
	{
		m_targetFramerate = rate;
		Application.targetFrameRate = rate;
	}

	public void UpdateTargetFramerate(int rate, bool dynamicFps)
	{
		m_DynamicFps = dynamicFps;
		m_targetFramerate = rate;
		Application.targetFrameRate = rate;
		Options.Get().SetInt(Option.GFX_TARGET_FRAME_RATE, m_targetFramerate);
		if (rate == Screen.currentResolution.refreshRate)
		{
			QualitySettings.vSyncCount = 1;
		}
		else
		{
			QualitySettings.vSyncCount = 0;
		}
	}

	private void InitializeResolution()
	{
	}

	private void InitializeScreen()
	{
		if (!Options.Get().GetBool(Option.GFX_FULLSCREEN) && Options.Get().HasOption(Option.GFX_WIN_POSX) && Options.Get().HasOption(Option.GFX_WIN_POSY))
		{
			int posx = Options.Get().GetInt(Option.GFX_WIN_POSX);
			int posy = Options.Get().GetInt(Option.GFX_WIN_POSY);
			if (posx < 0)
			{
				posx = 0;
			}
			if (posy < 0)
			{
				posy = 0;
			}
			Processor.RunCoroutine(SetPos(posx, posy, 0.6f));
		}
	}

	private void UpdateQualitySettings()
	{
		if (SystemInfo.graphicsMemorySize < 500 && m_GraphicsQuality > GraphicsQuality.Low)
		{
			m_GraphicsQuality = GraphicsQuality.Low;
		}
		Log.Graphics.Print("GraphicsManager Update, Graphics Quality: " + m_GraphicsQuality);
		UpdateRenderQualitySettings();
		UpdateAntiAliasing();
	}

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

	[DllImport("user32.dll")]
	private static extern IntPtr FindWindow(string className, string windowName);

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	private static bool SetWindowPosition(int x, int y, int resX = 0, int resY = 0)
	{
		if (PlatformFilePaths.IsOptionsFileOverridden())
		{
			SetWindowPos(GetCurrentProcessWindow(), 0, x, y, resX, resY, (resX * resY == 0) ? 1 : 0);
			return true;
		}
		IntPtr activeHwnd = GetActiveWindow();
		IntPtr foundHwnd = FindWindow(null, "Hearthstone");
		if (activeHwnd == foundHwnd)
		{
			SetWindowPos(activeHwnd, 0, x, y, resX, resY, (resX * resY == 0) ? 1 : 0);
			return true;
		}
		return false;
	}

	private static IntPtr GetCurrentProcessWindow()
	{
		IntPtr foundWindow = IntPtr.Zero;
		EnumWindows(delegate(IntPtr window, IntPtr param)
		{
			uint lpdwProcessId = 0u;
			GetWindowThreadProcessId(window, out lpdwProcessId);
			if (Process.GetCurrentProcess().Id == lpdwProcessId)
			{
				foundWindow = window;
				return false;
			}
			return true;
		}, IntPtr.Zero);
		return foundWindow;
	}

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	private static IntPtr GetActiveWindow()
	{
		return GetForegroundWindow();
	}

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

	public static int[] GetWindowPosition()
	{
		int[] array = new int[2];
		RECT winRect = default(RECT);
		GetWindowRect(GetCurrentProcessWindow(), out winRect);
		array[0] = winRect.Left;
		array[1] = winRect.Top;
		return array;
	}

	public void SetScreenResolution(int width, int height, bool fullscreen)
	{
		m_resizeManager.SetScreenResolution(width, height, fullscreen);
	}

	public void SetFullScreen()
	{
		m_resizeManager.SetFullScreen();
	}

	public void SetWindowedScreen(int width, int height)
	{
		m_resizeManager.SetWindowedScreen(width, height);
	}

	private void OnResolutionChanged(int width, int height)
	{
		int[] windowPosition = GetWindowPosition();
		int posX = windowPosition[0];
		int posY = windowPosition[1];
		if (posX + width > Screen.currentResolution.width)
		{
			posX = Screen.currentResolution.width - width;
		}
		if (posY + height > Screen.currentResolution.height)
		{
			posY = Screen.currentResolution.height - height;
		}
		if (posX < 0 || posX > Screen.currentResolution.width)
		{
			posX = 0;
		}
		if (posY + height > Screen.currentResolution.height)
		{
			posY = 0;
		}
		if (posY < 0 || posY > Screen.currentResolution.height)
		{
			posY = 0;
		}
		if (this.m_onResolutionChangedEvent != null && !PlatformSettings.IsMobileRuntimeOS)
		{
			this.m_onResolutionChangedEvent(width, height);
		}
		if (m_initialPositionSet)
		{
			Processor.RunCoroutine(SetPos(posX, posY));
		}
	}

	private IEnumerator SetPos(int x, int y, float delay = 0f)
	{
		if (HearthstoneApplication.IsInternal() && !PlatformFilePaths.IsOptionsFileOverridden())
		{
			m_initialPositionSet = true;
			yield break;
		}
		yield return new WaitForSeconds(delay);
		m_winPosX = x;
		m_winPosY = y;
		int[] currentPos = GetWindowPosition();
		int[] newPos = new int[2] { m_winPosX, m_winPosY };
		float startTime = Time.time;
		while ((currentPos[0] != newPos[0] || currentPos[1] != newPos[1]) && Time.time < startTime + 1f)
		{
			newPos[0] = m_winPosX;
			newPos[1] = m_winPosY;
			if (!SetWindowPosition(m_winPosX, m_winPosY))
			{
				break;
			}
			currentPos = GetWindowPosition();
			yield return null;
		}
		m_initialPositionSet = true;
	}

	public bool AllowMSAA()
	{
		return m_allowMSAA;
	}

	private void UpdateAntiAliasing()
	{
		m_allowMSAA = true;
		if (m_GraphicsQuality == GraphicsQuality.Low)
		{
			m_allowMSAA = false;
		}
		if (m_GraphicsQuality == GraphicsQuality.Medium && ServiceManager.TryGet<ITouchScreenService>(out var touchScreenService))
		{
			string deviceName = touchScreenService.GetIntelDeviceName();
			if (deviceName != null && (deviceName.Equals("BayTrail") || deviceName.Equals("Poulsbo") || deviceName.Equals("CloverTrail") || (deviceName.Contains("Haswell") && deviceName.Contains("Y6W"))))
			{
				m_allowMSAA = false;
			}
		}
		if (Options.Get().HasOption(Option.GFX_MSAA))
		{
			m_allowMSAA = Options.Get().GetInt(Option.GFX_MSAA) > 0;
		}
		Camera[] array = UnityEngine.Object.FindObjectsOfType(typeof(Camera)) as Camera[];
		for (int i = 0; i < array.Length; i++)
		{
			array[i].allowMSAA = m_allowMSAA;
		}
	}

	private void UpdateRenderQualitySettings()
	{
		int shaderLOD = 101;
		if (m_GraphicsQuality == GraphicsQuality.Low)
		{
			m_targetFramerate = 60;
			m_RealtimeShadows = false;
			SetQualityByName("Low");
			shaderLOD = 101;
		}
		if (m_GraphicsQuality == GraphicsQuality.Medium)
		{
			m_targetFramerate = 60;
			m_RealtimeShadows = false;
			SetQualityByName("Medium");
			shaderLOD = 201;
		}
		if (m_GraphicsQuality == GraphicsQuality.High)
		{
			m_RealtimeShadows = true;
			SetQualityByName("High");
			shaderLOD = 301;
		}
		Shader.DisableKeyword("LOW_QUALITY");
		ProjectedShadow[] array = UnityEngine.Object.FindObjectsOfType(typeof(ProjectedShadow)) as ProjectedShadow[];
		foreach (ProjectedShadow shadow in array)
		{
			shadow.enabled = !m_RealtimeShadows || shadow.m_enabledAlongsideRealtimeShadows;
		}
		RenderToTexture[] array2 = UnityEngine.Object.FindObjectsOfType(typeof(RenderToTexture)) as RenderToTexture[];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].ForceTextureRebuild();
		}
		Shader[] array3 = UnityEngine.Object.FindObjectsOfType(typeof(Shader)) as Shader[];
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i].maximumLOD = shaderLOD;
		}
		foreach (GameObject lowQualityObj in m_DisableLowQualityObjects)
		{
			if (!(lowQualityObj == null))
			{
				if (m_GraphicsQuality == GraphicsQuality.Low)
				{
					Log.Graphics.Print($"Low Quality Disable: {lowQualityObj.name}");
					lowQualityObj.SetActive(value: false);
				}
				else
				{
					Log.Graphics.Print($"Low Quality Enable: {lowQualityObj.name}");
					lowQualityObj.SetActive(value: true);
				}
			}
		}
		Shader.globalMaximumLOD = shaderLOD;
		SetScreenEffects();
	}

	private void UpdateFramerateSettings()
	{
		int vsync = 0;
		Options options = Options.Get();
		if (!options.HasOption(Option.GFX_TARGET_FRAME_RATE))
		{
			options.SetInt(Option.GFX_TARGET_FRAME_RATE, 60);
		}
		int fps = options.GetInt(Option.GFX_TARGET_FRAME_RATE);
		if (fps == 30)
		{
			m_DynamicFps = true;
		}
		else
		{
			m_DynamicFps = false;
			vsync = ((Screen.currentResolution.refreshRate == fps) ? 1 : 0);
		}
		m_targetFramerate = fps;
		if (ServiceManager.TryGet<ITouchScreenService>(out var touchScreenService) && touchScreenService.GetBatteryMode() == PowerSource.BatteryPower && m_targetFramerate > 30)
		{
			Log.Graphics.Print("Battery Mode Detected - Clamping Target Frame Rate from {0} to 30", m_targetFramerate);
			m_targetFramerate = 30;
			m_DynamicFps = false;
			options.SetInt(Option.GFX_TARGET_FRAME_RATE, m_targetFramerate);
			vsync = 0;
		}
		Application.targetFrameRate = m_targetFramerate;
		if (options.HasOption(Option.GFX_VSYNC))
		{
			QualitySettings.vSyncCount = options.GetInt(Option.GFX_VSYNC);
		}
		else
		{
			QualitySettings.vSyncCount = vsync;
		}
		Log.Graphics.Print($"Target frame rate: {Application.targetFrameRate}");
	}

	private void SetScreenEffects()
	{
		if (ScreenEffectsMgr.Get() != null)
		{
			if (m_GraphicsQuality == GraphicsQuality.Low)
			{
				ScreenEffectsMgr.Get().SetActive(enabled: false);
			}
			else
			{
				ScreenEffectsMgr.Get().SetActive(enabled: true);
			}
		}
	}

	private void SetQualityByName(string qualityName)
	{
		string[] names = QualitySettings.names;
		int qualityIndex = -1;
		int idx;
		for (idx = 0; idx < names.Length; idx++)
		{
			if (names[idx] == qualityName)
			{
				qualityIndex = idx;
			}
		}
		if (idx < 0)
		{
			Debug.LogError($"GraphicsManager: Quality Level not found: {qualityName}");
		}
		else
		{
			QualitySettings.SetQualityLevel(qualityIndex, applyExpensiveChanges: true);
		}
	}

	private void LogSystemInfo()
	{
		Debug.Log("System Info:");
		Debug.Log($"SystemInfo - Device Name: {SystemInfo.deviceName}");
		Debug.Log($"SystemInfo - Device Model: {SystemInfo.deviceModel}");
		Debug.Log($"SystemInfo - OS: {SystemInfo.operatingSystem}");
		Debug.Log($"SystemInfo - CPU Type: {SystemInfo.processorType}");
		Debug.Log($"SystemInfo - CPU Cores: {SystemInfo.processorCount}");
		Debug.Log($"SystemInfo - System Memory: {SystemInfo.systemMemorySize}");
		Debug.Log($"SystemInfo - Screen Resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height}");
		Debug.Log($"SystemInfo - Screen DPI: {Screen.dpi}");
		Debug.Log($"SystemInfo - GPU ID: {SystemInfo.graphicsDeviceID}");
		Debug.Log($"SystemInfo - GPU Name: {SystemInfo.graphicsDeviceName}");
		Debug.Log($"SystemInfo - GPU Vendor: {SystemInfo.graphicsDeviceVendor}");
		Debug.Log($"SystemInfo - GPU Memory: {SystemInfo.graphicsMemorySize}");
		Debug.Log($"SystemInfo - GPU Shader Level: {SystemInfo.graphicsShaderLevel}");
		Debug.Log($"SystemInfo - GPU NPOT Support: {SystemInfo.npotSupport}");
		Debug.Log($"SystemInfo - Graphics API (version): {SystemInfo.graphicsDeviceVersion}");
		Debug.Log($"SystemInfo - Graphics API (type): {SystemInfo.graphicsDeviceType}");
		Debug.Log($"SystemInfo - Graphics Supported Render Target Count: {SystemInfo.supportedRenderTargetCount}");
		Debug.Log($"SystemInfo - Graphics Supports 3D Textures: {SystemInfo.supports3DTextures}");
		Debug.Log($"SystemInfo - Graphics Supports Compute Shaders: {SystemInfo.supportsComputeShaders}");
		Debug.Log($"SystemInfo - Graphics Supports Shadows: {SystemInfo.supportsShadows}");
		Debug.Log($"SystemInfo - Graphics Supports Sparse Textures: {SystemInfo.supportsSparseTextures}");
		Debug.Log($"SystemInfo - Graphics RenderTextureFormat.ARGBHalf: {SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)}");
		Debug.Log(string.Format("SystemInfo - Graphics Metal Support: {0}", SystemInfo.graphicsDeviceVersion.StartsWith("Metal")));
	}
}
