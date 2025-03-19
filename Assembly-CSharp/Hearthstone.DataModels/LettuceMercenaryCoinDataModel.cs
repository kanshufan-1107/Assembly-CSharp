using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceMercenaryCoinDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 238;

	private string m_MercenaryName;

	private int m_Quantity;

	private bool m_GlowActive;

	private bool m_IsRandom;

	private int m_MercenaryId;

	private bool m_NameActive;

	private DataModelProperty[] m_properties = new DataModelProperty[6]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "mercenary_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "quantity",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "glow_active",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 517,
			PropertyDisplayName = "is_random",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 571,
			PropertyDisplayName = "mercenary_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 572,
			PropertyDisplayName = "name_active",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 238;

	public string DataModelDisplayName => "lettuce_mercenary_coin";

	public string MercenaryName
	{
		get
		{
			return m_MercenaryName;
		}
		set
		{
			if (!(m_MercenaryName == value))
			{
				m_MercenaryName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Quantity
	{
		get
		{
			return m_Quantity;
		}
		set
		{
			if (m_Quantity != value)
			{
				m_Quantity = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool GlowActive
	{
		get
		{
			return m_GlowActive;
		}
		set
		{
			if (m_GlowActive != value)
			{
				m_GlowActive = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsRandom
	{
		get
		{
			return m_IsRandom;
		}
		set
		{
			if (m_IsRandom != value)
			{
				m_IsRandom = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MercenaryId
	{
		get
		{
			return m_MercenaryId;
		}
		set
		{
			if (m_MercenaryId != value)
			{
				m_MercenaryId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool NameActive
	{
		get
		{
			return m_NameActive;
		}
		set
		{
			if (m_NameActive != value)
			{
				m_NameActive = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = (17 * 31 + ((m_MercenaryName != null) ? m_MercenaryName.GetHashCode() : 0)) * 31;
		_ = m_Quantity;
		int num2 = (num + m_Quantity.GetHashCode()) * 31;
		_ = m_GlowActive;
		int num3 = (num2 + m_GlowActive.GetHashCode()) * 31;
		_ = m_IsRandom;
		int num4 = (num3 + m_IsRandom.GetHashCode()) * 31;
		_ = m_MercenaryId;
		int num5 = (num4 + m_MercenaryId.GetHashCode()) * 31;
		_ = m_NameActive;
		return num5 + m_NameActive.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_MercenaryName;
			return true;
		case 2:
			value = m_Quantity;
			return true;
		case 3:
			value = m_GlowActive;
			return true;
		case 517:
			value = m_IsRandom;
			return true;
		case 571:
			value = m_MercenaryId;
			return true;
		case 572:
			value = m_NameActive;
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
			MercenaryName = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			Quantity = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			GlowActive = value != null && (bool)value;
			return true;
		case 517:
			IsRandom = value != null && (bool)value;
			return true;
		case 571:
			MercenaryId = ((value != null) ? ((int)value) : 0);
			return true;
		case 572:
			NameActive = value != null && (bool)value;
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
		case 3:
			info = Properties[2];
			return true;
		case 517:
			info = Properties[3];
			return true;
		case 571:
			info = Properties[4];
			return true;
		case 572:
			info = Properties[5];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
