using System;
using System.Collections.Generic;
using Hearthstone.Util;
using UnityEngine;

namespace Hearthstone.Core;

public class CompositeAssetCatalog
{
	private class AssetInfo
	{
		public string Guid;

		public int BundleId = -1;

		public VariantInfo VariantInfo;
	}

	private class VariantInfo
	{
		public AssetInfo Asset;

		public VariantInfo BaseAssetVariantInfo;

		public Dictionary<AssetVariantTags.Platform, VariantInfo> PlatformVariants;

		public Dictionary<AssetVariantTags.Locale, VariantInfo> LocaleVariants;

		public Dictionary<AssetVariantTags.Quality, VariantInfo> QualityVariants;

		public Dictionary<AssetVariantTags.Region, VariantInfo> RegionVariants;
	}

	private readonly List<string> m_assetBundlesName = new List<string>();

	private readonly List<uint> m_assetBundlesSize = new List<uint>();

	private Dictionary<string, AssetInfo> m_guidToAsset = new Dictionary<string, AssetInfo>();

	private readonly List<AssetVariantTags.Locale> m_loadedLocale = new List<AssetVariantTags.Locale>();

	private readonly List<AssetVariantTags.Region> m_loadedRegion = new List<AssetVariantTags.Region>();

	public IEnumerable<string> GetAllAssetBundleNames()
	{
		return m_assetBundlesName;
	}

	public bool TryGetAssetLocationFromGuid(string guid, out string assetBundleName)
	{
		if (guid != null && m_guidToAsset.TryGetValue(guid, out var asset))
		{
			assetBundleName = GetBundleName(asset.BundleId);
			return true;
		}
		assetBundleName = null;
		return false;
	}

	public bool TryResolveAsset(string guid, out string assetGuid, out string assetBundleName, AssetVariantTags.Locale locale, AssetVariantTags.Quality quality, AssetVariantTags.Platform platform, AssetVariantTags.Region region)
	{
		if (!TryFindBaseAsset(guid, out var resolvedAssetGuid, out var resolvedAssetBundle))
		{
			assetBundleName = null;
			assetGuid = null;
			return false;
		}
		if (TryGetQualityVariantLocationFromBaseGuid(resolvedAssetGuid, quality, out var variantAssetBundle, out var variantAssetGuid))
		{
			resolvedAssetGuid = variantAssetGuid;
			resolvedAssetBundle = variantAssetBundle;
		}
		if (TryGetPlatformVariantLocationFromBaseGuid(resolvedAssetGuid, platform, out variantAssetBundle, out variantAssetGuid))
		{
			resolvedAssetGuid = variantAssetGuid;
			resolvedAssetBundle = variantAssetBundle;
		}
		if (TryGetLocaleVariantLocationFromBaseGuid(resolvedAssetGuid, locale, out variantAssetBundle, out variantAssetGuid))
		{
			resolvedAssetGuid = variantAssetGuid;
			resolvedAssetBundle = variantAssetBundle;
		}
		if (TryGetRegionVariantLocationFromBaseGuid(resolvedAssetGuid, region, out variantAssetBundle, out variantAssetGuid))
		{
			resolvedAssetGuid = variantAssetGuid;
			resolvedAssetBundle = variantAssetBundle;
		}
		assetBundleName = resolvedAssetBundle;
		assetGuid = resolvedAssetGuid;
		return true;
	}

	public bool TryFindBaseAsset(string searchedGuid, out string baseGuid, out string baseBundle)
	{
		baseGuid = null;
		baseBundle = null;
		string nextSearchedGuid = searchedGuid;
		for (int i = 0; i < 5; i++)
		{
			if (m_guidToAsset.TryGetValue(nextSearchedGuid, out var foundAsset))
			{
				if (foundAsset.VariantInfo != null && foundAsset.VariantInfo.BaseAssetVariantInfo != null)
				{
					nextSearchedGuid = foundAsset.VariantInfo.BaseAssetVariantInfo.Asset.Guid;
					continue;
				}
				baseGuid = foundAsset.Guid;
				baseBundle = GetBundleName(foundAsset.BundleId);
				return true;
			}
			return false;
		}
		Error.AddDevFatal("Too many iterations looking for base asset of guid {0}. Probable base asset cycle", searchedGuid);
		return false;
	}

