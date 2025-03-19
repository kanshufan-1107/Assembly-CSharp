public class TheDarknessCandleSpawnToDeckSpell : SpawnToDeckSpell
{
	protected override void OnAction(SpellStateType prevStateType)
	{
		Card source = GetSourceCard();
		int dormantID = source.GetEntity().GetTag(GAME_TAG.TAG_SCRIPT_DATA_ENT_1);
		Entity dormantEntity = GameState.Get().GetEntity(dormantID);
		if (dormantEntity != null)
		{
			source = dormantEntity.GetCard();
		}
		SetSource(source.gameObject);
		source.GetActorSpell(SpellType.BATTLECRY).ActivateState(SpellStateType.ACTION);
		base.OnAction(prevStateType);
	}
}
