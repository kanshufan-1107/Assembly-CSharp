using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
public class UpdateTeammatePingAction : FsmStateAction
{
	public FsmGameObject m_fsmGameObject;

	public FsmInt m_PingType;

	public override void OnEnter()
	{
		base.OnEnter();
		TeammatePingOptions.SetSpriteFromPingType(m_fsmGameObject.Value.GetComponent<SpriteRenderer>(), (TEAMMATE_PING_TYPE)m_PingType.Value);
		Finish();
	}
}
