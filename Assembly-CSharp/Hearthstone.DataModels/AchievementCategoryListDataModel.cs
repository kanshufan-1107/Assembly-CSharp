using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class AchievementCategoryListDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 239;

	private DataModelList<AchievementCategoryDataModel> m_Categories = new DataModelList<AchievementCategoryDataModel>();

	private AchievementStatsDataModel m_Stats;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "categories",
			Type = typeof(DataModelList<AchievementCategoryDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "stats",
			Type = typeof(AchievementStatsDataModel)
		}
	};

	public int DataModelId => 239;

	public string DataModelDisplayName => "achievement_category_list";

	public DataModelList<AchievementCategoryDataModel> Categories
	{
		get
		{
			return m_Categories;
		}
		set
		{
			if (m_Categories != value)
			{
				RemoveNestedDataModel(m_Categories);
				RegisterNestedDataModel(value);
				m_Categories = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public AchievementStatsDataModel Stats
	{
		get
		{
			return m_Stats;
		}
		set
		{
			if (m_Stats != value)
			{
				RemoveNestedDataModel(m_Stats);
				RegisterNestedDataModel(value);
				m_Stats = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public AchievementCategoryListDataModel()
	{
		RegisterNestedDataModel(m_Categories);
		RegisterNestedDataModel(m_Stats);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_Categories != null && !inspectedDataModels.Contains(m_Categories.GetHashCode()))
		{
			inspectedDataModels.Add(m_Categories.GetHashCode());
			hash = hash * 31 + m_Categories.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_Stats != null && !inspectedDataModels.Contains(m_Stats.GetHashCode()))
		{
			inspectedDataModels.Add(m_Stats.GetHashCode());
			return hash * 31 + m_Stats.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Categories;
			return true;
		case 1:
			value = m_Stats;
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
			Categories = ((value != null) ? ((DataModelList<AchievementCategoryDataModel>)value) : null);
			return true;
		case 1:
			Stats = ((value != null) ? ((AchievementStatsDataModel)value) : null);
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
