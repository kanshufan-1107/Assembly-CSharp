using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Send an event based on an Actor's is In Mini Hand Or not.")]
public class ActorInMiniHandEventAction : ActorAction
{
	public FsmOwnerDefault m_ActorObject;

	public FsmEvent m_IsInMiniHandEvent;

	public FsmEvent m_isNotInMiniHandEvent;

	protected override GameObject GetActorOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_ActorObject);
	}

	public override void Reset()
	{
		m_ActorObject = null;
		m_IsInMiniHandEvent = null;
		m_isNotInMiniHandEvent = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_actor == null)
		{
			Finish();
			return;
		}
		if (!UniversalInputManager.UsePhoneUI && m_isNotInMiniHandEvent != null)
		{
			base.Fsm.Event(m_isNotInMiniHandEvent);
		}
		if (GameUtils.IsActorInMiniHand(m_actor))
		{
			base.Fsm.Event(m_IsInMiniHandEvent);
		}
		else
		{
			base.Fsm.Event(m_isNotInMiniHandEvent);
		}
		Finish();
	}
}
