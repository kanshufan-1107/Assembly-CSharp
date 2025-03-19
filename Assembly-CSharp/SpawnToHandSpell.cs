using System.Collections;
using Blizzard.T5.Core;
using Blizzard.T5.Game.Spells;
using UnityEngine;

public class SpawnToHandSpell : SuperSpell
{
	public float m_CardStartScale = 0.1f;

	public float m_CardDelay = 1f;

	public float m_CardStaggerMin;

	public float m_CardStaggerMax;

	public bool m_AccumulateStagger = true;

	public bool m_Shake = true;

	public float m_ShakeDelay;

	public CardTargetInfo m_friendlyCardTargetInfo;

	public CardTargetInfo m_opponentCardTargetInfo;

	public ShakeMinionIntensity m_ShakeIntensity = ShakeMinionIntensity.MediumShake;

	public Spell m_SpellPrefab;

	protected Map<int, Card> m_targetToOriginMap;

	public override bool AddPowerTargets()
	{
		return AddPowerTargetsInternal(fallbackToStartBlockTarget: false);
	}

	public override void RemoveAllTargets()
	{
		base.RemoveAllTargets();
		if (m_targetToOriginMap != null)
		{
			m_targetToOriginMap.Clear();
		}
	}

	protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
	{
		Network.PowerHistory power = task.GetPower();
		if (power.Type != Network.PowerType.FULL_ENTITY)
		{
			return null;
		}
		Network.Entity netEnt = (power as Network.HistFullEntity).Entity;
		Entity entity = GameState.Get().GetEntity(netEnt.ID);
		if (entity == null)
		{
			Debug.LogWarning($"{this}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {netEnt.ID} but there is no entity with that id");
			return null;
		}
		if (entity.GetZone() != TAG_ZONE.HAND)
		{
			return null;
		}
		return entity.GetCard();
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		FillUniqueOriginForTargets();
		StartCoroutine(DoEffectWithTiming());
	}

	protected virtual Vector3 GetOriginForTarget(int targetIndex = 0)
	{
		if (m_targetToOriginMap == null)
		{
			return GetFallbackOriginPosition();
		}
		if (!m_targetToOriginMap.TryGetValue(targetIndex, out var originCard))
		{
			return GetFallbackOriginPosition();
		}
		return originCard.transform.position;
	}

