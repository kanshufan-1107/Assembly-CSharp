using System.Collections;
using System.Collections.Generic;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusLettuce;
using UnityEngine;

namespace Hearthstone;

public class LettuceVillageTaskCollection : MonoBehaviour, IStaticWidgetCacheOwner
{
	[SerializeField]
	private GameObject m_scrollingRoot;

	[SerializeField]
	private AsyncReference m_listReference;

	private Widget m_taskList;

	private bool m_isListReady;

	[SerializeField]
	private ScrollLoader m_scrollLoader;

	[SerializeField]
	private GameObject m_cacheObject;

	private StaticWidgetTaskItemCache m_cache;

	public const int MAX_TASKS_PER_ROW = 2;

	private Widget m_widget;

	private bool m_isReady;

	private bool m_isDataModelReady;

	private bool m_hasPreloaded;

	private MercenaryVillageTaskCollectionDataModel m_dataModel;

	private List<MercenariesVisitorState> m_currentVisitors = new List<MercenariesVisitorState>();

	private List<MercenariesCompletedVisitorState> m_currentCompletedVisitors = new List<MercenariesCompletedVisitorState>();

	private List<MercenaryVillageTaskItemDataModel> m_currentDataModels = new List<MercenaryVillageTaskItemDataModel>();

	private Dictionary<int, MercenaryVillageTaskItemDataModel> m_mappedDataModels = new Dictionary<int, MercenaryVillageTaskItemDataModel>();

	private string m_currentSearchText;

	private HashSet<int> m_filteredMercIds = new HashSet<int>();

	private const string ChangingStatePauseRequest = "CHANGING_STATE";

	private const string ScrollLoaderPauseRequest = "SCROLL_LOADER_PAUSE";

	private Coroutine m_untilReadyCoroutine;

	public bool DoSearchAllMercData { get; set; }

