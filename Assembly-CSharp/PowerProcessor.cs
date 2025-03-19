using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Blizzard.T5.Core;
using Cysharp.Threading.Tasks;
using Hearthstone;
using PegasusGame;
using UnityEngine;

public class PowerProcessor
{
	public delegate void OnTaskEvent(float scheduleDiff);

	private class DelayedRealTimeTask
	{
		public PowerTask m_powerTask;

		public List<Network.PowerHistory> m_powerHistory;

		public int m_index;
	}

	private const string ATTACK_SPELL_CONTROLLER_PREFAB_PATH = "AttackSpellController.prefab:12acecc85ac575e43b87ec141b89269a";

	private const string SECRET_SPELL_CONTROLLER_PREFAB_PATH = "SecretSpellController.prefab:553af99c12154c547bc05dc3d9832931";

	private const string SIDE_QUEST_SPELL_CONTROLLER_PREFAB_PATH = "SideQuestSpellController.prefab:63762d08481f04642bbf3cde299feea2";

	private const string SIGIL_SPELL_CONTROLLER_PREFAB_PATH = "SigilSpellController.prefab:1f80634fbf70a654bbae7bf796bf11b2";

	private const string OBJECTIVE_SPELL_CONTROLLER_PREFAB_PATH = "ObjectiveSpellController.prefab:a3d627bc67f24e740a2e967b383ecc6e";

	private const string JOUST_SPELL_CONTROLLER_PREFAB_PATH = "JoustSpellController.prefab:89ac256005a4a8a46939a84460c2c221";

	private const string RITUAL_SPELL_CONTROLLER_PREFAB_PATH = "RitualSpellController.prefab:27c7bd4ffaa54fb4e9e64dad14a6e701";

	private const string REVEAL_CARD_SPELL_CONTROLLER_PREFAB_PATH = "RevealCardSpellController.prefab:17fd7ea79bfd4c24389d535a074199b6";

	private const string TRIGGER_SPELL_CONTROLLER_PREFAB_PATH = "TriggerSpellController.prefab:e0a2661f98a720d47ad4b85de228f4b4";

	private const string RESET_GAME_SPELL_CONTROLLER_PREFAB_PATH = "ResetGameSpellController.prefab:d8c1994d523574e42bffa17990917754";

	private const string SUB_SPELL_CONTROLLER_PREFAB_PATH = "SubSpellController.prefab:34966ff41154fce469d3ccb6d3b1655e";

	private const string INVOKE_SPELL_CONTROLLER_PREFAB_PATH = "InvokeSpellController.prefab:333b9273e033dd348ab0d5f81a5bbbcd";

	private const float STARSHIP_LAUNCH_TRIGGER_DELAY = 2f;

	private const float STARSHIP_LAUNCH_RAYNOR_TRIGGER_DELAY = 0.5f;

	private int m_nextTaskListId = 1;

	private bool m_buildingTaskList;

	private int m_totalSlushTime;

	private PowerHistoryTimeline m_currentTimeline;

	private List<OnTaskEvent> m_taskEventListeners = new List<OnTaskEvent>();

	private Stack<PowerTaskList> m_previousStack = new Stack<PowerTaskList>();

	private Stack<List<PowerTaskList>> m_deferredStack = new Stack<List<PowerTaskList>>();

	private Stack<PowerTaskList> m_subSpellOriginStack = new Stack<PowerTaskList>();

	private Queue<DelayedRealTimeTask> m_delayedRealTimeTasks = new Queue<DelayedRealTimeTask>();

	private PowerQueue m_powerQueue = new PowerQueue();

	private PowerTaskList m_currentTaskList;

	private PowerTaskList m_previousTaskList;

	private SubSpellController m_subSpellController;

	private bool m_historyBlocking;

	private bool m_artificialPauseFromMetadata;

	private PowerTaskList m_historyBlockingTaskList;

	private PowerTaskList m_busyTaskList;

	private PowerTaskList m_earlyConcedeTaskList;

	private bool m_handledFirstEarlyConcede;

	private PowerTaskList m_gameOverTaskList;

	private List<PowerHistoryTimeline> m_powerHistoryTimeline = new List<PowerHistoryTimeline>();

	private Map<int, int> m_powerHistoryTimelineIdIndex = new Map<int, int>();

	private PowerTaskList m_powerHistoryFirstTaskList;

	private PowerTaskList m_powerHistoryLastTaskList;

	private List<PowerTaskList> m_starshipLaunchTriggerTasks;

	private bool m_IsWaitingForStarshipLandingDelay;

	public PowerProcessor()
	{
		m_deferredStack.Push(new List<PowerTaskList>());
	}

	public PowerTaskList GetCurrentTaskList()
	{
		return m_currentTaskList;
	}

	public PowerQueue GetPowerQueue()
	{
		return m_powerQueue;
	}

	public void AddTaskEventListener(OnTaskEvent listener)
	{
		m_taskEventListeners.Add(listener);
	}

	public void RemoveTaskEventListener(OnTaskEvent listener)
	{
		m_taskEventListeners.Remove(listener);
	}

