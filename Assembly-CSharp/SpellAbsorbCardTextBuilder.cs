public class SpellAbsorbCardTextBuilder : CardTextBuilder
{
	public SpellAbsorbCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		string builtText = base.BuildCardTextInHand(entity);
		string alternateText = GetAlternateCardText(builtText, entity);
		return GetReferrencedSpellText(alternateText, entity);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string builtText = base.BuildCardTextInHand(entityDef);
		return GetAlternateCardText(builtText);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		string builtText = base.BuildCardTextInHand(entity);
		string alternateText = GetAlternateCardText(builtText, entity);
		return GetReferrencedSpellText(alternateText, entity);
	}

	private string GetAlternateCardText(string builtText, Entity entity)
	{
		int delimiterIndex = builtText.IndexOf('@');
		if (delimiterIndex < 0)
		{
			return builtText;
		}
		int alternateCardTextIndex = entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1);
		for (int i = 0; i < alternateCardTextIndex; i++)
		{
			builtText = builtText.Substring(delimiterIndex + 1);
			delimiterIndex = builtText.IndexOf('@');
			if (delimiterIndex < 0)
			{
				break;
			}
		}
		if (delimiterIndex >= 0)
		{
			builtText = builtText.Substring(0, delimiterIndex);
		}
		return builtText;
	}

	private string GetAlternateCardText(string builtText)
	{
		int delimiterIndex = builtText.IndexOf('@');
		if (delimiterIndex < 0)
		{
			return builtText;
		}
		return builtText.Substring(0, delimiterIndex);
	}

	private string GetReferrencedSpellText(string alternateText, Entity entity)
	{
		Entity referencedEntity = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_ENT_1));
		if (referencedEntity == null || !referencedEntity.HasValidDisplayName())
		{
			return alternateText;
		}
		string formattedText = string.Format(alternateText, referencedEntity.GetName());
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		if (tagChange.tag == 2)
		{
			if (card != null && card.GetActor() != null)
			{
				card.GetActor().UpdatePowersText();
			}
		}
		else
		{
			base.OnTagChange(card, tagChange);
		}
	}
}
