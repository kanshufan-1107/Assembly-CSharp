using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ShopSubPageDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 850;

	private ShopTabDataModel m_Tab;

	private DataModelList<ProductTierDataModel> m_Tiers = new DataModelList<ProductTierDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 851,
			PropertyDisplayName = "tab",
			Type = typeof(ShopTabDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 852,
			PropertyDisplayName = "tiers",
			Type = typeof(DataModelList<ProductTierDataModel>)
		}
	};

	public int DataModelId => 850;

	public string DataModelDisplayName => "shop_sub_tab";

	public ShopTabDataModel Tab
	{
		get
		{
			return m_Tab;
		}
		set
		{
			if (m_Tab != value)
			{
				RemoveNestedDataModel(m_Tab);
				RegisterNestedDataModel(value);
				m_Tab = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<ProductTierDataModel> Tiers
	{
		get
		{
			return m_Tiers;
		}
		set
		{
			if (m_Tiers != value)
			{
				RemoveNestedDataModel(m_Tiers);
				RegisterNestedDataModel(value);
				m_Tiers = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public ShopSubPageDataModel()
	{
		RegisterNestedDataModel(m_Tab);
		RegisterNestedDataModel(m_Tiers);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_Tab != null && !inspectedDataModels.Contains(m_Tab.GetHashCode()))
		{
			inspectedDataModels.Add(m_Tab.GetHashCode());
			hash = hash * 31 + m_Tab.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_Tiers != null && !inspectedDataModels.Contains(m_Tiers.GetHashCode()))
		{
			inspectedDataModels.Add(m_Tiers.GetHashCode());
			return hash * 31 + m_Tiers.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 851:
			value = m_Tab;
			return true;
		case 852:
			value = m_Tiers;
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
		case 851:
			Tab = ((value != null) ? ((ShopTabDataModel)value) : null);
			return true;
		case 852:
			Tiers = ((value != null) ? ((DataModelList<ProductTierDataModel>)value) : null);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 851:
			info = Properties[0];
			return true;
		case 852:
			info = Properties[1];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
