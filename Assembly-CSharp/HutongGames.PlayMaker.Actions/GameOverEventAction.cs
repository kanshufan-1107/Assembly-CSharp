namespace HutongGames.PlayMaker.Actions;

[Tooltip("Send an event based on if the game is over or not.")]
[ActionCategory("Pegasus")]
public class GameOverEventAction : FsmStateAction
{
	public FsmEvent m_GameOverEvent;

	public FsmEvent m_GameInProgressEvent;

	public override void Reset()
	{
		m_GameOverEvent = null;
		m_GameInProgressEvent = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (GameState.Get() != null && !GameState.Get().IsGameOver())
		{
			base.Fsm.Event(m_GameInProgressEvent);
		}
		else
		{
			base.Fsm.Event(m_GameOverEvent);
		}
		Finish();
	}
}
