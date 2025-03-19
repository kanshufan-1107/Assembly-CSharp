public class ScoreValueCountDownCardTextBuilder : CardTextBuilder
{
	public ScoreValueCountDownCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
		m_useEntityForTextInHand = true;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		string rawCardTextInHand = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		string value = GetProgressRemaining(entity).ToString();
		string formattedText = rawCardTextInHand.Replace("@", value);
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		return BuildCardTextInHand(entity);
	}

	private int GetProgressRemaining(Entity entity)
	{
		int tag = entity.GetTag(GAME_TAG.SCORE_VALUE_1);
		int currentProgress = entity.GetTag(GAME_TAG.SCORE_VALUE_2);
		int progressRemaining = tag - currentProgress;
		if (progressRemaining < 0)
		{
			progressRemaining = 0;
		}
		return progressRemaining;
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string rawCardTextInHand = CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId());
		string value = entityDef.GetTag(GAME_TAG.SCORE_VALUE_1).ToString();
		return TextUtils.TransformCardText(rawCardTextInHand.Replace("@", value));
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		if (card == null)
		{
			return;
		}
		Actor actor = card.GetActor();
		if (!(actor == null))
		{
			GAME_TAG tag = (GAME_TAG)tagChange.tag;
			if (tag == GAME_TAG.SCORE_VALUE_1 || tag == GAME_TAG.SCORE_VALUE_2)
			{
				actor.UpdateTextComponents();
			}
			else
			{
				base.OnTagChange(card, tagChange);
			}
		}
	}
}
