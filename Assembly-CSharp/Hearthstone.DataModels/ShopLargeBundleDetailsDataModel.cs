using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ShopLargeBundleDetailsDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 304;

	private string m_Name;

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "name",
			Type = typeof(string)
		}
	};

	public int DataModelId => 304;

	public string DataModelDisplayName => "shop_large_bundle_details";

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (!(m_Name == value))
			{
				m_Name = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		return 17 * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_Name;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			Name = ((value != null) ? ((string)value) : null);
			return true;
		}
		return false;
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		if (id == 0)
		{
			info = Properties[0];
			return true;
		}
		info = default(DataModelProperty);
		return false;
	}
}
