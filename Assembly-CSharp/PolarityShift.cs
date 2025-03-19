using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarityShift : SuperSpell
{
	public class MinionData
	{
		public GameObject gameObject;

		public Vector3 orgLocPos;

		public Quaternion orgLocRot;

		public Vector3 rotationDrift;

		public ParticleSystem glowParticle;

		public ParticleSystem lightningParticle;

		public ParticleSystem impactParticle;
	}

	public AnimationCurve m_HeightCurve;

	public float m_RotationDriftAmount;

	public AnimationCurve m_RotationDriftCurve;

	public float m_ParticleHeightOffset = 0.1f;

	public ParticleSystem m_GlowParticle;

	public ParticleSystem m_LightningParticle;

	public ParticleSystem m_ImpactParticle;

	public ParticleEffects m_ParticleEffects;

	public float m_CleanupTime = 2f;

	public float m_SpellFinishTime = 2f;

	private float m_HeightCurveLength;

	private float m_AnimTime;

	private AudioSource m_Sound;

	protected override void Awake()
	{
		m_Sound = GetComponent<AudioSource>();
		base.Awake();
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		if (m_HeightCurve.length == 0)
		{
			Debug.LogWarning("PolarityShift Spell height animation curve in not defined");
			base.OnAction(prevStateType);
			return;
		}
		if (m_RotationDriftCurve.length == 0)
		{
			Debug.LogWarning("PolarityShift Spell rotation drift animation curve in not defined");
			base.OnAction(prevStateType);
			return;
		}
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		m_HeightCurveLength = m_HeightCurve[m_HeightCurve.length - 1].time;
		m_ParticleEffects.m_ParticleSystems.Clear();
		List<MinionData> minions = new List<MinionData>();
		foreach (GameObject target in GetTargets())
		{
			MinionData data = new MinionData();
			data.gameObject = target;
			data.orgLocPos = target.transform.localPosition;
			data.orgLocRot = target.transform.localRotation;
			float rotDriftX = Mathf.Lerp(0f - m_RotationDriftAmount, m_RotationDriftAmount, Random.value);
			float rotDriftY = Mathf.Lerp(0f - m_RotationDriftAmount, m_RotationDriftAmount, Random.value) * 0.1f;
			float rotDriftZ = Mathf.Lerp(0f - m_RotationDriftAmount, m_RotationDriftAmount, Random.value);
			data.rotationDrift = new Vector3(rotDriftX, rotDriftY, rotDriftZ);
			data.glowParticle = Object.Instantiate(m_GlowParticle);
			data.glowParticle.transform.position = target.transform.position;
			data.glowParticle.transform.Translate(0f, m_ParticleHeightOffset, 0f, Space.World);
			data.lightningParticle = Object.Instantiate(m_LightningParticle);
			data.lightningParticle.transform.position = target.transform.position;
			data.lightningParticle.transform.Translate(0f, m_ParticleHeightOffset, 0f, Space.World);
			data.impactParticle = Object.Instantiate(m_ImpactParticle);
			data.impactParticle.transform.position = target.transform.position;
			data.impactParticle.transform.Translate(0f, m_ParticleHeightOffset, 0f, Space.World);
			m_ParticleEffects.m_ParticleSystems.Add(data.lightningParticle);
			if (m_Sound != null)
			{
				SoundManager.Get().Play(m_Sound);
			}
			minions.Add(data);
		}
		StartCoroutine(DoSpellFinished());
		StartCoroutine(MinionAnimation(minions));
	}

	private IEnumerator DoSpellFinished()
	{
		yield return new WaitForSeconds(m_SpellFinishTime);
		m_effectsPendingFinish--;
		FinishIfPossible();
	}

	private IEnumerator MinionAnimation(List<MinionData> minions)
	{
		foreach (MinionData minion3 in minions)
		{
			minion3.glowParticle.Play();
		}
		m_AnimTime = 0f;
		while (m_AnimTime < m_HeightCurveLength)
		{
			m_AnimTime += Time.deltaTime;
			float height = m_HeightCurve.Evaluate(m_AnimTime);
			float rotAmount = m_RotationDriftCurve.Evaluate(m_AnimTime);
			foreach (MinionData minion in minions)
			{
				minion.gameObject.transform.localPosition = new Vector3(minion.orgLocPos.x, minion.orgLocPos.y + height, minion.orgLocPos.z);
				minion.gameObject.transform.localRotation = minion.orgLocRot;
				minion.gameObject.transform.Rotate(minion.rotationDrift * rotAmount, Space.Self);
			}
			yield return null;
		}
		foreach (MinionData minion2 in minions)
		{
			minion2.impactParticle.Play();
			minion2.lightningParticle.Play();
			MinionShake.ShakeObject(minion2.gameObject, ShakeMinionType.RandomDirection, minion2.gameObject.transform.position, ShakeMinionIntensity.MediumShake, 0f, 0f, 0f);
		}
		if (minions.Count > 0)
		{
			ShakeCamera();
		}
		if (minions.Count > 0)
		{
			yield return new WaitForSeconds(m_CleanupTime);
			m_ParticleEffects.m_ParticleSystems.Clear();
			foreach (MinionData minion4 in minions)
			{
				Object.Destroy(minion4.glowParticle.gameObject);
				Object.Destroy(minion4.lightningParticle.gameObject);
				Object.Destroy(minion4.impactParticle.gameObject);
			}
		}
		OnStateFinished();
	}

	private void ShakeCamera()
	{
		CameraShakeMgr.Shake(Camera.main, new Vector3(0.1f, 0.1f, 0.1f), 0.75f);
	}
}
