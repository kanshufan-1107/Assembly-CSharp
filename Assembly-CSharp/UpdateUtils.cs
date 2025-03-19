using System;
using System.Collections.Generic;
using System.ComponentModel;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Hearthstone.Util;
using MiniJSON;
using UnityEngine;

public class UpdateUtils
{
	public enum HearthstoneStore
	{
		[Description("None")]
		NONE,
		[Description("Google")]
		GOOGLE,
		[Description("Amazon")]
		AMAZON,
		[Description("OneStore")]
		ONE_STORE,
		[Description("CN")]
		BLIZZARD,
		[Description("CN_Huawei")]
		HUAWEI,
		[Description("CN_Dashen")]
		DASHEN,
		[Description("AppStore")]
		APP_STORE,
		[Description("AppStoreCN")]
		APP_STORE_CN
	}

	private struct StoreInfo
	{
		public string Deeplink;

		public string Url;

		public StoreInfo(string deeplink, string url)
		{
			Deeplink = deeplink;
			Url = url;
		}

		public override string ToString()
		{
			return Deeplink + ", " + Url;
		}
	}

	private static Map<HearthstoneStore, StoreInfo> s_storeUrls = new Map<HearthstoneStore, StoreInfo>();

	public static bool AreUpdatesEnabledForCurrentPlatform
	{
		get
		{
			if (PlatformSettings.IsMobileRuntimeOS)
			{
				return !DemoMgr.Get().IsDemo();
			}
			return false;
		}
	}

	public static HearthstoneStore GetCurrentStore
	{
		get
		{
			if (PlatformSettings.RuntimeOS == OSCategory.Android)
			{
				return ConvertToHearthstoneStore(AndroidDeviceSettings.Get().m_HSStore);
			}
			if (PlatformSettings.RuntimeOS == OSCategory.iOS)
			{
				if (!RegionUtils.IsCNLegalRegion)
				{
					return HearthstoneStore.APP_STORE;
				}
				return HearthstoneStore.APP_STORE_CN;
			}
			return HearthstoneStore.NONE;
		}
	}

	public static bool addSkipBackupAttributeToItemAtPath(string path)
	{
		return true;
	}

	public static void ShowWirelessSettings()
	{
	}

	public static void ResizeListIfNeeded(List<string> list, int minSize)
	{
		if (list.Capacity < minSize)
		{
			list.Capacity = minSize;
		}
	}

	public static string GetLocaleFromAssetBundle(string assetBundleName)
	{
		string[] splitTags = assetBundleName.Split('-')[0].Split('_');
		if (splitTags.Length != 0)
		{
			string locale = splitTags[splitTags.Length - 1];
			if (locale.Length == 2)
			{
				locale = splitTags[splitTags.Length - 2];
			}
			locale = locale.Substring(0, 2) + locale.Substring(2, 2).ToUpper();
			if (Localization.IsValidLocaleName(locale))
			{
				return locale;
			}
		}
		return string.Empty;
	}

	public static bool GetSplitVersion(string versionStr, out int[] versionInt)
	{
		Log.Downloader.PrintInfo("VersionStr=" + versionStr);
		try
		{
			List<string> versionStrs = new List<string>();
			string[] versionStrSplit = versionStr.Split('_');
			int minVersionLength = 4;
			if (versionStrSplit.Length == 1)
			{
				versionStrs.AddRange(versionStr.Split('.'));
			}
			else
			{
				string binVer = Vars.Key("Mobile.BinaryVersion").GetStr("");
				string binVerInTail = string.Empty;
				if (!string.IsNullOrEmpty(binVer))
				{
					binVerInTail = "." + binVer;
					versionStrSplit[1] = versionStrSplit[1].Replace(binVerInTail, "");
				}
				versionStrs.AddRange(versionStrSplit[1].Split('-')[0].Split('.'));
				versionStrs.Add(versionStrSplit[0]);
				if (!string.IsNullOrEmpty(binVer))
				{
					versionStrs.Add(binVer);
					minVersionLength++;
				}
			}
			versionInt = Array.ConvertAll(versionStrs.ToArray(), int.Parse);
			if (versionInt.Length < minVersionLength)
			{
				throw new Exception("Version is too short");
			}
		}
		catch (Exception ex)
		{
			Error.AddDevFatal("Failed to parse the version string-'{0}': {1}", versionStr, ex.Message);
			versionInt = new int[0];
			return false;
		}
		return true;
	}

	public static T ProcessBlobKey<T>(string key, string debugKey, string description, T defaultValue)
	{
		string strInBlob = AgentEmbeddedAPI.GetOpaqueString(key);
		string debugValue = (Vars.Key(debugKey).HasValue ? Vars.Key(debugKey).GetStr(string.Empty) : string.Empty);
		T returnValue = defaultValue;
		if (!string.IsNullOrEmpty(strInBlob))
		{
			returnValue = ((defaultValue is bool) ? ((T)(object)GeneralUtils.ForceBool(strInBlob)) : ((!(defaultValue is int)) ? ((T)(object)strInBlob) : ((T)(object)GeneralUtils.ForceInt(strInBlob))));
		}
		if (!string.IsNullOrEmpty(debugValue))
		{
			Log.Downloader.PrintInfo("Has debug override for '" + (description ?? key) + "': " + debugValue);
			returnValue = ((defaultValue is bool) ? ((T)(object)GeneralUtils.ForceBool(debugValue)) : ((!(defaultValue is int)) ? ((T)(object)debugValue) : ((T)(object)GeneralUtils.ForceInt(debugValue))));
		}
		return returnValue;
	}

