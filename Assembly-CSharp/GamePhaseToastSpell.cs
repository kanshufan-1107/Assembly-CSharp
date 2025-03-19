public class GamePhaseToastSpell : Spell
{
	protected override void OnIdle(SpellStateType prevStateType)
	{
		base.OnIdle(prevStateType);
		m_taskList.DoAllTasks(delegate
		{
			ChangeState(SpellStateType.DEATH);
		});
	}
}
