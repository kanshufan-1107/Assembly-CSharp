public class UndatakahCardTextBuilder : CardTextBuilder
{
	public UndatakahCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		string replaceString = "";
		if (entity.HasTag(GAME_TAG.CUSTOMTEXT3))
		{
			replaceString = "GAMEPLAY_UNDATAKAH3";
		}
		else if (entity.HasTag(GAME_TAG.CUSTOMTEXT2))
		{
			replaceString = "GAMEPLAY_UNDATAKAH2";
		}
		else if (entity.HasTag(GAME_TAG.CUSTOMTEXT1))
		{
			replaceString = "GAMEPLAY_UNDATAKAH1";
		}
		Entity referencedEntity1 = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.CUSTOMTEXT1));
		Entity referencedEntity2 = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.CUSTOMTEXT2));
		Entity referencedEntity3 = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.CUSTOMTEXT3));
		string name1 = ((referencedEntity1 != null && referencedEntity1.HasValidDisplayName()) ? referencedEntity1.GetName() : GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
		string name2 = ((referencedEntity2 != null && referencedEntity2.HasValidDisplayName()) ? referencedEntity2.GetName() : GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
		string name3 = ((referencedEntity3 != null && referencedEntity3.HasValidDisplayName()) ? referencedEntity3.GetName() : GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
		string formattedText = TextUtils.TryFormat(GameStrings.Get(replaceString), name1, name2, name3);
		return TextUtils.TransformCardText(entity, formattedText);
	}
}
