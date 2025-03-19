using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Set scene ambient color")]
[ActionCategory("Pegasus")]
public class SetAmbientColorAction : FsmStateAction
{
	public FsmColor m_Color;

	public bool m_EveryFrame;

	public override void Reset()
	{
		m_Color = null;
		m_EveryFrame = false;
	}

	public override void OnEnter()
	{
		RenderSettings.ambientLight = m_Color.Value;
		if (!m_EveryFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		RenderSettings.ambientLight = m_Color.Value;
	}
}
