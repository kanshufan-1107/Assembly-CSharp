using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ProductDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 15;

	private long m_PmtId;

	private string m_Name;

	private string m_Description;

	private DataModelList<string> m_Tags = new DataModelList<string>();

	private DataModelList<RewardItemDataModel> m_Items = new DataModelList<RewardItemDataModel>();

	private DataModelList<PriceDataModel> m_Prices = new DataModelList<PriceDataModel>();

	private DataModelList<ProductDataModel> m_Variants = new DataModelList<ProductDataModel>();

	private ProductAvailability m_Availability;

	private string m_ShortName;

	private string m_FullDescription;

	private bool m_ShowMarketingImage;

	private DataModelList<IGamemodeAvailabilityService.Gamemode> m_RequiredGamemodes = new DataModelList<IGamemodeAvailabilityService.Gamemode>();

	private RewardListDataModel m_RewardList;

	private string m_DescriptionHeader;

	private string m_VariantName;

	private string m_FlavorText;

	private string m_ShopSwipeAnimDelay;

	private string m_AdditionalBannerData;

	private bool m_IsScheduled;

	private int m_DaysBeforeEnd;

	private int m_HoursBeforeEnd;

	private DataModelProperty[] m_properties = new DataModelProperty[21]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "pmt_id",
			Type = typeof(long)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "name",
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
			PropertyDisplayName = "tags",
			Type = typeof(DataModelList<string>)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "items",
			Type = typeof(DataModelList<RewardItemDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "prices",
			Type = typeof(DataModelList<PriceDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "variants",
			Type = typeof(DataModelList<ProductDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "availability",
			Type = typeof(ProductAvailability)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "shortName",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "full_description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 34,
			PropertyDisplayName = "show_marketing_image",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 35,
			PropertyDisplayName = "required_gamemodes",
			Type = typeof(DataModelList<IGamemodeAvailabilityService.Gamemode>)
		},
		new DataModelProperty
		{
			PropertyId = 36,
			PropertyDisplayName = "reward_list",
			Type = typeof(RewardListDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 38,
			PropertyDisplayName = "description_header",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 39,
			PropertyDisplayName = "variant_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 40,
			PropertyDisplayName = "flavor_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 810,
			PropertyDisplayName = "shop_swipe_anim_delay",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 809,
			PropertyDisplayName = "additional_banner_data",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 806,
			PropertyDisplayName = "is_scheduled",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 807,
			PropertyDisplayName = "days_before_end",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 808,
			PropertyDisplayName = "hours_before_end",
			Type = typeof(int)
		}
	};

	public int DataModelId => 15;

	public string DataModelDisplayName => "product";

	public long PmtId
	{
		get
		{
			return m_PmtId;
		}
		set
		{
			if (m_PmtId != value)
			{
				m_PmtId = value;
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

	public DataModelList<string> Tags
	{
		get
		{
			return m_Tags;
		}
		set
		{
			if (m_Tags != value)
			{
				RemoveNestedDataModel(m_Tags);
				RegisterNestedDataModel(value);
				m_Tags = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<RewardItemDataModel> Items
	{
		get
		{
			return m_Items;
		}
		set
		{
			if (m_Items != value)
			{
				RemoveNestedDataModel(m_Items);
				RegisterNestedDataModel(value);
				m_Items = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<PriceDataModel> Prices
	{
		get
		{
			return m_Prices;
		}
		set
		{
			if (m_Prices != value)
			{
				RemoveNestedDataModel(m_Prices);
				RegisterNestedDataModel(value);
				m_Prices = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<ProductDataModel> Variants
	{
		get
		{
			return m_Variants;
		}
		set
		{
			if (m_Variants != value)
			{
				RemoveNestedDataModel(m_Variants);
				RegisterNestedDataModel(value);
				m_Variants = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public ProductAvailability Availability
	{
		get
		{
			return m_Availability;
		}
		set
		{
			if (m_Availability != value)
			{
				m_Availability = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string ShortName
	{
		get
		{
			return m_ShortName;
		}
		set
		{
			if (!(m_ShortName == value))
			{
				m_ShortName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string FullDescription
	{
		get
		{
			return m_FullDescription;
		}
		set
		{
			if (!(m_FullDescription == value))
			{
				m_FullDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowMarketingImage
	{
		get
		{
			return m_ShowMarketingImage;
		}
		set
		{
			if (m_ShowMarketingImage != value)
			{
				m_ShowMarketingImage = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<IGamemodeAvailabilityService.Gamemode> RequiredGamemodes
	{
		get
		{
			return m_RequiredGamemodes;
		}
		set
		{
			if (m_RequiredGamemodes != value)
			{
				RemoveNestedDataModel(m_RequiredGamemodes);
				RegisterNestedDataModel(value);
				m_RequiredGamemodes = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public RewardListDataModel RewardList
	{
		get
		{
			return m_RewardList;
		}
		set
		{
			if (m_RewardList != value)
			{
				RemoveNestedDataModel(m_RewardList);
				RegisterNestedDataModel(value);
				m_RewardList = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DescriptionHeader
	{
		get
		{
			return m_DescriptionHeader;
		}
		set
		{
			if (!(m_DescriptionHeader == value))
			{
				m_DescriptionHeader = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string VariantName
	{
		get
		{
			return m_VariantName;
		}
		set
		{
			if (!(m_VariantName == value))
			{
				m_VariantName = value;
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

	public string ShopSwipeAnimDelay
	{
		get
		{
			return m_ShopSwipeAnimDelay;
		}
		set
		{
			if (!(m_ShopSwipeAnimDelay == value))
			{
				m_ShopSwipeAnimDelay = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string AdditionalBannerData
	{
		get
		{
			return m_AdditionalBannerData;
		}
		set
		{
			if (!(m_AdditionalBannerData == value))
			{
				m_AdditionalBannerData = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsScheduled
	{
		get
		{
			return m_IsScheduled;
		}
		set
		{
			if (m_IsScheduled != value)
			{
				m_IsScheduled = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int DaysBeforeEnd
	{
		get
		{
			return m_DaysBeforeEnd;
		}
		set
		{
			if (m_DaysBeforeEnd != value)
			{
				m_DaysBeforeEnd = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int HoursBeforeEnd
	{
		get
		{
			return m_HoursBeforeEnd;
		}
		set
		{
			if (m_HoursBeforeEnd != value)
			{
				m_HoursBeforeEnd = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public ProductDataModel()
	{
		RegisterNestedDataModel(m_Tags);
		RegisterNestedDataModel(m_Items);
		RegisterNestedDataModel(m_Prices);
		RegisterNestedDataModel(m_Variants);
		RegisterNestedDataModel(m_RequiredGamemodes);
		RegisterNestedDataModel(m_RewardList);
	}

	public int GetPropertiesHashCode()
	{
		int num = 17 * 31;
		_ = m_PmtId;
		int num2 = (((((((num + m_PmtId.GetHashCode()) * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0)) * 31 + ((m_Description != null) ? m_Description.GetHashCode() : 0)) * 31 + ((m_Tags != null) ? m_Tags.GetPropertiesHashCode() : 0)) * 31 + ((m_Items != null) ? m_Items.GetPropertiesHashCode() : 0)) * 31 + ((m_Prices != null) ? m_Prices.GetPropertiesHashCode() : 0)) * 31 + ((m_Variants != null) ? m_Variants.GetPropertiesHashCode() : 0)) * 31;
		_ = m_Availability;
		int num3 = (((num2 + m_Availability.GetHashCode()) * 31 + ((m_ShortName != null) ? m_ShortName.GetHashCode() : 0)) * 31 + ((m_FullDescription != null) ? m_FullDescription.GetHashCode() : 0)) * 31;
		_ = m_ShowMarketingImage;
		int num4 = ((((((((num3 + m_ShowMarketingImage.GetHashCode()) * 31 + ((m_RequiredGamemodes != null) ? m_RequiredGamemodes.GetPropertiesHashCode() : 0)) * 31 + ((m_RewardList != null) ? m_RewardList.GetPropertiesHashCode() : 0)) * 31 + ((m_DescriptionHeader != null) ? m_DescriptionHeader.GetHashCode() : 0)) * 31 + ((m_VariantName != null) ? m_VariantName.GetHashCode() : 0)) * 31 + ((m_FlavorText != null) ? m_FlavorText.GetHashCode() : 0)) * 31 + ((m_ShopSwipeAnimDelay != null) ? m_ShopSwipeAnimDelay.GetHashCode() : 0)) * 31 + ((m_AdditionalBannerData != null) ? m_AdditionalBannerData.GetHashCode() : 0)) * 31;
		_ = m_IsScheduled;
		int num5 = (num4 + m_IsScheduled.GetHashCode()) * 31;
		_ = m_DaysBeforeEnd;
		int num6 = (num5 + m_DaysBeforeEnd.GetHashCode()) * 31;
		_ = m_HoursBeforeEnd;
		return num6 + m_HoursBeforeEnd.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_PmtId;
			return true;
		case 1:
			value = m_Name;
			return true;
		case 2:
			value = m_Description;
			return true;
		case 3:
			value = m_Tags;
			return true;
		case 4:
			value = m_Items;
			return true;
		case 5:
			value = m_Prices;
			return true;
		case 6:
			value = m_Variants;
			return true;
		case 7:
			value = m_Availability;
			return true;
		case 8:
			value = m_ShortName;
			return true;
		case 9:
			value = m_FullDescription;
			return true;
		case 34:
			value = m_ShowMarketingImage;
			return true;
		case 35:
			value = m_RequiredGamemodes;
			return true;
		case 36:
			value = m_RewardList;
			return true;
		case 38:
			value = m_DescriptionHeader;
			return true;
		case 39:
			value = m_VariantName;
			return true;
		case 40:
			value = m_FlavorText;
			return true;
		case 810:
			value = m_ShopSwipeAnimDelay;
			return true;
		case 809:
			value = m_AdditionalBannerData;
			return true;
		case 806:
			value = m_IsScheduled;
			return true;
		case 807:
			value = m_DaysBeforeEnd;
			return true;
		case 808:
			value = m_HoursBeforeEnd;
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
			PmtId = ((value != null) ? ((long)value) : 0);
			return true;
		case 1:
			Name = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			Description = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			Tags = ((value != null) ? ((DataModelList<string>)value) : null);
			return true;
		case 4:
			Items = ((value != null) ? ((DataModelList<RewardItemDataModel>)value) : null);
			return true;
		case 5:
			Prices = ((value != null) ? ((DataModelList<PriceDataModel>)value) : null);
			return true;
		case 6:
			Variants = ((value != null) ? ((DataModelList<ProductDataModel>)value) : null);
			return true;
		case 7:
			Availability = ((value != null) ? ((ProductAvailability)value) : ProductAvailability.UNDEFINED);
			return true;
		case 8:
			ShortName = ((value != null) ? ((string)value) : null);
			return true;
		case 9:
			FullDescription = ((value != null) ? ((string)value) : null);
			return true;
		case 34:
			ShowMarketingImage = value != null && (bool)value;
			return true;
		case 35:
			RequiredGamemodes = ((value != null) ? ((DataModelList<IGamemodeAvailabilityService.Gamemode>)value) : null);
			return true;
		case 36:
			RewardList = ((value != null) ? ((RewardListDataModel)value) : null);
			return true;
		case 38:
			DescriptionHeader = ((value != null) ? ((string)value) : null);
			return true;
		case 39:
			VariantName = ((value != null) ? ((string)value) : null);
			return true;
		case 40:
			FlavorText = ((value != null) ? ((string)value) : null);
			return true;
		case 810:
			ShopSwipeAnimDelay = ((value != null) ? ((string)value) : null);
			return true;
		case 809:
			AdditionalBannerData = ((value != null) ? ((string)value) : null);
			return true;
		case 806:
			IsScheduled = value != null && (bool)value;
			return true;
		case 807:
			DaysBeforeEnd = ((value != null) ? ((int)value) : 0);
			return true;
		case 808:
			HoursBeforeEnd = ((value != null) ? ((int)value) : 0);
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
		case 34:
			info = Properties[10];
			return true;
		case 35:
			info = Properties[11];
			return true;
		case 36:
			info = Properties[12];
			return true;
		case 38:
			info = Properties[13];
			return true;
		case 39:
			info = Properties[14];
			return true;
		case 40:
			info = Properties[15];
			return true;
		case 810:
			info = Properties[16];
			return true;
		case 809:
			info = Properties[17];
			return true;
		case 806:
			info = Properties[18];
			return true;
		case 807:
			info = Properties[19];
			return true;
		case 808:
			info = Properties[20];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
