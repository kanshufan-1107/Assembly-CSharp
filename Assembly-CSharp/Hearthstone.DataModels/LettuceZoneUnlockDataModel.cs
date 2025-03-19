using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.DataModels;

public class LettuceZoneUnlockDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 567;

	private Texture m_ZoneTexture;

	private string m_FooterText;

	private string m_ZoneNameText;

	private DataModelProperty[] m_properties = new DataModelProperty[3]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "zone_texture",
			Type = typeof(Texture)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "footer_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "zone_name_text",
			Type = typeof(string)
		}
	};

	public int DataModelId => 567;

	public string DataModelDisplayName => "lettuce_zone_unlock";

	public Texture ZoneTexture
	{
		get
		{
			return m_ZoneTexture;
		}
		set
		{
			if (!(m_ZoneTexture == value))
			{
				m_ZoneTexture = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string FooterText
	{
		get
		{
			return m_FooterText;
		}
		set
		{
			if (!(m_FooterText == value))
			{
				m_FooterText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ZoneNameText
	{
		get
		{
			return m_ZoneNameText;
		}
		set
		{
			if (!(m_ZoneNameText == value))
			{
				m_ZoneNameText = value;
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
		return ((17 * 31 + ((m_ZoneTexture != null) ? m_ZoneTexture.GetHashCode() : 0)) * 31 + ((m_FooterText != null) ? m_FooterText.GetHashCode() : 0)) * 31 + ((m_ZoneNameText != null) ? m_ZoneNameText.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_ZoneTexture;
			return true;
		case 1:
			value = m_FooterText;
			return true;
		case 2:
			value = m_ZoneNameText;
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
			ZoneTexture = ((value != null) ? ((Texture)value) : null);
			return true;
		case 1:
			FooterText = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			ZoneNameText = ((value != null) ? ((string)value) : null);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
