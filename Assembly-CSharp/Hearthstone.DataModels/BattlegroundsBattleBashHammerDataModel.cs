using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class BattlegroundsBattleBashHammerDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 706;

	private int m_NumHammers;

	private DataModelProperty[] m_properties = new DataModelProperty[1]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "num_hammers",
			Type = typeof(int)
		}
	};

	public int DataModelId => 706;

	public string DataModelDisplayName => "battlegrounds_battle_bash_hammer";

	public int NumHammers
	{
		get
		{
			return m_NumHammers;
		}
		set
		{
			if (m_NumHammers != value)
			{
				m_NumHammers = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int num = 17 * 31;
		_ = m_NumHammers;
		return num + m_NumHammers.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		if (id == 0)
		{
			value = m_NumHammers;
			return true;
		}
		value = null;
		return false;
	}

	public bool SetPropertyValue(int id, object value)
	{
		if (id == 0)
		{
			NumHammers = ((value != null) ? ((int)value) : 0);
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
