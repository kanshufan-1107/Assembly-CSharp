using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class DeckSideboardDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 796;

	private string m_OwnerCardId;

	private TAG_PREMIUM m_Premium;

	private int m_MaxCards;

	private int m_CardCount;

	private bool m_HasInvalidCards;

	private DataModelList<TAG_CLASS> m_HeroClasses = new DataModelList<TAG_CLASS>();

	private bool m_EnableEditButton;

	private bool m_HighlightEditButton;

	private string m_ButtonLabelText;

	private int m_MinCards;

	private DataModelProperty[] m_properties = new DataModelProperty[10]
	{
		new DataModelProperty
		{
			PropertyId = 797,
			PropertyDisplayName = "owner_card_id",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 798,
			PropertyDisplayName = "premium",
			Type = typeof(TAG_PREMIUM)
		},
		new DataModelProperty
		{
			PropertyId = 799,
			PropertyDisplayName = "max_cards",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 800,
			PropertyDisplayName = "card_count",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 801,
			PropertyDisplayName = "has_invalid_cards",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 802,
			PropertyDisplayName = "hero_classes",
			Type = typeof(DataModelList<TAG_CLASS>)
		},
		new DataModelProperty
		{
			PropertyId = 803,
			PropertyDisplayName = "enable_edit_button",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 804,
			PropertyDisplayName = "highlight_edit_button",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 923,
			PropertyDisplayName = "button_label_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 924,
			PropertyDisplayName = "min_cards",
			Type = typeof(int)
		}
	};

	public int DataModelId => 796;

	public string DataModelDisplayName => "deck_sideboard";

	public string OwnerCardId
	{
		get
		{
			return m_OwnerCardId;
		}
		set
		{
			if (!(m_OwnerCardId == value))
			{
				m_OwnerCardId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TAG_PREMIUM Premium
	{
		get
		{
			return m_Premium;
		}
		set
		{
			if (m_Premium != value)
			{
				m_Premium = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MaxCards
	{
		get
		{
			return m_MaxCards;
		}
		set
		{
			if (m_MaxCards != value)
			{
				m_MaxCards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CardCount
	{
		get
		{
			return m_CardCount;
		}
		set
		{
			if (m_CardCount != value)
			{
				m_CardCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasInvalidCards
	{
		get
		{
			return m_HasInvalidCards;
		}
		set
		{
			if (m_HasInvalidCards != value)
			{
				m_HasInvalidCards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<TAG_CLASS> HeroClasses
	{
		get
		{
			return m_HeroClasses;
		}
		set
		{
			if (m_HeroClasses != value)
			{
				RemoveNestedDataModel(m_HeroClasses);
				RegisterNestedDataModel(value);
				m_HeroClasses = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool EnableEditButton
	{
		get
		{
			return m_EnableEditButton;
		}
		set
		{
			if (m_EnableEditButton != value)
			{
				m_EnableEditButton = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HighlightEditButton
	{
		get
		{
			return m_HighlightEditButton;
		}
		set
		{
			if (m_HighlightEditButton != value)
			{
				m_HighlightEditButton = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ButtonLabelText
	{
		get
		{
			return m_ButtonLabelText;
		}
		set
		{
			if (!(m_ButtonLabelText == value))
			{
				m_ButtonLabelText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MinCards
	{
		get
		{
			return m_MinCards;
		}
		set
		{
			if (m_MinCards != value)
			{
				m_MinCards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public DeckSideboardDataModel()
	{
		RegisterNestedDataModel(m_HeroClasses);
	}

	public int GetPropertiesHashCode()
	{
		int num = (17 * 31 + ((m_OwnerCardId != null) ? m_OwnerCardId.GetHashCode() : 0)) * 31;
		_ = m_Premium;
		int num2 = (num + m_Premium.GetHashCode()) * 31;
		_ = m_MaxCards;
		int num3 = (num2 + m_MaxCards.GetHashCode()) * 31;
		_ = m_CardCount;
		int num4 = (num3 + m_CardCount.GetHashCode()) * 31;
		_ = m_HasInvalidCards;
		int num5 = ((num4 + m_HasInvalidCards.GetHashCode()) * 31 + ((m_HeroClasses != null) ? m_HeroClasses.GetPropertiesHashCode() : 0)) * 31;
		_ = m_EnableEditButton;
		int num6 = (num5 + m_EnableEditButton.GetHashCode()) * 31;
		_ = m_HighlightEditButton;
		int num7 = ((num6 + m_HighlightEditButton.GetHashCode()) * 31 + ((m_ButtonLabelText != null) ? m_ButtonLabelText.GetHashCode() : 0)) * 31;
		_ = m_MinCards;
		return num7 + m_MinCards.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 797:
			value = m_OwnerCardId;
			return true;
		case 798:
			value = m_Premium;
			return true;
		case 799:
			value = m_MaxCards;
			return true;
		case 800:
			value = m_CardCount;
			return true;
		case 801:
			value = m_HasInvalidCards;
			return true;
		case 802:
			value = m_HeroClasses;
			return true;
		case 803:
			value = m_EnableEditButton;
			return true;
		case 804:
			value = m_HighlightEditButton;
			return true;
		case 923:
			value = m_ButtonLabelText;
			return true;
		case 924:
			value = m_MinCards;
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
		case 797:
			OwnerCardId = ((value != null) ? ((string)value) : null);
			return true;
		case 798:
			Premium = ((value != null) ? ((TAG_PREMIUM)value) : TAG_PREMIUM.NORMAL);
			return true;
		case 799:
			MaxCards = ((value != null) ? ((int)value) : 0);
			return true;
		case 800:
			CardCount = ((value != null) ? ((int)value) : 0);
			return true;
		case 801:
			HasInvalidCards = value != null && (bool)value;
			return true;
		case 802:
			HeroClasses = ((value != null) ? ((DataModelList<TAG_CLASS>)value) : null);
			return true;
		case 803:
			EnableEditButton = value != null && (bool)value;
			return true;
		case 804:
			HighlightEditButton = value != null && (bool)value;
			return true;
		case 923:
			ButtonLabelText = ((value != null) ? ((string)value) : null);
			return true;
		case 924:
			MinCards = ((value != null) ? ((int)value) : 0);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 797:
			info = Properties[0];
			return true;
		case 798:
			info = Properties[1];
			return true;
		case 799:
			info = Properties[2];
			return true;
		case 800:
			info = Properties[3];
			return true;
		case 801:
			info = Properties[4];
			return true;
		case 802:
			info = Properties[5];
			return true;
		case 803:
			info = Properties[6];
			return true;
		case 804:
			info = Properties[7];
			return true;
		case 923:
			info = Properties[8];
			return true;
		case 924:
			info = Properties[9];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
