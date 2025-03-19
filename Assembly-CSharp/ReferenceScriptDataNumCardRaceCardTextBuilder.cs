public class ReferenceScriptDataNumCardRaceCardTextBuilder : CardTextBuilder
{
	public ReferenceScriptDataNumCardRaceCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	private static string GetRaceString(TAG_RACE race, int count)
	{
		if (count > 1)
		{
			return GameStrings.GetRaceNameBattlegrounds(race);
		}
		return GameStrings.GetRaceName(race);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		return TextUtils.TransformCardText(BuildFormatedText(entityDef.GetCardId(), entityDef.GetTag(GAME_TAG.QUEST_PROGRESS_TOTAL), entityDef.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1), entityDef.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2)));
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
		string formattedText = BuildFormatedText(entity.GetCardId(), entity.GetTag(GAME_TAG.QUEST_PROGRESS_TOTAL), entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1), entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2));
		return TextUtils.TransformCardText(entity, formattedText);
	}

	private static string BuildFormatedText(string cardID, int count, int scriptDataNum1, int scriptDataNum2)
	{
		string rawText = CardTextBuilder.GetRawCardTextInHand(cardID);
		if (count != 0)
		{
			rawText = rawText.Replace("@", count.ToString());
		}
		string race1 = string.Empty;
		string race2 = string.Empty;
		if (scriptDataNum1 != 0)
		{
			race1 = GetRaceString((TAG_RACE)scriptDataNum1, count);
		}
		if (scriptDataNum2 != 0)
		{
			race2 = GetRaceString((TAG_RACE)scriptDataNum2, count);
		}
		return TextUtils.TryFormat(rawText, race1, race2);
	}
}
