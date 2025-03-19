public class HiddenEntityCardTextBuilder : CardTextBuilder
{
	public override string BuildCardTextInHand(Entity entity)
	{
		string[] substrings = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId()).Split('@');
		string builtText = substrings[0];
		int databaseID = entity.GetTag(GAME_TAG.HIDDEN_CHOICE);
		if (databaseID > 0 && substrings.Length > 1)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(databaseID);
			if (entityDef != null && entityDef.HasValidDisplayName())
			{
				builtText = TextUtils.TryFormat(substrings[1], CardTextBuilder.GetRawCardName(entityDef));
			}
		}
		return TextUtils.TransformCardText(entity, builtText);
	}
}
