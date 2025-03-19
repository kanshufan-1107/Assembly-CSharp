using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MercenaryMythicUpgradeDisplayDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 767;

	private MERCENARY_MYTHIC_UPGRADE_TYPE m_UpgradeType;

	private int m_UpgradeId;

	private int m_CurrentMythicLevel;

	private long m_CurrentRenown;

	private CardDataModel m_CurrentCard;

	private DataModelList<MercenaryMythicUpgradeChoiceDataModel> m_UpgradeChoices = new DataModelList<MercenaryMythicUpgradeChoiceDataModel>();

	private int m_CurrentChoice;

	private bool m_IsMinion;

	private string m_MercUpgradeText;

	private DataModelProperty[] m_properties = new DataModelProperty[9]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "upgrade_type",
			Type = typeof(MERCENARY_MYTHIC_UPGRADE_TYPE)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "upgrade_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "current_mythic_level",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "current_renown",
			Type = typeof(long)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "current_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "upgrade_choices",
			Type = typeof(DataModelList<MercenaryMythicUpgradeChoiceDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "current_choice",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "is_minion",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "merc_upgrade_text",
			Type = typeof(string)
		}
	};

	public int DataModelId => 767;

	public string DataModelDisplayName => "mercenary_mythic_upgrade_display";

	public MERCENARY_MYTHIC_UPGRADE_TYPE UpgradeType
	{
		get
		{
			return m_UpgradeType;
		}
		set
		{
			if (m_UpgradeType != value)
			{
				m_UpgradeType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int UpgradeId
	{
		get
		{
			return m_UpgradeId;
		}
		set
		{
			if (m_UpgradeId != value)
			{
				m_UpgradeId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CurrentMythicLevel
	{
		get
		{
			return m_CurrentMythicLevel;
		}
		set
		{
			if (m_CurrentMythicLevel != value)
			{
				m_CurrentMythicLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public long CurrentRenown
	{
		get
		{
			return m_CurrentRenown;
		}
		set
		{
			if (m_CurrentRenown != value)
			{
				m_CurrentRenown = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel CurrentCard
	{
		get
		{
			return m_CurrentCard;
		}
		set
		{
			if (m_CurrentCard != value)
			{
				RemoveNestedDataModel(m_CurrentCard);
				RegisterNestedDataModel(value);
				m_CurrentCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<MercenaryMythicUpgradeChoiceDataModel> UpgradeChoices
	{
		get
		{
			return m_UpgradeChoices;
		}
		set
		{
			if (m_UpgradeChoices != value)
			{
				RemoveNestedDataModel(m_UpgradeChoices);
				RegisterNestedDataModel(value);
				m_UpgradeChoices = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CurrentChoice
	{
		get
		{
			return m_CurrentChoice;
		}
		set
		{
			if (m_CurrentChoice != value)
			{
				m_CurrentChoice = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsMinion
	{
		get
		{
			return m_IsMinion;
		}
		set
		{
			if (m_IsMinion != value)
			{
				m_IsMinion = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string MercUpgradeText
	{
		get
		{
			return m_MercUpgradeText;
		}
		set
		{
			if (!(m_MercUpgradeText == value))
			{
				m_MercUpgradeText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MercenaryMythicUpgradeDisplayDataModel()
	{
		RegisterNestedDataModel(m_CurrentCard);
		RegisterNestedDataModel(m_UpgradeChoices);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_UpgradeType;
		int num2 = (num + m_UpgradeType.GetHashCode()) * 31;
		_ = m_UpgradeId;
		int num3 = (num2 + m_UpgradeId.GetHashCode()) * 31;
		_ = m_CurrentMythicLevel;
		int num4 = (num3 + m_CurrentMythicLevel.GetHashCode()) * 31;
		_ = m_CurrentRenown;
		int num5 = (((num4 + m_CurrentRenown.GetHashCode()) * 31 + ((m_CurrentCard != null) ? m_CurrentCard.GetPropertiesHashCode() : 0)) * 31 + ((m_UpgradeChoices != null) ? m_UpgradeChoices.GetPropertiesHashCode() : 0)) * 31;
		_ = m_CurrentChoice;
		int num6 = (num5 + m_CurrentChoice.GetHashCode()) * 31;
		_ = m_IsMinion;
		return (num6 + m_IsMinion.GetHashCode()) * 31 + ((m_MercUpgradeText != null) ? m_MercUpgradeText.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_UpgradeType;
			return true;
		case 1:
			value = m_UpgradeId;
			return true;
		case 2:
			value = m_CurrentMythicLevel;
			return true;
		case 3:
			value = m_CurrentRenown;
			return true;
		case 4:
			value = m_CurrentCard;
			return true;
		case 5:
			value = m_UpgradeChoices;
			return true;
		case 6:
			value = m_CurrentChoice;
			return true;
		case 7:
			value = m_IsMinion;
			return true;
		case 8:
			value = m_MercUpgradeText;
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
			UpgradeType = ((value != null) ? ((MERCENARY_MYTHIC_UPGRADE_TYPE)value) : MERCENARY_MYTHIC_UPGRADE_TYPE.INVALID);
			return true;
		case 1:
			UpgradeId = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			CurrentMythicLevel = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			CurrentRenown = ((value != null) ? ((long)value) : 0);
			return true;
		case 4:
			CurrentCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 5:
			UpgradeChoices = ((value != null) ? ((DataModelList<MercenaryMythicUpgradeChoiceDataModel>)value) : null);
			return true;
		case 6:
			CurrentChoice = ((value != null) ? ((int)value) : 0);
			return true;
		case 7:
			IsMinion = value != null && (bool)value;
			return true;
		case 8:
			MercUpgradeText = ((value != null) ? ((string)value) : null);
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
		case 3:
			info = Properties[3];
			return true;
		case 4:
			info = Properties[4];
			return true;
		case 5:
			info = Properties[5];
			return true;
		case 6:
			info = Properties[6];
			return true;
		case 7:
			info = Properties[7];
			return true;
		case 8:
			info = Properties[8];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
