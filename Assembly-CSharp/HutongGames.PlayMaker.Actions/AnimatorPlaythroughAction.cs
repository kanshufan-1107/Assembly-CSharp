using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Enables an Animator and plays one of its states and waits for it to complete.")]
public class AnimatorPlaythroughAction : FsmStateAction
{
	[CheckForComponent(typeof(Animator))]
	[Tooltip("Game Object to play the animation on.")]
	[RequiredField]
	public FsmOwnerDefault m_GameObject;

	public FsmString m_StateName;

	public FsmString m_LayerName;

	[HasFloatSlider(0f, 100f)]
	[Tooltip("Percent of time into the animation at which to start playing.")]
	public FsmFloat m_StartTimePercent;

	private AnimatorStateInfo m_currentAnimationState;

	private Animator m_checkComplete;

	private int m_checkLayer = -1;

	public override void Reset()
	{
		m_GameObject = null;
		m_StateName = null;
		m_LayerName = new FsmString
		{
			UseVariable = true
		};
		m_StartTimePercent = 0f;
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (!go)
		{
			Finish();
			return;
		}
		Animator animator = go.GetComponent<Animator>();
		if ((bool)animator)
		{
			int layer = -1;
			if (!m_LayerName.IsNone)
			{
				layer = AnimationUtil.GetLayerIndexFromName(animator, m_LayerName.Value);
			}
			float normalizedTime = float.NegativeInfinity;
			if (!m_StartTimePercent.IsNone)
			{
				normalizedTime = 0.01f * m_StartTimePercent.Value;
			}
			animator.enabled = true;
			animator.Play(m_StateName.Value, layer, normalizedTime);
			m_checkComplete = animator;
			m_checkLayer = ((layer != -1) ? layer : 0);
		}
		else
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		if (!(m_checkComplete == null) && m_checkComplete.GetCurrentAnimatorStateInfo(m_checkLayer).normalizedTime > 1f)
		{
			m_checkComplete = null;
			Finish();
		}
	}
}
