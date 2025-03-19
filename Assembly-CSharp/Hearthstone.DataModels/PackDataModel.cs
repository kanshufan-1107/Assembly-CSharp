using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class PackDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 25;

	private BoosterDbId m_Type;

	private int m_Quantity;

	private string m_BoosterName;

	private BoosterDbId m_AlternateBoosterId;

	private bool m_OverrideWatermark;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "type",
			Type = typeof(BoosterDbId)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "quantity",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "booster_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "alternate_booster_id",
			Type = typeof(BoosterDbId)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "override_watermark",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 25;

	public string DataModelDisplayName => "pack";

	public BoosterDbId Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			if (m_Type != value)
			{
				m_Type = value;
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

	public string BoosterName
	{
		get
		{
			return m_BoosterName;
		}
		set
		{
			if (!(m_BoosterName == value))
			{
				m_BoosterName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public BoosterDbId AlternateBoosterId
	{
		get
		{
			return m_AlternateBoosterId;
		}
		set
		{
			if (m_AlternateBoosterId != value)
			{
				m_AlternateBoosterId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool OverrideWatermark
	{
		get
		{
			return m_OverrideWatermark;
		}
		set
		{
			if (m_OverrideWatermark != value)
			{
				m_OverrideWatermark = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_Type;
		int num2 = (num + m_Type.GetHashCode()) * 31;
		_ = m_Quantity;
		int num3 = ((num2 + m_Quantity.GetHashCode()) * 31 + ((m_BoosterName != null) ? m_BoosterName.GetHashCode() : 0)) * 31;
		_ = m_AlternateBoosterId;
		int num4 = (num3 + m_AlternateBoosterId.GetHashCode()) * 31;
		_ = m_OverrideWatermark;
		return num4 + m_OverrideWatermark.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Type;
			return true;
		case 1:
			value = m_Quantity;
			return true;
		case 2:
			value = m_BoosterName;
			return true;
		case 3:
			value = m_AlternateBoosterId;
			return true;
		case 4:
			value = m_OverrideWatermark;
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
			Type = ((value != null) ? ((BoosterDbId)value) : BoosterDbId.INVALID);
			return true;
		case 1:
			Quantity = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			BoosterName = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			AlternateBoosterId = ((value != null) ? ((BoosterDbId)value) : BoosterDbId.INVALID);
			return true;
		case 4:
			OverrideWatermark = value != null && (bool)value;
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
