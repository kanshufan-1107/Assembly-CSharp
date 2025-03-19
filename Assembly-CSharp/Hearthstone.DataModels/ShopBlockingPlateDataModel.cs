using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ShopBlockingPlateDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 890;

	private ShopBlockingPlateType m_BlockingType;

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 891,
			PropertyDisplayName = "blocking_type",
			Type = typeof(ShopBlockingPlateType)
		}
	};

	public int DataModelId => 890;

	public string DataModelDisplayName => "shop_blocking_plate";

	public ShopBlockingPlateType BlockingType
	{
		get
		{
			return m_BlockingType;
		}
		set
		{
			if (m_BlockingType != value)
			{
				m_BlockingType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_BlockingType;
		return num + m_BlockingType.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 891)
		{
			value = m_BlockingType;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 891)
		{
			BlockingType = ((value != null) ? ((ShopBlockingPlateType)value) : ShopBlockingPlateType.None);
			return true;
		}
		return false;
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		if (id == 891)
		{
			info = Properties[0];
			return true;
		}
		info = default(DataModelProperty);
		return false;
	}
}
