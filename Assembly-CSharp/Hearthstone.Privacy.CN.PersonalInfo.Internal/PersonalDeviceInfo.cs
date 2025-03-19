using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.Privacy.CN.PersonalInfo.Internal;

internal static class PersonalDeviceInfo
{
	internal struct DeviceInfoField
	{
		public string Label { get; set; }

		public string Value { get; set; }
	}

	private struct DeviceInfoFetcher
	{
		public string LabelKey { get; set; }

		public Func<string> Fetcher { get; set; }

		public DeviceInfoFetcher(string labelKey, Func<string> func)
		{
			LabelKey = labelKey;
			Fetcher = func;
		}
	}

	private static readonly DeviceInfoFetcher[] s_fetchers = new DeviceInfoFetcher[8]
	{
		new DeviceInfoFetcher("GLOBAL_PERSONAL_INFO_OS_VERSION", () => SystemInfo.operatingSystem),
		new DeviceInfoFetcher("GLOBAL_PERSONAL_INFO_DEVICE_TYPE", GetDeviceType),
		new DeviceInfoFetcher("GLOBAL_PERSONAL_INFO_DEVICE_MODEL", () => SystemInfo.deviceModel),
		new DeviceInfoFetcher("GLOBAL_PERSONAL_INFO_DEVICE_NAME", () => SystemInfo.deviceName),
		new DeviceInfoFetcher("GLOBAL_PERSONAL_INFO_DEVICE_ID", () => SystemInfo.deviceUniqueIdentifier),
		new DeviceInfoFetcher("GLOBAL_PERSONAL_INFO_LOCALE", Localization.GetLocaleName),
		new DeviceInfoFetcher("GLOBAL_PERSONAL_INFO_REGION", () => GameStrings.Get("GLOBAL_REGION_CHINA")),
		new DeviceInfoFetcher("GLOBAL_PERSONAL_INFO_APP_ID", () => TelemetryManager.AppInstallId)
	};

	public static DeviceInfoField[] GetDeviceInfo()
	{
		List<DeviceInfoField> fields = new List<DeviceInfoField>(s_fetchers.Length);
		DeviceInfoFetcher[] array = s_fetchers;
		for (int i = 0; i < array.Length; i++)
		{
			DeviceInfoFetcher fetcher = array[i];
			fields.Add(new DeviceInfoField
			{
				Label = GameStrings.Get(fetcher.LabelKey),
				Value = fetcher.Fetcher()
			});
		}
		return fields.ToArray();
	}

	private static string GetDeviceType()
	{
		return SystemInfo.deviceType switch
		{
			DeviceType.Console => GameStrings.Get("GLOBAL_DEVICE_TYPE_CONSOLE"), 
			DeviceType.Desktop => GameStrings.Get("GLOBAL_DEVICE_TYPE_DESKTOP"), 
			DeviceType.Handheld => GameStrings.Get("GLOBAL_DEVICE_TYPE_HANDHELD"), 
			_ => string.Empty, 
		};
	}
}
