public class ReferenceScriptDataNum1CardDBIDCardTextBuilder : CardTextBuilder
{
	public ReferenceScriptDataNum1CardDBIDCardTextBuilder()
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
		int cardDBID = entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1);
		string name = GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY");
		if (cardDBID != 0)
		{
			CardDbfRecord cardRecord = GameDbf.Card.GetRecord(cardDBID);
			if (cardRecord != null)
			{
				name = cardRecord.Name;
			}
		}
		string formattedText = TextUtils.TryFormat(rawCardTextInHand, name);
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		return TextUtils.TryFormat(CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId()), GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
	}
}
