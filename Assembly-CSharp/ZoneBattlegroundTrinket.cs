using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneBattlegroundTrinket : Zone
{
	public int slot = 1;

	private const float DESTROYED_TRINKETS_WAIT_SEC = 0.5f;

	private const float FINAL_TRANSITION_SEC = 0.1f;

	private List<Card> m_destroyedTrinkets = new List<Card>();

	public override string ToString()
	{
		return $"{base.ToString()} (Battleground Trinket)";
	}

	public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		if (!base.CanAcceptTags(controllerId, zoneTag, cardType, entity))
		{
			return false;
		}
		if (cardType != TAG_CARDTYPE.BATTLEGROUND_TRINKET)
		{
			return false;
		}
		if (entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_6) != slot)
		{
			return false;
		}
		return true;
	}

	public override int RemoveCard(Card card)
	{
		int num = base.RemoveCard(card);
		if (num >= 0 && !m_destroyedTrinkets.Contains(card))
		{
			m_destroyedTrinkets.Add(card);
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
			m_destroyedTrinkets.Clear();
			UpdateLayoutFinished();
		}
		else
		{
			StartCoroutine(UpdateLayoutImpl());
		}
	}

	private IEnumerator UpdateLayoutImpl()
	{
		Card activeBgTrinket = m_cards[0];
		while (activeBgTrinket.IsDoNotSort())
		{
			yield return null;
		}
		activeBgTrinket.ShowCard();
		activeBgTrinket.EnableTransitioningZones(enable: true);
		string tweenName = ZoneMgr.Get().GetTweenName<ZoneBattlegroundTrinket>();
		if (m_Side == Player.Side.OPPOSING)
		{
			iTween.StopOthersByName(activeBgTrinket.gameObject, tweenName);
		}
		if (m_destroyedTrinkets.Count > 0)
		{
			yield return new WaitForSeconds(0.5f);
		}
		m_destroyedTrinkets.Clear();
		iTweenManager.Get().GetTweenHashTable();
		Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
		moveArgs.Add("position", base.transform.position);
		moveArgs.Add("time", 0.1f);
		moveArgs.Add("easetype", iTween.EaseType.easeOutCubic);
		moveArgs.Add("name", tweenName);
		iTween.MoveTo(activeBgTrinket.gameObject, moveArgs);
		StartFinishLayoutTimer(0.1f);
	}
}
