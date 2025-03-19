using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using UnityEngine;

public class W8Touch : ITouchScreenService, IService, IHasUpdate
{
	[StructLayout(0, Pack = 1)]
	public class tTouchData
	{
		public int m_x;

		public int m_y;

		public int m_ID;

		public int m_Time;
	}

	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	[Flags]
	public enum KeyboardFlags
	{
		Shown = 1,
		NotShown = 2,
		SuccessTabTip = 4,
		SuccessOSK = 8,
		ErrorTabTip = 0x10,
		ErrorOSK = 0x20,
		NotFoundTabTip = 0x40,
		NotFoundOSK = 0x80
	}

	public enum TouchState
	{
		None,
		InitialDown,
		Down,
		InitialUp
	}

	public class IntelDevice
	{
		private static readonly Map<int, string> DeviceIdMap = new Map<int, string>
		{
			{ 30720, "Auburn" },
			{ 28961, "Whitney" },
			{ 28963, "Whitney" },
			{ 28965, "Whitney" },
			{ 4402, "Solono" },
			{ 9570, "Brookdale" },
			{ 13698, "Montara" },
			{ 9586, "Springdale" },
			{ 9602, "Grantsdale" },
			{ 10114, "Grantsdale" },
			{ 9618, "Alviso" },
			{ 10130, "Alviso" },
			{ 10098, "Lakeport-G" },
			{ 10102, "Lakeport-G" },
			{ 10146, "Calistoga" },
			{ 10150, "Calistoga" },
			{ 10626, "Broadwater-G" },
			{ 10627, "Broadwater-G" },
			{ 10610, "Broadwater-G" },
			{ 10611, "Broadwater-G" },
			{ 10642, "Broadwater-G" },
			{ 10643, "Broadwater-G" },
			{ 10658, "Broadwater-G" },
			{ 10659, "Broadwater-G" },
			{ 10754, "Crestline" },
			{ 10755, "Crestline" },
			{ 10770, "Crestline" },
			{ 10771, "Crestline" },
			{ 10674, "Bearlake" },
			{ 10675, "Bearlake" },
			{ 10690, "Bearlake" },
			{ 10691, "Bearlake" },
			{ 10706, "Bearlake" },
			{ 10707, "Bearlake" },
			{ 10818, "Cantiga" },
			{ 10819, "Cantiga" },
			{ 11778, "Eaglelake" },
			{ 11779, "Eaglelake" },
			{ 11810, "Eaglelake" },
			{ 11811, "Eaglelake" },
			{ 11794, "Eaglelake" },
			{ 11795, "Eaglelake" },
			{ 11826, "Eaglelake" },
			{ 11827, "Eaglelake" },
			{ 11842, "Eaglelake" },
			{ 11843, "Eaglelake" },
			{ 11922, "Eaglelake" },
			{ 11923, "Eaglelake" },
			{ 70, "Arrandale" },
			{ 66, "Clarkdale" },
			{ 262, "Mobile_SandyBridge_GT1" },
			{ 278, "Mobile_SandyBridge_GT2" },
			{ 294, "Mobile_SandyBridge_GT2+" },
			{ 258, "DT_SandyBridge_GT2+" },
			{ 274, "DT_SandyBridge_GT2+" },
			{ 290, "DT_SandyBridge_GT2+" },
			{ 266, "SandyBridge_Server" },
			{ 270, "SandyBridge_Reserved" },
			{ 338, "Desktop_IvyBridge_GT1" },
			{ 342, "Mobile_IvyBridge_GT1" },
			{ 346, "Server_IvyBridge_GT1" },
			{ 350, "Reserved_IvyBridge_GT1" },
			{ 354, "Desktop_IvyBridge_GT2" },
			{ 358, "Mobile_IvyBridge_GT2" },
			{ 362, "Server_IvyBridge_GT2" },
			{ 1026, "Desktop_Haswell_GT1_Y6W" },
			{ 1030, "Mobile_Haswell_GT1_Y6W" },
			{ 1034, "Server_Haswell_GT1" },
			{ 1042, "Desktop_Haswell_GT2_U15W" },
			{ 1046, "Mobile_Haswell_GT2_U15W" },
			{ 1051, "Workstation_Haswell_GT2" },
			{ 1050, "Server_Haswell_GT2" },
			{ 1054, "Reserved_Haswell_DT_GT1.5_U15W" },
			{ 2566, "Mobile_Haswell_ULT_GT1_Y6W" },
			{ 2574, "Mobile_Haswell_ULX_GT1_Y6W" },
			{ 2582, "Mobile_Haswell_ULT_GT2_U15W" },
			{ 2590, "Mobile_Haswell_ULX_GT2_Y6W" },
			{ 2598, "Mobile_Haswell_ULT_GT3_U28W" },
			{ 2606, "Mobile_Haswell_ULT_GT3@28_U28W" },
			{ 3346, "Desktop_Haswell_GT2F" },
			{ 3350, "Mobile_Haswell_GT2F" },
			{ 3362, "Desktop_Crystal-Well_GT3" },
			{ 3366, "Mobile_Crystal-Well_GT3" },
			{ 3370, "Server_Crystal-Well_GT3" },
			{ 3889, "BayTrail" },
			{ 33032, "Poulsbo" },
			{ 33033, "Poulsbo" },
			{ 2255, "CloverTrail" },
			{ 40961, "CloverTrail" },
			{ 40962, "CloverTrail" },
			{ 40977, "CloverTrail" },
			{ 40978, "CloverTrail" }
		};

