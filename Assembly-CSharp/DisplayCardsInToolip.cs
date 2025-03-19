using System.Collections.Generic;
using UnityEngine;

public class DisplayCardsInToolip : MonoBehaviour
{
	public static readonly float APPEARANCE_DELAY = 0.55f;

	public static readonly float APPEARANCE_DURATION = 0.125f;

	public static readonly float CARD_SCALE = 1.3f;

	public static readonly float CARD_SCALE_PHONE_WITH_HERO_BUDDY = 0.94f;

	public static readonly float CARD_CYCLE_DURATION = 3f;

	public static readonly Vector3 s_leftOffset = new Vector3(-3.1f, 0.1f, 0f);

	public static readonly Vector3 s_rightOffset = new Vector3(3.1f, 0.1f, 0f);

	public static readonly Vector3 s_additionalOffsetDuringMulligan = new Vector3(0f, 0.4f, 0f);

	public static readonly Vector3 s_offsetExtraCardLeft = new Vector3(-2.9f, 0f, 0f);

	public static readonly Vector3 s_offsetExtraCardRight = new Vector3(2.9f, 0f, 0f);

	public static readonly Vector3 s_leftOffsetPhone = new Vector3(-2.6f, 0.1f, 0f);

	public static readonly Vector3 s_rightOffsetPhone = new Vector3(2.6f, 0.1f, 0f);

	public static readonly Vector3 s_additionalOffsetDuringMulliganPhone = new Vector3(0f, 0.4f, 0f);

	public static readonly Vector3 s_offsetExtraCardLeftPhone = new Vector3(-2.1f, 0f, 0f);

	public static readonly Vector3 s_offsetExtraCardRightPhone = new Vector3(2.1f, 0f, 0f);

	public static readonly Vector3 s_additionalOffsetColossalLeftPhone = new Vector3(-0.4f, 0f, 0f);

	public static readonly Vector3 s_additionalOffsetColossalRightPhone = new Vector3(0.4f, 0f, 0f);

	private List<Actor> m_cardActors = new List<Actor>();

	private Card m_ownerCard;

	private bool m_forceLeft;

	private bool m_forceRight;

	public void NotifyMousedOver()
	{
		ShowCardsInTooltipAfterDelay();
	}

	public void NotifyMousedOut()
	{
		HideCardsInTooltipActor();
	}

	public void NotifyPickedUp()
	{
		HideCardsInTooltipActor();
	}

	public void SetTooltipOnLeft(bool forceLeft)
	{
		m_forceLeft = forceLeft;
		if (forceLeft)
		{
			m_forceRight = false;
		}
	}

	public void SetTooltipOnRight(bool forceRight)
	{
		m_forceRight = forceRight;
		if (forceRight)
		{
			m_forceLeft = false;
		}
	}

	private void OnDestroy()
	{
		foreach (Actor cardToolip in m_cardActors)
		{
			if (cardToolip != null && cardToolip.gameObject != null)
			{
				cardToolip.Destroy();
			}
		}
		m_cardActors.Clear();
		m_ownerCard = null;
	}

	public void Setup(Card ownerCard)
	{
		if (ownerCard == null || ownerCard.GetEntity() == null)
		{
			Log.Spells.PrintError("DisplayCardsInToolip.Setup(): Invalid card was passed in.");
		}
		else
		{
			m_ownerCard = ownerCard;
		}
	}

	public int GetActorCardCount()
	{
		return m_cardActors.Count;
	}

