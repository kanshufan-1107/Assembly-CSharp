using System;
using Blizzard.BlizzardErrorMobile;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Plays an Animation on a Game Object and waits for the animation to finish.")]
public class AnimationPlayAction : FsmStateAction
{
	[RequiredField]
	[Tooltip("Game Object to play the animation on.")]
	[CheckForComponent(typeof(Animation))]
	public FsmOwnerDefault m_GameObject;

	[Tooltip("The name of the animation to play.")]
	[UIHint(UIHint.Animation)]
	public FsmString m_AnimName;

	public FsmString m_PhoneAnimName;

	[Tooltip("How to treat previously playing animations.")]
	public PlayMode m_PlayMode;

	[Tooltip("Time taken to cross fade to this animation.")]
	[HasFloatSlider(0f, 5f)]
	public FsmFloat m_CrossFadeSec;

	[Tooltip("Event to send when the animation is finished playing. NOTE: Not sent with Loop or PingPong wrap modes!")]
	public FsmEvent m_FinishEvent;

	[Tooltip("Event to send when the animation loops. If you want to send this event to another FSM use Set Event Target. NOTE: This event is only sent with Loop and PingPong wrap modes.")]
	public FsmEvent m_LoopEvent;

	[Tooltip("Stop playing the animation when this state is exited.")]
	public bool m_StopOnExit;

	private string m_animName;

	private AnimationState m_animState;

	private float m_prevAnimTime;

	public override void Reset()
	{
		m_GameObject = null;
		m_AnimName = null;
		m_PhoneAnimName = null;
		m_PlayMode = PlayMode.StopAll;
		m_CrossFadeSec = 0.3f;
		m_FinishEvent = null;
		m_LoopEvent = null;
		m_StopOnExit = false;
	}

	public override void OnEnter()
	{
		if (!CacheAnim())
		{
			Finish();
		}
		else
		{
			StartAnimation();
		}
	}

	public override void OnUpdate()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go == null)
		{
			Finish();
			return;
		}
		if (go.GetComponent<Animation>() == null)
		{
			Debug.LogError($"AnimationPlayAction.OnUpdate() - animation component is missing. GameObject: {go}, full path: {go.GetFullPath()}");
			ExceptionReporter.Get().ReportCaughtException(new Exception($"AnimationPlayAction.OnUpdate() - animation component is missing. GameObject: {go}, full path: {go.GetFullPath()}"));
			Finish();
			return;
		}
		if (m_animState == null)
		{
			Debug.LogError($"m_animState is null. GameObject: {go}, full path: {go.GetFullPath()}");
			ExceptionReporter.Get().ReportCaughtException(new Exception($"m_animState is null. GameObject: {go}, full path: {go.GetFullPath()}"));
			Finish();
			return;
		}
		if (!m_animState.enabled || (m_animState.wrapMode == WrapMode.ClampForever && m_animState.time > m_animState.length))
		{
			base.Fsm.Event(m_FinishEvent);
			Finish();
		}
		if (m_animState.wrapMode != WrapMode.ClampForever && m_animState.time > m_animState.length && m_prevAnimTime < m_animState.length)
		{
			base.Fsm.Event(m_LoopEvent);
		}
	}

	public override void OnExit()
	{
		if (m_StopOnExit)
		{
			StopAnimation();
		}
	}

	private bool CacheAnim()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go == null)
		{
			return false;
		}
		m_animName = m_AnimName.Value;
		if ((bool)UniversalInputManager.UsePhoneUI && !string.IsNullOrEmpty(m_PhoneAnimName.Value))
		{
			m_animName = m_PhoneAnimName.Value;
		}
		m_animState = go.GetComponent<Animation>()[m_animName];
		return true;
	}

	private void StartAnimation()
	{
		if (base.Fsm == null)
		{
			return;
		}
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go == null)
		{
			return;
		}
		Animation animation = go.GetComponent<Animation>();
		if (!(animation == null))
		{
			float crossFadeSec = ((m_CrossFadeSec == null) ? 0f : m_CrossFadeSec.Value);
			if (crossFadeSec <= Mathf.Epsilon)
			{
				animation.Play(m_animName, m_PlayMode);
			}
			else
			{
				animation.CrossFade(m_animName, crossFadeSec, m_PlayMode);
			}
			m_prevAnimTime = ((m_animState == null) ? 0f : m_animState.time);
		}
	}

	private void StopAnimation()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (!(go == null) && go.TryGetComponent<Animation>(out var anim))
		{
			anim.Stop(m_animName);
		}
	}
}
