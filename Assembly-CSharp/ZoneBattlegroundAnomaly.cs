using System.Collections;

public class ZoneBattlegroundAnomaly : Zone
{
	private const float INTERMEDIATE_TRANSITION_SEC = 0.9f;

	private const float FINAL_TRANSITION_SEC = 0.1f;

	public override string ToString()
	{
		return $"{base.ToString()} (Battleground Anomaly)";
	}

	public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		if (!base.CanAcceptTags(controllerId, zoneTag, cardType, entity))
		{
			return false;
		}
		if (cardType != TAG_CARDTYPE.BATTLEGROUND_ANOMALY)
		{
			return false;
		}
		return true;
	}

	public override int RemoveCard(Card card)
	{
		return base.RemoveCard(card);
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
			UpdateLayoutFinished();
		}
		else
		{
			StartCoroutine(UpdateLayoutImpl());
		}
	}

	private IEnumerator UpdateLayoutImpl()
	{
		Card activeAnomaly = m_cards[0];
		while (activeAnomaly.IsDoNotSort())
		{
			yield return null;
		}
		activeAnomaly.ShowCard();
		activeAnomaly.EnableTransitioningZones(enable: true);
		SpellUtils.ActivateStateIfNecessary(activeAnomaly.GetActor().GetSpell(SpellType.SUMMON_IN_FRIENDLY), SpellStateType.ACTION);
		StartFinishLayoutTimer(0.1f);
	}
}
