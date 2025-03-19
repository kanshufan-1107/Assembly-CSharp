using System.Collections;
using Hearthstone.Core;
using UnityEngine;

public class LegendaryRegisterSlaveAnimator : MonoBehaviour
{
	[InspectorName("Primary Animation Controller")]
	[Tooltip("If this is left empty it will try to find the animation controller from the actor.")]
	public LegendaryHeroAnimController m_animationController;

	[Tooltip("The value to multiply transition speeds by for this animator, if using with a shader controller always set this to 0.")]
	public float m_transitionMultiplier = 1f;

	private ILegendaryHeroPortrait m_heroPortrait;

	private Animator m_animator;

	private void Start()
	{
		m_animator = base.gameObject.GetComponent<Animator>();
		if (!(m_animator == null))
		{
			if (m_animationController == null)
			{
				Processor.RunCoroutine(RegisterSlaveAnimator());
			}
			else
			{
				m_animationController.AddSlaveAnimator(m_animator, m_transitionMultiplier);
			}
		}
	}

	private IEnumerator RegisterSlaveAnimator()
	{
		while (m_heroPortrait == null)
		{
			yield return null;
			m_heroPortrait = LegendaryUtil.FindLegendaryPortrait(base.gameObject);
		}
		m_heroPortrait.AddSlaveAnimator(m_animator, m_transitionMultiplier);
	}

	private void OnDestroy()
	{
		if (m_animationController != null)
		{
			m_animationController.RemoveSlaveAnimator(m_animator);
		}
		else if (m_animator != null && m_heroPortrait != null)
		{
			m_heroPortrait.RemoveSlaveAnimator(m_animator);
		}
	}
}