		public static string GetDeviceName(int deviceId)
		{
			if (!DeviceIdMap.TryGetValue(deviceId, out var deviceName))
			{
				return "";
			}
			return deviceName;
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int DelW8ShowKeyboard();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int DelW8HideKeyboard();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int DelW8ShowOSK();

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto)]
	private delegate int DelW8Initialize(string windowName);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void DelW8Shutdown();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int DelW8GetDeviceId();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate bool DelW8IsWindows8OrGreater();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate bool DelW8IsLastEventFromTouch();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int DelW8GetBatteryMode();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int DelW8GetPercentBatteryLife();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void DelW8GetDesktopRect(out RECT desktopRect);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate bool DelW8IsVirtualKeyboardVisible();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int DelW8GetTouchPointCount();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate bool DelW8GetTouchPoint(int i, tTouchData n);

	public enum FEEDBACK_TYPE
	{
		FEEDBACK_TOUCH_CONTACTVISUALIZATION = 1,
		FEEDBACK_PEN_BARRELVISUALIZATION,
		FEEDBACK_PEN_TAP,
		FEEDBACK_PEN_DOUBLETAP,
		FEEDBACK_PEN_PRESSANDHOLD,
		FEEDBACK_PEN_RIGHTTAP,
		FEEDBACK_TOUCH_TAP,
		FEEDBACK_TOUCH_DOUBLETAP,
		FEEDBACK_TOUCH_PRESSANDHOLD,
		FEEDBACK_TOUCH_RIGHTTAP,
		FEEDBACK_GESTURE_PRESSANDTAP
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate bool DelSetWindowFeedbackSetting(IntPtr hwnd, FEEDBACK_TYPE feedback, uint dwFlags, uint size, IntPtr configuration);

	private bool m_initialized;

	public bool m_isWindows8OrGreater;

	private IntPtr m_DLL = IntPtr.Zero;

	private int m_intializationAttemptCount;

	private TouchState[] m_touchState;

	private Vector3 m_touchPosition = new Vector3(-1f, -1f, 0f);

	private Vector2 m_touchDelta = new Vector2(0f, 0f);

	private RECT m_desktopRect;

	private bool m_isVirtualKeyboardVisible;

	private bool m_isVirtualKeyboardShowRequested;

	private bool m_isVirtualKeyboardHideRequested;

	private PowerSource m_lastPowerSourceState = PowerSource.Unintialized;

	private bool m_bWindowFeedbackSettingValue;

