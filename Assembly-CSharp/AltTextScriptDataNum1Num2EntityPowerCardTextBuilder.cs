using System.Text;

public class AltTextScriptDataNum1Num2EntityPowerCardTextBuilder : CardTextBuilder
{
	private static bool m_building;

	public AltTextScriptDataNum1Num2EntityPowerCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
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

	private string BuildTextWithReferenceEntityPowers(Entity entity)
	{
		string rawText = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		string formattedText;
		if (m_building)
		{
			formattedText = GetAlternateCardText(rawText, 0);
			return TextUtils.TransformCardText(entity, formattedText);
		}
		Entity referencedEntity = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1));
		Entity referencedEntity2 = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2));
		m_building = true;
		if (referencedEntity != null && referencedEntity2 != null)
		{
			rawText = GetAlternateCardText(rawText, 1);
			string powerText1 = referencedEntity.GetCardTextBuilder().BuildCardTextInHand(referencedEntity);
			string powerText2 = referencedEntity2.GetCardTextBuilder().BuildCardTextInHand(referencedEntity2);
			powerText1 = UberText.RemoveMarkupAndCollapseWhitespaces(powerText1, replaceCarriageReturnWithBreakHint: false, preserveBreakHint: false, preserveBold: true);
			powerText1 = RemoveLastCharacterFromString(powerText1, powerText1.LastIndexOf('.'));
			powerText2 = UberText.RemoveMarkupAndCollapseWhitespaces(powerText2, replaceCarriageReturnWithBreakHint: false, preserveBreakHint: false, preserveBold: true);
			powerText2 = RemoveLastCharacterFromString(powerText2, powerText2.LastIndexOf('.'));
			formattedText = string.Format(rawText, powerText1, powerText2);
		}
		else
		{
			formattedText = GetAlternateCardText(rawText, 0);
		}
		m_building = false;
		return TextUtils.TransformCardText(entity, formattedText);
	}

	private string RemoveLastCharacterFromString(string target, int index)
	{
		StringBuilder targetStringBuilder = new StringBuilder(target);
		if (index > 0 && index == target.Length - 1)
		{
			targetStringBuilder = targetStringBuilder.Remove(index, 1);
		}
		return targetStringBuilder.ToString();
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		return BuildTextWithReferenceEntityPowers(entity);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		return BuildTextWithReferenceEntityPowers(entity);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string builtText = base.BuildCardTextInHand(entityDef);
		return GetAlternateCardText(builtText, entityDef.GetTag(GAME_TAG.USE_ALTERNATE_CARD_TEXT));
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		switch ((GAME_TAG)tagChange.tag)
		{
		case GAME_TAG.TAG_SCRIPT_DATA_NUM_1:
			if (card != null && card.GetActor() != null)
			{
				card.GetActor().UpdateTextComponents();
			}
			break;
		case GAME_TAG.TAG_SCRIPT_DATA_NUM_2:
			if (card != null && card.GetActor() != null)
			{
				card.GetActor().UpdateTextComponents();
			}
			break;
		default:
			base.OnTagChange(card, tagChange);
			break;
		}
	}
}
