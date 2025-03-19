using Assets;
using Hearthstone.UI;
using PegasusLettuce;

namespace Hearthstone.DataModels;

public class MercenaryVillageTaskItemDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 309;

	private string m_Title;

	private string m_Description;

	private string m_MercShoutOut;

	private int m_Progress;

	private int m_ProgressNeeded;

	private MercenariesTaskState.Status m_TaskStatus;

	private LettuceVillageTaskBoard.TaskStyle m_TaskStyle;

	private RewardListDataModel m_RewardList;

	private int m_MercenaryId;

	private CardDataModel m_MercenaryCard;

	private int m_TaskId;

	private string m_ProgressMessage;

	private string m_MercenaryName;

	private MercenaryVisitor.VillageVisitorType m_TaskType;

	private bool m_IsTimedEvent;

	private string m_RemainingEventTime;

	private int m_TaskChainId;

	private int m_TaskChainIndex;

	private int m_TaskChainLength;

	private string m_MercenaryShortName;

	private TAG_ROLE m_MercenaryRole;

	private int m_MercenaryLevel;

	private bool m_IsRenownOffer;

	private DataModelProperty[] m_properties = new DataModelProperty[23]
	{
		new DataModelProperty
		{
			PropertyId = 310,
			PropertyDisplayName = "title",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 311,
			PropertyDisplayName = "description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 320,
			PropertyDisplayName = "merc_shoutout",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 321,
			PropertyDisplayName = "progress",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 322,
			PropertyDisplayName = "progress_needed",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 643,
			PropertyDisplayName = "task_status",
			Type = typeof(MercenariesTaskState.Status)
		},
		new DataModelProperty
		{
			PropertyId = 323,
			PropertyDisplayName = "task_style",
			Type = typeof(LettuceVillageTaskBoard.TaskStyle)
		},
		new DataModelProperty
		{
			PropertyId = 324,
			PropertyDisplayName = "reward_list",
			Type = typeof(RewardListDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 325,
			PropertyDisplayName = "mercenary_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 326,
			PropertyDisplayName = "mercenary_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 351,
			PropertyDisplayName = "task_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 361,
			PropertyDisplayName = "progress_message",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 362,
			PropertyDisplayName = "mercenary_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 479,
			PropertyDisplayName = "task_type",
			Type = typeof(MercenaryVisitor.VillageVisitorType)
		},
		new DataModelProperty
		{
			PropertyId = 611,
			PropertyDisplayName = "is_timed_event",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 613,
			PropertyDisplayName = "remaining_event_time",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 691,
			PropertyDisplayName = "task_chain_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 478,
			PropertyDisplayName = "task_chain_index",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 687,
			PropertyDisplayName = "task_chain_length",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 686,
			PropertyDisplayName = "mercenary_short_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 688,
			PropertyDisplayName = "mercenary_role",
			Type = typeof(TAG_ROLE)
		},
		new DataModelProperty
		{
			PropertyId = 689,
			PropertyDisplayName = "mercenary_level",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 701,
			PropertyDisplayName = "is_renown_offer",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 309;

	public string DataModelDisplayName => "mercenaryvillagetaskitem";

	public string Title
	{
		get
		{
			return m_Title;
		}
		set
		{
			if (!(m_Title == value))
			{
				m_Title = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Description
	{
		get
		{
			return m_Description;
		}
		set
		{
			if (!(m_Description == value))
			{
				m_Description = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string MercShoutOut
	{
		get
		{
			return m_MercShoutOut;
		}
		set
		{
			if (!(m_MercShoutOut == value))
			{
				m_MercShoutOut = value;
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

	public int ProgressNeeded
	{
		get
		{
			return m_ProgressNeeded;
		}
		set
		{
			if (m_ProgressNeeded != value)
			{
				m_ProgressNeeded = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public MercenariesTaskState.Status TaskStatus
	{
		get
		{
			return m_TaskStatus;
		}
		set
		{
			if (m_TaskStatus != value)
			{
				m_TaskStatus = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceVillageTaskBoard.TaskStyle TaskStyle
	{
		get
		{
			return m_TaskStyle;
		}
		set
		{
			if (m_TaskStyle != value)
			{
				m_TaskStyle = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardListDataModel RewardList
	{
		get
		{
			return m_RewardList;
		}
		set
		{
			if (m_RewardList != value)
			{
				RemoveNestedDataModel(m_RewardList);
				RegisterNestedDataModel(value);
				m_RewardList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MercenaryId
	{
		get
		{
			return m_MercenaryId;
		}
		set
		{
			if (m_MercenaryId != value)
			{
				m_MercenaryId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel MercenaryCard
	{
		get
		{
			return m_MercenaryCard;
		}
		set
		{
			if (m_MercenaryCard != value)
			{
				RemoveNestedDataModel(m_MercenaryCard);
				RegisterNestedDataModel(value);
				m_MercenaryCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TaskId
	{
		get
		{
			return m_TaskId;
		}
		set
		{
			if (m_TaskId != value)
			{
				m_TaskId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ProgressMessage
	{
		get
		{
			return m_ProgressMessage;
		}
		set
		{
			if (!(m_ProgressMessage == value))
			{
				m_ProgressMessage = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string MercenaryName
	{
		get
		{
			return m_MercenaryName;
		}
		set
		{
			if (!(m_MercenaryName == value))
			{
				m_MercenaryName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public MercenaryVisitor.VillageVisitorType TaskType
	{
		get
		{
			return m_TaskType;
		}
		set
		{
			if (m_TaskType != value)
			{
				m_TaskType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsTimedEvent
	{
		get
		{
			return m_IsTimedEvent;
		}
		set
		{
			if (m_IsTimedEvent != value)
			{
				m_IsTimedEvent = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string RemainingEventTime
	{
		get
		{
			return m_RemainingEventTime;
		}
		set
		{
			if (!(m_RemainingEventTime == value))
			{
				m_RemainingEventTime = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TaskChainId
	{
		get
		{
			return m_TaskChainId;
		}
		set
		{
			if (m_TaskChainId != value)
			{
				m_TaskChainId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TaskChainIndex
	{
		get
		{
			return m_TaskChainIndex;
		}
		set
		{
			if (m_TaskChainIndex != value)
			{
				m_TaskChainIndex = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int TaskChainLength
	{
		get
		{
			return m_TaskChainLength;
		}
		set
		{
			if (m_TaskChainLength != value)
			{
				m_TaskChainLength = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string MercenaryShortName
	{
		get
		{
			return m_MercenaryShortName;
		}
		set
		{
			if (!(m_MercenaryShortName == value))
			{
				m_MercenaryShortName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TAG_ROLE MercenaryRole
	{
		get
		{
			return m_MercenaryRole;
		}
		set
		{
			if (m_MercenaryRole != value)
			{
				m_MercenaryRole = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int MercenaryLevel
	{
		get
		{
			return m_MercenaryLevel;
		}
		set
		{
			if (m_MercenaryLevel != value)
			{
				m_MercenaryLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsRenownOffer
	{
		get
		{
			return m_IsRenownOffer;
		}
		set
		{
			if (m_IsRenownOffer != value)
			{
				m_IsRenownOffer = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MercenaryVillageTaskItemDataModel()
	{
		RegisterNestedDataModel(m_RewardList);
		RegisterNestedDataModel(m_MercenaryCard);
	}

	public int GetPropertiesHashCode()
	{
		int num = (((17 * 31 + ((m_Title != null) ? m_Title.GetHashCode() : 0)) * 31 + ((m_Description != null) ? m_Description.GetHashCode() : 0)) * 31 + ((m_MercShoutOut != null) ? m_MercShoutOut.GetHashCode() : 0)) * 31;
		_ = m_Progress;
		int num2 = (num + m_Progress.GetHashCode()) * 31;
		_ = m_ProgressNeeded;
		int num3 = (num2 + m_ProgressNeeded.GetHashCode()) * 31;
		_ = m_TaskStatus;
		int num4 = (num3 + m_TaskStatus.GetHashCode()) * 31;
		_ = m_TaskStyle;
		int num5 = ((num4 + m_TaskStyle.GetHashCode()) * 31 + ((m_RewardList != null) ? m_RewardList.GetPropertiesHashCode() : 0)) * 31;
		_ = m_MercenaryId;
		int num6 = ((num5 + m_MercenaryId.GetHashCode()) * 31 + ((m_MercenaryCard != null) ? m_MercenaryCard.GetPropertiesHashCode() : 0)) * 31;
		_ = m_TaskId;
		int num7 = (((num6 + m_TaskId.GetHashCode()) * 31 + ((m_ProgressMessage != null) ? m_ProgressMessage.GetHashCode() : 0)) * 31 + ((m_MercenaryName != null) ? m_MercenaryName.GetHashCode() : 0)) * 31;
		_ = m_TaskType;
		int num8 = (num7 + m_TaskType.GetHashCode()) * 31;
		_ = m_IsTimedEvent;
		int num9 = ((num8 + m_IsTimedEvent.GetHashCode()) * 31 + ((m_RemainingEventTime != null) ? m_RemainingEventTime.GetHashCode() : 0)) * 31;
		_ = m_TaskChainId;
		int num10 = (num9 + m_TaskChainId.GetHashCode()) * 31;
		_ = m_TaskChainIndex;
		int num11 = (num10 + m_TaskChainIndex.GetHashCode()) * 31;
		_ = m_TaskChainLength;
		int num12 = ((num11 + m_TaskChainLength.GetHashCode()) * 31 + ((m_MercenaryShortName != null) ? m_MercenaryShortName.GetHashCode() : 0)) * 31;
		_ = m_MercenaryRole;
		int num13 = (num12 + m_MercenaryRole.GetHashCode()) * 31;
		_ = m_MercenaryLevel;
		int num14 = (num13 + m_MercenaryLevel.GetHashCode()) * 31;
		_ = m_IsRenownOffer;
		return num14 + m_IsRenownOffer.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 310:
			value = m_Title;
			return true;
		case 311:
			value = m_Description;
			return true;
		case 320:
			value = m_MercShoutOut;
			return true;
		case 321:
			value = m_Progress;
			return true;
		case 322:
			value = m_ProgressNeeded;
			return true;
		case 643:
			value = m_TaskStatus;
			return true;
		case 323:
			value = m_TaskStyle;
			return true;
		case 324:
			value = m_RewardList;
			return true;
		case 325:
			value = m_MercenaryId;
			return true;
		case 326:
			value = m_MercenaryCard;
			return true;
		case 351:
			value = m_TaskId;
			return true;
		case 361:
			value = m_ProgressMessage;
			return true;
		case 362:
			value = m_MercenaryName;
			return true;
		case 479:
			value = m_TaskType;
			return true;
		case 611:
			value = m_IsTimedEvent;
			return true;
		case 613:
			value = m_RemainingEventTime;
			return true;
		case 691:
			value = m_TaskChainId;
			return true;
		case 478:
			value = m_TaskChainIndex;
			return true;
		case 687:
			value = m_TaskChainLength;
			return true;
		case 686:
			value = m_MercenaryShortName;
			return true;
		case 688:
			value = m_MercenaryRole;
			return true;
		case 689:
			value = m_MercenaryLevel;
			return true;
		case 701:
			value = m_IsRenownOffer;
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
		case 310:
			Title = ((value != null) ? ((string)value) : null);
			return true;
		case 311:
			Description = ((value != null) ? ((string)value) : null);
			return true;
		case 320:
			MercShoutOut = ((value != null) ? ((string)value) : null);
			return true;
		case 321:
			Progress = ((value != null) ? ((int)value) : 0);
			return true;
		case 322:
			ProgressNeeded = ((value != null) ? ((int)value) : 0);
			return true;
		case 643:
			TaskStatus = ((value != null) ? ((MercenariesTaskState.Status)value) : MercenariesTaskState.Status.INVALID);
			return true;
		case 323:
			TaskStyle = ((value != null) ? ((LettuceVillageTaskBoard.TaskStyle)value) : LettuceVillageTaskBoard.TaskStyle.NORMAL);
			return true;
		case 324:
			RewardList = ((value != null) ? ((RewardListDataModel)value) : null);
			return true;
		case 325:
			MercenaryId = ((value != null) ? ((int)value) : 0);
			return true;
		case 326:
			MercenaryCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 351:
			TaskId = ((value != null) ? ((int)value) : 0);
			return true;
		case 361:
			ProgressMessage = ((value != null) ? ((string)value) : null);
			return true;
		case 362:
			MercenaryName = ((value != null) ? ((string)value) : null);
			return true;
		case 479:
			TaskType = ((value != null) ? ((MercenaryVisitor.VillageVisitorType)value) : MercenaryVisitor.VillageVisitorType.STANDARD);
			return true;
		case 611:
			IsTimedEvent = value != null && (bool)value;
			return true;
		case 613:
			RemainingEventTime = ((value != null) ? ((string)value) : null);
			return true;
		case 691:
			TaskChainId = ((value != null) ? ((int)value) : 0);
			return true;
		case 478:
			TaskChainIndex = ((value != null) ? ((int)value) : 0);
			return true;
		case 687:
			TaskChainLength = ((value != null) ? ((int)value) : 0);
			return true;
		case 686:
			MercenaryShortName = ((value != null) ? ((string)value) : null);
			return true;
		case 688:
			MercenaryRole = ((value != null) ? ((TAG_ROLE)value) : TAG_ROLE.INVALID);
			return true;
		case 689:
			MercenaryLevel = ((value != null) ? ((int)value) : 0);
			return true;
		case 701:
			IsRenownOffer = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 310:
			info = Properties[0];
			return true;
		case 311:
			info = Properties[1];
			return true;
		case 320:
			info = Properties[2];
			return true;
		case 321:
			info = Properties[3];
			return true;
		case 322:
			info = Properties[4];
			return true;
		case 643:
			info = Properties[5];
			return true;
		case 323:
			info = Properties[6];
			return true;
		case 324:
			info = Properties[7];
			return true;
		case 325:
			info = Properties[8];
			return true;
		case 326:
			info = Properties[9];
			return true;
		case 351:
			info = Properties[10];
			return true;
		case 361:
			info = Properties[11];
			return true;
		case 362:
			info = Properties[12];
			return true;
		case 479:
			info = Properties[13];
			return true;
		case 611:
			info = Properties[14];
			return true;
		case 613:
			info = Properties[15];
			return true;
		case 691:
			info = Properties[16];
			return true;
		case 478:
			info = Properties[17];
			return true;
		case 687:
			info = Properties[18];
			return true;
		case 686:
			info = Properties[19];
			return true;
		case 688:
			info = Properties[20];
			return true;
		case 689:
			info = Properties[21];
			return true;
		case 701:
			info = Properties[22];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
