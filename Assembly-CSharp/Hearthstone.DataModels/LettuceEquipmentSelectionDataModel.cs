using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceEquipmentSelectionDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 268;

	private DataModelList<CardDataModel> m_EquipmentOptions = new DataModelList<CardDataModel>();

	private DataModelList<LettuceMercenaryDataModel> m_Mercenaries = new DataModelList<LettuceMercenaryDataModel>();

	private DataModelList<CardDataModel> m_MercenaryEquipment = new DataModelList<CardDataModel>();

	private LettuceMercenaryDataModel m_ChoiceMercenary;

	private CardDataModel m_ChoiceMercenaryEquipment;

	private bool m_ChoiceMercenaryHasEquipment;

	private DataModelProperty[] m_properties = new DataModelProperty[6]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "equipment_options",
			Type = typeof(DataModelList<CardDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "mercenaries",
			Type = typeof(DataModelList<LettuceMercenaryDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "mercenary_equipment",
			Type = typeof(DataModelList<CardDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "choice_mercenary",
			Type = typeof(LettuceMercenaryDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "choice_mercenary_equipment",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "choice_mercenary_has_equipment",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 268;

	public string DataModelDisplayName => "lettuce_equipment_selection";

	public DataModelList<CardDataModel> EquipmentOptions
	{
		get
		{
			return m_EquipmentOptions;
		}
		set
		{
			if (m_EquipmentOptions != value)
			{
				RemoveNestedDataModel(m_EquipmentOptions);
				RegisterNestedDataModel(value);
				m_EquipmentOptions = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<LettuceMercenaryDataModel> Mercenaries
	{
		get
		{
			return m_Mercenaries;
		}
		set
		{
			if (m_Mercenaries != value)
			{
				RemoveNestedDataModel(m_Mercenaries);
				RegisterNestedDataModel(value);
				m_Mercenaries = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<CardDataModel> MercenaryEquipment
	{
		get
		{
			return m_MercenaryEquipment;
		}
		set
		{
			if (m_MercenaryEquipment != value)
			{
				RemoveNestedDataModel(m_MercenaryEquipment);
				RegisterNestedDataModel(value);
				m_MercenaryEquipment = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceMercenaryDataModel ChoiceMercenary
	{
		get
		{
			return m_ChoiceMercenary;
		}
		set
		{
			if (m_ChoiceMercenary != value)
			{
				RemoveNestedDataModel(m_ChoiceMercenary);
				RegisterNestedDataModel(value);
				m_ChoiceMercenary = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel ChoiceMercenaryEquipment
	{
		get
		{
			return m_ChoiceMercenaryEquipment;
		}
		set
		{
			if (m_ChoiceMercenaryEquipment != value)
			{
				RemoveNestedDataModel(m_ChoiceMercenaryEquipment);
				RegisterNestedDataModel(value);
				m_ChoiceMercenaryEquipment = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ChoiceMercenaryHasEquipment
	{
		get
		{
			return m_ChoiceMercenaryHasEquipment;
		}
		set
		{
			if (m_ChoiceMercenaryHasEquipment != value)
			{
				m_ChoiceMercenaryHasEquipment = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceEquipmentSelectionDataModel()
	{
		RegisterNestedDataModel(m_EquipmentOptions);
		RegisterNestedDataModel(m_Mercenaries);
		RegisterNestedDataModel(m_MercenaryEquipment);
		RegisterNestedDataModel(m_ChoiceMercenary);
		RegisterNestedDataModel(m_ChoiceMercenaryEquipment);
	}

	public int GetPropertiesHashCode()
	{
		int num = (((((17 * 31 + ((m_EquipmentOptions != null) ? m_EquipmentOptions.GetPropertiesHashCode() : 0)) * 31 + ((m_Mercenaries != null) ? m_Mercenaries.GetPropertiesHashCode() : 0)) * 31 + ((m_MercenaryEquipment != null) ? m_MercenaryEquipment.GetPropertiesHashCode() : 0)) * 31 + ((m_ChoiceMercenary != null) ? m_ChoiceMercenary.GetPropertiesHashCode() : 0)) * 31 + ((m_ChoiceMercenaryEquipment != null) ? m_ChoiceMercenaryEquipment.GetPropertiesHashCode() : 0)) * 31;
		_ = m_ChoiceMercenaryHasEquipment;
		return num + m_ChoiceMercenaryHasEquipment.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_EquipmentOptions;
			return true;
		case 1:
			value = m_Mercenaries;
			return true;
		case 2:
			value = m_MercenaryEquipment;
			return true;
		case 4:
			value = m_ChoiceMercenary;
			return true;
		case 5:
			value = m_ChoiceMercenaryEquipment;
			return true;
		case 6:
			value = m_ChoiceMercenaryHasEquipment;
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
			EquipmentOptions = ((value != null) ? ((DataModelList<CardDataModel>)value) : null);
			return true;
		case 1:
			Mercenaries = ((value != null) ? ((DataModelList<LettuceMercenaryDataModel>)value) : null);
			return true;
		case 2:
			MercenaryEquipment = ((value != null) ? ((DataModelList<CardDataModel>)value) : null);
			return true;
		case 4:
			ChoiceMercenary = ((value != null) ? ((LettuceMercenaryDataModel)value) : null);
			return true;
		case 5:
			ChoiceMercenaryEquipment = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 6:
			ChoiceMercenaryHasEquipment = value != null && (bool)value;
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
		case 2:
			info = Properties[2];
			return true;
		case 4:
			info = Properties[3];
			return true;
		case 5:
			info = Properties[4];
			return true;
		case 6:
			info = Properties[5];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
