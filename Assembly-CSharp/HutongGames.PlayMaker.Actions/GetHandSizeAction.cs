namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Stores the size of a player's hand in passed int.")]
public class GetHandSizeAction : FsmStateAction
{
	[Tooltip("Which player's hand are we querying the size of?")]
	public Player.Side m_PlayerSide;

	[UIHint(UIHint.Variable)]
	[RequiredField]
	[Tooltip("Output variable.")]
	public FsmInt m_HandSize;

	public override void Reset()
	{
		m_PlayerSide = Player.Side.NEUTRAL;
		m_HandSize = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_HandSize == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - No variable hooked up to store hand size!", this);
			Finish();
			return;
		}
		if (GameState.Get() == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - GameState is null!", this);
			Finish();
			return;
		}
		if (m_PlayerSide == Player.Side.NEUTRAL)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - No hand exists for player side {1}!", this, m_PlayerSide);
			Finish();
			return;
		}
		Player player = ((m_PlayerSide == Player.Side.FRIENDLY) ? GameState.Get().GetFriendlySidePlayer() : GameState.Get().GetOpposingSidePlayer());
		if (player == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - Unable to find player for side {1}!", this, m_PlayerSide);
			Finish();
			return;
		}
		ZoneHand hand = player.GetHandZone();
		if (hand == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - Unable to find hand for player {1}!", this, player);
			Finish();
		}
		else
		{
			m_HandSize.Value = hand.GetCardCount();
			Finish();
		}
	}
}
