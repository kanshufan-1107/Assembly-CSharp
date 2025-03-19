using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.DataModels;

public class ShopMessageContentDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 301;

	private string m_Title;

	private string m_BodyText;

	private Texture m_ImageTexture;

	private DataModelProperty[] m_properties = new DataModelProperty[3]
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
			PropertyId = 3,
			PropertyDisplayName = "image_texture",
			Type = typeof(Texture)
		}
	};

	public int DataModelId => 301;

	public string DataModelDisplayName => "shop_message_content";

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

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		return ((17 * 31 + ((m_Title != null) ? m_Title.GetHashCode() : 0)) * 31 + ((m_BodyText != null) ? m_BodyText.GetHashCode() : 0)) * 31 + ((m_ImageTexture != null) ? m_ImageTexture.GetHashCode() : 0);
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
		case 3:
			value = m_ImageTexture;
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
		case 3:
			ImageTexture = ((value != null) ? ((Texture)value) : null);
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
		case 3:
			info = Properties[2];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
