using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceTrainingHallPopupDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 580;

	private DataModelList<LettuceMercenaryDataModel> m_MercenaryList = new DataModelList<LettuceMercenaryDataModel>();

	private int m_TrainingHallLevel;

	private bool m_IsPlayerDragging;

	private LettuceTrainingHallSlotDataModel m_Slot1;

	private LettuceTrainingHallSlotDataModel m_Slot2;

	private bool m_IsMercOverTrainingWindow;

	private string m_ErrorText;

	private int m_MaxTrainingHours;

	private bool m_IsPopupVisible;

	private DataModelProperty[] m_properties = new DataModelProperty[9]
	{
		new DataModelProperty
		{
			PropertyId = 581,
			PropertyDisplayName = "mercenary_list",
			Type = typeof(DataModelList<LettuceMercenaryDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 582,
			PropertyDisplayName = "training_hall_level",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 584,
			PropertyDisplayName = "is_player_dragging",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 592,
			PropertyDisplayName = "slot_1",
			Type = typeof(LettuceTrainingHallSlotDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 593,
			PropertyDisplayName = "slot_2",
			Type = typeof(LettuceTrainingHallSlotDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 594,
			PropertyDisplayName = "is_merc_over_training_window",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 603,
			PropertyDisplayName = "error_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 607,
			PropertyDisplayName = "max_training_hours",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 615,
			PropertyDisplayName = "is_popup_visible",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 580;

	public string DataModelDisplayName => "lettuce_training_hall_popup";

	public DataModelList<LettuceMercenaryDataModel> MercenaryList
	{
		get
		{
			return m_MercenaryList;
		}
		set
		{
			if (m_MercenaryList != value)
			{
				RemoveNestedDataModel(m_MercenaryList);
				RegisterNestedDataModel(value);
				m_MercenaryList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TrainingHallLevel
	{
		get
		{
			return m_TrainingHallLevel;
		}
		set
		{
			if (m_TrainingHallLevel != value)
			{
				m_TrainingHallLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsPlayerDragging
	{
		get
		{
			return m_IsPlayerDragging;
		}
		set
		{
			if (m_IsPlayerDragging != value)
			{
				m_IsPlayerDragging = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceTrainingHallSlotDataModel Slot1
	{
		get
		{
			return m_Slot1;
		}
		set
		{
			if (m_Slot1 != value)
			{
				RemoveNestedDataModel(m_Slot1);
				RegisterNestedDataModel(value);
				m_Slot1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceTrainingHallSlotDataModel Slot2
	{
		get
		{
			return m_Slot2;
		}
		set
		{
			if (m_Slot2 != value)
			{
				RemoveNestedDataModel(m_Slot2);
				RegisterNestedDataModel(value);
				m_Slot2 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsMercOverTrainingWindow
	{
		get
		{
			return m_IsMercOverTrainingWindow;
		}
		set
		{
			if (m_IsMercOverTrainingWindow != value)
			{
				m_IsMercOverTrainingWindow = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ErrorText
	{
		get
		{
			return m_ErrorText;
		}
		set
		{
			if (!(m_ErrorText == value))
			{
				m_ErrorText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MaxTrainingHours
	{
		get
		{
			return m_MaxTrainingHours;
		}
		set
		{
			if (m_MaxTrainingHours != value)
			{
				m_MaxTrainingHours = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsPopupVisible
	{
		get
		{
			return m_IsPopupVisible;
		}
		set
		{
			if (m_IsPopupVisible != value)
			{
				m_IsPopupVisible = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceTrainingHallPopupDataModel()
	{
		RegisterNestedDataModel(m_MercenaryList);
		RegisterNestedDataModel(m_Slot1);
		RegisterNestedDataModel(m_Slot2);
	}

	public int GetPropertiesHashCode()
	{
		int num = (17 * 31 + ((m_MercenaryList != null) ? m_MercenaryList.GetPropertiesHashCode() : 0)) * 31;
		_ = m_TrainingHallLevel;
		int num2 = (num + m_TrainingHallLevel.GetHashCode()) * 31;
		_ = m_IsPlayerDragging;
		int num3 = (((num2 + m_IsPlayerDragging.GetHashCode()) * 31 + ((m_Slot1 != null) ? m_Slot1.GetPropertiesHashCode() : 0)) * 31 + ((m_Slot2 != null) ? m_Slot2.GetPropertiesHashCode() : 0)) * 31;
		_ = m_IsMercOverTrainingWindow;
		int num4 = ((num3 + m_IsMercOverTrainingWindow.GetHashCode()) * 31 + ((m_ErrorText != null) ? m_ErrorText.GetHashCode() : 0)) * 31;
		_ = m_MaxTrainingHours;
		int num5 = (num4 + m_MaxTrainingHours.GetHashCode()) * 31;
		_ = m_IsPopupVisible;
		return num5 + m_IsPopupVisible.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 581:
			value = m_MercenaryList;
			return true;
		case 582:
			value = m_TrainingHallLevel;
			return true;
		case 584:
			value = m_IsPlayerDragging;
			return true;
		case 592:
			value = m_Slot1;
			return true;
		case 593:
			value = m_Slot2;
			return true;
		case 594:
			value = m_IsMercOverTrainingWindow;
			return true;
		case 603:
			value = m_ErrorText;
			return true;
		case 607:
			value = m_MaxTrainingHours;
			return true;
		case 615:
			value = m_IsPopupVisible;
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
		case 581:
			MercenaryList = ((value != null) ? ((DataModelList<LettuceMercenaryDataModel>)value) : null);
			return true;
		case 582:
			TrainingHallLevel = ((value != null) ? ((int)value) : 0);
			return true;
		case 584:
			IsPlayerDragging = value != null && (bool)value;
			return true;
		case 592:
			Slot1 = ((value != null) ? ((LettuceTrainingHallSlotDataModel)value) : null);
			return true;
		case 593:
			Slot2 = ((value != null) ? ((LettuceTrainingHallSlotDataModel)value) : null);
			return true;
		case 594:
			IsMercOverTrainingWindow = value != null && (bool)value;
			return true;
		case 603:
			ErrorText = ((value != null) ? ((string)value) : null);
			return true;
		case 607:
			MaxTrainingHours = ((value != null) ? ((int)value) : 0);
			return true;
		case 615:
			IsPopupVisible = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 581:
			info = Properties[0];
			return true;
		case 582:
			info = Properties[1];
			return true;
		case 584:
			info = Properties[2];
			return true;
		case 592:
			info = Properties[3];
			return true;
		case 593:
			info = Properties[4];
			return true;
		case 594:
			info = Properties[5];
			return true;
		case 603:
			info = Properties[6];
			return true;
		case 607:
			info = Properties[7];
			return true;
		case 615:
			info = Properties[8];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
