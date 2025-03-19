using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class QuestInspectionPopup : MonoBehaviour
{
	[SerializeField]
	private GameObject m_centerBone;

	[SerializeField]
	private Widget m_tileSingleRow;

	[SerializeField]
	private Widget[] m_tileMultiRow;

	[SerializeField]
	private int m_tilesPerSingleRow = 4;

	[SerializeField]
	private int m_tilesPerMultiRow = 3;

	private WidgetTemplate m_widget;

	private int m_shownQuestId = -1;

	private string m_currentQuestTileEvent = "";

	private const string CODE_SHOW_QUEST_ID = "CODE_SHOW_QUEST_ID";

	private const string STATE_SHOW_TILE_ALONE = "SHOW_TILE_ALONE";

	private const string STATE_SHOW_SINGLE_ROW = "SHOW_SINGLE_ROW";

	private const string STATE_SHOW_MULTI_ROW = "SHOW_MULTI_ROW";

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterDoneChangingStatesListener(HandleDoneChangingStates);
		m_widget.RegisterEventListener(HandleEvent);
	}

	private void HandleEvent(string eventName)
	{
		if (eventName == "CODE_SHOW_QUEST_ID")
		{
			int questId = (int)m_widget.GetDataModel<EventDataModel>().Payload;
			ShowQuest(questId);
		}
	}

	private void HandleDoneChangingStates(object _)
	{
		CenterListables();
	}

	private void ShowQuest(int questId)
	{
		int startingQuestId = questId;
		if (questId != m_shownQuestId)
		{
			QuestManager questManager = QuestManager.Get();
			m_shownQuestId = questId;
			QuestListDataModel questListDataModel = new QuestListDataModel();
			while (questId > 0)
			{
				QuestDataModel questDataModel = questManager.CreateQuestDataModelById(questId);
				questDataModel.DisplayMode = QuestManager.QuestDisplayMode.Inspection;
				if (questId == startingQuestId)
				{
					PlayerQuestState playerQuestState = questManager.GetPlayerQuestStateById(questId);
					if (!QuestManager.IsQuestActive(playerQuestState) && !QuestManager.IsQuestComplete(playerQuestState))
					{
						QuestDbfRecord questRecord = GameDbf.Quest.GetRecord(questId);
						questDataModel.TimeUntilNextQuest = questManager.GetTimeUntilNextQuestString(questRecord.QuestPool);
					}
				}
				questListDataModel.Quests.Add(questDataModel);
				questId = questDataModel.NextInChain;
			}
			m_currentQuestTileEvent = "";
			if (questListDataModel.Quests.Count == 1)
			{
				m_widget.BindDataModel(questListDataModel.Quests[0]);
				UnbindTileSingleRowDataModel();
				UnbindMultiRowDataModel();
				m_currentQuestTileEvent = "SHOW_TILE_ALONE";
			}
			else if (questListDataModel.Quests.Count <= m_tilesPerSingleRow)
			{
				m_tileSingleRow.BindDataModel(questListDataModel);
				UnbindSingleQuestDataModel();
				UnbindMultiRowDataModel();
				m_currentQuestTileEvent = "SHOW_SINGLE_ROW";
			}
			else if (questListDataModel.Quests.Count > m_tilesPerSingleRow)
			{
				for (int i = 0; i < m_tileMultiRow.Length; i++)
				{
					DataModelList<QuestDataModel> rowDataModelList = questListDataModel.Quests.Skip(m_tilesPerMultiRow * i).Take(m_tilesPerMultiRow).ToDataModelList();
					m_tileMultiRow[i].BindDataModel(new QuestListDataModel
					{
						Quests = rowDataModelList
					});
				}
				UnbindSingleQuestDataModel();
				UnbindTileSingleRowDataModel();
				m_currentQuestTileEvent = "SHOW_MULTI_ROW";
			}
		}
		if (!string.IsNullOrEmpty(m_currentQuestTileEvent))
		{
			m_widget.TriggerEvent(m_currentQuestTileEvent);
		}
	}

	private void UnbindSingleQuestDataModel()
	{
		m_widget.UnbindDataModel(207);
	}

	private void UnbindTileSingleRowDataModel()
	{
		m_tileSingleRow.UnbindDataModel(208);
	}

	private void UnbindMultiRowDataModel()
	{
		for (int i = 0; i < m_tileMultiRow.Length; i++)
		{
			m_tileMultiRow[i].UnbindDataModel(208);
		}
	}

	private void CenterListables()
	{
		Listable[] componentsInChildren = GetComponentsInChildren<Listable>(includeInactive: false);
		foreach (Listable listable in componentsInChildren)
		{
			if (!(listable == null) && listable.WidgetItems.Count() != 0)
			{
				BoxCollider orCreateColliderFromItemBounds = listable.GetOrCreateColliderFromItemBounds(isEnabled: false);
				TransformUtil.SetPoint(orCreateColliderFromItemBounds, Anchor.CENTER_XZ, m_centerBone, Anchor.CENTER_XZ);
				TransformUtil.SetLocalPosZ(orCreateColliderFromItemBounds, 0f);
			}
		}
	}
}
