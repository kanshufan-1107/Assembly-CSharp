using System.Collections;
using System.Collections.Generic;
using PegasusGame;
using UnityEngine;

public class RitualTriggerSpell : SuperSpell
{
	public RitualSpellConfig m_ritualSpellConfig;

	public float m_minTimeRitualTriggerSpellPlays = 2f;

	private Entity m_proxyRitualEntity;

	private Actor m_proxyRitualActor;

	private Spell m_ritualPortalSpellInstance;

	private RitualActivateSpell m_linkedSpellInstance;

	public override bool AddPowerTargets()
	{
		Player sourceController = m_taskList.GetSourceEntity().GetController();
		if (!m_ritualSpellConfig.m_showRitualVisualsInPlay && m_ritualSpellConfig.IsRitualEntityInPlay(sourceController))
		{
			return false;
		}
		int proxyRitualEntityId = sourceController.GetTag(m_ritualSpellConfig.m_proxyRitualEntityTag);
		m_proxyRitualEntity = GameState.Get().GetEntity(proxyRitualEntityId);
		if (m_proxyRitualEntity == null)
		{
			Log.Spells.PrintError("RitualTriggerSpell.AddPowerTargets(): Failed to get proxy ritual entity. Unable to display visuals. Proxy ritual entity ID: {0}, Proxy ritual entity tag: {1}", proxyRitualEntityId, m_ritualSpellConfig.m_proxyRitualEntityTag);
			return false;
		}
		if (!m_ritualSpellConfig.DoesTaskListContainRitualEntity(m_taskList, proxyRitualEntityId))
		{
			return false;
		}
		return base.AddPowerTargets();
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		if (InitPortalEffect())
		{
			m_linkedSpellInstance = GetRitualActivateSpell();
			if (m_linkedSpellInstance != null)
			{
				m_linkedSpellInstance.SetHasRitualTriggerSpell(hasSpell: true);
			}
			StartCoroutine(DoPortalAndTransformEffect());
		}
	}

	private RitualActivateSpell GetRitualActivateSpell()
	{
		for (PowerTaskList taskList = m_taskList; taskList != null; taskList = taskList.GetParent())
		{
			if (taskList.GetBlockType() == HistoryBlock.Type.POWER)
			{
				CardEffect effect = PowerSpellController.GetOrCreateEffect(taskList.GetSourceEntity().GetCard(), taskList);
				if (effect != null)
				{
					RitualActivateSpell spell = effect.GetSpell() as RitualActivateSpell;
					if (spell != null)
					{
						return spell;
					}
				}
			}
		}
		return null;
	}

	private bool InitPortalEffect()
	{
		Spell invokeSpell = m_ritualSpellConfig.GetRitualActivateSpell(m_proxyRitualEntity);
		if (invokeSpell == null)
		{
			return false;
		}
		m_proxyRitualActor = m_ritualSpellConfig.LoadRitualActor(m_proxyRitualEntity);
		if (m_proxyRitualActor == null)
		{
			return false;
		}
		m_ritualSpellConfig.UpdateAndPositionActor(m_proxyRitualActor);
		m_ritualPortalSpellInstance = SpellManager.Get().GetSpell(invokeSpell);
		SpellUtils.SetCustomSpellParent(m_ritualPortalSpellInstance, this);
		m_ritualPortalSpellInstance.AddSpellEventCallback(OnPortalSpellEvent);
		m_ritualPortalSpellInstance.AddStateFinishedCallback(OnPortalSpellStateFinished);
		TransformUtil.AttachAndPreserveLocalTransform(m_ritualPortalSpellInstance.transform, m_proxyRitualActor.transform);
		m_ritualSpellConfig.UpdateRitualActorComponents(m_proxyRitualActor);
		return true;
	}

