using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class DownloadManagerDialogModuleItemDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 879;

	private string m_ID;

	private DownloadManagerDialog.DownloadStatus m_Status;

	private string m_NameLabel;

	private string m_TotalSizeLabel;

	private string m_DownloadedSizeLabel;

	private string m_RemainingTimeLabel;

	private float m_Progress;

	private DataModelProperty[] m_properties = new DataModelProperty[7]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "id",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "status",
			Type = typeof(DownloadManagerDialog.DownloadStatus)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "name_label",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "total_size_label",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "downloaded_size_label",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "remaining_time_label",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "progress",
			Type = typeof(float)
		}
	};

	public int DataModelId => 879;

	public string DataModelDisplayName => "download_manager_dialog_module_item";

	public string ID
	{
		get
		{
			return m_ID;
		}
		set
		{
			if (!(m_ID == value))
			{
				m_ID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DownloadManagerDialog.DownloadStatus Status
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

	public string NameLabel
	{
		get
		{
			return m_NameLabel;
		}
		set
		{
			if (!(m_NameLabel == value))
			{
				m_NameLabel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string TotalSizeLabel
	{
		get
		{
			return m_TotalSizeLabel;
		}
		set
		{
			if (!(m_TotalSizeLabel == value))
			{
				m_TotalSizeLabel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DownloadedSizeLabel
	{
		get
		{
			return m_DownloadedSizeLabel;
		}
		set
		{
			if (!(m_DownloadedSizeLabel == value))
			{
				m_DownloadedSizeLabel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string RemainingTimeLabel
	{
		get
		{
			return m_RemainingTimeLabel;
		}
		set
		{
			if (!(m_RemainingTimeLabel == value))
			{
				m_RemainingTimeLabel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public float Progress
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

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int num = (17 * 31 + ((m_ID != null) ? m_ID.GetHashCode() : 0)) * 31;
		_ = m_Status;
		int num2 = (((((num + m_Status.GetHashCode()) * 31 + ((m_NameLabel != null) ? m_NameLabel.GetHashCode() : 0)) * 31 + ((m_TotalSizeLabel != null) ? m_TotalSizeLabel.GetHashCode() : 0)) * 31 + ((m_DownloadedSizeLabel != null) ? m_DownloadedSizeLabel.GetHashCode() : 0)) * 31 + ((m_RemainingTimeLabel != null) ? m_RemainingTimeLabel.GetHashCode() : 0)) * 31;
		_ = m_Progress;
		return num2 + m_Progress.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_ID;
			return true;
		case 1:
			value = m_Status;
			return true;
		case 2:
			value = m_NameLabel;
			return true;
		case 3:
			value = m_TotalSizeLabel;
			return true;
		case 4:
			value = m_DownloadedSizeLabel;
			return true;
		case 5:
			value = m_RemainingTimeLabel;
			return true;
		case 6:
			value = m_Progress;
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
			ID = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			Status = ((value != null) ? ((DownloadManagerDialog.DownloadStatus)value) : DownloadManagerDialog.DownloadStatus.NOT_REQUESTED);
			return true;
		case 2:
			NameLabel = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			TotalSizeLabel = ((value != null) ? ((string)value) : null);
			return true;
		case 4:
			DownloadedSizeLabel = ((value != null) ? ((string)value) : null);
			return true;
		case 5:
			RemainingTimeLabel = ((value != null) ? ((string)value) : null);
			return true;
		case 6:
			Progress = ((value != null) ? ((float)value) : 0f);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
