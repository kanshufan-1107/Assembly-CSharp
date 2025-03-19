using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class DownloadManagerDialogDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 880;

	private DataModelList<DownloadManagerDialogModuleItemDataModel> m_ModuleList = new DataModelList<DownloadManagerDialogModuleItemDataModel>();

	private DataModelList<DownloadManagerDialogOptionCheckboxDataModel> m_OptionList = new DataModelList<DownloadManagerDialogOptionCheckboxDataModel>();

	private string m_SizeAvailableLabel;

	private DownloadManagerDialog.DownloaderState m_DownloaderState;

	private bool m_OptionalAssetsDownloadEnabled;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "module_list",
			Type = typeof(DataModelList<DownloadManagerDialogModuleItemDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "option_list",
			Type = typeof(DataModelList<DownloadManagerDialogOptionCheckboxDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "size_available_label",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "downloader_state",
			Type = typeof(DownloadManagerDialog.DownloaderState)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "optional_assets_download_enabled",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 880;

	public string DataModelDisplayName => "download_manager_dialog";

	public DataModelList<DownloadManagerDialogModuleItemDataModel> ModuleList
	{
		get
		{
			return m_ModuleList;
		}
		set
		{
			if (m_ModuleList != value)
			{
				RemoveNestedDataModel(m_ModuleList);
				RegisterNestedDataModel(value);
				m_ModuleList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<DownloadManagerDialogOptionCheckboxDataModel> OptionList
	{
		get
		{
			return m_OptionList;
		}
		set
		{
			if (m_OptionList != value)
			{
				RemoveNestedDataModel(m_OptionList);
				RegisterNestedDataModel(value);
				m_OptionList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string SizeAvailableLabel
	{
		get
		{
			return m_SizeAvailableLabel;
		}
		set
		{
			if (!(m_SizeAvailableLabel == value))
			{
				m_SizeAvailableLabel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DownloadManagerDialog.DownloaderState DownloaderState
	{
		get
		{
			return m_DownloaderState;
		}
		set
		{
			if (m_DownloaderState != value)
			{
				m_DownloaderState = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool OptionalAssetsDownloadEnabled
	{
		get
		{
			return m_OptionalAssetsDownloadEnabled;
		}
		set
		{
			if (m_OptionalAssetsDownloadEnabled != value)
			{
				m_OptionalAssetsDownloadEnabled = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public DownloadManagerDialogDataModel()
	{
		RegisterNestedDataModel(m_ModuleList);
		RegisterNestedDataModel(m_OptionList);
	}

	public int GetPropertiesHashCode()
	{
		int num = (((17 * 31 + ((m_ModuleList != null) ? m_ModuleList.GetPropertiesHashCode() : 0)) * 31 + ((m_OptionList != null) ? m_OptionList.GetPropertiesHashCode() : 0)) * 31 + ((m_SizeAvailableLabel != null) ? m_SizeAvailableLabel.GetHashCode() : 0)) * 31;
		_ = m_DownloaderState;
		int num2 = (num + m_DownloaderState.GetHashCode()) * 31;
		_ = m_OptionalAssetsDownloadEnabled;
		return num2 + m_OptionalAssetsDownloadEnabled.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_ModuleList;
			return true;
		case 1:
			value = m_OptionList;
			return true;
		case 2:
			value = m_SizeAvailableLabel;
			return true;
		case 3:
			value = m_DownloaderState;
			return true;
		case 4:
			value = m_OptionalAssetsDownloadEnabled;
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
			ModuleList = ((value != null) ? ((DataModelList<DownloadManagerDialogModuleItemDataModel>)value) : null);
			return true;
		case 1:
			OptionList = ((value != null) ? ((DataModelList<DownloadManagerDialogOptionCheckboxDataModel>)value) : null);
			return true;
		case 2:
			SizeAvailableLabel = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			DownloaderState = ((value != null) ? ((DownloadManagerDialog.DownloaderState)value) : DownloadManagerDialog.DownloaderState.Idle);
			return true;
		case 4:
			OptionalAssetsDownloadEnabled = value != null && (bool)value;
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
