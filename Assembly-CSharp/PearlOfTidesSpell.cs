public class PearlOfTidesSpell : SuperSpell
{
	protected override void OnAction(SpellStateType prevStateType)
	{
		foreach (PowerTask task in m_taskList.GetTaskList())
		{
			if (task.GetPower() is Network.HistFullEntity fullEntity)
			{
				GameState.Get().GetEntity(fullEntity.Entity.ID).GetCard()
					.SuppressPlaySounds(suppress: true);
			}
		}
		base.OnAction(prevStateType);
	}
}
