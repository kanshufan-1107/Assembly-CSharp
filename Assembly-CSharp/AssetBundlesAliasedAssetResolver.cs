using Hearthstone.Core;
using UnityEngine;

public class AssetBundlesAliasedAssetResolver
{
	private ScriptableAssetMap m_cardsMap;

	private ScriptableAssetMap m_spriteAtlasMap;

	public void Shutdown()
	{
		m_cardsMap = null;
	}

	public AssetReference GetCardDefAssetRefFromCardId(string cardId)
	{
		InitCardsMapIfNeeded();
		return Resolve(cardId, m_cardsMap);
	}

	public AssetReference GetSpriteAtlasAssetRefFromTag(string atlasTag)
	{
		InitSpriteAtlasMapIfNeeded();
		return Resolve(atlasTag, m_spriteAtlasMap);
	}

	private static AssetReference Resolve(string alias, ScriptableAssetMap assetMap)
	{
		if (assetMap == null || assetMap.map == null)
		{
			Log.Asset.PrintError("[AssetBundlesAliasedAssetResolver] Cannot resolve {0}. Missing map", alias);
			return null;
		}
		if (assetMap.map.TryGetValue(alias, out var refString))
		{
			return AssetReference.CreateFromAssetString(refString);
		}
		Log.Asset.PrintError("[AssetBundlesAliasedAssetResolver] Cannot resolve {0} among {1} entries", alias, assetMap.map.Count);
		return null;
	}

	private void InitCardsMapIfNeeded()
	{
		if (m_cardsMap == null)
		{
			LoadFromBundle();
		}
	}

	private void InitSpriteAtlasMapIfNeeded()
	{
		if (m_spriteAtlasMap == null)
		{
			LoadFromBundle();
		}
	}

	public void LoadFromBundle()
	{
		string bundlePath = AssetBundleInfo.GetAssetBundlePath(ScriptableAssetManifest.MainManifestBundleName);
		if (!AssetBundleInfo.Exists(bundlePath))
		{
			Log.Asset.PrintError("[AssetBundlesAliasedAssetResolver] Cannot find asset bundle for ScriptableAssetMaps '{0}', editor {1}, playing {2}", bundlePath, Application.isEditor, Application.isPlaying);
			return;
		}
		AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
		if (bundle == null)
		{
			Log.Asset.PrintError("[AssetBundlesAliasedAssetResolver] Failed to open manifest bundle at {0}", bundlePath);
			return;
		}
		m_cardsMap = bundle.LoadAsset<ScriptableAssetMap>("Assets/AssetManifest/AssetMaps/cards_map.asset");
		if (m_cardsMap == null)
		{
			Error.AddDevFatal("Failed to load cards map at {0} from {1}", "Assets/AssetManifest/AssetMaps/cards_map.asset", bundlePath);
		}
		m_spriteAtlasMap = bundle.LoadAsset<ScriptableAssetMap>("Assets/AssetManifest/AssetMaps/sprite_atlas_map.asset");
		if (m_spriteAtlasMap == null)
		{
			Error.AddDevFatal("Failed to sprite atlas map at {0} from {1}", "Assets/AssetManifest/AssetMaps/sprite_atlas_map.asset", bundlePath);
		}
		bundle.Unload(unloadAllLoadedObjects: false);
	}
}
