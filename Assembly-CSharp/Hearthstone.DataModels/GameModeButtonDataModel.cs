using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class GameModeButtonDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 172;

	private string m_Name;

	private string m_Description;

	private string m_ButtonState;

	private int m_GameModeRecordId;

	private bool m_IsNew;

	private bool m_IsEarlyAccess;

	private bool m_IsBeta;

	private bool m_IsDownloadRequired;

	private bool m_IsDownloading;

	private string m_ModuleTag;

	private DataModelProperty[] m_properties = new DataModelProperty[10]
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
			PropertyDisplayName = "description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "buttonstate",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "gamemoderecordid",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "isnew",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 234,
			PropertyDisplayName = "isearlyaccess",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "isbeta",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "isdownloadrequired",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "isdownloading",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "moduletag",
			Type = typeof(string)
		}
	};

	public int DataModelId => 172;

	public string DataModelDisplayName => "gamemodebutton";

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

	public string ButtonState
	{
		get
		{
			return m_ButtonState;
		}
		set
		{
			if (!(m_ButtonState == value))
			{
				m_ButtonState = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int GameModeRecordId
	{
		get
		{
			return m_GameModeRecordId;
		}
		set
		{
			if (m_GameModeRecordId != value)
			{
				m_GameModeRecordId = value;
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

	public bool IsEarlyAccess
	{
		get
		{
			return m_IsEarlyAccess;
		}
		set
		{
			if (m_IsEarlyAccess != value)
			{
				m_IsEarlyAccess = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsBeta
	{
		get
		{
			return m_IsBeta;
		}
		set
		{
			if (m_IsBeta != value)
			{
				m_IsBeta = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsDownloadRequired
	{
		get
		{
			return m_IsDownloadRequired;
		}
		set
		{
			if (m_IsDownloadRequired != value)
			{
				m_IsDownloadRequired = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsDownloading
	{
		get
		{
			return m_IsDownloading;
		}
		set
		{
			if (m_IsDownloading != value)
			{
				m_IsDownloading = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ModuleTag
	{
		get
		{
			return m_ModuleTag;
		}
		set
		{
			if (!(m_ModuleTag == value))
			{
				m_ModuleTag = value;
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
		int num = (((17 * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0)) * 31 + ((m_Description != null) ? m_Description.GetHashCode() : 0)) * 31 + ((m_ButtonState != null) ? m_ButtonState.GetHashCode() : 0)) * 31;
		_ = m_GameModeRecordId;
		int num2 = (num + m_GameModeRecordId.GetHashCode()) * 31;
		_ = m_IsNew;
		int num3 = (num2 + m_IsNew.GetHashCode()) * 31;
		_ = m_IsEarlyAccess;
		int num4 = (num3 + m_IsEarlyAccess.GetHashCode()) * 31;
		_ = m_IsBeta;
		int num5 = (num4 + m_IsBeta.GetHashCode()) * 31;
		_ = m_IsDownloadRequired;
		int num6 = (num5 + m_IsDownloadRequired.GetHashCode()) * 31;
		_ = m_IsDownloading;
		return (num6 + m_IsDownloading.GetHashCode()) * 31 + ((m_ModuleTag != null) ? m_ModuleTag.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Name;
			return true;
		case 1:
			value = m_Description;
			return true;
		case 2:
			value = m_ButtonState;
			return true;
		case 3:
			value = m_GameModeRecordId;
			return true;
		case 4:
			value = m_IsNew;
			return true;
		case 234:
			value = m_IsEarlyAccess;
			return true;
		case 5:
			value = m_IsBeta;
			return true;
		case 6:
			value = m_IsDownloadRequired;
			return true;
		case 7:
			value = m_IsDownloading;
			return true;
		case 8:
			value = m_ModuleTag;
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
			Description = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			ButtonState = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			GameModeRecordId = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			IsNew = value != null && (bool)value;
			return true;
		case 234:
			IsEarlyAccess = value != null && (bool)value;
			return true;
		case 5:
			IsBeta = value != null && (bool)value;
			return true;
		case 6:
			IsDownloadRequired = value != null && (bool)value;
			return true;
		case 7:
			IsDownloading = value != null && (bool)value;
			return true;
		case 8:
			ModuleTag = ((value != null) ? ((string)value) : null);
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
		case 234:
			info = Properties[5];
			return true;
		case 5:
			info = Properties[6];
			return true;
		case 6:
			info = Properties[7];
			return true;
		case 7:
			info = Properties[8];
			return true;
		case 8:
			info = Properties[9];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
