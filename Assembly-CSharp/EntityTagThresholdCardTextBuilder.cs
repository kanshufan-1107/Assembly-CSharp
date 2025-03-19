public class EntityTagThresholdCardTextBuilder : CardTextBuilder
{
	public override string BuildCardTextInHand(Entity entity)
	{
		GAME_TAG entityTag = (GAME_TAG)entity.GetTag(GAME_TAG.ENTITY_TAG_THRESHOLD_TAG_ID);
		int tagValue = entity.GetTag(entityTag);
		int tagThreshold = entity.GetTag(GAME_TAG.ENTITY_TAG_THRESHOLD_VALUE);
		string builtText = base.BuildCardTextInHand(entity);
		int delimiterIndex = builtText.IndexOf('@');
		int delimiterIndex2 = builtText.IndexOf('@', delimiterIndex + 1);
		if (delimiterIndex >= 0 && delimiterIndex2 >= 0)
		{
			string formattedText = builtText.Substring(0, delimiterIndex);
			if (tagValue >= tagThreshold)
			{
				formattedText += builtText.Substring(delimiterIndex2 + 1);
			}
			else
			{
				int delimiterDelta = delimiterIndex2 - delimiterIndex - 1;
				formattedText += builtText.Substring(delimiterIndex + 1, delimiterDelta);
				formattedText = TextUtils.TryFormat(formattedText, tagThreshold - tagValue);
			}
			builtText = formattedText;
		}
		return builtText;
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

	public override string BuildCardTextInHistory(Entity entity)
	{
		string builtText = base.BuildCardTextInHistory(entity);
		int delimiterIndex = builtText.IndexOf('@');
		if (delimiterIndex >= 0)
		{
			builtText = builtText.Substring(0, delimiterIndex);
		}
		return builtText;
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
