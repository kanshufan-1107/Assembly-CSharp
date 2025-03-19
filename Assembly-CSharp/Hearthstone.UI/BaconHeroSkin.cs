using Blizzard.T5.Core.Utils;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI;

public class BaconHeroSkin : Card
{
	private string m_currentHeroPowerId;

	private string m_currentHeroId;

	protected bool m_HideHeroPower;

	[Overridable]
	public bool HideHeroPower
	{
		get
		{
			return m_HideHeroPower;
		}
		set
		{
			if (m_HideHeroPower != value)
			{
				m_HideHeroPower = value;
				UpdateActor();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_cardActor != null)
		{
			BaconCollectionHeroSkin heroSkinComponent = m_cardActor.GetComponent<BaconCollectionHeroSkin>();
			if (heroSkinComponent != null)
			{
				heroSkinComponent.UnregisterHeroPowerActorLoaded(OnHeroPowerActorLoaded);
			}
		}
	}

	protected override GameObject LoadActorByActorAssetType(EntityDef entityDef, TAG_PREMIUM premium)
	{
		GameObject actor = AssetLoader.Get().InstantiatePrefab("Card_Bacon_Hero_Skin.prefab:7b4af2ee64cfdf24e8ebc8fc817b9761", AssetLoadingOptions.IgnorePrefabPosition);
		if (actor != null)
		{
			GameObjectUtils.FindChild(actor.gameObject, "HeroPower_Parent")?.SetActive(value: false);
		}
		return actor;
	}

	protected override void UpdateActor()
	{
		if (m_cardActor == null)
		{
			return;
		}
		BaconCollectionHeroSkin heroSkinComponent = m_cardActor.GetComponent<BaconCollectionHeroSkin>();
		if (heroSkinComponent != null)
		{
			if (m_HideHeroPower)
			{
				heroSkinComponent.m_heroPowerParent.SetActive(value: false);
			}
			else
			{
				heroSkinComponent.m_heroPowerParent.SetActive(value: true);
				string heroId = m_cardActor.GetEntityDef().GetCardId();
				string heroPowerId = GameUtils.GetHeroPowerCardIdFromHero(m_cardActor.GetEntityDef().GetCardId());
				if (m_currentHeroPowerId != heroPowerId || m_currentHeroId != heroId)
				{
					m_currentHeroPowerId = heroPowerId;
					m_currentHeroId = heroId;
					heroSkinComponent.SetHeroPower(heroPowerId);
					heroSkinComponent.RegisterHeroPowerActorLoaded(OnHeroPowerActorLoaded);
				}
			}
			GetDataModel(122, out var statsPageData);
			GetDataModel(24, out var shopData);
			GetDataModel(229, out var rewardData);
			if (statsPageData != null || shopData != null || rewardData != null)
			{
				heroSkinComponent.ShowName = false;
			}
		}
		if (m_useShadow != m_isShowingShadow)
		{
			if (heroSkinComponent != null)
			{
				heroSkinComponent.ShowShadow(m_useShadow);
			}
			else
			{
				m_cardActor.ContactShadow(m_useShadow);
			}
			m_isShowingShadow = m_useShadow;
		}
		SetUpCustomEffect();
	}

	private void OnHeroPowerActorLoaded(Actor heroPowerActor)
	{
		if (heroPowerActor != null)
		{
			ApplyPopupRenderingTo(heroPowerActor.transform);
		}
	}
}
