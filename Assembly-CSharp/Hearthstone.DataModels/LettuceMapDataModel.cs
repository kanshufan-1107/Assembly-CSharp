using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceMapDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 198;

	private DataModelList<LettuceMapRowDataModel> m_Rows = new DataModelList<LettuceMapRowDataModel>();

	private LettuceMapType m_MapType;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "rows",
			Type = typeof(DataModelList<LettuceMapRowDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 754,
			PropertyDisplayName = "map_type",
			Type = typeof(LettuceMapType)
		}
	};

	public int DataModelId => 198;

	public string DataModelDisplayName => "lettuce_map";

	public DataModelList<LettuceMapRowDataModel> Rows
	{
		get
		{
			return m_Rows;
		}
		set
		{
			if (m_Rows != value)
			{
				RemoveNestedDataModel(m_Rows);
				RegisterNestedDataModel(value);
				m_Rows = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceMapType MapType
	{
		get
		{
			return m_MapType;
		}
		set
		{
			if (m_MapType != value)
			{
				m_MapType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceMapDataModel()
	{
		RegisterNestedDataModel(m_Rows);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_Rows != null && !inspectedDataModels.Contains(m_Rows.GetHashCode()))
		{
			inspectedDataModels.Add(m_Rows.GetHashCode());
			hash = hash * 31 + m_Rows.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_MapType;
		return num + m_MapType.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Rows;
			return true;
		case 754:
			value = m_MapType;
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
			Rows = ((value != null) ? ((DataModelList<LettuceMapRowDataModel>)value) : null);
			return true;
		case 754:
			MapType = ((value != null) ? ((LettuceMapType)value) : LettuceMapType.STANDARD);
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
		case 754:
			info = Properties[1];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
