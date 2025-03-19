public class AlternateCardTextCardTextBuilder : CardTextBuilder
{
	public AlternateCardTextCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		string builtText = base.BuildCardTextInHand(entity);
		return GetAlternateCardText(builtText, entity.GetTag(GAME_TAG.USE_ALTERNATE_CARD_TEXT));
	}

	private string GetAlternateCardText(string builtText, int alternateCardTextIndex)
	{
		int delimiterIndex = builtText.IndexOf('@');
		if (delimiterIndex < 0)
		{
			return builtText;
		}
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

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string builtText = base.BuildCardTextInHand(entityDef);
		return GetAlternateCardText(builtText, entityDef.GetTag(GAME_TAG.USE_ALTERNATE_CARD_TEXT));
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		string builtText = base.BuildCardTextInHand(entity);
		return GetAlternateCardText(builtText, entity.GetTag(GAME_TAG.USE_ALTERNATE_CARD_TEXT));
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		if (tagChange.tag == 955)
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
