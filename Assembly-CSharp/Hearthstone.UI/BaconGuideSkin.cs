using UnityEngine;

namespace Hearthstone.UI;

public class BaconGuideSkin : Card
{
	protected override GameObject LoadActorByActorAssetType(EntityDef entityDef, TAG_PREMIUM premium)
	{
		return AssetLoader.Get().InstantiatePrefab("Card_Guide_Skin.prefab:cf2cadaa8c6f7244fb9500edb2046c8b", AssetLoadingOptions.IgnorePrefabPosition);
	}

	protected override void UpdateActor()
	{
		if (m_cardActor == null)
		{
			return;
		}
		BaconCollectionGuideSkin guideSkinComponent = m_cardActor.GetComponent<BaconCollectionGuideSkin>();
		if (guideSkinComponent != null)
		{
			guideSkinComponent.ShowName = false;
		}
		if (m_useShadow != m_isShowingShadow)
		{
			if (guideSkinComponent != null)
			{
				guideSkinComponent.ShowShadow(m_useShadow);
			}
			else
			{
				m_cardActor.ContactShadow(m_useShadow);
			}
			m_isShowingShadow = m_useShadow;
		}
		SetUpCustomEffect();
	}
}
