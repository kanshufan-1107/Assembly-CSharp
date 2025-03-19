using System.Collections.Generic;
using Blizzard.T5.Configuration;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Util;
using UnityEngine;

public class PlatformSettings
{
	public static bool s_isDeviceSupported = true;

	public static bool s_isDeviceInMinSpec = true;

	public static OSCategory s_os = OSCategory.PC;

	public static MemoryCategory s_memory = MemoryCategory.High;

	public static ScreenCategory s_screen = ScreenCategory.PC;

	public static InputCategory s_input = InputCategory.Mouse;

	public static ScreenDensityCategory s_screenDensity = ScreenDensityCategory.High;

	private static LocaleVariant s_CurrentLocaleVariant = LocaleVariant.Global;

	private static IGraphicsManager s_graphicsManager;

	private static string s_deviceModel = null;

	private static bool s_isEmulating = false;

	public static OSCategory OS => s_os;

	public static OSCategory RuntimeOS => OSCategory.PC;

	public static LocaleVariant LocaleVariant
	{
		get
		{
			if (s_CurrentLocaleVariant != 0)
			{
				return s_CurrentLocaleVariant;
			}
			s_CurrentLocaleVariant = ((!HearthstoneApplication.IsCNMobileBinary) ? LocaleVariant.Global : LocaleVariant.China);
			return s_CurrentLocaleVariant;
		}
	}

	public static MemoryCategory Memory => s_memory;

	public static ScreenCategory Screen => s_screen;

	public static InputCategory Input => s_input;

	public static bool IsEmulating => s_isEmulating;

	public static string DeviceName
	{
		get
		{
			if (string.IsNullOrEmpty(SystemInfo.deviceModel))
			{
				return "unknown";
			}
			return SystemInfo.deviceModel;
		}
	}

	public static string DeviceModel => s_deviceModel ?? DeviceName;

	public static bool ShouldFallbackToLowRes
	{
		get
		{
			if (Application.isEditor || IsEmulating)
			{
				return false;
			}
			if (Screen == ScreenCategory.Phone)
			{
				return true;
			}
			if (s_graphicsManager == null)
			{
				s_graphicsManager = ServiceManager.Get<IGraphicsManager>();
			}
			if (IsMobileRuntimeOS && s_graphicsManager != null && s_graphicsManager.RenderQualityLevel == GraphicsQuality.Low)
			{
				return true;
			}
			if (RuntimeOS == OSCategory.iOS)
			{
				NetCache netCache = NetCache.Get();
				if (netCache != null)
				{
					NetCache.NetCacheFeatures guardianVars = netCache.GetNetObject<NetCache.NetCacheFeatures>();
					if (guardianVars != null && guardianVars.ForceIosLowRes)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public static bool IsMobileRuntimeOS
	{
		get
		{
			OSCategory runtimeOS = RuntimeOS;
			if (runtimeOS != OSCategory.iOS)
			{
				return runtimeOS == OSCategory.Android;
			}
			return true;
		}
	}

	public static bool IsTablet
	{
		get
		{
			if (IsMobile())
			{
				if (Screen != ScreenCategory.MiniTablet)
				{
					return Screen == ScreenCategory.Tablet;
				}
				return true;
			}
			return false;
		}
	}

	public static bool IsSteam => false;

	public static int GetBestScreenMatch(List<ScreenCategory> categories)
	{
		ScreenCategory screen = Screen;
		int index = 0;
		int difference = int.MaxValue;
		for (int i = 0; i < categories.Count; i++)
		{
			int newDifference = categories[i] - screen;
			if (newDifference >= 0 && newDifference < difference)
			{
				index = i;
				difference = newDifference;
			}
		}
		return index;
	}

	public static bool IsMobile()
	{
		if (OS != OSCategory.iOS)
		{
			return OS == OSCategory.Android;
		}
		return true;
	}

	public static void RecomputeDeviceSettings()
	{
		if (!EmulateMobileDevice())
		{
			s_os = OSCategory.PC;
			s_input = InputCategory.Mouse;
			s_screen = ScreenCategory.PC;
			s_screenDensity = ScreenDensityCategory.High;
			s_os = OSCategory.PC;
			int memory = SystemInfo.systemMemorySize;
			VarKey memEmulationKey = Vars.Key("Emulation.MemoryMB");
			if (memEmulationKey.HasValue)
			{
				memory = memEmulationKey.GetInt(memory);
			}
			MemoryCategory memoryCategory = ((memory >= 3500) ? ((memory < 4500) ? MemoryCategory.Medium : (s_memory = MemoryCategory.High)) : MemoryCategory.Low);
			s_memory = memoryCategory;
			if (memory < 3072)
			{
				Debug.LogWarning($"Low Memory Warning: Device has only {memory} MBs of system memory and is below min spec");
			}
		}
	}

	private static bool EmulateMobileDevice()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return false;
		}
		ConfigFile config = new ConfigFile();
		if (!config.FullLoad(PlatformFilePaths.GetClientConfigPath()))
		{
			Debug.LogWarningFormat("Failed to read DeviceEmulation from {0}", PlatformFilePaths.GetClientConfigPath());
			return false;
		}
		DevicePreset emulatedDevice = new DevicePreset();
		emulatedDevice.ReadFromConfig(config);
		if (emulatedDevice.name == "No Emulation")
		{
			return false;
		}
		if (!config.Get("Emulation.emulateOnDevice", defaultVal: false))
		{
			return false;
		}
		s_isEmulating = true;
		s_os = emulatedDevice.os;
		s_input = emulatedDevice.input;
		s_screen = emulatedDevice.screen;
		s_screenDensity = emulatedDevice.screenDensity;
		Log.DeviceEmulation.Print("Emulating an " + emulatedDevice.name);
		return true;
	}
}
