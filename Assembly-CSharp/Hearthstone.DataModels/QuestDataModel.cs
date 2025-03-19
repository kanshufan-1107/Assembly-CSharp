using System.Collections.Generic;
using Assets;
using Hearthstone.Progression;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class QuestDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 207;

	private string m_Name;

	private int m_Progress;

	private int m_Quota;

	private string m_Description;

	private DataModelList<string> m_Icon = new DataModelList<string>();

	private QuestPool.QuestPoolType m_PoolType;

	private string m_TimeUntilNextQuest;

	private int m_RerollCount;

	private RewardListDataModel m_Rewards;

	private int m_QuestId;

	private int m_RewardTrackXp;

	private string m_ProgressMessage;

	private QuestManager.QuestStatus m_Status;

	private int m_PoolId;

	private bool m_Abandonable;

	private int m_NextInChain;

	private string m_TimeUntilExpiration;

	private QuestManager.QuestDisplayMode m_DisplayMode;

	private Global.RewardTrackType m_RewardTrackType;

	private string m_DeepLink;

	private bool m_IsChainQuest;

	private QuestManager.QuestChangeState m_XPChangeStatus;

	private QuestManager.QuestChangeState m_QuotaChangeStatus;

	private QuestManager.QuestChangeState m_TriggerChangeStatus;

	private string m_ToastDescription;

	private DataModelProperty[] m_properties = new DataModelProperty[25]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "progress",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "quota",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "icon",
			Type = typeof(DataModelList<string>)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "pool",
			Type = typeof(QuestPool.QuestPoolType)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "time_until_next_quest",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "reroll_count",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "rewards",
			Type = typeof(RewardListDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "quest_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "reward_track_xp",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 13,
			PropertyDisplayName = "progress_message",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 14,
			PropertyDisplayName = "status",
			Type = typeof(QuestManager.QuestStatus)
		},
		new DataModelProperty
		{
			PropertyId = 15,
			PropertyDisplayName = "pool_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 16,
			PropertyDisplayName = "abandonable",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 17,
			PropertyDisplayName = "next_in_chain",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 18,
			PropertyDisplayName = "time_until_expiration",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 19,
			PropertyDisplayName = "display_mode",
			Type = typeof(QuestManager.QuestDisplayMode)
		},
		new DataModelProperty
		{
			PropertyId = 20,
			PropertyDisplayName = "reward_track_type",
			Type = typeof(Global.RewardTrackType)
		},
		new DataModelProperty
		{
			PropertyId = 21,
			PropertyDisplayName = "deep_link",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 22,
			PropertyDisplayName = "is_chain_quest",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 1114,
			PropertyDisplayName = "xp_change_status",
			Type = typeof(QuestManager.QuestChangeState)
		},
		new DataModelProperty
		{
			PropertyId = 1115,
			PropertyDisplayName = "quota_change_status",
			Type = typeof(QuestManager.QuestChangeState)
		},
		new DataModelProperty
		{
			PropertyId = 1116,
			PropertyDisplayName = "trigger_change_status",
			Type = typeof(QuestManager.QuestChangeState)
		},
		new DataModelProperty
		{
			PropertyId = 1117,
			PropertyDisplayName = "toast_description",
			Type = typeof(string)
		}
	};

	public int DataModelId => 207;

	public string DataModelDisplayName => "quest";

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (!(m_Name == value))
			{
				m_Name = value;
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

	public int Quota
	{
		get
		{
			return m_Quota;
		}
		set
		{
			if (m_Quota != value)
			{
				m_Quota = value;
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

	public DataModelList<string> Icon
	{
		get
		{
			return m_Icon;
		}
		set
		{
			if (m_Icon != value)
			{
				RemoveNestedDataModel(m_Icon);
				RegisterNestedDataModel(value);
				m_Icon = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public QuestPool.QuestPoolType PoolType
	{
		get
		{
			return m_PoolType;
		}
		set
		{
			if (m_PoolType != value)
			{
				m_PoolType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string TimeUntilNextQuest
	{
		get
		{
			return m_TimeUntilNextQuest;
		}
		set
		{
			if (!(m_TimeUntilNextQuest == value))
			{
				m_TimeUntilNextQuest = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int RerollCount
	{
		get
		{
			return m_RerollCount;
		}
		set
		{
			if (m_RerollCount != value)
			{
				m_RerollCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardListDataModel Rewards
	{
		get
		{
			return m_Rewards;
		}
		set
		{
			if (m_Rewards != value)
			{
				RemoveNestedDataModel(m_Rewards);
				RegisterNestedDataModel(value);
				m_Rewards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int QuestId
	{
		get
		{
			return m_QuestId;
		}
		set
		{
			if (m_QuestId != value)
			{
				m_QuestId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int RewardTrackXp
	{
		get
		{
			return m_RewardTrackXp;
		}
		set
		{
			if (m_RewardTrackXp != value)
			{
				m_RewardTrackXp = value;
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

	public QuestManager.QuestStatus Status
	{
		get
		{
			return m_Status;
		}
		set
		{
			if (m_Status != value)
			{
				m_Status = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int PoolId
	{
		get
		{
			return m_PoolId;
		}
		set
		{
			if (m_PoolId != value)
			{
				m_PoolId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Abandonable
	{
		get
		{
			return m_Abandonable;
		}
		set
		{
			if (m_Abandonable != value)
			{
				m_Abandonable = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NextInChain
	{
		get
		{
			return m_NextInChain;
		}
		set
		{
			if (m_NextInChain != value)
			{
				m_NextInChain = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string TimeUntilExpiration
	{
		get
		{
			return m_TimeUntilExpiration;
		}
		set
		{
			if (!(m_TimeUntilExpiration == value))
			{
				m_TimeUntilExpiration = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public QuestManager.QuestDisplayMode DisplayMode
	{
		get
		{
			return m_DisplayMode;
		}
		set
		{
			if (m_DisplayMode != value)
			{
				m_DisplayMode = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public Global.RewardTrackType RewardTrackType
	{
		get
		{
			return m_RewardTrackType;
		}
		set
		{
			if (m_RewardTrackType != value)
			{
				m_RewardTrackType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DeepLink
	{
		get
		{
			return m_DeepLink;
		}
		set
		{
			if (!(m_DeepLink == value))
			{
				m_DeepLink = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsChainQuest
	{
		get
		{
			return m_IsChainQuest;
		}
		set
		{
			if (m_IsChainQuest != value)
			{
				m_IsChainQuest = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public QuestManager.QuestChangeState XPChangeStatus
	{
		get
		{
			return m_XPChangeStatus;
		}
		set
		{
			if (m_XPChangeStatus != value)
			{
				m_XPChangeStatus = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public QuestManager.QuestChangeState QuotaChangeStatus
	{
		get
		{
			return m_QuotaChangeStatus;
		}
		set
		{
			if (m_QuotaChangeStatus != value)
			{
				m_QuotaChangeStatus = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public QuestManager.QuestChangeState TriggerChangeStatus
	{
		get
		{
			return m_TriggerChangeStatus;
		}
		set
		{
			if (m_TriggerChangeStatus != value)
			{
				m_TriggerChangeStatus = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ToastDescription
	{
		get
		{
			return m_ToastDescription;
		}
		set
		{
			if (!(m_ToastDescription == value))
			{
				m_ToastDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public QuestDataModel()
	{
		RegisterNestedDataModel(m_Icon);
		RegisterNestedDataModel(m_Rewards);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0);
		int num = hash * 31;
		_ = m_Progress;
		hash = num + m_Progress.GetHashCode();
		int num2 = hash * 31;
		_ = m_Quota;
		hash = num2 + m_Quota.GetHashCode();
		hash = hash * 31 + ((m_Description != null) ? m_Description.GetHashCode() : 0);
		if (m_Icon != null && !inspectedDataModels.Contains(m_Icon.GetHashCode()))
		{
			inspectedDataModels.Add(m_Icon.GetHashCode());
			hash = hash * 31 + m_Icon.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num3 = hash * 31;
		_ = m_PoolType;
		hash = num3 + m_PoolType.GetHashCode();
		hash = hash * 31 + ((m_TimeUntilNextQuest != null) ? m_TimeUntilNextQuest.GetHashCode() : 0);
		int num4 = hash * 31;
		_ = m_RerollCount;
		hash = num4 + m_RerollCount.GetHashCode();
		if (m_Rewards != null && !inspectedDataModels.Contains(m_Rewards.GetHashCode()))
		{
			inspectedDataModels.Add(m_Rewards.GetHashCode());
			hash = hash * 31 + m_Rewards.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num5 = hash * 31;
		_ = m_QuestId;
		hash = num5 + m_QuestId.GetHashCode();
		int num6 = hash * 31;
		_ = m_RewardTrackXp;
		hash = num6 + m_RewardTrackXp.GetHashCode();
		hash = hash * 31 + ((m_ProgressMessage != null) ? m_ProgressMessage.GetHashCode() : 0);
		int num7 = hash * 31;
		_ = m_Status;
		hash = num7 + m_Status.GetHashCode();
		int num8 = hash * 31;
		_ = m_PoolId;
		hash = num8 + m_PoolId.GetHashCode();
		int num9 = hash * 31;
		_ = m_Abandonable;
		hash = num9 + m_Abandonable.GetHashCode();
		int num10 = hash * 31;
		_ = m_NextInChain;
		hash = num10 + m_NextInChain.GetHashCode();
		hash = hash * 31 + ((m_TimeUntilExpiration != null) ? m_TimeUntilExpiration.GetHashCode() : 0);
		int num11 = hash * 31;
		_ = m_DisplayMode;
		hash = num11 + m_DisplayMode.GetHashCode();
		int num12 = hash * 31;
		_ = m_RewardTrackType;
		hash = num12 + m_RewardTrackType.GetHashCode();
		hash = hash * 31 + ((m_DeepLink != null) ? m_DeepLink.GetHashCode() : 0);
		int num13 = hash * 31;
		_ = m_IsChainQuest;
		hash = num13 + m_IsChainQuest.GetHashCode();
		int num14 = hash * 31;
		_ = m_XPChangeStatus;
		hash = num14 + m_XPChangeStatus.GetHashCode();
		int num15 = hash * 31;
		_ = m_QuotaChangeStatus;
		hash = num15 + m_QuotaChangeStatus.GetHashCode();
		int num16 = hash * 31;
		_ = m_TriggerChangeStatus;
		hash = num16 + m_TriggerChangeStatus.GetHashCode();
		return hash * 31 + ((m_ToastDescription != null) ? m_ToastDescription.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Name;
			return true;
		case 1:
			value = m_Progress;
			return true;
		case 2:
			value = m_Quota;
			return true;
		case 3:
			value = m_Description;
			return true;
		case 5:
			value = m_Icon;
			return true;
		case 6:
			value = m_PoolType;
			return true;
		case 7:
			value = m_TimeUntilNextQuest;
			return true;
		case 8:
			value = m_RerollCount;
			return true;
		case 10:
			value = m_Rewards;
			return true;
		case 11:
			value = m_QuestId;
			return true;
		case 12:
			value = m_RewardTrackXp;
			return true;
		case 13:
			value = m_ProgressMessage;
			return true;
		case 14:
			value = m_Status;
			return true;
		case 15:
			value = m_PoolId;
			return true;
		case 16:
			value = m_Abandonable;
			return true;
		case 17:
			value = m_NextInChain;
			return true;
		case 18:
			value = m_TimeUntilExpiration;
			return true;
		case 19:
			value = m_DisplayMode;
			return true;
		case 20:
			value = m_RewardTrackType;
			return true;
		case 21:
			value = m_DeepLink;
			return true;
		case 22:
			value = m_IsChainQuest;
			return true;
		case 1114:
			value = m_XPChangeStatus;
			return true;
		case 1115:
			value = m_QuotaChangeStatus;
			return true;
		case 1116:
			value = m_TriggerChangeStatus;
			return true;
		case 1117:
			value = m_ToastDescription;
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
			Name = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			Progress = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			Quota = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			Description = ((value != null) ? ((string)value) : null);
			return true;
		case 5:
			Icon = ((value != null) ? ((DataModelList<string>)value) : null);
			return true;
		case 6:
			PoolType = ((value != null) ? ((QuestPool.QuestPoolType)value) : QuestPool.QuestPoolType.NONE);
			return true;
		case 7:
			TimeUntilNextQuest = ((value != null) ? ((string)value) : null);
			return true;
		case 8:
			RerollCount = ((value != null) ? ((int)value) : 0);
			return true;
		case 10:
			Rewards = ((value != null) ? ((RewardListDataModel)value) : null);
			return true;
		case 11:
			QuestId = ((value != null) ? ((int)value) : 0);
			return true;
		case 12:
			RewardTrackXp = ((value != null) ? ((int)value) : 0);
			return true;
		case 13:
			ProgressMessage = ((value != null) ? ((string)value) : null);
			return true;
		case 14:
			Status = ((value != null) ? ((QuestManager.QuestStatus)value) : QuestManager.QuestStatus.UNKNOWN);
			return true;
		case 15:
			PoolId = ((value != null) ? ((int)value) : 0);
			return true;
		case 16:
			Abandonable = value != null && (bool)value;
			return true;
		case 17:
			NextInChain = ((value != null) ? ((int)value) : 0);
			return true;
		case 18:
			TimeUntilExpiration = ((value != null) ? ((string)value) : null);
			return true;
		case 19:
			DisplayMode = ((value != null) ? ((QuestManager.QuestDisplayMode)value) : QuestManager.QuestDisplayMode.Default);
			return true;
		case 20:
			RewardTrackType = ((value != null) ? ((Global.RewardTrackType)value) : Global.RewardTrackType.NONE);
			return true;
		case 21:
			DeepLink = ((value != null) ? ((string)value) : null);
			return true;
		case 22:
			IsChainQuest = value != null && (bool)value;
			return true;
		case 1114:
			XPChangeStatus = ((value != null) ? ((QuestManager.QuestChangeState)value) : QuestManager.QuestChangeState.NONE);
			return true;
		case 1115:
			QuotaChangeStatus = ((value != null) ? ((QuestManager.QuestChangeState)value) : QuestManager.QuestChangeState.NONE);
			return true;
		case 1116:
			TriggerChangeStatus = ((value != null) ? ((QuestManager.QuestChangeState)value) : QuestManager.QuestChangeState.NONE);
			return true;
		case 1117:
			ToastDescription = ((value != null) ? ((string)value) : null);
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
		case 5:
			info = Properties[4];
			return true;
		case 6:
			info = Properties[5];
			return true;
		case 7:
			info = Properties[6];
			return true;
		case 8:
			info = Properties[7];
			return true;
		case 10:
			info = Properties[8];
			return true;
		case 11:
			info = Properties[9];
			return true;
		case 12:
			info = Properties[10];
			return true;
		case 13:
			info = Properties[11];
			return true;
		case 14:
			info = Properties[12];
			return true;
		case 15:
			info = Properties[13];
			return true;
		case 16:
			info = Properties[14];
			return true;
		case 17:
			info = Properties[15];
			return true;
		case 18:
			info = Properties[16];
			return true;
		case 19:
			info = Properties[17];
			return true;
		case 20:
			info = Properties[18];
			return true;
		case 21:
			info = Properties[19];
			return true;
		case 22:
			info = Properties[20];
			return true;
		case 1114:
			info = Properties[21];
			return true;
		case 1115:
			info = Properties[22];
			return true;
		case 1116:
			info = Properties[23];
			return true;
		case 1117:
			info = Properties[24];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
