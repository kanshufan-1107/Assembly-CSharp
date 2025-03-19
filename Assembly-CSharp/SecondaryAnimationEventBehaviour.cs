using UnityEngine;

public class SecondaryAnimationEventBehaviour : StateMachineBehaviour
{
	public SecondaryAnimationEvent animEvent;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int _)
	{
		SecondaryAnimationEventListener controller = animator.GetComponent<SecondaryAnimationEventListener>();
		if (controller != null)
		{
			controller.NotifyAnimationEvent(animEvent);
		}
	}
}
