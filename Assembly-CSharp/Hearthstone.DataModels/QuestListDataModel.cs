using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class QuestListDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 208;

	private DataModelList<QuestDataModel> m_Quests = new DataModelList<QuestDataModel>();

	private string m_BankedQuestCountMessage;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "quests",
			Type = typeof(DataModelList<QuestDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "banked_quest_count_message",
			Type = typeof(string)
		}
	};

	public int DataModelId => 208;

	public string DataModelDisplayName => "quest_list";

	public DataModelList<QuestDataModel> Quests
	{
		get
		{
			return m_Quests;
		}
		set
		{
			if (m_Quests != value)
			{
				RemoveNestedDataModel(m_Quests);
				RegisterNestedDataModel(value);
				m_Quests = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string BankedQuestCountMessage
	{
		get
		{
			return m_BankedQuestCountMessage;
		}
		set
		{
			if (!(m_BankedQuestCountMessage == value))
			{
				m_BankedQuestCountMessage = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public QuestListDataModel()
	{
		RegisterNestedDataModel(m_Quests);
	}

	public int GetPropertiesHashCode()
	{
		return (17 * 31 + ((m_Quests != null) ? m_Quests.GetPropertiesHashCode() : 0)) * 31 + ((m_BankedQuestCountMessage != null) ? m_BankedQuestCountMessage.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Quests;
			return true;
		case 1:
			value = m_BankedQuestCountMessage;
			return true;
		default:
			value = null;
			return false;
		}
	}

	public bool SetPropertyValue(int id, object value)
	{
		switch (id)
		{
		case 0:
			Quests = ((value != null) ? ((DataModelList<QuestDataModel>)value) : null);
			return true;
		case 1:
			BankedQuestCountMessage = ((value != null) ? ((string)value) : null);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 0:
			info = Properties[0];
			return true;
		case 1:
			info = Properties[1];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
