using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class FriendDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 159;

	private string m_PlayerName;

	private string m_PlayerStatus;

	private bool m_IsOnline;

	private bool m_IsAway;

	private bool m_IsBusy;

	private bool m_IsInEditMode;

	private bool m_IsSelected;

	private bool m_IsInHS;

	private bool m_CanBeSpectated;

	private bool m_IsFriend;

	private DataModelProperty[] m_properties = new DataModelProperty[10]
	{
		new DataModelProperty
		{
			PropertyId = 160,
			PropertyDisplayName = "player_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 161,
			PropertyDisplayName = "player_status",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 162,
			PropertyDisplayName = "is_online",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 163,
			PropertyDisplayName = "is_away",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 164,
			PropertyDisplayName = "is_busy",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 167,
			PropertyDisplayName = "is_in_edit_mode",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 169,
			PropertyDisplayName = "is_selected",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 170,
			PropertyDisplayName = "is_in_hs",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 171,
			PropertyDisplayName = "can_be_spectated",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 172,
			PropertyDisplayName = "is_friend",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 159;

	public string DataModelDisplayName => "friend";

	public string PlayerName
	{
		get
		{
			return m_PlayerName;
		}
		set
		{
			if (!(m_PlayerName == value))
			{
				m_PlayerName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string PlayerStatus
	{
		get
		{
			return m_PlayerStatus;
		}
		set
		{
			if (!(m_PlayerStatus == value))
			{
				m_PlayerStatus = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsOnline
	{
		get
		{
			return m_IsOnline;
		}
		set
		{
			if (m_IsOnline != value)
			{
				m_IsOnline = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsAway
	{
		get
		{
			return m_IsAway;
		}
		set
		{
			if (m_IsAway != value)
			{
				m_IsAway = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsBusy
	{
		get
		{
			return m_IsBusy;
		}
		set
		{
			if (m_IsBusy != value)
			{
				m_IsBusy = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsInEditMode
	{
		get
		{
			return m_IsInEditMode;
		}
		set
		{
			if (m_IsInEditMode != value)
			{
				m_IsInEditMode = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsSelected
	{
		get
		{
			return m_IsSelected;
		}
		set
		{
			if (m_IsSelected != value)
			{
				m_IsSelected = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsInHS
	{
		get
		{
			return m_IsInHS;
		}
		set
		{
			if (m_IsInHS != value)
			{
				m_IsInHS = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool CanBeSpectated
	{
		get
		{
			return m_CanBeSpectated;
		}
		set
		{
			if (m_CanBeSpectated != value)
			{
				m_CanBeSpectated = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsFriend
	{
		get
		{
			return m_IsFriend;
		}
		set
		{
			if (m_IsFriend != value)
			{
				m_IsFriend = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = ((17 * 31 + ((m_PlayerName != null) ? m_PlayerName.GetHashCode() : 0)) * 31 + ((m_PlayerStatus != null) ? m_PlayerStatus.GetHashCode() : 0)) * 31;
		_ = m_IsOnline;
		int num2 = (num + m_IsOnline.GetHashCode()) * 31;
		_ = m_IsAway;
		int num3 = (num2 + m_IsAway.GetHashCode()) * 31;
		_ = m_IsBusy;
		int num4 = (num3 + m_IsBusy.GetHashCode()) * 31;
		_ = m_IsInEditMode;
		int num5 = (num4 + m_IsInEditMode.GetHashCode()) * 31;
		_ = m_IsSelected;
		int num6 = (num5 + m_IsSelected.GetHashCode()) * 31;
		_ = m_IsInHS;
		int num7 = (num6 + m_IsInHS.GetHashCode()) * 31;
		_ = m_CanBeSpectated;
		int num8 = (num7 + m_CanBeSpectated.GetHashCode()) * 31;
		_ = m_IsFriend;
		return num8 + m_IsFriend.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 160:
			value = m_PlayerName;
			return true;
		case 161:
			value = m_PlayerStatus;
			return true;
		case 162:
			value = m_IsOnline;
			return true;
		case 163:
			value = m_IsAway;
			return true;
		case 164:
			value = m_IsBusy;
			return true;
		case 167:
			value = m_IsInEditMode;
			return true;
		case 169:
			value = m_IsSelected;
			return true;
		case 170:
			value = m_IsInHS;
			return true;
		case 171:
			value = m_CanBeSpectated;
			return true;
		case 172:
			value = m_IsFriend;
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
		case 160:
			PlayerName = ((value != null) ? ((string)value) : null);
			return true;
		case 161:
			PlayerStatus = ((value != null) ? ((string)value) : null);
			return true;
		case 162:
			IsOnline = value != null && (bool)value;
			return true;
		case 163:
			IsAway = value != null && (bool)value;
			return true;
		case 164:
			IsBusy = value != null && (bool)value;
			return true;
		case 167:
			IsInEditMode = value != null && (bool)value;
			return true;
		case 169:
			IsSelected = value != null && (bool)value;
			return true;
		case 170:
			IsInHS = value != null && (bool)value;
			return true;
		case 171:
			CanBeSpectated = value != null && (bool)value;
			return true;
		case 172:
			IsFriend = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 160:
			info = Properties[0];
			return true;
		case 161:
			info = Properties[1];
			return true;
		case 162:
			info = Properties[2];
			return true;
		case 163:
			info = Properties[3];
			return true;
		case 164:
			info = Properties[4];
			return true;
		case 167:
			info = Properties[5];
			return true;
		case 169:
			info = Properties[6];
			return true;
		case 170:
			info = Properties[7];
			return true;
		case 171:
			info = Properties[8];
			return true;
		case 172:
			info = Properties[9];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
