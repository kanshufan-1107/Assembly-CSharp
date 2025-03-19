using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryVillageTaskCollectionDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 679;

	private DataModelList<MercenaryVillageTaskCollectionRowDataModel> m_TaskRows = new DataModelList<MercenaryVillageTaskCollectionRowDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 698,
			PropertyDisplayName = "task_rows",
			Type = typeof(DataModelList<MercenaryVillageTaskCollectionRowDataModel>)
		}
	};

	public int DataModelId => 679;

	public string DataModelDisplayName => "mercenary_village_task_collection";

	public DataModelList<MercenaryVillageTaskCollectionRowDataModel> TaskRows
	{
		get
		{
			return m_TaskRows;
		}
		set
		{
			if (m_TaskRows != value)
			{
				RemoveNestedDataModel(m_TaskRows);
				RegisterNestedDataModel(value);
				m_TaskRows = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MercenaryVillageTaskCollectionDataModel()
	{
		RegisterNestedDataModel(m_TaskRows);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_TaskRows != null && !inspectedDataModels.Contains(m_TaskRows.GetHashCode()))
		{
			inspectedDataModels.Add(m_TaskRows.GetHashCode());
			return hash * 31 + m_TaskRows.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 698)
		{
			value = m_TaskRows;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 698)
		{
			TaskRows = ((value != null) ? ((DataModelList<MercenaryVillageTaskCollectionRowDataModel>)value) : null);
			return true;
		}
		return false;
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		if (id == 698)
		{
			info = Properties[0];
			return true;
		}
		info = default(DataModelProperty);
		return false;
	}
}
