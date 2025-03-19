using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneBattlegroundHeroBuddy : Zone
{
	private const float DESTROYED_HERO_BUDDY_WAIT_SEC = 0.5f;

	private const float FINAL_TRANSITION_SEC = 0.1f;

	private List<Card> m_destroyedHeroBuddies = new List<Card>();

	public override string ToString()
	{
		return $"{base.ToString()} (Battleground hero Buddy)";
	}

	public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		if (!base.CanAcceptTags(controllerId, zoneTag, cardType, entity))
		{
			return false;
		}
		if (cardType != TAG_CARDTYPE.BATTLEGROUND_HERO_BUDDY)
		{
			return false;
		}
		return true;
	}

	public override int RemoveCard(Card card)
	{
		int num = base.RemoveCard(card);
		if (num >= 0 && !m_destroyedHeroBuddies.Contains(card))
		{
			m_destroyedHeroBuddies.Add(card);
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
			m_destroyedHeroBuddies.Clear();
			UpdateLayoutFinished();
		}
		else
		{
			StartCoroutine(UpdateLayoutImpl());
		}
	}

	private IEnumerator UpdateLayoutImpl()
	{
		Card activeBgHeroBuddy = m_cards[0];
		while (activeBgHeroBuddy.IsDoNotSort())
		{
			yield return null;
		}
		activeBgHeroBuddy.ShowCard();
		activeBgHeroBuddy.EnableTransitioningZones(enable: true);
		string tweenName = ZoneMgr.Get().GetTweenName<ZoneBattlegroundHeroBuddy>();
		if (m_Side == Player.Side.OPPOSING)
		{
			iTween.StopOthersByName(activeBgHeroBuddy.gameObject, tweenName);
		}
		if (m_destroyedHeroBuddies.Count > 0)
		{
			yield return new WaitForSeconds(0.5f);
		}
		m_destroyedHeroBuddies.Clear();
		iTweenManager.Get().GetTweenHashTable();
		Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
		moveArgs.Add("position", base.transform.position);
		moveArgs.Add("time", 0.1f);
		moveArgs.Add("easetype", iTween.EaseType.easeOutCubic);
		moveArgs.Add("name", tweenName);
		iTween.MoveTo(activeBgHeroBuddy.gameObject, moveArgs);
		StartFinishLayoutTimer(0.1f);
	}
}
