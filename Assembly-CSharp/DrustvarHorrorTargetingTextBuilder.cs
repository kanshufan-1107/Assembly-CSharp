public class DrustvarHorrorTargetingTextBuilder : ReferenceScriptDataNum1Num2EntityCardTextBuilder
{
	public override string GetTargetingArrowText(Entity entity)
	{
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(entity.GetCardId());
		if (record == null || record.TargetArrowText == null)
		{
			return string.Empty;
		}
		Entity referencedEntity = GameState.Get().GetEntity(entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_ENT_1));
		if (referencedEntity == null || !referencedEntity.HasValidDisplayName())
		{
			return string.Empty;
		}
		string formattedText = string.Format(record.TargetArrowText, referencedEntity.GetName());
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		return TextUtils.TransformCardText(string.Format(CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId()), GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY"), GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY")));
	}
}
