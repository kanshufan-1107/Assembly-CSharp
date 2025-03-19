using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Initialize a spell state, setting a variable that references one of the Actor's game objects by name.")]
public class SpellCustomActorVariable : FsmStateAction
{
	public FsmString m_objectName;

	public FsmGameObject m_actorObject;

	public override void Reset()
	{
		m_objectName = "";
		m_actorObject = null;
	}

	public override void OnEnter()
	{
		if (!m_actorObject.IsNone)
		{
			Actor actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(base.Owner);
			if (actor != null)
			{
				GameObject result = GameObjectUtils.FindChildBySubstring(actor.gameObject, m_objectName.Value);
				if (result == null)
				{
					Debug.LogWarning("Could not find object of name " + m_objectName?.ToString() + " in actor");
				}
				else
				{
					m_actorObject.Value = result;
				}
			}
		}
		Finish();
	}

	public override void OnUpdate()
	{
	}
}
