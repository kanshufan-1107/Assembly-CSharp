using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceBountyDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 193;

	private int m_BountyId;

	private AdventureMissionDataModel m_AdventureMission;

	private string m_ComingSoonText;

	private bool m_Available;

	private bool m_Complete;

	private string m_PosterText;

	private bool m_IsDisabled;

	private bool m_IsLocked;

	private bool m_IsEventLocked;

	private bool m_IsNew;

	private bool m_IsComingSoon;

	private int m_ComingSoonInDays;

	private int m_BestMythicLevel;

	private DataModelProperty[] m_properties = new DataModelProperty[13]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "bounty_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "adventure_mission",
			Type = typeof(AdventureMissionDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "coming_soon_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "available",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "complete",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "poster_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "is_disabled",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "is_locked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 619,
			PropertyDisplayName = "is_event_locked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 654,
			PropertyDisplayName = "is_new",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 779,
			PropertyDisplayName = "is_coming_soon",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 780,
			PropertyDisplayName = "is_coming_soon_in_days",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 781,
			PropertyDisplayName = "best_mythic_level",
			Type = typeof(int)
		}
	};

	public int DataModelId => 193;

	public string DataModelDisplayName => "lettuce_bounty";

	public int BountyId
	{
		get
		{
			return m_BountyId;
		}
		set
		{
			if (m_BountyId != value)
			{
				m_BountyId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public AdventureMissionDataModel AdventureMission
	{
		get
		{
			return m_AdventureMission;
		}
		set
		{
			if (m_AdventureMission != value)
			{
				RemoveNestedDataModel(m_AdventureMission);
				RegisterNestedDataModel(value);
				m_AdventureMission = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ComingSoonText
	{
		get
		{
			return m_ComingSoonText;
		}
		set
		{
			if (!(m_ComingSoonText == value))
			{
				m_ComingSoonText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Available
	{
		get
		{
			return m_Available;
		}
		set
		{
			if (m_Available != value)
			{
				m_Available = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Complete
	{
		get
		{
			return m_Complete;
		}
		set
		{
			if (m_Complete != value)
			{
				m_Complete = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string PosterText
	{
		get
		{
			return m_PosterText;
		}
		set
		{
			if (!(m_PosterText == value))
			{
				m_PosterText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsDisabled
	{
		get
		{
			return m_IsDisabled;
		}
		set
		{
			if (m_IsDisabled != value)
			{
				m_IsDisabled = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsLocked
	{
		get
		{
			return m_IsLocked;
		}
		set
		{
			if (m_IsLocked != value)
			{
				m_IsLocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsEventLocked
	{
		get
		{
			return m_IsEventLocked;
		}
		set
		{
			if (m_IsEventLocked != value)
			{
				m_IsEventLocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsNew
	{
		get
		{
			return m_IsNew;
		}
		set
		{
			if (m_IsNew != value)
			{
				m_IsNew = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsComingSoon
	{
		get
		{
			return m_IsComingSoon;
		}
		set
		{
			if (m_IsComingSoon != value)
			{
				m_IsComingSoon = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int ComingSoonInDays
	{
		get
		{
			return m_ComingSoonInDays;
		}
		set
		{
			if (m_ComingSoonInDays != value)
			{
				m_ComingSoonInDays = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int BestMythicLevel
	{
		get
		{
			return m_BestMythicLevel;
		}
		set
		{
			if (m_BestMythicLevel != value)
			{
				m_BestMythicLevel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceBountyDataModel()
	{
		RegisterNestedDataModel(m_AdventureMission);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_BountyId;
		hash = num + m_BountyId.GetHashCode();
		if (m_AdventureMission != null && !inspectedDataModels.Contains(m_AdventureMission.GetHashCode()))
		{
			inspectedDataModels.Add(m_AdventureMission.GetHashCode());
			hash = hash * 31 + m_AdventureMission.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		hash = hash * 31 + ((m_ComingSoonText != null) ? m_ComingSoonText.GetHashCode() : 0);
		int num2 = hash * 31;
		_ = m_Available;
		hash = num2 + m_Available.GetHashCode();
		int num3 = hash * 31;
		_ = m_Complete;
		hash = num3 + m_Complete.GetHashCode();
		hash = hash * 31 + ((m_PosterText != null) ? m_PosterText.GetHashCode() : 0);
		int num4 = hash * 31;
		_ = m_IsDisabled;
		hash = num4 + m_IsDisabled.GetHashCode();
		int num5 = hash * 31;
		_ = m_IsLocked;
		hash = num5 + m_IsLocked.GetHashCode();
		int num6 = hash * 31;
		_ = m_IsEventLocked;
		hash = num6 + m_IsEventLocked.GetHashCode();
		int num7 = hash * 31;
		_ = m_IsNew;
		hash = num7 + m_IsNew.GetHashCode();
		int num8 = hash * 31;
		_ = m_IsComingSoon;
		hash = num8 + m_IsComingSoon.GetHashCode();
		int num9 = hash * 31;
		_ = m_ComingSoonInDays;
		hash = num9 + m_ComingSoonInDays.GetHashCode();
		int num10 = hash * 31;
		_ = m_BestMythicLevel;
		return num10 + m_BestMythicLevel.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_BountyId;
			return true;
		case 1:
			value = m_AdventureMission;
			return true;
		case 2:
			value = m_ComingSoonText;
			return true;
		case 3:
			value = m_Available;
			return true;
		case 4:
			value = m_Complete;
			return true;
		case 5:
			value = m_PosterText;
			return true;
		case 6:
			value = m_IsDisabled;
			return true;
		case 7:
			value = m_IsLocked;
			return true;
		case 619:
			value = m_IsEventLocked;
			return true;
		case 654:
			value = m_IsNew;
			return true;
		case 779:
			value = m_IsComingSoon;
			return true;
		case 780:
			value = m_ComingSoonInDays;
			return true;
		case 781:
			value = m_BestMythicLevel;
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
			BountyId = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			AdventureMission = ((value != null) ? ((AdventureMissionDataModel)value) : null);
			return true;
		case 2:
			ComingSoonText = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			Available = value != null && (bool)value;
			return true;
		case 4:
			Complete = value != null && (bool)value;
			return true;
		case 5:
			PosterText = ((value != null) ? ((string)value) : null);
			return true;
		case 6:
			IsDisabled = value != null && (bool)value;
			return true;
		case 7:
			IsLocked = value != null && (bool)value;
			return true;
		case 619:
			IsEventLocked = value != null && (bool)value;
			return true;
		case 654:
			IsNew = value != null && (bool)value;
			return true;
		case 779:
			IsComingSoon = value != null && (bool)value;
			return true;
		case 780:
			ComingSoonInDays = ((value != null) ? ((int)value) : 0);
			return true;
		case 781:
			BestMythicLevel = ((value != null) ? ((int)value) : 0);
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
		case 619:
			info = Properties[8];
			return true;
		case 654:
			info = Properties[9];
			return true;
		case 779:
			info = Properties[10];
			return true;
		case 780:
			info = Properties[11];
			return true;
		case 781:
			info = Properties[12];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
