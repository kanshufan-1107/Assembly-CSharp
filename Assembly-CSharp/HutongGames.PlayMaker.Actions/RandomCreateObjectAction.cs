using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Randomly picks an object from a list and creates it. THE CREATED OBJECT MUST BE DESTROYED!")]
public class RandomCreateObjectAction : FsmStateAction
{
	[Tooltip("The created object gets stored in this variable.")]
	[UIHint(UIHint.Variable)]
	public FsmGameObject m_CreatedObject;

	[CompoundArray("Random Objects", "Object", "Weight")]
	public FsmGameObject[] m_Objects;

	[HasFloatSlider(0f, 1f)]
	public FsmFloat[] m_Weights;

	public override void Reset()
	{
		m_CreatedObject = new FsmGameObject
		{
			UseVariable = true
		};
		m_Objects = new FsmGameObject[3];
		m_Weights = new FsmFloat[3] { 1f, 1f, 1f };
	}

	public override void OnEnter()
	{
		if (m_Objects == null || m_Objects.Length == 0)
		{
			Finish();
			return;
		}
		GameObject createdObject = null;
		FsmGameObject fsmGo = m_Objects[ActionHelpers.GetRandomWeightedIndex(m_Weights)];
		if (fsmGo != null && !fsmGo.IsNone && (bool)fsmGo.Value)
		{
			createdObject = Object.Instantiate(fsmGo.Value);
			TransformUtil.CopyWorld(createdObject, fsmGo.Value);
		}
		if (m_CreatedObject != null)
		{
			m_CreatedObject.Value = createdObject;
		}
		Finish();
	}
}
