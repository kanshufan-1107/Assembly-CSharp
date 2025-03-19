using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class TwistHeroicDeckDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 943;

	private string m_Name;

	private CardDataModel m_HeroCard;

	private bool m_IsDeckLocked;

	private string m_RequiredDescription;

	private CardDataModel m_RequiredCard;

	private int m_CardCount;

	private CardDataModel m_PassiveCard;

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
			PropertyDisplayName = "hero_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "is_deck_locked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "required_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "required_card",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "card_count",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "passive_card",
			Type = typeof(CardDataModel)
		}
	};

	public int DataModelId => 943;

	public string DataModelDisplayName => "twist_heroic_deck";

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

	public bool IsDeckLocked
	{
		get
		{
			return m_IsDeckLocked;
		}
		set
		{
			if (m_IsDeckLocked != value)
			{
				m_IsDeckLocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string RequiredDescription
	{
		get
		{
			return m_RequiredDescription;
		}
		set
		{
			if (!(m_RequiredDescription == value))
			{
				m_RequiredDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel RequiredCard
	{
		get
		{
			return m_RequiredCard;
		}
		set
		{
			if (m_RequiredCard != value)
			{
				RemoveNestedDataModel(m_RequiredCard);
				RegisterNestedDataModel(value);
				m_RequiredCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int CardCount
	{
		get
		{
			return m_CardCount;
		}
		set
		{
			if (m_CardCount != value)
			{
				m_CardCount = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel PassiveCard
	{
		get
		{
			return m_PassiveCard;
		}
		set
		{
			if (m_PassiveCard != value)
			{
				RemoveNestedDataModel(m_PassiveCard);
				RegisterNestedDataModel(value);
				m_PassiveCard = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public TwistHeroicDeckDataModel()
	{
		RegisterNestedDataModel(m_HeroCard);
		RegisterNestedDataModel(m_RequiredCard);
		RegisterNestedDataModel(m_PassiveCard);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0);
		if (m_HeroCard != null && !inspectedDataModels.Contains(m_HeroCard.GetHashCode()))
		{
			inspectedDataModels.Add(m_HeroCard.GetHashCode());
			hash = hash * 31 + m_HeroCard.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_IsDeckLocked;
		hash = num + m_IsDeckLocked.GetHashCode();
		hash = hash * 31 + ((m_RequiredDescription != null) ? m_RequiredDescription.GetHashCode() : 0);
		if (m_RequiredCard != null && !inspectedDataModels.Contains(m_RequiredCard.GetHashCode()))
		{
			inspectedDataModels.Add(m_RequiredCard.GetHashCode());
			hash = hash * 31 + m_RequiredCard.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num2 = hash * 31;
		_ = m_CardCount;
		hash = num2 + m_CardCount.GetHashCode();
		if (m_PassiveCard != null && !inspectedDataModels.Contains(m_PassiveCard.GetHashCode()))
		{
			inspectedDataModels.Add(m_PassiveCard.GetHashCode());
			return hash * 31 + m_PassiveCard.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Name;
			return true;
		case 1:
			value = m_HeroCard;
			return true;
		case 2:
			value = m_IsDeckLocked;
			return true;
		case 3:
			value = m_RequiredDescription;
			return true;
		case 4:
			value = m_RequiredCard;
			return true;
		case 5:
			value = m_CardCount;
			return true;
		case 6:
			value = m_PassiveCard;
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
			HeroCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 2:
			IsDeckLocked = value != null && (bool)value;
			return true;
		case 3:
			RequiredDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 4:
			RequiredCard = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 5:
			CardCount = ((value != null) ? ((int)value) : 0);
			return true;
		case 6:
			PassiveCard = ((value != null) ? ((CardDataModel)value) : null);
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
