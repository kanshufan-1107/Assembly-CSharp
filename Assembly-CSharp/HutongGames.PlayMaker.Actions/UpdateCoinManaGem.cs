using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory(ActionCategory.GameObject)]
[Tooltip("Update Card Actor's coin mana gem.")]
public class UpdateCoinManaGem : FsmStateAction
{
	[RequiredField]
	[Tooltip("The Game Object to update.")]
	public FsmGameObject actorObject;

	public override void Reset()
	{
		actorObject = null;
	}

	public override void OnEnter()
	{
		CheckUpdateCoinManaGem();
		Finish();
	}

	private void CheckUpdateCoinManaGem()
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
		if (!(actor == null))
		{
			if (actor.UseCoinManaGem() && actor.GetCard().CanShowCoinManaGem())
			{
				actor.m_manaObject.SetActive(value: false);
				actor.ActivateSpellBirthState(SpellType.COIN_MANA_GEM);
			}
			else
			{
				actor.ActivateSpellDeathState(SpellType.COIN_MANA_GEM);
			}
		}
	}
}
