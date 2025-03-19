public class ReferenceScriptDataNum1Num2EntityCardTextBuilder : CardTextBuilder
{
	public ReferenceScriptDataNum1Num2EntityCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		string rawCardTextInHand = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		Entity referencedEntity = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1));
		Entity referencedEntity2 = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2));
		string name = ((referencedEntity != null && referencedEntity.HasValidDisplayName()) ? referencedEntity.GetName() : GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
		string name2 = ((referencedEntity2 != null && referencedEntity2.HasValidDisplayName()) ? referencedEntity2.GetName() : GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
		string formattedText = TextUtils.TryFormat(rawCardTextInHand, name, name2);
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		string rawCardTextInHand = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		Entity referencedEntity = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1));
		Entity referencedEntity2 = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2));
		string name = ((referencedEntity != null && referencedEntity.HasValidDisplayName()) ? referencedEntity.GetName() : GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
		string name2 = ((referencedEntity2 != null && referencedEntity2.HasValidDisplayName()) ? referencedEntity2.GetName() : GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
		string formattedText = TextUtils.TryFormat(rawCardTextInHand, name, name2);
		return TextUtils.TransformCardText(entity, formattedText);
	}
}
