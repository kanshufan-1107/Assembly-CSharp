using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ProfileGameModeStatListDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 221;

	private DataModelList<ProfileGameModeStatDataModel> m_Items = new DataModelList<ProfileGameModeStatDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "items",
			Type = typeof(DataModelList<ProfileGameModeStatDataModel>)
		}
	};

	public int DataModelId => 221;

	public string DataModelDisplayName => "profile_game_mode_stat_list";

	public DataModelList<ProfileGameModeStatDataModel> Items
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

	public DataModelProperty[] Properties => m_properties;

	public ProfileGameModeStatListDataModel()
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
		if (m_Items != null && !inspectedDataModels.Contains(m_Items.GetHashCode()))
		{
			inspectedDataModels.Add(m_Items.GetHashCode());
			return hash * 31 + m_Items.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_Items;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			Items = ((value != null) ? ((DataModelList<ProfileGameModeStatDataModel>)value) : null);
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
