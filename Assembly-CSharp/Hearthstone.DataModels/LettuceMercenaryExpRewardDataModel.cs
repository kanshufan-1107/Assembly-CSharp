using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceMercenaryExpRewardDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 251;

	private LettuceMercenaryDataModel m_Mercenary;

	private string m_ExperienceDeltaText;

	private bool m_LeveledUp;

	private DataModelProperty[] m_properties = new DataModelProperty[3]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "mercenary",
			Type = typeof(LettuceMercenaryDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "experience_delta_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "leveled_up",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 251;

	public string DataModelDisplayName => "lettuce_mercenary_exp_reward";

	public LettuceMercenaryDataModel Mercenary
	{
		get
		{
			return m_Mercenary;
		}
		set
		{
			if (m_Mercenary != value)
			{
				RemoveNestedDataModel(m_Mercenary);
				RegisterNestedDataModel(value);
				m_Mercenary = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ExperienceDeltaText
	{
		get
		{
			return m_ExperienceDeltaText;
		}
		set
		{
			if (!(m_ExperienceDeltaText == value))
			{
				m_ExperienceDeltaText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool LeveledUp
	{
		get
		{
			return m_LeveledUp;
		}
		set
		{
			if (m_LeveledUp != value)
			{
				m_LeveledUp = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceMercenaryExpRewardDataModel()
	{
		RegisterNestedDataModel(m_Mercenary);
	}

	public int GetPropertiesHashCode()
	{
		int num = ((17 * 31 + ((m_Mercenary != null) ? m_Mercenary.GetPropertiesHashCode() : 0)) * 31 + ((m_ExperienceDeltaText != null) ? m_ExperienceDeltaText.GetHashCode() : 0)) * 31;
		_ = m_LeveledUp;
		return num + m_LeveledUp.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Mercenary;
			return true;
		case 2:
			value = m_ExperienceDeltaText;
			return true;
		case 4:
			value = m_LeveledUp;
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
			Mercenary = ((value != null) ? ((LettuceMercenaryDataModel)value) : null);
			return true;
		case 2:
			ExperienceDeltaText = ((value != null) ? ((string)value) : null);
			return true;
		case 4:
			LeveledUp = value != null && (bool)value;
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
		case 2:
			info = Properties[1];
			return true;
		case 4:
			info = Properties[2];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
