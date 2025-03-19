using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class BattlegroundsEmoteLoadoutDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 645;

	private DataModelList<BattlegroundsEmoteDataModel> m_EmoteList = new DataModelList<BattlegroundsEmoteDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "emote_list",
			Type = typeof(DataModelList<BattlegroundsEmoteDataModel>)
		}
	};

	public int DataModelId => 645;

	public string DataModelDisplayName => "battlegrounds_emote_loadout";

	public DataModelList<BattlegroundsEmoteDataModel> EmoteList
	{
		get
		{
			return m_EmoteList;
		}
		set
		{
			if (m_EmoteList != value)
			{
				RemoveNestedDataModel(m_EmoteList);
				RegisterNestedDataModel(value);
				m_EmoteList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public BattlegroundsEmoteLoadoutDataModel()
	{
		RegisterNestedDataModel(m_EmoteList);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_EmoteList != null && !inspectedDataModels.Contains(m_EmoteList.GetHashCode()))
		{
			inspectedDataModels.Add(m_EmoteList.GetHashCode());
			return hash * 31 + m_EmoteList.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_EmoteList;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			EmoteList = ((value != null) ? ((DataModelList<BattlegroundsEmoteDataModel>)value) : null);
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
