using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using PegasusGame;
using UnityEngine;

[CustomEditClass]
public class TriggerSpellController : SpellController
{
	public List<AuxiliaryTriggerSpellEntry> m_AuxiliaryTriggerSpells = new List<AuxiliaryTriggerSpellEntry>();

	private Map<int, Spell> m_triggerSpellByEntityId = new Map<int, Spell>();

	private List<CardSoundSpell> m_triggerSoundSpells = new List<CardSoundSpell>();

	private Map<int, Spell> m_actorTriggerSpellByEntityId = new Map<int, Spell>();

	private Map<int, Spell> m_auxiliaryTriggerSpellByEntityId = new Map<int, Spell>();

	private static readonly float BAUBLE_WAIT_TIME_SEC = 1f;

	private int m_cardEffectsBlockingFinish;

	private int m_cardEffectsBlockingTaskListFinish;

	private int m_actorEffectsBlockingFinish;

	private bool m_waitingForBauble;

	private bool m_baubleBlockedFinish;

	protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
	{
		if (!HasSourceCard(taskList))
		{
			return false;
		}
		List<Entity> sourceEntities = taskList.GetSourceEntities();
		List<Card> sourceCards = new List<Card>();
		foreach (Entity sourceEntity in sourceEntities)
		{
			if (sourceEntity != null && sourceEntity.GetCard() != null)
			{
				sourceCards.Add(sourceEntity.GetCard());
			}
		}
		GameState gameState = GameState.Get();
		foreach (Card sourceCard in sourceCards)
		{
			Card auxiliarySourceCard = sourceCard;
			Entity sourceCardEntity = sourceCard.GetEntity();
			int entityId = sourceCardEntity.GetEntityId();
			if (CanPlayActorTriggerSpell(sourceCardEntity))
			{
				m_actorTriggerSpellByEntityId.Add(entityId, GetActorTriggerSpell(sourceCardEntity));
			}
			CardEffect effect = InitEffect(sourceCard);
			if (effect != null && CanPlayTriggerSpell(taskList, sourceCardEntity))
			{
				InitTriggerSpell(effect, sourceCard);
				InitTriggerSounds(effect, sourceCard);
			}
			if (sourceCardEntity.IsEnchantment())
			{
				Entity attachedEntity = gameState.GetEntity(sourceCardEntity.GetAttached());
				if (attachedEntity != null)
				{
					auxiliarySourceCard = attachedEntity.GetCard();
				}
			}
			if (!(auxiliarySourceCard != null))
			{
				continue;
			}
			Spell auxiliaryTriggerSpell = GetAuxiliaryTriggerSpell();
			if (auxiliaryTriggerSpell != null)
			{
				m_auxiliaryTriggerSpellByEntityId.Add(entityId, auxiliaryTriggerSpell);
				auxiliaryTriggerSpell.SetSource(auxiliarySourceCard.gameObject);
				if (!auxiliaryTriggerSpell.AttachPowerTaskList(m_taskList))
				{
					Log.Power.Print("{0}.AddPowerSourceAndTargets() - FAILED to add targets to spell for {1}", this, m_auxiliaryTriggerSpellByEntityId);
					m_auxiliaryTriggerSpellByEntityId.Remove(entityId);
				}
			}
		}
		if (m_triggerSpellByEntityId.Count == 0 && m_triggerSoundSpells.Count == 0 && m_actorTriggerSpellByEntityId.Count == 0 && m_auxiliaryTriggerSpellByEntityId.Count == 0)
		{
			Reset();
			if (!TurnStartManager.Get().IsCardDrawHandled((sourceCards.Count > 0) ? sourceCards[0] : null))
			{
				return TurnStartManager.Get().IsCardDrawHandled(taskList.GetStartDrawMetaDataCard());
			}
			return true;
		}
		SetSource(sourceCards);
		return true;
	}

	protected override bool HasSourceCard(PowerTaskList taskList)
	{
		List<Entity> sourceEntities = taskList.GetSourceEntities();
		if (sourceEntities == null || sourceEntities.Count == 0)
		{
			return false;
		}
		if (GetCardsWithActorTrigger(taskList).Count == 0)
		{
			Card startDrawMetaCard = taskList.GetStartDrawMetaDataCard();
			if (startDrawMetaCard != null)
			{
				return TurnStartManager.Get().IsCardDrawHandled(startDrawMetaCard);
			}
			return false;
		}
		return true;
	}

