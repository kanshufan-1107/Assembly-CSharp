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
		string formattedText = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		if (formattedText.Contains('@'))
		{
			string value = entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2).ToString();
			formattedText = formattedText.Replace("@", value);
		}
		Entity referencedEntity = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1));
		string name = ((referencedEntity != null && referencedEntity.HasValidDisplayName()) ? referencedEntity.GetName() : GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
		formattedText = TextUtils.TryFormat(formattedText, name);
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string rawText = CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId());
		string formattedText;
		if (rawText.Contains('@'))
		{
			string value = entityDef.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2).ToString();
			formattedText = rawText.Replace("@", value);
		}
		else
		{
			formattedText = rawText;
		}
		return TextUtils.TryFormat(formattedText, GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		GAME_TAG tag = (GAME_TAG)tagChange.tag;
		if ((uint)(tag - 2) <= 1u)
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
		Entity referencedEntity = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1));
		string name = ((referencedEntity != null && referencedEntity.HasValidDisplayName()) ? referencedEntity.GetName() : GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(entity.GetCardId());
		string builtText = string.Empty;
		if (record != null && record.TargetArrowText != null)
		{
			builtText = record.TargetArrowText.GetString();
		}
		string value = entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2).ToString();
		return TextUtils.TransformCardText(TextUtils.TryFormat(builtText.Replace("@", value), name));
	}
}
