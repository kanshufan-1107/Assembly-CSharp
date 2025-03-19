using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryVillageTaskBoardDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 307;

	private DataModelList<MercenaryVillageTaskBoardRowDataModel> m_TaskListRow = new DataModelList<MercenaryVillageTaskBoardRowDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 308,
			PropertyDisplayName = "task_list_row",
			Type = typeof(DataModelList<MercenaryVillageTaskBoardRowDataModel>)
		}
	};

	public int DataModelId => 307;

	public string DataModelDisplayName => "mercenaryvillagetaskboard";

	public DataModelList<MercenaryVillageTaskBoardRowDataModel> TaskListRow
	{
		get
		{
			return m_TaskListRow;
		}
		set
		{
			if (m_TaskListRow != value)
			{
				RemoveNestedDataModel(m_TaskListRow);
				RegisterNestedDataModel(value);
				m_TaskListRow = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MercenaryVillageTaskBoardDataModel()
	{
		RegisterNestedDataModel(m_TaskListRow);
	}

	public int GetPropertiesHashCode()
	{
		return 17 * 31 + ((m_TaskListRow != null) ? m_TaskListRow.GetPropertiesHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 308)
		{
			value = m_TaskListRow;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 308)
		{
			TaskListRow = ((value != null) ? ((DataModelList<MercenaryVillageTaskBoardRowDataModel>)value) : null);
			return true;
		}
		return false;
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		if (id == 308)
		{
			info = Properties[0];
			return true;
		}
		info = default(DataModelProperty);
		return false;
	}
}