	public static void Initialize()
	{
		s_storeUrls[HearthstoneStore.GOOGLE] = new StoreInfo("market://details?id={PKG}", "https://play.google.com/store/apps/details?id={PKG}");
		s_storeUrls[HearthstoneStore.AMAZON] = new StoreInfo("amzn://apps/android?p={PKG}", "https://www.amazon.com/gp/mas/dl/android?p={PKG}");
		s_storeUrls[HearthstoneStore.ONE_STORE] = new StoreInfo("onestore://common/product/OA00752154", "https://onesto.re/OA00752154");
		s_storeUrls[HearthstoneStore.BLIZZARD] = new StoreInfo("https://adl.netease.com/d/g/hs/c/gw", "https://adl.netease.com/d/g/hs/c/gw");
		s_storeUrls[HearthstoneStore.DASHEN] = new StoreInfo("https://xm.gameyw.netease.com/game-cps/ds/ld2/netease.wyds", "https://xm.gameyw.netease.com/game-cps/ds/ld2/netease.wyds");
		s_storeUrls[HearthstoneStore.HUAWEI] = new StoreInfo("hiapp://com.huawei.appmarket?activityName=activityUri|appdetail.activity&params={\"params\":[{\"name\":\"uri\",\"type\":\"String\",\"value\":\"app|C101767035\"}]}", "https://appgallery.huawei.com/#/app/C101767035");
		s_storeUrls[HearthstoneStore.APP_STORE] = new StoreInfo("https://itunes.apple.com/app/hearthstone-heroes-warcraft/id625257520?ls=1&mt=8", "https://itunes.apple.com/app/hearthstone-heroes-warcraft/id625257520?ls=1&mt=8");
		s_storeUrls[HearthstoneStore.APP_STORE_CN] = new StoreInfo("https://apps.apple.com/app/%E7%82%89%E7%9F%B3%E4%BC%A0%E8%AF%B4/id841140063", "https://apps.apple.com/app/%E7%82%89%E7%9F%B3%E4%BC%A0%E8%AF%B4/id841140063");
	}

	public static HearthstoneStore ConvertToHearthstoneStore(string storeKey)
	{
		foreach (KeyValuePair<HearthstoneStore, StoreInfo> store in s_storeUrls)
		{
			if (EnumUtils.GetString(store.Key).Equals(storeKey))
			{
				return store.Key;
			}
		}
		return HearthstoneStore.NONE;
	}

	public static bool LoadStoreURIJsonData(string json, HearthstoneStore store = HearthstoneStore.NONE)
	{
		if (string.IsNullOrEmpty(json))
		{
			return false;
		}
		if (store == HearthstoneStore.NONE)
		{
			store = GetCurrentStore;
			if (store == HearthstoneStore.NONE)
			{
				Log.Downloader.PrintError("Couldn't determine the store.");
				return false;
			}
		}
		StoreInfo storeInfo = s_storeUrls[store];
		string storeKey = EnumUtils.GetString(store);
		Log.Downloader.PrintDebug($"{store} = {storeKey}");
		bool success = false;
		try
		{
			if (Json.Deserialize(json) is JsonNode response && response.ContainsKey(storeKey))
			{
				JsonNode info = response[storeKey] as JsonNode;
				if (info.ContainsKey("deeplink"))
				{
					storeInfo.Deeplink = info["deeplink"] as string;
				}
				if (info.ContainsKey("url"))
				{
					storeInfo.Url = info["url"] as string;
				}
				s_storeUrls[store] = storeInfo;
				success = true;
			}
		}
		catch (Exception ex)
		{
			Log.Downloader.PrintError("Failed to parse the store info: " + ex.Message + "\n'" + json + "'");
		}
		if (success)
		{
			Log.Downloader.PrintInfo("Succeeded to set the store URL for " + storeKey);
			Log.Downloader.PrintDebug($"{s_storeUrls[store]}");
		}
		return success;
	}

	public static bool OpenAppStore()
	{
		_ = string.Empty;
		HearthstoneStore store = GetCurrentStore;
		if (store == HearthstoneStore.NONE || !s_storeUrls.TryGetValue(store, out var storeInfo))
		{
			return false;
		}
		if (PlatformSettings.RuntimeOS == OSCategory.Android)
		{
			string deeplink = storeInfo.Deeplink.Replace("{PKG}", AndroidDeviceSettings.Get().m_HSPackageName);
			string url = storeInfo.Url.Replace("{PKG}", AndroidDeviceSettings.Get().m_HSPackageName);
			AndroidDeviceSettings.Get().OpenAppStore(deeplink, url);
		}
		else if (PlatformSettings.RuntimeOS == OSCategory.iOS)
		{
			Application.OpenURL(storeInfo.Url);
		}
		return true;
	}
}
