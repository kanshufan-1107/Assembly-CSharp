using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LuckyDrawRewardDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 667;

	private LuckyDrawStatus m_Status;

	private RewardListDataModel m_RewardList;

	private LuckyDrawStyle m_Style;

	private bool m_IsOwned;

	private int m_RewardID;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "status",
			Type = typeof(LuckyDrawStatus)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "reward_list",
			Type = typeof(RewardListDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "style",
			Type = typeof(LuckyDrawStyle)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "is_owned",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "reward_id",
			Type = typeof(int)
		}
	};

	public int DataModelId => 667;

	public string DataModelDisplayName => "lucky_draw_reward";

	public LuckyDrawStatus Status
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

	public RewardListDataModel RewardList
	{
		get
		{
			return m_RewardList;
		}
		set
		{
			if (m_RewardList != value)
			{
				RemoveNestedDataModel(m_RewardList);
				RegisterNestedDataModel(value);
				m_RewardList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LuckyDrawStyle Style
	{
		get
		{
			return m_Style;
		}
		set
		{
			if (m_Style != value)
			{
				m_Style = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsOwned
	{
		get
		{
			return m_IsOwned;
		}
		set
		{
			if (m_IsOwned != value)
			{
				m_IsOwned = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int RewardID
	{
		get
		{
			return m_RewardID;
		}
		set
		{
			if (m_RewardID != value)
			{
				m_RewardID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LuckyDrawRewardDataModel()
	{
		RegisterNestedDataModel(m_RewardList);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_Status;
		hash = num + m_Status.GetHashCode();
		if (m_RewardList != null && !inspectedDataModels.Contains(m_RewardList.GetHashCode()))
		{
			inspectedDataModels.Add(m_RewardList.GetHashCode());
			hash = hash * 31 + m_RewardList.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num2 = hash * 31;
		_ = m_Style;
		hash = num2 + m_Style.GetHashCode();
		int num3 = hash * 31;
		_ = m_IsOwned;
		hash = num3 + m_IsOwned.GetHashCode();
		int num4 = hash * 31;
		_ = m_RewardID;
		return num4 + m_RewardID.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Status;
			return true;
		case 1:
			value = m_RewardList;
			return true;
		case 2:
			value = m_Style;
			return true;
		case 3:
			value = m_IsOwned;
			return true;
		case 4:
			value = m_RewardID;
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
			Status = ((value != null) ? ((LuckyDrawStatus)value) : LuckyDrawStatus.AVAILABLE);
			return true;
		case 1:
			RewardList = ((value != null) ? ((RewardListDataModel)value) : null);
			return true;
		case 2:
			Style = ((value != null) ? ((LuckyDrawStyle)value) : LuckyDrawStyle.COMMON);
			return true;
		case 3:
			IsOwned = value != null && (bool)value;
			return true;
		case 4:
			RewardID = ((value != null) ? ((int)value) : 0);
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
