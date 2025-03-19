using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Update Card Actor's tech level mana gem.")]
[ActionCategory(ActionCategory.GameObject)]
public class UpdateTechLevelManaGem : FsmStateAction
{
	[Tooltip("The Game Object to update.")]
	[RequiredField]
	public FsmGameObject actorObject;

	public override void Reset()
	{
		actorObject = null;
	}

	public override void OnEnter()
	{
		CheckUpdateTechLevelManaGem();
		Finish();
	}

	private void CheckUpdateTechLevelManaGem()
	{
		if (actorObject == null)
		{
			return;
		}
		GameObject go = actorObject.Value;
		if (go == null)
		{
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			return;
		}
		int techLevel = 0;
		if (actor.UseTechLevelManaGem())
		{
			if (actor.GetEntity() != null)
			{
				techLevel = actor.GetEntity().GetTechLevel();
			}
			else if (actor.GetEntityDef() != null)
			{
				techLevel = actor.GetEntityDef().GetTechLevel();
			}
		}
		if (techLevel != 0)
		{
			actor.m_manaObject.SetActive(value: false);
			Spell techLevelSpell = actor.GetSpell(SpellType.TECH_LEVEL_MANA_GEM);
			if (techLevelSpell != null)
			{
				techLevelSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("TechLevel").Value = techLevel;
				techLevelSpell.ActivateState(SpellStateType.BIRTH);
			}
		}
	}
}
