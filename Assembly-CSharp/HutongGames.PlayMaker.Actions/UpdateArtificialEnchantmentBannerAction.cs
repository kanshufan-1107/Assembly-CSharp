using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Updates artificial enchantment banner with text and description from custom choice cards.")]
[ActionCategory("Pegasus")]
public class UpdateArtificialEnchantmentBannerAction : SpellAction
{
	public FsmOwnerDefault m_SpellObject;

	[ArrayEditor(VariableType.GameObject, "", 0, 0, 65536)]
	public FsmArray m_EnchantmentBannerGameObjects;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_SpellObject);
	}

	public override void Reset()
	{
		m_EnchantmentBannerGameObjects = null;
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
		if (!m_EnchantmentBannerGameObjects.IsNone)
		{
			List<Card> choiceCards = customChoiceSpell.GetChoiceState().m_cards;
			object[] bannerObjects = m_EnchantmentBannerGameObjects.Values;
			if (choiceCards.Count != bannerObjects.Length)
			{
				global::Log.Spells.PrintError("{0}.OnEnter(): Choice cards and banner objects size mismatch!", this);
				Finish();
				return;
			}
			for (int cardIndex = 0; cardIndex < choiceCards.Count; cardIndex++)
			{
				Actor actor = choiceCards[cardIndex].GetActor();
				if (actor == null)
				{
					global::Log.Spells.PrintError("{0}.OnEnter(): Choice card {1} doesn't have an actor!", this, choiceCards[cardIndex]);
					Finish();
					return;
				}
				GameObject banner = (GameObject)bannerObjects[cardIndex];
				if (banner == null)
				{
					global::Log.Spells.PrintError("{0}.OnEnter(): Banner is null!", this);
					Finish();
					return;
				}
				UberText headerText = banner.transform.Find("HeaderName").GetComponent<UberText>();
				UberText component = banner.transform.Find("BodyText").GetComponent<UberText>();
				string name = actor.GetNameText().Text;
				string description = actor.GetPowersText();
				headerText.Text = name;
				component.Text = description;
			}
		}
		Finish();
	}
}
