using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LoanerDecksInfoDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 478;

	private string m_RemainingDeckTrialTime;

	private DataModelList<DeckChoiceDataModel> m_DeckChoices = new DataModelList<DeckChoiceDataModel>();

	private int m_DeckChoiceClassId;

	private int m_DeckChoiceTemplateId;

	private string m_DeckChoiceName;

	private string m_DeckChoiceFlavourText;

	private bool m_IsLoanerDeckAvailable;

	private bool m_IsSelectedDeckLoaner;

	private bool m_IsCurrentPageLoaner;

	private string m_CurrentSceneMode;

	private string m_DeckChoiceClassName;

	private bool m_HasSeenLoanerDeckFTUE;

	private DataModelProperty[] m_properties = new DataModelProperty[12]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "remaining_deck_trial_time",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "deck_choices",
			Type = typeof(DataModelList<DeckChoiceDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "deck_choice_class_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "deck_choice_template_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "deck_choice_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "deck_flavour_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "is_loaner_deck_available",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "is_selected_deck_loaner",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "is_current_page_loaner",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "current_scene_mode",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "deck_choice_class_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "has_seen_loaner_deck_ftue",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 478;

	public string DataModelDisplayName => "loaner_decks_info";

	public string RemainingDeckTrialTime
	{
		get
		{
			return m_RemainingDeckTrialTime;
		}
		set
		{
			if (!(m_RemainingDeckTrialTime == value))
			{
				m_RemainingDeckTrialTime = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<DeckChoiceDataModel> DeckChoices
	{
		get
		{
			return m_DeckChoices;
		}
		set
		{
			if (m_DeckChoices != value)
			{
				RemoveNestedDataModel(m_DeckChoices);
				RegisterNestedDataModel(value);
				m_DeckChoices = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int DeckChoiceClassId
	{
		get
		{
			return m_DeckChoiceClassId;
		}
		set
		{
			if (m_DeckChoiceClassId != value)
			{
				m_DeckChoiceClassId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int DeckChoiceTemplateId
	{
		get
		{
			return m_DeckChoiceTemplateId;
		}
		set
		{
			if (m_DeckChoiceTemplateId != value)
			{
				m_DeckChoiceTemplateId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DeckChoiceName
	{
		get
		{
			return m_DeckChoiceName;
		}
		set
		{
			if (!(m_DeckChoiceName == value))
			{
				m_DeckChoiceName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DeckChoiceFlavourText
	{
		get
		{
			return m_DeckChoiceFlavourText;
		}
		set
		{
			if (!(m_DeckChoiceFlavourText == value))
			{
				m_DeckChoiceFlavourText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsLoanerDeckAvailable
	{
		get
		{
			return m_IsLoanerDeckAvailable;
		}
		set
		{
			if (m_IsLoanerDeckAvailable != value)
			{
				m_IsLoanerDeckAvailable = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsSelectedDeckLoaner
	{
		get
		{
			return m_IsSelectedDeckLoaner;
		}
		set
		{
			if (m_IsSelectedDeckLoaner != value)
			{
				m_IsSelectedDeckLoaner = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsCurrentPageLoaner
	{
		get
		{
			return m_IsCurrentPageLoaner;
		}
		set
		{
			if (m_IsCurrentPageLoaner != value)
			{
				m_IsCurrentPageLoaner = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string CurrentSceneMode
	{
		get
		{
			return m_CurrentSceneMode;
		}
		set
		{
			if (!(m_CurrentSceneMode == value))
			{
				m_CurrentSceneMode = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DeckChoiceClassName
	{
		get
		{
			return m_DeckChoiceClassName;
		}
		set
		{
			if (!(m_DeckChoiceClassName == value))
			{
				m_DeckChoiceClassName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasSeenLoanerDeckFTUE
	{
		get
		{
			return m_HasSeenLoanerDeckFTUE;
		}
		set
		{
			if (m_HasSeenLoanerDeckFTUE != value)
			{
				m_HasSeenLoanerDeckFTUE = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LoanerDecksInfoDataModel()
	{
		RegisterNestedDataModel(m_DeckChoices);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_RemainingDeckTrialTime != null) ? m_RemainingDeckTrialTime.GetHashCode() : 0);
		if (m_DeckChoices != null && !inspectedDataModels.Contains(m_DeckChoices.GetHashCode()))
		{
			inspectedDataModels.Add(m_DeckChoices.GetHashCode());
			hash = hash * 31 + m_DeckChoices.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_DeckChoiceClassId;
		hash = num + m_DeckChoiceClassId.GetHashCode();
		int num2 = hash * 31;
		_ = m_DeckChoiceTemplateId;
		hash = num2 + m_DeckChoiceTemplateId.GetHashCode();
		hash = hash * 31 + ((m_DeckChoiceName != null) ? m_DeckChoiceName.GetHashCode() : 0);
		hash = hash * 31 + ((m_DeckChoiceFlavourText != null) ? m_DeckChoiceFlavourText.GetHashCode() : 0);
		int num3 = hash * 31;
		_ = m_IsLoanerDeckAvailable;
		hash = num3 + m_IsLoanerDeckAvailable.GetHashCode();
		int num4 = hash * 31;
		_ = m_IsSelectedDeckLoaner;
		hash = num4 + m_IsSelectedDeckLoaner.GetHashCode();
		int num5 = hash * 31;
		_ = m_IsCurrentPageLoaner;
		hash = num5 + m_IsCurrentPageLoaner.GetHashCode();
		hash = hash * 31 + ((m_CurrentSceneMode != null) ? m_CurrentSceneMode.GetHashCode() : 0);
		hash = hash * 31 + ((m_DeckChoiceClassName != null) ? m_DeckChoiceClassName.GetHashCode() : 0);
		int num6 = hash * 31;
		_ = m_HasSeenLoanerDeckFTUE;
		return num6 + m_HasSeenLoanerDeckFTUE.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_RemainingDeckTrialTime;
			return true;
		case 1:
			value = m_DeckChoices;
			return true;
		case 2:
			value = m_DeckChoiceClassId;
			return true;
		case 3:
			value = m_DeckChoiceTemplateId;
			return true;
		case 4:
			value = m_DeckChoiceName;
			return true;
		case 5:
			value = m_DeckChoiceFlavourText;
			return true;
		case 6:
			value = m_IsLoanerDeckAvailable;
			return true;
		case 7:
			value = m_IsSelectedDeckLoaner;
			return true;
		case 8:
			value = m_IsCurrentPageLoaner;
			return true;
		case 9:
			value = m_CurrentSceneMode;
			return true;
		case 10:
			value = m_DeckChoiceClassName;
			return true;
		case 11:
			value = m_HasSeenLoanerDeckFTUE;
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
			RemainingDeckTrialTime = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			DeckChoices = ((value != null) ? ((DataModelList<DeckChoiceDataModel>)value) : null);
			return true;
		case 2:
			DeckChoiceClassId = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			DeckChoiceTemplateId = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			DeckChoiceName = ((value != null) ? ((string)value) : null);
			return true;
		case 5:
			DeckChoiceFlavourText = ((value != null) ? ((string)value) : null);
			return true;
		case 6:
			IsLoanerDeckAvailable = value != null && (bool)value;
			return true;
		case 7:
			IsSelectedDeckLoaner = value != null && (bool)value;
			return true;
		case 8:
			IsCurrentPageLoaner = value != null && (bool)value;
			return true;
		case 9:
			CurrentSceneMode = ((value != null) ? ((string)value) : null);
			return true;
		case 10:
			DeckChoiceClassName = ((value != null) ? ((string)value) : null);
			return true;
		case 11:
			HasSeenLoanerDeckFTUE = value != null && (bool)value;
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
		case 9:
			info = Properties[9];
			return true;
		case 10:
			info = Properties[10];
			return true;
		case 11:
			info = Properties[11];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
