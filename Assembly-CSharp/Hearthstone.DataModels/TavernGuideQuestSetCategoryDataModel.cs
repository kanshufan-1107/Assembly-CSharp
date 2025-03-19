using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class TavernGuideQuestSetCategoryDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 870;

	private DataModelList<TavernGuideQuestSetDataModel> m_TavernGuideQuestSets = new DataModelList<TavernGuideQuestSetDataModel>();

	private string m_Title;

	private bool m_Enabled;

	private bool m_HasNewQuest;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "quest_sets",
			Type = typeof(DataModelList<TavernGuideQuestSetDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "title",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "enabled",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "has_new_quest",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 870;

	public string DataModelDisplayName => "tavern_guide_quest_set_category";

	public DataModelList<TavernGuideQuestSetDataModel> TavernGuideQuestSets
	{
		get
		{
			return m_TavernGuideQuestSets;
		}
		set
		{
			if (m_TavernGuideQuestSets != value)
			{
				RemoveNestedDataModel(m_TavernGuideQuestSets);
				RegisterNestedDataModel(value);
				m_TavernGuideQuestSets = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

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

	public bool Enabled
	{
		get
		{
			return m_Enabled;
		}
		set
		{
			if (m_Enabled != value)
			{
				m_Enabled = value;
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

	public TavernGuideQuestSetCategoryDataModel()
	{
		RegisterNestedDataModel(m_TavernGuideQuestSets);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_TavernGuideQuestSets != null && !inspectedDataModels.Contains(m_TavernGuideQuestSets.GetHashCode()))
		{
			inspectedDataModels.Add(m_TavernGuideQuestSets.GetHashCode());
			hash = hash * 31 + m_TavernGuideQuestSets.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		hash = hash * 31 + ((m_Title != null) ? m_Title.GetHashCode() : 0);
		int num = hash * 31;
		_ = m_Enabled;
		hash = num + m_Enabled.GetHashCode();
		int num2 = hash * 31;
		_ = m_HasNewQuest;
		return num2 + m_HasNewQuest.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_TavernGuideQuestSets;
			return true;
		case 1:
			value = m_Title;
			return true;
		case 2:
			value = m_Enabled;
			return true;
		case 3:
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
			TavernGuideQuestSets = ((value != null) ? ((DataModelList<TavernGuideQuestSetDataModel>)value) : null);
			return true;
		case 1:
			Title = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			Enabled = value != null && (bool)value;
			return true;
		case 3:
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
