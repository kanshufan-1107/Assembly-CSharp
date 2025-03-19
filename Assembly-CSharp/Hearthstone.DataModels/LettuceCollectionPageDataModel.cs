using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceCollectionPageDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 259;

	private DataModelList<LettuceMercenaryDataModel> m_MercenaryList = new DataModelList<LettuceMercenaryDataModel>();

	private bool m_CraftingModeActive;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "mercenary_list",
			Type = typeof(DataModelList<LettuceMercenaryDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "crafting_mode_active",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 259;

	public string DataModelDisplayName => "lettuce_collection_page";

	public DataModelList<LettuceMercenaryDataModel> MercenaryList
	{
		get
		{
			return m_MercenaryList;
		}
		set
		{
			if (m_MercenaryList != value)
			{
				RemoveNestedDataModel(m_MercenaryList);
				RegisterNestedDataModel(value);
				m_MercenaryList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool CraftingModeActive
	{
		get
		{
			return m_CraftingModeActive;
		}
		set
		{
			if (m_CraftingModeActive != value)
			{
				m_CraftingModeActive = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceCollectionPageDataModel()
	{
		RegisterNestedDataModel(m_MercenaryList);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_MercenaryList != null && !inspectedDataModels.Contains(m_MercenaryList.GetHashCode()))
		{
			inspectedDataModels.Add(m_MercenaryList.GetHashCode());
			hash = hash * 31 + m_MercenaryList.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_CraftingModeActive;
		return num + m_CraftingModeActive.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_MercenaryList;
			return true;
		case 1:
			value = m_CraftingModeActive;
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
			MercenaryList = ((value != null) ? ((DataModelList<LettuceMercenaryDataModel>)value) : null);
			return true;
		case 1:
			CraftingModeActive = value != null && (bool)value;
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
