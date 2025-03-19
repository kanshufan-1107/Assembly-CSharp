using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class RewardTrackNodeDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 230;

	private int m_Level;

	private string m_StyleName;

	private RewardTrackNodeRewardsDataModel m_FreeRewards;

	private RewardTrackNodeRewardsDataModel m_PaidRewards;

	private int m_CumulativeXpNeeded;

	private RewardTrackNodeAdditionalRewardsDataModel m_AdditionalRewards;

	private RewardTrackNodeRewardsDataModel m_PaidPremiumRewards;

	private DataModelProperty[] m_properties = new DataModelProperty[7]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "level",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "style_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "free_rewards",
			Type = typeof(RewardTrackNodeRewardsDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "paid_rewards",
			Type = typeof(RewardTrackNodeRewardsDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "cumulative_xp_needed",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "additional_rewards",
			Type = typeof(RewardTrackNodeAdditionalRewardsDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "paid_premium_rewards",
			Type = typeof(RewardTrackNodeRewardsDataModel)
		}
	};

	public int DataModelId => 230;

	public string DataModelDisplayName => "reward_track_node";

	public int Level
	{
		get
		{
			return m_Level;
		}
		set
		{
			if (m_Level != value)
			{
				m_Level = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string StyleName
	{
		get
		{
			return m_StyleName;
		}
		set
		{
			if (!(m_StyleName == value))
			{
				m_StyleName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardTrackNodeRewardsDataModel FreeRewards
	{
		get
		{
			return m_FreeRewards;
		}
		set
		{
			if (m_FreeRewards != value)
			{
				RemoveNestedDataModel(m_FreeRewards);
				RegisterNestedDataModel(value);
				m_FreeRewards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardTrackNodeRewardsDataModel PaidRewards
	{
		get
		{
			return m_PaidRewards;
		}
		set
		{
			if (m_PaidRewards != value)
			{
				RemoveNestedDataModel(m_PaidRewards);
				RegisterNestedDataModel(value);
				m_PaidRewards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CumulativeXpNeeded
	{
		get
		{
			return m_CumulativeXpNeeded;
		}
		set
		{
			if (m_CumulativeXpNeeded != value)
			{
				m_CumulativeXpNeeded = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardTrackNodeAdditionalRewardsDataModel AdditionalRewards
	{
		get
		{
			return m_AdditionalRewards;
		}
		set
		{
			if (m_AdditionalRewards != value)
			{
				RemoveNestedDataModel(m_AdditionalRewards);
				RegisterNestedDataModel(value);
				m_AdditionalRewards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardTrackNodeRewardsDataModel PaidPremiumRewards
	{
		get
		{
			return m_PaidPremiumRewards;
		}
		set
		{
			if (m_PaidPremiumRewards != value)
			{
				RemoveNestedDataModel(m_PaidPremiumRewards);
				RegisterNestedDataModel(value);
				m_PaidPremiumRewards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public RewardTrackNodeDataModel()
	{
		RegisterNestedDataModel(m_FreeRewards);
		RegisterNestedDataModel(m_PaidRewards);
		RegisterNestedDataModel(m_AdditionalRewards);
		RegisterNestedDataModel(m_PaidPremiumRewards);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_Level;
		hash = num + m_Level.GetHashCode();
		hash = hash * 31 + ((m_StyleName != null) ? m_StyleName.GetHashCode() : 0);
		if (m_FreeRewards != null && !inspectedDataModels.Contains(m_FreeRewards.GetHashCode()))
		{
			inspectedDataModels.Add(m_FreeRewards.GetHashCode());
			hash = hash * 31 + m_FreeRewards.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_PaidRewards != null && !inspectedDataModels.Contains(m_PaidRewards.GetHashCode()))
		{
			inspectedDataModels.Add(m_PaidRewards.GetHashCode());
			hash = hash * 31 + m_PaidRewards.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num2 = hash * 31;
		_ = m_CumulativeXpNeeded;
		hash = num2 + m_CumulativeXpNeeded.GetHashCode();
		if (m_AdditionalRewards != null && !inspectedDataModels.Contains(m_AdditionalRewards.GetHashCode()))
		{
			inspectedDataModels.Add(m_AdditionalRewards.GetHashCode());
			hash = hash * 31 + m_AdditionalRewards.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_PaidPremiumRewards != null && !inspectedDataModels.Contains(m_PaidPremiumRewards.GetHashCode()))
		{
			inspectedDataModels.Add(m_PaidPremiumRewards.GetHashCode());
			return hash * 31 + m_PaidPremiumRewards.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Level;
			return true;
		case 1:
			value = m_StyleName;
			return true;
		case 6:
			value = m_FreeRewards;
			return true;
		case 7:
			value = m_PaidRewards;
			return true;
		case 8:
			value = m_CumulativeXpNeeded;
			return true;
		case 9:
			value = m_AdditionalRewards;
			return true;
		case 10:
			value = m_PaidPremiumRewards;
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
			Level = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			StyleName = ((value != null) ? ((string)value) : null);
			return true;
		case 6:
			FreeRewards = ((value != null) ? ((RewardTrackNodeRewardsDataModel)value) : null);
			return true;
		case 7:
			PaidRewards = ((value != null) ? ((RewardTrackNodeRewardsDataModel)value) : null);
			return true;
		case 8:
			CumulativeXpNeeded = ((value != null) ? ((int)value) : 0);
			return true;
		case 9:
			AdditionalRewards = ((value != null) ? ((RewardTrackNodeAdditionalRewardsDataModel)value) : null);
			return true;
		case 10:
			PaidPremiumRewards = ((value != null) ? ((RewardTrackNodeRewardsDataModel)value) : null);
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
		case 6:
			info = Properties[2];
			return true;
		case 7:
			info = Properties[3];
			return true;
		case 8:
			info = Properties[4];
			return true;
		case 9:
			info = Properties[5];
			return true;
		case 10:
			info = Properties[6];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
