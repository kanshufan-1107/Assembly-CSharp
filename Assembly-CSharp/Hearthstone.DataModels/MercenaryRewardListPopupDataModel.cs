using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryRewardListPopupDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 508;

	private string m_Title;

	private DataModelList<MercenaryRewardListPopupTierDataModel> m_Tiers = new DataModelList<MercenaryRewardListPopupTierDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "title",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "tiers",
			Type = typeof(DataModelList<MercenaryRewardListPopupTierDataModel>)
		}
	};

	public int DataModelId => 508;

	public string DataModelDisplayName => "mercenary_reward_list_popup";

	public string Title
	{
		get
		{
			return m_Title;
		}
		set
		{
			if (!(m_Title == value))
			{
				m_Title = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<MercenaryRewardListPopupTierDataModel> Tiers
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

	public MercenaryRewardListPopupDataModel()
	{
		RegisterNestedDataModel(m_Tiers);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_Title != null) ? m_Title.GetHashCode() : 0);
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
		case 0:
			value = m_Title;
			return true;
		case 1:
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
		case 0:
			Title = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			Tiers = ((value != null) ? ((DataModelList<MercenaryRewardListPopupTierDataModel>)value) : null);
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
