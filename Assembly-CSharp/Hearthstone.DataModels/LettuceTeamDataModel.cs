using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceTeamDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 217;

	private long m_TeamId;

	private string m_TeamName;

	private DataModelList<LettuceMercenaryDataModel> m_MercenaryList = new DataModelList<LettuceMercenaryDataModel>();

	private bool m_Valid;

	private bool m_IsDisabled;

	private int m_MythicLevel;

	private DataModelProperty[] m_properties = new DataModelProperty[6]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "team_id",
			Type = typeof(long)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "team_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "mercenary_list",
			Type = typeof(DataModelList<LettuceMercenaryDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "valid",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "is_disabled",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 788,
			PropertyDisplayName = "mythic_level",
			Type = typeof(int)
		}
	};

	public int DataModelId => 217;

	public string DataModelDisplayName => "lettuce_team";

	public long TeamId
	{
		get
		{
			return m_TeamId;
		}
		set
		{
			if (m_TeamId != value)
			{
				m_TeamId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string TeamName
	{
		get
		{
			return m_TeamName;
		}
		set
		{
			if (!(m_TeamName == value))
			{
				m_TeamName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<LettuceMercenaryDataModel> MercenaryList
	{
		get
		{
			return m_MercenaryList;
		}
		set
		{
			if (m_MercenaryList != value)
			{
				RemoveNestedDataModel(m_MercenaryList);
				RegisterNestedDataModel(value);
				m_MercenaryList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Valid
	{
		get
		{
			return m_Valid;
		}
		set
		{
			if (m_Valid != value)
			{
				m_Valid = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsDisabled
	{
		get
		{
			return m_IsDisabled;
		}
		set
		{
			if (m_IsDisabled != value)
			{
				m_IsDisabled = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MythicLevel
	{
		get
		{
			return m_MythicLevel;
		}
		set
		{
			if (m_MythicLevel != value)
			{
				m_MythicLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceTeamDataModel()
	{
		RegisterNestedDataModel(m_MercenaryList);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_TeamId;
		int num2 = (((num + m_TeamId.GetHashCode()) * 31 + ((m_TeamName != null) ? m_TeamName.GetHashCode() : 0)) * 31 + ((m_MercenaryList != null) ? m_MercenaryList.GetPropertiesHashCode() : 0)) * 31;
		_ = m_Valid;
		int num3 = (num2 + m_Valid.GetHashCode()) * 31;
		_ = m_IsDisabled;
		int num4 = (num3 + m_IsDisabled.GetHashCode()) * 31;
		_ = m_MythicLevel;
		return num4 + m_MythicLevel.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_TeamId;
			return true;
		case 1:
			value = m_TeamName;
			return true;
		case 3:
			value = m_MercenaryList;
			return true;
		case 4:
			value = m_Valid;
			return true;
		case 5:
			value = m_IsDisabled;
			return true;
		case 788:
			value = m_MythicLevel;
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
			TeamId = ((value != null) ? ((long)value) : 0);
			return true;
		case 1:
			TeamName = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			MercenaryList = ((value != null) ? ((DataModelList<LettuceMercenaryDataModel>)value) : null);
			return true;
		case 4:
			Valid = value != null && (bool)value;
			return true;
		case 5:
			IsDisabled = value != null && (bool)value;
			return true;
		case 788:
			MythicLevel = ((value != null) ? ((int)value) : 0);
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
		case 788:
			info = Properties[5];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
