using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class MassPackOpeningSummaryDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 833;

	private int m_NumPacksOpened;

	private DataModelList<ClassCardCountDataModel> m_CommonsOpened = new DataModelList<ClassCardCountDataModel>();

	private DataModelList<ClassCardCountDataModel> m_RaresOpened = new DataModelList<ClassCardCountDataModel>();

	private DataModelList<ClassCardCountDataModel> m_EpicsOpened = new DataModelList<ClassCardCountDataModel>();

	private DataModelList<CardDataModel> m_LegendariesOpened = new DataModelList<CardDataModel>();

	private DataModelList<string> m_BannerTypeOrder = new DataModelList<string>();

	private DataModelList<ClassCardCountDataModel> m_LegendariesOpenedFallback = new DataModelList<ClassCardCountDataModel>();

	private int m_NumLegendariesOpened;

	private DataModelProperty[] m_properties = new DataModelProperty[8]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "num_packs_opened",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "commons_opened",
			Type = typeof(DataModelList<ClassCardCountDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "rares_opened",
			Type = typeof(DataModelList<ClassCardCountDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "epics_opened",
			Type = typeof(DataModelList<ClassCardCountDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "legendaries_opened",
			Type = typeof(DataModelList<CardDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "banner_type_order",
			Type = typeof(DataModelList<string>)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "legendaries_opened_fallback",
			Type = typeof(DataModelList<ClassCardCountDataModel>)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "num_legendaries_opened",
			Type = typeof(int)
		}
	};

	public int DataModelId => 833;

	public string DataModelDisplayName => "mass_pack_opening_summary";

	public int NumPacksOpened
	{
		get
		{
			return m_NumPacksOpened;
		}
		set
		{
			if (m_NumPacksOpened != value)
			{
				m_NumPacksOpened = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<ClassCardCountDataModel> CommonsOpened
	{
		get
		{
			return m_CommonsOpened;
		}
		set
		{
			if (m_CommonsOpened != value)
			{
				RemoveNestedDataModel(m_CommonsOpened);
				RegisterNestedDataModel(value);
				m_CommonsOpened = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<ClassCardCountDataModel> RaresOpened
	{
		get
		{
			return m_RaresOpened;
		}
		set
		{
			if (m_RaresOpened != value)
			{
				RemoveNestedDataModel(m_RaresOpened);
				RegisterNestedDataModel(value);
				m_RaresOpened = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<ClassCardCountDataModel> EpicsOpened
	{
		get
		{
			return m_EpicsOpened;
		}
		set
		{
			if (m_EpicsOpened != value)
			{
				RemoveNestedDataModel(m_EpicsOpened);
				RegisterNestedDataModel(value);
				m_EpicsOpened = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<CardDataModel> LegendariesOpened
	{
		get
		{
			return m_LegendariesOpened;
		}
		set
		{
			if (m_LegendariesOpened != value)
			{
				RemoveNestedDataModel(m_LegendariesOpened);
				RegisterNestedDataModel(value);
				m_LegendariesOpened = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<string> BannerTypeOrder
	{
		get
		{
			return m_BannerTypeOrder;
		}
		set
		{
			if (m_BannerTypeOrder != value)
			{
				RemoveNestedDataModel(m_BannerTypeOrder);
				RegisterNestedDataModel(value);
				m_BannerTypeOrder = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<ClassCardCountDataModel> LegendariesOpenedFallback
	{
		get
		{
			return m_LegendariesOpenedFallback;
		}
		set
		{
			if (m_LegendariesOpenedFallback != value)
			{
				RemoveNestedDataModel(m_LegendariesOpenedFallback);
				RegisterNestedDataModel(value);
				m_LegendariesOpenedFallback = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int NumLegendariesOpened
	{
		get
		{
			return m_NumLegendariesOpened;
		}
		set
		{
			if (m_NumLegendariesOpened != value)
			{
				m_NumLegendariesOpened = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public MassPackOpeningSummaryDataModel()
	{
		RegisterNestedDataModel(m_CommonsOpened);
		RegisterNestedDataModel(m_RaresOpened);
		RegisterNestedDataModel(m_EpicsOpened);
		RegisterNestedDataModel(m_LegendariesOpened);
		RegisterNestedDataModel(m_BannerTypeOrder);
		RegisterNestedDataModel(m_LegendariesOpenedFallback);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_NumPacksOpened;
		hash = num + m_NumPacksOpened.GetHashCode();
		if (m_CommonsOpened != null && !inspectedDataModels.Contains(m_CommonsOpened.GetHashCode()))
		{
			inspectedDataModels.Add(m_CommonsOpened.GetHashCode());
			hash = hash * 31 + m_CommonsOpened.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_RaresOpened != null && !inspectedDataModels.Contains(m_RaresOpened.GetHashCode()))
		{
			inspectedDataModels.Add(m_RaresOpened.GetHashCode());
			hash = hash * 31 + m_RaresOpened.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_EpicsOpened != null && !inspectedDataModels.Contains(m_EpicsOpened.GetHashCode()))
		{
			inspectedDataModels.Add(m_EpicsOpened.GetHashCode());
			hash = hash * 31 + m_EpicsOpened.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_LegendariesOpened != null && !inspectedDataModels.Contains(m_LegendariesOpened.GetHashCode()))
		{
			inspectedDataModels.Add(m_LegendariesOpened.GetHashCode());
			hash = hash * 31 + m_LegendariesOpened.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_BannerTypeOrder != null && !inspectedDataModels.Contains(m_BannerTypeOrder.GetHashCode()))
		{
			inspectedDataModels.Add(m_BannerTypeOrder.GetHashCode());
			hash = hash * 31 + m_BannerTypeOrder.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_LegendariesOpenedFallback != null && !inspectedDataModels.Contains(m_LegendariesOpenedFallback.GetHashCode()))
		{
			inspectedDataModels.Add(m_LegendariesOpenedFallback.GetHashCode());
			hash = hash * 31 + m_LegendariesOpenedFallback.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		int num2 = hash * 31;
		_ = m_NumLegendariesOpened;
		return num2 + m_NumLegendariesOpened.GetHashCode();
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_NumPacksOpened;
			return true;
		case 1:
			value = m_CommonsOpened;
			return true;
		case 2:
			value = m_RaresOpened;
			return true;
		case 3:
			value = m_EpicsOpened;
			return true;
		case 4:
			value = m_LegendariesOpened;
			return true;
		case 5:
			value = m_BannerTypeOrder;
			return true;
		case 6:
			value = m_LegendariesOpenedFallback;
			return true;
		case 7:
			value = m_NumLegendariesOpened;
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
			NumPacksOpened = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			CommonsOpened = ((value != null) ? ((DataModelList<ClassCardCountDataModel>)value) : null);
			return true;
		case 2:
			RaresOpened = ((value != null) ? ((DataModelList<ClassCardCountDataModel>)value) : null);
			return true;
		case 3:
			EpicsOpened = ((value != null) ? ((DataModelList<ClassCardCountDataModel>)value) : null);
			return true;
		case 4:
			LegendariesOpened = ((value != null) ? ((DataModelList<CardDataModel>)value) : null);
			return true;
		case 5:
			BannerTypeOrder = ((value != null) ? ((DataModelList<string>)value) : null);
			return true;
		case 6:
			LegendariesOpenedFallback = ((value != null) ? ((DataModelList<ClassCardCountDataModel>)value) : null);
			return true;
		case 7:
			NumLegendariesOpened = ((value != null) ? ((int)value) : 0);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
