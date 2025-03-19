using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.DataModels;

public class TextMessageContentDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 300;

	private string m_Title;

	private string m_IconType;

	private string m_BodyText;

	private Material m_ImageMaterial;

	private DataModelList<RewardItemDataModel> m_TextMessageItemDisplay = new DataModelList<RewardItemDataModel>();

	private Texture m_ImageTexture;

	private DataModelList<LettuceBountyDataModel> m_TextMessageItemBountyDisplay = new DataModelList<LettuceBountyDataModel>();

	private string m_Url;

	private DataModelProperty[] m_properties = new DataModelProperty[8]
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
			PropertyDisplayName = "icon_type",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "body_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "image_material",
			Type = typeof(Material)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "text_message_item_display",
			Type = typeof(DataModelList<RewardItemDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "image_texture",
			Type = typeof(Texture)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "text_message_item_bounty_display",
			Type = typeof(DataModelList<LettuceBountyDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "url",
			Type = typeof(string)
		}
	};

	public int DataModelId => 300;

	public string DataModelDisplayName => "text_message_content";

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

	public string IconType
	{
		get
		{
			return m_IconType;
		}
		set
		{
			if (!(m_IconType == value))
			{
				m_IconType = value;
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

	public Material ImageMaterial
	{
		get
		{
			return m_ImageMaterial;
		}
		set
		{
			if (!(m_ImageMaterial == value))
			{
				m_ImageMaterial = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<RewardItemDataModel> TextMessageItemDisplay
	{
		get
		{
			return m_TextMessageItemDisplay;
		}
		set
		{
			if (m_TextMessageItemDisplay != value)
			{
				RemoveNestedDataModel(m_TextMessageItemDisplay);
				RegisterNestedDataModel(value);
				m_TextMessageItemDisplay = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public Texture ImageTexture
	{
		get
		{
			return m_ImageTexture;
		}
		set
		{
			if (!(m_ImageTexture == value))
			{
				m_ImageTexture = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<LettuceBountyDataModel> TextMessageItemBountyDisplay
	{
		get
		{
			return m_TextMessageItemBountyDisplay;
		}
		set
		{
			if (m_TextMessageItemBountyDisplay != value)
			{
				RemoveNestedDataModel(m_TextMessageItemBountyDisplay);
				RegisterNestedDataModel(value);
				m_TextMessageItemBountyDisplay = value;
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

	public TextMessageContentDataModel()
	{
		RegisterNestedDataModel(m_TextMessageItemDisplay);
		RegisterNestedDataModel(m_TextMessageItemBountyDisplay);
	}

	public int GetPropertiesHashCode()
	{
		return (((((((17 * 31 + ((m_Title != null) ? m_Title.GetHashCode() : 0)) * 31 + ((m_IconType != null) ? m_IconType.GetHashCode() : 0)) * 31 + ((m_BodyText != null) ? m_BodyText.GetHashCode() : 0)) * 31 + ((m_ImageMaterial != null) ? m_ImageMaterial.GetHashCode() : 0)) * 31 + ((m_TextMessageItemDisplay != null) ? m_TextMessageItemDisplay.GetPropertiesHashCode() : 0)) * 31 + ((m_ImageTexture != null) ? m_ImageTexture.GetHashCode() : 0)) * 31 + ((m_TextMessageItemBountyDisplay != null) ? m_TextMessageItemBountyDisplay.GetPropertiesHashCode() : 0)) * 31 + ((m_Url != null) ? m_Url.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Title;
			return true;
		case 1:
			value = m_IconType;
			return true;
		case 2:
			value = m_BodyText;
			return true;
		case 3:
			value = m_ImageMaterial;
			return true;
		case 4:
			value = m_TextMessageItemDisplay;
			return true;
		case 5:
			value = m_ImageTexture;
			return true;
		case 6:
			value = m_TextMessageItemBountyDisplay;
			return true;
		case 7:
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
			IconType = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			BodyText = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			ImageMaterial = ((value != null) ? ((Material)value) : null);
			return true;
		case 4:
			TextMessageItemDisplay = ((value != null) ? ((DataModelList<RewardItemDataModel>)value) : null);
			return true;
		case 5:
			ImageTexture = ((value != null) ? ((Texture)value) : null);
			return true;
		case 6:
			TextMessageItemBountyDisplay = ((value != null) ? ((DataModelList<LettuceBountyDataModel>)value) : null);
			return true;
		case 7:
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
