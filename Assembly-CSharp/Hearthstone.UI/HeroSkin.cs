using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI;

[WidgetBehaviorDescription(Path = "Hearthstone/Hero Skin", UniqueWithinCategory = "asset")]
[AddComponentMenu("")]
public class HeroSkin : Card
{
	[Tooltip("This will show or hide the name in the hero")]
	[SerializeField]
	private bool m_showHeroName;

	[Tooltip("This will show or hide the class icon in the hero")]
	[SerializeField]
	private bool m_showHeroClass;

	[Tooltip("This will show or hide the hero tray when the bound hero card has a custom tray")]
	[SerializeField]
	private bool m_showCustomHeroTray;

	[Tooltip("If this hero card has an associated replacement i.e. Cthun, then show that instead")]
	[SerializeField]
	private bool m_showHeroReplacementCard;

	[SerializeField]
	[Tooltip("Optional: Applies tint to custom hero tray when it is loaded in.")]
	private Color m_customHeroTrayTint = Color.clear;

	[SerializeField]
	private GameObject m_heroTrayAsset;

	private GameObject m_loadedCustomHeroTray;

	[Overridable]
	public bool ShowHeroName
	{
		get
		{
			return m_showHeroName;
		}
		set
		{
			if (value != m_showHeroName)
			{
				m_showHeroName = value;
				UpdateActor();
			}
		}
	}

	[Overridable]
	public bool ShowHeroClass
	{
		get
		{
			return m_showHeroClass;
		}
		set
		{
			if (value != m_showHeroClass)
			{
				m_showHeroClass = value;
				UpdateActor();
			}
		}
	}

	[Overridable]
	public bool ShowCustomHeroTray
	{
		get
		{
			return m_showCustomHeroTray;
		}
		set
		{
			if (value != m_showCustomHeroTray)
			{
				m_showCustomHeroTray = value;
				UpdateActor();
			}
		}
	}

	[Overridable]
	public bool ShowHeroReplacementCard
	{
		get
		{
			return m_showHeroReplacementCard;
		}
		set
		{
			if (value != m_showHeroReplacementCard)
			{
				m_showHeroReplacementCard = value;
				UpdateActor();
			}
		}
	}

	protected override GameObject LoadActorByActorAssetType(EntityDef entityDef, TAG_PREMIUM premium)
	{
		GameObject actor = null;
		switch (m_zone)
		{
		case TAG_ZONE.HAND:
			actor = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHeroSkinOrHandActor(entityDef, premium), AssetLoadingOptions.IgnorePrefabPosition);
			break;
		case TAG_ZONE.PLAY:
			actor = AssetLoader.Get().InstantiatePrefab(ActorNames.GetPlayActorByTags(entityDef, premium), AssetLoadingOptions.IgnorePrefabPosition);
			break;
		default:
			Debug.LogWarningFormat("CustomWidgetBehavior:HeroSkin - Zone {0} not supported.", m_zone);
			break;
		}
		return actor;
	}

	protected override void UpdateActor()
	{
		if (m_cardActor == null)
		{
			return;
		}
		CollectionHeroSkin heroSkinComponent = m_cardActor.GetComponent<CollectionHeroSkin>();
		if (heroSkinComponent != null)
		{
			if (m_showHeroClass)
			{
				EntityDef cardEntityDef = DefLoader.Get().GetEntityDef(m_displayedCardId);
				if (cardEntityDef != null)
				{
					heroSkinComponent.SetClass(cardEntityDef);
				}
			}
			heroSkinComponent.m_classIcon.transform.parent.gameObject.SetActive(m_showHeroClass);
			heroSkinComponent.ShowName = m_showHeroName;
		}
		DefLoader.DisposableCardDef cardDef = DefLoader.Get().GetCardDef(m_displayedCardId);
		if (m_showHeroReplacementCard && cardDef?.CardDef != null && cardDef.CardDef.TryGetComponent<HeroCardSwitcher>(out var heroCardSwitcher))
		{
			DefLoader.DisposableCardDef disposableCardDef = cardDef;
			cardDef = heroCardSwitcher.GetReplacementCardDef();
			m_cardActor.SetCardDef(cardDef);
			disposableCardDef.Dispose();
		}
		bool shouldHideTray = true;
		if (m_showCustomHeroTray && cardDef?.CardDef != null && !string.IsNullOrEmpty(cardDef.CardDef.m_CustomHeroTray))
		{
			shouldHideTray = false;
			if (m_loadedCustomHeroTray == null)
			{
				m_loadedCustomHeroTray = Object.Instantiate(m_heroTrayAsset, base.transform);
			}
			else
			{
				m_loadedCustomHeroTray.SetActive(value: true);
			}
			AssetLoader.Get().LoadAsset<Texture>(cardDef?.CardDef.m_CustomHeroTray, OnHeroTrayTextureLoaded);
		}
		cardDef?.Dispose();
		if (shouldHideTray && m_loadedCustomHeroTray != null)
		{
			m_loadedCustomHeroTray.SetActive(value: false);
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

	private void OnHeroTrayTextureLoaded(AssetReference assetref, AssetHandle<Texture> asset, object callbackdata)
	{
		if (m_loadedCustomHeroTray == null)
		{
			return;
		}
		MeshRenderer render = m_loadedCustomHeroTray.GetComponentInChildren<MeshRenderer>();
		if (render == null)
		{
			Debug.LogError("HeroSkin: Failed to load custom tray texture as no MeshRenderer found.");
			return;
		}
		Material trayMat = render.GetMaterial();
		if (trayMat == null)
		{
			Debug.LogError("HeroSkin: Failed to load custom tray texture as no Material found.");
			return;
		}
		trayMat.mainTexture = asset;
		trayMat.color = m_customHeroTrayTint;
	}
}
