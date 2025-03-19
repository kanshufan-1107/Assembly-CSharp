using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Initialize a spell state, setting variables that reference the parent actor and its contents.")]
public class SpellInitActorVariables : FsmStateAction
{
	public FsmGameObject m_actorObject;

	public FsmGameObject m_rootObject;

	public FsmGameObject m_rootObjectMesh;

	protected Actor GetActor()
	{
		Actor actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(base.Owner);
		if (actor == null)
		{
			Card card = GameObjectUtils.FindComponentInThisOrParents<Card>(base.Owner);
			if (card != null)
			{
				actor = card.GetActor();
			}
		}
		return actor;
	}

	public override void Reset()
	{
	}

	public override void OnEnter()
	{
		Actor actor = GetActor();
		if (actor == null)
		{
			Finish();
			return;
		}
		GameObject actorObject = actor.gameObject;
		if (!m_actorObject.IsNone)
		{
			m_actorObject.Value = actorObject;
		}
		if (!m_rootObject.IsNone)
		{
			m_rootObject.Value = actor.GetRootObject();
		}
		if (!m_rootObjectMesh.IsNone && actor.GetMeshRenderer(getPortrait: true) != null)
		{
			m_rootObjectMesh.Value = actor.GetMeshRenderer(getPortrait: true).gameObject;
		}
		Finish();
	}
}
