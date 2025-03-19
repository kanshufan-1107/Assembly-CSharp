using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class HearthstoneFactionBanner : MonoBehaviour
{
	public GameObject m_factionIconRef;

	public GameObject m_factionBannerRef;

	public GameObject m_shadowRef;

	protected AssetHandle<Material> m_factionMaterial;

	public void SetFactionType(TAG_PREMIUM premium, CardColorSwitcher.FactionColorType faction)
	{
		AssetReference assetReference = CardColorSwitcher.Get().GetMaterialIcon(premium, faction);
		if (assetReference != null && m_factionIconRef != null)
		{
			AssetLoader.Get().LoadAsset(ref m_factionMaterial, assetReference);
			m_factionIconRef.GetComponent<Renderer>().SetMaterial(m_factionMaterial);
		}
		assetReference = CardColorSwitcher.Get().GetMaterialBanner(faction);
		if (assetReference != null && m_factionBannerRef != null)
		{
			AssetLoader.Get().LoadAsset(ref m_factionMaterial, assetReference);
			m_factionBannerRef.GetComponent<Renderer>().SetMaterial(m_factionMaterial);
		}
	}
}
