using System.Collections;
using System.Collections.Generic;
using PegasusGame;
using UnityEngine;

public class PowerSpellController : SpellController
{
	private Spell m_powerSpell;

	private List<CardSoundSpell> m_powerSoundSpells = new List<CardSoundSpell>();

	private Entity m_ownerHeroEntity;

	private Entity m_powerSourceEntity;

	private int m_cardEffectsBlockingFinish;

	private int m_cardEffectsBlockingTaskListFinish;

	protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
	{
		if (!HasSourceCard(taskList))
		{
			return false;
		}
		Entity sourceEntity = taskList.GetSourceEntity();
		Card sourceCard = sourceEntity.GetCard();
		CardEffect effect = GetOrCreateEffect(sourceCard, m_taskList);
		if (effect == null)
		{
			return false;
		}
		if (sourceEntity.IsMinion() || sourceEntity.IsHero() || sourceEntity.IsLocation())
		{
			if (sourceEntity.IsLocation())
			{
				bool wasLocationJustPlayed = false;
				PowerTaskList parentTaskList = taskList.GetParent();
				if (parentTaskList != null)
				{
					if (parentTaskList.HasZoneChanges())
					{
						wasLocationJustPlayed = true;
					}
					else
					{
						foreach (PowerTask task in parentTaskList.GetTaskList())
						{
							if (task.GetPower() is Network.HistTagChange { Tag: 261, Value: not 0 })
							{
								wasLocationJustPlayed = true;
								break;
							}
						}
					}
				}
				if (wasLocationJustPlayed)
				{
					Spell spell = effect.GetSpell();
					if (spell != null && !spell.IsActive())
					{
						Reset();
						return false;
					}
				}
			}
			if (!InitPowerSpell(effect, sourceCard))
			{
				if (!SpellUtils.CanAddPowerTargets(taskList))
				{
					Reset();
					return false;
				}
				if (GetActorBattlecrySpell(sourceCard) == null)
				{
					Reset();
					return false;
				}
			}
		}
		else
		{
			InitPowerSpell(effect, sourceCard);
			InitPowerSounds(effect, sourceCard);
			if (m_powerSpell == null && m_powerSoundSpells.Count == 0)
			{
				Reset();
				return false;
			}
		}
		SetSource(sourceCard);
		return true;
	}

	protected override void OnProcessTaskList()
	{
		if (!ActivateActorBattlecrySpell() && !ActivateCardEffects())
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
		if (m_powerSpell != null)
		{
			return m_powerSpell.ShouldReconnectIfStuck();
		}
		return base.ShouldReconnectIfStuck();
	}

	private void Reset()
	{
		if (m_powerSpell != null && m_powerSpell.GetPowerTaskList().GetId() == m_taskListId)
		{
			SpellUtils.PurgeSpell(m_powerSpell);
		}
		if (m_powerSoundSpells != null)
		{
			for (int i = 0; i < m_powerSoundSpells.Count; i++)
			{
				CardSoundSpell soundSpell = m_powerSoundSpells[i];
				if (soundSpell != null && soundSpell.GetPowerTaskList().GetId() == m_taskListId)
				{
					SpellUtils.PurgeSpell(soundSpell);
				}
			}
		}
		m_powerSpell = null;
		m_powerSoundSpells.Clear();
		m_cardEffectsBlockingFinish = 0;
		m_cardEffectsBlockingTaskListFinish = 0;
	}

	private IEnumerator WaitThenFinish()
	{
		yield return new WaitForSeconds(10f);
		Reset();
		base.OnFinished();
	}

	private Spell GetActorBattlecrySpell(Card card)
	{
		Spell actorBattlecrySpell = card.GetActorSpell(SpellType.BATTLECRY);
		if (actorBattlecrySpell == null)
		{
			return null;
		}
		if (!actorBattlecrySpell.HasUsableState(SpellStateType.ACTION))
		{
			return null;
		}
		return actorBattlecrySpell;
	}

	private bool ActivateActorBattlecrySpell()
	{
		Card card = GetSource();
		if (!CanActivateActorBattlecrySpell(card))
		{
			return false;
		}
		Spell actorBattlecrySpell = GetActorBattlecrySpell(card);
		if (actorBattlecrySpell == null)
		{
			return false;
		}
		m_taskList.SetActivateBattlecrySpellState();
		StartCoroutine(WaitThenActivateActorBattlecrySpell(actorBattlecrySpell));
		return true;
	}

	private bool CanActivateActorBattlecrySpell(Card card)
	{
		Entity entity = card.GetEntity();
		if (entity.GetZone() != TAG_ZONE.PLAY)
		{
			return false;
		}
		if (entity.HasTag(GAME_TAG.FAST_BATTLECRY))
		{
			return false;
		}
		Spell actorBattlecrySpell = GetActorBattlecrySpell(card);
		if (actorBattlecrySpell != null && actorBattlecrySpell.GetActiveState() == SpellStateType.BIRTH)
		{
			return true;
		}
		if (!m_taskList.ShouldActivateBattlecrySpell())
		{
			return false;
		}
		if (entity.HasBattlecry())
		{
			return true;
		}
		if (entity.HasCombo() && entity.GetController().IsComboActive())
		{
			return true;
		}
		return false;
	}

	private IEnumerator WaitThenActivateActorBattlecrySpell(Spell actorBattlecrySpell)
	{
		yield return new WaitForSeconds(0.2f);
		actorBattlecrySpell.ActivateState(SpellStateType.ACTION);
		if (!ActivateCardEffects())
		{
			base.OnProcessTaskList();
		}
	}

