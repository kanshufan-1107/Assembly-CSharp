using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory(ActionCategory.GameObject)]
[Tooltip("Sets the Parent of a Game Object.")]
public class SetParent : FsmStateAction
{
	[RequiredField]
	[Tooltip("The Game Object to parent.")]
	public FsmOwnerDefault gameObject;

	[Tooltip("The new parent for the Game Object. Leave empty or None to un-parent the Game Object.")]
	public FsmGameObject parent;

	[Tooltip("If true, the parent-relative position, scale and rotation are modified such that the object keeps the same world space position, rotation and scale as before. Hint: Setting to False can fix common UI scaling issues.")]
	public FsmBool worldPositionStays;

	[Tooltip("Set the local position to 0,0,0 after parenting.")]
	public FsmBool resetLocalPosition;

	[Tooltip("Set the local rotation to 0,0,0 after parenting.")]
	public FsmBool resetLocalRotation;

	[Tooltip("Set the local scale to 1,1,1 after parenting.")]
	public FsmBool resetLocalScale;

	[Tooltip("Sets the game object's layer to the same as the parent's if true")]
	public FsmBool changeToParentLayer;

	public override void Reset()
	{
		gameObject = null;
		parent = null;
		worldPositionStays = true;
		resetLocalPosition = null;
		resetLocalRotation = null;
		resetLocalScale = null;
		changeToParentLayer = null;
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(gameObject);
		if (go != null)
		{
			go.transform.SetParent((parent.Value == null) ? null : parent.Value.transform, worldPositionStays.Value);
			if (changeToParentLayer.Value)
			{
				int parentLayer = parent.Value.layer;
				LayerUtils.SetLayer(go, parentLayer, null);
			}
			if (resetLocalPosition.Value)
			{
				go.transform.localPosition = Vector3.zero;
			}
			if (resetLocalRotation.Value)
			{
				go.transform.localRotation = Quaternion.identity;
			}
			if (resetLocalScale.Value)
			{
				go.transform.localScale = Vector3.one;
			}
		}
		Finish();
	}
}
