using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Get scene ambient color")]
public class GetAmbientColorAction : FsmStateAction
{
	public FsmColor m_Color;

	public bool m_EveryFrame;

	public override void Reset()
	{
		m_Color = Color.white;
		m_EveryFrame = false;
	}

	public override void OnEnter()
	{
		Color sceneColor = Board.Get().m_AmbientColor;
		m_Color.Value = sceneColor;
		if (!m_EveryFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		m_Color.Value = RenderSettings.ambientLight;
	}
}
