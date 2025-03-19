using UnityEngine;

public class SecondaryAnimationEventListener : MonoBehaviour
{
	public delegate void SecondaryAnimationEventCallback(SecondaryAnimationEvent animEvent);

	public event SecondaryAnimationEventCallback OnAnimationEvent;

	public void NotifyAnimationEvent(SecondaryAnimationEvent animEvent)
	{
		this.OnAnimationEvent?.Invoke(animEvent);
	}
}