	public void AddCardsInTooltip(int cardID)
	{
		Entity entity = m_ownerCard.GetEntity();
		foreach (Actor cardActor in m_cardActors)
		{
			EntityDef entityDef = cardActor.GetEntityDef();
			if (entityDef != null && GameUtils.TranslateCardIdToDbId(entityDef.GetCardId()) == cardID)
			{
				return;
			}
		}
		using DefLoader.DisposableFullDef cardDef = DefLoader.Get().GetFullDef(cardID);
		if (cardDef?.EntityDef == null || cardDef?.CardDef == null)
		{
			Log.Spells.PrintError("CardsInTooltip.Setup(): Unable to load def for card ID {0}.", cardID);
			return;
		}
		TAG_PREMIUM ownerPremium = entity.GetPremiumType();
		if (ownerPremium == TAG_PREMIUM.DIAMOND && !cardDef.EntityDef.HasTag(GAME_TAG.HAS_DIAMOND_QUALITY))
		{
			ownerPremium = TAG_PREMIUM.SIGNATURE;
		}
		if (ownerPremium == TAG_PREMIUM.SIGNATURE && !cardDef.EntityDef.HasTag(GAME_TAG.HAS_SIGNATURE_QUALITY))
		{
			ownerPremium = TAG_PREMIUM.GOLDEN;
		}
		GameObject cardsInTooltipActorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(cardDef.EntityDef, ownerPremium), AssetLoadingOptions.IgnorePrefabPosition);
		if (cardsInTooltipActorGO == null)
		{
			Log.Spells.PrintError("AddCardsInTooltip(): Unable to load Hand Actor for entity def {0}.", cardDef.EntityDef);
			return;
		}
		Actor cardsInTooltipActor = cardsInTooltipActorGO.GetComponentInChildren<Actor>();
		cardsInTooltipActor.SetFullDef(cardDef);
		cardsInTooltipActor.SetPremium(ownerPremium);
		cardsInTooltipActor.SetCardBackSideOverride(entity.GetControllerSide());
		cardsInTooltipActor.SetWatermarkCardSetOverride(entity.GetWatermarkCardSetOverride());
		cardsInTooltipActor.UpdateAllComponents();
		LayerUtils.SetLayer(cardsInTooltipActor, GameLayer.Tooltip);
		if (cardsInTooltipActor.UseCoinManaGem())
		{
			cardsInTooltipActor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
		}
		if (cardDef.EntityDef.GetTag(GAME_TAG.BACON_BUDDY) != 0)
		{
			cardsInTooltipActor.ActivateSpellBirthState(SpellType.GHOSTMODE);
		}
		cardsInTooltipActor.Hide();
		cardsInTooltipActor.gameObject.SetActive(value: false);
		m_cardActors.Add(cardsInTooltipActor);
	}

	private int GetCurrentRotatingOffset()
	{
		int numCards = GetActorCardCount();
		if (numCards <= 1)
		{
			return 0;
		}
		float totalCycleDuration = CARD_CYCLE_DURATION * (float)numCards;
		return (int)Mathf.Floor(Time.time % totalCycleDuration / CARD_CYCLE_DURATION);
	}

	private void Update()
	{
		if (iTween.HasName(base.gameObject, "Appearing"))
		{
			return;
		}
		if (GameState.Get().IsMulliganManagerActive() && GameState.Get().GetBooleanGameOption(GameEntityOption.CARDS_IN_TOOLTIP_DONT_CYCLE_DURING_MULLIGAN))
		{
			int offsetIndex = 0;
			{
				foreach (Actor cardTooltip in m_cardActors)
				{
					if (cardTooltip != null && cardTooltip.IsShown() && cardTooltip.gameObject.activeSelf)
					{
						cardTooltip.transform.position = base.gameObject.transform.position + GetDesiredOffset(offsetIndex);
						offsetIndex++;
					}
				}
				return;
			}
		}
		int offsetIndex2 = GetCurrentRotatingOffset();
		for (int currentIndex = 0; currentIndex < m_cardActors.Count; currentIndex++)
		{
			Actor cardTooltip2 = m_cardActors[currentIndex];
			if (currentIndex == offsetIndex2)
			{
				cardTooltip2.Show();
				cardTooltip2.gameObject.SetActive(value: true);
			}
			else
			{
				cardTooltip2.Hide();
				cardTooltip2.gameObject.SetActive(value: false);
			}
			if (cardTooltip2 != null && cardTooltip2.IsShown() && cardTooltip2.gameObject.activeSelf)
			{
				cardTooltip2.transform.position = base.gameObject.transform.position + GetDesiredOffset(offsetIndex2);
			}
		}
	}

	private bool HasRelatedCards()
	{
		Entity ownerEntity = m_ownerCard.GetEntity();
		if (ownerEntity == null)
		{
			return false;
		}
		if (!ownerEntity.HasTag(GAME_TAG.COLOSSAL) && !ownerEntity.HasTag(GAME_TAG.DISPLAY_CARD_ON_MOUSEOVER))
		{
			return ownerEntity.IsHero();
		}
		return true;
	}

	private bool IsColossalLimbOnTheLeft()
	{
		foreach (Actor cardActor in m_cardActors)
		{
			Entity tooltipEnt = cardActor.GetEntity();
			EntityDef tooltipEntDef = cardActor.GetEntityDef();
			if ((tooltipEntDef != null && tooltipEntDef.HasTag(GAME_TAG.COLOSSAL_LIMB_ON_LEFT)) || (tooltipEnt != null && tooltipEnt.HasTag(GAME_TAG.COLOSSAL_LIMB_ON_LEFT)))
			{
				return true;
			}
		}
		return false;
	}

	private bool ShouldCardsGoInwardsOnMobile(ZoneHand handZone)
	{
		if (handZone.HandEnlarged())
		{
			return true;
		}
		if (GameMgr.Get().IsBattlegrounds() && GameState.Get().IsMulliganManagerActive())
		{
			return true;
		}
		return false;
	}

	private bool IsExtraNonColossalCardOnTheLeft()
	{
		int zonePosition = m_ownerCard.GetEntity().GetZonePosition();
		float middleIndex = (float)(m_ownerCard.GetController().GetHandZone().GetCardCount() + 1) / 2f;
		return (float)zonePosition > middleIndex;
	}

	private bool ShouldShowCardOnLeft(ZoneHand handZone)
	{
		if (m_forceLeft)
		{
			return true;
		}
		if (m_forceRight)
		{
			return false;
		}
		if (m_ownerCard == null || m_ownerCard.GetEntity() == null || handZone == null)
		{
			return false;
		}
		if (m_ownerCard.GetEntity().HasTag(GAME_TAG.COLOSSAL))
		{
			return IsColossalLimbOnTheLeft();
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (ShouldCardsGoInwardsOnMobile(handZone))
			{
				return IsExtraNonColossalCardOnTheLeft();
			}
			return true;
		}
		return IsExtraNonColossalCardOnTheLeft();
	}

	private Vector3 GetDesiredOffset(int offsetIndex)
	{
		if (!GameState.Get().IsMulliganManagerActive() || !GameState.Get().GetBooleanGameOption(GameEntityOption.CARDS_IN_TOOLTIP_DONT_CYCLE_DURING_MULLIGAN))
		{
			offsetIndex = 0;
		}
		ZoneHand handZone = m_ownerCard.GetZone() as ZoneHand;
		bool num = HasRelatedCards();
		Vector3 additionalRightOffset = Vector3.zero;
		Vector3 additionalLeftOffset = Vector3.zero;
		if (GameState.Get().IsMulliganManagerActive() && GameState.Get().GetBooleanGameOption(GameEntityOption.CARDS_IN_TOOLTIP_SHIFTED_DURING_MULLIGAN))
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				additionalRightOffset = s_additionalOffsetDuringMulliganPhone + new Vector3(-1f, 0f, 0f);
				additionalLeftOffset = s_additionalOffsetDuringMulliganPhone + new Vector3(1f, 0f, 0f);
			}
			else
			{
				additionalRightOffset = s_additionalOffsetDuringMulligan + new Vector3(-1f, 0f, 0f);
				additionalLeftOffset = s_additionalOffsetDuringMulligan + new Vector3(1f, 0f, 0f);
			}
		}
		if (num)
		{
			if (ShouldShowCardOnLeft(handZone))
			{
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					return s_leftOffsetPhone + additionalLeftOffset + s_additionalOffsetColossalLeftPhone + offsetIndex * s_offsetExtraCardLeftPhone;
				}
				return s_leftOffset + additionalLeftOffset + offsetIndex * s_offsetExtraCardLeft;
			}
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				return s_rightOffsetPhone + additionalRightOffset + s_additionalOffsetColossalRightPhone + offsetIndex * s_offsetExtraCardRightPhone;
			}
			return s_rightOffset + additionalRightOffset + offsetIndex * s_offsetExtraCardRight;
		}
		if (handZone != null && !handZone.ShouldShowCardTooltipOnRight(m_ownerCard))
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				return s_leftOffsetPhone + additionalLeftOffset + offsetIndex * s_offsetExtraCardLeftPhone;
			}
			return s_leftOffset + additionalLeftOffset + offsetIndex * s_offsetExtraCardLeft;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return s_rightOffsetPhone + additionalRightOffset + offsetIndex * s_offsetExtraCardRightPhone;
		}
		return s_rightOffset + additionalRightOffset + offsetIndex * s_offsetExtraCardRight;
	}

	private void OnCardsInToolipAppearUpdate(float newValue)
	{
		int offsetIndex = 0;
		bool hasHeroBuddy = false;
		foreach (Actor cardActor in m_cardActors)
		{
			Entity tooltipEnt = cardActor.GetEntity();
			EntityDef tooltipEntDef = cardActor.GetEntityDef();
			if ((tooltipEntDef != null && tooltipEntDef.GetTag(GAME_TAG.BACON_BUDDY) != 0) || (tooltipEnt != null && tooltipEnt.GetTag(GAME_TAG.BACON_BUDDY) != 0))
			{
				hasHeroBuddy = true;
			}
		}
		float cardScale = CARD_SCALE;
		if ((bool)UniversalInputManager.UsePhoneUI && hasHeroBuddy)
		{
			cardScale = CARD_SCALE_PHONE_WITH_HERO_BUDDY;
		}
		foreach (Actor cardTooltip in m_cardActors)
		{
			if (cardTooltip == null)
			{
				break;
			}
			if (!cardTooltip.gameObject.activeSelf)
			{
				cardTooltip.gameObject.SetActive(value: true);
			}
			if (!cardTooltip.IsShown())
			{
				cardTooltip.Show();
			}
			float newScale = cardScale * newValue;
			cardTooltip.transform.localScale = new Vector3(newScale, newScale, newScale);
			cardTooltip.transform.position = base.gameObject.transform.position + GetDesiredOffset(offsetIndex) * newValue;
			offsetIndex++;
		}
	}

	private void ShowCardsInTooltipAfterDelay()
	{
		foreach (Actor cardTooltip in m_cardActors)
		{
			if (cardTooltip == null)
			{
				return;
			}
			if (!cardTooltip.gameObject.activeSelf)
			{
				cardTooltip.gameObject.SetActive(value: true);
			}
			if ((cardTooltip.GetEntityDef() != null && cardTooltip.GetEntityDef().GetTag(GAME_TAG.BACON_BUDDY) != 0) || (cardTooltip.GetEntity() != null && cardTooltip.GetEntity().GetTag(GAME_TAG.BACON_BUDDY) != 0))
			{
				cardTooltip.ActivateSpellBirthState(SpellType.GHOSTMODE);
			}
			if (cardTooltip.UseTechLevelManaGem())
			{
				Spell techLevelSpell = cardTooltip.GetSpell(SpellType.TECH_LEVEL_MANA_GEM);
				if (techLevelSpell != null)
				{
					techLevelSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("TechLevel").Value = cardTooltip.GetEntityDef().GetTechLevel();
					techLevelSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
			else if (cardTooltip.UseCoinManaGem())
			{
				cardTooltip.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
			}
			cardTooltip.SetUnlit();
		}
		iTween.StopByName(base.gameObject, "Appearing");
		iTween.ValueTo(base.gameObject, iTween.Hash("onupdatetarget", base.gameObject, "onupdate", "OnCardsInToolipAppearUpdate", "time", APPEARANCE_DURATION, "delay", GameState.Get().GetGameEntity().ShouldDelayShowingCardInTooltip() ? APPEARANCE_DELAY : 0f, "to", 1f, "from", 0f, "name", "Appearing"));
	}

	private void HideCardsInTooltipActor()
	{
		OnCardsInToolipAppearUpdate(0f);
		foreach (Actor cardTooltip in m_cardActors)
		{
			if (cardTooltip == null)
			{
				return;
			}
			cardTooltip.Hide();
			cardTooltip.gameObject.SetActive(value: false);
		}
		iTween.StopByName(base.gameObject, "Appearing");
	}
}
