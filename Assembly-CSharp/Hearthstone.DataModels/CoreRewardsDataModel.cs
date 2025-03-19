using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class CoreRewardsDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 815;

	private string m_Class;

	private int m_NextLevel;

	private string m_NextRewardId;

	private CardDataModel m_NextReward;

	private string m_DisplayContext;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "class",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "next_level",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "next_reward_id",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "next_reward",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "display_context",
			Type = typeof(string)
		}
	};

	public int DataModelId => 815;

	public string DataModelDisplayName => "core_rewards";

	public string Class
	{
		get
		{
			return m_Class;
		}
		set
		{
			if (!(m_Class == value))
			{
				m_Class = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NextLevel
	{
		get
		{
			return m_NextLevel;
		}
		set
		{
			if (m_NextLevel != value)
			{
				m_NextLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string NextRewardId
	{
		get
		{
			return m_NextRewardId;
		}
		set
		{
			if (!(m_NextRewardId == value))
			{
				m_NextRewardId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel NextReward
	{
		get
		{
			return m_NextReward;
		}
		set
		{
			if (m_NextReward != value)
			{
				RemoveNestedDataModel(m_NextReward);
				RegisterNestedDataModel(value);
				m_NextReward = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DisplayContext
	{
		get
		{
			return m_DisplayContext;
		}
		set
		{
			if (!(m_DisplayContext == value))
			{
				m_DisplayContext = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public CoreRewardsDataModel()
	{
		RegisterNestedDataModel(m_NextReward);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_Class != null) ? m_Class.GetHashCode() : 0);
		int num = hash * 31;
		_ = m_NextLevel;
		hash = num + m_NextLevel.GetHashCode();
		hash = hash * 31 + ((m_NextRewardId != null) ? m_NextRewardId.GetHashCode() : 0);
		if (m_NextReward != null && !inspectedDataModels.Contains(m_NextReward.GetHashCode()))
		{
			inspectedDataModels.Add(m_NextReward.GetHashCode());
			hash = hash * 31 + m_NextReward.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		return hash * 31 + ((m_DisplayContext != null) ? m_DisplayContext.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Class;
			return true;
		case 1:
			value = m_NextLevel;
			return true;
		case 2:
			value = m_NextRewardId;
			return true;
		case 3:
			value = m_NextReward;
			return true;
		case 4:
			value = m_DisplayContext;
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
			Class = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			NextLevel = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			NextRewardId = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			NextReward = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 4:
			DisplayContext = ((value != null) ? ((string)value) : null);
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
