using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using UnityEngine;
using UnityEngine.U2D;

public class AliasedAssetResolver : IAliasedAssetResolver, IService
{
	private readonly AssetBundlesAliasedAssetResolver m_assetBundleResolver = new AssetBundlesAliasedAssetResolver();

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(IAssetLoader) };
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		if (AssetLoaderPrefs.AssetLoadingMethod == AssetLoaderPrefs.ASSET_LOADING_METHOD.ASSET_BUNDLES)
		{
			m_assetBundleResolver.LoadFromBundle();
		}
		yield break;
	}

	public void Shutdown()
	{
		m_assetBundleResolver.Shutdown();
	}

	public AssetReference GetCardDefAssetRefFromCardId(string cardId)
	{
		AssetReference cardDefAssetRefFromCardId = m_assetBundleResolver.GetCardDefAssetRefFromCardId(cardId);
		if (cardDefAssetRefFromCardId == null)
		{
			SendMissingAssetTelemetry(typeof(CardDef), "cardId", cardId, "prefab");
		}
		return cardDefAssetRefFromCardId;
	}

	public AssetReference GetSpriteAtlasAssetRefFromTag(string atlasTag)
	{
		AssetReference spriteAtlasAssetRefFromTag = m_assetBundleResolver.GetSpriteAtlasAssetRefFromTag(atlasTag);
		if (spriteAtlasAssetRefFromTag == null)
		{
			SendMissingAssetTelemetry(typeof(SpriteAtlas), "atlasTag", atlasTag, "spriteatlas");
		}
		return spriteAtlasAssetRefFromTag;
	}

	private static void SendMissingAssetTelemetry(Type assetType, string idLabel, string id, string fileExtension, string filePath = "")
	{
		if (Application.isEditor)
		{
			Log.Telemetry.Print("Missing " + assetType.Name + " in editor - not sending missing asset telemetry for " + idLabel + "=" + id + ", extension=" + fileExtension + ", filepath=" + (string.IsNullOrEmpty(filePath) ? "unknown" : filePath));
		}
		else
		{
			TelemetryManager.Client().SendAssetNotFound(assetType.Name, string.Empty, filePath, id + "." + fileExtension);
		}
	}
}
