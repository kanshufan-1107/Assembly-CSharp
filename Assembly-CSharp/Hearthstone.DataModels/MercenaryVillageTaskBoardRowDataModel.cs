using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryVillageTaskBoardRowDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 314;

	private DataModelList<MercenaryVillageTaskItemDataModel> m_TaskList = new DataModelList<MercenaryVillageTaskItemDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 315,
			PropertyDisplayName = "task_list",
			Type = typeof(DataModelList<MercenaryVillageTaskItemDataModel>)
		}
	};

	public int DataModelId => 314;

	public string DataModelDisplayName => "mercenaryvillagetaskboardrow";

	public DataModelList<MercenaryVillageTaskItemDataModel> TaskList
	{
		get
		{
			return m_TaskList;
		}
		set
		{
			if (m_TaskList != value)
			{
				RemoveNestedDataModel(m_TaskList);
				RegisterNestedDataModel(value);
				m_TaskList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MercenaryVillageTaskBoardRowDataModel()
	{
		RegisterNestedDataModel(m_TaskList);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_TaskList != null && !inspectedDataModels.Contains(m_TaskList.GetHashCode()))
		{
			inspectedDataModels.Add(m_TaskList.GetHashCode());
			return hash * 31 + m_TaskList.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 315)
		{
			value = m_TaskList;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 315)
		{
			TaskList = ((value != null) ? ((DataModelList<MercenaryVillageTaskItemDataModel>)value) : null);
			return true;
		}
		return false;
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		if (id == 315)
		{
			info = Properties[0];
			return true;
		}
		info = default(DataModelProperty);
		return false;
	}
}
