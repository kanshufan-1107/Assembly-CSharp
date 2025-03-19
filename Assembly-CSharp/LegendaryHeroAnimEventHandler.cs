using UnityEngine;

public class LegendaryHeroAnimEventHandler : MonoBehaviour
{
	private Card m_heroCard;

	public void SetActorObject(GameObject actorObject)
	{
		SetActor((actorObject != null) ? actorObject.GetComponent<Actor>() : null);
	}

	public void SetActor(Actor actor)
	{
		Card newCard = null;
		if (actor != null)
		{
			newCard = actor.GetCard();
		}
		m_heroCard = newCard;
	}

	public void RaiseAnimationEvent(LegendaryHeroAnimations animation)
	{
		if (m_heroCard != null)
		{
			m_heroCard.ActivateLegendaryHeroAnimEvent(animation);
		}
	}
}
