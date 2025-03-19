using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class AchievementSectionDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 226;

	private string m_Name;

	private AchievementListDataModel m_Achievements;

	private int m_ID;

	private DataModelList<AchievementDataModel> m_DisplayedAchievements = new DataModelList<AchievementDataModel>();

	private float m_DisplayDelay;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "achievements",
			Type = typeof(AchievementListDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "displayed_achievements",
			Type = typeof(DataModelList<AchievementDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "display_delay",
			Type = typeof(float)
		}
	};

	public int DataModelId => 226;

	public string DataModelDisplayName => "achievement_section";

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (!(m_Name == value))
			{
				m_Name = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public AchievementListDataModel Achievements
	{
		get
		{
			return m_Achievements;
		}
		set
		{
			if (m_Achievements != value)
			{
				RemoveNestedDataModel(m_Achievements);
				RegisterNestedDataModel(value);
				m_Achievements = value;
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

	public DataModelList<AchievementDataModel> DisplayedAchievements
	{
		get
		{
			return m_DisplayedAchievements;
		}
		set
		{
			if (m_DisplayedAchievements != value)
			{
				RemoveNestedDataModel(m_DisplayedAchievements);
				RegisterNestedDataModel(value);
				m_DisplayedAchievements = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public float DisplayDelay
	{
		get
		{
			return m_DisplayDelay;
		}
		set
		{
			if (m_DisplayDelay != value)
			{
				m_DisplayDelay = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public AchievementSectionDataModel()
	{
		RegisterNestedDataModel(m_Achievements);
		RegisterNestedDataModel(m_DisplayedAchievements);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0);
		if (m_Achievements != null && !inspectedDataModels.Contains(m_Achievements.GetHashCode()))
		{
			inspectedDataModels.Add(m_Achievements.GetHashCode());
			hash = hash * 31 + m_Achievements.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_ID;
		hash = num + m_ID.GetHashCode();
		if (m_DisplayedAchievements != null && !inspectedDataModels.Contains(m_DisplayedAchievements.GetHashCode()))
		{
			inspectedDataModels.Add(m_DisplayedAchievements.GetHashCode());
			hash = hash * 31 + m_DisplayedAchievements.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num2 = hash * 31;
		_ = m_DisplayDelay;
		return num2 + m_DisplayDelay.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Name;
			return true;
		case 1:
			value = m_Achievements;
			return true;
		case 3:
			value = m_ID;
			return true;
		case 4:
			value = m_DisplayedAchievements;
			return true;
		case 5:
			value = m_DisplayDelay;
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
			Name = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			Achievements = ((value != null) ? ((AchievementListDataModel)value) : null);
			return true;
		case 3:
			ID = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			DisplayedAchievements = ((value != null) ? ((DataModelList<AchievementDataModel>)value) : null);
			return true;
		case 5:
			DisplayDelay = ((value != null) ? ((float)value) : 0f);
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
		case 3:
			info = Properties[2];
			return true;
		case 4:
			info = Properties[3];
			return true;
		case 5:
			info = Properties[4];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