	public static CardEffect GetOrCreateEffect(Card card, PowerTaskList taskList)
	{
		if (card == null)
		{
			return null;
		}
		CardEffect effect = null;
		Network.HistBlockStart blockStart = taskList.GetBlockStart();
		string effectCardId = taskList.GetEffectCardId();
		int suboption = blockStart.SubOption;
		int effectIndex = blockStart.EffectIndex;
		Entity sourceEntity = card.GetEntity();
		string sourceCardID = sourceEntity?.GetCardId();
		if (string.IsNullOrEmpty(effectCardId) || string.IsNullOrEmpty(sourceCardID) || sourceCardID == effectCardId)
		{
			if (suboption >= 0)
			{
				effect = card.GetSubOptionEffect(suboption, effectIndex);
			}
			else if (!sourceEntity.HasTag(GAME_TAG.IS_USING_TRADE_OPTION) && !sourceEntity.HasTag(GAME_TAG.IS_USING_FORGE_OPTION) && !sourceEntity.HasTag(GAME_TAG.IS_USING_PASS_OPTION))
			{
				effect = card.GetPlayEffect(effectIndex);
			}
		}
		else
		{
			using DefLoader.DisposableCardDef def = DefLoader.Get().GetCardDef(effectCardId);
			CardEffectDef effectDef = null;
			if (suboption >= 0)
			{
				if (effectIndex > 0)
				{
					if (def.CardDef.m_AdditionalSubOptionEffectDefs == null)
					{
						return null;
					}
					if (suboption >= def.CardDef.m_AdditionalSubOptionEffectDefs.Count)
					{
						return null;
					}
					List<CardEffectDef> effectDefList = def.CardDef.m_AdditionalSubOptionEffectDefs[suboption];
					effectIndex--;
					if (effectIndex >= effectDefList.Count)
					{
						return null;
					}
					effectDef = effectDefList[effectIndex];
				}
				else
				{
					if (def.CardDef.m_SubOptionEffectDefs == null)
					{
						return null;
					}
					if (suboption >= def.CardDef.m_SubOptionEffectDefs.Count)
					{
						return null;
					}
					effectDef = def.CardDef.m_SubOptionEffectDefs[suboption];
				}
			}
			else if (effectIndex > 0)
			{
				if (def.CardDef.m_AdditionalPlayEffectDefs == null)
				{
					return null;
				}
				effectIndex--;
				if (effectIndex >= def.CardDef.m_AdditionalPlayEffectDefs.Count)
				{
					return null;
				}
				effectDef = def.CardDef.m_AdditionalPlayEffectDefs[effectIndex];
			}
			else
			{
				effectDef = def.CardDef.m_PlayEffectDef;
			}
			effect = card.GetOrCreateProxyEffect(blockStart, effectDef);
		}
		return effect;
	}

	private bool ActivateCardEffects()
	{
		bool num = ActivatePowerSpell();
		bool soundsSucceeded = ActivatePowerSounds();
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
			OnFinishedTaskList();
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

	private bool InitPowerSpell(CardEffect effect, Card card)
	{
		Spell spell = effect.GetSpell();
		if (spell == null)
		{
			return false;
		}
		if (!spell.HasUsableState(SpellStateType.ACTION))
		{
			Log.Power.PrintWarning("{0}.InitPowerSpell() - spell {1} for Card {2} has no {3} state", base.name, spell, card, SpellStateType.ACTION);
			return false;
		}
		if (!spell.AttachPowerTaskList(m_taskList))
		{
			Log.Power.Print("{0}.InitPowerSpell() - FAILED to attach task list to spell {1} for Card {2}", base.name, spell, card);
			return false;
		}
		if (spell.GetActiveState() != 0)
		{
			spell.ActivateState(SpellStateType.NONE);
		}
		m_powerSpell = spell;
		InitPowerSpellOwnerHero();
		m_cardEffectsBlockingFinish++;
		m_cardEffectsBlockingTaskListFinish++;
		return true;
	}

	private void InitPowerSpellOwnerHero()
	{
		if (!(m_powerSpell == null))
		{
			Player controller = m_powerSpell.GetPowerSourceCard().GetController();
			if (controller != null)
			{
				m_ownerHeroEntity = controller.GetHero();
				m_powerSourceEntity = m_powerSpell.GetPowerSource();
			}
		}
	}

	private bool ActivatePowerSpell()
	{
		if (m_powerSpell == null)
		{
			return false;
		}
		m_powerSpell.AddFinishedCallback(OnCardSpellFinished);
		m_powerSpell.AddStateFinishedCallback(OnCardSpellStateFinished);
		m_powerSpell.ActivateState(SpellStateType.ACTION);
		return true;
	}

	private bool InitPowerSounds(CardEffect effect, Card card)
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
					Log.Power.Print("{0}.InitPowerSounds() - FAILED to attach task list to PowerSoundSpell {1} for Card {2}", base.name, spell, card);
				}
				else
				{
					m_powerSoundSpells.Add(spell);
				}
			}
		}
		if (m_powerSoundSpells.Count == 0)
		{
			return false;
		}
		m_cardEffectsBlockingFinish++;
		m_cardEffectsBlockingTaskListFinish++;
		return true;
	}

	private bool ActivatePowerSounds()
	{
		if (m_powerSoundSpells.Count == 0)
		{
			return false;
		}
		Card card = GetSource();
		foreach (CardSoundSpell spell in m_powerSoundSpells)
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
}
