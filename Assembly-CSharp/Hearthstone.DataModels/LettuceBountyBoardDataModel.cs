using System.Collections.Generic;
using Assets;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.DataModels;

public class LettuceBountyBoardDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 194;

	private DataModelList<LettuceBountyDataModel> m_Bounties = new DataModelList<LettuceBountyDataModel>();

	private DataModelList<CardDataModel> m_BossCard = new DataModelList<CardDataModel>();

	private string m_BossName;

	private bool m_NeedsTeam;

	private int m_PageCount;

	private int m_PageIndex;

	private string m_BossDescription;

	private int m_AutoSelectedBountyRecordId;

	private int m_CurrentSelectedBountyRecordId;

	private Texture m_BountySetWatermark;

	private bool m_IsSelectedBountyLocked;

	private string m_BountySetShortGuid;

	private string m_HeaderText;

	private string m_BountyLockedText;

	private RewardListDataModel m_SelectedBountyRewardList;

	private LettuceBounty.MercenariesBountyDifficulty m_DifficultyMode;

	private int m_CurrentMythicLevel;

	private DataModelProperty[] m_properties = new DataModelProperty[17]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "bounties",
			Type = typeof(DataModelList<LettuceBountyDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "boss_card",
			Type = typeof(DataModelList<CardDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "boss_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "needs_team",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "page_count",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "page_index",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "boss_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "auto_selected_bounty_record_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "current_selected_bounty_record_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "bounty_set_watermark",
			Type = typeof(Texture)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "is_selected_bounty_locked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "bounty_set_short_guid",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 13,
			PropertyDisplayName = "header_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 14,
			PropertyDisplayName = "bounty_locked_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 15,
			PropertyDisplayName = "selected_bounty_reward_list",
			Type = typeof(RewardListDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 16,
			PropertyDisplayName = "difficulty_mode",
			Type = typeof(LettuceBounty.MercenariesBountyDifficulty)
		},
		new DataModelProperty
		{
			PropertyId = 786,
			PropertyDisplayName = "current_mythic_level",
			Type = typeof(int)
		}
	};

	public int DataModelId => 194;

	public string DataModelDisplayName => "lettuce_bounty_board";

	public DataModelList<LettuceBountyDataModel> Bounties
	{
		get
		{
			return m_Bounties;
		}
		set
		{
			if (m_Bounties != value)
			{
				RemoveNestedDataModel(m_Bounties);
				RegisterNestedDataModel(value);
				m_Bounties = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<CardDataModel> BossCard
	{
		get
		{
			return m_BossCard;
		}
		set
		{
			if (m_BossCard != value)
			{
				RemoveNestedDataModel(m_BossCard);
				RegisterNestedDataModel(value);
				m_BossCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string BossName
	{
		get
		{
			return m_BossName;
		}
		set
		{
			if (!(m_BossName == value))
			{
				m_BossName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool NeedsTeam
	{
		get
		{
			return m_NeedsTeam;
		}
		set
		{
			if (m_NeedsTeam != value)
			{
				m_NeedsTeam = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int PageCount
	{
		get
		{
			return m_PageCount;
		}
		set
		{
			if (m_PageCount != value)
			{
				m_PageCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int PageIndex
	{
		get
		{
			return m_PageIndex;
		}
		set
		{
			if (m_PageIndex != value)
			{
				m_PageIndex = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string BossDescription
	{
		get
		{
			return m_BossDescription;
		}
		set
		{
			if (!(m_BossDescription == value))
			{
				m_BossDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int AutoSelectedBountyRecordId
	{
		get
		{
			return m_AutoSelectedBountyRecordId;
		}
		set
		{
			if (m_AutoSelectedBountyRecordId != value)
			{
				m_AutoSelectedBountyRecordId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CurrentSelectedBountyRecordId
	{
		get
		{
			return m_CurrentSelectedBountyRecordId;
		}
		set
		{
			if (m_CurrentSelectedBountyRecordId != value)
			{
				m_CurrentSelectedBountyRecordId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public Texture BountySetWatermark
	{
		get
		{
			return m_BountySetWatermark;
		}
		set
		{
			if (!(m_BountySetWatermark == value))
			{
				m_BountySetWatermark = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsSelectedBountyLocked
	{
		get
		{
			return m_IsSelectedBountyLocked;
		}
		set
		{
			if (m_IsSelectedBountyLocked != value)
			{
				m_IsSelectedBountyLocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string BountySetShortGuid
	{
		get
		{
			return m_BountySetShortGuid;
		}
		set
		{
			if (!(m_BountySetShortGuid == value))
			{
				m_BountySetShortGuid = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string HeaderText
	{
		get
		{
			return m_HeaderText;
		}
		set
		{
			if (!(m_HeaderText == value))
			{
				m_HeaderText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string BountyLockedText
	{
		get
		{
			return m_BountyLockedText;
		}
		set
		{
			if (!(m_BountyLockedText == value))
			{
				m_BountyLockedText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardListDataModel SelectedBountyRewardList
	{
		get
		{
			return m_SelectedBountyRewardList;
		}
		set
		{
			if (m_SelectedBountyRewardList != value)
			{
				RemoveNestedDataModel(m_SelectedBountyRewardList);
				RegisterNestedDataModel(value);
				m_SelectedBountyRewardList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceBounty.MercenariesBountyDifficulty DifficultyMode
	{
		get
		{
			return m_DifficultyMode;
		}
		set
		{
			if (m_DifficultyMode != value)
			{
				m_DifficultyMode = value;
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

	public DataModelProperty[] Properties => m_properties;

	public LettuceBountyBoardDataModel()
	{
		RegisterNestedDataModel(m_Bounties);
		RegisterNestedDataModel(m_BossCard);
		RegisterNestedDataModel(m_SelectedBountyRewardList);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_Bounties != null && !inspectedDataModels.Contains(m_Bounties.GetHashCode()))
		{
			inspectedDataModels.Add(m_Bounties.GetHashCode());
			hash = hash * 31 + m_Bounties.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_BossCard != null && !inspectedDataModels.Contains(m_BossCard.GetHashCode()))
		{
			inspectedDataModels.Add(m_BossCard.GetHashCode());
			hash = hash * 31 + m_BossCard.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		hash = hash * 31 + ((m_BossName != null) ? m_BossName.GetHashCode() : 0);
		int num = hash * 31;
		_ = m_NeedsTeam;
		hash = num + m_NeedsTeam.GetHashCode();
		int num2 = hash * 31;
		_ = m_PageCount;
		hash = num2 + m_PageCount.GetHashCode();
		int num3 = hash * 31;
		_ = m_PageIndex;
		hash = num3 + m_PageIndex.GetHashCode();
		hash = hash * 31 + ((m_BossDescription != null) ? m_BossDescription.GetHashCode() : 0);
		int num4 = hash * 31;
		_ = m_AutoSelectedBountyRecordId;
		hash = num4 + m_AutoSelectedBountyRecordId.GetHashCode();
		int num5 = hash * 31;
		_ = m_CurrentSelectedBountyRecordId;
		hash = num5 + m_CurrentSelectedBountyRecordId.GetHashCode();
		hash = hash * 31 + ((m_BountySetWatermark != null) ? m_BountySetWatermark.GetHashCode() : 0);
		int num6 = hash * 31;
		_ = m_IsSelectedBountyLocked;
		hash = num6 + m_IsSelectedBountyLocked.GetHashCode();
		hash = hash * 31 + ((m_BountySetShortGuid != null) ? m_BountySetShortGuid.GetHashCode() : 0);
		hash = hash * 31 + ((m_HeaderText != null) ? m_HeaderText.GetHashCode() : 0);
		hash = hash * 31 + ((m_BountyLockedText != null) ? m_BountyLockedText.GetHashCode() : 0);
		if (m_SelectedBountyRewardList != null && !inspectedDataModels.Contains(m_SelectedBountyRewardList.GetHashCode()))
		{
			inspectedDataModels.Add(m_SelectedBountyRewardList.GetHashCode());
			hash = hash * 31 + m_SelectedBountyRewardList.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num7 = hash * 31;
		_ = m_DifficultyMode;
		hash = num7 + m_DifficultyMode.GetHashCode();
		int num8 = hash * 31;
		_ = m_CurrentMythicLevel;
		return num8 + m_CurrentMythicLevel.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Bounties;
			return true;
		case 1:
			value = m_BossCard;
			return true;
		case 2:
			value = m_BossName;
			return true;
		case 3:
			value = m_NeedsTeam;
			return true;
		case 4:
			value = m_PageCount;
			return true;
		case 5:
			value = m_PageIndex;
			return true;
		case 6:
			value = m_BossDescription;
			return true;
		case 7:
			value = m_AutoSelectedBountyRecordId;
			return true;
		case 8:
			value = m_CurrentSelectedBountyRecordId;
			return true;
		case 10:
			value = m_BountySetWatermark;
			return true;
		case 11:
			value = m_IsSelectedBountyLocked;
			return true;
		case 12:
			value = m_BountySetShortGuid;
			return true;
		case 13:
			value = m_HeaderText;
			return true;
		case 14:
			value = m_BountyLockedText;
			return true;
		case 15:
			value = m_SelectedBountyRewardList;
			return true;
		case 16:
			value = m_DifficultyMode;
			return true;
		case 786:
			value = m_CurrentMythicLevel;
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
			Bounties = ((value != null) ? ((DataModelList<LettuceBountyDataModel>)value) : null);
			return true;
		case 1:
			BossCard = ((value != null) ? ((DataModelList<CardDataModel>)value) : null);
			return true;
		case 2:
			BossName = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			NeedsTeam = value != null && (bool)value;
			return true;
		case 4:
			PageCount = ((value != null) ? ((int)value) : 0);
			return true;
		case 5:
			PageIndex = ((value != null) ? ((int)value) : 0);
			return true;
		case 6:
			BossDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 7:
			AutoSelectedBountyRecordId = ((value != null) ? ((int)value) : 0);
			return true;
		case 8:
			CurrentSelectedBountyRecordId = ((value != null) ? ((int)value) : 0);
			return true;
		case 10:
			BountySetWatermark = ((value != null) ? ((Texture)value) : null);
			return true;
		case 11:
			IsSelectedBountyLocked = value != null && (bool)value;
			return true;
		case 12:
			BountySetShortGuid = ((value != null) ? ((string)value) : null);
			return true;
		case 13:
			HeaderText = ((value != null) ? ((string)value) : null);
			return true;
		case 14:
			BountyLockedText = ((value != null) ? ((string)value) : null);
			return true;
		case 15:
			SelectedBountyRewardList = ((value != null) ? ((RewardListDataModel)value) : null);
			return true;
		case 16:
			DifficultyMode = ((value != null) ? ((LettuceBounty.MercenariesBountyDifficulty)value) : LettuceBounty.MercenariesBountyDifficulty.NONE);
			return true;
		case 786:
			CurrentMythicLevel = ((value != null) ? ((int)value) : 0);
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
		case 10:
			info = Properties[9];
			return true;
		case 11:
			info = Properties[10];
			return true;
		case 12:
			info = Properties[11];
			return true;
		case 13:
			info = Properties[12];
			return true;
		case 14:
			info = Properties[13];
			return true;
		case 15:
			info = Properties[14];
			return true;
		case 16:
			info = Properties[15];
			return true;
		case 786:
			info = Properties[16];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
