using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
public class UpdateArmorSpellAction : ActorAction
{
	public FsmGameObject m_ActorObject;

	protected override GameObject GetActorOwner()
	{
		return m_ActorObject.Value;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_actor == null)
		{
			Finish();
			return;
		}
		Entity entity = m_actor.GetEntity();
		if (entity != null && entity.GetArmor() > 0)
		{
			m_actor.SetArmorSpellState(SpellStateType.IDLE);
		}
		Finish();
	}
}
