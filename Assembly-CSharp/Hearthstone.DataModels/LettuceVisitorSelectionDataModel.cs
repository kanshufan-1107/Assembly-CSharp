using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceVisitorSelectionDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 498;

	private DataModelList<LettuceMercenaryDataModel> m_VisitorOptions = new DataModelList<LettuceMercenaryDataModel>();

	private DataModelList<MercenaryVillageTaskItemDataModel> m_TaskOptions = new DataModelList<MercenaryVillageTaskItemDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "visitor_options",
			Type = typeof(DataModelList<LettuceMercenaryDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "task_options",
			Type = typeof(DataModelList<MercenaryVillageTaskItemDataModel>)
		}
	};

	public int DataModelId => 498;

	public string DataModelDisplayName => "lettuce_visitor_selection";

	public DataModelList<LettuceMercenaryDataModel> VisitorOptions
	{
		get
		{
			return m_VisitorOptions;
		}
		set
		{
			if (m_VisitorOptions != value)
			{
				RemoveNestedDataModel(m_VisitorOptions);
				RegisterNestedDataModel(value);
				m_VisitorOptions = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<MercenaryVillageTaskItemDataModel> TaskOptions
	{
		get
		{
			return m_TaskOptions;
		}
		set
		{
			if (m_TaskOptions != value)
			{
				RemoveNestedDataModel(m_TaskOptions);
				RegisterNestedDataModel(value);
				m_TaskOptions = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceVisitorSelectionDataModel()
	{
		RegisterNestedDataModel(m_VisitorOptions);
		RegisterNestedDataModel(m_TaskOptions);
	}

	public int GetPropertiesHashCode()
	{
		return (17 * 31 + ((m_VisitorOptions != null) ? m_VisitorOptions.GetPropertiesHashCode() : 0)) * 31 + ((m_TaskOptions != null) ? m_TaskOptions.GetPropertiesHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_VisitorOptions;
			return true;
		case 1:
			value = m_TaskOptions;
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
			VisitorOptions = ((value != null) ? ((DataModelList<LettuceMercenaryDataModel>)value) : null);
			return true;
		case 1:
			TaskOptions = ((value != null) ? ((DataModelList<MercenaryVillageTaskItemDataModel>)value) : null);
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
