using System;
using UnityEngine;

[Serializable]
public class MobileOverrideValue<T>
{
	public ScreenCategory[] screens;

	public T[] values;

	public MobileOverrideValue()
	{
		screens = new ScreenCategory[1];
		screens[0] = ScreenCategory.PC;
		values = new T[1];
		values[0] = default(T);
	}

	public MobileOverrideValue(T defaultValue)
	{
		screens = new ScreenCategory[1] { ScreenCategory.PC };
		values = new T[1] { defaultValue };
	}

	public static implicit operator T(MobileOverrideValue<T> val)
	{
		if (val == null)
		{
			return default(T);
		}
		ScreenCategory[] screens = val.screens;
		T[] values = val.values;
		if (screens.Length < 1)
		{
			Debug.LogError("MobileOverrideValue should always have at least one value!");
			return default(T);
		}
		T result = values[0];
		ScreenCategory currentScreen = PlatformSettings.Screen;
		for (int i = 1; i < screens.Length; i++)
		{
			if (currentScreen == screens[i])
			{
				result = values[i];
			}
		}
		return result;
	}

	public T[] GetValues()
	{
		return values;
	}

	public T GetValueForScreen(ScreenCategory screen, object defaultValue)
	{
		int index = Array.IndexOf(screens, screen);
		if (index != -1)
		{
			return values[index];
		}
		return (T)defaultValue;
	}
}
