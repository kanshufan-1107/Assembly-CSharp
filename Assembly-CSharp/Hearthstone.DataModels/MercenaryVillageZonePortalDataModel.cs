using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.DataModels;

public class MercenaryVillageZonePortalDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 354;

	private Texture m_SelectedZoneTexture;

	private string m_SelectedZoneName;

	private string m_SelectedZoneDescription;

	private bool m_IsSelectedModeComingSoon;

	private bool m_IsSelectedModeLocked;

	private string m_SelectedModeLockedReason;

	private LettuceVillageZonePortal.BountySetDifficultyModes m_SelectedModeDifficulty;

	private int m_BossRushDaysTillReset;

	private int m_BossRushHoursTillReset;

	private DataModelProperty[] m_properties = new DataModelProperty[9]
	{
		new DataModelProperty
		{
			PropertyId = 501,
			PropertyDisplayName = "selected_zone_texture",
			Type = typeof(Texture)
		},
		new DataModelProperty
		{
			PropertyId = 507,
			PropertyDisplayName = "selected_zone_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 513,
			PropertyDisplayName = "selected_zone_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 514,
			PropertyDisplayName = "is_selected_mode_coming_soon",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 515,
			PropertyDisplayName = "is_selected_mode_locked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 516,
			PropertyDisplayName = "selected_mode_locked_reason",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 524,
			PropertyDisplayName = "selected_mode_difficulty",
			Type = typeof(LettuceVillageZonePortal.BountySetDifficultyModes)
		},
		new DataModelProperty
		{
			PropertyId = 774,
			PropertyDisplayName = "boss_rush_days_till_reset",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 775,
			PropertyDisplayName = "boss_rush_hours_till_reset",
			Type = typeof(int)
		}
	};

	public int DataModelId => 354;

	public string DataModelDisplayName => "mercenary_village_zone_portal";

	public Texture SelectedZoneTexture
	{
		get
		{
			return m_SelectedZoneTexture;
		}
		set
		{
			if (!(m_SelectedZoneTexture == value))
			{
				m_SelectedZoneTexture = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string SelectedZoneName
	{
		get
		{
			return m_SelectedZoneName;
		}
		set
		{
			if (!(m_SelectedZoneName == value))
			{
				m_SelectedZoneName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string SelectedZoneDescription
	{
		get
		{
			return m_SelectedZoneDescription;
		}
		set
		{
			if (!(m_SelectedZoneDescription == value))
			{
				m_SelectedZoneDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsSelectedModeComingSoon
	{
		get
		{
			return m_IsSelectedModeComingSoon;
		}
		set
		{
			if (m_IsSelectedModeComingSoon != value)
			{
				m_IsSelectedModeComingSoon = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsSelectedModeLocked
	{
		get
		{
			return m_IsSelectedModeLocked;
		}
		set
		{
			if (m_IsSelectedModeLocked != value)
			{
				m_IsSelectedModeLocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string SelectedModeLockedReason
	{
		get
		{
			return m_SelectedModeLockedReason;
		}
		set
		{
			if (!(m_SelectedModeLockedReason == value))
			{
				m_SelectedModeLockedReason = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public LettuceVillageZonePortal.BountySetDifficultyModes SelectedModeDifficulty
	{
		get
		{
			return m_SelectedModeDifficulty;
		}
		set
		{
			if (m_SelectedModeDifficulty != value)
			{
				m_SelectedModeDifficulty = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int BossRushDaysTillReset
	{
		get
		{
			return m_BossRushDaysTillReset;
		}
		set
		{
			if (m_BossRushDaysTillReset != value)
			{
				m_BossRushDaysTillReset = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int BossRushHoursTillReset
	{
		get
		{
			return m_BossRushHoursTillReset;
		}
		set
		{
			if (m_BossRushHoursTillReset != value)
			{
				m_BossRushHoursTillReset = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int num = (((17 * 31 + ((m_SelectedZoneTexture != null) ? m_SelectedZoneTexture.GetHashCode() : 0)) * 31 + ((m_SelectedZoneName != null) ? m_SelectedZoneName.GetHashCode() : 0)) * 31 + ((m_SelectedZoneDescription != null) ? m_SelectedZoneDescription.GetHashCode() : 0)) * 31;
		_ = m_IsSelectedModeComingSoon;
		int num2 = (num + m_IsSelectedModeComingSoon.GetHashCode()) * 31;
		_ = m_IsSelectedModeLocked;
		int num3 = ((num2 + m_IsSelectedModeLocked.GetHashCode()) * 31 + ((m_SelectedModeLockedReason != null) ? m_SelectedModeLockedReason.GetHashCode() : 0)) * 31;
		_ = m_SelectedModeDifficulty;
		int num4 = (num3 + m_SelectedModeDifficulty.GetHashCode()) * 31;
		_ = m_BossRushDaysTillReset;
		int num5 = (num4 + m_BossRushDaysTillReset.GetHashCode()) * 31;
		_ = m_BossRushHoursTillReset;
		return num5 + m_BossRushHoursTillReset.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 501:
			value = m_SelectedZoneTexture;
			return true;
		case 507:
			value = m_SelectedZoneName;
			return true;
		case 513:
			value = m_SelectedZoneDescription;
			return true;
		case 514:
			value = m_IsSelectedModeComingSoon;
			return true;
		case 515:
			value = m_IsSelectedModeLocked;
			return true;
		case 516:
			value = m_SelectedModeLockedReason;
			return true;
		case 524:
			value = m_SelectedModeDifficulty;
			return true;
		case 774:
			value = m_BossRushDaysTillReset;
			return true;
		case 775:
			value = m_BossRushHoursTillReset;
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
		case 501:
			SelectedZoneTexture = ((value != null) ? ((Texture)value) : null);
			return true;
		case 507:
			SelectedZoneName = ((value != null) ? ((string)value) : null);
			return true;
		case 513:
			SelectedZoneDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 514:
			IsSelectedModeComingSoon = value != null && (bool)value;
			return true;
		case 515:
			IsSelectedModeLocked = value != null && (bool)value;
			return true;
		case 516:
			SelectedModeLockedReason = ((value != null) ? ((string)value) : null);
			return true;
		case 524:
			SelectedModeDifficulty = ((value != null) ? ((LettuceVillageZonePortal.BountySetDifficultyModes)value) : LettuceVillageZonePortal.BountySetDifficultyModes.Normal);
			return true;
		case 774:
			BossRushDaysTillReset = ((value != null) ? ((int)value) : 0);
			return true;
		case 775:
			BossRushHoursTillReset = ((value != null) ? ((int)value) : 0);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 501:
			info = Properties[0];
			return true;
		case 507:
			info = Properties[1];
			return true;
		case 513:
			info = Properties[2];
			return true;
		case 514:
			info = Properties[3];
			return true;
		case 515:
			info = Properties[4];
			return true;
		case 516:
			info = Properties[5];
			return true;
		case 524:
			info = Properties[6];
			return true;
		case 774:
			info = Properties[7];
			return true;
		case 775:
			info = Properties[8];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
