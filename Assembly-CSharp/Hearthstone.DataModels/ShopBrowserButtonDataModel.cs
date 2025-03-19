using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class ShopBrowserButtonDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 19;

	private string m_DisplayText;

	private ProductDataModel m_DisplayProduct;

	private int m_SlotWidth;

	private int m_SlotHeight;

	private float m_SlotWidthPercentage;

	private float m_SlotHeightPercentage;

	private int m_SlotPositionX;

	private int m_SlotPositionY;

	private bool m_Hovered;

	private bool m_IsFiller;

	private ShopBlockingPlateDataModel m_BlockingPlate;

	private bool m_ShowTierData;

	private DataModelProperty[] m_properties = new DataModelProperty[12]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "display_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "display_product",
			Type = typeof(ProductDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "slot_width",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "slot_height",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 859,
			PropertyDisplayName = "slot_width_percentage",
			Type = typeof(float)
		},
		new DataModelProperty
		{
			PropertyId = 860,
			PropertyDisplayName = "slot_height_percentage",
			Type = typeof(float)
		},
		new DataModelProperty
		{
			PropertyId = 874,
			PropertyDisplayName = "slot_position_x",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 875,
			PropertyDisplayName = "slot_position_y",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "hovered",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "is_filler",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 892,
			PropertyDisplayName = "blocking_plate",
			Type = typeof(ShopBlockingPlateDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 907,
			PropertyDisplayName = "show_tier_data",
			Type = typeof(bool)
		}
	};

	public int DataModelId => 19;

	public string DataModelDisplayName => "shop_browser_button";

	public string DisplayText
	{
		get
		{
			return m_DisplayText;
		}
		set
		{
			if (!(m_DisplayText == value))
			{
				m_DisplayText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public ProductDataModel DisplayProduct
	{
		get
		{
			return m_DisplayProduct;
		}
		set
		{
			if (m_DisplayProduct != value)
			{
				RemoveNestedDataModel(m_DisplayProduct);
				RegisterNestedDataModel(value);
				m_DisplayProduct = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int SlotWidth
	{
		get
		{
			return m_SlotWidth;
		}
		set
		{
			if (m_SlotWidth != value)
			{
				m_SlotWidth = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int SlotHeight
	{
		get
		{
			return m_SlotHeight;
		}
		set
		{
			if (m_SlotHeight != value)
			{
				m_SlotHeight = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public float SlotWidthPercentage
	{
		get
		{
			return m_SlotWidthPercentage;
		}
		set
		{
			if (m_SlotWidthPercentage != value)
			{
				m_SlotWidthPercentage = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public float SlotHeightPercentage
	{
		get
		{
			return m_SlotHeightPercentage;
		}
		set
		{
			if (m_SlotHeightPercentage != value)
			{
				m_SlotHeightPercentage = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int SlotPositionX
	{
		get
		{
			return m_SlotPositionX;
		}
		set
		{
			if (m_SlotPositionX != value)
			{
				m_SlotPositionX = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int SlotPositionY
	{
		get
		{
			return m_SlotPositionY;
		}
		set
		{
			if (m_SlotPositionY != value)
			{
				m_SlotPositionY = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Hovered
	{
		get
		{
			return m_Hovered;
		}
		set
		{
			if (m_Hovered != value)
			{
				m_Hovered = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsFiller
	{
		get
		{
			return m_IsFiller;
		}
		set
		{
			if (m_IsFiller != value)
			{
				m_IsFiller = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public ShopBlockingPlateDataModel BlockingPlate
	{
		get
		{
			return m_BlockingPlate;
		}
		set
		{
			if (m_BlockingPlate != value)
			{
				RemoveNestedDataModel(m_BlockingPlate);
				RegisterNestedDataModel(value);
				m_BlockingPlate = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool ShowTierData
	{
		get
		{
			return m_ShowTierData;
		}
		set
		{
			if (m_ShowTierData != value)
			{
				m_ShowTierData = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public ShopBrowserButtonDataModel()
	{
		RegisterNestedDataModel(m_DisplayProduct);
		RegisterNestedDataModel(m_BlockingPlate);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		hash = hash * 31 + ((m_DisplayText != null) ? m_DisplayText.GetHashCode() : 0);
		if (m_DisplayProduct != null && !inspectedDataModels.Contains(m_DisplayProduct.GetHashCode()))
		{
			inspectedDataModels.Add(m_DisplayProduct.GetHashCode());
			hash = hash * 31 + m_DisplayProduct.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num = hash * 31;
		_ = m_SlotWidth;
		hash = num + m_SlotWidth.GetHashCode();
		int num2 = hash * 31;
		_ = m_SlotHeight;
		hash = num2 + m_SlotHeight.GetHashCode();
		int num3 = hash * 31;
		_ = m_SlotWidthPercentage;
		hash = num3 + m_SlotWidthPercentage.GetHashCode();
		int num4 = hash * 31;
		_ = m_SlotHeightPercentage;
		hash = num4 + m_SlotHeightPercentage.GetHashCode();
		int num5 = hash * 31;
		_ = m_SlotPositionX;
		hash = num5 + m_SlotPositionX.GetHashCode();
		int num6 = hash * 31;
		_ = m_SlotPositionY;
		hash = num6 + m_SlotPositionY.GetHashCode();
		int num7 = hash * 31;
		_ = m_Hovered;
		hash = num7 + m_Hovered.GetHashCode();
		int num8 = hash * 31;
		_ = m_IsFiller;
		hash = num8 + m_IsFiller.GetHashCode();
		if (m_BlockingPlate != null && !inspectedDataModels.Contains(m_BlockingPlate.GetHashCode()))
		{
			inspectedDataModels.Add(m_BlockingPlate.GetHashCode());
			hash = hash * 31 + m_BlockingPlate.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num9 = hash * 31;
		_ = m_ShowTierData;
		return num9 + m_ShowTierData.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_DisplayText;
			return true;
		case 1:
			value = m_DisplayProduct;
			return true;
		case 3:
			value = m_SlotWidth;
			return true;
		case 4:
			value = m_SlotHeight;
			return true;
		case 859:
			value = m_SlotWidthPercentage;
			return true;
		case 860:
			value = m_SlotHeightPercentage;
			return true;
		case 874:
			value = m_SlotPositionX;
			return true;
		case 875:
			value = m_SlotPositionY;
			return true;
		case 5:
			value = m_Hovered;
			return true;
		case 6:
			value = m_IsFiller;
			return true;
		case 892:
			value = m_BlockingPlate;
			return true;
		case 907:
			value = m_ShowTierData;
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
			DisplayText = ((value != null) ? ((string)value) : null);
			return true;
		case 1:
			DisplayProduct = ((value != null) ? ((ProductDataModel)value) : null);
			return true;
		case 3:
			SlotWidth = ((value != null) ? ((int)value) : 0);
			return true;
		case 4:
			SlotHeight = ((value != null) ? ((int)value) : 0);
			return true;
		case 859:
			SlotWidthPercentage = ((value != null) ? ((float)value) : 0f);
			return true;
		case 860:
			SlotHeightPercentage = ((value != null) ? ((float)value) : 0f);
			return true;
		case 874:
			SlotPositionX = ((value != null) ? ((int)value) : 0);
			return true;
		case 875:
			SlotPositionY = ((value != null) ? ((int)value) : 0);
			return true;
		case 5:
			Hovered = value != null && (bool)value;
			return true;
		case 6:
			IsFiller = value != null && (bool)value;
			return true;
		case 892:
			BlockingPlate = ((value != null) ? ((ShopBlockingPlateDataModel)value) : null);
			return true;
		case 907:
			ShowTierData = value != null && (bool)value;
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
		case 859:
			info = Properties[4];
			return true;
		case 860:
			info = Properties[5];
			return true;
		case 874:
			info = Properties[6];
			return true;
		case 875:
			info = Properties[7];
			return true;
		case 5:
			info = Properties[8];
			return true;
		case 6:
			info = Properties[9];
			return true;
		case 892:
			info = Properties[10];
			return true;
		case 907:
			info = Properties[11];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