	public StaticWidgetCacheBase Cache => m_cache;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		StaticWidgetTaskItemCache cache = StaticWidgetTaskItemCache.Get();
		if (cache != null)
		{
			m_cache = cache;
		}
		else
		{
			m_cache = Object.Instantiate(m_cacheObject).GetComponent<StaticWidgetTaskItemCache>();
		}
		m_scrollLoader.RegisterStartChangingState(OnScrollLoaderStateStartChanging);
		m_scrollLoader.RegisterDoneChangingState(OnScrollLoaderStateDoneChanging);
		m_scrollLoader.RegisterOnPausedChanged(OnScrollLoaderPaused);
	}

	private void Start()
	{
		m_listReference.RegisterReadyListener<Widget>(OnListReady);
		m_untilReadyCoroutine = StartCoroutine(WaitUntilReady());
	}

	private void OnDestroy()
	{
		if (m_untilReadyCoroutine != null)
		{
			StopCoroutine(m_untilReadyCoroutine);
		}
	}

	private IEnumerator WaitUntilReady()
	{
		while (!m_isListReady)
		{
			yield return null;
		}
		m_isReady = true;
		m_untilReadyCoroutine = null;
		RefreshVisuals();
	}

	public bool IsReady()
	{
		if (m_isReady)
		{
			return m_isDataModelReady;
		}
		return false;
	}

	public void OnOpen()
	{
		m_scrollLoader.Pause(isPaused: false);
	}

	public void OnClose()
	{
		m_scrollLoader.Pause(isPaused: true);
	}

	public void Refresh()
	{
		m_isDataModelReady = false;
		FetchData();
		RefreshVisuals();
	}

	public void FetchData()
	{
		m_currentVisitors.Clear();
		NetCache.NetCacheLettuceMap cachedLettuceMap = NetCache.Get().GetNetObject<NetCache.NetCacheLettuceMap>();
		List<MercenariesVisitorState> standardVisitors = LettuceVillageDataUtil.GetVisitorsByType(MercenaryVisitor.VillageVisitorType.STANDARD);
		HashSet<int> mercTeamIds = null;
		if (cachedLettuceMap != null && SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_MAP && cachedLettuceMap.Map != null && cachedLettuceMap.Map.TeamData != null && cachedLettuceMap.Map.TeamData.Count > 0)
		{
			mercTeamIds = new HashSet<int>();
			foreach (PegasusLettuce.LettuceTeam team in cachedLettuceMap.Map.TeamData)
			{
				if (team.MercenaryList == null)
				{
					continue;
				}
				foreach (LettuceTeamMercenary teamMember in team.MercenaryList.Mercenaries)
				{
					mercTeamIds.Add(teamMember.MercenaryId);
				}
			}
			foreach (MercenariesVisitorState visitor in standardVisitors)
			{
				MercenaryVisitorDbfRecord record = LettuceVillageDataUtil.GetVisitorRecordByID(visitor.VisitorId);
				if (record != null && mercTeamIds.Contains(record.MercenaryId))
				{
					m_currentVisitors.Add(visitor);
				}
			}
		}
		else
		{
			m_currentVisitors.AddRange(standardVisitors);
		}
		m_currentDataModels.Clear();
		foreach (MercenariesVisitorState state in m_currentVisitors)
		{
			if (state.ActiveTaskState == null)
			{
				continue;
			}
			bool isDirty = false;
			if (m_mappedDataModels.TryGetValue(state.VisitorId, out var dataModel))
			{
				if (LettuceVillageDataUtil.IsDataEqual(state, dataModel))
				{
					m_currentDataModels.Add(dataModel);
					continue;
				}
				isDirty = true;
			}
			MercenaryVillageTaskItemDataModel taskItem = CreateTaskItem(state);
			m_currentDataModels.Add(taskItem);
			m_mappedDataModels[state.VisitorId] = taskItem;
			if (isDirty && m_cache != null)
			{
				m_cache.Rebind(taskItem);
			}
		}
		m_currentCompletedVisitors = LettuceVillageDataUtil.GetCompletedVisitorsByType(MercenaryVisitor.VillageVisitorType.STANDARD);
		foreach (MercenariesCompletedVisitorState completedState in m_currentCompletedVisitors)
		{
			if (completedState == null)
			{
				continue;
			}
			if (mercTeamIds != null)
			{
				MercenaryVisitorDbfRecord record2 = LettuceVillageDataUtil.GetVisitorRecordByID(completedState.VisitorId);
				if (record2 == null || !mercTeamIds.Contains(record2.MercenaryId))
				{
					continue;
				}
			}
			bool isDirty2 = false;
			if (m_mappedDataModels.TryGetValue(completedState.VisitorId, out var dataModel2))
			{
				if (LettuceVillageDataUtil.IsDataEqual(completedState, dataModel2))
				{
					m_currentDataModels.Add(dataModel2);
					continue;
				}
				isDirty2 = true;
			}
			MercenaryVillageTaskItemDataModel taskItem2 = CreateTaskItem(completedState);
			m_currentDataModels.Add(taskItem2);
			m_mappedDataModels[completedState.VisitorId] = taskItem2;
			if (isDirty2 && m_cache != null)
			{
				m_cache.Rebind(taskItem2);
			}
		}
		if (!m_hasPreloaded)
		{
			if (m_cache != null)
			{
				m_cache.Preload(m_currentDataModels);
			}
			m_hasPreloaded = true;
		}
		RefreshVisuals();
	}

	public void RefreshVisuals()
	{
		if (!m_isReady)
		{
			return;
		}
		if (m_dataModel == null)
		{
			WidgetUtils.BindorCreateDataModel(m_widget, 679, ref m_dataModel);
			m_dataModel.TaskRows = new DataModelList<MercenaryVillageTaskCollectionRowDataModel>();
		}
		else
		{
			m_dataModel.TaskRows.Clear();
		}
		List<MercenaryVillageTaskItemDataModel> filteredData = FilterTaskData(m_currentDataModels);
		filteredData.Sort(CompareTaskData);
		for (int visitorIndex = 0; visitorIndex < filteredData.Count; visitorIndex++)
		{
			int rowIndex = Mathf.FloorToInt((float)visitorIndex / 2f);
			if (rowIndex >= m_dataModel.TaskRows.Count)
			{
				m_dataModel.TaskRows.Add(new MercenaryVillageTaskCollectionRowDataModel
				{
					TaskList = new DataModelList<MercenaryVillageTaskItemDataModel>()
				});
			}
			m_dataModel.TaskRows[rowIndex].TaskList.Add(filteredData[visitorIndex]);
		}
		m_taskList.BindDataModel(m_dataModel);
		m_isDataModelReady = true;
	}

	public bool TryGetDataModel(int visitorId, out MercenaryVillageTaskItemDataModel data)
	{
		return m_mappedDataModels.TryGetValue(visitorId, out data);
	}

	private static int CompareTaskData(MercenaryVillageTaskItemDataModel visitorStateA, MercenaryVillageTaskItemDataModel visitorStateB)
	{
		if (visitorStateA.TaskStatus == MercenariesTaskState.Status.CLAIMED || visitorStateB.TaskStatus == MercenariesTaskState.Status.CLAIMED)
		{
			return visitorStateA.TaskStatus.CompareTo(visitorStateB.TaskStatus);
		}
		float percentageAComplete = ((visitorStateA.ProgressNeeded == 0) ? 1f : ((float)visitorStateA.Progress / (float)visitorStateA.ProgressNeeded));
		float percentageBComplete = ((visitorStateB.ProgressNeeded == 0) ? 1f : ((float)visitorStateB.Progress / (float)visitorStateB.ProgressNeeded));
		bool isAComplete = percentageAComplete >= 1f;
		bool isBComplete = percentageBComplete >= 1f;
		if (isAComplete || isBComplete)
		{
			return isBComplete.CompareTo(isAComplete);
		}
		if (percentageAComplete != percentageBComplete)
		{
			return percentageBComplete.CompareTo(percentageAComplete);
		}
		if (visitorStateA.TaskChainIndex != visitorStateB.TaskChainIndex)
		{
			return visitorStateB.TaskChainIndex.CompareTo(visitorStateA.TaskChainIndex);
		}
		return string.Compare(visitorStateA.MercenaryShortName, visitorStateB.MercenaryShortName, ignoreCase: false, Localization.GetCultureInfo());
	}

	private MercenaryVillageTaskItemDataModel CreateTaskItem(MercenariesVisitorState state)
	{
		return ModifyTaskItem(LettuceVillageDataUtil.CreateTaskModelFromActiveVisitorState(state));
	}

	private MercenaryVillageTaskItemDataModel CreateTaskItem(MercenariesCompletedVisitorState completedState)
	{
		return ModifyTaskItem(LettuceVillageDataUtil.CreateTaskModelFromCompletedVisitorState(completedState));
	}

	private MercenaryVillageTaskItemDataModel ModifyTaskItem(MercenaryVillageTaskItemDataModel baseData)
	{
		baseData.TaskStyle = LettuceVillageTaskBoard.TaskStyle.COLLECTION;
		return baseData;
	}

	private void UpdateCurrentSearch(string searchString)
	{
		m_filteredMercIds.Clear();
		if (string.IsNullOrWhiteSpace(searchString))
		{
			m_currentSearchText = null;
		}
		else
		{
			searchString = searchString.ToLower();
			if (searchString == m_currentSearchText)
			{
				return;
			}
			m_currentSearchText = searchString;
			CollectionManager collectionManager = CollectionManager.Get();
			string currentSearchText = m_currentSearchText;
			bool? isOwned = true;
			bool? excludeCraftableFromOwned = true;
			foreach (LettuceMercenary merc in collectionManager.FindMercenaries(currentSearchText, isOwned, null, null, excludeCraftableFromOwned, ordered: false, null).m_mercenaries)
			{
				m_filteredMercIds.Add(merc.ID);
			}
		}
		RefreshVisuals();
	}

	private List<MercenaryVillageTaskItemDataModel> FilterTaskData(List<MercenaryVillageTaskItemDataModel> dataList)
	{
		List<MercenaryVillageTaskItemDataModel> filteredDataList = new List<MercenaryVillageTaskItemDataModel>();
		foreach (MercenaryVillageTaskItemDataModel data in dataList)
		{
			if (IsTaskDataFiltered(data))
			{
				filteredDataList.Add(data);
			}
		}
		return filteredDataList;
	}

	private bool IsTaskDataFiltered(MercenaryVillageTaskItemDataModel data)
	{
		if (m_currentSearchText == null)
		{
			return true;
		}
		if (DoSearchAllMercData)
		{
			if (m_filteredMercIds.Contains(data.MercenaryId))
			{
				return true;
			}
		}
		else if (data.MercenaryName.ToLower().Contains(m_currentSearchText))
		{
			return true;
		}
		if (data.Title.ToLower().Contains(m_currentSearchText))
		{
			return true;
		}
		if (data.Description.ToLower().Contains(m_currentSearchText))
		{
			return true;
		}
		string taskStatusLocKey = null;
		switch (data.TaskStatus)
		{
		case MercenariesTaskState.Status.NEW:
		case MercenariesTaskState.Status.ACTIVE:
			taskStatusLocKey = "GLUE_TASK_BOARD_SEARCH_ACTIVE";
			break;
		case MercenariesTaskState.Status.COMPLETE:
			taskStatusLocKey = "GLUE_TASK_BOARD_SEARCH_CLAIMABLE";
			break;
		case MercenariesTaskState.Status.CLAIMED:
			taskStatusLocKey = "GLUE_TASK_BOARD_SEARCH_COMPLETED";
			break;
		}
		if (taskStatusLocKey != null && string.Compare(m_currentSearchText, GameStrings.Get(taskStatusLocKey), ignoreCase: true, Localization.GetCultureInfo()) == 0)
		{
			return true;
		}
		return false;
	}

	public void OnSearchActivated()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			EnableInput(enable: false);
		}
	}

	public void OnSearchDeactivated(string oldSearchText, string newSearchText)
	{
		EnableInput(enable: true);
		UpdateCurrentSearch(newSearchText);
	}

	public void OnSearchCleared(bool updateVisuals)
	{
		EnableInput(enable: true);
		UpdateCurrentSearch(null);
	}

	private void OnListReady(Widget list)
	{
		m_isListReady = true;
		if (list == null)
		{
			Log.Lettuce.PrintError("Task list could not be found in LettuceVillageTaskCollection.");
		}
		else
		{
			m_taskList = list;
		}
	}

	private void OnScrollLoaderStateStartChanging()
	{
		if (m_cache != null)
		{
			m_cache.Pause(pause: true, "CHANGING_STATE");
		}
	}

	private void OnScrollLoaderStateDoneChanging()
	{
		if (m_cache != null)
		{
			m_cache.Pause(pause: false, "CHANGING_STATE");
		}
	}

	private void OnScrollLoaderPaused(bool isPaused)
	{
		if (m_cache != null)
		{
			m_cache.Pause(isPaused, "SCROLL_LOADER_PAUSE");
		}
	}

	private void EnableInput(bool enable)
	{
		SendEventUpwardStateAction.SendEventUpward(base.gameObject, enable ? "ENABLE_ALL_TASK_INPUT" : "DISABLE_ALL_TASK_INPUT");
	}
}
