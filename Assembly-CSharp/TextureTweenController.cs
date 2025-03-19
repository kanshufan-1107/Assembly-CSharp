using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class TextureTweenController : MonoBehaviour
{
	public float ForwardTransitionDuration = 0.5f;

	public AnimationCurve ForwardTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float ReverseTransitionDuration = 0.5f;

	public AnimationCurve ReverseTransitionCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	public List<Renderer> AffectedRenderers = new List<Renderer>();

	private IEnumerator m_currentTween;

	public void StartForwardTransition()
	{
		StopCurrentTransition();
		m_currentTween = RunTransition(ForwardTransitionCurve, ForwardTransitionDuration);
		StartCoroutine(m_currentTween);
	}

	public void StartReverseTransition()
	{
		StopCurrentTransition();
		if (base.gameObject.activeSelf)
		{
			m_currentTween = RunTransition(ReverseTransitionCurve, ReverseTransitionDuration);
			StartCoroutine(m_currentTween);
		}
	}

	public void StopCurrentTransition()
	{
		if (m_currentTween != null)
		{
			StopCoroutine(m_currentTween);
			m_currentTween = null;
		}
	}

	private IEnumerator RunTransition(AnimationCurve transitionCurve, float duration)
	{
		List<Material> affectedMaterials = new List<Material>();
		for (int materialIdx = 0; materialIdx < AffectedRenderers.Count; materialIdx++)
		{
			Material affectedMaterial;
			if (AffectedRenderers[materialIdx] != null && (affectedMaterial = AffectedRenderers[materialIdx].GetMaterial()) != null)
			{
				affectedMaterials.Add(affectedMaterial);
			}
		}
		float startTime = Time.time;
		float elapsedTime = 0f;
		while (elapsedTime < duration && duration > 0f)
		{
			elapsedTime = Time.time - startTime;
			float elapsedFraction = elapsedTime / duration;
			float transitionFraction = transitionCurve.Evaluate(elapsedFraction);
			for (int i = 0; i < affectedMaterials.Count; i++)
			{
				affectedMaterials[i].SetFloat("_Transistion", transitionFraction);
			}
			yield return null;
		}
		float completedFraction = transitionCurve.Evaluate(1f);
		for (int j = 0; j < affectedMaterials.Count; j++)
		{
			affectedMaterials[j].SetFloat("_Transistion", completedFraction);
		}
		m_currentTween = null;
	}
}
