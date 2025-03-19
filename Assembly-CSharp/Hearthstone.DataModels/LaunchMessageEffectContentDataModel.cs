using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.DataModels;

public class LaunchMessageEffectContentDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 348;

	private string m_EffectID;

	private Color m_EffectColor;

	private string m_EffectSoundID;

	private DataModelProperty[] m_properties = new DataModelProperty[3]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "effect_id",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "effect_color",
			Type = typeof(Color)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "effect_sound_id",
			Type = typeof(string)
		}
	};

	public int DataModelId => 348;

	public string DataModelDisplayName => "launch_message_effect_content";

	public string EffectID
	{
		get
		{
			return m_EffectID;
		}
		set
		{
			if (!(m_EffectID == value))
			{
				m_EffectID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public Color EffectColor
	{
		get
		{
			return m_EffectColor;
		}
		set
		{
			if (!(m_EffectColor == value))
			{
				m_EffectColor = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string EffectSoundID
	{
		get
		{
			return m_EffectSoundID;
		}
		set
		{
			if (!(m_EffectSoundID == value))
			{
				m_EffectSoundID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = (17 * 31 + ((m_EffectID != null) ? m_EffectID.GetHashCode() : 0)) * 31;
		_ = m_EffectColor;
		return (num + m_EffectColor.GetHashCode()) * 31 + ((m_EffectSoundID != null) ? m_EffectSoundID.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_EffectID;
			return true;
		case 1:
			value = m_EffectColor;
			return true;
		case 2:
			value = m_EffectSoundID;
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
			EffectID = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			EffectColor = ((value != null) ? ((Color)value) : default(Color));
			return true;
		case 2:
			EffectSoundID = ((value != null) ? ((string)value) : null);
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
