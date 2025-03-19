using Assets;

public class CardTextBuilder
{
	protected bool m_useEntityForTextInPlay;

	protected bool m_useEntityForTextInHand;

	private static CardTextBuilder m_fallbackCardTextBuilder;

	public CardTextBuilder()
	{
		m_useEntityForTextInPlay = false;
	}

	public static string GetRawCardTextInHand(string cardId)
	{
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(cardId);
		if (record != null && record.TextInHand != null)
		{
			return record.TextInHand;
		}
		return string.Empty;
	}

	public static string GetRawCardName(EntityDef entityDef)
	{
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(entityDef.GetCardId());
		if (record != null && record.Name != null)
		{
			return record.Name.GetString() ?? entityDef.GetDebugName();
		}
		return entityDef.GetDebugName();
	}

	public static string GetDefaultCardTextInHand(EntityDef entityDef)
	{
		return TextUtils.TransformCardText(GetRawCardTextInHand(entityDef.GetCardId()));
	}

	public static CardTextBuilder GetFallbackCardTextBuilder()
	{
		if (m_fallbackCardTextBuilder == null)
		{
			m_fallbackCardTextBuilder = CardTextBuilderFactory.Create(Assets.Card.CardTextBuilderType.DEFAULT);
		}
		return m_fallbackCardTextBuilder;
	}

	public static string GetDefaultCardName(EntityDef entityDef)
	{
		if (GameState.Get() != null && GameState.Get().IsMulliganPhase() && CollectionManager.Get().IsBattlegroundsHeroCard(entityDef.GetCardId()))
		{
			int dbId = GameUtils.TranslateCardIdToDbId(entityDef.GetCardId());
			if (CollectionManager.Get().GetBattlegroundsHeroSkinIdForSkinCardId(dbId, out var associatedSkinId) && CollectionManager.Get().GetBattlegroundsBaseCardIdForHeroSkinId(associatedSkinId, out var associatedBaseHeroCardId))
			{
				CardDbfRecord record = GameDbf.Card.GetRecord(associatedBaseHeroCardId);
				if (record != null && record.Name != null && !string.IsNullOrEmpty(record.Name.GetString()))
				{
					return TextUtils.TransformCardText(record.Name.GetString());
				}
			}
		}
		return TextUtils.TransformCardText(GetRawCardName(entityDef));
	}

	public virtual string BuildCardName(Entity entity)
	{
		return TextUtils.TryFormat(GetDefaultCardName(entity.GetEntityDef()), entity.GetTag(GAME_TAG.CARD_NAME_DATA_1));
	}

	public virtual string BuildCardName(EntityDef entityDef)
	{
		return TextUtils.TryFormat(GetDefaultCardName(entityDef), entityDef.GetTag(GAME_TAG.CARD_NAME_DATA_1));
	}

	public virtual string BuildCardTextInHand(Entity entity)
	{
		return TextUtils.TransformCardText(entity, GetRawCardTextInHand(entity.GetCardId()));
	}

	public virtual string BuildCardTextInHand(EntityDef entityDef)
	{
		return GetDefaultCardTextInHand(entityDef);
	}

	public virtual bool ContainsBonusDamageToken(Entity entity)
	{
		return TextUtils.HasBonusDamage(GetRawCardTextInHand(entity.GetCardId()));
	}

	public virtual bool ContainsBonusHealingToken(Entity entity)
	{
		return TextUtils.HasBonusHealing(GetRawCardTextInHand(entity.GetCardId()));
	}

	public virtual string BuildCardTextInHistory(Entity entity)
	{
		CardTextHistoryData cardTextHistoryData = entity.GetCardTextHistoryData();
		if (cardTextHistoryData == null)
		{
			Log.All.Print("CardTextBuilder.BuildCardTextInHistory: entity {0} does not have a CardTextHistoryData object.", entity.GetEntityId());
			return "";
		}
		EntityDef entityDef = entity.GetEntityDef();
		string rawText = GetRawCardTextInHand(entity.GetCardId() ?? entityDef.GetCardId());
		return TextUtils.TransformCardText(entity, cardTextHistoryData, rawText);
	}

	public virtual CardTextHistoryData CreateCardTextHistoryData()
	{
		return new CardTextHistoryData();
	}

	public virtual string GetTargetingArrowText(Entity entity)
	{
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(entity.GetCardId());
		if (record == null || record.TargetArrowText == null)
		{
			return string.Empty;
		}
		return TextUtils.TransformCardText(entity, record.TargetArrowText.GetString());
	}

	public virtual void OnTagChange(Card card, TagDelta tagChange)
	{
		switch ((GAME_TAG)tagChange.tag)
		{
		case GAME_TAG.OVERRIDECARDTEXTBUILDER:
			if (card != null && card.GetActor() != null)
			{
				Actor cardActor = card.GetActor();
				if (cardActor.GetEntity() != null && cardActor.GetEntity().GetEntityDef() != null)
				{
					cardActor.GetEntity().GetEntityDef().ClearCardTextBuilder();
				}
				cardActor.UpdatePowersText();
			}
			break;
		case GAME_TAG.CURRENT_SPELLPOWER_BASE:
			if (card != null && card.GetActor() != null)
			{
				card.GetActor().UpdatePowersText();
			}
			break;
		}
	}

	public bool ShouldUseEntityForTextInPlay()
	{
		return m_useEntityForTextInPlay;
	}

	public bool ShouldUseEntityForTextInHand()
	{
		return m_useEntityForTextInPlay;
	}
}