	private bool m_bIsWindowFeedbackDisabled;

	private static DelW8ShowKeyboard DLL_W8ShowKeyboard;

	private static DelW8HideKeyboard DLL_W8HideKeyboard;

	private static DelW8ShowOSK DLL_W8ShowOSK;

	private static DelW8Initialize DLL_W8Initialize;

	private static DelW8Shutdown DLL_W8Shutdown;

	private static DelW8GetDeviceId DLL_W8GetDeviceId;

	private static DelW8IsWindows8OrGreater DLL_W8IsWindows8OrGreater;

	private static DelW8IsLastEventFromTouch DLL_W8IsLastEventFromTouch;

	private static DelW8GetBatteryMode DLL_W8GetBatteryMode;

	private static DelW8GetPercentBatteryLife DLL_W8GetPercentBatteryLife;

	private static DelW8GetDesktopRect DLL_W8GetDesktopRect;

	private static DelW8IsVirtualKeyboardVisible DLL_W8IsVirtualKeyboardVisible;

	private static DelW8GetTouchPointCount DLL_W8GetTouchPointCount;

	private static DelW8GetTouchPoint DLL_W8GetTouchPoint;

	private event Action VirtualKeyboardDidShow;

	private event Action VirtualKeyboardDidHide;

	[DllImport("User32.dll")]
	public static extern IntPtr FindWindow(string className, string windowName);

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		if (LoadW8TouchDLL())
		{
			m_isWindows8OrGreater = DLL_W8IsWindows8OrGreater();
		}
		m_touchState = new TouchState[5];
		for (int i = 0; i < 5; i++)
		{
			m_touchState[i] = TouchState.None;
		}
		if (HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().OnShutdown += OnApplicationQuit;
		}
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(UniversalInputManager) };
	}

	public void Shutdown()
	{
		HearthstoneApplication.Get().OnShutdown -= OnApplicationQuit;
	}

	public void Update()
	{
		if (!m_initialized)
		{
			InitializeDLL();
		}
		if (!IsInitialized())
		{
			return;
		}
		DLL_W8GetDesktopRect(out m_desktopRect);
		bool isVirtualKeyboardVisible = DLL_W8IsVirtualKeyboardVisible();
		if (isVirtualKeyboardVisible != m_isVirtualKeyboardVisible)
		{
			m_isVirtualKeyboardVisible = isVirtualKeyboardVisible;
			if (isVirtualKeyboardVisible && this.VirtualKeyboardDidShow != null)
			{
				this.VirtualKeyboardDidShow();
			}
			else if (!isVirtualKeyboardVisible && this.VirtualKeyboardDidHide != null)
			{
				this.VirtualKeyboardDidHide();
			}
		}
		if (m_isVirtualKeyboardVisible)
		{
			m_isVirtualKeyboardShowRequested = false;
		}
		else
		{
			m_isVirtualKeyboardHideRequested = false;
		}
		PowerSource powerSourceState = GetBatteryMode();
		if (powerSourceState != m_lastPowerSourceState && ServiceManager.TryGet<GraphicsManager>(out var graphicsManager))
		{
			Log.W8Touch.Print("PowerSource Change Detected: {0}", powerSourceState);
			m_lastPowerSourceState = powerSourceState;
			graphicsManager.RenderQualityLevel = (GraphicsQuality)Options.Get().GetInt(Option.GFX_QUALITY);
		}
		if ((!DLL_W8IsLastEventFromTouch() && UniversalInputManager.Get().UseWindowsTouch()) || (DLL_W8IsLastEventFromTouch() && !UniversalInputManager.Get().UseWindowsTouch()))
		{
			ToggleTouchMode();
		}
		if (m_touchState == null)
		{
			return;
		}
		int touchCount = DLL_W8GetTouchPointCount();
		for (int i = 0; i < 5; i++)
		{
			tTouchData touchData = new tTouchData();
			bool validTouchData = false;
			if (i < touchCount)
			{
				validTouchData = DLL_W8GetTouchPoint(i, touchData);
			}
			if (validTouchData && i == 0)
			{
				Vector2 newTouchPosition = TransformTouchPosition(new Vector2(touchData.m_x, touchData.m_y));
				if (m_touchPosition.x != -1f && m_touchPosition.y != -1f && m_touchState[i] == TouchState.Down)
				{
					m_touchDelta.x = newTouchPosition.x - m_touchPosition.x;
					m_touchDelta.y = newTouchPosition.y - m_touchPosition.y;
				}
				else
				{
					m_touchDelta.x = (m_touchDelta.y = 0f);
				}
				m_touchPosition.x = newTouchPosition.x;
				m_touchPosition.y = newTouchPosition.y;
			}
			if (validTouchData && touchData.m_ID != -1)
			{
				if (m_touchState[i] == TouchState.Down || m_touchState[i] == TouchState.InitialDown)
				{
					m_touchState[i] = TouchState.Down;
				}
				else
				{
					m_touchState[i] = TouchState.InitialDown;
				}
			}
			else if (m_touchState[i] == TouchState.Down || m_touchState[i] == TouchState.InitialDown)
			{
				m_touchState[i] = TouchState.InitialUp;
			}
			else
			{
				m_touchState[i] = TouchState.None;
			}
		}
	}

	private Vector2 TransformTouchPosition(Vector2 touchInput)
	{
		Vector2 touchPosition = default(Vector2);
		if (Screen.fullScreen)
		{
			float originalAspectRatio = (float)Screen.width / (float)Screen.height;
			float actualAspectRatio = (float)m_desktopRect.Right / (float)m_desktopRect.Bottom;
			if (Mathf.Abs(originalAspectRatio - actualAspectRatio) < Mathf.Epsilon)
			{
				float widthRatio = (float)Screen.width / (float)m_desktopRect.Right;
				float heightRatio = (float)Screen.height / (float)m_desktopRect.Bottom;
				touchPosition.x = touchInput.x * widthRatio;
				touchPosition.y = ((float)m_desktopRect.Bottom - touchInput.y) * heightRatio;
			}
			else if (originalAspectRatio < actualAspectRatio)
			{
				float actualHeight = m_desktopRect.Bottom;
				float actualWidth = actualHeight * originalAspectRatio;
				float heightRatio2 = (float)Screen.height / actualHeight;
				float widthRatio2 = (float)Screen.width / actualWidth;
				float pillarBoxWidth = ((float)m_desktopRect.Right - actualWidth) / 2f;
				touchPosition.x = (touchInput.x - pillarBoxWidth) * widthRatio2;
				touchPosition.y = ((float)m_desktopRect.Bottom - touchInput.y) * heightRatio2;
			}
			else
			{
				float actualWidth2 = m_desktopRect.Right;
				float actualHeight2 = actualWidth2 / originalAspectRatio;
				float heightRatio3 = (float)Screen.height / actualHeight2;
				float widthRatio3 = (float)Screen.width / actualWidth2;
				float letterBoxHeight = ((float)m_desktopRect.Bottom - actualHeight2) / 2f;
				touchPosition.x = touchInput.x * widthRatio3;
				touchPosition.y = ((float)m_desktopRect.Bottom - touchInput.y - letterBoxHeight) * heightRatio3;
			}
		}
		else
		{
			touchPosition.x = touchInput.x;
			touchPosition.y = (float)Screen.height - touchInput.y;
		}
		return touchPosition;
	}

	private void ToggleTouchMode()
	{
		if (IsInitialized())
		{
			bool isEnabled = Options.Get().GetBool(Option.TOUCH_MODE);
			Options.Get().SetBool(Option.TOUCH_MODE, !isEnabled);
		}
	}

	public void ShowKeyboard()
	{
		if (IsInitialized() && !m_isVirtualKeyboardShowRequested && (!m_isVirtualKeyboardVisible || m_isVirtualKeyboardHideRequested))
		{
			if (m_isVirtualKeyboardHideRequested)
			{
				m_isVirtualKeyboardHideRequested = false;
			}
			KeyboardFlags errorCode = (KeyboardFlags)DLL_W8ShowKeyboard();
			_ = errorCode & KeyboardFlags.Shown;
			_ = 1;
			if ((errorCode & KeyboardFlags.Shown) == KeyboardFlags.Shown && (errorCode & KeyboardFlags.SuccessTabTip) == KeyboardFlags.SuccessTabTip)
			{
				m_isVirtualKeyboardShowRequested = true;
			}
		}
	}

	public void HideKeyboard()
	{
		if (IsInitialized() || m_isVirtualKeyboardVisible)
		{
			if (m_isVirtualKeyboardShowRequested)
			{
				m_isVirtualKeyboardShowRequested = false;
			}
			if (DLL_W8HideKeyboard() == 0)
			{
				m_isVirtualKeyboardHideRequested = true;
			}
		}
	}

	public string GetIntelDeviceName()
	{
		if (!IsInitialized())
		{
			return null;
		}
		return IntelDevice.GetDeviceName(DLL_W8GetDeviceId());
	}

	public PowerSource GetBatteryMode()
	{
		if (!IsInitialized())
		{
			return PowerSource.Unintialized;
		}
		return (PowerSource)DLL_W8GetBatteryMode();
	}

	public bool IsVirtualKeyboardVisible()
	{
		if (!IsInitialized())
		{
			return false;
		}
		return m_isVirtualKeyboardVisible;
	}

	public Vector3 GetTouchPosition()
	{
		if (!IsInitialized() || m_touchState == null)
		{
			return new Vector3(0f, 0f, 0f);
		}
		return new Vector3(m_touchPosition.x, m_touchPosition.y, m_touchPosition.z);
	}

	public Vector3 GetTouchPositionForGUI()
	{
		if (!IsInitialized() || m_touchState == null)
		{
			return new Vector3(0f, 0f, 0f);
		}
		Vector2 touchPosition = TransformTouchPosition(m_touchPosition);
		return new Vector3(touchPosition.x, touchPosition.y, m_touchPosition.z);
	}

	public bool IsTouchSupported()
	{
		return m_isWindows8OrGreater;
	}

	public void AddOnVirtualKeyboardShowListener(Action listener)
	{
		VirtualKeyboardDidShow -= listener;
		VirtualKeyboardDidShow += listener;
	}

	public void RemoveOnVirtualKeyboardShowListener(Action listener)
	{
		VirtualKeyboardDidShow -= listener;
	}

	public void AddOnVirtualKeyboardHideListener(Action listener)
	{
		VirtualKeyboardDidHide -= listener;
		VirtualKeyboardDidHide += listener;
	}

	public void RemoveOnVirtualKeyboardHideListener(Action listener)
	{
		VirtualKeyboardDidHide -= listener;
	}

	private IntPtr GetFunction(string name)
	{
		IntPtr procAddress = DLLUtils.GetProcAddress(m_DLL, name);
		if (procAddress == IntPtr.Zero)
		{
			Debug.LogError("Could not load W8TouchDLL." + name + "()");
			OnApplicationQuit();
		}
		return procAddress;
	}

	private bool LoadW8TouchDLL()
	{
		if (Environment.OSVersion.Version.Major < 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 2))
		{
			Log.W8Touch.Print("Windows Version is Pre-Windows 8");
			return false;
		}
		if (m_DLL == IntPtr.Zero)
		{
			m_DLL = DLLUtils.LoadPlugin("W8TouchDLL", handleError: false);
			if (m_DLL == IntPtr.Zero)
			{
				Log.W8Touch.Print("Could not load W8TouchDLL.dll");
				return false;
			}
		}
		DLL_W8ShowKeyboard = (DelW8ShowKeyboard)Marshal.GetDelegateForFunctionPointer(GetFunction("W8_ShowKeyboard"), typeof(DelW8ShowKeyboard));
		DLL_W8HideKeyboard = (DelW8HideKeyboard)Marshal.GetDelegateForFunctionPointer(GetFunction("W8_HideKeyboard"), typeof(DelW8HideKeyboard));
		DLL_W8ShowOSK = (DelW8ShowOSK)Marshal.GetDelegateForFunctionPointer(GetFunction("W8_ShowOSK"), typeof(DelW8ShowOSK));
		DLL_W8Initialize = (DelW8Initialize)Marshal.GetDelegateForFunctionPointer(GetFunction("W8_Initialize"), typeof(DelW8Initialize));
		DLL_W8Shutdown = (DelW8Shutdown)Marshal.GetDelegateForFunctionPointer(GetFunction("W8_Shutdown"), typeof(DelW8Shutdown));
		DLL_W8GetDeviceId = (DelW8GetDeviceId)Marshal.GetDelegateForFunctionPointer(GetFunction("W8_GetDeviceId"), typeof(DelW8GetDeviceId));
		DLL_W8IsWindows8OrGreater = (DelW8IsWindows8OrGreater)Marshal.GetDelegateForFunctionPointer(GetFunction("W8_IsWindows8OrGreater"), typeof(DelW8IsWindows8OrGreater));
		DLL_W8IsLastEventFromTouch = (DelW8IsLastEventFromTouch)Marshal.GetDelegateForFunctionPointer(GetFunction("W8_IsLastEventFromTouch"), typeof(DelW8IsLastEventFromTouch));
		DLL_W8GetBatteryMode = (DelW8GetBatteryMode)Marshal.GetDelegateForFunctionPointer(GetFunction("W8_GetBatteryMode"), typeof(DelW8GetBatteryMode));
		DLL_W8GetPercentBatteryLife = (DelW8GetPercentBatteryLife)Marshal.GetDelegateForFunctionPointer(GetFunction("W8_GetPercentBatteryLife"), typeof(DelW8GetPercentBatteryLife));
		DLL_W8GetDesktopRect = (DelW8GetDesktopRect)Marshal.GetDelegateForFunctionPointer(GetFunction("W8_GetDesktopRect"), typeof(DelW8GetDesktopRect));
		DLL_W8IsVirtualKeyboardVisible = (DelW8IsVirtualKeyboardVisible)Marshal.GetDelegateForFunctionPointer(GetFunction("W8_IsVirtualKeyboardVisible"), typeof(DelW8IsVirtualKeyboardVisible));
		DLL_W8GetTouchPointCount = (DelW8GetTouchPointCount)Marshal.GetDelegateForFunctionPointer(GetFunction("GetTouchPointCount"), typeof(DelW8GetTouchPointCount));
		DLL_W8GetTouchPoint = (DelW8GetTouchPoint)Marshal.GetDelegateForFunctionPointer(GetFunction("GetTouchPoint"), typeof(DelW8GetTouchPoint));
		return true;
	}

	private void OnApplicationQuit()
	{
		Log.W8Touch.Print("W8Touch.AppQuit()");
		if (!(m_DLL == IntPtr.Zero))
		{
			ResetWindowFeedbackSetting();
			if (DLL_W8Shutdown != null && m_initialized)
			{
				DLL_W8Shutdown();
				m_initialized = false;
			}
			if (!DLLUtils.FreeLibrary(m_DLL))
			{
				Debug.Log("Error unloading W8TouchDLL.dll");
			}
			m_DLL = IntPtr.Zero;
		}
	}

	private bool IsInitialized()
	{
		if (m_DLL != IntPtr.Zero && m_isWindows8OrGreater)
		{
			return m_initialized;
		}
		return false;
	}

	private void InitializeDLL()
	{
		if (m_intializationAttemptCount >= 10)
		{
			return;
		}
		string hearthstoneWindow = GameStrings.Get("GLOBAL_PROGRAMNAME_HEARTHSTONE");
		int errorCode = -1;
		if (DLL_W8Initialize != null)
		{
			errorCode = DLL_W8Initialize(hearthstoneWindow);
		}
		if (errorCode < 0)
		{
			m_intializationAttemptCount++;
			return;
		}
		Log.W8Touch.Print("W8Touch Start Success!");
		m_initialized = true;
		IntPtr User32DLL = DLLUtils.LoadLibrary("User32.DLL");
		if (User32DLL == IntPtr.Zero)
		{
			Log.W8Touch.Print("Could not load User32.DLL");
			return;
		}
		IntPtr pFunc = DLLUtils.GetProcAddress(User32DLL, "SetWindowFeedbackSetting");
		if (pFunc == IntPtr.Zero)
		{
			Log.W8Touch.Print("Could not load User32.SetWindowFeedbackSetting()");
		}
		else
		{
			IntPtr hearthstoneWindowHandle = FindWindow(null, "Hearthstone");
			if (hearthstoneWindowHandle == IntPtr.Zero)
			{
				hearthstoneWindowHandle = FindWindow(null, GameStrings.Get("GLOBAL_PROGRAMNAME_HEARTHSTONE"));
			}
			if (hearthstoneWindowHandle == IntPtr.Zero)
			{
				Log.W8Touch.Print("Unable to retrieve Hearthstone window handle!");
			}
			else
			{
				DelSetWindowFeedbackSetting obj = (DelSetWindowFeedbackSetting)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(DelSetWindowFeedbackSetting));
				int size = Marshal.SizeOf(typeof(int));
				IntPtr pSettingValue = Marshal.AllocHGlobal(size);
				Marshal.WriteInt32(pSettingValue, 0, m_bWindowFeedbackSettingValue ? 1 : 0);
				bool isSuccess = true;
				if (!obj(hearthstoneWindowHandle, FEEDBACK_TYPE.FEEDBACK_TOUCH_CONTACTVISUALIZATION, 0u, Convert.ToUInt32(size), pSettingValue))
				{
					Log.W8Touch.Print("FEEDBACK_TOUCH_CONTACTVISUALIZATION failed!");
					isSuccess = false;
				}
				if (!obj(hearthstoneWindowHandle, FEEDBACK_TYPE.FEEDBACK_TOUCH_TAP, 0u, Convert.ToUInt32(size), pSettingValue))
				{
					Log.W8Touch.Print("FEEDBACK_TOUCH_TAP failed!");
					isSuccess = false;
				}
				if (!obj(hearthstoneWindowHandle, FEEDBACK_TYPE.FEEDBACK_TOUCH_PRESSANDHOLD, 0u, Convert.ToUInt32(size), pSettingValue))
				{
					Log.W8Touch.Print("FEEDBACK_TOUCH_PRESSANDHOLD failed!");
					isSuccess = false;
				}
				if (!obj(hearthstoneWindowHandle, FEEDBACK_TYPE.FEEDBACK_TOUCH_DOUBLETAP, 0u, Convert.ToUInt32(size), pSettingValue))
				{
					Log.W8Touch.Print("FEEDBACK_TOUCH_DOUBLETAP failed!");
					isSuccess = false;
				}
				if (!obj(hearthstoneWindowHandle, FEEDBACK_TYPE.FEEDBACK_TOUCH_RIGHTTAP, 0u, Convert.ToUInt32(size), pSettingValue))
				{
					Log.W8Touch.Print("FEEDBACK_TOUCH_RIGHTTAP failed!");
					isSuccess = false;
				}
				if (!obj(hearthstoneWindowHandle, FEEDBACK_TYPE.FEEDBACK_GESTURE_PRESSANDTAP, 0u, Convert.ToUInt32(size), pSettingValue))
				{
					Log.W8Touch.Print("FEEDBACK_GESTURE_PRESSANDTAP failed!");
					isSuccess = false;
				}
				m_bIsWindowFeedbackDisabled = isSuccess;
				if (m_bIsWindowFeedbackDisabled)
				{
					Log.W8Touch.Print("Windows 8 Feedback Touch Gestures Disabled!");
				}
				Marshal.FreeHGlobal(pSettingValue);
			}
		}
		if (!DLLUtils.FreeLibrary(User32DLL))
		{
			Log.W8Touch.Print("Error unloading User32.dll");
		}
	}

	private void ResetWindowFeedbackSetting()
	{
		if (!m_initialized || !m_bIsWindowFeedbackDisabled)
		{
			return;
		}
		IntPtr User32DLL = DLLUtils.LoadLibrary("User32.DLL");
		if (User32DLL == IntPtr.Zero)
		{
			Log.W8Touch.Print("Could not load User32.DLL");
			return;
		}
		IntPtr pFunc = DLLUtils.GetProcAddress(User32DLL, "SetWindowFeedbackSetting");
		if (pFunc == IntPtr.Zero)
		{
			Log.W8Touch.Print("Could not load User32.SetWindowFeedbackSetting()");
		}
		else
		{
			IntPtr hearthstoneWindowHandle = FindWindow(null, "Hearthstone");
			if (hearthstoneWindowHandle == IntPtr.Zero)
			{
				hearthstoneWindowHandle = FindWindow(null, GameStrings.Get("GLOBAL_PROGRAMNAME_HEARTHSTONE"));
			}
			if (hearthstoneWindowHandle == IntPtr.Zero)
			{
				Log.W8Touch.Print("Unable to retrieve Hearthstone window handle!");
			}
			else
			{
				DelSetWindowFeedbackSetting obj = (DelSetWindowFeedbackSetting)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(DelSetWindowFeedbackSetting));
				IntPtr pSettingValue = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
				Marshal.WriteInt32(pSettingValue, 0, m_bWindowFeedbackSettingValue ? 1 : 0);
				bool isSuccess = true;
				if (!obj(hearthstoneWindowHandle, FEEDBACK_TYPE.FEEDBACK_TOUCH_CONTACTVISUALIZATION, 0u, 0u, IntPtr.Zero))
				{
					Log.W8Touch.Print("FEEDBACK_TOUCH_CONTACTVISUALIZATION failed!");
					isSuccess = false;
				}
				if (!obj(hearthstoneWindowHandle, FEEDBACK_TYPE.FEEDBACK_TOUCH_TAP, 0u, 0u, IntPtr.Zero))
				{
					Log.W8Touch.Print("FEEDBACK_TOUCH_TAP failed!");
					isSuccess = false;
				}
				if (!obj(hearthstoneWindowHandle, FEEDBACK_TYPE.FEEDBACK_TOUCH_PRESSANDHOLD, 0u, 0u, IntPtr.Zero))
				{
					Log.W8Touch.Print("FEEDBACK_TOUCH_PRESSANDHOLD failed!");
					isSuccess = false;
				}
				if (!obj(hearthstoneWindowHandle, FEEDBACK_TYPE.FEEDBACK_TOUCH_DOUBLETAP, 0u, 0u, IntPtr.Zero))
				{
					Log.W8Touch.Print("FEEDBACK_TOUCH_DOUBLETAP failed!");
					isSuccess = false;
				}
				if (!obj(hearthstoneWindowHandle, FEEDBACK_TYPE.FEEDBACK_TOUCH_RIGHTTAP, 0u, 0u, IntPtr.Zero))
				{
					Log.W8Touch.Print("FEEDBACK_TOUCH_RIGHTTAP failed!");
					isSuccess = false;
				}
				if (!obj(hearthstoneWindowHandle, FEEDBACK_TYPE.FEEDBACK_GESTURE_PRESSANDTAP, 0u, 0u, IntPtr.Zero))
				{
					Log.W8Touch.Print("FEEDBACK_GESTURE_PRESSANDTAP failed!");
					isSuccess = false;
				}
				m_bIsWindowFeedbackDisabled = !isSuccess;
				if (!m_bIsWindowFeedbackDisabled)
				{
					Log.W8Touch.Print("Windows 8 Feedback Touch Gestures Reset!");
				}
				Marshal.FreeHGlobal(pSettingValue);
			}
		}
		if (!DLLUtils.FreeLibrary(User32DLL))
		{
			Log.W8Touch.Print("Error unloading User32.dll");
		}
	}
}
