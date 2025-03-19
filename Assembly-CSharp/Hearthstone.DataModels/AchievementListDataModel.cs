using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class AchievementListDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 223;

	private DataModelList<AchievementDataModel> m_Achievements = new DataModelList<AchievementDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "achievements",
			Type = typeof(DataModelList<AchievementDataModel>)
		}
	};

	public int DataModelId => 223;

	public string DataModelDisplayName => "achievement_list";

	public DataModelList<AchievementDataModel> Achievements
	{
		get
		{
			return m_Achievements;
		}
		set
		{
			if (m_Achievements != value)
			{
				RemoveNestedDataModel(m_Achievements);
				RegisterNestedDataModel(value);
				m_Achievements = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public AchievementListDataModel()
	{
		RegisterNestedDataModel(m_Achievements);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_Achievements != null && !inspectedDataModels.Contains(m_Achievements.GetHashCode()))
		{
			inspectedDataModels.Add(m_Achievements.GetHashCode());
			return hash * 31 + m_Achievements.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_Achievements;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			Achievements = ((value != null) ? ((DataModelList<AchievementDataModel>)value) : null);
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
