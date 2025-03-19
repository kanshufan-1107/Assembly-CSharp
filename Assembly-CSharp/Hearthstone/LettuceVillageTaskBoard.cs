using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusLettuce;
using PegasusShared;
using UnityEngine;

namespace Hearthstone;

public class LettuceVillageTaskBoard : MonoBehaviour
{
	public enum TaskStyle
	{
		INVALID = -1,
		NORMAL,
		LEGENDARY,
		LOCKED,
		VACANT,
		VACANT_CAMPAIGN,
		VACANT_EVENT,
		COLLECTION,
		RENOWN
	}

	private struct TaskBoardDataModel
	{
		public MercenaryVillageTaskItemDataModel dataModel;

		public Date createdTime;

		private int GetPriority()
		{
			return dataModel.TaskType switch
			{
				MercenaryVisitor.VillageVisitorType.SPECIAL => 2, 
				MercenaryVisitor.VillageVisitorType.EVENT => 1, 
				_ => 0, 
			};
		}

		public int CompareTo(TaskBoardDataModel dataModel)
		{
			int priorityA = GetPriority();
			int priorityB = dataModel.GetPriority();
			if (priorityA != priorityB)
			{
				return priorityB.CompareTo(priorityA);
			}
			long utcA = TimeUtils.PegDateToFileTimeUtc(createdTime);
			long utcB = TimeUtils.PegDateToFileTimeUtc(dataModel.createdTime);
			return utcA.CompareTo(utcB);
		}
	}

	public const int MAX_TASKS_PER_ROW = 2;

	public const int MAX_TASKS = 6;

	public const int MAX_SPECIAL_TASKS = 2;

	private Widget m_widget;

	private MercenaryVillageTaskBoardDataModel m_dataModel;

	private List<MercenariesVisitorState> m_currentVisitors = new List<MercenariesVisitorState>();

	private List<MercenariesRenownOfferData> m_currentRenownOffers = new List<MercenariesRenownOfferData>();

	private List<TaskBoardDataModel> m_currentDataModels = new List<TaskBoardDataModel>();

	private Dictionary<int, MercenaryVillageTaskItemDataModel> m_mappedDataModels = new Dictionary<int, MercenaryVillageTaskItemDataModel>();

	private int m_numCampaignVisitors;

	private int m_numEventVisitors;

	private int m_numProceduralVisitors;

	private int m_numRenownOffers;

	private int m_prevUnlockedSlots;

