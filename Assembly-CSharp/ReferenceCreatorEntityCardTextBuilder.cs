public class ReferenceCreatorEntityCardTextBuilder : CardTextBuilder
{
	public ReferenceCreatorEntityCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		string rawCardTextInHand = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		Entity creator = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.CREATOR));
		string name = ((creator != null && creator.HasValidDisplayName()) ? creator.GetName() : GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"));
		string formattedText = TextUtils.TryFormat(rawCardTextInHand, name);
		return TextUtils.TransformCardText(entity, formattedText);
	}
}
