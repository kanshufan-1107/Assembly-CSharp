using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Set the required color on an UberText object")]
[ActionCategory("Pegasus")]
public class SetUberTextColor : FsmStateAction
{
	[RequiredField]
	public FsmOwnerDefault uberTextObject;

	public FsmColor textColor;

	[Tooltip("Set the UberText color every frame.")]
	public bool everyFrame;

	public override void Reset()
	{
		uberTextObject = null;
		textColor = Color.white;
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
				uberText.TextColor = textColor.Value;
			}
		}
	}
}
