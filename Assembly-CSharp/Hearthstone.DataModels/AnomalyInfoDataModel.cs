using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class AnomalyInfoDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 845;

	private string m_RemainingTime;

	private bool m_HasSeenAnomalyModeGlow;

	private string m_RulesText;

	private bool m_AlwaysActive;

	private bool m_EventActive;

	private string m_CurrentSceneMode;

	private DataModelProperty[] m_properties = new DataModelProperty[6]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "remaining_time",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "has_seen_anomaly_mode_glow",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "rules_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "always_active",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "event_active",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "current_scene_mode",
			Type = typeof(string)
		}
	};

	public int DataModelId => 845;

	public string DataModelDisplayName => "anomaly_info";

	public string RemainingTime
	{
		get
		{
			return m_RemainingTime;
		}
		set
		{
			if (!(m_RemainingTime == value))
			{
				m_RemainingTime = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool HasSeenAnomalyModeGlow
	{
		get
		{
			return m_HasSeenAnomalyModeGlow;
		}
		set
		{
			if (m_HasSeenAnomalyModeGlow != value)
			{
				m_HasSeenAnomalyModeGlow = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string RulesText
	{
		get
		{
			return m_RulesText;
		}
		set
		{
			if (!(m_RulesText == value))
			{
				m_RulesText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool AlwaysActive
	{
		get
		{
			return m_AlwaysActive;
		}
		set
		{
			if (m_AlwaysActive != value)
			{
				m_AlwaysActive = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool EventActive
	{
		get
		{
			return m_EventActive;
		}
		set
		{
			if (m_EventActive != value)
			{
				m_EventActive = value;
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

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int num = (17 * 31 + ((m_RemainingTime != null) ? m_RemainingTime.GetHashCode() : 0)) * 31;
		_ = m_HasSeenAnomalyModeGlow;
		int num2 = ((num + m_HasSeenAnomalyModeGlow.GetHashCode()) * 31 + ((m_RulesText != null) ? m_RulesText.GetHashCode() : 0)) * 31;
		_ = m_AlwaysActive;
		int num3 = (num2 + m_AlwaysActive.GetHashCode()) * 31;
		_ = m_EventActive;
		return (num3 + m_EventActive.GetHashCode()) * 31 + ((m_CurrentSceneMode != null) ? m_CurrentSceneMode.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_RemainingTime;
			return true;
		case 1:
			value = m_HasSeenAnomalyModeGlow;
			return true;
		case 2:
			value = m_RulesText;
			return true;
		case 3:
			value = m_AlwaysActive;
			return true;
		case 4:
			value = m_EventActive;
			return true;
		case 5:
			value = m_CurrentSceneMode;
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
			RemainingTime = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			HasSeenAnomalyModeGlow = value != null && (bool)value;
			return true;
		case 2:
			RulesText = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			AlwaysActive = value != null && (bool)value;
			return true;
		case 4:
			EventActive = value != null && (bool)value;
			return true;
		case 5:
			CurrentSceneMode = ((value != null) ? ((string)value) : null);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
