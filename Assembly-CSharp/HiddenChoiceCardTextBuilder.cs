public class HiddenChoiceCardTextBuilder : CardTextBuilder
{
	private string GetCorrectSubstring(string text, int hiddenChoiceValue)
	{
		string[] substrings = text.Split('@');
		if (substrings.Length <= hiddenChoiceValue)
		{
			Log.All.Print("HiddenChoiceCardTextBuilder.GetCorrectSubstring: entity does not have text for hidden choice {1}.", hiddenChoiceValue);
			return text;
		}
		return substrings[hiddenChoiceValue];
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		string builtText = base.BuildCardTextInHand(entity);
		int overrideTag = entity.GetTag(GAME_TAG.HIDDEN_CHOICE_OVERRIDE);
		return GetCorrectSubstring(builtText, (overrideTag != 0) ? overrideTag : entity.GetTag(GAME_TAG.HIDDEN_CHOICE));
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string builtText = base.BuildCardTextInHand(entityDef);
		int overrideTag = entityDef.GetTag(GAME_TAG.HIDDEN_CHOICE_OVERRIDE);
		return GetCorrectSubstring(builtText, (overrideTag != 0) ? overrideTag : entityDef.GetTag(GAME_TAG.HIDDEN_CHOICE));
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		string builtText = base.BuildCardTextInHistory(entity);
		int overrideTag = entity.GetTag(GAME_TAG.HIDDEN_CHOICE_OVERRIDE);
		return GetCorrectSubstring(builtText, (overrideTag != 0) ? overrideTag : entity.GetTag(GAME_TAG.HIDDEN_CHOICE));
	}
}
