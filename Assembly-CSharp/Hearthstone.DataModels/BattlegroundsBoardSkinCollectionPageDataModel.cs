using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class BattlegroundsBoardSkinCollectionPageDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 565;

	private DataModelList<BattlegroundsBoardSkinDataModel> m_BoardSkinList = new DataModelList<BattlegroundsBoardSkinDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "board_skin_list",
			Type = typeof(DataModelList<BattlegroundsBoardSkinDataModel>)
		}
	};

	public int DataModelId => 565;

	public string DataModelDisplayName => "battlegrounds_board_skin_collection_page";

	public DataModelList<BattlegroundsBoardSkinDataModel> BoardSkinList
	{
		get
		{
			return m_BoardSkinList;
		}
		set
		{
			if (m_BoardSkinList != value)
			{
				RemoveNestedDataModel(m_BoardSkinList);
				RegisterNestedDataModel(value);
				m_BoardSkinList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public BattlegroundsBoardSkinCollectionPageDataModel()
	{
		RegisterNestedDataModel(m_BoardSkinList);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_BoardSkinList != null && !inspectedDataModels.Contains(m_BoardSkinList.GetHashCode()))
		{
			inspectedDataModels.Add(m_BoardSkinList.GetHashCode());
			return hash * 31 + m_BoardSkinList.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_BoardSkinList;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			BoardSkinList = ((value != null) ? ((DataModelList<BattlegroundsBoardSkinDataModel>)value) : null);
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
