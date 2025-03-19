using System.Collections;
using Blizzard.T5.Core;
using PegasusLettuce;
using UnityEngine;

public class LettucePvPMissionEntity : LettuceMissionEntity
{
	private Spell m_versusSpell;

	private static readonly Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly Map<GameEntityOption, string> s_stringOptions = InitStringOptions();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.WAIT_FOR_RATING_INFO,
			true
		} };
	}

	private static Map<GameEntityOption, string> InitStringOptions()
	{
		return new Map<GameEntityOption, string>();
	}

	public LettucePvPMissionEntity()
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
		m_enemyAbilityOrderSpeechBubblesEnabled = false;
		Network.Get().RegisterNetHandler(MercenariesPvPRatingUpdate.PacketID.ID, OnRatingChange);
	}

	public override void OnDecommissionGame()
	{
		if (Network.Get() != null)
		{
			Network.Get().RemoveNetHandler(MercenariesPvPRatingUpdate.PacketID.ID, OnRatingChange);
		}
		base.OnDecommissionGame();
	}

	private void OnRatingChange()
	{
		MercenariesPvPRatingUpdate ratingUpdate = Network.Get().MercenariesPvPRatingUpdate();
		base.RatingChangeData = ratingUpdate;
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		PreloadPrefab("Lettuce_VersusSpell.prefab:1dec81ab7c8a7704d9f8b316085937a7", delegate(AssetReference assetRef, GameObject gameObject, object callbackData)
		{
			m_versusSpell = gameObject.GetComponent<Spell>();
			if (m_versusSpell != null)
			{
				m_versusSpell.AddStateFinishedCallback(delegate(Spell spell, SpellStateType prevStateType, object userData)
				{
					if (spell.GetActiveState() == SpellStateType.NONE)
					{
						GameEntity.Coroutines.StartCoroutine(WaitThenDestroyVersusSpell());
					}
				});
			}
		});
	}

	private IEnumerator WaitThenDestroyVersusSpell()
	{
		yield return new WaitForSeconds(10f);
		DestroyVersusSpell();
	}

	public override void OnTagChanged(TagDelta change)
	{
		base.OnTagChanged(change);
		if (change.tag == 2228)
		{
			switch ((SpellStateType)change.newValue)
			{
			case SpellStateType.ACTION:
				ActivateVersusSpellState(SpellStateType.ACTION);
				break;
			case SpellStateType.DEATH:
				ActivateVersusSpellState(SpellStateType.DEATH);
				break;
			}
		}
	}

	protected override void OnLettuceMissionEntityGameSceneLoaded()
	{
		if (GetTag(GAME_TAG.TURN) == 0)
		{
			ActivateVersusSpellState(SpellStateType.BIRTH);
		}
		else
		{
			DestroyVersusSpell();
		}
	}

	private void DestroyVersusSpell()
	{
		if (m_versusSpell != null)
		{
			Object.Destroy(m_versusSpell.gameObject);
			m_versusSpell = null;
		}
	}

	private void ActivateVersusSpellState(SpellStateType stateType)
	{
		if (m_versusSpell != null)
		{
			m_versusSpell.ActivateState(stateType);
		}
	}
}
