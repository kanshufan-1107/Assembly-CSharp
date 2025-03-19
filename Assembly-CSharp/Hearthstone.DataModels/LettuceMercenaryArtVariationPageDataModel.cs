using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceMercenaryArtVariationPageDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 448;

	private int m_ArtVatiationsOnPageCount;

	private bool m_ShowLeftArrow;

	private bool m_ShowRightArrow;

	private DataModelProperty[] m_properties = new DataModelProperty[3]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "art_variations_on_page_count",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "show_left_arrow",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "show_right_arrow",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 448;

	public string DataModelDisplayName => "lettuce_mercenary_art_variation_page";

	public int ArtVatiationsOnPageCount
	{
		get
		{
			return m_ArtVatiationsOnPageCount;
		}
		set
		{
			if (m_ArtVatiationsOnPageCount != value)
			{
				m_ArtVatiationsOnPageCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowLeftArrow
	{
		get
		{
			return m_ShowLeftArrow;
		}
		set
		{
			if (m_ShowLeftArrow != value)
			{
				m_ShowLeftArrow = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowRightArrow
	{
		get
		{
			return m_ShowRightArrow;
		}
		set
		{
			if (m_ShowRightArrow != value)
			{
				m_ShowRightArrow = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_ArtVatiationsOnPageCount;
		int num2 = (num + m_ArtVatiationsOnPageCount.GetHashCode()) * 31;
		_ = m_ShowLeftArrow;
		int num3 = (num2 + m_ShowLeftArrow.GetHashCode()) * 31;
		_ = m_ShowRightArrow;
		return num3 + m_ShowRightArrow.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_ArtVatiationsOnPageCount;
			return true;
		case 2:
			value = m_ShowLeftArrow;
			return true;
		case 3:
			value = m_ShowRightArrow;
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
			ArtVatiationsOnPageCount = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			ShowLeftArrow = value != null && (bool)value;
			return true;
		case 3:
			ShowRightArrow = value != null && (bool)value;
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
		case 2:
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
