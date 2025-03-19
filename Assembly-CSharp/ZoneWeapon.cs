using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneWeapon : Zone
{
	private const float INTERMEDIATE_Y_OFFSET = 1.5f;

	private const float INTERMEDIATE_TRANSITION_SEC = 0.9f;

	private const float DESTROYED_WEAPON_WAIT_SEC = 1.75f;

	private const float FINAL_TRANSITION_SEC = 0.1f;

	private List<Card> m_destroyedWeapons = new List<Card>();

	public override string ToString()
	{
		return $"{base.ToString()} (Weapon)";
	}

	public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		if (!base.CanAcceptTags(controllerId, zoneTag, cardType, entity))
		{
			return false;
		}
		if (cardType != TAG_CARDTYPE.WEAPON)
		{
			return false;
		}
		return true;
	}

	public override int RemoveCard(Card card)
	{
		int num = base.RemoveCard(card);
		if (num >= 0 && !m_destroyedWeapons.Contains(card))
		{
			m_destroyedWeapons.Add(card);
		}
		return num;
	}

	public override void UpdateLayout()
	{
		m_updatingLayout++;
		if (GameState.Get().IsMulliganManagerActive())
		{
			UpdateLayoutFinished();
		}
		else if (IsBlockingLayout())
		{
			UpdateLayoutFinished();
		}
		else if (m_cards.Count == 0)
		{
			m_destroyedWeapons.Clear();
			UpdateLayoutFinished();
		}
		else
		{
			StartCoroutine(UpdateLayoutImpl());
		}
	}

	private IEnumerator UpdateLayoutImpl()
	{
		Card equippedWeapon = GetCardAtIndex(0);
		if (equippedWeapon == null)
		{
			yield break;
		}
		while (equippedWeapon.IsDoNotSort())
		{
			yield return null;
			equippedWeapon = GetCardAtIndex(0);
			if (equippedWeapon == null)
			{
				yield break;
			}
		}
		equippedWeapon.ShowCard();
		equippedWeapon.EnableTransitioningZones(enable: true);
		string tweenName = ZoneMgr.Get()?.GetTweenName<ZoneWeapon>();
		if (tweenName != null && m_Side == Player.Side.OPPOSING)
		{
			iTween.StopOthersByName(equippedWeapon.gameObject, tweenName);
		}
		Vector3 intermediatePosition = base.transform.position;
		intermediatePosition.y += 1.5f;
		Hashtable moveArgs = iTweenManager.Get()?.GetTweenHashTable();
		if (moveArgs == null)
		{
			yield break;
		}
		moveArgs.Add("name", tweenName);
		moveArgs.Add("position", intermediatePosition);
		moveArgs.Add("time", 0.9f);
		iTween.MoveTo(equippedWeapon.gameObject, moveArgs);
		Hashtable rotateArgs = iTweenManager.Get()?.GetTweenHashTable();
		if (rotateArgs == null)
		{
			yield break;
		}
		rotateArgs.Add("name", tweenName);
		rotateArgs.Add("rotation", base.transform.localEulerAngles);
		rotateArgs.Add("time", 0.9f);
		iTween.RotateTo(equippedWeapon.gameObject, rotateArgs);
		Hashtable scaleArgs = iTweenManager.Get()?.GetTweenHashTable();
		if (scaleArgs != null)
		{
			scaleArgs.Add("name", tweenName);
			scaleArgs.Add("scale", base.transform.localScale);
			scaleArgs.Add("time", 0.9f);
			iTween.ScaleTo(equippedWeapon.gameObject, scaleArgs);
			yield return new WaitForSeconds(0.9f);
			if (m_destroyedWeapons.Count > 0)
			{
				yield return new WaitForSeconds(1.75f);
			}
			m_destroyedWeapons.Clear();
			moveArgs = iTweenManager.Get()?.GetTweenHashTable();
			if (moveArgs != null)
			{
				moveArgs.Add("position", base.transform.position);
				moveArgs.Add("time", 0.1f);
				moveArgs.Add("easetype", iTween.EaseType.easeOutCubic);
				moveArgs.Add("name", tweenName);
				iTween.MoveTo(equippedWeapon.gameObject, moveArgs);
				StartFinishLayoutTimer(0.1f);
			}
		}
	}
}
