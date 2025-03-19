using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnomalusDeathrattleSpell : Spell
{
	public Spell m_CustomDeathSpell;

	public float m_DelayBeforeStart = 1f;

	public float m_DelayDistanceModifier = 1f;

	public float m_RiseTime = 0.5f;

	public float m_HangTime = 1f;

	public float m_LiftHeightMin = 2f;

	public float m_LiftHeightMax = 3f;

	public float m_LiftRotMin = -15f;

	public float m_LiftRotMax = 15f;

	public float m_SlamTime = 0.15f;

	public float m_Bounceness = 0.2f;

	public float m_DelayAfterSpellFinish = 3f;

	private GameObject[] m_TargetActorGameObjects;

	private Actor[] m_TargetActors;

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		List<Card> cardTargetList = new List<Card>();
		List<Entity> entityTargetList = new List<Entity>();
		foreach (GameObject target in GetVisualTargets())
		{
			if (!(target == null))
			{
				Card targetCard = target.GetComponent<Card>();
				cardTargetList.Add(targetCard);
				entityTargetList.Add(targetCard.GetEntity());
			}
		}
		List<Entity> killedEntities = GameUtils.GetEntitiesKilledBySourceAmongstTargets(GetSourceCard().GetEntity().GetEntityId(), entityTargetList);
		foreach (Card targetCard2 in cardTargetList)
		{
			if (killedEntities.Exists((Entity killedEntity) => killedEntity.GetEntityId() == targetCard2.GetEntity().GetEntityId()))
			{
				targetCard2.OverrideCustomDeathSpell(SpellManager.Get().GetSpell(m_CustomDeathSpell));
			}
		}
		StartCoroutine(AnimateMinions());
	}

	private IEnumerator AnimateMinions()
	{
		if (Source == null)
		{
			yield break;
		}
		yield return new WaitForSeconds(m_DelayBeforeStart);
		float lastLiftTime = 0f;
		OnSpellFinished();
		m_TargetActorGameObjects = new GameObject[m_targets.Count];
		m_TargetActors = new Actor[m_targets.Count];
		for (int i = 0; i < m_targets.Count; i++)
		{
			GameObject targetCardGO = m_targets[i];
			if (targetCardGO == null)
			{
				continue;
			}
			Card targetCard = targetCardGO.GetComponent<Card>();
			if (targetCard == null)
			{
				continue;
			}
			Actor targetActor = targetCard.GetActor();
			if (targetActor == null)
			{
				continue;
			}
			m_TargetActors[i] = targetActor;
			GameObject target = targetActor.gameObject;
			if (!(target == null))
			{
				m_TargetActorGameObjects[i] = target;
				_ = target.transform.localPosition;
				Quaternion OrgRot = target.transform.localRotation;
				float dist = Vector3.Distance(Source.transform.position, target.transform.position);
				float liftTime = dist * m_DelayDistanceModifier;
				if (lastLiftTime < liftTime)
				{
					lastLiftTime = liftTime;
				}
				float randomHeight = Random.Range(m_LiftHeightMin, m_LiftHeightMax);
				Hashtable liftArgs = iTweenManager.Get().GetTweenHashTable();
				liftArgs.Add("time", m_RiseTime);
				liftArgs.Add("delay", dist * m_DelayDistanceModifier);
				liftArgs.Add("position", new Vector3(0f, randomHeight, 0f));
				liftArgs.Add("easetype", iTween.EaseType.easeOutExpo);
				liftArgs.Add("islocal", true);
				liftArgs.Add("name", $"Lift_{target.name}_{i}");
				iTween.MoveTo(target, liftArgs);
				Vector3 newRot = OrgRot.eulerAngles;
				newRot.x += Random.Range(m_LiftRotMin, m_LiftRotMax);
				newRot.z += Random.Range(m_LiftRotMin, m_LiftRotMax);
				Hashtable liftRotArgs = iTweenManager.Get().GetTweenHashTable();
				liftRotArgs.Add("time", m_RiseTime + m_HangTime);
				liftRotArgs.Add("delay", dist * m_DelayDistanceModifier);
				liftRotArgs.Add("rotation", newRot);
				liftRotArgs.Add("easetype", iTween.EaseType.easeOutQuad);
				liftRotArgs.Add("islocal", true);
				liftRotArgs.Add("name", $"LiftRot_{target.name}_{i}");
				iTween.RotateTo(target, liftRotArgs);
			}
		}
		yield return new WaitForSeconds(lastLiftTime);
		for (int j = 0; j < m_targets.Count; j++)
		{
			GameObject target2 = m_TargetActorGameObjects[j];
			if (target2 == null)
			{
				continue;
			}
			GameObject targetCardGO2 = m_targets[j];
			if (targetCardGO2 == null)
			{
				continue;
			}
			Card targetCard2 = targetCardGO2.GetComponent<Card>();
			if (targetCard2 == null)
			{
				continue;
			}
			if (targetCard2.GetZone().m_ServerTag == TAG_ZONE.GRAVEYARD)
			{
				Actor targetActor2 = m_TargetActors[j];
				if (targetActor2 == null)
				{
					continue;
				}
				targetActor2.DoCardDeathVisuals();
			}
			float slamDelay = 0f;
			Hashtable slamPosArgs = iTweenManager.Get().GetTweenHashTable();
			slamPosArgs.Add("time", m_SlamTime);
			slamPosArgs.Add("delay", m_DelayAfterSpellFinish + slamDelay);
			slamPosArgs.Add("position", Vector3.zero);
			slamPosArgs.Add("easetype", iTween.EaseType.easeInCubic);
			slamPosArgs.Add("islocal", true);
			slamPosArgs.Add("name", $"SlamPos_{target2.name}_{j}");
			iTween.MoveTo(target2, slamPosArgs);
			Hashtable slamRotArgs = iTweenManager.Get().GetTweenHashTable();
			slamRotArgs.Add("time", m_SlamTime * 0.8f);
			slamRotArgs.Add("delay", m_DelayAfterSpellFinish + slamDelay + m_SlamTime * 0.2f);
			slamRotArgs.Add("rotation", Vector3.zero);
			slamRotArgs.Add("easetype", iTween.EaseType.easeInQuad);
			slamRotArgs.Add("islocal", true);
			slamRotArgs.Add("name", $"SlamRot_{target2.name}_{j}");
			iTween.RotateTo(target2, slamRotArgs);
		}
	}
}
