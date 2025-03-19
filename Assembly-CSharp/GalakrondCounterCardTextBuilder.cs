public class GalakrondCounterCardTextBuilder : CardTextBuilder
{
	public GalakrondCounterCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		string rawCardTextInHand = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		string replace = ((entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2) - entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1) == 1) ? GameStrings.Get("GALAKROND_ONCE") : GameStrings.Get("GALAKROND_TWICE"));
		string formattedText = rawCardTextInHand.Replace("@", replace);
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		string rawCardTextInHand = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		string replace = ((entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2) - entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1) == 1) ? GameStrings.Get("GALAKROND_ONCE") : GameStrings.Get("GALAKROND_TWICE"));
		string formattedText = rawCardTextInHand.Replace("@", replace);
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string rawCardTextInHand = CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId());
		string replace = ((entityDef.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2) - entityDef.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1) == 1) ? GameStrings.Get("GALAKROND_ONCE") : GameStrings.Get("GALAKROND_TWICE"));
		return TextUtils.TransformCardText(rawCardTextInHand.Replace("@", replace));
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		if (tagChange.tag == 2 && card != null && card.GetActor() != null)
		{
			card.GetActor().UpdateTextComponents();
		}
	}
}
