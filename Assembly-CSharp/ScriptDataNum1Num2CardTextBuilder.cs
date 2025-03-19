public class ScriptDataNum1Num2CardTextBuilder : CardTextBuilder
{
	public ScriptDataNum1Num2CardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		string rawCardTextInHand = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		string dataNum1 = entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1).ToString();
		string dataNum2 = entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2).ToString();
		string formattedText = TextUtils.TryFormat(rawCardTextInHand, dataNum1, dataNum2);
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		string rawCardTextInHand = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		string dataNum1 = entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1).ToString();
		string dataNum2 = entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2).ToString();
		string formattedText = TextUtils.TryFormat(rawCardTextInHand, dataNum1, dataNum2);
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string rawCardTextInHand = CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId());
		string dataNum1 = entityDef.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1).ToString();
		string dataNum2 = entityDef.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2).ToString();
		return TextUtils.TransformCardText(TextUtils.TryFormat(rawCardTextInHand, dataNum1, dataNum2));
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		GAME_TAG tag = (GAME_TAG)tagChange.tag;
		if ((uint)(tag - 2) <= 1u)
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
