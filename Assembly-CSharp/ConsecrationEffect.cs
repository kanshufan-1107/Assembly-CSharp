using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Game.Spells.SuperSpells;
using UnityEngine;

public class ConsecrationEffect : MonoBehaviour
{
	public float m_StartDelayMin = 2f;

	public float m_StartDelayMax = 3f;

	public float m_LiftTime = 1f;

	public float m_LiftHeightMin = 2f;

	public float m_LiftHeightMax = 3f;

	public float m_LiftRotMin = -15f;

	public float m_LiftRotMax = 15f;

	public float m_HoverTime = 0.8f;

	public float m_SlamTime = 0.2f;

	public float m_Bounceness = 0.2f;

	public GameObject m_StartImpact;

	public GameObject m_EndImpact;

	public float m_TotalTime;

	private ISuperSpell m_SuperSpell;

	private List<GameObject> m_ImpactObjects;

	private AudioSource m_ImpactSound;

	private void Awake()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_LiftHeightMin = 3f;
			m_LiftHeightMax = 5f;
		}
	}

	private void Start()
	{
		Spell spell = GetComponent<Spell>();
		if (spell == null)
		{
			base.enabled = false;
			return;
		}
		m_SuperSpell = spell.GetSuperSpellParent();
		m_ImpactSound = GetComponent<AudioSource>();
		m_ImpactObjects = new List<GameObject>();
	}

	private void OnDestroy()
	{
		if (m_ImpactObjects.Count <= 0)
		{
			return;
		}
		foreach (GameObject impactObject in m_ImpactObjects)
		{
			Object.Destroy(impactObject);
		}
	}

	private void StartAnimation()
	{
		if (m_SuperSpell == null)
		{
			return;
		}
		int index = 0;
		foreach (GameObject target in m_SuperSpell.GetTargets())
		{
			Vector3 OrgPos = target.transform.position;
			Quaternion OrgRot = target.transform.rotation;
			index++;
			float randomDelay = Random.Range(m_StartDelayMin, m_StartDelayMax);
			GameObject startImpact = Object.Instantiate(m_StartImpact, OrgPos, OrgRot);
			m_ImpactObjects.Add(startImpact);
			ParticleSystem[] componentsInChildren = startImpact.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem obj in componentsInChildren)
			{
				ParticleSystem.MainModule particleMain = obj.main;
				particleMain.startDelay = randomDelay;
				obj.Play();
			}
			randomDelay += 0.2f;
			float randomHeight = Random.Range(m_LiftHeightMin, m_LiftHeightMax);
			Hashtable liftArgs = iTweenManager.Get().GetTweenHashTable();
			liftArgs.Add("time", m_LiftTime);
			liftArgs.Add("delay", randomDelay);
			liftArgs.Add("position", new Vector3(OrgPos.x, OrgPos.y + randomHeight, OrgPos.z));
			liftArgs.Add("easetype", iTween.EaseType.easeOutQuad);
			liftArgs.Add("name", $"Lift_{target.name}_{index}");
			iTween.MoveTo(target, liftArgs);
			Vector3 newRot = OrgRot.eulerAngles;
			newRot.x += Random.Range(m_LiftRotMin, m_LiftRotMax);
			newRot.z += Random.Range(m_LiftRotMin, m_LiftRotMax);
			Hashtable liftRotArgs = iTweenManager.Get().GetTweenHashTable();
			liftRotArgs.Add("time", m_LiftTime + m_HoverTime + m_SlamTime * 0.8f);
			liftRotArgs.Add("delay", randomDelay);
			liftRotArgs.Add("rotation", newRot);
			liftRotArgs.Add("easetype", iTween.EaseType.easeOutQuad);
			liftRotArgs.Add("name", $"LiftRot_{target.name}_{index}");
			iTween.RotateTo(target, liftRotArgs);
			float slamDelay = m_StartDelayMax + m_LiftTime + m_HoverTime;
			Hashtable slamPosArgs = iTweenManager.Get().GetTweenHashTable();
			slamPosArgs.Add("time", m_SlamTime);
			slamPosArgs.Add("delay", slamDelay);
			slamPosArgs.Add("position", OrgPos);
			slamPosArgs.Add("easetype", iTween.EaseType.easeInCubic);
			slamPosArgs.Add("name", $"SlamPos_{target.name}_{index}");
			iTween.MoveTo(target, slamPosArgs);
			Hashtable slamRotArgs = iTweenManager.Get().GetTweenHashTable();
			slamRotArgs.Add("time", m_SlamTime * 0.8f);
			slamRotArgs.Add("delay", slamDelay + m_SlamTime * 0.2f);
			slamRotArgs.Add("rotation", Vector3.zero);
			slamRotArgs.Add("easetype", iTween.EaseType.easeInQuad);
			slamRotArgs.Add("oncomplete", "Finished");
			slamRotArgs.Add("oncompletetarget", base.gameObject);
			slamRotArgs.Add("name", $"SlamRot_{target.name}_{index}");
			iTween.RotateTo(target, slamRotArgs);
			m_TotalTime = slamDelay + m_SlamTime;
			if ((bool)target.GetComponentInChildren<MinionShake>())
			{
				MinionShake.ShakeObject(target, ShakeMinionType.RandomDirection, target.transform.position, ShakeMinionIntensity.LargeShake, 1f, 0.1f, slamDelay + m_SlamTime, ignoreAnimationPlaying: true, ignoreHeight: true);
			}
			else
			{
				Bounce bounce = target.GetComponent<Bounce>();
				if (bounce == null)
				{
					bounce = target.AddComponent<Bounce>();
				}
				bounce.m_BounceAmount = randomHeight * m_Bounceness;
				bounce.m_BounceSpeed = 3.5f * Random.Range(0.8f, 1.3f);
				bounce.m_BounceCount = 3;
				bounce.m_Bounceness = m_Bounceness;
				bounce.m_Delay = slamDelay + m_SlamTime;
				bounce.StartAnimation();
			}
			GameObject endImpact = Object.Instantiate(m_EndImpact, OrgPos, OrgRot);
			m_ImpactObjects.Add(endImpact);
			componentsInChildren = endImpact.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem obj2 in componentsInChildren)
			{
				ParticleSystem.MainModule paticleMain = obj2.main;
				paticleMain.startDelay = slamDelay + m_SlamTime;
				obj2.Play();
			}
		}
	}

	private void Finished()
	{
		SoundManager.Get().Play(m_ImpactSound);
		CameraShakeMgr.Shake(Camera.main, new Vector3(0.15f, 0.15f, 0.15f), 0.9f);
	}
}
