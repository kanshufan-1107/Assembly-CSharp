using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[RequireComponent(typeof(UberCurve))]
[RequireComponent(typeof(LineRenderer))]
public class NetherspiteBeamSpell : Spell
{
	[Range(1f, 1000f)]
	public int m_fullPathPolys = 50;

	[Range(1f, 1000f)]
	public int m_blockedPathPolys = 5;

	public bool m_targetMinionToRight = true;

	public List<Vector3> m_sourceCardOffsets;

	public List<Vector3> m_destCardOffsets;

	public List<Vector3> m_fullPathPoints;

	public bool m_visualizeControlPoints;

	public string m_beamFadeInMaterialVar = "";

	private int m_beamFadeInPropertyID;

	public float m_beamFadeInTime = 1f;

	public Spell m_beamSourceSpell;

	public Spell m_beamTargetMinionSpell;

	public Spell m_beamTargetHeroSpell;

	public ParticleSystem m_fullPathParticles;

	public ParticleSystem m_blockedPathParticles;

	private bool m_usingFullPath;

	private Actor m_targetActor;

	private Spell m_beamTargetSpellInstance;

	private Spell m_beamSourceSpellInstance;

	private UberCurve m_uberCurve;

	private LineRenderer m_lineRenderer;

	private Material m_beamMaterial;

	private List<GameObject> m_visualizers = new List<GameObject>();

	protected override void Awake()
	{
		base.Awake();
		m_uberCurve = GetComponent<UberCurve>();
		m_lineRenderer = GetComponent<LineRenderer>();
		m_beamMaterial = m_lineRenderer.GetMaterial();
		if (!string.IsNullOrEmpty(m_beamFadeInMaterialVar))
		{
			m_beamFadeInPropertyID = Shader.PropertyToID(m_beamFadeInMaterialVar);
		}
	}

	protected override void OnBirth(SpellStateType prevStateType)
	{
		base.OnBirth(prevStateType);
		if (m_beamSourceSpell != null)
		{
			m_beamSourceSpellInstance = SpellManager.Get().GetSpell(m_beamSourceSpell);
			m_beamSourceSpellInstance.AddStateFinishedCallback(OnSpellStateFinished);
			m_beamSourceSpellInstance.transform.parent = GetSourceCard().GetActor().transform;
			TransformUtil.Identity(m_beamSourceSpellInstance);
			m_beamSourceSpellInstance.Activate();
		}
		if (m_fullPathParticles != null)
		{
			m_fullPathParticles.transform.parent = GetSourceCard().GetActor().transform;
			TransformUtil.Identity(m_fullPathParticles);
		}
		if (m_blockedPathParticles != null)
		{
			m_blockedPathParticles.transform.parent = GetSourceCard().GetActor().transform;
			TransformUtil.Identity(m_blockedPathParticles);
		}
		StartCoroutine("DoUpdate");
	}

	protected override void OnDeath(SpellStateType prevStateType)
	{
		base.OnDeath(prevStateType);
		if (m_beamSourceSpellInstance != null)
		{
			m_beamSourceSpellInstance.ActivateState(SpellStateType.DEATH);
		}
		if (m_fullPathParticles != null)
		{
			m_fullPathParticles.Stop();
			m_fullPathParticles.Clear();
		}
		if (m_blockedPathParticles != null)
		{
			m_blockedPathParticles.Stop();
			m_blockedPathParticles.Clear();
		}
		StopCoroutine("DoUpdate");
	}

	private IEnumerator DoUpdate()
	{
		while (true)
		{
			Actor currentTargetActor = GetTargetMinion();
			int numPathPolys;
			if (currentTargetActor == null)
			{
				m_usingFullPath = true;
				currentTargetActor = SpellUtils.FindOpponentPlayer(this).GetHeroCard().GetActor();
				numPathPolys = m_fullPathPolys;
				UpdateFullPathControlPoints();
			}
			else
			{
				m_usingFullPath = false;
				numPathPolys = m_blockedPathPolys;
				UpdateBlockedPathControlPoints(currentTargetActor);
			}
			if (currentTargetActor != m_targetActor)
			{
				if (m_beamTargetSpellInstance != null)
				{
					m_beamTargetSpellInstance.ActivateState(SpellStateType.DEATH);
				}
				if (m_usingFullPath)
				{
					if (!string.IsNullOrEmpty(m_beamFadeInMaterialVar))
					{
						iTween.StopByName(base.gameObject, "fadeBeam");
						UpdateBeamFade(0f);
						Hashtable args = iTweenManager.Get().GetTweenHashTable();
						args.Add("from", 0f);
						args.Add("to", 1f);
						args.Add("time", m_beamFadeInTime);
						args.Add("easetype", iTween.EaseType.linear);
						args.Add("onupdate", "UpdateBeamFade");
						args.Add("onupdatetarget", base.gameObject);
						args.Add("name", "fadeBeam");
						iTween.ValueTo(base.gameObject, args);
					}
					if (m_fullPathParticles != null)
					{
						m_fullPathParticles.Play();
					}
					if (m_blockedPathParticles != null)
					{
						m_blockedPathParticles.Stop();
						m_blockedPathParticles.Clear();
					}
				}
				else
				{
					if (m_fullPathParticles != null)
					{
						m_fullPathParticles.Stop();
						m_fullPathParticles.Clear();
					}
					if (m_blockedPathParticles != null)
					{
						m_blockedPathParticles.Play();
					}
				}
				m_targetActor = currentTargetActor;
				if (m_targetActor != null)
				{
					Spell beamTargetSpell = ((m_targetActor.GetEntity().GetCardType() == TAG_CARDTYPE.HERO) ? m_beamTargetHeroSpell : m_beamTargetMinionSpell);
					if (beamTargetSpell != null)
					{
						m_beamTargetSpellInstance = SpellManager.Get().GetSpell(beamTargetSpell);
						m_beamTargetSpellInstance.AddStateFinishedCallback(OnSpellStateFinished);
						m_beamTargetSpellInstance.transform.parent = m_targetActor.transform;
						TransformUtil.Identity(m_beamTargetSpellInstance);
						m_beamTargetSpellInstance.Activate();
					}
					else
					{
						m_beamTargetSpellInstance = null;
					}
				}
			}
			m_lineRenderer.positionCount = numPathPolys;
			for (int i = 0; i < numPathPolys; i++)
			{
				float t = (float)i / (float)numPathPolys;
				m_lineRenderer.SetPosition(i, m_uberCurve.CatmullRomEvaluateWorldPosition(t));
			}
			VisualizeControlPoints();
			yield return null;
		}
	}

