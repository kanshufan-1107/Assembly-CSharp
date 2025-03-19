using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class CardTileDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 262;

	private string m_CardId;

	private TAG_PREMIUM m_Premium;

	private int m_Count;

	private bool m_Selected;

	private CollectionDeckTileActor.GhostedState m_ForceGhostDisplayStyle;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 263,
			PropertyDisplayName = "id",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 264,
			PropertyDisplayName = "premium",
			Type = typeof(TAG_PREMIUM)
		},
		new DataModelProperty
		{
			PropertyId = 265,
			PropertyDisplayName = "count",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 277,
			PropertyDisplayName = "selected",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 740,
			PropertyDisplayName = "force_ghost_display_style",
			Type = typeof(CollectionDeckTileActor.GhostedState)
		}
	};

	public int DataModelId => 262;

	public string DataModelDisplayName => "card_tile";

	public string CardId
	{
		get
		{
			return m_CardId;
		}
		set
		{
			if (!(m_CardId == value))
			{
				m_CardId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TAG_PREMIUM Premium
	{
		get
		{
			return m_Premium;
		}
		set
		{
			if (m_Premium != value)
			{
				m_Premium = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Count
	{
		get
		{
			return m_Count;
		}
		set
		{
			if (m_Count != value)
			{
				m_Count = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Selected
	{
		get
		{
			return m_Selected;
		}
		set
		{
			if (m_Selected != value)
			{
				m_Selected = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CollectionDeckTileActor.GhostedState ForceGhostDisplayStyle
	{
		get
		{
			return m_ForceGhostDisplayStyle;
		}
		set
		{
			if (m_ForceGhostDisplayStyle != value)
			{
				m_ForceGhostDisplayStyle = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = (17 * 31 + ((m_CardId != null) ? m_CardId.GetHashCode() : 0)) * 31;
		_ = m_Premium;
		int num2 = (num + m_Premium.GetHashCode()) * 31;
		_ = m_Count;
		int num3 = (num2 + m_Count.GetHashCode()) * 31;
		_ = m_Selected;
		int num4 = (num3 + m_Selected.GetHashCode()) * 31;
		_ = m_ForceGhostDisplayStyle;
		return num4 + m_ForceGhostDisplayStyle.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 263:
			value = m_CardId;
			return true;
		case 264:
			value = m_Premium;
			return true;
		case 265:
			value = m_Count;
			return true;
		case 277:
			value = m_Selected;
			return true;
		case 740:
			value = m_ForceGhostDisplayStyle;
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
		case 263:
			CardId = ((value != null) ? ((string)value) : null);
			return true;
		case 264:
			Premium = ((value != null) ? ((TAG_PREMIUM)value) : TAG_PREMIUM.NORMAL);
			return true;
		case 265:
			Count = ((value != null) ? ((int)value) : 0);
			return true;
		case 277:
			Selected = value != null && (bool)value;
			return true;
		case 740:
			ForceGhostDisplayStyle = ((value != null) ? ((CollectionDeckTileActor.GhostedState)value) : CollectionDeckTileActor.GhostedState.NONE);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 263:
			info = Properties[0];
			return true;
		case 264:
			info = Properties[1];
			return true;
		case 265:
			info = Properties[2];
			return true;
		case 277:
			info = Properties[3];
			return true;
		case 740:
			info = Properties[4];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
