using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.DataModels;

public class MaterialDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 302;

	private Material m_Material;

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "material",
			Type = typeof(Material)
		}
	};

	public int DataModelId => 302;

	public string DataModelDisplayName => "material";

	public Material Material
	{
		get
		{
			return m_Material;
		}
		set
		{
			if (!(m_Material == value))
			{
				m_Material = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		return 17 * 31 + ((m_Material != null) ? m_Material.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_Material;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			Material = ((value != null) ? ((Material)value) : null);
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
