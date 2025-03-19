using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.DataModels;

public class BoxProductBannerDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 1125;

	private string m_HeaderText;

	private string m_ButtonText;

	private Color m_BannerColor;

	private Texture m_ImageTexture;

	private Texture m_SecondaryImageTexture;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "header_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "button_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "banner_color",
			Type = typeof(Color)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "image_texture",
			Type = typeof(Texture)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "secondary_image_texture",
			Type = typeof(Texture)
		}
	};

	public int DataModelId => 1125;

	public string DataModelDisplayName => "BoxProductBanner";

	public string HeaderText
	{
		get
		{
			return m_HeaderText;
		}
		set
		{
			if (!(m_HeaderText == value))
			{
				m_HeaderText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ButtonText
	{
		get
		{
			return m_ButtonText;
		}
		set
		{
			if (!(m_ButtonText == value))
			{
				m_ButtonText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public Color BannerColor
	{
		get
		{
			return m_BannerColor;
		}
		set
		{
			if (!(m_BannerColor == value))
			{
				m_BannerColor = value;
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

	public Texture SecondaryImageTexture
	{
		get
		{
			return m_SecondaryImageTexture;
		}
		set
		{
			if (!(m_SecondaryImageTexture == value))
			{
				m_SecondaryImageTexture = value;
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
		int num = ((17 * 31 + ((m_HeaderText != null) ? m_HeaderText.GetHashCode() : 0)) * 31 + ((m_ButtonText != null) ? m_ButtonText.GetHashCode() : 0)) * 31;
		_ = m_BannerColor;
		return ((num + m_BannerColor.GetHashCode()) * 31 + ((m_ImageTexture != null) ? m_ImageTexture.GetHashCode() : 0)) * 31 + ((m_SecondaryImageTexture != null) ? m_SecondaryImageTexture.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_HeaderText;
			return true;
		case 1:
			value = m_ButtonText;
			return true;
		case 2:
			value = m_BannerColor;
			return true;
		case 4:
			value = m_ImageTexture;
			return true;
		case 5:
			value = m_SecondaryImageTexture;
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
			HeaderText = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			ButtonText = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			BannerColor = ((value != null) ? ((Color)value) : default(Color));
			return true;
		case 4:
			ImageTexture = ((value != null) ? ((Texture)value) : null);
			return true;
		case 5:
			SecondaryImageTexture = ((value != null) ? ((Texture)value) : null);
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
		case 4:
			info = Properties[3];
			return true;
		case 5:
			info = Properties[4];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