	private bool m_dataModelIsReady;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
	}

	public void Refresh()
	{
		m_dataModelIsReady = false;
		FetchData();
		RefreshVisuals();
	}

	public bool IsDataModelReady()
	{
		return m_dataModelIsReady;
	}

	public void FetchData()
	{
		m_currentVisitors.Clear();
		m_currentRenownOffers.Clear();
		m_currentDataModels.Clear();
		m_mappedDataModels.Clear();
		m_numCampaignVisitors = 0;
		m_numEventVisitors = 0;
		m_numProceduralVisitors = 0;
		m_numRenownOffers = 0;
		m_currentVisitors.AddRange(LettuceVillageDataUtil.GetVisitorsByType(MercenaryVisitor.VillageVisitorType.SPECIAL));
		m_numCampaignVisitors = m_currentVisitors.Count;
		m_currentVisitors.AddRange(LettuceVillageDataUtil.GetVisitorsByType(MercenaryVisitor.VillageVisitorType.EVENT));
		m_numEventVisitors = m_currentVisitors.Count - m_numCampaignVisitors;
		m_currentVisitors.AddRange(LettuceVillageDataUtil.GetVisitorsByType(MercenaryVisitor.VillageVisitorType.PROCEDURAL));
		m_numProceduralVisitors = m_currentVisitors.Count - (m_numCampaignVisitors + m_numEventVisitors);
		m_currentRenownOffers.AddRange(LettuceVillageDataUtil.ActiveRenownStates);
		m_numRenownOffers = m_currentRenownOffers.Count;
		foreach (MercenariesVisitorState visitorState in m_currentVisitors)
		{
			MercenaryVillageTaskItemDataModel dataModel = LettuceVillageDataUtil.CreateTaskModelFromActiveVisitorState(visitorState);
			m_currentDataModels.Add(new TaskBoardDataModel
			{
				dataModel = dataModel,
				createdTime = visitorState.LastArrivalDate
			});
			m_mappedDataModels[visitorState.VisitorId] = dataModel;
		}
		foreach (MercenariesRenownOfferData renownOffer in m_currentRenownOffers)
		{
			MercenaryVillageTaskItemDataModel dataModel2 = LettuceVillageDataUtil.CreateTaskModelFromRenownOffer(renownOffer);
			m_currentDataModels.Add(new TaskBoardDataModel
			{
				dataModel = dataModel2,
				createdTime = renownOffer.AddedDate
			});
			m_mappedDataModels[renownOffer.MercenaryId] = dataModel2;
		}
		RefreshVisuals();
	}

	public void RefreshVisuals()
	{
		if (m_dataModel == null)
		{
			WidgetUtils.BindorCreateDataModel(m_widget, 307, ref m_dataModel);
			m_dataModel.TaskListRow = new DataModelList<MercenaryVillageTaskBoardRowDataModel>();
		}
		else
		{
			m_dataModel.TaskListRow.Clear();
		}
		int numUnlockedSlots = (m_prevUnlockedSlots = LettuceVillageDataUtil.GetCurrentTierPropertyForBuilding(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD, TierProperties.Buildingtierproperty.TASKSLOTS));
		int lockedSlots = 6 - numUnlockedSlots - 2;
		int vacantSlots = numUnlockedSlots - m_numProceduralVisitors - m_numRenownOffers;
		int numTasksAdded = 0;
		int realTasksAdded = 0;
		if (m_currentDataModels.Count > 0)
		{
			m_currentDataModels.Sort((TaskBoardDataModel dataModelA, TaskBoardDataModel dataModelB) => dataModelA.CompareTo(dataModelB));
			if (m_numCampaignVisitors == 0)
			{
				AddEmptyTaskItemWithStyle(TaskStyle.VACANT_CAMPAIGN);
				numTasksAdded++;
			}
			else
			{
				AddTaskItem(m_currentDataModels[0].dataModel);
				numTasksAdded++;
				realTasksAdded++;
			}
			if (m_numEventVisitors == 0 || m_currentDataModels.Count <= realTasksAdded)
			{
				AddEmptyTaskItemWithStyle(TaskStyle.VACANT_EVENT);
				numTasksAdded++;
			}
			else
			{
				AddTaskItem(m_currentDataModels[realTasksAdded].dataModel);
				numTasksAdded++;
				realTasksAdded++;
			}
			for (int i = realTasksAdded; i < m_currentDataModels.Count; i++)
			{
				MercenaryVillageTaskItemDataModel dataModel = m_currentDataModels[i].dataModel;
				if (numTasksAdded < 6)
				{
					AddTaskItem(dataModel);
					numTasksAdded++;
					realTasksAdded++;
				}
			}
		}
		TaskStyle vacantTaskStyle = (GameUtils.IsMercenariesVillageTutorialComplete() ? TaskStyle.VACANT : TaskStyle.LOCKED);
		for (int j = 0; j < vacantSlots; j++)
		{
			if (numTasksAdded < 6)
			{
				AddEmptyTaskItemWithStyle(vacantTaskStyle);
				numTasksAdded++;
			}
		}
		for (int k = 0; k < lockedSlots; k++)
		{
			if (numTasksAdded < 6)
			{
				AddEmptyTaskItemWithStyle(TaskStyle.LOCKED);
				numTasksAdded++;
			}
		}
		m_widget.BindDataModel(m_dataModel);
		m_dataModelIsReady = true;
	}

	public bool TryGetDataModel(int visitorId, out MercenaryVillageTaskItemDataModel data)
	{
		return m_mappedDataModels.TryGetValue(visitorId, out data);
	}

	public IEnumerable<MercenariesVisitorState> GetCurrentVisitors()
	{
		return m_currentVisitors;
	}

	public void OnVillageInfoUpdated()
	{
		if (LettuceVillageDataUtil.GetCurrentTierPropertyForBuilding(MercenaryBuilding.Mercenarybuildingtype.TASKBOARD, TierProperties.Buildingtierproperty.TASKSLOTS) != m_prevUnlockedSlots)
		{
			Refresh();
		}
	}

	private void AddTaskItem(MercenaryVillageTaskItemDataModel taskItem)
	{
		if (taskItem != null && m_dataModel != null)
		{
			int numRows = m_dataModel.TaskListRow.Count;
			MercenaryVillageTaskBoardRowDataModel lastBoardRow = null;
			if (numRows > 0)
			{
				lastBoardRow = m_dataModel.TaskListRow[numRows - 1];
			}
			if (lastBoardRow == null || lastBoardRow.TaskList.Count >= 2)
			{
				lastBoardRow = new MercenaryVillageTaskBoardRowDataModel();
				m_dataModel.TaskListRow.Add(lastBoardRow);
			}
			lastBoardRow.TaskList.Add(taskItem);
		}
	}

	private void AddEmptyTaskItemWithStyle(TaskStyle style)
	{
		MercenaryVillageTaskItemDataModel taskItem = new MercenaryVillageTaskItemDataModel
		{
			TaskStyle = style,
			ProgressMessage = string.Empty,
			Progress = 0,
			ProgressNeeded = 1,
			MercenaryName = string.Empty,
			MercenaryCard = null
		};
		AddTaskItem(taskItem);
	}
}
