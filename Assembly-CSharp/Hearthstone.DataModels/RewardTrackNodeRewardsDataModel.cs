using System.Collections.Generic;
using Hearthstone.UI;
using PegasusShared;

namespace Hearthstone.DataModels;

public class RewardTrackNodeRewardsDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 236;

	private int m_Level;

	private string m_Summary;

	private bool m_IsPremium;

	private bool m_IsClaimed;

	private RewardListDataModel m_Items;

	private bool m_IsClaimable;

	private int m_RewardTrackType;

	private int m_RewardTrackId;

	private RewardTrackPaidType m_PaidType;

	private bool m_IsUnrevealedAndLocked;

	private string m_PassLabel;

	private DataModelProperty[] m_properties = new DataModelProperty[11]
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
			PropertyDisplayName = "summary",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "is_premium",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "is_claimed",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "items",
			Type = typeof(RewardListDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "is_claimable",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "reward_track_type",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "reward_track_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "paid_type",
			Type = typeof(RewardTrackPaidType)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "is_unrevealed_and_locked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "pass_label",
			Type = typeof(string)
		}
	};

	public int DataModelId => 236;

	public string DataModelDisplayName => "reward_track_node_rewards";

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

	public string Summary
	{
		get
		{
			return m_Summary;
		}
		set
		{
			if (!(m_Summary == value))
			{
				m_Summary = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsPremium
	{
		get
		{
			return m_IsPremium;
		}
		set
		{
			if (m_IsPremium != value)
			{
				m_IsPremium = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsClaimed
	{
		get
		{
			return m_IsClaimed;
		}
		set
		{
			if (m_IsClaimed != value)
			{
				m_IsClaimed = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardListDataModel Items
	{
		get
		{
			return m_Items;
		}
		set
		{
			if (m_Items != value)
			{
				RemoveNestedDataModel(m_Items);
				RegisterNestedDataModel(value);
				m_Items = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsClaimable
	{
		get
		{
			return m_IsClaimable;
		}
		set
		{
			if (m_IsClaimable != value)
			{
				m_IsClaimable = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int RewardTrackType
	{
		get
		{
			return m_RewardTrackType;
		}
		set
		{
			if (m_RewardTrackType != value)
			{
				m_RewardTrackType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int RewardTrackId
	{
		get
		{
			return m_RewardTrackId;
		}
		set
		{
			if (m_RewardTrackId != value)
			{
				m_RewardTrackId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardTrackPaidType PaidType
	{
		get
		{
			return m_PaidType;
		}
		set
		{
			if (m_PaidType != value)
			{
				m_PaidType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsUnrevealedAndLocked
	{
		get
		{
			return m_IsUnrevealedAndLocked;
		}
		set
		{
			if (m_IsUnrevealedAndLocked != value)
			{
				m_IsUnrevealedAndLocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string PassLabel
	{
		get
		{
			return m_PassLabel;
		}
		set
		{
			if (!(m_PassLabel == value))
			{
				m_PassLabel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public RewardTrackNodeRewardsDataModel()
	{
		RegisterNestedDataModel(m_Items);
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
		hash = hash * 31 + ((m_Summary != null) ? m_Summary.GetHashCode() : 0);
		int num2 = hash * 31;
		_ = m_IsPremium;
		hash = num2 + m_IsPremium.GetHashCode();
		int num3 = hash * 31;
		_ = m_IsClaimed;
		hash = num3 + m_IsClaimed.GetHashCode();
		if (m_Items != null && !inspectedDataModels.Contains(m_Items.GetHashCode()))
		{
			inspectedDataModels.Add(m_Items.GetHashCode());
			hash = hash * 31 + m_Items.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num4 = hash * 31;
		_ = m_IsClaimable;
		hash = num4 + m_IsClaimable.GetHashCode();
		int num5 = hash * 31;
		_ = m_RewardTrackType;
		hash = num5 + m_RewardTrackType.GetHashCode();
		int num6 = hash * 31;
		_ = m_RewardTrackId;
		hash = num6 + m_RewardTrackId.GetHashCode();
		int num7 = hash * 31;
		_ = m_PaidType;
		hash = num7 + m_PaidType.GetHashCode();
		int num8 = hash * 31;
		_ = m_IsUnrevealedAndLocked;
		hash = num8 + m_IsUnrevealedAndLocked.GetHashCode();
		return hash * 31 + ((m_PassLabel != null) ? m_PassLabel.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Level;
			return true;
		case 1:
			value = m_Summary;
			return true;
		case 2:
			value = m_IsPremium;
			return true;
		case 3:
			value = m_IsClaimed;
			return true;
		case 4:
			value = m_Items;
			return true;
		case 5:
			value = m_IsClaimable;
			return true;
		case 6:
			value = m_RewardTrackType;
			return true;
		case 7:
			value = m_RewardTrackId;
			return true;
		case 8:
			value = m_PaidType;
			return true;
		case 9:
			value = m_IsUnrevealedAndLocked;
			return true;
		case 10:
			value = m_PassLabel;
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
			Summary = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			IsPremium = value != null && (bool)value;
			return true;
		case 3:
			IsClaimed = value != null && (bool)value;
			return true;
		case 4:
			Items = ((value != null) ? ((RewardListDataModel)value) : null);
			return true;
		case 5:
			IsClaimable = value != null && (bool)value;
			return true;
		case 6:
			RewardTrackType = ((value != null) ? ((int)value) : 0);
			return true;
		case 7:
			RewardTrackId = ((value != null) ? ((int)value) : 0);
			return true;
		case 8:
			PaidType = ((value != null) ? ((RewardTrackPaidType)value) : ((RewardTrackPaidType)0));
			return true;
		case 9:
			IsUnrevealedAndLocked = value != null && (bool)value;
			return true;
		case 10:
			PassLabel = ((value != null) ? ((string)value) : null);
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
		case 9:
			info = Properties[9];
			return true;
		case 10:
			info = Properties[10];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
