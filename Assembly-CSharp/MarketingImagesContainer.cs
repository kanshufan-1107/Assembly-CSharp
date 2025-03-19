using System.Collections.Generic;
using Hearthstone.MarketingImages;
using UnityEngine;

[CreateAssetMenu(fileName = "MarketingImagesContainer", menuName = "ScriptableObjects/MarketingImagesContainer")]
public class MarketingImagesContainer : ScriptableObject
{
	[SerializeField]
	private List<MarketingImageConfig> m_images = new List<MarketingImageConfig>();

	public IReadOnlyList<MarketingImageConfig> Images => m_images;
}
