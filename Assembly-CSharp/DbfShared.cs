using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Hearthstone.Util;
using UnityEngine;

public static class DbfShared
{
	private static AssetBundle s_assetBundle;

	private static EventTimingMap s_eventMap;

	public static AssetBundle GetAssetBundle()
	{
		return s_assetBundle;
	}

	public static EventTimingMap GetEventMap()
	{
		return s_eventMap;
	}

	public static void LoadSharedAssetBundle()
	{
		string dbfAssetBundlePath = GetSharedDBFAssetBundlePath();
		s_assetBundle = AssetBundle.LoadFromFile(dbfAssetBundlePath);
		if (s_assetBundle == null)
		{
			Debug.LogErrorFormat("Failed to load DBF asset bundle from: \"{0}\"", dbfAssetBundlePath);
		}
		else
		{
			LoadSpecialEventMap();
		}
	}

	public static IEnumerator<IAsyncJobResult> Job_LoadSharedDBFAssetBundle()
	{
		string dbfAssetBundlePath = GetSharedDBFAssetBundlePath();
		LoadAssetBundleFromFile loadDBFSharedAssetBundle = new LoadAssetBundleFromFile(dbfAssetBundlePath, failOnError: true);
		yield return loadDBFSharedAssetBundle;
		s_assetBundle = loadDBFSharedAssetBundle.LoadedAssetBundle;
		LoadSpecialEventMap();
	}

	private static void LoadSpecialEventMap()
	{
		s_eventMap = s_assetBundle.LoadAsset<EventTimingMap>("Assets/Game/DBF-Asset/EventMap.asset");
		s_eventMap.Initialize();
	}

	private static string GetSharedDBFAssetBundlePath()
	{
		if (AssetLoaderPrefs.AssetLoadingMethod == AssetLoaderPrefs.ASSET_LOADING_METHOD.ASSET_BUNDLES)
		{
			return PlatformFilePaths.CreateLocalFilePath(string.Format("Data/{0}{1}", AssetBundleInfo.BundlePathPlatformModifier(), "dbf.unity3d"));
		}
		return "Assets/Game/DBF-Asset/dbf.unity3d";
	}

	public static void Initialize()
	{
		s_eventMap = ScriptableObject.CreateInstance<EventTimingMap>();
	}

	public static void Reset()
	{
		if (s_assetBundle != null)
		{
			s_assetBundle.Unload(unloadAllLoadedObjects: true);
		}
		s_assetBundle = null;
		if (s_eventMap != null)
		{
			s_eventMap.Reset();
		}
	}
}
