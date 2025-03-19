using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class CraftingDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 650;

	private bool m_IsGolden;

	private int m_DustTotal;

	private int m_CreateDustCost;

	private int m_UpgradeDustCost;

	private int m_DisenchantDustValue;

	private int m_MaxCopies;

	private int m_NumOwnedNormal;

	private int m_NumOwnedGolden;

	private DataModelProperty[] m_properties = new DataModelProperty[8]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "is_golden",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "dust_total",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "create_dust_cost",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "upgrade_dust_cost",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "disenchant_dust_value",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "max_copies",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "num_owned_normal",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "num_owned_golden",
			Type = typeof(int)
		}
	};

	public int DataModelId => 650;

	public string DataModelDisplayName => "crafting";

	public bool IsGolden
	{
		get
		{
			return m_IsGolden;
		}
		set
		{
			if (m_IsGolden != value)
			{
				m_IsGolden = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int DustTotal
	{
		get
		{
			return m_DustTotal;
		}
		set
		{
			if (m_DustTotal != value)
			{
				m_DustTotal = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CreateDustCost
	{
		get
		{
			return m_CreateDustCost;
		}
		set
		{
			if (m_CreateDustCost != value)
			{
				m_CreateDustCost = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int UpgradeDustCost
	{
		get
		{
			return m_UpgradeDustCost;
		}
		set
		{
			if (m_UpgradeDustCost != value)
			{
				m_UpgradeDustCost = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int DisenchantDustValue
	{
		get
		{
			return m_DisenchantDustValue;
		}
		set
		{
			if (m_DisenchantDustValue != value)
			{
				m_DisenchantDustValue = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MaxCopies
	{
		get
		{
			return m_MaxCopies;
		}
		set
		{
			if (m_MaxCopies != value)
			{
				m_MaxCopies = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NumOwnedNormal
	{
		get
		{
			return m_NumOwnedNormal;
		}
		set
		{
			if (m_NumOwnedNormal != value)
			{
				m_NumOwnedNormal = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NumOwnedGolden
	{
		get
		{
			return m_NumOwnedGolden;
		}
		set
		{
			if (m_NumOwnedGolden != value)
			{
				m_NumOwnedGolden = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int num = 17 * 31;
		_ = m_IsGolden;
		int num2 = (num + m_IsGolden.GetHashCode()) * 31;
		_ = m_DustTotal;
		int num3 = (num2 + m_DustTotal.GetHashCode()) * 31;
		_ = m_CreateDustCost;
		int num4 = (num3 + m_CreateDustCost.GetHashCode()) * 31;
		_ = m_UpgradeDustCost;
		int num5 = (num4 + m_UpgradeDustCost.GetHashCode()) * 31;
		_ = m_DisenchantDustValue;
		int num6 = (num5 + m_DisenchantDustValue.GetHashCode()) * 31;
		_ = m_MaxCopies;
		int num7 = (num6 + m_MaxCopies.GetHashCode()) * 31;
		_ = m_NumOwnedNormal;
		int num8 = (num7 + m_NumOwnedNormal.GetHashCode()) * 31;
		_ = m_NumOwnedGolden;
		return num8 + m_NumOwnedGolden.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_IsGolden;
			return true;
		case 1:
			value = m_DustTotal;
			return true;
		case 2:
			value = m_CreateDustCost;
			return true;
		case 3:
			value = m_UpgradeDustCost;
			return true;
		case 4:
			value = m_DisenchantDustValue;
			return true;
		case 5:
			value = m_MaxCopies;
			return true;
		case 6:
			value = m_NumOwnedNormal;
			return true;
		case 7:
			value = m_NumOwnedGolden;
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
			IsGolden = value != null && (bool)value;
			return true;
		case 1:
			DustTotal = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			CreateDustCost = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			UpgradeDustCost = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			DisenchantDustValue = ((value != null) ? ((int)value) : 0);
			return true;
		case 5:
			MaxCopies = ((value != null) ? ((int)value) : 0);
			return true;
		case 6:
			NumOwnedNormal = ((value != null) ? ((int)value) : 0);
			return true;
		case 7:
			NumOwnedGolden = ((value != null) ? ((int)value) : 0);
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
