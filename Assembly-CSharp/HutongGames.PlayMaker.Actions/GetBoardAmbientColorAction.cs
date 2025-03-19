using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Get scene ambient color")]
[ActionCategory("Pegasus")]
public class GetBoardAmbientColorAction : FsmStateAction
{
	public FsmColor m_Color;

	public override void Reset()
	{
		m_Color = Color.white;
	}

	public override void OnEnter()
	{
		m_Color.Value = RenderSettings.ambientLight;
		Board board = Board.Get();
		if (board != null)
		{
			m_Color.Value = board.m_AmbientColor;
		}
		Finish();
	}
}
