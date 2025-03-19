using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ChangeMessageContentDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 356;

	private string m_Title;

	private string m_BodyText;

	private DataModelList<RewardItemDataModel> m_ChangeMessageItemDisplay = new DataModelList<RewardItemDataModel>();

	private string m_Url;

	private DataModelProperty[] m_properties = new DataModelProperty[4]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "title",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "body_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "change_message_item_display",
			Type = typeof(DataModelList<RewardItemDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "url",
			Type = typeof(string)
		}
	};

	public int DataModelId => 356;

	public string DataModelDisplayName => "change_message_content";

	public string Title
	{
		get
		{
			return m_Title;
		}
		set
		{
			if (!(m_Title == value))
			{
				m_Title = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string BodyText
	{
		get
		{
			return m_BodyText;
		}
		set
		{
			if (!(m_BodyText == value))
			{
				m_BodyText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<RewardItemDataModel> ChangeMessageItemDisplay
	{
		get
		{
			return m_ChangeMessageItemDisplay;
		}
		set
		{
			if (m_ChangeMessageItemDisplay != value)
			{
				RemoveNestedDataModel(m_ChangeMessageItemDisplay);
				RegisterNestedDataModel(value);
				m_ChangeMessageItemDisplay = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Url
	{
		get
		{
			return m_Url;
		}
		set
		{
			if (!(m_Url == value))
			{
				m_Url = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public ChangeMessageContentDataModel()
	{
		RegisterNestedDataModel(m_ChangeMessageItemDisplay);
	}

	public int GetPropertiesHashCode()
	{
		return (((17 * 31 + ((m_Title != null) ? m_Title.GetHashCode() : 0)) * 31 + ((m_BodyText != null) ? m_BodyText.GetHashCode() : 0)) * 31 + ((m_ChangeMessageItemDisplay != null) ? m_ChangeMessageItemDisplay.GetPropertiesHashCode() : 0)) * 31 + ((m_Url != null) ? m_Url.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Title;
			return true;
		case 1:
			value = m_BodyText;
			return true;
		case 2:
			value = m_ChangeMessageItemDisplay;
			return true;
		case 3:
			value = m_Url;
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
			Title = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			BodyText = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			ChangeMessageItemDisplay = ((value != null) ? ((DataModelList<RewardItemDataModel>)value) : null);
			return true;
		case 3:
			Url = ((value != null) ? ((string)value) : null);
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
