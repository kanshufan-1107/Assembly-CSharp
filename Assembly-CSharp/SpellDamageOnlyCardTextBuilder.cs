public class SpellDamageOnlyCardTextBuilder : CardTextBuilder
{
	public SpellDamageOnlyCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		TextUtils.TransformCardTextParams parameters = new TextUtils.TransformCardTextParams
		{
			DamageBonus = entity.GetDamageBonus()
		};
		return TextUtils.TransformCardText(CardTextBuilder.GetRawCardTextInHand(entity.GetCardId()), parameters);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		CardTextHistoryData historyData = entity.GetCardTextHistoryData();
		if (historyData == null)
		{
			Log.All.Print("SpellDamageOnlyCardTextBuilder.BuildCardTextInHistory: entity {0} does not have a CardTextHistoryData object.", entity.GetEntityId());
			return string.Empty;
		}
		TextUtils.TransformCardTextParams parameters = new TextUtils.TransformCardTextParams
		{
			DamageBonus = historyData.m_damageBonus
		};
		return TextUtils.TransformCardText(CardTextBuilder.GetRawCardTextInHand(entity.GetCardId()), parameters);
	}
}
