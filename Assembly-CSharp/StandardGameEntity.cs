public class StandardGameEntity : GameEntity
{
	public override void OnTagChanged(TagDelta change)
	{
		switch ((GAME_TAG)change.tag)
		{
		case GAME_TAG.NEXT_STEP:
			if (change.newValue == 6)
			{
				if (GameState.Get().IsMulliganManagerActive())
				{
					GameState.Get().SetMulliganBusy(busy: true);
				}
			}
			else if (change.newValue == 10 && (change.oldValue == 9 || change.oldValue == 19))
			{
				GameState.Get().GetTimeTracker().ResetAccruedLostTime();
				if (GameState.Get().IsLocalSidePlayerTurn())
				{
					TurnStartManager.Get().BeginPlayingTurnEvents();
				}
			}
			break;
		case GAME_TAG.STEP:
			if (change.newValue == 4)
			{
				MulliganManager.Get().BeginMulligan();
			}
			break;
		}
		base.OnTagChanged(change);
	}
}
