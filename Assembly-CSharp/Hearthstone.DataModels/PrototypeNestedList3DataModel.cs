using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class PrototypeNestedList3DataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 296;

	private int m_Int;

	private DataModelList<PrototypeDataModel> m_List = new DataModelList<PrototypeDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "int",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "list",
			Type = typeof(DataModelList<PrototypeDataModel>)
		}
	};

	public int DataModelId => 296;

	public string DataModelDisplayName => "prototype_nested_list3";

	public int Int
	{
		get
		{
			return m_Int;
		}
		set
		{
			if (m_Int != value)
			{
				m_Int = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<PrototypeDataModel> List
	{
		get
		{
			return m_List;
		}
		set
		{
			if (m_List != value)
			{
				RemoveNestedDataModel(m_List);
				RegisterNestedDataModel(value);
				m_List = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public PrototypeNestedList3DataModel()
	{
		RegisterNestedDataModel(m_List);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_Int;
		hash = num + m_Int.GetHashCode();
		if (m_List != null && !inspectedDataModels.Contains(m_List.GetHashCode()))
		{
			inspectedDataModels.Add(m_List.GetHashCode());
			return hash * 31 + m_List.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Int;
			return true;
		case 1:
			value = m_List;
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
			Int = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			List = ((value != null) ? ((DataModelList<PrototypeDataModel>)value) : null);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
