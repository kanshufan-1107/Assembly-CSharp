using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using PegasusGame;
using UnityEngine;

public class QuestCompleteSpell : Spell
{
	[Header("Quest card animation settings")]
	public float m_QuestCardScaleTime = 1f;

	public float m_QuestCardHoldTime = 1f;

	public Transform m_QuestStartBone;

	public Transform m_OpponentQuestStartBone;

	public Transform m_QuestEndBone;

	[Header("Quest reward - Default animation settings")]
	public float m_QuestRewardHoldTime = 1f;

	public AnimationEventDispatcher m_AnimationEventDispatcher;

	public Transform m_QuestRewardBone;

	[Header("Quest reward - Custom animation settings")]
	public Spell m_CustomRewardSpellPrefab;

	private Entity m_originalQuestEntity;

	private Actor m_fakeQuestActor;

	private Actor m_fakeQuestRewardActor;

	private Entity m_questReward;

	private int m_questRewardSpawnTaskIndex;

	public override bool AddPowerTargets()
	{
		if (!CanAddPowerTargets())
		{
			return false;
		}
		if (m_taskList.GetBlockType() != HistoryBlock.Type.TRIGGER)
		{
			return false;
		}
		m_originalQuestEntity = m_taskList.GetSourceEntity(warnIfNull: false);
		if (!m_originalQuestEntity.IsQuest() && !m_originalQuestEntity.IsQuestline())
		{
			Log.Spells.PrintError("QuestCompleteSpell.AddPowerTargets(): QuestCompleteSpell has been hooked up to a Card that is not a quest!");
			return false;
		}
		if (!FindQuestRewardFullEntityTask())
		{
			return false;
		}
		if (!LoadFakeQuestActors())
		{
			return false;
		}
		return true;
	}

	private bool FindQuestRewardFullEntityTask()
	{
		List<PowerTask> taskList = m_taskList.GetTaskList();
		for (int i = 0; i < taskList.Count; i++)
		{
			if (taskList[i].GetPower() is Network.HistFullEntity fullEntity)
			{
				m_questRewardSpawnTaskIndex = i;
				m_questReward = GameState.Get().GetEntity(fullEntity.Entity.ID);
				Log.Spells.PrintDebug("QuestCompleteSpell.FindQuestRewardFullEntityTask(): Found reward at task index:{0}, entityId:{1}", m_questRewardSpawnTaskIndex, fullEntity.Entity.ID);
				return true;
			}
		}
		return false;
	}

	private bool LoadFakeQuestActors()
	{
		if (!LoadFakeQuestActor())
		{
			return false;
		}
		if (m_CustomRewardSpellPrefab == null && !LoadFakeQuestRewardActor())
		{
			return false;
		}
		return true;
	}

