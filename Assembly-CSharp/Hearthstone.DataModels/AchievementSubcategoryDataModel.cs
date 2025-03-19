using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class AchievementSubcategoryDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 227;

	private string m_Name;

	private string m_FullName;

	private string m_Icon;

	private AchievementSectionListDataModel m_Sections;

	private AchievementStatsDataModel m_Stats;

	private int m_ID;

	private bool m_IsLocked;

	private DataModelProperty[] m_properties = new DataModelProperty[7]
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
			PropertyDisplayName = "full_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "icon",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "sections",
			Type = typeof(AchievementSectionListDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "stats",
			Type = typeof(AchievementStatsDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "is_locked",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 227;

	public string DataModelDisplayName => "achievement_subcategory";

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

	public string FullName
	{
		get
		{
			return m_FullName;
		}
		set
		{
			if (!(m_FullName == value))
			{
				m_FullName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Icon
	{
		get
		{
			return m_Icon;
		}
		set
		{
			if (!(m_Icon == value))
			{
				m_Icon = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public AchievementSectionListDataModel Sections
	{
		get
		{
			return m_Sections;
		}
		set
		{
			if (m_Sections != value)
			{
				RemoveNestedDataModel(m_Sections);
				RegisterNestedDataModel(value);
				m_Sections = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public AchievementStatsDataModel Stats
	{
		get
		{
			return m_Stats;
		}
		set
		{
			if (m_Stats != value)
			{
				RemoveNestedDataModel(m_Stats);
				RegisterNestedDataModel(value);
				m_Stats = value;
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

	public bool IsLocked
	{
		get
		{
			return m_IsLocked;
		}
		set
		{
			if (m_IsLocked != value)
			{
				m_IsLocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public AchievementSubcategoryDataModel()
	{
		RegisterNestedDataModel(m_Sections);
		RegisterNestedDataModel(m_Stats);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0);
		hash = hash * 31 + ((m_FullName != null) ? m_FullName.GetHashCode() : 0);
		hash = hash * 31 + ((m_Icon != null) ? m_Icon.GetHashCode() : 0);
		if (m_Sections != null && !inspectedDataModels.Contains(m_Sections.GetHashCode()))
		{
			inspectedDataModels.Add(m_Sections.GetHashCode());
			hash = hash * 31 + m_Sections.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_Stats != null && !inspectedDataModels.Contains(m_Stats.GetHashCode()))
		{
			inspectedDataModels.Add(m_Stats.GetHashCode());
			hash = hash * 31 + m_Stats.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_ID;
		hash = num + m_ID.GetHashCode();
		int num2 = hash * 31;
		_ = m_IsLocked;
		return num2 + m_IsLocked.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Name;
			return true;
		case 1:
			value = m_FullName;
			return true;
		case 2:
			value = m_Icon;
			return true;
		case 3:
			value = m_Sections;
			return true;
		case 4:
			value = m_Stats;
			return true;
		case 5:
			value = m_ID;
			return true;
		case 6:
			value = m_IsLocked;
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
			FullName = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			Icon = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			Sections = ((value != null) ? ((AchievementSectionListDataModel)value) : null);
			return true;
		case 4:
			Stats = ((value != null) ? ((AchievementStatsDataModel)value) : null);
			return true;
		case 5:
			ID = ((value != null) ? ((int)value) : 0);
			return true;
		case 6:
			IsLocked = value != null && (bool)value;
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