	public bool TryGetRegionVariantLocationFromBaseGuid(string baseAssetGuid, AssetVariantTags.Region region, out string variantBundle, out string variantGuid)
	{
		return TryGetVariantLocationFromBaseGuid(baseAssetGuid, region, GetRegionVariants, out variantBundle, out variantGuid);
	}

	public bool TryGetLocaleVariantLocationFromBaseGuid(string baseAssetGuid, AssetVariantTags.Locale locale, out string variantBundle, out string variantGuid)
	{
		return TryGetVariantLocationFromBaseGuid(baseAssetGuid, locale, GetLocaleVariants, out variantBundle, out variantGuid);
	}

	public bool TryGetPlatformVariantLocationFromBaseGuid(string baseAssetGuid, AssetVariantTags.Platform platform, out string variantBundle, out string variantGuid)
	{
		return TryGetVariantLocationFromBaseGuid(baseAssetGuid, platform, GetPlatformVariants, out variantBundle, out variantGuid);
	}

	public bool TryGetQualityVariantLocationFromBaseGuid(string baseAssetGuid, AssetVariantTags.Quality quality, out string variantBundle, out string variantGuid)
	{
		return TryGetVariantLocationFromBaseGuid(baseAssetGuid, quality, GetQualityVariants, out variantBundle, out variantGuid);
	}

	public uint GetAssetBundleSize(string bundleName)
	{
		int bundleId = m_assetBundlesName.IndexOf(bundleName);
		return GetBundleSize(bundleId);
	}

	private static Dictionary<AssetVariantTags.Region, VariantInfo> GetRegionVariants(VariantInfo variantInfo)
	{
		return variantInfo?.RegionVariants;
	}

	private static Dictionary<AssetVariantTags.Locale, VariantInfo> GetLocaleVariants(VariantInfo variantInfo)
	{
		return variantInfo?.LocaleVariants;
	}

	private static Dictionary<AssetVariantTags.Platform, VariantInfo> GetPlatformVariants(VariantInfo variantInfo)
	{
		return variantInfo?.PlatformVariants;
	}

	private static Dictionary<AssetVariantTags.Quality, VariantInfo> GetQualityVariants(VariantInfo variantInfo)
	{
		return variantInfo?.QualityVariants;
	}

	private string GetBundleName(int bundleId)
	{
		if (bundleId >= 0 && bundleId < m_assetBundlesName.Count)
		{
			return m_assetBundlesName[bundleId];
		}
		return "";
	}

	private uint GetBundleSize(int bundleId)
	{
		if (bundleId >= 0 && bundleId < m_assetBundlesSize.Count)
		{
			return m_assetBundlesSize[bundleId];
		}
		return 0u;
	}

	private bool TryGetVariantLocationFromBaseGuid<T>(string baseAssetGuid, T variantKey, Func<VariantInfo, Dictionary<T, VariantInfo>> variantsGetter, out string variantBundle, out string variantGuid)
	{
		if (m_guidToAsset.TryGetValue(baseAssetGuid, out var baseAsset))
		{
			Dictionary<T, VariantInfo> variants = variantsGetter(baseAsset.VariantInfo);
			if (variants != null && variants.TryGetValue(variantKey, out var variantInfo))
			{
				variantGuid = variantInfo.Asset.Guid;
				variantBundle = GetBundleName(variantInfo.Asset.BundleId);
				return true;
			}
		}
		variantGuid = null;
		variantBundle = null;
		return false;
	}

