using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Enables an Animator and plays one of its states.")]
public class AnimatorPlayAction : FsmStateAction
{
	[Tooltip("Game Object to play the animation on.")]
	[RequiredField]
	[CheckForComponent(typeof(Animator))]
	public FsmOwnerDefault m_GameObject;

	public FsmString m_StateName;

	public FsmString m_LayerName;

	[Tooltip("Percent of time into the animation at which to start playing.")]
	[HasFloatSlider(0f, 100f)]
	public FsmFloat m_StartTimePercent;

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
		}
		Finish();
	}
}
