public class BGQuestCardTextBuilder : CardTextBuilder
{
	public BGQuestCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public static string GetRaceString(TAG_RACE race, int count)
	{
		if (count > 1)
		{
			return GameStrings.GetRaceNameBattlegrounds(race);
		}
		return GameStrings.GetRaceName(race);
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
		int count = entity.GetTag(GAME_TAG.QUEST_PROGRESS_TOTAL);
		int rewardDBID = entity.GetTag(GAME_TAG.QUEST_REWARD_DATABASE_ID);
		string rewardName = string.Empty;
		string race1 = string.Empty;
		string race2 = string.Empty;
		if (rewardDBID != 0)
		{
			CardDbfRecord cardRecord = GameDbf.Card.GetRecord(rewardDBID);
			if (cardRecord != null)
			{
				rewardName = cardRecord.Name;
			}
		}
		if (entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1) != 0)
		{
			race1 = GetRaceString((TAG_RACE)entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1), count);
		}
		if (entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2) != 0)
		{
			race2 = GetRaceString((TAG_RACE)entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2), count);
		}
		string formattedText = TextUtils.TryFormat(rawCardTextInHand, count, rewardName, race1, race2);
		int delimiterIndex = formattedText.IndexOf('@');
		if (delimiterIndex >= 0)
		{
			formattedText = ((rewardDBID != 0) ? formattedText.Remove(delimiterIndex, 1) : formattedText.Substring(0, delimiterIndex));
		}
		return TextUtils.TransformCardText(entity, formattedText);
	}
}
