using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ClassPreviewDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 908;

	private string m_Name;

	private string m_Description;

	private string m_Strengths;

	private string m_Weaknesses;

	private HeroDataModel m_Hero;

	private TAG_CLASS m_TagClass;

	private bool m_ShowClassWins;

	private DataModelProperty[] m_properties = new DataModelProperty[7]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "strengths",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "weaknesses",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "hero",
			Type = typeof(HeroDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "tag_class",
			Type = typeof(TAG_CLASS)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "show_class_wins",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 908;

	public string DataModelDisplayName => "class_preview";

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (!(m_Name == value))
			{
				m_Name = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Description
	{
		get
		{
			return m_Description;
		}
		set
		{
			if (!(m_Description == value))
			{
				m_Description = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Strengths
	{
		get
		{
			return m_Strengths;
		}
		set
		{
			if (!(m_Strengths == value))
			{
				m_Strengths = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Weaknesses
	{
		get
		{
			return m_Weaknesses;
		}
		set
		{
			if (!(m_Weaknesses == value))
			{
				m_Weaknesses = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public HeroDataModel Hero
	{
		get
		{
			return m_Hero;
		}
		set
		{
			if (m_Hero != value)
			{
				RemoveNestedDataModel(m_Hero);
				RegisterNestedDataModel(value);
				m_Hero = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TAG_CLASS TagClass
	{
		get
		{
			return m_TagClass;
		}
		set
		{
			if (m_TagClass != value)
			{
				m_TagClass = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowClassWins
	{
		get
		{
			return m_ShowClassWins;
		}
		set
		{
			if (m_ShowClassWins != value)
			{
				m_ShowClassWins = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public ClassPreviewDataModel()
	{
		RegisterNestedDataModel(m_Hero);
	}

	public int GetPropertiesHashCode()
	{
		int num = (((((17 * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0)) * 31 + ((m_Description != null) ? m_Description.GetHashCode() : 0)) * 31 + ((m_Strengths != null) ? m_Strengths.GetHashCode() : 0)) * 31 + ((m_Weaknesses != null) ? m_Weaknesses.GetHashCode() : 0)) * 31 + ((m_Hero != null) ? m_Hero.GetPropertiesHashCode() : 0)) * 31;
		_ = m_TagClass;
		int num2 = (num + m_TagClass.GetHashCode()) * 31;
		_ = m_ShowClassWins;
		return num2 + m_ShowClassWins.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Name;
			return true;
		case 1:
			value = m_Description;
			return true;
		case 2:
			value = m_Strengths;
			return true;
		case 3:
			value = m_Weaknesses;
			return true;
		case 4:
			value = m_Hero;
			return true;
		case 5:
			value = m_TagClass;
			return true;
		case 6:
			value = m_ShowClassWins;
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
			Name = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			Description = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			Strengths = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			Weaknesses = ((value != null) ? ((string)value) : null);
			return true;
		case 4:
			Hero = ((value != null) ? ((HeroDataModel)value) : null);
			return true;
		case 5:
			TagClass = ((value != null) ? ((TAG_CLASS)value) : TAG_CLASS.INVALID);
			return true;
		case 6:
			ShowClassWins = value != null && (bool)value;
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
