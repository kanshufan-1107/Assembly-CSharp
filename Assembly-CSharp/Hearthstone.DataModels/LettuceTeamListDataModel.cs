using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceTeamListDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 218;

	private DataModelList<LettuceTeamDataModel> m_TeamList = new DataModelList<LettuceTeamDataModel>();

	private int m_AutoSelectedTeamId;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "team_list",
			Type = typeof(DataModelList<LettuceTeamDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "auto_selected_team_id",
			Type = typeof(int)
		}
	};

	public int DataModelId => 218;

	public string DataModelDisplayName => "lettuce_team_list";

	public DataModelList<LettuceTeamDataModel> TeamList
	{
		get
		{
			return m_TeamList;
		}
		set
		{
			if (m_TeamList != value)
			{
				RemoveNestedDataModel(m_TeamList);
				RegisterNestedDataModel(value);
				m_TeamList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int AutoSelectedTeamId
	{
		get
		{
			return m_AutoSelectedTeamId;
		}
		set
		{
			if (m_AutoSelectedTeamId != value)
			{
				m_AutoSelectedTeamId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceTeamListDataModel()
	{
		RegisterNestedDataModel(m_TeamList);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_TeamList != null && !inspectedDataModels.Contains(m_TeamList.GetHashCode()))
		{
			inspectedDataModels.Add(m_TeamList.GetHashCode());
			hash = hash * 31 + m_TeamList.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_AutoSelectedTeamId;
		return num + m_AutoSelectedTeamId.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_TeamList;
			return true;
		case 1:
			value = m_AutoSelectedTeamId;
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
			TeamList = ((value != null) ? ((DataModelList<LettuceTeamDataModel>)value) : null);
			return true;
		case 1:
			AutoSelectedTeamId = ((value != null) ? ((int)value) : 0);
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
