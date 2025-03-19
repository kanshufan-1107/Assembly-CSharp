public class ReferenceScriptDataNum1EntityCardTextBuilder : CardTextBuilder
{
	public ReferenceScriptDataNum1EntityCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		return BuildText(entity);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		return BuildText(entity);
	}

	private static string BuildText(Entity entity)
	{
		string rawCardTextInHand = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		Entity referencedEntity = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1));
		string name = ((referencedEntity != null && referencedEntity.HasValidDisplayName()) ? referencedEntity.GetName() : GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
		string formattedText = TextUtils.TryFormat(rawCardTextInHand, name);
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		return TextUtils.TryFormat(CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId()), GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		GAME_TAG tag = (GAME_TAG)tagChange.tag;
		if (tag == GAME_TAG.TAG_SCRIPT_DATA_NUM_1 || tag == GAME_TAG.TAG_SCRIPT_DATA_ENT_1)
		{
			if (card != null && card.GetActor() != null)
			{
				card.GetActor().UpdateTextComponents();
				InputManager.Get().ForceRefreshTargetingArrowText();
			}
		}
		else
		{
			base.OnTagChange(card, tagChange);
		}
	}

	public override string GetTargetingArrowText(Entity entity)
	{
		string value = entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1).ToString();
		Entity referencedEntity = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_ENT_1));
		string name = ((referencedEntity != null && referencedEntity.HasValidDisplayName()) ? referencedEntity.GetName() : GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(entity.GetCardId());
		string builtText = string.Empty;
		if (record != null && record.TargetArrowText != null)
		{
			builtText = record.TargetArrowText.GetString();
		}
		return TextUtils.TransformCardText(TextUtils.TryFormat(builtText.Replace("@", value), name));
	}
}
