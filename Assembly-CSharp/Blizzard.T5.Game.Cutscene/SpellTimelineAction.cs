using System.Collections;
using UnityEngine;

namespace Blizzard.T5.Game.Cutscene;

public class SpellTimelineAction : TimelineAction
{
	protected SpellType m_spellType;

	protected Spell m_spell;

	protected Actor m_actionSource;

	protected Actor m_actionTarget;

	protected LegendaryHeroAnimations m_legendaryAnimationType;

	public SpellTimelineAction(Spell spell, Actor source, Actor target)
	{
		InternalInitialize(SpellType.NONE, spell, source, target);
	}

	public SpellTimelineAction(SpellType spellType, Actor source, Actor target)
	{
		InternalInitialize(spellType, null, source, target);
	}

	private void InternalInitialize(SpellType spellType, Spell spell, Actor source, Actor target)
	{
		if (source == null)
		{
			Log.All.PrintError(string.Format("Cutscene {0} has invalid setup for spell type: {1} as no source Actor provided!", "SpellTimelineAction", spellType));
		}
		if (m_spell != null && SpellManager.Get() != null)
		{
			m_spell = SpellManager.Get().GetSpell(spell);
		}
		else
		{
			m_spell = spell;
		}
		m_spellType = spellType;
		m_actionSource = source;
		m_actionTarget = target;
		if (m_spell != null)
		{
			m_legendaryAnimationType = LegendaryHeroAnimations.SpellCard;
			return;
		}
		switch (spellType)
		{
		case SpellType.NONE:
			m_legendaryAnimationType = LegendaryHeroAnimations.None;
			break;
		case SpellType.SUMMON_IN:
			m_legendaryAnimationType = LegendaryHeroAnimations.SummonMinion;
			break;
		default:
			m_legendaryAnimationType = LegendaryHeroAnimations.SpellCard;
			break;
		}
	}

	public override void Init()
	{
		if (m_isReady || (m_spellType == SpellType.NONE && m_spell == null) || m_actionSource == null)
		{
			return;
		}
		if (m_spell == null)
		{
			m_spell = m_actionSource.GetSpell(m_spellType);
		}
		if (!(m_spell == null))
		{
			if (m_actionTarget != null)
			{
				m_spell.RemoveAllTargets();
				m_spell.AddTarget(m_actionTarget.GetCard().gameObject);
			}
			m_spell.SetSource(m_actionSource.GetCard().gameObject);
			m_isReady = true;
		}
	}

	public override IEnumerator Play()
	{
		if (!m_isReady)
		{
			Log.CosmeticPreview.PrintError("Cutscene timeline logic error! Play requested on GetType when action is not ready!");
			yield break;
		}
		m_hasFinishedPlaying = false;
		m_spell.AddStateFinishedCallback(OnStateFinishedCallback);
		m_spell.AddFinishedCallback(delegate
		{
			m_hasFinishedPlaying = true;
		});
		ActivateSpell();
		SafeTryTriggerLegendaryAnimation(m_legendaryAnimationType);
		yield return new WaitUntil(() => base.HasFinished);
	}

	public override void Stop()
	{
		Reset();
		if (m_spell != null)
		{
			m_spell.Deactivate();
		}
		m_hasFinishedPlaying = true;
	}

	private void OnStateFinishedCallback(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (prevStateType == SpellStateType.NONE)
		{
			spell.RemoveStateFinishedCallback(OnStateFinishedCallback);
			return;
		}
		SpellStateType nextState = spell.GuessNextStateType();
		if (nextState == SpellStateType.NONE || nextState == SpellStateType.DEATH)
		{
			spell.RemoveStateFinishedCallback(OnStateFinishedCallback);
		}
		else if (nextState != prevStateType)
		{
			spell.ActivateState(nextState);
		}
	}

	public override void Dispose()
	{
		if (m_spell != null)
		{
			SpellManager.Get()?.ReleaseSpell(m_spell, reset: true);
			m_spell = null;
		}
		m_isReady = false;
	}

	public override void Reset()
	{
		if (!m_isReady || m_spell == null)
		{
			return;
		}
		if (m_spell is SuperSpell superSpell)
		{
			Spell aeSpell = superSpell.GetActiveAreaEffectSpell();
			if (aeSpell != null)
			{
				aeSpell.ActivateState(SpellStateType.RESET);
			}
		}
		m_spell.ActivateState(SpellStateType.RESET);
	}

	protected void SafeTryTriggerLegendaryAnimation(LegendaryHeroAnimations anim)
	{
		if (anim == LegendaryHeroAnimations.None)
		{
			return;
		}
		Card sourceCard = m_actionSource.GetCard();
		if (!(sourceCard == null))
		{
			Card heroCard = sourceCard.GetHeroCard();
			if (!(heroCard == null))
			{
				heroCard.ActivateLegendaryHeroAnimEvent(anim);
			}
		}
	}

	private void ActivateSpell()
	{
		m_spell.Activate();
		if (m_spellType == SpellType.ENDGAME_VICTORY_STRIKE_FRIENDLY || m_spellType == SpellType.ENDGAME_LOSE_FRIENDLY)
		{
			m_spell.ActivateState(SpellStateType.ACTION);
		}
		else
		{
			m_spell.ActivateState(SpellStateType.BIRTH);
		}
	}
}