	private IEnumerator DoPortalAndTransformEffect()
	{
		m_ritualPortalSpellInstance.Activate();
		bool complete = false;
		PowerTaskList.CompleteCallback completeCallback = delegate
		{
			complete = true;
		};
		m_taskList.DoTasks(0, m_taskList.GetTaskList().Count, completeCallback);
		yield return new WaitForSeconds(m_minTimeRitualTriggerSpellPlays);
		while (!complete)
		{
			yield return null;
		}
		Spell spell = ActivateTransformSpell();
		while (spell != null && !spell.IsFinished())
		{
			yield return null;
		}
		m_proxyRitualActor.SetEntity(m_proxyRitualEntity);
		m_proxyRitualActor.SetCardDefFromEntity(m_proxyRitualEntity);
		m_proxyRitualActor.UpdateAllComponents();
		OnSpellFinished();
		OnStateFinished();
		PowerTaskList targetTaskList = m_taskList;
		if (m_linkedSpellInstance != null)
		{
			targetTaskList = m_linkedSpellInstance.GetPowerTaskList();
		}
		while (!CanClosePortal(targetTaskList))
		{
			yield return null;
		}
		m_ritualPortalSpellInstance.ActivateState(SpellStateType.DEATH);
	}

	public bool CanClosePortal(PowerTaskList targetTaskList)
	{
		List<PowerTaskList> futureTaskLists = GameState.Get().GetPowerProcessor().GetPowerQueue()
			.GetList();
		if (futureTaskLists.Count == 0)
		{
			return true;
		}
		PowerTaskList nextTaskList = futureTaskLists[0];
		if (nextTaskList == null)
		{
			return true;
		}
		if (nextTaskList.IsDescendantOfBlock(targetTaskList))
		{
			return false;
		}
		return true;
	}

	private void OnPortalSpellEvent(string eventName, object eventData, object userData)
	{
		if (eventName != m_ritualSpellConfig.m_portalSpellEventName)
		{
			Log.Spells.PrintError("RitualTriggerSpell received unexpected Spell Event {0}. Expected {1}", eventName, m_ritualSpellConfig.m_portalSpellEventName);
		}
		else if (m_ritualSpellConfig.m_hideRitualActor)
		{
			m_proxyRitualActor.Show();
		}
	}

	private void OnPortalSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			Object.Destroy(m_proxyRitualActor.gameObject);
			if (m_linkedSpellInstance != null)
			{
				m_linkedSpellInstance.SetHasRitualTriggerSpell(hasSpell: false);
				m_linkedSpellInstance.OnPortalSpellFinished();
			}
		}
	}

	private Spell ActivateTransformSpell()
	{
		Spell transformSpell = m_ritualSpellConfig.GetRitualTriggerSpell(m_proxyRitualEntity);
		if (transformSpell == null)
		{
			return null;
		}
		Spell spell = SpellManager.Get().GetSpell(transformSpell);
		spell.AddStateFinishedCallback(OnTransformSpellStateFinished);
		UpdateAndPositionTransformSpell(spell);
		SpellUtils.SetCustomSpellParent(spell, m_proxyRitualActor);
		TransformUtil.AttachAndPreserveLocalTransform(spell.transform, m_proxyRitualActor.transform);
		spell.ActivateState(SpellStateType.ACTION);
		return spell;
	}

	private void OnTransformSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			SpellManager.Get().ReleaseSpell(spell);
		}
	}

	public override bool CanPurge()
	{
		if (m_taskList != null && !m_taskList.IsEndOfBlock())
		{
			return false;
		}
		if (m_ritualPortalSpellInstance != null && m_ritualPortalSpellInstance.IsActive())
		{
			return false;
		}
		return true;
	}

	private void UpdateAndPositionActor(Actor actor)
	{
		if (!(actor == null))
		{
			if (m_ritualSpellConfig.m_hideRitualActor)
			{
				actor.Hide();
			}
			Transform invokeBone = Board.Get().FindBone(GetRitualBoneName());
			actor.transform.parent = invokeBone;
			actor.transform.localPosition = Vector3.zero;
		}
	}

	private void UpdateAndPositionTransformSpell(Spell spell)
	{
		if (!(spell == null))
		{
			Transform invokeBone = Board.Get().FindBone(GetRitualBoneName());
			spell.transform.parent = invokeBone;
			spell.transform.localPosition = Vector3.zero;
		}
	}

	private string GetRitualBoneName()
	{
		if (m_proxyRitualEntity != null)
		{
			if (m_proxyRitualEntity.GetControllerSide() != Player.Side.FRIENDLY)
			{
				return m_ritualSpellConfig.m_opponentBoneName;
			}
			return m_ritualSpellConfig.m_friendlyBoneName;
		}
		return string.Empty;
	}
}
