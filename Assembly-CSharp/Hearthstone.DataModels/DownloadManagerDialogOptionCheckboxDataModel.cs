using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class DownloadManagerDialogOptionCheckboxDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 902;

	private int m_ID;

	private bool m_IsChecked;

	private string m_NameLabel;

	private string m_DescriptionLabel;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "is_checked",
			Type = typeof(bool)
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
			PropertyDisplayName = "description_label",
			Type = typeof(string)
		}
	};

	public int DataModelId => 902;

	public string DataModelDisplayName => "download_manager_dialog_option_checkbox";

	public int ID
	{
		get
		{
			return m_ID;
		}
		set
		{
			if (m_ID != value)
			{
				m_ID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsChecked
	{
		get
		{
			return m_IsChecked;
		}
		set
		{
			if (m_IsChecked != value)
			{
				m_IsChecked = value;
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

	public string DescriptionLabel
	{
		get
		{
			return m_DescriptionLabel;
		}
		set
		{
			if (!(m_DescriptionLabel == value))
			{
				m_DescriptionLabel = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_ID;
		int num2 = (num + m_ID.GetHashCode()) * 31;
		_ = m_IsChecked;
		return ((num2 + m_IsChecked.GetHashCode()) * 31 + ((m_NameLabel != null) ? m_NameLabel.GetHashCode() : 0)) * 31 + ((m_DescriptionLabel != null) ? m_DescriptionLabel.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_ID;
			return true;
		case 1:
			value = m_IsChecked;
			return true;
		case 2:
			value = m_NameLabel;
			return true;
		case 3:
			value = m_DescriptionLabel;
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
			ID = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			IsChecked = value != null && (bool)value;
			return true;
		case 2:
			NameLabel = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			DescriptionLabel = ((value != null) ? ((string)value) : null);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
