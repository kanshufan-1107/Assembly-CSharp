namespace HutongGames.PlayMaker.Actions;

[Tooltip("Set scene ambient color")]
[ActionCategory("Pegasus")]
public class ResetAmbientColorAction : FsmStateAction
{
	private SetRenderSettings m_renderSettings;

	public override void Reset()
	{
	}

	public override void OnEnter()
	{
		Board board = Board.Get();
		if (board != null)
		{
			board.ResetAmbientColor();
		}
		Finish();
	}
}
