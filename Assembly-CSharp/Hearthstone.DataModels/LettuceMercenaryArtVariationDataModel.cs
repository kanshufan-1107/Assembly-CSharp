using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class LettuceMercenaryArtVariationDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 379;

	private int m_ArtVariationId;

	private CardDataModel m_Card;

	private bool m_Selected;

	private bool m_Unlocked;

	private bool m_NewlyUnlocked;

	private string m_LockedText;

	private DataModelProperty[] m_properties = new DataModelProperty[6]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "art_variation_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "selected",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "unlocked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "newly_unlocked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "locked_text",
			Type = typeof(string)
		}
	};

	public int DataModelId => 379;

	public string DataModelDisplayName => "lettuce_mercenary_art_variation";

	public int ArtVariationId
	{
		get
		{
			return m_ArtVariationId;
		}
		set
		{
			if (m_ArtVariationId != value)
			{
				m_ArtVariationId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel Card
	{
		get
		{
			return m_Card;
		}
		set
		{
			if (m_Card != value)
			{
				RemoveNestedDataModel(m_Card);
				RegisterNestedDataModel(value);
				m_Card = value;
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

	public bool Unlocked
	{
		get
		{
			return m_Unlocked;
		}
		set
		{
			if (m_Unlocked != value)
			{
				m_Unlocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool NewlyUnlocked
	{
		get
		{
			return m_NewlyUnlocked;
		}
		set
		{
			if (m_NewlyUnlocked != value)
			{
				m_NewlyUnlocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string LockedText
	{
		get
		{
			return m_LockedText;
		}
		set
		{
			if (!(m_LockedText == value))
			{
				m_LockedText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public LettuceMercenaryArtVariationDataModel()
	{
		RegisterNestedDataModel(m_Card);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_ArtVariationId;
		int num2 = ((num + m_ArtVariationId.GetHashCode()) * 31 + ((m_Card != null) ? m_Card.GetPropertiesHashCode() : 0)) * 31;
		_ = m_Selected;
		int num3 = (num2 + m_Selected.GetHashCode()) * 31;
		_ = m_Unlocked;
		int num4 = (num3 + m_Unlocked.GetHashCode()) * 31;
		_ = m_NewlyUnlocked;
		return (num4 + m_NewlyUnlocked.GetHashCode()) * 31 + ((m_LockedText != null) ? m_LockedText.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_ArtVariationId;
			return true;
		case 1:
			value = m_Card;
			return true;
		case 2:
			value = m_Selected;
			return true;
		case 3:
			value = m_Unlocked;
			return true;
		case 4:
			value = m_NewlyUnlocked;
			return true;
		case 5:
			value = m_LockedText;
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
			ArtVariationId = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			Card = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 2:
			Selected = value != null && (bool)value;
			return true;
		case 3:
			Unlocked = value != null && (bool)value;
			return true;
		case 4:
			NewlyUnlocked = value != null && (bool)value;
			return true;
		case 5:
			LockedText = ((value != null) ? ((string)value) : null);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
