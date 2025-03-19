using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettucePlayDisplayDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 256;

	private int m_ChestCurrentWins;

	private int m_ChestMaxWins;

	private float m_ChestProgressPercent;

	private string m_ChestProgressBarText;

	private int m_Rating;

	private int m_HighRatingTierIndex;

	private DataModelProperty[] m_properties = new DataModelProperty[6]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "chest_current_wins",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "chest_max_wins",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "chest_progress_percent",
			Type = typeof(float)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "chest_progress_bar_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "rating",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "high_rating_tier_index",
			Type = typeof(int)
		}
	};

	public int DataModelId => 256;

	public string DataModelDisplayName => "lettuce_play_display";

	public int ChestCurrentWins
	{
		get
		{
			return m_ChestCurrentWins;
		}
		set
		{
			if (m_ChestCurrentWins != value)
			{
				m_ChestCurrentWins = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int ChestMaxWins
	{
		get
		{
			return m_ChestMaxWins;
		}
		set
		{
			if (m_ChestMaxWins != value)
			{
				m_ChestMaxWins = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public float ChestProgressPercent
	{
		get
		{
			return m_ChestProgressPercent;
		}
		set
		{
			if (m_ChestProgressPercent != value)
			{
				m_ChestProgressPercent = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ChestProgressBarText
	{
		get
		{
			return m_ChestProgressBarText;
		}
		set
		{
			if (!(m_ChestProgressBarText == value))
			{
				m_ChestProgressBarText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Rating
	{
		get
		{
			return m_Rating;
		}
		set
		{
			if (m_Rating != value)
			{
				m_Rating = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HighRatingTierIndex
	{
		get
		{
			return m_HighRatingTierIndex;
		}
		set
		{
			if (m_HighRatingTierIndex != value)
			{
				m_HighRatingTierIndex = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_ChestCurrentWins;
		int num2 = (num + m_ChestCurrentWins.GetHashCode()) * 31;
		_ = m_ChestMaxWins;
		int num3 = (num2 + m_ChestMaxWins.GetHashCode()) * 31;
		_ = m_ChestProgressPercent;
		int num4 = ((num3 + m_ChestProgressPercent.GetHashCode()) * 31 + ((m_ChestProgressBarText != null) ? m_ChestProgressBarText.GetHashCode() : 0)) * 31;
		_ = m_Rating;
		int num5 = (num4 + m_Rating.GetHashCode()) * 31;
		_ = m_HighRatingTierIndex;
		return num5 + m_HighRatingTierIndex.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_ChestCurrentWins;
			return true;
		case 1:
			value = m_ChestMaxWins;
			return true;
		case 3:
			value = m_ChestProgressPercent;
			return true;
		case 4:
			value = m_ChestProgressBarText;
			return true;
		case 5:
			value = m_Rating;
			return true;
		case 6:
			value = m_HighRatingTierIndex;
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
			ChestCurrentWins = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			ChestMaxWins = ((value != null) ? ((int)value) : 0);
			return true;
		case 3:
			ChestProgressPercent = ((value != null) ? ((float)value) : 0f);
			return true;
		case 4:
			ChestProgressBarText = ((value != null) ? ((string)value) : null);
			return true;
		case 5:
			Rating = ((value != null) ? ((int)value) : 0);
			return true;
		case 6:
			HighRatingTierIndex = ((value != null) ? ((int)value) : 0);
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
		case 4:
			info = Properties[3];
			return true;
		case 5:
			info = Properties[4];
			return true;
		case 6:
			info = Properties[5];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
