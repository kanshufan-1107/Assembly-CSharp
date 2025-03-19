public class ReferenceScriptDataNum1EntityPower : CardTextBuilder
{
	private static bool m_building;

	public ReferenceScriptDataNum1EntityPower()
	{
		m_useEntityForTextInPlay = true;
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string builtText = base.BuildCardTextInHand(entityDef);
		int delimiterIndex = builtText.IndexOf('@');
		if (delimiterIndex >= 0)
		{
			builtText = builtText.Substring(0, delimiterIndex);
		}
		return builtText;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		return BuildTextWithReferenceEntityPower(entity);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		return BuildTextWithReferenceEntityPower(entity);
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

	private string BuildTextWithReferenceEntityPower(Entity entity)
	{
		string rawText = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		string formattedText;
		if (m_building)
		{
			formattedText = GetAlternateCardText(rawText, 0);
			return TextUtils.TransformCardText(entity, formattedText);
		}
		Entity referencedEntity = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1));
		m_building = true;
		if (referencedEntity != null)
		{
			rawText = GetAlternateCardText(rawText, 1);
			string powerText = referencedEntity.GetCardTextBuilder().BuildCardTextInHand(referencedEntity);
			powerText = powerText.Replace('\n', ' ');
			powerText = powerText.Replace("[x]", "");
			formattedText = TextUtils.TryFormat(rawText, powerText);
		}
		else
		{
			formattedText = GetAlternateCardText(rawText, 0);
		}
		m_building = false;
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		if (tagChange.tag == 2)
		{
			if (card != null && card.GetActor() != null)
			{
				card.GetActor().UpdateTextComponents();
			}
		}
		else
		{
			base.OnTagChange(card, tagChange);
		}
	}
}
