using System.Collections;
using UnityEngine;

namespace Blizzard.T5.Game.Cutscene;

public class SuperSpellTimelineAction : SpellTimelineAction
{
	private CardEffect m_sourceCardEffect;

	public SuperSpellTimelineAction(SuperSpell superSpell, Actor source, Actor target)
		: base(SpellType.NONE, source, target)
	{
		m_spell = superSpell;
	}

	public SuperSpellTimelineAction(Actor source, Actor target)
		: base(SpellType.NONE, source, target)
	{
	}

	public override void Init()
	{
		if (m_actionSource == null || m_actionTarget == null)
		{
			return;
		}
		CardEffectDef effectDef = m_actionSource.PlayEffectDef;
		if (effectDef == null || string.IsNullOrEmpty(effectDef.m_SpellPath))
		{
			return;
		}
		Card sourceCard = m_actionSource.GetCard();
		if (m_spell == null)
		{
			m_sourceCardEffect = new CardEffect(effectDef, sourceCard);
			m_spell = m_sourceCardEffect.GetSpell();
		}
		m_spell.transform.SetPositionAndRotation(m_actionSource.transform.position, m_spell.transform.rotation);
		if (!(m_spell == null))
		{
			if (m_actionTarget != null)
			{
				m_spell.RemoveAllTargets();
				m_spell.AddTarget(m_actionTarget.gameObject);
			}
			m_spell.SetSource(m_actionSource.gameObject);
			m_isReady = true;
		}
	}

	public override IEnumerator Play()
	{
		if (!m_isReady)
		{
			Log.CosmeticPreview.PrintError("Failed to play SuperSpellTimelineAction as it wasn't ready!");
			yield break;
		}
		m_hasFinishedPlaying = false;
		if (m_spell is SuperSpell superSpell)
		{
			superSpell.SetSource(m_actionSource.gameObject);
			superSpell.RemoveAllTargets();
			superSpell.AddTarget(m_actionTarget.gameObject);
			superSpell.RemoveAllVisualTargets();
			superSpell.AddVisualTarget(m_actionTarget.GetCard().gameObject);
			superSpell.AddFinishedCallback(delegate
			{
				m_hasFinishedPlaying = true;
			});
			ICutsceneActionListener listener = superSpell;
			if (listener != null)
			{
				listener.Play();
			}
			else
			{
				superSpell.ActivateState(SpellStateType.ACTION);
			}
			if (m_actionSource.GetEntity().IsHeroPower())
			{
				m_actionSource.GetCard().ShowExhaustedChange(exhausted: true);
			}
			SafeTryTriggerLegendaryAnimation(LegendaryHeroAnimations.HeroPower);
			yield return new WaitUntil(() => m_hasFinishedPlaying);
		}
		else
		{
			yield return base.Play();
		}
	}

	public override void Stop()
	{
		Card sourceCard = m_actionSource.GetCard();
		if (sourceCard != null && sourceCard.gameObject.activeInHierarchy)
		{
			sourceCard.ShowExhaustedChange(exhausted: false);
		}
		if (m_spell != null && m_spell is ICutsceneActionListener listener)
		{
			listener.Stop();
		}
		base.Stop();
	}
}
