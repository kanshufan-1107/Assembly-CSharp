using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ExpansionCollectionStatsDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 881;

	private DataModelList<string> m_ExpansionCollectionString = new DataModelList<string>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "expansion_collection_string",
			Type = typeof(DataModelList<string>)
		}
	};

	public int DataModelId => 881;

	public string DataModelDisplayName => "expansion_collection_stats";

	public DataModelList<string> ExpansionCollectionString
	{
		get
		{
			return m_ExpansionCollectionString;
		}
		set
		{
			if (m_ExpansionCollectionString != value)
			{
				RemoveNestedDataModel(m_ExpansionCollectionString);
				RegisterNestedDataModel(value);
				m_ExpansionCollectionString = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public ExpansionCollectionStatsDataModel()
	{
		RegisterNestedDataModel(m_ExpansionCollectionString);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_ExpansionCollectionString != null && !inspectedDataModels.Contains(m_ExpansionCollectionString.GetHashCode()))
		{
			inspectedDataModels.Add(m_ExpansionCollectionString.GetHashCode());
			return hash * 31 + m_ExpansionCollectionString.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_ExpansionCollectionString;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			ExpansionCollectionString = ((value != null) ? ((DataModelList<string>)value) : null);
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
