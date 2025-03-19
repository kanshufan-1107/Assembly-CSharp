using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class Misdirection : Spell
{
	public float m_ReticleFadeInTime = 0.8f;

	public float m_ReticleFadeOutTime = 0.4f;

	public float m_ReticlePathTime = 3f;

	public float m_ReticleBlur = 0.005f;

	public float m_ReticleBlurFocusTime = 0.8f;

	public Color m_ReticleAttackColor = Color.red;

	public float m_ReticleAttackScale = 1.1f;

	public float m_ReticleAttackTime = 0.3f;

	public Vector3 m_ReticleAttackRotate = new Vector3(0f, 90f, 0f);

	public float m_ReticleAttackHold = 0.25f;

	public GameObject m_Reticle;

	public bool m_AllowTargetingInitialTarget;

	public int m_ReticlePathDesiredMinimumTargets = 3;

	public int m_ReticlePathDesiredMaximumTargets = 4;

	private GameObject m_ReticleInstance;

	private Material m_ReticleMaterial;

	private Card m_AttackingEntityCard;

	private Card m_InitialTargetCard;

	private Color m_OrgAmbient;

	public override bool AddPowerTargets()
	{
		if (!CanAddPowerTargets())
		{
			return false;
		}
		AddMultiplePowerTargets();
		return true;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		StartAnimation();
	}

	private void ResolveTargets()
	{
		List<GameObject> spellTargetList = GetTargets();
		if (spellTargetList.Count < 3)
		{
			return;
		}
		m_AttackingEntityCard = spellTargetList[1].GetComponent<Card>();
		GameState state = GameState.Get();
		GameEntity gameEntity = state.GetGameEntity();
		Entity proposedDefenderEntity = state.GetEntity(gameEntity.GetTag(GAME_TAG.PROPOSED_DEFENDER));
		if (proposedDefenderEntity != null)
		{
			m_InitialTargetCard = proposedDefenderEntity.GetCard();
			return;
		}
		Entity powerTargetEntity = state.GetEntity(m_AttackingEntityCard.GetEntity().GetTag(GAME_TAG.CARD_TARGET));
		if (powerTargetEntity != null)
		{
			m_InitialTargetCard = powerTargetEntity.GetCard();
		}
		else
		{
			m_InitialTargetCard = spellTargetList[2].GetComponent<Card>();
		}
	}

	private void StartAnimation()
	{
		ResolveTargets();
		if (m_InitialTargetCard == null)
		{
			OnSpellFinished();
			return;
		}
		m_ReticleInstance = UnityEngine.Object.Instantiate(m_Reticle, m_InitialTargetCard.transform.position, Quaternion.identity);
		m_ReticleMaterial = m_ReticleInstance.GetComponentInChildren<MeshRenderer>().GetMaterial();
		m_ReticleMaterial.SetFloat("_Alpha", 0f);
		m_ReticleMaterial.SetFloat("_blur", m_ReticleBlur);
		StartCoroutine(ReticleFadeIn());
		StartCoroutine(AnimateReticle());
		AudioSource sound = GetComponent<AudioSource>();
		if (sound != null)
		{
			SoundManager.Get().Play(sound);
		}
	}

	private IEnumerator ReticleFadeIn()
	{
		Action<object> reticleFadeInUpdate = delegate(object amount)
		{
			m_ReticleMaterial.SetFloat("_Alpha", (float)amount);
		};
		Hashtable reticleFadeInArgs = iTweenManager.Get().GetTweenHashTable();
		reticleFadeInArgs.Add("time", m_ReticleFadeInTime);
		reticleFadeInArgs.Add("from", 0f);
		reticleFadeInArgs.Add("to", 1f);
		reticleFadeInArgs.Add("onupdate", reticleFadeInUpdate);
		reticleFadeInArgs.Add("onupdatetarget", m_ReticleInstance.gameObject);
		iTween.ValueTo(m_ReticleInstance.gameObject, reticleFadeInArgs);
		Hashtable reticleAttackScaleArgs = iTweenManager.Get().GetTweenHashTable();
		reticleAttackScaleArgs.Add("time", m_ReticleFadeInTime);
		reticleAttackScaleArgs.Add("scale", Vector3.one);
		reticleAttackScaleArgs.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.ScaleTo(m_ReticleInstance.gameObject, reticleAttackScaleArgs);
		yield break;
	}

	private void SetReticleAlphaValue(float val)
	{
		m_ReticleMaterial.SetFloat("_Alpha", val);
	}

	private IEnumerator AnimateReticle()
	{
		yield return new WaitForSeconds(m_ReticleFadeInTime);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("path", BuildAnimationPath());
		args.Add("time", m_ReticlePathTime);
		args.Add("easetype", iTween.EaseType.easeInOutQuad);
		args.Add("oncomplete", "ReticleAnimationComplete");
		args.Add("oncompletetarget", base.gameObject);
		args.Add("orienttopath", false);
		iTween.MoveTo(m_ReticleInstance, args);
	}

	private void ReticleAnimationComplete()
	{
		StartCoroutine(ReticleAttackAnimation());
	}

	private IEnumerator ReticleAttackAnimation()
	{
		Action<object> reticleAttackColorUpdate = delegate(object col)
		{
			if (m_ReticleInstance != null)
			{
				m_ReticleMaterial.SetColor("_Color", (Color)col);
			}
		};
		Hashtable reticleAttackColorArgs = iTweenManager.Get().GetTweenHashTable();
		reticleAttackColorArgs.Add("time", m_ReticleAttackTime);
		reticleAttackColorArgs.Add("from", m_ReticleMaterial.color);
		reticleAttackColorArgs.Add("to", m_ReticleAttackColor);
		reticleAttackColorArgs.Add("onupdate", reticleAttackColorUpdate);
		reticleAttackColorArgs.Add("onupdatetarget", base.gameObject);
		iTween.ValueTo(base.gameObject, reticleAttackColorArgs);
		Hashtable reticleAttackScaleArgs = iTweenManager.Get().GetTweenHashTable();
		reticleAttackScaleArgs.Add("time", m_ReticleAttackTime);
		reticleAttackScaleArgs.Add("scale", m_ReticleAttackScale);
		reticleAttackScaleArgs.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.ScaleTo(m_ReticleInstance, reticleAttackScaleArgs);
		Hashtable reticleAttackRotArgs = iTweenManager.Get().GetTweenHashTable();
		reticleAttackRotArgs.Add("time", m_ReticleAttackTime);
		reticleAttackRotArgs.Add("rotation", m_ReticleAttackRotate);
		reticleAttackRotArgs.Add("easetype", iTween.EaseType.easeOutBounce);
		iTween.RotateTo(m_ReticleInstance, reticleAttackRotArgs);
		Action<object> reticleFocusUpdate = delegate(object amount)
		{
			if (m_ReticleInstance != null)
			{
				m_ReticleMaterial.SetFloat("_Blur", (float)amount);
			}
		};
		Hashtable reticleFocusArgs = iTweenManager.Get().GetTweenHashTable();
		reticleFocusArgs.Add("time", m_ReticleBlurFocusTime);
		reticleFocusArgs.Add("from", m_ReticleBlur);
		reticleFocusArgs.Add("to", 0f);
		reticleFocusArgs.Add("onupdate", reticleFocusUpdate);
		reticleFocusArgs.Add("onupdatetarget", base.gameObject);
		iTween.ValueTo(base.gameObject, reticleFocusArgs);
		yield return new WaitForSeconds(m_ReticleBlurFocusTime + m_ReticleAttackHold);
		StartCoroutine(ReticleFadeOut());
	}

	private IEnumerator ReticleFadeOut()
	{
		OnSpellFinished();
		Action<object> reticleFadeOutUpdate = delegate(object amount)
		{
			if (m_ReticleInstance != null)
			{
				m_ReticleMaterial.SetFloat("_Alpha", (float)amount);
			}
		};
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("time", m_ReticleFadeOutTime);
		args.Add("from", 1f);
		args.Add("to", 0f);
		args.Add("onupdate", reticleFadeOutUpdate);
		args.Add("onupdatetarget", base.gameObject);
		iTween.ValueTo(base.gameObject, args);
		yield return new WaitForSeconds(m_ReticleFadeOutTime);
		UnityEngine.Object.Destroy(m_ReticleInstance);
	}

	private Vector3[] BuildAnimationPath()
	{
		Card[] possibleTargetCards = FindPossibleTargetCards();
		int pathPointCount = UnityEngine.Random.Range(m_ReticlePathDesiredMinimumTargets, m_ReticlePathDesiredMaximumTargets);
		if (pathPointCount >= possibleTargetCards.Length + 2)
		{
			pathPointCount = possibleTargetCards.Length + 2;
		}
		if (possibleTargetCards.Length <= 1)
		{
			return new Vector3[2]
			{
				m_InitialTargetCard.transform.position,
				GetTarget().transform.position
			};
		}
		List<Vector3> path = new List<Vector3>();
		path.Add(m_InitialTargetCard.transform.position);
		GameObject previousTarget = m_InitialTargetCard.gameObject;
		for (int idx = 1; idx < pathPointCount; idx++)
		{
			GameObject randomTarget = possibleTargetCards[UnityEngine.Random.Range(0, possibleTargetCards.Length - 1)].gameObject;
			if (randomTarget == previousTarget)
			{
				randomTarget = possibleTargetCards[UnityEngine.Random.Range(0, possibleTargetCards.Length - 1)].gameObject;
				if (randomTarget == previousTarget)
				{
					randomTarget = ((!(randomTarget == possibleTargetCards[possibleTargetCards.Length - 1])) ? possibleTargetCards[possibleTargetCards.Length - 1].gameObject : possibleTargetCards[0].gameObject);
				}
			}
			if (idx == pathPointCount - 1 && randomTarget == GetTarget() && randomTarget == previousTarget)
			{
				randomTarget = ((!(randomTarget == possibleTargetCards[possibleTargetCards.Length - 1])) ? possibleTargetCards[possibleTargetCards.Length - 1].gameObject : possibleTargetCards[0].gameObject);
			}
			path.Add(randomTarget.transform.position);
		}
		path.Add(GetTarget().transform.position);
		return path.ToArray();
	}

	private Card[] FindPossibleTargetCards()
	{
		List<Card> targetCards = new List<Card>();
		ZoneMgr zoneManager = ZoneMgr.Get();
		if (zoneManager == null)
		{
			return targetCards.ToArray();
		}
		foreach (ZonePlay item in zoneManager.FindZonesOfType<ZonePlay>())
		{
			foreach (Card card in item.GetCards())
			{
				if (!(card == m_AttackingEntityCard) && (!(card == m_InitialTargetCard) || m_AllowTargetingInitialTarget))
				{
					targetCards.Add(card);
				}
			}
		}
		foreach (ZoneHero item2 in zoneManager.FindZonesOfType<ZoneHero>())
		{
			foreach (Card card2 in item2.GetCards())
			{
				if (!(card2 == m_AttackingEntityCard) && (!(card2 == m_InitialTargetCard) || m_AllowTargetingInitialTarget))
				{
					targetCards.Add(card2);
				}
			}
		}
		return targetCards.ToArray();
	}

	private Card[] GetOpponentZoneMinions()
	{
		List<Card> targetCards = new List<Card>();
		foreach (Card card in GameState.Get().GetFirstOpponentPlayer(GetSourceCard().GetController()).GetBattlefieldZone()
			.GetCards())
		{
			if (!(card == m_AttackingEntityCard))
			{
				targetCards.Add(card);
			}
		}
		return targetCards.ToArray();
	}

	private Card GetCurrentPlayerHeroCard()
	{
		return GetSourceCard().GetController().GetHeroCard();
	}

	private Card GetOpponentHeroCard()
	{
		return GameState.Get().GetFirstOpponentPlayer(GetSourceCard().GetController()).GetHeroCard();
	}
}
