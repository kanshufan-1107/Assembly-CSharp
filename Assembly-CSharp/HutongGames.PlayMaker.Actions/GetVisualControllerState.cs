using Hearthstone.UI;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

public class GetVisualControllerState : FsmStateAction
{
	[RequiredField]
	[CheckForComponent(typeof(VisualController))]
	[Tooltip("The GameObject with the AudioSource component.")]
	public FsmOwnerDefault m_GameObject;

	[RequiredField]
	[UIHint(UIHint.Variable)]
	[Tooltip("Store the current state of the visual controller")]
	public FsmString currentState;

	public override void OnEnter()
	{
		GameObject go = m_GameObject.GameObject.Value;
		if (go == null)
		{
			Finish();
			return;
		}
		VisualController controller = go.GetComponent<VisualController>();
		if (controller != null)
		{
			currentState.Value = controller.State;
		}
		Finish();
	}
}
