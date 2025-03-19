using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSeeds : SuperSpell
{
	public class MinionData
	{
		public GameObject gameObject;

		public Vector3 orgLocPos;

		public Quaternion orgLocRot;

		public Vector3 rotationDrift;

		public Card card;
	}

	private enum SpellTargetType
	{
		None,
		Death,
		Create
	}

	public Spell m_CustomSpawnSpell;

	public Spell m_CustomDeathSpell;

	public float m_StartDeathSpellAdjustment = 0.01f;

	public AnimationCurve m_HeightCurve;

	public float m_RotationDriftAmount;

	public AnimationCurve m_RotationDriftCurve;

	public ParticleSystem m_ImpactParticles;

	public ParticleSystem m_DustParticles;

	private SpellTargetType m_TargetType;

	private float m_HeightCurveLength;

	private float m_AnimTime;

	private AudioSource m_Sound;

	protected override void Awake()
	{
		m_Sound = GetComponent<AudioSource>();
		base.Awake();
	}

	public override bool AddPowerTargets()
	{
		m_visualToTargetIndexMap.Clear();
		m_targetToMetaDataMap.Clear();
		m_targets.Clear();
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			PowerTask task = tasks[i];
			Card card = GetTargetCardFromPowerTask(i, task);
			if (!(card == null))
			{
				m_targets.Add(card.gameObject);
			}
		}
		if (m_targets.Count > 0)
		{
			return true;
		}
		return false;
	}

	protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
	{
		int entityID = 0;
		Network.PowerHistory power = task.GetPower();
		if (power.Type == Network.PowerType.FULL_ENTITY)
		{
			m_TargetType = SpellTargetType.Create;
			entityID = (power as Network.HistFullEntity).Entity.ID;
		}
		else
		{
			if (!(power is Network.HistTagChange { Tag: 360, Value: >0 } tagChange))
			{
				return null;
			}
			m_TargetType = SpellTargetType.Death;
			entityID = tagChange.Entity;
		}
		Entity entity = GameState.Get().GetEntity(entityID);
		if (entity == null)
		{
			Debug.LogWarning($"{this}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {entityID} but there is no entity with that id");
			return null;
		}
		return entity.GetCard();
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		if (m_TargetType == SpellTargetType.Death)
		{
			DeathEffect();
			return;
		}
		if (m_TargetType == SpellTargetType.Create)
		{
			StartCoroutine(CreateEffect());
			return;
		}
		m_effectsPendingFinish--;
		FinishIfPossible();
	}

	private IEnumerator CreateEffect()
	{
		foreach (GameObject target in GetTargets())
		{
			if (target == null)
			{
				continue;
			}
			Card targetCard = target.GetComponent<Card>();
			if (!(targetCard == null))
			{
				targetCard.OverrideCustomSpawnSpell(SpellManager.Get().GetSpell(m_CustomSpawnSpell));
				ZonePlay zone = (ZonePlay)targetCard.GetZone();
				if (!(zone == null))
				{
					zone.SetTransitionTime(0.01f);
				}
			}
		}
		m_effectsPendingFinish--;
		FinishIfPossible();
		ShakeCamera();
		yield return new WaitForSeconds(1f);
		foreach (GameObject target2 in GetTargets())
		{
			Card card = target2.GetComponent<Card>();
			if (!(card == null))
			{
				ZonePlay zone2 = (ZonePlay)card.GetZone();
				if (!(zone2 == null))
				{
					zone2.ResetTransitionTime();
				}
			}
		}
	}

	private void DeathEffect()
	{
		if (m_HeightCurve.length == 0)
		{
			Debug.LogWarning("PoisonSeeds Spell height animation curve in not defined");
			return;
		}
		if (m_RotationDriftCurve.length == 0)
		{
			Debug.LogWarning("PoisonSeeds Spell rotation drift animation curve in not defined");
			return;
		}
		if (m_CustomDeathSpell != null)
		{
			foreach (GameObject target in GetTargets())
			{
				if (!(target == null))
				{
					target.GetComponent<Card>().OverrideCustomDeathSpell(SpellManager.Get().GetSpell(m_CustomDeathSpell));
				}
			}
		}
		m_HeightCurveLength = m_HeightCurve[m_HeightCurve.length - 1].time;
		List<MinionData> minions = new List<MinionData>();
		foreach (GameObject target2 in GetTargets())
		{
			MinionData data = new MinionData();
			data.card = target2.GetComponent<Card>();
			data.gameObject = target2;
			data.orgLocPos = target2.transform.localPosition;
			data.orgLocRot = target2.transform.localRotation;
			float rotDriftX = Mathf.Lerp(0f - m_RotationDriftAmount, m_RotationDriftAmount, Random.value);
			float rotDriftY = Mathf.Lerp(0f - m_RotationDriftAmount, m_RotationDriftAmount, Random.value) * 0.1f;
			float rotDriftZ = Mathf.Lerp(0f - m_RotationDriftAmount, m_RotationDriftAmount, Random.value);
			data.rotationDrift = new Vector3(rotDriftX, rotDriftY, rotDriftZ);
			minions.Add(data);
		}
		StartCoroutine(AnimateDeathEffect(minions));
	}

	private IEnumerator AnimateDeathEffect(List<MinionData> minions)
	{
		if (m_Sound != null)
		{
			SoundManager.Get().Play(m_Sound);
		}
		List<ParticleSystem> impactParticles = new List<ParticleSystem>();
		foreach (MinionData minion in minions)
		{
			GameObject newImpactParticle = Object.Instantiate(m_ImpactParticles.gameObject);
			newImpactParticle.transform.parent = base.transform;
			newImpactParticle.transform.position = minion.gameObject.transform.position;
			impactParticles.Add(newImpactParticle.GetComponentInChildren<ParticleSystem>());
			GameObject obj = Object.Instantiate(m_DustParticles.gameObject);
			obj.transform.parent = base.transform;
			obj.transform.position = minion.gameObject.transform.position;
			obj.GetComponent<ParticleSystem>().Play();
		}
		m_AnimTime = 0f;
		bool finished = false;
		while (m_AnimTime < m_HeightCurveLength)
		{
			m_AnimTime += Time.deltaTime;
			float height = m_HeightCurve.Evaluate(m_AnimTime);
			float rotAmount = m_RotationDriftCurve.Evaluate(m_AnimTime);
			foreach (MinionData minion2 in minions)
			{
				minion2.gameObject.transform.localPosition = new Vector3(minion2.orgLocPos.x, minion2.orgLocPos.y + height, minion2.orgLocPos.z);
				minion2.gameObject.transform.localRotation = minion2.orgLocRot;
				minion2.gameObject.transform.Rotate(minion2.rotationDrift * rotAmount, Space.Self);
			}
			if (m_AnimTime > m_HeightCurveLength - m_StartDeathSpellAdjustment && !finished)
			{
				foreach (MinionData minion3 in minions)
				{
					if (minion3 != null && minion3.card != null && minion3.card.GetActor() != null)
					{
						minion3.card.GetActor().DoCardDeathVisuals();
					}
				}
				m_effectsPendingFinish--;
				FinishIfPossible();
				finished = true;
			}
			yield return null;
		}
		foreach (ParticleSystem item in impactParticles)
		{
			item.Play();
		}
		ShakeCamera();
	}

	private void ShakeCamera()
	{
		CameraShakeMgr.Shake(Camera.main, new Vector3(0.15f, 0.15f, 0.15f), 0.9f);
	}
}
