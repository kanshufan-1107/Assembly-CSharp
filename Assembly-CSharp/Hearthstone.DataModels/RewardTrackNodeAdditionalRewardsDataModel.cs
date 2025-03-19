using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class RewardTrackNodeAdditionalRewardsDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 871;

	private DataModelList<RewardTrackNodeRewardsDataModel> m_RewardLists = new DataModelList<RewardTrackNodeRewardsDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "reward_lists",
			Type = typeof(DataModelList<RewardTrackNodeRewardsDataModel>)
		}
	};

	public int DataModelId => 871;

	public string DataModelDisplayName => "reward_track_node_additional_rewards";

	public DataModelList<RewardTrackNodeRewardsDataModel> RewardLists
	{
		get
		{
			return m_RewardLists;
		}
		set
		{
			if (m_RewardLists != value)
			{
				RemoveNestedDataModel(m_RewardLists);
				RegisterNestedDataModel(value);
				m_RewardLists = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public RewardTrackNodeAdditionalRewardsDataModel()
	{
		RegisterNestedDataModel(m_RewardLists);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_RewardLists != null && !inspectedDataModels.Contains(m_RewardLists.GetHashCode()))
		{
			inspectedDataModels.Add(m_RewardLists.GetHashCode());
			return hash * 31 + m_RewardLists.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_RewardLists;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			RewardLists = ((value != null) ? ((DataModelList<RewardTrackNodeRewardsDataModel>)value) : null);
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
