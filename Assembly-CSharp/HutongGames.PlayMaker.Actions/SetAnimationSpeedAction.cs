using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Sets the speed of an Animation.")]
public class SetAnimationSpeedAction : FsmStateAction
{
	[CheckForComponent(typeof(Animation))]
	[RequiredField]
	[Tooltip("Game Object to play the animation on.")]
	public FsmOwnerDefault m_GameObject;

	[RequiredField]
	[Tooltip("The name of the animation to play.")]
	[UIHint(UIHint.Animation)]
	public FsmString m_AnimName;

	public FsmString m_PhoneAnimName;

	public FsmFloat m_Speed;

	public bool m_EveryFrame;

	private AnimationState m_animState;

	public override void Reset()
	{
		m_GameObject = null;
		m_AnimName = null;
		m_PhoneAnimName = null;
		m_Speed = 1f;
		m_EveryFrame = false;
	}

	public override void OnEnter()
	{
		if (!CacheAnim())
		{
			Finish();
			return;
		}
		UpdateSpeed();
		if (!m_EveryFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go == null)
		{
			Finish();
		}
		else if (go.GetComponent<Animation>() == null)
		{
			Debug.LogWarning($"SetAnimationSpeedAction.OnUpdate() - GameObject {go} is missing an animation component");
			Finish();
		}
		else
		{
			UpdateSpeed();
		}
	}

	private bool CacheAnim()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go == null)
		{
			return false;
		}
		string animName = m_AnimName.Value;
		if ((bool)UniversalInputManager.UsePhoneUI && !string.IsNullOrEmpty(m_PhoneAnimName.Value))
		{
			animName = m_PhoneAnimName.Value;
		}
		m_animState = go.GetComponent<Animation>()[animName];
		return true;
	}

	private void UpdateSpeed()
	{
		m_animState.speed = m_Speed.Value;
	}
}
