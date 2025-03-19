using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class TavernGuideDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 858;

	private DataModelList<TavernGuideQuestSetCategoryDataModel> m_TavernGuideQuestSetCategories = new DataModelList<TavernGuideQuestSetCategoryDataModel>();

	private TavernGuideQuestSetDataModel m_SelectedTavernGuideQuestSet;

	private int m_SelectedCategoryIndex;

	private int m_SelectedQuestSetIndex;

	private bool m_IsOpening;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "quest_set_categories",
			Type = typeof(DataModelList<TavernGuideQuestSetCategoryDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "selected_quest_set",
			Type = typeof(TavernGuideQuestSetDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "selected_quest_set_category_index",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "selected_quest_set_index",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "is_opening",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 858;

	public string DataModelDisplayName => "tavern_guide";

	public DataModelList<TavernGuideQuestSetCategoryDataModel> TavernGuideQuestSetCategories
	{
		get
		{
			return m_TavernGuideQuestSetCategories;
		}
		set
		{
			if (m_TavernGuideQuestSetCategories != value)
			{
				RemoveNestedDataModel(m_TavernGuideQuestSetCategories);
				RegisterNestedDataModel(value);
				m_TavernGuideQuestSetCategories = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TavernGuideQuestSetDataModel SelectedTavernGuideQuestSet
	{
		get
		{
			return m_SelectedTavernGuideQuestSet;
		}
		set
		{
			if (m_SelectedTavernGuideQuestSet != value)
			{
				RemoveNestedDataModel(m_SelectedTavernGuideQuestSet);
				RegisterNestedDataModel(value);
				m_SelectedTavernGuideQuestSet = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int SelectedCategoryIndex
	{
		get
		{
			return m_SelectedCategoryIndex;
		}
		set
		{
			if (m_SelectedCategoryIndex != value)
			{
				m_SelectedCategoryIndex = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int SelectedQuestSetIndex
	{
		get
		{
			return m_SelectedQuestSetIndex;
		}
		set
		{
			if (m_SelectedQuestSetIndex != value)
			{
				m_SelectedQuestSetIndex = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsOpening
	{
		get
		{
			return m_IsOpening;
		}
		set
		{
			if (m_IsOpening != value)
			{
				m_IsOpening = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public TavernGuideDataModel()
	{
		RegisterNestedDataModel(m_TavernGuideQuestSetCategories);
		RegisterNestedDataModel(m_SelectedTavernGuideQuestSet);
	}

	public int GetPropertiesHashCode()
	{
		int num = ((17 * 31 + ((m_TavernGuideQuestSetCategories != null) ? m_TavernGuideQuestSetCategories.GetPropertiesHashCode() : 0)) * 31 + ((m_SelectedTavernGuideQuestSet != null) ? m_SelectedTavernGuideQuestSet.GetPropertiesHashCode() : 0)) * 31;
		_ = m_SelectedCategoryIndex;
		int num2 = (num + m_SelectedCategoryIndex.GetHashCode()) * 31;
		_ = m_SelectedQuestSetIndex;
		int num3 = (num2 + m_SelectedQuestSetIndex.GetHashCode()) * 31;
		_ = m_IsOpening;
		return num3 + m_IsOpening.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_TavernGuideQuestSetCategories;
			return true;
		case 1:
			value = m_SelectedTavernGuideQuestSet;
			return true;
		case 2:
			value = m_SelectedCategoryIndex;
			return true;
		case 3:
			value = m_SelectedQuestSetIndex;
			return true;
		case 4:
			value = m_IsOpening;
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
			TavernGuideQuestSetCategories = ((value != null) ? ((DataModelList<TavernGuideQuestSetCategoryDataModel>)value) : null);
			return true;
		case 1:
			SelectedTavernGuideQuestSet = ((value != null) ? ((TavernGuideQuestSetDataModel)value) : null);
			return true;
		case 2:
			SelectedCategoryIndex = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			SelectedQuestSetIndex = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			IsOpening = value != null && (bool)value;
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
