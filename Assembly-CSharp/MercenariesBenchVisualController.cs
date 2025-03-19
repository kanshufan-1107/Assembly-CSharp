using System;
using System.Collections.Generic;
using UnityEngine;

public class MercenariesBenchVisualController : MonoBehaviour
{
	public GameObject m_banner;

	public UberText m_bannerText;

	public GameObject m_cardDisplayCenterBone;

	public float m_cardOffsetWidth;

	public ActorStateType m_assignedActorState = ActorStateType.CARD_IDLE;

	[Header("Full Screen FX")]
	public float m_fullScreenFXTransitionTime;

	public iTween.EaseType m_fullScreenFXEaseType;

	public float m_vignetteAmount;

	public float m_desaturateAmount;

	private readonly List<Actor> m_actors = new List<Actor>();

	private readonly Pool<EnchantmentBanner> m_bannerPool = new Pool<EnchantmentBanner>();

	private ScreenEffectsHandle m_screenEffectsHandle;

	private const float ENCHANTMENT_SCALING_FACTOR = 20f / 33f;

	public void Awake()
	{
		if (m_banner != null)
		{
			m_banner.SetActive(value: false);
		}
		m_bannerPool.SetCreateItemCallback(CreateEnchantmentBanner);
		m_bannerPool.SetDestroyItemCallback(DestroyEnchantmentBanner);
		m_bannerPool.SetExtensionCount(1);
		m_bannerPool.SetMaxReleasedItemCount(6);
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private EnchantmentBanner CreateEnchantmentBanner(int index)
	{
		EnchantmentBanner component = AssetLoader.Get().InstantiatePrefab("EnchantmentBanner.prefab:e7058664cd0b13f4bb45e5b5f0385f34", AssetLoadingOptions.IgnorePrefabPosition).GetComponent<EnchantmentBanner>();
		component.name = string.Format("{0}{1}", "EnchantmentBanner", index);
		component.transform.parent = base.transform;
		return component;
	}

	private void DestroyEnchantmentBanner(EnchantmentBanner panel)
	{
		UnityEngine.Object.Destroy(panel.gameObject);
	}

	public void OnFriendlyBenchMouseOver(Action<string, string> showRegularTooltip)
	{
		ZoneDeck deck = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.FRIENDLY);
		if (deck.GetCardCount() > 0)
		{
			if (deck.m_DeckCover != null)
			{
				LayerUtils.SetLayer(deck.m_DeckCover, GameLayer.Tooltip);
				deck.m_DeckCover.SetDeckCoverHighlightVisibility(isVisible: true);
			}
			foreach (Card card in deck.GetCards())
			{
				LoadCardActor(card);
			}
			LayoutCardActors();
			if (m_banner != null)
			{
				m_banner.SetActive(value: true);
			}
			List<EnchantmentBanner> activeBanners = m_bannerPool.GetActiveList();
			int diff = m_actors.Count - activeBanners.Count;
			if (diff > 0)
			{
				m_bannerPool.AcquireBatch(diff);
			}
			else if (diff < 0)
			{
				m_bannerPool.ReleaseBatch(m_actors.Count, -diff);
			}
			for (int i = 0; i < m_actors.Count; i++)
			{
				Actor actor = m_actors[i];
				Card card2 = actor.GetCard();
				activeBanners[i].UpdateEnchantments(card2, actor, 20f / 33f);
			}
			float fullScreenFXTransitionTime = m_fullScreenFXTransitionTime;
			iTween.EaseType fullScreenFXEaseType = m_fullScreenFXEaseType;
			VignetteParameters? vignette = new VignetteParameters(m_vignetteAmount);
			DesaturateParameters? desaturate = new DesaturateParameters(m_desaturateAmount);
			ScreenEffectParameters screenEffectParameters = new ScreenEffectParameters(ScreenEffectType.VIGNETTE | ScreenEffectType.DESATURATE, ScreenEffectPassLocation.PERSPECTIVE, fullScreenFXTransitionTime, fullScreenFXEaseType, null, vignette, desaturate, null);
			m_screenEffectsHandle.StartEffect(screenEffectParameters);
		}
		else
		{
			string headline = GameStrings.Get("GAMEPLAY_TOOLTIP_LETTUCE_BENCH_HEADLINE");
			ZoneHand hand = ZoneMgr.Get().FindZoneOfType<ZoneHand>(Player.Side.FRIENDLY);
			string description = GameStrings.Format("GAMEPLAY_TOOLTIP_LETTUCE_BENCH_DESCRIPTION", deck.GetCardCount() + hand.GetCardCount());
			showRegularTooltip(headline, description);
		}
	}

	public void OnFriendlyBenchMouseOut()
	{
		if (m_actors.Count <= 0)
		{
			return;
		}
		ZoneDeck deck = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.FRIENDLY);
		if (deck.m_DeckCover != null)
		{
			LayerUtils.SetLayer(deck.m_DeckCover, GameLayer.Default);
			deck.m_DeckCover.SetDeckCoverHighlightVisibility(isVisible: false);
		}
		foreach (Actor actor in m_actors)
		{
			actor.Destroy();
		}
		m_actors.Clear();
		if (m_banner != null)
		{
			m_banner.SetActive(value: false);
		}
		foreach (EnchantmentBanner active in m_bannerPool.GetActiveList())
		{
			active.ResetEnchantments();
		}
		m_screenEffectsHandle.StopEffect();
	}

	private void LayoutCardActors()
	{
		float scalingFactor = BigCard.Get().GetPlatformScalingFactor();
		Vector3 position = m_cardDisplayCenterBone.transform.position;
		float adjustedCardOffsetWidth = m_cardOffsetWidth * scalingFactor;
		position.x -= (float)(m_actors.Count - 1) * adjustedCardOffsetWidth / 2f;
		foreach (Actor actor in m_actors)
		{
			actor.transform.localScale *= scalingFactor;
			float zOffset = actor.GetMeshRenderer().bounds.size.z * (scalingFactor - 1f) / 2f;
			actor.transform.position = new Vector3(position.x, position.y, position.z - zOffset);
			position.x += adjustedCardOffsetWidth;
		}
	}

	private void LoadCardActor(Card card)
	{
		string assetRef = ActorNames.GetBigCardActor(card.GetEntity());
		Actor actor = AssetLoader.Get().InstantiatePrefab(assetRef, AssetLoadingOptions.IgnorePrefabPosition).GetComponent<Actor>();
		m_actors.Add(actor);
		using (DefLoader.DisposableCardDef cardDef = card.ShareDisposableCardDef())
		{
			actor.SetCardDef(cardDef);
		}
		Entity entity = card.GetEntity();
		actor.SetEntity(entity);
		actor.SetPremium(entity.GetPremiumType());
		actor.SetCard(card);
		actor.SetWatermarkCardSetOverride(entity.GetWatermarkCardSetOverride());
		actor.UpdateAllComponents();
		actor.SetActorState(m_assignedActorState);
		BoxCollider boxCollider = actor.GetComponentInChildren<BoxCollider>();
		if (boxCollider != null)
		{
			boxCollider.enabled = false;
		}
		actor.name = "MercBenchCard_" + actor.name;
		actor.transform.parent = base.transform;
		TransformUtil.Identity(actor.transform);
		LayerUtils.SetLayer(actor, GameLayer.Tooltip);
	}
}
