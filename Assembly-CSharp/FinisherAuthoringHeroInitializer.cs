using UnityEngine;

public class FinisherAuthoringHeroInitializer : MonoBehaviour
{
	[SerializeField]
	private Actor m_Actor;

	private void Start()
	{
		Card card = base.gameObject.AddComponent<Card>();
		if (null != card)
		{
			card.SetEntity(new FinisherAuthoringDummyEntity());
			card.SetActor(m_Actor);
		}
		CustomHeroFrameBehaviour frame = GetComponentInChildren<CustomHeroFrameBehaviour>();
		if (frame != null)
		{
			frame.UpdateFrame();
		}
	}
}
