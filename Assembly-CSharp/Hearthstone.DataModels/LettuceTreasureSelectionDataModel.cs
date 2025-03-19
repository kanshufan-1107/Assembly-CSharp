using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceTreasureSelectionDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 268;

	private DataModelList<CardDataModel> m_TreasureOptions = new DataModelList<CardDataModel>();

	private DataModelList<LettuceMercenaryDataModel> m_Mercenaries = new DataModelList<LettuceMercenaryDataModel>();

	private DataModelList<CardDataModel> m_MercenaryTreasure = new DataModelList<CardDataModel>();

	private LettuceMercenaryDataModel m_ChoiceMercenary;

	private CardDataModel m_ChoiceMercenaryTreasure;

	private bool m_ChoiceMercenaryHasTreasure;

	private bool m_ChoiceMercenaryHasUpgrade;

	private bool m_TreasureSelected;

	private DataModelProperty[] m_properties = new DataModelProperty[8]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "treasure_options",
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
			PropertyDisplayName = "mercenary_treasure",
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
			PropertyDisplayName = "choice_mercenary_treasure",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "choice_mercenary_has_treasure",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 614,
			PropertyDisplayName = "choice_mercenary_has_upgrade",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 769,
			PropertyDisplayName = "treasure_selected",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 268;

	public string DataModelDisplayName => "lettuce_treasure_selection";

	public DataModelList<CardDataModel> TreasureOptions
	{
		get
		{
			return m_TreasureOptions;
		}
		set
		{
			if (m_TreasureOptions != value)
			{
				RemoveNestedDataModel(m_TreasureOptions);
				RegisterNestedDataModel(value);
				m_TreasureOptions = value;
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

	public DataModelList<CardDataModel> MercenaryTreasure
	{
		get
		{
			return m_MercenaryTreasure;
		}
		set
		{
			if (m_MercenaryTreasure != value)
			{
				RemoveNestedDataModel(m_MercenaryTreasure);
				RegisterNestedDataModel(value);
				m_MercenaryTreasure = value;
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

	public CardDataModel ChoiceMercenaryTreasure
	{
		get
		{
			return m_ChoiceMercenaryTreasure;
		}
		set
		{
			if (m_ChoiceMercenaryTreasure != value)
			{
				RemoveNestedDataModel(m_ChoiceMercenaryTreasure);
				RegisterNestedDataModel(value);
				m_ChoiceMercenaryTreasure = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ChoiceMercenaryHasTreasure
	{
		get
		{
			return m_ChoiceMercenaryHasTreasure;
		}
		set
		{
			if (m_ChoiceMercenaryHasTreasure != value)
			{
				m_ChoiceMercenaryHasTreasure = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ChoiceMercenaryHasUpgrade
	{
		get
		{
			return m_ChoiceMercenaryHasUpgrade;
		}
		set
		{
			if (m_ChoiceMercenaryHasUpgrade != value)
			{
				m_ChoiceMercenaryHasUpgrade = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool TreasureSelected
	{
		get
		{
			return m_TreasureSelected;
		}
		set
		{
			if (m_TreasureSelected != value)
			{
				m_TreasureSelected = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceTreasureSelectionDataModel()
	{
		RegisterNestedDataModel(m_TreasureOptions);
		RegisterNestedDataModel(m_Mercenaries);
		RegisterNestedDataModel(m_MercenaryTreasure);
		RegisterNestedDataModel(m_ChoiceMercenary);
		RegisterNestedDataModel(m_ChoiceMercenaryTreasure);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_TreasureOptions != null && !inspectedDataModels.Contains(m_TreasureOptions.GetHashCode()))
		{
			inspectedDataModels.Add(m_TreasureOptions.GetHashCode());
			hash = hash * 31 + m_TreasureOptions.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_Mercenaries != null && !inspectedDataModels.Contains(m_Mercenaries.GetHashCode()))
		{
			inspectedDataModels.Add(m_Mercenaries.GetHashCode());
			hash = hash * 31 + m_Mercenaries.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_MercenaryTreasure != null && !inspectedDataModels.Contains(m_MercenaryTreasure.GetHashCode()))
		{
			inspectedDataModels.Add(m_MercenaryTreasure.GetHashCode());
			hash = hash * 31 + m_MercenaryTreasure.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_ChoiceMercenary != null && !inspectedDataModels.Contains(m_ChoiceMercenary.GetHashCode()))
		{
			inspectedDataModels.Add(m_ChoiceMercenary.GetHashCode());
			hash = hash * 31 + m_ChoiceMercenary.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_ChoiceMercenaryTreasure != null && !inspectedDataModels.Contains(m_ChoiceMercenaryTreasure.GetHashCode()))
		{
			inspectedDataModels.Add(m_ChoiceMercenaryTreasure.GetHashCode());
			hash = hash * 31 + m_ChoiceMercenaryTreasure.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_ChoiceMercenaryHasTreasure;
		hash = num + m_ChoiceMercenaryHasTreasure.GetHashCode();
		int num2 = hash * 31;
		_ = m_ChoiceMercenaryHasUpgrade;
		hash = num2 + m_ChoiceMercenaryHasUpgrade.GetHashCode();
		int num3 = hash * 31;
		_ = m_TreasureSelected;
		return num3 + m_TreasureSelected.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_TreasureOptions;
			return true;
		case 1:
			value = m_Mercenaries;
			return true;
		case 2:
			value = m_MercenaryTreasure;
			return true;
		case 4:
			value = m_ChoiceMercenary;
			return true;
		case 5:
			value = m_ChoiceMercenaryTreasure;
			return true;
		case 6:
			value = m_ChoiceMercenaryHasTreasure;
			return true;
		case 614:
			value = m_ChoiceMercenaryHasUpgrade;
			return true;
		case 769:
			value = m_TreasureSelected;
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
			TreasureOptions = ((value != null) ? ((DataModelList<CardDataModel>)value) : null);
			return true;
		case 1:
			Mercenaries = ((value != null) ? ((DataModelList<LettuceMercenaryDataModel>)value) : null);
			return true;
		case 2:
			MercenaryTreasure = ((value != null) ? ((DataModelList<CardDataModel>)value) : null);
			return true;
		case 4:
			ChoiceMercenary = ((value != null) ? ((LettuceMercenaryDataModel)value) : null);
			return true;
		case 5:
			ChoiceMercenaryTreasure = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 6:
			ChoiceMercenaryHasTreasure = value != null && (bool)value;
			return true;
		case 614:
			ChoiceMercenaryHasUpgrade = value != null && (bool)value;
			return true;
		case 769:
			TreasureSelected = value != null && (bool)value;
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
		case 614:
			info = Properties[6];
			return true;
		case 769:
			info = Properties[7];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
