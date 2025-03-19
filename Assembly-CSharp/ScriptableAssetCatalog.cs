using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScriptableAssetCatalog<T> : ScriptableObject where T : BaseAssetCatalogItem, new()
{
	[SerializeField]
	public int m_TotalAssets = 230000;

	[SerializeField]
	public List<T> m_assets = new List<T>();

	[SerializeField]
	public List<string> m_bundleNames = new List<string>();

	[SerializeField]
	public List<uint> m_bundleSizes = new List<uint>();

	public bool TryAddAsset(string guid, string bundleName, uint bundleSize)
	{
		m_assets.Add(new T
		{
			guid = guid,
			bundleId = (string.IsNullOrEmpty(bundleName) ? (-1) : GetOrAssignBundleId(bundleName, bundleSize))
		});
		return true;
	}

	protected int GetOrAssignBundleId(string bundleName, uint bundleSize)
	{
		int index = m_bundleNames.IndexOf(bundleName);
		if (index >= 0)
		{
			return index;
		}
		m_bundleNames.Add(bundleName);
		m_bundleSizes.Add(bundleSize);
		return m_bundleNames.Count - 1;
	}
}
[Serializable]
public class ScriptableAssetCatalog : ScriptableAssetCatalog<BaseAssetCatalogItem>
{
}