	protected override void OnProcessTaskList()
	{
		StartCoroutine(OnProcessTaskListImpl());
	}

	private IEnumerator OnProcessTaskListImpl()
	{
		if (GameState.Get().IsTurnStartManagerActive())
		{
			TurnStartManager.Get().NotifyOfTriggerVisual();
			while (TurnStartManager.Get().IsTurnStartIndicatorShowing())
			{
				yield return null;
			}
		}
		if (!ActivateInitialSpell())
		{
			base.OnProcessTaskList();
		}
	}

	protected override void OnFinished()
	{
		if (m_processingTaskList)
		{
			m_pendingFinish = true;
		}
		else
		{
			StartCoroutine(WaitThenFinish());
		}
	}

	public override bool ShouldReconnectIfStuck()
	{
		if (m_triggerSpellByEntityId.Count > 0)
		{
			foreach (KeyValuePair<int, Spell> pair in m_triggerSpellByEntityId)
			{
				if (pair.Value != null && pair.Value.ShouldReconnectIfStuck())
				{
					return true;
				}
			}
			return false;
		}
		return base.ShouldReconnectIfStuck();
	}

	private void Reset()
	{
		foreach (KeyValuePair<int, Spell> item in m_triggerSpellByEntityId)
		{
			Spell spell = item.Value;
			if (!(spell == null) && spell.GetPowerTaskList() != null && spell.GetPowerTaskList().GetId() == m_taskListId)
			{
				SpellUtils.PurgeSpell(spell);
			}
		}
		for (int i = 0; i < m_triggerSoundSpells.Count; i++)
		{
			CardSoundSpell soundSpell = m_triggerSoundSpells[i];
			if (soundSpell != null && soundSpell.GetPowerTaskList().GetId() == m_taskListId)
			{
				SpellUtils.PurgeSpell(soundSpell);
			}
		}
		foreach (KeyValuePair<int, Spell> item2 in m_auxiliaryTriggerSpellByEntityId)
		{
			Spell spell2 = item2.Value;
			if (!(spell2 == null) && spell2.GetPowerTaskList() != null && spell2.GetPowerTaskList().GetId() == m_taskListId)
			{
				SpellUtils.PurgeSpell(spell2);
			}
		}
		foreach (KeyValuePair<int, Spell> item3 in m_actorTriggerSpellByEntityId)
		{
			Spell spell3 = item3.Value;
			if (!(spell3 == null) && spell3.GetPowerTaskList() != null && spell3.GetPowerTaskList().GetId() == m_taskListId)
			{
				SpellUtils.PurgeSpell(spell3);
			}
		}
		m_triggerSpellByEntityId.Clear();
		m_auxiliaryTriggerSpellByEntityId.Clear();
		m_triggerSoundSpells.Clear();
		m_actorTriggerSpellByEntityId.Clear();
		m_cardEffectsBlockingFinish = 0;
		m_cardEffectsBlockingTaskListFinish = 0;
		m_actorEffectsBlockingFinish = 0;
	}

	private IEnumerator WaitThenFinish()
	{
		yield return new WaitForSeconds(10f);
		Reset();
		base.OnFinished();
	}

	private bool ActivateInitialSpell()
	{
		List<Entity> sourceEntities = m_taskList.GetSourceEntities();
		bool anyTriggered = false;
		foreach (Entity entity in sourceEntities)
		{
			if (ActivateActorTriggerSpell(entity.GetEntityId()))
			{
				anyTriggered = true;
				continue;
			}
			ActivateAuxiliaryTriggerSpell(entity.GetEntityId());
			if (ActivateCardEffects(entity.GetEntityId()))
			{
				anyTriggered = true;
			}
		}
		return anyTriggered;
	}

	private void ProcessCurrentTaskList()
	{
		if (m_taskList != null)
		{
			m_taskList.DoAllTasks();
		}
	}

	private List<Card> GetCardsWithActorTrigger(PowerTaskList taskList)
	{
		List<Entity> entities = taskList.GetSourceEntities();
		return GetCardsWithActorTrigger(entities);
	}

	private List<Card> GetCardsWithActorTrigger(List<Entity> entities)
	{
		List<Card> cards = new List<Card>();
		if (entities == null || entities.Count == 0)
		{
			return cards;
		}
		foreach (Entity entity in entities)
		{
			Card card = GetCardWithActorTrigger(entity);
			if (card != null)
			{
				cards.Add(card);
			}
		}
		return cards;
	}

