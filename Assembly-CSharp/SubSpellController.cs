using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.Game.Spells.SuperSpells;
using UnityEngine;

public class SubSpellController : SpellController
{
	private class SubSpellInstance
	{
		public Network.HistSubSpellStart SubSpellStartTask;

		public Spell SpellInstance;

		public bool SpellLoaded;
	}

	private Stack<SubSpellInstance> m_subSpellInstanceStack = new Stack<SubSpellInstance>();

	protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
	{
		SubSpellInstance subSpellInstance = GetSubSpellInstanceForTasklist(taskList);
		if (subSpellInstance == null)
		{
			return false;
		}
		if (!SpellUtils.CanAddPowerTargets(taskList))
		{
			CheckForSubSpellEnd(taskList);
			return false;
		}
		Network.HistSubSpellStart subSpellStart = subSpellInstance.SubSpellStartTask;
		Entity sourceEntity = taskList.GetSourceEntity();
		if (subSpellStart.SourceEntityID != 0)
		{
			sourceEntity = GameState.Get().GetEntity(subSpellStart.SourceEntityID);
		}
		Card sourceCard = sourceEntity?.GetCard();
		SetSource(sourceCard);
		if (subSpellStart.TargetEntityIDS.Count > 0)
		{
			foreach (int targetEntityID in subSpellStart.TargetEntityIDS)
			{
				Entity targetEntity = GameState.Get().GetEntity(targetEntityID);
				if (targetEntity != null)
				{
					Card targetCard = targetEntity.GetCard();
					if (!(targetCard == null) && !(sourceCard == targetCard) && !IsTarget(targetCard))
					{
						AddTarget(targetCard);
					}
				}
			}
		}
		else
		{
			List<PowerTask> tasks = m_taskList.GetTaskList();
			for (int i = 0; i < tasks.Count; i++)
			{
				PowerTask task = tasks[i];
				Card targetCard2 = GetTargetCardFromPowerTask(task);
				if (!(targetCard2 == null) && !(sourceCard == targetCard2) && !IsTarget(targetCard2))
				{
					AddTarget(targetCard2);
				}
			}
		}
		int num;
		if (!(sourceCard != null) && m_targets.Count <= 0)
		{
			if (sourceEntity == null)
			{
				num = 0;
				goto IL_016c;
			}
			if (!sourceEntity.IsGame())
			{
				num = (sourceEntity.IsPlayer() ? 1 : 0);
				if (num == 0)
				{
					goto IL_016c;
				}
			}
			else
			{
				num = 1;
			}
		}
		else
		{
			num = 1;
		}
		goto IL_0173;
		IL_016c:
		CheckForSubSpellEnd(taskList);
		goto IL_0173;
		IL_0173:
		return (byte)num != 0;
	}

	private SubSpellInstance GetSubSpellInstanceForTasklist(PowerTaskList taskList)
	{
		SubSpellInstance subSpellInstance = null;
		Network.HistSubSpellStart subSpellStartTask = taskList.GetSubSpellStart();
		if (subSpellStartTask != null)
		{
			subSpellInstance = new SubSpellInstance();
			subSpellInstance.SubSpellStartTask = subSpellStartTask;
			m_subSpellInstanceStack.Push(subSpellInstance);
			AssetReference assetReference = subSpellStartTask.SpellPrefabGUID;
			bool wasSuccess = false;
			if (!GameUtils.IsOnVFXDenylist(assetReference))
			{
				wasSuccess = AssetLoader.Get().InstantiatePrefab(assetReference, OnSubSpellLoadAttempted, subSpellInstance);
			}
			if (!wasSuccess)
			{
				OnSubSpellLoadAttempted(assetReference, null, subSpellInstance);
			}
		}
		else if (m_subSpellInstanceStack.Count > 0)
		{
			subSpellInstance = m_subSpellInstanceStack.Peek();
		}
		return subSpellInstance;
	}