	public void LoadBaseCatalog(AssetBundle baseAssetBundle)
	{
		string catalogPath = BaseCatalogAssetPath();
		ScriptableAssetCatalog catalog = baseAssetBundle.LoadAsset<ScriptableAssetCatalog>(catalogPath);
		if (catalog != null)
		{
			Log.Asset.PrintDebug("Loaded base catalog {0}", catalogPath);
			m_guidToAsset = new Dictionary<string, AssetInfo>(catalog.m_TotalAssets);
			AddAssetsFromCatalog(catalog.m_assets, catalog.m_bundleNames, catalog.m_bundleSizes);
		}
		else
		{
			Error.AddDevFatal("Failed to load base catalog '{0}' in bundle '{1}'", catalogPath, baseAssetBundle.name);
		}
	}

	public void LoadQualityCatalogs(AssetBundle baseAssetBundle)
	{
		foreach (AssetVariantTags.Quality quality in Enum.GetValues(typeof(AssetVariantTags.Quality)))
		{
			string catalogPath = QualityCatalogAssetPath(quality);
			ScriptableAssetVariantCatalog catalog = baseAssetBundle.LoadAsset<ScriptableAssetVariantCatalog>(catalogPath);
			if (catalog != null)
			{
				Log.Asset.PrintDebug("Loaded quality catalog {0}", catalogPath);
				AddAssetsFromCatalog(catalog.m_assets, catalog.m_bundleNames, catalog.m_bundleSizes);
				AddVariantsFromCatalog(catalog.m_assets, quality, LinkQualityVariant);
			}
		}
	}

	public void LoadPlatformCatalogs(AssetBundle baseAssetBundle)
	{
		foreach (AssetVariantTags.Platform platform in Enum.GetValues(typeof(AssetVariantTags.Platform)))
		{
			string catalogPath = PlatformCatalogAssetPath(platform);
			ScriptableAssetVariantCatalog catalog = baseAssetBundle.LoadAsset<ScriptableAssetVariantCatalog>(catalogPath);
			if (catalog != null)
			{
				Log.Asset.PrintDebug("Loaded platform catalog {0}", catalogPath);
				AddAssetsFromCatalog(catalog.m_assets, catalog.m_bundleNames, catalog.m_bundleSizes);
				AddVariantsFromCatalog(catalog.m_assets, platform, LinkPlatformVariant);
			}
		}
	}

	public void LoadRegionCatalogs()
	{
		AssetVariantTags.Region region = AssetVariantTags.GetRegionVariantTagForRegion(RegionUtils.CurrentRegion);
		if (m_loadedRegion.Contains(region))
		{
			Log.Asset.PrintInfo("Skip to load asset catalog which is already loaded: {0}", region);
			return;
		}
		string catalogBundleName = RegionCatalogBundleName(region);
		string bundlePath = AssetBundleInfo.GetAssetBundlePath(catalogBundleName);
		if (AssetBundleInfo.Exists(bundlePath))
		{
			AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
			if (bundle != null)
			{
				string catalogPath = RegionCatalogAssetPath(region);
				ScriptableAssetVariantCatalog catalog = bundle.LoadAsset<ScriptableAssetVariantCatalog>(catalogPath);
				if (catalog != null)
				{
					Log.Asset.PrintDebug("Loaded region catalog {0}", catalogPath);
					AddAssetsFromCatalog(catalog.m_assets, catalog.m_bundleNames, catalog.m_bundleSizes);
					AddVariantsFromCatalog(catalog.m_assets, region, LinkRegionVariant);
				}
				else
				{
					Error.AddDevFatal("Failed to load region catalog '{0}' in asset bundle '{1}'", catalogPath, bundlePath);
				}
				bundle.Unload(unloadAllLoadedObjects: false);
				m_loadedRegion.Add(region);
			}
			else
			{
				Error.AddDevFatal("Failed to load region bundle at {0}", bundlePath);
			}
		}
		else
		{
			Log.Asset.PrintWarning("Region catalog bundle {0} not found", catalogBundleName);
		}
	}

