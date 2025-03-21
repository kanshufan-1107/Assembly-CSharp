using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceEquipmentCraftingDisplayDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 293;

	private LettuceAbilityDataModel m_Equipment;

	private LettuceMercenaryCoinDataModel m_MercenaryCoin;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "equipment",
			Type = typeof(LettuceAbilityDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "mercenary_coin",
			Type = typeof(LettuceMercenaryCoinDataModel)
		}
	};

	public int DataModelId => 293;

	public string DataModelDisplayName => "lettuce_equipment_crafting_display";

	public LettuceAbilityDataModel Equipment
	{
		get
		{
			return m_Equipment;
		}
		set
		{
			if (m_Equipment != value)
			{
				RemoveNestedDataModel(m_Equipment);
				RegisterNestedDataModel(value);
				m_Equipment = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceMercenaryCoinDataModel MercenaryCoin
	{
		get
		{
			return m_MercenaryCoin;
		}
		set
		{
			if (m_MercenaryCoin != value)
			{
				RemoveNestedDataModel(m_MercenaryCoin);
				RegisterNestedDataModel(value);
				m_MercenaryCoin = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceEquipmentCraftingDisplayDataModel()
	{
		RegisterNestedDataModel(m_Equipment);
		RegisterNestedDataModel(m_MercenaryCoin);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_Equipment != null && !inspectedDataModels.Contains(m_Equipment.GetHashCode()))
		{
			inspectedDataModels.Add(m_Equipment.GetHashCode());
			hash = hash * 31 + m_Equipment.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_MercenaryCoin != null && !inspectedDataModels.Contains(m_MercenaryCoin.GetHashCode()))
		{
			inspectedDataModels.Add(m_MercenaryCoin.GetHashCode());
			return hash * 31 + m_MercenaryCoin.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Equipment;
			return true;
		case 1:
			value = m_MercenaryCoin;
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
			Equipment = ((value != null) ? ((LettuceAbilityDataModel)value) : null);
			return true;
		case 1:
			MercenaryCoin = ((value != null) ? ((LettuceMercenaryCoinDataModel)value) : null);
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
