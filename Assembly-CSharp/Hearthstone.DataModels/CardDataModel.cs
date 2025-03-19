using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class CardDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 27;

	private string m_CardId;

	private TAG_PREMIUM m_Premium;

	private string m_FlavorText;

	private int m_Attack;

	private int m_Health;

	private int m_Mana;

	private int m_Cooldown;

	private DataModelList<SpellType> m_SpellTypes = new DataModelList<SpellType>();

	private string m_Name;

	private bool m_Owned;

	private DataModelList<GameTagValueDataModel> m_GameTagOverrides = new DataModelList<GameTagValueDataModel>();

	private string m_ArtistCredit;

	private string m_Rarity;

	private string m_CardText;

	private bool m_IsShopPremiumHeroSkin;

	private TAG_CLASS m_Class;

	private DataModelProperty[] m_properties = new DataModelProperty[16]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "id",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "premium",
			Type = typeof(TAG_PREMIUM)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "flavor_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "attack",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "health",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "mana",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "cooldown",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "spell_types",
			Type = typeof(DataModelList<SpellType>)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "owned",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "game_tag_overrides",
			Type = typeof(DataModelList<GameTagValueDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "artist_credit",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 12,
			PropertyDisplayName = "rarity",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 13,
			PropertyDisplayName = "card_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 14,
			PropertyDisplayName = "is_shop_premium_hero_skin",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 15,
			PropertyDisplayName = "class",
			Type = typeof(TAG_CLASS)
		}
	};

	public int DataModelId => 27;

	public string DataModelDisplayName => "card";

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

	public string FlavorText
	{
		get
		{
			return m_FlavorText;
		}
		set
		{
			if (!(m_FlavorText == value))
			{
				m_FlavorText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Attack
	{
		get
		{
			return m_Attack;
		}
		set
		{
			if (m_Attack != value)
			{
				m_Attack = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Health
	{
		get
		{
			return m_Health;
		}
		set
		{
			if (m_Health != value)
			{
				m_Health = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Mana
	{
		get
		{
			return m_Mana;
		}
		set
		{
			if (m_Mana != value)
			{
				m_Mana = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Cooldown
	{
		get
		{
			return m_Cooldown;
		}
		set
		{
			if (m_Cooldown != value)
			{
				m_Cooldown = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<SpellType> SpellTypes
	{
		get
		{
			return m_SpellTypes;
		}
		set
		{
			if (m_SpellTypes != value)
			{
				RemoveNestedDataModel(m_SpellTypes);
				RegisterNestedDataModel(value);
				m_SpellTypes = value;
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

	public bool Owned
	{
		get
		{
			return m_Owned;
		}
		set
		{
			if (m_Owned != value)
			{
				m_Owned = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<GameTagValueDataModel> GameTagOverrides
	{
		get
		{
			return m_GameTagOverrides;
		}
		set
		{
			if (m_GameTagOverrides != value)
			{
				RemoveNestedDataModel(m_GameTagOverrides);
				RegisterNestedDataModel(value);
				m_GameTagOverrides = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ArtistCredit
	{
		get
		{
			return m_ArtistCredit;
		}
		set
		{
			if (!(m_ArtistCredit == value))
			{
				m_ArtistCredit = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Rarity
	{
		get
		{
			return m_Rarity;
		}
		set
		{
			if (!(m_Rarity == value))
			{
				m_Rarity = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string CardText
	{
		get
		{
			return m_CardText;
		}
		set
		{
			if (!(m_CardText == value))
			{
				m_CardText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsShopPremiumHeroSkin
	{
		get
		{
			return m_IsShopPremiumHeroSkin;
		}
		set
		{
			if (m_IsShopPremiumHeroSkin != value)
			{
				m_IsShopPremiumHeroSkin = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public TAG_CLASS Class
	{
		get
		{
			return m_Class;
		}
		set
		{
			if (m_Class != value)
			{
				m_Class = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public CardDataModel()
	{
		RegisterNestedDataModel(m_SpellTypes);
		RegisterNestedDataModel(m_GameTagOverrides);
	}

	public int GetPropertiesHashCode()
	{
		int num = (17 * 31 + ((m_CardId != null) ? m_CardId.GetHashCode() : 0)) * 31;
		_ = m_Premium;
		int num2 = ((num + m_Premium.GetHashCode()) * 31 + ((m_FlavorText != null) ? m_FlavorText.GetHashCode() : 0)) * 31;
		_ = m_Attack;
		int num3 = (num2 + m_Attack.GetHashCode()) * 31;
		_ = m_Health;
		int num4 = (num3 + m_Health.GetHashCode()) * 31;
		_ = m_Mana;
		int num5 = (num4 + m_Mana.GetHashCode()) * 31;
		_ = m_Cooldown;
		int num6 = (((num5 + m_Cooldown.GetHashCode()) * 31 + ((m_SpellTypes != null) ? m_SpellTypes.GetPropertiesHashCode() : 0)) * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0)) * 31;
		_ = m_Owned;
		int num7 = (((((num6 + m_Owned.GetHashCode()) * 31 + ((m_GameTagOverrides != null) ? m_GameTagOverrides.GetPropertiesHashCode() : 0)) * 31 + ((m_ArtistCredit != null) ? m_ArtistCredit.GetHashCode() : 0)) * 31 + ((m_Rarity != null) ? m_Rarity.GetHashCode() : 0)) * 31 + ((m_CardText != null) ? m_CardText.GetHashCode() : 0)) * 31;
		_ = m_IsShopPremiumHeroSkin;
		int num8 = (num7 + m_IsShopPremiumHeroSkin.GetHashCode()) * 31;
		_ = m_Class;
		return num8 + m_Class.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_CardId;
			return true;
		case 1:
			value = m_Premium;
			return true;
		case 2:
			value = m_FlavorText;
			return true;
		case 3:
			value = m_Attack;
			return true;
		case 4:
			value = m_Health;
			return true;
		case 5:
			value = m_Mana;
			return true;
		case 9:
			value = m_Cooldown;
			return true;
		case 6:
			value = m_SpellTypes;
			return true;
		case 7:
			value = m_Name;
			return true;
		case 8:
			value = m_Owned;
			return true;
		case 10:
			value = m_GameTagOverrides;
			return true;
		case 11:
			value = m_ArtistCredit;
			return true;
		case 12:
			value = m_Rarity;
			return true;
		case 13:
			value = m_CardText;
			return true;
		case 14:
			value = m_IsShopPremiumHeroSkin;
			return true;
		case 15:
			value = m_Class;
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
			CardId = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			Premium = ((value != null) ? ((TAG_PREMIUM)value) : TAG_PREMIUM.NORMAL);
			return true;
		case 2:
			FlavorText = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			Attack = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			Health = ((value != null) ? ((int)value) : 0);
			return true;
		case 5:
			Mana = ((value != null) ? ((int)value) : 0);
			return true;
		case 9:
			Cooldown = ((value != null) ? ((int)value) : 0);
			return true;
		case 6:
			SpellTypes = ((value != null) ? ((DataModelList<SpellType>)value) : null);
			return true;
		case 7:
			Name = ((value != null) ? ((string)value) : null);
			return true;
		case 8:
			Owned = value != null && (bool)value;
			return true;
		case 10:
			GameTagOverrides = ((value != null) ? ((DataModelList<GameTagValueDataModel>)value) : null);
			return true;
		case 11:
			ArtistCredit = ((value != null) ? ((string)value) : null);
			return true;
		case 12:
			Rarity = ((value != null) ? ((string)value) : null);
			return true;
		case 13:
			CardText = ((value != null) ? ((string)value) : null);
			return true;
		case 14:
			IsShopPremiumHeroSkin = value != null && (bool)value;
			return true;
		case 15:
			Class = ((value != null) ? ((TAG_CLASS)value) : TAG_CLASS.INVALID);
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
		case 9:
			info = Properties[6];
			return true;
		case 6:
			info = Properties[7];
			return true;
		case 7:
			info = Properties[8];
			return true;
		case 8:
			info = Properties[9];
			return true;
		case 10:
			info = Properties[10];
			return true;
		case 11:
			info = Properties[11];
			return true;
		case 12:
			info = Properties[12];
			return true;
		case 13:
			info = Properties[13];
			return true;
		case 14:
			info = Properties[14];
			return true;
		case 15:
			info = Properties[15];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
