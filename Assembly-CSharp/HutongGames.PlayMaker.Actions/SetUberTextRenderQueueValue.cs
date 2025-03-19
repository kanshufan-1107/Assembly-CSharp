using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Sets the render queue value on an UberText object.")]
public class SetUberTextRenderQueueValue : FsmStateAction
{
	[RequiredField]
	public FsmOwnerDefault uberTextObject;

	public FsmInt value;

	[Tooltip("Set the UberText every frame. Useful if the text variable is expected to change/animate.")]
	public bool everyFrame;

	public override void Reset()
	{
		uberTextObject = null;
		value = 0;
		everyFrame = false;
	}

	public override void OnEnter()
	{
		UpdateText();
		if (!everyFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		UpdateText();
	}

	private void UpdateText()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(uberTextObject);
		if (go != null)
		{
			UberText uberText = go.GetComponent<UberText>();
			if (uberText != null)
			{
				uberText.RenderQueue = value.Value;
			}
		}
	}
}
