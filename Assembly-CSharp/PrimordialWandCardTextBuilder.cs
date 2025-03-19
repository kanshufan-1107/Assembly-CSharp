public class PrimordialWandCardTextBuilder : CardTextBuilder
{
	public PrimordialWandCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	private string BuildText(Entity entity)
	{
		string builtText = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		int delimiterIndex = builtText.IndexOf('@');
		if (delimiterIndex >= 0)
		{
			builtText = builtText.Substring(delimiterIndex + 1);
		}
		builtText = string.Format(builtText, entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1) + 1);
		return TextUtils.TransformCardText(entity, builtText);
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		return BuildText(entity);
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
		return BuildText(entity);
	}
}