	private Card GetCardWithActorTrigger(Entity entity)
	{
		if (entity == null)
		{
			return null;
		}
		if (entity.IsEnchantment())
		{
			return GameState.Get().GetEntity(entity.GetAttached())?.GetCard();
		}
		return entity.GetCard();
	}

	private bool CanPlayTriggerSpell(PowerTaskList taskList, Entity entity)
	{
		Network.HistBlockStart blockStart = taskList.GetBlockStart();
		if (entity != null && blockStart != null && blockStart.TriggerKeyword == 2129)
		{
			int goal = entity.GetTag(GAME_TAG.SCORE_VALUE_1);
			if (entity.GetTag(GAME_TAG.SCORE_VALUE_2) + 1 < goal)
			{
				return false;
			}
		}
		return !SpellUtils.IsNonMetaTaskListInMetaBlock(taskList);
	}

	private bool CanPlayActorTriggerSpell(Entity entity)
	{
		if (!m_taskList.IsOrigin())
		{
			return false;
		}
		Card cardWithActorTrigger = GetCardWithActorTrigger(entity);
		if (cardWithActorTrigger == null)
		{
			return false;
		}
		if (cardWithActorTrigger.WillSuppressActorTriggerSpell())
		{
			return false;
		}
		if (!cardWithActorTrigger.CanShowActorVisuals())
		{
			return false;
		}
		_ = m_triggerSpellByEntityId.Count;
		_ = 0;
		return true;
	}

	private Spell GetActorTriggerSpell(Entity entity)
	{
		Network.HistBlockStart blockStart = m_taskList.GetBlockStart();
		if (blockStart == null)
		{
			return null;
		}
		int triggerKeyword = blockStart.TriggerKeyword;
		SpellType spellType = GetActorTriggerSpellType(triggerKeyword, entity);
		if (spellType == SpellType.NONE)
		{
			return null;
		}
		Card cardWithActorTrigger = GetCardWithActorTrigger(entity);
		if ((object)cardWithActorTrigger == null)
		{
			return null;
		}
		Spell spell = cardWithActorTrigger.GetActorSpell(spellType);
		if (spell != null)
		{
			spell.SetSource(cardWithActorTrigger.gameObject);
		}
		return spell;
	}

	private SpellType GetActorTriggerSpellType(int triggerKeyword, Entity entity)
	{
		SpellType spellType = SpellType.NONE;
		switch (triggerKeyword)
		{
		case 363:
		case 1944:
			spellType = SpellType.POISONOUS;
			break;
		case 2853:
			spellType = SpellType.VENOMOUS;
			break;
		case 403:
			spellType = SpellType.INSPIRE;
			break;
		case 685:
		case 1675:
			spellType = SpellType.LIFESTEAL;
			break;
		case 923:
			spellType = SpellType.OVERKILL;
			break;
		case 1427:
		case 2672:
			spellType = SpellType.SPELLBURST;
			break;
		case 340:
			spellType = SpellType.SPELLBURST;
			break;
		case 1637:
			spellType = SpellType.FRENZY;
			break;
		case 1920:
			spellType = SpellType.HONORABLEKILL;
			break;
		case 2129:
			spellType = SpellType.AVENGE;
			break;
		case 32:
		case 2821:
		case 3744:
			spellType = entity.GetTriggerSpellType();
			break;
		}
		return spellType;
	}

	private bool ActivateActorTriggerSpell(int entityId)
	{
		if (!m_actorTriggerSpellByEntityId.ContainsKey(entityId))
		{
			return false;
		}
		Spell spell = m_actorTriggerSpellByEntityId[entityId];
		if (spell == null)
		{
			return false;
		}
		Entity entity = m_taskList.GetSourceEntities().Find((Entity e) => e.GetEntityId() == entityId);
		Card card = GetCardWithActorTrigger(entity);
		if (card == null)
		{
			return false;
		}
		if (card.IsBaubleAnimating())
		{
			Log.Gameplay.PrintError("TriggerSpellController.ActivateTriggerSpell(): Clobbering bauble that is currently animating on Card {0}.", card);
		}
		card.DeactivateBaubles();
		card.SetIsBaubleAnimating(isAnimating: true);
		m_actorEffectsBlockingFinish++;
		spell.AddStateFinishedCallback(OnActorTriggerSpellStateFinished, entity);
		spell.ClearPositionDirtyFlag();
		spell.AttachPowerTaskList(m_taskList);
		spell.ActivateState(SpellStateType.ACTION);
		return true;
	}

