using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PegasusGame;
using Unity.Profiling;
using UnityEngine;

public class PowerTaskList
{
	public delegate void CompleteCallback(PowerTaskList taskList, int startIndex, int count, object userData);

	public class DamageInfo
	{
		public Entity m_entity;

		public int m_damage;
	}

	private class ZoneChangeCallbackData
	{
		public int m_startIndex;

		public int m_count;

		public CompleteCallback m_taskListCallback;

		public object m_taskListUserData;
	}

	private int m_id;

	private Network.HistBlockStart m_blockStart;

	private Network.HistBlockEnd m_blockEnd;

	private List<PowerTask> m_tasks = new List<PowerTask>();

	private ZoneChangeList m_zoneChangeList;

	private int m_pendingTasks;

	private bool m_isBatchable;

	private bool m_isDeferrable;

	private int m_deferredSourceId;

	private PowerTaskList m_previous;

	private PowerTaskList m_next;

	private PowerTaskList m_subSpellOrigin;

	private Network.HistSubSpellStart m_subSpellStart;

	private Network.HistSubSpellEnd m_subSpellEnd;

	private PowerTaskList m_parent;

	private bool m_attackDataBuilt;

	private AttackInfo m_attackInfo;

	private AttackType m_attackType;

	private Entity m_attacker;

	private Entity m_defender;

	private Entity m_proposedDefender;

	private bool m_repeatProposed;

	private bool m_willCompleteHistoryEntry;

	private Entity m_invokeEntityClone;

	private int? m_lastBattlecryEffectIndex;

	private float m_taskListStartTime;

	private float m_taskListEndTime;

	private int m_taskListSlushTimeMilliseconds = -1;

	private bool m_isHistoryBlockStart;

	private bool m_isHistoryBlockEnd;

	private bool m_collapsible;

	private List<Entity> m_sourceEntities = new List<Entity>();

	private bool m_sourceEntitiesAreDirty = true;

	private static ProfilerMarker s_getSourceEntitiesMarker = new ProfilerMarker("PowerProcessor.GetSourceEntities");

	public int GetId()
	{
		return m_id;
	}

	public void SetId(int id)
	{
		m_id = id;
	}

	public int GetDeferredSourceId()
	{
		return m_deferredSourceId;
	}

	public void SetDeferredSourceId(int id)
	{
		m_deferredSourceId = id;
	}

	public void AddTasks(PowerTaskList otherTaskList)
	{
		m_tasks.AddRange(otherTaskList.m_tasks);
		m_pendingTasks += otherTaskList.m_pendingTasks;
	}

	public void SetProcessStartTime()
	{
		m_taskListStartTime = Time.realtimeSinceStartup;
		GameState.Get().GetPowerProcessor().HandleTimelineStartEvent(m_id, m_taskListStartTime, m_isHistoryBlockStart, GetBlockStart());
	}

	public void SetProcessEndTime()
	{
		m_taskListEndTime = Time.realtimeSinceStartup;
		GameState.Get().GetPowerProcessor().HandleTimelineEndEvent(m_id, m_taskListEndTime, m_isHistoryBlockEnd);
	}

	public void SetDeferrable(bool deferrable)
	{
		m_isDeferrable = deferrable;
	}

	public bool IsDeferrable()
	{
		return m_isDeferrable;
	}

	public void SetBatchable(bool batchable)
	{
		m_isBatchable = batchable;
	}

	public bool IsBatchable()
	{
		if (m_isBatchable && m_blockStart != null)
		{
			return m_blockEnd != null;
		}
		return false;
	}

	public bool IsCollapsible(bool isEarlier)
	{
		if (isEarlier && !m_collapsible)
		{
			return false;
		}
		bool isArtificialInterrupt = false;
		if (m_tasks.Count > 0)
		{
			PowerTask lastTask = m_tasks[m_tasks.Count - 1];
			if (lastTask.GetPower() is Network.HistMetaData)
			{
				isArtificialInterrupt = ((Network.HistMetaData)lastTask.GetPower()).MetaType == HistoryMeta.Type.ARTIFICIAL_HISTORY_INTERRUPT;
			}
		}
		if (m_subSpellStart != null && !isEarlier)
		{
			return false;
		}
		if (m_subSpellEnd != null && isEarlier)
		{
			return false;
		}
		if (m_isHistoryBlockStart && !isEarlier)
		{
			return false;
		}
		if (m_isHistoryBlockEnd && isEarlier)
		{
			return false;
		}
		return !isArtificialInterrupt;
	}

	public void SetCollapsible(bool collapsible)
	{
		m_collapsible = collapsible;
	}

	public bool IsSlushTimeHelper()
	{
		if (m_tasks.Count != 1)
		{
			return false;
		}
		if (m_tasks[0].GetPower() is Network.HistMetaData)
		{
			return ((Network.HistMetaData)m_tasks[0].GetPower()).MetaType == HistoryMeta.Type.SLUSH_TIME;
		}
		return false;
	}

	public bool HasAnyTasksInImmediate()
	{
		return m_tasks.Count > 0;
	}

	public void SetHistoryBlockStart(bool isStart)
	{
		m_isHistoryBlockStart = isStart;
	}

	public void SetHistoryBlockEnd(bool isEnd)
	{
		m_isHistoryBlockEnd = isEnd;
	}

	public void OnTaskCompleted()
	{
		if (--m_pendingTasks == 0)
		{
			OnTaskListCompleted();
		}
	}

	public bool IsEmpty()
	{
		PowerTaskList origin = GetOrigin();
		if (origin.m_blockStart != null)
		{
			return false;
		}
		if (origin.m_blockEnd != null)
		{
			return false;
		}
		if (origin.m_tasks.Count > 0)
		{
			return false;
		}
		return true;
	}

	public bool IsOrigin()
	{
		return m_previous == null;
	}

