using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Hearthstone.Core;

public class ScriptableAssetManifest : IAssetManifest
{
	private static readonly string ASSET_MANIFEST_FOLDER_PATH = Path.Combine("Assets", "AssetManifest");

	private static readonly string TAGS_METADATA_ASSET_PATH = Path.Combine(ASSET_MANIFEST_FOLDER_PATH, "tags_metadata.asset");

	public static readonly string MainManifestBundleName = "asset_manifest.unity3d";

	public static readonly string BundleDepsAssetPath = Path.Combine(ASSET_MANIFEST_FOLDER_PATH, "bundle_deps.asset");

	private CompositeAssetCatalog m_assetsCatalog;

	private ScriptableAssetTagsMetadata m_tagsMetadata;

	public static ScriptableAssetManifest Load()
	{
		string baseManifestBundlePath = AssetBundleInfo.GetAssetBundlePath(MainManifestBundleName);
		if (!AssetBundleInfo.Exists(baseManifestBundlePath))
		{
			Log.Asset.PrintError("[ScriptableAssetManifest] Cannot find asset bundle for AssetManifest '{0}', editor {1}, playing {2}", MainManifestBundleName, Application.isEditor, Application.isPlaying);
			return null;
		}
		AssetBundle bundle = AssetBundle.LoadFromFile(baseManifestBundlePath);
		if (bundle == null)
		{
			Log.Asset.PrintError("[ScriptableAssetManifest] Failed to open manifest bundle at {0}, editor {1}, playing {2}", baseManifestBundlePath, Application.isEditor, Application.isPlaying);
			return null;
		}
		Debug.Log($"Main thread: {HearthstoneApplication.IsMainThread}");
		Log.Asset.PrintDebug("[ScriptableAssetManifest] Loaded AssetManifest bundle '{0}'", bundle);
		ScriptableAssetManifest scriptableAssetManifest = CreateEmpty();
		scriptableAssetManifest.LoadTagsMetadata(bundle);
		scriptableAssetManifest.m_assetsCatalog.LoadBaseCatalog(bundle);
		scriptableAssetManifest.m_assetsCatalog.LoadQualityCatalogs(bundle);
		scriptableAssetManifest.m_assetsCatalog.LoadPlatformCatalogs(bundle);
		scriptableAssetManifest.m_assetsCatalog.LoadRegionCatalogs();
		scriptableAssetManifest.m_assetsCatalog.LoadLocaleCatalogs();
		bundle.Unload(unloadAllLoadedObjects: false);
		return scriptableAssetManifest;
	}

	private void LoadTagsMetadata(AssetBundle baseAssetBundle)
	{
		ScriptableAssetTagsMetadata tagsMetadata = baseAssetBundle.LoadAsset<ScriptableAssetTagsMetadata>(TAGS_METADATA_ASSET_PATH);
		if (tagsMetadata != null)
		{
			Log.Asset.PrintDebug("[ScriptableAssetManifest] Loaded Tags metadata");
			m_tagsMetadata = tagsMetadata;
		}
		else
		{
			Error.AddDevFatal("[ScriptableAssetManifest] Failed to load tags metadata '{0}' in asset bundle '{1}'", TAGS_METADATA_ASSET_PATH, baseAssetBundle.name);
		}
	}

	public string[] GetAllAssetBundleNames(Locale locale)
	{
		IEnumerable<string> allBundles = m_assetsCatalog.GetAllAssetBundleNames();
		if (locale == Locale.UNKNOWN)
		{
			return allBundles.ToArray();
		}
		string localeVariantTag = AssetVariantTags.GetLocaleVariantTagForLocale(locale).ToString();
		char[] loc = new char[4];
		List<string> selectedBundleNames = new List<string>();
		foreach (string bundleName in allBundles)
		{
			int index = bundleName.IndexOf('-');
			loc[0] = bundleName[index - 4];
			loc[1] = bundleName[index - 3];
			int temp = bundleName[index - 2];
			loc[2] = ((temp > 96) ? ((char)(temp - 32)) : ((char)temp));
			temp = bundleName[index - 1];
			loc[3] = ((temp > 96) ? ((char)(temp - 32)) : ((char)temp));
			string locStr = new string(loc);
			if (!Localization.IsValidLocaleName(locStr) || locStr.Equals(localeVariantTag))
			{
				selectedBundleNames.Add(bundleName);
			}
		}
		return selectedBundleNames.ToArray();
	}

	public List<string> GetAssetBundleNamesForTags(string[] tags)
	{
		string[] allAssetBundleNames = GetAllAssetBundleNames(Localization.GetLocale());
		List<string> bundleNamesForTags = new List<string>();
		string[] array = allAssetBundleNames;
		foreach (string bundle in array)
		{
			List<string> bundleTags = new List<string>();
			GetTagsFromAssetBundle(bundle, bundleTags);
			if (bundleTags.All(((IEnumerable<string>)tags).Contains<string>))
			{
				bundleNamesForTags.Add(bundle);
			}
		}
		return bundleNamesForTags;
	}

	public bool TryGetDirectBundleFromGuid(string guid, out string assetBundleName)
	{
		return m_assetsCatalog.TryGetAssetLocationFromGuid(guid, out assetBundleName);
	}

	public void GetTagsFromAssetBundle(string assetBundleName, List<string> tagList)
	{
		m_tagsMetadata.GetTagsFromAssetBundle(assetBundleName, tagList);
	}

	public List<string> GetAllTags(string tagGroup, bool excludeOverridenTag)
	{
		return m_tagsMetadata.GetAllTags(tagGroup, excludeOverridenTag);
	}

	public bool TryResolveAsset(string guid, out string resolvedGuid, out string resolvedBundle, AssetVariantTags.Locale locale = AssetVariantTags.Locale.enUS, AssetVariantTags.Quality quality = AssetVariantTags.Quality.Normal, AssetVariantTags.Platform platform = AssetVariantTags.Platform.Any, AssetVariantTags.Region region = AssetVariantTags.Region.US)
	{
		return m_assetsCatalog.TryResolveAsset(guid, out resolvedGuid, out resolvedBundle, locale, quality, platform, region);
	}

	public string[] GetTagGroups()
	{
		return m_tagsMetadata.GetTagGroups();
	}

	public void GetTagsInTagGroup(string tagGroup, ref List<string> tags)
	{
		m_tagsMetadata.GetTagsInTagGroup(tagGroup, ref tags);
	}

	public string ConvertToOverrideTag(string tag, string tagGroup)
	{
		return m_tagsMetadata.ConvertToOverrideTag(tag, tagGroup);
	}

	public string GetTagGroupForTag(string tag)
	{
		return m_tagsMetadata.GetTagGroupForTag(tag);
	}

	public void ReadLocaleCatalogs()
	{
		m_assetsCatalog.LoadLocaleCatalogs();
	}

	public uint GetBundleSize(string bundleName)
	{
		return m_assetsCatalog.GetAssetBundleSize(bundleName);
	}

	public static ScriptableAssetManifest CreateEmpty()
	{
		return new ScriptableAssetManifest
		{
			m_assetsCatalog = new CompositeAssetCatalog(),
			m_tagsMetadata = ScriptableObject.CreateInstance<ScriptableAssetTagsMetadata>()
		};
	}
}