	public void FireTaskEvent(float expectedDiff)
	{
		OnTaskEvent[] array = m_taskEventListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](expectedDiff);
		}
	}

	public void OnMetaData(Network.HistMetaData metaData)
	{
		if (metaData.MetaType == HistoryMeta.Type.SHOW_BIG_CARD)
		{
			int specificPlayerID = metaData.Data;
			Player player = GameState.Get().GetPlayer(specificPlayerID);
			if (player != null && player.GetSide() != Player.Side.FRIENDLY && InputManager.Get().PermitDecisionMakingInput())
			{
				return;
			}
			int metaDataEntityId = metaData.Info[0];
			Entity metaDataEntity = GameState.Get().GetEntity(metaDataEntityId);
			if (metaDataEntity != null && !string.IsNullOrEmpty(metaDataEntity.GetCardId()))
			{
				SetHistoryBlockingTaskList();
				Entity sourceEntity = m_currentTaskList.GetSourceEntity();
				HistoryBlock.Type type = m_currentTaskList.GetBlockType();
				if (sourceEntity != null && sourceEntity.HasTag(GAME_TAG.FAST_BATTLECRY) && type == HistoryBlock.Type.POWER)
				{
					HistoryManager.Get().CreateFastBigCardFromMetaData(metaDataEntity);
					return;
				}
				int displayTimeMS = ((metaData.Info.Count > 1) ? metaData.Info[1] : 0);
				HistoryManager.Get().CreatePlayedBigCard(metaDataEntity, OnBigCardStarted, OnBigCardFinished, fromMetaData: true, countered: false, displayTimeMS);
			}
		}
		else if (metaData.MetaType == HistoryMeta.Type.BEGIN_LISTENING_FOR_TURN_EVENTS)
		{
			TurnStartManager.Get().BeginListeningForTurnEvents(fromMetadata: true);
		}
		else if (metaData.MetaType == HistoryMeta.Type.ARTIFICIAL_PAUSE)
		{
			int pauseTimeMS = metaData.Data;
			if (Gameplay.Get() != null)
			{
				ArtificiallyPausePowerProcessor(pauseTimeMS, Gameplay.Get().PausePowerToken).Forget();
			}
		}
	}

	public async UniTaskVoid ArtificiallyPausePowerProcessor(float pauseTimeMS, CancellationToken token)
	{
		m_artificialPauseFromMetadata = true;
		float timeToWait = pauseTimeMS / 1000f;
		float timeWaited = 0f;
		if (timeToWait > 0f)
		{
			GameState.Get().GetFriendlySidePlayer().GetHandZone()
				.UpdateLayout();
			GameState.Get().GetOpposingSidePlayer().GetHandZone()
				.UpdateLayout();
			GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
				.UpdateLayout();
			GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone()
				.UpdateLayout();
		}
		for (; timeWaited < timeToWait; timeWaited += Time.deltaTime)
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		m_artificialPauseFromMetadata = false;
	}

	public bool IsHistoryBlocking()
	{
		return m_historyBlocking;
	}

	public PowerTaskList GetHistoryBlockingTaskList()
	{
		return m_historyBlockingTaskList;
	}

	public void SetHistoryBlockingTaskList()
	{
		if (m_historyBlockingTaskList == null)
		{
			m_historyBlockingTaskList = m_currentTaskList;
		}
	}

	public void ForceStopHistoryBlocking()
	{
		m_historyBlocking = false;
		m_historyBlockingTaskList = null;
	}

	public PowerTaskList GetLastTaskList()
	{
		int count = m_powerQueue.Count;
		if (count > 0)
		{
			return m_powerQueue[count - 1];
		}
		return m_currentTaskList;
	}

	public bool HasEarlyConcedeTaskList()
	{
		return m_earlyConcedeTaskList != null;
	}

	public bool HasGameOverTaskList()
	{
		return m_gameOverTaskList != null;
	}

	public bool CanDoRealTimeTask()
	{
		GameState gameState = GameState.Get();
		if (gameState == null)
		{
			return false;
		}
		if (gameState.IsResetGamePending())
		{
			return false;
		}
		return true;
	}

	public bool CanDoTask(PowerTask task)
	{
		if (task.IsCompleted())
		{
			return true;
		}
		Network.PowerHistory power = task.GetPower();
		if (power.Type == Network.PowerType.META_DATA && ((Network.HistMetaData)power).MetaType == HistoryMeta.Type.SHOW_BIG_CARD && HistoryManager.Get().IsShowingBigCard())
		{
			return false;
		}
		if (GameState.Get().IsBusy())
		{
			return false;
		}
		if (m_artificialPauseFromMetadata)
		{
			return false;
		}
		return true;
	}

	public void ForEachTaskList(Action<int, PowerTaskList> predicate)
	{
		if (m_currentTaskList != null)
		{
			predicate(-1, m_currentTaskList);
		}
		for (int i = 0; i < m_powerQueue.Count; i++)
		{
			predicate(i, m_powerQueue[i]);
		}
	}

	public bool HasTaskLists()
	{
		if (m_currentTaskList != null)
		{
			return true;
		}
		if (m_powerQueue.Count > 0)
		{
			return true;
		}
		return false;
	}

	public bool HasTaskList(PowerTaskList taskList)
	{
		if (taskList == null)
		{
			return false;
		}
		if (m_currentTaskList == taskList)
		{
			return true;
		}
		if (m_powerQueue.Contains(taskList))
		{
			return true;
		}
		return false;
	}

	public void OnPowerHistory(List<Network.PowerHistory> powerList)
	{
		m_totalSlushTime = 0;
		m_buildingTaskList = true;
		m_powerHistoryFirstTaskList = null;
		m_powerHistoryLastTaskList = null;
		m_currentTimeline = new PowerHistoryTimeline();
		for (int i = 0; i < powerList.Count; i++)
		{
			PowerTaskList taskList = new PowerTaskList();
			if (m_previousStack.Count > 0)
			{
				PowerTaskList oldPrevious = m_previousStack.Pop();
				taskList.SetPrevious(oldPrevious);
				m_previousStack.Push(taskList);
			}
			if (m_subSpellOriginStack.Count > 0)
			{
				PowerTaskList subspellOrigin = m_subSpellOriginStack.Peek();
				if (taskList.GetOrigin() == subspellOrigin.GetOrigin())
				{
					taskList.SetSubSpellOrigin(subspellOrigin);
				}
			}
			BuildTaskList(powerList, ref i, taskList);
		}
		if (GameState.Get().AllowBatchedPowers())
		{
			for (int i2 = m_powerQueue.Count - 1; i2 > 0; i2--)
			{
				PowerTaskList current = m_powerQueue[i2];
				if (current.IsBatchable())
				{
					int j = i2 - 1;
					PowerTaskList previous = m_powerQueue[j];
					while ((previous.IsSlushTimeHelper() || !previous.HasAnyTasksInImmediate()) && j > 0)
					{
						previous = m_powerQueue[--j];
					}
					if (current.IsBatchable() && previous.IsBatchable() && current.GetBlockStart().TriggerKeyword == previous.GetBlockStart().TriggerKeyword)
					{
						current.FillMetaDataTargetSourceData();
						previous.FillMetaDataTargetSourceData();
						previous.AddTasks(current);
						foreach (int entity in current.GetBlockStart().Entities)
						{
							if (!previous.GetBlockStart().Entities.Contains(entity))
							{
								previous.GetBlockStart().Entities.Add(entity);
								previous.SetSourceEntitiesDirty();
							}
						}
						m_powerQueue.RemoveAt(i2);
					}
				}
			}
		}
		if (GameState.Get().AllowDeferredPowers())
		{
			FixUpOutOfOrderDeferredTasks();
			for (int i3 = m_powerQueue.Count - 1; i3 > 0; i3--)
			{
				PowerTaskList current2 = m_powerQueue[i3];
				if (current2.GetPrevious() == m_powerQueue[i3 - 1] && current2.IsCollapsible(isEarlier: false) && current2.GetPrevious().IsCollapsible(isEarlier: true))
				{
					current2.GetPrevious().AddTasks(current2);
					current2.GetPrevious().SetNext(null);
					if (current2.GetBlockEnd() != null)
					{
						current2.GetPrevious().SetBlockEnd(current2.GetBlockEnd());
					}
					foreach (PowerTaskList otherTaskList in m_powerQueue)
					{
						if (otherTaskList.GetPrevious() == current2)
						{
							otherTaskList.SetPrevious(current2.GetPrevious());
						}
					}
					m_powerQueue.RemoveAt(i3);
				}
			}
		}
		if (m_totalSlushTime > 0 && m_powerHistoryFirstTaskList != null && m_powerHistoryLastTaskList != null)
		{
			PowerTaskList firstTaskList = m_powerHistoryFirstTaskList;
			PowerTaskList lastTaskList = m_powerHistoryLastTaskList;
			firstTaskList.SetHistoryBlockStart(isStart: true);
			lastTaskList.SetHistoryBlockEnd(isEnd: true);
			m_currentTimeline.m_firstTaskId = firstTaskList.GetId();
			m_currentTimeline.m_lastTaskId = lastTaskList.GetId();
			m_currentTimeline.m_slushTime = m_totalSlushTime;
			m_powerHistoryTimeline.Add(m_currentTimeline);
			m_powerHistoryTimelineIdIndex.Add(m_currentTimeline.m_firstTaskId, m_powerHistoryTimeline.Count - 1);
			m_powerHistoryTimelineIdIndex.Add(m_currentTimeline.m_lastTaskId, m_powerHistoryTimeline.Count - 1);
			foreach (PowerHistoryTimelineEntry entry in m_currentTimeline.m_orderedEvents)
			{
				if (!m_powerHistoryTimelineIdIndex.ContainsKey(entry.taskId))
				{
					m_powerHistoryTimelineIdIndex.Add(entry.taskId, m_powerHistoryTimeline.Count - 1);
				}
			}
		}
		m_buildingTaskList = false;
	}

	private void FixUpOutOfOrderDeferredTasks()
	{
		if (!GameState.Get().AllowDeferredPowers())
		{
			return;
		}
		for (int i = m_powerQueue.Count - 1; i >= 0; i--)
		{
			PowerTaskList current = m_powerQueue[i];
			if (current.IsDeferrable())
			{
				FixUpOutOfOrderDeferredTasksInTasklist(current);
			}
		}
	}

	private void FixUpOutOfOrderDeferredTasksInTasklist(PowerTaskList deferredTaskList)
	{
		if (!GameState.Get().AllowDeferredPowers())
		{
			return;
		}
		Map<int, Map<int, List<int>>> entityChanges = GetEntityChangesForTaskList(deferredTaskList);
		for (int i = 0; i < m_powerQueue.Count; i++)
		{
			PowerTaskList current = m_powerQueue[i];
			if (current.GetId() == deferredTaskList.GetId())
			{
				break;
			}
			if (current.GetId() < deferredTaskList.GetDeferredSourceId())
			{
				continue;
			}
			if (current.IsDeferrable())
			{
				break;
			}
			Map<int, Map<int, List<int>>> laterEntityChanges = GetEntityChangesForTaskList(current);
			foreach (KeyValuePair<int, Map<int, List<int>>> entry in entityChanges)
			{
				int changeEntity = entry.Key;
				Map<int, List<int>> tagChanges = entry.Value;
				if (!laterEntityChanges.ContainsKey(changeEntity))
				{
					continue;
				}
				Map<int, List<int>> laterTagChanges = laterEntityChanges[changeEntity];
				foreach (KeyValuePair<int, List<int>> tagEntry in tagChanges)
				{
					int changeTag = tagEntry.Key;
					List<int> tagChangeValues = tagEntry.Value;
					if (laterTagChanges.ContainsKey(changeTag))
					{
						List<int> list = laterTagChanges[changeTag];
						int lastDeferredValue = tagChangeValues[tagChangeValues.Count - 1];
						int lastOriginalValue = list[list.Count - 1];
						deferredTaskList.FixupLastTagChangeForEntityTag(changeEntity, changeTag, lastOriginalValue);
						current.FixupLastTagChangeForEntityTag(changeEntity, changeTag, lastDeferredValue, fixLast: false);
					}
				}
			}
		}
	}

	private Map<int, Map<int, List<int>>> GetEntityChangesForTaskList(PowerTaskList taskList)
	{
		Map<int, Map<int, List<int>>> entityChanges = new Map<int, Map<int, List<int>>>();
		foreach (PowerTask tagChangeTask in taskList.GetTagChangeTasks())
		{
			Network.HistTagChange histTagChange = tagChangeTask.GetPower() as Network.HistTagChange;
			if (!entityChanges.ContainsKey(histTagChange.Entity))
			{
				entityChanges.Add(histTagChange.Entity, new Map<int, List<int>>());
			}
			if (!entityChanges[histTagChange.Entity].ContainsKey(histTagChange.Tag))
			{
				entityChanges[histTagChange.Entity].Add(histTagChange.Tag, new List<int>());
			}
			entityChanges[histTagChange.Entity][histTagChange.Tag].Add(histTagChange.Value);
		}
		return entityChanges;
	}

	public void HandleTimelineStartEvent(int tasklistId, float time, bool isBlockStart, Network.HistBlockStart blockStart)
	{
		if (!m_powerHistoryTimelineIdIndex.ContainsKey(tasklistId))
		{
			return;
		}
		int timelineIndex = m_powerHistoryTimelineIdIndex[tasklistId];
		PowerHistoryTimeline timeline = m_powerHistoryTimeline[timelineIndex];
		if (isBlockStart)
		{
			timeline.m_startTime = time;
			if (!HearthstoneApplication.IsPublic())
			{
				Debug.Log($"Timeline start event: (TasklistId: {tasklistId}) ---- (Expected Duration: {(float)timeline.m_slushTime * 0.001f})");
			}
		}
		if (timeline.m_orderedEventIndexLookup.ContainsKey(tasklistId))
		{
			int eventIndex = timeline.m_orderedEventIndexLookup[tasklistId];
			PowerHistoryTimelineEntry powerHistoryTimelineEntry = timeline.m_orderedEvents[eventIndex];
			powerHistoryTimelineEntry.entityId = blockStart?.Entities[0] ?? 0;
			float expected = (float)powerHistoryTimelineEntry.expectedStartOffset * 0.001f;
			float actual = (powerHistoryTimelineEntry.actualStartTime = time - timeline.m_startTime);
			FireTaskEvent(actual - expected);
			if (!HearthstoneApplication.IsPublic())
			{
				Debug.Log($"Task start event: (TasklistId: {tasklistId}) ---- (Expected: {expected} ---- (Actual: {actual}))");
			}
		}
	}

	public void HandleTimelineEndEvent(int tasklistId, float time, bool isBlockEnd)
	{
		if (!m_powerHistoryTimelineIdIndex.ContainsKey(tasklistId))
		{
			return;
		}
		int timelineIndex = m_powerHistoryTimelineIdIndex[tasklistId];
		PowerHistoryTimeline timeline = m_powerHistoryTimeline[timelineIndex];
		if (timeline.m_orderedEventIndexLookup.ContainsKey(tasklistId))
		{
			int eventIndex = timeline.m_orderedEventIndexLookup[tasklistId];
			PowerHistoryTimelineEntry timelineEntry = timeline.m_orderedEvents[eventIndex];
			float expected = (float)(timelineEntry.expectedStartOffset + timelineEntry.expectedTime) * 0.001f;
			float actual = time - timeline.m_startTime;
			FireTaskEvent(actual - expected);
			if (!HearthstoneApplication.IsPublic())
			{
				Debug.Log($"Task end event: (TasklistId: {tasklistId}) ---- (Expected: {expected} ---- (Actual: {actual}))");
				SceneDebugger.Get().AddSlushTimeEntry(tasklistId, (float)timelineEntry.expectedStartOffset * 0.001f, expected, timelineEntry.actualStartTime, actual, timelineEntry.entityId);
			}
		}
		if (isBlockEnd)
		{
			timeline.m_endTime = time;
			if (!HearthstoneApplication.IsPublic())
			{
				Debug.Log($"Timeline end event: (TasklistId: {tasklistId}) ---- (Expected: {(float)timeline.m_slushTime * 0.001f}) ---- (Actual: {timeline.m_endTime - timeline.m_startTime})");
			}
		}
	}

	public void ProcessPowerQueue()
	{
		while (GameState.Get().CanProcessPowerQueue())
		{
			if (m_busyTaskList != null)
			{
				m_busyTaskList = null;
			}
			else
			{
				PowerTaskList currentTaskList = m_powerQueue.Peek();
				if (HistoryManager.Get() != null && HistoryManager.Get().IsShowingBigCard())
				{
					if ((m_historyBlockingTaskList != null && !currentTaskList.IsDescendantOfBlock(m_historyBlockingTaskList)) || m_historyBlockingTaskList == null)
					{
						break;
					}
				}
				else
				{
					m_historyBlockingTaskList = null;
				}
				OnWillProcessTaskList(currentTaskList);
				if (GameState.Get().IsBusy())
				{
					m_busyTaskList = currentTaskList;
					break;
				}
			}
			if (CanEarlyConcede())
			{
				if (m_earlyConcedeTaskList == null && !m_handledFirstEarlyConcede)
				{
					DoEarlyConcedeVisuals();
					m_handledFirstEarlyConcede = true;
				}
				while (m_powerQueue.Count > 0)
				{
					m_currentTaskList = m_powerQueue.Dequeue();
					m_currentTaskList.DebugDump();
					CancelSpellsForEarlyConcede(m_currentTaskList);
					bool processed = false;
					if (GameState.Get().IsFinalWrapupStep() && GameState.Get().GetBooleanGameOption(GameEntityOption.EARLY_CONCEDE_PROCESS_SUB_SPELL_IN_FINAL_WRAPUP_STEP) && m_currentTaskList.IsSubSpellTaskList())
					{
						processed = DoSubSpellTaskListWithController(m_currentTaskList);
					}
					if (!processed)
					{
						m_currentTaskList.DoEarlyConcedeTasks();
					}
					m_currentTaskList = null;
				}
				break;
			}
			m_currentTaskList = m_powerQueue.Dequeue();
			if (m_previousTaskList == null || m_previousTaskList.GetOrigin() != m_currentTaskList.GetOrigin() || m_previousTaskList.GetParent() != m_currentTaskList.GetParent())
			{
				GameState.Get().ResetFriendlyCardDrawCounter();
				GameState.Get().ResetOpponentCardDrawCounter();
			}
			m_currentTaskList.DebugDump();
			OnProcessTaskList();
			StartCurrentTaskList();
		}
	}

	private int GetNextTaskListId()
	{
		int nextTaskListId = m_nextTaskListId;
		m_nextTaskListId = ((m_nextTaskListId == int.MaxValue) ? 1 : (m_nextTaskListId + 1));
		return nextTaskListId;
	}

	private bool CanDeferTaskList(Network.PowerHistory power)
	{
		if (!GameState.Get().AllowDeferredPowers())
		{
			return false;
		}
		if (power is Network.HistBlockStart blockStart)
		{
			return blockStart.IsDeferrable;
		}
		return false;
	}

	private bool CanBatchTaskList(Network.PowerHistory power)
	{
		if (!GameState.Get().AllowBatchedPowers())
		{
			return false;
		}
		if (power is Network.HistBlockStart blockStart)
		{
			return blockStart.IsBatchable;
		}
		return false;
	}

	private bool IsDeferBlockerTaskList(Network.PowerHistory power)
	{
		if (power is Network.HistBlockStart blockStart)
		{
			if (!blockStart.IsDeferBlocker && (blockStart.BlockType != HistoryBlock.Type.TRIGGER || blockStart.IsDeferrable))
			{
				if (blockStart.BlockType == HistoryBlock.Type.ATTACK)
				{
					return !blockStart.IsDeferrable;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private void BuildTaskList(List<Network.PowerHistory> powerList, ref int index, PowerTaskList taskList)
	{
		for (; index < powerList.Count; index++)
		{
			Network.PowerHistory power = powerList[index];
			Network.PowerType powerType = power.Type;
			if (powerType == Network.PowerType.BLOCK_START)
			{
				if (!taskList.IsEmpty())
				{
					EnqueueTaskList(taskList);
					if (taskList.IsDeferrable())
					{
						taskList.SetDeferrable(deferrable: false);
						List<PowerTaskList> currentDeferredList = m_deferredStack.Pop();
						if (m_deferredStack.Count > 0 && m_deferredStack.Peek().Contains(taskList))
						{
							m_deferredStack.Peek().Remove(taskList);
						}
						m_deferredStack.Push(currentDeferredList);
					}
				}
				PowerTaskList childOriginTaskList = new PowerTaskList();
				childOriginTaskList.SetBlockStart((Network.HistBlockStart)power);
				PowerTaskList originTaskList = taskList.GetOrigin();
				if (originTaskList.IsStartOfBlock())
				{
					childOriginTaskList.SetParent(originTaskList);
				}
				m_previousStack.Push(childOriginTaskList);
				if (IsDeferBlockerTaskList(power))
				{
					EnqueueDeferredTaskLists(combine: false);
					m_deferredStack.Push(new List<PowerTaskList>());
				}
				if (CanDeferTaskList(power))
				{
					if (m_deferredStack.Count > 0)
					{
						m_deferredStack.Peek().Add(childOriginTaskList);
						childOriginTaskList.SetDeferrable(deferrable: true);
					}
				}
				else
				{
					childOriginTaskList.SetBatchable(CanBatchTaskList(power));
				}
				m_deferredStack.Push(new List<PowerTaskList>());
				index++;
				BuildTaskList(powerList, ref index, childOriginTaskList);
				return;
			}
			if (powerType == Network.PowerType.BLOCK_END)
			{
				taskList.SetBlockEnd((Network.HistBlockEnd)power);
				if (m_previousStack.Count <= 0)
				{
					break;
				}
				m_previousStack.Pop();
				if (!taskList.IsDeferrable())
				{
					EnqueueTaskList(taskList);
					EnqueueDeferredTaskLists(combine: true);
					return;
				}
				if (m_powerQueue.Count > 0)
				{
					m_powerQueue.GetItem(m_powerQueue.Count - 1).SetCollapsible(collapsible: true);
				}
				taskList.SetDeferredSourceId(m_nextTaskListId);
				if (m_deferredStack.Count > 0)
				{
					List<PowerTaskList> deferredTaskLists = m_deferredStack.Pop();
					if (m_deferredStack.Count > 0)
					{
						m_deferredStack.Peek()?.AddRange(deferredTaskLists);
					}
					else
					{
						m_deferredStack.Push(deferredTaskLists);
					}
				}
				return;
			}
			switch (powerType)
			{
			case Network.PowerType.SUB_SPELL_START:
			{
				if (!taskList.HasTasks())
				{
					Network.HistMetaData dummyTask = new Network.HistMetaData
					{
						MetaType = HistoryMeta.Type.ARTIFICIAL_HISTORY_INTERRUPT
					};
					taskList.CreateTask(dummyTask);
				}
				EnqueueTaskList(taskList);
				if (taskList.IsDeferrable())
				{
					taskList.SetDeferrable(deferrable: false);
					List<PowerTaskList> currentDeferredList2 = m_deferredStack.Pop();
					if (m_deferredStack.Count > 0 && m_deferredStack.Peek().Contains(taskList))
					{
						m_deferredStack.Peek().Remove(taskList);
					}
					m_deferredStack.Push(currentDeferredList2);
				}
				PowerTaskList newTaskList2 = new PowerTaskList();
				newTaskList2.SetPrevious(taskList);
				newTaskList2.SetParent(taskList.GetParent());
				newTaskList2.SetSubSpellOrigin(newTaskList2);
				newTaskList2.SetSubSpellStart((Network.HistSubSpellStart)power);
				m_subSpellOriginStack.Push(newTaskList2);
				if (m_previousStack.Count > 0 && m_previousStack.Peek() == taskList)
				{
					m_previousStack.Pop();
					m_previousStack.Push(newTaskList2);
				}
				taskList = newTaskList2;
				break;
			}
			case Network.PowerType.SUB_SPELL_END:
				taskList.CreateTask(power);
				taskList.SetSubSpellEnd((Network.HistSubSpellEnd)power);
				EnqueueTaskList(taskList);
				if (m_subSpellOriginStack.Count > 0)
				{
					if (m_subSpellOriginStack.Pop() != taskList.GetSubSpellOrigin())
					{
						Log.Power.PrintError("{0}.BuildTaskList(): Mismatch between SUB_SPELL_END task and current task list's SubSpellOrigin!", this);
					}
				}
				else
				{
					Log.Power.PrintError("{0}.BuildTaskList(): Hit a SUB_SPELL_END task without a corresponding open SubSpellOrigin!", this);
				}
				if (index + 1 < powerList.Count)
				{
					PowerTaskList newTaskList = new PowerTaskList();
					newTaskList.SetPrevious(taskList);
					newTaskList.SetParent(taskList.GetParent());
					if (m_subSpellOriginStack.Count > 0 && m_subSpellOriginStack.Peek().GetParent() == taskList.GetParent())
					{
						newTaskList.SetSubSpellOrigin(m_subSpellOriginStack.Peek());
					}
					if (m_previousStack.Count > 0 && m_previousStack.Peek() == taskList)
					{
						m_previousStack.Pop();
						m_previousStack.Push(newTaskList);
					}
					taskList = newTaskList;
					continue;
				}
				break;
			}
			PowerTask task = taskList.CreateTask(power);
			if (powerType == Network.PowerType.META_DATA && ((Network.HistMetaData)power).MetaType == HistoryMeta.Type.ARTIFICIAL_HISTORY_INTERRUPT)
			{
				EnqueueTaskList(taskList);
				return;
			}
			if (CanDoRealTimeTask())
			{
				task.DoRealTimeTask(powerList, index);
				continue;
			}
			DelayedRealTimeTask delayed = new DelayedRealTimeTask();
			delayed.m_index = index;
			delayed.m_powerTask = task;
			delayed.m_powerHistory = new List<Network.PowerHistory>(powerList);
			m_delayedRealTimeTasks.Enqueue(delayed);
		}
		if (!taskList.IsEmpty())
		{
			EnqueueTaskList(taskList);
		}
		if (m_deferredStack.Count != 0)
		{
			EnqueueDeferredTaskLists(combine: true);
			if (m_deferredStack.Count == 0)
			{
				m_deferredStack.Push(new List<PowerTaskList>());
			}
		}
	}

	private void EnqueueDeferredTaskLists(bool combine)
	{
		if (m_deferredStack.Count <= 0)
		{
			return;
		}
		List<PowerTaskList> deferredTaskLists = m_deferredStack.Pop();
		for (int i = deferredTaskLists.Count - 1; i > 0; i--)
		{
			PowerTaskList current = deferredTaskLists[i];
			if (current.GetBlockStart() != null && combine)
			{
				for (int j = i - 1; j >= 0; j--)
				{
					PowerTaskList other = deferredTaskLists[j];
					if (other.GetBlockStart() != null && other.GetBlockStart().Entities.Count == current.GetBlockStart().Entities.Count)
					{
						bool allEntitiesTheSame = true;
						foreach (int entity in other.GetBlockStart().Entities)
						{
							if (!current.GetBlockStart().Entities.Contains(entity))
							{
								allEntitiesTheSame = false;
								break;
							}
						}
						if (allEntitiesTheSame)
						{
							other.AddTasks(current);
							deferredTaskLists.RemoveAt(i);
							break;
						}
					}
				}
			}
		}
		foreach (PowerTaskList tl in deferredTaskLists)
		{
			EnqueueTaskList(tl);
		}
	}

	public bool EntityHasPendingTasks(Entity entity)
	{
		int entityId = entity.GetEntityId();
		foreach (PowerTaskList taskList in m_powerQueue)
		{
			List<Entity> sourceEntities = taskList.GetSourceEntities(warnIfNull: false);
			if (sourceEntities != null && sourceEntities.Exists((Entity e) => e != null && e.GetEntityId() == entityId))
			{
				return true;
			}
			Entity targetEntity = taskList.GetTargetEntity(warnIfNull: false);
			if (targetEntity != null && targetEntity.GetEntityId() == entityId)
			{
				return true;
			}
			PowerTaskList parentTaskList = taskList.GetParent();
			if (parentTaskList != null)
			{
				List<Entity> parentSourceEntities = parentTaskList.GetSourceEntities(warnIfNull: false);
				if (parentSourceEntities != null && parentSourceEntities.Exists((Entity e) => e != null && e.GetEntityId() == entityId))
				{
					return true;
				}
				Entity parentTargetEntity = parentTaskList.GetTargetEntity(warnIfNull: false);
				if (parentTargetEntity != null && parentTargetEntity.GetEntityId() == entityId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void FlushDelayedRealTimeTasks()
	{
		while (CanDoRealTimeTask() && m_delayedRealTimeTasks.Count > 0)
		{
			DelayedRealTimeTask task = m_delayedRealTimeTasks.Dequeue();
			task.m_powerTask.DoRealTimeTask(task.m_powerHistory, task.m_index);
		}
	}

	private void EnqueueTaskList(PowerTaskList taskList)
	{
		m_totalSlushTime += taskList.GetTotalSlushTime();
		if (m_powerHistoryFirstTaskList == null)
		{
			m_powerHistoryFirstTaskList = taskList;
		}
		else
		{
			m_powerHistoryLastTaskList = taskList;
		}
		taskList.SetId(GetNextTaskListId());
		m_powerQueue.Enqueue(taskList);
		if (m_currentTimeline != null && taskList.GetTotalSlushTime() > 0)
		{
			m_currentTimeline.AddTimelineEntry(taskList.GetId(), taskList.GetTotalSlushTime());
		}
		if (taskList.HasFriendlyConcede())
		{
			m_earlyConcedeTaskList = taskList;
		}
		if (taskList.HasGameOver())
		{
			m_gameOverTaskList = taskList;
		}
	}

	private void OnWillProcessTaskList(PowerTaskList taskList)
	{
		if ((bool)ThinkEmoteManager.Get())
		{
			ThinkEmoteManager.Get().NotifyOfActivity();
		}
		if (!taskList.IsStartOfBlock() || taskList.GetBlockStart().BlockType != HistoryBlock.Type.PLAY)
		{
			return;
		}
		Entity sourceEntity = taskList.GetSourceEntity(warnIfNull: false);
		if (sourceEntity.GetController().IsOpposingSide())
		{
			string cardId = sourceEntity.GetCardId();
			if (string.IsNullOrEmpty(cardId))
			{
				cardId = FindRevealedCardId(taskList);
			}
			GameState.Get().GetGameEntity().NotifyOfOpponentWillPlayCard(cardId, sourceEntity);
		}
	}

	private bool ContainsBurnedCard(PowerTaskList taskList)
	{
		return ContainsMetaDataTaskWithInfo(taskList, HistoryMeta.Type.BURNED_CARD);
	}

	private bool ContainsPoisonousEffect(PowerTaskList taskList)
	{
		return ContainsMetaDataTaskWithInfo(taskList, HistoryMeta.Type.POISONOUS);
	}

	private bool ContainsCriticalHitEffect(PowerTaskList taskList)
	{
		return ContainsMetaDataTaskWithInfo(taskList, HistoryMeta.Type.CRITICAL_HIT);
	}

	private bool ContainsMetaDataTaskWithInfo(PowerTaskList taskList, HistoryMeta.Type metaType)
	{
		List<PowerTask> tasks = taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			if (power.Type != Network.PowerType.META_DATA)
			{
				continue;
			}
			Network.HistMetaData metaData = (Network.HistMetaData)power;
			if (metaData.MetaType != metaType)
			{
				continue;
			}
			if (metaData.Info.Count == 0)
			{
				Log.Power.PrintError("PowerProcessor.ContainsMetaDataTaskWithInfo(): metaData.Info.Count is 0, metaType: {0}", metaType);
				continue;
			}
			if (GameState.Get().GetEntity(metaData.Info[0]) == null)
			{
				Log.Power.PrintError("PowerProcessor.ContainsMetaDataTaskWithInfo(): metaData.Info contains an invalid entity (ID {0}), metaType: {1}", metaData.Info[0], metaType);
				continue;
			}
			return true;
		}
		return false;
	}

	private string FindRevealedCardId(PowerTaskList taskList)
	{
		taskList.GetBlockStart();
		List<PowerTask> tasks = taskList.GetTaskList();
		for (int i = 0; i < tasks.Count; i++)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			Network.HistShowEntity showEntity = power as Network.HistShowEntity;
			if (showEntity != null && taskList.GetSourceEntities() != null && taskList.GetSourceEntities().Exists((Entity e) => e != null && e.GetEntityId() == showEntity.Entity.ID))
			{
				return showEntity.Entity.CardID;
			}
		}
		return null;
	}

	private void OnProcessTaskList()
	{
		if (m_currentTaskList.IsStartOfBlock())
		{
			Network.HistBlockStart blockStart = m_currentTaskList.GetBlockStart();
			switch (blockStart.BlockType)
			{
			case HistoryBlock.Type.PLAY:
			{
				Entity source2 = m_currentTaskList.GetSourceEntity(warnIfNull: false);
				if (source2.IsControlledByFriendlySidePlayer())
				{
					GameState.Get().GetGameEntity().NotifyOfFriendlyPlayedCard(source2);
				}
				else
				{
					GameState.Get().GetGameEntity().NotifyOfOpponentPlayedCard(source2);
				}
				if (source2.IsMinion())
				{
					GameState.Get().GetGameEntity().NotifyOfMinionPlayed(source2);
				}
				else if (source2.IsHero())
				{
					GameState.Get().GetGameEntity().NotifyOfHeroChanged(source2);
				}
				else if (source2.IsWeapon())
				{
					GameState.Get().GetGameEntity().NotifyOfWeaponEquipped(source2);
				}
				else if (source2.IsSpell())
				{
					Entity target2 = m_currentTaskList.GetTargetEntity(warnIfNull: false);
					GameState.Get().GetGameEntity().NotifyOfSpellPlayed(source2, target2);
				}
				else if (source2.IsHeroPower())
				{
					Entity target3 = m_currentTaskList.GetTargetEntity(warnIfNull: false);
					GameState.Get().GetGameEntity().NotifyOfHeroPowerUsed(source2, target3);
				}
				break;
			}
			case HistoryBlock.Type.ATTACK:
			{
				Entity attacker = m_currentTaskList.GetAttacker();
				Entity defender = null;
				switch (m_currentTaskList.GetAttackType())
				{
				case AttackType.REGULAR:
					defender = m_currentTaskList.GetDefender();
					break;
				case AttackType.CANCELED:
					defender = m_currentTaskList.GetProposedDefender();
					break;
				}
				if (attacker != null && defender != null)
				{
					GameState.Get().GetGameEntity().NotifyOfEntityAttacked(attacker, defender);
				}
				break;
			}
			case HistoryBlock.Type.DEATHS:
				foreach (PowerTask task in m_currentTaskList.GetTaskList())
				{
					Network.PowerHistory power = task.GetPower();
					if (power.Type != Network.PowerType.TAG_CHANGE)
					{
						continue;
					}
					Network.HistTagChange tagChange = power as Network.HistTagChange;
					if (!GameUtils.IsEntityDeathTagChange(tagChange))
					{
						continue;
					}
					Entity entityThatDied = GameState.Get().GetEntity(tagChange.Entity);
					if (entityThatDied.IsMinion())
					{
						GameState.Get().GetGameEntity().NotifyOfMinionDied(entityThatDied);
					}
					else if (entityThatDied.IsHero())
					{
						GameState.Get().GetGameEntity().NotifyOfHeroDied(entityThatDied);
					}
					else
					{
						if (!entityThatDied.IsWeapon())
						{
							continue;
						}
						GameState.Get().GetGameEntity().NotifyOfWeaponDestroyed(entityThatDied);
						Player controller = entityThatDied.GetController();
						if (controller != null)
						{
							Card heroCard2 = controller.GetHeroCard();
							if (heroCard2 != null)
							{
								heroCard2.NotifyOfWeaponDestroyed(entityThatDied);
							}
						}
					}
				}
				break;
			case HistoryBlock.Type.POWER:
			{
				Entity source = m_currentTaskList.GetSourceEntity(warnIfNull: false);
				Entity target = m_currentTaskList.GetTargetEntity(warnIfNull: false);
				Card heroCard = (source?.GetController())?.GetHeroCard();
				if (heroCard != null)
				{
					if (source.IsWeapon())
					{
						heroCard.NotifyOfWeaponPlayed(source);
					}
					else if (source.IsSpell())
					{
						heroCard.NotifyOfSpellPlayed(source, target);
					}
					else if (source.IsHeroPower())
					{
						heroCard.NotifyOfHeroPowerPlayed(source, target);
					}
				}
				break;
			}
			}
			if (blockStart.BlockType == HistoryBlock.Type.POWER || blockStart.BlockType == HistoryBlock.Type.TRIGGER)
			{
				for (int i = 0; i < blockStart.EffectCardId.Count; i++)
				{
					if (string.IsNullOrEmpty(blockStart.EffectCardId[i]))
					{
						List<Entity> sourceEntities = m_currentTaskList.GetSourceEntities();
						if (sourceEntities != null && i < sourceEntities.Count && sourceEntities[i] != null)
						{
							blockStart.EffectCardId[i] = sourceEntities[i].GetCardId();
							blockStart.IsEffectCardIdClientCached[i] = true;
						}
					}
				}
			}
		}
		PrepareHistoryForCurrentTaskList();
		m_currentTaskList.CreateArtificialHistoryTilesFromMetadata();
	}

	private void PrepareHistoryForCurrentTaskList()
	{
		GameState gameState = GameState.Get();
		if (gameState == null || gameState.GameScenarioAllowsPowerPrinting())
		{
			Log.Power.Print("PowerProcessor.PrepareHistoryForCurrentTaskList() - m_currentTaskList={0}", m_currentTaskList.GetId());
		}
		Network.HistBlockStart blockStart = m_currentTaskList.GetBlockStart();
		if (blockStart == null)
		{
			return;
		}
		List<Entity> sourceEnts = m_currentTaskList.GetSourceEntities();
		if (sourceEnts != null && sourceEnts.Exists((Entity e) => e?.HasTag(GAME_TAG.CARD_DOES_NOTHING) ?? false))
		{
			return;
		}
		switch (blockStart.BlockType)
		{
		case HistoryBlock.Type.ATTACK:
		{
			AttackType attackType = m_currentTaskList.GetAttackType();
			Entity attacker = null;
			Entity defender = null;
			switch (attackType)
			{
			case AttackType.REGULAR:
				attacker = m_currentTaskList.GetAttacker();
				defender = m_currentTaskList.GetDefender();
				break;
			case AttackType.CANCELED:
				attacker = m_currentTaskList.GetAttacker();
				defender = m_currentTaskList.GetProposedDefender();
				break;
			}
			if (attacker != null && defender != null)
			{
				HistoryManager.Get().CreateAttackTile(attacker, defender, m_currentTaskList);
				m_currentTaskList.SetWillCompleteHistoryEntry(set: true);
			}
			if (HistoryManager.Get().HasHistoryEntry())
			{
				m_currentTaskList.NotifyHistoryOfAdditionalTargets();
			}
			break;
		}
		case HistoryBlock.Type.PLAY:
		{
			Entity playedEntity2 = m_currentTaskList.GetSourceEntity(warnIfNull: false);
			if (playedEntity2 == null)
			{
				break;
			}
			if (m_currentTaskList.IsStartOfBlock())
			{
				Entity targetedEntity2 = null;
				Entity suboptionEntity = null;
				if (m_currentTaskList.ShouldCreatePlayBlockHistoryTile())
				{
					if (blockStart.SubOption >= 0 && blockStart.SubOption < playedEntity2.GetSubCardIDs().Count)
					{
						suboptionEntity = GameState.Get().GetEntity(playedEntity2.GetSubCardIDs()[blockStart.SubOption]);
					}
					targetedEntity2 = GameState.Get().GetEntity(blockStart.Target);
					if (suboptionEntity != null && suboptionEntity.IsStarshipLaunchAbility())
					{
						suboptionEntity = null;
					}
					HistoryManager.Get().CreatePlayedTile(playedEntity2, targetedEntity2, suboptionEntity);
					m_currentTaskList.SetWillCompleteHistoryEntry(set: true);
				}
				if (ShouldShowPlayedBigCard(playedEntity2, blockStart))
				{
					bool spellCountered = m_currentTaskList.WasThePlayedSpellCountered(playedEntity2);
					SetHistoryBlockingTaskList();
					if (playedEntity2.IsTitan() && suboptionEntity != null)
					{
						HistoryManager.Get().CreatePlayedBigCard(suboptionEntity, OnBigCardStarted, OnBigCardFinished, fromMetaData: false, spellCountered, 0);
					}
					else
					{
						HistoryManager.Get().CreatePlayedBigCard(playedEntity2, OnBigCardStarted, OnBigCardFinished, fromMetaData: false, spellCountered, 0);
					}
				}
			}
			m_currentTaskList.NotifyHistoryOfAdditionalTargets();
			break;
		}
		case HistoryBlock.Type.POWER:
			if (HistoryManager.Get().HasHistoryEntry())
			{
				m_currentTaskList.NotifyHistoryOfAdditionalTargets();
			}
			break;
		case HistoryBlock.Type.JOUST:
			m_currentTaskList.NotifyHistoryOfAdditionalTargets();
			break;
		case HistoryBlock.Type.REVEAL_CARD:
			m_currentTaskList.NotifyHistoryOfAdditionalTargets();
			break;
		case HistoryBlock.Type.DECK_ACTION:
		{
			Entity playedEntity = m_currentTaskList.GetSourceEntity(warnIfNull: false);
			if (playedEntity == null)
			{
				break;
			}
			if (m_currentTaskList.IsStartOfBlock())
			{
				Entity targetedEntity = GameState.Get().GetEntity(blockStart.Target);
				foreach (PowerTask task in m_currentTaskList.GetTaskList())
				{
					if (task.GetPower().Type == Network.PowerType.TAG_CHANGE && ((Network.HistTagChange)task.GetPower()).Tag == 3070)
					{
						task.DoTask();
					}
				}
				if (playedEntity.IsForgeable())
				{
					HistoryManager.Get().CreateForgeTransformTile(playedEntity);
				}
				else
				{
					HistoryManager.Get().CreatePlayedTile(playedEntity, targetedEntity);
				}
				m_currentTaskList.SetWillCompleteHistoryEntry(set: true);
				if (ShouldShowPlayedBigCard(playedEntity, blockStart))
				{
					SetHistoryBlockingTaskList();
					HistoryManager.Get().CreatePlayedBigCard(playedEntity, OnBigCardStarted, OnBigCardFinished, fromMetaData: false, countered: false, 0);
				}
			}
			m_currentTaskList.NotifyHistoryOfAdditionalTargets();
			break;
		}
		case HistoryBlock.Type.TRIGGER:
		{
			Entity triggeredEntity = m_currentTaskList.GetSourceEntity(warnIfNull: false);
			if (triggeredEntity == null)
			{
				break;
			}
			if (triggeredEntity.IsSecret() || blockStart.TriggerKeyword == 1192 || blockStart.TriggerKeyword == 1749)
			{
				if (m_currentTaskList.IsStartOfBlock())
				{
					HistoryManager.Get().CreateTriggerTile(triggeredEntity);
					m_currentTaskList.SetWillCompleteHistoryEntry(set: true);
					SetHistoryBlockingTaskList();
					HistoryManager.Get().CreateTriggeredBigCard(triggeredEntity, OnBigCardStarted, OnBigCardFinished, fromMetaData: false, isSecret: true);
				}
				m_currentTaskList.NotifyHistoryOfAdditionalTargets();
				break;
			}
			bool notifyHistoryOfTargets = false;
			if (!m_currentTaskList.IsStartOfBlock())
			{
				notifyHistoryOfTargets = GetTriggerTaskListThatShouldCompleteHistoryEntry().WillBlockCompleteHistoryEntry();
			}
			else if (blockStart.ShowInHistory)
			{
				if (triggeredEntity.HasTag(GAME_TAG.HISTORY_PROXY))
				{
					Entity proxy = GameState.Get().GetEntity(triggeredEntity.GetTag(GAME_TAG.HISTORY_PROXY));
					HistoryManager.Get().CreatePlayedTile(proxy, null);
					if (triggeredEntity.GetController() != GameState.Get().GetFriendlySidePlayer() || !triggeredEntity.HasTag(GAME_TAG.HISTORY_PROXY_NO_BIG_CARD))
					{
						SetHistoryBlockingTaskList();
						HistoryManager.Get().CreateTriggeredBigCard(proxy, OnBigCardStarted, OnBigCardFinished, fromMetaData: false, isSecret: false);
					}
				}
				else
				{
					if (ShouldShowTriggeredBigCard(triggeredEntity))
					{
						SetHistoryBlockingTaskList();
						HistoryManager.Get().CreateTriggeredBigCard(triggeredEntity, OnBigCardStarted, OnBigCardFinished, fromMetaData: false, isSecret: false);
					}
					HistoryManager.Get().CreateTriggerTile(triggeredEntity);
				}
				GetTriggerTaskListThatShouldCompleteHistoryEntry().SetWillCompleteHistoryEntry(set: true);
				notifyHistoryOfTargets = true;
			}
			else if ((blockStart.TriggerKeyword == 685 || blockStart.TriggerKeyword == 923 || blockStart.TriggerKeyword == 363 || blockStart.TriggerKeyword == 1944 || blockStart.TriggerKeyword == 2853 || blockStart.TriggerKeyword == 1675 || blockStart.TriggerKeyword == 1920) && HistoryManager.Get().HasHistoryEntry())
			{
				notifyHistoryOfTargets = true;
			}
			else if (ContainsBurnedCard(m_currentTaskList))
			{
				if (m_currentTaskList.IsStartOfBlock())
				{
					HistoryManager.Get().CreateBurnedCardsTile();
					m_currentTaskList.SetWillCompleteHistoryEntry(set: true);
				}
				m_currentTaskList.NotifyHistoryOfAdditionalTargets();
			}
			else if (ContainsPoisonousEffect(m_currentTaskList) || ContainsCriticalHitEffect(m_currentTaskList))
			{
				notifyHistoryOfTargets = true;
			}
			if (notifyHistoryOfTargets)
			{
				m_currentTaskList.NotifyHistoryOfAdditionalTargets();
			}
			break;
		}
		case HistoryBlock.Type.FATIGUE:
			if (m_currentTaskList.IsStartOfBlock())
			{
				HistoryManager.Get().CreateFatigueTile();
				m_currentTaskList.SetWillCompleteHistoryEntry(set: true);
			}
			m_currentTaskList.NotifyHistoryOfAdditionalTargets();
			break;
		}
	}

	private void OnBigCardStarted()
	{
		m_historyBlocking = true;
	}

	private void OnBigCardFinished()
	{
		m_historyBlocking = false;
	}

	private bool ShouldShowPlayedBigCard(Entity sourceEntity, Network.HistBlockStart blockStart)
	{
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.USES_BIG_CARDS))
		{
			return false;
		}
		if (!InputManager.Get().PermitDecisionMakingInput())
		{
			return true;
		}
		if (sourceEntity.IsControlledByOpposingSidePlayer())
		{
			return true;
		}
		if (blockStart.ForceShowBigCard)
		{
			return true;
		}
		if (sourceEntity.IsLettuceAbility())
		{
			return true;
		}
		return false;
	}

	private bool ShouldShowTriggeredBigCard(Entity sourceEntity)
	{
		if (sourceEntity.GetZone() != TAG_ZONE.HAND)
		{
			return false;
		}
		if (sourceEntity.IsHidden())
		{
			return false;
		}
		if (!sourceEntity.HasTriggerVisual())
		{
			return false;
		}
		return true;
	}

	private PowerTaskList GetTriggerTaskListThatShouldCompleteHistoryEntry()
	{
		if (m_currentTaskList.GetBlockType() != HistoryBlock.Type.TRIGGER)
		{
			return null;
		}
		m_currentTaskList.GetParent()?.GetBlockType();
		return m_currentTaskList.GetOrigin();
	}

	private bool CanEarlyConcede()
	{
		if (!GameState.Get().IsGameCreated())
		{
			return false;
		}
		if (m_earlyConcedeTaskList != null)
		{
			return true;
		}
		if (GameState.Get().IsGameOver())
		{
			return false;
		}
		if (GameState.Get().WasConcedeRequested())
		{
			Network.HistTagChange gameOverChange = GameState.Get().GetRealTimeGameOverTagChange();
			if (gameOverChange != null && gameOverChange.Value != 4)
			{
				return true;
			}
		}
		return false;
	}

	private void DoEarlyConcedeVisuals()
	{
		if (!GameUtils.IsWaitingForOpponentReconnect())
		{
			GameState.Get().GetFriendlySidePlayer()?.PlayConcedeEmote();
		}
	}

	private void CancelSpellsForEarlyConcede(PowerTaskList taskList)
	{
		List<Entity> sourceEntities = taskList.GetSourceEntities();
		if (sourceEntities == null)
		{
			return;
		}
		foreach (Entity sourceEntity in sourceEntities)
		{
			if (sourceEntity == null)
			{
				continue;
			}
			Card sourceCard = sourceEntity.GetCard();
			if (!sourceCard || taskList.GetBlockStart().BlockType != HistoryBlock.Type.POWER)
			{
				continue;
			}
			Spell spell = sourceCard.GetPlaySpell(0);
			if ((bool)spell)
			{
				SpellStateType stateType = spell.GetActiveState();
				if (stateType != 0 && stateType != SpellStateType.CANCEL)
				{
					spell.ActivateState(SpellStateType.CANCEL);
				}
			}
		}
	}

	private void StartCurrentTaskList()
	{
		m_currentTaskList.SetProcessStartTime();
		GameState state = GameState.Get();
		if (!m_currentTaskList.IsSubSpellTaskList())
		{
			Network.HistBlockStart blockStart = m_currentTaskList.GetBlockStart();
			if (blockStart == null)
			{
				DoCurrentTaskList();
				return;
			}
			int firstSourceEntityId = ((blockStart.Entities.Count != 0) ? blockStart.Entities[0] : 0);
			if (m_currentTaskList.GetSourceEntities() == null || m_currentTaskList.GetSourceEntity() == null)
			{
				if (!state.EntityRemovedFromGame(firstSourceEntityId))
				{
					Debug.LogErrorFormat("PowerProcessor.StartCurrentTaskList() - WARNING got a power with a null source entity (ID={0})", firstSourceEntityId);
				}
				DoCurrentTaskList();
				return;
			}
		}
		if (!DoTaskListWithSpellController(state, m_currentTaskList, m_currentTaskList.GetSourceEntity()))
		{
			DoCurrentTaskList();
		}
	}

	private void DoCurrentTaskList()
	{
		m_currentTaskList.DoAllTasks(delegate
		{
			EndCurrentTaskList();
		});
	}

	private void EndCurrentTaskList()
	{
		GameState gameState = GameState.Get();
		if (gameState == null || gameState.GameScenarioAllowsPowerPrinting())
		{
			Log.Power.Print("PowerProcessor.EndCurrentTaskList() - m_currentTaskList={0}", (m_currentTaskList == null) ? "null" : m_currentTaskList.GetId().ToString());
		}
		if (m_currentTaskList == null)
		{
			GameState.Get().OnTaskListEnded(null);
			return;
		}
		if (m_currentTaskList.GetBlockEnd() != null)
		{
			if (m_currentTaskList.GetOrigin() == m_historyBlockingTaskList && m_currentTaskList.GetNext() == null)
			{
				m_historyBlockingTaskList = null;
			}
			Entity sourceEntity = m_currentTaskList.GetSourceEntity();
			if (sourceEntity != null && sourceEntity.HasReplacementsWhenPlayed())
			{
				CleanupReplacementPreviewEffects(sourceEntity);
			}
			if (m_currentTaskList.WillBlockCompleteHistoryEntry())
			{
				HistoryManager.Get().MarkCurrentHistoryEntryAsCompleted();
			}
		}
		GameState.Get().OnTaskListEnded(m_currentTaskList);
		m_previousTaskList = m_currentTaskList;
		m_currentTaskList = null;
	}

	private void CleanupReplacementPreviewEffects(Entity cardWithReplacementsEntity)
	{
		if (InputManager.Get().GetFriendlyHand().IsCardWithReplacementsBeingPlayed(cardWithReplacementsEntity))
		{
			InputManager.Get().GetFriendlyHand().ActivateCardWithReplacementsDeath();
			InputManager.Get().GetFriendlyHand().ClearReservedCard();
		}
	}

	public bool PerformTaskListOnCurrentGameState(PowerTaskList taskList)
	{
		return DoTaskListWithSpellController(GameState.Get(), taskList, null);
	}

	private bool DoTaskListWithSpellController(GameState state, PowerTaskList taskList, Entity sourceEntity)
	{
		HistoryBlock.Type blockType = taskList.GetBlockType();
		Network.HistBlockStart blockStart = taskList.GetBlockStart();
		if (taskList.IsSubSpellTaskList())
		{
			if (!DoSubSpellTaskListWithController(taskList))
			{
				return false;
			}
			return true;
		}
		switch (blockType)
		{
		case HistoryBlock.Type.ATTACK:
		{
			AttackSpellController attackSpellController = CreateAttackSpellController(taskList);
			if (!DoTaskListUsingController(attackSpellController, taskList))
			{
				DestroySpellController(attackSpellController);
				return false;
			}
			return true;
		}
		case HistoryBlock.Type.MOVE_MINION:
		{
			MoveMinionSpellController moveMinionSpellController = CreateMoveMinionSpellController(taskList);
			if (!DoTaskListUsingController(moveMinionSpellController, taskList))
			{
				DestroySpellController(moveMinionSpellController);
				return false;
			}
			return true;
		}
		case HistoryBlock.Type.POWER:
		{
			PowerSpellController powerSpellController = CreatePowerSpellController(taskList);
			if (!DoTaskListUsingController(powerSpellController, taskList))
			{
				DestroySpellController(powerSpellController);
				return false;
			}
			return true;
		}
		case HistoryBlock.Type.TRIGGER:
			if (sourceEntity != null && sourceEntity.IsSecret())
			{
				SecretSpellController secretSpellController = CreateSecretSpellController(taskList);
				if (!DoTaskListUsingController(secretSpellController, taskList))
				{
					DestroySpellController(secretSpellController);
					return false;
				}
			}
			else if (blockStart != null && blockStart.TriggerKeyword == 1192)
			{
				SideQuestSpellController sideQuestSpellController = CreateSideQuestSpellController(taskList);
				if (!DoTaskListUsingController(sideQuestSpellController, taskList))
				{
					DestroySpellController(sideQuestSpellController);
					return false;
				}
			}
			else if (blockStart != null && blockStart.TriggerKeyword == 1749)
			{
				SigilSpellController sigilSpellController = CreateSigilSpellController(taskList);
				if (!DoTaskListUsingController(sigilSpellController, taskList))
				{
					DestroySpellController(sigilSpellController);
					return false;
				}
			}
			else if (blockStart != null && blockStart.TriggerKeyword == 2311)
			{
				ObjectiveSpellController objectiveSpellController = CreateObjectiveSpellController(taskList);
				if (!DoTaskListUsingController(objectiveSpellController, taskList))
				{
					DestroySpellController(objectiveSpellController);
					return false;
				}
			}
			else if (blockStart != null && blockStart.TriggerKeyword == 4013)
			{
				Actor sourceActor = sourceEntity.GetCard().GetActor();
				if (!sourceActor.HasUsedStarshipLaunchAnimationDelay)
				{
					if (m_starshipLaunchTriggerTasks == null)
					{
						m_starshipLaunchTriggerTasks = new List<PowerTaskList>();
					}
					float delayAmount = 2f;
					if (sourceEntity.HasTag(GAME_TAG.IS_RELAUNCHED_STARSHIP))
					{
						delayAmount = 0.5f;
					}
					m_starshipLaunchTriggerTasks.Add(taskList);
					if (sourceActor != null)
					{
						m_IsWaitingForStarshipLandingDelay = true;
						sourceActor.StartCoroutine(DoTaskListAfterDelay(delayAmount));
						sourceActor.HasUsedStarshipLaunchAnimationDelay = true;
					}
				}
				if (m_IsWaitingForStarshipLandingDelay)
				{
					m_starshipLaunchTriggerTasks.Add(taskList);
				}
				else
				{
					TriggerSpellController triggerSpellController = CreateTriggerSpellController(taskList);
					if (!DoTaskListUsingController(triggerSpellController, taskList))
					{
						DestroySpellController(triggerSpellController);
						return false;
					}
				}
			}
			else
			{
				if (string.IsNullOrEmpty(sourceEntity.GetCardId()))
				{
					return false;
				}
				TriggerSpellController triggerSpellController2 = CreateTriggerSpellController(taskList);
				Card sourceCard = sourceEntity?.GetCard();
				Card metadataCard = taskList.GetStartDrawMetaDataCard();
				if (TurnStartManager.Get().IsCardDrawHandled(sourceCard) || TurnStartManager.Get().IsCardDrawHandled(metadataCard))
				{
					if (!triggerSpellController2.AttachPowerTaskList(taskList))
					{
						Log.Power.PrintWarning("TurnStartManager failed to handle a trigger. sourceCard:{0}, metadataCard:{1}, taskList:{2}", sourceCard, metadataCard, taskList);
						DestroySpellController(triggerSpellController2);
						return false;
					}
					triggerSpellController2.AddFinishedTaskListCallback(OnSpellControllerFinishedTaskList);
					triggerSpellController2.AddFinishedCallback(OnSpellControllerFinished);
					TurnStartManager.Get().NotifyOfSpellController(triggerSpellController2);
				}
				else if (!DoTaskListUsingController(triggerSpellController2, taskList))
				{
					DestroySpellController(triggerSpellController2);
					return false;
				}
			}
			return true;
		case HistoryBlock.Type.DEATHS:
		{
			DeathSpellController deathSpellController = CreateDeathSpellController(taskList);
			if (!DoTaskListUsingController(deathSpellController, taskList))
			{
				DestroySpellController(deathSpellController);
				return false;
			}
			return true;
		}
		case HistoryBlock.Type.FATIGUE:
		{
			FatigueSpellController fatigueSpellController = CreateFatigueSpellController(taskList);
			if (!fatigueSpellController.AttachPowerTaskList(taskList))
			{
				DestroySpellController(fatigueSpellController);
				return false;
			}
			fatigueSpellController.AddFinishedTaskListCallback(OnSpellControllerFinishedTaskList);
			fatigueSpellController.AddFinishedCallback(OnSpellControllerFinished);
			if (state.IsTurnStartManagerActive())
			{
				TurnStartManager.Get().NotifyOfSpellController(fatigueSpellController);
			}
			else
			{
				fatigueSpellController.DoPowerTaskList();
			}
			return true;
		}
		case HistoryBlock.Type.JOUST:
		{
			JoustSpellController joustSpellController = CreateJoustSpellController(taskList);
			if (!DoTaskListUsingController(joustSpellController, taskList))
			{
				DestroySpellController(joustSpellController);
				return false;
			}
			return true;
		}
		case HistoryBlock.Type.REVEAL_CARD:
		{
			RevealCardSpellController revealCardSpellController = CreateRevealCardSpellController(taskList);
			if (!DoTaskListUsingController(revealCardSpellController, taskList))
			{
				DestroySpellController(revealCardSpellController);
				return false;
			}
			return true;
		}
		case HistoryBlock.Type.GAME_RESET:
		{
			ResetGameSpellController gameResetSpellController = CreateResetGameSpellController(taskList);
			if (!DoTaskListUsingController(gameResetSpellController, taskList))
			{
				DestroySpellController(gameResetSpellController);
				return false;
			}
			return true;
		}
		case HistoryBlock.Type.PLAY:
			CheckDeactivatePlaySpellForSpellPlayBlock(taskList);
			CheckDeactivatePlaySpellForTransformation(taskList);
			TriggerLettuceSpeedTileVisual(taskList);
			break;
		}
		Log.Power.Print("PowerProcessor.DoTaskListForCard() - unhandled BlockType {0} for sourceEntity {1}", blockType, sourceEntity);
		return false;
	}

	private IEnumerator DoTaskListAfterDelay(float delayAmount)
	{
		yield return new WaitForSeconds(delayAmount);
		foreach (PowerTaskList task in m_starshipLaunchTriggerTasks)
		{
			TriggerSpellController triggerSpellController = CreateTriggerSpellController(task);
			if (!DoTaskListUsingController(triggerSpellController, task))
			{
				DestroySpellController(triggerSpellController);
			}
		}
		m_starshipLaunchTriggerTasks.Clear();
		m_IsWaitingForStarshipLandingDelay = false;
	}

	private void TriggerLettuceSpeedTileVisual(PowerTaskList taskList)
	{
		if (!taskList.IsStartOfBlock())
		{
			return;
		}
		Card abilityOwner = taskList?.GetSourceEntity()?.GetLettuceAbilityOwner()?.GetCard();
		if (!(abilityOwner == null))
		{
			abilityOwner.ActivateActorSpell(SpellType.MERCENARIES_COMBAT_BOOSH);
			abilityOwner.ActivateActorSpell(SpellType.MERCENARIES_HIGHLIGHT_ACTING_MINION);
			if (Gameplay.Get() != null)
			{
				WaitForLettuceAbilityBigCardThenContinuePowerProcessing(abilityOwner, Gameplay.Get().LettuceAbilityToken).Forget();
			}
		}
	}

	private async UniTaskVoid WaitForLettuceAbilityBigCardThenContinuePowerProcessing(Card actingMerc, CancellationToken token)
	{
		while (!HistoryManager.Get().HasBigCard())
		{
			await UniTask.Yield(PlayerLoopTiming.Update, token);
		}
		HistoryManager.Get().NotifyOfLettuceSpeedTileSpellFinished();
		LayerUtils.SetLayer(HistoryManager.Get().GetCurrentBigCard().m_mainCardActor.gameObject, GameLayer.IgnoreFullScreenEffects);
		GameState.Get().SetBusy(busy: false);
		await DisableMercenaryHighlightAfterBigCardFinishes(actingMerc, token);
	}

	private async UniTask DisableMercenaryHighlightAfterBigCardFinishes(Card activeMercenary, CancellationToken token)
	{
		if (!(activeMercenary == null))
		{
			while (HistoryManager.Get().HasBigCard())
			{
				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}
			Spell highlightSpell = activeMercenary.GetActorSpell(SpellType.MERCENARIES_HIGHLIGHT_ACTING_MINION, loadIfNeeded: false);
			if (highlightSpell != null)
			{
				highlightSpell.ActivateState(SpellStateType.DEATH);
			}
		}
	}

	private void CheckDeactivatePlaySpellForSpellPlayBlock(PowerTaskList taskList)
	{
		if (taskList.GetOrigin() != taskList)
		{
			return;
		}
		PowerTaskList nextTaskList = ((GetPowerQueue().Count > 0) ? GetPowerQueue().Peek() : null);
		if (nextTaskList != null && nextTaskList.GetParent() == taskList)
		{
			return;
		}
		Entity entity = taskList.GetSourceEntity();
		if (entity != null && entity.GetCardType() == TAG_CARDTYPE.SPELL)
		{
			Card card = entity.GetCard();
			if (!(card == null))
			{
				card.DeactivatePlaySpell();
			}
		}
	}

	private void CheckDeactivatePlaySpellForTransformation(PowerTaskList taskList)
	{
		if (taskList.GetBlockEnd() == null)
		{
			return;
		}
		PowerTaskList nextTaskList = ((GetPowerQueue().Count > 0) ? GetPowerQueue().Peek() : null);
		if (nextTaskList != null && nextTaskList.GetParent() == taskList)
		{
			return;
		}
		Entity entity = taskList.GetSourceEntity();
		if (entity != null && entity.HasTag(GAME_TAG.TRANSFORMED_FROM_CARD) && entity.GetCardType() == TAG_CARDTYPE.SPELL)
		{
			Card card = entity.GetCard();
			if (!(card == null))
			{
				card.DeactivatePlaySpell();
			}
		}
	}

	private bool DoSubSpellTaskListWithController(PowerTaskList taskList)
	{
		if (m_subSpellController == null)
		{
			m_subSpellController = CreateSpellController<SubSpellController>(null, "SubSpellController.prefab:34966ff41154fce469d3ccb6d3b1655e");
		}
		if (!m_subSpellController.AttachPowerTaskList(taskList))
		{
			return false;
		}
		m_subSpellController.AddFinishedTaskListCallback(OnSpellControllerFinishedTaskList);
		m_subSpellController.DoPowerTaskList();
		return true;
	}

	private bool DoTaskListUsingController(SpellController spellController, PowerTaskList taskList)
	{
		if (spellController == null)
		{
			Log.Power.Print("PowerProcessor.DoTaskListUsingController() - spellController=null");
			return false;
		}
		if (!spellController.AttachPowerTaskList(taskList))
		{
			return false;
		}
		spellController.AddFinishedTaskListCallback(OnSpellControllerFinishedTaskList);
		spellController.AddFinishedCallback(OnSpellControllerFinished);
		spellController.DoPowerTaskList();
		return true;
	}

	private void OnSpellControllerFinishedTaskList(SpellController spellController)
	{
		spellController.DetachPowerTaskList();
		if (m_currentTaskList != null)
		{
			DoCurrentTaskList();
		}
	}

	private void OnSpellControllerFinished(SpellController spellController)
	{
		DestroySpellController(spellController);
	}

	private AttackSpellController CreateAttackSpellController(PowerTaskList taskList)
	{
		string attackSpellController = "AttackSpellController.prefab:12acecc85ac575e43b87ec141b89269a";
		if (GameState.Get() != null && GameState.Get().GetGameEntity() != null)
		{
			string attackSpellControllerOverride = GameState.Get().GetGameEntity().GetAttackSpellControllerOverride(taskList.GetAttacker());
			if (!string.IsNullOrEmpty(attackSpellControllerOverride))
			{
				attackSpellController = attackSpellControllerOverride;
			}
		}
		return CreateSpellController<AttackSpellController>(taskList, attackSpellController);
	}

	private MoveMinionSpellController CreateMoveMinionSpellController(PowerTaskList taskList)
	{
		return CreateSpellController<MoveMinionSpellController>(taskList);
	}

	private SecretSpellController CreateSecretSpellController(PowerTaskList taskList)
	{
		return CreateSpellController<SecretSpellController>(taskList, "SecretSpellController.prefab:553af99c12154c547bc05dc3d9832931");
	}

	private SigilSpellController CreateSigilSpellController(PowerTaskList taskList)
	{
		return CreateSpellController<SigilSpellController>(taskList, "SigilSpellController.prefab:1f80634fbf70a654bbae7bf796bf11b2");
	}

	private ObjectiveSpellController CreateObjectiveSpellController(PowerTaskList taskList)
	{
		return CreateSpellController<ObjectiveSpellController>(taskList, "ObjectiveSpellController.prefab:a3d627bc67f24e740a2e967b383ecc6e");
	}

	private SideQuestSpellController CreateSideQuestSpellController(PowerTaskList taskList)
	{
		return CreateSpellController<SideQuestSpellController>(taskList, "SideQuestSpellController.prefab:63762d08481f04642bbf3cde299feea2");
	}

	private PowerSpellController CreatePowerSpellController(PowerTaskList taskList)
	{
		return CreateSpellController<PowerSpellController>(taskList);
	}

	private TriggerSpellController CreateTriggerSpellController(PowerTaskList taskList)
	{
		return CreateSpellController<TriggerSpellController>(taskList, "TriggerSpellController.prefab:e0a2661f98a720d47ad4b85de228f4b4");
	}

	private DeathSpellController CreateDeathSpellController(PowerTaskList taskList)
	{
		return CreateSpellController<DeathSpellController>(taskList);
	}

	private FatigueSpellController CreateFatigueSpellController(PowerTaskList taskList)
	{
		return CreateSpellController<FatigueSpellController>(taskList);
	}

	private JoustSpellController CreateJoustSpellController(PowerTaskList taskList)
	{
		return CreateSpellController<JoustSpellController>(taskList, "JoustSpellController.prefab:89ac256005a4a8a46939a84460c2c221");
	}

	private RevealCardSpellController CreateRevealCardSpellController(PowerTaskList taskList)
	{
		return CreateSpellController<RevealCardSpellController>(taskList, "RevealCardSpellController.prefab:17fd7ea79bfd4c24389d535a074199b6");
	}

	private ResetGameSpellController CreateResetGameSpellController(PowerTaskList taskList)
	{
		return CreateSpellController<ResetGameSpellController>(taskList, "ResetGameSpellController.prefab:d8c1994d523574e42bffa17990917754");
	}

	private T CreateSpellController<T>(PowerTaskList taskList = null, string prefabPath = null) where T : SpellController
	{
		GameObject go;
		T component;
		if (prefabPath == null)
		{
			go = new GameObject();
			component = go.AddComponent<T>();
		}
		else
		{
			go = AssetLoader.Get().InstantiatePrefab(prefabPath);
			component = go.GetComponent<T>();
		}
		if (taskList != null)
		{
			go.name = $"{typeof(T)} [taskListId={taskList.GetId()}]";
		}
		else
		{
			go.name = $"{typeof(T)}";
		}
		return component;
	}

	private void DestroySpellController(SpellController spellController)
	{
		UnityEngine.Object.Destroy(spellController.gameObject);
	}
}
