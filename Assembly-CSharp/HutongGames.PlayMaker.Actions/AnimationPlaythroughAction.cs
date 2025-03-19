using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Plays an Animation on a Game Object. Does not wait for the animation to finish.")]
public class AnimationPlaythroughAction : FsmStateAction
{
	[Tooltip("Game Object to play the animation on.")]
	[RequiredField]
	[CheckForComponent(typeof(Animation))]
	public FsmOwnerDefault m_GameObject;

	[UIHint(UIHint.Animation)]
	[Tooltip("The name of the animation to play.")]
	public FsmString m_AnimName;

	public FsmString m_PhoneAnimName;

	[Tooltip("How to treat previously playing animations.")]
	public PlayMode m_PlayMode;

	[HasFloatSlider(0f, 5f)]
	[Tooltip("Time taken to cross fade to this animation.")]
	public FsmFloat m_CrossFadeSec;

	public override void Reset()
	{
		m_GameObject = null;
		m_AnimName = null;
		m_PhoneAnimName = null;
		m_PlayMode = PlayMode.StopAll;
		m_CrossFadeSec = 0.3f;
	}

	public override void OnEnter()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go == null)
		{
			Debug.LogWarning("AnimationPlaythroughAction GameObject is null!");
			Finish();
			return;
		}
		string animName = m_AnimName.Value;
		if ((bool)UniversalInputManager.UsePhoneUI && !string.IsNullOrEmpty(m_PhoneAnimName.Value))
		{
			animName = m_PhoneAnimName.Value;
		}
		if (string.IsNullOrEmpty(animName))
		{
			Finish();
			return;
		}
		StartAnimation(go, animName);
		Finish();
	}

	private void StartAnimation(GameObject go, string animName)
	{
		float crossFadeSec = m_CrossFadeSec.Value;
		Animation animation = go.GetComponent<Animation>();
		if (animation == null)
		{
			Debug.LogWarning("AnimationPlaythroughAction: Trying to start an animation on a GameObject " + go.name + " without a animation component");
		}
		else if (crossFadeSec <= Mathf.Epsilon)
		{
			animation.Play(animName, m_PlayMode);
		}
		else
		{
			animation.CrossFade(animName, crossFadeSec, m_PlayMode);
		}
	}
}
