using System;
using UnityEngine;

[Serializable]
public class MaterialReference
{
	[SerializeField]
	private string m_materialRef;

	[SerializeField]
	private string m_mainTextureRef;

	public string MaterialRef => m_materialRef;

	public Material GetMaterial()
	{
		Material material = null;
		if (string.IsNullOrWhiteSpace(m_materialRef))
		{
			Debug.LogWarning($"Material Reference used with no value");
			return null;
		}
		if (AssetLoader.Get() == null)
		{
			return null;
		}
		material = AssetLoader.Get().LoadMaterial(m_materialRef);
		if (material == null)
		{
			return null;
		}
		if (!string.IsNullOrWhiteSpace(m_mainTextureRef))
		{
			if (material.mainTexture == null)
			{
				material.mainTexture = AssetLoader.Get().LoadTexture(m_mainTextureRef);
			}
			if (material.mainTexture == null)
			{
				Debug.LogWarning($"Material Reference attempted to load texture and failed: \"{m_mainTextureRef}\"");
			}
		}
		return material;
	}
}
