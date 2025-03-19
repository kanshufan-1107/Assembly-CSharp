public class KazakusPotionEffectCardTextBuilder : CardTextBuilder
{
	private string GetCorrectSubstring(string text)
	{
		int delimiterIndex = text.IndexOf('@');
		if (delimiterIndex >= 0)
		{
			return text.Substring(0, delimiterIndex);
		}
		return text;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		string builtText = base.BuildCardTextInHand(entity);
		return GetCorrectSubstring(builtText);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string builtText = base.BuildCardTextInHand(entityDef);
		return GetCorrectSubstring(builtText);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		string builtText = base.BuildCardTextInHistory(entity);
		return GetCorrectSubstring(builtText);
	}
}
