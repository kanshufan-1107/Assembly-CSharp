using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class HeroDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 111;

	private CardDataModel m_HeroCard;

	private CardDataModel m_HeroPowerCard;

	private string m_Name;

	private string m_Description;

	private int m_CardID;

	private DataModelProperty[] m_properties = new DataModelProperty[5]
	{
		new DataModelProperty
		{
			PropertyId = 112,
			PropertyDisplayName = "hero_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 113,
			PropertyDisplayName = "hero_power_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 141,
			PropertyDisplayName = "name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 121,
			PropertyDisplayName = "description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 674,
			PropertyDisplayName = "card_id",
			Type = typeof(int)
		}
	};

	public int DataModelId => 111;

	public string DataModelDisplayName => "hero";

	public CardDataModel HeroCard
	{
		get
		{
			return m_HeroCard;
		}
		set
		{
			if (m_HeroCard != value)
			{
				RemoveNestedDataModel(m_HeroCard);
				RegisterNestedDataModel(value);
				m_HeroCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel HeroPowerCard
	{
		get
		{
			return m_HeroPowerCard;
		}
		set
		{
			if (m_HeroPowerCard != value)
			{
				RemoveNestedDataModel(m_HeroPowerCard);
				RegisterNestedDataModel(value);
				m_HeroPowerCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

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

	public int CardID
	{
		get
		{
			return m_CardID;
		}
		set
		{
			if (m_CardID != value)
			{
				m_CardID = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public HeroDataModel()
	{
		RegisterNestedDataModel(m_HeroCard);
		RegisterNestedDataModel(m_HeroPowerCard);
	}

	public int GetPropertiesHashCode()
	{
		int num = ((((17 * 31 + ((m_HeroCard != null) ? m_HeroCard.GetPropertiesHashCode() : 0)) * 31 + ((m_HeroPowerCard != null) ? m_HeroPowerCard.GetPropertiesHashCode() : 0)) * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0)) * 31 + ((m_Description != null) ? m_Description.GetHashCode() : 0)) * 31;
		_ = m_CardID;
		return num + m_CardID.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 112:
			value = m_HeroCard;
			return true;
		case 113:
			value = m_HeroPowerCard;
			return true;
		case 141:
			value = m_Name;
			return true;
		case 121:
			value = m_Description;
			return true;
		case 674:
			value = m_CardID;
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
		case 112:
			HeroCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 113:
			HeroPowerCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 141:
			Name = ((value != null) ? ((string)value) : null);
			return true;
		case 121:
			Description = ((value != null) ? ((string)value) : null);
			return true;
		case 674:
			CardID = ((value != null) ? ((int)value) : 0);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 112:
			info = Properties[0];
			return true;
		case 113:
			info = Properties[1];
			return true;
		case 141:
			info = Properties[2];
			return true;
		case 121:
			info = Properties[3];
			return true;
		case 674:
			info = Properties[4];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
