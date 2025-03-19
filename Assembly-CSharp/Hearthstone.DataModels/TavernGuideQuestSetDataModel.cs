using Assets;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class TavernGuideQuestSetDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 857;

	private string m_Title;

	private string m_Description;

	private DataModelList<TavernGuideQuestDataModel> m_Quests = new DataModelList<TavernGuideQuestDataModel>();

	private TavernGuideQuestSet.TavernGuideQuestDisplayType m_QuestLayoutType;

	private TavernGuideQuestDataModel m_SelectedQuest;

	private TavernGuideQuestSet.TavernGuideCategory m_Category;

	private int m_ID;

	private AchievementDataModel m_CompletionAchievement;

	private bool m_HasNewQuest;

	private DataModelProperty[] m_properties = new DataModelProperty[9]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "title",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "quests",
			Type = typeof(DataModelList<TavernGuideQuestDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "quest_layout_type",
			Type = typeof(TavernGuideQuestSet.TavernGuideQuestDisplayType)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "selected_quest",
			Type = typeof(TavernGuideQuestDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "category",
			Type = typeof(TavernGuideQuestSet.TavernGuideCategory)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "completion_achievement",
			Type = typeof(AchievementDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "has_new_quest",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 857;

	public string DataModelDisplayName => "tavern_guide_quest_set";

	public string Title
	{
		get
		{
			return m_Title;
		}
		set
		{
			if (!(m_Title == value))
			{
				m_Title = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Description
	{
		get
		{
			return m_Description;
		}
		set
		{
			if (!(m_Description == value))
			{
				m_Description = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<TavernGuideQuestDataModel> Quests
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

	public TavernGuideQuestSet.TavernGuideQuestDisplayType QuestLayoutType
	{
		get
		{
			return m_QuestLayoutType;
		}
		set
		{
			if (m_QuestLayoutType != value)
			{
				m_QuestLayoutType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TavernGuideQuestDataModel SelectedQuest
	{
		get
		{
			return m_SelectedQuest;
		}
		set
		{
			if (m_SelectedQuest != value)
			{
				RemoveNestedDataModel(m_SelectedQuest);
				RegisterNestedDataModel(value);
				m_SelectedQuest = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TavernGuideQuestSet.TavernGuideCategory Category
	{
		get
		{
			return m_Category;
		}
		set
		{
			if (m_Category != value)
			{
				m_Category = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int ID
	{
		get
		{
			return m_ID;
		}
		set
		{
			if (m_ID != value)
			{
				m_ID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public AchievementDataModel CompletionAchievement
	{
		get
		{
			return m_CompletionAchievement;
		}
		set
		{
			if (m_CompletionAchievement != value)
			{
				RemoveNestedDataModel(m_CompletionAchievement);
				RegisterNestedDataModel(value);
				m_CompletionAchievement = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasNewQuest
	{
		get
		{
			return m_HasNewQuest;
		}
		set
		{
			if (m_HasNewQuest != value)
			{
				m_HasNewQuest = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public TavernGuideQuestSetDataModel()
	{
		RegisterNestedDataModel(m_Quests);
		RegisterNestedDataModel(m_SelectedQuest);
		RegisterNestedDataModel(m_CompletionAchievement);
	}

	public int GetPropertiesHashCode()
	{
		int num = (((17 * 31 + ((m_Title != null) ? m_Title.GetHashCode() : 0)) * 31 + ((m_Description != null) ? m_Description.GetHashCode() : 0)) * 31 + ((m_Quests != null) ? m_Quests.GetPropertiesHashCode() : 0)) * 31;
		_ = m_QuestLayoutType;
		int num2 = ((num + m_QuestLayoutType.GetHashCode()) * 31 + ((m_SelectedQuest != null) ? m_SelectedQuest.GetPropertiesHashCode() : 0)) * 31;
		_ = m_Category;
		int num3 = (num2 + m_Category.GetHashCode()) * 31;
		_ = m_ID;
		int num4 = ((num3 + m_ID.GetHashCode()) * 31 + ((m_CompletionAchievement != null) ? m_CompletionAchievement.GetPropertiesHashCode() : 0)) * 31;
		_ = m_HasNewQuest;
		return num4 + m_HasNewQuest.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Title;
			return true;
		case 1:
			value = m_Description;
			return true;
		case 2:
			value = m_Quests;
			return true;
		case 3:
			value = m_QuestLayoutType;
			return true;
		case 4:
			value = m_SelectedQuest;
			return true;
		case 5:
			value = m_Category;
			return true;
		case 6:
			value = m_ID;
			return true;
		case 7:
			value = m_CompletionAchievement;
			return true;
		case 8:
			value = m_HasNewQuest;
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
			Title = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			Description = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			Quests = ((value != null) ? ((DataModelList<TavernGuideQuestDataModel>)value) : null);
			return true;
		case 3:
			QuestLayoutType = ((value != null) ? ((TavernGuideQuestSet.TavernGuideQuestDisplayType)value) : TavernGuideQuestSet.TavernGuideQuestDisplayType.UNKNOWN);
			return true;
		case 4:
			SelectedQuest = ((value != null) ? ((TavernGuideQuestDataModel)value) : null);
			return true;
		case 5:
			Category = ((value != null) ? ((TavernGuideQuestSet.TavernGuideCategory)value) : TavernGuideQuestSet.TavernGuideCategory.UNKNOWN);
			return true;
		case 6:
			ID = ((value != null) ? ((int)value) : 0);
			return true;
		case 7:
			CompletionAchievement = ((value != null) ? ((AchievementDataModel)value) : null);
			return true;
		case 8:
			HasNewQuest = value != null && (bool)value;
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
		case 2:
			info = Properties[2];
			return true;
		case 3:
			info = Properties[3];
			return true;
		case 4:
			info = Properties[4];
			return true;
		case 5:
			info = Properties[5];
			return true;
		case 6:
			info = Properties[6];
			return true;
		case 7:
			info = Properties[7];
			return true;
		case 8:
			info = Properties[8];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
