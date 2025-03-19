using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryVillageTaskCollectionRowDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 683;

	private DataModelList<MercenaryVillageTaskItemDataModel> m_TaskList = new DataModelList<MercenaryVillageTaskItemDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 684,
			PropertyDisplayName = "task_list",
			Type = typeof(DataModelList<MercenaryVillageTaskItemDataModel>)
		}
	};

	public int DataModelId => 683;

	public string DataModelDisplayName => "mercenary_village_task_collection_row";

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

	public MercenaryVillageTaskCollectionRowDataModel()
	{
		RegisterNestedDataModel(m_TaskList);
	}

	public int GetPropertiesHashCode()
	{
		return 17 * 31 + ((m_TaskList != null) ? m_TaskList.GetPropertiesHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 684)
		{
			value = m_TaskList;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 684)
		{
			TaskList = ((value != null) ? ((DataModelList<MercenaryVillageTaskItemDataModel>)value) : null);
			return true;
		}
		return false;
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		if (id == 684)
		{
			info = Properties[0];
			return true;
		}
		info = default(DataModelProperty);
		return false;
	}
}
