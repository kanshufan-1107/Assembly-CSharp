using System.Collections;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class DeckCardBarSummonIn : SpellImpl
{
	public GameObject m_echoQuad;

	public GameObject m_fxEvaporate;

	private void OnDisable()
	{
		if (m_echoQuad != null)
		{
			m_echoQuad.GetComponent<Renderer>().GetMaterial().color = Color.clear;
		}
		if (m_fxEvaporate != null)
		{
			m_fxEvaporate.GetComponent<ParticleSystem>().Clear();
		}
	}

	protected override void OnBirth(SpellStateType prevStateType)
	{
		StartCoroutine(BirthState());
	}

	private IEnumerator BirthState()
	{
		InitActorVariables();
		GameObject frame = GetActorObject("Frame");
		SetVisibilityRecursive(frame, visible: false);
		SetVisibility(m_echoQuad, visible: true);
		SetVisibilityRecursive(frame, visible: true);
		PlayParticles(m_fxEvaporate, includeChildren: false);
		SetAnimationSpeed(m_echoQuad, "Secret_AbilityEchoFade", 0.5f);
		PlayAnimation(m_echoQuad, "Secret_AbilityEchoFade", PlayMode.StopAll);
		yield return new WaitForSeconds(1f);
		OnSpellFinished();
		SetVisibility(m_echoQuad, visible: false);
	}
}
