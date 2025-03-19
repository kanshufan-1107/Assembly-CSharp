using UnityEngine;

public class LegendaryHeroActorCache : MonoBehaviour
{
	public delegate void OnActorAttached(Actor actor);

	private Actor m_actor;

	public Actor Actor
	{
		get
		{
			return m_actor;
		}
		set
		{
			m_actor = value;
			TriggerActorAttachedEvent(value);
		}
	}

	public event OnActorAttached ActorAttachedEvent;

	private void TriggerActorAttachedEvent(Actor actor)
	{
		if (this.ActorAttachedEvent != null)
		{
			this.ActorAttachedEvent(actor);
		}
	}
}