	private Vector3 GetTargetCardOrigin(CardTargetInfo cardTargetInfo, bool isMySourcetCard, int targetIndex = 0)
	{
		Vector3 startPosition = GetFallbackOriginPosition();
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return startPosition;
		}
		switch (cardTargetInfo.Origin)
		{
		case CardTargetInfo.TargetOrigin.CARD:
			startPosition = GetOriginForTarget(targetIndex);
			break;
		case CardTargetInfo.TargetOrigin.OPPONENTS_DECK:
			startPosition = gameState.GetPlayerBySide((!isMySourcetCard) ? Player.Side.FRIENDLY : Player.Side.OPPOSING).GetDeckZone().transform.position;
			break;
		case CardTargetInfo.TargetOrigin.OPPONENTS_HAND:
			startPosition = gameState.GetPlayerBySide((!isMySourcetCard) ? Player.Side.FRIENDLY : Player.Side.OPPOSING).GetHandZone().transform.position;
			break;
		case CardTargetInfo.TargetOrigin.PLAYERS_DECK:
			startPosition = gameState.GetPlayerBySide(isMySourcetCard ? Player.Side.FRIENDLY : Player.Side.OPPOSING).GetDeckZone().transform.position;
			break;
		case CardTargetInfo.TargetOrigin.PLAYERS_HAND:
			startPosition = gameState.GetPlayerBySide(isMySourcetCard ? Player.Side.FRIENDLY : Player.Side.OPPOSING).GetHandZone().transform.position;
			break;
		}
		return startPosition;
	}

	protected void AddOriginForTarget(int targetIndex, Card card)
	{
		if (m_targetToOriginMap == null)
		{
			m_targetToOriginMap = new Map<int, Card>();
		}
		if (!m_targetToOriginMap.ContainsKey(targetIndex))
		{
			m_targetToOriginMap[targetIndex] = card;
		}
	}

	protected bool AddUniqueOriginForTarget(int targetIndex, Card card)
	{
		if (m_targetToOriginMap != null && m_targetToOriginMap.ContainsValue(card))
		{
			return false;
		}
		AddOriginForTarget(targetIndex, card);
		return true;
	}

	private void FillUniqueOriginForTargets()
	{
		ZonePlay play = GetSourceCard().GetEntity().GetController().GetBattlefieldZone();
		for (int i = 0; i < m_targets.Count; i++)
		{
			for (int j = 0; j < play.GetCardCount(); j++)
			{
				Card playCard = play.GetCardAtIndex(j);
				Card targetCard = m_targets[i].GetComponent<Card>();
				if (playCard.GetEntity().IsMinion() && targetCard.GetEntity().GetCreator() == playCard.GetEntity() && AddUniqueOriginForTarget(i, playCard))
				{
					break;
				}
			}
		}
	}

	protected virtual IEnumerator DoEffectWithTiming()
	{
		GameObject sourceObject = GetSource();
		Card component = sourceObject.GetComponent<Card>();
		Actor sourceActor = component.GetActor();
		bool isMySourceCard = component.GetController() == GameState.Get().GetFriendlySidePlayer();
		for (int i = 0; i < m_targets.Count; i++)
		{
			GameObject targetObject = m_targets[i];
			if (!(targetObject == null))
			{
				Card targetCard = targetObject.GetComponent<Card>();
				if (!(targetCard == null))
				{
					bool isMyTargetCard = targetCard.GetController() == GameState.Get().GetFriendlySidePlayer();
					CardTargetInfo targetCardTargetInfo = (isMySourceCard ? ((!isMyTargetCard) ? m_opponentCardTargetInfo : m_friendlyCardTargetInfo) : ((!isMyTargetCard) ? m_friendlyCardTargetInfo : m_opponentCardTargetInfo));
					targetCard.transform.position = GetTargetCardOrigin(targetCardTargetInfo, isMySourceCard, i);
					targetCard.transform.localScale = new Vector3(m_CardStartScale, m_CardStartScale, m_CardStartScale);
					targetCard.SetTransitionStyle(targetCardTargetInfo.TransitionStyle);
					targetCard.SetDoNotWarpToNewZone(on: true);
				}
			}
		}
		if ((bool)sourceActor && m_Shake)
		{
			GameObject sourceActorObject = sourceActor.gameObject;
			MinionShake.ShakeObject(sourceActorObject, ShakeMinionType.RandomDirection, sourceActorObject.transform.position, m_ShakeIntensity, 0.1f, 0f, m_ShakeDelay, ignoreAnimationPlaying: true);
		}
		yield return new WaitForSeconds(m_CardDelay);
		AddTransitionDelays();
		for (int j = 0; j < m_targets.Count; j++)
		{
			GameObject targetObject2 = m_targets[j];
			Card targetCard2 = targetObject2.GetComponent<Card>();
			if (m_SpellPrefab != null)
			{
				ISpell startSpell = CloneSpell(m_SpellPrefab, null);
				startSpell.SetSource(sourceObject);
				startSpell.AddTarget(targetObject2);
				startSpell.SetPosition(targetCard2.transform.position);
				float transitionDelay = targetCard2.GetTransitionDelay();
				StartCoroutine(ActivateSpellAfterDelay(startSpell, transitionDelay));
			}
		}
		m_effectsPendingFinish--;
		FinishIfPossible();
	}

	protected IEnumerator ActivateSpellAfterDelay(ISpell spell, float delay)
	{
		yield return new WaitForSeconds(delay);
		spell.Activate();
	}

	protected string GetCardIdForTarget(int targetIndex)
	{
		return m_targets[targetIndex].GetComponent<Card>().GetEntity().GetCardId();
	}

	protected Vector3 GetFallbackOriginPosition()
	{
		Card sourceCard = GetSource().GetComponent<Card>();
		if (sourceCard.GetEntity().HasTag(GAME_TAG.USE_LEADERBOARD_AS_SPAWN_ORIGIN) && PlayerLeaderboardManager.Get() != null)
		{
			PlayerLeaderboardCard playerCard = PlayerLeaderboardManager.Get().GetTileForPlayerId(sourceCard.GetEntity().GetTag(GAME_TAG.PLAYER_ID));
			if (playerCard != null)
			{
				return playerCard.m_tileActor.transform.position;
			}
			return PlayerLeaderboardManager.Get().transform.position;
		}
		return base.transform.position;
	}

	private void AddTransitionDelays()
	{
		if (m_CardStaggerMin <= 0f && m_CardStaggerMax <= 0f)
		{
			return;
		}
		if (m_AccumulateStagger)
		{
			float runningDelay = 0f;
			for (int i = 0; i < m_targets.Count; i++)
			{
				Card component = m_targets[i].GetComponent<Card>();
				float delay = Random.Range(m_CardStaggerMin, m_CardStaggerMax);
				runningDelay += delay;
				component.SetTransitionDelay(runningDelay);
			}
		}
		else
		{
			for (int j = 0; j < m_targets.Count; j++)
			{
				Card component2 = m_targets[j].GetComponent<Card>();
				float delay2 = Random.Range(m_CardStaggerMin, m_CardStaggerMax);
				component2.SetTransitionDelay(delay2);
			}
		}
	}
}