	public void OnSubSpellLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		SubSpellInstance subSpellInstance = (SubSpellInstance)callbackData;
		subSpellInstance.SpellLoaded = true;
		if (m_subSpellInstanceStack.Count <= 0)
		{
			Log.Power.PrintError("{0}.OnSubSpellLoaded(): Loaded GameObject without an active sub-spell! GameObject: {1}", this, go);
			return;
		}
		if (!m_subSpellInstanceStack.Contains(subSpellInstance))
		{
			Log.Power.PrintError("{0}.OnSubSpellLoaded(): SubSpellInstance is not on the active sub-spell stack! GameObject: {1}", this, go);
			return;
		}
		if (go == null)
		{
			Log.Power.PrintError("{0}.OnSubSpellLoaded(): Failed to load spell prefab! Prefab GUID: {1}", this, subSpellInstance.SubSpellStartTask.SpellPrefabGUID);
			return;
		}
		Spell subSpell = go.GetComponent<Spell>();
		if (subSpell == null)
		{
			Object.Destroy(go);
			Log.Power.PrintError("{0}.OnSubSpellLoaded(): Loaded spell prefab doesn't have a Spell component! Spell Prefab: {1}", this, go);
		}
		else if (subSpellInstance.SpellInstance != null)
		{
			Object.Destroy(go);
			Log.Power.PrintError("{0}.OnSubSpellLoaded(): Active SubSpellInstance already has an existing spell. Existing Spell: {1}, New Spell: {2}", this, subSpellInstance.SpellInstance, subSpell);
		}
		else
		{
			subSpell.AddStateFinishedCallback(OnSubSpellStateFinished);
			subSpellInstance.SpellInstance = subSpell;
		}
	}

	protected override void OnProcessTaskList()
	{
		if (m_subSpellInstanceStack.Count <= 0)
		{
			Log.Spells.PrintError("{0}.OnProcessTaskList(): No active sub-spell!", this);
			OnFinishedTaskList();
		}
		else
		{
			StartCoroutine(WaitForSubSpellThenDoTaskList());
		}
	}

	private IEnumerator WaitForSubSpellThenDoTaskList()
	{
		SubSpellInstance subSpellInstance = m_subSpellInstanceStack.Peek();
		while (!subSpellInstance.SpellLoaded)
		{
			yield return null;
		}
		if (!AttachTasklistToSubSpell(m_taskList, subSpellInstance))
		{
			CheckForSubSpellEnd(m_taskList);
			OnFinishedTaskList();
			yield break;
		}
		if (GameState.Get().IsTurnStartManagerActive())
		{
			TurnStartManager.Get().NotifyOfTriggerVisual();
			while (TurnStartManager.Get().IsTurnStartIndicatorShowing())
			{
				yield return null;
			}
		}
		subSpellInstance.SpellInstance.AddFinishedCallback(OnSubSpellFinished);
		subSpellInstance.SpellInstance.ActivateState(SpellStateType.ACTION);
	}

	private bool AttachTasklistToSubSpell(PowerTaskList taskList, SubSpellInstance subSpellInstance)
	{
		if (subSpellInstance.SpellInstance == null)
		{
			return false;
		}
		Spell subSpell = subSpellInstance.SpellInstance;
		Card sourceCard = taskList.GetSourceEntity()?.GetCard();
		if (sourceCard != null)
		{
			subSpell.SetSource(sourceCard.gameObject);
		}
		Network.HistSubSpellStart subSpellStartTask = subSpellInstance.SubSpellStartTask;
		if (subSpell.AttachPowerTaskList(taskList))
		{
			if (subSpellStartTask.SourceEntityID != 0)
			{
				Entity sourceEntity = GameState.Get().GetEntity(subSpellStartTask.SourceEntityID);
				if (sourceEntity != null && sourceEntity.GetCard() != null)
				{
					subSpell.SetSource(sourceEntity.GetCard().gameObject);
					subSpell.Location = SpellLocation.SOURCE;
				}
			}
			else
			{
				Card source = GetSource();
				if (source != null)
				{
					subSpell.SetSource(source.gameObject);
				}
			}
			if (subSpellStartTask.TargetEntityIDS.Count > 0)
			{
				subSpell.RemoveAllTargets();
				subSpell.RemoveAllVisualTargets();
				if (subSpell is ISuperSpell superSpell)
				{
					superSpell.TargetInfo.Behavior = SpellTargetBehavior.DEFAULT;
				}
				foreach (int targetEntityID in subSpellStartTask.TargetEntityIDS)
				{
					Entity targetEntity = GameState.Get().GetEntity(targetEntityID);
					if (targetEntity != null && targetEntity.GetCard() != null)
					{
						subSpell.AddTarget(targetEntity.GetCard().gameObject);
					}
				}
			}
			return true;
		}
		return false;
	}

	private void OnSubSpellFinished(Spell spell, object userData)
	{
		CheckForSubSpellEnd(spell.GetPowerTaskList());
		OnFinishedTaskList();
	}

	private void CheckForSubSpellEnd(PowerTaskList taskList)
	{
		if (taskList.GetSubSpellEnd() != null)
		{
			if (m_subSpellInstanceStack.Count <= 0)
			{
				Log.Spells.PrintError("{0}.CheckForSubSpellEnd(): SubSpellEnd task hit without an active sub-spell!", this);
			}
			else
			{
				m_subSpellInstanceStack.Pop();
			}
		}
	}

	private void OnSubSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() != 0)
		{
			return;
		}
		SubSpellInstance[] array = m_subSpellInstanceStack.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].SpellInstance == spell)
			{
				return;
			}
		}
		if (!(spell is SuperSpell) || !(spell as SuperSpell).m_SkipAutoDestroyForSubspell)
		{
			StartCoroutine(DestroySpellAfterDelay(spell));
		}
	}

	private IEnumerator DestroySpellAfterDelay(Spell spell)
	{
		yield return new WaitForSeconds(10f);
		if (spell != null && spell.gameObject != null)
		{
			SpellManager.Get().ReleaseSpell(spell);
		}
	}
}
