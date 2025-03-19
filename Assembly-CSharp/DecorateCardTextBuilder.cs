public class DecorateCardTextBuilder : CardTextBuilder
{
	public override string BuildCardTextInHand(Entity entity)
	{
		string formattedText = TextUtils.TryFormat(CardTextBuilder.GetRawCardTextInHand(entity.GetCardId()), entity.GetTag(GAME_TAG.COST), entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2));
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		if (!(entity.GetCardTextHistoryData() is DecorateCardTextHistoryData historyData))
		{
			Log.All.Print("DecorateCardTextBuilder.BuildCardTextInHistory: entity {0} does not have a DecorateCardTextHistoryData object.", entity.GetEntityId());
			return string.Empty;
		}
		string formattedText = TextUtils.TryFormat(CardTextBuilder.GetRawCardTextInHand(entity.GetCardId()), historyData.m_cost, historyData.m_decorationProgress);
		return TextUtils.TransformCardText(entity, historyData, formattedText);
	}

	public override CardTextHistoryData CreateCardTextHistoryData()
	{
		return new DecorateCardTextHistoryData();
	}
}
