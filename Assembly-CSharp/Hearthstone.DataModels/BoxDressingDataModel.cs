using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class BoxDressingDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 838;

	private string m_Type;

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "type",
			Type = typeof(string)
		}
	};

	public int DataModelId => 838;

	public string DataModelDisplayName => "box_dressing";

	public string Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			if (!(m_Type == value))
			{
				m_Type = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		return 17 * 31 + ((m_Type != null) ? m_Type.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_Type;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			Type = ((value != null) ? ((string)value) : null);
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
