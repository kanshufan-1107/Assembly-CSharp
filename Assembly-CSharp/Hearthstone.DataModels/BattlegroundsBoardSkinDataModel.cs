using Assets;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class BattlegroundsBoardSkinDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 564;

	private int m_BoardDbiId;

	private string m_DisplayName;

	private string m_Description;

	private bool m_IsOwned;

	private bool m_IsFavorite;

	private string m_ShopDetailsTexture;

	private string m_ShopDetailsMovie;

	private string m_DetailsDisplayName;

	private bool m_IsNew;

	private BattlegroundsBoardSkin.Bordertype m_BorderType;

	private string m_Rarity;

	private bool m_IsForCollectionPage;

	private string m_DetailsRenderConfig;

	private bool m_CosmeticsRenderingEnabled;

	private DataModelProperty[] m_properties = new DataModelProperty[14]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "board_dbi_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "display_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "is_owned",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "is_favorite",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "shop_details_texture",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "shop_details_movie",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "display_name_details",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "is_new",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "border_type",
			Type = typeof(BattlegroundsBoardSkin.Bordertype)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "rarity",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "is_for_collection_page",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 795,
			PropertyDisplayName = "details_render_config",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 823,
			PropertyDisplayName = "cosmetics_rendering_enabled",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 564;

	public string DataModelDisplayName => "battlegrounds_board_skin";

	public int BoardDbiId
	{
		get
		{
			return m_BoardDbiId;
		}
		set
		{
			if (m_BoardDbiId != value)
			{
				m_BoardDbiId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DisplayName
	{
		get
		{
			return m_DisplayName;
		}
		set
		{
			if (!(m_DisplayName == value))
			{
				m_DisplayName = value;
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

	public bool IsOwned
	{
		get
		{
			return m_IsOwned;
		}
		set
		{
			if (m_IsOwned != value)
			{
				m_IsOwned = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsFavorite
	{
		get
		{
			return m_IsFavorite;
		}
		set
		{
			if (m_IsFavorite != value)
			{
				m_IsFavorite = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ShopDetailsTexture
	{
		get
		{
			return m_ShopDetailsTexture;
		}
		set
		{
			if (!(m_ShopDetailsTexture == value))
			{
				m_ShopDetailsTexture = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ShopDetailsMovie
	{
		get
		{
			return m_ShopDetailsMovie;
		}
		set
		{
			if (!(m_ShopDetailsMovie == value))
			{
				m_ShopDetailsMovie = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DetailsDisplayName
	{
		get
		{
			return m_DetailsDisplayName;
		}
		set
		{
			if (!(m_DetailsDisplayName == value))
			{
				m_DetailsDisplayName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsNew
	{
		get
		{
			return m_IsNew;
		}
		set
		{
			if (m_IsNew != value)
			{
				m_IsNew = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public BattlegroundsBoardSkin.Bordertype BorderType
	{
		get
		{
			return m_BorderType;
		}
		set
		{
			if (m_BorderType != value)
			{
				m_BorderType = value;
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

	public bool IsForCollectionPage
	{
		get
		{
			return m_IsForCollectionPage;
		}
		set
		{
			if (m_IsForCollectionPage != value)
			{
				m_IsForCollectionPage = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DetailsRenderConfig
	{
		get
		{
			return m_DetailsRenderConfig;
		}
		set
		{
			if (!(m_DetailsRenderConfig == value))
			{
				m_DetailsRenderConfig = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool CosmeticsRenderingEnabled
	{
		get
		{
			return m_CosmeticsRenderingEnabled;
		}
		set
		{
			if (m_CosmeticsRenderingEnabled != value)
			{
				m_CosmeticsRenderingEnabled = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_BoardDbiId;
		int num2 = (((num + m_BoardDbiId.GetHashCode()) * 31 + ((m_DisplayName != null) ? m_DisplayName.GetHashCode() : 0)) * 31 + ((m_Description != null) ? m_Description.GetHashCode() : 0)) * 31;
		_ = m_IsOwned;
		int num3 = (num2 + m_IsOwned.GetHashCode()) * 31;
		_ = m_IsFavorite;
		int num4 = ((((num3 + m_IsFavorite.GetHashCode()) * 31 + ((m_ShopDetailsTexture != null) ? m_ShopDetailsTexture.GetHashCode() : 0)) * 31 + ((m_ShopDetailsMovie != null) ? m_ShopDetailsMovie.GetHashCode() : 0)) * 31 + ((m_DetailsDisplayName != null) ? m_DetailsDisplayName.GetHashCode() : 0)) * 31;
		_ = m_IsNew;
		int num5 = (num4 + m_IsNew.GetHashCode()) * 31;
		_ = m_BorderType;
		int num6 = ((num5 + m_BorderType.GetHashCode()) * 31 + ((m_Rarity != null) ? m_Rarity.GetHashCode() : 0)) * 31;
		_ = m_IsForCollectionPage;
		int num7 = ((num6 + m_IsForCollectionPage.GetHashCode()) * 31 + ((m_DetailsRenderConfig != null) ? m_DetailsRenderConfig.GetHashCode() : 0)) * 31;
		_ = m_CosmeticsRenderingEnabled;
		return num7 + m_CosmeticsRenderingEnabled.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_BoardDbiId;
			return true;
		case 1:
			value = m_DisplayName;
			return true;
		case 2:
			value = m_Description;
			return true;
		case 3:
			value = m_IsOwned;
			return true;
		case 4:
			value = m_IsFavorite;
			return true;
		case 5:
			value = m_ShopDetailsTexture;
			return true;
		case 6:
			value = m_ShopDetailsMovie;
			return true;
		case 7:
			value = m_DetailsDisplayName;
			return true;
		case 8:
			value = m_IsNew;
			return true;
		case 9:
			value = m_BorderType;
			return true;
		case 10:
			value = m_Rarity;
			return true;
		case 11:
			value = m_IsForCollectionPage;
			return true;
		case 795:
			value = m_DetailsRenderConfig;
			return true;
		case 823:
			value = m_CosmeticsRenderingEnabled;
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
			BoardDbiId = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			DisplayName = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			Description = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			IsOwned = value != null && (bool)value;
			return true;
		case 4:
			IsFavorite = value != null && (bool)value;
			return true;
		case 5:
			ShopDetailsTexture = ((value != null) ? ((string)value) : null);
			return true;
		case 6:
			ShopDetailsMovie = ((value != null) ? ((string)value) : null);
			return true;
		case 7:
			DetailsDisplayName = ((value != null) ? ((string)value) : null);
			return true;
		case 8:
			IsNew = value != null && (bool)value;
			return true;
		case 9:
			BorderType = ((value != null) ? ((BattlegroundsBoardSkin.Bordertype)value) : BattlegroundsBoardSkin.Bordertype.DEFAULT);
			return true;
		case 10:
			Rarity = ((value != null) ? ((string)value) : null);
			return true;
		case 11:
			IsForCollectionPage = value != null && (bool)value;
			return true;
		case 795:
			DetailsRenderConfig = ((value != null) ? ((string)value) : null);
			return true;
		case 823:
			CosmeticsRenderingEnabled = value != null && (bool)value;
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
		case 10:
			info = Properties[10];
			return true;
		case 11:
			info = Properties[11];
			return true;
		case 795:
			info = Properties[12];
			return true;
		case 823:
			info = Properties[13];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
