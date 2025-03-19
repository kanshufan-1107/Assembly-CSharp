using System.Collections;
using System.Collections.Generic;

public class ResetGameSpellController : SpellController
{
	public Spell m_DefaultHideScreenSpell;

	private static Spell s_hideScreenSpellInstance;

	private int m_resetGameTaskIndex;

	private Entity m_clonedSourceEntity;

	private Entity m_prevGameEntity;

	protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
	{
		s_hideScreenSpellInstance = null;
		Entity sourceEntity = taskList.GetSourceEntity();
		if (sourceEntity != null)
		{
			Card sourceCard = sourceEntity.GetCard();
			CardEffect effect = InitEffect(sourceCard);
			if (effect != null)
			{
				s_hideScreenSpellInstance = InitResetGameSpell(effect, sourceCard);
			}
		}
		if (!taskList.IsStartOfBlock() || !taskList.IsEndOfBlock())
		{
			Log.Gameplay.PrintWarning($"{this}.AddPowerSourceAndTargets(): ResetGame power block was split into multiple tasklists.");
		}
		m_resetGameTaskIndex = -1;
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int taskIndex = 0; taskIndex < tasks.Count; taskIndex++)
		{
			if (tasks[taskIndex].GetPower() is Network.HistResetGame)
			{
				m_resetGameTaskIndex = taskIndex;
				break;
			}
		}
		if (m_clonedSourceEntity == null && taskList.GetSourceEntity() != null)
		{
			m_clonedSourceEntity = taskList.GetSourceEntity().CloneForZoneMgr();
		}
		return true;
	}

	protected override void OnProcessTaskList()
	{
		StartCoroutine(DoEffectsWithTiming());
	}

	private IEnumerator DoEffectsWithTiming()
	{
		if (m_taskList.IsStartOfBlock())
		{
			if (m_prevGameEntity == null)
			{
				m_prevGameEntity = GameState.Get().GetGameEntity();
			}
			GameState.Get().GetGameEntity().NotifyOfResetGameStarted();
		}
		if (m_resetGameTaskIndex != -1)
		{
			if (s_hideScreenSpellInstance == null)
			{
				s_hideScreenSpellInstance = SpellManager.Get().GetSpell(m_DefaultHideScreenSpell);
			}
			s_hideScreenSpellInstance.ActivateState(SpellStateType.BIRTH);
			while (s_hideScreenSpellInstance.GetActiveState() != SpellStateType.IDLE)
			{
				yield return null;
			}
			PowerTask resetGameTask = m_taskList.GetTaskList()[m_resetGameTaskIndex];
			m_taskList.DoTasks(0, m_resetGameTaskIndex + 1);
			while (!resetGameTask.IsCompleted())
			{
				yield return null;
			}
		}
		List<Card> recreatedCards = new List<Card>();
		List<PowerTask> tasks = m_taskList.GetTaskList();
		for (int taskIndex = m_resetGameTaskIndex; taskIndex < tasks.Count; taskIndex++)
		{
			if (!(tasks[taskIndex].GetPower() is Network.HistFullEntity fullEntity))
			{
				continue;
			}
			Entity entity = GameState.Get().GetEntity(fullEntity.Entity.ID);
			if (entity != null)
			{
				Card card = entity.GetCard();
				if (!(card == null))
				{
					card.SuppressPlaySounds(suppress: true);
					card.SetTransitionStyle(ZoneTransitionStyle.INSTANT);
					recreatedCards.Add(card);
				}
			}
		}
		m_taskList.DoAllTasks();
		while (!m_taskList.IsComplete())
		{
			yield return null;
		}
		foreach (Card card2 in recreatedCards)
		{
			card2.SetTransitionStyle(ZoneTransitionStyle.NORMAL);
			card2.SuppressPlaySounds(suppress: false);
			Entity entity2 = card2.GetEntity();
			TAG_ZONE zoneTag = entity2.GetZone();
			if (zoneTag == TAG_ZONE.PLAY || zoneTag == TAG_ZONE.SECRET)
			{
				card2.ShowExhaustedChange(entity2.IsExhausted());
			}
		}
		if (m_taskList.IsEndOfBlock())
		{
			EndTurnButton.Get().Reset();
			s_hideScreenSpellInstance.ActivateState(SpellStateType.DEATH);
			while (s_hideScreenSpellInstance.GetActiveState() != 0)
			{
				yield return null;
			}
			SpellManager.Get().ReleaseSpell(s_hideScreenSpellInstance);
			s_hideScreenSpellInstance = null;
			GameState.Get().GetGameEntity().NotifyOfResetGameFinished(m_clonedSourceEntity, m_prevGameEntity);
			m_prevGameEntity = null;
		}
		OnFinishedTaskList();
		OnFinished();
	}

	private CardEffect InitEffect(Card card)
	{
		if (card == null)
		{
			return null;
		}
		int effectIndex = m_taskList.GetBlockStart().EffectIndex;
		if (effectIndex < 0)
		{
			return null;
		}
		return card.GetResetGameEffect(effectIndex);
	}

	private Spell InitResetGameSpell(CardEffect effect, Card card)
	{
		Spell spell = effect.GetSpell();
		if (spell == null)
		{
			return null;
		}
		if (!spell.AttachPowerTaskList(m_taskList))
		{
			Log.Power.Print("{0}.InitResetGameSpell() - FAILED to add targets to spell for {1}", this, card);
			return null;
		}
		return spell;
	}
}
