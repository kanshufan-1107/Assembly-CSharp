using System.Collections.Generic;
using Hearthstone.Progression;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class TavernGuideQuestDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 865;

	private string m_Title;

	private string m_SelectedDescription;

	private string m_RecommendedClasses;

	private QuestDataModel m_Quest;

	private TavernGuideManager.TavernGuideQuestStatus m_Status;

	private bool m_ShouldShowProgressAmount;

	private int m_ID;

	private string m_UnlockRequirementsDescription;

	private DataModelProperty[] m_properties = new DataModelProperty[8]
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
			PropertyDisplayName = "selected_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "recommended_classes",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "quest",
			Type = typeof(QuestDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "status",
			Type = typeof(TavernGuideManager.TavernGuideQuestStatus)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "should_show_progress_amount",
			Type = typeof(bool)
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
			PropertyDisplayName = "unlock_requirements_description",
			Type = typeof(string)
		}
	};

	public int DataModelId => 865;

	public string DataModelDisplayName => "tavern_guide_quest";

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

	public string SelectedDescription
	{
		get
		{
			return m_SelectedDescription;
		}
		set
		{
			if (!(m_SelectedDescription == value))
			{
				m_SelectedDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string RecommendedClasses
	{
		get
		{
			return m_RecommendedClasses;
		}
		set
		{
			if (!(m_RecommendedClasses == value))
			{
				m_RecommendedClasses = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public QuestDataModel Quest
	{
		get
		{
			return m_Quest;
		}
		set
		{
			if (m_Quest != value)
			{
				RemoveNestedDataModel(m_Quest);
				RegisterNestedDataModel(value);
				m_Quest = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TavernGuideManager.TavernGuideQuestStatus Status
	{
		get
		{
			return m_Status;
		}
		set
		{
			if (m_Status != value)
			{
				m_Status = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShouldShowProgressAmount
	{
		get
		{
			return m_ShouldShowProgressAmount;
		}
		set
		{
			if (m_ShouldShowProgressAmount != value)
			{
				m_ShouldShowProgressAmount = value;
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

	public string UnlockRequirementsDescription
	{
		get
		{
			return m_UnlockRequirementsDescription;
		}
		set
		{
			if (!(m_UnlockRequirementsDescription == value))
			{
				m_UnlockRequirementsDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public TavernGuideQuestDataModel()
	{
		RegisterNestedDataModel(m_Quest);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_Title != null) ? m_Title.GetHashCode() : 0);
		hash = hash * 31 + ((m_SelectedDescription != null) ? m_SelectedDescription.GetHashCode() : 0);
		hash = hash * 31 + ((m_RecommendedClasses != null) ? m_RecommendedClasses.GetHashCode() : 0);
		if (m_Quest != null && !inspectedDataModels.Contains(m_Quest.GetHashCode()))
		{
			inspectedDataModels.Add(m_Quest.GetHashCode());
			hash = hash * 31 + m_Quest.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_Status;
		hash = num + m_Status.GetHashCode();
		int num2 = hash * 31;
		_ = m_ShouldShowProgressAmount;
		hash = num2 + m_ShouldShowProgressAmount.GetHashCode();
		int num3 = hash * 31;
		_ = m_ID;
		hash = num3 + m_ID.GetHashCode();
		return hash * 31 + ((m_UnlockRequirementsDescription != null) ? m_UnlockRequirementsDescription.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Title;
			return true;
		case 1:
			value = m_SelectedDescription;
			return true;
		case 2:
			value = m_RecommendedClasses;
			return true;
		case 3:
			value = m_Quest;
			return true;
		case 4:
			value = m_Status;
			return true;
		case 5:
			value = m_ShouldShowProgressAmount;
			return true;
		case 6:
			value = m_ID;
			return true;
		case 7:
			value = m_UnlockRequirementsDescription;
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
			SelectedDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			RecommendedClasses = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			Quest = ((value != null) ? ((QuestDataModel)value) : null);
			return true;
		case 4:
			Status = ((value != null) ? ((TavernGuideManager.TavernGuideQuestStatus)value) : TavernGuideManager.TavernGuideQuestStatus.UNKNOWN);
			return true;
		case 5:
			ShouldShowProgressAmount = value != null && (bool)value;
			return true;
		case 6:
			ID = ((value != null) ? ((int)value) : 0);
			return true;
		case 7:
			UnlockRequirementsDescription = ((value != null) ? ((string)value) : null);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
