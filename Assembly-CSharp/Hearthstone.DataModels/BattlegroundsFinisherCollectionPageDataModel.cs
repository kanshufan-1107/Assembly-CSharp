using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class BattlegroundsFinisherCollectionPageDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 568;

	private DataModelList<BattlegroundsFinisherDataModel> m_FinisherList = new DataModelList<BattlegroundsFinisherDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "finisher_list",
			Type = typeof(DataModelList<BattlegroundsFinisherDataModel>)
		}
	};

	public int DataModelId => 568;

	public string DataModelDisplayName => "battlegrounds_finisher_collection_page";

	public DataModelList<BattlegroundsFinisherDataModel> FinisherList
	{
		get
		{
			return m_FinisherList;
		}
		set
		{
			if (m_FinisherList != value)
			{
				RemoveNestedDataModel(m_FinisherList);
				RegisterNestedDataModel(value);
				m_FinisherList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public BattlegroundsFinisherCollectionPageDataModel()
	{
		RegisterNestedDataModel(m_FinisherList);
	}

	public int GetPropertiesHashCode()
	{
		return 17 * 31 + ((m_FinisherList != null) ? m_FinisherList.GetPropertiesHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_FinisherList;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			FinisherList = ((value != null) ? ((DataModelList<BattlegroundsFinisherDataModel>)value) : null);
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
