using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceTrainingHallSlotDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 585;

	private bool m_SlotIsEmpty;

	private bool m_MercIsMaxLevel;

	private bool m_TrainingIsComplete;

	private int m_Progress;

	private int m_MaxExp;

	private LettuceMercenaryDataModel m_Mercenary;

	private bool m_Locked;

	private int m_SlotIndex;

	private int m_TotalTimeInTraining;

	private int m_PreparationTime;

	private string m_PreparationText;

	private bool m_IsNewlyUnlocked;

	private bool m_ShowAnimatedTraining;

	private DataModelProperty[] m_properties = new DataModelProperty[13]
	{
		new DataModelProperty
		{
			PropertyId = 586,
			PropertyDisplayName = "slot_is_empty",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 587,
			PropertyDisplayName = "merc_is_max_level",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 588,
			PropertyDisplayName = "training_is_complete",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 589,
			PropertyDisplayName = "progress",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 590,
			PropertyDisplayName = "max_exp",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 591,
			PropertyDisplayName = "mercenary",
			Type = typeof(LettuceMercenaryDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 595,
			PropertyDisplayName = "locked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 599,
			PropertyDisplayName = "slot_index",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 600,
			PropertyDisplayName = "total_time_in_training",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 601,
			PropertyDisplayName = "preparation_time",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 605,
			PropertyDisplayName = "preparation_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 608,
			PropertyDisplayName = "is_newly_unlocked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 609,
			PropertyDisplayName = "show_animated_training",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 585;

	public string DataModelDisplayName => "lettuce_training_hall_slot";

	public bool SlotIsEmpty
	{
		get
		{
			return m_SlotIsEmpty;
		}
		set
		{
			if (m_SlotIsEmpty != value)
			{
				m_SlotIsEmpty = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool MercIsMaxLevel
	{
		get
		{
			return m_MercIsMaxLevel;
		}
		set
		{
			if (m_MercIsMaxLevel != value)
			{
				m_MercIsMaxLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool TrainingIsComplete
	{
		get
		{
			return m_TrainingIsComplete;
		}
		set
		{
			if (m_TrainingIsComplete != value)
			{
				m_TrainingIsComplete = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Progress
	{
		get
		{
			return m_Progress;
		}
		set
		{
			if (m_Progress != value)
			{
				m_Progress = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MaxExp
	{
		get
		{
			return m_MaxExp;
		}
		set
		{
			if (m_MaxExp != value)
			{
				m_MaxExp = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceMercenaryDataModel Mercenary
	{
		get
		{
			return m_Mercenary;
		}
		set
		{
			if (m_Mercenary != value)
			{
				RemoveNestedDataModel(m_Mercenary);
				RegisterNestedDataModel(value);
				m_Mercenary = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Locked
	{
		get
		{
			return m_Locked;
		}
		set
		{
			if (m_Locked != value)
			{
				m_Locked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int SlotIndex
	{
		get
		{
			return m_SlotIndex;
		}
		set
		{
			if (m_SlotIndex != value)
			{
				m_SlotIndex = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TotalTimeInTraining
	{
		get
		{
			return m_TotalTimeInTraining;
		}
		set
		{
			if (m_TotalTimeInTraining != value)
			{
				m_TotalTimeInTraining = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int PreparationTime
	{
		get
		{
			return m_PreparationTime;
		}
		set
		{
			if (m_PreparationTime != value)
			{
				m_PreparationTime = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string PreparationText
	{
		get
		{
			return m_PreparationText;
		}
		set
		{
			if (!(m_PreparationText == value))
			{
				m_PreparationText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsNewlyUnlocked
	{
		get
		{
			return m_IsNewlyUnlocked;
		}
		set
		{
			if (m_IsNewlyUnlocked != value)
			{
				m_IsNewlyUnlocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowAnimatedTraining
	{
		get
		{
			return m_ShowAnimatedTraining;
		}
		set
		{
			if (m_ShowAnimatedTraining != value)
			{
				m_ShowAnimatedTraining = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceTrainingHallSlotDataModel()
	{
		RegisterNestedDataModel(m_Mercenary);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_SlotIsEmpty;
		hash = num + m_SlotIsEmpty.GetHashCode();
		int num2 = hash * 31;
		_ = m_MercIsMaxLevel;
		hash = num2 + m_MercIsMaxLevel.GetHashCode();
		int num3 = hash * 31;
		_ = m_TrainingIsComplete;
		hash = num3 + m_TrainingIsComplete.GetHashCode();
		int num4 = hash * 31;
		_ = m_Progress;
		hash = num4 + m_Progress.GetHashCode();
		int num5 = hash * 31;
		_ = m_MaxExp;
		hash = num5 + m_MaxExp.GetHashCode();
		if (m_Mercenary != null && !inspectedDataModels.Contains(m_Mercenary.GetHashCode()))
		{
			inspectedDataModels.Add(m_Mercenary.GetHashCode());
			hash = hash * 31 + m_Mercenary.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num6 = hash * 31;
		_ = m_Locked;
		hash = num6 + m_Locked.GetHashCode();
		int num7 = hash * 31;
		_ = m_SlotIndex;
		hash = num7 + m_SlotIndex.GetHashCode();
		int num8 = hash * 31;
		_ = m_TotalTimeInTraining;
		hash = num8 + m_TotalTimeInTraining.GetHashCode();
		int num9 = hash * 31;
		_ = m_PreparationTime;
		hash = num9 + m_PreparationTime.GetHashCode();
		hash = hash * 31 + ((m_PreparationText != null) ? m_PreparationText.GetHashCode() : 0);
		int num10 = hash * 31;
		_ = m_IsNewlyUnlocked;
		hash = num10 + m_IsNewlyUnlocked.GetHashCode();
		int num11 = hash * 31;
		_ = m_ShowAnimatedTraining;
		return num11 + m_ShowAnimatedTraining.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 586:
			value = m_SlotIsEmpty;
			return true;
		case 587:
			value = m_MercIsMaxLevel;
			return true;
		case 588:
			value = m_TrainingIsComplete;
			return true;
		case 589:
			value = m_Progress;
			return true;
		case 590:
			value = m_MaxExp;
			return true;
		case 591:
			value = m_Mercenary;
			return true;
		case 595:
			value = m_Locked;
			return true;
		case 599:
			value = m_SlotIndex;
			return true;
		case 600:
			value = m_TotalTimeInTraining;
			return true;
		case 601:
			value = m_PreparationTime;
			return true;
		case 605:
			value = m_PreparationText;
			return true;
		case 608:
			value = m_IsNewlyUnlocked;
			return true;
		case 609:
			value = m_ShowAnimatedTraining;
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
		case 586:
			SlotIsEmpty = value != null && (bool)value;
			return true;
		case 587:
			MercIsMaxLevel = value != null && (bool)value;
			return true;
		case 588:
			TrainingIsComplete = value != null && (bool)value;
			return true;
		case 589:
			Progress = ((value != null) ? ((int)value) : 0);
			return true;
		case 590:
			MaxExp = ((value != null) ? ((int)value) : 0);
			return true;
		case 591:
			Mercenary = ((value != null) ? ((LettuceMercenaryDataModel)value) : null);
			return true;
		case 595:
			Locked = value != null && (bool)value;
			return true;
		case 599:
			SlotIndex = ((value != null) ? ((int)value) : 0);
			return true;
		case 600:
			TotalTimeInTraining = ((value != null) ? ((int)value) : 0);
			return true;
		case 601:
			PreparationTime = ((value != null) ? ((int)value) : 0);
			return true;
		case 605:
			PreparationText = ((value != null) ? ((string)value) : null);
			return true;
		case 608:
			IsNewlyUnlocked = value != null && (bool)value;
			return true;
		case 609:
			ShowAnimatedTraining = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 586:
			info = Properties[0];
			return true;
		case 587:
			info = Properties[1];
			return true;
		case 588:
			info = Properties[2];
			return true;
		case 589:
			info = Properties[3];
			return true;
		case 590:
			info = Properties[4];
			return true;
		case 591:
			info = Properties[5];
			return true;
		case 595:
			info = Properties[6];
			return true;
		case 599:
			info = Properties[7];
			return true;
		case 600:
			info = Properties[8];
			return true;
		case 601:
			info = Properties[9];
			return true;
		case 605:
			info = Properties[10];
			return true;
		case 608:
			info = Properties[11];
			return true;
		case 609:
			info = Properties[12];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