	public void FillMetaDataTargetSourceData()
	{
		foreach (PowerTask task in m_tasks)
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type == Network.PowerType.META_DATA)
			{
				Network.HistMetaData metaData = (Network.HistMetaData)power;
				if (metaData.MetaType == HistoryMeta.Type.TARGET && metaData.Data == 0)
				{
					metaData.Data = GetSourceEntity()?.GetEntityId() ?? 0;
				}
			}
		}
	}

	public PowerTaskList GetOrigin()
	{
		PowerTaskList taskList = this;
		while (taskList.m_previous != null)
		{
			taskList = taskList.m_previous;
		}
		return taskList;
	}

	public PowerTaskList GetPrevious()
	{
		return m_previous;
	}

	public void SetPrevious(PowerTaskList taskList)
	{
		m_previous = taskList;
		taskList.m_next = this;
	}

	public PowerTaskList GetNext()
	{
		return m_next;
	}

	public void SetNext(PowerTaskList next)
	{
		m_next = next;
	}

	public PowerTaskList GetLast()
	{
		PowerTaskList taskList = this;
		while (taskList.m_next != null)
		{
			taskList = taskList.m_next;
		}
		return taskList;
	}

	public Network.HistBlockStart GetBlockStart()
	{
		return GetOrigin().m_blockStart;
	}

	public void SetBlockStart(Network.HistBlockStart blockStart)
	{
		m_blockStart = blockStart;
	}

	public Network.HistBlockEnd GetBlockEnd()
	{
		return m_blockEnd;
	}

	public void SetBlockEnd(Network.HistBlockEnd blockEnd)
	{
		m_blockEnd = blockEnd;
	}

	public PowerTaskList GetParent()
	{
		return GetOrigin().m_parent;
	}

	public void SetParent(PowerTaskList parent)
	{
		m_parent = parent;
	}

	public PowerTaskList GetParentWithBlockType(HistoryBlock.Type type)
	{
		for (PowerTaskList blockParent = GetParent(); blockParent != null; blockParent = blockParent.GetParent())
		{
			if (blockParent.IsBlockType(type))
			{
				return blockParent;
			}
		}
		return null;
	}

	public Network.HistSubSpellStart GetSubSpellStart()
	{
		return m_subSpellStart;
	}

	public void SetSubSpellStart(Network.HistSubSpellStart subSpellStart)
	{
		m_subSpellStart = subSpellStart;
	}

	public Network.HistSubSpellEnd GetSubSpellEnd()
	{
		return m_subSpellEnd;
	}

	public void SetSubSpellEnd(Network.HistSubSpellEnd subSpellEnd)
	{
		m_subSpellEnd = subSpellEnd;
	}

	public PowerTaskList GetSubSpellOrigin()
	{
		return m_subSpellOrigin;
	}

	public void SetSubSpellOrigin(PowerTaskList taskList)
	{
		m_subSpellOrigin = taskList;
	}

	public bool IsBlock()
	{
		return GetOrigin().m_blockStart != null;
	}

	public bool IsStartOfBlock()
	{
		if (!IsBlock())
		{
			return false;
		}
		return m_blockStart != null;
	}

	public bool IsEndOfBlock()
	{
		if (!IsBlock())
		{
			return false;
		}
		return m_blockEnd != null;
	}

	public bool IsEarlierInBlockThan(PowerTaskList taskList)
	{
		if (taskList == null)
		{
			return false;
		}
		for (PowerTaskList currList = taskList.m_previous; currList != null; currList = currList.m_previous)
		{
			if (this == currList)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsLaterInBlockThan(PowerTaskList taskList)
	{
		if (taskList == null)
		{
			return false;
		}
		for (PowerTaskList currList = taskList.m_next; currList != null; currList = currList.m_next)
		{
			if (this == currList)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInBlock(PowerTaskList taskList)
	{
		if (this == taskList)
		{
			return true;
		}
		if (IsEarlierInBlockThan(taskList))
		{
			return true;
		}
		if (IsLaterInBlockThan(taskList))
		{
			return true;
		}
		return false;
	}

	public bool IsDescendantOfBlock(PowerTaskList taskList)
	{
		if (taskList == null)
		{
			return false;
		}
		if (IsInBlock(taskList))
		{
			return true;
		}
		PowerTaskList originTaskList = taskList.GetOrigin();
		for (PowerTaskList currList = GetParent(); currList != null; currList = currList.m_parent)
		{
			if (currList == originTaskList)
			{
				return true;
			}
		}
		return false;
	}

	public List<PowerTask> GetTaskList()
	{
		return m_tasks;
	}

	public bool HasTasks()
	{
		return m_tasks.Count > 0;
	}

	public PowerTask CreateTask(Network.PowerHistory netPower)
	{
		m_pendingTasks++;
		PowerTask task = new PowerTask();
		task.SetPower(netPower);
		task.SetTaskCompleteCallback(OnTaskCompleted);
		m_tasks.Add(task);
		return task;
	}

	public bool GetTagUpdatedValue(Entity entity, GAME_TAG tag, ref int current)
	{
		if (entity == null)
		{
			return false;
		}
		bool found = false;
		List<PowerTask> tasks = GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			if (power.Type == Network.PowerType.TAG_CHANGE)
			{
				Network.HistTagChange tagChange = (Network.HistTagChange)power;
				if (tagChange.Entity == entity.GetEntityId() && tagChange.Tag == (int)tag)
				{
					current = tagChange.Value;
					found = true;
				}
			}
		}
		return found;
	}

	public Entity GetSourceEntity(bool warnIfNull = true)
	{
		Network.HistBlockStart blockStart = GetBlockStart();
		if (blockStart == null)
		{
			return null;
		}
		using (List<int>.Enumerator enumerator = blockStart.Entities.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				int entityId = enumerator.Current;
				Entity entity = GameState.Get().GetEntity(entityId);
				if (entity == null && warnIfNull && !GameState.Get().EntityRemovedFromGame(entityId))
				{
					string msg = $"PowerProcessor.GetSourceEntity() - task list {m_id} has a source entity with id {entityId} but there is no entity with that id";
					Log.Power.PrintWarning(msg);
					return null;
				}
				return entity;
			}
		}
		return null;
	}

	public List<Entity> GetSourceEntities(bool warnIfNull = true)
	{
		using (s_getSourceEntitiesMarker.Auto())
		{
			Network.HistBlockStart blockStart = GetBlockStart();
			if (blockStart == null)
			{
				return null;
			}
			if (!m_sourceEntitiesAreDirty)
			{
				return m_sourceEntities;
			}
			m_sourceEntities.Clear();
			List<int> entityIds = blockStart.Entities;
			m_sourceEntities.Capacity = entityIds.Count;
			foreach (int entityId in entityIds)
			{
				Entity entity = GameState.Get().GetEntity(entityId);
				if (entity == null && warnIfNull && !GameState.Get().EntityRemovedFromGame(entityId))
				{
					string msg = $"PowerProcessor.GetSourceEntity() - task list {m_id} has a source entity with id {entityId} but there is no entity with that id";
					Log.Power.PrintWarning(msg);
					return null;
				}
				m_sourceEntities.Add(entity);
			}
			m_sourceEntitiesAreDirty = false;
			return m_sourceEntities;
		}
	}

	public void SetSourceEntitiesDirty()
	{
		m_sourceEntitiesAreDirty = true;
	}

	public bool IsEffectCardIdClientCached(int entityId)
	{
		Network.HistBlockStart blockStart = GetBlockStart();
		if (blockStart == null)
		{
			return false;
		}
		int index = 0;
		using (List<int>.Enumerator enumerator = blockStart.Entities.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current != entityId)
			{
				index++;
			}
		}
		if (index >= blockStart.IsEffectCardIdClientCached.Count)
		{
			return false;
		}
		return blockStart.IsEffectCardIdClientCached[index];
	}

	public string GetEffectCardId(int entityId)
	{
		Network.HistBlockStart blockStart = GetBlockStart();
		if (blockStart == null)
		{
			return null;
		}
		int index = 0;
		using (List<int>.Enumerator enumerator = blockStart.Entities.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current != entityId)
			{
				index++;
			}
		}
		if (index >= blockStart.EffectCardId.Count)
		{
			return null;
		}
		string cardId = blockStart.EffectCardId[index];
		if (!string.IsNullOrEmpty(cardId))
		{
			return cardId;
		}
		return GetSourceEntity()?.GetCardId();
	}

	public EntityDef GetEffectEntityDef(int entityId)
	{
		string cardId = GetEffectCardId(entityId);
		if (string.IsNullOrEmpty(cardId))
		{
			return null;
		}
		return DefLoader.Get().GetEntityDef(cardId);
	}

	public string GetEffectCardId()
	{
		Entity entity = GetSourceEntity();
		if (entity == null)
		{
			return null;
		}
		return GetEffectCardId(entity.GetEntityId());
	}

	public EntityDef GetEffectEntityDef()
	{
		Entity entity = GetSourceEntity();
		if (entity == null)
		{
			return null;
		}
		return GetEffectEntityDef(entity.GetEntityId());
	}

	public Entity GetTargetEntity(bool warnIfNull = true)
	{
		Network.HistBlockStart blockStart = GetBlockStart();
		if (blockStart == null)
		{
			return null;
		}
		int entityId = blockStart.Target;
		Entity entity = GameState.Get().GetEntity(entityId);
		if (entity == null && warnIfNull && !GameState.Get().EntityRemovedFromGame(entityId))
		{
			string msg = $"PowerProcessor.GetTargetEntity() - task list {m_id} has a target entity with id {entityId} but there is no entity with that id";
			Log.Power.PrintWarning(msg);
			return null;
		}
		return entity;
	}

	public bool HasTargetEntity()
	{
		Network.HistBlockStart blockStart = GetBlockStart();
		if (blockStart == null)
		{
			return false;
		}
		int targetEntityId = blockStart.Target;
		return GameState.Get().GetEntity(targetEntityId) != null;
	}

	public bool HasMetaDataTasks()
	{
		foreach (PowerTask task in m_tasks)
		{
			if (task.GetPower().Type == Network.PowerType.META_DATA)
			{
				return true;
			}
		}
		return false;
	}

	public bool DoesBlockHaveMetaDataTasks()
	{
		for (PowerTaskList taskList = GetOrigin(); taskList != null; taskList = taskList.m_next)
		{
			if (taskList.HasMetaDataTasks())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCardDraw()
	{
		foreach (PowerTask task in m_tasks)
		{
			if (task.IsCardDraw())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCardMill()
	{
		foreach (PowerTask task in m_tasks)
		{
			if (task.IsCardMill())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFatigue()
	{
		foreach (PowerTask task in m_tasks)
		{
			if (task.IsFatigue())
			{
				return true;
			}
		}
		return false;
	}

	public int GetTotalSlushTime()
	{
		if (m_taskListSlushTimeMilliseconds > -1)
		{
			return m_taskListSlushTimeMilliseconds;
		}
		int totalSlush = 0;
		foreach (PowerTask task in m_tasks)
		{
			if (task.GetPower() is Network.HistMetaData { MetaType: HistoryMeta.Type.SLUSH_TIME } metaData)
			{
				totalSlush += metaData.Data;
			}
		}
		m_taskListSlushTimeMilliseconds = totalSlush;
		return totalSlush;
	}

	public bool HasEffectTimingMetaData()
	{
		foreach (PowerTask task in m_tasks)
		{
			if (task.GetPower() is Network.HistMetaData metaData)
			{
				if (metaData.MetaType == HistoryMeta.Type.TARGET)
				{
					return true;
				}
				if (metaData.MetaType == HistoryMeta.Type.EFFECT_TIMING)
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<PowerTask> GetTagChangeTasks()
	{
		List<PowerTask> tagChangeTasks = new List<PowerTask>();
		foreach (PowerTask task in m_tasks)
		{
			if (task.GetPower() is Network.HistTagChange)
			{
				tagChangeTasks.Add(task);
			}
		}
		return tagChangeTasks;
	}

	public bool DoesBlockHaveEffectTimingMetaData()
	{
		for (PowerTaskList taskList = GetOrigin(); taskList != null; taskList = taskList.m_next)
		{
			if (taskList.GetSubSpellOrigin() == GetSubSpellOrigin() && taskList.HasEffectTimingMetaData())
			{
				return true;
			}
		}
		return false;
	}

	public HistoryBlock.Type GetBlockType()
	{
		return GetBlockStart()?.BlockType ?? HistoryBlock.Type.INVALID;
	}

	public bool IsBlockType(HistoryBlock.Type type)
	{
		Network.HistBlockStart blockStart = GetBlockStart();
		if (blockStart == null)
		{
			return false;
		}
		return blockStart.BlockType == type;
	}

	public bool IsPlayBlock()
	{
		return IsBlockType(HistoryBlock.Type.PLAY);
	}

	public bool IsTriggerBlock()
	{
		return IsBlockType(HistoryBlock.Type.TRIGGER);
	}

	public bool IsDeathBlock()
	{
		return IsBlockType(HistoryBlock.Type.DEATHS);
	}

	public bool IsSubSpellTaskList()
	{
		return m_subSpellOrigin != null;
	}

	public void DoTasks(int startIndex, int count)
	{
		DoTasks(startIndex, count, null, null);
	}

	public void DoTasks(int startIndex, int count, CompleteCallback callback)
	{
		DoTasks(startIndex, count, callback, null);
	}

	public void DoTasks(int startIndex, int count, CompleteCallback callback, object userData)
	{
		bool giveToZoneMgr = false;
		int incompleteStartIndex = -1;
		int endIndex = Mathf.Min(startIndex + count - 1, m_tasks.Count - 1);
		for (int i = startIndex; i <= endIndex; i++)
		{
			PowerTask task = m_tasks[i];
			if (!task.IsCompleted())
			{
				if (incompleteStartIndex < 0)
				{
					incompleteStartIndex = i;
				}
				if (ZoneMgr.IsHandledPower(task.GetPower()))
				{
					giveToZoneMgr = true;
					break;
				}
			}
		}
		if (incompleteStartIndex < 0)
		{
			incompleteStartIndex = startIndex;
		}
		if (giveToZoneMgr)
		{
			ZoneChangeCallbackData callbackData = new ZoneChangeCallbackData();
			callbackData.m_startIndex = startIndex;
			callbackData.m_count = count;
			callbackData.m_taskListCallback = callback;
			callbackData.m_taskListUserData = userData;
			m_zoneChangeList = ZoneMgr.Get().AddServerZoneChanges(this, incompleteStartIndex, endIndex, OnZoneChangeComplete, callbackData);
			if (m_zoneChangeList != null)
			{
				return;
			}
		}
		if (Gameplay.Get() != null)
		{
			WaitForGameStateAndDoTasks(incompleteStartIndex, endIndex, startIndex, count, callback, userData, Gameplay.Get().TaskToken).Forget();
		}
		else
		{
			DoTasks(incompleteStartIndex, endIndex, startIndex, count, callback, userData);
		}
	}

	public void DoAllTasks(CompleteCallback callback)
	{
		DoTasks(0, m_tasks.Count, callback, null);
	}

	public void DoAllTasks()
	{
		DoTasks(0, m_tasks.Count, null, null);
	}

	public void DoEarlyConcedeTasks()
	{
		for (int i = 0; i < m_tasks.Count; i++)
		{
			m_tasks[i].DoEarlyConcedeTask();
		}
	}

	public bool IsComplete()
	{
		if (!AreTasksComplete())
		{
			return false;
		}
		if (!AreZoneChangesComplete())
		{
			return false;
		}
		return true;
	}

	public bool AreTasksComplete()
	{
		foreach (PowerTask task in m_tasks)
		{
			if (!task.IsCompleted())
			{
				return false;
			}
		}
		return true;
	}

	public Card GetStartDrawMetaDataCard()
	{
		for (int i = 0; i < m_tasks.Count; i++)
		{
			Network.PowerHistory power = m_tasks[i].GetPower();
			if (power.Type != Network.PowerType.META_DATA)
			{
				continue;
			}
			Network.HistMetaData metaData = (Network.HistMetaData)power;
			if (metaData.MetaType == HistoryMeta.Type.START_DRAW)
			{
				Entity entity = GameState.Get().GetEntity(metaData.Info[0]);
				if (entity != null)
				{
					return entity.GetCard();
				}
			}
		}
		return null;
	}

	public int FindEarlierIncompleteTaskIndex(int taskIndex)
	{
		for (int i = taskIndex - 1; i >= 0; i--)
		{
			if (!m_tasks[i].IsCompleted())
			{
				return i;
			}
		}
		return -1;
	}

	public bool HasEarlierIncompleteTask(int taskIndex)
	{
		return FindEarlierIncompleteTaskIndex(taskIndex) >= 0;
	}

	public bool HasZoneChanges()
	{
		return m_zoneChangeList != null;
	}

	public bool AreZoneChangesComplete()
	{
		if (m_zoneChangeList == null)
		{
			return true;
		}
		return m_zoneChangeList.IsComplete();
	}

	public AttackType GetAttackType()
	{
		BuildAttackData();
		return m_attackType;
	}

	public Entity GetAttacker()
	{
		BuildAttackData();
		return m_attacker;
	}

	public Entity GetDefender()
	{
		BuildAttackData();
		return m_defender;
	}

	public Entity GetProposedDefender()
	{
		BuildAttackData();
		return m_proposedDefender;
	}

	public bool IsRepeatProposedAttack()
	{
		BuildAttackData();
		return m_repeatProposed;
	}

	public bool HasGameOver()
	{
		for (int i = 0; i < m_tasks.Count; i++)
		{
			Network.PowerHistory power = m_tasks[i].GetPower();
			if (power.Type == Network.PowerType.TAG_CHANGE)
			{
				Network.HistTagChange tagChange = (Network.HistTagChange)power;
				if (GameUtils.IsGameOverTag(tagChange.Entity, tagChange.Tag, tagChange.Value))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasFriendlyConcede()
	{
		for (int i = 0; i < m_tasks.Count; i++)
		{
			Network.PowerHistory power = m_tasks[i].GetPower();
			if (power.Type == Network.PowerType.TAG_CHANGE && GameUtils.IsFriendlyConcede((Network.HistTagChange)power))
			{
				return true;
			}
		}
		return false;
	}

	public DamageInfo GetDamageInfo(Entity entity)
	{
		if (entity == null)
		{
			return null;
		}
		int entityId = entity.GetEntityId();
		foreach (PowerTask task in m_tasks)
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type == Network.PowerType.TAG_CHANGE)
			{
				Network.HistTagChange tagChange = power as Network.HistTagChange;
				if (tagChange.Tag == 44 && tagChange.Entity == entityId)
				{
					DamageInfo info = new DamageInfo();
					info.m_entity = GameState.Get().GetEntity(tagChange.Entity);
					info.m_damage = tagChange.Value - info.m_entity.GetDamage();
					return info;
				}
			}
		}
		return null;
	}

	public void SetWillCompleteHistoryEntry(bool set)
	{
		m_willCompleteHistoryEntry = set;
	}

	public bool WillCompleteHistoryEntry()
	{
		return m_willCompleteHistoryEntry;
	}

	public bool WillBlockCompleteHistoryEntry()
	{
		for (PowerTaskList taskList = GetOrigin(); taskList != null; taskList = taskList.m_next)
		{
			if (taskList.WillCompleteHistoryEntry())
			{
				return true;
			}
		}
		return false;
	}

	public bool WasThePlayedSpellCountered(Entity entity)
	{
		foreach (PowerTask task in m_tasks)
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type == Network.PowerType.TAG_CHANGE)
			{
				Network.HistTagChange tagChange = power as Network.HistTagChange;
				if (tagChange.Entity == entity.GetEntityId() && tagChange.Tag == 231 && tagChange.Value == 1)
				{
					return true;
				}
			}
		}
		foreach (PowerTaskList list in GameState.Get().GetPowerProcessor().GetPowerQueue()
			.GetList())
		{
			foreach (PowerTask task2 in list.GetTaskList())
			{
				Network.PowerHistory power2 = task2.GetPower();
				if (power2.Type == Network.PowerType.TAG_CHANGE)
				{
					Network.HistTagChange tagChange2 = power2 as Network.HistTagChange;
					if (tagChange2.Entity == entity.GetEntityId() && tagChange2.Tag == 231 && tagChange2.Value == 1)
					{
						return true;
					}
				}
			}
			if (list.GetBlockEnd() != null && list.GetBlockStart().BlockType == HistoryBlock.Type.PLAY)
			{
				return false;
			}
		}
		return false;
	}

	public void CreateArtificialHistoryTilesFromMetadata()
	{
		List<PowerTask> tasksToInclude = new List<PowerTask>();
		bool buildingArtificialTile = false;
		foreach (PowerTask task in GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type == Network.PowerType.META_DATA)
			{
				Network.HistMetaData metadata = (Network.HistMetaData)power;
				if (metadata.MetaType == HistoryMeta.Type.BEGIN_ARTIFICIAL_HISTORY_TILE || metadata.MetaType == HistoryMeta.Type.BEGIN_ARTIFICIAL_HISTORY_TRIGGER_TILE)
				{
					int metaDataEntityId = metadata.Info[0];
					Entity metaDataEntity = GameState.Get().GetEntity(metaDataEntityId);
					if (metaDataEntity != null)
					{
						if (buildingArtificialTile)
						{
							NotifyHistoryOfAdditionalTargets(tasksToInclude);
							HistoryManager.Get().MarkCurrentHistoryEntryAsCompleted();
							tasksToInclude.Clear();
						}
						else
						{
							buildingArtificialTile = true;
						}
						if (metadata.MetaType == HistoryMeta.Type.BEGIN_ARTIFICIAL_HISTORY_TRIGGER_TILE)
						{
							HistoryManager.Get().CreateTriggerTile(metaDataEntity);
						}
						else
						{
							HistoryManager.Get().CreatePlayedTile(metaDataEntity, null);
						}
					}
				}
				else if (buildingArtificialTile && metadata.MetaType == HistoryMeta.Type.END_ARTIFICIAL_HISTORY_TILE)
				{
					buildingArtificialTile = false;
					NotifyHistoryOfAdditionalTargets(tasksToInclude);
					HistoryManager.Get().MarkCurrentHistoryEntryAsCompleted();
					tasksToInclude.Clear();
				}
				else if (buildingArtificialTile)
				{
					tasksToInclude.Add(task);
				}
			}
			else if (buildingArtificialTile)
			{
				tasksToInclude.Add(task);
			}
		}
		if (buildingArtificialTile)
		{
			NotifyHistoryOfAdditionalTargets(tasksToInclude);
			HistoryManager.Get().MarkCurrentHistoryEntryAsCompleted();
		}
	}

	public void NotifyHistoryOfAdditionalTargets(List<PowerTask> tasksToInclude = null)
	{
		if (tasksToInclude == null)
		{
			tasksToInclude = GetTaskList();
		}
		bool buildingArtificialTile = false;
		List<int> sourceEntityIds = GetBlockStart()?.Entities;
		List<int> entitiesThatHaveDied = new List<int>();
		List<int> entitiesThatHaveMovedToSetAside = new List<int>();
		bool ignoreDamage = true;
		foreach (PowerTask item in tasksToInclude)
		{
			Network.PowerHistory power = item.GetPower();
			if (buildingArtificialTile)
			{
				if (power.Type == Network.PowerType.META_DATA && ((Network.HistMetaData)power).MetaType == HistoryMeta.Type.END_ARTIFICIAL_HISTORY_TILE)
				{
					buildingArtificialTile = false;
				}
			}
			else if (power.Type == Network.PowerType.META_DATA)
			{
				Network.HistMetaData metadata = (Network.HistMetaData)power;
				if (metadata.MetaType == HistoryMeta.Type.TARGET)
				{
					for (int i = 0; i < metadata.Info.Count; i++)
					{
						HistoryManager.Get().NotifyEntityAffected(metadata.Info[i], allowDuplicates: false, fromMetaData: false);
					}
				}
				else if (metadata.MetaType == HistoryMeta.Type.DAMAGE || metadata.MetaType == HistoryMeta.Type.HEALING)
				{
					ignoreDamage = false;
				}
				else if (metadata.MetaType == HistoryMeta.Type.OVERRIDE_HISTORY)
				{
					HistoryManager.Get().OverrideCurrentHistoryEntryWithMetaData();
				}
				else if (metadata.MetaType == HistoryMeta.Type.HISTORY_TARGET)
				{
					for (int j = 0; j < metadata.Info.Count; j++)
					{
						int metaDataEntityId = metadata.Info[j];
						Entity metaDataEntity = GameState.Get().GetEntity(metaDataEntityId);
						if (metaDataEntity != null)
						{
							HistoryManager.Get().NotifyEntityAffected(metaDataEntity, allowDuplicates: false, fromMetaData: true);
						}
					}
				}
				else if (metadata.MetaType == HistoryMeta.Type.HISTORY_TRIGGER_SOURCE)
				{
					if (metadata.Info.Count > 0)
					{
						Entity metaDataEntity2 = GameState.Get().GetEntity(metadata.Info[0]);
						HistoryManager.Get().OverrideCurrentHistoryTriggerSource(metaDataEntity2);
					}
				}
				else if (metadata.MetaType == HistoryMeta.Type.HISTORY_SOURCE_OWNER)
				{
					if (metadata.Info.Count > 0)
					{
						Entity metaDataEntity3 = GameState.Get().GetEntity(metadata.Info[0]);
						HistoryManager.Get().OverrideCurrentHistorySourceOwner(metaDataEntity3);
					}
				}
				else if (metadata.MetaType == HistoryMeta.Type.BURNED_CARD)
				{
					for (int k = 0; k < metadata.Info.Count; k++)
					{
						int metaDataEntityId2 = metadata.Info[k];
						Entity metaDataEntity4 = GameState.Get().GetEntity(metaDataEntityId2);
						if (metaDataEntity4 != null)
						{
							HistoryManager.Get().NotifyEntityAffected(metaDataEntity4, allowDuplicates: false, fromMetaData: true, dontDuplicateUntilEnd: false, isBurnedCard: true);
						}
					}
				}
				else if (metadata.MetaType == HistoryMeta.Type.POISONOUS)
				{
					for (int l = 0; l < metadata.Info.Count; l++)
					{
						int metaDataEntityId3 = metadata.Info[l];
						Entity metaDataEntity5 = GameState.Get().GetEntity(metaDataEntityId3);
						if (metaDataEntity5 != null)
						{
							HistoryManager.Get().NotifyEntityAffected(metaDataEntity5, allowDuplicates: false, fromMetaData: true, dontDuplicateUntilEnd: false, isBurnedCard: false, isPoisonous: true);
						}
					}
				}
				else if (metadata.MetaType == HistoryMeta.Type.CRITICAL_HIT)
				{
					for (int m = 0; m < metadata.Info.Count; m++)
					{
						int metaDataEntityId4 = metadata.Info[m];
						Entity metaDataEntity6 = GameState.Get().GetEntity(metaDataEntityId4);
						if (metaDataEntity6 != null)
						{
							HistoryManager.Get().NotifyEntityAffected(metaDataEntity6, allowDuplicates: false, fromMetaData: true, dontDuplicateUntilEnd: false, isBurnedCard: false, isPoisonous: false, isCriticalHit: true);
						}
					}
				}
				else if (metadata.MetaType == HistoryMeta.Type.HISTORY_TARGET_DONT_DUPLICATE_UNTIL_END)
				{
					for (int n = 0; n < metadata.Info.Count; n++)
					{
						int metaDataEntityId5 = metadata.Info[n];
						Entity metaDataEntity7 = GameState.Get().GetEntity(metaDataEntityId5);
						if (metaDataEntity7 != null)
						{
							HistoryManager.Get().NotifyEntityAffected(metaDataEntity7, allowDuplicates: true, fromMetaData: true, dontDuplicateUntilEnd: true);
						}
					}
				}
				else if (metadata.MetaType == HistoryMeta.Type.BEGIN_ARTIFICIAL_HISTORY_TILE || metadata.MetaType == HistoryMeta.Type.BEGIN_ARTIFICIAL_HISTORY_TRIGGER_TILE)
				{
					buildingArtificialTile = true;
				}
				else if (metadata.MetaType == HistoryMeta.Type.HISTORY_REMOVE_ENTITIES)
				{
					for (int num = 0; num < metadata.Info.Count; num++)
					{
						HistoryManager.Get().NotifyRemoveEntityFromAffectedList(metadata.Info[num]);
					}
				}
			}
			else if (power.Type == Network.PowerType.SHOW_ENTITY)
			{
				Network.HistShowEntity showEnt = (Network.HistShowEntity)power;
				bool isEnchantment = false;
				bool isInGraveyard = false;
				bool isInSetAside = false;
				Entity entity = GameState.Get().GetEntity(showEnt.Entity.ID);
				bool wasInHand = entity.GetZone() == TAG_ZONE.HAND;
				bool wasInSetAside = entity.GetZone() == TAG_ZONE.SETASIDE;
				foreach (Network.Entity.Tag tag in showEnt.Entity.Tags)
				{
					if (tag.Name == 202 && tag.Value == 6)
					{
						isEnchantment = true;
						break;
					}
					if (tag.Name == 49)
					{
						if (tag.Value == 4)
						{
							isInGraveyard = true;
						}
						else if (tag.Value == 6)
						{
							isInSetAside = true;
						}
					}
				}
				if (!isEnchantment && !(isInGraveyard && wasInSetAside) && !(isInSetAside && wasInSetAside))
				{
					if (isInGraveyard && !wasInHand)
					{
						HistoryManager.Get().NotifyEntityDied(showEnt.Entity.ID);
					}
					else
					{
						HistoryManager.Get().NotifyEntityAffected(showEnt.Entity.ID, allowDuplicates: false, fromMetaData: false);
					}
				}
			}
			else if (power.Type == Network.PowerType.FULL_ENTITY)
			{
				Network.HistFullEntity fullEnt = (Network.HistFullEntity)power;
				bool dontShowInHistory = false;
				bool isInBattlefield = false;
				bool isCreatedBySourceAction = false;
				foreach (Network.Entity.Tag tag2 in fullEnt.Entity.Tags)
				{
					GAME_TAG tagName = (GAME_TAG)tag2.Name;
					if (tagName == GAME_TAG.DONT_SHOW_IN_HISTORY && tag2.Value != 0)
					{
						dontShowInHistory = true;
						break;
					}
					if (tagName == GAME_TAG.CARDTYPE && tag2.Value == 6)
					{
						dontShowInHistory = true;
						break;
					}
					if (tagName == GAME_TAG.ZONE && (tag2.Value == 1 || tag2.Value == 7))
					{
						isInBattlefield = true;
					}
					else if (tagName == GAME_TAG.DISPLAYED_CREATOR && sourceEntityIds != null && sourceEntityIds.Contains(tag2.Value))
					{
						isCreatedBySourceAction = true;
					}
				}
				if (!dontShowInHistory && (isInBattlefield || isCreatedBySourceAction))
				{
					HistoryManager.Get().NotifyEntityAffected(fullEnt.Entity.ID, allowDuplicates: false, fromMetaData: false);
				}
			}
			else
			{
				if (power.Type != Network.PowerType.TAG_CHANGE)
				{
					continue;
				}
				Network.HistTagChange tagChange = (Network.HistTagChange)power;
				if (tagChange.ChangeDef)
				{
					continue;
				}
				Entity tagChangeEntity = GameState.Get().GetEntity(tagChange.Entity);
				if (tagChange.Tag == 44)
				{
					if (!entitiesThatHaveDied.Contains(tagChange.Entity) && !ignoreDamage)
					{
						HistoryManager.Get().NotifyDamageChanged(tagChangeEntity, tagChange.Value);
						ignoreDamage = true;
					}
				}
				else if (tagChange.Tag == 292)
				{
					if (!entitiesThatHaveDied.Contains(tagChange.Entity) && !entitiesThatHaveMovedToSetAside.Contains(tagChange.Entity))
					{
						HistoryManager.Get().NotifyArmorChanged(tagChangeEntity, tagChange.Value);
					}
				}
				else if (tagChange.Tag == 45)
				{
					if (!entitiesThatHaveDied.Contains(tagChange.Entity))
					{
						HistoryManager.Get().NotifyHealthChanged(tagChangeEntity, tagChange.Value);
					}
				}
				else if (tagChange.Tag == 318)
				{
					HistoryManager.Get().NotifyEntityAffected(tagChangeEntity, allowDuplicates: false, fromMetaData: false);
				}
				else if (tagChange.Tag == 385 && sourceEntityIds != null && sourceEntityIds.Contains(tagChange.Value))
				{
					HistoryManager.Get().NotifyEntityAffected(tagChangeEntity, allowDuplicates: false, fromMetaData: false);
				}
				else if (tagChange.Tag == 262)
				{
					HistoryManager.Get().NotifyEntityAffected(tagChangeEntity, allowDuplicates: false, fromMetaData: false);
				}
				if (GameUtils.IsHistoryDeathTagChange(tagChange))
				{
					HistoryManager.Get().NotifyEntityDied(tagChangeEntity);
					entitiesThatHaveDied.Add(tagChange.Entity);
				}
				if (GameUtils.IsHistoryMovedToSetAsideTagChange(tagChange))
				{
					entitiesThatHaveMovedToSetAside.Add(tagChange.Entity);
				}
				if (GameUtils.IsHistoryDiscardTagChange(tagChange))
				{
					HistoryManager.Get().NotifyEntityAffected(tagChangeEntity, allowDuplicates: false, fromMetaData: false);
				}
			}
		}
	}

	public bool ShouldCreatePlayBlockHistoryTile()
	{
		if (HistoryManager.Get() == null || !HistoryManager.Get().IsHistoryEnabled())
		{
			return false;
		}
		if (!IsPlayBlock())
		{
			return false;
		}
		PowerTaskList parentTaskList = GetParent();
		if (parentTaskList == null)
		{
			return true;
		}
		Entity parentSourceEntity = parentTaskList.GetSourceEntity();
		if (parentSourceEntity != null && parentSourceEntity.HasTag(GAME_TAG.CAST_RANDOM_SPELLS))
		{
			return false;
		}
		return true;
	}

	public void SetActivateBattlecrySpellState()
	{
		PowerTaskList blockParent = GetParentWithBlockType(HistoryBlock.Type.PLAY);
		if (blockParent != null)
		{
			Network.HistBlockStart blockStart = GetBlockStart();
			if (blockStart != null)
			{
				blockParent.m_lastBattlecryEffectIndex = blockStart.EffectIndex;
			}
		}
	}

	public bool ShouldActivateBattlecrySpell()
	{
		if (!IsOrigin())
		{
			return false;
		}
		PowerTaskList blockParent = GetParentWithBlockType(HistoryBlock.Type.PLAY);
		if (blockParent == null)
		{
			return false;
		}
		Network.HistBlockStart blockStart = GetBlockStart();
		if (blockStart == null)
		{
			return false;
		}
		if (blockParent.m_lastBattlecryEffectIndex.HasValue && blockParent.m_lastBattlecryEffectIndex != blockStart.EffectIndex)
		{
			return false;
		}
		return true;
	}

	public void DebugDump()
	{
		GameState gameState = GameState.Get();
		if (gameState == null || gameState.GameScenarioAllowsPowerPrinting())
		{
			DebugDump(Log.Power);
		}
	}

	public void DebugDump(Logger logger)
	{
		if (!logger.CanPrint())
		{
			return;
		}
		GameState state = GameState.Get();
		string indentation = string.Empty;
		int parentId = ((m_parent != null) ? m_parent.GetId() : 0);
		int previousId = ((m_previous != null) ? m_previous.GetId() : 0);
		logger.Print("PowerTaskList.DebugDump() - ID={0} ParentID={1} PreviousID={2} TaskCount={3}", m_id, parentId, previousId, m_tasks.Count);
		if (m_blockStart == null)
		{
			logger.Print("PowerTaskList.DebugDump() - {0}Block Start=(null)", indentation);
			indentation += "    ";
		}
		else
		{
			state.DebugPrintPower(logger, "PowerTaskList", m_blockStart, ref indentation);
		}
		for (int i = 0; i < m_tasks.Count; i++)
		{
			Network.PowerHistory netPower = m_tasks[i].GetPower();
			state.DebugPrintPower(logger, "PowerTaskList", netPower, ref indentation);
		}
		if (m_blockEnd == null)
		{
			if (indentation.Length >= "    ".Length)
			{
				indentation = indentation.Remove(indentation.Length - "    ".Length);
			}
			logger.Print("PowerTaskList.DebugDump() - {0}Block End=(null)", indentation);
		}
		else
		{
			state.DebugPrintPower(logger, "PowerTaskList", m_blockEnd, ref indentation);
		}
	}

	public override string ToString()
	{
		return $"id={m_id} tasks={m_tasks.Count} prevId={((m_previous != null) ? m_previous.GetId() : 0)} nextId={((m_next != null) ? m_next.GetId() : 0)} parentId={((m_parent != null) ? m_parent.GetId() : 0)}";
	}

	private void OnZoneChangeComplete(ZoneChangeList changeList, object userData)
	{
		ZoneChangeCallbackData callbackData = (ZoneChangeCallbackData)userData;
		if (callbackData.m_taskListCallback != null)
		{
			callbackData.m_taskListCallback(this, callbackData.m_startIndex, callbackData.m_count, callbackData.m_taskListUserData);
		}
	}

	private void OnTaskListCompleted()
	{
		SetProcessEndTime();
	}

	private async UniTaskVoid WaitForGameStateAndDoTasks(int incompleteStartIndex, int endIndex, int startIndex, int count, CompleteCallback callback, object userData, CancellationToken token)
	{
		int i = incompleteStartIndex;
		while (i <= endIndex)
		{
			PowerTask task = m_tasks[i];
			while (!GameState.Get().GetPowerProcessor().CanDoTask(task))
			{
				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}
			task.DoTask();
			while (GameState.Get().IsMulliganBusy())
			{
				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}
			int num = i + 1;
			i = num;
		}
		callback?.Invoke(this, startIndex, count, userData);
	}

	private void DoTasks(int incompleteStartIndex, int endIndex, int startIndex, int count, CompleteCallback callback, object userData)
	{
		for (int i = incompleteStartIndex; i <= endIndex; i++)
		{
			m_tasks[i].DoTask();
		}
		callback?.Invoke(this, startIndex, count, userData);
	}

	private void BuildAttackData()
	{
		if (!m_attackDataBuilt)
		{
			m_attackInfo = BuildAttackInfo();
			m_attackType = DetermineAttackType(out var info);
			m_attacker = null;
			m_defender = null;
			m_proposedDefender = null;
			switch (m_attackType)
			{
			case AttackType.REGULAR:
				m_attacker = info.m_attacker;
				m_defender = info.m_defender;
				break;
			case AttackType.PROPOSED:
				m_attacker = info.m_proposedAttacker;
				m_defender = info.m_proposedDefender;
				m_proposedDefender = info.m_proposedDefender;
				m_repeatProposed = info.m_repeatProposed;
				break;
			case AttackType.CANCELED:
				m_attacker = m_previous.GetAttacker();
				m_proposedDefender = m_previous.GetProposedDefender();
				break;
			case AttackType.ONLY_ATTACKER:
				m_attacker = info.m_attacker;
				break;
			case AttackType.ONLY_DEFENDER:
				m_defender = info.m_defender;
				break;
			case AttackType.ONLY_PROPOSED_ATTACKER:
				m_attacker = info.m_proposedAttacker;
				break;
			case AttackType.ONLY_PROPOSED_DEFENDER:
				m_proposedDefender = info.m_proposedDefender;
				m_defender = info.m_proposedDefender;
				break;
			case AttackType.WAITING_ON_PROPOSED_ATTACKER:
			case AttackType.WAITING_ON_PROPOSED_DEFENDER:
			case AttackType.WAITING_ON_ATTACKER:
			case AttackType.WAITING_ON_DEFENDER:
				m_attacker = m_previous.GetAttacker();
				m_defender = m_previous.GetDefender();
				break;
			}
			m_attackDataBuilt = true;
		}
	}

	private AttackInfo BuildAttackInfo()
	{
		GameState state = GameState.Get();
		AttackInfo info = new AttackInfo();
		bool foundTag = false;
		foreach (PowerTask task in GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type != Network.PowerType.TAG_CHANGE)
			{
				continue;
			}
			Network.HistTagChange tagChange = power as Network.HistTagChange;
			if (tagChange.Tag == 36)
			{
				info.m_defenderTagValue = tagChange.Value;
				if (tagChange.Value == 1)
				{
					info.m_defender = state.GetEntity(tagChange.Entity);
				}
				foundTag = true;
			}
			else if (tagChange.Tag == 38)
			{
				info.m_attackerTagValue = tagChange.Value;
				if (tagChange.Value == 1)
				{
					info.m_attacker = state.GetEntity(tagChange.Entity);
				}
				foundTag = true;
			}
			else if (tagChange.Tag == 39)
			{
				info.m_proposedAttackerTagValue = tagChange.Value;
				if (tagChange.Value != 0)
				{
					info.m_proposedAttacker = state.GetEntity(tagChange.Value);
				}
				foundTag = true;
			}
			else if (tagChange.Tag == 37)
			{
				info.m_proposedDefenderTagValue = tagChange.Value;
				if (tagChange.Value != 0)
				{
					info.m_proposedDefender = state.GetEntity(tagChange.Value);
				}
				foundTag = true;
			}
		}
		if (foundTag)
		{
			return info;
		}
		return null;
	}

	private AttackType DetermineAttackType(out AttackInfo info)
	{
		info = m_attackInfo;
		GameState gameState = GameState.Get();
		GameEntity gameEntity = gameState.GetGameEntity();
		Entity proposedAttacker = gameState.GetEntity(gameEntity.GetTag(GAME_TAG.PROPOSED_ATTACKER));
		Entity proposedDefender = gameState.GetEntity(gameEntity.GetTag(GAME_TAG.PROPOSED_DEFENDER));
		AttackType prevAttackType = AttackType.INVALID;
		Entity prevAttacker = null;
		Entity prevDefender = null;
		if (m_previous != null)
		{
			prevAttackType = m_previous.GetAttackType();
			prevAttacker = m_previous.GetAttacker();
			prevDefender = m_previous.GetDefender();
		}
		if (m_attackInfo != null)
		{
			if (m_attackInfo.m_attacker != null || m_attackInfo.m_defender != null)
			{
				if (m_attackInfo.m_attacker == null)
				{
					if (prevAttackType == AttackType.ONLY_ATTACKER || prevAttackType == AttackType.WAITING_ON_DEFENDER)
					{
						info = new AttackInfo();
						info.m_attacker = prevAttacker;
						info.m_defender = m_attackInfo.m_defender;
						return AttackType.REGULAR;
					}
					return AttackType.ONLY_DEFENDER;
				}
				if (m_attackInfo.m_defender == null)
				{
					if (prevAttackType == AttackType.ONLY_DEFENDER || prevAttackType == AttackType.WAITING_ON_ATTACKER)
					{
						info = new AttackInfo();
						info.m_attacker = m_attackInfo.m_attacker;
						info.m_defender = prevDefender;
						return AttackType.REGULAR;
					}
					return AttackType.ONLY_ATTACKER;
				}
				return AttackType.REGULAR;
			}
			if (m_attackInfo.m_proposedAttacker != null || m_attackInfo.m_proposedDefender != null)
			{
				if (m_attackInfo.m_proposedAttacker == null)
				{
					if (proposedAttacker != null)
					{
						info = new AttackInfo();
						info.m_proposedAttacker = proposedAttacker;
						info.m_proposedDefender = m_attackInfo.m_proposedDefender;
						return AttackType.PROPOSED;
					}
					return AttackType.ONLY_PROPOSED_DEFENDER;
				}
				if (m_attackInfo.m_proposedDefender == null)
				{
					if (proposedDefender != null)
					{
						info = new AttackInfo();
						info.m_proposedAttacker = m_attackInfo.m_proposedAttacker;
						info.m_proposedDefender = proposedDefender;
						return AttackType.PROPOSED;
					}
					return AttackType.ONLY_PROPOSED_ATTACKER;
				}
				return AttackType.PROPOSED;
			}
			if (prevAttackType == AttackType.REGULAR || prevAttackType == AttackType.INVALID)
			{
				return AttackType.INVALID;
			}
		}
		switch (prevAttackType)
		{
		case AttackType.PROPOSED:
			if ((proposedAttacker != null && proposedAttacker.GetZone() != TAG_ZONE.PLAY) || (proposedDefender != null && proposedDefender.GetZone() != TAG_ZONE.PLAY) || (proposedAttacker != null && proposedAttacker.IsDormant()) || (proposedDefender != null && proposedDefender.IsDormant()))
			{
				return AttackType.CANCELED;
			}
			if (prevAttacker != proposedAttacker || prevDefender != proposedDefender)
			{
				if (proposedDefender.IsToBeDestroyed())
				{
					return AttackType.CANCELED;
				}
				info = new AttackInfo();
				info.m_proposedAttacker = proposedAttacker;
				info.m_proposedDefender = proposedDefender;
				return AttackType.PROPOSED;
			}
			if (proposedAttacker != null && proposedDefender != null && !IsEndOfBlock())
			{
				info = new AttackInfo();
				info.m_proposedAttacker = proposedAttacker;
				info.m_proposedDefender = proposedDefender;
				info.m_repeatProposed = true;
				return AttackType.PROPOSED;
			}
			return AttackType.CANCELED;
		case AttackType.CANCELED:
			return AttackType.INVALID;
		default:
			if (IsEndOfBlock())
			{
				if (prevAttackType == AttackType.ONLY_ATTACKER || prevAttackType == AttackType.WAITING_ON_DEFENDER)
				{
					return AttackType.CANCELED;
				}
				Debug.LogWarningFormat("AttackSpellController.DetermineAttackType() - INVALID ATTACK prevAttackType={0} prevAttacker={1} prevDefender={2}", prevAttackType, prevAttacker, prevDefender);
				return AttackType.INVALID;
			}
			switch (prevAttackType)
			{
			case AttackType.ONLY_PROPOSED_ATTACKER:
			case AttackType.WAITING_ON_PROPOSED_DEFENDER:
				return AttackType.WAITING_ON_PROPOSED_DEFENDER;
			case AttackType.ONLY_PROPOSED_DEFENDER:
			case AttackType.WAITING_ON_PROPOSED_ATTACKER:
				return AttackType.WAITING_ON_PROPOSED_ATTACKER;
			case AttackType.ONLY_ATTACKER:
			case AttackType.WAITING_ON_DEFENDER:
				return AttackType.WAITING_ON_DEFENDER;
			case AttackType.ONLY_DEFENDER:
			case AttackType.WAITING_ON_ATTACKER:
				return AttackType.WAITING_ON_ATTACKER;
			default:
				return AttackType.INVALID;
			}
		}
	}

	public void FixupLastTagChangeForEntityTag(int changeEntity, int changeTag, int newValue, bool fixLast = true)
	{
		if (fixLast)
		{
			for (int i = m_tasks.Count - 1; i >= 0; i--)
			{
				if (m_tasks[i].GetPower() is Network.HistTagChange tagChange && changeEntity == tagChange.Entity && changeTag == tagChange.Tag)
				{
					tagChange.Value = newValue;
					break;
				}
			}
			return;
		}
		for (int j = 0; j < m_tasks.Count; j++)
		{
			if (m_tasks[j].GetPower() is Network.HistTagChange tagChange2 && changeEntity == tagChange2.Entity && changeTag == tagChange2.Tag)
			{
				tagChange2.Value = newValue;
				break;
			}
		}
	}
}
