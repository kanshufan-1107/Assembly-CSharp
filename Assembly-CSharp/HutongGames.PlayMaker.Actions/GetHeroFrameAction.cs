namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Put an Actor's cost data into variables.")]
public class GetHeroFrameAction : FsmStateAction
{
	public Player.Side m_side;

	public FsmGameObject m_FrameGameObject;

	public override void Reset()
	{
		m_side = Player.Side.NEUTRAL;
		m_FrameGameObject = new FsmGameObject
		{
			UseVariable = true
		};
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_side == Player.Side.NEUTRAL)
		{
			Finish();
			return;
		}
		if (m_side == Player.Side.FRIENDLY)
		{
			m_FrameGameObject.Value = Board.Get().m_FriendlyHeroTray;
		}
		else if (m_side == Player.Side.OPPOSING)
		{
			m_FrameGameObject.Value = Board.Get().m_OpponentHeroTray;
		}
		Finish();
	}
}
