using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MercenariesEndOfGameSpell : SuperSpell
{
	public float m_secondsOffsetToStartExplosionFx;

	private readonly List<Card> m_cardsToExplode = new List<Card>();

	private bool m_isExplosionFxStarted;

	private void AddPowerTargets_FromActorsInPlay(Player.Side side)
	{
		ZonePlay playZone = ZoneMgr.Get().FindZoneOfType<ZonePlay>(side);
		if (playZone != null)
		{
			m_cardsToExplode.AddRange(playZone.GetCards());
		}
	}

	public override bool AddPowerTargets()
	{
		if (GameState.Get().GetOpposingSidePlayer().GetTag<TAG_PLAYSTATE>(GAME_TAG.PLAYSTATE) != TAG_PLAYSTATE.WON)
		{
			AddPowerTargets_FromActorsInPlay(Player.Side.OPPOSING);
		}
		if (GameState.Get().GetFriendlySidePlayer().GetTag<TAG_PLAYSTATE>(GAME_TAG.PLAYSTATE) != TAG_PLAYSTATE.WON)
		{
			AddPowerTargets_FromActorsInPlay(Player.Side.FRIENDLY);
		}
		return true;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish += m_cardsToExplode.Count;
		base.OnAction(prevStateType);
		LettuceMissionEntity letlMissionEntity = GameState.Get()?.GetGameEntity() as LettuceMissionEntity;
		if (m_taskList.HasTasks())
		{
			letlMissionEntity?.RegisterOnEmoteBanterPlayedEvent(OnEmoteBanterPlayed);
		}
		m_taskList.DoAllTasks(delegate
		{
			letlMissionEntity?.UnregisterOnEmoteBanterPlayedEvent(OnEmoteBanterPlayed);
			PlayActorExplosionFx();
		});
	}

	private void OnEmoteBanterPlayed(LettuceMissionEntity letlMissionEntity, EmoteType emoteType, AudioSource audioSource)
	{
		if (emoteType == EmoteType.WELL_PLAYED)
		{
			letlMissionEntity.UnregisterOnEmoteBanterPlayedEvent(OnEmoteBanterPlayed);
			float delaySec = audioSource.clip.length - m_secondsOffsetToStartExplosionFx;
			if (delaySec > 0f)
			{
				StartCoroutine(WaitForSecondsThenPlayActorExplosionFx(delaySec));
			}
			else
			{
				PlayActorExplosionFx();
			}
		}
	}

	private IEnumerator WaitForSecondsThenPlayActorExplosionFx(float delaySec)
	{
		yield return new WaitForSeconds(delaySec);
		PlayActorExplosionFx();
	}

	private void PlayActorExplosionFx()
	{
		if (m_isExplosionFxStarted)
		{
			return;
		}
		m_isExplosionFxStarted = true;
		if (m_cardsToExplode.Count == 0)
		{
			FinishIfPossible();
			return;
		}
		foreach (Card card in m_cardsToExplode)
		{
			Actor actor = card.GetActor();
			if (actor == null)
			{
				OnExplodeSpellFinished();
				continue;
			}
			actor.ActivateAllSpellsDeathStates();
			actor.ToggleForceIdle(bOn: true);
			actor.SetActorState(ActorStateType.CARD_IDLE);
			if (card.ActivateActorSpell(SpellType.MERCENARIES_PORTRAIT_EXPLODE, delegate
			{
				OnExplodeSpellFinished();
			}) == null)
			{
				OnExplodeSpellFinished();
			}
		}
	}

	private void OnExplodeSpellFinished()
	{
		m_effectsPendingFinish--;
		FinishIfPossible();
	}

	protected override void OnDeath(SpellStateType prevStateType)
	{
		base.OnDeath(prevStateType);
		GameState.Get()?.GetGameEntity()?.ActivateEndOfGameSpellState(SpellStateType.DEATH);
	}
}
