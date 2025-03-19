using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.DataModels;

public class LaunchMessageContentDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 341;

	private string m_Title;

	private string m_IconType;

	private LaunchMessageEffectContentDataModel m_LaunchEffect;

	private Material m_ImageMaterial;

	private Texture m_Texture;

	private string m_SubLayout;

	private string m_Url;

	private DataModelProperty[] m_properties = new DataModelProperty[7]
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
			PropertyDisplayName = "launcheffect",
			Type = typeof(LaunchMessageEffectContentDataModel)
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
			PropertyDisplayName = "texture",
			Type = typeof(Texture)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "sub_layout",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "url",
			Type = typeof(string)
		}
	};

	public int DataModelId => 341;

	public string DataModelDisplayName => "launch_message_content";

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

	public LaunchMessageEffectContentDataModel LaunchEffect
	{
		get
		{
			return m_LaunchEffect;
		}
		set
		{
			if (m_LaunchEffect != value)
			{
				RemoveNestedDataModel(m_LaunchEffect);
				RegisterNestedDataModel(value);
				m_LaunchEffect = value;
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

	public Texture Texture
	{
		get
		{
			return m_Texture;
		}
		set
		{
			if (!(m_Texture == value))
			{
				m_Texture = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string SubLayout
	{
		get
		{
			return m_SubLayout;
		}
		set
		{
			if (!(m_SubLayout == value))
			{
				m_SubLayout = value;
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

	public LaunchMessageContentDataModel()
	{
		RegisterNestedDataModel(m_LaunchEffect);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_Title != null) ? m_Title.GetHashCode() : 0);
		hash = hash * 31 + ((m_IconType != null) ? m_IconType.GetHashCode() : 0);
		if (m_LaunchEffect != null && !inspectedDataModels.Contains(m_LaunchEffect.GetHashCode()))
		{
			inspectedDataModels.Add(m_LaunchEffect.GetHashCode());
			hash = hash * 31 + m_LaunchEffect.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		hash = hash * 31 + ((m_ImageMaterial != null) ? m_ImageMaterial.GetHashCode() : 0);
		hash = hash * 31 + ((m_Texture != null) ? m_Texture.GetHashCode() : 0);
		hash = hash * 31 + ((m_SubLayout != null) ? m_SubLayout.GetHashCode() : 0);
		return hash * 31 + ((m_Url != null) ? m_Url.GetHashCode() : 0);
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
			value = m_LaunchEffect;
			return true;
		case 3:
			value = m_ImageMaterial;
			return true;
		case 4:
			value = m_Texture;
			return true;
		case 5:
			value = m_SubLayout;
			return true;
		case 6:
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
			LaunchEffect = ((value != null) ? ((LaunchMessageEffectContentDataModel)value) : null);
			return true;
		case 3:
			ImageMaterial = ((value != null) ? ((Material)value) : null);
			return true;
		case 4:
			Texture = ((value != null) ? ((Texture)value) : null);
			return true;
		case 5:
			SubLayout = ((value != null) ? ((string)value) : null);
			return true;
		case 6:
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
