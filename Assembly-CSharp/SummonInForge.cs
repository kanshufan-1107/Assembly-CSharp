using System.Collections;
using UnityEngine;

public class SummonInForge : SpellImpl
{
	public GameObject m_burnIn;

	public GameObject m_blackBits;

	public GameObject m_smokePuff;

	public float m_burnInAnimationSpeed = 1f;

	public bool m_isHeroActor;

	public static string ACTOR_VISIBLE_EVENT = "ActorVisible";

	protected override void OnBirth(SpellStateType prevStateType)
	{
		StartCoroutine(BirthState());
	}

	private IEnumerator BirthState()
	{
		InitActorVariables();
		SetActorVisibility(visible: false, ignoreSpells: true);
		SetVisibility(m_burnIn, visible: true);
		SetAnimationSpeed(m_burnIn, "AllyInHandScryLines_Forge", m_burnInAnimationSpeed);
		PlayAnimation(m_burnIn, "AllyInHandScryLines_Forge", PlayMode.StopAll);
		PlayParticles(m_smokePuff, includeChildren: false);
		PlayParticles(m_blackBits, includeChildren: false);
		yield return new WaitForSeconds(0.2f);
		SetVisibility(m_burnIn, visible: true);
		Renderer particleRenderer = ((m_smokePuff != null) ? m_smokePuff.GetComponent<Renderer>() : null);
		if (particleRenderer != null)
		{
			particleRenderer.enabled = true;
		}
		particleRenderer = ((m_blackBits != null) ? m_blackBits.GetComponent<Renderer>() : null);
		if (particleRenderer != null)
		{
			particleRenderer.enabled = true;
		}
		SetActorVisibility(visible: true, ignoreSpells: true);
		OnSpellEvent(ACTOR_VISIBLE_EVENT, null);
		if (m_isHeroActor)
		{
			GameObject attackObject = GetActorObject("AttackObject");
			GameObject healthObject = GetActorObject("HealthObject");
			SetVisibilityRecursive(attackObject, visible: false);
			SetVisibilityRecursive(healthObject, visible: false);
		}
		yield return new WaitForSeconds(0.2f);
		OnSpellFinished();
	}
}
