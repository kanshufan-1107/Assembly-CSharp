using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ShopPageDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 847;

	private ShopTabDataModel m_Tab;

	private DataModelList<ShopSubPageDataModel> m_ShopSubPages = new DataModelList<ShopSubPageDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 848,
			PropertyDisplayName = "tab",
			Type = typeof(ShopTabDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 849,
			PropertyDisplayName = "shop_sub_pages",
			Type = typeof(DataModelList<ShopSubPageDataModel>)
		}
	};

	public int DataModelId => 847;

	public string DataModelDisplayName => "shop_page";

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

	public DataModelList<ShopSubPageDataModel> ShopSubPages
	{
		get
		{
			return m_ShopSubPages;
		}
		set
		{
			if (m_ShopSubPages != value)
			{
				RemoveNestedDataModel(m_ShopSubPages);
				RegisterNestedDataModel(value);
				m_ShopSubPages = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public ShopPageDataModel()
	{
		RegisterNestedDataModel(m_Tab);
		RegisterNestedDataModel(m_ShopSubPages);
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
		if (m_ShopSubPages != null && !inspectedDataModels.Contains(m_ShopSubPages.GetHashCode()))
		{
			inspectedDataModels.Add(m_ShopSubPages.GetHashCode());
			return hash * 31 + m_ShopSubPages.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 848:
			value = m_Tab;
			return true;
		case 849:
			value = m_ShopSubPages;
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
		case 848:
			Tab = ((value != null) ? ((ShopTabDataModel)value) : null);
			return true;
		case 849:
			ShopSubPages = ((value != null) ? ((DataModelList<ShopSubPageDataModel>)value) : null);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 848:
			info = Properties[0];
			return true;
		case 849:
			info = Properties[1];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
