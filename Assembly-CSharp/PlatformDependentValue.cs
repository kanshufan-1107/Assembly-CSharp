using System;
using UnityEngine;

[Serializable]
public class PlatformDependentValue<T>
{
	private bool resolved;

	private T result;

	[SerializeField]
	private PlatformCategory type;

	private T defaultValue;

	[SerializeField]
	private T[] settings = new T[14];

	[SerializeField]
	private bool[] isSet = new bool[14];

	public T PC
	{
		set
		{
			SetValue(PlatformSettingType.PC, value);
		}
	}

	public T Mac
	{
		set
		{
			SetValue(PlatformSettingType.Mac, value);
		}
	}

	public T iOS
	{
		set
		{
			SetValue(PlatformSettingType.iOS, value);
		}
	}

	public T Android
	{
		set
		{
			SetValue(PlatformSettingType.Android, value);
		}
	}

	public T Tablet
	{
		set
		{
			SetValue(PlatformSettingType.Tablet, value);
		}
	}

	public T MiniTablet
	{
		set
		{
			SetValue(PlatformSettingType.MiniTablet, value);
		}
	}

	public T Phone
	{
		set
		{
			SetValue(PlatformSettingType.Phone, value);
		}
	}

	public T Mouse
	{
		set
		{
			SetValue(PlatformSettingType.Mouse, value);
		}
	}

	public T Touch
	{
		set
		{
			SetValue(PlatformSettingType.Touch, value);
		}
	}

	public T LowMemory
	{
		set
		{
			SetValue(PlatformSettingType.LowMemory, value);
		}
	}

	public T MediumMemory
	{
		set
		{
			SetValue(PlatformSettingType.MediumMemory, value);
		}
	}

	public T HighMemory
	{
		set
		{
			SetValue(PlatformSettingType.HighMemory, value);
		}
	}

	public T Value
	{
		get
		{
			if (resolved)
			{
				return result;
			}
			switch (type)
			{
			case PlatformCategory.OS:
				result = GetOSSetting(PlatformSettings.OS);
				break;
			case PlatformCategory.Screen:
				result = GetScreenSetting(PlatformSettings.Screen);
				break;
			case PlatformCategory.Memory:
				result = GetMemorySetting(PlatformSettings.Memory);
				break;
			case PlatformCategory.Input:
				result = GetInputSetting(PlatformSettings.Input);
				break;
			}
			resolved = true;
			return result;
		}
	}

	public PlatformDependentValue()
	{
		InitSettingsMap();
	}

	public PlatformDependentValue(PlatformCategory t)
	{
		type = t;
		InitSettingsMap();
	}

	public static implicit operator T(PlatformDependentValue<T> val)
	{
		return val.Value;
	}

	private void InitSettingsMap()
	{
		for (int i = 0; i < 14; i++)
		{
			settings[i] = default(T);
			isSet[i] = false;
		}
	}

	private void SetValue(PlatformSettingType type, T value)
	{
		settings[(int)type] = value;
		isSet[(int)type] = true;
	}

	public T GetValue(PlatformSettingType type)
	{
		return settings[(int)type];
	}

	public bool IsSet(PlatformSettingType type)
	{
		return isSet[(int)type];
	}

	private T GetOSSetting(OSCategory os)
	{
		switch (os)
		{
		case OSCategory.PC:
			if (IsSet(PlatformSettingType.PC))
			{
				return GetValue(PlatformSettingType.PC);
			}
			break;
		case OSCategory.Mac:
			if (!IsSet(PlatformSettingType.Mac))
			{
				return GetOSSetting(OSCategory.PC);
			}
			return GetValue(PlatformSettingType.Mac);
		case OSCategory.iOS:
			if (!IsSet(PlatformSettingType.iOS))
			{
				return GetOSSetting(OSCategory.PC);
			}
			return GetValue(PlatformSettingType.iOS);
		case OSCategory.Android:
			if (!IsSet(PlatformSettingType.Android))
			{
				return GetOSSetting(OSCategory.PC);
			}
			return GetValue(PlatformSettingType.Android);
		}
		Debug.LogError("Could not find OS dependent value");
		return default(T);
	}

	private T GetScreenSetting(ScreenCategory screen)
	{
		switch (screen)
		{
		case ScreenCategory.PC:
			if (IsSet(PlatformSettingType.PC))
			{
				return GetValue(PlatformSettingType.PC);
			}
			break;
		case ScreenCategory.Tablet:
			if (!IsSet(PlatformSettingType.Tablet))
			{
				return GetScreenSetting(ScreenCategory.PC);
			}
			return GetValue(PlatformSettingType.Tablet);
		case ScreenCategory.Phone:
			if (!IsSet(PlatformSettingType.Phone))
			{
				return GetScreenSetting(ScreenCategory.Tablet);
			}
			return GetValue(PlatformSettingType.Phone);
		case ScreenCategory.MiniTablet:
			if (!IsSet(PlatformSettingType.MiniTablet))
			{
				return GetScreenSetting(ScreenCategory.Tablet);
			}
			return GetValue(PlatformSettingType.MiniTablet);
		}
		Debug.LogError("Could not find screen dependent value");
		return default(T);
	}

	private T GetMemorySetting(MemoryCategory memory)
	{
		switch (memory)
		{
		case MemoryCategory.Low:
			if (IsSet(PlatformSettingType.LowMemory))
			{
				return GetValue(PlatformSettingType.LowMemory);
			}
			break;
		case MemoryCategory.Medium:
			if (!IsSet(PlatformSettingType.MediumMemory))
			{
				return GetMemorySetting(MemoryCategory.Low);
			}
			return GetValue(PlatformSettingType.MediumMemory);
		case MemoryCategory.High:
			if (!IsSet(PlatformSettingType.HighMemory))
			{
				return GetMemorySetting(MemoryCategory.Medium);
			}
			return GetValue(PlatformSettingType.HighMemory);
		}
		Debug.LogError("Could not find memory dependent value");
		return default(T);
	}

	private T GetInputSetting(InputCategory input)
	{
		switch (input)
		{
		case InputCategory.Mouse:
			if (IsSet(PlatformSettingType.Mouse))
			{
				return GetValue(PlatformSettingType.Mouse);
			}
			break;
		case InputCategory.Touch:
			if (!IsSet(PlatformSettingType.Touch))
			{
				return GetInputSetting(InputCategory.Mouse);
			}
			return GetValue(PlatformSettingType.Touch);
		}
		Debug.LogError("Could not find input dependent value");
		return default(T);
	}
}
