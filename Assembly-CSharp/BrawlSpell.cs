using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrawlSpell : Spell
{
	public float m_MinJumpHeight = 1.5f;

	public float m_MaxJumpHeight = 2.5f;

	public float m_MinJumpInDelay = 0.1f;

	public float m_MaxJumpInDelay = 0.2f;

	public float m_JumpInDuration = 1.5f;

	public iTween.EaseType m_JumpInEaseType = iTween.EaseType.linear;

	public float m_HoldTime = 0.1f;

	public float m_MinJumpOutDelay = 0.1f;

	public float m_MaxJumpOutDelay = 0.2f;

	public float m_JumpOutDuration = 1.5f;

	public iTween.EaseType m_JumpOutEaseType = iTween.EaseType.easeOutBounce;

	public float m_SurvivorHoldDuration = 0.5f;

	public List<GameObject> m_LeftJumpOutBones;

	public List<GameObject> m_RightJumpOutBones;

	public AudioSource m_JumpInSoundPrefab;

	public float m_JumpInSoundDelay;

	public AudioSource m_JumpOutSoundPrefab;

	public float m_JumpOutSoundDelay;

	public AudioSource m_LandSoundPrefab;

	public float m_LandSoundDelay;

	private int m_jumpsPending;

	private Card m_survivorCard;

	protected override void OnAction(SpellStateType prevStateType)
	{
		if (m_targets.Count > 0)
		{
			m_survivorCard = FindSurvivor();
			StartJumpIns();
		}
		else
		{
			OnSpellFinished();
			OnStateFinished();
		}
	}

	private Card FindSurvivor()
	{
		foreach (GameObject target in m_targets)
		{
			bool isSurvivor = true;
			Card targetCard = target.GetComponent<Card>();
			foreach (PowerTask task in m_taskList.GetTaskList())
			{
				Network.PowerHistory power = task.GetPower();
				if (power.Type != Network.PowerType.TAG_CHANGE)
				{
					continue;
				}
				Network.HistTagChange tagChange = power as Network.HistTagChange;
				if (tagChange.Tag == 360 && tagChange.Value == 1)
				{
					Entity entity = GameState.Get().GetEntity(tagChange.Entity);
					if (entity == null)
					{
						Debug.LogWarning($"{this}.FindSurvivor() - WARNING trying to get entity with id {tagChange.Entity} but there is no entity with that id");
					}
					else if (targetCard == entity.GetCard())
					{
						isSurvivor = false;
						break;
					}
				}
			}
			if (isSurvivor)
			{
				return targetCard;
			}
		}
		return null;
	}

	private void StartJumpIns()
	{
		m_jumpsPending = m_targets.Count;
		List<Card> brawlerCards = new List<Card>(m_jumpsPending);
		foreach (GameObject target in m_targets)
		{
			Card targetCard = target.GetComponent<Card>();
			brawlerCards.Add(targetCard);
		}
		float startSec = 0f;
		while (brawlerCards.Count > 0)
		{
			int index = Random.Range(0, brawlerCards.Count);
			Card brawlerCard = brawlerCards[index];
			brawlerCards.RemoveAt(index);
			StartJumpIn(brawlerCard, ref startSec);
		}
	}

	private void StartJumpIn(Card card, ref float startSec)
	{
		float delaySec = Random.Range(m_MinJumpInDelay, m_MaxJumpInDelay);
		StartCoroutine(JumpIn(card, startSec + delaySec));
		startSec += delaySec;
	}

	private IEnumerator JumpIn(Card card, float delaySec)
	{
		yield return new WaitForSeconds(delaySec);
		Vector3[] path = new Vector3[3];
		path[0] = card.transform.position;
		path[2] = base.transform.position;
		path[1] = 0.5f * (path[0] + path[2]);
		float jumpHeight = Random.Range(m_MinJumpHeight, m_MaxJumpHeight);
		path[1].y += jumpHeight;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("path", path);
		args.Add("orienttopath", true);
		args.Add("time", m_JumpInDuration);
		args.Add("easetype", m_JumpInEaseType);
		args.Add("oncomplete", "OnJumpInComplete");
		args.Add("oncompletetarget", base.gameObject);
		args.Add("oncompleteparams", card);
		iTween.MoveTo(card.gameObject, args);
		if (m_JumpInSoundPrefab != null)
		{
			StartCoroutine(LoadAndPlaySound(m_JumpInSoundPrefab, m_JumpInSoundDelay));
		}
	}

	private void OnJumpInComplete(Card targetCard)
	{
		targetCard.HideCard();
		m_jumpsPending--;
		if (m_jumpsPending <= 0)
		{
			StartCoroutine(Hold());
		}
	}

	private IEnumerator Hold()
	{
		yield return new WaitForSeconds(m_HoldTime);
		StartJumpOuts();
	}

	private void StartJumpOuts()
	{
		m_jumpsPending = m_targets.Count - 1;
		List<int> usedLeftBoneIndexes = new List<int>();
		List<int> usedRightBoneIndexes = new List<int>();
		float startSec = 0f;
		bool leftSide = true;
		for (int i = 0; i < m_targets.Count; i++)
		{
			Card targetCard = m_targets[i].GetComponent<Card>();
			if (targetCard == m_survivorCard)
			{
				continue;
			}
			GameObject bone = null;
			if (leftSide)
			{
				bone = GetFreeBone(m_LeftJumpOutBones, usedLeftBoneIndexes);
				if (bone == null)
				{
					usedLeftBoneIndexes.Clear();
					bone = GetFreeBone(m_LeftJumpOutBones, usedLeftBoneIndexes);
				}
			}
			else
			{
				bone = GetFreeBone(m_RightJumpOutBones, usedRightBoneIndexes);
				if (bone == null)
				{
					usedRightBoneIndexes.Clear();
					bone = GetFreeBone(m_RightJumpOutBones, usedRightBoneIndexes);
				}
			}
			float delaySec = Random.Range(m_MinJumpOutDelay, m_MaxJumpOutDelay);
			StartCoroutine(JumpOut(targetCard, startSec + delaySec, bone.transform.position));
			startSec += delaySec;
			leftSide = !leftSide;
		}
		if (m_jumpsPending <= 0)
		{
			OnJumpOutComplete(null);
		}
	}

	private GameObject GetFreeBone(List<GameObject> boneList, List<int> usedBoneIndexes)
	{
		List<int> freeBoneIndexes = new List<int>();
		for (int j = 0; j < boneList.Count; j++)
		{
			if (!usedBoneIndexes.Contains(j))
			{
				freeBoneIndexes.Add(j);
			}
		}
		if (freeBoneIndexes.Count == 0)
		{
			return null;
		}
		int freeIndex = Random.Range(0, freeBoneIndexes.Count - 1);
		int index = freeBoneIndexes[freeIndex];
		usedBoneIndexes.Add(index);
		return boneList[index];
	}

	private IEnumerator JumpOut(Card card, float delaySec, Vector3 destPos)
	{
		yield return new WaitForSeconds(delaySec);
		card.transform.rotation = Quaternion.identity;
		card.ShowCard();
		Vector3[] path = new Vector3[3];
		path[0] = card.transform.position;
		path[2] = destPos;
		path[1] = 0.5f * (path[0] + path[2]);
		float jumpHeight = Random.Range(m_MinJumpHeight, m_MaxJumpHeight);
		path[1].y += jumpHeight;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("path", path);
		args.Add("time", m_JumpOutDuration);
		args.Add("easetype", m_JumpOutEaseType);
		args.Add("oncomplete", "OnJumpOutComplete");
		args.Add("oncompletetarget", base.gameObject);
		args.Add("oncompleteparams", card);
		iTween.MoveTo(card.gameObject, args);
		if (m_JumpOutSoundPrefab != null)
		{
			StartCoroutine(LoadAndPlaySound(m_JumpOutSoundPrefab, m_JumpOutSoundDelay));
		}
		if (m_LandSoundPrefab != null)
		{
			StartCoroutine(LoadAndPlaySound(m_LandSoundPrefab, m_LandSoundDelay));
		}
	}

	private void OnJumpOutComplete(Card targetCard)
	{
		m_jumpsPending--;
		if (m_jumpsPending <= 0)
		{
			ActivateState(SpellStateType.DEATH);
			StartCoroutine(SurvivorHold());
		}
	}

	private IEnumerator SurvivorHold()
	{
		m_survivorCard.transform.rotation = Quaternion.identity;
		m_survivorCard.ShowCard();
		yield return new WaitForSeconds(m_SurvivorHoldDuration);
		if (IsSurvivorAlone())
		{
			m_survivorCard.GetZone().UpdateLayout();
		}
		OnSpellFinished();
		OnStateFinished();
	}

	private bool IsSurvivorAlone()
	{
		Zone survivorZone = m_survivorCard.GetZone();
		foreach (GameObject target in m_targets)
		{
			Card targetCard = target.GetComponent<Card>();
			if (!(targetCard == m_survivorCard) && targetCard.GetZone() == survivorZone)
			{
				return false;
			}
		}
		return true;
	}

	private IEnumerator LoadAndPlaySound(AudioSource prefab, float delaySec)
	{
		AudioSource source = Object.Instantiate(prefab);
		source.transform.parent = base.transform;
		TransformUtil.Identity(source);
		yield return new WaitForSeconds(delaySec);
		SoundManager.Get().PlayPreloaded(source);
	}
}
