using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceExperienceTwoScoopDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 282;

	private DataModelList<LettuceMercenaryExpRewardDataModel> m_ExpRewards = new DataModelList<LettuceMercenaryExpRewardDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "experience_rewards",
			Type = typeof(DataModelList<LettuceMercenaryExpRewardDataModel>)
		}
	};

	public int DataModelId => 282;

	public string DataModelDisplayName => "lettuce_experience_two_scoop";

	public DataModelList<LettuceMercenaryExpRewardDataModel> ExpRewards
	{
		get
		{
			return m_ExpRewards;
		}
		set
		{
			if (m_ExpRewards != value)
			{
				RemoveNestedDataModel(m_ExpRewards);
				RegisterNestedDataModel(value);
				m_ExpRewards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceExperienceTwoScoopDataModel()
	{
		RegisterNestedDataModel(m_ExpRewards);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_ExpRewards != null && !inspectedDataModels.Contains(m_ExpRewards.GetHashCode()))
		{
			inspectedDataModels.Add(m_ExpRewards.GetHashCode());
			return hash * 31 + m_ExpRewards.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_ExpRewards;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			ExpRewards = ((value != null) ? ((DataModelList<LettuceMercenaryExpRewardDataModel>)value) : null);
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
