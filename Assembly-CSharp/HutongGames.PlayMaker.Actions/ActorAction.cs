using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("INTERNAL USE ONLY. Do not put this on your FSMs.")]
public abstract class ActorAction : FsmStateAction
{
	protected Actor m_actor;

	public Actor GetActor()
	{
		if (m_actor == null)
		{
			GameObject go = GetActorOwner();
			if (go != null)
			{
				m_actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(go);
			}
		}
		return m_actor;
	}

	protected abstract GameObject GetActorOwner();

	public override void Reset()
	{
	}

	public override void OnEnter()
	{
		GetActor();
		if (m_actor == null)
		{
			Debug.LogError($"{this}.OnEnter() - FAILED to find Actor component on Owner \"{base.Owner}\"");
		}
	}
}
