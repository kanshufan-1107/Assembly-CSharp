using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ShopTabDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 842;

	private string m_Id;

	private string m_Name;

	private string m_Icon;

	private bool m_Locked;

	private IGamemodeAvailabilityService.Gamemode m_LockedMode;

	private IGamemodeAvailabilityService.Status m_LockedReason;

	private bool m_HasUndisplayedProducts;

	private bool m_NotificationEnabled;

	private DataModelProperty[] m_properties = new DataModelProperty[8]
	{
		new DataModelProperty
		{
			PropertyId = 853,
			PropertyDisplayName = "id",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 843,
			PropertyDisplayName = "name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 844,
			PropertyDisplayName = "icon",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 854,
			PropertyDisplayName = "locked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 857,
			PropertyDisplayName = "locked_reason_mode",
			Type = typeof(IGamemodeAvailabilityService.Gamemode)
		},
		new DataModelProperty
		{
			PropertyId = 855,
			PropertyDisplayName = "locked_reason_status",
			Type = typeof(IGamemodeAvailabilityService.Status)
		},
		new DataModelProperty
		{
			PropertyId = 856,
			PropertyDisplayName = "has_undisplayed_products",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 1110,
			PropertyDisplayName = "notification_enabled",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 842;

	public string DataModelDisplayName => "shop_tab";

	public string Id
	{
		get
		{
			return m_Id;
		}
		set
		{
			if (!(m_Id == value))
			{
				m_Id = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

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

	public string Icon
	{
		get
		{
			return m_Icon;
		}
		set
		{
			if (!(m_Icon == value))
			{
				m_Icon = value;
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

	public IGamemodeAvailabilityService.Gamemode LockedMode
	{
		get
		{
			return m_LockedMode;
		}
		set
		{
			if (m_LockedMode != value)
			{
				m_LockedMode = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public IGamemodeAvailabilityService.Status LockedReason
	{
		get
		{
			return m_LockedReason;
		}
		set
		{
			if (m_LockedReason != value)
			{
				m_LockedReason = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasUndisplayedProducts
	{
		get
		{
			return m_HasUndisplayedProducts;
		}
		set
		{
			if (m_HasUndisplayedProducts != value)
			{
				m_HasUndisplayedProducts = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool NotificationEnabled
	{
		get
		{
			return m_NotificationEnabled;
		}
		set
		{
			if (m_NotificationEnabled != value)
			{
				m_NotificationEnabled = value;
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
		int num = (((17 * 31 + ((m_Id != null) ? m_Id.GetHashCode() : 0)) * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0)) * 31 + ((m_Icon != null) ? m_Icon.GetHashCode() : 0)) * 31;
		_ = m_Locked;
		int num2 = (num + m_Locked.GetHashCode()) * 31;
		_ = m_LockedMode;
		int num3 = (num2 + m_LockedMode.GetHashCode()) * 31;
		_ = m_LockedReason;
		int num4 = (num3 + m_LockedReason.GetHashCode()) * 31;
		_ = m_HasUndisplayedProducts;
		int num5 = (num4 + m_HasUndisplayedProducts.GetHashCode()) * 31;
		_ = m_NotificationEnabled;
		return num5 + m_NotificationEnabled.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 853:
			value = m_Id;
			return true;
		case 843:
			value = m_Name;
			return true;
		case 844:
			value = m_Icon;
			return true;
		case 854:
			value = m_Locked;
			return true;
		case 857:
			value = m_LockedMode;
			return true;
		case 855:
			value = m_LockedReason;
			return true;
		case 856:
			value = m_HasUndisplayedProducts;
			return true;
		case 1110:
			value = m_NotificationEnabled;
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
		case 853:
			Id = ((value != null) ? ((string)value) : null);
			return true;
		case 843:
			Name = ((value != null) ? ((string)value) : null);
			return true;
		case 844:
			Icon = ((value != null) ? ((string)value) : null);
			return true;
		case 854:
			Locked = value != null && (bool)value;
			return true;
		case 857:
			LockedMode = ((value != null) ? ((IGamemodeAvailabilityService.Gamemode)value) : IGamemodeAvailabilityService.Gamemode.NONE);
			return true;
		case 855:
			LockedReason = ((value != null) ? ((IGamemodeAvailabilityService.Status)value) : IGamemodeAvailabilityService.Status.NONE);
			return true;
		case 856:
			HasUndisplayedProducts = value != null && (bool)value;
			return true;
		case 1110:
			NotificationEnabled = value != null && (bool)value;
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 853:
			info = Properties[0];
			return true;
		case 843:
			info = Properties[1];
			return true;
		case 844:
			info = Properties[2];
			return true;
		case 854:
			info = Properties[3];
			return true;
		case 857:
			info = Properties[4];
			return true;
		case 855:
			info = Properties[5];
			return true;
		case 856:
			info = Properties[6];
			return true;
		case 1110:
			info = Properties[7];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
