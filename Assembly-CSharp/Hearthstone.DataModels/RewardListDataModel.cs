using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class RewardListDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 34;

	private bool m_ChooseOne;

	private DataModelList<RewardItemDataModel> m_Items = new DataModelList<RewardItemDataModel>();

	private string m_Description;

	private string m_AdditionalDescription;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "choose_one",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 35,
			PropertyDisplayName = "items",
			Type = typeof(DataModelList<RewardItemDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 15,
			PropertyDisplayName = "description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 792,
			PropertyDisplayName = "additional_description",
			Type = typeof(string)
		}
	};

	public int DataModelId => 34;

	public string DataModelDisplayName => "reward_list";

	public bool ChooseOne
	{
		get
		{
			return m_ChooseOne;
		}
		set
		{
			if (m_ChooseOne != value)
			{
				m_ChooseOne = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<RewardItemDataModel> Items
	{
		get
		{
			return m_Items;
		}
		set
		{
			if (m_Items != value)
			{
				RemoveNestedDataModel(m_Items);
				RegisterNestedDataModel(value);
				m_Items = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Description
	{
		get
		{
			return m_Description;
		}
		set
		{
			if (!(m_Description == value))
			{
				m_Description = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string AdditionalDescription
	{
		get
		{
			return m_AdditionalDescription;
		}
		set
		{
			if (!(m_AdditionalDescription == value))
			{
				m_AdditionalDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public RewardListDataModel()
	{
		RegisterNestedDataModel(m_Items);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_ChooseOne;
		hash = num + m_ChooseOne.GetHashCode();
		if (m_Items != null && !inspectedDataModels.Contains(m_Items.GetHashCode()))
		{
			inspectedDataModels.Add(m_Items.GetHashCode());
			hash = hash * 31 + m_Items.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		hash = hash * 31 + ((m_Description != null) ? m_Description.GetHashCode() : 0);
		return hash * 31 + ((m_AdditionalDescription != null) ? m_AdditionalDescription.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 1:
			value = m_ChooseOne;
			return true;
		case 35:
			value = m_Items;
			return true;
		case 15:
			value = m_Description;
			return true;
		case 792:
			value = m_AdditionalDescription;
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
		case 1:
			ChooseOne = value != null && (bool)value;
			return true;
		case 35:
			Items = ((value != null) ? ((DataModelList<RewardItemDataModel>)value) : null);
			return true;
		case 15:
			Description = ((value != null) ? ((string)value) : null);
			return true;
		case 792:
			AdditionalDescription = ((value != null) ? ((string)value) : null);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 1:
			info = Properties[0];
			return true;
		case 35:
			info = Properties[1];
			return true;
		case 15:
			info = Properties[2];
			return true;
		case 792:
			info = Properties[3];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
