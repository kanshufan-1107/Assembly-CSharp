using UnityEngine;
using UnityEngine.Animations;

public class LegendaryHeroStateRefCount : StateMachineBehaviour
{
	public string ParameterName;

	private int m_parameterID;

	private void Awake()
	{
		m_parameterID = Animator.StringToHash(ParameterName);
	}

	private void SetValue(Animator animator, int valueDelta)
	{
		int currentValue = animator.GetInteger(m_parameterID);
		animator.SetInteger(m_parameterID, currentValue + valueDelta);
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		SetValue(animator, 1);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		SetValue(animator, -1);
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		OnStateEnter(animator, stateInfo, layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		OnStateExit(animator, stateInfo, layerIndex);
	}
}
