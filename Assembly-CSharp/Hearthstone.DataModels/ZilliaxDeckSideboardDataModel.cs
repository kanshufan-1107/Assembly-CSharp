using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ZilliaxDeckSideboardDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 924;

	private int m_FunctionalModuleCardCount;

	private int m_CosmeticModuleCardCount;

	private bool m_IsZilliaxAlreadyCrafted;

	private bool m_ShouldShowZilliaxPreview;

	private int m_FunctionalModuleMaxCount;

	private int m_CosmeticModuleMaxCount;

	private CardDataModel m_ZilliaxPreviewCard;

	private bool m_IsDragging;

	private string m_DraggingSource;

	private bool m_DoesZilliaxMatchStart;

	private bool m_DoesZilliaxMatchASavedVersion;

	private DataModelProperty[] m_properties = new DataModelProperty[11]
	{
		new DataModelProperty
		{
			PropertyId = 925,
			PropertyDisplayName = "functional_module_card_count",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 926,
			PropertyDisplayName = "cosmetic_module_card_count",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 927,
			PropertyDisplayName = "is_zilliax_already_crafted",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 928,
			PropertyDisplayName = "should_show_zilliax_preview",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 929,
			PropertyDisplayName = "functional_module_max_count",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 930,
			PropertyDisplayName = "cosmetic_module_max_count",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 931,
			PropertyDisplayName = "zilliax_preview_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 932,
			PropertyDisplayName = "is_dragging",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 933,
			PropertyDisplayName = "dragging_source",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 935,
			PropertyDisplayName = "does_zilliax_match_start",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 945,
			PropertyDisplayName = "does_zilliax_match_saved_versions",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 924;

	public string DataModelDisplayName => "zilliax_deck_sideboard";

	public int FunctionalModuleCardCount
	{
		get
		{
			return m_FunctionalModuleCardCount;
		}
		set
		{
			if (m_FunctionalModuleCardCount != value)
			{
				m_FunctionalModuleCardCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CosmeticModuleCardCount
	{
		get
		{
			return m_CosmeticModuleCardCount;
		}
		set
		{
			if (m_CosmeticModuleCardCount != value)
			{
				m_CosmeticModuleCardCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsZilliaxAlreadyCrafted
	{
		get
		{
			return m_IsZilliaxAlreadyCrafted;
		}
		set
		{
			if (m_IsZilliaxAlreadyCrafted != value)
			{
				m_IsZilliaxAlreadyCrafted = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShouldShowZilliaxPreview
	{
		get
		{
			return m_ShouldShowZilliaxPreview;
		}
		set
		{
			if (m_ShouldShowZilliaxPreview != value)
			{
				m_ShouldShowZilliaxPreview = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int FunctionalModuleMaxCount
	{
		get
		{
			return m_FunctionalModuleMaxCount;
		}
		set
		{
			if (m_FunctionalModuleMaxCount != value)
			{
				m_FunctionalModuleMaxCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CosmeticModuleMaxCount
	{
		get
		{
			return m_CosmeticModuleMaxCount;
		}
		set
		{
			if (m_CosmeticModuleMaxCount != value)
			{
				m_CosmeticModuleMaxCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel ZilliaxPreviewCard
	{
		get
		{
			return m_ZilliaxPreviewCard;
		}
		set
		{
			if (m_ZilliaxPreviewCard != value)
			{
				RemoveNestedDataModel(m_ZilliaxPreviewCard);
				RegisterNestedDataModel(value);
				m_ZilliaxPreviewCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsDragging
	{
		get
		{
			return m_IsDragging;
		}
		set
		{
			if (m_IsDragging != value)
			{
				m_IsDragging = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DraggingSource
	{
		get
		{
			return m_DraggingSource;
		}
		set
		{
			if (!(m_DraggingSource == value))
			{
				m_DraggingSource = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool DoesZilliaxMatchStart
	{
		get
		{
			return m_DoesZilliaxMatchStart;
		}
		set
		{
			if (m_DoesZilliaxMatchStart != value)
			{
				m_DoesZilliaxMatchStart = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool DoesZilliaxMatchASavedVersion
	{
		get
		{
			return m_DoesZilliaxMatchASavedVersion;
		}
		set
		{
			if (m_DoesZilliaxMatchASavedVersion != value)
			{
				m_DoesZilliaxMatchASavedVersion = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public ZilliaxDeckSideboardDataModel()
	{
		RegisterNestedDataModel(m_ZilliaxPreviewCard);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_FunctionalModuleCardCount;
		int num2 = (num + m_FunctionalModuleCardCount.GetHashCode()) * 31;
		_ = m_CosmeticModuleCardCount;
		int num3 = (num2 + m_CosmeticModuleCardCount.GetHashCode()) * 31;
		_ = m_IsZilliaxAlreadyCrafted;
		int num4 = (num3 + m_IsZilliaxAlreadyCrafted.GetHashCode()) * 31;
		_ = m_ShouldShowZilliaxPreview;
		int num5 = (num4 + m_ShouldShowZilliaxPreview.GetHashCode()) * 31;
		_ = m_FunctionalModuleMaxCount;
		int num6 = (num5 + m_FunctionalModuleMaxCount.GetHashCode()) * 31;
		_ = m_CosmeticModuleMaxCount;
		int num7 = ((num6 + m_CosmeticModuleMaxCount.GetHashCode()) * 31 + ((m_ZilliaxPreviewCard != null) ? m_ZilliaxPreviewCard.GetPropertiesHashCode() : 0)) * 31;
		_ = m_IsDragging;
		int num8 = ((num7 + m_IsDragging.GetHashCode()) * 31 + ((m_DraggingSource != null) ? m_DraggingSource.GetHashCode() : 0)) * 31;
		_ = m_DoesZilliaxMatchStart;
		int num9 = (num8 + m_DoesZilliaxMatchStart.GetHashCode()) * 31;
		_ = m_DoesZilliaxMatchASavedVersion;
		return num9 + m_DoesZilliaxMatchASavedVersion.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 925:
			value = m_FunctionalModuleCardCount;
			return true;
		case 926:
			value = m_CosmeticModuleCardCount;
			return true;
		case 927:
			value = m_IsZilliaxAlreadyCrafted;
			return true;
		case 928:
			value = m_ShouldShowZilliaxPreview;
			return true;
		case 929:
			value = m_FunctionalModuleMaxCount;
			return true;
		case 930:
			value = m_CosmeticModuleMaxCount;
			return true;
		case 931:
			value = m_ZilliaxPreviewCard;
			return true;
		case 932:
			value = m_IsDragging;
			return true;
		case 933:
			value = m_DraggingSource;
			return true;
		case 935:
			value = m_DoesZilliaxMatchStart;
			return true;
		case 945:
			value = m_DoesZilliaxMatchASavedVersion;
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
		case 925:
			FunctionalModuleCardCount = ((value != null) ? ((int)value) : 0);
			return true;
		case 926:
			CosmeticModuleCardCount = ((value != null) ? ((int)value) : 0);
			return true;
		case 927:
			IsZilliaxAlreadyCrafted = value != null && (bool)value;
			return true;
		case 928:
			ShouldShowZilliaxPreview = value != null && (bool)value;
			return true;
		case 929:
			FunctionalModuleMaxCount = ((value != null) ? ((int)value) : 0);
			return true;
		case 930:
			CosmeticModuleMaxCount = ((value != null) ? ((int)value) : 0);
			return true;
		case 931:
			ZilliaxPreviewCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 932:
			IsDragging = value != null && (bool)value;
			return true;
		case 933:
			DraggingSource = ((value != null) ? ((string)value) : null);
			return true;
		case 935:
			DoesZilliaxMatchStart = value != null && (bool)value;
			return true;
		case 945:
			DoesZilliaxMatchASavedVersion = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 925:
			info = Properties[0];
			return true;
		case 926:
			info = Properties[1];
			return true;
		case 927:
			info = Properties[2];
			return true;
		case 928:
			info = Properties[3];
			return true;
		case 929:
			info = Properties[4];
			return true;
		case 930:
			info = Properties[5];
			return true;
		case 931:
			info = Properties[6];
			return true;
		case 932:
			info = Properties[7];
			return true;
		case 933:
			info = Properties[8];
			return true;
		case 935:
			info = Properties[9];
			return true;
		case 945:
			info = Properties[10];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