	private bool LoadFakeQuestActor()
	{
		GameObject fakeQuestActorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(m_originalQuestEntity), AssetLoadingOptions.IgnorePrefabPosition);
		if (fakeQuestActorGO == null)
		{
			Log.Spells.PrintError("QuestCompleteSpell.LoadFakeQuestActor(): Unable to load hand actor for entity {0}.", m_originalQuestEntity);
			return false;
		}
		GetComponent<PlayMakerFSM>().FsmVariables.GetFsmGameObject("RewardCard").Value = fakeQuestActorGO;
		m_fakeQuestActor = fakeQuestActorGO.GetComponentInChildren<Actor>();
		m_fakeQuestActor.SetEntity(m_originalQuestEntity);
		m_fakeQuestActor.SetCardDefFromEntity(m_originalQuestEntity);
		m_fakeQuestActor.SetPremium(m_originalQuestEntity.GetPremiumType());
		m_fakeQuestActor.SetWatermarkCardSetOverride(m_originalQuestEntity.GetWatermarkCardSetOverride());
		m_fakeQuestActor.UpdateAllComponents();
		m_fakeQuestActor.Hide();
		return true;
	}

	private bool LoadFakeQuestRewardActor()
	{
		string questRewardCardID = "";
		if (m_originalQuestEntity.IsQuest())
		{
			questRewardCardID = QuestController.GetRewardCardIDFromQuestCardID(m_originalQuestEntity);
		}
		else if (m_originalQuestEntity.IsQuestline())
		{
			questRewardCardID = QuestlineController.GetRewardCardIDFromQuestCardID(m_originalQuestEntity);
		}
		if (string.IsNullOrEmpty(questRewardCardID))
		{
			Log.Spells.PrintError("QuestCompleteSpell.LoadFakeQuestRewardActor(): No reward card ID found for quest card ID {0}.", m_originalQuestEntity.GetCardId());
			return false;
		}
		if (m_questReward.GetCardId() != questRewardCardID)
		{
			return false;
		}
		using DefLoader.DisposableFullDef questRewardDef = DefLoader.Get().GetFullDef(questRewardCardID);
		if (questRewardDef?.CardDef == null || questRewardDef?.EntityDef == null)
		{
			Log.Spells.PrintError("QuestCompleteSpell.LoadFakeQuestRewardActor(): Unable to load def for card ID {0}.", questRewardCardID);
			return false;
		}
		GameObject fakeQuestRewardActorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetHandActor(questRewardDef.EntityDef, m_originalQuestEntity.GetPremiumType()), AssetLoadingOptions.IgnorePrefabPosition);
		if (fakeQuestRewardActorGO == null)
		{
			Log.Spells.PrintError("QuestCompleteSpell.LoadFakeQuestRewardActor(): Unable to load Hand Actor for entity def {0}.", questRewardDef.EntityDef);
			return false;
		}
		FsmGameObject fsmObject = GetComponent<PlayMakerFSM>().FsmVariables.GetFsmGameObject("QuestSecondCard");
		if (fsmObject != null)
		{
			fsmObject.Value = fakeQuestRewardActorGO;
		}
		m_fakeQuestRewardActor = fakeQuestRewardActorGO.GetComponentInChildren<Actor>();
		m_fakeQuestRewardActor.SetFullDef(questRewardDef);
		m_fakeQuestRewardActor.SetPremium(m_originalQuestEntity.GetPremiumType());
		m_fakeQuestRewardActor.SetCardBackSideOverride(m_originalQuestEntity.GetControllerSide());
		m_fakeQuestRewardActor.SetWatermarkCardSetOverride(m_originalQuestEntity.GetWatermarkCardSetOverride());
		m_fakeQuestRewardActor.UpdateDynamicTextFromQuestEntity(m_originalQuestEntity);
		m_fakeQuestRewardActor.UpdateAllComponents();
		m_fakeQuestRewardActor.Hide();
		TransformUtil.CopyWorld(m_fakeQuestRewardActor, m_QuestRewardBone);
		return true;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		m_originalQuestEntity.GetCard().HideCard();
		StartCoroutine(ScaleUpFakeQuestActor());
	}

	private IEnumerator ScaleUpFakeQuestActor()
	{
		m_fakeQuestActor.Show();
		Transform startBone = ((m_originalQuestEntity.GetControllerSide() == Player.Side.FRIENDLY) ? m_QuestStartBone : m_OpponentQuestStartBone);
		if (startBone != null && m_QuestEndBone != null)
		{
			TransformUtil.CopyWorld(m_fakeQuestActor, startBone);
			iTween.MoveTo(m_fakeQuestActor.gameObject, m_QuestEndBone.position, m_QuestCardScaleTime);
			iTween.ScaleTo(m_fakeQuestActor.gameObject, m_QuestEndBone.localScale, m_QuestCardScaleTime);
		}
		yield return new WaitForSeconds(m_QuestCardScaleTime + m_QuestCardHoldTime);
		ActivateState(SpellStateType.DEATH);
	}

	protected override void OnDeath(SpellStateType prevStateType)
	{
		base.OnDeath(prevStateType);
		if (m_CustomRewardSpellPrefab != null)
		{
			Log.Spells.PrintDebug("QuestCompleteSpell.OnDeath(): Register custom reward spell");
			m_AnimationEventDispatcher.RegisterAnimationEventListener(OnCustomAnimationEvent);
		}
		else
		{
			Log.Spells.PrintDebug("QuestCompleteSpell.OnDeath(): Register default reward spell");
			m_AnimationEventDispatcher.RegisterAnimationEventListener(OnDefaultAnimationEvent);
		}
	}

	private IEnumerator WaitForRewardActor()
	{
		Card questRewardCard = m_questReward.GetCard();
		questRewardCard.SetDoNotSort(on: true);
		questRewardCard.SetDoNotWarpToNewZone(on: true);
		questRewardCard.HideCard();
		Log.Spells.PrintDebug("QuestCompleteSpell.WaitForRewardActor(): Start processing tasks up to reward task");
		m_taskList.DoTasks(0, m_questRewardSpawnTaskIndex + 1);
		while (questRewardCard.GetActor() == null || questRewardCard.IsActorLoading())
		{
			bool hasActor = questRewardCard.GetActor() != null;
			Log.Spells.PrintDebug("QuestCompleteSpell.WaitForRewardActor(): hasActor: {0}, IsActorLoading:{1}", hasActor, questRewardCard.IsActorLoading());
			yield return null;
		}
	}

	private void OnCustomAnimationEvent(Object obj)
	{
		m_AnimationEventDispatcher.UnregisterAnimationEventListener(OnCustomAnimationEvent);
		m_fakeQuestActor.Hide();
		StartCoroutine(RunCustomRewardAnimation());
	}

	private IEnumerator RunCustomRewardAnimation()
	{
		yield return WaitForRewardActor();
		Log.Spells.PrintDebug("QuestCompleteSpell.RunCustomRewardAnimation(): Reward actor ready");
		Card questRewardCard = m_questReward.GetCard();
		Transform rewardCardPosition = questRewardCard.GetZone().GetZoneTransformForCard(questRewardCard);
		TransformUtil.CopyWorld(questRewardCard, rewardCardPosition);
		Spell spell = SpellManager.Get().GetSpell(m_CustomRewardSpellPrefab);
		SpellUtils.SetCustomSpellParent(spell, questRewardCard.GetActor());
		spell.SetSource(questRewardCard.gameObject);
		spell.AddFinishedCallback(OnCustomRewardSpellFinished);
		spell.AddStateFinishedCallback(OnCustomRewardSpellStateFinished);
		spell.Activate();
		Log.Spells.PrintDebug("QuestCompleteSpell.RunCustomRewardAnimation(): Activated custom spell");
	}

	private void OnCustomRewardSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			Log.Spells.PrintDebug("QuestCompleteSpell.OnCustomRewardSpellStateFinished(): NONE state reached");
			SpellManager.Get().ReleaseSpell(spell);
			OnStateFinished();
		}
	}

	private void OnCustomRewardSpellFinished(Spell spell, object userData)
	{
		Log.Spells.PrintDebug("QuestCompleteSpell.OnCustomRewardSpellFinished()");
		Card questRewardCard = m_questReward.GetCard();
		questRewardCard.SetDoNotSort(on: false);
		questRewardCard.SetDoNotWarpToNewZone(on: false);
		questRewardCard.GetZone().UpdateLayout();
		if (questRewardCard.GetZone() is ZoneHeroPower)
		{
			questRewardCard.DisableHeroPowerFlipSoundOnce();
			questRewardCard.ActivateStateSpells();
		}
		OnSpellFinished();
	}

	private void OnDefaultAnimationEvent(Object obj)
	{
		m_AnimationEventDispatcher.UnregisterAnimationEventListener(OnDefaultAnimationEvent);
		m_fakeQuestRewardActor.Show();
		m_fakeQuestActor.Hide();
		StartCoroutine(MoveRewardToHand());
	}

	private IEnumerator MoveRewardToHand()
	{
		yield return new WaitForSeconds(m_QuestRewardHoldTime);
		if (m_questReward.GetZone() != TAG_ZONE.SETASIDE)
		{
			yield return WaitForRewardActor();
			Card questRewardCard = m_questReward.GetCard();
			if (questRewardCard.GetEntity().IsHidden())
			{
				yield return StartCoroutine(SpellUtils.FlipActorAndReplaceWithCard(m_fakeQuestRewardActor, questRewardCard, 0.5f));
			}
			else
			{
				TransformUtil.CopyWorld(questRewardCard, m_fakeQuestRewardActor);
				m_fakeQuestRewardActor.Hide();
			}
			ZoneTransitionStyle zoneTransitionStyle = ((questRewardCard.GetControllerSide() == Player.Side.FRIENDLY) ? ZoneTransitionStyle.SLOW : ZoneTransitionStyle.NORMAL);
			questRewardCard.SetTransitionStyle(zoneTransitionStyle);
			questRewardCard.SetDoNotSort(on: false);
			questRewardCard.SetDoNotWarpToNewZone(on: false);
			questRewardCard.GetZone().UpdateLayout();
			if (questRewardCard.GetZone() is ZoneBattlegroundQuestReward)
			{
				questRewardCard.ActivateStateSpells();
			}
		}
		else
		{
			yield return new WaitForSeconds(m_QuestRewardHoldTime);
			m_fakeQuestRewardActor.Hide();
		}
		OnSpellFinished();
		OnStateFinished();
	}
}
