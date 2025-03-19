using System.Collections;
using UnityEngine;

[CustomEditClass]
public class ChessAttackAnimation : Spell
{
	public GameObject m_ChessShockwaveRed;

	public GameObject m_ChessShockwaveBlue;

	public GameObject m_ChessTrailRed;

	public GameObject m_ChessTrailBlue;

	public GameObject m_ChessImpactRed;

	public GameObject m_ChessImpactBlue;

	public GameObject m_ChessSettleDust;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_ShowAttackSoundPrefab;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_ShowImpactSoundPrefab;

	public float m_ImpactEffectDelay = 0.3f;

	public float m_SpellFinishDelay = 0.15f;

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		StartCoroutine(AttackAnimation());
	}

	private void Finish()
	{
		OnSpellFinished();
		OnStateFinished();
	}

	private IEnumerator AttackAnimation()
	{
		if (m_targets.Count == 0)
		{
			Finish();
			yield break;
		}
		string tweenLabel = ZoneMgr.Get().GetTweenName<ZonePlay>();
		while (iTween.CountByName(GetSourceCard().gameObject, tweenLabel) > 0)
		{
			yield return null;
		}
		GameObject source = GetSourceCard().gameObject;
		Vector3 sourceOrgPos = source.transform.position;
		Vector3 sourceOrgRot = source.transform.eulerAngles;
		Vector3 sourceOrgScale = source.transform.localScale;
		GameObject targetA = m_targets[0].gameObject;
		Vector3 targetAPos = targetA.transform.position;
		GameObject dustFX = Object.Instantiate(m_ChessSettleDust);
		Vector3 scaleMax = new Vector3(source.transform.localScale.x * 1.2f, source.transform.localScale.y * 1.2f, source.transform.localScale.z * 1.2f);
		float zWindup = ((source.transform.position.z > targetA.transform.position.z) ? Random.Range(0.65f, 0.85f) : Random.Range(-0.65f, -0.85f));
		float zOverreach = ((source.transform.position.z > targetA.transform.position.z) ? (-0.1f) : 0.1f);
		Vector3 targetFacing = ((sourceOrgPos.z > targetAPos.z) ? new Vector3(sourceOrgRot.x - 15f, sourceOrgRot.y, sourceOrgRot.z) : new Vector3(sourceOrgRot.x + 15f, sourceOrgRot.y, sourceOrgRot.z));
		iTween.MoveTo(source, iTween.Hash("position", new Vector3(sourceOrgPos.x, sourceOrgPos.y + 1f, sourceOrgPos.z + zWindup), "easetype", iTween.EaseType.easeOutQuart, "time", 0.15f));
		iTween.ScaleTo(source, iTween.Hash("scale", scaleMax, "time", 0.2f));
		StartCoroutine(DoSpellFinished());
		iTween.RotateTo(source, iTween.Hash("rotation", targetFacing, "easetype", iTween.EaseType.easeOutQuart, "time", 0.1f, "delay", 0.15f));
		iTween.MoveTo(source, iTween.Hash("position", new Vector3(sourceOrgPos.x, sourceOrgPos.y + 0.1f, sourceOrgPos.z + zOverreach), "easetype", iTween.EaseType.easeOutQuart, "time", 0.05f, "delay", 0.25f));
		iTween.RotateTo(source, iTween.Hash("rotation", sourceOrgRot, "easetype", iTween.EaseType.easeOutQuart, "time", 0.05f, "delay", 0.25f));
		iTween.ScaleTo(source, iTween.Hash("scale", sourceOrgScale, "time", 0.05f, "delay", 0.25f));
		iTween.MoveTo(source, iTween.Hash("position", sourceOrgPos, "easetype", iTween.EaseType.easeOutQuart, "time", 0.3f, "delay", 0.25f));
		dustFX.transform.parent = base.transform;
		dustFX.transform.position = new Vector3(sourceOrgPos.x, sourceOrgPos.y + 1f, sourceOrgPos.z);
		dustFX.GetComponent<ParticleSystem>().Play();
		yield return new WaitForSeconds(m_ImpactEffectDelay);
		if (!string.IsNullOrEmpty(m_ShowAttackSoundPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_ShowAttackSoundPrefab);
		}
		yield return new WaitForSeconds(0.3f);
	}

	private IEnumerator PlayImpactEffects()
	{
		if (m_targets.Count == 0)
		{
			Finish();
			yield break;
		}
		GameObject source = GetSourceCard().gameObject;
		GameObject targetA = m_targets[0].gameObject;
		Vector3 targetAPos = targetA.transform.position;
		Vector3 targetARot = targetA.transform.eulerAngles;
		GameObject shockwaveFX = ((source.transform.position.z > targetA.transform.position.z) ? Object.Instantiate(m_ChessShockwaveRed) : Object.Instantiate(m_ChessShockwaveBlue));
		ParticleSystem shockwaveParticles = shockwaveFX.GetComponent<ParticleSystem>();
		ParticleSystem.MainModule shockwaveFXMain = shockwaveParticles.main;
		GameObject impactFX = ((source.transform.position.z > targetA.transform.position.z) ? Object.Instantiate(m_ChessImpactBlue) : Object.Instantiate(m_ChessImpactRed));
		ParticleSystem impactParticles = impactFX.GetComponent<ParticleSystem>();
		ParticleSystem.MainModule impactFXMain = impactParticles.main;
		float shockwaveFXOffset = ((source.transform.position.z > targetA.transform.position.z) ? 0.25f : (-0.25f));
		float impactDelay = 0.15f;
		bool targettingKing = ((m_targets.Count == 1 && (targetA.transform.position.z < -7f || targetA.transform.position.z > -2f)) ? true : false);
		float shockwaveFXRotation = ((m_targets.Count == 2 && source.transform.position.z > targetA.transform.position.z) ? 0f : ((m_targets.Count == 1 && source.transform.position.z > targetA.transform.position.z && Mathf.Abs(source.transform.position.x) - Mathf.Abs(targetA.transform.position.x) < -0.5f) ? 0.523599f : ((m_targets.Count == 1 && source.transform.position.z > targetA.transform.position.z && Mathf.Abs(source.transform.position.x) - Mathf.Abs(targetA.transform.position.x) > 0.5f) ? (-0.523599f) : ((m_targets.Count == 1 && source.transform.position.z < targetA.transform.position.z && Mathf.Abs(source.transform.position.x) - Mathf.Abs(targetA.transform.position.x) < -0.5f) ? 2.61799f : ((m_targets.Count == 1 && source.transform.position.z < targetA.transform.position.z && Mathf.Abs(source.transform.position.x) - Mathf.Abs(targetA.transform.position.x) > 0.5f) ? 3.66519f : ((m_targets.Count != 1 || !(source.transform.position.z > targetA.transform.position.z)) ? 3.14159f : 0f))))));
		shockwaveFX.transform.parent = base.transform;
		impactFX.transform.parent = base.transform;
		shockwaveFX.transform.position = new Vector3(source.transform.position.x, source.transform.position.y + 0.5f, source.transform.position.z - shockwaveFXOffset);
		shockwaveFXMain.startRotation = shockwaveFXRotation;
		if (m_targets.Count == 2)
		{
			shockwaveFXMain.startSize = 4f;
			iTween.MoveTo(shockwaveFX, iTween.Hash("position", new Vector3(source.transform.position.x, targetA.transform.position.y + 0.5f, targetA.transform.position.z + shockwaveFXOffset), "time", 0.4f));
			shockwaveParticles.Play();
		}
		else if (targettingKing)
		{
			GameObject shockwaveTrail = ((source.transform.position.z > targetA.transform.position.z) ? Object.Instantiate(m_ChessTrailRed) : Object.Instantiate(m_ChessTrailBlue));
			shockwaveTrail.transform.parent = base.transform;
			shockwaveTrail.transform.position = new Vector3(source.transform.position.x, source.transform.position.y + 0.1f, source.transform.position.z);
			impactDelay = 0.5f;
			float midpointX = (source.transform.position.x + targetA.transform.position.x) * 0.5f;
			float midpointZ = ((targetA.transform.position.z > -4f) ? (-2.4f) : (-6.4f));
			if (source.transform.position.x + targetA.transform.position.x < -17.5f || source.transform.position.x + targetA.transform.position.x > -12.5f)
			{
				iTween.MoveTo(shockwaveTrail, iTween.Hash("path", new Vector3[2]
				{
					new Vector3(midpointX, targetA.transform.position.y + 2f, midpointZ),
					new Vector3(targetA.transform.position.x, targetA.transform.position.y + 0.1f, targetA.transform.position.z)
				}, "easetype", iTween.EaseType.easeOutQuad, "time", 0.4f));
			}
			else
			{
				impactDelay = 0.4f;
				iTween.MoveTo(shockwaveTrail, iTween.Hash("position", new Vector3(targetA.transform.position.x, targetA.transform.position.y + 0.5f, targetA.transform.position.z), "time", 0.3f));
			}
		}
		else
		{
			iTween.MoveTo(shockwaveFX, iTween.Hash("position", new Vector3(targetA.transform.position.x, targetA.transform.position.y + 0.5f, targetA.transform.position.z + shockwaveFXOffset), "time", 0.4f));
			shockwaveParticles.Play();
		}
		impactFX.transform.position = new Vector3(targetA.transform.position.x, targetA.transform.position.y + 1f, targetA.transform.position.z);
		impactFXMain.startDelay = impactDelay;
		impactParticles.Play();
		if (!targettingKing)
		{
			ShakeMinion(targetA, targetAPos, targetARot);
		}
		if (m_targets.Count == 2)
		{
			GameObject targetB = m_targets[1].gameObject;
			Vector3 targetBPos = targetB.transform.position;
			Vector3 targetBRot = targetB.transform.eulerAngles;
			GameObject obj = ((source.transform.position.z > targetB.transform.position.z) ? Object.Instantiate(m_ChessImpactBlue) : Object.Instantiate(m_ChessImpactRed));
			obj.transform.parent = base.transform;
			obj.transform.position = new Vector3(targetB.transform.position.x, targetB.transform.position.y + 1f, targetB.transform.position.z);
			obj.GetComponent<ParticleSystem>().Play();
			ShakeMinion(targetB, targetBPos, targetBRot);
		}
		yield return new WaitForSeconds(impactDelay);
		if (!string.IsNullOrEmpty(m_ShowImpactSoundPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_ShowImpactSoundPrefab);
		}
	}

	private void ShakeMinion(GameObject target, Vector3 targetOrgPos, Vector3 targetOrgRot)
	{
		iTween.MoveTo(target, iTween.Hash("position", new Vector3(targetOrgPos.x, targetOrgPos.y + 0.15f, targetOrgPos.z), "time", 0.05f, "islocal", true));
		iTween.RotateTo(target, iTween.Hash("rotation", new Vector3(Random.Range(-15f, 15f), Random.Range(-15f, 15f), Random.Range(-15f, 15f)), "time", 0.08f, "islocal", true));
		iTween.RotateTo(target, iTween.Hash("rotation", new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f)), "time", 0.08f, "islocal", true, "delay", 0.08f));
		iTween.RotateTo(target, iTween.Hash("rotation", new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)), "time", 0.08f, "islocal", true, "delay", 0.16f));
		iTween.MoveTo(target, iTween.Hash("position", targetOrgPos, "time", 0.08f, "islocal", true, "delay", 0.24f));
		iTween.RotateTo(target, iTween.Hash("rotation", targetOrgRot, "time", 0.08f, "islocal", true, "delay", 0.24f));
	}

	private IEnumerator DoSpellFinished()
	{
		if (m_targets.Count == 0)
		{
			Finish();
			yield break;
		}
		GameObject source = GetSourceCard().gameObject;
		GameObject targetA = m_targets[0].gameObject;
		bool useSpellFinishDelay = false;
		if (m_targets.Count == 1 && (targetA.transform.position.z < -7f || targetA.transform.position.z > -2f) && (source.transform.position.x + targetA.transform.position.x < -17.5f || source.transform.position.x + targetA.transform.position.x > -12.5f))
		{
			useSpellFinishDelay = true;
		}
		yield return new WaitForSeconds(m_ImpactEffectDelay);
		StartCoroutine(PlayImpactEffects());
		if (useSpellFinishDelay)
		{
			yield return new WaitForSeconds(m_SpellFinishDelay);
		}
		foreach (GameObject target in m_targets)
		{
			GameUtils.DoDamageTasks(m_taskList, GetSourceCard(), target.GetComponentInChildren<Card>());
		}
		foreach (GameObject target2 in m_targets)
		{
			while (iTween.HasTween(target2))
			{
				yield return null;
			}
		}
		while (iTween.HasTween(source))
		{
			yield return null;
		}
		OnSpellFinished();
		yield return new WaitForSeconds(1f);
		OnStateFinished();
	}
}
