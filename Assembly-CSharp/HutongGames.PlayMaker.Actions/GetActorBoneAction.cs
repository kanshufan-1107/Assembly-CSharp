using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Stores the value of an Actor's bone of a given name into a Vector3.")]
[ActionCategory("Pegasus")]
public class GetActorBoneAction : FsmStateAction
{
	public FsmGameObject m_actorObject;

	public string m_boneName;

	[UIHint(UIHint.Variable)]
	[Tooltip("Output variable.")]
	[RequiredField]
	public FsmVector3 m_Bone;

	public override void Reset()
	{
		m_Bone = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_Bone == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - No variable hooked up to store tag value!", this);
			Finish();
			return;
		}
		Actor actorObject = m_actorObject.Value.GetComponent<Actor>();
		if (actorObject == null)
		{
			global::Log.All.PrintError("{0}.OnEnter() - FAILED to find actor component", this);
			Finish();
			return;
		}
		GameObject bone = actorObject.FindBone(m_boneName);
		if (bone == null)
		{
			global::Log.All.PrintError("{0}.OnEnter() - FAILED to find bone '{1}'", this, m_boneName);
			Finish();
		}
		else
		{
			m_Bone.Value = bone.transform.localPosition;
			Finish();
		}
	}
}
