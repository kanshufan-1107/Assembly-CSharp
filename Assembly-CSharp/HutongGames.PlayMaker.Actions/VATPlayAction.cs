using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Plays an animation from the Vertex Animation component on the diamond model.")]
[ActionCategory("Pegasus")]
public class VATPlayAction : FsmStateAction
{
	[RequiredField]
	[Tooltip("Game Object that has the VertexAnimation.")]
	[CheckForComponent(typeof(VertexAnimation))]
	public FsmOwnerDefault m_GameObject;

	[RequiredField]
	[UIHint(UIHint.FsmString)]
	[Tooltip("The name of the animation to play.")]
	public FsmString m_AnimName;

	public bool m_waitToFinish;

	public bool m_overwriteAnimationSpeed;

	[Tooltip("Will use this speed instead of the one defined in VertexAnimation")]
	[HideIf("ShouldHideAnimationSpeed")]
	public float m_animationSpeed = 1f;

	private VertexAnimation m_vertexAnimation;

	private float m_animationDuration;

	private float m_currentTime;

	private bool m_requestFinish;

	public bool ShouldHideAnimationSpeed()
	{
		return !m_overwriteAnimationSpeed;
	}

	public override void Reset()
	{
		m_GameObject = null;
		m_AnimName = "";
		m_waitToFinish = false;
	}

	public override void OnEnter()
	{
		if (!CacheAnim())
		{
			m_requestFinish = true;
			return;
		}
		StartAnimation();
		if (!m_waitToFinish)
		{
			m_requestFinish = true;
		}
		else if (m_animationDuration <= 0f)
		{
			m_requestFinish = true;
		}
	}

	public override void OnUpdate()
	{
		if (m_requestFinish)
		{
			Finish();
			return;
		}
		m_currentTime += Time.deltaTime;
		if (m_currentTime >= m_animationDuration)
		{
			Finish();
		}
	}

	private bool CacheAnim()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_GameObject);
		if (go == null)
		{
			return false;
		}
		VertexAnimation[] vertexAnimationComponents = go.GetComponentsInParent<VertexAnimation>(includeInactive: true);
		if (vertexAnimationComponents.Length == 0)
		{
			m_vertexAnimation = null;
			Debug.LogWarning($"VATPlayAction - GameObject is missing a vertex animation component");
			return false;
		}
		m_vertexAnimation = vertexAnimationComponents[0];
		if (m_overwriteAnimationSpeed)
		{
			m_vertexAnimation.OverwriteAnimationSpeed(m_animationSpeed);
		}
		else
		{
			m_vertexAnimation.UseDefaultAnimationSpeed();
		}
		m_animationDuration = m_vertexAnimation.GetAnimationLength(m_AnimName.Value);
		return true;
	}

	private void StartAnimation()
	{
		m_vertexAnimation.StartAnimation(m_AnimName.Value);
		m_currentTime = 0f;
	}
}
