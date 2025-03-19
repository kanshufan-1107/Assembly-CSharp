public class MultipleEntityNamesCardTextBuilder : CardTextBuilder
{
	private static readonly GAME_TAG[] m_referencedTags = new GAME_TAG[10]
	{
		GAME_TAG.CARDTEXT_ENTITY_0,
		GAME_TAG.CARDTEXT_ENTITY_1,
		GAME_TAG.CARDTEXT_ENTITY_2,
		GAME_TAG.CARDTEXT_ENTITY_3,
		GAME_TAG.CARDTEXT_ENTITY_4,
		GAME_TAG.CARDTEXT_ENTITY_5,
		GAME_TAG.CARDTEXT_ENTITY_6,
		GAME_TAG.CARDTEXT_ENTITY_7,
		GAME_TAG.CARDTEXT_ENTITY_8,
		GAME_TAG.CARDTEXT_ENTITY_9
	};

	public MultipleEntityNamesCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		return BuildText(entity);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		if (entityDef == null)
		{
			return "";
		}
		string rawText = CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId());
		string[] referencedEntityNames = new string[m_referencedTags.Length];
		string DEFAULT_VALUE = GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY");
		for (int i = 0; i < m_referencedTags.Length; i++)
		{
			int entityID = entityDef.GetTag(m_referencedTags[i]);
			if (entityID == 0)
			{
				referencedEntityNames[i] = DEFAULT_VALUE;
				continue;
			}
			Entity referencedEntity = GameState.Get().GetEntity(entityID);
			if (referencedEntity == null || !referencedEntity.HasValidDisplayName())
			{
				referencedEntityNames[i] = DEFAULT_VALUE;
			}
			else
			{
				referencedEntityNames[i] = referencedEntity.GetName();
			}
		}
		object[] args = referencedEntityNames;
		return TextUtils.TryFormat(rawText, args);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		return BuildText(entity);
	}

	private static string BuildText(Entity entity)
	{
		if (entity == null)
		{
			return "";
		}
		string rawText = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		string[] referencedEntityNames = new string[m_referencedTags.Length];
		string DEFAULT_VALUE = GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY");
		for (int i = 0; i < m_referencedTags.Length; i++)
		{
			int entityID = entity.GetTag(m_referencedTags[i]);
			if (entityID == 0)
			{
				referencedEntityNames[i] = "";
				continue;
			}
			Entity referencedEntity = GameState.Get().GetEntity(entityID);
			if (referencedEntity == null)
			{
				referencedEntityNames[i] = DEFAULT_VALUE;
			}
			else if (!referencedEntity.HasValidDisplayName())
			{
				referencedEntityNames[i] = DEFAULT_VALUE;
			}
			else if (referencedEntity.IsControlledByOpposingSidePlayer() && !referencedEntity.HasTag(GAME_TAG.REVEALED) && referencedEntity.GetZone() != TAG_ZONE.PLAY)
			{
				referencedEntityNames[i] = DEFAULT_VALUE;
			}
			else
			{
				referencedEntityNames[i] = referencedEntity.GetName();
			}
		}
		object[] args = referencedEntityNames;
		string formattedText = TextUtils.TryFormat(rawText, args);
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		GAME_TAG[] referencedTags = m_referencedTags;
		foreach (GAME_TAG tag in referencedTags)
		{
			if (tagChange.tag == (int)tag && card != null && card.GetActor() != null)
			{
				card.GetActor().UpdateTextComponents();
				break;
			}
		}
	}
}