	public void LoadLocaleCatalogs()
	{
		if (Localization.GetLocale() == Locale.enUS)
		{
			return;
		}
		AssetVariantTags.Locale locale = AssetVariantTags.GetLocaleVariantTagForLocale(Localization.GetLocale());
		if (m_loadedLocale.Contains(locale))
		{
			Log.Asset.PrintInfo("Skip to load asset catalog which is already loaded: {0}", locale);
			return;
		}
		string catalogBundleName = LocaleCatalogBundleName(locale);
		string bundlePath = AssetBundleInfo.GetAssetBundlePath(catalogBundleName);
		if (AssetBundleInfo.Exists(bundlePath))
		{
			AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
			if (bundle != null)
			{
				string catalogPath = LocaleCatalogAssetPath(locale);
				ScriptableAssetVariantCatalog catalog = bundle.LoadAsset<ScriptableAssetVariantCatalog>(catalogPath);
				if (catalog != null)
				{
					Log.Asset.PrintDebug("Loaded locale catalog {0}", catalogPath);
					AddAssetsFromCatalog(catalog.m_assets, catalog.m_bundleNames, catalog.m_bundleSizes);
					AddVariantsFromCatalog(catalog.m_assets, locale, LinkLocaleVariant);
				}
				else
				{
					Error.AddDevFatal("Failed to load locale catalog '{0}' in asset bundle '{1}'", catalogPath, bundlePath);
				}
				bundle.Unload(unloadAllLoadedObjects: false);
				m_loadedLocale.Add(locale);
			}
			else
			{
				Error.AddDevFatal("Failed to load catalog bundle at {0}", bundlePath);
			}
		}
		else
		{
			Log.Asset.PrintWarning("Locale catalog bundle {0} not found", catalogBundleName);
		}
	}

	private void AddAssetsFromCatalog<T>(List<T> assets, List<string> bundleNames, List<uint> bundleSizes) where T : BaseAssetCatalogItem
	{
		int assetsCount = assets.Count;
		int bundleCount = bundleNames.Count;
		for (int i = 0; i < assetsCount; i++)
		{
			T asset = assets[i];
			if (asset.bundleId >= 0 && asset.bundleId < bundleCount)
			{
				uint bundleSize = 0u;
				if (asset.bundleId < bundleSizes.Count)
				{
					bundleSize = bundleSizes[asset.bundleId];
				}
				else
				{
					Error.AddDevFatal("Bundle id {0} out of bounds for {1}. Bundle size not found.", asset.bundleId, asset.guid);
				}
				TryAddOrUpdateAsset(asset.guid, bundleNames[asset.bundleId], bundleSize, out var _);
			}
			else
			{
				Error.AddDevFatal("Bundle id {0} out of bounds for {1}", asset.bundleId, asset.guid);
			}
		}
	}

	private void AddVariantsFromCatalog<T>(List<VariantAssetCatalogItem> variants, T variantKey, Action<VariantInfo, VariantInfo, T> linkAction)
	{
		for (int i = 0; i < variants.Count; i++)
		{
			VariantAssetCatalogItem variant = variants[i];
			TryAddVariant(variant.baseGuid, variant.guid, variantKey, linkAction);
		}
	}

	private bool TryAddVariant<T>(string baseGuid, string variantGuid, T variantKey, Action<VariantInfo, VariantInfo, T> linkAction)
	{
		if (!TryAddOrUpdateAsset(baseGuid, out var baseAsset) || !TryAddOrUpdateAsset(variantGuid, out var variantAsset))
		{
			return false;
		}
		VariantInfo baseInfo = GetOrCreateVariantInfo(baseAsset);
		VariantInfo variantInfo = GetOrCreateVariantInfo(variantAsset);
		variantInfo.BaseAssetVariantInfo = baseInfo;
		linkAction(baseInfo, variantInfo, variantKey);
		return true;
	}

	private void LinkRegionVariant(VariantInfo baseInfo, VariantInfo variant, AssetVariantTags.Region region)
	{
		try
		{
			if (baseInfo.RegionVariants == null)
			{
				baseInfo.RegionVariants = new Dictionary<AssetVariantTags.Region, VariantInfo>();
			}
			baseInfo.RegionVariants.Add(region, variant);
		}
		catch (Exception ex)
		{
			Log.Asset.PrintError("Failed to run LinkRegionVariant: " + ex);
		}
	}

