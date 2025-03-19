using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class DeckDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 648;

	private string m_Name;

	private CardBackDataModel m_CardBack;

	private bool m_RandomCardBackFavoritesOnly;

	private HeroDataModel m_Hero;

	private bool m_HeroOverride;

	private DataModelList<CardDataModel> m_Cards = new DataModelList<CardDataModel>();

	private bool m_DraggingDeckAssignment;

	private bool m_RandomHeroFavoritesOnly;

	private CardDataModel m_CosmeticCoin;

	private bool m_RandomCoinFavoritesOnly;

	private DataModelProperty[] m_properties = new DataModelProperty[10]
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
			PropertyDisplayName = "card_back",
			Type = typeof(CardBackDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "random_card_back_favorites_only",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "hero",
			Type = typeof(HeroDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "hero_override",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "cards",
			Type = typeof(DataModelList<CardDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "dragging_deck_assignment",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "random_hero_favorites_only",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "cosmetic_coin",
			Type = typeof(CardDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "random_coin_favorites_only",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 648;

	public string DataModelDisplayName => "deck";

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

	public CardBackDataModel CardBack
	{
		get
		{
			return m_CardBack;
		}
		set
		{
			if (m_CardBack != value)
			{
				RemoveNestedDataModel(m_CardBack);
				RegisterNestedDataModel(value);
				m_CardBack = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool RandomCardBackFavoritesOnly
	{
		get
		{
			return m_RandomCardBackFavoritesOnly;
		}
		set
		{
			if (m_RandomCardBackFavoritesOnly != value)
			{
				m_RandomCardBackFavoritesOnly = value;
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

	public bool HeroOverride
	{
		get
		{
			return m_HeroOverride;
		}
		set
		{
			if (m_HeroOverride != value)
			{
				m_HeroOverride = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<CardDataModel> Cards
	{
		get
		{
			return m_Cards;
		}
		set
		{
			if (m_Cards != value)
			{
				RemoveNestedDataModel(m_Cards);
				RegisterNestedDataModel(value);
				m_Cards = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool DraggingDeckAssignment
	{
		get
		{
			return m_DraggingDeckAssignment;
		}
		set
		{
			if (m_DraggingDeckAssignment != value)
			{
				m_DraggingDeckAssignment = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool RandomHeroFavoritesOnly
	{
		get
		{
			return m_RandomHeroFavoritesOnly;
		}
		set
		{
			if (m_RandomHeroFavoritesOnly != value)
			{
				m_RandomHeroFavoritesOnly = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public CardDataModel CosmeticCoin
	{
		get
		{
			return m_CosmeticCoin;
		}
		set
		{
			if (m_CosmeticCoin != value)
			{
				RemoveNestedDataModel(m_CosmeticCoin);
				RegisterNestedDataModel(value);
				m_CosmeticCoin = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool RandomCoinFavoritesOnly
	{
		get
		{
			return m_RandomCoinFavoritesOnly;
		}
		set
		{
			if (m_RandomCoinFavoritesOnly != value)
			{
				m_RandomCoinFavoritesOnly = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public DeckDataModel()
	{
		RegisterNestedDataModel(m_CardBack);
		RegisterNestedDataModel(m_Hero);
		RegisterNestedDataModel(m_Cards);
		RegisterNestedDataModel(m_CosmeticCoin);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0);
		if (m_CardBack != null && !inspectedDataModels.Contains(m_CardBack.GetHashCode()))
		{
			inspectedDataModels.Add(m_CardBack.GetHashCode());
			hash = hash * 31 + m_CardBack.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_RandomCardBackFavoritesOnly;
		hash = num + m_RandomCardBackFavoritesOnly.GetHashCode();
		if (m_Hero != null && !inspectedDataModels.Contains(m_Hero.GetHashCode()))
		{
			inspectedDataModels.Add(m_Hero.GetHashCode());
			hash = hash * 31 + m_Hero.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num2 = hash * 31;
		_ = m_HeroOverride;
		hash = num2 + m_HeroOverride.GetHashCode();
		if (m_Cards != null && !inspectedDataModels.Contains(m_Cards.GetHashCode()))
		{
			inspectedDataModels.Add(m_Cards.GetHashCode());
			hash = hash * 31 + m_Cards.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num3 = hash * 31;
		_ = m_DraggingDeckAssignment;
		hash = num3 + m_DraggingDeckAssignment.GetHashCode();
		int num4 = hash * 31;
		_ = m_RandomHeroFavoritesOnly;
		hash = num4 + m_RandomHeroFavoritesOnly.GetHashCode();
		if (m_CosmeticCoin != null && !inspectedDataModels.Contains(m_CosmeticCoin.GetHashCode()))
		{
			inspectedDataModels.Add(m_CosmeticCoin.GetHashCode());
			hash = hash * 31 + m_CosmeticCoin.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num5 = hash * 31;
		_ = m_RandomCoinFavoritesOnly;
		return num5 + m_RandomCoinFavoritesOnly.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Name;
			return true;
		case 1:
			value = m_CardBack;
			return true;
		case 2:
			value = m_RandomCardBackFavoritesOnly;
			return true;
		case 3:
			value = m_Hero;
			return true;
		case 4:
			value = m_HeroOverride;
			return true;
		case 5:
			value = m_Cards;
			return true;
		case 6:
			value = m_DraggingDeckAssignment;
			return true;
		case 7:
			value = m_RandomHeroFavoritesOnly;
			return true;
		case 8:
			value = m_CosmeticCoin;
			return true;
		case 9:
			value = m_RandomCoinFavoritesOnly;
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
			CardBack = ((value != null) ? ((CardBackDataModel)value) : null);
			return true;
		case 2:
			RandomCardBackFavoritesOnly = value != null && (bool)value;
			return true;
		case 3:
			Hero = ((value != null) ? ((HeroDataModel)value) : null);
			return true;
		case 4:
			HeroOverride = value != null && (bool)value;
			return true;
		case 5:
			Cards = ((value != null) ? ((DataModelList<CardDataModel>)value) : null);
			return true;
		case 6:
			DraggingDeckAssignment = value != null && (bool)value;
			return true;
		case 7:
			RandomHeroFavoritesOnly = value != null && (bool)value;
			return true;
		case 8:
			CosmeticCoin = ((value != null) ? ((CardDataModel)value) : null);
			return true;
		case 9:
			RandomCoinFavoritesOnly = value != null && (bool)value;
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
		case 7:
			info = Properties[7];
			return true;
		case 8:
			info = Properties[8];
			return true;
		case 9:
			info = Properties[9];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
