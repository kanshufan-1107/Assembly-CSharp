using System;

[Serializable]
public class ScriptableAssetVariantCatalog : ScriptableAssetCatalog<VariantAssetCatalogItem>
{
	public bool TryAddVariant(string variantGuid, string variantBundle, uint bundleSize, string baseGuid)
	{
		if (TryAddAsset(variantGuid, variantBundle, bundleSize))
		{
			m_assets[m_assets.Count - 1].baseGuid = baseGuid;
			return true;
		}
		return false;
	}
}
