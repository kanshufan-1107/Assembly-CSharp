using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceRandomMercenaryDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 485;

	private bool m_RestrictRarity;

	private TAG_RARITY m_Rarity;

	private TAG_PREMIUM m_Premium;

	private int m_Quantity;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "restrict_rarity",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "rarity",
			Type = typeof(TAG_RARITY)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "premium",
			Type = typeof(TAG_PREMIUM)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "quantity",
			Type = typeof(int)
		}
	};

	public int DataModelId => 485;

	public string DataModelDisplayName => "lettuce_random_mercenary";

	public bool RestrictRarity
	{
		get
		{
			return m_RestrictRarity;
		}
		set
		{
			if (m_RestrictRarity != value)
			{
				m_RestrictRarity = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TAG_RARITY Rarity
	{
		get
		{
			return m_Rarity;
		}
		set
		{
			if (m_Rarity != value)
			{
				m_Rarity = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TAG_PREMIUM Premium
	{
		get
		{
			return m_Premium;
		}
		set
		{
			if (m_Premium != value)
			{
				m_Premium = value;
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

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_RestrictRarity;
		int num2 = (num + m_RestrictRarity.GetHashCode()) * 31;
		_ = m_Rarity;
		int num3 = (num2 + m_Rarity.GetHashCode()) * 31;
		_ = m_Premium;
		int num4 = (num3 + m_Premium.GetHashCode()) * 31;
		_ = m_Quantity;
		return num4 + m_Quantity.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_RestrictRarity;
			return true;
		case 1:
			value = m_Rarity;
			return true;
		case 2:
			value = m_Premium;
			return true;
		case 3:
			value = m_Quantity;
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
			RestrictRarity = value != null && (bool)value;
			return true;
		case 1:
			Rarity = ((value != null) ? ((TAG_RARITY)value) : TAG_RARITY.INVALID);
			return true;
		case 2:
			Premium = ((value != null) ? ((TAG_PREMIUM)value) : TAG_PREMIUM.NORMAL);
			return true;
		case 3:
			Quantity = ((value != null) ? ((int)value) : 0);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
