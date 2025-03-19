using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Get the GameObjects for each Card's Actor in current choice in left-to-right order. Requires the Spell to be extended from CustomChoiceSpell.")]
[ActionCategory("Pegasus")]
public class SpellGetChoiceCardActorsAction : SpellAction
{
	public FsmOwnerDefault m_SpellObject;

	[ArrayEditor(VariableType.GameObject, "", 0, 0, 65536)]
	public FsmArray m_ChoiceCardActorGameObjects;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_SpellObject);
	}

	public override void Reset()
	{
		m_SpellObject = null;
		m_ChoiceCardActorGameObjects = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Spell spell = GetSpell();
		if (spell == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter(): Unable to find Spell.", this);
			Finish();
			return;
		}
		CustomChoiceSpell customChoiceSpell = spell as CustomChoiceSpell;
		if (customChoiceSpell == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter(): Spell {1} is not extended from CustomChoiceSpell.", this, spell);
			Finish();
			return;
		}
		if (customChoiceSpell.GetChoiceState() == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter(): Spell {1} does not have a valid ChoiceState.", this, spell);
			Finish();
			return;
		}
		if (!m_ChoiceCardActorGameObjects.IsNone)
		{
			List<Card> choiceCards = customChoiceSpell.GetChoiceState().m_cards;
			GameObject[] actorGameObjects = new GameObject[choiceCards.Count];
			for (int cardIndex = 0; cardIndex < choiceCards.Count; cardIndex++)
			{
				Actor actor = choiceCards[cardIndex].GetActor();
				if (actor == null)
				{
					global::Log.Spells.PrintError("{0}.OnEnter(): Choice card {1} doesn't have an actor!", this, choiceCards[cardIndex]);
					Finish();
					return;
				}
				actorGameObjects[cardIndex] = actor.gameObject;
			}
			FsmArray choiceCardActorGameObjects = m_ChoiceCardActorGameObjects;
			object[] values = actorGameObjects;
			choiceCardActorGameObjects.Values = values;
		}
		Finish();
	}
}
