using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class DeckDetailsDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 290;

	private ProductDataModel m_Product;

	private MiniSetDetailsDataModel m_MiniSetDetails;

	private string m_AltDescription;

	private DataModelProperty[] m_properties = new DataModelProperty[3]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "product",
			Type = typeof(ProductDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "mini_set_details",
			Type = typeof(MiniSetDetailsDataModel)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "alt_description",
			Type = typeof(string)
		}
	};

	public int DataModelId => 290;

	public string DataModelDisplayName => "deck_details";

	public ProductDataModel Product
	{
		get
		{
			return m_Product;
		}
		set
		{
			if (m_Product != value)
			{
				RemoveNestedDataModel(m_Product);
				RegisterNestedDataModel(value);
				m_Product = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public MiniSetDetailsDataModel MiniSetDetails
	{
		get
		{
			return m_MiniSetDetails;
		}
		set
		{
			if (m_MiniSetDetails != value)
			{
				RemoveNestedDataModel(m_MiniSetDetails);
				RegisterNestedDataModel(value);
				m_MiniSetDetails = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string AltDescription
	{
		get
		{
			return m_AltDescription;
		}
		set
		{
			if (!(m_AltDescription == value))
			{
				m_AltDescription = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public DeckDetailsDataModel()
	{
		RegisterNestedDataModel(m_Product);
		RegisterNestedDataModel(m_MiniSetDetails);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		if (m_Product != null && !inspectedDataModels.Contains(m_Product.GetHashCode()))
		{
			inspectedDataModels.Add(m_Product.GetHashCode());
			hash = hash * 31 + m_Product.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		if (m_MiniSetDetails != null && !inspectedDataModels.Contains(m_MiniSetDetails.GetHashCode()))
		{
			inspectedDataModels.Add(m_MiniSetDetails.GetHashCode());
			hash = hash * 31 + m_MiniSetDetails.GetPropertiesHashCode(inspectedDataModels);
		}
		else
		{
			hash *= 31;
		}
		return hash * 31 + ((m_AltDescription != null) ? m_AltDescription.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Product;
			return true;
		case 1:
			value = m_MiniSetDetails;
			return true;
		case 2:
			value = m_AltDescription;
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
			Product = ((value != null) ? ((ProductDataModel)value) : null);
			return true;
		case 1:
			MiniSetDetails = ((value != null) ? ((MiniSetDetailsDataModel)value) : null);
			return true;
		case 2:
			AltDescription = ((value != null) ? ((string)value) : null);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
