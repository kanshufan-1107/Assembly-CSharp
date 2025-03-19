using System.Collections;
using PegasusGame;
using UnityEngine;

public class CastSpellCardFromHandSepll : Spell
{
	[SerializeField]
	private float m_BigCardDisplayTime = 1f;

	public override bool AddPowerTargets()
	{
		foreach (PowerTask task in m_taskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type != Network.PowerType.META_DATA)
			{
				continue;
			}
			Network.HistMetaData metaData = (Network.HistMetaData)power;
			if (metaData.MetaType == HistoryMeta.Type.TARGET && metaData.Info.Count != 0)
			{
				int entityID = metaData.Info[0];
				Entity entity = GameState.Get().GetEntity(entityID);
				if (entity != null && entity.GetZone() == TAG_ZONE.HAND)
				{
					Card card = entity.GetCard();
					AddTarget(card.gameObject);
					return true;
				}
			}
		}
		return false;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		StartCoroutine(DoEffectWithTiming());
		base.OnAction(prevStateType);
	}

	private IEnumerator DoEffectWithTiming()
	{
		Card card = GetTargetCard();
		Player controller = card.GetController();
		card.SetDoNotSort(on: true);
		bool complete;
		if (controller.IsFriendlySide())
		{
			yield return StartCoroutine(MoveCardToBigCardSpot());
			yield return StartCoroutine(PlayPowerUpSpell());
			complete = true;
		}
		else
		{
			yield return StartCoroutine(ShowBigCard());
			complete = true;
		}
		while (!complete)
		{
			yield return null;
		}
		card.SetDoNotSort(on: false);
		OnSpellFinished();
		OnStateFinished();
	}

	private IEnumerator ShowBigCard()
	{
		Card targetCard = GetTargetCard();
		targetCard.HideCard();
		Entity entity = targetCard.GetEntity();
		UpdateTags(entity);
		HistoryManager.Get().CreatePlayedBigCard(entity, delegate
		{
		}, delegate
		{
		}, fromMetaData: true, countered: false, (int)(m_BigCardDisplayTime * 1000f));
		yield return new WaitForSeconds(m_BigCardDisplayTime);
	}

	private void UpdateTags(Entity entity)
	{
		foreach (PowerTask task in m_taskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type != Network.PowerType.SHOW_ENTITY)
			{
				continue;
			}
			Network.Entity netEnt = (power as Network.HistShowEntity).Entity;
			if (netEnt.ID != entity.GetEntityId())
			{
				continue;
			}
			entity.LoadCard(netEnt.CardID);
			{
				foreach (Network.Entity.Tag netTag in netEnt.Tags)
				{
					entity.SetTag(netTag.Name, netTag.Value);
				}
				return;
			}
		}
		foreach (PowerTask task2 in m_taskList.GetTaskList())
		{
			Network.PowerHistory power2 = task2.GetPower();
			if (power2.Type == Network.PowerType.TAG_CHANGE)
			{
				Network.HistTagChange tagChange = (Network.HistTagChange)power2;
				if (tagChange.Entity == entity.GetEntityId() && (tagChange.Tag == 219 || tagChange.Tag == 199 || tagChange.Tag == 48))
				{
					entity.SetTag(tagChange.Tag, tagChange.Value);
				}
			}
		}
	}

	private IEnumerator MoveCardToBigCardSpot()
	{
		while (HistoryManager.Get().IsShowingBigCard())
		{
			yield return null;
		}
		Card targetCard = GetTargetCard();
		string boneName = HistoryManager.Get().GetBigCardBoneName();
		Transform bone = Board.Get().FindBone(boneName);
		iTween.MoveTo(targetCard.gameObject, bone.position, m_BigCardDisplayTime);
		iTween.RotateTo(targetCard.gameObject, bone.rotation.eulerAngles, m_BigCardDisplayTime);
		iTween.ScaleTo(targetCard.gameObject, new Vector3(1f, 1f, 1f), m_BigCardDisplayTime);
		SoundManager.Get().LoadAndPlay("play_card_from_hand_1.prefab:ac4be75e319a97947a68308a08e54e88");
		yield return new WaitForSeconds(m_BigCardDisplayTime);
	}

	private IEnumerator PlayPowerUpSpell()
	{
		Card card = GetTargetCard();
		Spell powerUpSpell = card.GetActor().GetSpell(SpellType.POWER_UP);
		if (powerUpSpell == null)
		{
			yield break;
		}
		bool complete = false;
		powerUpSpell.AddStateFinishedCallback(delegate(Spell spell, SpellStateType prevStateType, object userData)
		{
			if (prevStateType == SpellStateType.BIRTH)
			{
				complete = true;
			}
		});
		powerUpSpell.ActivateState(SpellStateType.BIRTH);
		while (!complete)
		{
			yield return null;
		}
		powerUpSpell.Deactivate();
	}
}