	private void LinkLocaleVariant(VariantInfo baseInfo, VariantInfo variant, AssetVariantTags.Locale locale)
	{
		try
		{
			if (baseInfo.LocaleVariants == null)
			{
				baseInfo.LocaleVariants = new Dictionary<AssetVariantTags.Locale, VariantInfo>();
			}
			baseInfo.LocaleVariants.Add(locale, variant);
		}
		catch (Exception ex)
		{
			Log.Asset.PrintError("Failed to run LinkLocaleVariant: " + ex);
		}
	}

	private void LinkQualityVariant(VariantInfo baseInfo, VariantInfo variant, AssetVariantTags.Quality quality)
	{
		if (baseInfo.QualityVariants == null)
		{
			baseInfo.QualityVariants = new Dictionary<AssetVariantTags.Quality, VariantInfo>();
		}
		baseInfo.QualityVariants.TryAdd(quality, variant);
	}

	private void LinkPlatformVariant(VariantInfo baseInfo, VariantInfo variant, AssetVariantTags.Platform platform)
	{
		if (baseInfo.PlatformVariants == null)
		{
			baseInfo.PlatformVariants = new Dictionary<AssetVariantTags.Platform, VariantInfo>();
		}
		baseInfo.PlatformVariants.TryAdd(platform, variant);
	}

	private VariantInfo GetOrCreateVariantInfo(AssetInfo asset)
	{
		if (asset.VariantInfo == null)
		{
			(asset.VariantInfo = new VariantInfo()).Asset = asset;
		}
		return asset.VariantInfo;
	}

	private bool TryAddOrUpdateAsset(string guid, string bundleName, uint bundleSize, out AssetInfo updatedAsset)
	{
		TryAddOrUpdateAsset(guid, out updatedAsset);
		if (!string.IsNullOrEmpty(bundleName))
		{
			updatedAsset.BundleId = GetOrAssignBundleId(bundleName, bundleSize);
		}
		return true;
	}

	private bool TryAddOrUpdateAsset(string guid, out AssetInfo updatedAsset)
	{
		if (string.IsNullOrEmpty(guid))
		{
			Error.AddDevFatal("AddOrUpdateAsset: guid is required");
			updatedAsset = null;
			return false;
		}
		if (!m_guidToAsset.TryGetValue(guid, out updatedAsset))
		{
			updatedAsset = new AssetInfo
			{
				Guid = guid
			};
			m_guidToAsset[guid] = updatedAsset;
		}
		return true;
	}

	private int GetOrAssignBundleId(string bundleName, uint bundleSize)
	{
		int bundleId = m_assetBundlesName.IndexOf(bundleName);
		if (bundleId >= 0)
		{
			return bundleId;
		}
		m_assetBundlesName.Add(bundleName);
		m_assetBundlesSize.Add(bundleSize);
		return m_assetBundlesName.Count - 1;
	}

	private static string RegionCatalogBundleName(AssetVariantTags.Region region)
	{
		return $"asset_manifest_{region.ToString().ToLower()}.unity3d";
	}

	private static string LocaleCatalogBundleName(AssetVariantTags.Locale locale)
	{
		return $"asset_manifest_{locale.ToString().ToLower()}.unity3d";
	}

	private static string BaseCatalogAssetPath()
	{
		return "Assets/AssetManifest/base_assets_catalog.asset";
	}

	private static string QualityCatalogAssetPath(AssetVariantTags.Quality quality)
	{
		return $"Assets/AssetManifest/asset_catalog_quality_{quality.ToString().ToLower()}.asset";
	}

	private static string PlatformCatalogAssetPath(AssetVariantTags.Platform platform)
	{
		return $"Assets/AssetManifest/asset_catalog_platform_{platform.ToString().ToLower()}.asset";
	}

	private static string LocaleCatalogAssetPath(AssetVariantTags.Locale locale)
	{
		return $"Assets/AssetManifest/asset_catalog_locale_{locale.ToString().ToLower()}.asset";
	}

	private static string RegionCatalogAssetPath(AssetVariantTags.Region region)
	{
		return $"Assets/AssetManifest/asset_catalog_region_{region.ToString().ToLower()}.asset";
	}
}
