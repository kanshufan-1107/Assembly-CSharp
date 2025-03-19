using System;
using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("INTERNAL USE ONLY. Do not put this on your FSMs.")]
[ActionCategory("Pegasus")]
public abstract class SpellAction : FsmStateAction
{
	public enum Which
	{
		SOURCE,
		TARGET,
		SOURCE_HERO,
		CHOSEN_TARGET,
		SOURCE_PLAYER,
		SPELL_PARENT,
		GAME_ENTITY
	}

	protected Spell m_spell;

	public Spell GetSpell()
	{
		if (m_spell == null)
		{
			GameObject go = GetSpellOwner();
			if (go != null)
			{
				m_spell = GameObjectUtils.FindComponentInThisOrParents<Spell>(go);
			}
		}
		return m_spell;
	}

	public Card GetCard(Which which)
	{
		Spell spell = GetSpell();
		if (spell == null)
		{
			return null;
		}
		if (which == Which.TARGET)
		{
			return spell.GetTargetCard();
		}
		Card sourceCard = spell.GetSourceCard();
		if (which == Which.SOURCE_HERO && sourceCard != null)
		{
			return sourceCard.GetHeroCard();
		}
		if (which == Which.CHOSEN_TARGET)
		{
			Card targetCard = spell.GetPowerTargetCard();
			if (targetCard != null)
			{
				return targetCard;
			}
		}
		if (which == Which.SOURCE_PLAYER)
		{
			global::Log.All.PrintError("{0} cannot get card for source player: players are not cards. Did you mean to choose SOURCE_HERO?", this);
			if (sourceCard != null)
			{
				return sourceCard.GetHeroCard();
			}
		}
		if (which == Which.SPELL_PARENT)
		{
			Actor parentActor = GetSpellActor(spell);
			if (parentActor != null)
			{
				return parentActor.GetCard();
			}
		}
		return sourceCard;
	}

	public Entity GetEntity(Which which)
	{
		Spell spell = GetSpell();
		if (spell == null)
		{
			return null;
		}
		if (which == Which.TARGET)
		{
			Card targetCard = spell.GetTargetCard();
			if (targetCard != null)
			{
				return targetCard.GetEntity();
			}
			return null;
		}
		Card sourceCard = spell.GetSourceCard();
		if (which == Which.SOURCE_HERO && sourceCard != null)
		{
			return sourceCard.GetHero();
		}
		if (which == Which.SOURCE_PLAYER && sourceCard != null)
		{
			return sourceCard.GetController();
		}
		if (which == Which.CHOSEN_TARGET)
		{
			Card targetCard2 = spell.GetPowerTargetCard();
			if (targetCard2 != null)
			{
				return targetCard2.GetEntity();
			}
		}
		if (which == Which.SPELL_PARENT)
		{
			Actor parentActor = GetSpellActor(spell);
			if (parentActor != null)
			{
				return parentActor.GetEntity();
			}
		}
		if (which == Which.GAME_ENTITY)
		{
			return GameState.Get()?.GetGameEntity();
		}
		if (sourceCard != null)
		{
			return sourceCard.GetEntity();
		}
		return null;
	}

	public Actor GetActor(Which which)
	{
		Card card = GetCard(which);
		if (card == null)
		{
			return null;
		}
		return card.GetActor();
	}

	public int GetIndexMatchingCardId(string cardId, string[] cardIds)
	{
		if (cardId == null || cardIds == null || cardIds.Length == 0)
		{
			return -1;
		}
		for (int i = 0; i < cardIds.Length; i++)
		{
			string currCardId = cardIds[i].Trim();
			if (cardId.Equals(currCardId, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	protected abstract GameObject GetSpellOwner();

	public override void OnEnter()
	{
		GetSpell();
		if (m_spell == null)
		{
			Debug.LogError($"{this}.OnEnter() - FAILED to find Spell component on Owner \"{base.Owner}\"");
		}
	}

	private Actor GetSpellActor(Spell spell)
	{
		return spell.gameObject.GetComponentInParent<Actor>();
	}
}
