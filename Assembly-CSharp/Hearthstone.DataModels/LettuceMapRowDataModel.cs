using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceMapRowDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 200;

	private DataModelList<LettuceMapCoinDataModel> m_Coins = new DataModelList<LettuceMapCoinDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "coins",
			Type = typeof(DataModelList<LettuceMapCoinDataModel>)
		}
	};

	public int DataModelId => 200;

	public string DataModelDisplayName => "lettuce_map_row";

	public DataModelList<LettuceMapCoinDataModel> Coins
	{
		get
		{
			return m_Coins;
		}
		set
		{
			if (m_Coins != value)
			{
				RemoveNestedDataModel(m_Coins);
				RegisterNestedDataModel(value);
				m_Coins = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceMapRowDataModel()
	{
		RegisterNestedDataModel(m_Coins);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_Coins != null && !inspectedDataModels.Contains(m_Coins.GetHashCode()))
		{
			inspectedDataModels.Add(m_Coins.GetHashCode());
			return hash * 31 + m_Coins.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_Coins;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			Coins = ((value != null) ? ((DataModelList<LettuceMapCoinDataModel>)value) : null);
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