	private void UpdateBeamFade(float fadeValue)
	{
		Color beamColor = m_beamMaterial.GetColor(m_beamFadeInPropertyID);
		beamColor.a = fadeValue;
		m_beamMaterial.SetColor(m_beamFadeInPropertyID, beamColor);
	}

	private void UpdateBlockedPathControlPoints(Actor minionToRight)
	{
		int totalPoints = m_sourceCardOffsets.Count + m_destCardOffsets.Count;
		if (m_uberCurve.m_controlPoints.Count != totalPoints)
		{
			m_uberCurve.m_controlPoints.Clear();
			for (int i = 0; i < totalPoints; i++)
			{
				m_uberCurve.m_controlPoints.Add(new UberCurve.UberCurveControlPoint());
			}
		}
		int index = 0;
		Card sourceCard = GetSourceCard();
		int i2 = 0;
		while (i2 < m_sourceCardOffsets.Count)
		{
			m_uberCurve.m_controlPoints[index].position = sourceCard.transform.position + m_sourceCardOffsets[i2];
			i2++;
			index++;
		}
		int i3 = 0;
		while (i3 < m_destCardOffsets.Count)
		{
			m_uberCurve.m_controlPoints[index].position = minionToRight.transform.position + m_destCardOffsets[i3];
			i3++;
			index++;
		}
	}

	private void UpdateFullPathControlPoints()
	{
		int totalPoints = m_sourceCardOffsets.Count + m_fullPathPoints.Count;
		if (m_uberCurve.m_controlPoints.Count != totalPoints)
		{
			m_uberCurve.m_controlPoints.Clear();
			for (int i = 0; i < totalPoints; i++)
			{
				m_uberCurve.m_controlPoints.Add(new UberCurve.UberCurveControlPoint());
			}
		}
		int index = 0;
		Card sourceCard = GetSourceCard();
		int i2 = 0;
		while (i2 < m_sourceCardOffsets.Count)
		{
			m_uberCurve.m_controlPoints[index].position = sourceCard.transform.position + m_sourceCardOffsets[i2];
			i2++;
			index++;
		}
		int i3 = 0;
		while (i3 < m_fullPathPoints.Count)
		{
			m_uberCurve.m_controlPoints[index].position = m_fullPathPoints[i3];
			i3++;
			index++;
		}
	}

	private Actor GetTargetMinion()
	{
		int zonePos = GetSourceCard().GetZonePosition();
		ZonePlay playZone = GetSourceCard().GetController().GetBattlefieldZone();
		for (int targetZonePos = (m_targetMinionToRight ? (zonePos + 1) : (zonePos - 1)); targetZonePos > 0 && targetZonePos <= playZone.GetCardCount(); targetZonePos += (m_targetMinionToRight ? 1 : (-1)))
		{
			Card card = playZone.GetCardAtSlot(targetZonePos);
			if (card.IsActorReady())
			{
				return card.GetActor();
			}
		}
		return null;
	}

	private void VisualizeControlPoints()
	{
		if (!m_visualizeControlPoints)
		{
			foreach (GameObject visualizer2 in m_visualizers)
			{
				Object.Destroy(visualizer2);
			}
			m_visualizers.Clear();
		}
		else if (m_visualizers.Count != m_uberCurve.m_controlPoints.Count)
		{
			foreach (GameObject visualizer3 in m_visualizers)
			{
				Object.Destroy(visualizer3);
			}
			m_visualizers.Clear();
			for (int i = 0; i < m_uberCurve.m_controlPoints.Count; i++)
			{
				GameObject visualizer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				visualizer.transform.position = m_uberCurve.m_controlPoints[i].position;
				visualizer.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				m_visualizers.Add(visualizer);
			}
		}
		else
		{
			for (int j = 0; j < m_uberCurve.m_controlPoints.Count; j++)
			{
				m_visualizers[j].transform.position = m_uberCurve.transform.TransformPoint(m_uberCurve.m_controlPoints[j].position);
			}
		}
	}

	private void OnSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			SpellManager.Get().ReleaseSpell(spell);
		}
	}
}
