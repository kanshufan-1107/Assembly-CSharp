using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MagneticBeamSpell : Spell
{
	private LineRenderer m_lineRenderer;

	protected override void Awake()
	{
		base.Awake();
		m_lineRenderer = GetComponent<LineRenderer>();
		m_lineRenderer.enabled = false;
	}

	protected override void OnBirth(SpellStateType prevStateType)
	{
		base.OnBirth(prevStateType);
		m_lineRenderer.enabled = true;
		StartCoroutine(DoUpdate());
	}

	private IEnumerator DoUpdate()
	{
		while (true)
		{
			if (Source == null || m_targets.Count == 0 || m_targets[0] == null)
			{
				if (m_lineRenderer.enabled)
				{
					m_lineRenderer.enabled = false;
				}
			}
			else
			{
				if (!m_lineRenderer.enabled)
				{
					m_lineRenderer.enabled = true;
				}
				m_lineRenderer.SetPosition(0, Source.transform.position);
				m_lineRenderer.SetPosition(1, m_targets[0].transform.position);
			}
			yield return null;
		}
	}

	protected override void OnDeath(SpellStateType prevStateType)
	{
		base.OnDeath(prevStateType);
		m_lineRenderer.enabled = false;
		StopAllCoroutines();
	}
}
