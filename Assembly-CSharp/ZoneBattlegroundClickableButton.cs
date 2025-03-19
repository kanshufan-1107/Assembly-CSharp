using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneBattlegroundClickableButton : Zone
{
	private const float INTERMEDIATE_Y_OFFSET = 1.5f;

	private const float INTERMEDIATE_TRANSITION_SEC = 0.9f;

	private const float DESTROYED_BG_CLICKABLE_BUTTON_WAIT_SEC = 1.75f;

	private const float FINAL_TRANSITION_SEC = 0.1f;

	private List<Card> m_destroyedBGClickableButton = new List<Card>();

	public override string ToString()
	{
		return $"{base.ToString()} (Battleground Clickable Button)";
	}

	public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		if (!base.CanAcceptTags(controllerId, zoneTag, cardType, entity))
		{
			return false;
		}
		if (cardType != TAG_CARDTYPE.BATTLEGROUND_CLICKABLE_BUTTON)
		{
			return false;
		}
		return true;
	}

	public override int RemoveCard(Card card)
	{
		int num = base.RemoveCard(card);
		if (num >= 0 && !m_destroyedBGClickableButton.Contains(card))
		{
			m_destroyedBGClickableButton.Add(card);
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
			m_destroyedBGClickableButton.Clear();
			UpdateLayoutFinished();
		}
		else
		{
			StartCoroutine(UpdateLayoutImpl());
		}
	}

	private IEnumerator UpdateLayoutImpl()
	{
		Card activeBgClickableButton = m_cards[0];
		while (activeBgClickableButton.IsDoNotSort())
		{
			yield return null;
		}
		activeBgClickableButton.ShowCard();
		activeBgClickableButton.EnableTransitioningZones(enable: true);
		string tweenName = ZoneMgr.Get().GetTweenName<ZoneBattlegroundClickableButton>();
		if (m_Side == Player.Side.OPPOSING)
		{
			iTween.StopOthersByName(activeBgClickableButton.gameObject, tweenName);
		}
		Vector3 intermediatePosition = base.transform.position;
		intermediatePosition.y += 1.5f;
		Hashtable moveArgs = iTweenManager.Get().GetTweenHashTable();
		moveArgs.Add("name", tweenName);
		moveArgs.Add("position", intermediatePosition);
		moveArgs.Add("time", 0.9f);
		iTween.MoveTo(activeBgClickableButton.gameObject, moveArgs);
		Hashtable rotateArgs = iTweenManager.Get().GetTweenHashTable();
		rotateArgs.Add("name", tweenName);
		rotateArgs.Add("rotation", base.transform.localEulerAngles);
		rotateArgs.Add("time", 0.9f);
		iTween.RotateTo(activeBgClickableButton.gameObject, rotateArgs);
		Hashtable scaleArgs = iTweenManager.Get().GetTweenHashTable();
		scaleArgs.Add("name", tweenName);
		scaleArgs.Add("scale", base.transform.localScale);
		scaleArgs.Add("time", 0.9f);
		iTween.ScaleTo(activeBgClickableButton.gameObject, scaleArgs);
		yield return new WaitForSeconds(0.9f);
		if (m_destroyedBGClickableButton.Count > 0)
		{
			yield return new WaitForSeconds(1.75f);
		}
		m_destroyedBGClickableButton.Clear();
		moveArgs = iTweenManager.Get().GetTweenHashTable();
		moveArgs.Add("position", base.transform.position);
		moveArgs.Add("time", 0.1f);
		moveArgs.Add("easetype", iTween.EaseType.easeOutCubic);
		moveArgs.Add("name", tweenName);
		iTween.MoveTo(activeBgClickableButton.gameObject, moveArgs);
		StartFinishLayoutTimer(0.1f);
	}
}
