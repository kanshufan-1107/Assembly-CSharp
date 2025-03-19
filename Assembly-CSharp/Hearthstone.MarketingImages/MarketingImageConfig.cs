using System;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using UnityEngine;

namespace Hearthstone.MarketingImages;

[Serializable]
public class MarketingImageConfig
{
	[Tooltip("Product this image is intended for")]
	public long ProductId;

	public MarketingImageSlot SlotCompatibility;

	[Tooltip("Should the frame be overlaid? (Untick if the frame is already baked into the image)")]
	public bool AutoFrame = true;

	[AssetReferencePicker(AssetReferenceKind.Texture)]
	[Tooltip("Texture to use (soft reference)")]
	public AssetReference TextureAsset;

	[Tooltip("Normalized offset (used for shifting the image left/right up/down (used only when texture is wider/taller than the slot)")]
	public Vector2 TextureOffset;

	[Tooltip("Comma-separated list of tags that may be used to alter behavior of the image")]
	[SerializeField]
	private string m_tags;

	private HashSet<string> m_tagSet;

	[NonSerialized]
	public AssetHandle<Texture2D> TextureHandle;

	[NonSerialized]
	public string TextureUrl;

	[NonSerialized]
	public Texture2D Texture;

	public HashSet<string> Tags => m_tagSet ?? (m_tagSet = ParseTags(m_tags));

	private static HashSet<string> ParseTags(string rawTags)
	{
		HashSet<string> splitTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		string[] array = (rawTags ?? string.Empty).Split(',');
		foreach (string tag in array)
		{
			if (!string.IsNullOrWhiteSpace(tag))
			{
				splitTags.Add(tag.Trim());
			}
		}
		return splitTags;
	}

	public void DestroyCachedTexture()
	{
		if ((bool)TextureHandle)
		{
			TextureHandle.Dispose();
			TextureHandle = null;
			Texture = null;
		}
		else if ((bool)Texture)
		{
			UnityEngine.Object.Destroy(Texture);
			Texture = null;
		}
	}
}
