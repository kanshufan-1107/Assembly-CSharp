using HutongGames.PlayMaker;

public class BaconBaubleUpbeatSpell : Spell
{
	protected void UpdateFSMVar()
	{
		if (m_fsm == null)
		{
			return;
		}
		FsmBool fsmVar = m_fsm.FsmVariables.GetFsmBool("IsUpbeat");
		if (fsmVar == null)
		{
			return;
		}
		Card source = GetSourceCard();
		GameEntity shopEntity = GameState.Get().GetGameEntity();
		if (shopEntity == null || !shopEntity.IsInBattlegroundsShopPhase() || source == null)
		{
			fsmVar.Value = false;
			return;
		}
		int current = source.GetEntity()?.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1) ?? 0;
		if (m_taskList != null)
		{
			m_taskList.GetTagUpdatedValue(source?.GetEntity(), GAME_TAG.TAG_SCRIPT_DATA_NUM_1, ref current);
		}
		fsmVar.Value = current == 1;
	}

	protected override void OnIdle(SpellStateType prevStateType)
	{
		UpdateFSMVar();
		base.OnIdle(prevStateType);
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		UpdateFSMVar();
		base.OnAction(prevStateType);
	}
}