	private void OnActorTriggerSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (prevStateType == SpellStateType.ACTION)
		{
			spell.RemoveStateFinishedCallback(OnActorTriggerSpellStateFinished, userData);
			StartCoroutine(FinishActorTriggerSpell(spell, prevStateType, userData));
		}
	}

	private IEnumerator FinishActorTriggerSpell(Spell spell, SpellStateType prevStateType, object userData)
	{
		Entity entity = (Entity)userData;
		m_baubleBlockedFinish = false;
		m_waitingForBauble = true;
		bool activatedCardEffects = ActivateCardEffects(entity.GetEntityId());
		if (!activatedCardEffects)
		{
			ProcessCurrentTaskList();
		}
		ActivateAuxiliaryTriggerSpell(entity.GetEntityId());
		switch (m_actorTriggerSpellByEntityId[entity.GetEntityId()].GetSpellType())
		{
		case SpellType.TRIGGER:
		case SpellType.POISONOUS:
		case SpellType.FAST_TRIGGER:
		case SpellType.INSPIRE:
		case SpellType.LIFESTEAL:
		case SpellType.DORMANT:
		case SpellType.OVERKILL:
		case SpellType.HONORABLEKILL:
		case SpellType.AVENGE:
		case SpellType.VENOMOUS:
		case SpellType.TRIGGER_XY:
		case SpellType.TRIGGER_XY_STAY:
			yield return null;
			break;
		default:
			yield return new WaitForSeconds(BAUBLE_WAIT_TIME_SEC);
			break;
		}
		Card card = GetCardWithActorTrigger(entity);
		card.SetIsBaubleAnimating(isAnimating: false);
		if (card.CanShowActorVisuals())
		{
			card.UpdateBauble();
		}
		m_waitingForBauble = false;
		m_actorEffectsBlockingFinish--;
		if (m_actorEffectsBlockingFinish <= 0)
		{
			if (!activatedCardEffects)
			{
				base.OnProcessTaskList();
			}
			else if (m_baubleBlockedFinish)
			{
				OnFinishedTaskList();
			}
		}
	}

	private CardEffect InitEffect(Card card)
	{
		if (card == null)
		{
			return null;
		}
		Network.HistBlockStart blockStart = m_taskList.GetBlockStart();
		int entityId = card.GetEntity().GetEntityId();
		string effectCardId = m_taskList.GetEffectCardId(entityId);
		int effectIndex = blockStart.EffectIndex;
		if (effectIndex < 0)
		{
			if (string.IsNullOrEmpty(effectCardId) || m_taskList.IsEffectCardIdClientCached(entityId))
			{
				return null;
			}
			effectIndex = 0;
		}
		CardEffect effect = null;
		string sourceCardID = card.GetEntity()?.GetCardId();
		if (string.IsNullOrEmpty(effectCardId) || sourceCardID == effectCardId)
		{
			effect = card.GetTriggerEffect(effectIndex);
		}
		else
		{
			using DefLoader.DisposableCardDef def = DefLoader.Get().GetCardDef(effectCardId);
			if (def.CardDef.m_TriggerEffectDefs == null)
			{
				return null;
			}
			if (effectIndex >= def.CardDef.m_TriggerEffectDefs.Count)
			{
				return null;
			}
			effect = new CardEffect(def.CardDef.m_TriggerEffectDefs[effectIndex], card);
		}
		return effect;
	}

	private bool ActivateCardEffects(int entityId)
	{
		bool num = ActivateTriggerSpell(entityId);
		bool soundsSucceeded = ActivateTriggerSounds();
		return num || soundsSucceeded;
	}

	private void OnCardSpellFinished(Spell spell, object userData)
	{
		CardSpellFinished();
	}

	private void OnCardSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			CardSpellNoneStateEntered();
		}
	}

	private void CardSpellFinished()
	{
		m_cardEffectsBlockingTaskListFinish--;
		if (m_cardEffectsBlockingTaskListFinish <= 0)
		{
			if (m_waitingForBauble)
			{
				m_baubleBlockedFinish = true;
				ProcessCurrentTaskList();
			}
			else
			{
				OnFinishedTaskList();
			}
		}
	}

	private void CardSpellNoneStateEntered()
	{
		m_cardEffectsBlockingFinish--;
		if (m_cardEffectsBlockingFinish <= 0)
		{
			OnFinished();
		}
	}

	private void InitTriggerSpell(CardEffect effect, Card card)
	{
		Spell spell = effect.GetSpell();
		if (!(spell == null))
		{
			if (!spell.AttachPowerTaskList(m_taskList))
			{
				Log.Power.Print("{0}.InitTriggerSpell() - FAILED to add targets to spell for {1}", this, card);
			}
			else
			{
				m_triggerSpellByEntityId.Add(card.GetEntity().GetEntityId(), spell);
				m_cardEffectsBlockingFinish++;
				m_cardEffectsBlockingTaskListFinish++;
			}
		}
	}

	private bool ActivateTriggerSpell(int entityId)
	{
		if (!m_triggerSpellByEntityId.ContainsKey(entityId))
		{
			return false;
		}
		Spell spell = m_triggerSpellByEntityId[entityId];
		if (spell == null)
		{
			return false;
		}
		spell.ForceUpdateTransform();
		spell.AddFinishedCallback(OnCardSpellFinished);
		spell.AddStateFinishedCallback(OnCardSpellStateFinished);
		spell.ActivateState(SpellStateType.ACTION);
		return true;
	}

	private bool InitTriggerSounds(CardEffect effect, Card card)
	{
		List<CardSoundSpell> soundSpells = effect.GetSoundSpells();
		if (soundSpells == null)
		{
			return false;
		}
		if (soundSpells.Count == 0)
		{
			return false;
		}
		foreach (CardSoundSpell spell in soundSpells)
		{
			if ((bool)spell)
			{
				if (!spell.AttachPowerTaskList(m_taskList))
				{
					Log.Power.Print("{0}.InitTriggerSounds() - FAILED to attach task list to TriggerSoundSpell {1} for Card {2}", base.name, spell, card);
				}
				else
				{
					m_triggerSoundSpells.Add(spell);
				}
			}
		}
		if (m_triggerSoundSpells.Count == 0)
		{
			return false;
		}
		m_cardEffectsBlockingFinish++;
		m_cardEffectsBlockingTaskListFinish++;
		return true;
	}

	private bool ActivateTriggerSounds()
	{
		if (m_triggerSoundSpells.Count == 0)
		{
			return false;
		}
		Card card = GetSource();
		foreach (CardSoundSpell spell in m_triggerSoundSpells)
		{
			if ((bool)spell)
			{
				card.ActivateSoundSpell(spell);
			}
		}
		if (m_taskList.IsOrigin() && !m_taskList.DoesBlockHaveEffectTimingMetaData())
		{
			Network.HistMetaData metaData = new Network.HistMetaData();
			metaData.MetaType = HistoryMeta.Type.EFFECT_TIMING;
			m_taskList.CreateTask(metaData);
		}
		CardSpellFinished();
		CardSpellNoneStateEntered();
		return true;
	}

	private Spell GetAuxiliaryTriggerSpell()
	{
		int triggerKeyword = m_taskList.GetBlockStart().TriggerKeyword;
		for (int i = 0; i < m_AuxiliaryTriggerSpells.Count; i++)
		{
			if (m_AuxiliaryTriggerSpells[i].m_TriggerKeyword == (GAME_TAG)triggerKeyword)
			{
				Spell spell = SpellManager.Get().GetSpell(m_AuxiliaryTriggerSpells[i].m_Spell);
				if (spell != null)
				{
					return spell;
				}
				Log.Gameplay.PrintError("{0}.GetAuxiliaryTriggerSpell(): keyword:{1}, spell:{2}", this, triggerKeyword, m_AuxiliaryTriggerSpells[i].m_Spell);
				return null;
			}
		}
		return null;
	}

	private void ActivateAuxiliaryTriggerSpell(int entityId)
	{
		if (m_auxiliaryTriggerSpellByEntityId.ContainsKey(entityId) && !(m_auxiliaryTriggerSpellByEntityId[entityId] == null))
		{
			m_auxiliaryTriggerSpellByEntityId[entityId].ActivateState(SpellStateType.ACTION);
		}
	}

	protected override float GetLostFrameTimeCatchUpSeconds()
	{
		return 0.2f;
	}
}
